# Proto Rail-Shooter Modules — Catalog

> 1인칭 건슈팅 (하우스 오브 더 데드류 온-레일 슈터) 프로토 모듈 카탈로그.
> `_genres-index.md` 에서 `rail-shooter` 장르로 라우팅된 뒤
> `_questionnaire-rail-shooter.md` 답변에 따라 활성화할 모듈 번들을 본 문서에서 조회.
>
> 각 모듈은 최종적으로 `.claude/skills/proto-<name>/SKILL.md` 로 승격된다.
> 공유 모듈은 본 문서에서 **참조만** 하고, 실제 정의는 각자 파일에 둔다.

## 핵심 컨셉 요약

- **장르**: 1인칭 온-레일 슈터. 참고: 하우스 오브 더 데드 / 타임 크라이시스.
- **핵심 메커닉**: 시점/이동은 제한, **에임만 자유**, 엄폐 + 자리 이동 + 불릿타임으로 생존.
- **카메라**: 기존 `camera-rig` 에 `FixedRail` / `WaypointRail` / `SlotRail` 모드 추가.
- **전투 모델**: 적 → 플레이어 (일방). 플레이어는 엄폐로만 회피. 발사 = Raycast hitscan.
- **레벨**: 섹션 선형 체인 (분기 없음).
- **결정론**: 필수 아님. seeded RNG 권장 (스폰 재현성).
- **데이터**: ScriptableObject (SO) 기반. JSON 익스포트는 선택.

## 전체 모듈 맵

### Rail Shooter 전용 모듈

