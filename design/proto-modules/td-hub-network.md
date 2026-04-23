# Module: td-hub-network

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

발전소–릴레이–타워 **연결 그래프**. 선(link) 연결 상태에 따라 타워의 전력 상태를 전파.
HR-TD-7 에 따라 타워는 이 모듈의 이벤트로만 전력 상태를 안다.

## 2. Hard Rules

- 발전소(Power Source) 노드에서 도달 불가능한 타워는 `isPowered=false`.
- 네트워크 변경(허브 추가/제거/링크 추가/제거) → 재계산 → `OnPowerChanged` 발행.
- 같은 두 허브 간 중복 링크 금지.
- 링크 비용 = 유클리드(또는 맨해튼 — 배치 모드 따름) 거리 × `costPerUnit`.

## 3. Public API

```csharp
namespace Proto.TD.Network
{
    public struct HubId { public int Value; }
    public struct LinkId { public int Value; }

    public sealed class HubNetwork
    {
        public HubId AddHub(HubType kind, float3 worldPos);
        public void RemoveHub(HubId id);
        public bool TryAddLink(HubId a, HubId b, out LinkId linkId, out float cost);
        public void RemoveLink(LinkId id);

        public bool IsPowered(HubId tower);
        public float TotalLinkCost { get; }

        public event Action<HubId, bool> OnPowerChanged;
        public event Action<HubId> OnHubAdded;
        public event Action<LinkId> OnLinkAdded;
    }
}
```

## 4. Dependencies

- Required: `td-placement-*` 중 하나.
- Optional: `td-resource-ledger` (비용 차감), `td-tower-core` (전력 이벤트 구독).

## 5. Default Prefabs/Assets

- 스크립트: `HubNetwork.cs`, `NetworkReachability.cs`.
- 프리팹: `TD_Link_Line.prefab` (LineRenderer 기반 선 시각화).

## 6. Skill Hook

`/proto-td-hub`:

1. `HubNetwork.cs` 생성 (인접 리스트 + BFS 도달성).
2. `TD_Link_Line.prefab` 생성.
3. 런타임 시각화: 각 링크마다 LineRenderer 인스턴스.
4. 전력 상태 UI 표시 (타워 위 녹색/적색 표식).

## 7. Verification

- 발전소↔타워 직접 연결 → `IsPowered=true`.
- 발전소 제거 → `OnPowerChanged(false)` 발행.
- 중복 링크 시도 → 실패.
