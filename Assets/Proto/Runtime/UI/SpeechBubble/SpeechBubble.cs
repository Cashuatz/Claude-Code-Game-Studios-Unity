using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 네임스페이스 충돌 회피: 이 어셈블리에 Proto.Camera 네임스페이스가 존재하므로
// 부모 네임스페이스 스코프에서 Camera 가 Proto.Camera(namespace) 로 해석된다.
// UnityEngine.Camera(type) 을 명확히 가리키기 위해 alias 를 둔다.
using UnityCamera = UnityEngine.Camera;

namespace Proto.UI.Speech
{
    /// <summary>
    /// 캐릭터 머리 위 말풍선 (Screen Space - Overlay 기반).
    ///
    /// 렌더 정책:
    ///  - World Space Canvas 를 쓰지 않는 이유: URP Render Scale 변경에 영향받기 때문.
    ///  - Overlay Canvas 는 렌더 타겟 독립. LateUpdate 에서 월드 타겟을 WorldToScreenPoint 로
    ///    스크린 좌표로 바꿔 RectTransform.position 에 찍음 → 카메라를 따라다니는 "빌보드" 효과 자동.
    ///
    /// 외형:
    ///  - 본체: 9-slice 라운디드 사각형 (SpeechBubbleSpriteFactory 프로시저 생성)
    ///  - 꼬리: 본체 하단 중앙에서 아래로 뾰족한 삼각형 (pivot = (0.5, 1))
    ///
    /// 동작:
    ///  - 타이핑: uGUI Text.text.Substring 방식 (한글 포함 모든 유니코드 OK)
    ///  - 소멸 정책: <see cref="BubbleDismissMode"/>
    ///  - 동시성 정책: <see cref="BubbleConcurrencyMode"/>
    ///
    /// 생성:
    ///  (A) Inspector 에 직접 배치 (Canvas 자식으로 본체/꼬리/텍스트 구성 후 레퍼런스 드래그)
    ///  (B) <see cref="Create"/> 정적 팩토리로 한 줄에 생성.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpeechBubble : MonoBehaviour
    {
        // ───── 추적 타겟 ─────
        [Header("Target (World)")]
        [Tooltip("말풍선이 따라붙을 월드 트랜스폼. null 이면 transform.position 고정.")]
        [SerializeField] private Transform _target;
        [Tooltip("타겟 피벗 기준 월드 오프셋 (보통 Y = 캐릭터 머리 위).")]
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [Tooltip("비우면 UnityCamera.main 자동 캐시.")]
        [SerializeField] private UnityCamera _camera;
        [Tooltip("타겟이 카메라 뒤로 갔을 때 말풍선을 숨길지.")]
        [SerializeField] private bool _hideWhenBehindCamera = true;

        // ───── 시각 참조 ─────
        [Header("Visual References")]
        [SerializeField] private RectTransform _bodyRect;
        [SerializeField] private Image _bodyImage;
        [SerializeField] private Image _tailImage;
        [SerializeField] private Text _text;

        // ───── 외형 ─────
        [Header("Appearance")]
        [SerializeField] private Color _bubbleColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color _textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField, Min(16)] private int _roundedTextureSize = 64;
        [SerializeField, Min(0)] private int _cornerRadius = 16;
        [SerializeField] private Vector2 _tailSize = new Vector2(20f, 14f);
        [Tooltip("꼬리 anchoredPosition.y (본체 하단 중앙 기준, 보통 0).")]
        [SerializeField] private float _tailAnchoredY = 0f;

        // ───── 타이핑 ─────
        [Header("Typing")]
        [SerializeField, Min(1f)] private float _charsPerSecond = 30f;

        // ───── 동작 정책 ─────
        [Header("Behavior")]
        [SerializeField] private BubbleDismissMode _defaultDismissMode = BubbleDismissMode.AutoHide;
        [SerializeField, Min(0f)] private float _defaultHoldSeconds = 2f;
        [SerializeField] private BubbleConcurrencyMode _concurrencyMode = BubbleConcurrencyMode.Overwrite;

        // ───── 내부 상태 ─────
        private Coroutine _routine;
        private readonly Queue<PendingLine> _pending = new();
        private bool _isSpeaking;
        private bool _appearanceApplied;
        private string _appendBuffer = string.Empty;

        private readonly struct PendingLine
        {
            public readonly string Text;
            public readonly BubbleDismissMode Mode;
            public readonly float Hold;
            public PendingLine(string t, BubbleDismissMode m, float h) { Text = t; Mode = m; Hold = h; }
        }

        // ───── 공개 프로퍼티 ─────
        public bool IsSpeaking => _isSpeaking;
        public BubbleConcurrencyMode ConcurrencyMode { get => _concurrencyMode; set => _concurrencyMode = value; }

