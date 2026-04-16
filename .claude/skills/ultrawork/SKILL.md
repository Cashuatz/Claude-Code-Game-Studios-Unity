---
name: ultrawork
description: 병렬 실행 모드 — 독립 작업을 동시에 실행
argument-hint: "<task description with parallel work items>"
---

# /ultrawork — 병렬 실행 모드

독립적인 여러 작업을 동시에 실행하여 처리 속도를 극대화하는 모드입니다.
CCGS의 협업 프로토콜을 준수합니다.

## 사용 시점
- "빠르게", "병렬", "ultrawork", "ulw" 키워드
- 서로 독립적인 2개 이상의 작업이 있을 때
- /team-* 스킬과 결합하여 팀 작업을 가속할 때

## 실행 절차

1. **작업 분류**: 독립 작업 vs 의존성 있는 작업 구분
2. **모델 라우팅**:
   - 간단한 조회: haiku (omc-explore)
   - 표준 구현: sonnet (gameplay-programmer, engine-programmer 등)
   - 복잡한 분석: opus (technical-director)
3. **독립 작업 동시 실행**: 모든 독립 작업을 한 번에 발사
4. **의존 작업 순차 실행**: 선행 작업 완료 후 후속 작업 실행
5. **경량 검증**: 빌드/테스트 통과, 새 에러 없음 확인

## 백그라운드 실행 규칙

**백그라운드** (`run_in_background: true`):
- npm install, pip install, dotnet build
- Unity Build Pipeline
- 테스트 스위트

**포그라운드** (blocking):
- git status, ls, pwd
- 파일 읽기/편집
- 간단한 명령

## CCGS 오버라이드 규칙
- 병렬 실행 중에도 사용자 승인 필수
- 디렉터 게이트 존중
- CCGS 에이전트를 우선 사용, omc-explore/debugger/verifier를 보조로 활용

## 관계

```
ralph (지속 실행 래퍼)
 └── includes: ultrawork (이 스킬)
     └── provides: 병렬 실행만

autopilot (자율 실행)
 └── includes: ralph
     └── includes: ultrawork (이 스킬)
```
