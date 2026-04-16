---
Document Role: Core Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# Test Relations (Base)

> **정답(ground truth)을 모를 때, 정답 대신 관계(relation)를 검증한다.**

이 문서는 정답 oracle 없이 결과를 판정할 수 있는 7가지 test relation 패턴을 정의한다. 각 relation에 고유 ID(MR-XX)를 부여하여 VALIDATION_CHECKLIST, FAILURE_TAXONOMY, ORACLE_CATALOG와 cross-reference 한다.

```
"출력이 맞는지"를 모를 때
"출력들 사이의 관계가 성립하는지"는 확인할 수 있다.
```

---

## MR-01: Permutation Invariance (순서 불변성)

### Pattern

입력 요소의 순서를 변경해도 출력이 동일하거나 동치(equivalent)여야 한다. 순서에 의존하지 않아야 하는 연산이 순서에 의존하면 구현 결함.

### Template

```
Relation ID: MR-01
Instance ID: MR-01-[PROJECT]-[NNN]
Given: 입력 집합 S = {s1, s2, ..., sn}
When: S의 임의 순열 S' = permute(S)를 생성
Then: f(S) == f(S')
Equivalence 기준: [출력의 어떤 속성이 동일해야 하는지 명시]
실패 시 의미: [순서 의존 결함의 구체적 형태]
```

### 적용 조건

**사용할 때:**
- 입력이 집합(set)이나 그래프 등 본질적으로 순서 없는 구조일 때
- "먼저 처리된 것이 유리한" 편향 의심 시
- 병렬 처리로 전환 시 결과 변화 관찰 시

**사용하지 않을 때:**
- 입력 순서가 의미를 가지는 시퀀스(시계열, 문장) 처리
- 순서에 따라 다른 결과가 정당하게 기대되는 연산 (예: 큐 처리)
- 순서 의존적 누적 연산 (예: 부동소수점 덧셈의 순서 의존성은 허용 범위일 수 있음)

### cross-reference

- 주 연결 checklist: VC-D-03 (Edge Case Coverage)
- 관련 failure type: FT-07 (non-deterministic behavior)
- 관련 oracle: OR-06 (Metamorphic Oracle)

---

## MR-02: Monotonicity (단조성)

### Pattern

입력의 특정 차원이 증가할 때 출력의 특정 차원이 감소하지 않아야 한다(또는 그 역). 물리적/논리적 단조 관계가 성립해야 하는 곳에서 비단조 동작이 나타나면 결함.

### Template

```
Relation ID: MR-02
Instance ID: MR-02-[PROJECT]-[NNN]
Given: 입력 x1, x2 where x1.dimension_A < x2.dimension_A
       (나머지 모든 입력 차원 동일)
When: f(x1), f(x2)를 각각 계산
Then: f(x1).dimension_B <= f(x2).dimension_B  (non-decreasing)
      OR f(x1).dimension_B >= f(x2).dimension_B  (non-increasing)
방향: [증가/감소 중 어느 방향인지 명시]
예외 허용: [plateau, saturation 등 허용 조건 명시]
실패 시 의미: [비단조 동작의 구체적 의미]
```

### 적용 조건

**사용할 때:**
- 물리적 인과관계가 존재하는 입출력 쌍일 때
- "더 넣으면 더 나와야" 하는 직관이 있는 시스템
- 비선형 동작이 예상되더라도 구간별 단조성 검증 가능 시

**사용하지 않을 때:**
- 입출력 관계가 비단조적인 것이 정당한 경우 (예: U자 곡선, 공진 주파수)
- 입력 차원이 출력에 영향을 주지 않는 것이 정상인 경우
- threshold 기반 동작에서 threshold 전후 불연속이 예상되는 경우

### cross-reference

- 주 연결 checklist: VC-D-03 (Edge Case Coverage), VC-D-01 (Functional Correctness)
- 관련 failure type: FT-04 (incorrect computation)
- 관련 oracle: OR-06 (Metamorphic Oracle)

---

## MR-03: Round-Trip Consistency (왕복 일관성)

### Pattern

변환(transform) 후 역변환(inverse transform) 적용 시 원래 상태로 복원. 완전 복원 불가 시 손실 범위가 정의된 tolerance 내여야 한다.

