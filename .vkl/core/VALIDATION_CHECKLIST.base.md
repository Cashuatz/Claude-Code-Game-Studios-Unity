---
Document Role: Core Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# Validation Checklist (Base)

> **verification 통과는 validation 통과를 의미하지 않는다.**

이 문서는 verification(기계적 확인)과 validation(의미적 확인)을 구조적으로 분리하며, 각 항목에 고유 ID를 부여하여 추적 가능하게 한다. 모든 판정에는 오라클 근거가 필수다.

```
           +--------------------------+
           |      검증 결과물          |
           +------------+-------------+
                        |
           +------------+-------------+
           |                          |
  +--------v---------+    +-----------v---------+
  | VERIFICATION     |    | VALIDATION          |
  | VC-V-XX          |    | VC-D-XX             |
  |                  |    |                     |
  | 판정: Claude     |    | 판정: Human+Oracle  |
  | 근거: 명시 규칙  |    | 근거: 의도/도메인   |
  | 자동화: 가능     |    | 자동화: 불가        |
  +------------------+    +---------------------+
           |                          |
           | 통과해도 아래가 실패 가능  |
           +--------------------------+
```

---

## Section 1: Verification Checklist (VC-V-XX)

Claude가 oracle 참조만으로 독립 판정할 수 있는 항목. 인간 개입 없이 pass/fail 결정.

| ID | 항목 | pass/fail 기준 | 사용 오라클 | 자동화 가능 여부 |
|----|------|---------------|-----------|---------------|
| VC-V-01 | **Format Compliance** | 출력이 선언된 스키마(JSON, YAML 등)에 대해 파싱 에러 0건. 필수 필드 누락 0건. 구조 depth가 스키마와 일치 | OR-02 (Format Oracle) | 가능 -- schema validator 실행 |
| VC-V-02 | **Execution Success** | 빌드/파싱/런타임 실행이 exit code 0 완료. stderr에 error-level 메시지 0건 | OR-03 (Execution Oracle) | 가능 -- CI/빌드 스크립트 실행 |
| VC-V-03 | **Log/Artifact Presence** | 요구된 로그 파일이 존재하고 비어있지 않음. 중간 산출물(intermediate artifact)이 선언된 경로에 존재 | OR-04 (Observability Oracle) | 가능 -- 파일 존재/크기 검사 |
| VC-V-04 | **Changed File Scope Match** | 변경된 파일 목록이 선언된 scope와 정확히 일치. scope 외 파일의 diff가 0건 | OR-01 (Spec Oracle) | 가능 -- git diff --name-only 비교 |
| VC-V-05 | **No Scope Violation** | 선언된 scope 밖의 코드, 설정, 리소스에 대한 변경/삭제/추가가 0건. FT-12(scope violation/over-edit) 해당 변경 없음 | OR-01 (Spec Oracle) | 가능 -- diff 범위 검사 |
| VC-V-06 | **Output Contract Compliance** | OUTPUT_CONTRACT에 정의된 9개 section이 모두 존재. 각 section에 최소 1개 항목 기재 | OR-02 (Format Oracle) | 가능 -- section heading 파싱 |
| VC-V-07 | **Failure Taxonomy Classification Present** | 발견된 모든 실패에 FT-01~FT-12 중 하나 이상의 분류 코드 기재. 미분류 실패 0건 | OR-02 (Format Oracle) + FAILURE_TAXONOMY | 가능 -- 코드 패턴 매칭 |
| VC-V-08 | **Oracle Citation Present** | 모든 판정(pass/fail/skip)에 사용된 oracle이 OR-01~OR-09 중 하나로 명시. oracle 없는 판정 0건 | OR-02 (Format Oracle) + ORACLE_CATALOG | 가능 -- 필드 존재 검사 |
| VC-V-09 | **Next Action = Exactly 1 Small Loop** | "다음 액션" 항목이 정확히 1개. 해당 액션이 현재 루프보다 작거나 같은 scope. 복수 액션 또는 scope 확대 시 fail | OR-01 (Spec Oracle) + ROLE_AND_RULES (loop scope 원칙) | 부분 가능 -- 액션 수 자동, scope 비교 수동 |
| VC-V-10 | **No Free-Form Approval Language** | "looks good", "seems correct", "should work", "아마 괜찮을 것" 등 근거 없는 승인 표현 0건. 모든 긍정 판정에 oracle 참조 동반 | OR-02 (Format Oracle) | 가능 -- 금지 패턴 regex 매칭 |
| VC-V-11 | **Knowledge Asset Recorded** | 루프에서 발견된 새로운 규칙, 가설, spec gap이 runtime 기록 형식(예: `case_logs/CL-YYYY-MM-DD-NNN.md`, `observations/OBS-YYYY-MM-DD-NNN.md`, `temporary_hypotheses/HYP-YYYY-MM-DD-NNN.md`)으로 기록됨. 발견 사항 있는데 기록 없으면 fail | OR-04 (Observability Oracle) | 부분 가능 -- 존재 검사 자동, 완전성 수동 |
| VC-V-12 | **Failure Type - Cause Consistency** | 분류된 failure type 정의와 실제 기술된 원인 일치. FT-01로 분류했는데 원인이 실행 에러이면 fail | FAILURE_TAXONOMY 정의 대조 | 부분 가능 -- 정의 키워드 매칭 자동, 의미 일치 수동 |

