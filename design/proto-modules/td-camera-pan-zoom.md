# Module: td-camera-pan-zoom

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

TD 카메라 Pan + Zoom 입력. `camera-rig` 에 TopDown/QuarterBuilder 모드 추가하고
마우스 드래그 / 휠 줌 / WASD 패닝 제공.

## 2. Hard Rules

- `camera-rig` 의 `RequestMode` API 만 사용 (HR-4).
- 줌 한계 (min/max) SO 에 정의.
- 맵 바운더리 밖 Pan 금지.

## 3. Public API

```csharp
namespace Proto.TD.Camera
{
    public sealed class TdCameraPanZoom
    {
        public float Zoom { get; set; }
        public float3 Focus { get; set; }
        public void Pan(float2 delta);
        public void ZoomBy(float delta);
    }
}
```

## 4. Dependencies

- Required: `camera-rig`, `td-level-template-*`.

## 5. Default Prefabs/Assets

- 스크립트: `TdCameraPanZoom.cs`.
- SO: `CameraProfile_TopDown.asset`, `CameraProfile_Quarter.asset`.

## 6. Skill Hook

`/proto-td-camera-input`:

1. `TdCameraPanZoom.cs` 생성.
2. 카메라 프로파일 SO 2종 생성.
3. Input System 바인딩: 우클릭+드래그 Pan, 휠 Zoom, WASD Pan.
4. `camera-rig.RequestMode` 로 모드 활성.

## 7. Verification

- 마우스 드래그 → Pan 이동.
- 휠 → 줌 변화.
- 바운더리 외 Pan 시도 → 클램프.
