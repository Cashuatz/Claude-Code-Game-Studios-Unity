---
Document Role: Core Policy
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# ESCALATION_POLICY

VKL 검증 루프에서 Claude가 **반드시 판정을 중단하고 인간에게 넘겨야 하는 조건**을 정의한다. 이것은 권장 사항이 아니라 **강제 규칙(hard rule)**이다. 조건이 충족되면 Claude는 자체 판단으로 진행하지 않고 반드시 Escalation Notice를 출력한다.

---

## Core Principle

> Claude는 **verification(검증)** 영역에서만 독립 판정을 내린다. **Validation(검인)** 영역의 판단이 필요한 순간, 또는 verification 자체의 신뢰성이 의심되는 순간에는 반드시 escalation한다.

---

## 10 Escalation Trigger Conditions

### Condition #1: Validation-Only Judgment Remaining

**조건:** 모든 verification 항목이 pass이지만, 최종 decision이 품질(quality), 의도 적합성(intent alignment), 사용자 감각(feel)에 의존하는 경우.

**근거:** Claude는 인간이 지각하는 품질이나 의도 부합 여부를 판단할 수 없다. 모든 기계적 검증을 통과했더라도 "이게 사용자가 원한 건가?"는 별개의 질문이다.

**행동:**
1. 통과한 모든 verification 결과를 정리하여 제시한다.
2. Validation이 필요한 구체적 항목을 명시한다.
3. validation 판단을 돕기 위한 구체적 질문을 제시한다.

---

### Condition #2: Oracle Conflict

**조건:** 두 개 이상의 oracle(OR-XX)이 동일 항목에 대해 상반된 판정을 내리는 경우.

**근거:** Claude에게는 oracle 간 충돌을 해소할 메타-오라클(meta-oracle)이 없다. 둘 중 하나를 선택하는 것 자체가 Claude의 역할 범위를 초과한다.

**행동:**
1. 충돌하는 각 oracle의 결과와 근거를 나란히 제시한다.
2. 충돌 지점(conflict point)을 명확히 식별한다.
3. 충돌을 해소할 수 있는 추가 oracle이나 데이터를 제안한다.
4. 인간에게 어떤 oracle을 우선할지 결정을 요청한다.

---

### Condition #3: High-Confidence Conclusion Required Without Logs

**조건:** 확정적 판정이 필요한 항목에 대해, 중간 상태(intermediate state), 로그, 실행 결과물 등 관찰 가능한 증거가 없거나 접근할 수 없는 경우.

**근거:** 관찰 가능한 증거 없이 내리는 모든 결론은 추측이다. VKL의 핵심 원칙은 "관찰한 것만 보고한다"이다.

**행동:**
1. 어떤 증거가 없는지(missing evidence)를 구체적으로 나열한다.
2. 그 증거가 있었다면 어떤 판정에 사용되었을지 설명한다.
3. 누락된 데이터 제공 또는 인간의 직접 판정을 요청한다.
4. **절대로** "로그가 없지만 아마 문제없을 것이다" 식의 추측 판정을 하지 않는다.

---

### Condition #4: Relation Tests Pass but Perceived Quality Is Low

**조건:** Metamorphic relation(MR-XX) 검사, property 검사가 모두 통과했으나, 결과물의 품질이 기능적으로 부정확하거나 "무언가 잘못되어 보이는" 경우.

**근거:** 모든 테스트가 통과했는데 결과가 이상하면 **테스트 관계 자체가 불충분하거나 잘못 설계되었을 가능성**을 시사한다. Claude가 테스트를 자체 수정하면 자기 검증 순환(self-verification loop)에 빠진다.

**행동:**
1. 통과한 모든 테스트 결과를 제시한다.
2. 품질이 의심되는 구체적 지점을 설명한다.
3. 현재 테스트 관계가 검사하지 못하는 속성을 분석한다.
4. 인간에게 (a) 결과물 품질이 실제 문제인지, (b) 테스트 관계를 수정해야 하는지 판단을 요청한다.

---

### Condition #5: Spec Gap Is Central and User Hasn't Decided

**조건:** 최종 판정의 핵심이 되는 항목에 대한 스펙이 존재하지 않으며, 사용자가 아직 해당 스펙을 결정하지 않은 경우.

**근거:** 핵심 스펙이 없는 항목에 대한 모든 구현 선택은 추측이다. Claude가 임의로 기본값을 채우면 FT-02 (hidden semantic rule misfill)의 직접 원인이 된다.

