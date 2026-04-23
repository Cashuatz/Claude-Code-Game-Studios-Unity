# Module: td-game-rule-coredefense

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-C. 기지 체력 0 = 패배. 단독 또는 timed/kill 과 조합.

## 2. Hard Rules

- `CoreHp.OnCoreDestroyed` 구독으로 패배 판정.
- 단독 모드: 모든 웨이브 끝까지 HP > 0 이면 승리.

## 3. Public API

```csharp
namespace Proto.TD.Rule
{
    public sealed class CoreDefenseRule
    {
        public bool IsLost { get; }
        public bool IsWon { get; }
        public event Action OnLost;
        public event Action OnWon;
    }
}
```

## 4. Dependencies

- Required: `td-core-hp`, `td-wave-spawner`, `td-sim-core`.
- Optional: `td-hud-corehp`.

## 5. Default Prefabs/Assets

- 스크립트: `CoreDefenseRule.cs`.
- SO: `GameRule_Core_100hp.asset`.

## 6. Skill Hook

`/proto-td-rule core`:

1. `CoreDefenseRule.cs` 생성.
2. `td-core-hp` 자동 설치.
3. WaveSpawner.IsComplete + CoreHp > 0 → OnWon.

## 7. Verification

- HP 0 → OnLost.
- 모든 웨이브 완료 + HP > 0 → OnWon.
