# Proto Modules — Progress Snapshot

> 다장르 프로토 디자인 번들 작업 진행 현황.
> 세션 간 컨텍스트 보존 및 다음 작업자가 즉시 이어갈 수 있도록 작성된 스냅샷.
>
> **최종 갱신**: 2026-04-23
> **작업 브랜치**: `Proto`
> **원격**: `origin/Proto` (모든 커밋 푸시 완료)

## 1. 목표와 범위

- **목표**: 수강생이 첫 메시지에 한 문장 아이디어를 말하면, Claude 가 장르를
  자동 라우팅하고 사전 정의된 질문을 순차 진행한 뒤, 답변 매트릭스에 맞춰
  미리 구워둔 모듈 번들을 설치하는 파이프라인.
  (과거에는 `/proto-start` 스킬이 이 진입을 명시 명령으로 받았으나,
  Proto 패키지 마감 과정에서 스킬 제거·자연어 대화 진입으로 통합됨.
  스킬 제거 배경은 `.vkl/runtime/case_logs/` 후반 기록 참조.)
- **지원 장르 3종**:
  - `turn3d` — 블아류 쿼터뷰 유사턴제 3D 액션
  - `td` — 타워 디펜스 (네트워크 허브 연결형, 엔드필드류)
  - `rail-shooter` — 1인칭 건슈팅 (하우스 오브 더 데드류 온-레일 슈터)
- **비목표 (의도적 제외)**:
  - 혼합 장르 지원
  - 런타임 즉흥 장르/모듈 선택
  - 실제 스킬 구현 (`/proto-td-*`, `/proto-rs-*` 등) — 현 시점은 **디자인 문서만**

## 2. 파일 인벤토리 (design/proto-modules/)

### 2.1 공통 인프라

| 파일 | 용도 | 상태 |
|---|---|---|
| `_genres-index.md` | 장르 라우팅 + 공유 모듈 매트릭스 + VFX 레시피 카탈로그 | ✅ |
| `_conventions.md` | 공통 Hard Rules (HR-1 ~ HR-11) | ✅ (기존) |
| `_progress.md` | 본 문서 (진행 스냅샷) | ✅ |

### 2.2 장르별 3종 세트

| 장르 | 카탈로그 | 질문지 | 컨벤션 | 개별 모듈 문서 |
|---|---|---|---|---|
| `turn3d` | `_catalog-turn3d.md` ✅ | `_questionnaire-turn3d.md` ✅ | `_conventions.md` (공통) | 9/16 완료 (기존) — 7개 미작성 |
| `td` | `_catalog-td.md` ✅ | `_questionnaire-td.md` ✅ | `_conventions-td.md` ✅ | 37/37 완료 ✅ (+ `stage-pcg-wfc` 추가 2026-04-24) |
| `rail-shooter` | `_catalog-rail-shooter.md` ✅ | `_questionnaire-rail-shooter.md` ✅ | `_conventions-rail-shooter.md` ✅ | 0/47 **미작성** |

### 2.3 공유 모듈 개별 문서

| 파일 | 상태 |
|---|---|
| `camera-rig.md` | ✅ (기존) |
| `character-render-kit.md` | ✅ (기존) |
| `combat-scheme.md` | ✅ (기존) |
| `movement-core.md` | ✅ (기존) |
| `path-picker.md` | ✅ (기존) |
| `pcg-city-layout.md` | ✅ (기존) |
| `stage-graph.md` | ✅ (기존) |
| `timeline-cue.md` | ✅ (기존) |
| `turn-system.md` | ✅ (기존) |
| `hud-kit.md` | ✅ (신규) |
| `sound-kit.md` | ✅ (신규) |
| `save-kit.md` | ✅ (신규) |
| `shader-fx-kit.md` | ✅ (신규) |
| `post-process-kit.md` | ✅ (신규) |
| `stage-pcg-wfc.md` | ✅ (2026-04-24, 장르 중립 WFC 솔버, TD Q7 연계) |

### 2.4 TD 개별 모듈 문서 (36/36 완료)

