# Proto TD Modules — Catalog

> 타워 디펜스(네트워크 허브 연결형, 엔드필드류) 프로토 모듈 카탈로그.
> `_genres-index.md` 에서 `td` 장르로 라우팅된 뒤 `_questionnaire-td.md` 답변에
> 따라 활성화할 모듈 번들을 본 문서에서 조회한다.
>
> 각 모듈은 최종적으로 `.claude/skills/proto-<name>/SKILL.md` 로 승격된다.
> 공유 모듈(camera-rig 등) 은 본 문서에서 **참조만** 하고, 실제 정의는
> 각자 파일(`camera-rig.md` 등) 에 둔다.

## 핵심 컨셉 요약

- **장르**: 1인 시점 탑다운/쿼터뷰 타워 디펜스. 참고: 명일방주: 엔드필드 빌드 모드.
- **핵심 메커닉**: 제한된 "선 자원" 으로 **발전소 → 릴레이 허브 → 타워** 연결, 타워가
  사거리 내 적을 자동 공격.
- **적**: 사전 정의 경로를 따라 진입 (기본). 선택에 따라 FlowField / 동적 우회.
- **자원**: 선(link) 의 유클리드 거리 합 ≤ 예산. 예산 재생 방식은 수강생 선택.
- **카메라**: TopDown / QuarterBuilder / (선택) Orbit. 기존 `camera-rig` 확장.
- **씬**: 단일 씬 (공통 HR-2). 레벨 전환은 런타임 GameObject 재구성.
- **시뮬**: 결정론 순수 C# 코어. 웹(TS) 시뮬과 bit-exact 동기화.
- **데이터**: JSON 이 원본, SO 는 파생물 (HR-TD-4).

## 전체 모듈 맵

### TD 전용 모듈

| # | 모듈 ID | Tier | 영역 | 한 줄 요약 |
|---|---|---|---|---|
| 1 | `td-sim-core` | 코어·시뮬 | 시뮬 | 결정론 tick 엔진, RNG, fixed timestep, 상태 스냅샷 |
| 2 | `td-json-importer` | 코어·시뮬 | 데이터 | JSON → SO 빌드타임 변환, 스키마 검증 |
| 3 | `td-web-bridge` | 코어·시뮬 | 데이터 | 웹 에디터 산출물 감시/재로드, 리플레이 임포트 |
| 4 | `td-level-template-small` | 코어·공간 | 레벨 | 20x20 기본 맵 템플릿 |
| 5 | `td-level-template-medium` | 코어·공간 | 레벨 | 40x40 기본 맵 템플릿 |
| 6 | `td-level-template-large` | 코어·공간 | 레벨 | 80x80 기본 맵 템플릿 (+ 미니맵 필수) |
| 7 | `td-placement-grid` | 코어·공간 | 배치 | 셀 스냅 배치, manhattan 거리 |
| 8 | `td-placement-free` | 코어·공간 | 배치 | 자유 배치 + 허브 최소 간격 제약 |
| 9 | `td-hub-network` | 코어·공간 | 그래프 | 발전소-릴레이-타워 네트워크 그래프, 전력 전파 |
| 10 | `td-resource-ledger` | 코어·공간 | 자원 | 선 예산 관리 (fixed / regen / kill 모드) |
| 11 | `td-resource-tick` | 코어·공간 | 자원 | 시간 재생 모드 (Q5-B) 전용 서브 |
| 12 | `td-kill-reward-hook` | 코어·공간 | 자원 | 적 사망 시 예산 증가 (Q5-C) 전용 서브 |
| 13 | `td-pathing-fixed` | 코어·전투 | 이동 | 웨이포인트 고정 경로, `stage-graph` 재활용 |
| 14 | `td-pathing-flowfield` | 코어·전투 | 이동 | FlowField 기반 최단 경로 |
| 15 | `td-pathing-dynamic` | 코어·전투 | 이동 | 장애물 재계산 포함 동적 FlowField |
| 16 | `td-flowfield-builder` | 코어·전투 | 이동 | 타일맵 → FlowField 생성기 (14/15 의존) |
| 17 | `td-tower-core` | 코어·전투 | 전투 | 타워 정의, 사거리, 공격 속도, 타겟팅 |
| 18 | `td-enemy-core` | 코어·전투 | 전투 | 적 정의, HP, 속도, 이동 로직 |
| 19 | `td-bullet-system` | 코어·전투 | 전투 | 투사체 (hitscan/projectile), 풀링 |
| 20 | `td-wave-spawner` | 코어·전투 | 전투 | 웨이브 타임라인, single/multi lane 스폰 |
| 21 | `td-core-hp` | 룰 | 승패 | 기지 체력 컴포넌트 + 이벤트 |
| 22 | `td-game-rule-timed` | 룰 | 승패 | 시간 버티기 규칙 |
| 23 | `td-game-rule-killcount` | 룰 | 승패 | N 처치 승리 규칙 |
| 24 | `td-game-rule-coredefense` | 룰 | 승패 | 기지 방어 규칙 |
| 25 | `td-camera-pan-zoom` | UI·카메라 | 카메라 | `camera-rig` 확장: Pan + Zoom 입력 |
| 26 | `td-camera-mode-toggle` | UI·카메라 | 카메라 | TopDown ↔ QuarterBuilder ↔ Orbit 전환 버튼 |
| 27 | `td-hud-timer` | UI | HUD | 남은 시간 표시 위젯 |
| 28 | `td-hud-killcount` | UI | HUD | 처치 수 표시 위젯 |
| 29 | `td-hud-corehp` | UI | HUD | 기지 체력 바 위젯 |
| 30 | `td-hud-budget` | UI | HUD | 남은 선 예산 표시 위젯 (항상 활성) |
| 31 | `td-minimap` | UI | HUD | 미니맵 (Large 레벨 필수, Medium 선택) |
| 32 | `td-sprite-pack-default` | 아트 | 에셋 | 2D 빌보드 적/타워 기본 번들 (Q8-A) |
| 33 | `td-lowpoly-pack-default` | 아트 | 에셋 | 3D 로우폴리 적/타워 기본 번들 (Q8-B) |
| 34 | `td-lowpoly-env-default` | 아트 | 에셋 | 3D 로우폴리 배경/지형 번들 (Q8-C 보조) |
| 35 | `td-sim-parity` | 검증 | QA | 웹 ↔ Unity 결과 동일성 검증기 (HR-TD-9) |
| 36 | `td-playtest-runner` | 검증 | QA | Monte Carlo 자동 플레이테스트 (HR-TD-8) |
| 37 | `stage-pcg-wfc` | 코어·공간 | 레벨 | Wave Function Collapse 기반 결정론 타일 배치 (TD 레벨 템플릿 생성 엔진) |

