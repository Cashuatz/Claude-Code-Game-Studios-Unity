# Proto 브랜치 세션 핸드오프 — 2026-04-24

> **목적:** 컴퓨터 이전 후 새 환경에서 Claude를 띄우면 이 문서 하나로 현재 상태 복구 + 즉시 Phase 1 착수 가능.
> **작성:** 2026-04-24 Loop 5 종료 시점
> **브랜치:** `Proto` (HEAD = `d58b681`)
> **VKL 모드:** **풀 VKL** (경량 모드 제거됨, 매 판단 응답에 9섹션 OUTPUT_CONTRACT 적용)

---

## 0. 빠른 재진입 체크리스트 (새 컴퓨터에서 가장 먼저)

1. 이 파일 읽기: `docs/HANDOFF-2026-04-24-session.md`
2. 프로젝트 CLAUDE.md 재로드: `CLAUDE.md` + `docs/PROTO-QUICKSTART.ko.md`
3. VKL core/project 로드 (풀 모드):
   - `.vkl/core/ROLE_AND_RULES.md`
   - `.vkl/core/OUTPUT_CONTRACT.md`
   - `.vkl/core/ESCALATION_POLICY.md`
   - `.vkl/core/FAILURE_TAXONOMY.base.md`
   - `.vkl/core/ORACLE_CATALOG.base.md`
   - `.vkl/project/*.md`
4. 세션 VKL 루프 복구 (시간순):
   - `CL-2026-04-24-007.md` — 공백 식별
   - `CL-2026-04-24-008.md` — B/D 수신
   - `CL-2026-04-24-009.md` — C-1 확정, HYP→OBS 승격
   - `CL-2026-04-24-012.md` — Phase 0 구현
   - `CL-2026-04-24-013.md` — Play Mode 검증, Input System 버그 해소
   - `OBS-2026-04-24-003.md` — 장르별 완전 분리 규칙
   - `HYP-2026-04-24-001.md` — C-2 tentative
5. 설계 개요: `design/proto-concept.md`
6. 세션 타임라인: 본 문서 Section 4
7. **다음 액션:** Section 8(Phase 1 TD 계획) + Section 9(열린 분기) 확인 후 사용자에게 "C-2 응답 or Phase 1 시작?" 제시

**첫 말:** "Proto 브랜치 핸드오프 복구 완료. Loop 6 — Phase 1 TD 착수 대기. C-2(SpeechBubble 역할) 먼저 확정하시겠습니까 아니면 디폴트(장식 전용)로 바로 TD 시작하시겠습니까?"

---

## 1. 현재 상태 1장 요약

**목표:** Proto 브랜치에서 3장르(turn3d / td / rail-shooter) 각 3분 플레이 가능한 수직 슬라이스.

**Phase 0 완료** (이 세션 산출물):
- 런처 씬 + 3장르 껍데기 씬 + EditorBuildSettings 4씬 등록
- `SceneFlow` DontDestroyOnLoad 싱글톤 (LoadGenre/ReturnToLauncher API)
- `LauncherUI` (3 버튼 + 숫자키 1/2/3 핫키)
- `LauncherSetup` Editor 헬퍼 (`Proto/Phase0_BuildAll` 메뉴)
- EventSystem은 **New Input System**용 `InputSystemUIInputModule` (레거시 StandaloneInputModule 사용 금지 — 프로젝트 Input Settings 때문)
- 정적+런타임 오라클 5개 pass, Play Mode 진입 시 Console 완전 클린

**Phase 1~3 미착수:** 실제 게임플레이(적·타워·턴·레일·전투) 0%.

**사용자 피드백 주의:** Phase 0 껍데기 씬이 런처와 시각적으로 너무 비슷해서 "1 눌렀는데 뭘 한거지" 반응 발생. UX 보강 옵션은 Section 9-3 참조.

---

## 2. 사용자 확정 결정 매트릭스