### Template

```
Relation ID: MR-03
Instance ID: MR-03-[PROJECT]-[NNN]
Given: 원본 데이터 D
When: D' = transform(D), D'' = inverse_transform(D')
Then: distance(D, D'') <= tolerance
Tolerance 정의: [구체적 수치 또는 조건]
손실 허용 여부: [lossy / lossless]
distance 함수: [비교에 사용할 거리/동치 함수 명시]
실패 시 의미: [왕복 실패의 구체적 형태]
```

### 적용 조건

**사용할 때:**
- 데이터 변환/직렬화가 관여되는 모든 곳
- 두 시스템 간 데이터 교환이 있을 때
- "저장했다 불러오면 달라져요" 류의 버그 의심 시
- export/import, encode/decode, serialize/deserialize 쌍이 존재할 때

**사용하지 않을 때:**
- 의도적으로 비가역적인 변환 (예: 해시 함수, 손실 압축에서 복원 불필요 시)
- 역변환 자체가 정의되지 않은 연산
- 변환 과정에서 의도적 정보 손실이 설계에 포함된 경우 (이 경우 tolerance를 넓게 설정하여 활용 가능)

### cross-reference

- 주 연결 checklist: VC-D-01 (Functional Correctness), VC-D-07 (Integration Impact)
- 관련 failure type: FT-05 (data loss), FT-06 (integration failure)
- 관련 oracle: OR-05 (Comparison Oracle), OR-06 (Metamorphic Oracle)

---

## MR-04: Representation Invariance (표현 불변성)

### Pattern

동일한 의미를 다른 형식/단위/표기법으로 표현해도 결과가 동일해야 한다. 표현 방식에 의존하는 로직은 본질이 아닌 우연에 의존하는 것.

### Template

```
Relation ID: MR-04
Instance ID: MR-04-[PROJECT]-[NNN]
Given: 동일 의미를 가진 두 표현 R1, R2
       (R1과 R2는 구문적으로 다르지만 의미적으로 동치)
When: f(R1), f(R2)를 각각 계산
Then: f(R1) == f(R2)
동치 정의: [의미적 동치의 기준]
표현 차이: [R1과 R2가 어떻게 다른지 명시]
실패 시 의미: [표현 의존 결함의 구체적 형태]
```

### 적용 조건

**사용할 때:**
- 입력에 여러 표현 방식 존재 시 (단위, 좌표계, 인코딩 등)
- "필드 이름을 바꿨더니 결과가 달라졌다" 류의 버그
- 하드코딩된 문자열 비교가 로직에 사용되고 있을 때
- 다국어, 다중 인코딩, 다중 API 버전 지원 시

**사용하지 않을 때:**
- 표현 차이가 의미 차이를 수반하는 경우 (예: little-endian vs big-endian이 프로토콜 스펙에 의해 다른 의미)
- 정규화(canonicalization)가 명시적으로 사용자 책임인 경우

### cross-reference

- 주 연결 checklist: VC-D-05 (Hidden Semantic Rule Detection)
- 관련 failure type: FT-02 (hidden semantic rule misfill), FT-08 (encoding error)
- 관련 oracle: OR-06 (Metamorphic Oracle)

---

## MR-05: Negative Oracle (네거티브 오라클)

### Pattern

특정 조건이 성립하면 반드시 실패하거나 특정 결과가 나와야 한다. "이 입력에서 성공하면 오히려 버그"인 경우를 정의. 시스템의 경계와 제약 검증.

### Template

```
Relation ID: MR-05
Instance ID: MR-05-[PROJECT]-[NNN]
Given: 의도적 위반 조건 C (정상 입력의 전제 조건을 위반)
When: f(C)를 실행
Then: 결과가 반드시 [실패 / 에러 코드 X / 특정 값]
      AND 결과가 [정상 성공 / 임의 값]이면 FAIL
위반 조건 분류: [precondition violation / boundary violation / type violation / state violation]
기대 실패 모드: [구체적 에러 타입 또는 반환값]
실패 시 의미: [정상 처리가 왜 버그인지]
```

### 적용 조건

