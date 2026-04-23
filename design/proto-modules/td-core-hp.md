# Module: td-core-hp

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

기지(Core) 체력. Q4-C (기지 방어) 선택 시 필수.

## 2. Hard Rules

- 적이 `OnReachedCore` 발행 시 기지 HP 에서 `EnemySO.damageToCore` 차감.
- HP ≤ 0 → `OnCoreDestroyed` 발행 (게임 패배 트리거).

## 3. Public API

```csharp
namespace Proto.TD.Rule
{
    public sealed class CoreHp
    {
        public int Current { get; }
        public int Max { get; }
        public void Damage(int amount);
        public event Action<int, int> OnChanged;
        public event Action OnCoreDestroyed;
    }
}
```

## 4. Dependencies

- Required: `td-enemy-core`, `td-sim-core`.
- Optional: `td-hud-corehp`.

## 5. Default Prefabs/Assets

- 스크립트: `CoreHp.cs`.
- SO: `CoreHp_Default_100.asset`.

## 6. Skill Hook

`/proto-td-core-hp`:

1. `CoreHp.cs` 생성.
2. SO 생성 (max=100).
3. Enemy.OnReachedCore 구독.

## 7. Verification

- 초기 Current = 100.
- 적 1마리 도달 → HP 감소.
- HP 0 → OnCoreDestroyed.
