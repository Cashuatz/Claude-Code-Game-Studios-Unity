---
Document Role: Core Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# FAILURE_TAXONOMY.base.md

> 본 문서는 VKL의 **크로스 프로젝트 실패 분류 체계**이다.
> 프로젝트별 실패 유형은 `.vkl/project/FAILURE_TAXONOMY.project.md`에 FT-P-XXX ID로 등록한다.

---

## Severity Scale

| Level | 한국어 | 정의 | 조치 시한 |
|-------|--------|------|-----------|
| **Critical** | 치명적 | 기능 전체가 동작 불능이거나 데이터 손실/보안 침해가 발생한다. 후속 작업이 모두 blocked. | 즉시 -- 다른 모든 작업 중단 후 수정 |
| **High** | 높음 | 주요 기능이 의도와 다르게 동작한다. 잘못된 결과가 다운스트림으로 전파될 위험이 있다. | 현재 루프 내 수정 필수 |
| **Medium** | 보통 | 부분적 결함이나 핵심 경로는 동작한다. 엣지 케이스 또는 비기능 요건 위반. | 다음 루프까지 수정 허용 |
| **Low** | 낮음 | 코드 스타일, 로그 누락, 사소한 포맷 불일치. 기능에 영향 없음. | 백로그 등록 후 일괄 처리 |

**Severity 조정 규칙:**
- 다운스트림 전파 가능성이 있으면 한 단계 올린다.
- 동일 실패가 2회 이상 반복되면 한 단계 올린다.
- 확실하지 않으면 높은 쪽을 선택한다 (보수적 판정).

---

## Classification Rules

### 복수 유형 해당 시 판정 규칙

1. **Root Cause Rule (근본 원인 우선):** 증상이 아닌 근본 원인의 유형을 선택한다.
2. **Upstream First Rule (업스트림 우선):** 원인 체인에서 가장 상류 유형을 primary로, 하류 유형을 secondary로 태깅한다.
3. **Specificity Rule (구체성 우선):** 동일 수준이면 더 구체적 유형을 선택한다. (예: FT-01 vs FT-02 -- 적극적 오보완 증거가 있으면 FT-02)
4. **Multi-Tag Rule (다중 태깅):** 독립적 원인이 둘 이상이면 모두 태깅한다. primary/secondary 구분 필수.

### Upstream vs Downstream 표기

```
Primary:   FT-XX (upstream)
Secondary: FT-YY (downstream), FT-ZZ (downstream)
```

**규칙:**
- Upstream은 반드시 1개만 지정한다.
- Downstream은 여러 개 가능하다.
- 수정 순서: 항상 upstream부터 수정한다.
- Upstream 수정 후 downstream이 자동 해결되는지 먼저 확인한다.
- 자동 해결되지 않는 downstream만 별도 수정한다.

---

## 12 Failure Types

---

### FT-01: Spec Gap (명세 공백)

| 항목 | 내용 |
|------|------|
| **ID** | FT-01 |
| **Name** | Spec Gap (명세 공백) |
| **Definition** | 명세에 특정 동작/조건/제약이 아예 기술되지 않아 LLM이 자체 추론으로 빈칸을 채운 상태. 명세가 명시적으로 틀린 것이 아니라 **침묵**하고 있는 것이 핵심 구분점. |
| **Symptoms** | 1. 구현이 "돌아가지만" 의도와 다른 결과를 생성한다. 2. 같은 프롬프트로 여러 번 실행 시 해당 부분의 구현이 매번 달라진다. 3. 코드 리뷰 시 "이건 왜 이렇게 했어?"라는 질문이 나온다. 4. 명세에서 관련 키워드를 검색해도 해당 동작에 대한 언급이 없다. |
| **Representative Causes** | 1. 요구사항 작성 시 암묵적 지식(domain knowledge)이 빠졌다. 2. 프롬프트가 "happy path"만 기술하고 예외/경계 조건을 생략했다. 3. 다단계 파이프라인에서 중간 단계의 출력 스펙이 정의되지 않았다. |
| **Priority Check Points** | 1. 명세에서 해당 동작을 키워드 검색한다. 2. 구현된 동작이 명세 어느 문장에서 도출되었는지 역추적한다. 3. 동일 기능을 별도로 구현하게 하면 다른 결과가 나오는지 확인한다. |
| **Return Path** | 1. OR-01로 명세 누락 지점을 특정한다. 2. 발견된 규칙을 명세에 추가한다 (spec-patch). 3. Knowledge Base에 "이 명세는 X 조건에 대해 침묵한다" 엔트리를 등록한다. 4. 해당 지점에 regression guard를 추가한다. |
| **Connected Oracles** | Primary: OR-01 / Secondary: OR-08 |