**행동:**
1. 부재하는 스펙을 식별한다 -- "X에 대한 정의/규칙/기준이 스펙에 없습니다."
2. 이 스펙 부재가 전체 결과에 미치는 영향을 설명한다.
3. 가능한 선택지(options)를 장단점과 함께 나열한다.
4. 사용자에게 결정을 요청한다.
5. **절대로** "일반적으로 이렇게 하니까"라는 논리로 스펙 공백을 채우지 않는다.

---

### Condition #6: Case Log Confidence Level Is "Low"

**조건:** 케이스 로그 작성 시 Confidence Level이 "Low"로 평가되는 경우.

**근거:** Low confidence는 관찰, 분류, 판정 중 하나 이상이 불확실하다는 의미이다. 불확실한 판정을 기반으로 다음 루프를 진행하면 오류가 전파된다.

**행동:**
1. Confidence가 Low인 구체적 이유를 명시한다.
2. 불확실한 부분을 식별한다 (관찰? 분류? oracle 판정?).
3. 확신을 높이려면 무엇이 필요한지 제안한다.
4. 인간에게 케이스 로그 리뷰를 요청한다.

---

### Condition #7: Same Failure Type (FT-XX) Appeared 3+ Times Consecutively

**조건:** 동일한 Failure Type(FT-XX 또는 FT-P-XXX)이 연속 3개 이상의 루프에서 반복 발생하는 경우.

**근거:** 같은 실패가 3회 반복되면 루프가 문제를 해결하지 못하고 있다. 원인 분석 자체가 잘못되었거나, 문제 범위가 현재 루프를 초과한다. 같은 접근의 반복은 자원 낭비다.

**행동:**
1. 반복되는 FT-XX ID와 연속 횟수를 보고한다.
2. 각 루프에서 시도한 해결 방법과 결과를 요약한다.
3. 반복 원인을 분석한다 (원인 오진? 해결 범위 부족? 근본 원인이 다른 곳?).
4. 인간에게 접근 전환 또는 범위 재설정을 요청한다.

---

### Condition #8: Required Fix Exceeds Current Loop Scope

**조건:** 이번 루프에서 발견된 문제의 수정 범위가 선언한 Excluded Scope 또는 Loop Scope를 초과하는 경우.

**근거:** 범위 초과 수정을 Claude가 임의 수행하면 FT-12 (scope violation/over-edit)가 된다. 범위 확장은 인간이 결정한다.

**행동:**
1. 발견된 문제와 필요한 수정 범위를 보고한다.
2. 현재 루프 범위와의 차이를 명시한다.
3. 범위 확장 / 새 루프 설계 / 현재 범위 내 부분 수정 중 인간에게 선택을 요청한다.

---

### Condition #9: External Verifier Is Unavailable but Needed

**조건:** 판정에 외부 검증기(컴파일러, 테스트 러너, 린터, 시뮬레이션 엔진 등)의 결과가 필요하지만, 해당 검증기에 접근할 수 없는 경우.

**근거:** 외부 검증기 없이 "아마 컴파일될 것이다"는 추측이다. FT-07 (external verifier failure)을 정확히 분류하려면 실제 실행 결과가 필요하다.

**행동:**
1. 필요한 외부 검증기를 명시한다.
2. 접근 불가 사유를 설명한다.
3. 인간에게 (a) 검증기 결과 제공 또는 (b) 검증기 없이 진행할지 결정을 요청한다.

---

### Condition #10: TBD Field Turned Critical

**조건:** 사용자가 이전에 "미정(TBD)"으로 표시한 항목이 이번 루프에서 판정의 핵심 의존성으로 드러난 경우.

**근거:** "미정"은 "나중에 결정하겠다"는 의미인데, 그 "나중"이 지금이 된 것이다. Claude가 미정 항목을 임의로 채우면 FT-01 (spec gap) 또는 FT-02 (misfill)의 직접 원인이 된다.

**행동:**
1. 어떤 "미정" 항목이 핵심 의존성이 되었는지 명시한다.
2. 이 항목 없이 판정 진행이 불가능한 이유를 설명한다.
3. 결정을 위한 컨텍스트(현재 검증 결과, 관련 제약 조건)를 제시한다.
4. 사용자에게 결정을 요청한다.

---

## Oracle Conflict Resolution Rules

