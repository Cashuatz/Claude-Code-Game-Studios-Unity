# Module: shader-fx-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ (소환/이탈/피격) / td ✅ (건설/파괴/피격) / rail-shooter ✅ (적 등장/사망/피격)

## 1. Purpose

URP 기반 **공용 셰이더 이펙트 라이브러리**.
모든 장르가 **등장/사망/피격/전환/선택** 연출에 동일한 셰이더·머티리얼·트윈 API 를 재사용한다.

**수록 레시피 (6종)**:

| ID | 용도 | 주 파라미터 |
|---|---|---|
| `dissolve` | 등장·퇴장·디졸브 전환 | `_DissolveAmount`, `_DissolveNoise`, `_EdgeColor`, `_EdgeWidth` |
| `rim-light` | 선택·호버·강조 | `_RimColor`, `_RimPower`, `_RimIntensity` |
| `hologram` | 유닛 프리뷰·소환 전 상태·AR 느낌 | `_HoloColor`, `_ScanlineSpeed`, `_ScanlineDensity`, `_Alpha` |
| `scanline` | CRT·사이버 오버레이·피격 정지 | `_ScanlineColor`, `_ScanlineSpeed`, `_ScanlineDensity`, `_Glitch` |
| `hit-flash` | 피격·데미지 번쩍 | `_FlashColor`, `_FlashAmount` |
| `outline` | 선택·아웃라인·강조 (포스트 X, 오브젝트 단위) | `_OutlineColor`, `_OutlineWidth` |

각 레시피는 **독립 `.shader` 파일** 로 제공. 필요하면 레시피 조합(예: dissolve + rim-light) 은 **머티리얼 전환** 으로 해결. 한 셰이더에 모든 기능 혼합 금지.

## 2. Hard Rules

- **HR-12 (제안, Proto 완화)** — Surface Shader / built-in 셰이더 작성 **절대 금지**.
  모든 커스텀 셰이더는 **URP 호환 HLSL (`.shader`)** 또는 **Shader Graph (`.shadergraph`)**.
  - Proto 단계 에서는 프로그래매틱 `.shadergraph` 작성이 비현실적이므로
    **URP HLSL** 을 임시 허용. 실 프로덕션 진입 시 Shader Graph 로 이관 가능성 검토.
  - `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"` 필수.
  - `Tags { "RenderPipeline" = "UniversalPipeline" }` 필수.
- **파라미터 네이밍 고정** — 아래 표준 상수 이름만 사용. 새 파라미터는 이 문서에 등록 후 사용.
  ```
  // dissolve
  _DissolveAmount, _DissolveNoise, _EdgeColor, _EdgeWidth
  // rim-light
  _RimColor, _RimPower, _RimIntensity
  // hologram
  _HoloColor, _ScanlineSpeed, _ScanlineDensity, _Alpha
  // scanline
  _ScanlineColor, _ScanlineSpeed, _ScanlineDensity, _Glitch
  // hit-flash
  _FlashColor, _FlashAmount
  // outline
  _OutlineColor, _OutlineWidth
  ```
- **머티리얼 인스턴스 금지** — 런타임 파라미터 조작은 **`MaterialPropertyBlock` (MPB) 경유**.
  `renderer.material` 직접 접근·대입 금지 (메모리 누수).
- **트윈은 자체 구현** — DOTween 등 외부 의존 금지. `ShaderFloatTween` (Coroutine) 사용.
- **Ease 강제** — 모든 수치 애니메이션은 `Linear 금지`, 기본 `EaseOutQuad`.
- **Time.timeScale 영향 분리** — 트윈은 `Time.unscaledDeltaTime` 사용 (HR-5 time-scale 모듈 격리).
- **Z-write / Blend 명시** — 투명 계열(디졸브 퇴장, 홀로그램)은 `Blend SrcAlpha OneMinusSrcAlpha`,
  `ZWrite Off`. 불투명은 `ZWrite On`. 셰이더 헤더에 반드시 명시.
- **양면 렌더링 선택적** — 홀로그램·스캔라인은 `Cull Off` 허용, 기타는 `Cull Back` 기본.
- **싱글 패스 원칙** — 아웃라인은 2-pass 허용(백페이스 확장 + 원본). 나머지는 1-pass.

## 3. Public API

