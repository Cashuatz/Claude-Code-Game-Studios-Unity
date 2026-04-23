# Module: env-bush-billboard

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md) / [`shader-fx-kit.md`](./shader-fx-kit.md)
>
> **장르 적용**: turn3d ✅ (환경 러프) / td ✅ (환경 데코) / rail-shooter ✅ (길가 덤불)

## 1. Purpose

**임포스터 방식의 부쉬(수풀·덤불) 렌더링** — 한 덩어리 오브젝트가 여러 잎 카드의 집합처럼 보이되,
라이팅은 **구 형태의 볼륨감**을 유지하도록 한다.

핵심 원리:
1. 구 표면에 N 개의 쿼드 센터를 피보나치 분포로 배치.
2. 각 쿼드의 4 버텍스는 **모두 같은 월드 위치(센터)에 겹친 상태**로 저장한다.
3. UV0 은 `(0,0)~(1,1)` 풀 셀 (한 셀짜리 트림시트 느낌) — 잎 클러스터 텍스처 샘플링용.
4. UV1 은 `(-0.5, -0.5)~(0.5, 0.5)` 로컬 2D 오프셋 — 빌보드 확장용.
5. 버텍스 셰이더가 카메라 right/up 으로 센터에서 확장 → **퍼-쿼드 빌보드**.
6. NORMAL 은 `center.normalized` (구의 외향 노멀) 로 세팅 → 확장 후에도 보존 →
   라이팅은 **매끈한 구**처럼 계산.

결과: 비주얼은 잎 카드 집합, 셰이딩은 구 — The Last of Us / Witcher 류 수풀.

## 2. Hard Rules

- **HR-12 (제안, Proto 완화)** 준수: Surface Shader / built-in 금지.
  URP HLSL (`Tags { "RenderPipeline" = "UniversalPipeline" }` + URP Core.hlsl 포함) 로 작성.
- **퍼-쿼드 빌보드 수학 고정**:
  - 카메라 right/up 을 월드에서 추출: `UNITY_MATRIX_I_V._m00_m10_m20` (right),
    `UNITY_MATRIX_I_V._m01_m11_m21` (up).
  - 확장: `worldPos = centerWS + camRightWS * uv1.x * _QuadSize + camUpWS * uv1.y * _QuadSize`.
- **노멀 보존 절대 원칙**:
  - 버텍스 셰이더가 메시의 `NORMAL` 을 세팅된 값 그대로 `TransformObjectToWorldNormal` 로 변환만 하고
    **변형/재계산 금지**.
  - 빌보딩은 위치만 변형, 노멀은 손대지 않는다.
- **메시 구조 고정**:
  - 버텍스 수 = `quadCount * 4`.
  - 각 쿼드의 4 버텍스는 **동일한 position / normal** 을 가진다 (UV 만 다름).
  - 삼각형 인덱스: `(v0, v2, v1), (v0, v3, v2)` 순서.
- **알파 테스트** (투명 블렌딩 금지):
  - `Queue = "AlphaTest"`, `ZWrite On`, `clip(tex.a - _AlphaCutoff)`.
  - 반투명 블렌드는 OIT 이슈·정렬 문제로 Proto 스코프 밖.
- **Cull Off** — 쿼드가 카메라를 향해 돌아와도 후면 컬 방지 (양면 보임 보장).
- **윈드 스웨이**는 **쿼드 상단만** 흔든다 (`uv1.y + 0.5` 를 가중치로 사용 → 하단 고정).
- **머티리얼 파라미터 네이밍** (확장 금지):
  ```
  _BaseMap, _BaseColor,
  _QuadSize, _AlphaCutoff,
  _AmbientColor, _BacklightColor, _BacklightIntensity,
  _WindStrength, _WindSpeed
  ```

## 3. Public API

```csharp
namespace Proto.Environment.Bush
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BushBillboard : MonoBehaviour
    {
        public int   quadCount;         // 4~256
        public float sphereRadius;      // 구 반지름 (쿼드 센터 분포 반경)
        public float quadSize;          // 쿼드 변 길이 (월드)
        public Material materialOverride;
        public Color baseColor;
        public Color ambientColor;
        public float backlightIntensity;

        // 재빌드. 파라미터 바꾼 뒤 호출. 런타임/에디터 모두 가능.
        public void Build();

        // 리프 텍스처 프로시저 생성 (RGBA, 중앙 방사형 알파 + Perlin 경계).
        public static Texture2D BuildLeafTexture(int size);
    }
}
```

## 4. Dependencies

- **Required**: URP (Universal Render Pipeline) 패키지, Unity 6 이상.
- **Optional**: `shader-fx-kit` (디졸브 등장·퇴장 레시피와 조합 가능 — 나무 제거 연출).

## 5. Default Prefabs / Assets

```
Assets/Proto/VFX/Environment/
└── Shaders/
    └── ProtoEnv_BushBillboard.shader

Assets/Proto/Runtime/Environment/Bush/
├── BushBillboard.cs       ← 메시 프로시저 + 머티리얼 프로비저닝 + 리프 텍스처 생성
└── BushDemo.cs            ← 비교 데모 (Lit 구 1 + 부쉬 3종)

Assets/Proto/Scenes/
└── BushDemo.unity         ← 검증용 씬

(선택) 런타임 외 에셋 파일:
Assets/Proto/VFX/Environment/Materials/M_Env_BushBillboard.mat
Assets/Proto/VFX/Environment/Textures/T_Env_LeafCluster.png
```

## 6. Skill Hook

