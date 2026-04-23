using UnityEngine;

namespace Proto.Camera
{
    [CreateAssetMenu(fileName = "Camera_BackView", menuName = "Proto/Camera/ModeConfig")]
    public class CameraModeConfig : ScriptableObject
    {
        public CameraMode mode = CameraMode.BackView;
        public Vector3 offset = new Vector3(0f, 2f, -5f);
        public Vector3 lookAtLocalOffset = new Vector3(0f, 1.5f, 0f);
        [Range(10f, 120f)] public float fov = 50f;
        public float minDistance = 4f;
        public float maxDistance = 20f;
    }
}
