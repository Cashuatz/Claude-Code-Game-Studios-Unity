# Module: td-kill-reward-hook

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q5-C 전용 서브모듈. 적 사망 시 `ResourceLedger.Grant(reward)` 호출.

## 2. Hard Rules

- 적 유형별 보상값은 `EnemySO.killReward` 필드에서 읽음.
- 보상은 **시뮬 tick 내부**에서 발생 (결정론 유지).

## 3. Public API

```csharp
namespace Proto.TD.Resource
{
    public sealed class KillRewardHook
    {
        public void OnEnemyKilled(EnemyId id, EnemySO def, ResourceLedger ledger);
    }
}
```

## 4. Dependencies

- Required: `td-resource-ledger`, `td-enemy-core`.

## 5. Default Prefabs/Assets

- 스크립트: `KillRewardHook.cs`.
- SO: `Economy_Kill_5perHit.asset`.

## 6. Skill Hook

`/proto-td-resource kill` 호출 시 자동 포함.

1. `KillRewardHook.cs` 생성.
2. `td-enemy-core` 의 `OnEnemyKilled` 이벤트 구독.

## 7. Verification

- 적 처치 시 ledger.Current 가 `EnemySO.killReward` 만큼 증가.
- Max 초과 시 클램프.
