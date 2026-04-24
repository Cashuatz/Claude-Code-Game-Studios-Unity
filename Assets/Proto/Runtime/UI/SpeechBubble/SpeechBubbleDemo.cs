using UnityEngine;
using UnityEngine.InputSystem;

namespace Proto.UI.Speech
{
    /// <summary>
    /// 말풍선 테스트 데모.
    /// Play 후 키 입력으로 즉시 검증:
    ///  - 1 : 짧은 메시지 (반응형 크기 최소 바운드 확인)
    ///  - 2 : 중간 메시지 (기본)
    ///  - 3 : 긴 메시지 (wrap + 세로 확장 확인)
    ///  - Space : 현재 defaultMessage 재발화
    ///  - 0 : Hide
    ///  - WASD / 방향키 : 캐릭터 이동 (화면 엣지 클램프와 tail 추적 확인)
    /// font 에 Assets/Proto/Font/NotoSansKR-Regular.ttf 지정 권장 (미지정 시 한글 깨짐).
    /// </summary>
    public class SpeechBubbleDemo : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        [Header("Font (한글 지원 필요)")]
        [SerializeField] private Font _font;

        [Header("Messages")]
        [SerializeField] private string _shortMessage = "안녕!";
        [TextArea(2, 4)]
        [SerializeField] private string _mediumMessage = "안녕! 나는 프로토 말풍선이야. 한 글자씩 나타난다.";
        [TextArea(3, 8)]
        [SerializeField] private string _longMessage =
            "이건 훨씬 긴 테스트 메시지야. 최대 폭에 도달하면 자동으로 줄바꿈되고 풍선 크기가 알아서 늘어나야 정상이야. " +
            "등장할 때는 살짝 말랑하게 튀어나오고 글자가 추가될 때마다 미세하게 움찔거려야 합니다.";

        [Header("Behavior")]
        [SerializeField] private BubbleDismissMode _dismissMode = BubbleDismissMode.AutoHide;
        [SerializeField, Min(0f)] private float _holdSeconds = 2.5f;
        [SerializeField] private bool _playOnStart = true;

        [Header("Target Control")]
        [Tooltip("WASD/방향키로 타겟을 월드 XZ 평면에서 이동. 0 이면 비활성.")]
        [SerializeField, Min(0f)] private float _targetMoveSpeed = 4f;

        private SpeechBubble _bubble;

        private void Start()
        {
            if (_target == null) _target = transform;
            _bubble = SpeechBubble.Create(_target, _font);
            _bubble.SetTarget(_target, _worldOffset);
            if (_playOnStart) _bubble.Show(_mediumMessage, _dismissMode, _holdSeconds);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) _bubble?.Show(_shortMessage, _dismissMode, _holdSeconds);
            if (kb.digit2Key.wasPressedThisFrame) _bubble?.Show(_mediumMessage, _dismissMode, _holdSeconds);
            if (kb.digit3Key.wasPressedThisFrame) _bubble?.Show(_longMessage, _dismissMode, _holdSeconds);
            if (kb.spaceKey.wasPressedThisFrame)  _bubble?.Show(_mediumMessage, _dismissMode, _holdSeconds);
            if (kb.digit0Key.wasPressedThisFrame) _bubble?.Hide();

            if (_target != null && _targetMoveSpeed > 0f)
            {
                Vector2 mv = Vector2.zero;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    mv.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  mv.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  mv.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) mv.x += 1f;
                if (mv.sqrMagnitude > 0.0001f)
                {
                    mv = mv.normalized * _targetMoveSpeed * Time.deltaTime;
                    _target.position += new Vector3(mv.x, 0f, mv.y);
                }
            }
        }

        [ContextMenu("Say Medium")]
        private void SayMedium() { if (Application.isPlaying) _bubble?.Show(_mediumMessage, _dismissMode, _holdSeconds); }

        [ContextMenu("Hide")]
        private void HideNow() => _bubble?.Hide();
    }
}
