# Module: td-camera-mode-toggle

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q3-C (가변 카메라). TopDown ↔ QuarterBuilder ↔ Orbit 전환 버튼 UI.

## 2. Hard Rules

- 모드 전환은 버튼 입력으로만 (자동 전환 금지 — 프로토 범위).
- `camera-rig.RequestMode` 경유.

## 3. Public API

```csharp
namespace Proto.TD.Camera
{
    public sealed class TdCameraModeToggle
    {
        public void ToggleNext();
        public void SetMode(CameraMode mode);
        public event Action<CameraMode> OnModeChanged;
    }
}
```

## 4. Dependencies

- Required: `camera-rig`, `td-camera-pan-zoom`, `hud-kit`.

## 5. Default Prefabs/Assets

- 스크립트: `TdCameraModeToggle.cs`.
- 프리팹: `TD_HUD_CameraToggle.prefab` (3 버튼 세트).

## 6. Skill Hook

`/proto-td-camera-modes`:

1. `TdCameraModeToggle.cs` 생성.
2. HUD 버튼 프리팹 생성 (TopLeft 슬롯 배치).
3. Orbit 프로파일 추가.

## 7. Verification

- 버튼 클릭 → 카메라 모드 전환.
- 전환 중 블렌드 0.3s.
