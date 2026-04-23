# Module: td-resource-tick

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q5-B 전용 서브모듈. 시간당 자원 재생. SimCore tick 마다 `ResourceLedger.Grant` 호출.

## 2. Hard Rules

- SimCore tick 기준 재생 (HR-TD-3). `Time.deltaTime` 금지.
- 재생량은 SO 값 고정, 런타임 수정 금지 (프로토 범위).

## 3. Public API

```csharp
namespace Proto.TD.Resource
{
    public sealed class ResourceTick
    {
        public float PerSecond { get; set; } = 2f;
        public void OnSimTick(float tickSeconds, ResourceLedger ledger);
    }
}
```

## 4. Dependencies

- Required: `td-resource-ledger`, `td-sim-core`.

## 5. Default Prefabs/Assets

- 스크립트: `ResourceTick.cs`.
- SO: `Economy_Regen_2perSec.asset` (`td-resource-ledger` 의 regen 모드에서 공유).

## 6. Skill Hook

`/proto-td-resource regen` 호출 시 자동 포함. 개별 호출 불필요.

1. `ResourceTick.cs` 생성.
2. SimCore.OnTick 구독 등록.

## 7. Verification

- 10초 시뮬 → 20 자원 증가 (perSecond=2).
- Current > Max 초과 시 Max 로 클램프.
