---
name: note
description: 세션 간 지식 보존을 위한 노트 저장
argument-hint: "<note content>"
---

# /note — 노트패드

컨텍스트 압축(compaction)을 넘어서도 유지되어야 할 정보를 기록합니다.

## 사용 시점
- "기억해", "note", "노트" 키워드
- 세션 간 유지해야 할 결정 사항
- 디버깅 중 발견한 중요 패턴
- 아키텍처 결정의 이유

## 저장 위치
- `.claude/agent-memory/notes/` 디렉토리에 날짜별 파일로 저장

## 형식

```markdown
# Note: [제목]
Date: YYYY-MM-DD
Context: [어떤 작업 중 기록했는지]

## 내용
[노트 본문]

## 관련 파일
- [관련 파일 경로들]
```

## 규칙
- 기존 노트를 덮어쓰지 않음 (append-only)
- 간결하게 작성 (핵심만)
- 관련 파일 경로 포함