---

## Section 2: Validation Checklist (VC-D-XX)

인간 판단 또는 외부 oracle이 필요한 항목. Claude가 단독으로 pass를 선언할 수 없다.

| ID | 항목 | pass/fail 기준 | 필요 오라클 | escalation 조건 |
|----|------|---------------|-----------|----------------|
| VC-D-01 | **Functional Correctness** | 구현 결과가 요구된 기능을 실제로 수행하는가. 에러 없이 실행되는 것이 아니라 의도된 동작을 하는가 | OR-08 (Human Oracle) 또는 OR-05 (Comparison Oracle -- 레퍼런스 구현 존재 시) | VC-V-02 통과했으나 실제 동작이 요구사항과 다를 때. 레퍼런스 구현/기대 출력 부재 시 |
| VC-D-02 | **Intent Alignment** | 결과가 사용자의 실제 의도와 일치하는가. literal spec 충족과 intent 충족은 별개 | OR-08 (Human Oracle) | spec을 문자 그대로 충족하지만 의도와 다를 때. 암묵적 기대가 명시되지 않았을 때 |
| VC-D-03 | **Edge Case Coverage** | 경계 조건(빈 입력, 최대값, 0, 음수, 동시성 등) 처리 여부 | OR-06 (Metamorphic Oracle) + OR-07 (Negative Oracle) | 핵심 경로만 테스트되고 경계 조건 테스트 0건일 때. 도메인 알려진 edge case 미커버 시 |
| VC-D-04 | **Quality Perception** | 사용자가 결과물 품질 수준을 수용할 수 있는가. 기능적으로 맞더라도 UX, 성능, 코드 품질이 기대 이하일 수 있음 | OR-08 (Human Oracle) | "맞긴 한데 이 정도면 안 됨" 수준일 때. 비기능 요구사항 미명시 시 |
| VC-D-05 | **Hidden Semantic Rule Detection** | 스펙에 명시되지 않은 의미 규칙이 존재하는데 Claude가 자체적으로 채운 곳이 있는가. misfill 여부 탐지 | OR-01 (Spec Oracle) + OR-08 (Human Oracle) | 스펙에 정의되지 않은 용어/개념이 구현에 사용되었을 때. 도메인 전문가 확인 없이 의미 판단 시. **FT-02 (hidden semantic rule misfill) 의심 시 즉시 escalation** |
| VC-D-06 | **Spec Gap Identification** | 스펙에 빈칸(미정의, 모호, 모순)을 발견하고 명시적으로 표시했는가. 빈칸을 조용히 채우지 않았는가 | OR-01 (Spec Oracle) + OR-08 (Human Oracle) | spec gap 발견되었으나 미보고 시. Claude가 gap을 자체 해석한 흔적 있을 때. **FT-01 (spec gap) 해당 시 즉시 escalation** |
| VC-D-07 | **Integration Impact** | 이번 변경이 다른 모듈, 시스템, 워크플로우에 영향을 주지 않는가 | OR-05 (Comparison Oracle -- 변경 전/후) + OR-09 (Environment Oracle) | 변경 scope가 단일 파일을 넘을 때. 공유 인터페이스, 글로벌 상태, 설정 파일 수정 시 |
| VC-D-08 | **Performance Acceptability** | 실행 시간, 메모리 사용, 응답 속도가 수용 가능 범위인가 | OR-03 (Execution Oracle -- 벤치마크) + OR-08 (Human Oracle -- 기준 정의) | 성능 요구사항이 명시되어 있으나 미측정 시. 이전 대비 성능 저하 관찰 시 |
| VC-D-09 | **Maintainability** | 코드가 향후 수정/확장 가능한 구조인가. 하드코딩, 매직넘버, 중복, 과도한 결합 부재 여부 | OR-08 (Human Oracle) + OR-05 (Comparison Oracle -- 코딩 컨벤션 대조) | 단일 함수 100줄 초과 시. 동일 로직 3곳 이상 중복 시. 문서 없는 매직넘버 존재 시 |