---

### FT-02: Hidden Semantic Rule Misfill (숨은 의미 규칙 오보완)

| 항목 | 내용 |
|------|------|
| **ID** | FT-02 |
| **Name** | Hidden Semantic Rule Misfill (숨은 의미 규칙 오보완) |
| **Definition** | 명세에 직접 기술되지 않은 도메인 고유 의미 규칙을 LLM이 그럴듯하지만 틀린 휴리스틱으로 보완한 상태. FT-01(단순 공백)과 달리 LLM이 **적극적으로 잘못된 규칙을 생성**해 적용한 것이 핵심 차이. |
| **Symptoms** | 1. 코드가 실행되고 결과도 "그럴듯하지만" 도메인 전문가가 보면 틀렸다. 2. 잘못된 해석이 후속 단계에서 더 구체적인 잘못된 구현으로 발전한다 (snowball effect). 3. 디버깅 시 "로직은 맞는데 전제가 틀렸다"는 결론에 도달한다. 4. 변수명/함수명이 도메인 용어를 사용하지만 실제 의미와 미묘하게 다르다. |
| **Representative Causes** | 1. 도메인 용어가 일상 언어와 의미가 다르다 (예: "boundary" = bbox 경계 vs. 물리적 외벽). 2. LLM이 학습 데이터의 가장 빈번한 용례를 선택했지만 현재 맥락에서는 희소한 용례가 정답이다. 3. 다단계 추론에서 초기 근사값이 이후 단계에서 확정값으로 취급되었다. |
| **Priority Check Points** | 1. 해당 도메인 용어의 **이 프로젝트에서의 정의**를 조회한다. 2. LLM이 적용한 규칙을 명시적으로 진술하게 하고 규칙의 출처(명세? 추론?)를 추적한다. 3. 해당 규칙의 반례(counterexample)를 구성하여 테스트한다. 4. 이전 iteration에서 같은 개념이 어떻게 해석되었는지 비교한다. |
| **Return Path** | 1. OR-01 + OR-08로 올바른 의미 규칙을 확정한다. 2. 확정된 규칙을 semantic-rules 지식 베이스에 등록한다. 3. 해당 규칙이 적용되는 모든 코드 지점을 역추적하여 수정한다. 4. OR-06로 수정 후 관계 불변량을 검증한다. |
| **Connected Oracles** | Primary: OR-01, OR-08 / Secondary: OR-06, OR-05 |

---

### FT-03: Input Normalization Failure (입력 정제 실패)

| 항목 | 내용 |
|------|------|
| **ID** | FT-03 |
| **Name** | Input Normalization Failure (입력 정제 실패) |
| **Definition** | 외부 입력(사용자 입력, 파일, API 응답 등)이 코드가 기대하는 형식/범위/인코딩과 맞지 않는 상태에서 처리에 진입하여 오동작이 발생한 상태. |
| **Symptoms** | 1. 특정 입력에서만 실패하고 다른 입력에서는 정상 동작한다. 2. 에러 메시지가 파싱/변환 관련이다 (TypeError, ValueError, encoding error). 3. 입력을 수동으로 정제하면 문제가 사라진다. 4. 같은 "논리적 값"이지만 포맷이 다른 입력에서 동작이 달라진다. |
| **Representative Causes** | 1. 입력 검증/정제 레이어가 누락되었거나 불완전하다. 2. 업스트림 시스템의 출력 포맷이 변경되었다. 3. 유니코드, 줄바꿈, 공백, BOM 등 보이지 않는 문자가 포함되었다. |
| **Priority Check Points** | 1. 실패를 유발한 정확한 입력 바이트열을 확보한다. 2. 해당 입력이 통과하는 검증/정제 체인을 단계별로 추적한다. 3. 입력 스키마/타입 정의와 실제 입력을 비교한다. |
| **Return Path** | 1. OR-02로 입력 포맷 위반 지점을 특정한다. 2. 입력 정제 로직을 추가/수정한다. 3. 문제 입력을 regression test에 추가한다. 4. 입력 스키마 문서를 갱신한다. |
| **Connected Oracles** | Primary: OR-02 / Secondary: OR-03 |

