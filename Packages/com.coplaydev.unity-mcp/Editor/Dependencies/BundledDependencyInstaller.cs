using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.Dependencies
{
    /// <summary>
    /// Provisions a bundled uv binary + managed Python runtime so MCP for Unity
    /// works on machines without a pre-installed Python/uv toolchain.
    ///
    /// Layout:
    ///   Seed   (git-tracked, immutable) → {package}/Editor~/Bundled/uv/&lt;rid&gt;/uv.exe
    ///   Runtime (user-local, updatable) → %LOCALAPPDATA%/MCPForUnity/uv/
    ///
    /// Threading: all Unity editor APIs (EditorPrefs, PackageInfo, EditorApplication)
    /// are main-thread only. Provisioning runs in three phases:
    ///   Phase 1 (main)       — read EditorPrefs + cached package root into a snapshot.
    ///   Phase 2 (background) — copy seed, run `uv self update`, `uv python install`.
    ///   Phase 3 (main)       — persist EditorPrefs, set state, raise events.
    /// </summary>
    public static class BundledDependencyInstaller
    {
        private const string InstalledVersionPref = "MCPForUnity.Bundled.InstalledUvVersion";
        private const string TargetPythonVersion = "3.13";
        private const int PythonInstallTimeoutMs = 180_000;

        // UnityEditor.PackageManager.PackageInfo.FindForAssembly is main-thread
        // only. Bootstrap caches the resolved path before scheduling background
        // work so the background phase can still locate the seed binary.
        private static string s_cachedPackageRoot;
        private static volatile bool s_provisioningInFlight;

        public enum InstallResult
        {
            AlreadyInstalled,
            Installed,
            Updated,
            Failed,
            UnsupportedPlatform,
        }

        /// <summary>True once provisioning has run to completion at least once.</summary>
        public static bool IsProvisioned { get; private set; }

        /// <summary>Last result from provisioning; null until provisioning finishes.</summary>
        public static InstallResult? LastResult { get; private set; }

        /// <summary>
        /// Fires on the Unity main thread after provisioning finishes, whether
        /// or not it succeeded. Late subscribers receive an immediate replay of
        /// the most recent result so they don't miss the signal.
        /// </summary>
        public static event Action<InstallResult> ProvisioningCompleted
        {
            add
            {
                _provisioningCompleted += value;
                if (IsProvisioned && LastResult.HasValue)
                {
                    try { value?.Invoke(LastResult.Value); }
                    catch (Exception ex) { McpLog.Warn($"BundledDependencyInstaller: late subscriber threw — {ex.Message}"); }
                }
            }
            remove { _provisioningCompleted -= value; }
        }

        private static Action<InstallResult> _provisioningCompleted;

        /// <summary>
        /// Must be called from the Unity main thread. Resolves and caches the
        /// package root so the background phase can find the seed binary.
        /// </summary>
        public static void CachePackageRoot()
        {
            try
            {
                var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(BundledDependencyInstaller).Assembly);
                if (info != null && !string.IsNullOrEmpty(info.resolvedPath))
                {
                    s_cachedPackageRoot = info.resolvedPath;
                }
            }
            catch (Exception ex)
            {
                McpLog.Warn($"BundledDependencyInstaller: CachePackageRoot failed — {ex.Message}");
            }
        }

        /// <summary>
        /// Kick off provisioning. Must be called from the Unity main thread.
        /// Returns immediately; subscribers to <see cref="ProvisioningCompleted"/>
        /// are invoked on the main thread when the work finishes.
        /// </summary>
        public static void BeginProvisioning(bool force = false)
        {
            if (s_provisioningInFlight)
            {
                McpLog.Info("BundledDependencyInstaller: provisioning already in flight; ignoring duplicate request.", always: false);
                return;
            }

            // PHASE 1 — snapshot main-thread-only state before going async.
            var snapshot = new Snapshot
            {
                Force = force,
                PackageRoot = s_cachedPackageRoot,
                LastInstalledVersion = EditorPrefs.GetString(InstalledVersionPref, string.Empty),
            };

            s_provisioningInFlight = true;

            // PHASE 2 — heavy work on a background thread.
            _ = Task.Run(() =>
            {
                string newVersionToPersist = null;
                InstallResult result;
                try
                {
                    result = RunProvisioningBackground(snapshot, out newVersionToPersist);
                }
                catch (Exception ex)
                {
                    // Main-thread safe: just a log + Failed result.
                    UnityEngine.Debug.LogError($"[MCP for Unity] BundledDependencyInstaller: provisioning threw — {ex}");
                    result = InstallResult.Failed;
                }

                // PHASE 3 — marshal back to the main thread to touch Unity APIs.
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(newVersionToPersist))
                        {
                            EditorPrefs.SetString(InstalledVersionPref, newVersionToPersist);
                        }
                        LastResult = result;
                        IsProvisioned = true;
                        McpLog.Info($"BundledDependencyInstaller: {result}", always: true);
                        RaiseProvisioningCompleted(result);
                    }
                    finally
                    {
                        s_provisioningInFlight = false;
                    }
                };
            });
        }

        /// <summary>Absolute directory containing the runtime uv binary.</summary>
        public static string GetRuntimeDir()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                localAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? "",
                    ".mcp-for-unity");
            }
            return Path.Combine(localAppData, "MCPForUnity", "uv");
        }

        /// <summary>Absolute path to the runtime uv executable, whether or not it exists.</summary>
        public static string GetRuntimeUvPath() => Path.Combine(GetRuntimeDir(), GetUvExeName());

        /// <summary>True if the runtime uv executable exists on disk.</summary>
        public static bool IsRuntimeAvailable() => File.Exists(GetRuntimeUvPath());

        // ---------------------------------------------------------------------
        // Phase 2 implementation (background-thread safe)
        // ---------------------------------------------------------------------

        private struct Snapshot
        {
            public bool Force;
            public string PackageRoot;
            public string LastInstalledVersion;
        }

        private static InstallResult RunProvisioningBackground(Snapshot snapshot, out string newVersionToPersist)
        {
            newVersionToPersist = null;

            if (!TryGetSeedUvPath(snapshot.PackageRoot, out string seedUv, out string rid))
            {
                UnityEngine.Debug.LogWarning($"[MCP for Unity] BundledDependencyInstaller: no seed uv for current platform (rid={rid ?? "unknown"}). Falling back to system uv.");
                return InstallResult.UnsupportedPlatform;
            }

            string runtimeDir = GetRuntimeDir();
            string runtimeUv = Path.Combine(runtimeDir, GetUvExeName());

            try
            {
                Directory.CreateDirectory(runtimeDir);

                // Copy seed → runtime when: forced, runtime missing, or seed binary
                // differs from runtime (new version shipped via seed replacement).
                bool needsCopy = snapshot.Force
                    || !File.Exists(runtimeUv)
                    || IsSeedDifferentFromRuntime(seedUv, runtimeUv);

                if (needsCopy)
                {
                    CopySeedToRuntime(seedUv, runtimeDir);
                    UnityEngine.Debug.Log($"[MCP for Unity] BundledDependencyInstaller: seeded uv at {runtimeUv}");
                }

                if (!TryReadVersion(runtimeUv, out string uvVersion))
                {
                    UnityEngine.Debug.LogError($"[MCP for Unity] BundledDependencyInstaller: runtime uv did not report a version ({runtimeUv}).");
                    return InstallResult.Failed;
                }

                bool firstRun = string.IsNullOrEmpty(snapshot.LastInstalledVersion) || snapshot.Force;

                if (!EnsureManagedPython(runtimeUv))
                {
                    return InstallResult.Failed;
                }

                newVersionToPersist = uvVersion;
                return needsCopy ? (firstRun ? InstallResult.Installed : InstallResult.Updated)
                    : InstallResult.AlreadyInstalled;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP for Unity] BundledDependencyInstaller: provisioning failed — {ex.Message}");
                return InstallResult.Failed;
            }
        }

        private static void RaiseProvisioningCompleted(InstallResult result)
        {
            var handler = _provisioningCompleted;
            if (handler == null) return;
            try { handler.Invoke(result); }
            catch (Exception ex) { McpLog.Warn($"BundledDependencyInstaller: subscriber threw — {ex.Message}"); }
        }

        private static bool TryGetSeedUvPath(string cachedPackageRoot, out string seedUv, out string rid)
        {
            seedUv = null;
            rid = GetCurrentRuntimeId();
            if (rid == null) return false;

            string packageRoot = !string.IsNullOrEmpty(cachedPackageRoot)
                ? cachedPackageRoot
                : ResolvePackageRootFromFilesystem();

            if (string.IsNullOrEmpty(packageRoot)) return false;

            string seedDir = Path.Combine(packageRoot, "Editor~", "Bundled", "uv", rid);
            string candidate = Path.Combine(seedDir, GetUvExeName());
            if (!File.Exists(candidate))
            {
                UnityEngine.Debug.LogWarning($"[MCP for Unity] BundledDependencyInstaller: expected seed binary missing at {candidate}");
                return false;
            }

            seedUv = candidate;
            return true;
        }

        private static string ResolvePackageRootFromFilesystem()
        {
            try
            {
                string asmLocation = typeof(BundledDependencyInstaller).Assembly.Location;
                if (!string.IsNullOrEmpty(asmLocation) && File.Exists(asmLocation))
                {
                    var dir = new DirectoryInfo(asmLocation).Parent;
                    while (dir != null)
                    {
                        if (File.Exists(Path.Combine(dir.FullName, "package.json")))
                        {
                            return dir.FullName;
                        }
                        dir = dir.Parent;
                    }
                }

                string projectPackages = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Packages",
                    "com.coplaydev.unity-mcp");
                if (File.Exists(Path.Combine(projectPackages, "package.json")))
                {
                    return projectPackages;
                }
            }
            catch
            {
                // Swallow — caller handles null.
            }

            return null;
        }

        private static string GetCurrentRuntimeId()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => null,
                };
            }
            // macOS / Linux seeds are not bundled in the initial rollout (B1).
            return null;
        }

        private static string GetUvExeName()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "uv.exe" : "uv";

        private static void CopySeedToRuntime(string seedUv, string runtimeDir)
        {
            string seedDir = Path.GetDirectoryName(seedUv) ?? "";
            foreach (string file in Directory.GetFiles(seedDir))
            {
                string target = Path.Combine(runtimeDir, Path.GetFileName(file));
                File.Copy(file, target, overwrite: true);
            }
        }

        private static bool TryReadVersion(string uvExe, out string version)
        {
            version = null;
            if (!ExecPath.TryRun(uvExe, "--version", null, out string stdout, out string stderr, 10_000))
            {
                return false;
            }
            string output = !string.IsNullOrWhiteSpace(stdout) ? stdout : stderr;
            if (string.IsNullOrWhiteSpace(output)) return false;

            string[] parts = output.Trim().Split(' ');
            if (parts.Length >= 2 && parts[0].Equals("uv", StringComparison.OrdinalIgnoreCase))
            {
                version = parts[1];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true when the seed and runtime uv binaries differ. Compares
        /// file size and last-write timestamp — sufficient to detect seed
        /// replacement via git without running the executable.
        /// </summary>
        private static bool IsSeedDifferentFromRuntime(string seedPath, string runtimePath)
        {
            try
            {
                var seed = new FileInfo(seedPath);
                var runtime = new FileInfo(runtimePath);
                if (seed.Length != runtime.Length) return true;
                if (seed.LastWriteTimeUtc > runtime.LastWriteTimeUtc) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool EnsureManagedPython(string uvExe)
        {
            try
            {
                if (!ExecPath.TryRun(
                        uvExe,
                        $"python install {TargetPythonVersion}",
                        null,
                        out string stdout,
                        out string stderr,
                        PythonInstallTimeoutMs))
                {
                    UnityEngine.Debug.LogError($"[MCP for Unity] BundledDependencyInstaller: `uv python install {TargetPythonVersion}` failed — {stderr?.Trim()}");
                    return false;
                }

                string output = !string.IsNullOrWhiteSpace(stdout) ? stdout : stderr;
                if (!string.IsNullOrWhiteSpace(output))
                {
                    UnityEngine.Debug.Log($"[MCP for Unity] BundledDependencyInstaller: python install → {output.Trim()}");
                }
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP for Unity] BundledDependencyInstaller: python install threw — {ex.Message}");
                return false;
            }
        }
    }
}
