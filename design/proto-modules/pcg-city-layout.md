# Module: PCGCityLayout

> 참고: [`_catalog-turn3d.md`](./_catalog-turn3d.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

런 시작 시 시드로부터 **StageGraph를 절차적으로 생성**한다. 시가지 느낌의
노드 배치(삼거리/사거리), 노드 종류 분포(Combat 다수 + Shop/Event/Boss 소수),
거리/길이 제약을 만족시킨다. 로그라이크 재배치 제공.

StageGraph 는 데이터 컨테이너, PCGCityLayout 은 생성기.

## 2. Hard Rules

- **HR-3 준수**: 노드당 최대 3방향 보장. 생성 중 초과 발생 시 edge drop.
- 동일 시드 → 동일 결과. `Random.seed` 직접 건드리지 않고 `System.Random(seed)` 사용.
- 생성 완료 전까지 StageGraph 는 `CurrentNode = null` (접근 시 예외 대신 명시적 에러).
- **HR-8 준수**: 노드 수/분포/시드 범위 등 수치는 `LayoutProfile` SO.
- **HR-10 준수**: 생성된 프리팹/씬 요소는 `Assets/Proto/` 밖으로 나가지 않음.

## 3. Public API

```csharp
namespace Proto.Stage.PCG
{
    public interface ICityLayoutGenerator
    {
        /// <summary>
        /// 시드와 프로필로 StageGraph 를 채운다. 이미 채워진 경우 클리어 후 재생성.
        /// </summary>
        void Generate(IStageGraph graph, int seed, LayoutProfile profile);
    }

    [CreateAssetMenu(menuName = "Proto/Stage/LayoutProfile")]
    public class LayoutProfile : ScriptableObject
    {
        [Header("Size")]
        public int minNodes = 8;
        public int maxNodes = 14;

        [Header("Distribution (weights, Combat implicit rest)")]
        [Range(0, 1)] public float shopWeight  = 0.15f;
        [Range(0, 1)] public float eventWeight = 0.15f;
        [Range(0, 1)] public float bossWeight  = 0.08f;   // 0이면 Boss 없음

        [Header("Geometry")]
        public float gridSpacing = 12f;           // 노드 간 기본 거리
        public float jitterRadius = 2f;           // 노드 좌표 jitter
        public int maxBranchesPerNode = 3;        // HR-3 이하
        [Range(0, 1)] public float branchProbability = 0.45f;

        [Header("Constraints")]
        public bool forceBossAtEnd = true;
        public int minPathLengthToEnd = 5;
    }
}
```

구현 클래스 `GridCityLayoutGenerator`:

- 알고리즘: 4방 그리드 기반 depth-first walk + branch 분기.
- 최소/최대 노드 수 만족까지 반복.
- End 노드는 가장 먼 말단에 배치.
- `forceBossAtEnd` 가 true 면 End 직전 노드를 Boss 로 치환.

## 4. Dependencies

- **Required**: `StageGraph`
- **Optional**:
  - `SaveKit` — 시드 저장/복원
  - `HUDKit` — 미니맵 표시 (노드/엣지 렌더)

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Data/Config/Layout_Default.asset` | 기본 `LayoutProfile` (8~14 nodes) |
| `Assets/Proto/Data/Config/Layout_Small.asset` | 5~7 nodes (빠른 테스트용) |
| `Assets/Proto/Data/Config/Layout_Large.asset` | 14~20 nodes |

Runtime 스크립트:
- `ICityLayoutGenerator.cs`
- `LayoutProfile.cs`
- `GridCityLayoutGenerator.cs`
- `PCGStageRoot.cs` — StageGraph 와 Generator 를 잇는 Bootstrap MB

`PCGStageRoot` MB 는 `StageRoot.prefab` 에 추가. 에디터에서 `Generate` 버튼으로
에디터 타임 미리보기도 가능하게 (ContextMenu).

## 6. Skill Hook

`/proto-pcg-layout` 호출 시:

1. `Assets/Proto/Runtime/Stage/PCG/` 생성.
2. 스크립트 4개 생성.
3. LayoutProfile 3개 에셋 생성 (Default/Small/Large).
4. `StageRoot.prefab` 에 `PCGStageRoot` MB 추가 (`[RequireComponent(typeof(StageGraph))]`).
5. `PCGStageRoot` 의 Inspector 기본 설정:
   - Profile: `Layout_Default`
   - Seed: `0` (0 = 런 시작 시 랜덤)
   - GenerateOnAwake: `true`
6. 시드 3개로 각각 Generate 호출해보는 에디터 테스트 스크립트 자동 실행.
7. 콘솔 에러 확인.

옵션 인자:
- `--profile <default|small|large>` — 초기 프로필.
- `--seed <int>` — 고정 시드 (디버그용).

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `execute_code` 스모크:
   ```csharp
   var root = UnityEngine.Object.FindObjectOfType<Proto.Stage.PCG.PCGStageRoot>();
   var graph = root.GetComponent<Proto.Stage.StageGraph>();
   var profile = Resources.Load<Proto.Stage.PCG.LayoutProfile>("Layout_Default");

   // 같은 시드 → 같은 노드 개수 + 같은 Start/End 위치 (determinism)
   var gen = new Proto.Stage.PCG.GridCityLayoutGenerator();
   gen.Generate(graph, seed: 42, profile);
   int count1 = graph.Nodes.Count;
   var startPos1 = graph.Start.WorldPosition;

   gen.Generate(graph, seed: 42, profile);
   int count2 = graph.Nodes.Count;
   var startPos2 = graph.Start.WorldPosition;

   UnityEngine.Debug.Assert(count1 == count2, "PCG not deterministic: node count");
   UnityEngine.Debug.Assert(startPos1 == startPos2, "PCG not deterministic: start pos");

   // HR-3 검증 — 모든 노드 exits <= 3
   foreach (var node in graph.Nodes)
       UnityEngine.Debug.Assert(node.Edges.Count <= 3, $"HR-3 violation: node {node.Id} has {node.Edges.Count} exits");
   ```
3. 시각 검증: QuarterView 카메라 스크린샷 → 노드/엣지 배치 확인.
4. 3개 다른 시드로 Generate 호출, 세 번의 스크린샷 확인 (시드마다 다른 레이아웃).

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-DETERMINISM-01 (같은 시드 동일 결과)
- OR-HARD-RULE-03 (HR-3)
