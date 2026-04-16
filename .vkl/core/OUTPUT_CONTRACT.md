---
Document Role: Core Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: All projects using VKL
---

# OUTPUT_CONTRACT

검증 루프 완료 시 Claude가 반드시 준수해야 하는 출력 형식을 정의한다. 자유 형식 산문(free-form prose)은 메인 출력에서 금지된다. 모든 내용은 아래 9개 필수 섹션 안에 구조화된 형식으로 들어가야 한다.

---

## General Rules

1. **9개 필수 섹션은 모두 존재해야 한다.** 순서를 바꾸지 않는다.
2. **섹션 내부 형식(테이블, 불릿 리스트 등)은 아래 명세를 따른다.** 형식을 임의로 변경하지 않는다.
3. **"N/A" 허용 규칙:** 각 섹션별로 N/A 허용 여부를 명시한다. N/A가 허용되지 않는 섹션에서 N/A를 쓰면 계약 위반이다.
4. **Decision 필드의 허용 값:** `pass` / `fail` / `uncertain` -- 이 3개 외의 값은 금지한다.
5. **모든 판정에는 근거(evidence)가 동반되어야 한다.** 근거 없는 판정은 무효다.
6. **ID 참조 규칙:** Failure Taxonomy는 FT-XX (core) 또는 FT-P-XXX (project), Oracle은 OR-XX (core) 또는 OR-P-XXX (project) ID를 사용한다. Metamorphic/Property는 relation 정의를 가리킬 때 `MR-XX` (core) 또는 `MR-P-XXX` (project) ID를 사용하고, 실제 실행한 relation instance를 가리킬 때 `MR-XX-[PROJECT]-[NNN]` 또는 `MR-P-XXX-[NNN]` 형식의 instance ID를 사용할 수 있다.
7. **Knowledge Assets 기록 위치:** `runtime/`에 새 파일로 기록하거나, `core/`/`project/` 변경이 필요하면 `proposals/`에 제안서를 작성한다.

---

## Section 1: Loop Goal

```markdown
## [Loop Goal]

- **목표:** {이번 루프의 검증 대상과 완료 조건}
- **루프 번호:** {동일 Task에 대한 반복 검증 횟수. 첫 루프면 1}
- **선행 루프 참조:** {이전 루프의 Decision과 Next Action 요약. 첫 루프면 "없음"}
```

**Required sub-fields:** 목표, 루프 번호, 선행 루프 참조 -- 3개 모두 필수.

**Format constraints:** 불릿 리스트. 목표는 검증 대상과 완료 조건을 한 문장으로 기술한다.

**N/A 허용:** 불가. 이 섹션은 항상 채워야 한다.

**Anti-patterns:**
- 목표가 모호한 경우: "코드를 검증한다" (X) -> "fracture 함수의 boundary 계산이 thinnest axis를 제외하는지 검증한다" (O)
- 선행 루프 참조를 생략하는 경우

---

## Section 2: Excluded Scope

```markdown
## [Excluded Scope]

- {제외 항목 1}
- {제외 항목 2}

**실제 위반 여부:** {루프 수행 중 제외 범위를 침범했으면 기술. 없으면 "위반 없음"}
```

**Required sub-fields:** 제외 항목 목록, 실제 위반 여부.

**Format constraints:** 불릿 리스트 + 위반 여부 판정. 범위 침범 시 Failure Taxonomy에 FT-12 (scope violation/over-edit)를 기록한다.

**N/A 허용:** 불가. 제외 범위가 없어도 "명시적 제외 항목 없음. 모든 관련 범위를 검증 대상에 포함함."으로 기술한다.

**Anti-patterns:**
- 제외 범위를 선언했지만 Observed Signals에 해당 범위 관련 변경이 포함된 경우
- "실제 위반 여부" 필드 누락

---

## Section 3: Observed Signals

