using UnityEngine;

namespace Proto.UI.Speech
{
    /// <summary>
    /// 말풍선 테스트 데모. 아무 GameObject 에 붙이고 Play.
    ///  - target 을 지정 안 하면 자기 자신(transform) 을 추적.
    ///  - font 에 Assets/Proto/Font/NotoSansKR-Regular.ttf 지정 권장 (미지정 시 한글 깨짐).
    /// Inspector Context Menu "Say Again" 으로 재생 가능.
    /// </summary>
    public class SpeechBubbleDemo : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        [Header("Font (한글 지원 필요)")]
        [SerializeField] private Font _font;

        [Header("Message")]
        [TextArea(2, 6)]
        [SerializeField] private string _message = "안녕! 나는 프로토타입 말풍선이야.";
        [SerializeField] private BubbleDismissMode _dismissMode = BubbleDismissMode.AutoHide;
        [SerializeField, Min(0f)] private float _holdSeconds = 2f;

        [Header("Auto")]
        [SerializeField] private bool _playOnStart = true;

        private SpeechBubble _bubble;

        private void Start()
        {
            if (_target == null) _target = transform;
            _bubble = SpeechBubble.Create(_target, _font);
            _bubble.SetTarget(_target, _worldOffset);
            if (_playOnStart) _bubble.Show(_message, _dismissMode, _holdSeconds);
        }

        [ContextMenu("Say Again")]
        private void SayAgain()
        {
            if (_bubble == null && Application.isPlaying) Start();
            else _bubble?.Show(_message, _dismissMode, _holdSeconds);
        }

        [ContextMenu("Hide")]
        private void HideNow() => _bubble?.Hide();
    }
}