Oracle 충돌(Condition #2) 발생 시 Claude가 따르는 구체적 절차.

### 절차

```
1. 충돌하는 oracle들의 ID, 판정 결과, 근거를 테이블로 정리한다.

   | Oracle ID | 판정 | 근거 | 신뢰도 |
   |-----------|------|------|--------|
   | OR-XX     | pass | ...  | high   |
   | OR-YY     | fail | ...  | medium |

2. 충돌 지점(conflict point)을 식별한다.
   - 같은 데이터에 대한 해석 차이인가?
   - 서로 다른 데이터를 보고 있는가?
   - oracle의 적용 범위가 다른가?

3. 메타-오라클이 존재하는지 확인한다.
   - project/ 레이어에 oracle 우선순위 규칙이 정의되어 있는가?
   - 이전 루프에서 인간이 oracle 우선순위를 결정한 선례가 있는가?

4. 메타-오라클이 없으면 반드시 에스컬레이션한다.
   - Claude가 신뢰도 등급으로 oracle을 선택하는 것은 금지한다.
   - "OR-XX가 high이므로 OR-XX를 우선한다"는 자의적 해결이다.
```

### 금지 행동

- Oracle 간 "다수결"로 판정 (3개 중 2개가 pass이므로 pass)
- 신뢰도 등급만으로 oracle 선택
- 충돌을 무시하고 한쪽 oracle만 보고

---

## Insufficient Oracle Protocol

사용 가능한 oracle이 2개 미만이거나, 핵심 판정에 필요한 oracle이 부재한 경우의 절차.

### 절차

```
1. 현재 사용 가능한 oracle 목록을 작성한다.

2. 판정에 필요하지만 부재한 oracle을 식별한다.
   - 어떤 oracle이 있어야 하는가?
   - 왜 부재한가? (환경 제약? 스펙 미정의? 외부 도구 미접근?)

3. oracle 2개 미만 사용 시:
   - Failure Taxonomy에 "validation shortfall"을 기록한다.
   - Decision은 "uncertain"으로 설정한다.

4. 인간에게 다음을 요청한다:
   - 부재한 oracle의 대체 수단 제공
   - 또는 현재 oracle만으로 판정 진행 승인
```

---

## Blocked State Rules

Escalation을 발행한 후, 인간의 응답을 기다리는 동안의 행동 규칙.

### 계속할 수 있는 것

| 상태 | 허용 행동 |
|------|----------|
| Advisory escalation | escalation과 무관한 다른 verification 항목 계속 진행 |
| Blocking/Advisory | Observed Signals 정리 및 케이스 로그 작성 (Decision은 Uncertain으로) |
| Blocking/Advisory | 다음 루프의 예비 설계 준비 (escalation 해소 시 즉시 실행 가능하도록) |
| Blocking/Advisory | `.vkl/runtime/`에 현재까지의 지식 자산 기록 |

### 해서는 안 되는 것

| 금지 행동 | 이유 |
|----------|------|
| Blocking escalation 상태에서 다음 루프로 넘어감 | 미해소 상태에서 진행하면 오류 전파 |
| Escalated 항목에 대해 "아마 이럴 것이다"로 진행 | 추측 진행은 misfill의 원인 |
| 인간 응답 없이 escalation을 자체 해소(de-escalate) | De-escalation 조건이 충족되지 않음 |
| "시간이 지났으니 기본값으로 진행합니다" | 타임아웃 처리 금지 |
| Blocking escalation을 advisory로 격하 | 심각도 축소 금지 |
| 여러 옵션을 자체적으로 시도 | 옵션 제시는 가능, 선택은 인간이 함 |

---

## Escalation Severity

### Blocking Escalation

**정의:** 이 escalation이 해소되기 전까지 현재 루프의 판정을 완료할 수 없다. 다음 루프로 진행할 수 없다.

**해당 조건:**

| # | Condition | 핵심 질문 |
|---|-----------|----------|
| 2 | Oracle Conflict | 두 oracle이 상반된 판정을 내리는가? |
| 3 | No Logs / No Evidence | 관찰 근거 없이 판정을 내려야 하는가? |
| 5 | Central Spec Gap | 핵심 스펙이 없는데 판정해야 하는가? |
| 9 | External Verifier Unavailable | 외부 검증기가 필요한데 접근 불가인가? |
| 10 | TBD Field Is Critical | 미정 항목이 판정 핵심 의존성이 되었는가? |

**Blocking 시 Claude의 행동:**
- Decision 섹션에 **uncertain**을 기재한다.
- Next Action에 "Escalation 해소 후 이 루프 재실행"을 기재한다.
- 다음 루프로 넘어가지 않는다.

### Advisory Escalation

**정의:** 인간의 주의를 요하지만, 현재 루프의 나머지 verification 항목은 계속 진행할 수 있다. 단, 최종 decision에 미해소 상태가 반영되어야 한다.

**해당 조건:**

| # | Condition | 핵심 질문 |
|---|-----------|----------|
| 1 | Validation-Only Judgment | 품질/의도 적합성은 인간만 판단할 수 있는가? |
| 4 | Tests Pass but Quality Low | 테스트는 통과했는데 결과가 이상한가? |
| 6 | Low Confidence Case Log | 이 케이스 로그의 판정을 신뢰할 수 있는가? |
| 7 | 3+ Consecutive Same FT-XX | 같은 실패가 3번 이상 반복되는가? |
| 8 | Fix Exceeds Loop Scope | 수정 범위가 루프 범위를 초과하는가? |

**Advisory 시 Claude의 행동:**
- 나머지 verification 항목을 계속 진행한다.
- Decision 섹션에 escalation 미해소 상태를 명시한다.
- Next Action에 escalation 해소를 위한 항목을 포함한다.

---

## Escalation Notice Template

Escalation 발동 시 Claude는 반드시 아래 구조화된 형식으로 출력한다. 자유 형식(free-form)으로 에스컬레이션하지 않는다.

```markdown
## Escalation Notice

- **Trigger:** {발동된 조건 번호와 이름. 예: "Condition #5: Spec Gap Is Central and User Hasn't Decided"}
- **Severity:** {Blocking / Advisory}
- **Evidence:** {이 조건이 충족되었다는 관찰 근거. Observed Signals 또는 Oracle Evaluation에서 인용}
- **Missing:** {판정 진행에 부족한 정보, 데이터, 결정 사항}
- **Options:** {가능한 해결 방안 나열. 각 옵션의 장단점 포함. 없으면 "해결 방안 제시 불가 -- 인간 판단 필요"}
- **Recommended Human Action:** {인간에게 요청하는 구체적 행동}
```

### Escalation Notice 작성 규칙

1. **구체적으로 작성한다.** "스펙이 불명확합니다" (X) -> "X 항목의 Y 조건이 스펙에 정의되어 있지 않습니다" (O).
2. **증거를 인용한다.** Evidence 필드에는 반드시 이번 루프의 Observed Signals 또는 Oracle Evaluation에서 관찰한 구체적 데이터를 인용한다.
3. **옵션을 제시할 때 편향하지 않는다.** Claude가 선호하는 옵션에 "추천"이라고 표시하지 않는다. 모든 옵션을 동등하게 제시한다.
4. **1개의 Escalation Notice에 1개의 Trigger만 기재한다.** 여러 조건이 동시에 충족되면 별도의 Escalation Notice를 각각 출력한다.
5. **Severity를 반드시 명시한다.** Blocking인지 Advisory인지에 따라 Claude의 후속 행동이 달라진다.

---

## De-Escalation Conditions

### 자동 De-Escalation (인간 응답 불필요)

| 원래 조건 | De-Escalation 사유 |
|----------|-------------------|
| Condition #3 (No Logs) | 누락되었던 로그, 실행 결과가 제공되어 관찰 근거가 충족됨 |
| Condition #9 (Verifier Unavailable) | 외부 검증기 결과가 제공됨 |
| Condition #2 (Oracle Conflict) | 인간이 지정한 추가 oracle 적용 결과 한쪽 oracle의 근거가 무효화되어 충돌 해소 |

### 인간 응답 필수 De-Escalation

| 원래 조건 | De-Escalation 사유 |
|----------|-------------------|
| Condition #5 (Central Spec Gap) | 사용자가 미정의 항목에 대해 명시적으로 결정을 내림 |
| Condition #10 (TBD Field Critical) | 사용자가 미정 항목을 결정함 |
| Condition #1 (Validation-Only) | 사용자가 validation 항목을 직접 확인하고 승인/거부를 명시함 |
| Condition #4 (Quality Low) | 사용자가 품질을 직접 확인하고 판단을 내림 |
| Condition #7 (3+ Same FT-XX) | 사용자가 새로운 접근 방식 또는 범위를 지정함 |
| Condition #8 (Fix Exceeds Scope) | 사용자가 범위 확장/유지/분할을 결정함 |
| Condition #6 (Low Confidence) | 사용자가 low-confidence 케이스 로그를 리뷰하고 코멘트를 남김 |

### De-Escalation 후 행동

1. De-escalation 사유와 해소 방법을 `.vkl/runtime/`에 기록한다.
2. Decision 섹션을 uncertain에서 pass/fail로 업데이트한다.
3. 인간의 결정 사항을 Knowledge Assets에 반영한다.
4. `core/` 또는 `project/` 변경이 필요하면 `.vkl/proposals/`에 제안서를 작성한다.
5. 다음 루프를 진행한다.

---

## Escalation Trigger Quick-Reference Table

| # | Trigger Name | Severity | 핵심 질문 |
|---|-------------|----------|----------|
| 1 | Validation-Only Judgment | Advisory | 품질/의도 적합성은 인간만 판단할 수 있는가? |
| 2 | Oracle Conflict | Blocking | 두 oracle(OR-XX)이 상반된 판정을 내리는가? |
| 3 | No Logs / No Evidence | Blocking | 관찰 근거 없이 판정을 내려야 하는가? |
| 4 | Tests Pass but Quality Low | Advisory | MR-XX 통과했는데 결과가 이상한가? |
| 5 | Central Spec Gap | Blocking | 핵심 스펙이 없는데 판정해야 하는가? |
| 6 | Low Confidence Case Log | Advisory | 이 케이스 로그의 판정을 신뢰할 수 있는가? |
| 7 | 3+ Consecutive Same FT-XX | Advisory | 같은 FT-XX가 3번 이상 반복되는가? |
| 8 | Fix Exceeds Loop Scope | Advisory | 수정 범위가 선언된 루프 범위를 초과하는가? |
| 9 | External Verifier Unavailable | Blocking | 외부 검증기가 필요한데 접근 불가인가? |
| 10 | TBD Field Is Critical | Blocking | 미정 항목이 판정 핵심 의존성이 되었는가? |

---

## Example: Blender Fracture -- Escalation at "Boundary" Definition Ambiguity

### 배경

Glass fracture 스크립트에서 "outer boundary" 판정 로직이 세 차례에 걸쳐 변경되었다:

1. **루프 1:** 충격 지점(impact point) 거리 기준 boundary 판정. 결과: 비대칭 분포 -> 실패 (FT-02).
2. **루프 2:** Bounding box 기준 전환. 모든 축에 동일 비율 threshold 적용. 이 시점에서 문제 미인식.
3. **루프 3:** 얇은 유리판(두께 0.1, 폭/높이 10.0) 테스트. outer 비율 87% -> **Escalation 발동.**

### Escalation 발동 순간

루프 3에서 다음 관찰 시점:
- OR-03 (Execution Oracle): "outer 비율 87%는 비정상" -> fail
- OR-06 (Metamorphic Oracle): "두께 변화 시 outer 비율 비례적 급증" -> fail
- 원인: Z축(두께 축) threshold 0.05가 지나치게 작아 거의 모든 조각이 outer로 분류

Claude는 "thinnest axis를 제외하면 해결된다"를 추론할 수 있었다. 그러나 이것은 **스펙에 없는 규칙을 채우는 행위(misfill)**이므로 에스컬레이션이 올바른 행동이다.

### 발행된 Escalation Notice

```markdown
## Escalation Notice

- **Trigger:** Condition #5: Spec Gap Is Central and User Hasn't Decided
- **Severity:** Blocking
- **Evidence:** 루프 3에서 OR-03이 outer 비율 87%로 fail, OR-06이 두께 변화 robustness 위반으로 fail. Z축 threshold 0.05로 대부분 조각이 outer 분류.
- **Missing:** "Outer boundary" 판정에서 축별 적용 규칙. (a) 모든 축 동일 비율? (b) thinnest axis 제외? (c) 축별 다른 비율?
- **Options:**
  - Option A: thinnest axis를 boundary 판정에서 제외. 장점: 얇은 오브젝트에서 직관적. 단점: "가장 짧은 축" 기준 미정의, 정육면체 동작 별도 정의 필요.
  - Option B: 축별 다른 threshold 비율 적용. 장점: 정보 손실 없음. 단점: 추가 파라미터 필요.
  - Option C: 2D 투영(가장 넓은 면) 기반 전환. 장점: 얇은 오브젝트에서 직관적. 단점: 3D 정보 일부 손실.
- **Recommended Human Action:** 위 세 옵션 중 적합한 방식을 결정해 주세요. 고려 사항: (1) 유리판 외 다른 형태에도 사용되는지, (2) "boundary"의 도메인 정의.
```

### 이 사례의 교훈

| 교훈 | 설명 |
|------|------|
| Escalation은 선언이다 | "모른다"의 고백이 아니라 "이것은 인간의 결정 영역이다"의 선언 |
| FT-02는 연쇄적이다 | 1차 misfill 교정 후 2차 misfill 발생. 스펙 공백은 연쇄적 결정 트리 |
| Escalation 시점이 중요하다 | spec gap의 존재만으로는 escalation하지 않음. gap이 판정 핵심이 될 때 발동 |
| Metamorphic Oracle이 트리거 역할 | 두께 변화 MR-XX 검사가 "비정상"의 확신을 제공하여 escalation 근거가 됨 |
| 기록은 runtime/에 | 이 escalation의 결과와 인간 결정은 runtime/에 기록. 규칙 변경은 proposals/로 |
