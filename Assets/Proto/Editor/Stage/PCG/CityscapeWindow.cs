#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Proto.Stage.PCG;
using Proto.TD.Level.Wfc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Proto.EditorTools.Stage.PCG
{
    /// <summary>
    /// 시가지 PCG 통합 에디터 윈도우.
    /// - Block Subdivision 모드: CityscapeGenerator (프리미티브 Instantiate 기반)
    /// - WFC Hybrid 모드: TdWfcLevelSpawner → TdLevelWfcAdapter (BlockLayout + A* + WFC)
    ///
    /// 두 모드는 별도 씬 루트(_CityscapeRoot / _WfcRoot)에 배치되며, Clear 는 모드별로
    /// 동작한다. 메트릭·로그는 공용 섹션에 표시한다.
    /// </summary>
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

        private enum AlgorithmMode { BlockSubdivision, WfcHybrid }

        private const string BlockRootObjectName = "_CityscapeRoot";
        private const string WfcRootObjectName   = "_WfcRoot";
        private const string ProfileDefaultPath  = "Assets/Proto/Data/Config/Cityscape_Default.asset";

        private AlgorithmMode _mode = AlgorithmMode.BlockSubdivision;

        // Block mode state
        private CityscapeProfile _profile;
        private CityscapeAssetSet _assetSet;
        private int _blockSeed = 12345;
        private CityscapeMetrics _lastBlockMetrics;
        private bool _hasBlockMetrics;

        // WFC mode state
        private int    _wfcGridSize   = 40;
        private string _wfcSeedHex    = "C0FFEE";
        private int    _wfcLaneCount  = 2;
        private Vector2Int _wfcCoreBase = new Vector2Int(20, 20);
        private Vector2Int _wfcPowerA   = new Vector2Int(5, 5);
        private Vector2Int _wfcPowerB   = new Vector2Int(34, 34);
        private double _wfcLinkBudget  = 100.0;
        private bool   _wfcWriteJson   = true;
        private string _wfcJsonFileName = "Level_Wfc_Sample_Medium.json";
        private bool _hasWfcMetrics;
        private TdLevelWfcAdapter.Result _lastWfcResult;

        // Logs (공용)
        private readonly List<string> _logs = new List<string>(256);

        // UI handles
        private EnumField _modeField;
        private VisualElement _blockSection;
        private VisualElement _wfcSection;

        private ObjectField _profileField;
        private ObjectField _assetSetField;
        private IntegerField _blockSeedField;

        private IntegerField _wfcGridSizeField;
        private TextField   _wfcSeedField;
        private IntegerField _wfcLaneField;
        private Vector2IntField _wfcCoreBaseField;
        private Vector2IntField _wfcPowerAField;
        private Vector2IntField _wfcPowerBField;
        private DoubleField _wfcLinkBudgetField;
        private Toggle _wfcWriteJsonToggle;
        private TextField _wfcJsonNameField;

        private TextField _metricsField;
        private ScrollView _logScroll;

        [MenuItem("Proto/Stage/Cityscape Window", priority = 20)]
        public static void Open()
        {
            var win = GetWindow<CityscapeWindow>("Cityscape PCG");
            win.minSize = new Vector2(460, 780);
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

            BuildAlgorithmSection(root);
            BuildBlockSection(root);
            BuildWfcSection(root);
            BuildActionSection(root);
            BuildMetricsSection(root);
            BuildLogsSection(root);

            RefreshModeVisibility();
            RefreshMetricsField();
            RefreshLogScroll();
        }

        // ---------- algorithm mode ----------

        private void BuildAlgorithmSection(VisualElement root)
        {
            VisualElement body;
            var section = CreateSectionShell("Algorithm", "MODE", out body);

            _modeField = new EnumField("Algorithm", _mode);
            _modeField.RegisterValueChangedCallback(evt =>
            {
                _mode = (AlgorithmMode)evt.newValue;
                RefreshModeVisibility();
                RefreshMetricsField();
            });
            StyleRowField(_modeField);
            body.Add(_modeField);

            var helpBox = new Label(
                "• BlockSubdivision: 기존 시가지 그레이박싱 (프리미티브 직접 배치)\n" +
                "• WfcHybrid: TD 레벨 — 블록 계획 + A* 경로 + WFC 인접 검증");
            helpBox.style.color = SubtleText;
            helpBox.style.fontSize = 10;
            helpBox.style.marginTop = 6;
            helpBox.style.whiteSpace = WhiteSpace.Normal;
            body.Add(helpBox);

            root.Add(section);
        }

        private void RefreshModeVisibility()
        {
            if (_blockSection != null)
                _blockSection.style.display = _mode == AlgorithmMode.BlockSubdivision
                    ? DisplayStyle.Flex : DisplayStyle.None;
            if (_wfcSection != null)
                _wfcSection.style.display = _mode == AlgorithmMode.WfcHybrid
                    ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ---------- block subdivision section ----------

        private void BuildBlockSection(VisualElement root)
        {
            VisualElement body;
            _blockSection = CreateSectionShell("Block Subdivision — Controls", "PCG", out body);

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

            // Seed
            var seedRow = new VisualElement();
            seedRow.style.flexDirection = FlexDirection.Row;
            seedRow.style.alignItems = Align.Center;
            seedRow.style.marginTop = 10;

            _blockSeedField = new IntegerField("Seed") { value = _blockSeed };
            _blockSeedField.style.flexGrow = 1;
            _blockSeedField.RegisterValueChangedCallback(evt => _blockSeed = evt.newValue);
            StyleRowField(_blockSeedField);
            seedRow.Add(_blockSeedField);

            var randSeedBtn = new Button(RandomizeBlockSeed) { text = "Rand" };
            randSeedBtn.style.width = 48;
            randSeedBtn.style.marginLeft = 4;
            StyleButton(randSeedBtn, AccentColor, height: 20);
            seedRow.Add(randSeedBtn);
            body.Add(seedRow);

            root.Add(_blockSection);
        }

        // ---------- WFC section ----------

        private void BuildWfcSection(VisualElement root)
        {
            VisualElement body;
            _wfcSection = CreateSectionShell("WFC Hybrid — Controls", "PCG+WFC", out body);

            _wfcGridSizeField = new IntegerField("Grid Size") { value = _wfcGridSize };
            _wfcGridSizeField.RegisterValueChangedCallback(evt => _wfcGridSize = Mathf.Max(10, evt.newValue));
            StyleRowField(_wfcGridSizeField);
            body.Add(_wfcGridSizeField);

            _wfcSeedField = new TextField("Seed (hex)") { value = _wfcSeedHex };
            _wfcSeedField.RegisterValueChangedCallback(evt => _wfcSeedHex = evt.newValue);
            StyleRowField(_wfcSeedField);
            _wfcSeedField.style.marginTop = 4;
            body.Add(_wfcSeedField);

            var seedBtnRow = new VisualElement();
            seedBtnRow.style.flexDirection = FlexDirection.Row;
            seedBtnRow.style.marginTop = 4;
            var randHexBtn = new Button(RandomizeWfcSeed) { text = "Rand Hex Seed" };
            randHexBtn.style.flexGrow = 1;
            StyleButton(randHexBtn, AccentColor, height: 20);
            seedBtnRow.Add(randHexBtn);
            body.Add(seedBtnRow);

            _wfcLaneField = new IntegerField("Lane Count") { value = _wfcLaneCount };
            _wfcLaneField.RegisterValueChangedCallback(evt => _wfcLaneCount = Mathf.Clamp(evt.newValue, 1, 4));
            StyleRowField(_wfcLaneField);
            _wfcLaneField.style.marginTop = 4;
            body.Add(_wfcLaneField);

            _wfcCoreBaseField = new Vector2IntField("Core Base") { value = _wfcCoreBase };
            _wfcCoreBaseField.RegisterValueChangedCallback(evt => _wfcCoreBase = evt.newValue);
            StyleRowField(_wfcCoreBaseField);
            _wfcCoreBaseField.style.marginTop = 4;
            body.Add(_wfcCoreBaseField);

            _wfcPowerAField = new Vector2IntField("Power A") { value = _wfcPowerA };
            _wfcPowerAField.RegisterValueChangedCallback(evt => _wfcPowerA = evt.newValue);
            StyleRowField(_wfcPowerAField);
            _wfcPowerAField.style.marginTop = 4;
            body.Add(_wfcPowerAField);

            _wfcPowerBField = new Vector2IntField("Power B") { value = _wfcPowerB };
            _wfcPowerBField.RegisterValueChangedCallback(evt => _wfcPowerB = evt.newValue);
            StyleRowField(_wfcPowerBField);
            _wfcPowerBField.style.marginTop = 4;
            body.Add(_wfcPowerBField);

            _wfcLinkBudgetField = new DoubleField("Link Budget") { value = _wfcLinkBudget };
            _wfcLinkBudgetField.RegisterValueChangedCallback(evt => _wfcLinkBudget = evt.newValue);
            StyleRowField(_wfcLinkBudgetField);
            _wfcLinkBudgetField.style.marginTop = 4;
            body.Add(_wfcLinkBudgetField);

            _wfcWriteJsonToggle = new Toggle("Write JSON to Assets") { value = _wfcWriteJson };
            _wfcWriteJsonToggle.RegisterValueChangedCallback(evt => _wfcWriteJson = evt.newValue);
            StyleRowField(_wfcWriteJsonToggle);
            _wfcWriteJsonToggle.style.marginTop = 6;
            body.Add(_wfcWriteJsonToggle);

            _wfcJsonNameField = new TextField("JSON Name") { value = _wfcJsonFileName };
            _wfcJsonNameField.RegisterValueChangedCallback(evt => _wfcJsonFileName = evt.newValue);
            StyleRowField(_wfcJsonNameField);
            _wfcJsonNameField.style.marginTop = 4;
            body.Add(_wfcJsonNameField);

            root.Add(_wfcSection);
        }

        // ---------- actions (mode-dispatching) ----------

        private void BuildActionSection(VisualElement root)
        {
            VisualElement body;
            var section = CreateSectionShell("Actions", "RUN", out body);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;

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

            var autoBtn = new Button(OnAutoTest3) { text = "Auto Test (3 seeds)" };
            autoBtn.style.marginTop = 6;
            StyleButton(autoBtn, AccentColor, height: 26);
            body.Add(autoBtn);

            var focusBtn = new Button(OnFocusSceneOnRoot) { text = "Focus Scene on Active Root" };
            focusBtn.style.marginTop = 4;
            StyleButton(focusBtn, AccentColor, height: 22);
            body.Add(focusBtn);

            root.Add(section);
        }

        // ---------- metrics / logs ----------

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
            _metricsField.style.minHeight = 180;
            _metricsField.style.whiteSpace = WhiteSpace.Normal;
            _metricsField.style.color = SubtleText;
            _metricsField.style.fontSize = 11;
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
            _logScroll.style.height = 180;
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

        // ---------- actions dispatching ----------

        private void OnGenerate()
        {
            switch (_mode)
            {
                case AlgorithmMode.BlockSubdivision: GenerateBlock(); break;
                case AlgorithmMode.WfcHybrid:        GenerateWfc();   break;
            }
        }

        private void OnClear()
        {
            switch (_mode)
            {
                case AlgorithmMode.BlockSubdivision: ClearBlock(); break;
                case AlgorithmMode.WfcHybrid:        ClearWfc();   break;
            }
        }

        private void OnAutoTest3()
        {
            if (_mode == AlgorithmMode.BlockSubdivision)
            {
                int[] seeds = { 1, 42, 9999 };
                _logs.Add("=== Auto Test Start (Block) ===");
                foreach (var s in seeds) { _blockSeed = s; _blockSeedField?.SetValueWithoutNotify(s); GenerateBlock(); }
                _logs.Add("=== Auto Test End (Block) ===");
            }
            else
            {
                string[] seeds = { "1", "42", "C0FFEE" };
                _logs.Add("=== Auto Test Start (WFC) ===");
                foreach (var s in seeds) { _wfcSeedHex = s; _wfcSeedField?.SetValueWithoutNotify(s); GenerateWfc(); }
                _logs.Add("=== Auto Test End (WFC) ===");
            }
            RefreshLogScroll();
        }

        private void OnFocusSceneOnRoot()
        {
            string rootName = _mode == AlgorithmMode.BlockSubdivision ? BlockRootObjectName : WfcRootObjectName;
            var root = GameObject.Find(rootName);
            if (root == null) { _logs.Add($"[Cityscape] No root '{rootName}' found in scene."); RefreshLogScroll(); return; }
            Selection.activeGameObject = root;
            SceneView.FrameLastActiveSceneView();
        }

        // ---------- Block mode actions ----------

        private void GenerateBlock()
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

            var root = EnsureRoot(BlockRootObjectName);
            var res = CityscapeGenerator.Generate(root.transform, _blockSeed, _profile, _assetSet, _logs);

            if (res.Status == CityscapeGenerator.GenerateStatus.Ok)
            {
                _lastBlockMetrics = res.Metrics;
                _hasBlockMetrics = true;
                RefreshMetricsField();
            }
            else
            {
                _logs.Add($"[Cityscape] Generate failed: {res.Status} — {res.Reason}");
            }
            RefreshLogScroll();
            EditorSceneManager.MarkSceneDirty(root.scene);
        }

        private void ClearBlock()
        {
            var root = GameObject.Find(BlockRootObjectName);
            if (root != null) { CityscapeGenerator.Clear(root.transform); _logs.Add("[Cityscape] Block root cleared."); }
            else _logs.Add("[Cityscape] No block root to clear.");
            RefreshLogScroll();
        }

        private void RandomizeBlockSeed()
        {
            _blockSeed = UnityEngine.Random.Range(1, int.MaxValue);
            _blockSeedField?.SetValueWithoutNotify(_blockSeed);
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

        // ---------- WFC mode actions ----------

        private void GenerateWfc()
        {
            var rootGo = EnsureRoot(WfcRootObjectName);
            var spawner = rootGo.GetComponent<TdWfcLevelSpawner>();
            if (spawner == null) spawner = rootGo.AddComponent<TdWfcLevelSpawner>();

            spawner.gridSize = _wfcGridSize;
            spawner.seedHex = string.IsNullOrWhiteSpace(_wfcSeedHex) ? "1" : _wfcSeedHex.Trim();
            spawner.laneCount = _wfcLaneCount;
            spawner.coreBase = _wfcCoreBase;
            spawner.powerSources = new[] { _wfcPowerA, _wfcPowerB };
            spawner.linkBudget = _wfcLinkBudget;
            spawner.writeJsonToAssets = _wfcWriteJson;
            spawner.jsonFileName = _wfcJsonFileName;
            spawner.autoGenerateOnStart = false;
            spawner.centerAtOrigin = true;
            spawner.cellSize = 1f;
            spawner.spawnRoot = rootGo.transform; // 모든 타일 큐브를 _WfcRoot 바로 아래에

            _logs.Add($"[Wfc] Generate grid={_wfcGridSize} seed=0x{spawner.seedHex} lanes={_wfcLaneCount}");
            spawner.Generate();

            if (spawner.HasResult)
            {
                _lastWfcResult = spawner.LastResult;
                _hasWfcMetrics = true;
                if (!_lastWfcResult.Success)
                    _logs.Add($"[Wfc] FAIL status={_lastWfcResult.WfcStatus} reason={_lastWfcResult.Reason} contradict=({_lastWfcResult.ContradictionX},{_lastWfcResult.ContradictionY})");
                else
                    _logs.Add($"[Wfc] Done iterations={_lastWfcResult.Iterations} paths={(_lastWfcResult.Paths==null?0:_lastWfcResult.Paths.Count)} blocks={_lastWfcResult.BlockStats.Blocks}");
                RefreshMetricsField();
            }
            RefreshLogScroll();
            EditorSceneManager.MarkSceneDirty(rootGo.scene);
        }

        private void ClearWfc()
        {
            var root = GameObject.Find(WfcRootObjectName);
            if (root != null)
            {
                var t = root.transform;
                for (int i = t.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(t.GetChild(i).gameObject);
                _logs.Add("[Wfc] Root children cleared.");
            }
            else _logs.Add("[Wfc] No WFC root to clear.");
            RefreshLogScroll();
        }

        private void RandomizeWfcSeed()
        {
            ulong v = ((ulong)(uint)UnityEngine.Random.Range(1, int.MaxValue) << 32) |
                      (uint)UnityEngine.Random.Range(1, int.MaxValue);
            _wfcSeedHex = v.ToString("X");
            _wfcSeedField?.SetValueWithoutNotify(_wfcSeedHex);
        }

        // ---------- shared helpers ----------

        private static GameObject EnsureRoot(string name)
        {
            var root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(root, $"Create {name}");
            }
            return root;
        }

        private void CopyMetricsToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = ComposeMetricsReport();
            _logs.Add("[Cityscape] Metrics copied to clipboard.");
            RefreshLogScroll();
        }

        private void CopyLogsToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = string.Join("\n", _logs);
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

        // ---------- metrics report ----------

        private void RefreshMetricsField()
        {
            if (_metricsField == null) return;
            _metricsField.SetValueWithoutNotify(ComposeMetricsReport());
        }

        private string ComposeMetricsReport()
        {
            if (_mode == AlgorithmMode.BlockSubdivision)
            {
                if (!_hasBlockMetrics) return "(no Block run yet — Generate 버튼을 누르세요)";
                var sb = new StringBuilder(256);
                sb.AppendLine("Algorithm : BlockSubdivision");
                sb.Append(_lastBlockMetrics.ToReport());
                return sb.ToString();
            }
            else
            {
                if (!_hasWfcMetrics) return "(no WFC run yet — Generate 버튼을 누르세요)";
                return ComposeWfcReport(_lastWfcResult);
            }
        }

        private string ComposeWfcReport(TdLevelWfcAdapter.Result r)
        {
            var sb = new StringBuilder(512);
            sb.AppendLine("Algorithm : WfcHybrid");
            sb.AppendLine($"Seed      : 0x{_wfcSeedHex}");
            sb.AppendLine($"Grid      : {_wfcGridSize} x {_wfcGridSize}");
            sb.AppendLine($"Lanes     : {_wfcLaneCount}");
            sb.AppendLine($"Success   : {r.Success}");
            sb.AppendLine($"Status    : {r.WfcStatus}");
            if (!string.IsNullOrEmpty(r.Reason))
                sb.AppendLine($"Reason    : {r.Reason}");
            sb.AppendLine($"Iterations: {r.Iterations}");
            if (r.Paths != null)
            {
                int total = 0;
                var lens = new List<int>(r.Paths.Count);
                foreach (var lane in r.Paths) { lens.Add(lane.Count); total += lane.Count; }
                sb.AppendLine($"Paths     : {r.Paths.Count} (cells={total}; per-lane=[{string.Join(",", lens)}])");
            }
            var bs = r.BlockStats;
            sb.AppendLine($"Blocks    : {bs.Blocks}  (E{bs.Empty}/O{bs.Open}/M{bs.Mixed}/D{bs.Dense})");
            sb.AppendLine($"Cells     : building={bs.BuildingCells}  road={bs.RoadCells}");
            if (!r.Success && r.ContradictionX >= 0)
                sb.AppendLine($"Contradict: ({r.ContradictionX},{r.ContradictionY})");
            if (!string.IsNullOrEmpty(r.LevelJson) && _wfcWriteJson)
                sb.Append   ($"JSON      : Assets/Proto/Data/TD/Levels/{_wfcJsonFileName}");
            return sb.ToString();
        }

        // ---------- log refresh ----------

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
                label.style.minWidth = 100;
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
