using UnityEngine;
using Proto.Movement;
using Proto.Camera;

namespace Proto.Testing
{
    /// <summary>
    /// MovementAgent/CameraRig 이벤트를 ProtoTestBus 로 전달.
    /// VKL 에서 "무슨 일이 일어났는지" 관측하기 위한 브리지.
    /// </summary>
    [RequireComponent(typeof(MovementAgent))]
    [DisallowMultipleComponent]
    public sealed class ProtoEventEmitter : MonoBehaviour
    {
        [SerializeField] private CameraRig _rig;
        [SerializeField] private float _moveEmitThrottleSec = 0.25f;
        [SerializeField] private float _moveEmitBigDeltaMag = 0.5f;

        private MovementAgent _agent;
        private bool _lastGrounded;
        private double _lastMoveEmit;

        private void Awake()
        {
            _agent = GetComponent<MovementAgent>();
            if (_rig == null) _rig = FindFirstObjectByType<CameraRig>();
        }

        private void OnEnable()
        {
            if (_agent != null)
            {
                _agent.Moved += HandleMoved;
                _agent.Collided += HandleCollided;
            }
            if (_rig != null)
            {
                _rig.ModeChanged += HandleModeChanged;
                _rig.TransitionCompleted += HandleTransitionCompleted;
            }
            _lastGrounded = _agent != null && _agent.IsGrounded;
        }

        private void OnDisable()
        {
            if (_agent != null)
            {
                _agent.Moved -= HandleMoved;
                _agent.Collided -= HandleCollided;
            }
            if (_rig != null)
            {
                _rig.ModeChanged -= HandleModeChanged;
                _rig.TransitionCompleted -= HandleTransitionCompleted;
            }
        }

        private void Update()
        {
            if (_agent == null) return;
            bool grounded = _agent.IsGrounded;
            if (grounded != _lastGrounded)
            {
                ProtoTestBus.Emit(ProtoEventKind.GroundState,
                    grounded ? "Landed" : "Airborne");
                _lastGrounded = grounded;
            }
        }

        private void HandleMoved(Vector3 delta)
        {
            float mag = delta.magnitude;
            bool big = mag > _moveEmitBigDeltaMag;
            bool timeExpired = (Time.timeAsDouble - _lastMoveEmit) > _moveEmitThrottleSec;
            if (!big && !timeExpired) return;

            _lastMoveEmit = Time.timeAsDouble;
            ProtoTestBus.Emit(ProtoEventKind.Move,
                $"delta=({delta.x:F2},{delta.y:F2},{delta.z:F2}) mag={mag:F2} pos=({_agent.Position.x:F2},{_agent.Position.y:F2},{_agent.Position.z:F2})");
        }

        private void HandleCollided(Collider other)
        {
            ProtoTestBus.Emit(ProtoEventKind.Collision,
                $"with={(other != null ? other.name : "<null>")}");
        }

        private void HandleModeChanged(CameraMode from, CameraMode to)
        {
            ProtoTestBus.Emit(ProtoEventKind.ModeChange, $"{from} -> {to}");
        }

        private void HandleTransitionCompleted(CameraMode mode)
        {
            ProtoTestBus.Emit(ProtoEventKind.TransitionCompleted, mode.ToString());
        }
    }
}
