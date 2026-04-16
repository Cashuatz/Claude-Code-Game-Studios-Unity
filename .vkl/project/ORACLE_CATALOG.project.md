---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# ORACLE_CATALOG.project.md

> **Document Role: Project Reference** -- `.vkl/core/ORACLE_CATALOG.base.md`를 **overlay(확장)**한다.
> core의 9가지 오라클은 그대로 유효하며, 이 문서는 Unity 게임 개발 프로젝트에 특화된 오라클을 추가 정의한다.
> **Overlay Of:** `.vkl/core/ORACLE_CATALOG.base.md`
> **Merge Rule:** core oracle catalog를 먼저 로드한 뒤 이 project oracle catalog를 추가 적용한다.

---

## Project-Specific Oracles

### OR-P-001: Play Mode Execution Oracle (Play Mode 실행 오라클)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | OR-03 (Execution Oracle) |
| **Definition** | Unity Editor의 Play Mode에서 실제 게임 로직을 실행하여 동작을 확인한다. |
| **When to Use** | 게임플레이 로직 구현 후, MonoBehaviour 라이프사이클 확인, 물리/UI 동작 확인 |
| **Limitations** | Play Mode ≠ 빌드 동작 (Editor-only API, 프레임 레이트 차이, 비결정적 물리) |
| **Connected FT** | FT-P-005 (빌드/플랫폼 차이), FT-P-003 (물리 비결정성) |

### OR-P-002: Profiler Measurement Oracle (프로파일러 측정 오라클)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | OR-05 (Comparison Oracle) |
| **Definition** | Unity Profiler (CPU/GPU/Memory) 측정값을 성능 예산과 비교하여 판정한다. |
| **When to Use** | 성능 최적화 전후 비교, 프레임 드롭/메모리 릭 의심, GC Allocation/Draw Call 확인 |
| **Limitations** | Editor 프로파일링 ≠ 빌드 성능, Deep Profiling 오버헤드, 타겟 디바이스 차이 |
| **Connected FT** | FT-P-003, FT-P-005 |

### OR-P-003: GDD Formula Comparison Oracle (GDD 공식 대조 오라클)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | OR-01 (Spec Oracle) |
| **Definition** | GDD에 정의된 공식/수치/밸런스 값과 실제 구현값을 대조하여 일치 여부를 판정한다. |
| **When to Use** | 밸런스 공식 구현 후, /balance-check 결과 해석, ScriptableObject 값 검증 |
| **Limitations** | GDD TBD 항목은 검증 불가 (FT-01), 런타임 상호작용 결과는 단순 대조 불가 |
| **Connected FT** | FT-01 (Spec Gap), FT-02 (Misfill) |

### OR-P-004: Compilation Oracle (컴파일 오라클)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | OR-03 (Execution Oracle) |
| **Definition** | dotnet build 또는 Unity 컴파일러를 통해 C# 코드의 컴파일 성공 여부를 판정한다. |
| **When to Use** | C# 수정 후, /compile-check 실행, 어셈블리 참조 변경 후, 리팩토링 후 |
| **Limitations** | dotnet build ≠ Unity 특화 컴파일, .csproj 필요, Editor/Runtime 컨텍스트 차이 |
| **Connected FT** | FT-P-005 (빌드/플랫폼 차이), FT-05 (Execution Failure) |

### OR-P-005: Unity Test Framework Oracle (테스트 프레임워크 오라클)

| 항목 | 내용 |
|------|------|
| **Overlay Of** | OR-03 (Execution Oracle) |
| **Definition** | Unity Test Framework (NUnit) EditMode/PlayMode 테스트 실행 결과를 판정 근거로 사용한다. |
| **When to Use** | 기능 구현 후 자동 테스트 검증, /smoke-check, /ultraqa, 회귀 테스트 |
| **Limitations** | EditMode ≠ 라이프사이클 실행, PlayMode는 느리고 비결정적, 테스트 미존재 시 불가 |
| **Connected FT** | FT-10 (Metamorphic Failure), FT-05 (Execution Failure) |

---

## Oracle Selection Guide

| 상황 | 1st Oracle | 2nd Oracle | 3rd Oracle |
|------|------------|------------|------------|
| 게임플레이 로직 구현 후 | OR-P-001 (Play Mode) | OR-P-005 (Test) | OR-P-003 (GDD) |
| 밸런스 수치 검증 | OR-P-003 (GDD Formula) | OR-P-005 (Test) | OR-08 (Human) |
| C# 코드 수정 후 | OR-P-004 (Compilation) | OR-P-005 (Test) | OR-03 (Execution) |
| 성능 최적화 후 | OR-P-002 (Profiler) | OR-P-001 (Play Mode) | OR-05 (Comparison) |
| 물리 시스템 수정 후 | OR-P-001 (Play Mode) | OR-P-002 (Profiler) | OR-06 (Metamorphic) |
