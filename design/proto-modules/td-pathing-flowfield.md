# Module: td-pathing-flowfield

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q2-B. 목표 지점에서 역전파된 **벡터장** 을 따라 적 이동. 정적 (처음 1회 빌드).

## 2. Hard Rules

- HR-TD-5 완화: 고정 경로 대신 flow field.
- 빌드는 레벨 로드 시 1회만. 런타임 재빌드 금지 (dynamic 은 `td-pathing-dynamic`).
- 타일 해상도 기본 1m/cell. 높은 해상도는 성능 부담.

## 3. Public API

```csharp
namespace Proto.TD.Pathing
{
    public sealed class FlowFieldPathing
    {
        public float3 Direction(float3 worldPos);
        public bool IsReachable(float3 worldPos);
    }
}
```

## 4. Dependencies

- Required: `td-flowfield-builder`, `td-sim-core`, `td-enemy-core`.

## 5. Default Prefabs/Assets

- 스크립트: `FlowFieldPathing.cs`.
- SO: `FlowField_Default.asset` (런타임 채워짐).

## 6. Skill Hook

`/proto-td-pathing flowfield`:

1. `FlowFieldPathing.cs` 생성.
2. `td-flowfield-builder` 자동 설치 (의존성).
3. 레벨 로드 시 FlowField 빌드 실행.
4. 시각화: 에디터 Gizmos 로 벡터 화살표 표시.

## 7. Verification

- 빌드 후 모든 타일이 목표 방향 벡터 보유.
- 도달 불가 영역 `IsReachable=false`.
