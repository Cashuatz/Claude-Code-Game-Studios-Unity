# Module: td-hud-corehp

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-C. 기지 체력 바 (우상단).

## 2. Hard Rules

- `hud-kit` 규약.
- HP < 30% 시 `pp-low-hp` PP 프로파일 자동 가중치 1.0 (post-process-kit 연동).

## 3. Public API

```csharp
namespace Proto.TD.Hud
{
    public sealed class HudCoreHp : IHudWidget
    {
        public string WidgetId => "td-corehp";
        public HudSlot TargetSlot => HudSlot.TopLeft;
    }
}
```

## 4. Dependencies

- Required: `hud-kit`, `td-core-hp`.
- Optional: `post-process-kit`.

## 5. Default Prefabs/Assets

- 프리팹: `TD_HUD_CoreHp.prefab` (Slider + 숫자).

## 6. Skill Hook

`/proto-td-hud corehp`.

## 7. Verification

- HP 변화 시 슬라이더 즉시 반영.
- HP 30% 이하 → PP 저체력 효과.
