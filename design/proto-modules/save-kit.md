# Module: save-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ / td ✅ / rail-shooter ✅ (전 장르 공용)

## 1. Purpose

프로토 범위의 **세션·런·메타** 3계층 저장 시스템.
장르마다 저장할 내용은 다르지만(turn3d 런 시드, td 레벨 진행, rail 체크포인트), 포맷과
로드/저장 API 는 공통이다.

## 2. Hard Rules

- **진입점 1개**: `SaveHub.Save(slot, key, value)` / `Load(slot, key)` 만 사용. `PlayerPrefs` 직접 사용 금지.
- **3계층 슬롯**:
  - `Session` — Play 모드 진입부터 종료까지 (메모리, 비영속)
  - `Run` — 현재 로그라이크 런 단위 (JSON 파일)
  - `Meta` — 영구 저장 (JSON 파일, 메타 프로그레션)
- **포맷**: JSON 만 (바이너리 금지). 스키마 변경 시 `schemaVersion` 필드 증가.
- **경로**: `Application.persistentDataPath/Proto/<slot>.json`. 프로토 단계 별도 암호화 없음.
- **원자적 쓰기**: 임시 파일 작성 후 `File.Move` (원자성 보장).
- **Save 이벤트**: 모든 저장 완료 시 `SaveHub.OnSaved` 이벤트 발행. HUD 표기 가능.

## 3. Public API

```csharp
namespace Proto.Shared.Save
{
    public enum SaveSlot { Session, Run, Meta }

    public static class SaveHub
    {
        public static void Save<T>(SaveSlot slot, string key, T value);
        public static T Load<T>(SaveSlot slot, string key, T defaultValue = default);
        public static bool HasKey(SaveSlot slot, string key);
        public static void Delete(SaveSlot slot, string key);
        public static void Clear(SaveSlot slot);

        public static event Action<SaveSlot> OnSaved;
        public static event Action<SaveSlot> OnLoaded;
    }

    // 공식 번들 DTO 예시 (장르마다 자체 DTO 추가)
    [Serializable]
    public class RunStateDto
    {
        public int schemaVersion = 1;
        public uint runSeed;
        public int currentStage;
        public int currentHp;
        public List<string> acquiredAbilities;
    }

    [Serializable]
    public class MetaProgressionDto
    {
        public int schemaVersion = 1;
        public int totalRuns;
        public int bestScore;
        public List<string> unlockedContent;
    }
}
```

## 4. Dependencies

- **Required**: 없음 (System.IO, Unity JsonUtility).
- **Optional**: `hud-kit` (Save 인디케이터 위젯).

## 5. Default Prefabs / Assets

- 프리팹 불필요 (정적 클래스).
- `SaveConfig.asset` — 저장 경로 오버라이드, 스키마 버전 관리.
- `MetaProgression_Default.asset` — turn3d Q7-C 기본 메타 세팅.

## 6. Skill Hook

`/proto-save-kit`:

1. `Assets/Proto/Runtime/Shared/Save/` 에 스크립트 복사.
2. `SaveConfig.asset` 생성 (기본 경로, schemaVersion=1).
3. `persistentDataPath/Proto/` 디렉토리 생성 (없으면).
4. 테스트: `Save<int>(Meta, "test", 42)` → `Load<int>(Meta, "test") == 42` 확인 후 `Delete`.
5. 검증: 에러 0, 경로 쓰기/읽기 성공.

## 7. Verification

- **컴파일**: 0 에러.
- **경로 존재**: `Application.persistentDataPath/Proto/` 디렉토리 쓰기 가능.
- **원자성**: Save 중 프로세스 강제 종료 가정 — 임시 파일만 남고 원본 무결성 유지 (수동 테스트).
- **JSON 스키마**: 저장 파일에 `schemaVersion` 필드 반드시 존재.
- **Session 휘발성**: Play 종료 후 재진입 시 Session 슬롯 비어있음.
