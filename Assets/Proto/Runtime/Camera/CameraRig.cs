using System;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.Camera
{
    [DisallowMultipleComponent]
    public sealed class CameraRig : MonoBehaviour, ICameraRig
    {
        [Header("Camera")]
        [SerializeField] private UnityEngine.Camera _camera;

        [Header("Mode Configs")]
        [SerializeField] private CameraModeConfig _backView;
        [SerializeField] private CameraModeConfig _quarterView;
        [SerializeField] private CameraModeConfig _sideView;

        [Header("Transition")]
        [SerializeField] private TransitionProfile _defaultTransition;
        [SerializeField] private CameraMode _initialMode = CameraMode.BackView;

        [Header("Framing Strategy")]
        [SerializeField] private StrategyKind _strategyKind = StrategyKind.WeightedAverage;

        public enum StrategyKind { Single, WeightedAverage, BoundingBox }

        private readonly List<IFramingTarget> _targets = new List<IFramingTarget>(8);
        private IFramingStrategy _strategy;

        private CameraMode _currentMode;
        private CameraMode _toMode;
        private TransitionProfile _activeTransition;
        private float _transitionElapsed;
        private bool _transitioning;

        private Vector3 _fromPos;
        private Quaternion _fromRot;
        private float _fromFov;

        public CameraMode CurrentMode => _currentMode;
        public bool IsTransitioning => _transitioning;

        public event Action<CameraMode, CameraMode> ModeChanged;
        public event Action<CameraMode> TransitionCompleted;

        private void Awake()
        {
            if (_camera == null) _camera = GetComponentInChildren<UnityEngine.Camera>(true);
            if (_camera == null)
            {
                Debug.LogError($"[CameraRig] No Camera component found on '{name}' or its children.", this);
                enabled = false;
                return;
            }
            _strategy = MakeStrategy(_strategyKind);
            _currentMode = _initialMode;
            _toMode = _initialMode;
        }

        private void Start()
        {
            var (pos, rot, fov) = ComputeFrame(GetConfig(_initialMode));
            _camera.transform.SetPositionAndRotation(pos, rot);
            _camera.fieldOfView = fov;
        }

        public void RequestMode(CameraMode mode, TransitionProfile profile = null)
        {
            if (!_transitioning && _currentMode == mode) return;
            if (_transitioning && _toMode == mode) return;

            var chosen = profile != null ? profile : _defaultTransition;
            if (chosen == null || chosen.durationSec <= 0f)
            {
                var prev = _currentMode;
                _currentMode = mode;
                _toMode = mode;
                _transitioning = false;
                ModeChanged?.Invoke(prev, mode);
                TransitionCompleted?.Invoke(mode);
                return;
            }

            _fromPos = _camera.transform.position;
            _fromRot = _camera.transform.rotation;
            _fromFov = _camera.fieldOfView;
            _toMode = mode;
            _activeTransition = chosen;
            _transitionElapsed = 0f;
            _transitioning = true;
        }

        public void RegisterTarget(IFramingTarget target)
        {
            if (target == null || _targets.Contains(target)) return;
            _targets.Add(target);
        }

        public void UnregisterTarget(IFramingTarget target)
        {
            if (target == null) return;
            _targets.Remove(target);
        }

        public void SetFramingStrategy(IFramingStrategy strategy)
        {
            _strategy = strategy ?? MakeStrategy(_strategyKind);
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            Vector3 pos; Quaternion rot; float fov;

            if (_transitioning && _activeTransition != null)
            {
                var (toPos, toRot, toFov) = ComputeFrame(GetConfig(_toMode));

                _transitionElapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(_transitionElapsed / _activeTransition.durationSec);

                float posT = _activeTransition.positionEase.Evaluate(tNorm);
                float rotT = _activeTransition.rotationEase.Evaluate(tNorm);
                float fovT = _activeTransition.fovEase.Evaluate(tNorm);

                pos = Vector3.Lerp(_fromPos, toPos, posT);
                rot = Quaternion.Slerp(_fromRot, toRot, rotT);
                fov = Mathf.Lerp(_fromFov, toFov, fovT);

                if (tNorm >= 1f)
                {
                    _transitioning = false;
                    var prev = _currentMode;
                    _currentMode = _toMode;
                    ModeChanged?.Invoke(prev, _toMode);
                    TransitionCompleted?.Invoke(_toMode);
                }
            }
            else
            {
                var (p, r, f) = ComputeFrame(GetConfig(_currentMode));
                pos = p; rot = r; fov = f;
            }

            _camera.transform.SetPositionAndRotation(pos, rot);
            _camera.fieldOfView = fov;
        }

        private (Vector3 pos, Quaternion rot, float fov) ComputeFrame(CameraModeConfig cfg)
        {
            if (cfg == null)
            {
                return (_camera.transform.position, _camera.transform.rotation, _camera.fieldOfView);
            }

            if (_strategy == null) _strategy = MakeStrategy(_strategyKind);

            var (anchor, distance) = _strategy.Compute(_targets, cfg);

            Vector3 offsetDir = cfg.offset.sqrMagnitude > 1e-6f
                ? cfg.offset.normalized
                : Vector3.back;

            Vector3 pos = anchor + offsetDir * distance;
            Vector3 lookAt = anchor + cfg.lookAtLocalOffset;
            Vector3 toTarget = lookAt - pos;
            Quaternion rot = toTarget.sqrMagnitude > 1e-6f
                ? Quaternion.LookRotation(toTarget, Vector3.up)
                : Quaternion.identity;

            return (pos, rot, cfg.fov);
        }

        private CameraModeConfig GetConfig(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.BackView: return _backView;
                case CameraMode.QuarterView: return _quarterView;
                case CameraMode.SideView: return _sideView;
                default: return _backView;
            }
        }

        private static IFramingStrategy MakeStrategy(StrategyKind kind)
        {
            switch (kind)
            {
                case StrategyKind.Single: return new SingleTargetStrategy();
                case StrategyKind.BoundingBox: return new BoundingBoxStrategy();
                case StrategyKind.WeightedAverage:
                default: return new WeightedAverageStrategy();
            }
        }
    }
}
