# Module: td-game-rule-timed

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-A. 제한 시간 동안 버티면 승리.

## 2. Hard Rules

- 시간은 시뮬 tick 기반 (HR-TD-3).
- 기본 3분 (180s).

## 3. Public API

```csharp
namespace Proto.TD.Rule
{
    public sealed class TimedRule
    {
        public float DurationSeconds { get; }
        public float ElapsedSeconds { get; }
        public bool IsWon { get; }
        public event Action OnWon;
    }
}
```

## 4. Dependencies

- Required: `td-sim-core`.
- Optional: `td-hud-timer`, `td-game-rule-coredefense` (조합).

## 5. Default Prefabs/Assets

- 스크립트: `TimedRule.cs`.
- SO: `GameRule_Timed_3min.asset`.

## 6. Skill Hook

`/proto-td-rule timed` 또는 `timed+core`:

1. `TimedRule.cs` 생성.
2. SO 생성 (180s).
3. SimCore.OnTick 구독.
4. ElapsedSeconds ≥ Duration → OnWon.

## 7. Verification

- 180초 시뮬 → OnWon 발행.
- CoreHp 조합 시 CoreHp 가 먼저 0 이면 패배 우선.
