Shader "Proto/Env/BushBillboard"
{
    Properties
    {
        [MainTexture] _BaseMap("Leaf Cluster (RGBA)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (0.35, 0.55, 0.22, 1)
        _QuadSize("Quad Size", Float) = 0.45
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.35
        _AmbientColor("Ambient Color", Color) = (0.08, 0.10, 0.07, 1)
        _WrapAmount("Wrap Lambert (0=hard, 0.5=soft)", Range(0, 0.8)) = 0.15
        _BacklightColor("Backlight Color", Color) = (0.9, 1.0, 0.5, 1)
        _BacklightIntensity("Backlight Intensity", Range(0, 2)) = 0.35
        _WindStrength("Wind Strength", Range(0, 0.3)) = 0.04
        _WindSpeed("Wind Speed", Float) = 1.2
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        LOD 150

        Pass
        {
            Name "ForwardBush"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Attribute 배치 규약:
            //   POSITION  : 쿼드 센터 (OS). 같은 쿼드의 4 버텍스는 모두 동일한 센터.
            //   NORMAL    : 원본 구 노멀 (center.normalized). 라이팅용으로 빌보딩 후에도 보존.
            //   TEXCOORD0 : 0~1 (해당 쿼드의 UV, 리프 텍스처 샘플링)
            //   TEXCOORD1 : -0.5~+0.5 (로컬 오프셋, 빌보드 확장에 사용)
            struct Attributes
            {
                float4 positionOS  : POSITION;
                float3 normalOS    : NORMAL;
                float2 uv          : TEXCOORD0;
                float2 localOffset : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _QuadSize;
                float _AlphaCutoff;
                float4 _AmbientColor;
                float _WrapAmount;
                float4 _BacklightColor;
                float _BacklightIntensity;
                float _WindStrength;
                float _WindSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // 쿼드 센터 WS
                float3 centerWS = TransformObjectToWorld(IN.positionOS.xyz);

                // 카메라 right/up 을 월드 공간에서 추출
                // UNITY_MATRIX_I_V 는 view → world. 그 기저 벡터가 카메라 축의 월드 표현.
                float3 camRightWS = normalize(UNITY_MATRIX_I_V._m00_m10_m20);
                float3 camUpWS    = normalize(UNITY_MATRIX_I_V._m01_m11_m21);

                // 빌보드 확장 (쿼드별 센터 기준)
                float3 worldPos = centerWS
                               + camRightWS * IN.localOffset.x * _QuadSize
                               + camUpWS    * IN.localOffset.y * _QuadSize;

                // 윈드 스웨이 (센터 좌표 위상 오프셋, 쿼드 상단만 흔들림)
                float wind = sin(_Time.y * _WindSpeed + centerWS.x * 2.0 + centerWS.z * 2.0) * _WindStrength;
                worldPos.xz += float2(wind, wind * 0.6) * saturate(IN.localOffset.y + 0.5);

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);   // ★ 구 노멀 유지
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.viewDirWS   = GetWorldSpaceViewDir(worldPos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(tex.a - _AlphaCutoff);

                Light mainLight = GetMainLight();

                float3 N = normalize(IN.normalWS);
                float3 L = mainLight.direction;
                float3 V = normalize(IN.viewDirWS);

                // Wrapped Lambert — _WrapAmount=0 이면 순수 Lambert(하드), 0.5 면 half-Lambert(소프트).
                // 부쉬는 기본 0.15 (살짝만 부드럽게 하고 명암 대비는 유지).
                float NoL = dot(N, L);
                half wrapLambert = saturate((NoL + _WrapAmount) / (1.0 + _WrapAmount));
                half3 directLit = mainLight.color * wrapLambert;

                // 백라이트/서브서퍼스 의사표현: 태양 반대 방향에서 잎이 살짝 빛남
                // 시야 방향과 광원 반대 방향이 정렬될 때만 활성화 (역광 실루엣 느낌)
                half backDot = saturate(dot(-N, L));
                half viewAlign = saturate(dot(V, -L));
                half3 back = _BacklightColor.rgb * _BacklightIntensity * backDot * viewAlign;

                half3 albedo = tex.rgb * _BaseColor.rgb;
                half3 col = albedo * (directLit + _AmbientColor.rgb) + albedo * back;

                return half4(col, 1);
            }
            ENDHLSL
        }

        // 섀도우 캐스터는 빌보드 불일치 때문에 생략 (HR: Proto 단순화).
    }

    FallBack Off
}