모두 7섹션 템플릿 (Purpose / Hard Rules / Public API / Dependencies / Default Prefabs/Assets / Skill Hook / Verification):

```
td-sim-core             td-json-importer        td-web-bridge
td-level-template-small td-level-template-medium td-level-template-large
td-placement-grid       td-placement-free
td-hub-network          td-resource-ledger      td-resource-tick     td-kill-reward-hook
td-pathing-fixed        td-pathing-flowfield    td-pathing-dynamic   td-flowfield-builder
td-tower-core           td-enemy-core           td-bullet-system     td-wave-spawner
td-core-hp              td-game-rule-timed      td-game-rule-killcount td-game-rule-coredefense
td-camera-pan-zoom      td-camera-mode-toggle
td-hud-timer            td-hud-killcount        td-hud-corehp        td-hud-budget   td-minimap
td-sprite-pack-default  td-lowpoly-pack-default td-lowpoly-env-default
td-sim-parity           td-playtest-runner
```

## 3. 커밋 히스토리 (본 작업 세션)

| 커밋 | 내용 |
|---|---|
| `1cc49fa` | 다장르 프로토 디자인 번들 (TD + Rail Shooter 3종 세트) + VFX 공유 모듈 참조 |
| `399fdcf` | turn3d 장르 질문지 + PP 테마 레시피 3종 (pp-pastel/dark/retro) |
| `81ce5b1` | 공유 모듈 5종 (hud/sound/save/shader-fx/post-process) + /proto-start 장르 라우팅 |
| `27e9a76` | Proto 작업 규칙 5 — .claude/ 수정은 stage+mv 패턴 필수 |
| `e4c50b4` | TD 36 모듈 개별 문서 |

직전 베이스 커밋: `36eaa02 docs(proto): Proto 모듈 카탈로그 초안 — 9/16 모듈 + 하드룰 HR-11 추가`

## 4. 런타임 동작 플로우 (완성된 파이프라인)

```
수강생: /proto-start "좀비 피해서 3분 버티는 게임"
   ↓
Claude Phase 0: git branch 확인 → Proto 확인
Claude Phase 1: 아이디어 수집
Claude Phase 1.5: _genres-index.md 라우팅
                    ↓
         ┌──────────┼──────────┐
         ↓          ↓          ↓
     turn3d        td      rail-shooter       (매칭 없음)
         ↓          ↓          ↓                 ↓
Claude Phase 1.6: _questionnaire-<genre>.md  ← (generic)
   Q1 → Q2 → ... → Q8 (한 번에 하나씩)
                    ↓
Claude Phase 2: design/proto-concept.md 작성 (장르 + 답변 매트릭스 + 설치 계획)
                    ↓
Claude Phase 3~6: Unity 스크립트 작성 + 자동 커밋/푸시
                    ↓
Claude Phase 7: 이터레이션 루프 (수강생 추가 요청)
```

**현재 제약**: Phase 2 의 "설치 계획"은 참조 문서만 존재. 실제 모듈 설치 스킬
(`/proto-td-*`, `/proto-rs-*`, `/proto-turn3d-*`) 은 **미구현**.
Phase 4 는 현재도 generic WASD 컨트롤러만 작성됨.

## 5. 주요 설계 결정

### 5.1 장르별 파일 분기

- 단일 `_catalog.md` 가 장르가 늘어날수록 혼탁해지므로 `_catalog-<genre>.md` 로 분리.
- 질문지(`_questionnaire-<genre>.md`) + 컨벤션(`_conventions-<genre>.md`) 까지 3종 세트.
- 장르 전환은 **처음부터 재시작** (부분 이전 금지).

### 5.2 "미리 구워둔 번들" 원칙

- 수강생 답변이 오면 Claude 는 판단 없이 카탈로그의 "모듈 번들 매핑" 을 조회해 설치.
- 런타임 즉흥 판단 금지 — 모든 분기와 대응 구성은 사전 정의.
- 질문지에 없는 답변 → 자동 Default, 1문장 메모.

### 5.3 웹-first 시뮬 (TD 한정)

