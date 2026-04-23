# Pattern: URP Render Pipeline Hooks & Dynamic Volume Override

> **분류**: 공용 패턴 문서 (모듈 아님)
> **적용 대상**: URP 기반 모든 장르 (turn3d / td / rail-shooter)
> **연계**: `post-process-kit.md`, `shader-fx-kit.md`, `env-bush-billboard.md`

## 1. 이 패턴이 하는 일

URP 에서 **카메라별 · 프레임별로**:

1. 커스텀 `ScriptableRenderPass` 를 **동적으로 삽입** 한다.
2. 해당 카메라의 `VolumeStack` 에 있는 PP 컴포넌트 값을 **런타임에 오버라이드** 한다.
3. 커스텀 패스에서 **풀스크린 삼각형**을 쏘면 → **커스텀 PP 패스** 가 만들어진다.

**왜 중요한가**:
- PP 효과를 **게임플레이 상태에 동적으로 연동** 가능 (예: 플레이어 피격 시 Bloom 급등, 줌 시 DoF 초점 이동).
- `VolumeProfile.asset` 을 수정하지 않아 **원본 프로파일 오염 없음**.
- 카메라마다 다른 PP 적용 가능 (멀티카메라 UI · 미니맵에 PP 미적용 등).

## 2. 레퍼런스 코드 (사용자 제공, 2026-04-24)

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class Test : MonoBehaviour
{
    [SerializeField] float distance;
    static Transform s_transform;

    void OnEnable()
    {
        s_transform = this.transform;
        RenderPipelineManager.beginCameraRendering  += OnBeginCameraRendering;
        RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering  -= OnBeginCameraRendering;
        RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
    }

    void OnBeginContextRendering(ScriptableRenderContext ctx, System.Collections.Generic.List<Camera> cameras) { }

    void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        cam.GetUniversalAdditionalCameraData()
           .scriptableRenderer
           .EnqueuePass(TempPass.Instance.Value);
    }

    static void Process(in VolumeStack stack)
    {
        if (stack == null) return;

        var dof = stack.GetComponent<UnityEngine.Rendering.Universal.DepthOfField>();
        dof.focusDistance.Override(Vector3.Distance(Camera.main.transform.position, s_transform.position));

        var bloom = stack.GetComponent<UnityEngine.Rendering.Universal.Bloom>();
        bloom.intensity.Override(10000);
        bloom.threshold.Override(0);
    }

    sealed class TempPass : ScriptableRenderPass
    {
        internal static readonly Lazy<TempPass> Instance = new(() => new TempPass());

        public TempPass() { renderPassEvent = RenderPassEvent.BeforeRendering; }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            var acd = data.cameraData.camera.GetUniversalAdditionalCameraData();
            Process(acd.volumeStack);
        }
    }
}
```

## 3. 3단 콤보 요지

### 3.1 카메라별 훅
```csharp
RenderPipelineManager.beginCameraRendering += (ctx, cam) => { ... };
```
- **Per-camera** 호출. 카메라마다 로직 분기 가능.
- **카메라 렌더 직전** 이므로 EnqueuePass 타이밍으로 적합.
- 반드시 `-=` 로 해제 (OnDisable · OnDestroy).

### 3.2 커스텀 패스 삽입
```csharp
cam.GetUniversalAdditionalCameraData()
   .scriptableRenderer
   .EnqueuePass(myPassInstance);
