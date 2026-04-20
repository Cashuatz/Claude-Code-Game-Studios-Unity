# Module: PathPicker

> 참고: [`_catalog.md`](./_catalog.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

StageGraph의 `GetAvailableExits()` 결과를 사용자가 선택하도록 UI·입력으로
변환한다. **카메라 모드별 입력 해석기가 다르다**:

- **QuarterView** → 맵 위 엣지/노드를 터치·클릭 (탑다운 맵)
- **BackView** → 전방 최대 3방향 (좌/정/우) 선택 (방향키 or 화면 hotspot)
- **SideView** → 좌/우 2방향 (사이드뷰 시점에서 의미 있는 축만)

선택이 일어나면 `StageGraph.TraverseEdge(edge)` 호출. 이동 연출은
MovementCore + CameraRig 담당.

## 2. Hard Rules

- **HR-4 준수**: 입력 감지는 PathPicker 내부로 격리. 다른 스크립트가 touch/key 읽어
  TraverseEdge 호출 금지.
- **HR-9 준수**: 엣지 선택 결과는 `PathSelectionResolved` 이벤트로 발행.
- 카메라 모드 전환 시 UI 자동 교체 (`CameraRig.ModeChanged` 구독).
- **런타임 UI 는 uGUI (Canvas)** — `_conventions.md` HR-11. 에디터 확장은 UI Toolkit 허용.
- 선택 가능 엣지 0개면 "막힘" 상태 — 턴 소모 없이 UI 만 알림 (전투 종료 조건 체크는 TurnSystem).

## 3. Public API

```csharp
namespace Proto.Stage.Picker
{
    public enum PickerMode { Disabled, QuarterView, BackView, SideView }

    public interface IPathPicker
    {
        PickerMode Mode { get; }
        IReadOnlyList<StageEdge> CurrentCandidates { get; }

        void Show(IReadOnlyList<StageEdge> candidates);
        void Hide();

        /// <summary>프로그램 방식 선택 (AI 또는 자동 진행용).</summary>
        void SelectEdge(StageEdge edge);

        event Action<StageEdge> EdgeSelected;
        event Action Cancelled;        // ESC / back
    }

    /// <summary>
    /// 카메라 모드별 UI 어댑터. 각 어댑터는 자기 Canvas 하위 패널을
    /// 소유하며 MonoBehaviour 로 구현된다 (uGUI).
    /// </summary>
    public interface ICameraModeAdapter
    {
        PickerMode PickerMode { get; }

        /// <summary>어댑터 활성화 + 후보 엣지로 UI 채우기.</summary>
        void Show(IReadOnlyList<StageEdge> candidates);

        /// <summary>어댑터 비활성화 + 내부 UI 요소 풀로 회수.</summary>
        void Hide();

        event Action<StageEdge> EdgeChosen;
    }
}
```

구현 어댑터 3개 — **모두 `MonoBehaviour` + uGUI**:

- `QuarterViewAdapter`
  - Canvas 위에 노드 위 클릭 가능 `Image`/`Button` 오버레이.
  - 월드 좌표 → 스크린 좌표 변환(`Camera.WorldToScreenPoint`)으로 매 프레임 위치 갱신.
  - 엣지마다 아이콘 1개 pool 에서 꺼냄.
- `BackViewAdapter`
  - `HorizontalLayoutGroup` 으로 좌/정/우 3개 `Button` 배치.
  - 카메라 `forward` 기준 entryDirection 제외 후 좌/정/우 매핑.
- `SideViewAdapter`
  - 좌/우 2개 `Button`. 사이드뷰 축과 맞지 않는 엣지는 비활성.

모든 Button 은 클릭 시 `EdgeChosen?.Invoke(edge)` 호출.

## 4. Dependencies

- **Required**: `StageGraph`, `CameraRig`
- **Optional**:
  - `TurnSystem` — `PickerMode` 가 턴 시작 시 Show, 이동 완료 시 Hide
  - `HUDKit` — UIDocument 공유

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/UI/PathPickerRoot.prefab` | Canvas (Screen Space - Overlay) + `PathPicker` MB |
| `Assets/Proto/Prefabs/UI/PathPicker_Quarter.prefab` | QuarterView 어댑터 패널 (노드 마커 풀) |
| `Assets/Proto/Prefabs/UI/PathPicker_Back.prefab` | BackView 어댑터 패널 (좌/정/우 3버튼 HorizontalLayoutGroup) |
| `Assets/Proto/Prefabs/UI/PathPicker_Side.prefab` | SideView 어댑터 패널 (좌/우 2버튼) |
| `Assets/Proto/Prefabs/UI/NodeMarker.prefab` | QuarterView 용 노드 지시 Image + Button |

Canvas 구조:
```
PathPickerRoot (Canvas, Screen Space - Overlay)
├── EventSystem (씬에 1개 보장)
├── QuarterViewAdapter (MonoBehaviour + GameObject)
│   └── NodeMarker 풀 (Instantiate pool)
├── BackViewAdapter
│   └── HorizontalLayoutGroup
│       ├── LeftButton
│       ├── CenterButton
│       └── RightButton
└── SideViewAdapter
    ├── LeftButton
    └── RightButton
```

카메라 모드 바뀌면 해당 어댑터 GameObject 만 `SetActive(true)`, 나머지 false.

## 6. Skill Hook

`/proto-path-picker` 호출 시:

1. `Assets/Proto/Runtime/Stage/Picker/` 생성.
2. 스크립트 생성: `PickerMode.cs`, `IPathPicker.cs`, `PathPicker.cs`,
   `ICameraModeAdapter.cs`, `QuarterViewAdapter.cs`, `BackViewAdapter.cs`,
   `SideViewAdapter.cs`, `NodeMarker.cs` (QuarterView 마커 스크립트).
3. `Assets/Proto/Prefabs/UI/` 에 Canvas 프리팹 + 어댑터 프리팹 3개 + NodeMarker 프리팹 생성.
   `ProtoMain.unity` 씬에 EventSystem 존재 보장 (없으면 자동 생성).
4. `PathPickerRoot.prefab` 을 `ProtoMain.unity` 에 배치.
5. `PathPicker` MB 가 `CameraRig.ModeChanged` 구독 + StageGraph 참조 주입.
6. 콘솔 에러 확인.

옵션 인자:
- `--with-debug-auto` — 에디터 전용 "첫 엣지 자동 선택" 디버그 MB 추가.

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="PathPicker", search_method="by_component")` → 1.
3. Play 진입, 3 모드 각각에서 PathPicker UI 가 교체되는지 확인 (스크린샷 3장).
4. `execute_code` 스모크:
   ```csharp
   var picker = UnityEngine.Object.FindObjectOfType<Proto.Stage.Picker.PathPicker>();
   var graph = UnityEngine.Object.FindObjectOfType<Proto.Stage.StageGraph>();

   bool selected = false;
   picker.EdgeSelected += e => selected = true;

   var exits = graph.GetAvailableExits();
   if (exits.Count > 0)
   {
       picker.SelectEdge(exits[0]);
       UnityEngine.Debug.Assert(selected, "EdgeSelected event not raised");
       UnityEngine.Debug.Assert(graph.CurrentNode.Id != graph.Start.Id,
           "TraverseEdge did not advance current node");
   }
   ```
5. HR-3 검증 — BackView 에서 표시되는 hotspot 수는 `exits.Count` 와 일치, 최대 3.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-INTEGRATION-01 (CameraMode ↔ PickerMode 교체 동작)
- OR-HARD-RULE-03
