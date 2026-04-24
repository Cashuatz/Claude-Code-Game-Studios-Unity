# Proto Rail-Shooter — Questionnaire & Branch Matrix

> 수강생이 첫 메시지에 "1인칭 건슈팅 / 하우스 오브 더 데드류 / 온-레일 슈터"
> 컨셉을 말했을 때, Claude 가 이 문서를 참조해 수강생에게 질문하고, 답변에 따라
> 활성화할 모듈 번들을 결정한다. 모든 분기는 사전 정의되어 있으며 런타임 즉흥 판단 금지.

## 장르 핵심 전제 (고정, 질문 불필요)

수강생에게 묻지 않고 고정되는 값 — `_conventions-rail-shooter.md` 에서 강제:

- **시점**: 1인칭. 3인칭 어깨너머는 프로토 범위 밖.
- **플레이어 몸체**: 없음 (시점 + 무기 모델만 보임). 전신 리그 없음.
- **에임**: 마우스/컨트롤러 스틱 → 화면 조준점 이동. 카메라 회전 X, 크로스헤어만 이동.
  (Yaw/Pitch 카메라 회전은 Q1 에서 별도 분기)
- **이동**: 기본적으로 **플레이어는 걸어다니지 않는다**. 카메라가 레일을 따라 흐르거나
  슬롯 간 점프만 (Q1/Q2 결정).
- **데미지 흐름**: 적 → 플레이어 (일방). 플레이어는 엄폐로만 피격 회피.
- **피격 판정**: 레이캐스트 히트스캔 기본. 투사체(총알 비행) 는 선택 (Q4).

## 질문 제시 규칙

- 한 번에 하나씩. 답 듣고 다음 질문.
- 평문 한국어 선택지, 기술 용어 금지.
- "몰라" / "아무거나" → Default.
- 선택지 밖 답 → Default + 1문장 메모: "일단 기본값으로 가고, 나중에 바꿔도 돼요."
- Q1~Q8 완료 전 코드 작성 금지.

---

## Q1. 카메라 진행 방식 (Camera Rail)

**수강생 질문 원문**
> "플레이어가 맵을 어떻게 돌아다녀요? (1) 그냥 그 자리에서 쭉 쏴요 (2) 길을 따라 카메라가 자동으로 움직이고 나는 쏘기만 (3) 몇 군데 정해진 자리 중에 골라서 이동"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (완전 고정)** | "그 자리에서 쭉" | `rail-camera-static`, `rail-aim-controller` | HR-RS-1 | `CameraProfile_RailStatic.asset` |
| **B (웨이포인트 자동 진행)** | "자동으로 움직여" | `rail-camera-waypoint`, `rail-aim-controller`, `stage-graph` (레일 경로) | HR-RS-1 | `RailPath_Default.asset` (웨이포인트 6개) |
| **C (슬롯 이동)** | "몇 군데 자리" | `rail-camera-slot`, `rail-aim-controller`, `rail-slot-manager` | HR-RS-1 | `SlotSet_3Positions.asset` |
| **Default** | B | — | — | — |

**후속 메모**:
- A 는 가장 단순, 학습용으로 좋음. 하지만 재미가 금방 소진.
- B 는 하우스 오브 더 데드 정통 스타일.
- C 는 타임 크라이시스 스타일. Q2 엄폐와 강하게 결합.

---

## Q2. 엄폐 시스템 (Cover)

**수강생 질문 원문**
> "엄폐(숨기) 기능은 어떻게? (1) 없어요, 그냥 맞아요 (2) 버튼 누르면 그 자리에서 숨어요 (3) 지정된 엄폐물 위치로 가서 숨어요"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (없음)** | "없음" | `rail-hp-system` (단순 체력만) | — | `HP_Default_100.asset` |
| **B (제자리 엄폐)** | "그 자리에서 숨어" | `rail-cover-crouch`, `rail-hp-system`, `rail-cover-input` | HR-RS-2 | `CoverConfig_Crouch.asset` |
| **C (슬롯 엄폐)** | "엄폐물 위치로" | `rail-cover-slot`, `rail-hp-system`, `rail-cover-input`, `rail-slot-manager` | HR-RS-2 | `CoverSet_3Slots.asset` |
| **Default** | B | — | — | — |

**후속 메모**:
- Q1 C (슬롯 이동) + Q2 C (슬롯 엄폐) 조합 시 `rail-slot-manager` 1개로 둘 다 커버.
- Q1 A (완전 고정) + Q2 A (엄폐 없음) 조합은 **게임오버 = 5초 맞으면 끝** 급 난이도. 수강생에게 1문장 경고.

---

## Q3. 불릿타임 (Bullet Time)

