---
Document Role: Proposal Guide
Update Policy: Claude write-allowed. 새 proposal 파일 추가 가능. 기존 proposal 수정 금지.
Owner: Session operator (Claude or Human)
Scope: This project
---

# Proposals Layer — 운영 가이드

## 1. Proposals 폴더 목적

`proposals/` 폴더는 읽기 전용 문서(`core/`, `project/`)에 변경이 필요할 때 그 변경 내용을 제안하는 공간이다.

검증 루프 실행 중 새로운 failure type, oracle, checklist 항목, 또는 규칙 수정이 필요하다고 판단되더라도 Claude는 해당 문서를 직접 수정하지 않는다. 대신 이 폴더에 제안서(proposal)를 작성하고, 사람이 검토·승인한 뒤 직접 반영한다.

**핵심 원칙:** Claude의 역할은 proposal 작성까지다. 승격(promotion) 실행은 사람만 한다.

---

## 2. 언제 Proposal을 생성해야 하는가

다음 상황에서 proposal을 생성한다:

| 상황 | 변경 대상 문서 |
|------|--------------|
| 새로운 failure type 추가가 필요할 때 | `FAILURE_TAXONOMY` |
| 새로운 oracle 추가가 필요할 때 | `ORACLE_CATALOG` |
| 새로운 checklist 항목이 필요할 때 | `VALIDATION_CHECKLIST` |
| 새로운 test relation이 필요할 때 | `TEST_RELATIONS` |
| 기존 규칙 또는 정의의 수정이 필요할 때 | 해당 문서 |
| spec gap이 해소되어 확정 규칙으로 등록해야 할 때 | `ORACLE_CATALOG` 또는 `FAILURE_TAXONOMY` |
| operational principle 변경이 필요할 때 | `core/` 내 해당 문서 |

**판단 기준:** case log의 `Lessons` 항목이 "규칙/목록 추가 필요"를 언급하고 있다면 proposal을 작성한다. 단순 관찰 기록은 `observations/`에 남긴다.

---

## 3. 파일명 규칙

모든 proposal 파일은 날짜 기반 시퀀스를 사용한다.

| 패턴 | 예시 |
|------|------|
| `PROP-YYYY-MM-DD-NNN.md` | `PROP-2026-04-15-001.md` |

같은 날 여러 proposal이 생성될 경우 NNN을 001부터 순차 증가시킨다.

---

## 4. Proposal 형식

`proposals/` 에 저장되는 변경 제안서의 전체 템플릿이다.

```markdown
# Proposal: PROP-YYYY-MM-DD-NNN

## Metadata
- Proposal ID: PROP-YYYY-MM-DD-NNN
- Date: YYYY-MM-DD
- Author: Claude / Human
- Target Document: [변경 대상 파일 경로 — 예: .vkl/core/ORACLE_CATALOG.base.md]
- Target Layer: core / project

## Proposed Change
[구체적으로 무엇을 추가/수정/삭제하는지. 가능하면 실제 추가될 텍스트 블록을 그대로 기술]

## Reason
[왜 이 변경이 필요한지 — 어떤 검증 루프에서 이 필요성이 발견되었는가]

## Related Case Logs
[관련 CL-YYYY-MM-DD-NNN IDs — 없으면 "없음"]

## Related Failure IDs
[관련 FT-XX 또는 FT-P-XXX IDs — 없으면 "없음"]

## Related Oracle IDs
[관련 OR-XX 또는 OR-P-XXX IDs — 없으면 "없음"]

## Proposed New IDs
[새로 할당할 ID 목록 — 기존 목록에 추가되는 신규 항목의 임시 ID]
예: FT-P-003, OR-P-007

## Risk of Adoption
[이 변경을 반영했을 때의 리스크 — 기존 분류 체계와의 충돌, 하위 호환성 문제 등]

## Human Approval Status
- [ ] Reviewed
- [ ] Approved
- [ ] Promoted to target document
- Reviewer: [이름]
- Approval date: [날짜]
```

**작성 지침:**
- `Proposed Change`는 최대한 구체적으로 기술한다. 대상 문서의 어느 섹션에 무엇을 어떻게 추가/수정하는지 명시한다. 가능하면 실제 삽입될 마크다운 블록을 그대로 포함한다.
- `Proposed New IDs`는 프로젝트 임시 ID 체계(`-P-` 접두 포함)를 사용한다. 사람이 승인 시 최종 ID로 확정한다.
- `Risk of Adoption`은 "없음"으로 생략하지 않는다. 모든 변경에는 리스크가 존재한다.

---

## 5. Proposal 처리 규칙

### Claude가 할 수 있는 것

- `proposals/` 폴더에 새 `PROP-*.md` 파일을 생성한다.
- proposal 내용을 최대한 구체적으로 기술한다 (실제 삽입될 텍스트 포함).
- 하나의 case log에서 여러 proposal이 필요하면 각각 별도 파일로 작성한다.

