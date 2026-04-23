Shader "Proto/Fx/Scanline"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (0.8, 0.8, 0.8, 1)
        _ScanlineColor("Scanline Color", Color) = (0, 1, 0.4, 1)
        _ScanlineSpeed("Scanline Speed", Float) = 6.0
        _ScanlineDensity("Scanline Density", Float) = 60.0
        _Glitch("Glitch", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardScanline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

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
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ScanlineColor;
                float _ScanlineSpeed;
                float _ScanlineDensity;
                float _Glitch;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                float ndc = IN.screenPos.y / IN.screenPos.w;
                float scanBand = step(0.5, frac(ndc * _ScanlineDensity - _Time.y * _ScanlineSpeed));

                float glitchWave = (sin(_Time.y * 37.0) * 0.5 + 0.5) * _Glitch;
                float mask = scanBand * (0.3 + glitchWave);

                return half4(lerp(baseCol.rgb, _ScanlineColor.rgb, mask), 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
