# Module: td-sim-parity

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

HR-TD-9 강제. 웹(TS) 시뮬과 Unity(C#) 시뮬의 **bit-exact 일치 검증기**.
CI / 수동 호출 시 동일 seed + 입력 재생 후 최종 상태 비교.

## 2. Hard Rules

- 불일치 시 PR 블록 (CI 통합 시).
- 최종 상태만 비교 (중간 tick 비교는 선택).

## 3. Public API

```csharp
namespace Proto.TD.Validation
{
    public sealed class SimParityChecker
    {
        public ParityResult Compare(string webReplayJsonPath, string unityReplayJsonPath);
    }

    public struct ParityResult
    {
        public bool IsMatch;
        public List<string> Diffs;
    }
}
```

## 4. Dependencies

- Required: `td-sim-core`, `td-web-bridge`.

## 5. Default Prefabs/Assets

- 스크립트: `SimParityChecker.cs`.
- 샘플: `web-sim/tests/parity/replay_sample.json`.

## 6. Skill Hook

`/proto-td-parity`:

1. `SimParityChecker.cs` 생성.
2. 샘플 리플레이 2개 (웹/Unity) 생성.
3. 비교 실행 → 불일치 리포트.

## 7. Verification

- 동일 로직·시드 → 일치.
- 의도적 불일치 (RNG 변경 시뮬) → 실패 감지.
