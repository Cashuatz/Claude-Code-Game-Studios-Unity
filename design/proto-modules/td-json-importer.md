# Module: td-json-importer

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

JSON (웹 에디터 1차 소스) → ScriptableObject 변환. 레벨/웨이브/타워/적 데이터를
Unity 런타임이 소비할 수 있게 빌드타임 임포트한다.

## 2. Hard Rules

- TD HR-TD-4 강제: JSON 이 원본, SO 는 파생물.
- 스키마 검증 실패 시 임포트 전면 중단. 부분 적용 금지.
- `schemaVersion` 필드 필수 — 버전 불일치 시 에러 (자동 마이그레이션 없음).
- 임포트 결과 파일은 `Assets/Proto/Data/TD/<kind>/` 에만 생성.

## 3. Public API

```csharp
namespace Proto.TD.Data
{
    public static class TdJsonImporter
    {
        public static LevelSO ImportLevel(string jsonPath);
        public static WaveSO ImportWave(string jsonPath);
        public static TowerSO ImportTower(string jsonPath);
        public static EnemySO ImportEnemy(string jsonPath);
        public static void ImportAll(string rootDirectory);  // 재귀 임포트

        public static event Action<string> OnImported;        // 파일 경로
        public static event Action<string, string> OnError;   // 파일 경로, 에러 메시지
    }

    [CreateAssetMenu] public class LevelSO : ScriptableObject { /* ... */ }
    [CreateAssetMenu] public class WaveSO : ScriptableObject { /* ... */ }
    [CreateAssetMenu] public class TowerSO : ScriptableObject { /* ... */ }
    [CreateAssetMenu] public class EnemySO : ScriptableObject { /* ... */ }
}
```

## 4. Dependencies

- Required: `td-sim-core` (DTO 재사용).
- Optional: `td-web-bridge` (자동 재임포트).

## 5. Default Prefabs/Assets

- 스크립트: `Assets/Proto/Editor/TD/TdJsonImporter.cs`, `LevelSO.cs`, `WaveSO.cs`, `TowerSO.cs`, `EnemySO.cs`.
- 스키마: `web-sim/schema/level.schema.json` (공유).
- asmdef: `Assets/Proto/Editor/TD/Proto.TD.Editor.asmdef`.

## 6. Skill Hook

`/proto-td-import`:

1. `Editor/TD/TdJsonImporter.cs` 생성 (AssetPostprocessor 패턴).
2. SO 타입 4종 생성.
3. 샘플 JSON 3개 (`level_sample.json`, `wave_sample.json`, `tower_sample.json`) 생성.
4. 임포트 실행 → 대응 SO 생성 확인.
5. 잘못된 JSON (스키마 위반) 임포트 → 에러 이벤트 발행, SO 생성 안 됨.

## 7. Verification

- JSON 샘플 3개 임포트 성공.
- 스키마 버전 틀린 JSON → 에러.
- 생성된 SO 에셋이 `Data/TD/<kind>/` 에 존재.
