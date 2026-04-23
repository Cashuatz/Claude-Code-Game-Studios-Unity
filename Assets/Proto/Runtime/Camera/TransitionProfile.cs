using UnityEngine;

namespace Proto.Camera
{
    [CreateAssetMenu(fileName = "Camera_Transition_Default", menuName = "Proto/Camera/TransitionProfile")]
    public class TransitionProfile : ScriptableObject
    {
        public float durationSec = 0.6f;
        public AnimationCurve positionEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve rotationEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve fovEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}
