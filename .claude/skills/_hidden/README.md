# `_hidden/` — Proto 브랜치에서 비활성화된 스킬

이 디렉토리의 스킬들은 **프로토타이핑 경량 버전(Proto 브랜치)**에서 비활성화되었습니다.

Claude Code는 `.claude/skills/*/SKILL.md` 패턴만 스캔하므로, 이 하위 디렉토리의 스킬은 자동 로드되지 않습니다.

## 복원 방법

- **개별 복원**: `git mv .claude/skills/_hidden/<name> .claude/skills/<name>`
- **전체 복원 (main 머지)**: `main` 브랜치로 전환하면 이 이동이 적용되지 않아 모든 스킬이 다시 활성화됩니다.

## Proto에서 노출된 스킬 (11개)

- `brainstorm` — 게임 컨셉 아이디어 발굴
- `codex` — 교차 검증
- `compile-check` — C# 컴파일 확인
- `editor-layout` — 에디터 UI 규칙
- `help` — 다음 할 일 안내
- `learner` — 세션 지식 추출
- `note` — 세션 간 메모 저장
- `prototype` — 빠른 프로토타이핑
- `smoke-check` — 기본 작동 확인
- `unity-mcp` — Unity MCP 도구 안내
- `verify` — VKL 검증 루프 (의미 검증이 필요할 때)

**진입 방법**: Proto 브랜치는 전용 진입 스킬 없이 동작한다.
수강생이 첫 메시지에 한 문장 아이디어를 말하면 Claude 가 바로 받아
`design/proto-concept.md` 생성 → 시작 스크립트까지 진행한다.
(초기 버전에는 `/proto-start` 진입 스킬이 있었으나 장르 라우팅·PlayerController
샘플·자동 push 가 최신 규칙과 어긋나 제거됨.)

## 왜 숨겼는가

이 86개 중 대부분은 **정통 스튜디오 워크플로**(GDD → 아키텍처 → 에픽 → 스토리 → 구현 → QA → 릴리스)를 전제로 한다. Proto 브랜치는 **20시간 이내 / 3분 빌드**가 목표로, 이 워크플로의 오버헤드를 감당할 수 없다.
