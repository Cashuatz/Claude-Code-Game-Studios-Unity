---
name: verify
description: VKL 검증 루프 실행 — 코드/기능을 체계적으로 검증하고 지식화한다
model: sonnet
---

# /verify — VKL 검증 루프

## 사용 시점
- /story-done 전에 구현 결과를 정밀 검증할 때
- /code-review에서 의심스러운 부분을 심층 검증할 때
- 반복적으로 같은 종류의 버그가 발생할 때
- 사용자가 "검증해줘", "verify", "VKL" 이라고 할 때

## 실행 절차

1. **문서 읽기** (아래 순서)
   - .vkl/core/ROLE_AND_RULES.md
   - .vkl/core/OUTPUT_CONTRACT.md
   - .vkl/core/ESCALATION_POLICY.md
   - .vkl/project/PROJECT_CONTEXT.md
   - .vkl/project/FAILURE_TAXONOMY.project.md
   - .vkl/project/ORACLE_CATALOG.project.md
   - .vkl/project/VALIDATION_CHECKLIST.project.md
   - .vkl/project/TEST_RELATIONS.project.md

2. **Verification** — VC-V-01~12 (core) + VC-P-V (project) 항목별 체크
3. **Oracle 실행** — 필요한 Oracle 호출 및 결과 기록
4. **Metamorphic 체크** — MR 관계 테스트
5. **Validation** — VC-D-01~09 (core) + VC-P-D (project). 인간 판단 필요 항목은 에스컬레이션
6. **실패 분류** — 모든 실패를 FT-XX / FT-P-XXX로 분류
7. **9섹션 출력** — OUTPUT_CONTRACT 형식으로 결과 보고
8. **기록** — .vkl/runtime/에 case_log, observations, oracle_runs 기록
9. **Proposal** — 새 규칙이 필요하면 .vkl/proposals/ 에 생성

## 출력 형식 (9섹션 — OUTPUT_CONTRACT)

매 검증 결과는 반드시 아래 섹션을 포함:

### 1. Loop Goal
이번 검증 루프의 목표와 범위

### 2. Excluded Scope
이번 루프에서 의도적으로 제외한 범위

### 3. Observed Signals
수집한 신호와 증거 (로그, 테스트 결과, 코드 분석)

### 4. Failure Taxonomy
발견된 실패의 FT-XX / FT-P-XXX 분류

### 5. Oracle Evaluation
사용한 Oracle과 판정 결과 (OR-XX)

### 6. Metamorphic / Property Checks
MR 관계 테스트 결과

### 7. Decision
pass / fail / uncertain (Oracle ID 근거 필수)

### 8. Next Action
더 좁은 범위의 다음 루프 또는 사용자 에스컬레이션

### 9. Knowledge Assets Updated
runtime/proposals에 기록한 내용 목록