### 공유 모듈 (imports)

| 모듈 ID | 출처 | TD 에서의 역할 |
|---|---|---|
| `camera-rig` | `camera-rig.md` | `TopDownBuilder` / `QuarterBuilder` 모드 추가 |
| `stage-graph` | `stage-graph.md` | 고정 경로 모드에서 적 경로 그래프 |
| `character-render-kit` | `character-render-kit.md` | 적/타워 렌더 추상화 (빌보드↔스킨드) |
| `hud-kit` | (예정) | HUD 위젯 기반 프레임 |
| `sound-kit` | (예정) | BGM/SFX |
| `save-kit` | (예정) | 레벨 진행/best 기록 |
| `shader-fx-kit` | `_genres-index.md` 참조 | 건설/파괴/적 사망 디졸브 |
| `post-process-kit` | `_genres-index.md` 참조 | 저체력/게임오버 PP 프로파일 |

## 의존성 그래프

```
td-sim-core ◄──────────────┐
   ▲                       │
   │                       │
td-json-importer           │
   ▲                       │
   │                       │
td-web-bridge              │
                           │
td-level-template-{S,M,L} ──► stage-pcg-wfc (선택, JSON 생성 엔진)
                           │
                           │
td-placement-{grid,free} ──┼──► td-hub-network ──► td-resource-ledger
                           │                          ▲
                           │                          ├── td-resource-tick (Q5-B)
                           │                          └── td-kill-reward-hook (Q5-C)
                           │
td-pathing-fixed ──► stage-graph
td-pathing-{flowfield,dynamic} ──► td-flowfield-builder
                           │
td-tower-core ──► td-bullet-system
   ▲
   │
td-enemy-core ──► td-pathing-* (선택된 것)
   ▲
   │
td-wave-spawner

td-core-hp ◄── td-game-rule-coredefense
td-game-rule-{timed,killcount,coredefense}

td-camera-pan-zoom ──► camera-rig
td-camera-mode-toggle ──► camera-rig

td-hud-{timer,killcount,corehp,budget,minimap} ──► hud-kit
td-{sprite,lowpoly}-pack-default ──► character-render-kit

td-sim-parity ──► td-sim-core, td-web-bridge
td-playtest-runner ──► td-sim-core, td-json-importer
```

