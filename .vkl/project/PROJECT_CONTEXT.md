---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# PROJECT_CONTEXT.md

Unity 기반 게임 개발 프로젝트의 도메인 컨텍스트, 용어 사전, 숨은 의미 규칙 위험 목록, 주요 리스크, 인간 확인 필수 항목을 정의한다.

---

## Section 1: 프로젝트 목적

Unity 엔진 환경에서 동작하는 게임을 개발한다. 주요 기능 범위는 다음과 같다:

1. **게임플레이 시스템**: MonoBehaviour/DOTS 기반 게임 로직, 전투 시스템, 캐릭터 제어, AI 행동 트리 등 핵심 게임 메커니즘을 구현한다.
2. **에셋 관리**: Addressables/AssetBundle을 활용한 비동기 에셋 로딩, 씬 전환, 메모리 관리 파이프라인을 구축한다.
3. **데이터 설계**: ScriptableObject 기반 게임 데이터 아키텍처, 런타임 인스턴스 관리, 저장/로드 시스템을 설계한다.
4. **물리/충돌**: Unity Physics 또는 Havok Physics를 활용한 물리 시뮬레이션, 충돌 감지, 레이캐스트 기반 상호작용을 구현한다.
5. **UI/UX**: UI Toolkit 또는 uGUI 기반 사용자 인터페이스, 데이터 바인딩, 지역화(Localization) 시스템을 구축한다.

---

## Section 2: 도메인 용어 사전

아래 용어는 **이 프로젝트에서의 정의**이다. 일반적인 의미와 다를 수 있으므로 반드시 이 사전의 정의를 따른다.

