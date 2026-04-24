# Proto TD — Questionnaire & Branch Matrix

> 수강생이 첫 메시지에 "타워 디펜스"(네트워크 허브 연결형) 컨셉을 말했을 때,
> Claude 가 **이 문서를 참조하여 수강생에게 질문하고**, 답변에 따라 활성화할
> 모듈 번들을 결정한다. 모든 분기는 사전 정의되어 있으며 런타임 즉흥 판단 금지.
>
> **사용 흐름**:
> 1. 수강생이 TD 컨셉 언급 → Claude 가 이 문서의 Q1~Q8 을 **순서대로** 제시.
> 2. 수강생 답 → 해당 분기의 `active modules` / `active HR` / `default assets` 를 `_catalog-td.md` 에서 조회.
> 3. 수강생이 답을 미루면 **Default** 값 자동 적용 (표기된 대로).
> 4. 모든 답 수집 후 설치 순서는 `_catalog-td.md` 의 Phase 착수 순서 따름.

## 질문 제시 규칙 (Claude 가 지킬 것)

- **한 번에 하나씩**. 답 듣고 다음 질문.
- 선택지는 수강생 눈높이 평문 한국어로 제시 (기술 용어 금지).
- 수강생이 "몰라" / "아무거나" 답하면 **Default** 선택. 이유 설명 금지.
- 수강생이 선택지 밖 답변(예: "둘 다" / "나중에") 을 주면 **Default** 로 대체 후 1문장 메모: "일단 기본값으로 가고, 나중에 바꾸고 싶으면 말씀해 주세요."
- Q1~Q8 을 모두 묻기 전에는 코드 쓰지 말 것. 수집 완료 후 일괄 설치.

---

## Q1. 배치 방식 (Placement)

**수강생 질문 원문**
> "타워랑 발전소를 놓을 때, 격자에 딱딱 맞춰서 놓을래요, 아니면 원하는 자리에 자유롭게 놓을래요?"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (셀기반)** | "격자에 맞춰서" | `td-placement-grid`, `td-hub-network` (grid-snap), `td-resource-ledger` (manhattan-distance) | HR-TD-4, HR-TD-6 | `Grid_Default.asset` (10x10, cell=1m) |
| **B (비셀기반)** | "자유롭게" | `td-placement-free`, `td-hub-network` (free-snap), `td-resource-ledger` (euclidean-distance) | HR-TD-4, HR-TD-6 | `PlacementConfig_Free.asset` (minSpacing=1.0m) |
| **Default** | B | — | — | — |

**후속 메모**: A 선택 시 웹 레벨 에디터도 grid 모드로 고정.

---

## Q2. 적 길찾기 (Enemy Pathing)

**수강생 질문 원문**
> "적들이 어떻게 움직이면 좋겠어요? (1) 정해진 길만 따라 쭉 와요 (2) 알아서 가장 가까운 길을 찾아와요 (3) 플레이어 건물 보고 돌아가요"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (고정 경로)** | "정해진 길만" | `td-pathing-fixed`, `stage-graph` (TD 모드) | HR-TD-5 | `Path_Default.asset` (웨이포인트 5개) |
| **B (Flow Field)** | "알아서 가까운 길" | `td-pathing-flowfield`, `td-flowfield-builder` | HR-TD-5 (완화) | `FlowField_Default.asset` |
| **C (동적 우회)** | "건물 보고 돌아가요" | `td-pathing-dynamic`, `td-flowfield-builder` (재계산 on/off) | HR-TD-5 **무효화** (명시적 예외) | `FlowField_Dynamic.asset` |
| **Default** | A | — | — | — |

**후속 메모**:
- C 는 결정론 보장이 까다로워 자동 플레이테스트 (HR-TD-8) 가 느려짐 — 수강생에게 1문장 경고: "알아서 돌아가는 버전은 좀 더 무거울 수 있어요."
- B/C 선택 시 `stage-graph` 는 사용 안 함, 대신 타일맵 기반 FlowField.

---

## Q3. 카메라 뷰 (Camera)

**수강생 질문 원문**
> "맵을 어디서 보면 좋을까요? (1) 위에서 내려다보기 (2) 비스듬히 내려다보기(쿼터뷰) (3) 둘 다 자유롭게 바뀌게"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (Top-Down)** | "위에서" | `camera-rig` (+TopDown mode), `td-camera-pan-zoom` | HR-4 | `CameraProfile_TopDown.asset` (angle=90°, height=20m) |
| **B (쿼터뷰)** | "비스듬히" | `camera-rig` (+QuarterBuilder mode), `td-camera-pan-zoom` | HR-4 | `CameraProfile_Quarter.asset` (angle=55°, height=15m) |
| **C (가변)** | "자유롭게 바뀌게" | `camera-rig` (+TopDown, +QuarterBuilder, +Orbit), `td-camera-pan-zoom`, `td-camera-mode-toggle` | HR-4 | 위 2개 + `CameraProfile_Orbit.asset` |
| **Default** | B | — | — | — |

