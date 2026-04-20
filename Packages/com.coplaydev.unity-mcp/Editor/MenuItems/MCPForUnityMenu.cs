using System.IO;
using System.Text;
using MCPForUnity.Editor.Dependencies;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;
using MCPForUnity.Editor.Setup;
using MCPForUnity.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.MenuItems
{
    public static class MCPForUnityMenu
    {
        [MenuItem("Window/MCP For Unity/Toggle MCP Window %#m", priority = 1)]
        public static void ToggleMCPWindow()
        {
            if (MCPForUnityEditorWindow.HasAnyOpenWindow())
            {
                MCPForUnityEditorWindow.CloseAllOpenWindows();
            }
            else
            {
                MCPForUnityEditorWindow.ShowWindow();
            }
        }

        [MenuItem("Window/MCP For Unity/Local Setup Window", priority = 2)]
        public static void ShowSetupWindow()
        {
            SetupWindowService.ShowSetupWindow();
        }


        [MenuItem("Window/MCP For Unity/Edit EditorPrefs", priority = 3)]
        public static void ShowEditorPrefsWindow()
        {
            EditorPrefsWindow.ShowWindow();
        }

        [MenuItem("Window/MCP For Unity/Dependencies/Show Status", priority = 99)]
        public static void ShowDependencyStatus()
        {
            var sb = new StringBuilder();

            // --- Bundled provisioning -------------------------------------------------
            sb.AppendLine("Bundled uv + Python");
            sb.AppendLine($"  Provisioned : {(BundledDependencyInstaller.IsProvisioned ? "yes" : "pending")}");
            if (BundledDependencyInstaller.LastResult.HasValue)
            {
                sb.AppendLine($"  Last result : {BundledDependencyInstaller.LastResult}");
            }
            string runtimeUv = BundledDependencyInstaller.GetRuntimeUvPath();
            sb.AppendLine($"  Runtime uv  : {runtimeUv}");
            sb.AppendLine($"  Exists      : {(File.Exists(runtimeUv) ? "yes" : "no")}");

            // --- Transport state ------------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("MCP Bridge");
            try
            {
                bool httpRunning = MCPServiceLocator.TransportManager.IsRunning(TransportMode.Http);
                bool stdioRunning = MCPServiceLocator.TransportManager.IsRunning(TransportMode.Stdio);
                sb.AppendLine($"  HTTP bridge : {(httpRunning ? "running" : "stopped")}");
                sb.AppendLine($"  Stdio bridge: {(stdioRunning ? "running" : "stopped")}");
            }
            catch (System.Exception ex)
            {
                sb.AppendLine($"  (transport manager unavailable: {ex.Message})");
            }

            // --- Local HTTP server reachability --------------------------------------
            sb.AppendLine();
            sb.AppendLine("Local HTTP Server");
            try
            {
                bool reachable = MCPServiceLocator.Server.IsLocalHttpServerReachable();
                sb.AppendLine($"  Reachable   : {(reachable ? "yes" : "no")}");
            }
            catch (System.Exception ex)
            {
                sb.AppendLine($"  (probe failed: {ex.Message})");
            }

            string report = sb.ToString();
            Debug.Log("[MCP for Unity] " + report);
            EditorUtility.DisplayDialog("MCP for Unity — Status", report, "OK");
        }

        [MenuItem("Window/MCP For Unity/Dependencies/Reinstall Bundled uv + Python", priority = 100)]
        public static void ReinstallBundledDependencies()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Reinstall Bundled uv + Python",
                "Re-copy the bundled uv binary to the user runtime folder and re-run `uv python install`. " +
                "This runs in the background; a dialog will appear when it completes. Continue?",
                "Reinstall",
                "Cancel");
            if (!confirm) return;

            void OnDone(BundledDependencyInstaller.InstallResult result)
            {
                BundledDependencyInstaller.ProvisioningCompleted -= OnDone;
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "MCP for Unity",
                    $"Bundled dependency provisioning finished: {result}",
                    "OK");
            }

            BundledDependencyInstaller.ProvisioningCompleted += OnDone;
            EditorUtility.DisplayProgressBar(
                "MCP for Unity",
                "Provisioning bundled uv + Python in background…",
                0.3f);
            BundledDependencyInstaller.BeginProvisioning(force: true);
        }
    }
}
