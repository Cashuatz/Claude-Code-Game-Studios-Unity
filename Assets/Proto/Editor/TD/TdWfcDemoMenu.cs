using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Proto.TD.Level.Wfc;
using UnityCamera = UnityEngine.Camera;
using UnityLight = UnityEngine.Light;

namespace Proto.TD.Editor
{
    internal static class TdWfcDemoMenu
    {
        private const string ScenePath = "Assets/Proto/Scenes/TdWfcDemo.unity";
        private const string SpawnerName = "WfcSpawner";

        [MenuItem("Tools/Proto TD/Generate WFC Demo Level")]
        public static void GenerateDemo()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    scene = EditorSceneManager.GetActiveScene();
                }
                else
                {
                    Debug.LogWarning("[TdWfcDemo] cancelled");
                    return;
                }
            }

            var spawnerGO = GameObject.Find(SpawnerName);
            if (spawnerGO == null)
            {
                spawnerGO = new GameObject(SpawnerName);
                spawnerGO.AddComponent<TdWfcLevelSpawner>();
            }
            var spawner = spawnerGO.GetComponent<TdWfcLevelSpawner>();
            if (spawner == null) spawner = spawnerGO.AddComponent<TdWfcLevelSpawner>();

            spawner.gridSize = 60;
            spawner.seedHex = "C0FFEE";
            spawner.laneCount = 2;
            spawner.coreBase = new Vector2Int(30, 30);
            spawner.powerSources = new[] { new Vector2Int(8, 8), new Vector2Int(52, 52) };
            spawner.autoGenerateOnStart = false;
            spawner.writeJsonToAssets = true;
            spawner.Generate();

            EnsureLighting();
            EnsureCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[TdWfcDemo] done");
        }

        private static void EnsureLighting()
        {
            if (GameObject.Find("Directional Light") != null) return;
            var light = new GameObject("Directional Light");
            var l = light.AddComponent<UnityLight>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
            l.color = Color.white;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void EnsureCamera()
        {
            var cam = UnityCamera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<UnityCamera>();
                go.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            cam.orthographic = false;
            cam.fieldOfView = 40f;
            cam.transform.position = new Vector3(0f, 70f, -42f);
            cam.transform.rotation = Quaternion.Euler(62f, 0f, 0f);
        }
    }
}
