#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Proto.Launcher;

namespace Proto.EditorTools
{
    /// <summary>
    /// 런처·3장르 껍데기 씬을 에디터에서 한 방에 구성한다.
    /// 메뉴: Proto → Phase 0 → ...
    /// </summary>
    public static class LauncherSetup
    {
        private const string LauncherPath = "Assets/Proto/Scenes/Proto_Launcher.unity";
        private const string Turn3dPath = "Assets/Proto/Scenes/Proto_Turn3d.unity";
        private const string TdPath = "Assets/Proto/Scenes/Proto_TD.unity";
        private const string RailPath = "Assets/Proto/Scenes/Proto_RailShooter.unity";

        [MenuItem("Proto/Phase0_BuildLauncher")]
        public static void BuildLauncherUI()
        {
            var scene = EditorSceneManager.OpenScene(LauncherPath, OpenSceneMode.Single);
            ClearRoots(scene);

            CreateCamera(scene);
            CreateEventSystem(scene);
            CreateLauncherCanvas(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[LauncherSetup] Proto_Launcher 구성 완료");
        }

        [MenuItem("Proto/Phase0_BuildShells")]
        public static void BuildGenreShells()
        {
            BuildShell(Turn3dPath, "Turn3d — 블아 파티 턴제 (Phase 2 예정)", new Color(0.92f, 0.62f, 0.55f));
            BuildShell(TdPath, "TD — 엔드필드 허브 네트워크 (Phase 1 예정)", new Color(0.90f, 0.80f, 0.30f));
            BuildShell(RailPath, "Rail-Shooter — 좀비 섹션 (Phase 3 예정)", new Color(0.50f, 0.70f, 0.95f));
            Debug.Log("[LauncherSetup] 3 genre shells built.");
        }

        [MenuItem("Proto/Phase0_BuildAll")]
        public static void BuildAll()
        {
            BuildLauncherUI();
            BuildGenreShells();
        }

        private static void BuildShell(string path, string title, Color accent)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ClearRoots(scene);

            CreateCamera(scene, new Vector3(0f, 5f, -10f), new Vector3(20f, 0f, 0f));
            CreateLight(scene);
            CreateGround(scene);
            CreateEventSystem(scene);
            CreateShellCanvas(scene, title, accent);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ClearRoots(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateCamera(Scene scene, Vector3? pos = null, Vector3? euler = null)
        {
            var go = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.tag = "MainCamera";
            var cam = go.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 1f);
            go.AddComponent<AudioListener>();
            go.transform.position = pos ?? new Vector3(0f, 1f, -10f);
            go.transform.eulerAngles = euler ?? Vector3.zero;
            return go;
        }

        private static GameObject CreateLight(Scene scene)
        {
            var go = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(go, scene);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            go.transform.eulerAngles = new Vector3(50f, -30f, 0f);
            return go;
        }

        private static GameObject CreateGround(Scene scene)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.localScale = new Vector3(5f, 1f, 5f);
            return go;
        }

        private static GameObject CreateEventSystem(Scene scene)
        {
            var go = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            return go;
        }

        private static GameObject CreateLauncherCanvas(Scene scene)
        {
            var canvasGo = MakeCanvas(scene, "Canvas");

            CreateFullscreenImage(canvasGo.transform, "Background", new Color(0.08f, 0.08f, 0.10f, 1f));

            var title = CreateText(canvasGo.transform, "Title",
                "PROTO LAUNCHER\n3장르 데모 선택",
                new Vector2(0f, -100f), new Vector2(1400f, 240f), 64, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            var btn1 = CreateButton(canvasGo.transform, "Button_Turn3d",
                "[1] Turn3d — 블아 스타일 파티 턴제",
                new Vector2(0f, 160f), new Color(0.92f, 0.62f, 0.55f, 1f));
            var btn2 = CreateButton(canvasGo.transform, "Button_TD",
                "[2] TD — 엔드필드 허브 네트워크",
                new Vector2(0f, 0f), new Color(0.90f, 0.80f, 0.30f, 1f));
            var btn3 = CreateButton(canvasGo.transform, "Button_RailShooter",
                "[3] Rail-Shooter — 하우스오브더데드류",
                new Vector2(0f, -160f), new Color(0.50f, 0.70f, 0.95f, 1f));

            var status = CreateText(canvasGo.transform, "StatusLabel",
                "버튼 또는 숫자키 1/2/3",
                new Vector2(0f, 80f), new Vector2(1400f, 60f), 26, new Color(0.78f, 0.78f, 0.78f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            status.alignment = TextAlignmentOptions.Center;

            var ui = canvasGo.AddComponent<LauncherUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("_turn3dButton").objectReferenceValue = btn1;
            so.FindProperty("_tdButton").objectReferenceValue = btn2;
            so.FindProperty("_railShooterButton").objectReferenceValue = btn3;
            so.FindProperty("_statusLabel").objectReferenceValue = status;
            so.FindProperty("_enableHotkeys").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            return canvasGo;
        }

        private static GameObject CreateShellCanvas(Scene scene, string titleText, Color accent)
        {
            var canvasGo = MakeCanvas(scene, "Canvas");

            var banner = new GameObject("Banner");
            banner.transform.SetParent(canvasGo.transform, false);
            var bannerImg = banner.AddComponent<Image>();
            bannerImg.color = new Color(accent.r, accent.g, accent.b, 0.25f);
            var bannerRt = banner.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0f, 1f);
            bannerRt.anchorMax = new Vector2(1f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0f, 0f);
            bannerRt.sizeDelta = new Vector2(0f, 120f);

            var title = CreateText(banner.transform, "Title", titleText,
                new Vector2(0f, 0f), new Vector2(-40f, 0f), 36, Color.white,
                new Vector2(0f, 0f), new Vector2(1f, 1f));
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.offsetMin = new Vector2(30f, 10f);
            titleRt.offsetMax = new Vector2(-30f, -10f);
            title.alignment = TextAlignmentOptions.MidlineLeft;

            CreateText(canvasGo.transform, "Hint",
                "Esc — 런처로 복귀 (후속 스프린트에서 구현)",
                new Vector2(0f, 60f), new Vector2(1000f, 40f), 22, new Color(0.78f, 0.78f, 0.78f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

            return canvasGo;
        }

        private static GameObject MakeCanvas(Scene scene, string name)
        {
            var canvasGo = new GameObject(name);
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        private static void CreateFullscreenImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text,
            Vector2 anchoredPos, Vector2 size, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Color color)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var img = btnGo.AddComponent<Image>();
            img.color = color;
            var btn = btnGo.AddComponent<Button>();
            var rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(680f, 120f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(btnGo.transform, false);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.08f, 0.08f, 0.10f, 1f);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            return btn;
        }
    }
}
#endif
