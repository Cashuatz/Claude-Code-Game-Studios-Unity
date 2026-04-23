# Module: TurnSystem

> 참고: [`_catalog-turn3d.md`](./_catalog-turn3d.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

유사턴제 전투 흐름을 관리한다. **선택 중인 유닛은 타임슬로우**, 타임오버 시
자동 턴 패스. 1:1 / 1:N / N:1 / N:N 전부 지원. `Time.timeScale` 을 건드리지
않고 **유닛별 LocalTimeScale** 로 구현한다 (HR-5).

턴의 본질은 "어느 유닛이 지금 행동할 권리를 가지는가"로, 순차가 아니라
**동시·중첩 진행** 가능. 각 유닛은 자체 게이지를 채우고 게이지 완료 시
한 번의 액션(CombatScheme 선택) 기회가 있다.

## 2. Hard Rules

- **HR-5 준수**: `Time.timeScale` 금지. `Unit.LocalTimeScale` 사용.
- **HR-5 준수**: 타임오버 판정은 Unix ms 타임스탬프. `Time.frameCount` 금지.
- **HR-6 준수**: 액션 발동은 `CombatScheme.ResolveAction(...)` 만 호출.
- **HR-9 준수**: 턴 상태 변화는 이벤트로 브로드캐스트. 다른 모듈은 폴링 금지.
- 동시 턴 활성 가능하되 **같은 팀 내 1명 제한** (우리 유닛 A, B 가 동시에 액션
  고르는 것 금지 — UI 혼란 방지).

## 3. Public API

```csharp
namespace Proto.Turn
{
    public enum TurnPhase
    {
        Idle,           // 게이지 채우는 중
        Selecting,      // 행동 선택 중 (타임슬로우 적용)
        Executing,      // 선택한 행동 실행 중
        Cooldown        // 행동 후 쿨다운 (게이지 다시 차오름)
    }

    public enum Team { Player, Enemy, Neutral }

    public interface IUnit
    {
        int UnitId { get; }
        Team Team { get; }
        bool IsAlive { get; }
        float LocalTimeScale { get; set; }  // 1.0 기본, 슬로우 시 0.05 등
    }

    public interface ITurnActor
    {
        IUnit Unit { get; }
        TurnPhase Phase { get; }
        float GaugeRatio { get; }           // 0~1, 1 도달 시 Selecting 진입
        long SelectingDeadlineUnixMs { get; } // Selecting 중 타임아웃 시점
    }

    public interface ITurnSystem
    {
        IReadOnlyList<ITurnActor> Actors { get; }

        void Register(IUnit unit, TurnProfile profile);
        void Unregister(IUnit unit);

        /// <summary>수동 시작 (전투 개시). 이미 실행 중이면 noop.</summary>
        void Begin();

        /// <summary>수동 종료 (전투 종료 조건 충족 시).</summary>
        void End();

        /// <summary>현재 행동 선택 중인 플레이어 측 actor (없으면 null).</summary>
        ITurnActor PlayerSelectingActor { get; }

        event Action<ITurnActor> PhaseEntered;   // Selecting/Executing 등 진입
        event Action<ITurnActor> PhaseExited;
        event Action<ITurnActor> Timeout;        // Selecting 에서 데드라인 도과
    }
}

[CreateAssetMenu(menuName = "Proto/Turn/TurnProfile")]
public class TurnProfile : ScriptableObject
{
    [Header("Gauge")]
    public float gaugeFillSeconds = 5f;         // 게이지 0→1 기본 소요
    public float gaugeFillRandomness = 0.1f;    // ±10% 랜덤

    [Header("Selection Window")]
    public float selectingSeconds = 3f;         // Selecting 유지 시간
    public float selectingLocalTimeScale = 0.05f; // 이 액터의 LocalTimeScale (슬로우)
    public float selectingAmbientTimeScale = 0.3f;// 주변 액터들의 LocalTimeScale

    [Header("Cooldown")]
    public float cooldownSeconds = 1.5f;        // 액션 후 다음 게이지 시작까지
}
```

## 4. Dependencies

- **Required**:
  - `IUnit` 구현은 Phase B `CharacterRenderKit` 에 속하는 `Unit.cs` 가 담당.
    TurnSystem 자체는 인터페이스만 의존.
- **Optional**:
  - `CombatScheme` — Selecting 진입 시 호출 (어떤 Scheme 인지에 따라 UI 전환)
  - `CameraRig` — Selecting 중인 플레이어 측 actor 로 프레이밍 전환
  - `HUDKit` — 게이지 바, 턴 인디케이터

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Turn/TurnSystem.prefab` | Root GameObject + `TurnSystem` MB |
| `Assets/Proto/Data/Config/Turn_Default.asset` | 기본 `TurnProfile` |
| `Assets/Proto/Data/Config/Turn_Fast.asset` | 짧은 게이지/선택 (테스트용) |
| `Assets/Proto/Data/Config/Turn_Slow.asset` | 긴 게이지/선택 |

Runtime 스크립트:
- `TurnPhase.cs`, `Team.cs`, `IUnit.cs`, `ITurnActor.cs`, `ITurnSystem.cs`
- `TurnProfile.cs`, `TurnSystem.cs`, `TurnActor.cs`

구현 포인트:
- `Update()` 에서 `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()` 로 경과 계산.
  `Time.deltaTime * LocalTimeScale` 로 게이지 증가.
- Selecting 진입 시 **다른 모든 액터**의 LocalTimeScale 을
  `profile.selectingAmbientTimeScale` 로 설정, 자신은 `selectingLocalTimeScale`.
- Selecting 종료 (행동 선택 or 타임아웃) 시 모두 1.0 복귀.

## 6. Skill Hook

`/proto-turn-system` 호출 시:

1. `Assets/Proto/Runtime/Turn/` 생성.
2. 스크립트 생성 (위 목록).
3. TurnProfile SO 3개 생성.
4. `TurnSystem.prefab` 생성 + `ProtoMain.unity` 배치.
5. 콘솔 에러 확인.

**전제**: CharacterRenderKit 이 먼저 설치되어 있지 않으면 `IUnit` 구현체가 없어
실제 전투는 굴릴 수 없음 — 스킬 메시지에 "다음: /proto-character 호출" 안내.

옵션 인자:
- `--profile <default|fast|slow>`

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="TurnSystem", search_method="by_component")` → 1.
3. `execute_code` 스모크 (mock IUnit 등록):
   ```csharp
   var sys = UnityEngine.Object.FindObjectOfType<Proto.Turn.TurnSystem>();
   var mockPlayer = new Proto.Turn.Tests.MockUnit(1, Proto.Turn.Team.Player);
   var mockEnemy  = new Proto.Turn.Tests.MockUnit(2, Proto.Turn.Team.Enemy);
   var profile = Resources.Load<Proto.Turn.TurnProfile>("Turn_Fast");

   sys.Register(mockPlayer, profile);
   sys.Register(mockEnemy, profile);
   sys.Begin();

   // 게이지 0.5초 대기 — Fast 프로필이면 게이지 일부 차야 함
   yield return new WaitForSeconds(0.5f);
   var playerActor = sys.Actors.First(a => a.Unit.UnitId == 1);
   UnityEngine.Debug.Assert(playerActor.GaugeRatio > 0, "gauge did not advance");

   // HR-5 검증
   UnityEngine.Debug.Assert(Mathf.Approximately(Time.timeScale, 1f),
       "HR-5 violation: Time.timeScale modified");
   ```
4. `Tests/MockUnit.cs` 를 에디터 전용 asmdef 에 생성해 위 스모크가 돌도록.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-HARD-RULE-05 (HR-5 Time.timeScale 불변 검증)
- OR-INTEGRATION-02 (Selecting 진입 → 주변 LocalTimeScale 감소 확인)
