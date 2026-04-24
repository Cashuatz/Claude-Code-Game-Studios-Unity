using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace Proto.Launcher
{
    [DisallowMultipleComponent]
    public sealed class LauncherUI : MonoBehaviour
    {
        [Header("Buttons (required)")]
        [SerializeField] private Button _turn3dButton;
        [SerializeField] private Button _tdButton;
        [SerializeField] private Button _railShooterButton;

        [Header("Labels (optional)")]
        [SerializeField] private TMP_Text _statusLabel;

        [Header("Hotkeys")]
        [SerializeField] private bool _enableHotkeys = true;

        private void Awake()
        {
            if (_turn3dButton == null || _tdButton == null || _railShooterButton == null)
            {
                Debug.LogError($"[LauncherUI] Buttons not assigned on '{name}'. Disabling.", this);
                enabled = false;
                return;
            }

            _turn3dButton.onClick.AddListener(() => Launch(GenreId.Turn3d));
            _tdButton.onClick.AddListener(() => Launch(GenreId.TD));
            _railShooterButton.onClick.AddListener(() => Launch(GenreId.RailShooter));
        }

        private void OnDestroy()
        {
            if (_turn3dButton != null) _turn3dButton.onClick.RemoveAllListeners();
            if (_tdButton != null) _tdButton.onClick.RemoveAllListeners();
            if (_railShooterButton != null) _railShooterButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (!_enableHotkeys) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) Launch(GenreId.Turn3d);
            else if (kb.digit2Key.wasPressedThisFrame) Launch(GenreId.TD);
            else if (kb.digit3Key.wasPressedThisFrame) Launch(GenreId.RailShooter);
        }

        private void Launch(GenreId genre)
        {
            if (SceneFlow.Instance == null)
            {
                Debug.LogError("[LauncherUI] SceneFlow.Instance is null. Bootstrap failed.");
                SetStatus("SceneFlow 부트스트랩 실패");
                return;
            }

            if (SceneFlow.Instance.IsLoading)
            {
                SetStatus("이미 로딩 중");
                return;
            }

            SetStatus($"로딩 중: {genre}");
            SceneFlow.Instance.LoadGenre(genre);
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null) _statusLabel.text = text;
        }
    }
}
