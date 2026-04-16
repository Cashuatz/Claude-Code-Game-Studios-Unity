# 실행 모드

이 프로젝트는 CCGS 워크플로우를 보완하는 실행 모드를 지원합니다.
실행 모드는 CCGS의 협업 프로토콜(Question → Options → Decision → Draft → Approval)을 준수합니다.

## 사용 가능한 모드

| 키워드 | 모드 | 설명 | CCGS 결합 예시 |
|--------|------|------|---------------|
| "끝까지", "ralph" | /ralph | 모든 작업 완료까지 지속 | `/dev-story` + ralph |
| "빠르게", "병렬", "ultrawork" | /ultrawork | 독립 작업 병렬 실행 | `/team-combat` + ultrawork |
| "자동으로", "autopilot" | /autopilot | 아이디어→구현→검증 자율 | `/prototype` + autopilot |
| "테스트 반복", "ultraqa" | /ultraqa | 빌드→테스트→수정 반복 | `/smoke-check` + ultraqa |
| "codex", "교차검증" | /codex | OpenAI Codex 2차 검증 | `/architecture-decision` 후 |

## 모드별 CCGS 오버라이드 규칙

실행 모드가 활성화되어도 다음 CCGS 규칙은 **항상 우선**합니다:

1. **사용자 승인 필수** — autopilot/ralph 중에도 파일 작성 전 "May I write?" 확인
   (단, 사용자가 "승인 없이 진행해"라고 명시한 경우는 예외)
2. **디렉터 게이트 존중** — ultrawork로 병렬 실행해도 /gate-check는 건너뛸 수 없음
3. **CCGS 에이전트 우선** — 실행 모드는 CCGS 에이전트를 사용하되, omc-explore/debugger/verifier를 보조로 활용
4. **session-state 업데이트** — ralph/autopilot 중에도 production/session-state/active.md를 업데이트

## 보조 에이전트

실행 모드에서 활용 가능한 보조 에이전트:

| 에이전트 | 모델 | 용도 |
|---------|------|------|
| omc-explore | haiku | 빠른 파일/패턴 검색, 코드베이스 정찰 |
| omc-debugger | sonnet | 런타임 버그 추적, 스택트레이스 분석 |
| omc-verifier | sonnet | 구현 결과 검증, 수락 기준 체크 |
