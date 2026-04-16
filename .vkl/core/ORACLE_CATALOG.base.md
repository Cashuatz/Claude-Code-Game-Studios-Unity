---
Document Role: Core Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# ORACLE_CATALOG.base.md

> 본 문서는 VKL의 **크로스 프로젝트 오라클 판정 기준표**이다.
> 프로젝트별 오라클은 `.vkl/project/ORACLE_CATALOG.project.md`에 OR-P-XXX ID로 등록한다.
> **"No Oracle, No Judgment."** -- 오라클 없이 내린 판정은 무효.

---

## 9 Oracle Types

---

### OR-01: Spec Oracle (명세 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-01 |
| **Name** | Spec Oracle (명세 오라클) |
| **Definition** | 명세(사양서, 프롬프트, 요구사항 문서, Knowledge Base 등록 규칙)에 기술된 내용과 실제 구현을 대조하여 일치 여부를 판정한다. |
| **When to Use** | 1. 구현의 정확성을 판정해야 할 때 (가장 먼저 시도해야 하는 기본 오라클). 2. 특정 동작이 "의도된 것인지" 판단해야 할 때. 3. 변경 범위가 요청을 초과했는지 확인할 때. 4. Knowledge Base에 등록된 규칙과 구현을 대조할 때. |
| **Limitations** | 1. 명세 자체에 빈칸이 있으면 판정할 수 없다 (FT-01 상황). 2. 명세가 모호하면 해석이 갈려 결정력이 약하다. 3. 명세가 틀린 경우 틀린 판정을 내린다. 4. 비기능 요건(성능, 사용성)은 정량적으로 기술되지 않는 한 판정 불가. |
| **Input Requirements** | 1. 명세 문서 또는 프롬프트 원문. 2. Knowledge Base의 관련 규칙 엔트리 (있는 경우). 3. 구현 코드 또는 실행 결과. |
| **Output Format** | `verdict: PASS / FAIL / INCONCLUSIVE`, `spec_reference`, `deviation`, `confidence: high / medium / low` |
| **Connected Failure Types** | Primary: FT-01, FT-02, FT-04, FT-12 |

---

### OR-02: Format Oracle (포맷 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-02 |
| **Name** | Format Oracle (포맷 오라클) |
| **Definition** | 입출력의 포맷, 스키마, 구조가 정의된 규약과 일치하는지 기계적으로 검증한다. JSON Schema, XML Schema, 정규표현식 매칭, 타입 체크 등이 구현체에 해당. |
| **When to Use** | 1. 출력이 다운스트림 소비자에게 전달되기 전에 포맷을 검증할 때. 2. 입력이 처리 파이프라인에 진입하기 전에 형식을 확인할 때. 3. 직렬화/역직렬화 결과를 확인할 때. 4. 파일 포맷, API 응답 구조 등을 검증할 때. |
| **Limitations** | 1. 포맷이 올바르더라도 **값의 정확성**은 판정할 수 없다. 2. 포맷 규약 자체가 정의되지 않았으면 적용 불가. 3. 자유형 텍스트 출력에는 제한적으로만 적용 가능. |
| **Input Requirements** | 1. 포맷 규약 정의 (스키마, 정규식, 타입 정의 등). 2. 검증 대상 데이터 (입력 또는 출력). |
| **Output Format** | `verdict: PASS / FAIL`, `target: input / output`, `schema_ref`, `violations[]` |
| **Connected Failure Types** | Primary: FT-03, FT-04 / Secondary: FT-07 |

---

