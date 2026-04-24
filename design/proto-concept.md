# Proto Concept — 3장르 통합 데모

> **목표:** Proto 브랜치에서 turn3d / td / rail-shooter 3장르를 각각 3분 플레이 가능한 수직 슬라이스로 구현.
> **생성:** 2026-04-24 (Loop 3 확정, CL-2026-04-24-009)
> **VKL 모드:** 풀 VKL (경량 모드 제거됨)

## 1. 장르별 레퍼런스 (사용자 확정)

| 장르 | 레퍼런스 | 핵심 메커닉 |
|---|---|---|
| `turn3d` | 블아 스타일 소규모 파티 턴제 | 여러 유닛 조작, 턴/라운드, 스킬/필살기, 캐릭터 대사 |
| `td` | 엔드필드 허브-네트워크 + 자유 배치 + 코어 방어 | 발전소↔릴레이↔타워 선 연결, 예산 내 배치, 코어 HP 방어 |
| `rail-shooter` | 하우스오브더데드류 좀비 섹션 + 엄폐 + 불릿타임 | 1인칭 고정 레일, 에임만 조작, 엄폐 슬롯, 불릿타임 게이지 |

## 2. 씬 구조 (사용자 확정 — B=(1))

```
Proto_Launcher.unity      — 3장르 진입 버튼 + SceneFlow(DontDestroyOnLoad)
Proto_Turn3d.unity        — 블아 스타일 파티 턴제
Proto_TD.unity            — 엔드필드 허브 네트워크 TD
Proto_RailShooter.unity   — 하우스오브더데드류 RS
```

## 3. 프리팹 전략 (사용자 확정 — C-1=(2))

- **장르별 완전 분리.** 공통 Unit_Base.prefab은 현재 샘플로만 보존, 3장르 공용 금지.
- 디렉토리:
  - `Assets/Proto/Prefabs/Turn3d/` (Turn3d_Party_*, Turn3d_Enemy_*)
  - `Assets/Proto/Prefabs/TD/` (TD_Builder, TD_Enemy_*, TD_Tower_*, TD_PowerHub, TD_RelayHub, TD_Core)
  - `Assets/Proto/Prefabs/RailShooter/` (RS_Enemy_Zombie, RS_CoverSlot)
- **코드 재사용은 가능:** MovementAgent, CameraRig, AStarGrid, WFC, PCG, SpeechBubble, ShaderFx.
- 근거: OBS-2026-04-24-003

## 4. SpeechBubble 역할 (C-2 — 장르별 개별 결정)

- Phase 0 런처·3 껍데기 씬에서는 **미사용**.
- Phase 1(TD)·Phase 2(Turn3d)·Phase 3(Rail-shooter) 각각 착수 전 사용자 OR-08 확인.
- 근거: HYP-2026-04-24-001 C-2 (tentative 유지)

## 5. 작업 순서 (Claude 자율 결정 — D)

| Phase | 내용 | 근거 |
|---|---|---|
| **Phase 0** | 런처 + 3 껍데기 씬 + SceneFlow 싱글톤 + 공통 스모크 루프 | 선결 공통 작업 |
| **Phase 1** | TD 수직 슬라이스 (엔드필드 허브-네트워크) | `_progress.md` 기준 TD 문서 37/37 완성 — spec gap 최소 |
| **Phase 2** | Turn3d 수직 슬라이스 (블아 스타일) | 문서 9/16 + 공유 모듈 성숙 |
| **Phase 3** | Rail-Shooter 수직 슬라이스 (하우스오브더데드류) | 문서 0/47 — 최소 MVP 8~10개 모듈 문서 선행 후 구현 |

## 6. 기존 자산 (3장르 공유 인프라)

| 항목 | 상태 |
|---|---|
| CameraRig (3모드) | ✅ 완성 |
| MovementAgent (HR-1 준수) | ✅ 완성 |
| WFC Terrain Generator | ✅ 완성 (TdWfcLevelSpawner) |
| PCG Cityscape Generator | ✅ 완성 |
| AStarGrid (결정론 A*) | ✅ 완성 |
| SpeechBubble | ✅ 완성 |
| ShaderFx Hub (디졸브) | ✅ 완성 |
| Bush Billboard | ✅ 완성 |
| ProtoDebugInput (WASD+1/2/3) | ✅ 완성 |
| ProtoTestBus/Runner | ✅ 완성 |

## 7. 미구현 (3장르 공통 + 장르별)

### 공통 (Phase 0에서 해결)
- 런처 UI + 씬 전환 매니저
- 4씬 EditorBuildSettings 등록

### 장르별 (Phase 1~3)
- **TD**: 허브-네트워크, 선 예산, 자유 배치, 적 경로 이동, 타워/투사체, 웨이브, 코어 HP, HUD
- **Turn3d**: 턴 시스템, 파티 유닛 관리, path-picker, combat-scheme, 스킬/Ult, 턴 UI
- **Rail-Shooter**: Fixed-Rail 카메라 모드, 에임 컨트롤러, 엄폐 슬롯, 불릿타임, 좀비 적, 섹션 스폰

## 8. VKL 검증 원칙

- 풀 모드 — 매 판단 응답에 9섹션 OUTPUT_CONTRACT 적용
- Oracle ID 필수, FT ID 필수, 신뢰도 명시
- spec gap 발견 시 FT-01 분류, 추측 금지 → HYP 작성 후 OR-08 에스컬레이션
- `.vkl/runtime/` append-only, `core/`·`project/` 읽기 전용

## 9. 레퍼런스

- 설계 카탈로그: `design/proto-modules/_genres-index.md`, `_catalog-td.md`, `_catalog-turn3d.md`, `_catalog-rail-shooter.md`
- 진척 스냅샷: `design/proto-modules/_progress.md` (2026-04-23)
- VKL 케이스 로그: `.vkl/runtime/case_logs/CL-2026-04-24-007~009.md`
- HYP/OBS: `HYP-2026-04-24-001.md`, `OBS-2026-04-24-003.md`