- TD 장르의 시뮬 코어는 결정론 순수 C# (HR-TD-1).
- 웹(TS) 에 동일 로직 구현 + JSON 스키마 공유 + bit-exact 검증 (HR-TD-9).
- 레벨 에디터는 웹에서, Unity 는 프레젠테이션 + 임포터.
- **미구현**: 웹 시뮬 실제 코드 (`web-sim/`) 는 아직 없음 — 디자인 문서만.

### 5.4 공유 VFX/셰이더 참조

- `_genres-index.md` 에 `shader-fx-kit` (디졸브) / `post-process-kit` (PP 패스) 공통 참조.
- 레시피 ID 표준화:
  - 장르 공용: `dissolve-in/out/edge-glow`, `pp-bullettime/ult-active/low-hp/hit-flash/grayscale-over`
  - turn3d 테마: `pp-pastel`, `pp-dark`, `pp-retro`
  - rail-shooter 테마: `pp-horror`, `pp-cyber`, `pp-weird`
- Hard Rule HR-12/13 제안 상태 (Shader Graph 강제 / PP 컨트롤러 경유).

### 5.5 `.claude/` 수정은 Stage + mv 패턴

- 프로젝트 CLAUDE.md 작업 규칙 5 에 박힘.
- 메모리 (`feedback_claude_dir_staging.md`) 에도 저장.
- `.gitignore` 에 `.stage/` 등록.
- 적용 이유: 직접 Edit/Write 가 매번 권한 프롬프트를 띄워 수강생 흐름 단절.

## 6. 남은 작업

### 6.1 즉시 후속 (디자인 문서)

- [ ] **Rail Shooter 47 모듈 개별 문서** — 장르별 3턴 분할의 2/3 차례
- [ ] **turn3d 누락 모듈 7개 개별 문서** — 기존 카탈로그는 16개지만 파일은 9개만 있음
  - 없는 것: ability-catalog, damage-system, effect-kit, enemy-behavior, sound-kit(turn3d 특수), hud-kit(turn3d 특수), save-kit(turn3d 특수)
  - 공유 모듈과 중복 가능성 검토 필요 — 장르별 특수화 vs 공유 모듈 재활용

### 6.2 중기 (실제 구현)

- [ ] TD 모듈 실제 Unity 구현 — 스킬 `/proto-td-*` 작성
- [ ] **`stage-pcg-wfc` 실 구현** — 솔버 코어 + `WfcTileSO` / `WfcTileSetSO` + 기본 타일셋 12종 + `TdLevelWfcAdapter`. `td-level-template-*` 의 JSON 생성 스텝이 본 모듈로 연결됨.
- [ ] TD 웹 시뮬레이터 (`web-sim/`) 실 구현 — TS 코어 + 레벨 에디터 + Monte Carlo 러너
- [ ] JSON 스키마 초안 (`level.schema.json`, `wave.schema.json`, `tower.schema.json`, `enemy.schema.json`) — **`stage-pcg-wfc` 의 JSON 출력이 `level.schema.json` 초안 1차 레퍼런스**
- [ ] Rail Shooter 모듈 실제 Unity 구현
- [ ] turn3d 모듈 실제 Unity 구현

### 6.3 연계 업데이트

- [ ] VKL oracle 등록 — 각 HR-TD-XX / HR-RS-XX 에 oracle ID 부여
- [ ] HR-12 / HR-13 (Shader Graph / PP 컨트롤러) 제안 → 정식 HR 승격 결정
- [ ] `.claude/skills/_hidden/` 에 있는 TD/Rail/turn3d 관련 스킬이 없는지 감사
- [ ] `/proto-start` 의 Phase 2 컨셉 파일 템플릿 (`docs/templates/proto-game-concept.md`) 에
      장르 답변 매트릭스 섹션 추가

### 6.4 미결 사항 (강사 검토 대기)

- ~~TD Q4 "전멸(kill)" 의 "모든 웨이브 종료" 판정 로직 정확화~~ → **결정 (2026-04-23)**: 게임 완결성 우선. `AllWavesSpawned && AliveCount==0 && Kills>=Target` AND 조건. 사살만 카운트, 누수 제외. `td-game-rule-killcount.md` 에 반영.