---

### FT-04: Format/Structure Violation (포맷/구조 위반)

| 항목 | 내용 |
|------|------|
| **ID** | FT-04 |
| **Name** | Format/Structure Violation (포맷/구조 위반) |
| **Definition** | 코드의 **출력**이 요구된 포맷, 스키마, 구조 규약을 위반하는 상태. 내용(값)은 올바를 수 있지만 형식이 틀렸다. |
| **Symptoms** | 1. 다운스트림 소비자가 파싱 에러를 발생시킨다. 2. JSON schema validation, XML schema validation 등이 실패한다. 3. 사람이 읽으면 내용은 맞지만 기계가 처리할 수 없다. 4. 필수 필드 누락, 필드 순서 오류, 잘못된 데이터 타입이 관찰된다. |
| **Representative Causes** | 1. 출력 포맷 스펙이 불명확하거나 변경되었다. 2. 문자열 템플릿/직렬화 로직에 버그가 있다. 3. 특정 조건에서 출력 경로가 분기하여 일관성이 깨졌다. |
| **Priority Check Points** | 1. 출력 스키마 정의서와 실제 출력을 diff한다. 2. 출력 생성 코드의 모든 분기 경로를 확인한다. 3. 정상 출력과 비정상 출력을 나란히 비교한다. |
| **Return Path** | 1. OR-02로 위반 지점을 특정한다. 2. 출력 생성 로직을 수정한다. 3. 출력 schema validator를 테스트 체인에 추가한다. 4. 포맷 규약 문서를 갱신/보강한다. |
| **Connected Oracles** | Primary: OR-02 / Secondary: OR-01 |

---

### FT-05: Execution Failure (실행 실패)

| 항목 | 내용 |
|------|------|
| **ID** | FT-05 |
| **Name** | Execution Failure (실행 실패) |
| **Definition** | 코드가 런타임에 크래시, 예외, 무한 루프, 타임아웃 등으로 정상 완료되지 못하는 상태. 로직의 옳고 그름 이전에 **실행 자체가 안 되는 것**이 핵심. |
| **Symptoms** | 1. 스택 트레이스, 예외 메시지가 출력된다. 2. 프로세스가 비정상 종료 코드(non-zero exit)를 반환한다. 3. 응답이 없거나 타임아웃이 발생한다. 4. 메모리 사용량/CPU가 비정상적으로 증가한다. |
| **Representative Causes** | 1. 컴파일/빌드 에러 (구문 오류, 타입 불일치). 2. 런타임 예외 (null reference, index out of bounds, division by zero). 3. 리소스 고갈 (메모리, 파일 핸들, 네트워크 연결). |
| **Priority Check Points** | 1. 에러 메시지/스택 트레이스를 정확히 읽는다. 2. 실패 지점의 입력 상태를 확인한다. 3. 동일 코드가 다른 환경에서는 동작하는지 확인한다 (FT-11 배제). |
| **Return Path** | 1. OR-03로 실행 성공/실패를 판정한다. 2. 에러의 직접 원인을 수정한다. 3. 해당 실패 경로에 대한 테스트를 추가한다. 4. 필요시 OR-04로 로그 충분성을 확인한다. |
| **Connected Oracles** | Primary: OR-03 / Secondary: OR-04, OR-09 |

---

