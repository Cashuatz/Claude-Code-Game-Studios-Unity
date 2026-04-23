# Module: td-level-template-small

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q7-A (Small) 레벨 템플릿. 20x20 유닛 크기, 한 화면 조망, 학습용.

## 2. Hard Rules

- 공통 HR-2 (단일 씬), HR-8 (SO 기반), HR-TD-10 (JSON 원본).
- 최소 경로 1개, 발전소 스폰 포인트 1개, 기지 1개.
- 맵 바운더리 외 허브 배치 금지 (`td-placement-*` 에서 검증).

## 3. Public API

```csharp
namespace Proto.TD.Level
{
    public static class SmallLevelTemplate
    {
        public const int GridSize = 20;
        public const float CellSize = 1f;
        public static LevelSO CreateDefault();
    }
}
```

## 4. Dependencies

- Required: `td-json-importer` (LevelSO 로드).
- Optional: `td-placement-grid` (셀 배치 연동).

## 5. Default Prefabs/Assets

- JSON: `Assets/Proto/Data/TD/Levels/Level_Small_Default.json` (20x20, 경로 1개 5웨이포인트).
- SO: `Level_Small_Default.asset` (임포트 후 생성).
- 프리팹: `TD_Level_Small.prefab` (지형 메시 + 기준점).

## 6. Skill Hook

`/proto-td-level small`:

1. `Level_Small_Default.json` 생성 (20x20, 1 경로, 발전소 1, 기지 1).
2. 지형 프리팹 생성 (plane 20x20, URP Lit 머티리얼).
3. JSON 임포트 → `Level_Small_Default.asset`.
4. 씬에 템플릿 인스턴스 배치.
5. 카메라 초기 위치·줌 설정 (맵 전체 조망).

## 7. Verification

- JSON/SO 존재.
- 씬 로드 시 20x20 지형 표시.
- 카메라 frustum 에 맵 전체 포함.