### OR-03: Execution Oracle (실행 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-03 |
| **Name** | Execution Oracle (실행 오라클) |
| **Definition** | 코드가 에러 없이 실행 완료되는지, 기대한 exit code를 반환하는지, 지정된 시간 내에 완료되는지를 판정한다. 출력의 내용 정확성은 판정하지 않는다 -- 실행 성공/실패만 판정. |
| **When to Use** | 1. 코드를 수정한 후 빌드/실행이 되는지 기본 확인할 때. 2. 새로 작성한 코드를 처음 실행할 때. 3. CI/CD 파이프라인의 각 단계 통과 여부를 확인할 때. 4. 성능 regression을 의심하여 실행 시간을 확인할 때. |
| **Limitations** | 1. 실행 성공해도 결과가 올바른지 알 수 없다 (false confidence 위험). 2. 비결정적 실패는 한 번의 실행으로 판정할 수 없다. 3. 조용한 실패(silent failure)를 감지할 수 없다. |
| **Input Requirements** | 1. 실행 가능한 코드 또는 빌드 명령. 2. 기대 exit code (기본: 0). 3. 타임아웃 기준 (있는 경우). |
| **Output Format** | `verdict: PASS / FAIL / TIMEOUT`, `exit_code`, `expected_exit_code`, `duration_ms`, `error_summary` |
| **Connected Failure Types** | Primary: FT-05, FT-07 / Secondary: FT-11, FT-12 |

---

### OR-04: Observability Oracle (관측 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-04 |
| **Name** | Observability Oracle (관측 오라클) |
| **Definition** | 시스템의 실행 과정이 충분히 관측 가능한지(로그, 메트릭, 트레이스 등) 판정한다. 다른 오라클이 판정을 내리기 위해 필요한 **정보 인프라**가 갖추어져 있는지를 평가하는 메타 오라클. |
| **When to Use** | 1. 다른 실패를 진단하려 했는데 정보가 부족할 때. 2. 새로운 기능을 추가한 후 관측 가능성을 점검할 때. 3. 디버깅 세션 시작 시 진단 가능성을 사전 평가할 때. 4. 로그 시스템 자체의 정상 동작을 확인할 때. |
| **Limitations** | 1. 관측 가능하다고 해서 동작이 올바른 것은 아니다 (관측성 != 정확성). 2. 과도한 관측이 오히려 문제인 경우도 있으나, "충분성"만 판정한다. 3. 보안/개인정보 제약으로 관측 불가한 경우 제약을 해제할 수 없다. |
| **Input Requirements** | 1. 진단 대상 실패 유형 또는 진단 시나리오. 2. 현재 사용 가능한 로그/메트릭/트레이스 목록. 3. 진단에 필요한 정보 항목 체크리스트. |
| **Output Format** | `verdict: SUFFICIENT / INSUFFICIENT`, `required_info[]`, `coverage`, `recommendation` |
| **Connected Failure Types** | Primary: FT-06 / Secondary: FT-05 (진단 지원) |

---

### OR-05: Comparison Oracle (비교 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-05 |
| **Name** | Comparison Oracle (비교 오라클) |
| **Definition** | 두 개 이상의 결과를 직접 비교하여 일관성, 동등성, 또는 예상된 차이를 판정한다. 비교 대상은 이전 버전 출력, 다른 구현 출력, 골든 데이터셋, 또는 수정 전후 결과 등. |
| **When to Use** | 1. 수정 전후의 동작을 비교하여 regression을 확인할 때. 2. 두 가지 다른 구현이 동일한 결과를 내는지 확인할 때. 3. 골든 데이터셋과 실제 출력을 비교할 때. 4. A/B 테스트 결과를 판정할 때. |
| **Limitations** | 1. 비교 대상(reference) 자체가 틀려있으면 잘못된 판정. 2. 부동소수점 비교 시 tolerance 기준이 필요하다. 3. 비결정적 출력(난수, 타임스탬프 포함)은 직접 비교할 수 없다. 4. 골든 데이터셋이 없으면 적용 불가. |
| **Input Requirements** | 1. 비교 대상 A (기준, reference). 2. 비교 대상 B (검증 대상, candidate). 3. 비교 방법 (exact match, approximate, structural 등). 4. Tolerance 기준 (수치 비교 시). |
| **Output Format** | `verdict: MATCH / MISMATCH / PARTIAL_MATCH`, `comparison_type`, `reference`, `candidate`, `differences[]`, `match_ratio` |
| **Connected Failure Types** | Primary: FT-08, FT-10 / Secondary: FT-09, FT-12 |

