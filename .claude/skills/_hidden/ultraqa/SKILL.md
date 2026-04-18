---
name: ultraqa
description: QA 사이클링 — 빌드/테스트/수정을 목표 달성까지 반복
argument-hint: "[--tests|--build|--lint] [goal description]"
---

# /ultraqa — QA 사이클링 모드

테스트/빌드/린트를 반복 실행하여 품질 목표를 달성할 때까지 자동으로 수정합니다.
/smoke-check 스킬을 보완합니다.

## 사용 시점
- "테스트 반복", "ultraqa", "빌드 고쳐" 키워드
- /smoke-check 실패 후 자동 수정이 필요할 때
- C# 컴파일 에러를 반복적으로 수정할 때

## 목표 파싱

| 호출 | 목표 유형 | 검사 내용 |
|------|----------|----------|
| `/ultraqa --tests` | tests | 테스트 스위트 통과 |
| `/ultraqa --build` | build | 빌드 성공 (exit 0) |
| `/ultraqa --lint` | lint | 린트 에러 없음 |

구조화된 목표 없으면 인자를 커스텀 목표로 해석.

## 사이클 워크플로우 (최대 5회)

1. **QA 실행**: 목표 유형에 따라 검증 실행
   - `--tests`: 테스트 스위트 실행
   - `--build`: dotnet build 또는 Unity Build Pipeline
   - `--lint`: 린트 검사
2. **결과 확인**: 통과 시 완료, 실패 시 계속
3. **진단**: technical-director에게 실패 원인 분석 요청
4. **수정**: 해당 전문 프로그래머 에이전트가 수정 적용
5. **반복**: Step 1로 복귀

## 종료 조건

| 조건 | 동작 |
|------|------|
| 목표 달성 | 성공 보고: "ULTRAQA COMPLETE: N사이클 만에 목표 달성" |
| 5사이클 도달 | 진단 보고: "ULTRAQA STOPPED: 최대 사이클. 진단: ..." |
| 같은 실패 3회 | 조기 종료: "ULTRAQA STOPPED: 같은 실패 3회. 근본 원인: ..." |

## 진행 출력

```
[ULTRAQA Cycle 1/5] 테스트 실행 중...
[ULTRAQA Cycle 1/5] FAILED - 3개 테스트 실패
[ULTRAQA Cycle 1/5] 진단 중...
[ULTRAQA Cycle 1/5] 수정: PlayerSystem.cs - 누락된 초기화
[ULTRAQA Cycle 2/5] 테스트 실행 중...
[ULTRAQA Cycle 2/5] PASSED - 47개 테스트 모두 통과
[ULTRAQA COMPLETE] 2사이클 만에 목표 달성
```

## CCGS 연동
- /smoke-check 실패 시 자동 전환 가능
- /compile-check 결과를 입력으로 사용 가능
- /story-done 전 품질 검증에 활용
