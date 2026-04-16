---
Document Role: Runtime Guide
Update Policy: Claude write-allowed (append-only). 기존 파일 덮어쓰기 금지. 새 파일 추가만 가능.
Owner: Session operator (Claude or Human)
Scope: This project, current and future sessions
---

# Runtime Layer — 운영 가이드

## 1. Runtime 폴더 목적

`runtime/` 폴더는 세션 중 생성되는 모든 기록물의 저장소다.

검증 루프(Validation-Knowledge-Loop)는 반복적으로 실행되며, 각 실행의 결과, 관찰, 가설, oracle 판정을 누적 기록으로 남긴다. 이 기록은 나중에 proposal 생성, 규칙 개선, 지식 정제의 근거가 된다.

**핵심 원칙: append-only.** 기존 기록을 수정하거나 삭제하지 않는다. 판단이 바뀌었을 때는 새 파일을 추가하고 이전 파일을 참조한다.

### 하위 폴더 역할

| 폴더 | 역할 |
|------|------|
| `case_logs/` | 검증 루프 1회 실행의 전체 결과를 기록한다. loop goal, 사용된 oracle, 판정 결과, 다음 액션을 포함한다. |
| `observations/` | 세션 중 관찰된 신호, 로그, 중간 산출물을 기록한다. 분석 전 날것의 데이터를 보존하는 목적이다. |
| `oracle_runs/` | 개별 oracle 실행 결과를 기록한다. oracle ID, 입력, 판정, 근거, 판정 한계를 포함한다. |
| `temporary_hypotheses/` | 아직 검증되지 않은 잠정 가설을 기록한다. 검증 결과가 바뀌거나 더 정밀한 가설이 생기면 새 HYP 파일로 후속 기록하고, 기준 문서 반영이 필요하면 proposal로 승격 요청한다. |

---

## 2. 파일명 규칙

모든 파일명은 날짜 기반 시퀀스를 사용한다. 같은 날 여러 파일이 생성될 경우 NNN을 001부터 순차 증가시킨다.

| 폴더 | 패턴 | 예시 |
|------|------|------|
| `case_logs/` | `CL-YYYY-MM-DD-NNN.md` | `CL-2026-04-15-001.md` |
| `observations/` | `OBS-YYYY-MM-DD-NNN.md` | `OBS-2026-04-15-001.md` |
| `oracle_runs/` | `ORUN-YYYY-MM-DD-NNN.md` | `ORUN-2026-04-15-001.md` |
| `temporary_hypotheses/` | `HYP-YYYY-MM-DD-NNN.md` | `HYP-2026-04-15-001.md` |

**주의:** 파일명에 오타나 날짜 오류가 있어도 덮어쓰지 않는다. 새 파일에 정정 내용을 기록하고 이전 파일을 참조한다.

---

## 3. Case Log 형식

`case_logs/` 에 저장되는 검증 루프 결과 기록의 전체 템플릿이다.

```markdown
# Case Log: CL-YYYY-MM-DD-NNN

## Metadata
- Case ID: CL-YYYY-MM-DD-NNN
- Date: YYYY-MM-DD
- Session: [세션 식별자]
- Operator: Claude / Human

## Loop Goal
[이번 루프의 검증 목표 — 1~2문장으로 구체적으로 기술]

## Input Summary
[사용된 입력 자료 요약 — 어떤 파일, 로그, 실행 결과를 기반으로 했는지]

## Observed Signals
- Logs: [관찰된 로그 내용 또는 OBS 파일 참조]
- Artifacts: [중간 산출물 — 생성된 파일, 스크린샷, 출력 데이터 등]
- Execution: [실제 실행 결과 요약]

## Applied Oracles
| Oracle ID | 판정 결과 | 근거 |
|-----------|----------|------|
| OR-XX     | pass / fail / uncertain | [근거 요약] |

## Failure Classification
| Failure ID | 분류명 | 심각도 | upstream/downstream |
|------------|--------|--------|---------------------|
| FT-XX      | [분류명] | critical / major / minor | upstream / downstream / isolated |

## Decision
[pass / fail / uncertain]

[판정 근거를 1~3문장으로 기술. uncertain인 경우 무엇이 불명확한지 명시]

## Next Action
[다음에 실행할 가장 좁은 루프 1개 — 구체적인 행동 단위로 기술]

## Related Proposal
[관련 proposal ID (예: PROP-2026-04-15-001), 없으면 "없음"]

## Lessons
[이번 루프에서 배운 것 — 다음 루프나 rule 개선에 반영될 내용]
```