---

### OR-06: Metamorphic Oracle (메타모픽 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-06 |
| **Name** | Metamorphic Oracle (메타모픽 오라클) |
| **Definition** | 개별 출력의 정확성이 아닌, 입력 변환에 따른 출력 간 관계(metamorphic relation)가 성립하는지 판정한다. 정답을 모르더라도 관계가 보존되는지 확인할 수 있어서 오라클 부재 상황에서 특히 강력. |
| **When to Use** | 1. 기대 출력(골든 데이터)이 존재하지 않아 OR-05를 적용할 수 없을 때. 2. 수학적/물리적 불변량이 존재하는 도메인일 때. 3. 일관성(consistency)을 검증해야 할 때. 4. 입력 변환에 대한 출력의 예측 가능한 변화를 확인할 때. |
| **Limitations** | 1. 적용 가능한 metamorphic relation 식별 자체가 도메인 지식을 요한다. 2. 모든 기능에 적용 가능한 relation이 존재하는 것은 아니다. 3. Relation이 성립해도 개별 출력이 올바른 것은 아니다 (필요조건, 충분조건 아님). 4. 근사적 relation만 존재하는 경우 tolerance 설정이 어렵다. |
| **Input Requirements** | 1. Metamorphic relation 정의 (MR). 2. 원본 입력 및 출력. 3. 변환된 입력 및 그에 대한 출력. |
| **Output Format** | `verdict: RELATION_HOLDS / RELATION_VIOLATED`, `metamorphic_relation`, `source_input/output`, `follow_up_input/output`, `expected_relation`, `actual_relation` |
| **Connected Failure Types** | Primary: FT-10, FT-02 (관계 기반 검출) / Secondary: FT-08, FT-09 |

---

### OR-07: Negative Oracle (네거티브 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-07 |
| **Name** | Negative Oracle (네거티브 오라클) |
| **Definition** | 올바른 출력이 무엇인지 정의하는 대신, **발생해서는 안 되는 것**을 정의하고 그것의 부재를 확인하여 판정한다. "이것은 절대 일어나면 안 된다"라는 불변량 기반. |
| **When to Use** | 1. 정상 동작을 완전히 정의하기 어렵지만 비정상 동작은 명확히 정의할 수 있을 때. 2. 보안 속성 검증 (예: 사용자 A 데이터가 사용자 B에게 노출 금지). 3. 비정상/악의적/엣지 케이스 입력에 대한 방어 확인. 4. 크래시, 데이터 손실, 무한 루프 등 절대 조건 검증. |
| **Limitations** | 1. "일어나서는 안 되는 것"의 목록이 완전하지 않을 수 있다. 2. 부정적 조건을 모두 통과해도 긍정적으로 올바른 것은 아니다. 3. 조건이 너무 느슨하면 실제 결함을 놓친다. 4. 조건이 너무 엄격하면 false positive가 발생한다. |
| **Input Requirements** | 1. 네거티브 조건 목록: "X가 발생하면 FAIL." 2. 실행 결과 또는 시스템 상태. 3. 검증 방법 (assertion, invariant check, sanitizer 등). |
| **Output Format** | `verdict: CLEAN / VIOLATION`, `negative_conditions_checked[]`, `violations_detail[]` |
| **Connected Failure Types** | Primary: FT-09 / Secondary: FT-08, FT-05 |

---