**수강생 질문 원문**
> "불릿타임(적이 슬로우 되는 기능) 넣을까요? 넣으면 어떻게 충전해요? (1) 안 넣음 (2) 쿨다운 후 다시 사용 (3) 게이지가 차면 발동 (4) 적 처치하면 게이지 증가"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (없음)** | "안 넣음" | — | — | — |
| **B (쿨다운)** | "쿨다운" | `rail-bullettime`, `rail-bt-gauge-cooldown`, `rail-hud-bt`, `post-process-kit` (`pp-bullettime`) | HR-RS-3 | `BT_Cooldown_15s.asset` |
| **C (게이지 자연충전)** | "게이지" | `rail-bullettime`, `rail-bt-gauge-time`, `rail-hud-bt`, `post-process-kit` (`pp-bullettime`) | HR-RS-3 | `BT_Gauge_Time.asset` |
| **D (킬 충전)** | "적 처치" | `rail-bullettime`, `rail-bt-gauge-kill`, `rail-hud-bt`, `post-process-kit` (`pp-bullettime`) | HR-RS-3 | `BT_Gauge_Kill.asset` |
| **Default** | D (킬 충전) | — | — | — |

**후속 메모**:
- 불릿타임 발동 시 자동으로 `pp-bullettime` PP 프로파일 가중치 1.0, 종료 시 0.0 (트윈).
- 적 슬로우는 `Time.timeScale` 금지 — `Enemy.LocalTimeScale` 로 유닛 단위 조작 (공통 HR-5 정신 계승).
- **HR-RS-3** 가 이걸 강제함.

---

## Q4. 무기 시스템 (Weapon)

**수강생 질문 원문**
> "무기는 어떻게? (1) 권총 하나로 끝 (2) 여러 개 들고 바꿔가며 (3) 쓸수록 강해지는 업그레이드"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (단일)** | "권총 하나" | `rail-weapon-single` | — | `Weapon_Pistol.asset` |
| **B (다중 교체)** | "여러 개" | `rail-weapon-multi`, `rail-weapon-swap-input` | — | `Weapon_Pistol.asset`, `Weapon_Shotgun.asset`, `Weapon_SMG.asset` |
| **C (업그레이드)** | "강해지는" | `rail-weapon-upgrade`, `rail-weapon-slot-default` | — | `Weapon_Pistol.asset` + `UpgradeTree_Default.asset` |
| **Default** | A (단일 권총) | — | — | — |

**후속 메모**:
- B 선택 시 무기 슬롯 최대 3개로 제한 (UI 복잡도 방지).
- 발사 모델(hitscan vs projectile) 은 무기별 설정이지만, 프로토는 **hitscan 고정** — 수강생 선택 불필요.

---

## Q5. 재장전 (Reload)

**수강생 질문 원문**
> "재장전은? (1) 없음 - 무한 탄창 (2) 탄 다 쓰면 버튼 눌러서 장전 (3) 자동으로 재장전"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (없음)** | "무한" | — | — | — |
| **B (수동)** | "버튼 눌러" | `rail-reload-manual`, `rail-reload-input`, `rail-hud-ammo` | — | `Ammo_Default.asset` |
| **C (자동)** | "자동" | `rail-reload-auto`, `rail-hud-ammo` | — | `Ammo_Default.asset` |
| **Default** | C (자동) | — | — | — |

**후속 메모**: A 는 액션 긴장감 낮음. B 는 버튼 하나 추가 부담. C 는 긴장감과 접근성 균형 — 기본값.

---

## Q6. 승리/패배 조건 (Win/Lose)

**수강생 질문 원문**
> "언제 이기고 져요? (1) 한 레벨 끝까지 가면 승리 (2) 보스 처치하면 승리 (3) 제한 시간 안에 점수 N점 이상"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (레벨 완주)** | "끝까지" | `rail-rule-section`, `rail-hud-progress` | — | `Rule_Section_Default.asset` |
| **B (보스 처치)** | "보스 처치" | `rail-rule-boss`, `rail-boss-core`, `rail-hud-bosshp` | — | `Boss_Default.asset`, `Rule_Boss.asset` |
| **C (점수)** | "점수 N점" | `rail-rule-score`, `rail-score-system`, `rail-hud-score` | — | `Rule_Score_5000.asset` |
| **A+B** | "끝까지 가서 보스" | A + B | — | 두 에셋 |
| **Default** | A (레벨 완주) | — | — | — |

**후속 메모**:
- Q6 A 선택 시 Q8 (섹션 수) 와 연동 — 섹션 전부 통과 = 완주.
- Q6 B 만 단독 선택 시 섹션 수는 자동 1 (보스방 바로 시작).
- 패배 조건은 항상 "플레이어 HP 0" (고정, 질문 안 함).

---

## Q7. 적 타입 & 아트 (Enemy + Art)

