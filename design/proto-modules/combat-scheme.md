# Module: CombatScheme

> 참고: [`_catalog.md`](./_catalog.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

Selecting 상태의 유닛이 **어떻게 액션을 선택하는가**를 정의하는 **교체 가능한
플러그인**. 같은 TurnSystem 위에서 4가지 입력 패러다임 중 하나를 끼울 수 있음:

- **Auto**     — 자동 타게팅, 선택 불필요 (AI 기본값)
- **Card**     — 블루아카이브식 스킬카드 탭
- **Rhythm**   — 비트에 맞춘 버튼 입력
- **QTE**      — 순간 타이밍 입력 (액션게임스러움)

CombatScheme 교체만으로 같은 게임 시스템을 **전혀 다른 장르 느낌**으로 만들 수 있다.
기획 단계에서 가장 많이 실험할 지점이라 **런타임 핫스왑** 지원.

## 2. Hard Rules

- **HR-6 준수**: 유닛의 키 입력, 터치 감지는 전부 Scheme 내부. 유닛 스크립트가 직접 읽지 않음.
- **HR-8 준수**: 수치·밸런싱 파라미터는 `CombatSchemeConfig` SO.
- **HR-11 준수**: UI 는 uGUI. 각 Scheme 마다 Canvas 패널 프리팹 보유.
- Scheme 은 **1개만 활성** 가능. 교체 시 이전 Scheme 의 `OnDetach()` 호출 후 새 것 `OnAttach()`.
- Scheme 은 **stateless** 지향. 상태는 전부 `TurnActor` / `AbilityCatalog` 측에 저장.

## 3. Public API

```csharp
namespace Proto.Combat
{
    public enum SchemeKind { Auto, Card, Rhythm, QTE }

    /// <summary>액션 해결 요청. Scheme 이 판정 후 결과를 반환.</summary>
    public readonly struct ActionRequest
    {
        public ITurnActor Actor { get; }
        public IReadOnlyList<AbilityDefinition> AvailableAbilities { get; }
        public long DeadlineUnixMs { get; }   // TurnSystem 이 강제하는 마감
    }

    public readonly struct ActionResult
    {
        public AbilityDefinition ChosenAbility { get; }
        public IReadOnlyList<IUnit> Targets { get; }
        public float QualityScore { get; }    // 0~1, Rhythm/QTE 타이밍 점수. Auto/Card 는 1.0.
        public bool TimedOut { get; }
    }

    public interface ICombatScheme
    {
        SchemeKind Kind { get; }
        bool IsActive { get; }

        void OnAttach(CombatSchemeContext context);
        void OnDetach();

        /// <summary>Selecting 진입 시 TurnSystem 이 호출. 비동기 완료 대기.</summary>
        Task<ActionResult> ResolveAction(ActionRequest request, CancellationToken ct);
    }

    public class CombatSchemeContext
    {
        public ITurnSystem Turn { get; }
        public Canvas SchemeCanvas { get; }   // 이 Scheme 용으로 할당된 Canvas 패널
        public CombatSchemeConfig Config { get; }
    }
}

[CreateAssetMenu(menuName = "Proto/Combat/SchemeConfig")]
public class CombatSchemeConfig : ScriptableObject
{
    public SchemeKind kind;

    [Header("Card Scheme")]
    public int cardHandSize = 3;
    public float cardRechargeSeconds = 2f;

    [Header("Rhythm Scheme")]
    public float beatsPerMinute = 120f;
    public float hitWindowMs = 200f;

    [Header("QTE Scheme")]
    public float qteWindowMs = 400f;
    public KeyCode qteKey = KeyCode.Space;
}
```

### 기본 구현 4종

| Scheme | 동작 요약 |
|--------|-----------|
| `AutoScheme` | `AvailableAbilities[0]` + 자동 타게팅. `QualityScore = 1`. 즉시 완료. |
| `CardScheme` | uGUI 패널에 `cardHandSize` 개 카드 Button. 터치 시 해당 ability 사용. 타겟은 자동 (추후 `TargetPicker` 분리). |
| `RhythmScheme` | BPM 기반 메트로놈 + 화면 링. 링이 맞으면 히트. 판정별 `QualityScore`. |
| `QTEScheme` | 선택된 ability 발동 순간 `qteWindowMs` 내 키 입력 요구. 성공/실패 판정. |

## 4. Dependencies

- **Required**: `TurnSystem`, `AbilityCatalog` (아직 없지만 Phase C 에서 연결)
- **Optional**:
  - `HUDKit` — 카드 UI, 리듬 링 등의 비주얼 자산
  - `SoundKit` — 리듬 박자, QTE 피드백 SFX

Phase B 시점 (AbilityCatalog 없음)에는 **stub AbilityDefinition** 을 임시로 둠
(Phase C 에서 실제 정의로 교체).

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Combat/CombatSchemeRoot.prefab` | Canvas + `CombatSchemeHost` MB |
| `Assets/Proto/Prefabs/Combat/Scheme_Auto.prefab` | Auto 패널 (빈 UI) |
| `Assets/Proto/Prefabs/Combat/Scheme_Card.prefab` | Card 패널 (3개 Button + HorizontalLayoutGroup) |
| `Assets/Proto/Prefabs/Combat/Scheme_Rhythm.prefab` | Rhythm 패널 (중앙 링 Image + 메트로놈) |
| `Assets/Proto/Prefabs/Combat/Scheme_QTE.prefab` | QTE 패널 (중앙 링 + 키 가이드) |
| `Assets/Proto/Data/Config/Scheme_Auto.asset` | `CombatSchemeConfig` |
| `Assets/Proto/Data/Config/Scheme_Card.asset` | |
| `Assets/Proto/Data/Config/Scheme_Rhythm.asset` | |
| `Assets/Proto/Data/Config/Scheme_QTE.asset` | |

Runtime 스크립트:
- `SchemeKind.cs`, `ActionRequest.cs`, `ActionResult.cs`, `ICombatScheme.cs`
- `CombatSchemeContext.cs`, `CombatSchemeConfig.cs`, `CombatSchemeHost.cs`
- `AutoScheme.cs`, `CardScheme.cs`, `RhythmScheme.cs`, `QTEScheme.cs`
- `AbilityDefinition.cs` (Phase B stub — Phase C 에서 확장)

## 6. Skill Hook

`/proto-combat-scheme <auto|card|rhythm|qte>` 호출 시:

1. `Assets/Proto/Runtime/Combat/` 생성 (첫 호출 시).
2. 첫 호출 시 **공통 인프라** 설치: `ICombatScheme`, `CombatSchemeHost`, `ActionRequest/Result`, stub `AbilityDefinition`.
3. **지정 Scheme** 만 추가 설치: 해당 `*.cs` + `Scheme_*.prefab` + `Scheme_*.asset`.
4. `CombatSchemeHost` MB 는 `TurnSystem.PhaseEntered` 구독, Selecting 진입 시 현재 Scheme 의 `ResolveAction` 호출.
5. 씬에 활성 Scheme 패널만 `SetActive(true)`.
6. 콘솔 에러 확인.

두 번째 이후 호출 시:
- 기존 Scheme 을 교체하지 않고 **추가 설치**. 런타임에 `CombatSchemeHost.SetActiveScheme(kind)` 호출로 전환.

옵션 인자:
- `--replace` — 기존 활성 Scheme 을 즉시 새 것으로 교체 (에디터 디폴트 변경).

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="CombatSchemeHost", search_method="by_component")` → 1.
3. 첫 호출 (`/proto-combat-scheme auto`) 직후 `execute_code`:
   ```csharp
   var host = UnityEngine.Object.FindObjectOfType<Proto.Combat.CombatSchemeHost>();
   UnityEngine.Debug.Assert(host.ActiveScheme?.Kind == Proto.Combat.SchemeKind.Auto);
   ```
4. 추가 호출 (`/proto-combat-scheme card`) 후:
   ```csharp
   host.SetActiveScheme(Proto.Combat.SchemeKind.Card);
   UnityEngine.Debug.Assert(host.ActiveScheme.Kind == Proto.Combat.SchemeKind.Card);
   // 이전 Auto 프리팹은 비활성화되었는지
   var autoPanel = GameObject.Find("Scheme_Auto");
   UnityEngine.Debug.Assert(!autoPanel.activeSelf);
   ```
5. HR-6 검증 — 프로토 코드 전체에서 `Input.GetKey` / `Touchscreen` 직접 참조가
   `Assets/Proto/Runtime/Combat/` 외부에 없는지 grep.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-INTEGRATION-03 (Scheme 핫스왑)
- OR-HARD-RULE-06 (HR-6 입력 격리 검증)
