# Module: td-sim-core

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

결정론 tick 엔진. UnityEngine 참조 없이 순수 C# 로 TD 시뮬을 돌리는 코어.
웹(TS) 구현과 bit-exact 동기화 기반. 모든 TD 모듈의 `Tick(dt)` 진입점.

## 2. Hard Rules

- 공통 `_conventions.md` HR-8/9/10 상속
- TD 전용 `_conventions-td.md` **HR-TD-1/2/3** 강제: UnityEngine 금지, Xorshift RNG 만, fixed 20ms.
- `Proto.TD.Sim.*` 네임스페이스 내 모든 파일은 UnityEngine using 금지 (빌드 타임 검증).

## 3. Public API

```csharp
namespace Proto.TD.Sim
{
    public sealed class SimCore
    {
        public SimCore(uint seed, float tickSeconds = 0.02f);
        public uint Seed { get; }
        public long TickCount { get; }
        public float TickSeconds { get; }
        public Rng Rng { get; }
        public LevelState State { get; }

        public void Step();              // 1 tick 진행
        public void StepN(int ticks);    // N tick 진행
        public SimSnapshot Snapshot();   // 현재 상태 복제
        public void Restore(SimSnapshot snap);
    }

    public sealed class Rng
    {
        public Rng(uint seed);
        public uint NextU32();
        public float NextFloat01();
        public int Range(int minInclusive, int maxExclusive);
    }

    public readonly struct SimSnapshot { /* 불변 구조체 */ }
}
```

## 4. Dependencies

- Required: 없음 (pure C#).
- Optional: `td-json-importer` (입력 데이터 로드 시).

## 5. Default Prefabs/Assets

- 프리팹 없음. 순수 C# 라이브러리.
- 파일: `Assets/Proto/Runtime/TD/Sim/SimCore.cs`, `Rng.cs`, `LevelState.cs`, `SimSnapshot.cs`.

## 6. Skill Hook

`/proto-td-sim`:

1. `Assets/Proto/Runtime/TD/Sim/` 디렉토리 생성.
2. `SimCore.cs`, `Rng.cs` (Xorshift64), `LevelState.cs`, `SimSnapshot.cs` 생성.
3. `Assets/Proto/Runtime/TD/Proto.TD.Sim.asmdef` 생성 — **UnityEngine 참조 제외**.
4. 스모크 테스트: `new SimCore(42)` 생성 후 `StepN(100)` → 에러 0.
5. 결정론 검증: 동일 seed 2회 `StepN(100)` → 상태 bit-exact 일치.

## 7. Verification

- 컴파일 에러 0.
- asmdef 에 UnityEngine 참조 없음 (정적 검증: `grep -r "UnityEngine" Assets/Proto/Runtime/TD/Sim/` → 0 매치).
- 결정론 테스트 통과 (동일 seed → 동일 최종 상태).