**사용할 때:**
- 입력의 전제 조건(precondition)이 명시되어 있을 때
- "이건 당연히 안 되어야 하는데" 하는 경우
- 에러 처리와 경계 조건의 견고함 검증
- positive test만으로는 시스템 한계 파악 불가 시

**사용하지 않을 때:**
- 시스템이 의도적으로 모든 입력을 허용하는 설계 (graceful degradation)
- 실패 조건 자체가 모호하여 정의할 수 없을 때
- 실패 모드가 환경에 따라 달라져야 하는 경우 (이 경우 환경별로 분리 정의)

### cross-reference

- 주 연결 checklist: VC-D-03 (Edge Case Coverage), VC-D-01 (Functional Correctness)
- 관련 failure type: FT-03 (missing error handling), FT-09 (boundary condition failure)
- 관련 oracle: OR-07 (Negative Oracle)

---

## MR-06: Property/Invariant (속성/불변식)

### Pattern

입력과 무관하게 항상 성립해야 하는 속성. 어떤 입력이든 이 속성이 깨지면 결함. 정답을 몰라도 정답이 반드시 만족해야 하는 제약 검사 가능.

### Template

```
Relation ID: MR-06
Instance ID: MR-06-[PROJECT]-[NNN]
For all: 유효한 입력 x in Domain
Invariant: P(f(x)) == true
Property 정의: [속성의 수학적/논리적 표현]
위반 시 의미: [이 속성 위반이 어떤 결함인지]
```

### 일반 불변식 템플릿 3종

#### Template A: Conservation (보존 법칙)

```
MR-06-A: Conservation
For all: 유효한 입력 x
Invariant: measure(input) ~= measure(output)  (tolerance 내)
measure 정의: [보존되어야 하는 양 -- 개수, 합계, 질량, 에너지 등]
tolerance: [허용 오차]
적용 예: 이미지 리사이즈(픽셀 총 수 비례), 데이터 파이프라인(레코드 수 보존),
         물리 시뮬레이션(에너지 보존)
```

#### Template B: Bounds (경계 조건)

```
MR-06-B: Bounds
For all: 유효한 입력 x
Invariant: lower_bound <= f(x) <= upper_bound
bound 정의: [하한과 상한의 근거]
적용 예: 확률([0, 1]), 퍼센트([0, 100]), 정규화([-1, 1]),
         인덱스([0, length-1])
```

#### Template C: Structural Consistency (구조적 일관성)

```
MR-06-C: Structural Consistency
For all: 유효한 입력 x
Invariant: structural_check(f(x)) == true
check 정의: [구조적 속성의 구체적 조건]
적용 예: 트리(사이클 없음), 그래프(연결 보장), 리스트(중복 없음),
         JSON(valid parse), 참조(dangling reference 없음)
```

### 적용 조건

**사용할 때:**
- 도메인에 물리 법칙, 수학적 제약, 비즈니스 규칙 등 불변 조건 존재 시
- 정답은 모르지만 "이건 반드시 성립해야 한다"는 조건이 있을 때
- 다양한 입력에 대해 일괄 검증 필요 시
- regression test 자동화 기초로 사용 시

**사용하지 않을 때:**
- 불변식 자체가 조건부인 경우 (이 경우 조건을 명시하여 conditional invariant로 전환)
- 속성이 "대부분 성립"하지만 예외가 정당한 경우 (예외 목록을 명시하여 활용 가능)

### cross-reference

- 주 연결 checklist: VC-D-01 (Functional Correctness), VC-D-03 (Edge Case Coverage)
- 관련 failure type: FT-04 (incorrect computation), FT-10 (invariant violation)
- 관련 oracle: OR-06 (Metamorphic Oracle), OR-07 (Negative Oracle)

---

## MR-07: Minimal Counterexample Logging

### 목적

relation 실패 시 재현과 원인 분석을 위한 최소 반례(minimal counterexample) 기록. 반례 기록 없이 "실패했다"만 보고하면 디버깅 불가.

### 기록해야 할 조건

- MR-01 ~ MR-06 중 하나 이상이 fail했을 때
- fail 원인이 불분명하여 가설 수립이 필요할 때
- 동일 relation이 다른 입력에서도 fail하여 패턴 파악이 필요할 때