### OR-08: Human Oracle (인간 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-08 |
| **Name** | Human Oracle (인간 오라클) |
| **Definition** | 도메인 전문가 또는 사용자에게 판정을 위임한다. 다른 오라클로 판정이 불가능하거나, 도메인 고유 암묵적 지식이 필요하거나, 최종 승인이 필요한 경우에 사용하는 **최상위 권위 오라클**. |
| **When to Use** | 1. 도메인 고유 용어의 정확한 의미를 확인해야 할 때 (FT-02 대응 핵심). 2. 명세에 기술되지 않은 동작의 의도를 확인해야 할 때. 3. 주관적 품질 판정이 필요할 때 (UX, 시각적 결과물). 4. 다른 오라클들이 INCONCLUSIVE를 반환했을 때. 5. 최종 릴리스 승인이 필요할 때. |
| **Limitations** | 1. 응답 시간이 비결정적이다 (사람의 가용성에 의존). 2. 비용이 높다. 3. 일관성이 보장되지 않는다. 4. 자동화 루프에 통합하기 어렵다 (루프가 블로킹됨). 5. 인간의 판정도 틀릴 수 있다. |
| **Input Requirements** | 1. 명확하고 구체적인 질문 (예/아니오 또는 선택지 형태가 이상적). 2. 판정에 필요한 맥락 요약. 3. LLM의 잠정 판정과 근거. 4. 구체적인 예시/스크린샷 (가능한 경우). |
| **Output Format** | `verdict: CONFIRMED / REJECTED / MODIFIED`, `question_asked`, `human_response`, `applied_rule`, `knowledge_base_update: true / false` |
| **Connected Failure Types** | Primary: FT-02, FT-01 / Secondary: 모든 FT에 대해 최종 판정 역할 가능 |

---

### OR-09: Environment Oracle (환경 오라클)

| 항목 | 내용 |
|------|------|
| **ID** | OR-09 |
| **Name** | Environment Oracle (환경 오라클) |
| **Definition** | 실행 환경(OS, 런타임 버전, 의존성 트리, 환경 변수, 캐시 상태 등)이 코드가 요구하는 조건과 일치하는지 판정한다. |
| **When to Use** | 1. "내 컴퓨터에서는 되는데" 상황이 발생했을 때. 2. CI와 로컬의 실행 결과가 다를 때. 3. 의존성 업데이트 후 동작이 달라졌을 때. 4. 캐시 관련 비결정적 실패가 의심될 때. 5. 새로운 환경에 배포하기 전에 호환성을 확인할 때. |
| **Limitations** | 1. 환경 차이가 원인인지 확정하려면 클린 환경에서의 재현이 필요하다. 2. 모든 환경 변수를 열거하는 것은 현실적으로 불가능하다. 3. 환경 문제와 코드 문제가 혼합된 경우 분리가 어렵다. |
| **Input Requirements** | 1. 성공 환경의 환경 정보 (runtime version, OS, env vars, dependency versions). 2. 실패 환경의 환경 정보. 3. Lock file diff (있는 경우). 4. 캐시/임시 파일 상태. |
| **Output Format** | `verdict: ENVIRONMENT_MATCH / ENVIRONMENT_MISMATCH`, `differences[]`, `impact`, `recommendation` |
| **Connected Failure Types** | Primary: FT-11 / Secondary: FT-05, FT-07 |

---

## Oracle Selection Guide

상황별 오라클 빠른 조회표.

| 상황 | 1st Oracle | 2nd Oracle | 3rd Oracle |
|------|------------|------------|------------|
| 코드 수정 직후 기본 확인 | OR-03 | OR-02 | OR-01 |
| 구현이 요구사항에 맞는지 확인 | OR-01 | OR-05 | -- |
| 출력 포맷이 올바른지 확인 | OR-02 | -- | -- |
| 수정 전후 동작 비교 | OR-05 | OR-06 | -- |
| 기대 출력을 모를 때 | OR-06 | OR-07 | OR-08 |
| 도메인 용어/규칙 확인 | OR-08 | OR-01 | -- |
| 디버깅 정보가 부족할 때 | OR-04 | -- | -- |
| 환경 문제가 의심될 때 | OR-09 | OR-03 | -- |
| 엣지 케이스 방어 확인 | OR-07 | OR-06 | -- |
| CI 파이프라인 실패 | OR-03 | OR-09 | OR-02 |
| 리팩터링 후 regression 확인 | OR-05 | OR-03 | OR-06 |
| 보안 속성 검증 | OR-07 | OR-08 | -- |
| 최종 릴리스 승인 | OR-01 | OR-08 | OR-05 |

