# Module: shader-fx-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ (소환/이탈 디졸브) / td ✅ (건설/파괴) / rail-shooter ✅ (적 등장/사망)

## 1. Purpose

URP Shader Graph 기반 **디졸브 및 일반 셰이더 이펙트** 공용 라이브러리.
각 장르가 등장/사망/전환 연출에 동일한 머티리얼과 트윈 API 를 재사용하도록 한다.

## 2. Hard Rules

- **HR-12 (제안)** 강제: Surface Shader / built-in 셰이더 작성 금지. 모든 커스텀 셰이더는 **URP Shader Graph** 소스.
- Shader 상수 이름 고정:
  - `_DissolveAmount` (0~1), `_DissolveNoise` (Tex), `_EdgeColor` (Color), `_EdgeWidth` (0~0.2).
- 머티리얼 인스턴스 관리: **`MaterialPropertyBlock` 만 사용**. `renderer.material` 인스턴스 생성 금지 (메모리 누수 방지).
- 트윈은 외부 의존(DOTween 등) 금지. 자체 `ShaderFloatTween` 사용 — `Coroutine` 기반.
- 디졸브 애니메이션은 **항상 ease 포함** (Linear 금지) — 느낌 차이가 큼. Default: `EaseOutQuad`.

## 3. Public API

```csharp
namespace Proto.Shared.ShaderFx
{
    public enum DissolveRecipe
    {
        DissolveIn,        // 0 → 1 (등장)
        DissolveOut,       // 1 → 0 (퇴장)
        DissolveEdgeGlow,  // 양방향 + 강한 엣지 발광 (섹션 전환)
    }

    public static class ShaderFxHub
    {
        // 렌더러에 디졸브 적용. 완료 콜백 가능.
        public static Coroutine ApplyDissolve(
            Renderer target,
            DissolveRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null
        );

        // 다중 렌더러 (SkinnedMesh + 무기 + 장신구 등) 동시 적용
        public static Coroutine ApplyDissolveGroup(
            IReadOnlyList<Renderer> targets,
            DissolveRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null
        );

        // 수동 제어: 매 프레임 값 세팅
        public static void SetDissolveValue(Renderer target, float value);
    }

    // 직접 접근용 (특수 케이스)
    public static class ShaderFloatTween
    {
        public static Coroutine Tween(
            MonoBehaviour host,
            MaterialPropertyBlock mpb,
            int propertyId,
            float from, float to,
            float duration,
            Func<float, float> easing = null,
            Action onComplete = null
        );
    }
}
```

## 4. Dependencies

- **Required**: URP (Universal Render Pipeline), Unity Shader Graph 패키지.
- **Optional**: `sound-kit` (디졸브 사운드 연동), `post-process-kit` (엣지 글로우가 PP 과 상호작용).

## 5. Default Prefabs / Assets

- `M_Dissolve_Base.mat` — 기본 디졸브 머티리얼 (불투명 → 노이즈 마스크 → 엣지 컬러).
- `SG_Dissolve_Base.shadergraph` — Shader Graph 소스.
- `T_DissolveNoise_01.png` — 노이즈 텍스처 (256x256, 타일형).
- `T_DissolveNoise_02.png` — 대체 노이즈 (유기체 느낌).
- `ShaderFxConfig.asset` — 기본 duration / easing / 레시피별 EdgeColor 프리셋 SO.

## 6. Skill Hook

`/proto-shader-fx` (인자: `dissolve-in | dissolve-out | dissolve-edge-glow | all`):

1. `Assets/Proto/Runtime/Shared/ShaderFx/` 에 스크립트 복사.
2. `Assets/Proto/VFX/Shared/Shaders/` 에 `SG_Dissolve_Base.shadergraph` 생성 (미존재 시).
3. `Assets/Proto/VFX/Shared/Materials/M_Dissolve_Base.mat` 생성.
4. `Assets/Proto/VFX/Shared/Textures/` 에 노이즈 텍스처 배치 (프리셋 2종).
5. `ShaderFxConfig.asset` 생성.
6. 검증: 큐브에 머티리얼 적용 → `ApplyDissolve(renderer, DissolveOut, 1f)` → 1초 후 큐브 투명.

## 7. Verification

- **컴파일**: 0 에러.
- **Shader Graph 존재**: `SG_Dissolve_Base.shadergraph` 가 `Assets/Proto/VFX/Shared/Shaders/`.
- **머티리얼 파라미터**: `M_Dissolve_Base.mat` 에 `_DissolveAmount` / `_DissolveNoise` / `_EdgeColor` / `_EdgeWidth` 필드 모두 존재.
- **런타임 스모크**: 큐브에 적용 후 Play → `ApplyDissolve(cube, DissolveOut, 1f)` → 스크린샷 0.5s / 1.2s 비교, 투명도 변화 시각 확인.
- **메모리 누수**: `renderer.material` 직접 생성 여부 정적 분석 (`Proto.Shared.ShaderFx` asmdef 내).
