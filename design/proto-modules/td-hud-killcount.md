# Module: td-hud-killcount

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-B. "N / Target" 처치 수 표시.

## 2. Hard Rules

- `hud-kit` 규약.
- KillCountRule 이벤트만 구독.

## 3. Public API

```csharp
namespace Proto.TD.Hud
{
    public sealed class HudKillCount : IHudWidget
    {
        public string WidgetId => "td-killcount";
        public HudSlot TargetSlot => HudSlot.TopRight;
    }
}
```

## 4. Dependencies

- Required: `hud-kit`, `td-game-rule-killcount`.

## 5. Default Prefabs/Assets

- 프리팹: `TD_HUD_KillCount.prefab`.

## 6. Skill Hook

`/proto-td-hud killcount`.

## 7. Verification

- 적 처치마다 N 증가.
- N == Target 시 강조.
