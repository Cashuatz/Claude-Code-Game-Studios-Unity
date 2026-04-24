# Proto 브랜치 — 프로토타이핑 경량 버전 (URP 3D)

> ⚠️ **이 브랜치는 프로토타이핑 경량 모드입니다.**
> 20시간 이내 / 3분 플레이 가능한 빌드를 목표로 합니다.
> 정식 CCGS 워크플로(GDD → 아키텍처 → 에픽 → 스토리 → 구현)는 `main` 브랜치에 있습니다.

## 기술 스택

- **Engine**: Unity 6.x LTS
- **Rendering**: URP (Universal Render Pipeline)
- **Project Type**: 3D
- **Input**: Unity Input System (신규)
- **Language**: C#
- **Build**: Windows standalone (WebGL도 가능)

## 진입 방법

사용자(프로토타이핑 대상자)에게는 딱 두 가지만 기억시킵니다:

1. **첫 메시지에 한 문장으로 아이디어 말하기** — 예: "좀비 피해서 3분 버티는 게임". 별도 명령이나 슬래시 스킬 없음. Claude 가 바로 받아 컨셉 정리 → 시작 스크립트까지 진행합니다.
2. Unity에서 Play 버튼 클릭.

세부 가이드는 `docs/PROTO-QUICKSTART.ko.md` 참조.

@docs/PROTO-QUICKSTART.ko.md

## 작업 규칙 (Proto 전용)

### 1. 파일 쓰기 — 승인 게이트 비활성화
- 기본 CCGS는 **"May I write this to [filepath]?"** 승인이 의무이지만, Proto 브랜치에서는 **비활성화**합니다.
- Claude는 **1줄 요약만 먼저 말하고 바로 파일을 씁니다**.
- 예: "플레이어 컨트롤러를 `Assets/Scripts/Proto/PlayerController.cs`에 작성합니다." → 즉시 Write.
- **예외 (항상 승인받기)**: 파일 삭제, `git reset --hard`, 다른 브랜치 체크아웃, `main` 수정 시도.

### 2. Git — Claude가 로컬까지 대행
- 사용자에게 **Git 개념·명령·GUI를 노출하지 않습니다**.
- 매 의미 단위 작업 종료 시 Claude가 다음을 자동 실행:
  ```
  git add <변경된 파일>
  git commit -m "<한글 메시지>"
  ```
- 커밋 메시지는 **한글**, 형식은 자유 (예: "추가: 점프 기능", "수정: 적이 이제 플레이어 따라감").
- **`git push` 는 Claude 가 실행하지 않습니다** — HTTPS credential 프롬프트가 수강생 흐름을 끊기 때문. 사용자가 "푸시해줘" / "원격에 올려줘" 라고 명시 요청한 경우에만 실행.
- `main` 병합·force-push·reset 관련 요청은 **거부**하고 사용자에게 확인.

### 3. Unity 작업 경계
- Claude는 `Assets/` 하위 **스크립트(.cs), 프리팹, ScriptableObject**만 씁니다.
- `ProjectSettings/`, `Packages/manifest.json` 수정은 **사용자 확인 후**에만.
- Unity Editor 조작은 **Unity MCP** 도구를 통해 진행 (MCP 서버가 안 떠 있으면 스크립트만 쓰고 사용자에게 "Unity에서 파일 새로고침하세요" 안내).

### 4. 디자인 문서 — 필수 아님
- GDD, ADR, 에픽, 스토리 파일 **모두 생략 가능**합니다.
- 필요한 것은 딱 하나: `design/proto-concept.md` (10줄 컨셉 파일) — 첫 대화에서 Claude 가 자동 생성. 템플릿은 `docs/templates/proto-game-concept.md`.

### 5. `.claude/` 수정 — Stage + mv 패턴 필수
- `.claude/` 하위 파일(skills, agents, docs, settings 등)은 `Edit` / `Write` 도구로 **직접 수정 금지**.
  해당 경로 쓰기는 매번 권한 프롬프트를 띄우므로 수강생 흐름을 끊는다.
- **정해진 워크플로**:
  1. 먼저 staging 경로에 파일을 쓴다: `.stage/claude/<원래상대경로>`
     - 예: `.claude/skills/prototype/SKILL.md` → `.stage/claude/skills/prototype/SKILL.md`
  2. `git add .stage/claude/...` 로 스테이징 (선택).
  3. Bash `mv` 한 방으로 실제 위치에 배포:
     ```bash
     mkdir -p .claude/skills/prototype && \
     mv .stage/claude/skills/prototype/SKILL.md .claude/skills/prototype/SKILL.md
     ```
  4. 커밋 시 `.claude/` 경로의 변경만 포함. `.stage/claude/` 는 `.gitignore` 로 제외 권장.
- **예외**: 읽기 전용 조회(`Read` / `Glob` / `Grep`) 는 자유.
- `.claude/worktrees/` 는 런타임 생성물이라 수정 불필요.

## 노출된 스킬·에이전트

**Skills (11개)**: `prototype`, `help`, `compile-check`, `smoke-check`, `brainstorm`, `codex`, `note`, `learner`, `editor-layout`, `unity-mcp`, `verify`

**Agents (9개)**: `prototyper`, `unity-specialist`, `gameplay-programmer`, `ui-programmer`, `art-director`, `game-designer`, `omc-explore`, `omc-debugger`, `omc-verifier`

나머지는 `.claude/skills/_hidden/`·`.claude/agents/_hidden/`에 이동되어 비활성화됨. `main` 브랜치에 전체 유지.

## 검증 원칙 (VKL)

Proto 브랜치에서도 **VKL(Validation-Knowledge-Loop) 검증 원칙은 유효**합니다.
복잡한 판단·검증이 필요할 때 `/verify`를 호출하거나 아래 원칙을 따릅니다.

@.claude/docs/vkl-bridge.md

@.vkl/SessionStart.md

## 코딩 스타일 (최소)

- **네이밍**: 클래스/메서드 PascalCase, 지역변수 camelCase, private 필드 `_camelCase`, 상수 UPPER_SNAKE_CASE
- **파일명**: 클래스명과 일치
- **네임스페이스**: `Proto.<Feature>` (예: `Proto.Player`, `Proto.Enemy`)
- **테스트**: Proto에서는 **선택**. 빠른 반복을 우선.

## Context Management (Proto 축약)

- 중요한 변경 후 `production/session-state/active.md`에 한 줄 기록 (선택).
- 컨텍스트가 70% 넘으면 `/compact` 실행.
- 자세한 규칙은 main 브랜치의 `.claude/docs/context-management.md` 참조.