**작성 지침:**
- `Loop Goal`은 "무엇을 검증하는가"에 집중한다. "테스트한다"가 아니라 "X가 Y 조건에서 Z를 만족하는지 검증한다"처럼 기술한다.
- `Applied Oracles`는 비워둘 수 없다. 오라클이 불충분하면 그 사실을 기록하고 `Decision`에서 insufficient oracle 또는 escalation 상태를 명시한다.
- `Next Action`은 반드시 1개만 기술한다. 여러 후속 행동이 필요하면 가장 우선순위가 높은 것 1개를 선택한다.

---

## 4. Oracle Run 기록 형식

`oracle_runs/` 에 저장되는 개별 oracle 실행 결과의 템플릿이다.

```markdown
# Oracle Run: ORUN-YYYY-MM-DD-NNN

- Oracle ID: OR-XX / OR-P-XXX
- Target: [검증 대상 — 어떤 동작, 속성, 조건을 판정하는가]
- Input: [oracle에 전달된 입력 — 구체적인 값, 파일, 상태]
- Result: pass / fail / uncertain
- Evidence: [판정 근거 — 실제 관찰된 수치, 로그, 출력]
- Limitations observed: [이 oracle로 판정할 수 없었던 것 — 커버하지 못한 케이스, 관찰 한계]
```

**작성 지침:**
- 하나의 ORUN 파일에는 하나의 oracle 실행만 기록한다.
- 같은 oracle을 다른 입력으로 여러 번 실행했다면 각각 별도 파일로 기록한다.
- `Limitations observed`는 "없음"으로 생략하지 않는다. 모든 oracle에는 판정 한계가 존재한다.

---

## 5. Temporary Hypothesis 형식

`temporary_hypotheses/` 에 저장되는 잠정 가설의 템플릿이다.

```markdown
# Hypothesis: HYP-YYYY-MM-DD-NNN

- Hypothesis: [가설 내용 — 구체적이고 반증 가능한 형태로 기술]
- Confidence: low / medium / high
- Evidence for: [지지 근거 — 현재까지 관찰된 지지 데이터]
- Evidence against: [반대 근거 — 현재까지 관찰된 반례 또는 약점]
- Test plan: [검증 방법 — 어떤 oracle 또는 실험으로 검증할 수 있는가]
- Status: tentative / tested-pass / tested-fail
- Supersedes: [이 파일이 대체하거나 정정하는 이전 가설 ID (예: HYP-2026-04-15-001), 해당 시에만 기재]
```

**Status 의미:**

| Status | 설명 |
|--------|------|
| `tentative` | 아직 검증 전. 가설로서만 유효 |
| `tested-pass` | 검증 통과. proposal 작성 검토 대상 |
| `tested-fail` | 검증 실패. 기각됨 |

**가설 대체 규칙:** 기존 가설을 대체할 때는 새 `HYP` 파일을 만들고 `Supersedes` 필드에 이전 가설 ID를 적는다. 이전 파일은 수정하지 않는다.

---

## 6. Append-only 규칙

Runtime 폴더는 **절대 수정하지 않는다.** 모든 변경은 새 파일 추가로만 이루어진다.

### 구체적 규칙

**가설이 기각되거나 더 정밀한 가설로 대체되었을 때:**
1. 기존 `HYP` 파일을 수정하지 않는다.
2. 새 `HYP` 파일을 만든다.
3. 새 파일의 `Status`에 현재 검증 결과(`tested-fail` 또는 새 가설의 상태)를 기록한다.
4. 새 파일의 `Supersedes:`에 이전 HYP ID를 기록한다.

