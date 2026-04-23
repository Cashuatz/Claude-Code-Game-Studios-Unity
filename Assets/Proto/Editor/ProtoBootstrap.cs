#if UNITY_EDITOR
using System.IO;
using Proto.Camera;
using Proto.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Proto.EditorTools
{
    /// <summary>
    /// Proto 프로토타입 씬을 1클릭으로 빌드한다.
    /// 메뉴: Proto/Bootstrap/Build ProtoMain Scene
    ///
    /// 생성 대상:
    ///  - SO 7개 (Movement 3 + Camera Mode 3 + Transition 1)
    ///  - 프리팹 2개 (Unit_Base, CameraRig)
    ///  - 씬 1개 (ProtoMain.unity) + 배치 (Plane/Light/Unit/Rig)
    ///
    /// 모든 경로는 _conventions.md 스펙과 일치.
    /// </summary>
    public static class ProtoBootstrap
    {
        private const string DataConfigRoot = "Assets/Proto/Data/Config";
        private const string PrefabRoot = "Assets/Proto/Prefabs";
        private const string SceneRoot = "Assets/Proto/Scenes";
        private const string ProtoMainScenePath = SceneRoot + "/ProtoMain.unity";

        [MenuItem("Proto/Bootstrap/Build ProtoMain Scene", priority = 1)]
        public static void BuildAll()
        {
            EnsureFolders();

            // 1. ScriptableObject 에셋 생성
            var moveDefault = CreateOrUpdate<MovementProfile>(
                $"{DataConfigRoot}/Movement_Default.asset",
                p => { p.radius = 0.4f; p.height = 1.8f; p.maxSpeed = 5f; });

            CreateOrUpdate<MovementProfile>(
                $"{DataConfigRoot}/Movement_Heavy.asset",
                p => { p.radius = 0.5f; p.height = 2.0f; p.maxSpeed = 3f; p.acceleration = 20f; });

            CreateOrUpdate<MovementProfile>(
                $"{DataConfigRoot}/Movement_Light.asset",
                p => { p.radius = 0.35f; p.height = 1.6f; p.maxSpeed = 7f; p.acceleration = 60f; });

            var camBack = CreateOrUpdate<CameraModeConfig>(
                $"{DataConfigRoot}/Camera_BackView.asset",
                c => { c.mode = CameraMode.BackView;
                       c.offset = new Vector3(0f, 2f, -5f);
                       c.lookAtLocalOffset = new Vector3(0f, 1.5f, 0f);
                       c.fov = 55f; });

            var camQuarter = CreateOrUpdate<CameraModeConfig>(
                $"{DataConfigRoot}/Camera_QuarterView.asset",
                c => { c.mode = CameraMode.QuarterView;
                       c.offset = new Vector3(0f, 10f, -10f);
                       c.lookAtLocalOffset = new Vector3(0f, 0f, 2f);
                       c.fov = 35f; });

            var camSide = CreateOrUpdate<CameraModeConfig>(
                $"{DataConfigRoot}/Camera_SideView.asset",
                c => { c.mode = CameraMode.SideView;
                       c.offset = new Vector3(10f, 2f, 0f);
                       c.lookAtLocalOffset = new Vector3(0f, 1.5f, 0f);
                       c.fov = 45f; });

            var camTransition = CreateOrUpdate<TransitionProfile>(
                $"{DataConfigRoot}/Camera_Transition_Default.asset",
                t => { t.durationSec = 0.6f;
                       t.positionEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                       t.rotationEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                       t.fovEase = AnimationCurve.Linear(0f, 0f, 1f, 1f); });

            AssetDatabase.SaveAssets();

            // 2. Unit_Base.prefab 생성
            var unitPath = $"{PrefabRoot}/Movement/Unit_Base.prefab";
            var unitGo = new GameObject("Unit_Base");
            var cap = unitGo.AddComponent<CapsuleCollider>();
            cap.direction = 1;
            cap.radius = 0.4f;
            cap.height = 1.8f;
            cap.center = new Vector3(0f, 0.9f, 0f);
            cap.isTrigger = false;
            var agent = unitGo.AddComponent<MovementAgent>();
            SetPrivateSerialized(agent, "_profile", moveDefault);

            var unitPrefab = PrefabUtility.SaveAsPrefabAsset(unitGo, unitPath);
            Object.DestroyImmediate(unitGo);

            // 3. CameraRig.prefab 생성
            var rigPath = $"{PrefabRoot}/Camera/CameraRig.prefab";
            var rigGo = new GameObject("CameraRig");
            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(rigGo.transform, false);
            var cam = camGo.AddComponent<UnityEngine.Camera>();
            camGo.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 55f;

            var rig = rigGo.AddComponent<CameraRig>();
            SetPrivateSerialized(rig, "_camera", cam);
            SetPrivateSerialized(rig, "_backView", camBack);
            SetPrivateSerialized(rig, "_quarterView", camQuarter);
            SetPrivateSerialized(rig, "_sideView", camSide);
            SetPrivateSerialized(rig, "_defaultTransition", camTransition);

            var rigPrefab = PrefabUtility.SaveAsPrefabAsset(rigGo, rigPath);
            Object.DestroyImmediate(rigGo);

            AssetDatabase.Refresh();

            // 4. ProtoMain.unity 씬 생성/배치 (EmptyScene; 조명/그라운드/유닛/카메라는 수동 배치)
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);

            // Directional light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Unit 배치
            var unitInstance = (GameObject)PrefabUtility.InstantiatePrefab(unitPrefab);
            unitInstance.transform.position = new Vector3(0f, 0.01f, 0f);

            // CameraRig 배치
            var rigInstance = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            rigInstance.transform.position = Vector3.zero;

            // 씬 저장
            EditorSceneManager.SaveScene(scene, ProtoMainScenePath);

            // Build Settings 에 씬 추가 (맨 위)
            AddSceneToBuildSettings(ProtoMainScenePath);

            EditorUtility.DisplayDialog("Proto Bootstrap",
                "ProtoMain 씬이 생성되었습니다.\n\n" +
                $"- SO: {DataConfigRoot}\n" +
                $"- Prefabs: {PrefabRoot}/Movement, {PrefabRoot}/Camera\n" +
                $"- Scene: {ProtoMainScenePath}\n\n" +
                "씬을 열고 Play 하면 정지 상태에서 Unit 을 카메라가 BackView 로 비춥니다.",
                "확인");

            Debug.Log("[ProtoBootstrap] Build complete: scene + 2 prefabs + 7 SOs.");
        }

        [MenuItem("Proto/Bootstrap/Open ProtoMain Scene", priority = 2)]
        public static void OpenScene()
        {
            if (!File.Exists(ProtoMainScenePath))
            {
                EditorUtility.DisplayDialog("Proto", "ProtoMain.unity 가 없습니다. 먼저 'Build ProtoMain Scene' 을 실행하세요.", "확인");
                return;
            }
            EditorSceneManager.OpenScene(ProtoMainScenePath, OpenSceneMode.Single);
        }

        // -------- helpers --------

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Proto");
            EnsureFolder("Assets/Proto/Data");
            EnsureFolder("Assets/Proto/Data/Config");
            EnsureFolder("Assets/Proto/Prefabs");
            EnsureFolder("Assets/Proto/Prefabs/Movement");
            EnsureFolder("Assets/Proto/Prefabs/Camera");
            EnsureFolder("Assets/Proto/Scenes");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static T CreateOrUpdate<T>(string path, System.Action<T> configure) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(existing, path);
            }
            configure?.Invoke(existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void SetPrivateSerialized(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"[ProtoBootstrap] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var current = EditorBuildSettings.scenes;
            foreach (var s in current)
            {
                if (s.path == scenePath) return;
            }

            var list = new EditorBuildSettingsScene[current.Length + 1];
            list[0] = new EditorBuildSettingsScene(scenePath, enabled: true);
            for (int i = 0; i < current.Length; i++) list[i + 1] = current[i];
            EditorBuildSettings.scenes = list;
        }
    }
}
#endif