        // ───── Unity 라이프사이클 ─────

        private void Awake()
        {
            if (_camera == null) _camera = UnityCamera.main;
            EnsureAppearance();
            if (_bodyRect != null) _bodyRect.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_camera == null) _camera = UnityCamera.main;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            if (_camera == null)
            {
                _camera = UnityCamera.main;
                if (_camera == null) return;
            }

            var worldPos = _target.position + _worldOffset;
            var sp = _camera.WorldToScreenPoint(worldPos);

            if (sp.z < 0f && _hideWhenBehindCamera)
            {
                if (_bodyRect != null && _bodyRect.gameObject.activeSelf)
                    _bodyRect.gameObject.SetActive(false);
                return;
            }
            // 카메라 뒤에서 앞으로 돌아왔고 아직 말하는 중이면 복구
            if (_bodyRect != null && _isSpeaking && !_bodyRect.gameObject.activeSelf)
                _bodyRect.gameObject.SetActive(true);

            // Screen Space - Overlay 기준: transform.position 을 픽셀 좌표로 직접 세팅
            transform.position = new Vector3(sp.x, sp.y, 0f);
        }

        // ───── 공개 API ─────

        public void SetTarget(Transform target, Vector3? worldOffset = null)
        {
            _target = target;
            if (worldOffset.HasValue) _worldOffset = worldOffset.Value;
        }

        public void SetCamera(UnityCamera cam) => _camera = cam;

        public void Show(string text) => Show(text, null, null);

        public void Show(string text, BubbleDismissMode? dismiss, float? holdSeconds)
        {
            var mode = dismiss ?? _defaultDismissMode;
            var hold = holdSeconds ?? _defaultHoldSeconds;

            switch (_concurrencyMode)
            {
                case BubbleConcurrencyMode.Overwrite:
                    StopRoutine();
                    _pending.Clear();
                    _appendBuffer = string.Empty;
                    _routine = StartCoroutine(PlayRoutine(text, mode, hold));
                    break;
                case BubbleConcurrencyMode.Queue:
                    _pending.Enqueue(new PendingLine(text, mode, hold));
                    if (!_isSpeaking) ProcessNext();
                    break;
                case BubbleConcurrencyMode.Ignore:
                    if (_isSpeaking) return;
                    _routine = StartCoroutine(PlayRoutine(text, mode, hold));
                    break;
                case BubbleConcurrencyMode.Append:
                    if (_isSpeaking) _appendBuffer += text;
                    else _routine = StartCoroutine(PlayRoutine(text, mode, hold));
                    break;
            }
        }

        public void Hide()
        {
            StopRoutine();
            _pending.Clear();
            _appendBuffer = string.Empty;
            _isSpeaking = false;
            if (_bodyRect != null) _bodyRect.gameObject.SetActive(false);
        }

        /// <summary>런타임 폰트 교체.</summary>
        public void SetFont(Font font)
        {
            if (_text != null && font != null) _text.font = font;
        }

        // ───── 내부 ─────

        private void ProcessNext()
        {
            if (_pending.Count == 0) return;
            var p = _pending.Dequeue();
            _routine = StartCoroutine(PlayRoutine(p.Text, p.Mode, p.Hold));
        }

        private void StopRoutine()
        {
            if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        }

        private IEnumerator PlayRoutine(string text, BubbleDismissMode mode, float hold)
        {
            _isSpeaking = true;
            if (_bodyRect != null) _bodyRect.gameObject.SetActive(true);
            if (_text != null) _text.text = string.Empty;

            string full = text ?? string.Empty;
            float timer = 0f;
            int shown = 0;

            while (true)
            {
                timer += Time.deltaTime * Mathf.Max(1f, _charsPerSecond);
                int targetChars = Mathf.Min(Mathf.FloorToInt(timer), full.Length);
                if (targetChars != shown)
                {
                    shown = targetChars;
                    if (_text != null) _text.text = full.Substring(0, shown);
                }

                // Append 모드: 타이핑 도중 들어온 추가 텍스트를 full 에 병합
                if (_appendBuffer.Length > 0)
                {
                    full += _appendBuffer;
                    _appendBuffer = string.Empty;
                }

                if (shown >= full.Length) break;
                yield return null;
            }

            // 타이핑 완료
            if (mode == BubbleDismissMode.AutoHide)
            {
                if (hold > 0f) yield return new WaitForSeconds(hold);
                if (_bodyRect != null) _bodyRect.gameObject.SetActive(false);
            }
            // Manual: 그대로 유지. HideOnNext: 유지되다 다음 Show 가 교체.

            _isSpeaking = false;
            _routine = null;

            if (_concurrencyMode == BubbleConcurrencyMode.Queue && _pending.Count > 0)
                ProcessNext();
        }

        private void EnsureAppearance()
        {
            if (_appearanceApplied) return;

            if (_bodyImage != null)
            {
                if (_bodyImage.sprite == null)
                    _bodyImage.sprite = SpeechBubbleSpriteFactory.GetRoundedRect(_roundedTextureSize, _cornerRadius);
                _bodyImage.type = Image.Type.Sliced;
                _bodyImage.color = _bubbleColor;
            }
            if (_tailImage != null)
            {
                int tw = Mathf.Max(4, Mathf.RoundToInt(_tailSize.x));
                int th = Mathf.Max(4, Mathf.RoundToInt(_tailSize.y));
                if (_tailImage.sprite == null)
                    _tailImage.sprite = SpeechBubbleSpriteFactory.GetTriangle(tw, th);
                _tailImage.color = _bubbleColor;

                var rt = _tailImage.rectTransform;
                rt.sizeDelta = _tailSize;
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot     = new Vector2(0.5f, 1f); // 상단 중앙이 기준 → body 하단 중앙에 붙으면 아래로 뾰족
                rt.anchoredPosition = new Vector2(0f, _tailAnchoredY);
            }
            if (_text != null)
            {
                _text.color = _textColor;
                _text.text = string.Empty;
            }
            _appearanceApplied = true;
        }

        // ───── Static Factory ─────

        /// <summary>
        /// 타겟에 붙는 말풍선 GameObject 를 계층째 한 줄로 생성. Overlay Canvas 없으면 자동 생성.
        /// font 를 넘기지 않으면 Unity 내장 LegacyRuntime (한글 미지원) 이 기본으로 들어감 —
        /// 한글을 쓰려면 반드시 font 인자로 NotoSansKR 등을 명시할 것.
        /// </summary>
        public static SpeechBubble Create(Transform target, Font font = null, Canvas canvasOverride = null)
        {
            var canvas = canvasOverride != null ? canvasOverride : FindOrCreateOverlayCanvas();

            // ── Root (타겟 월드 → 스크린 좌표가 찍히는 지점) ──
            var rootGO = new GameObject("SpeechBubble", typeof(RectTransform));
            var rootRT = (RectTransform)rootGO.transform;
            rootRT.SetParent(canvas.transform, false);
            rootRT.anchorMin = rootRT.anchorMax = new Vector2(0f, 0f);
            rootRT.pivot = new Vector2(0.5f, 0f);
            rootRT.sizeDelta = Vector2.zero;

            // ── Body (9-slice 라운디드) ──
            var bodyGO = new GameObject("Body",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var bodyRT = (RectTransform)bodyGO.transform;
            bodyRT.SetParent(rootRT, false);
            bodyRT.anchorMin = bodyRT.anchorMax = new Vector2(0.5f, 0f);
            bodyRT.pivot = new Vector2(0.5f, 0f); // 바닥 중앙이 root 와 맞물림
            bodyRT.sizeDelta = new Vector2(240f, 72f);

            var bodyImg = bodyGO.GetComponent<Image>();
            bodyImg.raycastTarget = false;

            // ── Tail (Body 하단 중앙 자식) ──
            var tailGO = new GameObject("Tail",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tailGO.transform.SetParent(bodyRT, false);
            var tailImg = tailGO.GetComponent<Image>();
            tailImg.raycastTarget = false;

            // ── Text (Body 자식, 안쪽 패딩) ──
            var textGO = new GameObject("Text",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var textRT = (RectTransform)textGO.transform;
            textRT.SetParent(bodyRT, false);
            textRT.anchorMin = new Vector2(0f, 0f);
            textRT.anchorMax = new Vector2(1f, 1f);
            textRT.offsetMin = new Vector2(14f, 10f);
            textRT.offsetMax = new Vector2(-14f, -10f);

            var txt = textGO.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.fontSize = 20;
            txt.raycastTarget = false;
            txt.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // ── SpeechBubble 컴포넌트 ──
            var sb = rootGO.AddComponent<SpeechBubble>();
            sb._bodyRect = bodyRT;
            sb._bodyImage = bodyImg;
            sb._tailImage = tailImg;
            sb._text = txt;
            sb._target = target;
            sb._camera = UnityCamera.main;
            sb.EnsureAppearance();

            // 꼬리 길이만큼 body 를 위로 밀어 꼬리 끝(타겟 포인트) 이 root 위치와 맞물리게
            bodyRT.anchoredPosition = new Vector2(0f, sb._tailSize.y);

            return sb;
        }

        private static Canvas FindOrCreateOverlayCanvas()
        {
            var existing = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in existing)
            {
                if (c.isActiveAndEnabled && c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
            }
            var go = new GameObject("SpeechBubbleCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            return canvas;
        }
    }
}
