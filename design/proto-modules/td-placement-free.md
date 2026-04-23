# Module: td-placement-free

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q1-B (Free, 기본값). 자유 배치 모드. 월드 좌표 `float3` 로 허브 배치. 최소 간격 제약.

## 2. Hard Rules

- 허브 간 최소 거리 `minSpacing` (기본 1.0m) 강제. 위반 시 배치 거부.
- 맵 바운더리 외 배치 금지.
- 유클리드 거리로 연결 비용 계산 (HR-TD-6).

## 3. Public API

```csharp
namespace Proto.TD.Placement
{
    public sealed class FreePlacement
    {
        public float MinSpacing { get; set; } = 1.0f;
        public bool TryPlace(HubType kind, float3 worldPos, out HubId id);
        public void Remove(HubId id);
        public event Action<HubId, float3> OnPlaced;
        public event Action<float3, string> OnPlacementRejected;
    }
}
```

## 4. Dependencies

- Required: `td-hub-network`, `td-level-template-*`.
- Optional: `td-resource-ledger`.

## 5. Default Prefabs/Assets

- 스크립트: `FreePlacement.cs`.
- SO: `PlacementConfig_Free.asset` (minSpacing, 바운더리).
- 프리팹: `TD_FreePreview.prefab` (드래그 프리뷰).

## 6. Skill Hook

`/proto-td-placement free`:

1. `FreePlacement.cs` 생성.
2. `PlacementConfig_Free.asset` 생성 (minSpacing=1.0).
3. 드래그 프리뷰 프리팹 생성.
4. 허브 프리팹 3종 공용 (Grid 와 동일).
5. 배치 테스트: 유효 위치 → 성공, 근접 위치 → 거부.

## 7. Verification

- 마우스 드래그 → 프리뷰 이동.
- 유효 드롭 → 허브 생성.
- minSpacing 위반 → 거부 이벤트.