| 코드 | 질문 | 답변 | 상태 | 기록 |
|---|---|---|---|---|
| A-turn3d | turn3d 레퍼런스 | 블아 스타일 소규모 파티 턴제 | CONFIRMED | CL-008 |
| A-td | td 레퍼런스 | 엔드필드 허브-네트워크 + 자유 배치 + 코어 방어 | CONFIRMED | CL-008 |
| A-rail | rail-shooter 레퍼런스 | 하우스오브더데드류 좀비 섹션 + 엄폐 + 불릿타임 | CONFIRMED | CL-008 |
| B | 씬 구성 방식 | 장르별 독립 3씬 + 런처 씬 | CONFIRMED | CL-008 |
| C-1 | 캐릭터 프리팹 전략 | 장르별 완전 분리 (공통 프리팹 금지) | CONFIRMED | [OBS-003](../.vkl/runtime/observations/OBS-2026-04-24-003.md) |
| C-2 | SpeechBubble 역할 | (미응답 — advisory) | **OPEN** | [HYP-001](../.vkl/runtime/temporary_hypotheses/HYP-2026-04-24-001.md) |
| D | 작업 순서 | Claude 자율 결정 | CONFIRMED (위임) | CL-008 |

**C-2 디폴트 가정 (HYP-001 C-2 tentative):** 장식·튜토리얼 힌트 전용. 게임플레이 신호(데미지·쿨다운·상태)와 분리. 사용자 승인 없이는 장르별 Phase 진입 전 반드시 OR-08 재확인.

**D 결정 (Claude가 자율 선택):** Phase 0 → TD → Turn3d → Rail-shooter 순. 근거: `_progress.md` 문서 완성도(td=37/37 > turn3d=9/16 > rail=0/47) 객관 지표.

---

## 3. 장르별 레퍼런스 상세 (Phase 1~3 스펙)

### Phase 1: TD — 엔드필드류
- 허브-네트워크 (발전소↔릴레이↔타워 선 연결, 전력 전파)
- 자유 배치 (허브 최소 간격 제약)
- 코어 방어 (기지 HP)
- 킬 보상 자원 재생
- 적 고정 경로 이동 (AStarGrid 재활용)
- 참조: `design/proto-modules/_catalog-td.md`, 37개 `td-*.md` 모듈

### Phase 2: Turn3d — 블아 스타일
- 소규모 파티 턴제 (여러 유닛 조작)
- 턴/라운드, 스킬/필살기, 캐릭터 대사
- 참조: `design/proto-modules/_catalog-turn3d.md`, 9개 문서 (7개 미작성)

### Phase 3: Rail-Shooter — 하우스오브더데드류
- Fixed-Rail 카메라 모드 (카메라리그에 모드 추가 필요)
- 에임만 조작, 엄폐 슬롯, 불릿타임 게이지
- 좀비 섹션
- 참조: `design/proto-modules/_catalog-rail-shooter.md` (47 모듈 전부 미작성 — MVP 8~10개 선정 후 문서 선행)

---

## 4. 세션 타임라인 (Loop 1~5)

| Loop | 목적 | 결과 | Case Log |
|---|---|---|---|
| 1 | 3장르 데모 공백 식별 + A 에스컬레이션 | uncertain, A 수신 | [CL-007](../.vkl/runtime/case_logs/CL-2026-04-24-007.md) |
| 2 | B/D 수신, HYP-001 생성 (C 임시 가정) | uncertain, C-2 잔여 | [CL-008](../.vkl/runtime/case_logs/CL-2026-04-24-008.md) |
| 3 | C-1 수신, HYP-001 C-1 기각 → OBS-003 승격 | uncertain, Phase 0 착수 준비 완료 | [CL-009](../.vkl/runtime/case_logs/CL-2026-04-24-009.md) |
| 4 | "GO /ultrawork /ralph" — Phase 0 실 구현 | uncertain (런타임 검증 이월) | [CL-012](../.vkl/runtime/case_logs/CL-2026-04-24-012.md) |
| 5 | Play Mode 검증 + Input System 버그 발견·수정 | **pass** (UX FAIL 별건) | [CL-013](../.vkl/runtime/case_logs/CL-2026-04-24-013.md) |

