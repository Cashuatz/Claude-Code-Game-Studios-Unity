Shader "Proto/Fx/Dissolve"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        _DissolveNoise("Dissolve Noise", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth("Edge Width", Range(0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DissolveNoise);  SAMPLER(sampler_DissolveNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _DissolveAmount;
                float4 _EdgeColor;
                float _EdgeWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half noise = SAMPLE_TEXTURE2D(_DissolveNoise, sampler_DissolveNoise, IN.uv).r;
                clip(noise - _DissolveAmount);

                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                half edgeMask = step(noise - _DissolveAmount, _EdgeWidth);
                return lerp(baseCol, _EdgeColor, edgeMask);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