### Required Fields

```yaml
counterexample:
  id: "CE-[relation_id]-[순번]"
  timestamp: "YYYY-MM-DD HH:MM:SS"
  relation: "[relation ID, 예: MR-02-FRAC-001]"
  relation_type: "[permutation_invariance | monotonicity | round_trip |
                   representation_invariance | negative_oracle | property_invariant]"

  input:
    description: "[입력의 의미 설명]"
    value: "[구체적 입력값 -- 재현 가능해야 함]"
    minimized: "[yes / no / in_progress]"

  expected_output:
    description: "[relation에 의해 기대되는 출력 또는 조건]"
    value: "[구체적 기대값 또는 조건식]"

  actual_output:
    description: "[실제 관찰된 출력]"
    value: "[구체적 실제값]"

  diff:
    type: "[value_mismatch | condition_violation | exception | timeout]"
    detail: "[expected vs actual의 구체적 차이]"
    magnitude: "[차이의 크기 -- 수치인 경우]"

  hypothesis:
    primary: "[가장 유력한 원인 가설]"
    alternatives:
      - "[대안 가설 1]"
      - "[대안 가설 2]"
    evidence_for: "[가설을 지지하는 관찰]"
    evidence_against: "[가설에 반하는 관찰]"

  context:
    loop_id: "[현재 검증 루프 ID]"
    environment: "[OS, 버전, 관련 설정]"
    related_counterexamples: ["CE-xxx", "CE-yyy"]
    related_knowledge_assets: ["KA-xxx"]

  minimization_notes: |
    [입력 최소화 시도 기록.
     어디까지 줄였을 때 여전히 실패하는지,
     어디부터 성공으로 바뀌는지 기록]
```

### 최소화(Minimization) 절차

```
1. 현재 실패 입력을 기록.
2. 입력 요소를 하나씩 제거하며 여전히 실패하는지 확인.
3. 더 이상 제거할 수 없으면 (제거 시 성공) 최소 반례 확정.
4. minimized: yes로 표시.
5. 제거된 요소 중 실패에 기여하지 않는 것들을 기록 (noise reduction).
```

---

## Selection Guide: 어떤 Relation을 먼저 시도할 것인가

### Decision Tree

```
시스템에 대해 무엇을 알고 있는가?
|
+-- "입력-출력 쌍이 있다" (정답은 모름)
|   +-- 입력이 집합/순서 무관 -> MR-01 (Permutation Invariance) 먼저
|   +-- 입력에 크기/양 차원 있음 -> MR-02 (Monotonicity) 먼저
|   +-- 변환-역변환 가능 -> MR-03 (Round-Trip) 먼저
|
+-- "도메인 규칙/제약을 알고 있다"
|   +-- "이건 반드시 성립" -> MR-06 (Property/Invariant) 먼저
|   +-- "이건 반드시 실패" -> MR-05 (Negative Oracle) 먼저
|   +-- "같은 의미, 다른 표현" -> MR-04 (Representation Invariance) 먼저
|
+-- "아무것도 모른다"
|   +-- Step 1: MR-06 기본 Property 탐색 (출력 범위, null 아님, 타입 등)
|   +-- Step 2: MR-05 명백한 Negative Oracle (빈 입력, 0, null)
|   +-- Step 3: MR-01 Permutation Invariance (가장 넓은 적용 범위)
|
+-- "이전 루프에서 특정 relation 실패"
    +-- 실패 counterexample 분석 -> 관련 relation 추가 실행
    +-- MR-07로 기록 후 hypothesis 기반 다음 relation 선택
```

### Priority Matrix

| 상황 | 1순위 | 2순위 | 3순위 |
|------|-------|-------|-------|
| 새 기능, 스펙 불완전 | MR-06 Property | MR-05 Negative | MR-02 Monotonicity |
| 버그 재현 시도 | MR-05 Negative | MR-04 Representation | MR-01 Permutation |
| 리팩토링 후 회귀 | MR-03 Round-Trip | MR-01 Permutation | MR-06 Property |
| 데이터 파이프라인 | MR-03 Round-Trip | MR-06 Property (보존) | MR-02 Monotonicity |
| 물리 시뮬레이션 | MR-02 Monotonicity | MR-06 Property | MR-05 Negative |
| 다중 입력 형식 | MR-04 Representation | MR-01 Permutation | MR-03 Round-Trip |

