# Module: sound-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ / td ✅ / rail-shooter ✅ (전 장르 공용)

## 1. Purpose

BGM / SFX / Voice / Stinger 의 **재생·믹싱·풀링** 공통 프레임워크.
장르별 오디오 이벤트(적 사망, 타워 발사, 불릿타임 발동 등) 가 이 모듈의 API 로만
재생되도록 강제하여 중복 리소스·중복 구현을 방지한다.

## 2. Hard Rules

- **진입점 1개**: 모든 오디오 재생은 `AudioHub.Play*()` 경유. `AudioSource.Play()` 직접 호출 금지.
- **풀링 강제**: SFX 는 `AudioSourcePool` 에서 대여. 씬에 `AudioSource` 를 스크립트로 즉석 생성 금지.
- **믹서 그룹 고정**: `Master / BGM / SFX / Voice / UI` 5개 그룹만. 추가는 `_conventions.md` 에 proposal.
- **비동기 로드**: `AudioClip` 은 `Resources` 금지, **Addressables** 로만 로드 (향후 도입 시). 프로토 단계에서는 `Assets/Proto/Sound/` 직접 참조 허용.
- BGM 전환은 **페이드 인/아웃 강제** (`AudioHub.PlayBgm` 내부에서 크로스페이드).
- 리듬(turn3d Q2-C Rhythm) 은 `BeatSync` 서브모듈 경유. `Time.time` 기반 비트 계산 금지 — DSP 시간 사용.

## 3. Public API

```csharp
namespace Proto.Shared.Sound
{
    public enum AudioBus { Master, Bgm, Sfx, Voice, Ui }

    public static class AudioHub
    {
        // SFX: one-shot, 자동 반환
        public static void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f);
        public static void PlaySfx(string addressableKey, float volume = 1f);
        public static void PlaySfxAt(AudioClip clip, Vector3 worldPos, float volume = 1f);

        // BGM: 단일 채널, 크로스페이드
        public static void PlayBgm(AudioClip clip, float fadeSeconds = 1f, bool loop = true);
        public static void StopBgm(float fadeSeconds = 1f);

        // Voice: 단일 채널, 큐잉
        public static void PlayVoice(AudioClip clip);
        public static void StopVoice();

        // Stinger: BGM 위에 짧게 겹쳐 재생
        public static void PlayStinger(AudioClip clip, float volume = 1f);

        // Bus 볼륨
        public static void SetBusVolume(AudioBus bus, float linear01);
        public static float GetBusVolume(AudioBus bus);
    }

    // 비트 동기: Rhythm CombatScheme 전용
    public static class BeatSync
    {
        public static double CurrentBeat { get; }       // DSP 시간 기반
        public static event Action<int> OnBeat;         // 정수 비트마다 발행
        public static void StartTrack(BeatTrackAsset asset);
        public static void StopTrack();
    }

    [CreateAssetMenu]
    public class BeatTrackAsset : ScriptableObject
    {
        public AudioClip Music;
        public float BPM;
        public int BeatsPerBar;
        public double FirstBeatOffset;  // 초 단위
    }
}
```

## 4. Dependencies

- **Required**: Unity `AudioMixer` (`AudioMixer_Master.mixer`).
- **Optional**: `hud-kit` (볼륨 설정 UI), `save-kit` (볼륨 저장).

## 5. Default Prefabs / Assets

- `AudioHub.prefab` — `AudioSource` 3개 (BGM current/next/voice) + Pool 루트.
- `AudioMixer_Master.mixer` — 5 그룹 구성.
- `AudioSourcePool.prefab` — 16개 풀링 소스.
- `BeatTrack_Example.asset` — 리듬 예시 (120 BPM, 4박자).

## 6. Skill Hook

`/proto-sound-kit`:

1. `Assets/Proto/Runtime/Shared/Sound/` 에 스크립트 복사.
2. `AudioHub.prefab` 생성 + 씬 배치 (DontDestroyOnLoad).
3. `AudioMixer_Master.mixer` 생성 + 5 그룹.
4. `AudioSourcePool` 풀 초기화.
5. 테스트 재생: 무음 1프레임 `PlaySfx(null)` 안전 호출 검증.
6. 검증: 에러 0, Mixer 5 그룹 확인.

## 7. Verification

- **컴파일**: 0 에러.
- **Mixer**: `AudioMixer_Master.mixer` 에 Master/Bgm/Sfx/Voice/Ui 5개 그룹.
- **Hub 존재**: 씬에 `AudioHub` 1개, DontDestroyOnLoad 플래그 ON.
- **풀 기본 크기**: `AudioSourcePool.Count == 16`.
- **BeatSync (Rhythm 선택 시만)**: `BeatTrack_Example.asset` 재생 후 `OnBeat` 이벤트 4초에 8회 발행.
