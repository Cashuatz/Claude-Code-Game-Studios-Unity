---
Document Role: Archive Snapshot
Update Policy: Read-only. 이 아카이브는 2026-04-24 Proto 패키지 마감 시점의 VKL 기록이다. 수정·삭제 금지.
Owner: Historical
Scope: Reference only
---

# Archive — Proto 패키지 마감 (2026-04-24)

이 디렉토리는 Proto 브랜치가 **재사용 가능한 패키지**로 마감되기 직전까지 이 저장소에서 실제로 쌓아 올린 VKL 기록의 스냅샷이다.

## 왜 아카이브되었나

Proto 브랜치는 다양한 수강생이 각자의 게임 프로토타이핑에 재사용할 수 있도록 **공용 패키지** 로 정비됐다. 마감 직전까지의 `runtime/` (case logs / observations / oracle runs / temporary hypotheses) 과 `proposals/` 는 대부분 이 저장소의 **특정 실사례 (TD WFC 시가지 PCG, BushBillboard 셰이더, SpeechBubble UI, Phase 0 런처 등)** 에서 발생한 검증 기록이며, 다음 수강생의 세션이 빈 runtime 에서 시작할 수 있도록 통째로 여기로 옮겨왔다.

## 남아 있는 독립 재사용 산출물 (본 아카이브 바깥)

아카이브에 들어온 기록들에서 **독립적으로 재사용 가능**하다고 판정된 산출물은 이미 `.vkl/project/` 또는 `design/proto-modules/_conventions.md` 에 승격된 상태다:

- **OR-P-006** (PCG Domain Rule Oracle) → `.vkl/project/ORACLE_CATALOG.project.md`
- **GUIDE-PCG-01** (PCG 배치 오브젝트의 시각+데이터 이중 설계) → `design/proto-modules/_conventions.md`
- **GUIDE-PCG-02** (시가지 계열 PCG 다양성 축 우선순위) → `design/proto-modules/_conventions.md`

출처 proposal: `proposals/PROP-2026-04-24-001.md`, `proposals/PROP-2026-04-24-002.md` (본 아카이브 내).

## 구성

```
2026-04-24-proto-package-finalization/
├── README.md (이 파일)
├── runtime/
│   ├── INDEX.md                        (아카이브 시점 테마별 스냅샷)
│   ├── case_logs/        (14 파일, CL-2026-04-24-001~014)
│   ├── observations/     (5 파일, OBS-2026-04-24-001~005)
│   ├── oracle_runs/      (13 파일, ORUN-2026-04-24-001~013)
│   └── temporary_hypotheses/ (HYP-2026-04-24-001, 002)
└── proposals/
    ├── PROP-2026-04-24-001.md (Approved partial — OR-P-006 + GUIDE-PCG-02 승격)
    └── PROP-2026-04-24-002.md (Approved — GUIDE-PCG-01 승격, TD A/B 는 실사례 미구현 spec gap 유지)
```

## 어떻게 읽을 것인가

수강생이 이 패키지로 자기 게임 프로토타입을 만들다가 "과거에 이런 상황에서 어떻게 검증했지?" 같은 **패턴 참고**가 필요할 때 이 아카이브를 조회한다. 특히:

- **PCG 모듈 설계에서 다양성 축이 막힐 때** → `runtime/case_logs/CL-2026-04-24-001~003.md` (도메인 룰 교정 loop)
- **WFC 솔버를 처음 도입할 때** → `CL-2026-04-24-006.md` (v1 40×40, 1387 iter) + `CL-2026-04-24-010.md` (v2 하이브리드)
- **PCG 시각과 게임 시스템 연결 인터페이스가 헷갈릴 때** → `CL-2026-04-24-014.md` + `PROP-2026-04-24-002.md`
- **셰이더 네임스페이스 충돌** → `CL-2026-04-24-005.md` (SpeechBubble Camera alias 사례)
- **Phase 0 스모크 진입 구조** → `CL-2026-04-24-008, 012, 013`

어디까지나 **참고 사례**다. 수강생 자신의 세션 기록은 아카이브 바깥 `.vkl/runtime/` 에 새로 쌓는다.
