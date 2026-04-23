using UnityEngine;
using Proto.Camera;

namespace Proto.Testing
{
    /// <summary>
    /// CameraRig 의 프레이밍 타겟으로 등록되는 간이 컴포넌트.
    /// Start 에서 씬의 CameraRig 를 찾아 자기 자신을 등록한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProtoUnitTarget : MonoBehaviour, IFramingTarget
    {
        [SerializeField, Range(0f, 1f)] private float _weight = 1f;
        [SerializeField] private CameraRig _rig;

        public Vector3 Position => transform.position;
        public float Weight => _weight;

        private void Start()
        {
            if (_rig == null) _rig = FindFirstObjectByType<CameraRig>();
            if (_rig != null) _rig.RegisterTarget(this);
        }

        private void OnDestroy()
        {
            if (_rig != null) _rig.UnregisterTarget(this);
        }
    }
}
