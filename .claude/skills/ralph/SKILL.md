---
name: ralph
description: 끝까지 완료 모드 — 모든 작업이 검증 완료될 때까지 지속
argument-hint: "<task description>"
---

# /ralph — 끝까지 완료 모드

CCGS 워크플로우에서 작업을 끝까지 완료하는 지속 실행 모드입니다.
CCGS의 협업 프로토콜(Question → Options → Decision → Draft → Approval)을 준수합니다.

## 사용 시점
- "끝까지", "ralph", "don't stop", "must complete" 키워드
- 작업이 여러 단계를 거쳐 완료 검증이 필요할 때
- 스토리 구현 + 테스트 + 리뷰까지 한 번에 처리할 때

## 실행 절차

1. **작업 분해**: 사용자 요청을 개별 수락 기준이 있는 작업 항목으로 분해
2. **작업 선택**: 가장 높은 우선순위의 미완료 항목 선택
3. **구현**: CCGS 에이전트에 위임하여 구현
   - 간단한 조회: haiku 모델 (omc-explore)
   - 표준 구현: sonnet 모델 (gameplay-programmer, engine-programmer 등)
   - 복잡한 분석: opus 모델 (technical-director)
4. **수락 기준 검증**: 각 기준에 대해 실제 증거로 검증
5. **항목 완료**: 모든 기준 통과 시 완료 표시
6. **전체 완료 체크**: 모든 항목 완료 여부 확인. 미완료 시 Step 2로 복귀
7. **사용자 승인**: CCGS 프로토콜에 따라 사용자에게 최종 결과 보고

## 병렬 실행 규칙
- 독립적인 작업은 동시에 실행 (ultrawork 결합)
- 장시간 작업 (빌드, 테스트)은 백그라운드 실행
- 의존성 있는 작업은 순차 실행

## CCGS 오버라이드 규칙
- ralph 중에도 파일 작성 전 사용자 승인 필수 (명시적 예외 제외)
- /gate-check는 건너뛸 수 없음
- session-state는 계속 업데이트
- VKL 에스컬레이션 조건도 적용 (블로킹 조건 시 작업 중단)

## 중단 조건
- 사용자가 "stop", "cancel", "abort" 발화
- 사용자 입력이 필요한 근본적 차단 (missing credentials, unclear requirements)
- 같은 이슈가 3회 이상 반복 시 사용자에게 보고
- VKL 블로킹 에스컬레이션 발생

## 완료 체크리스트
- [ ] 모든 작업 항목의 수락 기준 통과
- [ ] 빌드 성공 (해당 시)
- [ ] 테스트 통과 (해당 시)
- [ ] 사용자에게 완료 보고