**후속 메모**: 기존 `camera-rig` 에 모드를 **추가**하는 방식 (신규 모듈 아님). 수강생이 "블아류 장르"도 같이 하고 싶다 말하면 기존 3모드 + TD 2모드 = 5모드 공존 가능.

---

## Q4. 승리/패배 조건 (Win/Lose)

**수강생 질문 원문**
> "어떻게 해야 이기고 져요? (1) 몇 분 버티면 승리 (2) 적 N마리 다 처치하면 승리 (3) 우리 기지 체력 0 되면 패배 (조합 가능)"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (시간 버티기)** | "몇 분 버티면" | `td-game-rule-timed`, `td-hud-timer` | — | `GameRule_Timed_3min.asset` |
| **B (전멸)** | "N마리 처치" | `td-game-rule-killcount`, `td-hud-killcount`, `td-wave-spawner` | — | `GameRule_Kill_50.asset` |
| **C (기지 방어)** | "기지 체력 0 패배" | `td-game-rule-coredefense`, `td-core-hp`, `td-hud-corehp` | — | `GameRule_Core_100hp.asset` |
| **A+C** | "시간 버티고 기지 지키기" | A + C | — | 두 에셋 모두 |
| **B+C** | "다 처치하고 기지 지키기" | B + C | — | 두 에셋 모두 |
| **Default** | A+C (3분 버티기 + 기지 100hp) | — | — | — |

**후속 메모**: 기본값은 엔드필드류 표준 (시간 + 기지 체력). 수강생 답이 모호하면 이걸로.

---

## Q5. 자원 재생 방식 (Resource Regen)

**수강생 질문 원문**
> "선을 까는 자원이 어떻게 생겨요? (1) 처음에 딱 정해진 양만 (2) 시간 지나면 조금씩 차 (3) 적을 잡으면 더 얻어요"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (고정 예산)** | "정해진 양만" | `td-resource-ledger` (mode=fixed) | HR-TD-6 | `Economy_Fixed_100.asset` |
| **B (시간 재생)** | "시간 지나면" | `td-resource-ledger` (mode=regen), `td-resource-tick` | HR-TD-6 | `Economy_Regen_2perSec.asset` |
| **C (킬 보상)** | "적 잡으면" | `td-resource-ledger` (mode=kill), `td-kill-reward-hook` | HR-TD-6 | `Economy_Kill_5perHit.asset` |
| **B+C** | "둘 다" | B + C | HR-TD-6 | 두 에셋 병합 |
| **Default** | B (시간 재생, 2/sec) | — | — | — |

**후속 메모**: C 는 "선 해체 후 재연결" 같은 고급 플레이가 가능해지지만, 프로토 밸런싱이 어려움. 수강생이 명확히 원하면 선택, 아니면 B.

---

## Q6. 적 경로 개수 (Number of Enemy Paths)

**수강생 질문 원문**
> "적이 들어오는 길이 몇 개면 좋을까요? (1) 한 방향에서만 (2) 2~3군데서 (3) 사방에서 몰려와"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (단일 경로)** | "한 방향" | `td-wave-spawner` (singleLane) | — | `Waves_SingleLane.asset` |
| **B (2~3 경로)** | "2~3군데" | `td-wave-spawner` (multiLane=2~3) | — | `Waves_TwoLane.asset`, `Waves_ThreeLane.asset` |
| **C (사방)** | "사방에서" | `td-wave-spawner` (multiLane=4+), `td-pathing-*` 필수 재검토 | — | `Waves_FourLane.asset` |
| **Default** | B (2 lane) | — | — | — |

**후속 메모**: C 는 맵이 충분히 커야 함 (Q7 연동). 맵 Small 에 C 선택 시 자동으로 B 로 다운그레이드하고 알림.

---

## Q7. 맵 크기 (Map Size)

**수강생 질문 원문**
> "맵 크기는 어느 정도로? (1) 작게 - 한 화면 (2) 중간 - 좀 움직여야 (3) 크게 - 미니맵 필요"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (Small)** | "한 화면" | `td-level-template-small` | — | `Level_Small_20x20.json` |
| **B (Medium)** | "좀 움직여야" | `td-level-template-medium`, `td-minimap` (선택) | — | `Level_Medium_40x40.json` |
| **C (Large)** | "미니맵 필요" | `td-level-template-large`, `td-minimap` (필수) | — | `Level_Large_80x80.json` |
| **Default** | B (Medium 40x40) | — | — | — |

