# Proto Modules — Conventions & Hard Rules

> 모든 Proto 모듈은 이 문서의 규칙을 위반하면 **안 된다**.
> 위반은 스킬 실행 시 검증에서 FAIL 처리되고 즉시 롤백한다.

## Hard Rules (위반 금지)

### HR-1. Rigidbody 전면 금지
- `Rigidbody` / `Rigidbody2D` 컴포넌트는 어떤 프리팹/씬에도 **추가하지 않는다**.
- 물리 질의는 허용: `Physics.OverlapSphere`, `Physics.CheckCapsule`,
  `Physics.Raycast`, `Physics.ComputePenetration` 등 정적 API만 사용.
- Collider는 **트리거 또는 질의용**으로만 존재. 물리 시뮬레이션에 의존하지 않는다.
- 이동/충돌 해소는 전부 `Transform` 조작 + 수동 push-out 계산으로 구현한다.
- **검증**: 씬/프리팹 저장 시 `Rigidbody` 검출하는 에디터 validator 추가 예정
  (별도 스킬 `/proto-lint`).

### HR-2. 단일 씬
- 게임 시작부터 종료까지 **한 개의 Unity Scene**에서만 동작.
- 스테이지 전환 = 런타임 GameObject 생성/파괴. `SceneManager.LoadScene` 금지
  (프로토 범위 내).
- 예외: **에디터 전용 테스트 씬**은 별도 허용 (Play 모드 진입 안 함).

### HR-3. PCG 레이아웃
- 스테이지 그래프는 **런 시작 시 시드로 생성**. 정적 배치 금지.
- 노드당 **최대 3방향**. 진입한 방향으로는 되돌아가지 않는다 (no-backtrack).
- 시드는 `SaveKit`으로 런 단위 저장.

### HR-4. 카메라 전환 트리거화
- CameraRig 3모드 전환은 **이벤트 기반**. 임의 스크립트가 직접 모드 바꾸지 않는다.
- 전환 API: `CameraRig.RequestMode(CameraMode mode, TransitionProfile profile)`.

### HR-5. 유사턴제 불변식
- TurnSystem 은 **유닛 단위 time-scale** 조작으로 타임슬로우 구현.
- `Time.timeScale` 직접 조작 금지. `Unit.LocalTimeScale` 사용.
- 타임오버 판정은 **서버 타임스탬프 기반** (Unix ms), 프레임 카운트 금지.

### HR-6. 액션 입력은 CombatScheme 을 통해서만
- 유닛의 스킬·공격 발동은 `CombatScheme.ResolveAction(...)` 만 통한다.
- 스킬 버튼 / 카드 / 리듬 / QTE 구현은 `CombatScheme` 서브클래스로.
- 유닛 스크립트가 키보드/터치 직접 읽는 것 금지.

### HR-7. 캐릭터 렌더 폴리모프
- 유닛 프리팹은 **CharacterRenderKit** 추상화를 통해서만 시각화.
- `BillboardRenderer` / `SkinnedRenderer` 구현체만 존재. 둘 다 같은 API 준수.
- 연출 코드는 렌더 타입에 분기하지 않는다 (Liskov 치환 가능).

### HR-8. 데이터-코드 분리
- 게임플레이 수치는 **전부 ScriptableObject** 또는 외부 config(`.json`/`.asset`).
- 인스펙터 하드코드 금지 (prefab 내 inline 수치 metadata 제외).
- 공식은 `AbilityDefinition.Formula` 필드에서 읽는다.

### HR-9. 이벤트 버스
- 모듈 간 결합은 **C# event** 또는 `GameEventChannel` (ScriptableObject-as-event).
- 서로 참조하지 않는다. 모듈 추가/제거로 시스템이 깨지면 안 됨.
- 예외: 동일 Tier 내 직접 참조 허용 (예: MovementCore ↔ StageGraph).

