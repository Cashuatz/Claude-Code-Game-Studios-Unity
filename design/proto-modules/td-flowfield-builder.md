# Module: td-flowfield-builder

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

FlowField 빌더 유틸리티. Brushfire / Dijkstra 기반 목표 방향 계산.
`td-pathing-flowfield` 및 `td-pathing-dynamic` 공용.

## 2. Hard Rules

- 순수 C# (HR-TD-1) — UnityEngine 참조 금지.
- 동일 입력 → 동일 출력 (결정론).

## 3. Public API

```csharp
namespace Proto.TD.Pathing
{
    public static class FlowFieldBuilder
    {
        public static float3[,] Build(
            int width, int height, float cellSize,
            int2 goal,
            bool[,] blocked
        );
    }
}
```

## 4. Dependencies

- Required: `td-sim-core` (순수 C# 환경).

## 5. Default Prefabs/Assets

- 스크립트: `FlowFieldBuilder.cs`.

## 6. Skill Hook

`/proto-td-flowfield`:

1. `FlowFieldBuilder.cs` 생성 (pure C#).
2. 단위 테스트: 10x10 그리드 + 중앙 목표 → 모든 타일 벡터 검증.

## 7. Verification

- 순수 C# asmdef 에 포함.
- 동일 입력 2회 → bit-exact 동일 출력.
