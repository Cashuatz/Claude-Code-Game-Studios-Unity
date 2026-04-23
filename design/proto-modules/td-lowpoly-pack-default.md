# Module: td-lowpoly-pack-default

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q8-B (3D 블록). 적/타워 3D 로우폴리 번들.

## 2. Hard Rules

- `character-render-kit` skinned 모드.
- 트라이앵글 카운트 1000 이하 / 모델.
- 공통 URP Lit 머티리얼 팔레트 3종.

## 3. Public API

해당 없음 (데이터 번들).

## 4. Dependencies

- Required: `character-render-kit`.

## 5. Default Prefabs/Assets

- 번들 SO: `TD_Models_Lowpoly.asset`.
- 프리팹: `TD_Enemy_Lowpoly.prefab`, `TD_Tower_Lowpoly.prefab`, `TD_Hub_Lowpoly.prefab`.
- 메시: `Assets/Proto/Art/TD/Models/` (3~5 모델).

## 6. Skill Hook

`/proto-td-art 3d`:

1. 모델 에셋 임포트 (또는 placeholder 큐브/구).
2. URP Lit 머티리얼 팔레트 3종 생성.
3. 프리팹 조립.

## 7. Verification

- 프리팹 정상 렌더.
- 머티리얼 팔레트 적용.