---

## Strength Hierarchy

아래에서 위로 갈수록 강도가 높다. 충돌 시 강도가 높은 오라클의 판정을 우선한다.

```
[최상위 -- 가장 강력]

  OR-08: Human Oracle
    |  사람의 판정이 가장 강력. 일관성이 낮으므로
    |  판정 내용을 반드시 Knowledge Base에 기록.
    |
  OR-01: Spec Oracle
    |  명세에 명시된 내용은 추론보다 우선.
    |  명세 자체의 오류 가능성은 OR-08로 확인.
    |
  OR-06: Metamorphic Oracle
  OR-05: Comparison Oracle
    |  관계/비교 기반 판정은 실행 결과보다 신뢰도가 높다.
    |  (실행 성공해도 관계가 깨지면 결함)
    |
  OR-07: Negative Oracle
    |  "절대 안 되는 것"의 위반은 명확한 결함.
    |
  OR-02: Format Oracle
  OR-03: Execution Oracle
  OR-09: Environment Oracle
    |  기계적 검증으로 결과 명확.
    |  상위 오라클과 충돌하면 양보.
    |
  OR-04: Observability Oracle

[최하위 -- 보조적]
```

---

## Conflict Resolution Rules

### 충돌 해결 절차

1. **위계 규칙 적용:** 위계가 다르면 상위 오라클의 판정을 우선한다.

2. **동일 위계 충돌 시:**
   - 두 오라클의 입력 데이터가 동일한지 확인한다.
   - 입력이 다르면 올바른 입력으로 재판정한다.
   - 입력이 동일하면 OR-08로 에스컬레이션한다.

3. **OR-08 vs OR-01 충돌 시 (특수 케이스):**
   - 명세에 명시된 내용과 사람의 판정이 다르면 명세가 틀린 것일 수 있다.
   - **명세 오류**로 기록하고, 사람의 판정을 따르며, 명세를 갱신한다.
   - 갱신된 명세를 Knowledge Base에 등록한다.

4. **충돌 기록 의무:** 모든 충돌을 기록한다.
   ```yaml
   oracle_conflict:
     oracle_a: OR-XX (verdict: Y)
     oracle_b: OR-YY (verdict: Z)
     resolution: "[어떤 오라클의 판정을 따랐는지와 이유]"
     knowledge_base_update: "[갱신 내용]"
   ```

---

## Insufficient Oracle Protocol

### 판정 불가(Insufficient Oracle) 조건

다음 중 하나라도 해당하면 오라클이 불충분하다:
- 모든 적용 가능한 오라클이 INCONCLUSIVE를 반환했다.
- 적용 가능한 오라클이 존재하지 않는다 (입력 요건 미충족).
- 두 오라클이 모순되며 위계 규칙으로도 해결되지 않는다.

### 대응 프로토콜

```
1. [기록] 판정 불가 상황을 명시적으로 기록한다.
   insufficient_oracle:
     attempted: [시도한 오라클 목록]
     reason: "[각 오라클이 왜 판정 불가인지]"

2. [에스컬레이션] OR-08 (Human Oracle)로 에스컬레이션한다.
   - 무엇을 판정하려 했는지
   - 어떤 오라클을 시도했고 왜 실패했는지
   - LLM의 잠정 판단 (있으면)
   - 사람에게 어떤 정보/판정을 원하는지

3. [임시 조치] 에스컬레이션 응답 대기 중:
   - 해당 판정에 의존하는 작업을 BLOCKED로 표시한다.
   - 판정에 의존하지 않는 다른 작업은 계속 진행한다.
   - "확인 없이 진행"하지 않는다.

4. [Knowledge Base 갱신] 판정이 확정되면:
   - 확정된 규칙/판정을 Knowledge Base에 등록한다.
   - 향후 동일 상황에서 오라클 부재가 발생하지 않도록 한다.
```