```markdown
## [Observed Signals]

### Execution Results
- {실행 결과 1: 명령어/동작 -> 결과}

### Logs
- {로그 항목 1: 타임스탬프 또는 위치 + 내용}

### Intermediate Artifacts
- {산출물 1: 파일명/경로 + 요약}

### Anomalies
- {예상과 다른 관찰 1}
- {없으면: "예상 외 관찰 사항 없음"}
```

**Required sub-fields:** Execution Results (필수), Logs (조건부), Intermediate Artifacts (조건부), Anomalies (필수).

**Format constraints:** 4개 하위 섹션. 각 항목은 관찰 가능한 사실만 기록한다.

**N/A 허용:** 부분 허용. Execution Results와 Anomalies는 필수. Logs와 Intermediate Artifacts는 Input에서 해당 항목이 "N/A"였을 경우에만 N/A 허용.

**Anti-patterns:**
- Anomalies를 단순히 비워두는 것 (확인했지만 없었다 = "예상 외 관찰 사항 없음"으로 명시)
- 관찰이 아닌 추론을 기록하는 것

---

## Section 4: Failure Taxonomy

```markdown
## [Failure Taxonomy]

| ID | 분류명 | 근거 | 심각도 | upstream/downstream |
|----|--------|------|--------|---------------------|
| {FT-XX 또는 FT-P-XXX} | {유형명} | {Observed Signals에서 도출한 구체적 근거} | {critical/major/minor/info} | {upstream: 원인 방향 / downstream: 영향 방향} |
```

**Required sub-fields:** ID, 분류명, 근거, 심각도, upstream/downstream -- 5개 모두 필수.

**Format constraints:** 테이블. ID는 core taxonomy면 FT-XX, project taxonomy면 FT-P-XXX 형식이다.

**심각도 기준:**
- `critical` -- 핵심 기능이 틀리거나, 데이터 손상/보안 위험
- `major` -- 기능이 부분적으로 틀리거나, 특정 조건에서 실패
- `minor` -- 기능은 맞지만 품질/성능/가독성 문제
- `info` -- 실패는 아니지만 향후 위험 요소로 기록할 가치

**발견된 실패가 없을 때:**

```markdown
| — | (없음) | 모든 유형 검토 완료. 해당 실패 없음. | — | — |
```

**N/A 허용:** 불가. 실패가 없더라도 "(없음)" 행으로 명시한다.

**Anti-patterns:**
- ID 없이 분류명만 기재
- 근거에 "느낌", "아마", "~인 것 같다" 사용
- 12개(core) 유형 검토 없이 "별다른 문제 없음" 기재

---

## Section 5: Oracle Evaluation

```markdown
## [Oracle Evaluation]

| ID | 오라클 | 판정 결과 | 판정 근거 | 부족한 오라클 |
|----|--------|----------|----------|-------------|
| {OR-XX 또는 OR-P-XXX} | {오라클 이름} | {pass/fail/uncertain} | {판정의 구체적 근거} | {보완 필요한 오라클 또는 "이 오라클 범위 내 충분"} |
```

**Required sub-fields:** ID, 오라클, 판정 결과, 판정 근거, 부족한 오라클 -- 5개 모두 필수.

**Format constraints:** 테이블. ID는 core oracle이면 OR-XX, project oracle이면 OR-P-XXX.

**추가 규칙:**
- Input에서 "사용"으로 표시한 모든 oracle이 이 테이블에 나타나야 한다. 누락은 계약 위반.
- 사용한 oracle이 2개 미만이면 "validation shortfall"을 Failure Taxonomy에 기록한다.
- human oracle이 지정되었으나 실제 확인을 받지 못한 경우, 판정 결과를 "uncertain"으로 쓰고 이유를 근거에 명시한다.

**N/A 허용:** 불가. 최소 2개 이상의 oracle 평가가 있어야 한다.

**Anti-patterns:**
- oracle 없이 "이 기능은 정상 동작합니다"
- "테스트를 통과했습니다" (어떤 oracle로 판정했는지 불명)
- 부족한 오라클 열을 비워두는 것

