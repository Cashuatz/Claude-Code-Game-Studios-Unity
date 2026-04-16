---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# FAILURE_TAXONOMY.project.md

> **Document Role: Project Reference** -- `.vkl/core/FAILURE_TAXONOMY.base.md`를 **overlay(확장)**한다.
> core의 12가지 실패 유형은 그대로 유효하며, 이 문서는 Unity 게임 개발 프로젝트에 특화된 실패 유형을 추가 정의한다.
> **Overlay Of:** `.vkl/core/FAILURE_TAXONOMY.base.md`
> **Merge Rule:** core taxonomy를 먼저 로드한 뒤 이 project taxonomy를 추가 적용한다. 이 문서는 core FT-XX를 대체하지 않는다.
>
> **ID 규칙:** 프로젝트 실패 유형은 `FT-P-XXX` 형식을 사용한다. core FT-XX와 중복되지 않는다.
>
> **분류 규칙:** core의 Classification Rules(Root Cause Rule, Upstream First Rule, Specificity Rule, Multi-Tag Rule)가 동일하게 적용된다. FT-P-XXX는 FT-XX와 함께 multi-tag될 수 있다.

---

## Project-Specific Failure Types

---

### FT-P-001: 씬 전환 시 상태 소실 (Scene Transition State Loss)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | FT-05 (primary overlay) |
| **Definition** | 씬 전환(SceneManager.LoadScene) 시 게임 상태, 매니저 오브젝트, 런타임 데이터가 의도치 않게 파괴되거나, 반대로 DontDestroyOnLoad로 인해 싱글톤이 중복 생성되는 상태. 씬 전환 전후의 상태 연속성이 보장되지 않는다. |
| **Symptoms** | - 씬 전환 후 게임 매니저가 null이 되어 NullReferenceException 발생. <br>- DontDestroyOnLoad 오브젝트가 씬 재진입 시 중복 생성된다. <br>- Additive 씬 로딩/언로딩 시 의존하는 오브젝트가 함께 파괴된다. <br>- 씬 전환 후 이벤트 구독(delegate, Action)이 해제되어 콜백이 동작하지 않는다. <br>- 플레이어 진행 상황(인벤토리, 퀘스트 상태 등)이 씬 전환 시 초기화된다. |
| **Representative Causes** | 1. DontDestroyOnLoad 호출 누락으로 매니저 오브젝트가 씬과 함께 파괴된다. <br>2. 싱글톤 패턴에서 중복 방지 로직(Instance 존재 시 Destroy) 미구현. <br>3. Awake에서 이전 씬의 오브젝트를 참조하는데, 해당 오브젝트가 이미 파괴됨. <br>4. 씬 전환 시 Coroutine이 중단되지만, 그 결과에 의존하는 후속 로직이 존재. <br>5. 이벤트 구독을 OnDisable/OnDestroy에서 해제하지 않아 파괴된 오브젝트에 콜백 호출. |
| **Priority Check Points** | 1. 씬 전환 시 유지되어야 하는 오브젝트 목록과 DontDestroyOnLoad 호출 여부를 대조한다. <br>2. 싱글톤 패턴의 중복 방지 로직(Awake에서 Instance 체크)을 확인한다. <br>3. 씬 전환 전후의 오브젝트 생존 여부를 런타임에서 로그로 확인한다. <br>4. 이벤트 구독/해제 쌍이 OnEnable/OnDisable 또는 OnDestroy에서 완전히 매칭되는지 확인한다. |
| **Return Path** | 1. **OR-P-001 (Play Mode 실행 결과)** 로 씬 전환 전후 상태를 확인한다. <br>2. **OR-08 (Human Oracle)** 로 싱글톤/매니저 생명주기 패턴을 확정한다 (HC-001). <br>3. DontDestroyOnLoad 오브젝트 목록을 문서화한다. <br>4. 씬 전환 시 자동 정리/복원되는 상태 관리 시스템을 도입한다. <br>5. Knowledge Base에 씬 전환 체크리스트를 등록한다. |
| **Connected Oracle(s)** | Primary: **OR-P-001**, **OR-08** / Secondary: **OR-03**, **OR-04** |
| **Connected Core FT** | FT-05 (Execution Failure), FT-02 (Hidden Semantic Rule Misfill) |

---

