---
name: codex
description: Codex CLI를 사용한 교차 검증
argument-hint: "<question or review request>"
---

# /codex — 교차 검증

OpenAI Codex CLI를 사용하여 2차 검증을 수행합니다.
CCGS에서 중요한 아키텍처 결정이나 코드 리뷰 시 교차 검증에 활용합니다.

## 사용 시점
- /architecture-decision 후 교차 검증
- /code-review에서 의심스러운 부분 검증
- /security-audit의 2차 확인
- 사용자가 "codex", "교차검증", "세컨드오피니언" 발화 시

## 명령

```bash
codex exec --skip-git-repo-check --ephemeral -m gpt-5.4 -c model_reasoning_effort=high "PROMPT"
```

## 요구사항
- `codex` CLI가 시스템에 설치되어 있어야 함 (`npm install -g @openai/codex`)
- OPENAI_API_KEY 환경변수 필요

## 사용 패턴

### 코드 리뷰
```bash
codex exec --skip-git-repo-check --ephemeral -m gpt-5.4 -c model_reasoning_effort=high "Review this code for bugs and security issues: [code snippet]"
```

### 아키텍처 검증
```bash
codex exec --skip-git-repo-check --ephemeral -m gpt-5.4 -c model_reasoning_effort=high "Is this architectural approach sound? [ADR summary]"
```

### 보안 감사
```bash
codex exec --skip-git-repo-check --ephemeral -m o3 -c model_reasoning_effort=high "Security audit: [code or design]"
```
