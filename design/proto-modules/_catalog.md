# Proto Modules — Catalog

> 블루아카이브류 쿼터뷰 3D 유사턴제 프로토를 **조립식**으로 빌드하기 위한
> 모듈 카탈로그. 16개 모듈 + 하드 룰 + 의존성 그래프 + 착수 순서.
> 각 모듈은 최종적으로 `.claude/skills/proto-<name>/SKILL.md` 로 승격됨.

## 핵심 컨셉 요약

- **장르**: 쿼터뷰 3D 유사턴제 전투. 참고: 블루아카이브 인게임.
- **유닛 규모**: 1:1 / 1:N / N:1 / N:N 모두 허용.
- **카메라**: 백뷰 ↔ 쿼터뷰 ↔ 사이드뷰 3모드, 연출 트리거로 전환.
- **턴**: 유사턴. 선택 중 타임슬로우, 타임오버 시 상대 턴으로 패스.
- **씬**: 단일 씬. 스테이지는 런타임 구조물.
- **레이아웃**: PCG 시가지, 로그라이크 재배치, 노드당 최대 3방향, 왔던 길 제외.
- **캐릭터 렌더**: 빌보드 2D + 스킨드 3D 공존. 좌/우 엣지 일러스트 컷인.
- **연출 훅**: Timeline 기반 Intro / Ultimate / Outro.
- **액션 입력 방식**: 기획 모듈로 교체 가능 (Auto / Card / Rhythm / QTE).

## 16 모듈 전체 맵

| # | 모듈 | Tier | 영역 | 한 줄 요약 |
|---|------|------|------|-----------|
| 1 | CameraRig | 코어·공간 | 공간/이동 | 3모드 카메라 전환 + N-유닛 프레이밍 |
| 2 | MovementCore | 코어·공간 | 공간/이동 | 논-rigidbody 이동, Physics 검출 기반 |
| 3 | StageGraph | 코어·공간 | 공간/이동 | 노드-엣지 스테이지, 3-way + 진입로 제외 |
| 4 | PCGCityLayout | 코어·공간 | 공간/이동 | 시드 기반 StageGraph 생성기 |
| 5 | PathPicker | 코어·공간 | 공간/이동 | 카메라 모드별 경로 선택 UI |
| 6 | TurnSystem | 코어·전투 | 전투 흐름 | N:N 턴 큐 + 타임슬로우 + 타임오버 패스 |
| 7 | CombatScheme | 코어·전투 | 전투 흐름 | 액션 입력 플러그인 (Auto/Card/Rhythm/QTE) |
| 8 | CharacterRenderKit | 코어·표현 | 표현 | 빌보드 ↔ 스킨드 폴리모프 + 컷인 슬롯 |
| 9 | TimelineCue | 코어·표현 | 표현 | Intro/Ult/Outro 훅 |
| 10 | AbilityCatalog | 확장·전투 | 전투 내용물 | 스킬/궁극기 정의 + 타게팅 룰 |
| 11 | DamageSystem | 확장·전투 | 전투 내용물 | 데미지 계산 + 버프/디버프 + 내성 |
| 12 | EffectKit | 확장·전투 | 전투 내용물 | VFX/피격효과/이펙트 풀 |
| 13 | EnemyBehavior | 확장·전투 | 전투 내용물 | AI FSM/BT, N:N 타게팅 |
| 14 | SoundKit | 확장·상위 | 상위 흐름 | BGM/SFX/stinger 믹서 + 보이스 |
| 15 | HUDKit | 확장·상위 | 상위 흐름 | HP바/턴 인디케이터/스킬 카드/레터박스 |
| 16 | SaveKit | 확장·상위 | 상위 흐름 | 로그라이크 런 상태 + 영구 진행 |

### 후속 Tier로 연기

- LocalizationKit, InventoryKit, ProgressionKit — 블아류 코어 외. 필요 시점에 추가.

## 의존성 그래프 (설치 순서 결정용)

