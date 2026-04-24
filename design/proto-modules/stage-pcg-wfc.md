# Module: stage-pcg-wfc

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md) / [`_conventions.md`](./_conventions.md)
> 관련 모듈: [`td-level-template-small.md`](./td-level-template-small.md), [`td-level-template-medium.md`](./td-level-template-medium.md), [`td-level-template-large.md`](./td-level-template-large.md)
> 관련 VKL: PROP-2026-04-24-001 (OR-P-006 / HR-PCG-City-01·02, 검토 대기)

## 1. Purpose

Wave Function Collapse (파동붕괴함수) 기반 **결정론적 타일 배치 솔버**.
Q7(맵 크기) 선택에 따라 20×20 / 40×40 / 80×80 TD 레벨을 생성한다.
기존 `CityscapeGenerator` 의 Block Subdivision 방식이 처리하기 어려운
**국소 호환성 규칙** (도로 연속성, 건물-도로 정렬, 발전소·기지·타워 패드
간 공간 문법) 을 타일 소켓 제약으로 강제한다.

**왜 별도 모듈인가**
- `CityscapeGenerator` (서브디비전) = 거시 레이아웃(블록 분할·밀도 분포) 에 강점. 유지.
- WFC = 미시 인접 규칙 (어떤 타일 옆에 어떤 타일이 올 수 있는지) 에 강점.
- 두 알고리즘은 상호 보완이며, 본 모듈은 우선 **TD 레벨 JSON 생성**(HR-TD-10 경로) 단독 기동.
- 이름을 `stage-pcg-wfc` 로 장르 중립 지정 — turn3d / rail-shooter 에서도 타일셋만 교체해 재사용 가능.

**범위 밖**
- 런타임 실시간 재생성 (프로토는 빌드타임 또는 에디터 액션으로만).
- 3D 볼류메트릭 WFC (2.5D 격자 전용).
- 학습 기반 타일 자동 추출 (tileset 은 인간 저작).

## 2. Hard Rules

**상속 — 공통 `_conventions.md`**
- HR-1 (Rigidbody 금지 — 타일 프리팹은 static mesh)
- HR-8 (데이터-코드 분리 — tileset 은 SO, 출력은 JSON)
- HR-10 (생성물은 `Assets/Proto/` 하위 전용)

**상속 — `_conventions-td.md`**
- HR-TD-2 (RNG 는 seeded Xorshift64 `Proto.TD.Sim.Rng` 만 사용. `System.Random` / `UnityEngine.Random` 금지)
- HR-TD-4 (JSON 원본 — WFC 결과는 `level.schema.json` 준수 JSON 으로 직렬화)
- HR-TD-5 (적 경로는 사전 정의 — 경로 타일은 WFC 실행 **전에** 고정)
- HR-TD-10 (출력 경로 `Assets/Proto/Data/TD/Levels/<level-id>.json`)

HR-TD-1 은 본 모듈 **엄격 적용 대상 아님** — 네임스페이스가 `Proto.TD.Sim.*` 이
아닌 `Proto.Stage.PCG.Wfc.*` 이므로 `UnityEngine` 참조 허용. 단, 결정론은 HR-TD-2 로 담보.

**모듈 고유 제약**
- **WFC-1. 제약 선행**: 경로 타일·발전소·기지·맵 경계는 WFC 실행 전에 `FixedCells` 로 박는다. WFC 는 나머지 셀만 결정.
- **WFC-2. 수렴 보장 원칙**: 타일셋은 항상 수렴 가능해야 한다. 수렴 실패(Contradiction) 시 **에스컬레이션** — seed 변경 재시도 금지(결정론 깨짐). 타일셋 자체 수정이 해법.
- **WFC-3. 경계 조건**: 맵 가장자리 셀은 `borderTileId` 만 배치 가능. 기본값은 `grass-flat`.
- **WFC-4. Tie-break 결정론**: 동일 엔트로피 셀이 복수일 때 `(y, x)` 사전순 최소 셀 우선. 관측(collapse) 타일 선택도 weight 기반이지만 RNG 호출 순서가 고정되어야 한다.
- **WFC-5. 건물 타일은 도로 축 정렬**: PROP-2026-04-24-001 HR-PCG-City-01 승격 대기 중. 승격 전에도 본 모듈에서는 선제 준수 — 건물 타일 소켓 정의가 도로와만 접하도록 강제.

## 3. Public API