---

## Section 3: 실행 규칙

### 3.1 Verification 실행 규칙

```
순서:
  Phase 1 (구조적): VC-V-01 → VC-V-02 → VC-V-03 → VC-V-04 → VC-V-05 → VC-V-06
  Phase 2 (내용적): VC-V-07 → VC-V-08 → VC-V-09 → VC-V-10 → VC-V-11 → VC-V-12

중단 조건 (short-circuit):
  - VC-V-01 (Format) fail → 나머지 전체 중단. 즉시 실패 보고.
  - VC-V-02 (Execution) fail → 나머지 전체 중단. 즉시 실패 보고.
  - Phase 1에서 2개 이상 fail → Phase 2 진입 않음.

기록:
  - 각 항목 pass/fail을 OUTPUT_CONTRACT 해당 section에 기재.
  - "부분 가능" 항목은 자동 검사 결과 먼저 기록 후, 수동 확인 필요 부분 명시 표시.
```

### 3.2 Validation 실행 규칙

```
전제 조건:
  - Section 1 (Verification) 전체 pass 후에만 실행.

독립 판정 금지:
  - Claude는 validation 항목에 대해 단독으로 "pass" 선언 불가.
  - 반드시 oracle을 통해 근거 확보 후 판정.

escalation 우선순위:
  1. VC-D-05 (Hidden Semantic Rule)
  2. VC-D-06 (Spec Gap)
  3. VC-D-01 (Functional Correctness)
  4. 나머지

판정 등급 (4단계):
  - pass: oracle 근거 확보, 문제 없음
  - conditional pass: 조건 명시 필수 (조건 미충족 시 fail로 전환)
  - fail: oracle 근거 확보, 문제 확인
  - blocked: 판정에 필요한 oracle 사용 불가. 어떤 oracle 필요한지 기록

blocked 처리:
  - blocked 항목은 해결될 때까지 전체 validation을 "미완료"로 표시.
  - blocked 상태에서 강제 pass 전환 금지.
```

### 3.3 Verification Pass + Validation Fail 처리

이 상태는 **가장 위험한 상태**다. 형식적으로 완벽하나 의미적으로 틀린 결과물이 "통과"로 보일 수 있다.

```
처리 절차:
  1. 즉시 중단 -- 해당 결과물을 "검증 완료"로 표시하지 않는다.
  2. 원인 역추적 -- validation fail 원인이 verification 항목의 불충분함 때문인지 확인.
  3. Verification 항목 보강 -- 이번 validation fail을 향후 verification에서 잡을 수 있도록
     새 verification 규칙을 knowledge asset으로 기록.
  4. Spec gap 확인 -- validation fail이 spec 빈칸 때문이면 FT-01 분류,
     빈칸 채우기는 인간에게 요청.
  5. 루프 재진입 -- 보강된 verification 규칙으로 검증 루프 재실행.

금지 문장:
  "모든 verification 항목이 pass이므로 문제가 없습니다"
```

---

## Section 4: Gray Zone 규칙

verification 항목이 validation으로 전환되는 조건. 규칙 자체가 불확실하면 해당 항목은 validation으로 승격된다.