---

## Section 6: Metamorphic / Property Checks

```markdown
## [Metamorphic / Property Checks]

| ID | 관계/속성 | 통과/실패 | 실패 시 깨진 관계 |
|----|-----------|----------|-----------------|
| {MR-XX / MR-P-XXX / MR-XX-[PROJECT]-[NNN] / MR-P-XXX-[NNN]} | {이름과 설명} | {pass/fail/not tested} | {실패: 기대값 vs 실제값. 통과: "—"} |
```

**Required sub-fields:** ID, 관계/속성, 통과/실패, 실패 시 깨진 관계 -- 4개 모두 필수.

**Format constraints:** 테이블. ID는 relation 정의를 적을 때 core면 `MR-XX`, project면 `MR-P-XXX`를 사용한다. 실제 실행한 테스트 케이스를 구분해야 할 때는 `MR-XX-[PROJECT]-[NNN]` 또는 `MR-P-XXX-[NNN]` instance ID를 사용한다.

**추가 규칙:**
- `not tested`는 기술적 제약으로 테스트하지 못한 경우에만 허용하며, 사유를 "실패 시 깨진 관계" 열에 기술한다.
- 하나라도 fail이면 Failure Taxonomy에 "metamorphic relation failure"를 추가하고, Decision은 "pass"가 될 수 없다.
- 모든 항목이 "not tested"이면 Failure Taxonomy에 "validation shortfall"을 기록한다.

**N/A 허용:** Input의 Test Relations이 모두 "N/A (이유)"인 경우에만 허용. 이유를 이 섹션에도 복사한다.

**Anti-patterns:**
- ID 없이 관계명만 기재
- fail인데 Decision이 pass인 경우

---

## Section 7: Decision

```markdown
## [Decision]

- **판정:** {pass / fail / uncertain}
- **판정 근거 요약:** {2-3문장. Oracle Evaluation과 Failure Taxonomy를 종합한 결론}
- **신뢰도:** {high / medium / low}
- **신뢰도 근거:** {medium 이하인 이유. high면 "사용된 오라클이 충분하고 실패 유형이 발견되지 않음"으로 갈음 가능}
```

**Required sub-fields:** 판정, 판정 근거 요약, 신뢰도, 신뢰도 근거 -- 4개 모두 필수.

**Format constraints:** 불릿 리스트.

**판정 규칙:**

| 조건 | 판정 |
|------|------|
| 모든 oracle pass + critical/major 없음 + TBD 없음 | `pass` |
| critical 또는 major 실패 존재 | `fail` |
| oracle 결과가 uncertain + critical/major 없음 | `uncertain` |
| TBD 필드 잔존 + 실질적 실패 없음 | `uncertain` |
| oracle 2개 미만 사용 | `uncertain` (validation shortfall) |

**금지 조합:** `pass` + 신뢰도 `low` -- 이 조합은 모순이므로 금지한다.

**N/A 허용:** 불가. 어떤 상황에서든 3가지 중 하나로 판정해야 한다.

**Anti-patterns:**
- 판정 근거 요약에 새로운 정보를 추가하는 것 (이전 섹션을 종합만 한다)
- 신뢰도 근거 없이 high를 기재하는 것

---

## Section 8: Next Action

```markdown
## [Next Action]

- **다음 행동:** {구체적인 다음 단계}
- **담당:** {Claude / 사용자 / 외부 검증자}
- **선행 조건:** {다음 행동을 수행하기 위해 필요한 전제 조건}
- **예상 실패 유형 (다음 루프):** {다음 루프에서 주의할 FT-XX 유형 1-3개}
```

**Required sub-fields:** 다음 행동, 담당, 선행 조건, 예상 실패 유형 -- 4개 모두 필수.

**Format constraints:** 불릿 리스트. Decision이 pass면 "추가 검증 불필요. 완료." 가능.