**UX 피드백 (Loop 5 직후, 미번호 루프):** "1 눌렀는데 뭘 한거지?" → Phase 0 껍데기 씬 UX 보강 필요 (Verification PASS + Validation FAIL).

---

## 5. 산출물 파일 인벤토리 (이 세션)

### 신규 스크립트
| 파일 | 역할 |
|---|---|
| `Assets/Proto/Runtime/Launcher/SceneFlow.cs` | DDOL 싱글톤, GenreId enum, LoadGenre/ReturnToLauncher, RuntimeInitializeOnLoadMethod Bootstrap |
| `Assets/Proto/Runtime/Launcher/LauncherUI.cs` | Canvas용 MonoBehaviour, 3 Button 참조 + 1/2/3 핫키 (Keyboard.current) + StatusLabel |
| `Assets/Proto/Editor/LauncherSetup.cs` | Editor 헬퍼, `Proto/Phase0_*` 메뉴 3종, Canvas+UI 프로그래매틱 구성 |

### 신규 씬
| 파일 | 내용 |
|---|---|
| `Assets/Proto/Scenes/Proto_Launcher.unity` | Main Camera + EventSystem(`InputSystemUIInputModule`) + Canvas(Background/Title/3Button/StatusLabel, `LauncherUI` 부착) |
| `Assets/Proto/Scenes/Proto_Turn3d.unity` | Main Camera(pos 0,5,-10, euler 20,0,0) + Directional Light + Ground + EventSystem + Canvas(Banner+Hint) |
| `Assets/Proto/Scenes/Proto_TD.unity` | 상동 (Banner 색 노랑) |
| `Assets/Proto/Scenes/Proto_RailShooter.unity` | 상동 (Banner 색 파랑) |

### 수정된 파일
| 파일 | 변경 |
|---|---|
| `Assets/Proto/Runtime/Proto.asmdef` | `Unity.ugui` 참조 추가 |
| `Assets/Proto/Editor/Proto.Editor.asmdef` | `Unity.ugui`, `Unity.InputSystem` 참조 추가 |
| `ProjectSettings/EditorBuildSettings.asset` | 4씬 enabled=true 등록 (Proto_Launcher buildIndex=0부터 순서대로) |

### 신규 VKL 기록
| 파일 | 유형 |
|---|---|
| `.vkl/runtime/case_logs/CL-2026-04-24-007.md` | Loop 1 |
| `.vkl/runtime/case_logs/CL-2026-04-24-008.md` | Loop 2 |
| `.vkl/runtime/case_logs/CL-2026-04-24-009.md` | Loop 3 |
| `.vkl/runtime/case_logs/CL-2026-04-24-012.md` | Loop 4 |
| `.vkl/runtime/case_logs/CL-2026-04-24-013.md` | Loop 5 |
| `.vkl/runtime/observations/OBS-2026-04-24-003.md` | C-1 장르별 완전 분리 확정 |
| `.vkl/runtime/temporary_hypotheses/HYP-2026-04-24-001.md` | C 임시 가정 (C-1 rejected, C-2 tentative) |

### 신규 설계 문서
- `design/proto-concept.md` — 3장르 통합 데모 개요
- 본 핸드오프 문서

---

## 6. 커밋 히스토리 (이 세션)

```
d58b681 vkl: CL-012 Phase 0 구현 기록 + CL-013 런타임 검증 + Input System 버그 해소
336637b 수정: Phase 0 — EventSystem을 New Input System용 InputSystemUIInputModule로 교체
afae60d 추가: Phase 0 — 런처 씬 + 3장르 껍데기 씬 + SceneFlow 싱글톤
```

**주의:** `git push`는 Proto 브랜치 규칙에 따라 실행되지 않았음. 원격 동기화는 사용자가 직접 수행 필요.

---

## 7. 환경 이슈 & 워크어라운드 (FT-11 계열)