| 전환 조건 | 예시 | 전환 후 처리 |
|----------|------|------------|
| 규칙 근거 spec이 모호 | "적절한 크기로 분할하라" -- "적절한" 기준 부재 | VC-D-06 (Spec Gap)으로 이동. 인간에게 기준 정의 요청 |
| 자동 검사 도구 신뢰도 낮음 | 스키마 validator가 semantic 유효성 미검사 | VC-D-01 (Functional Correctness)과 병행 검사 |
| 환경 의존적 규칙 | "빌드 성공"이 특정 OS/버전에서만 유의미 | OR-09 (Environment Oracle) 확인 후 판정 |
| 이전 루프에서 해당 항목이 misfill 원인 | 과거에 format은 맞았으나 의미가 틀렸던 필드 | 해당 필드를 validation으로 승격. knowledge asset 기록 |
| 규칙이 2개 이상 해석 가능 | "boundary"가 bbox 경계인지 물리적 외곽면인지 불명확 | VC-D-05 (Hidden Semantic Rule)로 이동. 해석을 채우지 않고 보고 |

### 전환 시 필수 기록 템플릿

```yaml
gray_zone_transition:
  original_item: "VC-V-XX"
  target_item: "VC-D-YY"
  reason: "[구체적 전환 사유]"
  discovered_at: "[루프 번호 / 단계]"
  action: "[escalation | oracle 추가 확보 | 규칙 재정의]"
  evidence: "[전환 판단 근거]"
```

---

## Section 5: 로그 및 중간 산출물 검증 원칙

```
원칙 1 (존재 검증):
  - 선언된 모든 로그/산출물이 지정 경로에 존재해야 한다.
  - 파일 크기 > 0 이어야 한다 (빈 파일은 fail).
  - 검증 항목: VC-V-03

원칙 2 (시간 순서 검증):
  - 로그의 타임스탬프가 실행 순서와 일치해야 한다.
  - 역순 타임스탬프는 실행 환경 이상을 의미한다.

원칙 3 (중간 산출물 일관성):
  - 중간 산출물 간 참조가 유효해야 한다 (깨진 참조 0건).
  - 최종 산출물이 중간 산출물을 올바르게 통합해야 한다.

원칙 4 (관찰 가능성):
  - 로그가 없는 단계는 검증 불가하므로, 검증 대상에서 제외하고 blocked 표시.
  - blocked 단계가 존재하면 전체 검증 결과에 "관찰 불가 구간 있음" 경고 포함.
```

---

## Section 6: 확장 규칙

```
프로젝트별 항목:
  - 프로젝트 고유 verification 항목: VC-P-V-XXX (예: VC-P-V-001)
  - 프로젝트 고유 validation 항목:   VC-P-D-XXX (예: VC-P-D-001)
  - 정의 위치: .vkl/project/VALIDATION_CHECKLIST.project.md

ID 할당 규칙:
  - core 항목 (VC-V-XX, VC-D-XX): 이 문서에서만 정의. 최대 99개.
  - project 항목 (VC-P-V-XXX, VC-P-D-XXX): 프로젝트 문서에서 정의. 최대 999개.
  - core 항목은 프로젝트에서 재정의 불가. 추가만 가능.

cross-reference 규칙:
  - 모든 VC-V-XX / VC-D-XX는 ORACLE_CATALOG의 OR-XX와 1:N 매핑.
  - 모든 VC-V-XX / VC-D-XX는 FAILURE_TAXONOMY의 FT-XX와 N:M 매핑.
  - MR-XX (TEST_RELATIONS)는 주로 VC-D-03 (Edge Case Coverage)와 연결.

승격/강등 절차:
  - project 항목이 3개 이상 프로젝트에서 반복되면 core 항목 승격 제안 가능.
  - 제안은 .vkl/proposals/에 작성. 인간 승인 후 이 문서에 반영.
```

---

## Section 7: Quick Reference

