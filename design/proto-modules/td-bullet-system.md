# Module: td-bullet-system

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

투사체 시스템. 기본 **hitscan** (즉시 적중). 선택에 따라 projectile 변형 가능.

## 2. Hard Rules

- 시뮬 tick 기준. `Time.deltaTime` 금지.
- Projectile 변형도 결정론 유지 (속도 · 방향 결정적).
- 풀링 강제 (GC 최소화).

## 3. Public API

```csharp
namespace Proto.TD.Bullet
{
    public enum BulletMode { Hitscan, Projectile }

    public sealed class BulletSystem
    {
        public BulletMode Mode { get; set; } = BulletMode.Hitscan;
        public void Fire(float3 origin, EnemyId target, float damage);
        public event Action<EnemyId, float> OnHit;
    }
}
```

## 4. Dependencies

- Required: `td-enemy-core`, `td-sim-core`.
- Optional: `shader-fx-kit` (히트 디졸브), `sound-kit` (발사 SFX).

## 5. Default Prefabs/Assets

- 스크립트: `BulletSystem.cs`.
- 프리팹: `TD_Bullet_Hitscan.prefab` (LineRenderer 0.1초 flash).

## 6. Skill Hook

`/proto-td-bullet`:

1. `BulletSystem.cs` 생성.
2. 히트스캔 풀 초기화 (LineRenderer 16개).
3. 효과음 훅 (선택).

## 7. Verification

- Fire 호출 → 0.1s 내 `OnHit` 발행.
- Target 이 이미 죽었으면 miss 처리 (warning 로그).
