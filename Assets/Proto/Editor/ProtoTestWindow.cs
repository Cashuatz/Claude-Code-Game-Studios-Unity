#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Proto.Camera;
using Proto.Movement;
using Proto.Testing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Proto.EditorTools
{
    /// <summary>
    /// Proto VKL 테스트 하네스 윈도우.
    /// 메뉴: Proto/Test Window
    ///
    /// 제공:
    ///  - Runtime Metrics: Unit.Position/Velocity/Grounded, CameraRig.CurrentMode/IsTransitioning, 타겟 수, FPS
    ///  - Manual Actions: Teleport / MoveBy / RequestMode / 스모크 테스트
    ///  - Event Log: ProtoTestBus 이벤트 타임라인
    ///  - VKL Export: 관측값을 .vkl/runtime/observations/OBS-YYYY-MM-DD-NNN.md 로 저장
    /// </summary>
    public sealed class ProtoTestWindow : EditorWindow
    {
        // ===== color palette (editor-layout 규칙) =====
        private static readonly Color windowBackground  = new Color(0.22f,  0.22f,  0.22f,  1f);
        private static readonly Color panelBackground   = new Color(0.235f, 0.235f, 0.235f, 1f);
        private static readonly Color panelBorderColor  = new Color(0.17f,  0.17f,  0.17f,  1f);
        private static readonly Color headerBackground  = new Color(0.235f, 0.235f, 0.235f, 1f);
        private static readonly Color accentColor       = new Color(0.36f,  0.36f,  0.36f,  1f);
        private static readonly Color subtleTextColor   = new Color(0.78f,  0.78f,  0.78f,  1f);

        // Emphasis colors
        private static readonly Color emphasisGeneral = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color emphasisInfo    = new Color(0.4f, 0.6f, 1f,   1f);
        private static readonly Color emphasisWarn    = new Color(1f,   0.6f, 0.2f, 1f);
        private static readonly Color emphasisError   = new Color(1f,   0.3f, 0.3f, 1f);

        // ===== cached refs =====
        private MovementAgent _agent;
        private CameraRig _rig;

        // ===== runtime displays =====
        private Label _statePlayLabel;
        private Label _posLabel;
        private Label _velLabel;
        private Label _groundedLabel;
        private Label _modeLabel;
        private Label _transitionLabel;
        private Label _targetCountLabel;
        private Label _fpsLabel;
        private Label _eventCountLabel;
        private ScrollView _logScroll;
        private Button _clearLogButton;
        private VisualElement _warningBox;
        private Label _warningLabel;

        // ===== fps estimator =====
        private int _fpsFrames;
        private double _fpsTimerStart;
        private float _fpsSmoothed;

        [MenuItem("Proto/Test Window", priority = 100)]
        public static void Open()
        {
            var wnd = GetWindow<ProtoTestWindow>();
            wnd.titleContent = new GUIContent("Proto Test");
            wnd.minSize = new Vector2(360f, 520f);
            wnd.Show();
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.backgroundColor = windowBackground;
            rootVisualElement.style.paddingLeft = 8f;
            rootVisualElement.style.paddingRight = 8f;
            rootVisualElement.style.paddingTop = 8f;
            rootVisualElement.style.paddingBottom = 8f;

            BuildUI();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            ProtoTestBus.OnEvent += OnBusEvent;
            ProtoTestBus.OnCleared += OnBusCleared;

            rootVisualElement.schedule.Execute(PollMetrics).Every(100);
            rootVisualElement.schedule.Execute(TickFps).Every(16);

            _fpsTimerStart = EditorApplication.timeSinceStartup;
            _fpsFrames = 0;

            RefreshAllLogs();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ProtoTestBus.OnEvent -= OnBusEvent;
            ProtoTestBus.OnCleared -= OnBusCleared;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                _agent = null;
                _rig = null;
            }
        }

        // ===== layout =====

        private void BuildUI()
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1f;
            rootVisualElement.Add(scroll);

            // 경고 박스 (Play 모드 아닐 때 표시)
            _warningBox = BuildEmphasisBox(emphasisWarn);
            _warningLabel = new Label("Play 모드가 아닙니다. 버튼 일부만 동작합니다.");
            _warningLabel.style.color = Color.white;
            _warningLabel.style.fontSize = 11;
            _warningBox.Add(_warningLabel);
            scroll.Add(_warningBox);

            scroll.Add(BuildRuntimeStateSection());
            scroll.Add(BuildActionsSection());
            scroll.Add(BuildEventLogSection());
            scroll.Add(BuildExportSection());
        }

        private VisualElement BuildRuntimeStateSection()
        {
            var section = CreateSectionShell("RUNTIME STATE", "LIVE", out var body);

            _statePlayLabel = AddMetricRow(body, "Play Mode");
            _posLabel       = AddMetricRow(body, "Unit.Position");
            _velLabel       = AddMetricRow(body, "Unit.Velocity");
            _groundedLabel  = AddMetricRow(body, "Unit.Grounded");
            _modeLabel      = AddMetricRow(body, "CameraRig.Mode");
            _transitionLabel= AddMetricRow(body, "Rig.Transitioning");
            _targetCountLabel = AddMetricRow(body, "FramingTargets");
            _fpsLabel       = AddMetricRow(body, "FPS (est.)");
            _eventCountLabel= AddMetricRow(body, "Event Log Size");

            return section;
        }

        private VisualElement BuildActionsSection()
        {
            var section = CreateSectionShell("MANUAL ACTIONS", "VKL", out var body);

            var noteBox = BuildEmphasisBox(emphasisInfo);
            var noteLbl = new Label("Play 모드에서 동작합니다. WASD=이동, 1/2/3=카메라 모드.");
            noteLbl.style.color = Color.white;
            noteLbl.style.fontSize = 11;
            noteLbl.style.whiteSpace = WhiteSpace.Normal;
            noteBox.Add(noteLbl);
            body.Add(noteBox);

            // Teleport / MoveBy
            var foldMove = new Foldout { text = "Movement", value = true };
            StyleFoldout(foldMove, "AGENT");
            foldMove.Add(MakeButton("Teleport to Origin", () => AgentOp(a => a.Teleport(Vector3.zero), "Teleport(0,0,0)")));
            foldMove.Add(MakeButton("Teleport +5 Z",      () => AgentOp(a => a.Teleport(new Vector3(0f, 0f, 5f)), "Teleport(0,0,5)")));
            foldMove.Add(MakeButton("MoveBy +1 X",         () => AgentOp(a => a.MoveBy(new Vector3(1f, 0f, 0f)), "MoveBy(+1,0,0)")));
            foldMove.Add(MakeButton("MoveBy -1 X",         () => AgentOp(a => a.MoveBy(new Vector3(-1f, 0f, 0f)), "MoveBy(-1,0,0)")));
            foldMove.Add(MakeButton("Halt",               () => AgentOp(a => { a.Halt(); return MoveResult.Ok; }, "Halt")));
            body.Add(foldMove);

            // Camera
            var foldCam = new Foldout { text = "Camera", value = true };
            StyleFoldout(foldCam, "RIG");
            foldCam.Add(MakeButton("Request BackView",    () => RigOp(r => r.RequestMode(CameraMode.BackView),    "Mode=BackView")));
            foldCam.Add(MakeButton("Request QuarterView", () => RigOp(r => r.RequestMode(CameraMode.QuarterView), "Mode=QuarterView")));
            foldCam.Add(MakeButton("Request SideView",    () => RigOp(r => r.RequestMode(CameraMode.SideView),    "Mode=SideView")));
            body.Add(foldCam);

            // Smoke Test
            var foldSmoke = new Foldout { text = "Smoke Test", value = true };
            StyleFoldout(foldSmoke, "SUITE");
            foldSmoke.Add(MakeButton("Run Smoke: TP → Move → Mode Cycle", RunSmokeTest));
            body.Add(foldSmoke);

            return section;
        }

        private VisualElement BuildEventLogSection()
        {
            var section = CreateSectionShell("EVENT LOG", "BUS", out var body);

            _logScroll = new ScrollView(ScrollViewMode.Vertical);
            _logScroll.style.maxHeight = 220f;
            _logScroll.style.minHeight = 120f;
            _logScroll.style.backgroundColor = windowBackground;
            _logScroll.style.borderLeftWidth = 1f;
            _logScroll.style.borderRightWidth = 1f;
            _logScroll.style.borderTopWidth = 1f;
            _logScroll.style.borderBottomWidth = 1f;
            _logScroll.style.borderLeftColor = panelBorderColor;
            _logScroll.style.borderRightColor = panelBorderColor;
            _logScroll.style.borderTopColor = panelBorderColor;
            _logScroll.style.borderBottomColor = panelBorderColor;
            _logScroll.style.paddingLeft = 6f;
            _logScroll.style.paddingRight = 6f;
            _logScroll.style.paddingTop = 4f;
            _logScroll.style.paddingBottom = 4f;
            body.Add(_logScroll);

            _clearLogButton = MakeButton("Clear Log", () =>
            {
                ProtoTestBus.Clear();
                ProtoTestBus.Emit(ProtoEventKind.Info, "Log cleared by user.");
            });
            _clearLogButton.style.marginTop = 8f;
            body.Add(_clearLogButton);

            return section;
        }

        private VisualElement BuildExportSection()
        {
            var section = CreateSectionShell("VKL EXPORT", "OBS", out var body);

            var box = BuildEmphasisBox(emphasisGeneral);
            var lbl = new Label(".vkl/runtime/observations/ 하위에 OBS-YYYY-MM-DD-NNN.md 로 저장합니다.");
            lbl.style.color = Color.white;
            lbl.style.fontSize = 11;
            lbl.style.whiteSpace = WhiteSpace.Normal;
            box.Add(lbl);
            body.Add(box);

            body.Add(MakeButton("Export Current State as VKL Observation", ExportObservation));

            return section;
        }

        // ===== shell / header / accent =====

        private VisualElement CreateSectionShell(string title, string badge, out VisualElement bodyContainer)
        {
            var shell = new VisualElement();
            shell.style.flexDirection = FlexDirection.Column;
            shell.style.backgroundColor = panelBackground;
            shell.style.overflow = Overflow.Hidden;
            shell.style.marginBottom = 10f;
            shell.style.borderLeftWidth = 1f;
            shell.style.borderRightWidth = 1f;
            shell.style.borderTopWidth = 1f;
            shell.style.borderBottomWidth = 1f;
            shell.style.borderLeftColor = panelBorderColor;
            shell.style.borderRightColor = panelBorderColor;
            shell.style.borderTopColor = panelBorderColor;
            shell.style.borderBottomColor = panelBorderColor;

            shell.Add(CreateSectionHeader(title, badge));
            shell.Add(CreateSectionAccentBar());

            bodyContainer = new VisualElement();
            bodyContainer.style.flexGrow = 1f;
            bodyContainer.style.flexDirection = FlexDirection.Column;
            bodyContainer.style.paddingLeft = 14f;
            bodyContainer.style.paddingRight = 14f;
            bodyContainer.style.paddingTop = 12f;
            bodyContainer.style.paddingBottom = 14f;
            bodyContainer.style.backgroundColor = panelBackground;

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
            header.style.backgroundColor = headerBackground;
            header.style.paddingLeft = 14f;
            header.style.paddingRight = 14f;

            var leftGroup = new VisualElement();
            leftGroup.style.flexDirection = FlexDirection.Row;
            leftGroup.style.alignItems = Align.Center;

            var accent = new VisualElement();
            accent.style.width = 2f;
            accent.style.height = 18f;
            accent.style.backgroundColor = accentColor;
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
                badge.style.color = subtleTextColor;
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
            bar.style.backgroundColor = panelBorderColor;
            return bar;
        }

        private void StyleFoldout(Foldout foldout, string badgeText)
        {
            if (foldout == null) return;
            foldout.style.marginBottom = 8f;
            foldout.style.backgroundColor = panelBackground;
            foldout.style.borderLeftWidth = 1f;
            foldout.style.borderRightWidth = 1f;
            foldout.style.borderTopWidth = 1f;
            foldout.style.borderBottomWidth = 1f;
            foldout.style.borderLeftColor = panelBorderColor;
            foldout.style.borderRightColor = panelBorderColor;
            foldout.style.borderTopColor = panelBorderColor;
            foldout.style.borderBottomColor = panelBorderColor;

            var toggle = foldout.Q<Toggle>();
            if (toggle != null)
            {
                toggle.style.backgroundColor = headerBackground;
                toggle.style.height = 26f;
                toggle.style.paddingLeft = 10f;
                toggle.style.paddingRight = 10f;
                toggle.style.alignItems = Align.Center;
                toggle.style.unityFontStyleAndWeight = FontStyle.Bold;
                toggle.style.color = subtleTextColor;

                if (!string.IsNullOrEmpty(badgeText))
                {
                    var badge = new Label(badgeText.ToUpperInvariant());
                    badge.style.color = subtleTextColor;
                    badge.style.fontSize = 9;
                    badge.style.paddingLeft = 8f;
                    badge.style.paddingRight = 8f;
                    toggle.Add(badge);
                }
            }

            var content = foldout.contentContainer;
            if (content != null)
            {
                content.style.paddingLeft = 12f;
                content.style.paddingRight = 12f;
                content.style.paddingTop = 10f;
                content.style.paddingBottom = 12f;
                content.style.backgroundColor = panelBackground;
            }
        }

        private VisualElement BuildEmphasisBox(Color leftBorder)
        {
            var box = new VisualElement();
            box.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            box.style.paddingLeft = 12f;
            box.style.paddingRight = 12f;
            box.style.paddingTop = 10f;
            box.style.paddingBottom = 10f;
            box.style.borderLeftWidth = 2f;
            box.style.borderLeftColor = leftBorder;
            box.style.marginBottom = 12f;
            return box;
        }

        // ===== helpers =====

        private Label AddMetricRow(VisualElement parent, string name)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4f;

            var nameLbl = new Label(name);
            nameLbl.style.width = 140f;
            nameLbl.style.flexShrink = 0;
            nameLbl.style.color = subtleTextColor;
            nameLbl.style.fontSize = 11;
            row.Add(nameLbl);

            var valueLbl = new Label("-");
            valueLbl.style.flexGrow = 1f;
            valueLbl.style.color = Color.white;
            valueLbl.style.fontSize = 11;
            valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(valueLbl);

            parent.Add(row);
            return valueLbl;
        }

        private Button MakeButton(string text, Action onClick)
        {
            var btn = new Button(onClick) { text = text };
            btn.style.height = 24f;
            btn.style.marginBottom = 4f;
            btn.style.fontSize = 11;
            return btn;
        }

        private void EnsureRefs()
        {
            if (_agent == null) _agent = FindFirstObjectByType<MovementAgent>();
            if (_rig == null) _rig = FindFirstObjectByType<CameraRig>();
        }

        private MoveResult AgentOp(Func<MovementAgent, MoveResult> op, string label)
        {
            if (!Application.isPlaying)
            {
                ProtoTestBus.Emit(ProtoEventKind.Warning, $"Action ignored (not in play): {label}");
                return MoveResult.Invalid;
            }
            EnsureRefs();
            if (_agent == null)
            {
                ProtoTestBus.Emit(ProtoEventKind.Error, "MovementAgent not found in scene.");
                return MoveResult.Invalid;
            }
            var result = op(_agent);
            ProtoTestBus.Emit(ProtoEventKind.Action, $"{label} -> {result}");
            return result;
        }

        private void RigOp(Action<CameraRig> op, string label)
        {
            if (!Application.isPlaying)
            {
                ProtoTestBus.Emit(ProtoEventKind.Warning, $"Action ignored (not in play): {label}");
                return;
            }
            EnsureRefs();
            if (_rig == null)
            {
                ProtoTestBus.Emit(ProtoEventKind.Error, "CameraRig not found in scene.");
                return;
            }
            op(_rig);
            ProtoTestBus.Emit(ProtoEventKind.Action, label);
        }

        private void RunSmokeTest()
        {
            if (!Application.isPlaying)
            {
                ProtoTestBus.Emit(ProtoEventKind.Warning, "Smoke test requires play mode.");
                return;
            }
            EnsureRefs();
            if (_agent == null || _rig == null)
            {
                ProtoTestBus.Emit(ProtoEventKind.Error, "Agent/Rig not found; smoke aborted.");
                return;
            }

            ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: start");
            _agent.Teleport(Vector3.zero);
            ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: teleport(0,0,0)");
            _agent.MoveBy(new Vector3(1f, 0f, 0f));
            ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: moveBy(+1,0,0)");
            _rig.RequestMode(CameraMode.QuarterView);
            ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: mode=QuarterView (transition starts)");

            // 딜레이 콜로 다음 단계
            EditorApplication.delayCall += () =>
            {
                if (_rig != null) _rig.RequestMode(CameraMode.SideView);
                ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: mode=SideView (delayed)");
            };
            EditorApplication.delayCall += () =>
            {
                if (_rig != null) _rig.RequestMode(CameraMode.BackView);
                ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: mode=BackView (delayed)");
            };
            EditorApplication.delayCall += () =>
            {
                ProtoTestBus.Emit(ProtoEventKind.Action, "SMOKE: end");
            };
        }

        // ===== polling =====

        private void PollMetrics()
        {
            EnsureRefs();
            if (_statePlayLabel != null)
                _statePlayLabel.text = Application.isPlaying ? "PLAY" : "EDIT";

            if (_warningBox != null)
                _warningBox.style.display = Application.isPlaying ? DisplayStyle.None : DisplayStyle.Flex;

            if (_agent != null)
            {
                if (_posLabel != null) _posLabel.text = Fmt(_agent.Position);
                if (_velLabel != null) _velLabel.text = Fmt(_agent.Velocity);
                if (_groundedLabel != null) _groundedLabel.text = _agent.IsGrounded ? "true" : "false";
            }
            else
            {
                SetUnknown(_posLabel, _velLabel, _groundedLabel);
            }

            if (_rig != null)
            {
                if (_modeLabel != null) _modeLabel.text = _rig.CurrentMode.ToString();
                if (_transitionLabel != null) _transitionLabel.text = _rig.IsTransitioning ? "true" : "false";
                int count = 0;
                foreach (var t in EnumerateFramingTargets()) count++;
                if (_targetCountLabel != null) _targetCountLabel.text = count.ToString();
            }
            else
            {
                SetUnknown(_modeLabel, _transitionLabel, _targetCountLabel);
            }

            if (_fpsLabel != null) _fpsLabel.text = _fpsSmoothed > 0f ? _fpsSmoothed.ToString("F0") : "-";
            if (_eventCountLabel != null) _eventCountLabel.text = ProtoTestBus.Count.ToString();
        }

        private IEnumerable<Proto.Camera.IFramingTarget> EnumerateFramingTargets()
        {
            var all = FindObjectsByType<ProtoUnitTarget>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++) yield return all[i];
        }

        private void SetUnknown(params Label[] labels)
        {
            foreach (var lbl in labels) if (lbl != null) lbl.text = "-";
        }

        private string Fmt(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        private void TickFps()
        {
            _fpsFrames++;
            double now = EditorApplication.timeSinceStartup;
            double elapsed = now - _fpsTimerStart;
            if (elapsed >= 0.5)
            {
                _fpsSmoothed = (float)(_fpsFrames / elapsed);
                _fpsFrames = 0;
                _fpsTimerStart = now;
            }
        }

        // ===== log handling =====

        private void OnBusEvent(ProtoEvent evt)
        {
            AppendLogRow(evt);
        }

        private void OnBusCleared()
        {
            if (_logScroll != null) _logScroll.Clear();
        }

        private void RefreshAllLogs()
        {
            if (_logScroll == null) return;
            _logScroll.Clear();
            var list = ProtoTestBus.Events;
            for (int i = 0; i < list.Count; i++) AppendLogRow(list[i]);
        }

        private void AppendLogRow(ProtoEvent evt)
        {
            if (_logScroll == null) return;
            var row = new Label(FormatEvent(evt));
            row.style.fontSize = 10;
            row.style.color = ColorForKind(evt.kind);
            row.style.unityTextAlign = TextAnchor.UpperLeft;
            row.style.whiteSpace = WhiteSpace.Normal;
            row.style.marginBottom = 2f;
            _logScroll.Add(row);

            // auto-scroll to bottom
            _logScroll.schedule.Execute(() =>
            {
                var scroller = _logScroll.verticalScroller;
                if (scroller != null) scroller.value = scroller.highValue;
            }).StartingIn(16);
        }

        private static string FormatEvent(ProtoEvent evt)
        {
            return $"[t={evt.timeAsDouble,7:F3}] {evt.kind,-20} {evt.detail}";
        }

        private static Color ColorForKind(ProtoEventKind kind)
        {
            switch (kind)
            {
                case ProtoEventKind.Error:     return new Color(1f, 0.35f, 0.35f);
                case ProtoEventKind.Warning:   return new Color(1f, 0.75f, 0.3f);
                case ProtoEventKind.Collision: return new Color(1f, 0.55f, 0.3f);
                case ProtoEventKind.Action:    return new Color(0.55f, 0.8f, 1f);
                case ProtoEventKind.ModeChange:
                case ProtoEventKind.TransitionCompleted: return new Color(0.7f, 0.9f, 0.6f);
                case ProtoEventKind.Move:
                case ProtoEventKind.Teleport:
                case ProtoEventKind.GroundState: return new Color(0.85f, 0.85f, 0.85f);
                case ProtoEventKind.Info:
                default: return new Color(0.7f, 0.7f, 0.7f);
            }
        }

        // ===== VKL export =====

        private void ExportObservation()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            string dir = Path.Combine(projectRoot, ".vkl/runtime/observations").Replace('\\', '/');
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int n = 1;
            string path;
            do
            {
                path = Path.Combine(dir, $"OBS-{today}-{n:D3}.md").Replace('\\', '/');
                n++;
            } while (File.Exists(path));

            var sb = new StringBuilder();
            sb.AppendLine($"# Observation OBS-{today}-{(n - 1):D3}");
            sb.AppendLine();
            sb.AppendLine("## Context");
            sb.AppendLine($"- Emitted: {DateTime.Now:O}");
            sb.AppendLine($"- Play Mode: {Application.isPlaying}");
            sb.AppendLine($"- Scene: {UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path}");
            sb.AppendLine($"- Unity Version: {Application.unityVersion}");
            sb.AppendLine();

            EnsureRefs();
            sb.AppendLine("## Metrics Snapshot");
            sb.AppendLine("| Key | Value |");
            sb.AppendLine("|---|---|");
            sb.AppendLine($"| Unit.Position | {(_agent != null ? Fmt(_agent.Position) : "N/A")} |");
            sb.AppendLine($"| Unit.Velocity | {(_agent != null ? Fmt(_agent.Velocity) : "N/A")} |");
            sb.AppendLine($"| Unit.IsGrounded | {(_agent != null ? _agent.IsGrounded.ToString() : "N/A")} |");
            sb.AppendLine($"| CameraRig.CurrentMode | {(_rig != null ? _rig.CurrentMode.ToString() : "N/A")} |");
            sb.AppendLine($"| CameraRig.IsTransitioning | {(_rig != null ? _rig.IsTransitioning.ToString() : "N/A")} |");
            int tgt = 0; foreach (var _ in EnumerateFramingTargets()) tgt++;
            sb.AppendLine($"| FramingTargets | {tgt} |");
            sb.AppendLine($"| FPS (est.) | {_fpsSmoothed:F0} |");
            sb.AppendLine();

            sb.AppendLine("## Recent Events");
            var list = ProtoTestBus.Events;
            int start = Mathf.Max(0, list.Count - 50);
            for (int i = start; i < list.Count; i++)
            {
                var e = list[i];
                sb.AppendLine($"{i - start + 1}. `[t={e.timeAsDouble:F3}]` **{e.kind}** — {e.detail}");
            }
            sb.AppendLine();

            sb.AppendLine("## Oracle Candidates");
            sb.AppendLine("- OR-COMPILE-01 (컴파일 에러 0 확인 필요)");
            sb.AppendLine("- OR-RUNTIME-01 (런타임 에러 0 — 이 관측 시점 기준)");
            sb.AppendLine("- OR-HARD-RULE-01 (HR-1 Rigidbody 부재 검증 필요)");
            sb.AppendLine();
            sb.AppendLine("## Notes");
            sb.AppendLine("(필요 시 사용자가 수동으로 추가)");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            ProtoTestBus.Emit(ProtoEventKind.Info, $"VKL OBS exported: {path}");
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