---

## FT to OR Mapping Table

| Failure Type | Primary Oracle(s) | Secondary Oracle(s) | 비고 |
|--------------|-------------------|---------------------|------|
| **FT-01** Spec Gap | OR-01, OR-08 | -- | 명세에 없으면 OR-08로 에스컬레이션 |
| **FT-02** Hidden Semantic Rule Misfill | OR-01, OR-08 | OR-06, OR-05 | OR-08 핵심 -- 도메인 규칙은 사람만 확정 가능 |
| **FT-03** Input Normalization | OR-02 | OR-03 | 포맷 검증으로 대부분 감지 |
| **FT-04** Format/Structure Violation | OR-02 | OR-01 | 스키마 기반 기계적 검증 |
| **FT-05** Execution Failure | OR-03 | OR-04, OR-09 | 실행 후 환경 요인 배제 |
| **FT-06** Observability Failure | OR-04 | OR-03 | 메타 오라클 -- 다른 판정의 전제 조건 |
| **FT-07** External Verifier | OR-03 | OR-02, OR-09 | 검증기 에러와 환경 요인 분리 |
| **FT-08** Validation Shortfall | OR-05, OR-06 | OR-07 | 기존 테스트가 놓치는 것을 관계/비교로 감지 |
| **FT-09** Edge-Case Fragility | OR-07 | OR-06, OR-05 | 네거티브 조건으로 방어 확인 |
| **FT-10** Metamorphic Relation | OR-06 | OR-05 | 관계 검증이 본질 |
| **FT-11** Environment/Version/Cache | OR-09 | OR-03 | 환경 비교가 핵심 |
| **FT-12** Scope Violation | OR-01 | OR-05, OR-03 | 명세 범위와 실제 변경 범위 비교 |

---

## OR Quick-Lookup Table

| ID | Name | 판정 대상 | verdict 값 | 위계 등급 |
|----|------|----------|-----------|----------|
| OR-01 | Spec Oracle | 명세 vs 구현 일치 | PASS / FAIL / INCONCLUSIVE | 2 (상위) |
| OR-02 | Format Oracle | 포맷/스키마 준수 | PASS / FAIL | 4 (하위) |
| OR-03 | Execution Oracle | 실행 성공/실패 | PASS / FAIL / TIMEOUT | 4 (하위) |
| OR-04 | Observability Oracle | 관측 가능성 충분 여부 | SUFFICIENT / INSUFFICIENT | 5 (최하위) |
| OR-05 | Comparison Oracle | 두 결과 간 일치 | MATCH / MISMATCH / PARTIAL_MATCH | 3 (중간) |
| OR-06 | Metamorphic Oracle | 관계 불변량 성립 | RELATION_HOLDS / RELATION_VIOLATED | 3 (중간) |
| OR-07 | Negative Oracle | 금지 조건 부재 | CLEAN / VIOLATION | 3.5 (중간-하위) |
| OR-08 | Human Oracle | 도메인 전문가 판정 | CONFIRMED / REJECTED / MODIFIED | 1 (최상위) |
| OR-09 | Environment Oracle | 환경 일치 | ENVIRONMENT_MATCH / ENVIRONMENT_MISMATCH | 4 (하위) |

---

## Extension Rules

프로젝트별 오라클은 `.vkl/project/ORACLE_CATALOG.project.md`에 정의한다.