| 이슈 | 증상 | 워크어라운드 |
|---|---|---|
| **MCP `execute_code` Windows CMD 길이 제한** | `Error running mono.exe: 파일 이름이나 확장명이 너무 깁니다` | 인라인 C#은 500바이트 이하. 장문 로직은 `Assets/Proto/Editor/*.cs` 파일로 분리 + `[MenuItem]` + `execute_menu_item`으로 호출 |
| **MCP `execute_menu_item` 경로 파싱 제약** | 공백·괄호·선행 숫자 조합 메뉴명은 실패 | 패턴 `[A-Za-z0-9_]/[A-Za-z0-9_]` 준수. 예: `Proto/Phase0_BuildAll` ✅, `Proto/Phase 0/3. Build All (Launcher + Shells)` ❌ |
| **MCP `refresh_unity(wait_for_ready=true)` 불완전 대기** | 도메인 리로드가 끝나기 전 복귀 | 외부 `sleep 5~10s` 또는 에러 로그 폴링 후 재시도 |
| **`find_gameobjects`가 DDOL 씬 미검색** | SceneFlow Bootstrap 오브젝트를 0개로 반환 | 부트스트랩 성공은 "Console 에러 0"으로 간접 검증 |
| **New Input System + 레거시 EventSystem 충돌** | `InvalidOperationException: switched active Input handling to Input System` | EventSystem에 `InputSystemUIInputModule`(UnityEngine.InputSystem.UI) 사용, `StandaloneInputModule` 금지 |
| **Play Mode Stop 시 missing script × 3** | Console 에러, DDOL 씬의 MCP-FOR-UNITY 또는 기존 잔재 | **Phase 0 코드와 무관** (advisory). Phase 1 착수 블로킹 아님 |

---

## 8. Phase 1 TD 착수 계획 (Loop 6 다음 단계)

### 모듈 구현 순서 (엔드필드류 수직 슬라이스)

1. **`TD_Core` 프리팹 + `TdCoreHp.cs`** — 중앙 기지, HP 컴포넌트, 게임오버 이벤트
2. **`TD_Enemy_Walker` 프리팹 + `TdEnemy.cs` + `TdPathingFixed.cs`** — 고정 경로 이동, 기존 `AStarGrid`(`Assets/Proto/Runtime/TD/Level/AStarGrid.cs`) 재활용
3. **`TD_Tower_Basic` 프리팹 + `TdTowerCore.cs` + `TdBulletSystem.cs`** — 사거리·쿨다운·타겟팅, hitscan 또는 간단 투사체
4. **`TdWaveSpawner.cs`** — 웨이브 타임라인, 단일 lane → 2 lane 단계적 확장
5. **`TD_PowerHub` + `TD_RelayHub` 프리팹 + `TdHubNetwork.cs`** — 발전소↔릴레이↔타워 그래프, 전력 전파 단조성
6. **`TdPlacementFree.cs`** — 허브 최소 간격 제약 + 선 거리 계산
7. **`TdResourceLedger.cs` (kill 모드)** — 적 처치 시 예산 증가
8. **HUD 3종** — `td-hud-corehp`, `td-hud-budget`, `td-hud-killcount`

### 네임스페이스 / 디렉토리 (OBS-003 규칙)
- 스크립트: `Proto.TD.*` 네임스페이스, `Assets/Proto/Runtime/TD/`
- 프리팹: `Assets/Proto/Prefabs/TD/`
- ScriptableObject: `Assets/Proto/Data/TD/`

### 선결 확인 (Q2/Q5/Q6/Q8 디폴트)
사용자에게 확인 필요 — 기존 질문지 `design/proto-modules/_questionnaire-td.md` 참조:
- Q2 (길찾기): Fixed (A*) vs FlowField vs 동적 — **디폴트 A*** (코드 기존 존재)
- Q5 (자원 재생): fixed vs regen vs kill — 사용자 A 답변 "엔드필드"에 따라 **kill** 적용
- Q6 (적 경로 수): single vs 2~3 lane vs 4+ — **2 lane 디폴트**
- Q8 (아트 팩): 2D 빌보드 vs 3D 로우폴리 — **프리미티브 큐브 먼저, 추후 교체**

