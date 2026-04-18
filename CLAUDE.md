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

1. `/proto-start` 입력 → 한 문장으로 아이디어 설명
2. Unity에서 Play 버튼 클릭

세부 가이드는 `docs/PROTO-QUICKSTART.ko.md` 참조.

@docs/PROTO-QUICKSTART.ko.md

## 작업 규칙 (Proto 전용)

### 1. 파일 쓰기 — 승인 게이트 비활성화
- 기본 CCGS는 **"May I write this to [filepath]?"** 승인이 의무이지만, Proto 브랜치에서는 **비활성화**합니다.
- Claude는 **1줄 요약만 먼저 말하고 바로 파일을 씁니다**.
- 예: "플레이어 컨트롤러를 `Assets/Scripts/Proto/PlayerController.cs`에 작성합니다." → 즉시 Write.
- **예외 (항상 승인받기)**: 파일 삭제, `git reset --hard`, 다른 브랜치 체크아웃, `main` 수정 시도.

### 2. Git — Claude가 완전 대행
- 사용자에게 **Git 개념·명령·GUI를 노출하지 않습니다**.
- 매 의미 단위 작업 종료 시 Claude가 다음을 자동 실행:
  ```
  git add <변경된 파일>
  git commit -m "<한글 메시지>"
  git push origin Proto
  ```
- 커밋 메시지는 **한글**, 형식은 자유 (예: "추가: 점프 기능", "수정: 적이 이제 플레이어 따라감").
- `main` 병합·force-push·reset 관련 요청은 **거부**하고 사용자에게 확인.

### 3. Unity 작업 경계
- Claude는 `Assets/` 하위 **스크립트(.cs), 프리팹, ScriptableObject**만 씁니다.
- `ProjectSettings/`, `Packages/manifest.json` 수정은 **사용자 확인 후**에만.
- Unity Editor 조작은 **Unity MCP** 도구를 통해 진행 (MCP 서버가 안 떠 있으면 스크립트만 쓰고 사용자에게 "Unity에서 파일 새로고침하세요" 안내).

### 4. 디자인 문서 — 필수 아님
- GDD, ADR, 에픽, 스토리 파일 **모두 생략 가능**합니다.
- 필요한 것은 딱 하나: `design/proto-concept.md` (10줄 컨셉 파일) — `/proto-start`가 자동 생성.

## 노출된 스킬·에이전트

**Skills (11개)**: `proto-start`, `prototype`, `help`, `compile-check`, `smoke-check`, `brainstorm`, `codex`, `note`, `learner`, `editor-layout`, `unity-mcp`

**Agents (9개)**: `prototyper`, `unity-specialist`, `gameplay-programmer`, `ui-programmer`, `art-director`, `game-designer`, `omc-explore`, `omc-debugger`, `omc-verifier`

나머지는 `.claude/skills/_hidden/`·`.claude/agents/_hidden/`에 이동되어 비활성화됨. `main` 브랜치에 전체 유지.

## 코딩 스타일 (최소)

- **네이밍**: 클래스/메서드 PascalCase, 지역변수 camelCase, private 필드 `_camelCase`, 상수 UPPER_SNAKE_CASE
- **파일명**: 클래스명과 일치
- **네임스페이스**: `Proto.<Feature>` (예: `Proto.Player`, `Proto.Enemy`)
- **테스트**: Proto에서는 **선택**. 빠른 반복을 우선.

## Context Management (Proto 축약)

- 중요한 변경 후 `production/session-state/active.md`에 한 줄 기록 (선택).
- 컨텍스트가 70% 넘으면 `/compact` 실행.
- 자세한 규칙은 main 브랜치의 `.claude/docs/context-management.md` 참조.