| 항목 | 규칙 |
|------|------|
| **ID 체계** | OR-P-XXX (OR-P-001부터 순차 부여) |
| **필수 필드** | base OR-XX의 모든 필드와 동일한 구조 사용 |
| **parent 참조** | 가장 가까운 base OR-XX를 parent로 명시 (예: `parent: OR-06`) |
| **위계 등급** | 반드시 parent OR-XX와 동일한 위계 등급을 상속하거나, 명시적 근거와 함께 조정 |
| **scope 선언** | 해당 프로젝트에서만 유효함을 선언 |
| **병합 금지** | project OR가 범용적이라 판단되면 `.vkl/proposals/`에 base 승격 제안서를 작성한다. 직접 이 문서를 수정하지 않는다. |

---

## Example: Blender Fracture -- Boundary/Thickness Axis Misfill

### 상황

Blender glass fracture 프로젝트에서 "outer boundary"가 "bounding box 전체 경계"로 오해석되어 두께 축이 경계 계산에 포함되었다. 실제로는 "가장 얇은 축 제외(exclude thinnest axis)" 규칙이 적용되어야 한다.

### 오라클 적용 기록

**OR-01 (Spec Oracle)** -- 부분적 감지
```yaml
oracle: OR-01
verdict: INCONCLUSIVE
spec_reference: "요구사항 문서"
deviation: "명세에 'outer boundary'의 정확한 정의가 없음"
confidence: low
```
OR-01은 "명세에 빈칸이 있다"는 것은 감지하지만, 어떻게 채워야 하는지는 판정 불가.

**OR-08 (Human Oracle)** -- 완전 감지 (사전 적용했다면)
```yaml
oracle: OR-08
verdict: CONFIRMED
question_asked: "'outer boundary'는 bbox 전체 축 경계입니까, 특정 축 제외 경계입니까?"
human_response: "가장 얇은 축(두께 방향)은 제외해야 합니다."
applied_rule: "exclude-thinnest-axis"
knowledge_base_update: true
```

**OR-06 (Metamorphic Oracle)** -- 사후 감지
```yaml
oracle: OR-06
verdict: RELATION_VIOLATED
metamorphic_relation: "MR-001: 유리 두께 변경 시 outer boundary 불변"
source_input: "thickness=2mm, width=100mm, height=150mm"
source_output: "boundary = (100, 150, 2) -- 두께 포함"
follow_up_input: "thickness=5mm, width=100mm, height=150mm"
follow_up_output: "boundary = (100, 150, 5) -- 두께 변경됨"
expected_relation: "boundary에 thickness 미포함이므로 출력 동일"
actual_relation: "boundary에 thickness 포함되어 출력 변경"
```

### 권장 오라클 적용 순서 (이 사례)

```
1. OR-01 -> "outer boundary" 정의 검색 -> 부재 감지 -> INCONCLUSIVE
2. OR-08 -> 도메인 전문가에게 질의 -> "exclude thinnest axis" 규칙 획득
3. OR-01 -> Knowledge Base 갱신 후 재적용 -> 규칙 위반 감지
4. OR-06 -> 두께 변경 불변량 검증 -> RELATION_HOLDS 확인
5. OR-05 -> 수정 전후 결과 비교 -> 변경이 의도 범위 내인지 확인
```

### 핵심 교훈

| 교훈 | 설명 |
|------|------|
| OR-01의 INCONCLUSIVE는 신호 | OR-01이 판정 불가하다는 것 자체가 FT-01/FT-02의 징후. OR-08로 에스컬레이션 필수. |
| OR-06는 골든 데이터 없이도 작동 | 올바른 boundary를 몰라도, "두께를 바꿔도 boundary 불변"이라는 관계로 검증 가능. |
| 도메인 용어는 항상 OR-08 선행 | "boundary", "edge", "surface" 같은 일상 용어가 도메인에서 특수한 의미를 가질 때 반드시 OR-08로 의미 확정 후 구현. |

---

*End of ORACLE_CATALOG.base.md*
