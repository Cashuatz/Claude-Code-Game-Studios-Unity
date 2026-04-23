# Module: td-game-rule-killcount

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q4-B. 지정된 적 N마리 처치 시 승리. **단, 게임 완결성을 위해 "모든 웨이브 스폰 완료 + 맵 위 생존 적 0" 조건을 함께 만족해야 최종 승리 판정**.

## 2. Hard Rules

- `Enemy.OnKilled` 구독으로 카운트. 사살(Killed)만 카운트, 기지 도달 누수(Leaked)는 **제외**.
- 기본 목표 50 kill.
- **승리 판정은 AND 조건**:
  1. `WaveSpawner.AllWavesSpawned == true` (모든 웨이브 스폰 완료)
  2. `EnemyRegistry.AliveCount == 0` (맵 위 생존 적 0)
  3. `KillCountRule.Current >= Target` (목표 킬 달성)
- 조건 1·2 만족 + 킬카운트 미달 시 → **패배 (FAIL)**. 목표를 못 채웠으므로.
- 조건 3 먼저 달성해도 승리 확정 안 됨 — HUD 에 "목표 달성, 잔당 소탕 중" 표시하고 1·2 대기.

## 3. Public API

```csharp
namespace Proto.TD.Rule
{
    public sealed class KillCountRule
    {
        public int Target { get; }
        public int Current { get; }         // 누적 사살 수 (누수 제외)
        public bool TargetReached { get; }  // Current >= Target
        public bool IsWon { get; }          // TargetReached && AllWavesSpawned && AliveCount == 0
        public bool IsLost { get; }         // AllWavesSpawned && AliveCount == 0 && !TargetReached

        public event Action<int> OnKillCounted;   // arg: 현재 Current
        public event Action OnTargetReached;       // 목표만 달성 (최종 승리 아님)
        public event Action OnWon;
        public event Action OnLost;
    }
}
```

## 4. Dependencies

- Required: `td-enemy-core`, `td-sim-core`, `td-wave-spawner` (AllWavesSpawned 플래그 필요).
- Optional: `td-hud-killcount`.

## 5. Default Prefabs/Assets

- 스크립트: `KillCountRule.cs`.
- SO: `GameRule_Kill_50.asset` (Target=50).

## 6. Skill Hook

`/proto-td-rule kill` 또는 `kill+core`.

## 7. Verification

- `OnKilled` 이벤트 50회 발생 + 모든 웨이브 스폰 완료 + 맵 위 생존 적 0 → `OnWon`.
- 모든 웨이브 스폰 완료 + 맵 위 생존 적 0 + 사살 < 50 → `OnLost`.
- 사살 ≥ 50 이지만 웨이브 진행 중 → `OnTargetReached` 만 발생, `OnWon` 보류.
- 누수(Leaked) 적 1마리당 카운트 변화 없음 확인.