### FT-06: Observability Failure (로그/관측 실패)

| 항목 | 내용 |
|------|------|
| **ID** | FT-06 |
| **Name** | Observability Failure (로그/관측 실패) |
| **Definition** | 시스템이 동작은 하지만 그 동작 과정을 관측할 수 없거나 관측 정보가 불충분/부정확하여 다른 실패의 진단이 방해받는 상태. |
| **Symptoms** | 1. 버그를 재현했지만 로그에 관련 정보가 없다. 2. 로그가 있지만 어떤 입력/상태에서 발생했는지 알 수 없다. 3. 디버그 출력이 너무 많아서 핵심 정보가 묻힌다. 4. 타임스탬프, 식별자 등 상관관계 추적 정보가 없다. |
| **Representative Causes** | 1. 로깅이 아예 구현되지 않았다. 2. 로그 레벨이 부적절하게 설정되어 중요 정보가 필터링되었다. 3. 구조화 로깅이 아닌 자유형 문자열 로그라 파싱이 불가능하다. |
| **Priority Check Points** | 1. 현재 실패를 진단하는 데 필요한 정보가 무엇인지 정의한다. 2. 그 정보가 현재 로그/메트릭에 포함되어 있는지 확인한다. 3. 로그 수집/저장 파이프라인이 정상인지 확인한다. |
| **Return Path** | 1. OR-04로 관측성 수준을 판정한다. 2. 필요한 로그/메트릭 포인트를 추가한다. 3. 로그 포맷을 구조화한다 (structured logging). 4. Observability 체크리스트를 갱신한다. |
| **Connected Oracles** | Primary: OR-04 / Secondary: OR-03 |

---

### FT-07: External Verifier Failure (외부 검증기 실패)

| 항목 | 내용 |
|------|------|
| **ID** | FT-07 |
| **Name** | External Verifier Failure (외부 검증기 실패) |
| **Definition** | 외부 검증 도구(linter, type checker, static analyzer, CI pipeline 등)가 코드를 거부했지만 코드의 **기능적 의도**는 올바를 수 있는 상태. 검증기 규칙과 구현 사이의 불일치. |
| **Symptoms** | 1. CI/CD 파이프라인이 실패한다. 2. Lint 경고/에러가 발생한다. 3. 타입 체커가 타입 불일치를 보고한다. 4. 코드는 로컬에서 동작하지만 검증기를 통과하지 못한다. |
| **Representative Causes** | 1. 검증기의 규칙을 인지하지 못한 상태에서 코드를 작성했다. 2. 검증기 설정이 변경되어 이전에 통과하던 코드가 거부되었다. 3. 검증기 자체의 버그 또는 오탐(false positive). |
| **Priority Check Points** | 1. 검증기 에러 메시지를 정확히 파악한다. 2. 해당 규칙의 의도를 이해한다 (정당한 거부인지 오탐인지). 3. 검증기 버전과 설정을 확인한다. |
| **Return Path** | 1. OR-03 + 해당 외부 검증기로 실행 결과를 재확인한다. 2. 정당한 거부이면 코드를 수정한다. 3. 오탐이면 검증기 설정을 조정하거나 예외 규칙을 추가한다. 4. 검증기 규칙을 Knowledge Base에 등록한다. |
| **Connected Oracles** | Primary: OR-03 / Secondary: OR-02, OR-09 |

---

### FT-08: Validation Shortfall (validation 미달)