| 차원 | Verification (VC-V-XX) | Validation (VC-D-XX) |
|------|----------------------|---------------------|
| 질문 | "규칙대로 했는가?" | "맞는 것을 만들었는가?" |
| 판정 주체 | Claude (자동) | Human + External Oracle |
| 근거 | 명시 규칙, 스키마, spec | 의도, 품질 기대, 도메인 지식 |
| 실패 시 | 즉시 수정 후 재실행 | escalation 후 인간 판단 대기 |
| 전체 pass 의미 | "형식은 맞다" | "의미도 맞다" |
| 한계 | 규칙이 불완전하면 무의미 | 인간 응답 대기 시간 |

---

## Appendix A: Blender Fracture Case -- boundary/thickness axis misfill

### 배경

Blender fracture 프로젝트에서 "outer boundary" 용어의 의미가 단계별로 잘못 채워진(misfill) 사례.

### misfill 진행

```
Stage 1: "outer boundary" -> "impact distance 기준 바깥 경계" (초기 해석)
Stage 2: "outer boundary" -> "bbox(bounding box) 경계" (bbox 코드 주변에서 유추)
Stage 3: "outer boundary" -> "thickness axis 포함 전체 경계" (두께 축까지 확장)
Stage 4: 최종 발견 -> "exclude thinnest axis" 규칙 존재 (가장 얇은 축 제외)
```

### Verification 관점: 모두 pass

| 항목 | Stage 1 | Stage 2 | Stage 3 | Stage 4 |
|------|---------|---------|---------|---------|
| VC-V-01 Format Compliance | PASS | PASS | PASS | PASS |
| VC-V-02 Execution Success | PASS | PASS | PASS | PASS |
| VC-V-03 Log/Artifact Presence | PASS | PASS | PASS | PASS |
| VC-V-04 Changed File Scope Match | PASS | PASS | PASS | PASS |
| VC-V-05 No Scope Violation | PASS | PASS | PASS | PASS |
| VC-V-06 Output Contract Compliance | PASS | PASS | PASS | PASS |
| VC-V-07 Failure Taxonomy Present | PASS | PASS | PASS | PASS |
| VC-V-08 Oracle Citation Present | PASS | PASS | PASS | PASS |

4개 stage 모두 verification 전체 pass. 형식적으로 완벽.

### Validation 관점: 잡았을 항목

| 항목 | Stage 1~3 | 탐지 가능 시점 |
|------|-----------|--------------|
| VC-D-01 Functional Correctness | **FAIL** | Stage 1 -- 레퍼런스 출력과 비교 시 boundary 수 불일치 |
| VC-D-02 Intent Alignment | **FAIL** | Stage 1 -- "outer boundary가 뭘 의미하는지" 질문 시 발견 |
| VC-D-05 Hidden Semantic Rule | **FAIL** | Stage 1 -- "outer boundary" 정의 부재를 표시 시 발견 |
| VC-D-06 Spec Gap Identification | **FAIL** | Stage 1 -- "boundary 축 제외 규칙" 스펙 미정의 보고 시 발견 |

### 교훈 요약

```
1. "outer boundary"에 대해 FT-01 (spec gap) 선언 후 인간에게 정의 요청 필요.
2. Claude가 "impact distance 기준"으로 해석한 순간 FT-02 (hidden semantic rule misfill) 발생.
   해석 근거 oracle 부재.
3. 동일 용어 해석이 stage마다 변경된 것 자체가 red flag.
   -> MR-04 (Representation Invariance) 위반: 동일 용어 해석 변경 = spec gap.
4. Verification만으로는 절대 발견 불가. VC-D-05, VC-D-06만이 탐지 가능.
```

### 생성되어야 할 Knowledge Asset

```yaml
knowledge_asset:
  id: KA-BLENDER-001
  type: discovered_rule
  source_loop: fracture-boundary-stage4
  rule: >
    boundary 관련 용어가 스펙에 정의되지 않은 경우, 축(axis) 관련 제외 규칙이
    존재할 수 있다. 물리 시뮬레이션에서 '가장 얇은 축 제외'는 빈번한 hidden rule.
  failure_type: FT-02
  oracle_needed: OR-08 (Human Oracle -- 도메인 전문가)
  prevention: >
    boundary/edge/face 등 기하학적 용어 등장 시 즉시 FT-01 분류 후 정의 요청.
    자체 해석 금지.
  confidence: confirmed
  related_checklist_items: [VC-D-05, VC-D-06]
  related_test_relations: [MR-04]
```