| # | 모듈 ID | Tier | 영역 | 한 줄 요약 |
|---|---|---|---|---|
| 1 | `rail-input-root` | 코어·입력 | 입력 | Input Action Asset 기본 바인딩 (Shoot/Reload/Cover/BulletTime/Swap) |
| 2 | `rail-camera-static` | 코어·카메라 | 카메라 | Q1-A: 완전 고정 카메라 |
| 3 | `rail-camera-waypoint` | 코어·카메라 | 카메라 | Q1-B: 웨이포인트 자동 진행 (`stage-graph` 재활용) |
| 4 | `rail-camera-slot` | 코어·카메라 | 카메라 | Q1-C: 슬롯 간 점프 전환 |
| 5 | `rail-slot-manager` | 코어·카메라 | 카메라 | Q1-C/Q2-C 공용 슬롯 정의·전환 로직 |
| 6 | `rail-aim-controller` | 코어·입력 | 에임 | 크로스헤어 이동, 스크린→월드 Ray 변환 |
| 7 | `rail-hit-raycast` | 코어·전투 | 전투 | `Physics.Raycast` 기반 피격 판정 + 이벤트 |
| 8 | `rail-weapon-core` | 코어·전투 | 전투 | 무기 기본 스펙 (사거리/데미지/연사/탄창) |
| 9 | `rail-weapon-single` | 코어·전투 | 전투 | Q4-A: 단일 무기 (피스톨 고정) |
| 10 | `rail-weapon-multi` | 코어·전투 | 전투 | Q4-B: 다중 무기 슬롯 |
| 11 | `rail-weapon-swap-input` | 코어·전투 | 전투 | Q4-B: 휠/버튼 무기 교체 입력 |
| 12 | `rail-weapon-upgrade` | 코어·전투 | 전투 | Q4-C: 단일 무기 업그레이드 트리 |
| 13 | `rail-weapon-slot-default` | 코어·전투 | 전투 | Q4-C: 업그레이드 기본 슬롯 (데미지/연사/정확도) |
| 14 | `rail-reload-manual` | 코어·전투 | 전투 | Q5-B: 수동 재장전 |
| 15 | `rail-reload-auto` | 코어·전투 | 전투 | Q5-C: 자동 재장전 |
| 16 | `rail-reload-input` | 코어·전투 | 전투 | Q5-B: 재장전 입력 핸들러 |
| 17 | `rail-cover-crouch` | 코어·생존 | 엄폐 | Q2-B: 제자리 엄폐 상태 |
| 18 | `rail-cover-slot` | 코어·생존 | 엄폐 | Q2-C: 슬롯 위치 엄폐 |
| 19 | `rail-cover-input` | 코어·생존 | 엄폐 | 엄폐 입력 핸들러 (Q2-B/C 공용) |
| 20 | `rail-hp-system` | 코어·생존 | 생존 | 플레이어 HP + 피격 이벤트 |
| 21 | `rail-player-state` | 코어·생존 | 생존 | Uncovered/Covering/Dead 상태 머신 |
| 22 | `rail-bullettime` | 코어·특수 | 특수 | Q3: 불릿타임 활성/비활성 컨트롤러 |
| 23 | `rail-bt-gauge-cooldown` | 코어·특수 | 특수 | Q3-B: 쿨다운 기반 게이지 |
| 24 | `rail-bt-gauge-time` | 코어·특수 | 특수 | Q3-C: 시간 자연충전 게이지 |
| 25 | `rail-bt-gauge-kill` | 코어·특수 | 특수 | Q3-D: 킬 기반 게이지 |
| 26 | `rail-enemy-core` | 코어·적 | 적 | 적 HP·이동·AI 3상태 FSM (Approach/Attack/Dead) |
| 27 | `rail-enemy-pack-zombie` | 에셋·적 | 적 | Q7-A: 좀비 팩 (3종) |
| 28 | `rail-enemy-pack-robot` | 에셋·적 | 적 | Q7-B: 로봇 팩 (3종) |
| 29 | `rail-enemy-pack-soldier` | 에셋·적 | 적 | Q7-C: 군인 팩 (3종) |
| 30 | `rail-enemy-pack-creature` | 에셋·적 | 적 | Q7-D: 크리처 팩 (3종) |
| 31 | `rail-spawn-scheduler` | 코어·적 | 스폰 | `SpawnEvent` 시퀀스 재생기 |
| 32 | `rail-section-single` | 코어·레벨 | 섹션 | Q8-A: 1섹션 |
| 33 | `rail-section-chain` | 코어·레벨 | 섹션 | Q8-B/C: 섹션 선형 체인 |
| 34 | `rail-section-transition` | 코어·레벨 | 섹션 | 섹션 전환 연출 (`timeline-cue` + 디졸브) |
| 35 | `rail-section-checkpoint` | 코어·레벨 | 섹션 | Q8-C: 체크포인트 저장 (`save-kit` 연계) |
| 36 | `rail-rule-section` | 룰 | 승패 | Q6-A: 모든 섹션 완주 승리 |
| 37 | `rail-rule-boss` | 룰 | 승패 | Q6-B: 보스 처치 승리 |
| 38 | `rail-rule-score` | 룰 | 승패 | Q6-C: 점수 도달 승리 |
| 39 | `rail-boss-core` | 룰 | 승패 | 보스 유닛 (HP, 페이즈, 약점) |
| 40 | `rail-score-system` | 룰 | 승패 | 점수 계산 (적 종류·연속 킬·크로스헤어 정확도) |
| 41 | `rail-hud-crosshair` | UI | HUD | 크로스헤어 위젯 (항상 활성) |
| 42 | `rail-hud-ammo` | UI | HUD | 탄창 UI (Q5-B/C) |
| 43 | `rail-hud-bt` | UI | HUD | 불릿타임 게이지 (Q3-B/C/D) |
| 44 | `rail-hud-score` | UI | HUD | 점수 위젯 (Q6-C) |
| 45 | `rail-hud-bosshp` | UI | HUD | 보스 HP 바 (Q6-B) |
| 46 | `rail-hud-progress` | UI | HUD | 섹션 진행 바 (Q6-A) |
| 47 | `rail-hud-playerhp` | UI | HUD | 플레이어 HP (항상 활성) |

