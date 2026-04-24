# Proto Turn3D — Questionnaire & Branch Matrix

> 수강생이 첫 메시지에 "블루아카이브류 / 유사턴제 / 쿼터뷰 RPG / JRPG" 컨셉을
> 말했을 때, Claude 가 이 문서를 참조해 수강생에게 질문하고, 답변에 따라 활성화할
> 모듈 번들을 결정한다. 모든 분기는 사전 정의되어 있으며 런타임 즉흥 판단 금지.

## 장르 핵심 전제 (고정, 질문 불필요)

수강생에게 묻지 않고 고정되는 값 — `_conventions.md` 공통 HR 에서 강제:

- **뷰**: 3D 쿼터뷰 베이스. 단일 씬(HR-2). PCG 여부는 Q4 에서 분기.
- **턴 모델**: 유사턴제 (타임슬로우 선택 + 타임오버 패스). 순수 실시간/순수 턴제 불가.
- **타임 스케일**: `Unit.LocalTimeScale` 만 사용. `Time.timeScale` 직접 조작 금지 (HR-5).
- **액션 입력 경유**: 모든 스킬·공격은 `CombatScheme.ResolveAction()` 만 통과 (HR-6).
- **캐릭터 렌더 추상화**: `CharacterRenderKit` 통일. 렌더 타입 분기 금지 (HR-7).
- **이동 모델**: 논-Rigidbody (HR-1). Transform + 수동 push-out.
- **스테이지 전환**: 런타임 재구성. `SceneManager.LoadScene` 금지 (HR-2).

## 질문 제시 규칙

- 한 번에 하나씩. 답 듣고 다음 질문.
- 평문 한국어 선택지, 기술 용어 금지.
- "몰라" / "아무거나" → Default.
- 선택지 밖 답 → Default + 1문장 메모.
- Q1~Q8 완료 전 코드 작성 금지.

---

## Q1. 유닛 규모 (Squad Size)

**수강생 질문 원문**
> "전투할 때 우리 편/적이 몇 명이면 좋을까요? (1) 1대1 솔로 (2) 3~5명씩 소규모 팀전 (3) 7명 이상 대규모 난전"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (1:1 솔로)** | "1대1" | `turn-system` [mode=solo] | HR-5 | `TurnConfig_Solo.asset` |
| **B (소규모 N:N)** | "3~5명 팀전" | `turn-system` [mode=squad], `enemy-behavior` [targeting=squad] | HR-5 | `TurnConfig_Squad_3v3.asset` |
| **C (대규모)** | "7명 이상" | `turn-system` [mode=horde], `enemy-behavior` [targeting=horde], `hud-kit` [+compact-unit-bar] | HR-5 | `TurnConfig_Horde_7v7.asset` |
| **Default** | B (3v3 팀전) | — | — | — |

**후속 메모**:
- C 선택 시 궁극기 컷신 빈도 자동 감소 (Q6 궁극기 선택해도 중첩 방지).
- A 선택 시 `path-picker` 단순화 (다중 유닛 경로 선택 UI 불필요).

---

## Q2. 액션 입력 방식 (Combat Scheme)

**수강생 질문 원문**
> "전투 중에 뭘 해야 해요? (1) 자동 전투 - 구경만 (2) 카드 뽑아서 스킬 고르기 (3) 타이밍 맞춰 누르기 리듬 (4) 순간 반응 QTE"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (Auto)** | "자동" | `combat-scheme` [variant=auto] | HR-6 | `CombatScheme_Auto.asset` |
| **B (Card)** | "카드 뽑기" | `combat-scheme` [variant=card], `hud-kit` [+card-hand] | HR-6 | `CombatScheme_Card.asset` + `CardDeck_Default.asset` |
| **C (Rhythm)** | "타이밍" | `combat-scheme` [variant=rhythm], `hud-kit` [+rhythm-bar], `sound-kit` [+beat-sync] | HR-6 | `CombatScheme_Rhythm.asset` |
| **D (QTE)** | "순간 반응" | `combat-scheme` [variant=qte], `hud-kit` [+qte-prompt] | HR-6 | `CombatScheme_QTE.asset` |
| **Default** | B (Card) | — | — | — |

**후속 메모**:
- `combat-scheme` 모듈은 4가지 변형이 플러그인 구조 — 런타임 교체 가능 (프로토엔 고정).
- A 는 학습용. 재미 최저, 안정성 최고.
- C 는 `sound-kit` BGM 과 비트 동기화 필요 — 별도 비트 메타데이터 에셋 요구.

---

## Q3. 카메라 모드 (Camera)

**수강생 질문 원문**
> "카메라는? (1) 비스듬히 내려다보기만 (쿼터뷰 고정) (2) 비스듬히 + 등뒤 (2모드 전환) (3) 비스듬히 + 등뒤 + 옆에서 (3모드)"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (쿼터 고정)** | "쿼터뷰만" | `camera-rig` [modes=quarter] | HR-4 | `CameraProfile_Quarter.asset` |
| **B (2모드)** | "쿼터 + 백뷰" | `camera-rig` [modes=quarter+back], `timeline-cue` [+camera-mode-trigger] | HR-4 | 위 + `CameraProfile_Back.asset` |
| **C (3모드)** | "3모드 다" | `camera-rig` [modes=quarter+back+side], `timeline-cue` [+camera-mode-trigger] | HR-4 | 위 + `CameraProfile_Side.asset` |
| **Default** | B (2모드) | — | — | — |

