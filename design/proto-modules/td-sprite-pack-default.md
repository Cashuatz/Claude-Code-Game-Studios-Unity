# Module: td-sprite-pack-default

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q8-A (2D 도트/빌보드). 적/타워 2D 스프라이트 번들.

## 2. Hard Rules

- `character-render-kit` billboard 모드 사용.
- 모든 스프라이트는 동일 pixels-per-unit (기본 32).

## 3. Public API

해당 없음 (데이터 번들).

## 4. Dependencies

- Required: `character-render-kit`.

## 5. Default Prefabs/Assets

- 번들 SO: `TD_Sprites_Default.asset`.
- 프리팹: `TD_Enemy_BillboardBasic.prefab`, `TD_Tower_BillboardBasic.prefab`, `TD_Hub_BillboardBasic.prefab`.
- 스프라이트: `Assets/Proto/Art/TD/Sprites/` (3~5 장).

## 6. Skill Hook

`/proto-td-art 2d`:

1. 스프라이트 에셋 임포트 (또는 placeholder 흰색 큐브 스프라이트).
2. 빌보드 프리팹 생성.
3. `character-render-kit` billboard 모드 설정.

## 7. Verification

- 프리팹 씬에 스폰 → 항상 카메라를 향함.
- Z-fighting 없음 (렌더 순서 조정).