### Claude가 할 수 없는 것

- `core/` 또는 `project/` 폴더의 파일을 직접 수정한다.
- proposal의 `Human Approval Status` 체크박스를 변경한다.
- "이미 승인된 것으로 간주"하고 대상 문서에 내용을 반영한다.
- 승인 여부와 무관하게 proposal 내용을 자동 적용한다.

### 사람이 할 일

1. proposal 파일을 검토한다.
2. `Human Approval Status` — `Reviewed`, `Approved` 항목을 체크한다.
3. 승인된 경우 대상 문서에 변경 내용을 직접 반영한다.
4. 반영 완료 후 proposal 파일의 `Promoted to target document`를 체크하고 날짜를 기입한다.
5. 반려된 경우 proposal 파일에 반려 이유를 코멘트로 추가한다. 파일은 삭제하지 않는다.

### 반려된 Proposal의 보존

반려된 proposal도 삭제하지 않는다. 반려 이유를 파일 하단에 다음 형식으로 추가한다:

```markdown
## Rejection Note
- Rejected by: [이름]
- Rejection date: [날짜]
- Reason: [반려 이유]
```

---

## 6. 승격 금지 규칙 (CRITICAL)

```
절대 금지:
- Claude가 core/ 파일을 직접 수정
- Claude가 project/ 파일을 직접 수정
- Claude가 proposal의 Human Approval Status를 변경
- Claude가 "사람이 승인한 것으로 간주"하고 승격 실행
- Claude가 Proposed New IDs를 확정 ID로 취급하고 다른 문서에 직접 기입

허용:
- Claude가 proposals/ 에 새 파일 추가
- Claude가 runtime/ 에 새 파일 추가
- Claude가 proposal에서 대상 문서의 변경 내용을 구체적으로 기술
- Claude가 case log나 observation에서 proposal을 참조 (ID만 기재)
```

이 규칙을 위반하면 core/project 문서의 무결성이 훼손된다. Claude가 자의적으로 규칙을 확장하거나 수정할 수 없도록 이 구조가 설계되어 있다.

---

## 7. 예시 — Blender Fracture 검증 케이스

다음은 `PROP-2026-04-15-001.md`의 실제 작성 예시다. `CL-2026-04-15-001`에서 발견된 thickness axis 문제를 FAILURE_TAXONOMY의 FT-P-002 check point에 반영하기 위한 proposal이다.

```markdown
# Proposal: PROP-2026-04-15-001

## Metadata
- Proposal ID: PROP-2026-04-15-001
- Date: 2026-04-15
- Author: Claude
- Target Document: .vkl/project/FAILURE_TAXONOMY.project.md
- Target Layer: project

## Proposed Change
FAILURE_TAXONOMY.project.md 내 FT-P-002 (Axis Detection Error) 항목의
`Priority Check Points` 섹션에 다음 항목을 추가한다:

> - [ ] thickness axis가 boundary bounding box 계산에서 명시적으로 제외되었는가?
>   - 제외되지 않은 경우: thickness 방향의 전체 오브젝트 크기가 boundary 조각
>     크기에 누적되어 스케일 오류가 발생할 수 있음.
>   - 확인 방법: 오브젝트의 두께 방향 axis (예: Z) 기준 bounding box 계산 코드에서
>     해당 axis를 제외하는 분기가 존재하는지 검토.

## Reason
CL-2026-04-15-001 검증 루프에서 Blender fracture의 boundary 조각 크기가
비정상적으로 크게 산출되는 원인이 thickness axis 포함에 있음을 확인했다.
이 동작은 현재 FT-P-002의 `Priority Check Points`에 명시되어 있지 않아 동일 오류의
재발 시 탐지가 어렵다. check point 추가로 이후 루프에서 즉각 탐지 가능하도록 한다.

## Related Case Logs
CL-2026-04-15-001

## Related Failure IDs
FT-P-002

## Related Oracle IDs
OR-01, OR-P-002

## Proposed New IDs
없음 (기존 FT-P-002 항목 내 check point 추가이므로 신규 ID 불필요)

## Risk of Adoption
- FT-P-002가 다른 프로젝트(비-Blender 환경)에도 적용되는 범용 항목이라면
  "thickness axis"라는 Blender 특화 표현이 혼란을 줄 수 있음.
- 완화 방안: check point 설명에 "(Blender Cell Fracture 환경)" 태그를 붙여
  적용 범위를 명시.

## Human Approval Status
- [ ] Reviewed
- [ ] Approved
- [ ] Promoted to target document
- Reviewer:
- Approval date:
```

---

*이 파일 자체는 proposals/ 의 README이며 append-only 대상이 아닙니다. 내용 수정이 필요한 경우 proposal을 통해 요청하세요.*