**후속 메모**:
- 모드 전환은 **이벤트 기반** (HR-4). 연출/스크립트 트리거로만 변경.
- C 는 궁극기(Q6) 컷신에서 사이드뷰 자동 활용 — 컷신 퀄리티↑, 설정 복잡도↑.
- A 는 학습용으로 적합. 연출 감동은 낮음.

---

## Q4. 스테이지 구조 (Stage Layout)

**수강생 질문 원문**
> "맵은? (1) 정해진 스테이지 몇 개 (2) 매번 다르게 생성되는 로그라이크식"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (정적)** | "정해진 스테이지" | `stage-graph` [mode=authored], `path-picker` | HR-3 **비활성** | `Stage_Authored_01.asset` ~ `_03.asset` |
| **B (PCG)** | "로그라이크" | `stage-graph` [mode=pcg], `pcg-city-layout`, `path-picker`, `save-kit` [+run-seed] | HR-3 **활성** | `PCGConfig_Default.asset` |
| **Default** | B (PCG) | — | — | — |

**후속 메모**:
- A 선택 시 HR-3 (PCG 룰) 가 비활성 — 즉 "런 시작 시 시드 생성" 규칙 완화. 정적 스테이지 3개 기본 제공.
- B 선택 시 노드당 최대 3방향, 진입 방향 backtrack 금지 (HR-3) 강제.
- A + Q7-C (로그라이크 런) 조합 불가 — 자동 경고 후 A 를 B 로 업그레이드.

---

## Q5. 캐릭터 렌더 (Character Render)

**수강생 질문 원문**
> "캐릭터 그림은? (1) 2D 일러스트 빌보드(항상 정면) (2) 3D 모델 (3) 우리는 2D, 적은 3D 섞어서"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (빌보드 2D)** | "2D 일러스트" | `character-render-kit` [renderers=billboard] | HR-7 | `Unit_Ally_Billboard.prefab`, `Unit_Enemy_Billboard.prefab` |
| **B (스킨드 3D)** | "3D 모델" | `character-render-kit` [renderers=skinned] | HR-7 | `Unit_Ally_Skinned.prefab`, `Unit_Enemy_Skinned.prefab` |
| **C (혼합)** | "섞어서" | `character-render-kit` [renderers=billboard+skinned] | HR-7 | 위 두 세트 모두 |
| **Default** | C (혼합) | — | — | — |

**후속 메모**:
- 렌더 타입 분기 금지 (HR-7) — 연출 코드는 `CharacterRenderKit` 추상만 사용.
- A 는 블아 정통 스타일. 적/동료 통일감 높음.
- C 는 연출 다양성 최고, 에셋 부담 최대.

---

## Q6. 전투 컨텐츠 복잡도 (Combat Content)

**수강생 질문 원문**
> "전투에서 뭘 써요? (1) 기본 공격만 (2) 기본 공격 + 스킬 (3) 기본 + 스킬 + 궁극기(필살기 컷신)"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (기본 공격)** | "기본만" | `ability-catalog` [tiers=basic], `damage-system`, `effect-kit` [basic-hits] | — | `Ability_BasicAttack.asset` |
| **B (+스킬)** | "스킬" | `ability-catalog` [tiers=basic+skill], `damage-system`, `effect-kit` [+skill-fx] | — | 위 + `Ability_Skill_*.asset` (3~5개) |
| **C (+궁극기)** | "필살기 컷신" | `ability-catalog` [tiers=basic+skill+ult], `damage-system`, `effect-kit` [+ult-fx], `timeline-cue` [+ult-sequence], `post-process-kit` [+pp-ult-active] | — | 위 + `Ability_Ult_*.asset`, `Timeline_Ult_*.playable` |
| **Default** | C (궁극기까지) | — | — | — |

**후속 메모**:
- C 의 궁극기 컷신은 Q1 A/B 에서 권장. Q1-C (대규모 난전) 에서는 컷신 빈도 자동 감소 (1분 쿨다운 이상).
- C 선택 시 Q3 에서 사이드뷰 (B+ 또는 C) 권장 — 컷신용.
- `post-process-kit` 의 `pp-ult-active` 레시피 자동 포함.

---

## Q7. 게임 분량 (Content Volume)