**수강생 질문 원문**
> "적은 누구고 분위기는? (1) 좀비 - 어두운 호러 (2) 로봇/기계 - 사이버펑크 (3) 군인 - 밀리터리 스릴러 (4) 외계 크리처 - 기괴한 스타일리시"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (좀비/호러)** | "좀비" | `rail-enemy-pack-zombie`, `post-process-kit` (`pp-horror`) | — | `EnemyPack_Zombie.asset`, `PP_Horror.asset` |
| **B (기계/사이버)** | "로봇" | `rail-enemy-pack-robot`, `post-process-kit` (`pp-cyber`) | — | `EnemyPack_Robot.asset`, `PP_Cyber.asset` |
| **C (군인/밀리터리)** | "군인" | `rail-enemy-pack-soldier` | — | `EnemyPack_Soldier.asset` |
| **D (크리처)** | "외계" | `rail-enemy-pack-creature`, `post-process-kit` (`pp-weird`) | — | `EnemyPack_Creature.asset`, `PP_Weird.asset` |
| **Default** | A (좀비/호러) | — | — | — |

**후속 메모**:
- `pp-horror` / `pp-cyber` / `pp-weird` 는 `_genres-index.md` 의 `post-process-kit` 확장으로 추가 정의 필요 (현재 미정의 — 카탈로그에 TBD 로 기재).
- 각 팩은 기본 적 3종 (약/중/강) 세트. 디졸브 적용 (등장/사망).

---

## Q8. 맵 섹션 수 (Map Sections)

**수강생 질문 원문**
> "맵은 몇 구역? (1) 한 구역만 짧게 (2) 2~3구역 - 짧은 레벨 (3) 5구역 이상 - 본격적"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (1 섹션)** | "한 구역" | `rail-section-single` | — | `Section_Default.asset` |
| **B (2~3 섹션)** | "짧은 레벨" | `rail-section-chain`, `rail-section-transition` | — | `Sections_Default_3.asset`, `timeline-cue` (섹션 전환) |
| **C (5+)** | "본격적" | `rail-section-chain`, `rail-section-transition`, `rail-section-checkpoint` | — | `Sections_Default_5.asset`, `timeline-cue` |
| **Default** | B (3 섹션) | — | — | — |

**후속 메모**:
- C 선택 시 **체크포인트 저장** 필요 → `save-kit` 활성.
- Q6 B (보스만) 선택 시 Q8 답 무시하고 1 섹션으로 고정.
- 섹션 간 전환은 `timeline-cue` + `shader-fx-kit` (`dissolve-edge-glow`) 사용.

---

## 답변 수집 종료 후 Claude 가 할 일

1. **요약 출력** (수강생): 8줄 정리.
2. **확인 1회**: "이대로 만들까요?"
3. **`_catalog-rail-shooter.md` Phase 착수 순서로 설치**:
   - Phase A: 시점/카메라/에임 (Q1, Q2 기반)
   - Phase B: 무기/발사/피격 (Q4, Q5)
   - Phase C: 적/스폰/섹션 (Q7, Q8)
   - Phase D: 규칙/UI (Q6, Q3)
   - Phase E: 연출/PP (디졸브, PP 프로파일)
4. **Phase 종료마다 공통 검증** + 장르별 검증 (`_conventions-rail-shooter.md` 참조).
5. **에스컬레이션**:
   - Q1 A + Q2 A 조합 감지 시 "너무 어려워질 수 있어요" 사전 경고.
   - Q6 B 와 Q8 충돌 시 Default 통지 후 자동 조정.

---

## 질문 안 하는 것 (고정값)

- 시점: 항상 1인칭.
- 플레이어 이동: 카메라 레일 / 슬롯 점프만 (Q1 에서 분기). 자유 walking FPS 불가.
- 피격 판정: hitscan 기본. 투사체는 후속 과제.
- 플레이어 죽음 = 레벨 실패 (체크포인트 리로드 or 레벨 재시작).
- UI: uGUI (공통 HR-11).
- 결정론: rail-shooter 는 결정론 필수 아님 (TD 와 달리). 다만 **seeded RNG 는 권장** — 적 스폰 패턴 재현성 때문.

---

## 미결 사항 (강사 검토 대기)

- **`pp-horror` / `pp-cyber` / `pp-weird`** PP 프로파일 스펙 미정의.
  `_genres-index.md` 의 `post-process-kit` 섹션에 레시피 ID 추가 필요.
- **보스전 시스템** (Q6 B) 상세 설계 미정 — 페이즈 전환, 약점 부위, HP 바 갱신 로직 등.
- **Q7 적 팩** 실제 3D 모델/2D 스프라이트 자산 출처 미정 — 프로토 범위에서 Unity Asset Store 프리셋 사용 가정.
- **입력 바인딩** (사격/엄폐/불릿타임/재장전/무기교체) 기본 키 매핑 표 필요.