**N/A 허용:** 불가. Decision이 pass여도 "완료" 행동을 명시한다.

**Anti-patterns:**
- "전체를 다시 점검하자" (범위 축소 원칙 위반)
- 담당자가 불명확한 경우
- 예상 실패 유형을 생략하는 경우

---

## Section 9: Knowledge Assets Updated

```markdown
## [Knowledge Assets Updated]

### New Hidden Semantic Rules Discovered
- {이번 루프에서 발견된 숨은 의미 규칙. 없으면 "없음"}
- **기록 위치:** {runtime/에 새 파일 작성 완료 / proposals/에 제안서 작성 완료}

### Failure Patterns Cataloged
- {재사용 가능한 실패 패턴. 없으면 "없음"}
- **기록 위치:** {runtime/ 또는 proposals/}

### Oracle Effectiveness Notes
- {유효했거나 무효했던 oracle에 대한 메모}
- **기록 위치:** {runtime/}

### Reusable Test Relations
- {다른 루프에서도 재사용 가능한 MR-XX / MR-P-XXX relation, 또는 특정 실행 맥락이 중요한 경우 그 instance ID}
- **기록 위치:** {runtime/ 또는 proposals/}

### Updated Spec Clarifications
- {스펙에 추가/수정이 필요한 항목. 없으면 "없음"}
- **기록 위치:** {proposals/에 제안서 작성}
```

**Required sub-fields:** 5개 하위 섹션 모두 필수. 각 하위 섹션에 기록 위치를 명시한다.

**Format constraints:** 불릿 리스트 + 기록 위치 명시.

**기록 위치 규칙:**
- 새로운 발견 사항 -> `runtime/`에 새 파일로 기록
- `core/` 또는 `project/` 변경이 필요한 사항 -> `proposals/`에 제안서 작성
- 기존 `runtime/` 파일 수정 -> 금지. 새 파일로 추가한다.

**N/A 허용:** 불가. 각 하위 필드에 "없음"은 허용하지만, 모든 하위 필드가 동시에 "없음"이면 "정말로 학습할 내용이 없는지" 1회 자기 검토를 수행한 뒤 결과를 기술한다.

**Anti-patterns:**
- hidden semantic rule을 발견했지만 이 섹션에 미기록 (orphaned knowledge)
- Failure Taxonomy에 새 패턴이 있지만 Failure Patterns에 미반영
- 기록 위치를 명시하지 않는 것
- `core/` 또는 `project/`에 직접 기록했다고 쓰는 것

---

## N/A Allowance Summary

| 섹션 | N/A 허용 | 조건 |
|------|----------|------|
| Loop Goal | 불가 | -- |
| Excluded Scope | 불가 | -- |
| Observed Signals | 부분 허용 | Logs와 Intermediate Artifacts만 Input에서 N/A였을 때 허용 |
| Failure Taxonomy | 불가 | 실패 없으면 "(없음)" 행으로 명시 |
| Oracle Evaluation | 불가 | 최소 2개 oracle 필수 |
| Metamorphic / Property Checks | 조건부 허용 | Input의 Test Relations이 모두 N/A일 때만 |
| Decision | 불가 | -- |
| Next Action | 불가 | -- |
| Knowledge Assets Updated | 불가 | -- |

---

## Anti-Patterns Summary

| # | Anti-Pattern | 위반 유형 |
|---|-------------|----------|
| 1 | Free-Form Praise: "코드가 잘 작성되어 있습니다" | 주관적 평가. oracle이 아니다. |
| 2 | Vague Assessment: "대체로 잘 동작하는 것 같습니다" | "것 같다"는 판정이 아니다. |
| 3 | Missing Oracle Attribution: "이 기능은 정상 동작합니다" | 어떤 OR-XX로 판정했는지 불명. |
| 4 | Skipped Taxonomy Review: "별다른 문제가 발견되지 않았습니다" | 전수 검토 증거 없음. |
| 5 | Orphaned Knowledge: 발견했지만 Knowledge Assets에 미기록 | 지식 환류 실패. |
| 6 | Confidence Without Justification: pass + high인데 oracle 1개 | 근거 불충분. |
| 7 | Scope Creep Without Declaration: Excluded Scope 위반 미기록 | scope violation 미분류. |
| 8 | Wrong Write Target: Knowledge Assets를 core/에 직접 기록 | 권한 매트릭스 위반. |

