# Module: post-process-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ (Ult PP, 테마 PP) / td ✅ (저체력, 게임오버) / rail-shooter ✅ (불릿타임, 피격, 테마 PP)

## 1. Purpose

URP **Post-Processing 커스텀 패스 + VolumeProfile 레시피** 공용 라이브러리.
장르별 PP 효과(불릿타임 블루톤, Ult 레이디얼 블러, 저HP 적색 펄스 등) 를
단일 컨트롤러 API 로 블렌딩/토글한다.

## 2. Hard Rules

- **HR-13 (제안)** 강제: PP 효과는 **반드시 `PostProcessController`** 경유.
  스크립트가 `Volume.weight` / `VolumeProfile` 직접 수정 금지.
- URP 설정: Forward Renderer + HDR + Bloom Enabled. 변경 시 재검증.
- 전역 `Volume` 1개 (`PostProcessRoot.prefab` 내). 로컬 Volume 은 본 모듈 외부에서만 생성.
- 프로파일 단위 **가중치 블렌드** (0~1). 즉시 토글 금지, 반드시 `SetWeight(id, w, durationSec)` 사용.
- 동시 활성 프로파일 최대 **4개** (성능 버짓). 초과 시 가장 오래된 프로파일 자동 페이드 아웃.

## 3. Public API

```csharp
namespace Proto.Shared.PostFx
{
    public static class PostProcessController
    {
        public static void SetWeight(string profileId, float weight, float durationSec = 0.3f);
        public static float GetWeight(string profileId);
        public static void ClearAll(float fadeOutSec = 0.5f);
        public static bool IsActive(string profileId);
        public static event Action<string, float> OnWeightChanged;
    }

    // 레시피 등록
    [CreateAssetMenu]
    public class PostFxRecipe : ScriptableObject
    {
        public string Id;                 // "pp-bullettime", "pp-ult-active" 등
        public VolumeProfile Profile;     // URP VolumeProfile 에셋
        public float DefaultBlendIn = 0.3f;
        public float DefaultBlendOut = 0.5f;
        [Range(0, 1)] public float MaxWeight = 1f;
    }

    [CreateAssetMenu]
    public class PostFxLibrary : ScriptableObject
    {
        public List<PostFxRecipe> Recipes;
    }
}
```

## 4. Dependencies

- **Required**: URP (Universal Render Pipeline), Volume 시스템.
- **Optional**: `shader-fx-kit` (엣지 글로우 상호작용), `sound-kit` (PP 토글 시 스팅어).

## 5. Default Prefabs / Assets

- `PostProcessRoot.prefab` — Volume + Camera + 기본 Forward Renderer 참조.
- `PostFxLibrary_Default.asset` — 전체 레시피 등록 SO.
- 공용 레시피 (5개):
  - `PP_BulletTime.asset` / VolumeProfile: 블루톤 + 채도 감소 + 속도선 블러
  - `PP_UltActive.asset` / VolumeProfile: 레이디얼 블러 + 비네트 + 채도 튕김
  - `PP_LowHP.asset` / VolumeProfile: 적색 펄스 + 비네트
  - `PP_HitFlash.asset` / VolumeProfile: 단발 적색 플래시 (0.2s)
  - `PP_GrayscaleOver.asset` / VolumeProfile: 그레이스케일 페이드
- Rail-shooter 테마 레시피:
  - `PP_Horror.asset` (녹/청록 + 그레인 + 가장자리 어둠)
  - `PP_Cyber.asset` (네온 + 스캔라인 + 크로매틱)
  - `PP_Weird.asset` (색 왜곡 + 낮은 대비 + 블러)
- Turn3d 테마 레시피:
  - `PP_Pastel.asset` (밝은 채도 + 소프트 블룸 + 따뜻한 화이트 밸런스)
  - `PP_Dark.asset` (낮은 채도 + 진한 비네트 + 차가운 그림자)
  - `PP_Retro.asset` (픽셀화 + 제한 팔레트 + 디더링)

## 6. Skill Hook

`/proto-postfx` (인자: 레시피 ID 하나 또는 `all`):

1. `Assets/Proto/Runtime/Shared/PostFx/` 에 스크립트 복사.
2. `Assets/Proto/VFX/Shared/PP/` 에 해당 VolumeProfile 및 `PostFxRecipe.asset` 생성.
3. `PostFxLibrary_Default.asset` 에 레시피 등록.
4. `PostProcessRoot.prefab` 생성 (Volume + 라이브러리 참조), 씬 배치.
5. 검증: Play 진입 후 `PostProcessController.SetWeight("<id>", 1f)` → 화면 변화 시각 확인.

## 7. Verification

- **컴파일**: 0 에러.
- **URP 설정**: `UniversalRenderPipelineAsset` 존재, Forward Renderer + HDR ON.
- **Volume 존재**: 씬에 `PostProcessRoot` 1개.
- **레시피 등록**: `PostFxLibrary_Default.asset` 에 요청된 레시피 포함.
- **블렌드 동작**: `SetWeight("pp-bullettime", 1f, 0.3f)` → 0.4초 후 `GetWeight == 1f` 근사값.
- **최대 4개 제한**: 5번째 레시피 활성화 시 가장 오래된 것 자동 페이드 아웃 (로그 출력).
