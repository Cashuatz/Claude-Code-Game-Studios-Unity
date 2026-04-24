---
name: codex
description: Codex CLI를 사용한 교차 검증
argument-hint: "<question or review request>"
---

# /codex — 교차 검증

OpenAI Codex CLI를 사용하여 2차 검증을 수행합니다.
중요한 판단(아키텍처 결정, 버그 원인 추정, 보안 의심)이 떠오를 때
Claude 단독 판정이 약해 보이면 이 스킬로 독립된 의견을 한 번 더 확인합니다.

## 사용 시점
- 설계 결정이 돌이키기 어려운 규모일 때 (DB 스키마, 저장 포맷, 공개 API)
- 버그 원인 가설이 불확실한데 로그/재현이 모호할 때
- 보안·데이터 파괴 가능성 있는 코드 변경 리뷰
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
