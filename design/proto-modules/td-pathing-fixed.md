# Module: td-pathing-fixed

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q2-A (기본값). 적 경로 = 레벨 JSON 의 웨이포인트 배열. `stage-graph` 재활용.
플레이어 구조물과 무관하게 고정. HR-TD-5 강제.

## 2. Hard Rules

- HR-TD-5: 고정 경로만. 우회·재계산 금지.
- 웨이포인트 최소 2개 (스폰 → 기지), 최대 16개.
- 경로는 레벨 JSON `paths: [[{x,y,z},...],...]` 필드에서 로드.

## 3. Public API

```csharp
namespace Proto.TD.Pathing
{
    public sealed class FixedPathing
    {
        public int PathCount { get; }
        public float3 NextPoint(int pathIndex, float progressMeters);
        public float PathLength(int pathIndex);
        public bool IsAtEnd(int pathIndex, float progressMeters);
    }
}
```

## 4. Dependencies

- Required: `stage-graph`, `td-sim-core`, `td-enemy-core`.

## 5. Default Prefabs/Assets

- 스크립트: `FixedPathing.cs`.
- SO: `Path_Default.asset` (5 웨이포인트 샘플).
- 시각화: Gizmos 기반 에디터 미리보기.

## 6. Skill Hook

`/proto-td-pathing fixed`:

1. `FixedPathing.cs` 생성.
2. `stage-graph` 의 Node/Edge 구조 재사용.
3. 기본 경로 SO 생성 + 샘플 레벨 JSON 경로 연동.
4. Enemy 가 호출할 `NextPoint` API 바인딩.

## 7. Verification

- 경로 1개 → 적이 스폰부터 기지까지 직진.
- 경로 2개 → 적 웨이브 절반은 경로 0, 절반은 1.
- 진행률 100% 시 `IsAtEnd==true`.
