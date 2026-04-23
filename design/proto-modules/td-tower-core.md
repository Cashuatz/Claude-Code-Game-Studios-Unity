# Module: td-tower-core

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

타워 정의 + 사거리·공격속도·타겟팅. 전력 상태는 `td-hub-network` 이벤트로만 수신.

## 2. Hard Rules

- HR-TD-7 강제: `HubNetwork.OnPowerChanged` 구독만으로 전력 반영.
- 공격은 시뮬 tick 기준 (HR-TD-3), `Time.deltaTime` 금지.
- 스탯은 `TowerSO` 에서 로드, 런타임 수정 금지.

## 3. Public API

```csharp
namespace Proto.TD.Tower
{
    public enum TargetingMode { FirstInRange, ClosestToCore, LowestHp, HighestHp }

    public sealed class Tower
    {
        public TowerSO Def { get; }
        public HubId HubId { get; }
        public bool IsPowered { get; }
        public TargetingMode Targeting { get; set; }

        public event Action<EnemyId> OnFired;
    }

    [CreateAssetMenu]
    public class TowerSO : ScriptableObject
    {
        public string Id;
        public float Range;
        public float FireRate;      // shots per second
        public float Damage;
        public GameObject VisualPrefab;
    }
}
```

## 4. Dependencies

- Required: `td-hub-network`, `td-bullet-system`, `td-enemy-core`, `td-sim-core`.

## 5. Default Prefabs/Assets

- 스크립트: `Tower.cs`, `TowerSO.cs`.
- SO: `Tower_Basic.asset` (range=3, fireRate=1, damage=10).
- 프리팹: `TD_Tower_Basic.prefab` (간단한 터렛 비주얼).

## 6. Skill Hook

`/proto-td-tower`:

1. `Tower.cs`, `TowerSO.cs` 생성.
2. `Tower_Basic.asset` 생성.
3. `TD_Tower_Basic.prefab` 생성 (캡슐 + 큐브 상판).
4. `td-bullet-system` 연동: 발사 시 `BulletSystem.Fire(origin, target, damage)`.

## 7. Verification

- 사거리 내 적 감지.
- FireRate 에 맞춰 발사.
- Power off 시 미동작.