---

## Metamorphic Testing vs. Property-Based Testing

### 분류

- **Property-Based Testing**: 단일 실행 출력이 특정 속성 만족 여부 검사. `f(x)` 결과 자체를 검증. MR-05 (Negative Oracle), MR-06 (Property/Invariant) 해당.
- **Metamorphic Testing**: 두 실행 출력 사이 관계 검사. `f(x)`와 `f(x')`의 관계 검증. MR-01 (Permutation), MR-02 (Monotonicity), MR-03 (Round-Trip), MR-04 (Representation) 해당.

### 선택 기준

| Metamorphic이 유용한 경우 | Property-Based가 유용한 경우 |
|--------------------------|---------------------------|
| 정답을 전혀 모를 때 | 출력 범위/타입은 알 때 |
| "같은 의미 다른 표현" 상황 | "이건 절대 안 됨" 상황 |
| 입력 변형이 쉬울 때 | 불변 조건이 명확할 때 |
| 비결정적 시스템 | 결정적 시스템 |
| 출력이 복잡한 구조 (비교 기준 정의 어려움) | 출력이 단순한 값 (범위 검사 용이) |
| 기존 테스트 없고 탐색적 단계 | 기존 테스트 있고 보강 단계 |

### 병행 사용 절차

```
Step 1: Property-Based로 기본 불변식 확립 (범위, 타입, null 아님 등)
Step 2: Metamorphic으로 관계 기반 검증 추가 (순서 불변, 단조성 등)
Step 3: 실패 발견 시 Negative Oracle로 경계 조건 강화
Step 4: 모든 실패를 MR-07 counterexample로 기록
```

---

## Failed Relation -> Knowledge Asset 변환 절차

relation 실패는 단순한 테스트 실패가 아니다. 시스템에 대한 새로운 발견이다. 반드시 knowledge asset으로 환류시킨다.

### 변환 흐름

```
1. Counterexample 기록 (MR-07 형식)
   |
2. Hypothesis 검증
   |  가설 확인 ---------------------------+
   |  가설 기각 -> 새 가설 수립 -> 1로 복귀  |
   |                                      |
3. 발견 사실 분류 <------------------------+
   |
   +-- 새 규칙 발견 -> type: "discovered_rule"
   +-- 새 불변식 발견 -> type: "invariant"
   |   -> VALIDATION_CHECKLIST의 verification 항목 추가 가능
   +-- Spec gap 발견 -> type: "spec_gap"
   |   -> VALIDATION_CHECKLIST의 VC-D-06 escalation 트리거
   +-- 기존 규칙 수정 -> type: "rule_revision"
   |   -> 기존 KA의 revision history에 추가
   |
4. Knowledge Asset 기록 (runtime 형식 -- 예: `case_logs/CL-YYYY-MM-DD-NNN.md`, `observations/OBS-YYYY-MM-DD-NNN.md`)
   |
5. Checklist 업데이트
   |
   +-- 새 verification 항목 추가 가능? -> VC-V-XX 또는 VC-P-V-XXX 추가
   +-- 새 validation 항목 추가 필요? -> VC-D-XX 또는 VC-P-D-XXX 추가
   +-- 기존 항목 강화 필요? -> 해당 항목 기준 구체화
   |
6. Test Relation 추가
   |
   +-- 발견 사실에서 새 relation 도출
       대상 위치: .vkl/runtime/ (즉시 적용) 또는 .vkl/proposals/ (검토 후 적용)
```

### 변환 Template

