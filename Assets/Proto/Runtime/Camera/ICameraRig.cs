using System;

namespace Proto.Camera
{
    public interface ICameraRig
    {
        CameraMode CurrentMode { get; }
        bool IsTransitioning { get; }
        bool IsPlayingShot { get; }

        void RequestMode(CameraMode mode, TransitionProfile profile = null);

        /// <summary>
        /// 타임라인·궁극기 등에서 일정 시간동안 카메라를 오버라이드.
        /// 재생 중 RequestMode 가 호출되면 쇼트는 즉시 취소되고 모드 전환이 이어진다.
        /// 쇼트 종료 시 현 모드 프레임으로 기본 TransitionProfile 따라 부드럽게 복귀.
        /// </summary>
        void PlayShot(IShotProfile profile);

        /// <summary>현재 쇼트를 강제 종료하고 복귀 전환을 시작.</summary>
        void StopShot();

        void RegisterTarget(IFramingTarget target);
        void UnregisterTarget(IFramingTarget target);

        void SetFramingStrategy(IFramingStrategy strategy);

        event Action<CameraMode, CameraMode> ModeChanged;
        event Action<CameraMode> TransitionCompleted;
        event Action ShotStarted;
        event Action ShotCompleted;
    }
}
