---
name: Proto 브랜치에서 git push 금지
description: commit 까지만 자동, push 는 절대 실행하지 않음 (credential 프롬프트 회피)
type: feedback
originSessionId: a2612cc2-2886-4ffa-9713-9dfb3c386f3f
---
Proto 브랜치에서 Claude 는 `git add` + `git commit` 까지만 수행하고 **`git push` 는 실행하지 않는다**.

**Why:** push 시 HTTPS credential 프롬프트가 매번 떠서 수강생/사용자 흐름이 끊긴다. 2026-04-24 사용자 요청.

**How to apply:**
- 작업 완료 시 commit 까지만 자동 실행
- push 는 사용자가 직접 하거나, 사용자가 명시적으로 "푸시해줘"/"원격에 올려줘" 라고 요청한 경우에만 실행
- 프로젝트 CLAUDE.md 의 "Git — Claude가 로컬까지만 대행" 섹션과 일치시킬 것