```yaml
# Counterexample -> Knowledge Asset 변환
source_counterexample: "CE-[id]"
discovered_at: "YYYY-MM-DD"

knowledge_asset:
  id: "KA-[PROJECT]-[순번]"
  type: "[discovered_rule | invariant | spec_gap | rule_revision]"
  content: "[발견된 사실의 명확한 서술]"
  failure_type: "[FT-01 ~ FT-12]"
  oracle_needed: "[OR-01 ~ OR-09]"
  confidence: "[hypothesis | tested | confirmed]"

checklist_update:
  new_verification_items: ["VC-V-XX 또는 VC-P-V-XXX: 설명"]
  new_validation_items: ["VC-D-XX 또는 VC-P-D-XXX: 설명"]
  modified_items: ["VC-V-YY: 수정 내용"]

new_test_relations:
  - id: "[MR-XX-PROJECT-NNN 또는 MR-P-XXX]"
    type: "[relation type]"
    description: "[새 relation 설명]"
    derived_from: "CE-[id]"
```

### 환류 대상 결정 기준

```
즉시 적용 (.vkl/runtime/):
  - confidence가 confirmed이고, 기존 규칙을 깨뜨리지 않을 때
  - 단순 불변식 추가 (MR-06 유형)
  - negative oracle 추가 (MR-05 유형)

검토 후 적용 (.vkl/proposals/):
  - 기존 규칙과 충돌 가능성이 있을 때
  - core 문서 (이 문서 포함)의 수정이 필요할 때
  - confidence가 hypothesis 또는 tested 단계일 때
```

---

## 확장 규칙

```
프로젝트별 relation:
  - 프로젝트 고유 relation: MR-P-XXX (예: MR-P-001)
  - 정의 위치: .vkl/project/TEST_RELATIONS.project.md

ID 할당 규칙:
  - core relation (MR-XX): 이 문서에서만 정의. 최대 99개.
  - project relation (MR-P-XXX): 프로젝트 문서에서 정의. 최대 999개.
  - core relation은 프로젝트에서 재정의 불가. 인스턴스화만 가능.

인스턴스 ID 규칙:
  - core relation의 프로젝트 적용: MR-XX-[PROJECT]-[NNN]
    예: MR-01-FRAC-001 (Blender Fracture 프로젝트의 순서 불변성 테스트 #1)
  - project relation의 인스턴스: MR-P-XXX-[NNN]
    예: MR-P-001-001

cross-reference 규칙:
  - 모든 MR-XX는 VALIDATION_CHECKLIST의 VC-D-XX와 N:M 매핑.
  - 모든 MR-XX는 ORACLE_CATALOG의 OR-XX와 N:M 매핑.
  - 모든 counterexample(CE-XX)은 FAILURE_TAXONOMY의 FT-XX와 1:N 매핑.

승격 절차:
  - project relation이 3개 이상 프로젝트에서 반복되면 core 승격 제안 가능.
  - 제안은 .vkl/proposals/에 작성. 인간 승인 후 이 문서에 반영.
```

---

## Quick Reference: 7 Relations at a Glance

| ID | Relation | 핵심 질문 | 정답 필요 여부 | 유형 |
|----|----------|----------|--------------|------|
| MR-01 | Permutation Invariance | 순서 바꿔도 같은가? | 불필요 | Metamorphic |
| MR-02 | Monotonicity | 더 넣으면 더 나오는가? | 불필요 | Metamorphic |
| MR-03 | Round-Trip Consistency | 갔다 오면 원래대로인가? | 불필요 | Metamorphic |
| MR-04 | Representation Invariance | 다르게 써도 같은 결과인가? | 불필요 | Metamorphic |
| MR-05 | Negative Oracle | 이건 반드시 실패하는가? | 실패 조건만 필요 | Property-Based |
| MR-06 | Property/Invariant | 항상 성립하는 것이 성립하는가? | 속성만 필요 | Property-Based |
| MR-07 | Counterexample Log | 실패를 재현/최소화할 수 있는가? | (기록 규칙) | Meta |

---

## Appendix A: Blender Fracture Case -- boundary/thickness axis misfill

### 배경

Blender fracture 프로젝트에서 "outer boundary" 용어의 의미가 단계별로 잘못 채워진(misfill) 사례. test relation이 각 stage에서 어떻게 문제를 탐지할 수 있었는지 보여준다.

### 적용 가능했던 relation

