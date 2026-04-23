#if UNITY_EDITOR
using System.Collections.Generic;
using Proto.Stage.PCG;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Proto.EditorTools.Stage.PCG
{
    public class CityscapeWindow : EditorWindow
    {
        // editor-layout 색상 팔레트
        private static readonly Color WindowBackground = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color PanelBackground  = new Color(0.235f, 0.235f, 0.235f, 1f);
        private static readonly Color PanelBorder      = new Color(0.17f, 0.17f, 0.17f, 1f);
        private static readonly Color HeaderBackground = new Color(0.235f, 0.235f, 0.235f, 1f);
        private static readonly Color AccentColor      = new Color(0.36f, 0.36f, 0.36f, 1f);
        private static readonly Color SubtleText       = new Color(0.78f, 0.78f, 0.78f, 1f);
        private static readonly Color WarnAccent       = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color InfoAccent       = new Color(0.4f, 0.6f, 1f, 1f);
        private static readonly Color LogBackground    = new Color(0.16f, 0.16f, 0.16f, 1f);

        private const string RootObjectName = "_CityscapeRoot";
        private const string ProfileDefaultPath = "Assets/Proto/Data/Config/Cityscape_Default.asset";

        private CityscapeProfile _profile;
        private CityscapeAssetSet _assetSet;
        private int _seed = 12345;
        private CityscapeMetrics _lastMetrics;
        private bool _hasMetrics;
        private readonly List<string> _logs = new List<string>(256);

        private ObjectField _profileField;
        private ObjectField _assetSetField;
        private IntegerField _seedField;
        private TextField _metricsField;
        private ScrollView _logScroll;

        [MenuItem("Proto/Stage/Cityscape Window")]
        public static void Open()
        {
            var win = GetWindow<CityscapeWindow>("Cityscape PCG");
            win.minSize = new Vector2(420, 640);
        }

        private void OnEnable()
        {
            _profile  = AssetDatabase.LoadAssetAtPath<CityscapeProfile>(ProfileDefaultPath);
            _assetSet = CityscapeAssetBuilder.LoadDefaultAssetSet();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = WindowBackground;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            BuildControlsSection(root);
            BuildMetricsSection(root);
            BuildLogsSection(root);

            RefreshMetricsField();
            RefreshLogScroll();
        }

        // ---------- sections ----------

        private void BuildControlsSection(VisualElement root)
        {
            VisualElement body;
            var section = CreateSectionShell("Controls", "PCG", out body);

            // Profile 슬롯
            _profileField = new ObjectField("Profile")
            {
                objectType = typeof(CityscapeProfile),
                allowSceneObjects = false,
                value = _profile,
            };
            _profileField.RegisterValueChangedCallback(evt => _profile = evt.newValue as CityscapeProfile);
            StyleRowField(_profileField);
            body.Add(_profileField);

            var ensureProfileBtn = new Button(EnsureDefaultProfile) { text = "Ensure Default Profile" };
            StyleButton(ensureProfileBtn, AccentColor, height: 22);
            ensureProfileBtn.style.marginTop = 4;
            body.Add(ensureProfileBtn);

            // AssetSet 슬롯
            _assetSetField = new ObjectField("Asset Set")
            {
                objectType = typeof(CityscapeAssetSet),
                allowSceneObjects = false,
                value = _assetSet,
            };
            _assetSetField.RegisterValueChangedCallback(evt => _assetSet = evt.newValue as CityscapeAssetSet);
            StyleRowField(_assetSetField);
            _assetSetField.style.marginTop = 8;
            body.Add(_assetSetField);

            var ensureAssetsBtn = new Button(BuildDefaultAssets) { text = "Build Default Asset Set (6 prefabs + 6 mats)" };
            StyleButton(ensureAssetsBtn, AccentColor, height: 22);
            ensureAssetsBtn.style.marginTop = 4;
            body.Add(ensureAssetsBtn);

            // Seed row
            var seedRow = new VisualElement();
            seedRow.style.flexDirection = FlexDirection.Row;
            seedRow.style.alignItems = Align.Center;
            seedRow.style.marginTop = 10;

            _seedField = new IntegerField("Seed") { value = _seed };
            _seedField.style.flexGrow = 1;
            _seedField.RegisterValueChangedCallback(evt => _seed = evt.newValue);
            StyleRowField(_seedField);
            seedRow.Add(_seedField);

            var randSeedBtn = new Button(RandomizeSeed) { text = "Rand" };
            randSeedBtn.style.width = 48;
            randSeedBtn.style.marginLeft = 4;
            StyleButton(randSeedBtn, AccentColor, height: 20);
            seedRow.Add(randSeedBtn);
            body.Add(seedRow);

            // Generate / Clear
            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.marginTop = 10;

            var genBtn = new Button(OnGenerate) { text = "Generate" };
            genBtn.style.flexGrow = 1;
            StyleButton(genBtn, InfoAccent, height: 30);
            btnRow.Add(genBtn);

            var clearBtn = new Button(OnClear) { text = "Clear" };
            clearBtn.style.flexGrow = 1;
            clearBtn.style.marginLeft = 4;
            StyleButton(clearBtn, WarnAccent, height: 30);
            btnRow.Add(clearBtn);
            body.Add(btnRow);

            var autoBtn = new Button(OnAutoTest3Seeds) { text = "Auto Test (seeds 1, 42, 9999)" };
            autoBtn.style.marginTop = 6;
            StyleButton(autoBtn, AccentColor, height: 26);
            body.Add(autoBtn);

            var focusBtn = new Button(OnFocusSceneOnRoot) { text = "Focus Scene on Cityscape" };
            focusBtn.style.marginTop = 4;
            StyleButton(focusBtn, AccentColor, height: 22);
            body.Add(focusBtn);

            root.Add(section);
        }

        private void BuildMetricsSection(VisualElement root)
        {
            VisualElement body;
            var section = CreateSectionShell("Metrics", "RESULT", out body);

            _metricsField = new TextField
            {
                multiline = true,
                isReadOnly = true,
                value = "(no run yet)",
            };
            _metricsField.style.minHeight = 160;
            _metricsField.style.whiteSpace = WhiteSpace.Normal;
            _metricsField.style.color = SubtleText;
            _metricsField.style.fontSize = 11;
            // TextField 내부 필드 스타일
            var inner = _metricsField.Q("unity-text-input");
            if (inner != null)
            {
                inner.style.backgroundColor = LogBackground;
                inner.style.color = SubtleText;
                inner.style.paddingLeft = 6;
                inner.style.paddingRight = 6;
                inner.style.paddingTop = 4;
                inner.style.paddingBottom = 4;
            }
            body.Add(_metricsField);

            var copyBtn = new Button(CopyMetricsToClipboard) { text = "Copy Metrics to Clipboard" };
            copyBtn.style.marginTop = 4;
            StyleButton(copyBtn, AccentColor, height: 22);
            body.Add(copyBtn);

            root.Add(section);
        }

        private void BuildLogsSection(VisualElement root)
        {
            VisualElement body;
            var section = CreateSectionShell("Logs", "DEBUG", out body);

            _logScroll = new ScrollView(ScrollViewMode.Vertical);
            _logScroll.style.height = 200;
            _logScroll.style.backgroundColor = LogBackground;
            _logScroll.style.borderTopWidth = 1;
            _logScroll.style.borderBottomWidth = 1;
            _logScroll.style.borderLeftWidth = 1;
            _logScroll.style.borderRightWidth = 1;
            _logScroll.style.borderTopColor = PanelBorder;
            _logScroll.style.borderBottomColor = PanelBorder;
            _logScroll.style.borderLeftColor = PanelBorder;
            _logScroll.style.borderRightColor = PanelBorder;
            _logScroll.style.paddingLeft = 6;
            _logScroll.style.paddingRight = 6;
            _logScroll.style.paddingTop = 4;
            _logScroll.style.paddingBottom = 4;
            body.Add(_logScroll);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.marginTop = 4;

            var clearLogBtn = new Button(() => { _logs.Clear(); RefreshLogScroll(); }) { text = "Clear Logs" };
            clearLogBtn.style.flexGrow = 1;
            StyleButton(clearLogBtn, AccentColor, height: 20);
            btnRow.Add(clearLogBtn);

            var copyLogBtn = new Button(CopyLogsToClipboard) { text = "Copy Logs" };
            copyLogBtn.style.flexGrow = 1;
            copyLogBtn.style.marginLeft = 4;
            StyleButton(copyLogBtn, AccentColor, height: 20);
            btnRow.Add(copyLogBtn);

            body.Add(btnRow);

            root.Add(section);
        }

        // ---------- actions ----------

        private void OnGenerate()
        {
            if (_profile == null)
            {
                _logs.Add("[Cityscape] Profile is null. Click 'Ensure Default Profile' first.");
                RefreshLogScroll();
                return;
            }
            if (_assetSet == null || !_assetSet.IsComplete())
            {
                _logs.Add("[Cityscape] AssetSet missing or incomplete. Click 'Build Default Asset Set' first.");
                RefreshLogScroll();
                return;
            }

            var root = EnsureRootObject();
            var res = CityscapeGenerator.Generate(root.transform, _seed, _profile, _assetSet, _logs);

            if (res.Status == CityscapeGenerator.GenerateStatus.Ok)
            {
                _lastMetrics = res.Metrics;
                _hasMetrics = true;
                RefreshMetricsField();
            }
            else
            {
                _logs.Add($"[Cityscape] Generate failed: {res.Status} — {res.Reason}");
            }

            RefreshLogScroll();
            EditorSceneManager.MarkSceneDirty(root.scene);
        }

        private void OnClear()
        {
            var root = GameObject.Find(RootObjectName);
            if (root != null)
            {
                CityscapeGenerator.Clear(root.transform);
                _logs.Add("[Cityscape] Cleared scene root children.");
            }
            else _logs.Add("[Cityscape] No root to clear.");
            RefreshLogScroll();
        }

        private void OnAutoTest3Seeds()
        {
            int[] seeds = { 1, 42, 9999 };
            _logs.Add("=== Auto Test Start ===");
            foreach (var s in seeds)
            {
                _seed = s;
                _seedField?.SetValueWithoutNotify(s);
                OnGenerate();
            }
            _logs.Add("=== Auto Test End ===");
            RefreshLogScroll();
        }

        private void OnFocusSceneOnRoot()
        {
            var root = GameObject.Find(RootObjectName);
            if (root == null)
            {
                _logs.Add("[Cityscape] No root found in scene.");
                RefreshLogScroll();
                return;
            }
            Selection.activeGameObject = root;
            SceneView.FrameLastActiveSceneView();
        }

        private void RandomizeSeed()
        {
            _seed = UnityEngine.Random.Range(1, int.MaxValue);
            _seedField?.SetValueWithoutNotify(_seed);
        }

        private GameObject EnsureRootObject()
        {
            var root = GameObject.Find(RootObjectName);
            if (root == null)
            {
                root = new GameObject(RootObjectName);
                Undo.RegisterCreatedObjectUndo(root, "Create Cityscape Root");
            }
            return root;
        }

        private void EnsureDefaultProfile()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CityscapeProfile>(ProfileDefaultPath);
            if (existing == null)
            {
                EnsureFolder("Assets/Proto/Data");
                EnsureFolder("Assets/Proto/Data/Config");
                existing = ScriptableObject.CreateInstance<CityscapeProfile>();
                AssetDatabase.CreateAsset(existing, ProfileDefaultPath);
                AssetDatabase.SaveAssets();
                _logs.Add($"[Cityscape] Created default profile at {ProfileDefaultPath}");
            }
            else _logs.Add($"[Cityscape] Default profile already exists at {ProfileDefaultPath}");

            _profile = existing;
            _profileField?.SetValueWithoutNotify(existing);
            RefreshLogScroll();
        }

        private void BuildDefaultAssets()
        {
            var set = CityscapeAssetBuilder.BuildDefaults();
            _assetSet = set;
            _assetSetField?.SetValueWithoutNotify(set);
            _logs.Add($"[Cityscape] Default asset set ready. Missing after build: {set.DescribeMissing()}");
            RefreshLogScroll();
        }

        private void CopyMetricsToClipboard()
        {
            var text = _hasMetrics ? _lastMetrics.ToReport() : "(no run yet)";
            EditorGUIUtility.systemCopyBuffer = text;
            _logs.Add("[Cityscape] Metrics copied to clipboard.");
            RefreshLogScroll();
        }

        private void CopyLogsToClipboard()
        {
            var text = string.Join("\n", _logs);
            EditorGUIUtility.systemCopyBuffer = text;
            _logs.Add("[Cityscape] Logs copied to clipboard.");
            RefreshLogScroll();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ---------- refresh ----------

        private void RefreshMetricsField()
        {
            if (_metricsField == null) return;
            _metricsField.SetValueWithoutNotify(_hasMetrics ? _lastMetrics.ToReport() : "(no run yet — Generate 버튼을 누르세요)");
        }

        private void RefreshLogScroll()
        {
            if (_logScroll == null) return;
            _logScroll.Clear();
            int start = Mathf.Max(0, _logs.Count - 200);
            for (int i = start; i < _logs.Count; i++)
            {
                var line = new Label(_logs[i]);
                line.style.color = SubtleText;
                line.style.fontSize = 10;
                line.style.whiteSpace = WhiteSpace.Normal;
                _logScroll.Add(line);
            }
            _logScroll.schedule.Execute(() =>
            {
                if (_logScroll?.verticalScroller != null)
                    _logScroll.verticalScroller.value = _logScroll.verticalScroller.highValue;
            });
        }

        // ---------- editor-layout helpers ----------

        private VisualElement CreateSectionShell(string title, string badge, out VisualElement bodyContainer)
        {
            var shell = new VisualElement();
            shell.style.flexDirection = FlexDirection.Column;
            shell.style.backgroundColor = PanelBackground;
            shell.style.overflow = Overflow.Hidden;
            shell.style.marginBottom = 10f;
            shell.style.borderLeftWidth = 1f;
            shell.style.borderRightWidth = 1f;
            shell.style.borderTopWidth = 1f;
            shell.style.borderBottomWidth = 1f;
            shell.style.borderLeftColor = PanelBorder;
            shell.style.borderRightColor = PanelBorder;
            shell.style.borderTopColor = PanelBorder;
            shell.style.borderBottomColor = PanelBorder;
            shell.Add(CreateSectionHeader(title, badge));
            shell.Add(CreateSectionAccentBar());

            bodyContainer = new VisualElement();
            bodyContainer.style.flexGrow = 1f;
            bodyContainer.style.flexDirection = FlexDirection.Column;
            bodyContainer.style.paddingLeft = 14f;
            bodyContainer.style.paddingRight = 14f;
            bodyContainer.style.paddingTop = 12f;
            bodyContainer.style.paddingBottom = 14f;
            bodyContainer.style.backgroundColor = PanelBackground;
            shell.Add(bodyContainer);
            return shell;
        }

        private VisualElement CreateSectionHeader(string title, string badgeText)
        {
            var header = new VisualElement();
            header.style.height = 40f;
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.backgroundColor = HeaderBackground;
            header.style.paddingLeft = 14f;
            header.style.paddingRight = 14f;

            var leftGroup = new VisualElement();
            leftGroup.style.flexDirection = FlexDirection.Row;
            leftGroup.style.alignItems = Align.Center;

            var accent = new VisualElement();
            accent.style.width = 2f;
            accent.style.height = 18f;
            accent.style.backgroundColor = AccentColor;
            accent.style.marginRight = 8f;
            leftGroup.Add(accent);

            var titleLabel = new Label(title);
            titleLabel.style.color = Color.white;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 12;
            leftGroup.Add(titleLabel);
            header.Add(leftGroup);

            if (!string.IsNullOrEmpty(badgeText))
            {
                var badge = new Label(badgeText.ToUpperInvariant());
                badge.style.color = SubtleText;
                badge.style.unityFontStyleAndWeight = FontStyle.Bold;
                badge.style.fontSize = 9;
                badge.style.paddingLeft = 8f;
                badge.style.paddingRight = 8f;
                badge.style.paddingTop = 2f;
                badge.style.paddingBottom = 2f;
                header.Add(badge);
            }
            return header;
        }

        private VisualElement CreateSectionAccentBar()
        {
            var bar = new VisualElement();
            bar.style.height = 1f;
            bar.style.backgroundColor = PanelBorder;
            return bar;
        }

        private static void StyleRowField(VisualElement field)
        {
            field.style.marginTop = 4;
            var label = field.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = 80;
                label.style.color = SubtleText;
            }
        }

        private static void StyleButton(Button btn, Color borderAccent, float height)
        {
            btn.style.height = height;
            btn.style.borderLeftWidth = 2;
            btn.style.borderLeftColor = borderAccent;
            btn.style.color = Color.white;
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
    }
}
#endif