```csharp
namespace Proto.Stage.PCG.Wfc
{
    // ── 타일 정의 (SO) ────────────────────────────────────────────────
    [CreateAssetMenu(menuName = "Proto/Stage/Wfc/Tile")]
    public class WfcTileSO : ScriptableObject
    {
        public string TileId;                 // "road-straight-ns", "grass-flat", ...
        public TileRole Role;                 // Void / Path / Grass / Building / PowerPad / TowerPad / CoreBase / Border
        public GameObject Prefab;             // 렌더용 (Void 는 null 허용)
        public WfcSocket North, East, South, West;
        public int Weight;                    // 선택 가중치 (≥1)
        public bool AllowRotation;            // 90° 회전 4종 자동 파생 허용 여부
    }

    public enum TileRole { Void, Path, Grass, Building, PowerPad, TowerPad, CoreBase, Border }

    [Serializable]
    public struct WfcSocket
    {
        public string Id;                     // "road", "grass", "bldg-wall", ...
        public bool Symmetric;                // 반대 방향 소켓과 자동 매칭 여부
    }

    // ── 타일셋 번들 (SO) ──────────────────────────────────────────────
    [CreateAssetMenu(menuName = "Proto/Stage/Wfc/TileSet")]
    public class WfcTileSetSO : ScriptableObject
    {
        public WfcTileSO[] Tiles;
        public string BorderTileId;           // WFC-3 경계
        public string DefaultGrassTileId;
        // 검증 유틸
        public bool Validate(out string reason);
    }

    // ── 솔버 I/O ─────────────────────────────────────────────────────
    public struct WfcInput
    {
        public int Width, Height;
        public ulong Seed;                    // HR-TD-2 Xorshift64 seed
        public WfcTileSetSO TileSet;
        public IReadOnlyList<WfcFixedCell> FixedCells;
    }

    public struct WfcFixedCell { public int X, Y; public string TileId; }

    public struct WfcOutput
    {
        public WfcStatus Status;
        public string Reason;
        public string[,] Grid;                // TileId[Width, Height]
        public int Iterations;
        public int ContradictionCell;         // -1 if Ok
    }

    public enum WfcStatus { Ok, Contradiction, InvalidInput, TilesetIncomplete }

    // ── 솔버 엔트리 ───────────────────────────────────────────────────
    public static class WfcSolver
    {
        public static WfcOutput Solve(WfcInput input);
    }
}

namespace Proto.TD.Level.Wfc
{
    // TD 전용 어댑터 — Q7 크기 + Q6 lanes + 경로 waypoint + 발전소/기지 좌표
    // 를 받아 WfcInput 을 구성하고 level.schema.json 형식 JSON 을 반환.
    public static class TdLevelWfcAdapter
    {
        public static string BuildLevelJson(TdWfcSpec spec);
    }

    public struct TdWfcSpec
    {
        public int GridSize;                  // 20 / 40 / 80
        public ulong Seed;
        public Vector2Int[][] PathWaypoints;  // Q6 lanes 수만큼
        public Vector2Int[] PowerSources;
        public Vector2Int CoreBase;
        public Proto.Stage.PCG.Wfc.WfcTileSetSO TileSet;
    }
}
```

**이벤트**: 없음 (순수 함수 모듈). 프레젠테이션 레이어에서 `WfcOutput.Grid` 를 받아 프리팹 스폰.

**JSON 출력 필드 (level.schema.json 미리보기)**:
```json
{
  "schemaVersion": 1,
  "levelId": "level-wfc-medium-0xC0FFEE",
  "gridSize": { "w": 40, "h": 40 },
  "cellSize": 1.0,
  "seed": "0xC0FFEE",
  "tiles": [ ["grass-flat", "road-straight-ns", ...], ... ],
  "paths": [ [ {"x":0,"y":20}, {"x":39,"y":20} ] ],
  "powerSources": [ {"x":5,"y":5} ],
  "coreBase": {"x":35,"y":35},
  "linkBudget": 100
}
```

## 4. Dependencies

**Required**
- `td-json-importer` — WFC 출력 JSON 을 Unity SO 로 재임포트.
- `Proto.TD.Sim.Rng` (Xorshift64) — HR-TD-2 공식 RNG. 본 모듈은 이 타입을 import 만 (정의는 `td-sim-core`).

**Optional**
- `td-placement-grid` — WFC 는 셀 기반이므로 placement-grid 의 cell registry 재활용 가능.
- `td-level-template-{small,medium,large}` — 본 모듈은 이들의 **생성 엔진**. 템플릿 문서의 "JSON 생성" 스텝이 본 모듈로 위임된다.
- `stage-graph` — 고정 경로(Q2-A) 모드에서 경로 waypoint 를 stage-graph 노드로 전개.
- `character-render-kit` — 타일 프리팹이 캐릭터 렌더 파이프라인과 같은 머티리얼 규약을 따라야 할 때.
- `env-bush-billboard` — `Wfc_Tile_Grass_Bush` 의 프리팹 소스.