```
CameraRig ────────────────────────────┐
                                      │
MovementCore ◄──┐                     │
                │                     ▼
StageGraph ◄────┼─── PathPicker ──► (사용자 입력)
                │         ▲
PCGCityLayout ──┘         │
                          │
TurnSystem ◄── CombatScheme
    ▲               ▲
    │               │
    └── AbilityCatalog ◄── DamageSystem ◄── EffectKit
            ▲
            │
    EnemyBehavior

CharacterRenderKit ─► (유닛 스폰 필요한 모든 모듈)

TimelineCue / SoundKit / HUDKit — cross-cutting, 거의 모든 모듈이 이벤트 발행

SaveKit — 최상위, 나머지가 안정된 후
```

## Phase 착수 순서

**Phase A — 빈 시가지 돌아다니기** (Tier 1 / 5 모듈)
1. CameraRig → 2. MovementCore → 3. StageGraph → 4. PCGCityLayout → 5. PathPicker

☑ 종결 조건: PCG 생성된 시가지 3갈림길에서 경로 탭 → 다음 노드 이동, 카메라 3모드 전환 가능.

**Phase B — 전투 1회전 & 표현** (6~9)
6. TurnSystem → 7. CombatScheme → 8. CharacterRenderKit → 9. TimelineCue

☑ 종결 조건: N:N 전투 개시, 타임슬로우 중 액션 입력(Auto 기본), Intro/Ult/Outro 컷신 재생.

**Phase C — 전투 내용물** (10~13)
10. AbilityCatalog → 11. DamageSystem → 12. EffectKit → 13. EnemyBehavior

☑ 종결 조건: 스킬 캐스팅 → 데미지 계산 → 이펙트 재생 → 적 AI 반응.

**Phase D — 상위 게임 흐름** (14~16)
14. SoundKit → 15. HUDKit → 16. SaveKit

☑ 종결 조건: 로그라이크 런 1회 완결 (시작 → 전투 → 진행 → 사망/승리 → 저장).

## 스킬 매핑 (최종 이동 대상)

모듈 1개 ≈ 스킬 1개. 네이밍: `/proto-<kebab-case>`.

| 모듈 | 스킬 |
|------|------|
| CameraRig | `/proto-camera-rig` |
| MovementCore | `/proto-movement` |
| StageGraph | `/proto-stage-graph` |
| PCGCityLayout | `/proto-pcg-layout` |
| PathPicker | `/proto-path-picker` |
| TurnSystem | `/proto-turn-system` |
| CombatScheme | `/proto-combat-scheme` (인자: `auto\|card\|rhythm\|qte`) |
| CharacterRenderKit | `/proto-character` (인자: `billboard\|skinned`) |
| TimelineCue | `/proto-timeline-cue` (인자: `intro\|ult\|outro`) |
| AbilityCatalog | `/proto-ability` |
| DamageSystem | `/proto-damage` |
| EffectKit | `/proto-effect` |
| EnemyBehavior | `/proto-enemy-ai` |
| SoundKit | `/proto-sound` |
| HUDKit | `/proto-hud` |
| SaveKit | `/proto-save` |

별도 메타 스킬:
- `/proto-skeleton` — Phase A의 CameraRig + SceneRoot 최소 설치 (Phase A 시작 전에 1회)
- `/proto-playtest` — 모듈 어디든 설치 후 호출, Play 진입 + `read_console` + 스크린샷으로 즉시 검증

## 모듈 문서 공통 템플릿

각 `design/proto-modules/<module>.md` 는 아래 7섹션 고정:

```
1. Purpose       — 뭐 하는 모듈, 왜 필요한가
2. Hard Rules    — 위반 금지 제약 (_conventions.md 참조)
3. Public API    — 외부에 노출하는 타입/이벤트
4. Dependencies  — Required / Optional
5. Default Prefabs/Assets — 구워둘 프리팹·SO 이름
6. Skill Hook    — /proto-xxx 호출 시 수행할 액션 목록
7. Verification  — 설치 후 동작 검증법 (콘솔 로그, 스크린샷, MCP 툴 호출)
```

최종 스킬 승격 시 1/2/3/6/7 을 프론트매터 + 본문에 재배치.
