using System;

namespace Proto.Camera
{
    public interface ICameraRig
    {
        CameraMode CurrentMode { get; }
        bool IsTransitioning { get; }

        void RequestMode(CameraMode mode, TransitionProfile profile = null);

        void RegisterTarget(IFramingTarget target);
        void UnregisterTarget(IFramingTarget target);

        void SetFramingStrategy(IFramingStrategy strategy);

        event Action<CameraMode, CameraMode> ModeChanged;
        event Action<CameraMode> TransitionCompleted;
    }
}