### 공유 모듈 (imports)

| 모듈 ID | 출처 | Rail Shooter 에서의 역할 |
|---|---|---|
| `camera-rig` | `camera-rig.md` | `FixedRail` / `WaypointRail` / `SlotRail` 모드 추가 |
| `stage-graph` | `stage-graph.md` | Q1-B 웨이포인트 경로 그래프 |
| `character-render-kit` | `character-render-kit.md` | 적 렌더 (빌보드/스킨드 둘 다 가능) |
| `timeline-cue` | `timeline-cue.md` | 섹션 전환 / 보스 등장 / 게임 오버 컷신 |
| `shader-fx-kit` | `_genres-index.md` | 적 등장·사망 디졸브 |
| `post-process-kit` | `_genres-index.md` | `pp-bullettime`, `pp-hit-flash`, `pp-horror`, `pp-cyber`, `pp-weird` |
| `hud-kit` | (예정) | HUD 프레임 |
| `sound-kit` | (예정) | BGM/SFX |
| `save-kit` | (예정) | 체크포인트, 최고 점수 |

## 의존성 그래프

```
rail-input-root
   ▲
   │ (모든 입력 핸들러가 의존)
   │
rail-aim-controller ──► rail-hit-raycast
                              ▲
                              │
rail-weapon-core ─────────────┤
   ▲                          │
   ├── rail-weapon-single     │
   ├── rail-weapon-multi ─── rail-weapon-swap-input
   └── rail-weapon-upgrade ── rail-weapon-slot-default
                              │
rail-reload-{manual,auto} ────┤
  (manual) ──► rail-reload-input

rail-camera-{static,waypoint,slot} ──► camera-rig (모드 추가)
rail-camera-waypoint ──► stage-graph
rail-camera-slot / rail-cover-slot ──► rail-slot-manager

rail-cover-{crouch,slot} ──► rail-player-state
   ▲
   └── rail-cover-input
        │
rail-hp-system ──► rail-player-state

rail-bullettime
   ▲
   ├── rail-bt-gauge-cooldown   (Q3-B)
   ├── rail-bt-gauge-time       (Q3-C)
   └── rail-bt-gauge-kill       (Q3-D)
        │
        └─► post-process-kit (pp-bullettime)

rail-enemy-core ──► character-render-kit
   ▲
   └── rail-enemy-pack-{zombie,robot,soldier,creature}
        │
        └─► shader-fx-kit (dissolve-in/out)

rail-spawn-scheduler ──► rail-enemy-core

rail-section-{single,chain,checkpoint}
   ▲
   └── rail-section-transition ──► timeline-cue, shader-fx-kit

rail-rule-section  ──► rail-section-chain
rail-rule-boss     ──► rail-boss-core ──► rail-enemy-core
rail-rule-score    ──► rail-score-system

rail-hud-* ──► hud-kit
```

## Phase 착수 순서

**Phase A — 시점 + 에임** (1 → 6)
1. `rail-input-root` → 2. `camera-rig` + Q1 선택 카메라 모듈 → 3. `rail-aim-controller` → 4. `rail-hud-crosshair`

☑ 종결 조건: Play 시 크로스헤어가 마우스 따라 움직이고, 카메라는 선택된 레일/슬롯 모드로 동작. 사격 아직 없음.

**Phase B — 무기 + 발사 + 피격** (6 → 16)
5. `rail-hit-raycast` → 6. `rail-weapon-core` → 7. Q4 무기 모듈 → 8. Q5 재장전 모듈 → 9. `rail-hud-ammo` (Q5-B/C)

☑ 종결 조건: 허공에 대고 사격 시 탄 수 차감(Q5-B/C), 자동 재장전(Q5-C), 히트 이벤트 발행.

