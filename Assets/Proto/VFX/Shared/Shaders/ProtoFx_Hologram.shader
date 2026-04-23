Shader "Proto/Fx/Hologram"
{
    Properties
    {
        _HoloColor("Holo Color", Color) = (0.3, 0.8, 1, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.7
        _ScanlineSpeed("Scanline Speed", Float) = 2.0
        _ScanlineDensity("Scanline Density", Float) = 20.0
        _RimPower("Rim Power", Range(0.1, 8)) = 2.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Name "ForwardHologram"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _HoloColor;
                float _Alpha;
                float _ScanlineSpeed;
                float _ScanlineDensity;
                float _RimPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);
                float rim = 1.0 - saturate(dot(N, V));
                rim = pow(rim, _RimPower);

                float scan = sin(IN.positionOS.y * _ScanlineDensity + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;

                float a = _Alpha * lerp(0.3, 1.0, rim) * lerp(0.7, 1.0, scan);
                return half4(_HoloColor.rgb * a, a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