### FT-P-002: Addressables/AssetBundle 로드 실패 (Asset Loading Failure)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | FT-05 (primary overlay) |
| **Definition** | Addressables 또는 AssetBundle을 통한 에셋 로드가 실패하거나, 로드된 에셋의 참조가 유효하지 않거나, 로드/릴리스 생명주기 관리 오류로 메모리 릭이 발생하는 상태. |
| **Symptoms** | - Addressables.LoadAssetAsync에서 InvalidKeyException 발생. <br>- 에셋 로드 완료 콜백에서 null 에셋이 반환된다. <br>- 씬 전환 후에도 이전 씬의 에셋이 메모리에 남아 있다 (Release 누락). <br>- 카탈로그 갱신 후 기존 키로 에셋을 찾지 못한다. <br>- 빌드 환경에서만 에셋 로드가 실패한다 (에디터에서는 AssetDatabase로 폴백). <br>- 다수의 에셋을 동시 로드 시 메모리 부족(OOM) 발생. |
| **Representative Causes** | 1. Addressables 키(address)가 실제 에셋 경로/이름과 불일치한다. <br>2. Addressables 카탈로그가 빌드 후 갱신되지 않았다. <br>3. AsyncOperationHandle의 Release가 누락되어 메모리 릭이 누적된다. <br>4. 씬 전환 시 진행 중인 비동기 로딩의 콜백이 파괴된 오브젝트를 참조한다. <br>5. 에디터에서는 Use Asset Database 모드로 동작하여 키 불일치를 감지하지 못한다. |
| **Priority Check Points** | 1. Addressables 키가 에셋에 실제로 할당된 address와 일치하는지 확인한다. <br>2. 카탈로그 빌드 날짜/버전이 최신 에셋 변경과 일치하는지 확인한다. <br>3. 모든 LoadAssetAsync에 대응하는 Release 호출이 있는지 코드 검색한다. <br>4. 씬 전환 시 진행 중인 AsyncOperationHandle 목록을 관리하는 로직이 있는지 확인한다. <br>5. Addressables Profiler에서 에셋 참조 카운트를 확인한다. |
| **Return Path** | 1. **OR-P-001 (Play Mode 실행 결과)** 로 에셋 로드 성공/실패를 확인한다. <br>2. **OR-P-002 (Profiler 측정값)** 로 메모리 릭 여부를 판정한다. <br>3. **OR-08 (Human Oracle)** 로 에셋 로딩 실패 시 폴백 전략을 확정한다 (HC-003). <br>4. Addressables 키 레지스트리를 생성하여 키-에셋 매핑을 문서화한다. <br>5. 에셋 로드/릴리스 래퍼를 도입하여 참조 카운팅을 자동화한다. |
| **Connected Oracle(s)** | Primary: **OR-P-001**, **OR-P-002** / Secondary: **OR-04**, **OR-08** |
| **Connected Core FT** | FT-05 (Execution Failure), FT-11 (Environment/Version/Cache Issue) |

---

### FT-P-003: 물리 비결정성 (Physics Non-Determinism)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | FT-10 (primary overlay) |
| **Definition** | 물리 시뮬레이션이 동일 입력에서 일관되지 않은 결과를 생성하거나, FixedUpdate와 Update의 혼용으로 인해 물리 동작이 프레임레이트에 의존하는 상태. 충돌 레이어 설정 누락으로 인한 충돌 미감지도 포함한다. |
| **Symptoms** | - 동일 조건에서 반복 플레이 시 물리 시뮬레이션 결과가 달라진다. <br>- Rigidbody 이동/회전을 Update에서 수행하여 프레임레이트에 따라 동작이 달라진다. <br>- Physics Layer Collision Matrix에서 특정 레이어 간 충돌이 비활성화되어 충돌이 감지되지 않는다. <br>- Physics Material의 마찰/반발 계수가 의도와 다르게 적용된다. <br>- Trigger/Collision 이벤트가 간헐적으로 누락된다 (고속 이동 시 터널링). |
| **Representative Causes** | 1. Rigidbody.MovePosition/AddForce를 Update에서 호출한다 (FixedUpdate에서 호출해야 함). <br>2. Transform.position 직접 변경으로 물리 엔진을 우회한다. <br>3. 새 Physics Layer 추가 후 Collision Matrix 갱신을 누락한다. <br>4. Physics Material이 할당되지 않아 기본값이 적용된다. <br>5. Continuous Collision Detection이 필요한 고속 오브젝트에 Discrete 모드가 설정되어 있다. |
| **Priority Check Points** | 1. Rigidbody 관련 코드가 FixedUpdate 내에서만 실행되는지 확인한다. <br>2. Transform.position 직접 변경이 물리 오브젝트에 사용되지 않는지 확인한다. <br>3. Physics Layer Collision Matrix 설정이 기획 의도와 일치하는지 확인한다. <br>4. 모든 물리 오브젝트에 적절한 Physics Material이 할당되었는지 확인한다. <br>5. 고속 이동 오브젝트의 Collision Detection Mode를 확인한다. |
| **Return Path** | 1. **OR-P-001 (Play Mode 실행 결과)** 로 물리 동작의 일관성을 확인한다. <br>2. **OR-P-002 (Profiler 측정값)** 로 물리 연산 부하를 확인한다. <br>3. **OR-06 (Metamorphic Oracle)** + MR-P-001로 물리 단조성을 검증한다. <br>4. **OR-08 (Human Oracle)** 로 결정론적 물리의 필요 여부를 확인한다 (HC-004). <br>5. 물리 관련 코딩 컨벤션(FixedUpdate 사용 규칙)을 문서화한다. |
| **Connected Oracle(s)** | Primary: **OR-P-001**, **OR-06** / Secondary: **OR-P-002**, **OR-08** |
| **Connected Core FT** | FT-10 (Metamorphic Relation Failure), FT-02 (Hidden Semantic Rule Misfill), FT-09 (Edge-Case Fragility) |