**독립**
- `CityscapeGenerator` (`Assets/Proto/Runtime/Stage/PCG/`) — 블록 서브디비전. 본 모듈과 알고리즘 공유 없음. 장래 하이브리드 확장은 별도 proposal.

## 5. Default Prefabs/Assets

**SO**
- `Assets/Proto/Data/Config/Wfc/TileSet_TD_Default.asset` — TD 3종 크기 공용 기본 타일셋.

**타일 SO — 최소 번들 (12종)**
| TileId | Role | 회전 허용 | 소켓 (N/E/S/W) | 가중 |
|---|---|---|---|---|
| `road-straight-ns` | Path | no | road/grass/road/grass | 5 |
| `road-straight-ew` | Path | no | grass/road/grass/road | 5 |
| `road-corner-ne` | Path | yes (4종 자동 파생) | road/road/grass/grass | 3 |
| `road-t-n` | Path | yes | grass/road/road/road | 2 |
| `road-cross` | Path | no | road/road/road/road | 1 |
| `grass-flat` | Grass | no | grass/grass/grass/grass | 10 |
| `grass-bush` | Grass | no | grass/grass/grass/grass | 2 |
| `building-small` | Building | yes | bldg-wall/bldg-wall/bldg-wall/bldg-wall | 4 |
| `power-pad` | PowerPad | no | road/road/road/road (주변 도로 필수) | — (fixed) |
| `tower-pad` | TowerPad | no | grass/road/grass/road | — (fixed) |
| `core-base` | CoreBase | no | road/road/road/road | — (fixed) |
| `border-void` | Border | no | void/void/void/void | — (fixed) |

**소켓 매칭 규칙** (Symmetric=true 기준 자동 매칭):
- `road` ↔ `road`
- `grass` ↔ `grass` / `road` / `bldg-wall`
- `bldg-wall` ↔ `bldg-wall` / `grass`  ← 건물은 다른 건물과 붙을 수 있으나 도로에는 직접 접하지 않음 (HR-PCG-City-01 정신)
- `void` ↔ `void` / `grass`  ← 경계

**프리팹**
- `Assets/Proto/Prefabs/TD/Wfc/TD_Wfc_Tile_<id>.prefab` — 1×1 cell 메시. URP Lit 머티리얼, static.
- Placeholder 아트: 단색 cube + 머티리얼 컬러. 실 미술 에셋은 구현 단계에서.

**샘플 출력 JSON (고정 산출물)**
- `Assets/Proto/Data/TD/Levels/Level_Wfc_Sample_Medium.json` — seed=`0xC0FFEE`, 40×40, 2 lane. 문서 검증 / 결정론 회귀 테스트용. 본 모듈 수정 시 재생성 후 diff 0 이어야 함.

## 6. Skill Hook

**새 스킬**: `/proto-stage-wfc` (장르 중립)
- 인자: `size=small|medium|large`, `seed=<hex>`, `tileset=<path>`

**TD 연계 호출**: `/proto-td-level` (기존 스킬 확장)
- 인자: `size=small|medium|large`, `--wfc` (플래그. 생략 시 수동 JSON 사용)

**수행 순서**
1. `TileSet_TD_Default.asset` 로드. 없으면 생성 서브스킬 `/proto-stage-wfc-tileset-default` 호출.
2. Q7 크기 확정 (20/40/80), cellSize=1.
3. 경로 waypoint 생성 — Q6 lanes 수만큼 (기본 서브 알고리즘 TBD, §미결 사항 참조).
4. `FixedCells` 구성: border + 경로 타일 + PowerSource + CoreBase.
5. `WfcSolver.Solve(input)` 호출.
6. `Status == Ok` 확인 후 `TdLevelWfcAdapter.BuildLevelJson(...)` 로 JSON 직렬화.
7. `Assets/Proto/Data/TD/Levels/Level_Wfc_<size>_<seed>.json` 저장.
8. `td-json-importer` 호출 → `Level_Wfc_<size>_<seed>.asset` 생성.
9. 씬에 `TdLevelRoot` GameObject 루트에 타일 프리팹 스폰.
10. 카메라(`camera-rig`) 초기 위치를 grid 중앙 / 줌은 맵 전체 조망으로 조정.