| Relation | 적용 방법 | 탐지 가능 시점 |
|----------|----------|--------------|
| MR-01 Permutation | fracture 조각 순서 변경 후 boundary 분류 비교 | Stage 1 -- 조각 순서에 따라 boundary 수 변동 시 즉시 탐지 |
| MR-02 Monotonicity | impact force 증가 시 fragment count non-decreasing 확인 | Stage 1 -- thickness axis 오해석으로 비단조 동작 나타남 |
| MR-04 Representation | dimension을 (x,y,z) vs (z,y,x) 순서로 전달 비교 | Stage 1 -- 배열 인덱스 하드코딩으로 결과 불일치 즉시 발견 |
| MR-05 Negative | 세 축이 동일한 정육면체에서 thinnest axis 판정 | Stage 1 -- ambiguous 반환 대신 임의 축 선택 시 결함 확인 |
| MR-06 Property | 전체 조각 volume 합 == 원본 volume (보존 법칙) | Stage 2 -- boundary 오분류로 volume 계산 오류 탐지 |

### 핵심 counterexample

```yaml
counterexample:
  id: "CE-MR-04-FRAC-001-01"
  timestamp: "2025-03-15 14:22:00"
  relation: "MR-04-FRAC-001"
  relation_type: "representation_invariance"

  input:
    description: "동일 오브젝트 dimension을 (x,y,z)와 (z,y,x) 순서로 전달"
    value:
      R1: "(x=0.5, y=2.0, z=0.1)"
      R2: "(z=0.1, y=2.0, x=0.5)"
    minimized: "yes -- 3축 모두 다른 값이 최소 재현 조건"

  expected_output:
    description: "두 입력 모두 동일한 물리적 축(가장 얇은 축) 식별"
    value: "R1: z축 (0.1), R2: z축 (0.1) -> 동일"

  actual_output:
    description: "R2에서 다른 축을 가장 얇은 축으로 식별"
    value: "R1: z축 (0.1), R2: x축 (0.1이지만 배열 index 0)"

  diff:
    type: "value_mismatch"
    detail: "R1은 index[2]=0.1을 최솟값으로 정확히 식별. R2는 index[0]=0.1을 식별했으나
             'x축'으로 labeling. 물리적으로는 z축인데 배열 위치 기준으로 x축이라 판정"
    magnitude: "축 식별 자체가 틀림 -- critical"

  hypothesis:
    primary: "detect_thinnest_axis()가 배열 인덱스를 축 이름으로 하드코딩.
              index[0]='x', index[1]='y', index[2]='z' 매핑. 물리적 축 미확인"
    alternatives:
      - "입력 파서가 (z,y,x) 순서를 (x,y,z)로 재해석하여 값 순서 뒤바뀜"
      - "축 이름이 아닌 축 인덱스가 내부 사용되는데 인덱스 재매핑 미수행"
    evidence_for: "소스에서 axis_names = ['x','y','z'] 하드코딩, 입력 순서 무관 index 접근"
    evidence_against: "아직 없음 -- 소스 확인 필요"

  context:
    loop_id: "fracture-boundary-loop-003"
    environment: "Blender 3.6, Python 3.10"
    related_counterexamples: []
    related_knowledge_assets: ["KA-BLENDER-001"]

  minimization_notes: |
    - 3축 모두 다른 값(0.1, 0.5, 2.0)이 최소 재현 조건
    - 2축 동일 시(0.1, 0.1, 2.0) 다른 relation(MR-05)과 겹침
    - 값 자체보다 "순서 swap"이 핵심 트리거
```

### 교훈

```
1. MR-04 (Representation Invariance) 한 번이면 Stage 1에서 즉시 발견 가능했다.
   dimension 표현 순서를 바꿔 넣기만 했어도 축 식별 결함이 드러남.

2. 동일 용어의 해석이 stage마다 변경된 것 자체가 MR-04 위반 신호.
   "동일 용어 해석 변경 = spec gap" -> FT-01 분류 -> VC-D-06 escalation.

3. 이 counterexample에서 생성된 knowledge asset (KA-BLENDER-001):
   boundary/edge/face 등 기하학적 용어 등장 시 즉시 FT-01 분류.
   자체 해석 금지. 인간에게 정의 요청.

4. 새로 추가되어야 할 relation:
   MR-06-FRAC-005: "axis label과 axis index의 매핑이 일관적이어야 함"
   (이 counterexample에서 도출)
```