**Phase C — 적 + 스폰 + 엄폐 + HP** (17 → 31)
10. `rail-enemy-core` → 11. Q7 적 팩 → 12. `rail-spawn-scheduler` → 13. `rail-hp-system` + `rail-player-state` → 14. Q2 엄폐 모듈 + `rail-cover-input` → 15. `rail-hud-playerhp`

☑ 종결 조건: 스폰된 적이 플레이어에게 접근/공격, 엄폐 시 피격 무효, 피격 시 HP 감소, HP 0 시 섹션 재시작.

**Phase D — 섹션 + 규칙** (32 → 46)
16. Q8 섹션 모듈 → 17. `rail-section-transition` → 18. Q6 규칙 모듈 + 관련 HUD → 19. 체크포인트(Q8-C)

☑ 종결 조건: 섹션 1→2→...→N 순차 진행, 마지막 섹션 클리어 시 승리 화면, 보스/점수 조건(Q6) 정상 동작.

**Phase E — 불릿타임 + 연출 + 아트** (22 → 25, PP/FX)
20. Q3 불릿타임 모듈 → 21. `post-process-kit` (`pp-bullettime` + Q7 테마 PP) → 22. `shader-fx-kit` (dissolve 적용) → 23. `timeline-cue` (섹션 전환/보스 등장)

☑ 종결 조건: 불릿타임 발동 시 적 슬로우 + PP 효과, 적 등장/사망 디졸브, 섹션 전환 컷신.

## 스킬 매핑

모듈 1개 ≈ 스킬 1개. 네이밍: `/proto-<kebab-case>`.

| 모듈 (대표) | 스킬 |
|---|---|
| rail-input-root | `/proto-rs-input` |
| rail-camera-{static,waypoint,slot} | `/proto-rs-camera` (인자: `static\|waypoint\|slot`) |
| rail-slot-manager | `/proto-rs-slots` |
| rail-aim-controller | `/proto-rs-aim` |
| rail-hit-raycast | `/proto-rs-hit` |
| rail-weapon-core + Q4 변형 | `/proto-rs-weapon` (인자: `single\|multi\|upgrade`) |
| rail-reload-{manual,auto} | `/proto-rs-reload` (인자: `none\|manual\|auto`) |
| rail-cover-{crouch,slot} + input | `/proto-rs-cover` (인자: `none\|crouch\|slot`) |
| rail-hp-system + player-state | `/proto-rs-hp` |
| rail-bullettime + Q3 게이지 | `/proto-rs-bullettime` (인자: `none\|cooldown\|time\|kill`) |
| rail-enemy-core | `/proto-rs-enemy-core` |
| rail-enemy-pack-{zombie,robot,soldier,creature} | `/proto-rs-enemy-pack` (인자: `zombie\|robot\|soldier\|creature`) |
| rail-spawn-scheduler | `/proto-rs-spawn` |
| rail-section-{single,chain} + transition | `/proto-rs-sections` (인자: `single\|chain\|chain+checkpoint`) |
| rail-rule-{section,boss,score} | `/proto-rs-rule` (인자: `section\|boss\|score\|section+boss`) |
| rail-boss-core | `/proto-rs-boss` |
| rail-score-system | `/proto-rs-score` |
| rail-hud-* | `/proto-rs-hud` (다중 인자) |

별도 메타 스킬:
- `/proto-rs-skeleton` — Phase A 최소 설치 (input + camera + aim + crosshair). 장르 확정 직후 1회.
- `/proto-rs-all` — Phase A→E 전체 자동 설치. 질문지 답 수집 완료 후 호출.

## 모듈 번들 매핑 (질문지 답 → 설치할 모듈 세트)

