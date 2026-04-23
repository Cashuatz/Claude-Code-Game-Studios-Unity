# Module: CameraRig

> 참고: [`_catalog-turn3d.md`](./_catalog-turn3d.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

3개 카메라 모드(백뷰 / 쿼터뷰 / 사이드뷰)를 **단일 Rig**에서 관리하고
연출·입력 이벤트에 따라 전환한다. N-유닛 프레이밍(단일 추적, 파티 평균,
바운딩박스 감쌈)을 지원해 1:1부터 N:N까지 대응한다.

여러 Cinemachine 브레인을 쓰지 않는다 — 단일 Camera + 모드 전환 추상화.

## 2. Hard Rules

- **HR-4 준수**: 모드 전환은 `CameraRig.RequestMode(mode, profile)` 만 통한다.
  다른 스크립트가 `Camera.transform` 직접 건드리지 않는다.
- 프레이밍 타겟은 `IFramingTarget` 인터페이스로만 전달. GameObject 직접 참조 금지.
- `FixedUpdate` 사용 금지 (Rigidbody 없음). `LateUpdate` 에서 위치 업데이트.
- **HR-10 준수**: 모든 코드는 `Assets/Proto/Runtime/Camera/` 하위.

## 3. Public API

```csharp
namespace Proto.Camera
{
    public enum CameraMode { BackView, QuarterView, SideView }

    public interface IFramingTarget
    {
        Vector3 Position { get; }
        float Weight { get; }            // 프레이밍 가중치 (0~1). 사망 시 0.
    }

    public interface ICameraRig
    {
        CameraMode CurrentMode { get; }
        bool IsTransitioning { get; }

        /// <summary>부드러운 모드 전환. profile=null 이면 기본 블렌딩.</summary>
        void RequestMode(CameraMode mode, TransitionProfile profile = null);

        /// <summary>프레이밍 타겟 등록/해제. 여러 유닛 동시 등록 가능.</summary>
        void RegisterTarget(IFramingTarget target);
        void UnregisterTarget(IFramingTarget target);

        /// <summary>프레이밍 전략 교체 (단일/평균/바운딩박스).</summary>
        void SetFramingStrategy(IFramingStrategy strategy);

        event Action<CameraMode, CameraMode> ModeChanged;   // (from, to)
        event Action<CameraMode> TransitionCompleted;
    }

    public interface IFramingStrategy
    {
        (Vector3 anchor, float distance) Compute(
            IReadOnlyList<IFramingTarget> targets,
            CameraModeConfig modeConfig);
    }
}
```

`TransitionProfile` / `CameraModeConfig` 는 ScriptableObject:

```csharp
[CreateAssetMenu(menuName = "Proto/Camera/TransitionProfile")]
public class TransitionProfile : ScriptableObject
{
    public float durationSec = 0.6f;
    public AnimationCurve positionEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve rotationEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fovEase     = AnimationCurve.Linear(0, 0, 1, 1);
}

[CreateAssetMenu(menuName = "Proto/Camera/ModeConfig")]
public class CameraModeConfig : ScriptableObject
{
    public CameraMode mode;
    public Vector3 offset;          // 앵커 기준 오프셋
    public Vector3 lookAtLocalOffset;
    public float fov = 50f;
    public float minDistance = 4f;  // 바운딩박스 프레이밍 시 최소/최대 거리
    public float maxDistance = 20f;
}
```

## 4. Dependencies

- **Required**: 없음 (leaf 모듈)
- **Optional**:
  - `TimelineCue` — 컷신 타임라인이 `ICameraRig.RequestMode` 호출
  - `EventBus` (HR-9) — 글로벌 `CameraModeChanged` 이벤트 발행

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Camera/CameraRig.prefab` | 루트 GameObject + Camera + `CameraRig` MB |
| `Assets/Proto/Data/Config/Camera_BackView.asset` | `CameraModeConfig` (백뷰 기본값) |
| `Assets/Proto/Data/Config/Camera_QuarterView.asset` | 쿼터뷰 기본값 |
| `Assets/Proto/Data/Config/Camera_SideView.asset` | 사이드뷰 기본값 |
| `Assets/Proto/Data/Config/Camera_Transition_Default.asset` | 기본 `TransitionProfile` |

기본값 가이드:
- **BackView**: offset `(0, 2, -5)`, fov 55, lookAt `(0, 1.5, 0)`
- **QuarterView**: offset `(0, 10, -10)`, fov 35, lookAt `(0, 0, 2)`
- **SideView**: offset `(10, 2, 0)`, fov 45, lookAt `(0, 1.5, 0)`

3개 프레이밍 전략 구현:
- `SingleTargetStrategy` — 가중치 최대 타겟 추적
- `WeightedAverageStrategy` — 가중평균 위치, 고정 거리
- `BoundingBoxStrategy` — 모든 타겟 감쌈, 거리 자동 조정

## 6. Skill Hook

`/proto-camera-rig` 호출 시 수행:

1. `Assets/Proto/Runtime/Camera/` 디렉토리 생성.
2. 스크립트 생성: `CameraMode.cs`, `ICameraRig.cs`, `CameraRig.cs`,
   `IFramingTarget.cs`, `IFramingStrategy.cs`, `CameraModeConfig.cs`,
   `TransitionProfile.cs`, `SingleTargetStrategy.cs`, `WeightedAverageStrategy.cs`,
   `BoundingBoxStrategy.cs`.
3. ScriptableObject 에셋 4개 생성 (3 mode + 1 transition).
4. `CameraRig.prefab` 생성. Main Camera 를 이 프리팹이 소유.
5. `ProtoMain.unity` 씬에 프리팹 인스턴스 배치, 기존 Main Camera 제거.
6. 콘솔 에러 확인. 에러 있으면 롤백.

옵션 인자:
- `--no-overwrite` — 기존 에셋 있으면 스킵.
- `--strategy <single|avg|bbox>` — 기본 전략 지정 (기본 `avg`).

## 7. Verification

1. `read_console(types=["error"])` → 0 에러.
2. `find_gameobjects(search_term="CameraRig", search_method="by_name")` → 1개.
3. `manage_editor(action="play")` → 3초 대기 → `read_console(types=["error"])` → 0.
4. `manage_camera(action="screenshot", camera="CameraRig/Camera", include_image=True, max_resolution=512)` → 사용자에게 제시.
5. `execute_code` 로 다음 스모크 테스트 실행:

```csharp
var rig = GameObject.Find("CameraRig").GetComponent<Proto.Camera.CameraRig>();
rig.RequestMode(Proto.Camera.CameraMode.QuarterView);
// 1초 후
UnityEngine.Debug.Assert(rig.CurrentMode == Proto.Camera.CameraMode.QuarterView);
```

6. `manage_editor(action="stop")`.

**오라클 바인딩** (`.vkl/core/ORACLE_CATALOG.base.md`):
- OR-COMPILE-01 (컴파일 에러 0)
- OR-RUNTIME-01 (런타임 에러 0)
- OR-VISUAL-01 (스크린샷 존재)
