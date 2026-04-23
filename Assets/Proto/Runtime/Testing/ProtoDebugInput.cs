using UnityEngine;
using UnityEngine.InputSystem;
using Proto.Movement;
using Proto.Camera;

namespace Proto.Testing
{
    /// <summary>
    /// 디버그용 입력 MB. WASD 로 카메라-상대 이동, 1/2/3 으로 카메라 모드 전환.
    /// Proto 브랜치에서 실제 게임플레이 입력은 CombatScheme 책임(HR-6).
    /// 이 MB 는 테스트 전용.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProtoDebugInput : MonoBehaviour
    {
        [SerializeField] private MovementAgent _agent;
        [SerializeField] private CameraRig _rig;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private bool _cameraRelative = true;

        private void Reset()
        {
            _agent = GetComponent<MovementAgent>();
        }

        private void Awake()
        {
            if (_agent == null) _agent = GetComponent<MovementAgent>();
            if (_rig == null) _rig = FindFirstObjectByType<CameraRig>();
        }

        private void Update()
        {
            if (_agent == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            Vector3 input = Vector3.zero;
            if (kb.wKey.isPressed) input.z += 1f;
            if (kb.sKey.isPressed) input.z -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed) input.x -= 1f;

            if (input.sqrMagnitude > 1e-6f)
            {
                Vector3 worldDir = input.normalized;
                if (_cameraRelative && _rig != null)
                {
                    var camT = UnityEngine.Camera.main != null
                        ? UnityEngine.Camera.main.transform
                        : _rig.transform;
                    Vector3 fwd = Vector3.ProjectOnPlane(camT.forward, Vector3.up);
                    Vector3 right = Vector3.ProjectOnPlane(camT.right, Vector3.up);
                    if (fwd.sqrMagnitude > 1e-6f) fwd.Normalize();
                    else fwd = Vector3.forward;
                    if (right.sqrMagnitude > 1e-6f) right.Normalize();
                    else right = Vector3.right;
                    worldDir = (fwd * worldDir.z + right * worldDir.x);
                    if (worldDir.sqrMagnitude > 1e-6f) worldDir.Normalize();
                }

                _agent.MoveBy(worldDir * (_moveSpeed * Time.deltaTime));
            }

            if (_rig != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) _rig.RequestMode(CameraMode.BackView);
                else if (kb.digit2Key.wasPressedThisFrame) _rig.RequestMode(CameraMode.QuarterView);
                else if (kb.digit3Key.wasPressedThisFrame) _rig.RequestMode(CameraMode.SideView);
            }
        }
    }
}
