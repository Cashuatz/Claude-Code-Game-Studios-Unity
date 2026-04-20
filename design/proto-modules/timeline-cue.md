# Module: TimelineCue

> 참고: [`_catalog.md`](./_catalog.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

연출 컷신을 **Unity Timeline (PlayableDirector)** 으로 실행하기 위한 3개 슬롯:

- **Intro**  — 전투 시작, 적 등장 등
- **Ultimate** — 궁극기 발동 (캐릭터별 커스텀)
- **Outro** — 전투 종료, 승리/패배 연출

각 슬롯은 이름으로 TimelineAsset 을 찾아 재생하고, 완료/중단 이벤트를 발행한다.
재생 중에는 **TurnSystem 이 일시정지**되고, CameraRig 는 Timeline 제어를 위임받는다.

Timeline 자체는 Unity 내장. 이 모듈은 **재생 큐 + 훅**을 제공하는 얇은 레이어.

## 2. Hard Rules

- **HR-9 준수**: Timeline 재생 시작/종료는 이벤트로 브로드캐스트. 다른 모듈이 폴링 금지.
- **HR-4 준수**: Timeline 내부 카메라 트랙은 `CameraRig.RequestMode` 호출로 모드 전환.
  `Cinemachine` 트랙 금지 (프로젝트 전체 Cinemachine 비사용).
- Timeline 재생 중 TurnSystem 은 **일시정지** (모든 LocalTimeScale 0). 종료 시 복귀.
- Ultimate 컷신은 캐릭터별 TimelineAsset 을 `CharacterDefinition.ultimateTimeline` 에 연결.
- TimelineAsset 은 `Assets/Proto/Timelines/` 하위에만 저장.

## 3. Public API

```csharp
namespace Proto.Timeline
{
    public enum CueSlot { Intro, Ultimate, Outro }

    public readonly struct CueRequest
    {
        public CueSlot Slot { get; }
        public TimelineAsset Asset { get; }
        public IUnit Caster { get; }        // Ultimate 용, 다른 슬롯은 null 허용
        public Dictionary<string, object> Bindings { get; } // 트랙 바인딩 오버라이드
    }

    public interface ITimelineCueDirector
    {
        bool IsPlaying { get; }
        CueSlot? CurrentSlot { get; }

        /// <summary>큐 재생. 이미 재생 중이면 대기열에 적재.</summary>
        Task Play(CueRequest request);

        /// <summary>현재 재생 중인 큐 즉시 종료 (스킵).</summary>
        void Skip();

        /// <summary>대기열 포함 전체 중단.</summary>
        void ClearQueue();

        event Action<CueSlot> CueStarted;
        event Action<CueSlot> CueEnded;
        event Action<CueSlot> CueSkipped;
    }
}
```

### 구현 `TimelineCueDirector : MonoBehaviour`

- `PlayableDirector` 컴포넌트 1개 소유. 요청 시 해당 director 에 `TimelineAsset` 할당 후 `Play()`.
- `stopped` 이벤트 수신 시 `CueEnded` 발행 후 다음 큐 재생.
- 재생 시작 시 `TurnSystem.SetPaused(true)`, 종료 시 `false`.

### TimelineAsset 규칙

- 카메라 트랙: `CameraRig.RequestMode(CameraMode, TransitionProfile)` 을 호출하는
  **Custom Activation Track** 또는 **Signal Receiver** 로 구현 (Cinemachine 트랙 금지).
- 애니메이션 트랙: `SkinnedRenderer` 의 `Animator` 바인딩 OK.
- Audio 트랙: `SoundKit` 과 공유 (Phase D 연결).
- 커스텀 시그널: `CutinSignal`, `ScreenShakeSignal` 등 Proto 가 제공 (Phase B 말에 최소셋).

## 4. Dependencies

- **Required**: Unity Timeline 패키지 (`com.unity.timeline`) — Unity 6 기본 포함
- **Optional**:
  - `CameraRig` — 카메라 트랙 바인딩
  - `TurnSystem` — 재생 중 일시정지
  - `CutinDirector` — 컷인 시그널 수신
  - `SoundKit` — 오디오 트랙

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Timeline/TimelineCueRoot.prefab` | 빈 GameObject + `PlayableDirector` + `TimelineCueDirector` MB |
| `Assets/Proto/Timelines/Intro_Default.playable` | 빈 Intro (2초 카메라 줌 in) |
| `Assets/Proto/Timelines/Ultimate_Placeholder.playable` | 빈 Ultimate (3초 컷인 + 카메라 흔들림 시그널) |
| `Assets/Proto/Timelines/Outro_Default.playable` | 빈 Outro (2초 페이드) |
| `Assets/Proto/Runtime/Timeline/Signals/` | `CutinSignal.asset`, `ScreenShakeSignal.asset` |

Runtime 스크립트:
- `CueSlot.cs`, `CueRequest.cs`, `ITimelineCueDirector.cs`, `TimelineCueDirector.cs`
- `CutinSignalReceiver.cs`, `ScreenShakeSignalReceiver.cs`
- `CameraModeSignalReceiver.cs` (카메라 모드 전환 시그널)

## 6. Skill Hook

`/proto-timeline-cue <intro|ult|outro>` 호출 시:

1. `Assets/Proto/Runtime/Timeline/` 생성 (첫 호출 시).
2. 공통 스크립트 설치 (첫 호출 시):
   - `CueSlot`, `CueRequest`, `ITimelineCueDirector`, `TimelineCueDirector`
   - 시그널 리시버 3개 (Cutin, ScreenShake, CameraMode)
   - 시그널 SO asset 2개 (Cutin, ScreenShake)
3. 지정 슬롯의 `*.playable` 기본 자산 생성.
4. 첫 호출 시 `TimelineCueRoot.prefab` + `ProtoMain.unity` 배치.
5. 콘솔 에러 확인.

옵션 인자:
- `--skip-default-asset` — 기본 TimelineAsset 생성 건너뜀 (사용자가 이미 작성 중일 때).

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="TimelineCueDirector", search_method="by_component")` → 1.
3. `execute_code` 스모크 (Intro 재생):
   ```csharp
   var director = UnityEngine.Object.FindObjectOfType<Proto.Timeline.TimelineCueDirector>();
   var asset = Resources.Load<UnityEngine.Timeline.TimelineAsset>("Intro_Default");
   var req = new Proto.Timeline.CueRequest(
       Proto.Timeline.CueSlot.Intro, asset, caster: null, bindings: null);

   bool ended = false;
   director.CueEnded += _ => ended = true;

   var task = director.Play(req);
   yield return new WaitForSeconds(2.5f);

   UnityEngine.Debug.Assert(ended, "CueEnded not raised after timeline completion");
   UnityEngine.Debug.Assert(!director.IsPlaying);
   ```
4. 재생 중 TurnSystem 일시정지 검증:
   ```csharp
   var sys = UnityEngine.Object.FindObjectOfType<Proto.Turn.TurnSystem>();
   director.Play(req);
   yield return null;
   foreach (var actor in sys.Actors)
       UnityEngine.Debug.Assert(actor.Unit.LocalTimeScale == 0f,
           "TurnSystem not paused during timeline");
   ```
5. 시각 검증 — 재생 중 스크린샷 2~3프레임 간격 저장.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01
- OR-INTEGRATION-04 (Timeline ↔ TurnSystem 일시정지)
- OR-HARD-RULE-04 (HR-4 카메라 전환은 RequestMode 만)