```csharp
namespace Proto.Shared.ShaderFx
{
    /// <summary>
    /// 레시피 ID. 각 값은 대응 셰이더/머티리얼과 1:1.
    /// </summary>
    public enum ShaderFxRecipe
    {
        DissolveIn,         // 0 → 1 (등장)
        DissolveOut,        // 1 → 0 (퇴장)
        DissolveEdgeGlow,   // 엣지 발광 강화 버전 (섹션 전환)
        RimLight,           // 선택·호버
        Hologram,           // 유닛 프리뷰
        Scanline,           // CRT 오버레이
        HitFlash,           // 피격 번쩍
        Outline,            // 아웃라인 강조
    }

    public static class ShaderFxHub
    {
        /// <summary>레시피를 렌더러에 적용. 완료 시 onComplete 호출.</summary>
        public static Coroutine Apply(
            Renderer target,
            ShaderFxRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null);

        /// <summary>다중 렌더러(SkinnedMesh+무기+장신구) 동시 적용.</summary>
        public static Coroutine ApplyGroup(
            IReadOnlyList<Renderer> targets,
            ShaderFxRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null);

        /// <summary>수동 제어: 매 프레임 진행도(0~1) 세팅.</summary>
        public static void SetProgress(
            Renderer target,
            ShaderFxRecipe recipe,
            float progress01);

        /// <summary>레시피 머티리얼을 가져온다 (메모리 풀, MPB 대상 렌더러 재할당 시 사용).</summary>
        public static Material GetSharedMaterial(ShaderFxRecipe recipe);

        /// <summary>
        /// 레시피에 연결된 기본 파라미터(색상/강도)를 덮어쓴다.
        /// 예: HitFlash 의 _FlashColor 를 피격 타입별로 바꿀 때.
        /// </summary>
        public static void SetParam(
            Renderer target,
            ShaderFxRecipe recipe,
            int shaderPropertyId,
            float value);

        public static void SetParam(
            Renderer target,
            ShaderFxRecipe recipe,
            int shaderPropertyId,
            Color value);
    }

    public static class ShaderFloatTween
    {
        public static Coroutine Tween(
            MonoBehaviour host,
            Renderer target,
            int propertyId,
            float from,
            float to,
            float duration,
            Func<float, float> easing = null,
            Action onComplete = null);
    }

    /// <summary>표준 Ease 함수 모음.</summary>
    public static class Easings
    {
        public static float Linear(float t);
        public static float EaseOutQuad(float t);
        public static float EaseInQuad(float t);
        public static float EaseInOutQuad(float t);
        public static float EaseOutCubic(float t);
        public static float EaseOutElastic(float t);
    }
}
```

## 4. Dependencies

- **Required**: URP (Universal Render Pipeline) 패키지.
- **Optional**:
  - `sound-kit` — 디졸브/히트플래시 사운드 연동.
  - `post-process-kit` — 아웃라인·림 라이트가 블룸과 상호작용.

## 5. Default Prefabs / Assets

```
Assets/Proto/VFX/Shared/
├── Shaders/
│   ├── ProtoFx_Dissolve.shader
│   ├── ProtoFx_RimLight.shader
│   ├── ProtoFx_Hologram.shader
│   ├── ProtoFx_Scanline.shader
│   ├── ProtoFx_HitFlash.shader
│   └── ProtoFx_Outline.shader
├── Materials/
│   ├── M_Fx_Dissolve.mat
│   ├── M_Fx_RimLight.mat
│   ├── M_Fx_Hologram.mat
│   ├── M_Fx_Scanline.mat
│   ├── M_Fx_HitFlash.mat
│   └── M_Fx_Outline.mat
├── Textures/
│   ├── T_DissolveNoise_01.png   ← 256x256 타일형 Perlin 노이즈
│   └── T_DissolveNoise_02.png   ← 256x256 유기체 느낌
└── ShaderFxConfig.asset         ← 레시피별 기본 duration/easing/색상 프리셋 SO

Assets/Proto/Runtime/Shared/ShaderFx/
├── ShaderFxHub.cs
├── ShaderFxRecipe.cs             ← enum
├── ShaderFloatTween.cs
├── Easings.cs
└── ShaderFxConfig.cs             ← ScriptableObject
```

## 6. Skill Hook

**`/proto-shader-fx`** (인자: `dissolve | rim-light | hologram | scanline | hit-flash | outline | all | demo`)

