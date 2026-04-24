# Memory Snapshot — 글로벌 auto-memory 백업 (2026-04-24)

이 디렉토리는 컴퓨터 이전 시 재설치용 auto-memory 덤프다.

## 원본 위치
```
~/.claude/projects/d--Dev-Claude-Claude-Code-Game-Studios-Unity-WorkSpace-Claude-Code-Game-Studios-Unity/memory/
```

**주의:** 슬러그 `d--Dev-Claude-...`는 프로젝트의 **절대 경로를 치환한 형태**다. 새 컴퓨터에서 프로젝트 경로가 다르면 슬러그도 달라진다.

## 새 컴퓨터에서 재설치하는 방법

1. 프로젝트를 새 컴퓨터에 clone
2. Claude Code를 한 번 띄워 세션을 시작 → 새 슬러그로 빈 memory 디렉토리가 자동 생성됨
3. 또는 수동으로 디렉토리 생성:
   ```
   mkdir -p ~/.claude/projects/<new-slug>/memory
   ```
4. 이 디렉토리의 3개 파일을 해당 경로로 복사:
   - `MEMORY.md`
   - `feedback_claude_dir_staging.md`
   - `feedback_no_git_push.md`
5. Claude Code 재시작 → 메모리 자동 로드

## 슬러그 계산 규칙
프로젝트 절대 경로를 다음과 같이 치환:
- 드라이브 콜론 제거 (`D:\` → `d`)
- 백슬래시·슬래시 → `-`
- 공백 → `-`

예:
- `D:\Dev\Claude\Claude-Code-Game-Studios-Unity-WorkSpace\Claude-Code-Game-Studios-Unity`
  → `d--Dev-Claude-Claude-Code-Game-Studios-Unity-WorkSpace-Claude-Code-Game-Studios-Unity`

새 경로가 다르면 새 슬러그 계산 후 그 디렉토리에 복사.

## 내용 개요
- **MEMORY.md** — 인덱스 (1줄당 하나의 메모리 참조)
- **feedback_claude_dir_staging.md** — `.claude/` 수정은 `.stage/` 경유 + `mv`로 배포. 권한 프롬프트 최소화.
- **feedback_no_git_push.md** — Proto 브랜치에서 `git push` 금지. 사용자 명시 요청 시에만 실행.

향후 추가 고려 메모리(HANDOFF-2026-04-24-session.md Section 11-4 참조):
- Verification PASS ≠ Validation PASS 강조 (Loop 5 사용자 UX 피드백)
- Unity New Input System + EventSystem은 반드시 `InputSystemUIInputModule`
- MCP unityMCP 도구 제약 (execute_code 500B, menu 경로 `[A-Za-z0-9_]/`, refresh_unity 외부 sleep)