---

### FT-P-004: UI 상태-데이터 불일치 (UI State-Data Mismatch)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | FT-06 (primary overlay) |
| **Definition** | UI에 표시되는 정보가 실제 게임 데이터와 일치하지 않거나, UI 이벤트 처리 순서 오류로 인해 사용자 입력이 올바르게 반영되지 않는 상태. 지역화 키 누락으로 인한 텍스트 미표시도 포함한다. |
| **Symptoms** | - 게임 데이터가 변경되었지만 UI가 갱신되지 않는다 (바인딩 끊김). <br>- UI Toolkit의 데이터 바인딩이 해제되어 초기값이 유지된다. <br>- Canvas 이벤트 시스템에서 버튼 클릭이 다른 UI 요소에 가려져 무시된다. <br>- 지역화 키가 누락되어 빈 문자열 또는 키 자체("UI_BUTTON_CONFIRM")가 표시된다. <br>- UI 애니메이션 완료 전 데이터가 변경되어 표시가 중간 상태에서 멈춘다. <br>- 여러 팝업의 표시/숨김 순서가 꼬여 잘못된 UI가 최상위에 표시된다. |
| **Representative Causes** | 1. UI Toolkit BindingPath가 데이터 모델의 프로퍼티 이름과 불일치한다. <br>2. 데이터 변경 알림(INotifyPropertyChanged, Observable 등)이 누락된다. <br>3. Canvas의 Sort Order 또는 Raycast Target 설정이 잘못되어 이벤트가 차단된다. <br>4. Localization Table에 해당 키가 등록되지 않았다. <br>5. UI 갱신 타이밍이 데이터 변경보다 먼저 실행되어 이전 값을 표시한다. |
| **Priority Check Points** | 1. UI 데이터 바인딩이 올바른 프로퍼티에 연결되어 있는지 확인한다. <br>2. 데이터 변경 시 UI 갱신 이벤트가 발행되는지 확인한다. <br>3. Canvas 계층 구조와 Sort Order가 기획 의도와 일치하는지 확인한다. <br>4. 모든 지역화 키가 Localization Table에 등록되어 있는지 확인한다. <br>5. UI 갱신 순서(이벤트 호출 순서)가 데이터 변경 후에 실행되는지 확인한다. |
| **Return Path** | 1. **OR-P-001 (Play Mode 실행 결과)** 로 UI 표시와 실제 데이터를 비교한다. <br>2. **OR-P-003 (GDD 공식 대조)** 로 UI에 표시된 수치가 기획 의도와 일치하는지 확인한다. <br>3. **OR-08 (Human Oracle)** 로 UI/UX 흐름의 자연스러움을 확인한다. <br>4. 지역화 키 누락을 빌드 시 자동 감지하는 검증 스크립트를 도입한다. <br>5. UI 상태 머신을 도입하여 팝업/화면 전환 순서를 관리한다. |
| **Connected Oracle(s)** | Primary: **OR-P-001**, **OR-P-003** / Secondary: **OR-08**, **OR-04** |
| **Connected Core FT** | FT-06 (Observability Failure), FT-02 (Hidden Semantic Rule Misfill) |

---

