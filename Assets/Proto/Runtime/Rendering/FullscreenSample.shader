Shader "Proto/Rendering/FullscreenSample"
{
    Properties
    {
        _EffectStrength  ("Effect Strength (desat)", Range(0,1)) = 0.85
        _VignetteRadius  ("Vignette Radius",         Range(0,1)) = 0.8
        _VignetteSoftness("Vignette Softness",       Range(0,1)) = 0.45
        _ScanlineStrength("Scanline Strength",       Range(0,1)) = 0.12
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            Name "ProtoFullscreenSamplePass"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off  ZWrite Off  ZTest Always

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            sampler2D _BlitTexture;

            float _EffectStrength;
            float _VignetteRadius;
            float _VignetteSoftness;
            float _ScanlineStrength;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                // 핵심: 변환 없음 — Mesh 의 정점이 이미 clip-space 좌표
                OUT.positionCS = float4(IN.positionOS.xy, 0.0, 1.0);
                OUT.uv = IN.uv;
                // D3D/Metal/Vulkan 계열 RT 는 top-left origin — 소스 샘플링 y 보정
                // (GL-only 타겟이면 이 블록 제거 또는 SHADER_API_GLCORE 체크로 전환)
                #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12) || \
                    defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || \
                    defined(SHADER_API_XBOXONE) || defined(SHADER_API_GAMECORE) || \
                    defined(SHADER_API_PS4) || defined(SHADER_API_PS5) || \
                    defined(SHADER_API_SWITCH)
                    OUT.uv.y = 1.0 - OUT.uv.y;
                #endif
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                half3 col = tex2D(_BlitTexture, uv).rgb;

                half  gray  = dot(col, half3(0.299, 0.587, 0.114));
                half3 desat = lerp(col, half3(gray, gray, gray), _EffectStrength);

                float2 cv = uv - 0.5;
                float  d  = length(cv) * 1.41421356;
                float  vig = smoothstep(_VignetteRadius, _VignetteRadius - _VignetteSoftness, d);

                float scan = 1.0 - _ScanlineStrength * (0.5 + 0.5 * sin(uv.y * 1200.0));

                return half4(desat * vig * scan, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
