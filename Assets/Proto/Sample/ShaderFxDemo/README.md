# ShaderFxDemo — 셰이더 FX 레시피 허브 데모

공용 `Runtime/Shared/ShaderFx/{ShaderFxHub, ShaderFxRecipe, Easings}` 와 `VFX/Shared/Shaders/*` 6종 (Dissolve / HitFlash / Hologram / Outline / RimLight / Scanline) 을 한 씬에서 토글하며 조합을 실험할 수 있는 데모.

## 구성

```
ShaderFxDemo/
├── Runtime/ShaderFxDemo.cs      — UGUI 토글 + 레시피 파라미터 슬라이더
├── Scenes/ShaderFxDemo.unity    — 참조 Cube/Sphere + 6 FX 토글 UI
└── README.md
```

## 공용 모듈과의 관계

- `Runtime/Shared/ShaderFx/ShaderFxHub.cs` (공용) — 여러 레시피를 매터리얼 1개에 쌓는 관리자.
- `Runtime/Shared/ShaderFx/ShaderFxRecipe.cs` (공용) — 레시피 추상 (파라미터 + 타겟 프로퍼티).
- `Runtime/Shared/ShaderFx/Easings.cs` (공용) — 시간 커브 세트 (Cubic/Quint/Bounce 등).
- `VFX/Shared/Shaders/Proto_Fx_*.shader` (공용) — 각 FX 셰이더.
- 이 Sample 의 `ShaderFxDemo.cs` 는 런타임 UI 로 레시피 활성/비활성 + 파라미터 실시간 조절.

## 실행 방법

1. `ShaderFxDemo.unity` → Play.
2. 화면 좌측 UI 에서 FX 체크박스를 토글하며 Cube/Sphere 에 어떻게 올라가는지 확인.
3. 슬라이더로 파라미터 (Outline Width, Scanline Freq 등) 조정.

## 활용 포인트

- 피격 / 홀로그램 / 아웃라인 같은 자주 쓰는 시각 효과의 코드 경로 확인.
- 신규 FX 를 `ShaderFxRecipe` 상속으로 추가하는 방법 템플릿.
- 다른 Sample 씬이 FX 를 넣을 때 이 허브 API 만 호출하면 됨.