### 예상 실패 유형 (Loop 6)
- **FT-P-003** (물리 비결정성) — 적 이동을 FixedUpdate 또는 자체 tick으로 강제해야 함
- **FT-10** (Metamorphic Relation Failure) — 허브 네트워크 토폴로지 변경 시 전력 단조성(MR-P-001) 위반 가능
- **FT-02** (Hidden Semantic Rule Misfill) — Q2/Q5/Q6/Q8 디폴트 채울 때 사용자 OR-08 확인 필수

### MR 적용 예정
- `MR-P-001` (단조성 — 레벨 상승 시 타워 공격력 증가)
- `MR-P-005` (부정 — HP 0 이하 코어에 적 충돌 시 행동 불가)
- `MR-P-006` (스케일링 — 타워 사거리 증가 시 감지 적 수 non-decreasing)

---

## 9. 열린 분기 (사용자 결정 대기)

### 9-1. C-2: SpeechBubble 역할
- **Option A:** 장식·튜토리얼 전용 (디폴트, HYP-001 C-2 tentative)
- **Option B:** 장르별 게임플레이 신호로 통합 (TD 코어 경고, turn3d 스킬 대사, rail-shooter 적 조롱)
- **Option C:** "알아서" — Claude는 Option A 적용

### 9-2. Phase 1 착수 신호
- **"Phase 1 시작"** or **"TD 가즈아"** 또는 **C-2 응답과 함께** → Loop 6 진입

### 9-3. Phase 0 UX 보강 (사용자 "1 눌렀는데 뭘 한거지" 피드백 대응)
- **Option A:** 껍데기 씬 시각 차별화 (대형 중앙 장르 라벨 + 장르 accent 배경·Ground 색 + Esc로 런처 복귀)
- **Option B:** Phase 0는 그대로 두고 Phase 1에서 TD 씬만 실제 게임플레이로 채움
- **Option C:** A 후 B

### 9-4. Rail-Shooter 47개 모듈 문서
- `_catalog-rail-shooter.md` 기준으로 47 모듈 전부는 Proto 범위 초과
- Phase 3 진입 시 **MVP 8~10개**만 선정해서 문서 선행 후 구현

---

## 10. VKL 원칙 리마인더 (풀 모드 필수)

- **매 판단 응답에 9섹션 OUTPUT_CONTRACT 적용**
- 모든 판정에 oracle ID (OR-XX / OR-P-XXX)
- 모든 실패에 failure taxonomy ID (FT-XX / FT-P-XXX)
- spec gap 추측 금지 → HYP 또는 `.vkl/proposals/` 작성
- `.vkl/core/`·`.vkl/project/` 읽기 전용, `.vkl/runtime/` append-only
- Escalation 조건 충족 시 판단 중단 (ESCALATION_POLICY.md 10 조건)
- **금지:** verification PASS를 validation PASS로 취급 (Loop 5 사용자 UX FAIL 사례 재발 방지)

### 본 세션에서 발견한 재사용 가능한 패턴
- `MR-P-[proto]-002` (Round-Trip 씬 왕복 후 SceneFlow 싱글톤 유일성, core MR-03 overlay)
- `MR-P-[proto]-003` (Permutation 씬 방문 순서 무관, core MR-01 overlay)
- 두 MR은 Phase 0 Editor 헬퍼 메뉴 (`Proto/Phase0_RuntimeSmoke` 가칭)로 자동 실행 후 `.vkl/proposals/` 승격 제안 예정

---

## 11. 글로벌 auto-memory 덤프

**위치:** `~/.claude/projects/d--Dev-Claude-Claude-Code-Game-Studios-Unity-WorkSpace-Claude-Code-Game-Studios-Unity/memory/`

### 11-1. `MEMORY.md`
```markdown
# Memory Index

- [.claude/ 수정은 stage+mv 패턴 필수](feedback_claude_dir_staging.md) — `.claude/` 쓰기는 `.stage/` 에 먼저 작성 후 Bash `mv` 로 배포 (권한 프롬프트 최소화)
- [Proto 브랜치에서 git push 금지](feedback_no_git_push.md) — commit 까지만 자동, push 는 사용자가 명시 요청할 때만 (credential 프롬프트 회피)
```

