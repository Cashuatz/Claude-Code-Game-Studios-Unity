# TD_CityWfc — 시가지 PCG + WFC + TD 레벨 실사례

타워 디펜스용 맵을 **시가지 PCG** (블록 분할 → 건물 lot) 와 **WFC 타일 솔버** (도로 연결성 + 광장 패턴) 의 **하이브리드** 로 생성하는 실사례.

## 구성

```
TD_CityWfc/
├── Runtime/TD/
│   ├── Level/
│   │   ├── AStarGrid.cs            — blocked[,] 기반 A* (타워/적 경로 테스트용)
│   │   ├── JsonWriter.cs           — LevelDesigner 산출물 JSON 직렬화
│   │   ├── TdPathGenerator.cs
│   │   ├── Block/
│   │   │   └── BlockLayoutPlanner.cs
│   │   └── Wfc/
│   │       ├── TdDefaultTileSet.cs  — TD 규약에 맞춘 타일셋 기본값
│   │       ├── TdLevelWfcAdapter.cs — PCG 결과 → WFC 초기 조건 변환
│   │       └── TdWfcLevelSpawner.cs — 최종 씬 인스턴스화
│   └── Sim/
│       └── Rng.cs                   — 결정론 시드 RNG
├── Editor/TD/
│   └── TdWfcDemoMenu.cs              — Proto/TD WFC 메뉴 (레벨 생성 + 씬 스폰)
├── Data/TD/
│   ├── Levels/Level_Wfc_Sample_Medium.json  — 40×40 샘플 레벨
│   └── Schema/level.schema.json             — JSON 스키마
├── Prefabs/TD/Wfc/                   — TD-specific WFC 결과 프리팹 (있을 경우)
├── Tests/Editor/
│   ├── Proto.Tests.Editor.asmdef     — EditMode 테스트 어셈블리
│   ├── WfcAdjacencyTests.cs          — 타일 인접 호환
│   ├── WfcConnectivityTests.cs       — core 도달성 + 수렴률 ≥90%
│   └── WfcDeterminismTests.cs        — 동일 시드 bit-exact 재현
├── Scenes/
│   ├── Proto_TD.unity                — 런처에서 도달하는 TD 껍데기 씬
│   └── TdWfcDemo.unity               — WFC 결과 시각 확인 전용 씬
└── README.md
```

## 공용 모듈과의 관계

- `Runtime/Stage/PCG/Cityscape*` (공용) — 건물 블록 분할·AssetSet 추상·결과 메트릭.
- `Runtime/Stage/Wfc/*` (공용) — 타일 SO + 소켓 제약 솔버 (장르 무관).
- `Runtime/Camera/*`, `Runtime/Movement/*` (공용) — TD 카메라 + 이동 에이전트.
- 이 Sample 은 위 공용 모듈들을 **TD 규약** (HR-TD-*) 에 맞게 엮은 레이어일 뿐이다.

## 핵심 기법

- **블록-WFC 하이브리드** — 거시 레이아웃은 서브디비전 (결정적), 미시 타일 연결은 WFC (확률적). 도로 연결성과 블록 정렬을 동시에 확보.
- **결정론 시드 RNG (`Sim/Rng.cs`)** — 같은 시드 → bit-exact 동일 레벨 (OR-03 Replay Oracle 충족).
- **JSON 레벨 직렬화** — 생성된 레벨을 외부 도구와 왕복 (`Data/TD/Levels/*.json`).
- **EditMode 테스트** — 인접·연결·결정론 3축을 WfcXxxTests 로 자동 검증.

## 알려진 Spec Gap (TD 구현 재개 시 참고)

- `TdWfcLevelSpawner` 가 **FlowFieldBuilder.blocked[,]** 배열을 어떻게 공급하는지 명세 미완. `Cityscape*.GenerateResult` 에 `BlockedTiles` 를 추가하는 **Option A** 가 최우선 권고 (`.vkl/archive/2026-04-24-proto-package-finalization/proposals/PROP-2026-04-24-002.md` 참고).