---

## Complete Template (Copy-Paste Ready)

```markdown
## [Loop Goal]

- **목표:**
- **루프 번호:**
- **선행 루프 참조:**

## [Excluded Scope]

-

**실제 위반 여부:**

## [Observed Signals]

### Execution Results
-

### Logs
-

### Intermediate Artifacts
-

### Anomalies
-

## [Failure Taxonomy]

| ID | 분류명 | 근거 | 심각도 | upstream/downstream |
|----|--------|------|--------|---------------------|
|    |        |      |        |                     |

## [Oracle Evaluation]

| ID | 오라클 | 판정 결과 | 판정 근거 | 부족한 오라클 |
|----|--------|----------|----------|-------------|
|    |        |          |          |             |

## [Metamorphic / Property Checks]

| ID | 관계/속성 | 통과/실패 | 실패 시 깨진 관계 |
|----|-----------|----------|-----------------|
|    |           |          |                 |

## [Decision]

- **판정:**
- **판정 근거 요약:**
- **신뢰도:**
- **신뢰도 근거:**

## [Next Action]

- **다음 행동:**
- **담당:**
- **선행 조건:**
- **예상 실패 유형 (다음 루프):**

## [Knowledge Assets Updated]

### New Hidden Semantic Rules Discovered
-
- **기록 위치:**

### Failure Patterns Cataloged
-
- **기록 위치:**

### Oracle Effectiveness Notes
-
- **기록 위치:**

### Reusable Test Relations
-
- **기록 위치:**

### Updated Spec Clarifications
-
- **기록 위치:**
```

---

## Example: Blender Fracture -- Boundary/Thickness Axis Misfill

아래는 Blender 유리 파쇄 프로젝트에서 "outer boundary" 해석 오류를 검증한 루프 3의 실제 출력 예시다.

---

## [Loop Goal]

- **목표:** Blender 유리 파쇄 스크립트의 outer boundary 계산 로직이 "가장 얇은 축(thinnest axis)을 제외"하는 규칙을 올바르게 구현했는지 검증한다.
- **루프 번호:** 3
- **선행 루프 참조:** 루프 2에서 fail 판정. impact distance 계산이 여전히 3축 bbox를 사용하고 있었음. Next Action: boundary.py의 `get_outer_boundary()` 함수가 thinnest axis를 실제로 제외하는지 코드 수준에서 재검증.

## [Excluded Scope]

- 파쇄 시뮬레이션 물리 엔진 파라미터 (damping, stiffness 등)
- 렌더링 출력 (셰이더, 조명)
- Voronoi/Boolean 파쇄 모드 -- glass fracture 전용 루프
- UI/UX 변경

**실제 위반 여부:** 위반 없음. 변경은 boundary.py와 impact_calc.py에 한정됨.

## [Observed Signals]

### Execution Results
- `python boundary.py --test` 실행 -> 3개 테스트 케이스 중 3개 통과
  - Case 1 (10x10x0.5): excluded axis = Z, boundary axes = [X, Y] -- 정상
  - Case 2 (5x0.3x8): excluded axis = Y, boundary axes = [X, Z] -- 정상
  - Case 3 (0.2x6x6): excluded axis = X, boundary axes = [Y, Z] -- 정상
- `python impact_calc.py --compare` 실행 -> 수정 후 impact distance가 수정 전보다 12-38% 감소 확인