### 11-2. `feedback_claude_dir_staging.md`
```yaml
---
name: .claude/ 수정은 stage+mv 패턴 필수
type: feedback
---
```
요약: `.claude/` 하위 파일은 Edit/Write 직접 호출 금지. `.stage/claude/...`에 쓰고 Bash `mv`로 배포. `.stage/`는 `.gitignore` 등록됨. CLAUDE.md 작업 규칙 5에도 박힘. 프롬프트 최소화가 이유.

### 11-3. `feedback_no_git_push.md`
```yaml
---
name: Proto 브랜치에서 git push 금지
type: feedback
---
```
요약: `git add` + `git commit`까지만 자동. `git push`는 credential 프롬프트 회피 목적으로 금지. 사용자가 "푸시해줘" 명시 요청 시에만 실행.

### 11-4. 신규 메모리 후보 (핸드오프 시 추가 고려)
이 세션에서 추가로 기록할 만한 것:
- **user 메모리:** 사용자는 수강생 데모 흐름을 중시하며 **Verification PASS가 Validation PASS 아님**을 명시적으로 인지시킬 것 (Loop 5 UX 피드백 근거)
- **feedback 메모리:** Unity New Input System 프로젝트에서 EventSystem 구성 시 `StandaloneInputModule` 금지, `InputSystemUIInputModule` 필수
- **reference 메모리:** MCP unityMCP 도구 제약 (execute_code 500B, menu_item 경로 `[A-Za-z0-9_]/`, refresh_unity 외부 sleep 필요) — 세션 시작 시 참조

---

## 12. 새 컴퓨터 환경 체크리스트

| 항목 | 확인 방법 |
|---|---|
| Unity 6000.3.13f1 설치 | `Unity Hub` 또는 `ProjectSettings/ProjectVersion.txt` |
| .NET SDK (`dotnet build` 동작) | `dotnet --version` |
| Git remote 자격 증명 | push 는 수동, 자격 증명 헬퍼 설정 권장 |
| MCP unityMCP 서버 동작 | `mcp__unityMCP__read_console` 호출 → success |
| 프로젝트 경로 | 새 경로로 바뀌면 `~/.claude/projects/<slug>/memory/` 경로도 slug 변경 → MEMORY.md 이동 필요 |
| `.vkl/runtime/` 존재 | append-only 유지, 내용 커밋에 포함됨 |
| EditorBuildSettings 4씬 등록 유지 | Unity Editor 열어 Build Settings 확인 |

**경로 차이 주의:** 이 문서의 절대 경로(`d:\Dev\Claude\...`)는 현 컴퓨터 기준. 새 컴퓨터에서는 상대 경로로 참조하거나 해당 슬러그를 업데이트.

---

## 13. 재개 첫 명령 템플릿

새 환경에서 Claude에게 줄 첫 지시:

```
Proto 브랜치 세션 핸드오프 문서 읽고 복구해:
docs/HANDOFF-2026-04-24-session.md

풀 VKL 모드로 가동하고, 9섹션 OUTPUT_CONTRACT 준수.

다음 질문 하나만 물어봐:
"C-2(SpeechBubble 역할) 답변 주실래요 아니면 디폴트(장식 전용)로 Phase 1 TD 착수할까요?"
```

---

## 14. 미해결 이슈 요약 (복구 후 바로 처리 가능)

| # | 이슈 | 심각도 | 위치 |
|---|---|---|---|
| 1 | C-2 SpeechBubble 역할 미응답 | Advisory | HYP-001 |
| 2 | Phase 0 껍데기 씬 UX 차별화 부족 | Major (UX) | Section 9-3 |
| 3 | Play Mode Stop 시 missing script × 3 | Minor (advisory, Phase 0 무관) | CL-013 |
| 4 | MR-P-[proto]-002/003 instance 자동 실행 | Infrastructure | Loop 6에서 Editor 헬퍼 메뉴로 해결 |
| 5 | Rail-Shooter 47 모듈 중 MVP 선정 | Phase 3 선결 | Section 9-4 |
| 6 | turn3d 미작성 모듈 7개 | Phase 2 선결 | `_progress.md` 6.1 |

---

**End of HANDOFF-2026-04-24-session.md**