## Phase 착수 순서

**Phase A — 시뮬 코어 + 빈 맵** (Tier 1·2 일부)
1. `td-sim-core` → 2. `td-json-importer` → 3. `td-web-bridge` → 4. 맵 템플릿(Q7) 1개 → 5. `camera-rig` + `td-camera-pan-zoom` (+모드 Q3)

☑ 종결 조건: 선택된 맵이 씬에 로드됨, 카메라로 맵 조망 가능, 결정론 tick 1초 실행 후 동일 seed 재실행 bit-exact 일치.

**Phase B — 배치 + 네트워크** (Tier 2 나머지)
6. 배치 모듈(Q1) → 7. `td-hub-network` → 8. `td-resource-ledger` (+Q5 서브) → 9. `td-hud-budget`

☑ 종결 조건: 수강생이 발전소·릴레이·타워를 배치하고 선으로 잇기 가능, 예산 UI 실시간 갱신, 연결 안 된 타워는 전력 OFF 표기.

**Phase C — 적·타워·전투** (Tier 3)
10. `td-pathing-*` (Q2 선택) → 11. `td-enemy-core` → 12. `td-tower-core` → 13. `td-bullet-system` → 14. `td-wave-spawner` (+Q6 lane 수)

☑ 종결 조건: 웨이브 1회차 진행, 타워가 적 공격, 적 사망/도착 시 이벤트 발생.

**Phase D — 규칙·HUD·아트** (Tier 4·5·6)
15. `td-core-hp` (Q4 조건에 포함 시) → 16. Q4 게임 규칙 모듈 → 17. 대응 HUD 위젯 → 18. 아트 팩(Q8) → 19. 미니맵(Q7-C 필수, B 선택)

☑ 종결 조건: 레벨 1회 플레이 완주 (승리 또는 패배 루트 확인), UI 정상 동작, 아트 적용됨.

**Phase E — 검증·자동화** (Tier 7)
20. `td-sim-parity` → 21. `td-playtest-runner`

☑ 종결 조건:
- 웹 ↔ Unity 동일 seed 재생 결과 일치 (HR-TD-9).
- Monte Carlo 100회 실행 결과 `design/playtest-reports/` 에 기록 (HR-TD-8).

## 스킬 매핑

모듈 1개 ≈ 스킬 1개. 네이밍: `/proto-<kebab-case>`.

| 모듈 | 스킬 |
|---|---|
| td-sim-core | `/proto-td-sim` |
| td-json-importer | `/proto-td-import` |
| td-web-bridge | `/proto-td-web-bridge` |
| td-level-template-small\|medium\|large | `/proto-td-level` (인자: `small\|medium\|large`) |
| td-placement-grid\|free | `/proto-td-placement` (인자: `grid\|free`) |
| td-hub-network | `/proto-td-hub` |
| td-resource-ledger | `/proto-td-resource` (인자: `fixed\|regen\|kill\|regen+kill`) |
| td-pathing-fixed\|flowfield\|dynamic | `/proto-td-pathing` (인자: `fixed\|flowfield\|dynamic`) |
| td-flowfield-builder | `/proto-td-flowfield` (조건부) |
| td-tower-core | `/proto-td-tower` |
| td-enemy-core | `/proto-td-enemy` |
| td-bullet-system | `/proto-td-bullet` |
| td-wave-spawner | `/proto-td-waves` (인자: `single\|two\|three\|four`) |
| td-core-hp | `/proto-td-core-hp` |
| td-game-rule-timed\|killcount\|coredefense | `/proto-td-rule` (인자: `timed\|kill\|core\|timed+core\|kill+core`) |
| td-camera-pan-zoom | `/proto-td-camera-input` |
| td-camera-mode-toggle | `/proto-td-camera-modes` (조건부) |
| td-hud-* | `/proto-td-hud` (다중 인자) |
| td-minimap | `/proto-td-minimap` |
| td-sprite-pack-default | `/proto-td-art` (인자: `2d`) |
| td-lowpoly-pack-default | `/proto-td-art` (인자: `3d`) |
| td-lowpoly-env-default | `/proto-td-art` (인자: `mixed`) |
| td-sim-parity | `/proto-td-parity` |
| td-playtest-runner | `/proto-td-playtest` |
| stage-pcg-wfc | `/proto-stage-wfc` (인자: `size`, `seed`, `tileset`) — TD 레벨은 `/proto-td-level --wfc` 로도 호출 |

