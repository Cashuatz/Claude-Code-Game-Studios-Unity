# Module: td-wave-spawner

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

웨이브 타임라인 기반 적 스폰. Q6 (경로 수) 에 따라 single/multi-lane.

## 2. Hard Rules

- 스폰은 시뮬 tick (HR-TD-3).
- 웨이브 정의는 `WaveSO` 에서 로드 (HR-TD-4).
- 단일 lane / 2~3 / 4+ 지원.

## 3. Public API

```csharp
namespace Proto.TD.Wave
{
    public sealed class WaveSpawner
    {
        public int LaneCount { get; }
        public int CurrentWaveIndex { get; }
        public int TotalWaves { get; }
        public bool IsComplete { get; }

        public void StartNextWave();
        public event Action<int> OnWaveStart;
        public event Action<int> OnWaveComplete;
    }

    [CreateAssetMenu]
    public class WaveSO : ScriptableObject
    {
        public List<WaveEntry> Entries;
    }

    [Serializable]
    public struct WaveEntry
    {
        public float triggerTime;
        public string enemyId;
        public int laneIndex;
        public int count;
        public float spreadSeconds;
    }
}
```

## 4. Dependencies

- Required: `td-enemy-core`, `td-pathing-*`, `td-sim-core`.

## 5. Default Prefabs/Assets

- 스크립트: `WaveSpawner.cs`, `WaveSO.cs`.
- SO: `Waves_SingleLane.asset`, `Waves_TwoLane.asset`, `Waves_ThreeLane.asset`, `Waves_FourLane.asset`.

## 6. Skill Hook

`/proto-td-waves` (인자: `single|two|three|four`):

1. `WaveSpawner.cs`, `WaveSO.cs` 생성.
2. 인자에 맞는 SO 생성 (3~5 웨이브, 각 웨이브 10~20 적).
3. SimCore.OnTick 구독 → triggerTime 도달 시 `Enemy` 스폰.

## 7. Verification

- 웨이브 시작 → `OnWaveStart` 발행.
- 모든 적 사망 or 기지 도달 → `OnWaveComplete`.
- 마지막 웨이브 완료 → `IsComplete=true`.
