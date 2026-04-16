# VKL 검증 원칙 (항상 적용)

이 프로젝트는 Validation-Knowledge-Loop(VKL) 검증 체계를 사용합니다.
VKL 문서는 `.vkl/` 디렉토리에 있으며, 아래 원칙은 모든 작업에 항상 적용됩니다.

## 판단 원칙

- 모든 PASS/FAIL/UNCERTAIN 판정에는 **oracle ID**를 붙인다 (`.vkl/core/ORACLE_CATALOG.base.md` 참조)
- Oracle 없는 판정은 무효다
- **Verification**(기계적 검증)과 **Validation**(의미 검증)을 분리한다
- 명세 빈칸은 **spec gap**(FT-01)으로 분류한다 — 추측으로 채우지 않는다
- 숨은 의미 규칙이 필요하면 **misfill 가능성**(FT-02)으로 표기한다
- 로그/증거가 없으면 원인 단정을 보류한다

## 6대 금지 규칙

1. 항목별 검증 없이 "괜찮아 보인다" 판단 금지
2. Verification 통과 = Validation 통과로 간주 금지
3. 스펙 빈칸을 추측으로 채우기 금지 → 에스컬레이션
4. 로그 없이 원인 단정 금지
5. 전면 재작성 제안 금지 → 최소 변경
6. 숨은 의미 규칙을 사실로 확정 금지

## 권한 모델

- `.vkl/core/*` — **read-only**. Claude가 절대 수정하지 않는다.
- `.vkl/project/*` — **read-only**. Claude가 절대 수정하지 않는다.
- `.vkl/runtime/*` — **append-only**. 새 파일 추가만 가능, 기존 파일 수정/삭제 금지.
- `.vkl/proposals/*` — **append-only**. 규칙 변경이 필요하면 proposal 파일을 생성한다.

## 기록 원칙

- 검증 루프 실행 기록 → `.vkl/runtime/case_logs/CL-YYYY-MM-DD-NNN.md`
- 관찰값 → `.vkl/runtime/observations/OBS-YYYY-MM-DD-NNN.md`
- Oracle 실행 결과 → `.vkl/runtime/oracle_runs/ORUN-YYYY-MM-DD-NNN.md`
- 잠정 가설 → `.vkl/runtime/temporary_hypotheses/HYP-YYYY-MM-DD-NNN.md`
- runtime 기록은 **append-only**. 기존 기록 덮어쓰기 대신 새 파일로 supersede.

## Proposal 생성 조건

다음 상황에서는 `.vkl/proposals/PROP-YYYY-MM-DD-NNN.md`를 생성한다:
- 새 failure type이 필요할 때
- 새 oracle이 필요할 때
- checklist 항목 추가가 필요할 때
- 새 metamorphic relation이 필요할 때
- 프로젝트 문맥의 숨은 의미 규칙이 발견됐을 때
- 기존 규칙 충돌이 발생했을 때

**Claude는 core/ 또는 project/ 를 직접 수정하지 않는다. proposal을 생성하면 인간이 검토 후 수동 적용한다.**

## 에스컬레이션 조건

다음 조건에서 Claude는 작업을 멈추고 사용자에게 넘긴다.
ralph/autopilot 등 실행 모드 중에도 적용:

| 조건 | 심각도 |
|------|--------|
| Validation 판단만 남은 경우 | Advisory |
| Oracle 간 충돌 | **Blocking** |
| 증거 없이 높은 확신의 판단 필요 | **Blocking** |
| 테스트 통과했으나 품질 의심 | Advisory |
| GDD에 미정의된 핵심 사양 발견 | **Blocking** |
| 검증 신뢰도 LOW | Advisory |
| 같은 FT가 3회 이상 연속 | Advisory |
| 수정이 선언 스코프 초과 | Advisory |
| 외부 검증 도구 사용 불가 | **Blocking** |
| 미정의 파라미터가 판단 핵심 | **Blocking** |

## CCGS 스킬과의 연결

| CCGS 스킬 | VKL 활용 |
|-----------|---------|
| `/bug-report` | 실패 분류 시 FT-XX ID 사용 (`.vkl/core/FAILURE_TAXONOMY.base.md`) |
| `/bug-triage` | FT-XX 기반 우선순위 판단 |
| `/code-review` | Oracle 근거 명시 (OR-XX) |
| `/story-done` | Verification 항목(VC-V) 체크 후 Validation 항목(VC-D)은 사용자에게 |
| `/gate-check` | Director Gate 결과에 Oracle ID 기재 |
| `/qa-plan` | Metamorphic Relations(MR-XX)을 테스트 케이스로 활용 |
| `/balance-check` | MR-G-01(단조성), MR-G-02(불변성) 패턴 적용 |
| `/verify` | 전체 VKL 검증 루프 실행 (계층 2) |
