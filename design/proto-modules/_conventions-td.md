# Proto TD — Conventions & Hard Rules

> 타워 디펜스(네트워크 허브 연결형, 엔드필드류) 프로토 전용 규칙.
> 공통 규칙은 `_conventions.md` (HR-1 ~ HR-11) 를 **그대로 상속**.
> 이 문서는 TD 장르에서만 추가되는 Hard Rule (HR-TD-XX) 을 정의한다.
> 수강생 답변과 무관하게 **고정**되는 룰이다 (분기 없음).

## 상속되는 공통 Hard Rules

`_conventions.md` 의 다음 규칙은 TD에도 그대로 적용:

- HR-1. Rigidbody 전면 금지 (물리는 static query만)
- HR-2. 단일 씬 (`Assets/Proto/Scenes/ProtoMain.unity`)
- HR-4. 카메라 전환은 이벤트 기반 (`CameraRig.RequestMode`)
- HR-8. 데이터-코드 분리 (ScriptableObject / JSON)
- HR-9. 이벤트 버스 (C# event or GameEventChannel)
- HR-10. 에디터-런타임 경계 (`Assets/Proto/` 하위에만 생성)
- HR-11. UI 런타임은 uGUI, 에디터는 UI Toolkit

**적용되지 않는 공통 규칙**:

- HR-3. PCG 레이아웃 — TD는 정적/사전 정의 레벨 사용 (웹 에디터 산출물)
- HR-5. 유사턴제 불변식 — TD는 실시간
- HR-6. CombatScheme — TD는 타워가 자동 공격, 액션 입력 없음
- HR-7. CharacterRenderKit — TD에서는 적/타워 렌더로만 사용 (폴리모프 유지)

## TD 전용 Hard Rules

### HR-TD-1. 시뮬레이션 코어는 결정론적 순수 C#

- 시뮬 코어 (`Proto.TD.Sim.*` 네임스페이스) 는 **`UnityEngine` 참조 금지**.
- 허용: `System`, `System.Collections.Generic`, `System.Numerics` (또는 자체 Fixed-point math).
- 금지: `Time.deltaTime`, `Random.Range`, `UnityEngine.Vector3`, `GameObject`, `MonoBehaviour`.
- 결정론 보장: 동일 `seed` + 동일 입력 시퀀스 → 동일 결과 (프레임 단위 비교 가능).

### HR-TD-2. RNG는 seeded Xorshift만

- `System.Random`, `UnityEngine.Random` **금지**.
- 공식 RNG: `Proto.TD.Sim.Rng` (Xorshift64 또는 PCG32, 구현 1벌 고정).
- 시뮬 인스턴스마다 RNG 인스턴스 1개, 생성자에 seed 강제 주입.

### HR-TD-3. Fixed Timestep

- 시뮬 tick은 고정 간격: **16ms (62.5Hz)** 또는 **20ms (50Hz)** 둘 중 하나. 프로젝트 단일값.
- 기본값: **20ms**. 변경 시 `_catalog-td.md` 에 기록.
- 렌더는 Unity 기본 프레임레이트로 따로 돌고, 시뮬은 tick 누적/보간.
- `Time.deltaTime` 기반 로직은 시뮬 코어 밖(프레젠테이션 레이어) 에서만 허용.

### HR-TD-4. 데이터 1차 소스는 JSON

- 레벨(`level.json`), 웨이브(`wave.json`), 타워(`tower.json`), 적(`enemy.json`) 정의는 **JSON 이 원본**.
- Unity ScriptableObject 는 **파생물** — 빌드/임포트 타임에 JSON 에서 생성.
- 웹 시뮬레이터(TypeScript) 와 Unity 가 **동일 JSON 스키마** 사용.
- 스키마 버전 필드 필수 (`"schemaVersion": 1`). 하위 호환 깨지면 버전 올림.

### HR-TD-5. 적 경로는 사전 정의 (고정 경로만)

- 프로토 범위에서 적은 **플레이어 구조물을 우회하지 않는다**.
- 적 경로는 레벨 JSON 에 `paths: [[{x,y,z}, ...], ...]` 로 박혀 있음.
- FlowField / NavMesh / 동적 우회는 **후속 과제**, 프로토엔 금지.
- 예외: 길찾기 옵션으로 FlowField 를 선택한 경우 `td-pathing-flowfield` 모듈 활성 (카탈로그 참조).

### HR-TD-6. 선(Link) 자원은 유클리드 거리 기반

- 허브 A ↔ 허브 B 연결 비용 = `distance(A, B) * costPerUnit`.
- 셀기반 레벨에서도 내부적으로는 유클리드 (단, 허브는 셀 중심으로 스냅).
- 총 예산은 레벨 JSON 의 `linkBudget` 필드.
- 연결 트리는 반드시 **발전소(Power Source) 노드에서 도달 가능**해야 타워 가동.

### HR-TD-7. 타워-허브 결합은 반드시 이벤트 경유

- 타워는 자기가 "전력 공급 중"인지 직접 판단하지 않음.
- `HubNetwork.OnPowerChanged(towerId, isPowered)` 이벤트 구독으로만 상태 반영.
- 네트워크 그래프 변경 (허브 추가/제거/연결 변경) 시 네트워크 재계산 → 이벤트 발행.

### HR-TD-8. 자동 플레이테스트는 결정론 전제

- `/td-playtest` 스킬은 **seed 목록 + 플레이어 입력 스크립트** 를 받아 N회 시뮬 실행.
- Unity 없이 **웹 시뮬레이터만으로 실행 가능**해야 함 (CLI: `node sim/run.js`).
- 결과는 `design/playtest-reports/<level>-<date>.json` 에 통계(클리어율/평균 자원/평균 클리어 시간) 기록.
- 동일 seed 재실행 결과 불일치 → **결정론 깨짐 = FAIL**, 즉시 에스컬레이션.

### HR-TD-9. 웹 ↔ Unity 동기화 검증

- 모든 CI 라운드에서 다음을 검증:
  1. 웹 시뮬과 Unity 시뮬에 **같은 seed + 입력** 주입
  2. 최종 상태(남은 자원, 사망한 적 수, 클리어 여부) 비교
  3. 불일치 시 포팅 오류로 판정, 해당 PR 블록
- 검증 스킬: `/td-sim-parity`.

### HR-TD-10. 레벨 에디터 산출물 경로

- 웹 레벨 에디터 익스포트는 `Assets/Proto/Data/TD/Levels/<level-id>.json` 으로 고정.
- Unity 측 임포터는 해당 경로 감시, JSON 변경 시 대응 SO 재생성.
- 수동 편집 금지 (JSON 이 원본이지만 웹 에디터로만 생성).

## 파일·폴더 규칙 (TD 전용 추가)

```
Assets/Proto/
├── Runtime/
│   └── TD/
│       ├── Sim/              ← 결정론 시뮬 코어 (UnityEngine 금지)
│       │   ├── Rng.cs
│       │   ├── FixedTick.cs
│       │   ├── HubNetwork.cs
│       │   ├── TowerSim.cs
│       │   ├── EnemySim.cs
│       │   └── LevelState.cs
│       ├── Data/             ← JSON 로더, SO 생성기
│       ├── Presentation/     ← MonoBehaviour 렌더 브릿지
│       ├── Input/            ← 배치/연결 UI 핸들러
│       └── Playtest/         ← 자동 플레이테스트 러너
├── Data/
│   └── TD/
│       ├── Levels/           ← <level-id>.json (웹 에디터 산출)
│       ├── Waves/
│       ├── Towers/
│       └── Enemies/
└── Editor/
    └── TD/
        └── JsonImporter.cs   ← JSON → SO 변환

web-sim/                       ← 리포지터리 루트 (Unity 밖)
├── src/
│   ├── sim/                  ← C# 시뮬 코어와 1:1 대응
│   ├── editor/               ← 레벨 에디터 (Canvas UI)
│   ├── viewer/               ← 리플레이 뷰어
│   └── playtest/             ← Monte Carlo 러너
├── schema/                   ← JSON 스키마 (공유 소스)
│   ├── level.schema.json
│   ├── wave.schema.json
│   ├── tower.schema.json
│   └── enemy.schema.json
└── package.json
```

## 네이밍 (TD 전용)

- 시뮬 네임스페이스: `Proto.TD.Sim.<Kind>` (예: `Proto.TD.Sim.HubNetwork`)
- 프레젠테이션: `Proto.TD.View.<Kind>`
- JSON 필드: `camelCase` (웹 관례 따름)
- SO 파일: `TD_Level_<name>.asset`, `TD_Tower_<name>.asset`, 등
- 프리팹: `TD_Hub_<name>.prefab`, `TD_Tower_<name>.prefab`, `TD_Enemy_<name>.prefab`

## 검증 공통 (TD 추가)

기존 공통 검증(컴파일/씬/프리팹/런타임/스크린샷) 에 더해:

1. **결정론 스모크**: 동일 seed 로 시뮬 2회 실행 → 최종 상태 동일한지 bit-exact 비교.
2. **JSON 스키마 검증**: `Data/TD/**/*.json` 을 `schema/*.json` 으로 validate.
3. **네트워크 그래프 무결성**: 발전소 → 모든 가동 타워까지 연결 경로 존재.
4. **자원 예산 체크**: 배치된 선의 거리 합 ≤ `linkBudget`.

실패 시 롤백 + 사유 보고 (1문장).

## 메타 규칙

- 본 문서는 **수강생 답변과 무관하게 고정**. 분기 없음.
- 수정이 필요하면 `.vkl/proposals/` 에 proposal 추가 후 인간 검토.
- VKL oracle 부착: 각 HR-TD 에 대응하는 oracle ID 필요 (TBD, `.vkl/core/` 확장 시 부여).