1. `Assets/Proto/Runtime/Shared/ShaderFx/` 에 C# 스크립트 복사.
2. `Assets/Proto/VFX/Shared/Shaders/` 에 대응 `.shader` 생성.
3. `Assets/Proto/VFX/Shared/Materials/` 에 `.mat` 생성 (셰이더 할당 + 기본값).
4. `Assets/Proto/VFX/Shared/Textures/` 에 노이즈 텍스처 배치 (디졸브만).
5. `ShaderFxConfig.asset` 생성.
6. 인자가 `demo` 또는 `all` 이면 `Assets/Proto/Runtime/Shared/ShaderFx/ShaderFxDemo.cs` + 씬 구성:
   - 빈 GameObject `ShaderFxDemo` 에 `ShaderFxDemo` 컴포넌트.
   - 큐브 6개를 가로로 배치 (각 레시피 1개).
   - 3초마다 순차 발동, 무한 루프.
7. Play 검증: `manage_editor(play)` → 3초 대기 → 스크린샷 → `stop`.

## 7. Verification

- **컴파일**: C# / 셰이더 모두 에러 0 (`read_console(types=["error"])`).
- **셰이더 파일 존재**: 6개 `.shader`, 6개 `.mat` 가 정상 경로에 위치.
- **머티리얼 파라미터**: 각 `.mat` 가 2.Hard Rules 의 "파라미터 네이밍 고정" 표의 필드 보유.
- **런타임 스모크**:
  - 큐브 6개 생성 후 `ShaderFxHub.Apply(cube[i], recipe[i], 1f)` → 1초 후 각 효과 가시 변화.
  - 재실행 시 결과 동일 (idempotent).
- **메모리 누수**:
  - `Application.isPlaying=true` 상태에서 10회 반복 적용 후 `Resources.FindObjectsOfTypeAll<Material>().Length` 증가 없음 ±2.
  - `renderer.material` 호출 정적 검색 0건 (`Proto.Shared.ShaderFx` asmdef 내).
- **VKL Oracle**: `OR-SHADERFX-01`(컴파일) / `OR-SHADERFX-02`(파라미터 표준) / `OR-SHADERFX-03`(MPB 경유) 부착.
  FT-XX 분류 없이 PASS 금지.

## 8. 레시피 상세 (구현 힌트)

### 8.1 Dissolve

```
_DissolveAmount(0~1) 값이 텍스처 샘플 값보다 크면 discard.
엣지: abs(sample - amount) < _EdgeWidth 범위에 _EdgeColor 를 additive blend.
```

### 8.2 Rim Light

```
float rim = 1.0 - saturate(dot(normalWS, viewDirWS));
rim = pow(rim, _RimPower) * _RimIntensity;
finalColor = baseColor + _RimColor.rgb * rim;
```

### 8.3 Hologram

```
float scan = sin(positionOS.y * _ScanlineDensity + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
float rim = 1.0 - saturate(dot(normalWS, viewDirWS));
alpha = _Alpha * lerp(0.3, 1.0, rim) * lerp(0.8, 1.0, scan);
finalColor = _HoloColor.rgb;
Blend SrcAlpha One; ZWrite Off; Cull Off;
```

### 8.4 Scanline

```
float line = step(0.5, frac(positionCS.y / _ScreenParams.y * _ScanlineDensity - _Time.y * _ScanlineSpeed));
float glitch = (sin(_Time.y * 37.0) * 0.5 + 0.5) * _Glitch;
finalColor = lerp(baseColor, _ScanlineColor.rgb, line * (0.2 + glitch));
```

### 8.5 Hit Flash

```
finalColor = lerp(baseColor, _FlashColor.rgb, _FlashAmount);   // 0=원본, 1=플래시색
```
Ease: `EaseOutCubic` 로 `_FlashAmount` 를 1 → 0 감쇠 (권장 150ms).

### 8.6 Outline

2-pass:
- **Pass 1**: 백페이스 확장. `positionOS += normalOS * _OutlineWidth;` → `_OutlineColor` 단색.
- **Pass 2**: 원본 Lit 렌더.

## 9. 사용 예시 (이터레이션 단계용)

```csharp
// 유닛 등장
ShaderFxHub.Apply(unit.Renderer, ShaderFxRecipe.DissolveIn, 0.8f);

// 피격
ShaderFxHub.Apply(enemy.Renderer, ShaderFxRecipe.HitFlash, 0.15f);

// 선택 하이라이트 (수동 on/off)
ShaderFxHub.SetProgress(tower.Renderer, ShaderFxRecipe.RimLight, 1f);  // 켜기
ShaderFxHub.SetProgress(tower.Renderer, ShaderFxRecipe.RimLight, 0f);  // 끄기

// 유닛 프리뷰 (배치 전)
previewRenderer.sharedMaterial = ShaderFxHub.GetSharedMaterial(ShaderFxRecipe.Hologram);
```