별도 메타 스킬:
- `/proto-td-skeleton` — Phase A 최소 설치 (sim-core + json-importer + 빈 레벨 + 카메라). 장르 확정 직후 1회.
- `/proto-td-all` — Phase A→E 전체 자동 설치. 수강생 질문지 답 수집 완료 후 호출.

## 모듈 번들 매핑 (질문지 답 → 설치할 모듈 세트)

수강생이 Q1~Q8 을 모두 답하면 아래 규칙으로 활성 모듈 목록 확정.
Default 값은 `_questionnaire-td.md` 참조.

```
Base (항상 설치):
  td-sim-core, td-json-importer, td-web-bridge,
  camera-rig, td-camera-pan-zoom,
  td-hub-network, td-resource-ledger,
  td-tower-core, td-enemy-core, td-bullet-system, td-wave-spawner,
  td-hud-budget,
  character-render-kit, hud-kit, sound-kit, save-kit,
  shader-fx-kit, post-process-kit,
  td-sim-parity, td-playtest-runner

Q1 (배치):
  A → td-placement-grid
  B → td-placement-free

Q2 (길찾기):
  A → td-pathing-fixed, stage-graph
  B → td-pathing-flowfield, td-flowfield-builder
  C → td-pathing-dynamic, td-flowfield-builder

Q3 (카메라):
  A → camera-rig[TopDown 모드]
  B → camera-rig[QuarterBuilder 모드]
  C → camera-rig[TopDown + QuarterBuilder + Orbit], td-camera-mode-toggle

Q4 (승패):
  A 포함 → td-game-rule-timed, td-hud-timer
  B 포함 → td-game-rule-killcount, td-hud-killcount
  C 포함 → td-game-rule-coredefense, td-core-hp, td-hud-corehp

Q5 (자원 재생):
  A → td-resource-ledger[fixed]
  B → td-resource-ledger[regen], td-resource-tick
  C → td-resource-ledger[kill], td-kill-reward-hook
  B+C → 위 둘 병합

Q6 (적 경로 수):
  A → td-wave-spawner[singleLane]
  B → td-wave-spawner[multiLane=2~3]
  C → td-wave-spawner[multiLane=4+]

Q7 (맵 크기):
  A → td-level-template-small
  B → td-level-template-medium [+td-minimap 선택]
  C → td-level-template-large, td-minimap [필수]

Q8 (아트):
  A → td-sprite-pack-default, character-render-kit[billboard]
  B → td-lowpoly-pack-default, character-render-kit[skinned]
  C → td-sprite-pack-default, td-lowpoly-env-default, character-render-kit[both]
```

## 모듈 문서 공통 템플릿

각 `design/proto-modules/td-<module>.md` 는 기존 7섹션 템플릿 그대로:

```
1. Purpose       — 뭐 하는 모듈, 왜 필요한가
2. Hard Rules    — _conventions-td.md 참조 + 모듈 고유 제약
3. Public API    — 외부에 노출하는 타입/이벤트/JSON 스키마
4. Dependencies  — Required / Optional + 공유 모듈 import 리스트
5. Default Prefabs/Assets — 구워둘 프리팹·SO·JSON 이름
6. Skill Hook    — /proto-td-xxx 호출 시 수행할 액션 목록
7. Verification  — 설치 후 동작 검증법 (HR-TD-9 sim parity 포함)
```

각 모듈 문서는 **후속 턴에 개별 작성** (모듈 1개 당 1파일, 총 36개). 현 단계에서는 본 카탈로그가 유일한 참조 소스.

## 미결·후속 과제

- **모듈 세부 문서 36개** 작성 — 위 템플릿 따라 `td-<module>.md` 개별 생성.
- **웹 시뮬레이터 레포 구조**: `web-sim/` 디렉토리 초기 스켈레톤 (package.json, schema/, sim/, editor/, playtest/). 현재 TBD.
- **JSON 스키마 초안**: `level.schema.json`, `wave.schema.json`, `tower.schema.json`, `enemy.schema.json`. 웹·Unity 구현 시작 전 확정 필수.
- **결정론 검증 테스트 케이스**: `td-sim-parity` 에 들어갈 표준 seed + 입력 시퀀스 세트 정의.
- **Q4 B (전멸)** 의 "웨이브 종료"와 "모든 적 처치" 판정 정확화 — `_questionnaire-td.md` 미결 사항 참조.
- **공유 모듈 `hud-kit` / `sound-kit` / `save-kit`** 은 아직 정의 파일 없음 — TD 작업 전 혹은 TD 작업 중 별도 작성 필요.
