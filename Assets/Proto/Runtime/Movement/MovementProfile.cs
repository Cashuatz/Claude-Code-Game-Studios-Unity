using UnityEngine;

namespace Proto.Movement
{
    [CreateAssetMenu(fileName = "Movement_Default", menuName = "Proto/Movement/MovementProfile")]
    public class MovementProfile : ScriptableObject
    {
        [Header("Capsule")]
        public float radius = 0.4f;
        public float height = 1.8f;

        [Header("Speed")]
        public float maxSpeed = 5f;
        public float acceleration = 40f;

        [Header("Terrain")]
        public float stepOffset = 0.3f;
        public float slopeLimit = 45f;
        public LayerMask blockingLayers = ~0;
        public LayerMask groundLayers = 1;

        [Header("Gravity")]
        public bool applyGravity = true;
        public float gravity = -20f;

        [Header("Resolution")]
        [Range(1, 8)] public int maxPushoutIterations = 3;
        public float skinWidth = 0.02f;
    }
}
