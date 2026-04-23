using UnityEngine;

namespace Proto.Camera
{
    /// <summary>
    /// 캐릭터 앵커 주위를 공전하는 카메라 쇼트.
    /// 궁극기 발동 연출의 기본형.
    /// </summary>
    public sealed class OrbitShot : IShotProfile
    {
        public float Duration { get; }
        public float Radius { get; }
        public float Height { get; }
        public float Revolutions { get; }
        public float StartAngleDeg { get; }
        public float Fov { get; }

        public OrbitShot(
            float duration = 1.5f,
            float radius = 5f,
            float height = 2f,
            float revolutions = 1f,
            float startAngleDeg = 0f,
            float fov = 50f)
        {
            Duration = Mathf.Max(duration, 0.01f);
            Radius = Mathf.Max(radius, 0.1f);
            Height = height;
            Revolutions = revolutions;
            StartAngleDeg = startAngleDeg;
            Fov = Mathf.Clamp(fov, 10f, 120f);
        }

        public Vector3 GetPosition(Vector3 anchor, float t01)
        {
            float angleRad = (StartAngleDeg * Mathf.Deg2Rad) + (t01 * Revolutions * Mathf.PI * 2f);
            return anchor + new Vector3(
                Mathf.Sin(angleRad) * Radius,
                Height,
                Mathf.Cos(angleRad) * Radius);
        }

        public Quaternion GetRotation(Vector3 anchor, Vector3 pos, float t01)
        {
            Vector3 toAnchor = anchor - pos;
            if (toAnchor.sqrMagnitude < 1e-6f) return Quaternion.identity;
            return Quaternion.LookRotation(toAnchor, Vector3.up);
        }

        public float GetFov(float t01) => Fov;
    }
}