| 항목 | 내용 |
|------|------|
| **ID** | FT-08 |
| **Name** | Validation Shortfall (validation 미달) |
| **Definition** | 기존 테스트/검증이 해당 실패를 감지할 수 없는 상태. 코드에 버그가 있지만 테스트가 그 버그를 다루지 않아서 "모든 테스트 통과"가 거짓 안전감을 준다. |
| **Symptoms** | 1. 모든 테스트가 통과하지만 수동 검증에서 버그가 발견된다. 2. 코드 커버리지는 높지만 결함이 발견된다. 3. 회귀(regression) 버그가 테스트 스위트를 통과한다. 4. 테스트 케이스가 특정 입력 범위만 다루고 있다. |
| **Representative Causes** | 1. 테스트가 핵심 불변량이 아닌 구현 세부사항을 검증하고 있다. 2. 엣지 케이스에 대한 테스트가 없다. 3. 테스트 자체에 버그가 있어서 항상 통과한다 (tautological test). |
| **Priority Check Points** | 1. 발견된 버그를 감지해야 했던 테스트가 있는지 확인한다. 2. 기존 테스트의 assertion이 실제로 의미 있는 속성을 검증하는지 확인한다. 3. Mutation testing으로 테스트 스위트의 민감도를 평가한다. |
| **Return Path** | 1. OR-05 또는 OR-06로 검증 갭을 식별한다. 2. 누락된 테스트 케이스를 추가한다. 3. 테스트 전략을 재검토하고 갱신한다. 4. Knowledge Base에 "이 영역은 테스트가 취약하다" 엔트리를 등록한다. |
| **Connected Oracles** | Primary: OR-05, OR-06 / Secondary: OR-07 |

---

### FT-09: Edge-Case Fragility (엣지 케이스 취약)

| 항목 | 내용 |
|------|------|
| **ID** | FT-09 |
| **Name** | Edge-Case Fragility (엣지 케이스 취약) |
| **Definition** | 정상 범위 입력에서는 올바르게 동작하지만 경계값, 극한값, 비정형 입력에서 실패하는 상태. |
| **Symptoms** | 1. 빈 입력, 단일 원소, 최대 크기 등 경계 조건에서 실패한다. 2. 특수 문자, 음수, 0, null 등에서 예상치 못한 동작이 발생한다. 3. 동시성(concurrent) 조건에서 간헐적으로 실패한다 (race condition). 4. 정상 테스트는 모두 통과하지만 fuzz testing에서 크래시가 발생한다. |
| **Representative Causes** | 1. 구현이 "typical case"만 고려하고 boundary를 다루지 않았다. 2. off-by-one 에러. 3. 타입의 최소/최대값(int overflow 등)을 고려하지 않았다. |
| **Priority Check Points** | 1. 경계값 분석(Boundary Value Analysis)을 수행한다. 2. 동치 분할(Equivalence Partitioning)의 각 클래스에서 테스트한다. 3. null, empty, max, min, negative 입력을 시도한다. |
| **Return Path** | 1. OR-07로 비정상 입력 처리를 검증한다. 2. OR-06로 관계 불변량을 확인한다. 3. 경계값 테스트를 추가한다. 4. 방어적 코딩 패턴(guard clause, input validation)을 적용한다. |
| **Connected Oracles** | Primary: OR-07 / Secondary: OR-06, OR-05 |

---

### FT-10: Metamorphic Relation Failure (관계 테스트 실패)

| 항목 | 내용 |
|------|------|
| **ID** | FT-10 |
| **Name** | Metamorphic Relation Failure (관계 테스트 실패) |
| **Definition** | 개별 입출력은 올바르게 보이지만 입력 간 관계에서 기대되는 출력 관계가 성립하지 않는 상태. 예: 입력을 2배로 하면 출력도 2배여야 하는데 그렇지 않다. |
| **Symptoms** | 1. 단일 테스트 케이스는 통과하지만 입력을 체계적으로 변환한 쌍(pair)에서 관계가 깨진다. 2. 일관성이 없다 -- 같은 연산을 다른 순서로 하면 결과가 달라진다. 3. Scale 변환, 회전, 순서 변경 등 불변량이 위반된다. 4. 단조성("A > B이면 f(A) > f(B)") 같은 수학적 관계가 깨진다. |
| **Representative Causes** | 1. 알고리즘이 수학적 성질(교환법칙, 결합법칙, 단조성 등)을 보존하지 않는다. 2. 부동소수점 연산 누적 오차가 관계를 깨뜨린다. 3. 상태 의존적 구현이 입력 순서에 따라 다른 결과를 낸다. |
| **Priority Check Points** | 1. 해당 기능에 적용 가능한 metamorphic relation을 식별한다. 2. 관계가 깨지는 최소 입력 쌍을 찾는다. 3. 중간 계산 결과를 추적하여 관계가 어디서 깨지는지 특정한다. |
| **Return Path** | 1. OR-06로 관계 위반을 확정한다. 2. 관계를 보존하도록 알고리즘을 수정한다. 3. Metamorphic test를 회귀 테스트에 추가한다. 4. 수정 후 OR-05로 기존 결과와의 일관성을 확인한다. |
| **Connected Oracles** | Primary: OR-06 / Secondary: OR-05 |

