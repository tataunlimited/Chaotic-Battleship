// Creates a new menu item in Unity's shader list
Shader "Gemini/ShipWithHighlightURP"
{
    // Properties for BOTH the main texture and the glow
    Properties
    {
        [Header(Main Ship Properties)]
        _MainTex("Base Texture (RGB)", 2D) = "white" {}
        _Color("Base Color", Color) = (1, 1, 1, 1)

        [Header(Highlight Properties)]
        _GlowColor("Glow Color", Color) = (0, 1, 0, 0.5)
        _GlowPower("Glow Power", Range(0.1, 10.0)) = 2.5
        _GlowIntensity("Glow Intensity", Range(0.0, 5.0)) = 1.0 // Use 0 to disable glow
    }

    SubShader
    {
        // This tag block applies to the whole subshader
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        // --- PASS 1: Main Ship Texture (Standard Lit Pass) ---
        Pass
        {
            Name "MainLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Includes for a standard Lit URP shader
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 worldNormal  : NORMAL;
                float3 worldPos     : TEXCOORD0;
                float2 uv           : TEXCOORD1;
            };

            // Define properties for this pass
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Standard lighting calculations
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                Light mainLight = GetMainLight();
                half3 lambert = LightingLambert(mainLight.color, mainLight.direction, IN.worldNormal);
                half3 finalColor = texColor.rgb * lambert;
                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }

        // --- PASS 2: Fresnel Glow Highlight ---
        Pass
        {
            Name "Glow"
            
            // --- Shader State ---
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // Standard transparency

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 worldNormal  : TEXCOORD0;
                float3 viewDir      : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                half _GlowPower;
                half _GlowIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // To disable the glow pass entirely if intensity is zero
                if (_GlowIntensity <= 0.0)
                {
                    discard;
                }

                IN.worldNormal = normalize(IN.worldNormal);
                IN.viewDir = normalize(IN.viewDir);

                float fresnel = 1.0 - saturate(dot(IN.worldNormal, IN.viewDir));
                fresnel = pow(fresnel, _GlowPower);

                half3 finalColor = _GlowColor.rgb * fresnel * _GlowIntensity;
                half finalAlpha = _GlowColor.a * fresnel;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}