**수강생 질문 원문**
> "한 판 얼마나? (1) 전투 1번만 (학습용) (2) 스테이지 3~5개 짧은 단편 (3) 로그라이크 런(맵/적 매번 달라지는 반복 플레이)"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (1 전투)** | "1번만" | (save-kit 제외) | — | `Stage_Authored_01.asset` 1개만 |
| **B (3~5 스테이지 단편)** | "3~5개" | `save-kit` [mode=progress] | — | `Stage_Authored_01~05.asset` or PCG N=5 |
| **C (로그라이크 런)** | "반복 플레이" | `save-kit` [mode=run+meta], `pcg-city-layout` (Q4-B 강제) | HR-3 (Q4-B 로 강제) | `RunConfig_Default.asset`, `MetaProgression_Default.asset` |
| **Default** | B (3~5 단편) | — | — | — |

**후속 메모**:
- C 선택 시 Q4 답이 A (정적) 였다면 **B (PCG) 로 자동 업그레이드** + 수강생에게 1문장 안내.
- C 는 `save-kit` 에 "런 상태 (시드/진행/영구 메타)" 2계층 저장 요구.

---

## Q8. 아트 스타일 (Art Style)

**수강생 질문 원문**
> "전체 분위기는? (1) 밝은 파스텔 애니풍 (2) 어두운 판타지 (3) 픽셀/로우폴리 레트로"

| 선택지 | 수강생 표현 | Active Modules | Active HR | Default Assets |
|---|---|---|---|---|
| **A (파스텔 애니)** | "밝은 애니" | `character-render-kit` + `sprite-pack-pastel`, `post-process-kit` [+pp-pastel] | — | `SpritePack_Pastel.asset`, `PP_Pastel.asset` |
| **B (다크 판타지)** | "어두운 판타지" | `character-render-kit` + `model-pack-darkfantasy`, `post-process-kit` [+pp-dark] | — | `ModelPack_DarkFantasy.asset`, `PP_Dark.asset` |
| **C (픽셀/로우폴리)** | "레트로" | `character-render-kit` + `retro-pack`, `post-process-kit` [+pp-retro] | — | `RetroPack.asset`, `PP_Retro.asset` |
| **Default** | A (파스텔 애니) | — | — | — |

**후속 메모**:
- `pp-pastel` / `pp-dark` / `pp-retro` 는 `_genres-index.md` `post-process-kit` 에 **미등록** — TBD.
- Q5-A (빌보드 2D) + Q8-A (파스텔) 조합이 블아 정통.
- Q5-B (3D) + Q8-C (레트로) = 로우폴리 룩.

---

## 답변 수집 종료 후 Claude 가 할 일

1. **요약 출력** (수강생): 8줄 정리.
2. **확인 1회**: "이대로 만들까요?"
3. **`_catalog-turn3d.md` Phase 착수 순서로 설치**:
   - Phase A: 빈 시가지 돌아다니기 (CameraRig → MovementCore → StageGraph → PCGCityLayout → PathPicker)
   - Phase B: 전투 1회전 & 표현 (TurnSystem → CombatScheme → CharacterRenderKit → TimelineCue)
   - Phase C: 전투 내용물 (AbilityCatalog → DamageSystem → EffectKit → EnemyBehavior)
   - Phase D: 상위 게임 흐름 (SoundKit → HUDKit → SaveKit)
4. **Phase 종결 조건 검증**: `_catalog-turn3d.md` 의 각 Phase 종결 조건 체크.
5. **에스컬레이션**:
   - Q4-A + Q7-C 충돌 시 자동 조정 + 안내.
   - Q1-C + Q6-C 조합 시 궁극기 쿨다운 자동 증가 안내.
   - Q2-C (Rhythm) 선택 시 BGM 비트 데이터 없으면 사용자 확인.

---

## 질문 안 하는 것 (고정값)

- 뷰: 항상 3D 쿼터뷰 베이스 (카메라 모드만 Q3 에서 분기).
- 턴 모델: 항상 유사턴제 (타임슬로우 + 타임오버 패스). 순수 실시간 / 순수 턴제 불가.
- 물리: Rigidbody 금지 (HR-1).
- 씬: 단일 씬 (HR-2).
- 이벤트 버스: C# event 또는 `GameEventChannel` (HR-9).
- UI: 런타임 uGUI, 에디터 UI Toolkit (HR-11).
- 네임스페이스/폴더: `_conventions.md` 규정 준수.

수강생이 이 중 하나를 바꾸려 하면 → 거부 + 이유 1문장.

---

## 미결 사항 (강사 검토 대기)

- **`pp-pastel` / `pp-dark` / `pp-retro`** PP 프로파일 스펙 미정 — `_genres-index.md` `post-process-kit` 섹션에 레시피 ID 추가 필요.
- **`sprite-pack-pastel` / `model-pack-darkfantasy` / `retro-pack`** 실제 에셋 출처 미정. 프로토 범위에서 Unity Asset Store 프리셋 가정.
- **Q2-C (Rhythm)** BGM 비트 메타데이터 스펙 미정 — `sound-kit` 정의 파일 작성 시 함께.
- **Q7-C (로그라이크)** 메타 프로그레션 영구 저장 포맷 — `save-kit` 정의 파일 작성 시 함께.
- **Q6 스킬/궁극기 기본 세트 밸런싱**: 기본 제공 에셋 3~5개의 수치 TBD.
