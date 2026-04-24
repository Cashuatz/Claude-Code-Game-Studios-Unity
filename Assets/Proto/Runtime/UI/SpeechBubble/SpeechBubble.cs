using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 네임스페이스 충돌 회피: 이 어셈블리에 Proto.Camera 네임스페이스가 존재하므로
// 부모 네임스페이스 스코프에서 Camera 가 Proto.Camera(namespace) 로 해석된다.
using UnityCamera = UnityEngine.Camera;

namespace Proto.UI.Speech
{
    /// <summary>
    /// 캐릭터 머리 위 말풍선 (Screen Space - Overlay).
    ///
    /// 특성:
    ///  - 반응형 크기: 타이핑이 진행될 때마다 Text preferred 크기에 맞춰 Body 가 자라고,
    ///    maxTextWidth 를 넘으면 자동으로 wrap 되어 세로로 늘어난다.
    ///  - 스크린 엣지 클램프 + 꼬리 보정: 본체는 화면 밖으로 삐져나가지 않도록 margin 안쪽으로
    ///    이동하고, 꼬리는 반대 방향으로 anchoredPosition.x 를 이동시켜 여전히 타겟을 가리킨다.
    ///  - 말랑함: 등장 시 back-ease overshoot, 사라질 때 ease-in collapse, 타이핑 시 미세 pulse.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpeechBubble : MonoBehaviour
    {
        // ───── 추적 타겟 ─────
        [Header("Target (World)")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private UnityCamera _camera;
        [SerializeField] private bool _hideWhenBehindCamera = true;

        // ───── 시각 참조 ─────
        [Header("Visual References")]
        [SerializeField] private RectTransform _bodyRect;
        [SerializeField] private Image _bodyImage;
        [SerializeField] private Image _tailImage;
        [SerializeField] private Text _text;
        [SerializeField] private RectTransform _textRect;

        // ───── 외형 ─────
        [Header("Appearance")]
        [SerializeField] private Color _bubbleColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color _textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField, Min(16)] private int _roundedTextureSize = 64;
        [SerializeField, Min(0)] private int _cornerRadius = 16;
        [SerializeField] private Vector2 _tailSize = new Vector2(20f, 14f);
        [Tooltip("본체 텍스트 주변 여백 (좌우, 상하).")]
        [SerializeField] private Vector2 _padding = new Vector2(16f, 10f);
        [Tooltip("텍스트가 이 값을 넘으면 자동 줄바꿈 후 세로 확장.")]
        [SerializeField, Min(50f)] private float _maxTextWidth = 360f;
        [SerializeField, Min(16f)] private float _minBodyWidth = 80f;
        [SerializeField, Min(16f)] private float _minBodyHeight = 40f;
        [SerializeField, Min(0f)] private float _screenEdgeMargin = 16f;

        // ───── 타이핑 ─────
        [Header("Typing")]
        [SerializeField, Min(1f)] private float _charsPerSecond = 30f;

        // ───── 동작 정책 ─────
        [Header("Behavior")]
        [SerializeField] private BubbleDismissMode _defaultDismissMode = BubbleDismissMode.AutoHide;
        [SerializeField, Min(0f)] private float _defaultHoldSeconds = 2f;
        [SerializeField] private BubbleConcurrencyMode _concurrencyMode = BubbleConcurrencyMode.Overwrite;

        // ───── 말랑 애니 ─────
        [Header("Jiggle")]
        [SerializeField, Min(0.05f)] private float _popInDuration = 0.24f;
        [SerializeField, Min(0.05f)] private float _popOutDuration = 0.14f;
        [Tooltip("등장 back-ease overshoot 강도.")]
        [SerializeField, Min(0f)] private float _popOvershoot = 1.7f;
        [Tooltip("타이핑 한 글자당 스케일 kick 량.")]
        [SerializeField, Min(0f)] private float _typePulseKick = 0.035f;
        [Tooltip("pulse 감쇠 속도 (1/sec).")]
        [SerializeField, Min(0.1f)] private float _typePulseDecay = 4f;
        [Tooltip("pulse 누적 상한.")]
        [SerializeField, Min(0.01f)] private float _typePulseMax = 0.16f;

        // ───── 내부 상태 ─────
        private Coroutine _routine;
        private Coroutine _popRoutine;
        private readonly Queue<PendingLine> _pending = new();
        private bool _isSpeaking;
        private string _appendBuffer = string.Empty;

        // 말랑 애니 레이어: base(pop-in/out) + pulse(typing)
        private float _baseScale = 1f;
        private float _pulseScale;

        private readonly struct PendingLine
        {
            public readonly string Text;
            public readonly BubbleDismissMode Mode;
            public readonly float Hold;
            public PendingLine(string t, BubbleDismissMode m, float h) { Text = t; Mode = m; Hold = h; }
        }

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
            if (_bodyRect != null && _isSpeaking && !_bodyRect.gameObject.activeSelf)
                _bodyRect.gameObject.SetActive(true);

            transform.position = new Vector3(sp.x, sp.y, 0f);

            ApplyScreenEdgeClamp();

            // 매 프레임 pulse 감쇠 후 최종 스케일 합성.
            _pulseScale = Mathf.Max(0f, _pulseScale - _typePulseDecay * Time.deltaTime);
            if (_bodyRect != null)
                _bodyRect.localScale = Vector3.one * (_baseScale + _pulseScale);
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
            if (_bodyRect == null) return;
            if (!_bodyRect.gameObject.activeSelf) return;
            if (_popRoutine != null) StopCoroutine(_popRoutine);
            _popRoutine = StartCoroutine(PopOut());
        }

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
            UpdateBubbleSize();

