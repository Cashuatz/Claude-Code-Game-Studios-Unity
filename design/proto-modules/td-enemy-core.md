# Module: td-enemy-core

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

적 유닛 정의 + 이동 + HP. 이동은 선택된 `td-pathing-*` 에 위임.

## 2. Hard Rules

- HR-1 (Rigidbody 금지). Transform + 수동 이동.
- 공격 없음 (적은 기지 도달만 목표. 타워 공격은 무시).
- 사망 시 `OnEnemyKilled` 이벤트 발행.

## 3. Public API

```csharp
namespace Proto.TD.Enemy
{
    public struct EnemyId { public int Value; }

    public sealed class Enemy
    {
        public EnemyId Id { get; }
        public EnemySO Def { get; }
        public float Hp { get; }
        public bool IsDead { get; }

        public void TakeDamage(float amount);
        public event Action<EnemyId> OnReachedCore;
        public event Action<EnemyId> OnKilled;
    }

    [CreateAssetMenu]
    public class EnemySO : ScriptableObject
    {
        public string Id;
        public float MaxHp;
        public float MoveSpeed;
        public float KillReward;
        public int DamageToCore;
        public GameObject VisualPrefab;
    }
}
```

## 4. Dependencies

- Required: `td-pathing-*` 중 하나, `td-sim-core`, `character-render-kit`.

## 5. Default Prefabs/Assets

- 스크립트: `Enemy.cs`, `EnemySO.cs`.
- SO: `Enemy_Basic.asset` (hp=30, speed=1, reward=5, damage=1).
- 프리팹: `TD_Enemy_Basic.prefab`.

## 6. Skill Hook

`/proto-td-enemy-core`:

1. `Enemy.cs`, `EnemySO.cs` 생성.
2. 기본 SO + 프리팹 생성.
3. 이동: 선택된 pathing 모듈의 `NextPoint` / `Direction` 호출.
4. Shader FX (dissolve-in) 적용 — 스폰 시.

## 7. Verification

- 적 스폰 → 경로 이동 → 기지 도달 시 `OnReachedCore`.
- TakeDamage 로 HP 0 이면 `OnKilled` + dissolve-out.