### Logs
- `boundary.py` 로그: `[INFO] Excluded axis: Z, thickness: 0.500` (Case 1)
- `boundary.py` 로그: `[INFO] Excluded axis: Y, thickness: 0.300` (Case 2)
- `boundary.py` 로그: `[INFO] Excluded axis: X, thickness: 0.200` (Case 3)
- `impact_calc.py` 로그: `[INFO] Impact distance (2-axis): 5.000` (Case 1)

### Intermediate Artifacts
- `/output/impact_comparison.csv` -- 수정 전/후 impact distance 비교 테이블 (3 케이스)
- `/output/boundary_viz_case1.png` -- Case 1 boundary 시각화 (XY 평면만 표시됨)

### Anomalies
- 정육면체 입력(10x10x10)을 추가 테스트한 결과, excluded axis가 항상 Z로 고정됨. 동률 처리 로직이 "마지막 축 선택" 하드코딩으로 구현되어 있음. 스펙에 이 경우의 동작이 정의되어 있지 않음.
- `bbox_utils.py`의 `get_dimensions()`가 로컬 스케일을 사용하고 있음. 월드 스케일과 다를 때 결과가 달라질 수 있음.

## [Failure Taxonomy]

| ID | 분류명 | 근거 | 심각도 | upstream/downstream |
|----|--------|------|--------|---------------------|
| FT-02 | hidden semantic rule misfill | "outer boundary" 정의가 3단계를 거치며 변질: (1) 스펙 "thickness axis 제외" -> (2) 초기 구현 "bbox 전체 사용" -> (3) 동률 축 처리 미정의. 루프 1-2에서 역추적 발견. | major | upstream: 스펙 #GF-012의 동률 케이스 미정의 -> downstream: boundary.py의 Z축 폴백 하드코딩 |
| FT-01 | spec gap | 정육면체(모든 축 동일 두께) 입력 시 어떤 축을 제외할지 스펙에 정의 없음. Anomalies에서 발견. | major | upstream: 스펙 #GF-012 -> downstream: boundary.py `get_outer_boundary()` |
| FT-09 | edge-case fragility | 비균일 월드 스케일 적용 시 thinnest axis 판정이 시각적 결과와 불일치할 수 있음. | minor | upstream: bbox_utils.py -> downstream: boundary.py |

## [Oracle Evaluation]

| ID | 오라클 | 판정 결과 | 판정 근거 | 부족한 오라클 |
|----|--------|----------|----------|-------------|
| OR-01 | spec oracle | uncertain | 스펙 #GF-012의 "thickness axis 제외" 규칙은 구현됨. 그러나 동률 케이스 스펙 부재로 완전한 pass 불가. | human oracle -- 동률 케이스 의도 확인 필요 |
| OR-03 | execution oracle | pass | 3개 표준 테스트 케이스 모두 통과. 입력 -> 기대 출력 일치. | negative oracle -- 비정상 입력 커버리지 부족 |
| OR-04 | observability oracle | pass | 요구된 모든 로그 출력 확인. excluded axis와 impact distance 값 추적 가능. | 이 오라클 범위 내 충분 |
| OR-05 | comparison oracle | pass | 수정 후 impact distance 감소 확인. 3축 -> 2축 전환 과대 계산 제거. | 이 오라클 범위 내 충분 |
| OR-06 | metamorphic oracle | pass | MR-P-001, MR-P-002, MR-06-FRAC-001, MR-06-FRAC-002 모두 통과. | 이 오라클 범위 내 충분 |
| OR-07 | negative oracle | uncertain | 정육면체 입력 시 에러/경고 없이 Z축 폴백 처리. 의도된 동작인지 불분명. | human oracle -- 경고 발생 여부 확인 필요 |

## [Metamorphic / Property Checks]

