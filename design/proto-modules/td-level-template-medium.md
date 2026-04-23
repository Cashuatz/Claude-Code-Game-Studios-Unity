# Module: td-level-template-medium

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q7-B (Medium) 레벨 템플릿. 40x40, 기본값. 미니맵 선택.

## 2. Hard Rules

- `td-level-template-small` 과 동일, 크기만 확대.
- 경로 2개 (Q6-B 가 기본값이므로 사전 제공).

## 3. Public API

```csharp
namespace Proto.TD.Level
{
    public static class MediumLevelTemplate
    {
        public const int GridSize = 40;
        public const float CellSize = 1f;
        public static LevelSO CreateDefault();
    }
}
```

## 4. Dependencies

- Required: `td-json-importer`.
- Optional: `td-minimap` (40x40 은 미니맵 선택), `td-placement-*`.

## 5. Default Prefabs/Assets

- JSON: `Level_Medium_Default.json`.
- SO: `Level_Medium_Default.asset`.
- 프리팹: `TD_Level_Medium.prefab` (plane 40x40).

## 6. Skill Hook

`/proto-td-level medium`:

1. JSON 생성 (40x40, 경로 2개, 발전소 1, 기지 1).
2. 지형 프리팹 생성.
3. JSON 임포트.
4. 씬 배치, 카메라 중앙 정렬.

## 7. Verification

- JSON/SO 존재, 40x40 크기.
- 경로 2개 모두 기지까지 연결됨.
