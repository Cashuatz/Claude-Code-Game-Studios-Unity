# VKL Runtime Index — Proto 패키지 마감 스냅샷 (2026-04-24)

> 이 인덱스는 **append-only**. 기존 인덱스를 지우거나 덮어쓰지 말고 새 스냅샷을 추가해
> supersede 하라. runtime 기록 자체는 `case_logs/` · `observations/` · `oracle_runs/`
> · `temporary_hypotheses/` 에 원본 유지.

## 현재까지 기록 수량

| 카테고리 | 수량 | 최신 ID |
|---|---|---|
| case_logs | 14 | CL-2026-04-24-014 |
| observations | 5 | OBS-2026-04-24-005 |
| oracle_runs | 13 | ORUN-2026-04-24-013 |
| temporary_hypotheses | 2 | HYP-2026-04-24-002 |
| proposals | 2 | PROP-2026-04-24-002 |

## 테마별 분류 (2026-04-24 기준)

### 1. TD / PCG (시가지 WFC·CityscapeGenerator) — 이 저장소 실사례
주 흐름: 초기 3-loop 튜닝 → v1/v2/v3 레벨 디자인화 → FlowField blocked 배열 spec gap 확인.

- CL-001, CL-002, CL-003 — 도메인 룰 교정 (yaw jitter 잘못된 축 → GUIDE-PCG-02)
- CL-006 — WFC v1 (40×40, 1387 iter)
- CL-010 — WFC v2 하이브리드 (Block Subdivision + WFC)
- CL-011 — WFC v3 레벨 디자인화 (60×60, Sidewalk/Planter, 도로 위계)
- CL-014 — PCG ↔ FlowField blocked 배열 연결 검증 (FT-01 spec gap 확정, Option A 권고)
- OBS-002, OBS-003, OBS-004 — WFC 관측·장르별 완전 분리 규칙
- ORUN-006, 008~013 — WFC/CityscapeGenerator 오라클 실행

### 2. Phase 0 런처 + 3장르 껍데기 씬 — 이 저장소 실사례
- CL-007 — 3장르 프로토 데모 자산 인벤토리
- CL-008 — Phase 0~4 계획 확정
- CL-009 — "장르별 완전 분리" 확정 (HYP→OBS 승격)
- CL-012 — Phase 0 정적 구현 (런처 + 3개 껍데기 씬, SceneFlow 싱글톤)
- CL-013 — Phase 0 런타임 검증 (Input System 버그 수정)
- HYP-001 — 공통 캐릭터 프리팹/SpeechBubble 역할 임시 가정 (기각, 장르별 분리로 전환)

### 3. 환경 에셋 & UI
- CL-004 — BushBillboard 셰이더 조명 수정 (Rev A→B)
- CL-005 — SpeechBubble C# 네임스페이스 충돌 (Proto.Camera vs UnityEngine.Camera)
- OBS-001 — 부쉬-Lambert 조명 대비비 정량 기준

### 4. Proto 패키지 마감 관련 (신규)
- HYP-002 — FlowField blocked Option A 가설 (CL-014 에서 생성, PROP-002 로 승격 권고)

## 일반화되어 다른 프로젝트로 전파 가능한 산출물

| 출처 | 산출물 | 반영 위치 |
|---|---|---|
| CL-001/002/003 | PCG 다양성 축 우선순위 | `design/proto-modules/_conventions.md` GUIDE-PCG-02 |
| CL-014 | PCG 시각+데이터 이중 설계 규칙 | `design/proto-modules/_conventions.md` GUIDE-PCG-01 |
| CL-001~003 통합 | PCG Domain Rule Oracle | `.vkl/project/ORACLE_CATALOG.project.md` OR-P-006 |
| _pattern URP 세션 + Mesh FullscreenTriangle 구현 | Mesh 기반 풀스크린 삼각형 패턴 | `design/proto-modules/_pattern-urp-render-pipeline-hooks.md` §4b + `Assets/Proto/Runtime/Rendering/` |

## 이 저장소 고유로 보존 (일반화 스킵)

- WFC 타일셋 아키텍처 상세 (CL-006), 하이브리드 PCG 알고리즘 (CL-010), 레벨 디자인화 축(CL-011)
- BushBillboard Rev A→B 조명 공식 (CL-004, OBS-001)
- Phase 0 런처·3장르 씬 구성 (CL-007~013)

후속 프로토 프로젝트가 유사 상황에 부딪히면 이 파일들을 "참고 예시" 로 읽을 것.

## 승인된 Proposal 요약

- **PROP-2026-04-24-001** — Approved (partial). OR-P-006 + Oracle Selection Guide 행 승격. HR 대신 GUIDE-PCG-02 로 완화 적용.
- **PROP-2026-04-24-002** — Approved (C 만). GUIDE-PCG-01 승격. A/B 는 TD 미구현 모듈의 spec gap 으로 유지, 재개 시 Option A 채택 권고.
