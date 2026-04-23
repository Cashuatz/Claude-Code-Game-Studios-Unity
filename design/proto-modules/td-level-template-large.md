# Module: td-level-template-large

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q7-C (Large) 레벨 템플릿. 80x80, 미니맵 필수.

## 2. Hard Rules

- `td-minimap` 필수 의존. 설치 시 미니맵 모듈 자동 활성.
- 플레이테스트 Monte Carlo 회수 1/4 감소 (HR-TD-8 성능 조정).

## 3. Public API

```csharp
namespace Proto.TD.Level
{
    public static class LargeLevelTemplate
    {
        public const int GridSize = 80;
        public const float CellSize = 1f;
        public static LevelSO CreateDefault();
    }
}
```

## 4. Dependencies

- Required: `td-json-importer`, **`td-minimap`**.
- Optional: `td-placement-*`.

## 5. Default Prefabs/Assets

- JSON: `Level_Large_Default.json`.
- SO: `Level_Large_Default.asset`.
- 프리팹: `TD_Level_Large.prefab`.

## 6. Skill Hook

`/proto-td-level large`:

1. JSON 생성 (80x80, 경로 3~4개, 발전소 2, 기지 1).
2. 지형 프리팹 생성.
3. 미니맵 모듈 자동 설치 (`/proto-td-minimap` 내부 호출).
4. JSON 임포트.
5. 카메라 초기 줌-아웃 조정.

## 7. Verification

- JSON/SO 존재, 80x80 크기.
- 미니맵 활성.
- 플레이테스트 러너 회수 설정이 기본의 1/4 로 자동 조정됨.
