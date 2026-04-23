# Module: td-resource-ledger

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

선(link) 자원 예산 관리. 배치된 링크 비용 합 ≤ 예산 보장. Q5 에 따라 재생 모드 3종.

## 2. Hard Rules

- HR-TD-6 강제: 링크 비용 = 유클리드 거리 × costPerUnit (그리드 모드는 맨해튼).
- 예산 초과 링크 시도 → 거부 이벤트 발행.
- 모드: `fixed` | `regen` | `kill` | `regen+kill`.

## 3. Public API

```csharp
namespace Proto.TD.Resource
{
    public enum RegenMode { Fixed, Regen, Kill, RegenPlusKill }

    public sealed class ResourceLedger
    {
        public float Current { get; }
        public float Max { get; }
        public RegenMode Mode { get; set; }

        public bool TrySpend(float amount);
        public void Refund(float amount);
        public void Grant(float amount);  // 재생/보상

        public event Action<float, float> OnChanged; // current, max
        public event Action<float> OnInsufficient;   // 요청 금액
    }
}
```

## 4. Dependencies

- Required: `td-hub-network`.
- Optional: `td-resource-tick` (regen), `td-kill-reward-hook` (kill), `td-hud-budget`.

## 5. Default Prefabs/Assets

- 스크립트: `ResourceLedger.cs`.
- SO 모드별:
  - `Economy_Fixed_100.asset`
  - `Economy_Regen_2perSec.asset`
  - `Economy_Kill_5perHit.asset`

## 6. Skill Hook

`/proto-td-resource` (인자: `fixed|regen|kill|regen+kill`):

1. `ResourceLedger.cs` 생성.
2. 모드 SO 생성.
3. `td-hub-network` 의 `OnLinkAdded` 구독하여 비용 자동 차감.
4. 인자에 따라 `td-resource-tick` / `td-kill-reward-hook` 추가 설치.

## 7. Verification

- 초기 `Current == Max`.
- Link 추가 시 비용만큼 차감.
- 초과 시도 → `OnInsufficient` 이벤트.