---

### FT-11: Environment/Version/Cache Issue (환경/버전/캐시 문제)

| 항목 | 내용 |
|------|------|
| **ID** | FT-11 |
| **Name** | Environment/Version/Cache Issue (환경/버전/캐시 문제) |
| **Definition** | 코드 자체에는 결함이 없지만 실행 환경, 의존성 버전, 캐시 상태 차이로 인해 동작이 달라지는 상태. |
| **Symptoms** | 1. "내 컴퓨터에서는 되는데" 현상. 2. 클린 빌드에서는 되지만 기존 캐시에서는 실패한다 (또는 그 반대). 3. 특정 OS, 런타임 버전에서만 실패한다. 4. 의존성 업데이트 후 이전에 동작하던 기능이 깨진다. |
| **Representative Causes** | 1. 의존성 버전이 lock file에 고정되지 않았다. 2. 환경 변수, 파일 경로, 권한 등이 환경마다 다르다. 3. 이전 실행의 캐시/임시 파일이 현재 실행에 영향을 준다. |
| **Priority Check Points** | 1. 실패 환경과 성공 환경의 차이를 비교한다 (runtime version, OS, env vars). 2. 클린 환경(새 컨테이너, 캐시 삭제)에서 재현을 시도한다. 3. 의존성 트리를 비교한다 (lock file diff). |
| **Return Path** | 1. OR-09로 환경 요인을 판정한다. 2. 환경 요구사항을 명시적으로 문서화한다. 3. 의존성을 고정하고 캐시 전략을 정비한다. 4. CI 환경을 재현 가능하게 구성한다 (Docker, Nix 등). |
| **Connected Oracles** | Primary: OR-09 / Secondary: OR-03 |

---

### FT-12: Scope Violation / Over-Edit (과잉 수정 또는 범위 침범)

| 항목 | 내용 |
|------|------|
| **ID** | FT-12 |
| **Name** | Scope Violation / Over-Edit (과잉 수정 또는 범위 침범) |
| **Definition** | 수정 범위가 요청된 범위를 초과하여 관련 없는 코드까지 변경하거나, 수정이 의도한 것보다 과도하게 공격적이어서 부작용을 초래하는 상태. LLM 특유의 "도움이 되고 싶은" 경향이 주된 원인. |
| **Symptoms** | 1. PR diff에 요청하지 않은 변경이 포함되어 있다. 2. 수정 후 관련 없는 기능이 깨진다. 3. 리팩터링이 요청 범위를 넘어 전체 파일/모듈로 확산되었다. 4. 변수명, import, 포맷 등이 요청 없이 변경되었다. |
| **Representative Causes** | 1. LLM이 "더 나은 코드"를 만들기 위해 자발적으로 리팩터링을 수행했다. 2. 수정 범위에 대한 명시적 제약이 프롬프트에 없었다. 3. 한 파일의 수정이 import/dependency 체인을 따라 전파되었다. |
| **Priority Check Points** | 1. 변경된 파일/라인을 요청 범위와 비교한다. 2. 요청하지 않은 변경이 의도적 개선인지 부작용인지 판단한다. 3. 변경되지 않아야 할 기능의 회귀 테스트를 수행한다. |
| **Return Path** | 1. OR-01로 요청 범위를 재확인한다. 2. 범위 외 변경을 revert한다. 3. OR-05로 revert 후 원래 기능이 보존되었는지 확인한다. 4. 향후 프롬프트에 범위 제약을 명시적으로 추가한다. |
| **Connected Oracles** | Primary: OR-01 / Secondary: OR-05, OR-03 |