**Case Log의 Decision이 나중에 바뀌었을 때:**
1. 기존 `CL` 파일을 수정하지 않는다.
2. 새 `CL` 파일을 만들고 `Input Summary`에 "Revision of CL-이전ID"를 명시한다.
3. 이전 판정이 왜 바뀌었는지 `Lessons`에 기술한다.

**Oracle Run 결과가 잘못 기록되었을 때:**
1. 기존 `ORUN` 파일을 수정하지 않는다.
2. 새 `ORUN` 파일을 만들고 `Target` 항목 상단에 "Correction of ORUN-이전ID"를 명시한다.

**추가 기록 원칙:**
- correction / revision / supersession / addendum은 모두 **새 파일**로만 남긴다.
- 새 파일 본문에서 이전 ID를 명시적으로 참조한다.

---

## 7. 예시 — Blender Fracture 검증 케이스

다음은 `CL-2026-04-15-001.md`의 실제 작성 예시다. boundary 계산에서 thickness axis가 포함될 때 발생하는 fracture 오동작을 검증한 루프다.

```markdown
# Case Log: CL-2026-04-15-001

## Metadata
- Case ID: CL-2026-04-15-001
- Date: 2026-04-15
- Session: blender-fracture-boundary-debug-001
- Operator: Claude

## Loop Goal
Blender Cell Fracture에서 boundary 조각의 크기가 예상보다 크게 계산되는 원인이
thickness axis 포함 여부에 있는지 검증한다.

## Input Summary
- Blender 3.6 Cell Fracture 플러그인 소스 (cell_fracture/__init__.py)
- 테스트 오브젝트: 단순 박스 메시 (2m × 2m × 0.1m, thickness axis = Z)
- 재현 케이스: boundary_mode=True, source_limit=20 설정 시 일부 조각 크기 이상

## Observed Signals
- Logs: OBS-2026-04-15-001.md 참조 (Blender 콘솔 출력)
- Artifacts: 조각 크기 통계 — 정상 조각 평균 0.18m³, 비정상 boundary 조각 최대 1.4m³
- Execution: thickness axis(Z)를 포함한 bounding box 계산 시 Z 범위가 전체 오브젝트
  크기(0.1m)가 아닌 2m 기준으로 산출되는 것을 확인

## Applied Oracles
| Oracle ID | 판정 결과 | 근거 |
|-----------|----------|------|
| OR-01     | fail     | `.vkl/project/PROJECT_CONTEXT.md`의 outer boundary 정의와 달리 thickness axis가 boundary 계산에 포함됨 |
| OR-P-002  | fail     | 가장 얇은 축(Z)이 제외되지 않아 outer boundary 축 수가 2가 아닌 사실상 3축으로 동작 |
| OR-07     | uncertain | 다른 axis configuration과 동률 축 케이스에서 동일 증상이 재현되는지 추가 확인 필요 |

## Failure Classification
| Failure ID | 분류명 | 심각도 | upstream/downstream |
|------------|--------|--------|---------------------|
| FT-P-002   | Axis Detection Error | major | upstream |
| FT-P-001   | Boundary Definition Ambiguity | major | downstream |

## Decision
fail

thickness axis(Z)가 boundary bounding box 계산에서 제외되지 않아 조각 크기가
비정상적으로 산출됨. OR-03 판정 기준 명확한 fail. OR-07은 추가 검증 필요.

## Next Action
thickness axis를 boundary 계산 단계에서 명시적으로 제외한 후 동일 테스트 재실행.
(OR-03 재적용)

## Related Proposal
PROP-2026-04-15-001

## Lessons
Cell Fracture의 boundary 계산은 thickness axis를 암묵적으로 포함한다.
이 동작은 문서화되지 않은 암묵적 전제이며, `.vkl/project/FAILURE_TAXONOMY.project.md`
의 FT-P-002 check point와 `.vkl/project/ORACLE_CATALOG.project.md`의 OR-P-002 사용
가이드를 더 명시적으로 유지해야 함. → Proposal로 등록.
```

---

*이 파일 자체는 runtime/ 의 README이며 append-only 정책의 대상이 아닙니다. 내용 수정이 필요한 경우 proposal을 통해 요청하세요.*
