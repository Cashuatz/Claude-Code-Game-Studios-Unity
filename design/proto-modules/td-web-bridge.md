# Module: td-web-bridge

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

웹 레벨 에디터 산출물 감시 + 리플레이 임포트. `Assets/Proto/Data/TD/Levels/*.json`
파일 변화 감지 시 자동 재임포트 + 리플레이 JSON(`seed + input stream`) 을 시뮬 재생용으로 로드.

## 2. Hard Rules

- TD HR-TD-4/HR-TD-9/HR-TD-10 강제.
- 웹 에디터 외부 생성 파일 수정 금지 — 읽기 전용 감시.
- 리플레이 재생 중 시뮬 상태 외부 변경 금지.

## 3. Public API

```csharp
namespace Proto.TD.Data
{
    public static class WebBridge
    {
        public static void StartWatch(string levelDirectory);
        public static void StopWatch();
        public static event Action<string> OnLevelChanged;

        public static ReplayData LoadReplay(string jsonPath);
        public static void PlayReplay(SimCore sim, ReplayData replay);
    }

    [Serializable]
    public class ReplayData
    {
        public int schemaVersion = 1;
        public uint seed;
        public string levelId;
        public List<ReplayInput> inputs;
    }
}
```

## 4. Dependencies

- Required: `td-sim-core`, `td-json-importer`.
- Optional: `td-sim-parity` (검증 파이프).

## 5. Default Prefabs/Assets

- 스크립트: `Assets/Proto/Editor/TD/WebBridge.cs`.
- 샘플: `Assets/Proto/Data/TD/Replays/replay_sample.json`.

## 6. Skill Hook

`/proto-td-web-bridge`:

1. `WebBridge.cs` 생성 (FileSystemWatcher 기반).
2. 기본 감시 경로 `Assets/Proto/Data/TD/Levels/` 설정.
3. 샘플 리플레이 JSON 생성.
4. 테스트: 샘플 JSON 터치 → `OnLevelChanged` 발행.
5. `PlayReplay` 실행 → SimCore 에 입력 순차 적용.

## 7. Verification

- FileSystemWatcher 정상 동작.
- ReplayData 로드 성공.
- 리플레이 적용 후 시뮬 상태가 예상 최종 상태와 bit-exact 일치.