### HR-10. 에디터-런타임 경계
- 스킬이 심는 프리팹/SO/스크립트는 **Assets/Proto/** 하위에만 생성.
- 업스트림 MCP 패키지(`Packages/com.coplaydev.unity-mcp/`) 수정 금지
  (embed된 상태지만 프로토 스킬이 건드리면 안 됨).

### HR-11. UI 런타임은 uGUI
- **런타임 UI** (HUD, 스킬카드, PathPicker, 메뉴 등): **uGUI (Canvas)** 만 사용.
  - `Canvas`, `RectTransform`, `Image`, `Text`/`TMP_Text`, `Button`, `Slider`,
    `HorizontalLayoutGroup` 등.
  - `TextMeshPro` 권장 (Image/Button 과 함께).
- **에디터 확장** (EditorWindow, Custom Inspector 등): **UI Toolkit (UXML/USS)** 허용.
  이미 `.claude/rules/editor-ui-code.md` 에 규칙 명시됨.
- `UIDocument` 는 에디터 전용. 런타임 씬에 배치 금지.
- `Canvas` 는 일반적으로 **Screen Space - Overlay** 기본. 월드 공간 UI 는 모듈별로 명시.
- 모든 프로토 씬은 `EventSystem` GameObject 1개 보장 (스킬이 없으면 생성).

### HR-12. Editor 메뉴 루트는 `Proto/` 로 고정
- 모든 `[MenuItem(...)]` 경로는 **`Proto/` 루트 하위**에 둔다.
  - 금지: `Tools/`, `Assets/`, `GameObject/`, `Window/`, `Help/` 등 Unity 기본 루트 사용.
  - 금지: `Tools/Proto/...`, `Proto Tools/...`, `MyGame/...` 같은 변종 루트.
- **예외**: `CONTEXT/<Component>/...` 컴포넌트 컨텍스트 메뉴는 허용 (Unity 규약).
- **표준 카테고리** (추가 시 이 목록에 선등록):
  - `Proto/Bootstrap/...` — 씬/프로젝트 초기화 (priority 1–9)
  - `Proto/Stage/...` — 스테이지·레벨 생성기 (priority 20–29)
  - `Proto/Environment/...` — 환경 에셋·VFX 베이커 (priority 40–49)
  - `Proto/Test Window` 등 단독 도구 (priority 100+)
- **priority 규칙**: 카테고리 간격 ≥ 11 (Unity 는 간격 11 이상일 때 구분선 생성).
- **언더스코어 금지**: `Phase0_BuildLauncher` 같은 플랫 네이밍 대신
  `Proto/Bootstrap/Build Launcher` 식 슬래시 계층 + 공백 구분.
- **검증**: 새 에디터 스크립트 작성 시 `Grep "MenuItem\\(\"(?!Proto/|CONTEXT/)"` 로 위반 검출.

## 파일·폴더 규칙

### Asset 경로
```
Assets/Proto/
├── Runtime/                ← MonoBehaviour, ScriptableObject 스크립트
│   ├── Camera/
│   ├── Movement/
│   ├── Stage/
│   ├── Turn/
│   ├── Combat/
│   ├── Render/
│   ├── Timeline/
│   ├── Ability/
│   ├── Damage/
│   ├── Effect/
│   ├── Enemy/
│   ├── Sound/
│   ├── HUD/
│   └── Save/
├── Prefabs/                ← 프리팹 (카테고리별 하위 폴더)
├── Data/                   ← ScriptableObject 에셋
│   ├── Abilities/
│   ├── Units/
│   └── Config/
├── Scenes/
│   └── ProtoMain.unity     ← 유일한 플레이 씬
├── Timelines/
└── VFX/
```

### 네이밍
- **클래스**: `PascalCase`, 모듈 prefix. 예: `CameraRig`, `CameraMode`, `TurnQueue`.
- **네임스페이스**: `Proto.<Module>`. 예: `Proto.Camera`, `Proto.Turn`.
- **ScriptableObject 파일**: `<Kind>_<Name>.asset`. 예: `Ability_Fireball.asset`.
- **프리팹**: `<Kind>_<Name>.prefab`. 예: `Unit_Ally.prefab`, `Stage_Node.prefab`.
- **씬**: 단일 씬 `ProtoMain.unity` 고정.

### 어셈블리 정의 (asmdef)
- 루트: `Assets/Proto/Runtime/Proto.asmdef`
- Editor 전용: `Assets/Proto/Editor/Proto.Editor.asmdef`
- 모듈별 asmdef 분리는 **Phase D 종료 후 재평가** (초기엔 단일 asmdef로 빠르게).

## 검증 공통 (Verification)

모든 스킬은 설치 후 아래를 순서대로 확인:

1. **컴파일**: `mcp__unityMCP__read_console(types=["error"])` → 에러 0.
2. **씬 로드**: `Assets/Proto/Scenes/ProtoMain.unity` 열려 있음.
3. **프리팹 존재**: 해당 모듈의 "Default Prefabs/Assets"가 `Assets/Proto/Prefabs/` 또는 `Data/`에 있음.
4. **런타임**: 필요 시 `manage_editor(action="play")` → 5초 후 에러 0 → `stop`.
5. **시각 검증**: `manage_camera(action="screenshot", include_image=True, max_resolution=512)` 결과를 사용자에게 제시.

실패 시:
- **롤백**: 방금 생성한 에셋 전부 제거.
- **사유 보고**: 어느 단계에서 무슨 에러가 났는지 사용자에게 1문장 요약.
- **재시도 금지**: 동일 스킬 재호출로 자동 수정 시도 안 함. 사용자 판단 기다림.

## 메타 규칙

- 모든 스킬은 **idempotent** — 두 번 호출해도 결과 동일 (기존 에셋 덮어쓰기 OK).
- 모든 스킬은 **reversible** — `/proto-undo-<name>` 자동 생성 권장 (Phase D).
- 모든 모듈은 **VKL oracle ID 부착** — `.vkl/core/ORACLE_CATALOG.base.md` 참조.
  FT-XX 분류 없는 PASS 판정 금지.