**아래 항목들은 "지금 막고 있지 않음" 으로 판정되어 TBD 유지. 재방문 조건 명기.**

- **Rail Shooter 보스전 페이즈 전환 / 약점 부위 로직** — *재방문 조건*: 수강생이 Q6-B (보스 처치) 를 고르거나, `rail-boss-core.md` 모듈 문서를 실제 작성하는 시점.
- **turn3d Q2-C Rhythm BGM 비트 메타데이터 스펙** — *재방문 조건*: 수강생이 turn3d Q2-C (리듬 모드) 를 고르는 시점. 그 전까지는 리듬 모드 자체가 유휴 분기.
- **각 장르 아트 팩 에셋 출처** — *재방문 조건*: 실 구현(Unity 스크립트 작성) 단계 진입. 디자인 문서 단계에서는 "Unity Asset Store 프리셋 사용" 기본값으로 충분.
- **공유 `hud-kit` / `sound-kit` / `save-kit` 의 장르별 특수화 예시** — *재방문 조건*: 장르별 모듈 47/36/16 개를 실 구현하며 특수 위젯이 실제로 필요해지는 시점. API 는 정의됨, 구체 예시는 그때 자연스럽게 드러남.

## 7. 다음 세션이 알아야 할 것

1. **이 스냅샷이 최신**. 세션 진입 시 먼저 읽기.
2. **파일 수정 규칙**:
   - `design/` 하위는 자유 수정.
   - `.claude/` 하위는 `.stage/` → `mv` 패턴 필수 (CLAUDE.md 작업 규칙 5).
3. **커밋 정책**: 의미 단위 작업 종료 시 자동 커밋 + 푸시 (Proto CLAUDE.md 규칙 2).
   커밋 메시지 한글.
4. **VKL 검증**: 판정 시 oracle ID 필수 (vkl-bridge.md 규칙).
   이 작업은 디자인 문서 수준이라 oracle 부착 대기 중.
5. **Rail Shooter 47 모듈** 작성 시 TD 36 모듈과 동일한 7섹션 템플릿 사용.
   네이밍: `Proto.RailShooter.<Sub>`, 프리팹 `RS_<Kind>_<Name>`, SO `RS_<Kind>_<Name>.asset`.
6. **turn3d 누락 7모듈** 은 기존 9개 파일의 스타일을 따를 것 (블아 컨셉 유지).

## 8. 재개 힌트 (즉시 작업할 때)

**Rail Shooter 47 모듈 작성 시**:
```
참조:
- _catalog-rail-shooter.md 의 "전체 모듈 맵" 섹션
- _conventions-rail-shooter.md 의 HR-RS-1 ~ HR-RS-10
- TD 36 모듈 (td-*.md) 를 템플릿으로 사용

필요한 47개 모듈 ID 는 _catalog-rail-shooter.md 에 열거됨:
rail-input-root, rail-camera-{static,waypoint,slot}, rail-slot-manager,
rail-aim-controller, rail-hit-raycast, rail-weapon-{core,single,multi,swap-input,upgrade,slot-default},
rail-reload-{manual,auto,input}, rail-cover-{crouch,slot,input},
rail-hp-system, rail-player-state,
rail-bullettime, rail-bt-gauge-{cooldown,time,kill},
rail-enemy-core, rail-enemy-pack-{zombie,robot,soldier,creature},
rail-spawn-scheduler,
rail-section-{single,chain,transition,checkpoint},
rail-rule-{section,boss,score}, rail-boss-core, rail-score-system,
rail-hud-{crosshair,ammo,bt,score,bosshp,progress,playerhp}
```

**turn3d 누락 모듈 작성 시**:
```
참조:
- _catalog-turn3d.md 의 "16 모듈 전체 맵"
- 기존 9개 파일 스타일

작성 필요: ability-catalog, damage-system, effect-kit, enemy-behavior,
          + sound-kit/hud-kit/save-kit 이 turn3d 특수화 필요한지 판단
```