**`/proto-env-bush`** (인자: `default | high-density | tall-grass | dry-bush | demo`)

1. `Assets/Proto/VFX/Environment/Shaders/ProtoEnv_BushBillboard.shader` 설치.
2. `Assets/Proto/Runtime/Environment/Bush/BushBillboard.cs` 설치.
3. 인자별 프리셋 프리팹 생성:
   - `default`: q=48 r=0.9 size=0.5 (녹색)
   - `high-density`: q=120 r=0.8 size=0.35 (울창)
   - `tall-grass`: q=24 r=0.4 size=0.7 (드문드문)
   - `dry-bush`: q=40 r=0.7 size=0.5 (갈색 톤 오버라이드)
4. 인자가 `demo` 면 `BushDemo.unity` + `BushDemo.cs` 설치 후 Play → 스크린샷.

## 7. Verification

- **컴파일**: C# / 셰이더 에러 0 (`read_console(types=["error"])`).
- **메시 구조 검증**:
  - `mesh.vertexCount == quadCount * 4`
  - `mesh.normals` 의 각 4-버텍스 그룹은 동일한 값 (구 외향 노멀)
  - `mesh.uv[v]` 4 개 패턴 `(0,0)(1,0)(1,1)(0,1)`
  - `mesh.uv2[v]` 4 개 패턴 `(-.5,-.5)(.5,-.5)(.5,.5)(-.5,.5)`
- **런타임 스모크** (`BushDemo.unity`):
  - Play → 3 초 후 스크린샷 확인.
  - 비교용 Lit 구와 병치하여 **전체 명암 분포가 비슷해야** 한다 (구 노멀 라이팅 근거).
  - 카메라가 궤도 회전해도 쿼드는 **항상 카메라를 향해 정면** 유지.
- **VKL Oracle**:
  - `OR-BUSH-01` — 셰이더 컴파일
  - `OR-BUSH-02` — 메시 구조 검사 (vertex count / uv 패턴)
  - `OR-BUSH-03` — 시각 비교 (Lit 구 대비 명암 차이 ≤ 30% — 주관적 판정, 기록용)

## 8. 기법 요점 정리 (구현 힌트)

### 8.1 메시 빌드 요지 (C# 의사코드)
```csharp
for (int i = 0; i < quadCount; i++)
{
    var center = FibonacciSphere(i, quadCount) * radius;
    var normal = center.normalized;
    int v = i * 4;
    verts[v..v+4] = { center, center, center, center };
    norms[v..v+4] = { normal, normal, normal, normal };
    uv0  [v..v+4] = { (0,0), (1,0), (1,1), (0,1) };
    uv1  [v..v+4] = { (-.5,-.5), (.5,-.5), (.5,.5), (-.5,.5) };
    tris       ..= { v+0, v+2, v+1,  v+0, v+3, v+2 };
}
```

### 8.2 버텍스 셰이더 요지 (HLSL)
```hlsl
float3 centerWS  = TransformObjectToWorld(IN.positionOS);
float3 camRightWS = normalize(UNITY_MATRIX_I_V._m00_m10_m20);
float3 camUpWS    = normalize(UNITY_MATRIX_I_V._m01_m11_m21);

float3 worldPos = centerWS
                + camRightWS * IN.uv1.x * _QuadSize
                + camUpWS    * IN.uv1.y * _QuadSize;

OUT.positionHCS = TransformWorldToHClip(worldPos);
OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);  // ★ 구 노멀 유지
```

### 8.3 프래그먼트 셰이더 요지
```hlsl
half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
clip(tex.a - _AlphaCutoff);

Light L = GetMainLight();
half halfLambert = saturate(dot(IN.normalWS, L.direction)) * 0.5 + 0.5;
half3 lit = tex.rgb * _BaseColor.rgb * (L.color * halfLambert + _AmbientColor.rgb);
return half4(lit, 1);
```

### 8.4 왜 half-Lambert 인가
- 부쉬·나뭇잎은 실제로 서브서퍼스·광산란이 강해 순수 Lambert(`max(0, NoL)`) 로 하면
  그림자 쪽이 너무 죽는다.
- `NoL * 0.5 + 0.5` 는 어두운 쪽에도 노말 방향성을 남기면서 밝은 쪽 피크를 낮춘다 → 잎의 "부드러운" 느낌.

### 8.5 성능 노트
- 드로우콜 1개 / 부쉬 (모두 단일 mesh + 단일 머티리얼).
- 버텍스 수 = quadCount × 4. 128 버쉬 × 50 쿼드 = 25,600 버텍스 — 모바일 저사양에서도 무난.
- 잎 텍스처 하나 공유 권장. 변형은 `_BaseColor` 로 오버라이드.

## 9. 확장 아이디어 (TBD)

- **LOD 자동화**: 카메라 거리별 `quadCount` 자동 축소 (16→8→4→1).
- **우드 컴포넌트 결합**: 고정된 Lit 트렁크(줄기) + 빌보드 크라운(수관) 합성.
- **배치 인스턴싱**: `Graphics.DrawMeshInstanced` 로 1 드로우콜에 수백 부쉬.
- **노멀 지터**: 각 쿼드의 노멀을 구 노멀 + 작은 무작위 회전으로 변주 → 라이팅에 잡음 추가.
- **셰도우 캐스터 패스**: 이중 패스 (빌보드 확장 동일 + 알파 클립) — 현재는 생략.

> 재방문 조건: 실 레벨에서 수풀이 눈에 띄게 평면적으로 보이거나 성능 이슈가 생기는 시점.
