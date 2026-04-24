---
name: .claude/ 수정은 stage+mv 패턴 필수
description: 이 프로젝트에서 .claude/ 하위 파일(skills/agents/docs 등)을 수정할 때 Edit/Write 직접 호출 금지, .stage/ 에 쓰고 Bash mv 로 배포
type: feedback
originSessionId: 6b5e8102-733f-4542-a12c-ee583d004df0
---
이 프로젝트에서 `.claude/` 하위 파일(skills, agents, docs, settings 등)을 수정할 때:

- **금지**: `Edit` / `Write` 도구로 `.claude/<path>` 직접 쓰기.
- **필수 워크플로**:
  1. `.stage/claude/<원래상대경로>` 에 파일을 쓴다.
  2. Bash `mv` 한 방으로 `.claude/<원래상대경로>` 로 배포.
  3. 커밋에는 `.claude/` 변경만 포함. `.stage/` 는 `.gitignore` 에 등록됨.
- **예외**: 읽기 전용(`Read`/`Glob`/`Grep`) 은 자유.

**Why**: `.claude/` 직접 수정은 Edit/Write 도구가 매번 권한 프롬프트를 띄워 수강생 흐름을 끊는다. `mv` 한 방은 프롬프트를 한 번만 발생시켜 흐름 유지가 쉽다. 사용자(강사)가 Proto 브랜치 수강생 데모 중 반복되는 프롬프트 문제를 겪고 2026-04-23 에 명시적으로 요청.

**How to apply**:
- Proto 브랜치에서 `/proto-start` 등 스킬 수정 필요 시 즉시 적용.
- 다른 브랜치에서도 `.claude/` 수정은 동일 패턴 권장.
- 규칙은 프로젝트 `CLAUDE.md` 작업 규칙 섹션 "5. `.claude/` 수정 — Stage + mv 패턴 필수" 에도 박혀 있음. 두 곳 모두 load 됨.
- `.gitignore` 에 `.stage/` 등록되어 있음 — 이미 반영됨.