| # | Term | 프로젝트 정의 | 주의 사항 |
|---|------|-------------|----------|
| 1 | **MonoBehaviour** | Unity 컴포넌트의 기본 클래스. Awake, Start, Update 등 라이프사이클 메서드를 제공하며, GameObject에 부착하여 동작한다. | 라이프사이클 순서(Awake→OnEnable→Start→Update)를 정확히 이해해야 한다. 동일 프레임 내 실행 순서는 Script Execution Order로 제어한다. |
| 2 | **ScriptableObject** | 에디터와 런타임에서 공유 가능한 데이터 컨테이너. 인스턴스 간 데이터를 공유하며 에셋으로 저장된다. | **핵심 규칙: 런타임에서 ScriptableObject를 직접 수정하면 에디터 에셋 자체가 변경된다.** 런타임 수정이 필요하면 반드시 인스턴스를 복제(Instantiate)하여 사용한다. |
| 3 | **Addressables** | Unity의 비동기 에셋 로딩 시스템. 에셋을 키(key) 또는 레이블(label)로 참조하고, 런타임에 비동기로 로드/언로드한다. | 카탈로그 갱신, 키 불일치, 메모리 릭(Release 누락)에 주의. 씬 전환 시 로딩 중인 AsyncOperationHandle의 생명주기를 관리해야 한다. |
| 4 | **DOTS (Data-Oriented Technology Stack)** | Unity의 데이터 지향 프로그래밍 스택. ECS(Entity Component System), Burst Compiler, Job System으로 구성된다. | MonoBehaviour와 혼용 시 데이터 흐름 방향을 명확히 해야 한다. Managed 타입을 IComponentData에 사용할 수 없다. |
| 5 | **asmdef (Assembly Definition)** | Unity의 C# 어셈블리 분리 단위. 코드 의존성을 제어하고 컴파일 시간을 최적화한다. | 순환 참조 금지. 테스트 코드는 별도 asmdef로 분리한다. Runtime/Editor asmdef 구분 필수. |
| 6 | **SerializeField** | private 필드를 Unity Inspector에 노출하는 어트리뷰트. 직렬화 가능한 타입에만 적용 가능하다. | public 필드는 자동 직렬화되므로 SerializeField가 불필요하다. 직렬화 불가 타입(Dictionary, Interface 등)에 적용하면 무시된다. |
| 7 | **Prefab** | 재사용 가능한 GameObject 템플릿. 씬에 인스턴스화하여 사용하며, Prefab 수정 시 모든 인스턴스에 반영된다. | Prefab Variant와 Nested Prefab의 오버라이드 규칙을 이해해야 한다. 런타임 Instantiate 후에는 Prefab 연결이 끊어진다. |
| 8 | **씬(Scene)** | Unity의 콘텐츠 단위. 게임 오브젝트, 라이팅, 네비게이션 등을 포함한다. | Additive 로딩과 Single 로딩의 차이를 이해해야 한다. DontDestroyOnLoad 오브젝트는 씬 전환 시 유지된다. |
| 9 | **Physics Layer** | Unity 물리 엔진의 충돌 필터링 단위. Layer Collision Matrix에서 레이어 간 충돌 여부를 설정한다. | **핵심 규칙: 새 레이어 추가 시 Collision Matrix를 반드시 갱신한다.** 기본값은 모든 레이어 간 충돌 활성화이나, 프로젝트에서는 명시적 설정을 사용한다. |
| 10 | **FixedUpdate** | 물리 시뮬레이션용 고정 시간 간격 업데이트. Time.fixedDeltaTime 간격으로 호출된다. | **핵심 규칙: 물리 관련 코드(Rigidbody 조작)는 반드시 FixedUpdate에서 실행한다.** Update에서 물리를 조작하면 비결정적 동작이 발생한다. |
| 11 | **Coroutine** | MonoBehaviour에서 제공하는 비동기 실행 패턴. yield return으로 실행을 중단/재개한다. | MonoBehaviour가 비활성화되면 Coroutine이 중단된다. 씬 전환 시 자동 정리되지 않을 수 있다. async/await과 혼용 시 생명주기 관리에 주의. |
| 12 | **AssetReference** | Addressables에서 특정 에셋을 참조하는 직렬화 가능한 참조 타입. Inspector에서 에셋을 할당한다. | 로드 후 반드시 Release를 호출해야 메모리 릭이 방지된다. |
| 13 | **GDD (Game Design Document)** | 게임 기획 문서. 게임플레이 공식, 밸런스 수치, 스테이지 구성 등 기획 의도를 정의한다. | 코드 구현 시 GDD의 공식/수치를 정확히 반영해야 한다. GDD 변경 시 코드 갱신이 필수. |
| 14 | **ADR (Architecture Decision Record)** | 아키텍처 결정 기록. 기술적 선택의 배경, 대안, 결정 사유를 문서화한다. | 구현 시 관련 ADR을 참조하여 결정 사유를 이해한 뒤 작업한다. ADR과 상충되는 구현은 금지. |
| 15 | **IL2CPP** | Unity의 AOT(Ahead-Of-Time) 컴파일러. C# 코드를 C++로 변환 후 네이티브 코드로 컴파일한다. | 리플렉션 사용 시 코드 스트리핑으로 인한 런타임 오류 가능. link.xml로 보존할 타입을 명시해야 한다. |
| 16 | **URP/HDRP** | Unity의 렌더 파이프라인. URP(Universal Render Pipeline)는 범용, HDRP(High Definition Render Pipeline)는 고품질 그래픽용이다. | 셰이더 호환성이 파이프라인에 종속된다. Built-in 셰이더는 SRP에서 작동하지 않는다. |
| 17 | **UniTask** | Unity 환경에 최적화된 async/await 라이브러리. 0 GC allocation을 지원하며 PlayerLoop 기반으로 동작한다. | Coroutine 대체로 사용 가능하나, CancellationToken 관리가 필수. 오브젝트 파괴 시 자동 취소를 위해 destroyCancellationToken을 활용한다. |

---

## Section 3: 자주 발생하는 숨은 의미 규칙 목록 (Known Misfill Risks)

아래는 이 프로젝트에서 확인되었거나 높은 가능성으로 예상되는 hidden semantic rule misfill 위험이다. Claude는 이 목록의 항목과 관련된 구현을 만나면 **즉시 FT-P-001 또는 FT-02로 분류하고 인간에게 확인을 요청**해야 한다.

### HSR-001: MonoBehaviour 라이프사이클 순서 의존성

| 항목 | 내용 |
|------|------|
| **위험** | Awake/Start/OnEnable 호출 순서에 대한 암묵적 가정이 코드 전반에 분산되어 있다. |
| **의미 1** | Awake는 모든 오브젝트의 초기화 — 자기 자신의 참조 설정에 사용 |
| **의미 2** | Start는 다른 오브젝트의 Awake가 완료된 후 — 외부 참조 획득에 사용 |
| **의미 3** | OnEnable은 Awake 직후와 SetActive(true) 시 모두 호출 — 구독 등록에 사용하나, 첫 Awake 시점에서는 다른 오브젝트가 아직 초기화되지 않았을 수 있음 |
| **실제 규칙** | 동일 프레임 내 서로 다른 MonoBehaviour 간의 Awake 호출 순서는 Script Execution Order 또는 씬 내 배치 순서에 의존하며, 이는 암묵적이고 취약하다. |
| **misfill 패턴** | "Awake에서 다른 싱글톤의 Instance에 접근" → 접근 대상이 아직 Awake되지 않아 NullReferenceException 발생. |
| **연결 FT** | FT-01, FT-02, FT-P-001 |