            // Pop-in 말랑 등장
            if (_popRoutine != null) StopCoroutine(_popRoutine);
            _popRoutine = StartCoroutine(PopIn());

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
                    UpdateBubbleSize();
                    // 새 글자마다 살짝 움찔.
                    _pulseScale = Mathf.Min(_pulseScale + _typePulseKick, _typePulseMax);
                }

                if (_appendBuffer.Length > 0)
                {
                    full += _appendBuffer;
                    _appendBuffer = string.Empty;
                }

                if (shown >= full.Length) break;
                yield return null;
            }

            if (mode == BubbleDismissMode.AutoHide)
            {
                if (hold > 0f) yield return new WaitForSeconds(hold);
                if (_popRoutine != null) StopCoroutine(_popRoutine);
                _popRoutine = StartCoroutine(PopOut());
            }

            _isSpeaking = false;
            _routine = null;

            if (_concurrencyMode == BubbleConcurrencyMode.Queue && _pending.Count > 0)
                ProcessNext();
        }

        private IEnumerator PopIn()
        {
            _baseScale = 0.4f;
            float t = 0f;
            while (t < _popInDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / _popInDuration);
                _baseScale = Mathf.Lerp(0.4f, 1f, EaseOutBack(k, _popOvershoot));
                yield return null;
            }
            _baseScale = 1f;
            _popRoutine = null;
        }

        private IEnumerator PopOut()
        {
            float start = _baseScale;
            float t = 0f;
            while (t < _popOutDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / _popOutDuration);
                _baseScale = Mathf.Lerp(start, 0f, k * k);
                yield return null;
            }
            _baseScale = 1f;
            _pulseScale = 0f;
            if (_bodyRect != null) _bodyRect.gameObject.SetActive(false);
            _popRoutine = null;
        }

        private static float EaseOutBack(float x, float c1)
        {
            float c3 = c1 + 1f;
            float xm1 = x - 1f;
            return 1f + c3 * xm1 * xm1 * xm1 + c1 * xm1 * xm1;
        }

        /// <summary>현재 Text.text 에 맞춰 Body 크기 재계산. 필요 시 wrap 모드 전환.</summary>
        private void UpdateBubbleSize()
        {
            if (_text == null || _bodyRect == null || _textRect == null) return;

            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            float naturalW = _text.preferredWidth;
            float usedW, usedH;

            if (naturalW > _maxTextWidth)
            {
                _text.horizontalOverflow = HorizontalWrapMode.Wrap;
                // Wrap 시 preferredHeight 는 sizeDelta.x 에 의존하므로 먼저 폭을 고정.
                _textRect.sizeDelta = new Vector2(_maxTextWidth, _textRect.sizeDelta.y);
                usedW = _maxTextWidth;
                usedH = _text.preferredHeight;
            }
            else
            {
                usedW = naturalW;
                usedH = _text.preferredHeight;
            }

            _textRect.sizeDelta = new Vector2(usedW, usedH);

            float bw = Mathf.Max(_minBodyWidth, usedW + _padding.x * 2f);
            float bh = Mathf.Max(_minBodyHeight, usedH + _padding.y * 2f);
            _bodyRect.sizeDelta = new Vector2(bw, bh);
        }

        /// <summary>화면 가장자리에서 본체를 안쪽으로 밀고, 꼬리는 반대방향으로 이동시켜 여전히 타겟을 가리키게.</summary>
        private void ApplyScreenEdgeClamp()
        {
            if (_bodyRect == null) return;

            float bw = _bodyRect.rect.width;
            float sw = Screen.width;
            float m = _screenEdgeMargin;
            float rootX = transform.position.x;
            float minX = rootX - bw * 0.5f;
            float maxX = rootX + bw * 0.5f;

            float offsetX = 0f;
            if (minX < m) offsetX = m - minX;
            else if (maxX > sw - m) offsetX = (sw - m) - maxX;

            _bodyRect.anchoredPosition = new Vector2(offsetX, _tailSize.y);

            if (_tailImage != null)
            {
                var rt = _tailImage.rectTransform;
                var ap = rt.anchoredPosition;
                ap.x = -offsetX; // body 가 +로 이동했으면 tail 은 body local 기준 -로 보정 → 꼭짓점이 타겟 X 에 고정.
                rt.anchoredPosition = ap;
            }
        }

        private void EnsureAppearance()
        {
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
                rt.pivot     = new Vector2(0.5f, 1f); // 상단 중앙 pivot → body 하단에서 아래로 뾰족.
                rt.anchoredPosition = Vector2.zero;
            }
            if (_text != null)
            {
                _text.color = _textColor;
                _text.text = string.Empty;
                _text.alignment = TextAnchor.MiddleCenter;
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
                _text.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        // ───── Static Factory ─────

        public static SpeechBubble Create(Transform target, Font font = null, Canvas canvasOverride = null)
        {
            var canvas = canvasOverride != null ? canvasOverride : FindOrCreateOverlayCanvas();

            var rootGO = new GameObject("SpeechBubble", typeof(RectTransform));
            var rootRT = (RectTransform)rootGO.transform;
            rootRT.SetParent(canvas.transform, false);
            rootRT.anchorMin = rootRT.anchorMax = new Vector2(0f, 0f);
            rootRT.pivot = new Vector2(0.5f, 0f);
            rootRT.sizeDelta = Vector2.zero;

            var bodyGO = new GameObject("Body",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var bodyRT = (RectTransform)bodyGO.transform;
            bodyRT.SetParent(rootRT, false);
            bodyRT.anchorMin = bodyRT.anchorMax = new Vector2(0.5f, 0f);
            bodyRT.pivot = new Vector2(0.5f, 0f);
            bodyRT.sizeDelta = new Vector2(160f, 56f);

            var bodyImg = bodyGO.GetComponent<Image>();
            bodyImg.raycastTarget = false;

            var tailGO = new GameObject("Tail",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tailGO.transform.SetParent(bodyRT, false);
            var tailImg = tailGO.GetComponent<Image>();
            tailImg.raycastTarget = false;

            // Text 는 center-pivot 수동 제어 (stretch 안 씀 → UpdateBubbleSize 에서 폭/높이 직접 세팅).
            var textGO = new GameObject("Text",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var textRT = (RectTransform)textGO.transform;
            textRT.SetParent(bodyRT, false);
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.anchoredPosition = Vector2.zero;
            textRT.sizeDelta = new Vector2(100f, 28f);

            var txt = textGO.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.fontSize = 20;
            txt.raycastTarget = false;
            txt.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var sb = rootGO.AddComponent<SpeechBubble>();
            sb._bodyRect = bodyRT;
            sb._bodyImage = bodyImg;
            sb._tailImage = tailImg;
            sb._text = txt;
            sb._textRect = textRT;
            sb._target = target;
            sb._camera = UnityCamera.main;
            sb.EnsureAppearance();
            sb.UpdateBubbleSize();

            bodyRT.anchoredPosition = new Vector2(0f, sb._tailSize.y);
            bodyRT.gameObject.SetActive(false);

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
