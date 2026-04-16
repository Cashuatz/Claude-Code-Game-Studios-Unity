---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# VALIDATION_CHECKLIST.project.md

> **Document Role: Project Reference** -- `.vkl/core/VALIDATION_CHECKLIST.base.md`를 **overlay(확장)**한다.
> core의 verification(VC-V-XX) 및 validation(VC-D-XX) 항목은 그대로 유효하며,
> 이 문서는 Unity 게임 개발 프로젝트에 특화된 항목을 추가 정의한다.
>
> **ID 규칙:** 프로젝트 verification은 `VC-P-V-XX`, validation은 `VC-P-D-XX` 형식을 사용한다.

---

## Project Verification Items (VC-P-V: 기계적 검증)

### VC-P-V-01: asmdef C# 컴파일 성공
- **검증**: `dotnet build {.csproj}` exit code 0
- **Oracle**: OR-P-004 (Compilation)

### VC-P-V-02: MonoBehaviour 라이프사이클 메서드 이름 정확
- **검증**: Awake/Start/Update 등 메서드명 오타 검색
- **Oracle**: OR-02 (Format)

### VC-P-V-03: SerializeField 어트리뷰트 적용
- **검증**: Inspector 노출 private 필드에 [SerializeField] 확인
- **Oracle**: OR-P-004 + OR-P-001

### VC-P-V-04: Addressables 키 일치
- **검증**: 코드 내 Addressables 키가 에셋 그룹에 존재하는지 확인
- **Oracle**: OR-P-004

### VC-P-V-05: 에디터/빌드 동작 일치
- **검증**: #if UNITY_EDITOR 블록이 런타임 로직에 영향 여부, Editor-only API 확인
- **Oracle**: OR-P-004 + OR-P-001

---

## Project Validation Items (VC-P-D: 의미 검증 — 인간 판단 필요)

### VC-P-D-01: 게임플레이 느낌이 GDD 의도와 일치
- **Claude 역할**: GDD와 구현의 차이점 식별
- **인간 판단**: "재미있는가?", "의도한 느낌인가?"

### VC-P-D-02: UI/UX 흐름이 직관적
- **Claude 역할**: UX 스펙과 구현 차이, 접근성 위반 분석
- **인간 판단**: "사용하기 편한가?"

### VC-P-D-03: 밸런스 수치가 기획 의도에 부합
- **Claude 역할**: GDD 공식 대조, 단조성 테스트, 극단값 분석
- **인간 판단**: "밸런스가 재미를 해치는가?"

---

## 검증 실행 순서

1. **VC-P-V-01** (컴파일) — 가장 먼저. 실패 시 나머지 무의미
2. **VC-P-V-02~05** (구조적 검증) — 병렬 실행 가능
3. **Core VC-V-01~12** — 프로젝트 검증과 함께
4. **VC-P-D-01~03** (의미 검증) — 기계적 검증 후 인간 에스컬레이션