| ID | 관계/속성 | 통과/실패 | 실패 시 깨진 관계 |
|----|-----------|----------|-----------------|
| MR-P-002 | 유리판 90도 회전(X<->Z) 시 excluded axis가 대응 회전하고 impact distance 절대값이 유지됨 | pass | -- |
| MR-P-001 | thickness axis 값 2배 증가 시 outer boundary 크기 불변 | pass | -- |
| MR-06-FRAC-001 | outer boundary 축 수 = 항상 2 (property) | pass | -- |
| MR-06-FRAC-002 | excluded axis의 bbox 크기 <= 나머지 2축 bbox 크기 (property) | pass | -- |

## [Decision]

- **판정:** uncertain
- **판정 근거 요약:** 핵심 기능(thinnest axis 제외)은 올바르게 구현되었고 OR-03, OR-05, OR-06 모두 pass. 그러나 OR-01과 OR-07이 uncertain이며, 이는 정육면체(동률 축) 케이스에 대한 스펙 부재(FT-01, major) 때문이다. major 심각도 실패가 존재하므로 pass 조건 미충족.
- **신뢰도:** medium
- **신뢰도 근거:** 표준 케이스 high 신뢰도이나, 동률 케이스와 비균일 스케일 미검증으로 전체 medium 하향.

## [Next Action]

- **다음 행동:** 정육면체(동률 축) 입력에서 어떤 축을 제외해야 하는지 사용자에게 tiebreaker 규칙을 확인한다.
- **담당:** 사용자
- **선행 조건:** OR-01과 OR-07의 uncertain 근거를 함께 제시해야 한다.
- **예상 실패 유형 (다음 루프):** FT-01 (동률 케이스 spec gap), FT-02 (동률 규칙 misfill), FT-09 (비균일 스케일)

## [Knowledge Assets Updated]

### New Hidden Semantic Rules Discovered
- **"exclude thinnest axis" 규칙:** "outer boundary"는 thinnest axis를 제외한 2축으로 정의됨. 3단계에 걸쳐 의미 변질 관찰.
- **동률 축 미정의 규칙 (발견, 미해소):** 모든 축 동일 두께 시 제외 축 미정의. 현재 Z축 하드코딩 폴백.
- **기록 위치:** `.vkl/runtime/case_logs/CL-2026-03-12-003.md`, `.vkl/runtime/temporary_hypotheses/HYP-2026-03-12-001.md`

### Failure Patterns Cataloged
- **다단계 의미 변질 패턴:** 스펙 용어가 구현 과정에서 여러 단계를 거치며 원래 의미를 잃는 패턴. 향후 스펙 용어가 2개 이상의 함수를 거칠 때 각 단계에서 의미 보존 검증 필요.
- **기록 위치:** `.vkl/runtime/observations/OBS-2026-03-12-002.md`

### Oracle Effectiveness Notes
- comparison oracle(OR-05)이 이 사례에서 가장 효과적. 수정 전/후 수치 비교가 문제 존재와 해결을 가장 명확히 입증.
- negative oracle(OR-07)이 동률 케이스 spec gap 발견에 결정적.
- spec oracle(OR-01) 단독으로는 불충분. 스펙 자체에 gap이 있었기 때문.
- **기록 위치:** `.vkl/runtime/oracle_runs/ORUN-2026-03-12-004.md`

### Reusable Test Relations
- **MR-P-002 (Axis Rotation Correspondence):** 오브젝트 90도 회전 시 excluded axis가 대응 회전하고 경계 계산 의미가 유지된다. 다른 geometry 함수에도 적용 가능.
- **MR-06-FRAC-002 (excluded axis minimality property):** 제외 축 크기 <= 나머지 축. boundary 관련 모든 함수의 불변 속성.
- **기록 위치:** `.vkl/runtime/observations/OBS-2026-03-12-003.md`

### Updated Spec Clarifications
- **#GF-012 추가 필요:** 모든 축 동일 두께 시 동작 정의.
- **#GF-012 명확화 필요:** "두께" 기준이 로컬 스케일인지 월드 스케일인지 명시.
- **기록 위치:** `.vkl/proposals/PROP-2026-03-12-001.md` 제안서 작성 완료
