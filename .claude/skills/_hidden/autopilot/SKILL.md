---
name: autopilot
description: 자율 구현 모드 — 아이디어에서 검증된 코드까지 전체 자동화
argument-hint: "<product idea or task description>"
---

# /autopilot — 자율 구현 모드

간단한 아이디어 설명에서 완성된 코드까지 전체 과정을 자동 처리합니다.
CCGS의 협업 프로토콜을 준수하되, 사용자가 "승인 없이 진행해"라고 명시한 경우 자율 실행합니다.

## 사용 시점
- "autopilot", "자동으로", "build me", "I want a" 키워드
- 프로토타이핑, 새 기능 전체 구현
- /prototype 스킬과 결합 가능

## 실행 단계

### Phase 0 — 확장
사용자의 아이디어를 상세 스펙으로 확장 (technical-director 활용)

### Phase 1 — 계획
구현 계획 수립 (producer + technical-director)

### Phase 2 — 구현
ralph + ultrawork를 결합하여 병렬 구현
- CCGS 프로그래머 에이전트들에 위임
- 간단한 작업: haiku, 표준 작업: sonnet, 복잡한 작업: opus

### Phase 3 — QA
ultraqa 모드로 빌드→테스트→수정 반복 (최대 5회)
- 같은 에러 3회 반복 시 사용자에게 보고

### Phase 4 — 검증
다중 관점 리뷰 (병렬 실행)
- technical-director: 기능 완성도
- security-engineer: 보안 취약점
- lead-programmer: 코드 품질
- 모두 승인해야 통과; 거부 시 수정 후 재검증

### Phase 5 — 완료
사용자에게 결과 보고

## CCGS 오버라이드 규칙
- Phase 0/1에서 사용자 확인 후 진행 권장
- /gate-check 건너뛸 수 없음
- session-state 업데이트 필수
- VKL 에스컬레이션 적용

## 중단 조건
- 같은 QA 에러 3회 반복 (근본 문제 — 사용자 개입 필요)
- 검증이 3라운드 후에도 실패
- 사용자가 "stop", "cancel", "abort" 발화

## 완료 체크리스트
- [ ] 모든 5개 Phase 완료
- [ ] Phase 4의 모든 검증자 승인
- [ ] 테스트 통과 (신선한 출력)
- [ ] 빌드 성공 (신선한 출력)
- [ ] 사용자에게 요약 보고
