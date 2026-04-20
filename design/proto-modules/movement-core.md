# Module: MovementCore

> 참고: [`_catalog.md`](./_catalog.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

Rigidbody 를 쓰지 않고 유닛을 이동·충돌 해소하는 저수준 모듈. CapsuleCast
기반 질의 + 수동 push-out 으로 N 유닛 동시 제어. 경사·계단·장애물 회피,
간단한 중력(선택) 지원.

`MovementCore` 자체는 "어디로 얼마만큼 움직일지"를 결정하지 않는다. 상위
모듈(StageGraph 이동, Ability 넉백, 플레이어 입력)이 의도(Intent)를 주면
실제 이동을 물리적으로 안전하게 실행해 준다.

## 2. Hard Rules

- **HR-1 준수**: `Rigidbody` 절대 금지. `Physics.CapsuleCast`, `ComputePenetration` 만.
- Collider 는 **질의용 only**. 다른 Collider 와 물리 시뮬레이션하지 않는다.
- `Physics.autoSyncTransforms` 건드리지 않는다 (경고 있음). 필요 시
  `Physics.SyncTransforms()` 수동 호출.
- 이동 직접 API (`agent.transform.position = ...`) 금지. `MoveBy` / `MoveTo` 사용.
- **HR-9 준수**: 다른 모듈과는 event (`MovementAgent.Moved`, `Collided`) 로 통신.

## 3. Public API

```csharp
namespace Proto.Movement
{
    public enum MoveResult
    {
        Ok,
        Blocked,        // 장애물로 목적지 도달 불가
        PartiallyMoved, // 일부만 이동 (블록에 부딪힘)
        Invalid         // 비정상 입력 (NaN 등)
    }

    public interface IMovementAgent
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }           // 마지막 프레임 이동 벡터 / dt
        MovementProfile Profile { get; }

        /// <summary>월드 델타만큼 이동 시도. 블록 시 부분 이동.</summary>
        MoveResult MoveBy(Vector3 delta);

        /// <summary>월드 좌표로 직선 이동 시도.</summary>
        MoveResult MoveTo(Vector3 worldTarget);

        /// <summary>순간이동 (텔레포트). 충돌 검사 후 안전 지점으로.</summary>
        MoveResult Teleport(Vector3 worldTarget);

        /// <summary>이동 명령 즉시 중단 (현재 프레임부터 Velocity 0).</summary>
        void Halt();

        event Action<Vector3> Moved;                 // 실제 이동 델타
        event Action<Collider> Collided;             // 블록 유발 콜라이더
    }
}

[CreateAssetMenu(menuName = "Proto/Movement/MovementProfile")]
public class MovementProfile : ScriptableObject
{
    [Header("Capsule")]
    public float radius = 0.4f;
    public float height = 1.8f;

    [Header("Speed")]
    public float maxSpeed = 5f;             // m/s
    public float acceleration = 40f;        // m/s^2 (속도 수렴용)

    [Header("Terrain")]
    public float stepOffset = 0.3f;         // 타고 넘을 수 있는 단차
    public float slopeLimit = 45f;          // 최대 오를 경사각 (도)
    public LayerMask blockingLayers = ~0;
    public LayerMask groundLayers = 1;

    [Header("Gravity")]
    public bool applyGravity = true;
    public float gravity = -20f;

    [Header("Resolution")]
    public int maxPushoutIterations = 3;    // ComputePenetration 루프 한도
    public float skinWidth = 0.02f;
}
```

## 4. Dependencies

- **Required**: 없음
- **Optional**:
  - `StageGraph` — 노드 간 이동 시 StageGraph 가 `MoveTo(node.Position)` 호출
  - `TurnSystem` — `LocalTimeScale` 적용 시 이동 속도 자동 스케일

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Movement/Unit_Base.prefab` | 캡슐 Collider(trigger=false, query only) + `MovementAgent` MB + `IFramingTarget` 구현. Rigidbody 없음. |
| `Assets/Proto/Data/Config/Movement_Default.asset` | 기본 `MovementProfile` |
| `Assets/Proto/Data/Config/Movement_Heavy.asset` | 무거운 유닛 (느림, 밀리지 않음) |
| `Assets/Proto/Data/Config/Movement_Light.asset` | 경량 유닛 (빠름) |

`Unit_Base.prefab` 은 이후 CharacterRenderKit 이 하위에 렌더 GameObject 붙임.

## 6. Skill Hook

`/proto-movement` 호출 시:

1. `Assets/Proto/Runtime/Movement/` 생성.
2. 스크립트 생성: `MoveResult.cs`, `IMovementAgent.cs`, `MovementAgent.cs`,
   `MovementProfile.cs`.
3. ScriptableObject 3개 생성 (Default / Heavy / Light).
4. `Unit_Base.prefab` 생성 (CapsuleCollider + MovementAgent).
5. 테스트 씬 유닛 1개 스폰 + `ProtoMain.unity` 에 배치.
6. WASD 입력 스크립트는 **설치하지 않음** — 그건 Phase B `CombatScheme` 책임.
   대신 `/proto-movement --with-debug-input` 옵션 주면 임시 디버그 이동 MB 추가.
7. 콘솔 에러 확인.

옵션 인자:
- `--with-debug-input` — 디버그용 WASD MB 추가 (Editor only).
- `--profile <default|heavy|light>` — `Unit_Base.prefab` 기본 프로필.

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="MovementAgent", search_method="by_component")` → ≥1.
3. Play 진입 + 디버그 입력 있을 경우 WASD 로 이동 확인 (스크린샷).
4. `execute_code` 스모크:
   ```csharp
   var agent = GameObject.Find("Unit_Base(Clone)").GetComponent<Proto.Movement.MovementAgent>();
   var result = agent.MoveBy(new Vector3(1, 0, 0));
   UnityEngine.Debug.Assert(result == Proto.Movement.MoveResult.Ok || result == Proto.Movement.MoveResult.PartiallyMoved);
   UnityEngine.Debug.Assert(agent.GetComponent<Rigidbody>() == null, "HR-1 violation: Rigidbody detected");
   ```
5. HR-1 검증 — 스킬 설치 후 `Unit_Base.prefab` 및 씬 내 유닛에 `Rigidbody` 없음 확인.
6. `manage_editor(action="stop")`.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01, OR-HARD-RULE-01 (HR-1 준수 검증)