**실패 처리**
- `Status == Contradiction` → **에스컬레이션** 1문장: "타일셋 인접 규칙이 셀 (x,y) 에서 수렴 실패 — 타일셋 점검 필요". `.vkl/runtime/case_logs/` 에 failing seed + tileset snapshot 기록. **재시도 금지** (WFC-2).
- `Status == TilesetIncomplete` → 누락 타일 ID 열거 후 사용자 확인.
- `Status == InvalidInput` → 인자 오류. 사용법 재출력.

## 7. Verification

### 7.1 Verification (기계적)
| 항목 | Oracle | 방법 |
|---|---|---|
| 컴파일 0 | OR-07 Artifact Presence | `/compile-check` |
| 결정론 스모크 | OR-03 Deterministic Replay | 동일 seed 2회 실행 → `Grid` bit-exact 동일 + `Iterations` 동일 |
| JSON 스키마 준수 | OR-01 Spec Oracle | `level.schema.json` validate 통과 (스키마 확정 후) |
| 인접 규칙 무결성 | OR-04 Property Oracle | 출력 grid 전체 쌍에서 소켓 호환 검증 |
| 연결성 | OR-04 Property Oracle | 모든 PowerSource → CoreBase 경로가 `Path` role 타일로 연결 (BFS) |
| 경계 타일 | OR-01 Spec Oracle | 가장자리 4면 전체가 `borderTileId` (WFC-3) |

### 7.2 Validation (의미)
| 항목 | Oracle | 기준 |
|---|---|---|
| 도메인 룰 준수 | OR-P-006 PCG Domain Rule (PROP-2026-04-24-001, 검토 대기) | 건물 타일이 도로 축에 정렬 (HR-PCG-City-01 정신) |
| 플레이어 가독성 | OR-08 Human Oracle | 생성 맵이 "시가지 / TD 레벨답게" 보이는가. 스크린샷 확인 필수. |
| 다양성 | OR-06 Metamorphic | seed 별 결과가 충분히 다르되 통계적 분포 유지 |

### 7.3 Metamorphic Relations
- **MR-A (seed 민감도)**: seed 바꾸면 grid 달라진다. 동일 grid 나오면 FAIL (RNG 미주입 의심).
- **MR-B (대칭 회전)**: `AllowRotation=true` 타일은 90° 회전 파생물이 원본과 소켓 호환 동일해야 함.
- **MR-C (가중치 단조성)**: 타일 weight 를 2배로 올리면 Monte Carlo 100회 평균 등장 비율이 증가(통계적으로 유의).
- **MR-D (fixed cell 불변식)**: `FixedCells` 에 박은 타일은 출력에서 그대로 유지되어야 한다.

### 7.4 기록 원칙
- 수렴 실패(Contradiction) 케이스 → `.vkl/runtime/case_logs/CL-YYYY-MM-DD-NNN.md` 에
  failing seed + 타일셋 snapshot + contradiction cell 좌표 기록.
- 최초 구현 완료 시 OBS + ORUN 한 세트 생성 (결정론 스모크 증거).

---

## 미결 · 후속 과제

- **경로 생성 서브 알고리즘**: Q6 lanes 수만큼 waypoint 경로를 어떻게 뽑을지. 후보 (a) 가장자리 랜덤 진입점 + A* (b) 미리 뽑힌 `Path_*.asset` 풀에서 샘플링. 현 문서는 (b) 가정. 실 구현 전 확정 필요.
- **타일셋 미술 에셋**: 12종 타일 프리팹은 URP Lit cube + 컬러 placeholder. 실 미술 에셋 출처 미정 (Asset Store / 직접 모델링). `_progress.md §6.4 재방문 조건`.
- **`level.schema.json` 초안**: 카탈로그 미결 과제에서 선행 필요. 본 모듈의 JSON 출력 필드가 스키마 초안의 1차 레퍼런스가 될 수 있음.
- **CityscapeGenerator 하이브리드**: 블록 서브디비전 출력 → 블록 내부 WFC 적용. 별도 `PROP-YYYY-pcg-hybrid.md` 제안 필요.
- **turn3d / rail-shooter 재사용**: 본 모듈은 장르 중립. 장르별 `WfcTileSetSO` 분리만 해주면 재사용. 해당 장르 작업 시점에 실검증.
- **`td-sim-parity` 표준 seed 확장**: WFC 결정론 회귀 테스트용 seed 세트 추가. 구현 단계에서 병합.
- **HR 승격 여부**: PROP-2026-04-24-001 의 `HR-PCG-City-01/02` 가 `_conventions.md` 에 승격되면 본 문서 §2 에서 "proposed" 표기 제거.