### HSR-002: ScriptableObject와 런타임 인스턴스 혼동

| 항목 | 내용 |
|------|------|
| **위험** | ScriptableObject를 런타임에서 직접 수정하면 에디터 에셋 자체가 변이되어, Play Mode 종료 후에도 변경이 유지된다. |
| **핵심 차이** | ScriptableObject 에셋 = 공유 원본 데이터, Instantiate(SO) = 런타임 전용 복제본. 두 개념이 코드에서 명시적으로 구분되지 않으면 데이터 오염이 발생한다. |
| **misfill 패턴** | "캐릭터 스탯 SO를 런타임에서 직접 수정" → 에디터 에셋이 변경됨 → 다음 Play에서 초기값이 달라짐. "같은 SO를 참조하는 여러 적 캐릭터 중 하나만 수정했는데 전부 변경됨" (공유 참조 문제). |
| **연결 FT** | FT-02, FT-P-002 |

### HSR-003: Addressables 비동기 로딩과 씬 전환 타이밍 충돌

| 항목 | 내용 |
|------|------|
| **위험** | Addressables 비동기 로딩이 완료되기 전에 씬 전환이 발생하면, 콜백이 이미 파괴된 오브젝트를 참조하거나 AsyncOperationHandle이 릭된다. |
| **올바른 처리** | 씬 전환 전에 진행 중인 모든 AsyncOperationHandle을 취소하거나 완료 대기한 후 Release해야 한다. CancellationToken 패턴 또는 씬 생명주기에 연동된 로딩 매니저가 필요하다. |
| **misfill 패턴** | "씬 전환 시 로딩 중인 에셋의 콜백에서 Destroyed 오브젝트 접근" → MissingReferenceException. "Release 호출 없이 씬 전환" → 메모리 릭 누적. |
| **연결 FT** | FT-02, FT-P-002, FT-P-001 |

### HSR-004: Unity Physics vs Havok Physics 동작 차이

| 항목 | 내용 |
|------|------|
| **위험** | Unity Physics와 Havok Physics는 동일한 DOTS Physics 인터페이스를 사용하지만, 시뮬레이션 결과가 다를 수 있다. |
| **규칙** | 물리 시뮬레이션의 결정론적 동작(determinism)이 필요한 경우, 특정 물리 엔진에 대한 의존성을 명시적으로 문서화하고 테스트해야 한다. |
| **misfill 패턴** | "Unity Physics에서 테스트 통과 → Havok으로 교체 → 충돌 감지 타이밍이 달라져 게임플레이 버그 발생." "물리 재질(Physics Material)의 마찰/반발 계수 해석이 엔진마다 다름." |
| **연결 FT** | FT-01, FT-02, FT-P-003 |

### HSR-005: Editor 모드와 Play 모드의 동작 불일치

| 항목 | 내용 |
|------|------|
| **위험** | Editor에서 정상 동작하는 코드가 빌드(IL2CPP) 환경에서 실패하거나, Play Mode에서의 동작이 실제 빌드와 다를 수 있다. |
| **미정의 상황** | (1) 리플렉션 기반 코드가 IL2CPP 스트리핑으로 제거됨. (2) #if UNITY_EDITOR 분기의 런타임 누락. (3) AssetDatabase API가 빌드에서 사용 불가. (4) 에디터 전용 ScriptableObject 인스턴스 동작 차이. |
| **연결 FT** | FT-01, FT-P-005 |

---

## Section 4: 현재 알려진 주요 리스크

