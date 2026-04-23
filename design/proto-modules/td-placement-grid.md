# Module: td-placement-grid

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q1-A (Grid). 셀 스냅 배치 모드. 허브·타워는 셀 중심에만 배치. 맨해튼 거리 기반 연결 비용.

## 2. Hard Rules

- 허브 위치는 정수 셀 좌표 (`int2`) 로만 저장.
- 같은 셀에 2개 이상 허브 금지.
- 셀 외부 배치 시도 → 이벤트 `OnPlacementRejected` 발행, 배치 취소.

## 3. Public API

```csharp
namespace Proto.TD.Placement
{
    public sealed class GridPlacement
    {
        public bool TryPlace(HubType kind, int2 cell, out HubId id);
        public void Remove(HubId id);
        public bool IsCellOccupied(int2 cell);
        public event Action<HubId, int2> OnPlaced;
        public event Action<int2, string> OnPlacementRejected;  // reason
    }

    public enum HubType { PowerSource, Relay, Tower }
}
```

## 4. Dependencies

- Required: `td-hub-network`, `td-level-template-*`.
- Optional: `td-resource-ledger` (예산 체크).

## 5. Default Prefabs/Assets

- 스크립트: `Assets/Proto/Runtime/TD/Placement/GridPlacement.cs`.
- 프리팹: `TD_Cell_Highlight.prefab` (마우스 호버 표시).

## 6. Skill Hook

`/proto-td-placement grid`:

1. `GridPlacement.cs` 생성.
2. Input → 셀 좌표 변환 유틸 추가 (`Camera.ScreenPointToRay` + 평면 교차).
3. 셀 하이라이트 프리팹 생성.
4. 허브 프리팹 프로토타입 3종 (`TD_Hub_Power.prefab`, `_Relay.prefab`, `_Tower.prefab`) 배치 테스트.

## 7. Verification

- 마우스 클릭 → 셀 스냅 허브 배치.
- 점유 셀 재클릭 → `OnPlacementRejected` 발행.
- 외부 셀 클릭 → 배치 실패.