---

## Classification Decision Tree

아래 결정 트리를 위에서 아래로 따라가며 첫 번째로 해당하는 leaf에서 분류한다.

```
[START] 실패가 감지되었다
  |
  +--[Q1] 코드가 실행되는가? (빌드/런타임 에러 없이 완료)
  |    |
  |    +--[NO] 에러가 환경/버전/캐시 문제인가?
  |    |    |
  |    |    +--[YES] --> FT-11: Environment/Version/Cache Issue
  |    |    +--[NO]  --> FT-05: Execution Failure
  |    |
  |    +--[YES] 다음 질문으로
  |
  +--[Q2] 외부 검증기(linter, CI, type checker)가 거부하는가?
  |    |
  |    +--[YES] --> FT-07: External Verifier Failure
  |    +--[NO]  다음 질문으로
  |
  +--[Q3] 출력 포맷/구조가 올바른가?
  |    |
  |    +--[NO] 입력이 원인인가 출력 생성이 원인인가?
  |    |    |
  |    |    +--[입력] --> FT-03: Input Normalization Failure
  |    |    +--[출력] --> FT-04: Format/Structure Violation
  |    |
  |    +--[YES] 다음 질문으로
  |
  +--[Q4] 수정 범위가 요청을 초과했는가?
  |    |
  |    +--[YES] --> FT-12: Scope Violation / Over-Edit
  |    +--[NO]  다음 질문으로
  |
  +--[Q5] 관측/로그가 충분한가? (진단에 필요한 정보가 있는가?)
  |    |
  |    +--[NO] --> FT-06: Observability Failure (다른 FT와 복합 가능)
  |    +--[YES] 다음 질문으로
  |
  +--[Q6] 출력 값이 올바른가? (기능적으로 기대와 일치하는가?)
  |    |
  |    +--[YES] Metamorphic relation도 성립하는가?
  |    |    |
  |    |    +--[NO]  --> FT-10: Metamorphic Relation Failure
  |    |    +--[YES] 엣지 케이스에서도 성립하는가?
  |    |         |
  |    |         +--[NO]  --> FT-09: Edge-Case Fragility
  |    |         +--[YES] 기존 테스트가 이를 검증하고 있었는가?
  |    |              |
  |    |              +--[NO]  --> FT-08: Validation Shortfall
  |    |              +--[YES] --> (실패 아님 또는 재분석 필요)
  |    |
  |    +--[NO] 잘못된 값의 원인이 명세 부재인가?
  |         |
  |         +--[YES] LLM이 적극적으로 잘못된 규칙을 생성했는가?
  |         |    |
  |         |    +--[YES] --> FT-02: Hidden Semantic Rule Misfill
  |         |    +--[NO]  --> FT-01: Spec Gap
  |         |
  |         +--[NO] --> (다른 FT 재검토 또는 복합 분류)
```

**주의:** 이 트리는 primary 분류 전용이다. Secondary 분류는 트리와 별도로 전체 12 유형을 검토하여 태깅한다.

---

## FT Quick-Lookup Table

| ID | Name | 핵심 키워드 | Primary Oracle | Severity 기본값 |
|----|------|------------|----------------|-----------------|
| FT-01 | Spec Gap | 명세 침묵, 빈칸, 미기술 | OR-01 | High |
| FT-02 | Hidden Semantic Rule Misfill | 오보완, 도메인 오해, snowball | OR-01, OR-08 | High |
| FT-03 | Input Normalization Failure | 파싱 에러, 입력 형식, 인코딩 | OR-02 | Medium |
| FT-04 | Format/Structure Violation | 출력 포맷, 스키마 위반, 직렬화 | OR-02 | Medium |
| FT-05 | Execution Failure | 크래시, 예외, 타임아웃, exit code | OR-03 | Critical |
| FT-06 | Observability Failure | 로그 부재, 진단 불가, 추적 불가 | OR-04 | Medium |
| FT-07 | External Verifier Failure | CI 실패, lint, type check | OR-03 | High |
| FT-08 | Validation Shortfall | 테스트 통과+버그 존재, 커버리지 갭 | OR-05, OR-06 | High |
| FT-09 | Edge-Case Fragility | 경계값, null, 빈 입력, fuzz | OR-07 | Medium |
| FT-10 | Metamorphic Relation Failure | 관계 깨짐, 불변량 위반, 비일관 | OR-06 | High |
| FT-11 | Environment/Version/Cache Issue | 환경 차이, 버전, 캐시, "내 PC에서는 됨" | OR-09 | Medium |
| FT-12 | Scope Violation / Over-Edit | 범위 초과, 불필요 변경, 자발적 리팩터 | OR-01 | High |