```
Base (항상 설치):
  rail-input-root,
  camera-rig (모드 1~3개 Q1 에 따라),
  rail-aim-controller, rail-hud-crosshair,
  rail-hit-raycast, rail-weapon-core,
  rail-enemy-core, rail-spawn-scheduler,
  rail-hp-system, rail-player-state, rail-hud-playerhp,
  character-render-kit, hud-kit, sound-kit, save-kit,
  shader-fx-kit, post-process-kit,
  timeline-cue

Q1 (카메라 레일):
  A → rail-camera-static
  B → rail-camera-waypoint, stage-graph
  C → rail-camera-slot, rail-slot-manager

Q2 (엄폐):
  A → (엄폐 없음, HP 만)
  B → rail-cover-crouch, rail-cover-input
  C → rail-cover-slot, rail-cover-input, rail-slot-manager  (Q1-C 와 중복 시 공용)

Q3 (불릿타임):
  A → (없음)
  B → rail-bullettime, rail-bt-gauge-cooldown, rail-hud-bt, post-process-kit[pp-bullettime]
  C → rail-bullettime, rail-bt-gauge-time, rail-hud-bt, post-process-kit[pp-bullettime]
  D → rail-bullettime, rail-bt-gauge-kill, rail-hud-bt, post-process-kit[pp-bullettime]

Q4 (무기):
  A → rail-weapon-single
  B → rail-weapon-multi, rail-weapon-swap-input
  C → rail-weapon-upgrade, rail-weapon-slot-default

Q5 (재장전):
  A → (없음)
  B → rail-reload-manual, rail-reload-input, rail-hud-ammo
  C → rail-reload-auto, rail-hud-ammo

Q6 (승리):
  A → rail-rule-section, rail-hud-progress
  B → rail-rule-boss, rail-boss-core, rail-hud-bosshp
  C → rail-rule-score, rail-score-system, rail-hud-score
  A+B → 위 둘 병합

Q7 (테마/적 팩):
  A → rail-enemy-pack-zombie, post-process-kit[pp-horror]
  B → rail-enemy-pack-robot, post-process-kit[pp-cyber]
  C → rail-enemy-pack-soldier
  D → rail-enemy-pack-creature, post-process-kit[pp-weird]

Q8 (섹션 수):
  A → rail-section-single
  B → rail-section-chain, rail-section-transition
  C → rail-section-chain, rail-section-transition, rail-section-checkpoint, save-kit (필수)
```

## 모듈 문서 공통 템플릿

각 `design/proto-modules/rail-<module>.md` 는 기존 7섹션 템플릿 그대로:

```
1. Purpose
2. Hard Rules     — _conventions-rail-shooter.md 참조 + 모듈 고유 제약
3. Public API     — 타입/이벤트
4. Dependencies   — Required / Optional + 공유 모듈 import 리스트
5. Default Prefabs/Assets
6. Skill Hook     — /proto-rs-xxx 실행 액션
7. Verification
```

모듈 세부 문서 47개는 **후속 턴에 개별 작성**. 현 단계에서는 본 카탈로그가 유일한 참조 소스.

## 미결·후속 과제

- **PP 프로파일 `pp-horror` / `pp-cyber` / `pp-weird` / `pp-hit-flash`**: 스펙 미정. `_genres-index.md` `post-process-kit` 섹션에 레시피 ID 추가 필요.
- **보스 페이즈 시스템**: `rail-boss-core` 페이즈 전환 트리거, 약점 부위 로직 등 상세 설계 TBD.
- **적 팩 자산 출처**: Q7 각 팩의 실제 모델/스프라이트 — Unity Asset Store 프리셋 가정. 승인된 출처 리스트 필요.
- **입력 기본 키 매핑**: `_conventions-rail-shooter.md` HR-RS-9 에 언급된 키 매핑을 실제 Input Action Asset 으로 구워둘 것.
- **모듈 세부 문서 47개**: 후속 작성.
- **공유 `hud-kit` / `sound-kit` / `save-kit`** 정의 파일 부재 — Rail Shooter 진행 중 또는 전에 별도 작성 필요.
- **스코어 시스템 Q6-C 밸런싱 기본값**: `Rule_Score_5000.asset` 의 적별 점수 테이블 TBD.