```
- `UniversalAdditionalCameraData` — URP 가 카메라마다 자동 부착하는 확장 데이터.
- `scriptableRenderer` — 실제 렌더러 인스턴스 (Forward · Deferred · 2D 중 하나).
- `EnqueuePass` — **해당 프레임의 해당 카메라** 에만 효과. 프레임 끝나면 큐는 리셋.
- `renderPassEvent` 로 삽입 위치 결정:
  - `BeforeRendering`: 렌더 시작 직전 (VolumeStack 오버라이드용 이상적)
  - `BeforeRenderingPostProcessing`: PP 직전 (커스텀 PP 삽입)
  - `AfterRenderingPostProcessing`: PP 이후 (UI 블러 등)
  - `AfterRendering`: 최종 프레임 버퍼 확정 후

### 3.3 VolumeStack 런타임 오버라이드
```csharp
var stack = data.cameraData.camera.GetUniversalAdditionalCameraData().volumeStack;
var dof   = stack.GetComponent<DepthOfField>();
dof.focusDistance.Override(worldDistance);   // 이 프레임만, 이 카메라만
```
- **VolumeProfile 에셋은 건드리지 않는다** — 메모리상 Stack 만 수정.
- `.Override(value)` 는 다음 프레임에 VolumeManager 가 Stack 을 재평가하면 자동 리셋. 매 프레임 세팅 반복 필수.
- `GetComponent<T>()` 는 VolumeStack 버전 — `GameObject.GetComponent` 아님.

## 4. 커스텀 PP 패스로 확장

레퍼런스 코드는 Execute 가 VolumeStack 수정만 하지만, **풀스크린 삼각형 드로우** 를 추가하면 실제 PP 패스가 된다:

```csharp
public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
{
    var cmd = CommandBufferPool.Get("MyCustomPP");

    // 1) 카메라 컬러 타겟으로 설정
    cmd.SetRenderTarget(data.cameraData.renderer.cameraColorTargetHandle);

    // 2) 풀스크린 삼각형 드로우 — vertex 3 개, procedural
    cmd.DrawProcedural(Matrix4x4.identity, _myMaterial, 0, MeshTopology.Triangles, 3, 1);

    ctx.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
}
```

풀스크린 삼각형 셰이더 본문 (vertex 쪽):
```hlsl
// 3-vertex 삼각형이 화면 전체를 덮음 (uv 0~1)
Varyings vert(uint vertexID : SV_VertexID)
{
    Varyings OUT;
    OUT.positionHCS = float4(
        (vertexID == 2) ?  3.0 : -1.0,
        (vertexID == 1) ? -3.0 :  1.0,
        0.0, 1.0);
    OUT.uv = float2(
        (vertexID == 2) ? 2.0 : 0.0,
        (vertexID == 1) ? 2.0 : 0.0);
    return OUT;
}
```

그 결과 fragment 는 화면의 모든 픽셀을 순회 → **자유로운 PP** (디졸브 화면 전환, 스크린 왜곡, 커스텀 톤매핑 등).

## 5. Proto 하드룰 (이 패턴 사용 시)

- **이벤트 해제 의무** — OnEnable 에서 `+=` 한 모든 콜백은 OnDisable 에서 `-=`.
  해제 누락 시 scene 재로드마다 핸들러가 중복 누적 → **FT-05 (자원 누수)**.
- **static 참조 캐시 시 생명주기 명시** — 레퍼런스 코드의 `s_transform` 처럼 static 필드를 쓰면
  씬 전환 시 dangling 위험. 해제 시점 규칙 문서화 필수.
- **Override 반복 세팅** — 매 프레임 호출 전제. 한 번만 세팅하고 끝나면 다음 프레임에 리셋됨.
- **VolumeProfile.asset 직접 수정 금지** — 항상 VolumeStack 경유.
  프로파일 에셋 수정은 에디터 세션을 "더럽힘" (HR-10 업스트림 오염 금지와 같은 정신).
- **EnqueuePass 는 프레임 지속성 없음** — 매 `beginCameraRendering` 훅에서 다시 enqueue 필수.

## 6. 응용 예시 (Proto 맥락)

| 시나리오 | beginCameraRendering 에서 | VolumeStack 오버라이드 | 커스텀 패스 |
|---|---|---|---|
| 플레이어 피격 시 화면 빨개짐 | Hit 타이머 감시 | `ColorAdjustments.saturation.Override(-50)` | (없음) |
| 줌 시 DoF 자동 초점 | 카메라 forward ray → 충돌 오브젝트 | `DoF.focusDistance.Override(hit.distance)` | (없음) |
| 스킬 발동 시 방사형 블러 | 스킬 타이머 감시 | (없음) | 풀스크린 삼각형 + radial blur 셰이더 |
| 유닛 선택 하이라이트 (화면공간 아웃라인) | (없음) | (없음) | Color + Depth → Sobel PP 패스 |
| 섹션 전환 디졸브 (화면 와이프) | 전환 타이머 감시 | (없음) | 풀스크린 삼각형 + dissolve 노이즈 |

## 7. VKL 검증 원칙

이 패턴을 사용하는 모든 모듈은 아래 오라클을 집행한다:

- **OR-03 (Execution)**: Play 모드 진입 후 콘솔 에러 0.
- **OR-04 (Observability)**: Frame Debugger 로 커스텀 패스가 실제로 큐에 들어가 실행됐는지 확인.
- **OR-06 (Metamorphic)**: OnEnable ↔ OnDisable 반복 100회 후 핸들러 리스트 길이 불변 (리크 없음).
- **OR-08 (Human)**: 시각 산출물은 PRE/POST 스크린샷 비교로 최종 판정.

## 8. 참고 링크 (외부)

- Unity Manual — [URP: Scriptable Render Passes](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/manual/renderer-features/scriptable-renderer-features/inject-a-pass-using-a-scriptable-renderer-feature.html)
- Unity Manual — [Volume Overrides at Runtime](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@latest/manual/Volumes-Scripting.html)
- URP Core.hlsl — `Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl`

## 9. Proto 단계 원칙

- 본 패턴 **자체** 를 구현하는 모듈은 따로 만들지 않는다 (너무 저레벨).
- PP 효과가 필요한 각 모듈이 **이 문서를 참조** 해 자체 적용한다.
- 반복 패턴이 3개 이상 등장하면 `post-process-kit.md` 에 공용 베이스 클래스로 승격 검토.