### FT-P-005: 빌드/플랫폼 차이 (Build/Platform Divergence)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | FT-11 (primary overlay) |
| **Definition** | Unity Editor(Play Mode)에서 정상 동작하는 기능이 실제 빌드(Standalone, Mobile, Console 등)에서 실패하거나, 특정 플랫폼에서만 동작하지 않는 상태. IL2CPP 리플렉션 실패, 플랫폼별 셰이더 미지원, 에디터 전용 API 사용 등이 대표적이다. |
| **Symptoms** | - 에디터 Play Mode에서는 정상이지만 빌드 후 특정 기능이 동작하지 않는다. <br>- IL2CPP 빌드에서 System.Reflection 관련 MissingMethodException 또는 ExecutionEngineException 발생. <br>- 특정 플랫폼(iOS, Android, WebGL 등)에서 셰이더 컴파일 실패 또는 렌더링 이상. <br>- AssetDatabase, EditorApplication 등 에디터 전용 API가 빌드에서 호출되어 컴파일 에러. <br>- #if UNITY_EDITOR 분기 누락으로 빌드에서 의도하지 않은 코드 경로 실행. <br>- 플랫폼별 입력 장치(터치, 게임패드) 차이로 인한 입력 처리 실패. |
| **Representative Causes** | 1. IL2CPP의 코드 스트리핑이 리플렉션으로만 접근하는 타입/메서드를 제거한다. <br>2. link.xml에 보존할 타입이 등록되지 않았다. <br>3. Editor 폴더 밖에서 UnityEditor 네임스페이스를 참조한다. <br>4. 셰이더가 특정 Graphics API(Vulkan, Metal, OpenGL ES)를 지원하지 않는다. <br>5. #if UNITY_EDITOR / #if !UNITY_EDITOR 조건 컴파일이 누락되었다. |
| **Priority Check Points** | 1. 리플렉션을 사용하는 모든 코드 위치를 식별하고 link.xml 등록 여부를 확인한다. <br>2. UnityEditor 네임스페이스 참조가 Editor asmdef 또는 #if UNITY_EDITOR 내부에만 있는지 확인한다. <br>3. 타겟 플랫폼별 셰이더 변형(variant)이 빌드에 포함되어 있는지 확인한다. <br>4. 실제 빌드(Development Build)를 수행하여 에디터와 동일하게 동작하는지 확인한다. <br>5. 플랫폼별 입력 시스템 설정(Input System package)을 확인한다. |
| **Return Path** | 1. **OR-P-004 (dotnet build / Unity 컴파일러)** 로 빌드 성공 여부를 확인한다. <br>2. **OR-P-001 (Play Mode 실행 결과)** 와 실제 빌드 실행 결과를 비교한다. <br>3. **OR-P-005 (Unity Test Framework 결과)** 로 EditMode/PlayMode 테스트 통과 여부를 확인한다. <br>4. **OR-08 (Human Oracle)** 로 타겟 플랫폼 목록을 확정한다 (HC-007). <br>5. IL2CPP 빌드 테스트를 CI 파이프라인에 추가한다. |
| **Connected Oracle(s)** | Primary: **OR-P-004**, **OR-P-005** / Secondary: **OR-P-001**, **OR-08** |
| **Connected Core FT** | FT-11 (Environment/Version/Cache Issue), FT-05 (Execution Failure) |

---

## Classification Guide: Core FT + Project FT-P 선택 기준

### 분류 우선순위

프로젝트 실패를 분류할 때 아래 순서로 확인한다:

```
1. Core FT 해당 여부 먼저 확인 (FT-01 ~ FT-12)
2. Project FT-P가 core보다 더 구체적으로 설명하는지 확인
3. 더 구체적이면 FT-P-XXX를 primary로, core FT-XX를 connected로 태깅
4. 동일 수준이면 core FT-XX를 primary로, FT-P-XXX를 secondary로 태깅
```

### 빠른 조회표

| 증상 | 1st Check | 2nd Check |
|------|-----------|-----------|
| 씬 전환 후 오브젝트/상태 소실 | FT-P-001 | FT-05, FT-02 |
| 에셋 로드 실패/메모리 릭 | FT-P-002 | FT-05, FT-11 |
| 물리 동작 불일치/비결정적 | FT-P-003 | FT-10, FT-02 |
| UI 표시와 데이터 불일치 | FT-P-004 | FT-06, FT-02 |
| 에디터에서만 작동, 빌드에서 실패 | FT-P-005 | FT-11, FT-05 |
| 라이프사이클 순서 문제 | FT-P-001 | FT-02 |
| 실행 자체 실패 (컴파일 에러) | FT-05 | FT-P-005 |
| 테스트 통과인데 결과 틀림 | FT-08 | FT-P-001, FT-P-003 |
