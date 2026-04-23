# Module: StageGraph

> 참고: [`_catalog-turn3d.md`](./_catalog-turn3d.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

시가지 레이아웃을 **노드-엣지 그래프**로 표현한다. 각 노드는 하나의 "스테이지"
(전투가 벌어지거나, 상점/이벤트가 배치될 수 있는 공간). 엣지는 이동 가능한
통로. PCG 가 이 구조를 런타임에 생성하고, PathPicker 가 엣지를 선택해 다음
노드로 이동한다.

**씬이 아니다** (HR-2). 단일 씬 안에서 런타임 GameObject 로만 구성.

## 2. Hard Rules

- **HR-3 준수**: 노드당 **최대 3방향**. 진입한 방향(`entryDirection`)으로는
  되돌아가지 않는다. `GetAvailableExits()` 에서 자동 필터.
- **HR-2 준수**: 씬 전환 금지. 노드 전환 시 현재 노드 GameObject 비활성화 +
  다음 노드 활성화 (또는 런타임 생성).
- **HR-3 준수**: PCG 시드를 알고 있어야 재현 가능. `StageGraph.Seed` 필드.
- 그래프 구조는 **immutable after creation** — 런 시작 시 완성되고 런 중 변경 금지.
- 노드 좌표는 **월드 공간**. 상대 좌표 계산 금지.

## 3. Public API

```csharp
namespace Proto.Stage
{
    public enum Direction { North, East, South, West }   // 4방위 기준

    public static class DirectionExtensions
    {
        public static Direction Opposite(this Direction d) =>
            (Direction)(((int)d + 2) % 4);
    }

    public interface IStageNode
    {
        int Id { get; }
        Vector3 WorldPosition { get; }
        StageNodeKind Kind { get; }             // Start/Combat/Shop/Event/Boss/End
        IReadOnlyList<StageEdge> Edges { get; } // 이 노드에서 나가는 모든 엣지 (<=3)
    }

    public readonly struct StageEdge
    {
        public int FromId { get; }
        public int ToId { get; }
        public Direction Direction { get; }     // From 기준
        public float Length { get; }            // 월드 거리
    }

    public enum StageNodeKind { Start, Combat, Shop, Event, Boss, End }

    public interface IStageGraph
    {
        int Seed { get; }
        IReadOnlyList<IStageNode> Nodes { get; }
        IStageNode Start { get; }
        IStageNode CurrentNode { get; }
        Direction? LastEntryDirection { get; }  // 현재 노드에 진입한 방향

        IStageNode GetNode(int id);
        IReadOnlyList<StageEdge> GetAvailableExits();   // HR-3: entryDirection 제외

        /// <summary>엣지 통해 이동. entryDirection 자동 갱신.</summary>
        void TraverseEdge(StageEdge edge);

        event Action<IStageNode, IStageNode> NodeChanged;   // (from, to)
    }
}
```

## 4. Dependencies

- **Required**: 없음 (컨테이너)
- **Optional**:
  - `PCGCityLayout` — 실제 노드/엣지를 생성
  - `MovementCore` — `TraverseEdge` 시 유닛들을 `MoveTo(node.WorldPosition)` 호출
  - `PathPicker` — 사용 가능 엣지를 UI 로 표시
  - `SaveKit` — `Seed` 저장/로드로 런 재현

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Stage/StageRoot.prefab` | 빈 GameObject + `StageGraph` MB. ProtoMain 에 1개. |
| `Assets/Proto/Prefabs/Stage/Node_Combat.prefab` | 전투 노드 비주얼 (placeholder 큐브 + 바닥) |
| `Assets/Proto/Prefabs/Stage/Node_Shop.prefab` | 상점 노드 |
| `Assets/Proto/Prefabs/Stage/Node_Event.prefab` | 이벤트 노드 |
| `Assets/Proto/Prefabs/Stage/Node_Boss.prefab` | 보스 노드 |
| `Assets/Proto/Prefabs/Stage/Node_Start.prefab` / `Node_End.prefab` | 시작/끝 |
| `Assets/Proto/Prefabs/Stage/Path_Road.prefab` | 엣지 비주얼 (연결 도로) |

초기 비주얼은 placeholder — ProbeBuilder 없이 순수 primitive 로 구성. VFX/텍스처는 후속.

## 6. Skill Hook

`/proto-stage-graph` 호출 시:

1. `Assets/Proto/Runtime/Stage/` 생성.
2. 스크립트 생성: `Direction.cs`, `IStageNode.cs`, `StageEdge.cs`,
   `StageNodeKind.cs`, `IStageGraph.cs`, `StageGraph.cs`, `StageNode.cs`.
3. 각 노드 종류별 placeholder 프리팹 7개 생성 (Start/End/Combat/Shop/Event/Boss).
4. `StageRoot.prefab` 생성 + `ProtoMain.unity` 에 배치.
5. **예제 그래프 1개 하드코드** (PCG 없이도 작동 확인용):
   - 5개 노드: Start → Combat → (Shop / Event) → Combat → Boss → End
   - 예시 엣지 배치로 HR-3 (3-way max, no-backtrack) 검증 가능
6. 콘솔 에러 확인.

옵션 인자:
- `--sample <linear|branching|full>` — 예제 그래프 복잡도.

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="StageGraph", search_method="by_component")` → 1.
3. `execute_code` 스모크:
   ```csharp
   var graph = UnityEngine.Object.FindObjectOfType<Proto.Stage.StageGraph>();
   UnityEngine.Debug.Assert(graph.Start != null);
   UnityEngine.Debug.Assert(graph.CurrentNode == graph.Start);

   // HR-3: 첫 노드는 entryDirection 없어서 모든 엣지 사용 가능
   var firstExits = graph.GetAvailableExits();
   UnityEngine.Debug.Assert(firstExits.Count <= 3, "HR-3 violation: more than 3 exits");

   // 첫 엣지로 이동
   if (firstExits.Count > 0) graph.TraverseEdge(firstExits[0]);

   // 이동 후 entryDirection 은 첫 엣지의 반대
   var nextExits = graph.GetAvailableExits();
   foreach (var e in nextExits)
       UnityEngine.Debug.Assert(e.Direction != graph.LastEntryDirection,
           "HR-3 violation: backtrack exit exposed");
   ```
4. 시각 검증: QuarterView 카메라로 스크린샷 → 노드/엣지 배치 확인.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-HARD-RULE-03 (HR-3 3-way + no-backtrack 검증)
