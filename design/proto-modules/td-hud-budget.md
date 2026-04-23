# Module: td-hud-budget

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

선 자원 예산 UI (항상 활성). 남은 자원 / 최대 표시.

## 2. Hard Rules

- `hud-kit` 규약.
- `ResourceLedger.OnChanged` 구독.

## 3. Public API

```csharp
namespace Proto.TD.Hud
{
    public sealed class HudBudget : IHudWidget
    {
        public string WidgetId => "td-budget";
        public HudSlot TargetSlot => HudSlot.BottomCenter;
    }
}
```

## 4. Dependencies

- Required: `hud-kit`, `td-resource-ledger`.

## 5. Default Prefabs/Assets

- 프리팹: `TD_HUD_Budget.prefab` (큰 숫자 + 단위 "m").

## 6. Skill Hook

`/proto-td-hud budget` (또는 base 설치 시 자동 포함).

## 7. Verification

- 링크 추가 시 숫자 감소.
- 0 도달 시 적색 경고.