---

## Extension Rules

프로젝트별 실패 유형은 `.vkl/project/FAILURE_TAXONOMY.project.md`에 정의한다.

| 항목 | 규칙 |
|------|------|
| **ID 체계** | FT-P-XXX (FT-P-001부터 순차 부여) |
| **필수 필드** | base FT-XX의 모든 필드와 동일한 구조 사용 |
| **parent 참조** | 가장 가까운 base FT-XX를 parent로 명시 (예: `parent: FT-02`) |
| **scope 선언** | 해당 프로젝트에서만 유효함을 선언 |
| **병합 금지** | project FT가 범용적이라 판단되면 `.vkl/proposals/`에 base 승격 제안서를 작성한다. 직접 이 문서를 수정하지 않는다. |

---

## Example: Blender Fracture -- Boundary/Thickness Axis Misfill

### 상황

Blender glass fracture 프로젝트에서 "outer boundary" 개념이 다단계에 걸쳐 오해석됨.

| 단계 | LLM의 해석 | 실제 정답 |
|------|-----------|-----------|
| 1단계 | "outer boundary" = 충격 지점으로부터의 거리 | 외벽 물리적 경계 |
| 2단계 | "outer boundary" = bounding box의 전체 경계 | 두께 축(가장 얇은 축)을 제외한 외벽 경계 |
| 3단계 | "boundary"에 두께 축도 포함 | "가장 얇은 축은 제외" 규칙 적용 |

### 분류 기록

```yaml
failure_id: BF-2024-001
classification:
  primary:
    type: FT-02
    name: Hidden Semantic Rule Misfill
    severity: High
    upstream: true
    evidence:
      - LLM이 "boundary"를 일반적 의미(bbox 전체)로 해석
      - 실제로는 도메인 고유 규칙("가장 얇은 축 제외") 적용 필요
      - 3단계에 걸쳐 snowball
  secondary:
    - type: FT-01
      name: Spec Gap
      downstream: true
      evidence: "outer boundary"의 정확한 정의가 명세에 부재
    - type: FT-08
      name: Validation Shortfall
      downstream: true
      evidence: 두께 축 포함 여부를 검증하는 테스트 부재

oracle_applied:
  - OR-01: 명세에서 "outer boundary" 정의 검색 -> 부재 확인
  - OR-08: 도메인 전문가에게 정확한 의미 확인
  - OR-06: 두께 변경 불변량 검증 -> 관계 위반 확인
  - OR-05: 올바른 규칙 적용 전후 결과 비교

resolution:
  spec_patch: '"exclude thinnest axis" 규칙 추가'
  knowledge_base_entry:
    rule: exclude-thinnest-axis
    domain: fracture-simulation
```

### FT-02 vs FT-01 판별 근거

| 판별 기준 | FT-01 | FT-02 |
|-----------|-------|-------|
| 명세 상태 | 빈칸이 있음 | 빈칸이 있음 (동일) |
| LLM 행동 | 빈칸을 빈칸으로 남기거나 기본값 사용 | **적극적으로 잘못된 규칙을 생성하여 적용** |
| 본 사례 | -- | "boundary = bbox 전체"라는 규칙을 생성, 3단계에 걸쳐 적용 |
| 결론 | Secondary (공백 자체) | **Primary (오보완 행위)** |

---

*End of FAILURE_TAXONOMY.base.md*
