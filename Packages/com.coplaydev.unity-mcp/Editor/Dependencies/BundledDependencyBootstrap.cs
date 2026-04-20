using MCPForUnity.Editor.Constants;
using MCPForUnity.Editor.Helpers;
using UnityEditor;

namespace MCPForUnity.Editor.Dependencies
{
    /// <summary>
    /// Runs once per editor load: kicks off bundled uv + managed Python
    /// provisioning on the main thread. BundledDependencyInstaller handles
    /// phase separation (main → background → main) internally, so this file
    /// only needs to be the single entry point.
    /// </summary>
    [InitializeOnLoad]
    internal static class BundledDependencyBootstrap
    {
        private const string SessionFlag = "MCPForUnity.Bundled.BootstrapRan";

        static BundledDependencyBootstrap()
        {
            // Always hydrate state so domain reloads don't show "pending" in
            // the Status menu. Cheap — just reads EditorPrefs + File.Exists.
            BundledDependencyInstaller.HydrateFromPersistedState();

            // Default Auto-Start ON for fresh installs (no key yet). Users who
            // explicitly toggle it off keep that preference — HasKey check only
            // fires on the very first run.
            if (!EditorPrefs.HasKey(EditorPrefKeys.AutoStartOnLoad))
            {
                EditorPrefs.SetBool(EditorPrefKeys.AutoStartOnLoad, true);
            }

            if (SessionState.GetBool(SessionFlag, false)) return;
            SessionState.SetBool(SessionFlag, true);

            // Both calls below MUST run on the Unity main thread (this static
            // ctor is invoked by Unity on the main thread during InitializeOnLoad).
            BundledDependencyInstaller.CachePackageRoot();
            BundledDependencyInstaller.BeginProvisioning();

            McpLog.Info("BundledDependencyBootstrap: provisioning scheduled.", always: false);
        }
    }
}