| # | 리스크 | 심각도 | 상태 | 관련 FT |
|---|--------|--------|------|---------|
| R-001 | 씬 전환 시 싱글톤 중복 생성/소실로 인한 상태 관리 불안정 | Critical | 아키텍처 결정 필요. DontDestroyOnLoad vs 씬 종속 매니저 방식 미확정. | FT-P-001 |
| R-002 | Addressables 카탈로그 버전 불일치로 인한 에셋 로드 실패 | High | 리모트 카탈로그 갱신 정책 미정의. 로컬 폴백 전략 필요. | FT-P-002 |
| R-003 | FixedUpdate/Update 혼용으로 인한 물리 비결정성 | Medium | 코딩 컨벤션으로 규칙 정의 중. 정적 분석 도구 미적용. | FT-P-003 |
| R-004 | UI 데이터 바인딩 해제 시 상태-표시 불일치 | Medium | UI Toolkit vs uGUI 선택 미확정. 바인딩 패턴 표준화 필요. | FT-P-004 |
| R-005 | IL2CPP 빌드에서 리플렉션 기반 시스템 실패 | High | link.xml 관리 정책 부재. 런타임 리플렉션 사용 범위 미파악. | FT-P-005 |
| R-006 | 플랫폼별 셰이더 호환성 미검증 | Medium | 타겟 플랫폼 목록 확정 필요. 셰이더 변형(variant) 관리 전략 미수립. | FT-P-005 |

---

## Section 5: 인간에게 반드시 확인해야 하는 규칙 목록

아래 항목은 Claude가 독립적으로 판정할 수 없는 사항이다. 해당 항목과 관련된 구현/검증을 수행할 때 반드시 인간(OR-08)에게 확인을 요청한다.

### HC-001: 싱글톤/매니저 생명주기 패턴

```
질문: 씬 전환 시 매니저 오브젝트의 생명주기를 어떤 패턴으로 관리하는가?
현재 가정: DontDestroyOnLoad 싱글톤 패턴.
리스크: Additive 씬 로딩 시 중복 생성, 싱글톤 간 초기화 순서 의존성,
       테스트 격리(Isolation) 어려움.
```

### HC-002: ScriptableObject 런타임 수정 정책

```
질문: ScriptableObject를 런타임에서 수정해도 되는 경우가 있는가?
      있다면 어떤 SO가 수정 가능이고 어떤 SO가 읽기 전용인가?
현재 가정: 모든 SO는 런타임에서 읽기 전용. 수정 필요 시 Instantiate로 복제.
리스크: 복제 비용, 원본-복제본 동기화 누락, 에디터와 빌드 간 동작 차이.
```

### HC-003: Addressables 로딩 실패 시 폴백 전략

```
질문: 에셋 로드 실패 시 기본 에셋(fallback)을 사용하는가, 에러를 표시하는가,
      게임을 중단하는가?
현재 가정: 없음 (에셋별로 다를 수 있음).
리스크: 폴백 없이 로드 실패하면 NullReferenceException 연쇄 발생.
```

### HC-004: 물리 엔진 선택 및 결정론 요구사항

```
질문: 게임에서 결정론적 물리(deterministic physics)가 필요한가?
      (예: 리플레이 시스템, 네트워크 동기화)
현재 가정: 비결정론 허용 (Unity Physics 기본 설정).
리스크: 결정론이 필요한데 비결정론으로 구현하면 리플레이/네트코드에서 디싱크 발생.
```

### HC-005: GDD 수치/공식의 구현 기준

```
질문: GDD에 명시된 게임플레이 공식(데미지 공식, 경험치 테이블 등)을
      코드에서 어떤 정밀도로 구현해야 하는가?
현재 가정: GDD 공식을 그대로 코드로 옮긴다.
리스크: GDD 공식이 부동소수점 정밀도에 따라 기대와 다른 결과를 낼 수 있다.
       정수 기반 계산 vs 실수 기반 계산의 차이가 밸런스에 영향.
```

### HC-006: 지역화(Localization) 키 관리 정책

```
질문: 지역화 키의 네이밍 컨벤션, 누락 키 처리 방식, 기본 언어 폴백 규칙은?
현재 가정: 없음.
리스크: 키 누락 시 빈 문자열이 표시되어 사용자 경험 저하.
       키 네이밍 불일치로 잘못된 텍스트가 표시될 수 있음.
```

### HC-007: 타겟 플랫폼 및 최소 사양

```
질문: 지원 대상 플랫폼 목록과 각 플랫폼의 최소 사양은?
현재 가정: 없음.
리스크: 플랫폼별 API 차이(파일 시스템, 입력 장치, 그래픽 API),
       메모리/성능 제약 조건이 구현 방향에 영향.
```
