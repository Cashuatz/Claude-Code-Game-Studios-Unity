# Module: td-minimap

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

미니맵. Q7-C (Large) 필수, Q7-B (Medium) 선택.

## 2. Hard Rules

- 별도 Camera 인스턴스 (미니맵 전용, Orthographic Top-Down).
- RenderTexture → uGUI RawImage 로 표시.
- 적/타워/기지/발전소 아이콘 색 구분.

## 3. Public API

```csharp
namespace Proto.TD.Hud
{
    public sealed class Minimap : IHudWidget
    {
        public string WidgetId => "td-minimap";
        public HudSlot TargetSlot => HudSlot.BottomRight;
        public void SetZoom(float z);
        public void SetFocus(float3 worldPos);
    }
}
```

## 4. Dependencies

- Required: `hud-kit`, `td-level-template-*`, `td-hub-network`, `td-enemy-core`.

## 5. Default Prefabs/Assets

- 프리팹: `TD_HUD_Minimap.prefab` (RawImage 256x256).
- Camera: `TD_MinimapCamera.prefab`.
- RT: `TD_Minimap.rendertexture`.

## 6. Skill Hook

`/proto-td-minimap`:

1. Minimap 카메라 + RT + RawImage 설정.
2. 아이콘 렌더 레이어 분리.
3. 클릭 시 메인 카메라 Focus 이동.

## 7. Verification

- 미니맵에 맵 전체 표시.
- 적 이동이 실시간 반영.
- 클릭 → 카메라 포커스 이동.
