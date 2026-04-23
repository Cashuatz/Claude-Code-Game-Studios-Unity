# Module: td-hud-timer

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-A 용 HUD 위젯. 남은 시간 mm:ss 표시.

## 2. Hard Rules

- `hud-kit` 위젯 규약 준수.
- TimedRule 이벤트 구독만. 직접 접근 금지.

## 3. Public API

```csharp
namespace Proto.TD.Hud
{
    public sealed class HudTimer : IHudWidget
    {
        public string WidgetId => "td-timer";
        public HudSlot TargetSlot => HudSlot.TopCenter;
    }
}
```

## 4. Dependencies

- Required: `hud-kit`, `td-game-rule-timed`.

## 5. Default Prefabs/Assets

- 프리팹: `TD_HUD_Timer.prefab` (TMP Text, TopCenter).

## 6. Skill Hook

`/proto-td-hud timer` (또는 `/proto-td-hud` 다중 인자).

## 7. Verification

- 시간 감소 시 실시간 갱신.
- 0 도달 시 녹색 깜박임 애니메이션.
