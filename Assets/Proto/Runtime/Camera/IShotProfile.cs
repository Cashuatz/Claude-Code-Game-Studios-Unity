using UnityEngine;

namespace Proto.Camera
{
    /// <summary>
    /// 일정 시간 동안 카메라 위치/회전/FOV 를 구동하는 쇼트 프로파일.
    /// 궁극기·컷신 등 "타임라인 → 캐릭터 중심 카메라워크 → 복귀" 시나리오용.
    ///
    /// `CameraRig.PlayShot(IShotProfile)` 으로 재생. 재생 중에는 기본 모드 프레임이
    /// 덮어씌워진다. 완료되면 현 모드로 부드럽게 복귀.
    /// </summary>
    public interface IShotProfile
    {
        /// <summary>총 재생 시간(초). 0 이하이면 즉시 종료.</summary>
        float Duration { get; }

        /// <summary>anchor = 프레이밍 전략이 계산한 캐릭터/파티 앵커 위치. t01 = 0..1.</summary>
        Vector3 GetPosition(Vector3 anchor, float t01);

        /// <summary>카메라 회전. pos 는 GetPosition 결과를 받아 사용 가능(LookAt 등).</summary>
        Quaternion GetRotation(Vector3 anchor, Vector3 pos, float t01);

        /// <summary>시야각(도). t01 로 보간해도 되고 상수여도 됨.</summary>
        float GetFov(float t01);
    }
}