**후속 메모**:
- 맵 크기는 자동 플레이테스트 (HR-TD-8) 의 시뮬 예산에 직접 영향. Large 선택 시 Monte Carlo 회수 자동 1/4 감소.
- Large + Q6 C (사방) 조합 시 수강생에게 "맵이 커서 오래 걸릴 수 있어요" 1문장 경고.

---

## Q8. 아트 스타일 (Art Style)

**수강생 질문 원문**
> "그림 스타일은 어떻게? (1) 2D 도트/빌보드 (2) 간단한 3D 블록 (3) 캐릭터만 2D, 배경은 3D"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (2D 도트)** | "2D/도트" | `character-render-kit` (billboard only), `td-sprite-pack-default` | HR-7 | `TD_Sprites_Default.prefab` 번들 |
| **B (3D 블록)** | "3D 블록" | `character-render-kit` (skinned only), `td-lowpoly-pack-default` | HR-7 | `TD_Models_Lowpoly.prefab` 번들 |
| **C (혼합)** | "캐릭터 2D, 배경 3D" | `character-render-kit` (both), `td-sprite-pack-default`, `td-lowpoly-env-default` | HR-7 | 양쪽 번들 |
| **Default** | B (3D 블록) | — | — | — |

**후속 메모**: A 선택 시 카메라 Q3 는 A (Top-Down) 로 자동 교체 제안 (빌보드는 top-down 이 자연스러움). 수강생이 "싫어" 하면 원래 선택 유지.

---

## 답변 수집 종료 후 Claude 가 할 일

1. **요약 출력** (수강생에게): "정리: 격자/자유배치 X, 적은 Y, 카메라 Z, ..." 8줄.
2. **수강생 확인**: "이대로 만들까요?" 단 1회만. "네" 계열 답이면 진행, "바꿀래" 이면 해당 질문만 재질의.
3. **`_catalog-td.md` 의 Phase 착수 순서에 따라 설치**:
   - Phase A: 기반 (Camera, Level, Placement, HubNetwork, ResourceLedger)
   - Phase B: 전투 (Tower, Enemy, Wave, Pathing)
   - Phase C: UI·규칙 (HUD, GameRule)
   - Phase D: 검증 (Playtest, SimParity)
4. **각 Phase 종료마다**: 컴파일/씬/프리팹/스크린샷 검증 (공통 규칙 + HR-TD-9 sim parity).
5. **에스컬레이션**:
   - 결정론 깨지면 → 사용자(강사)에게 보고, 수강생 작업 중단.
   - 두 질문 답이 서로 충돌하면 (예: Q7=Small + Q6=사방) → Default 로 대체 전 수강생에게 1문장 선택 재확인.

---

## 질문 안 하는 것 (고정값, 수강생 선택권 없음)

아래는 수강생에게 묻지 않고 고정으로 간다 — `_conventions-td.md` HR 에 명시:

- **결정론**: 항상 ON (HR-TD-1).
- **RNG**: Xorshift64, seed 는 레벨 JSON 에서 고정 (HR-TD-2).
- **Tick rate**: 20ms / 50Hz (HR-TD-3).
- **데이터 원본**: JSON (HR-TD-4).
- **웹 에디터 사용**: 필수 (수강생이 직접 쓰진 않아도, 레벨은 웹에서 생성·익스포트).
- **네임스페이스/폴더 구조**: `_conventions-td.md` 규정 그대로.

수강생이 이 중 무언가를 "바꾸고 싶어" 하면 → **거부**하고 이유 1문장: "이건 프로토 안정성 때문에 고정이에요."

---

## 미결 사항 (강사 검토 대기)

- **Q3 C (가변 카메라)** 선택 시 모드 전환 트리거 방식 — 버튼? 자동? 현재는 **버튼** 기본값 가정.
- **Q5 C (킬 보상)** 선택 시 보상량 밸런싱 기본값 — `5 per kill` 로 고정했으나 레벨별 조정 필요.
- ~~**Q4 B (전멸)** 에서 "다 처치"가 웨이브 종료와 어떻게 맞물리는지~~ → **결정됨 (2026-04-23)**: 게임 완결성 원칙. 승리 판정은 `AllWavesSpawned && AliveCount==0 && Kills>=Target` AND 조건. 조건 1·2 만족 + 킬 미달 시 패배. 사살만 카운트, 누수는 제외. 세부 규격은 [`td-game-rule-killcount.md`](./td-game-rule-killcount.md) 참조.
