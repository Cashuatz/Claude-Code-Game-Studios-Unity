# Module: td-playtest-runner

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

HR-TD-8 강제. 자동 Monte Carlo 플레이테스트 러너.
seed 목록 + 플레이어 입력 스크립트 받아 N회 시뮬 실행, 통계 리포트 생성.

## 2. Hard Rules

- 결정론 기반 (HR-TD-1).
- 웹 전용 CLI 실행 가능해야 함 (`node sim/run.js`).
- 결과는 `design/playtest-reports/<level>-<date>.json`.

## 3. Public API

```csharp
namespace Proto.TD.Validation
{
    public sealed class PlaytestRunner
    {
        public PlaytestReport Run(
            LevelSO level,
            IReadOnlyList<uint> seeds,
            IReadOnlyList<InputScript> inputs
        );
    }

    public class PlaytestReport
    {
        public int TotalRuns;
        public int Clears;
        public float ClearRate;
        public float AvgSurvivalSeconds;
        public float AvgBudgetRemaining;
        public List<RunOutcome> Outcomes;
    }
}
```

## 4. Dependencies

- Required: `td-sim-core`, `td-json-importer`.

## 5. Default Prefabs/Assets

- 스크립트: `PlaytestRunner.cs`.
- TS 러너: `web-sim/playtest/run.ts` (공유 코어 재사용).

## 6. Skill Hook

`/proto-td-playtest`:

1. `PlaytestRunner.cs` 생성.
2. 기본 seed 100개 세트.
3. 더미 입력 스크립트 생성 (초기 3 타워 배치).
4. Unity 에디터 메뉴 `Proto/TD/Run Playtest` 추가.
5. 결과 JSON 저장.

## 7. Verification

- 100회 실행 후 리포트 파일 생성.
- 클리어율 출력.
- Large 맵 선택 시 자동으로 25회로 감소.
