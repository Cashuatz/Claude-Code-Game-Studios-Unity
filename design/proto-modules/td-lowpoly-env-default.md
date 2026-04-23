# Module: td-lowpoly-env-default

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q8-C (혼합 — 캐릭터 2D / 배경 3D) 의 배경 로우폴리 번들.

## 2. Hard Rules

- 배경 요소만. 인터랙션 없음.
- Static 플래그 ON (light baking 대비).

## 3. Public API

해당 없음.

## 4. Dependencies

- Required: `td-sprite-pack-default` (캐릭터 측).

## 5. Default Prefabs/Assets

- 프리팹: `TD_Env_Ground.prefab`, `TD_Env_Rocks.prefab`, `TD_Env_Trees.prefab`.
- 머티리얼: `M_Env_Grass.mat`, `M_Env_Stone.mat`.

## 6. Skill Hook

`/proto-td-art mixed`:

1. 배경 프리팹 생성.
2. 지형에 산재 배치 (레벨 템플릿과 통합).

## 7. Verification

- 배경이 카메라 frustum 내 정상 렌더.
- Static batching 적용 확인.
