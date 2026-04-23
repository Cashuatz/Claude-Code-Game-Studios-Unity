# Module: td-game-rule-killcount

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-B. 지정된 적 N마리 처치 시 승리.

## 2. Hard Rules

- `Enemy.OnKilled` 구독으로 카운트.
- 기본 목표 50 kill.

## 3. Public API

```csharp
namespace Proto.TD.Rule
{
    public sealed class KillCountRule
    {
        public int Target { get; }
        public int Current { get; }
        public bool IsWon { get; }
        public event Action<int> OnKillCounted;
        public event Action OnWon;
    }
}
```

## 4. Dependencies

- Required: `td-enemy-core`, `td-sim-core`.
- Optional: `td-hud-killcount`.

## 5. Default Prefabs/Assets

- 스크립트: `KillCountRule.cs`.
- SO: `GameRule_Kill_50.asset`.

## 6. Skill Hook

`/proto-td-rule kill` 또는 `kill+core`.

## 7. Verification

- 50 kill → OnWon.
