Shader "Ocean/Water_OceanURP"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.14, 0.55, 0.75, 0.85)
        _DeepColor    ("Deep Color", Color)    = (0.02, 0.12, 0.2, 0.95)
        _FoamColor    ("Foam Color", Color)    = (1,1,1,1)

        _Normal1 ("Normal A", 2D) = "bump" {}
        _Normal2 ("Normal B", 2D) = "bump" {}
        _NormalTiling ("Normal Tiling", Vector) = (0.2, 0.2, 0.12, 0.12)
        _NormalSpeed  ("Normal Speed",  Vector) = (0.03, 0.02, -0.02, 0.04)
        _NormalStrength ("Normal Strength", Range(0,2)) = 1

        _FresnelPower  ("Fresnel Power", Range(0.1, 8)) = 3.5
        _FoamThreshold ("Foam Threshold", Range(0, 1))  = 0.75
        _FoamIntensity ("Foam Intensity", Range(0, 2))  = 0.8

        _DepthStrength ("Depth Strength", Range(0, 3)) = 1.2  // requires depth tex
        _OverallAlpha  ("Alpha", Range(0,1)) = 0.9

        // Ripple arrays (max 8)
        _RippleCount ("Ripple Count", Int) = 0
        _RipplePos0 ("RipplePos0", Vector) = (0,0,0,0)
        _RipplePos1 ("RipplePos1", Vector) = (0,0,0,0)
        _RipplePos2 ("RipplePos2", Vector) = (0,0,0,0)
        _RipplePos3 ("RipplePos3", Vector) = (0,0,0,0)
        _RipplePos4 ("RipplePos4", Vector) = (0,0,0,0)
        _RipplePos5 ("RipplePos5", Vector) = (0,0,0,0)
        _RipplePos6 ("RipplePos6", Vector) = (0,0,0,0)
        _RipplePos7 ("RipplePos7", Vector) = (0,0,0,0)
        // xy = world pos (XZ plane), z = startTime, w = seed/unused

        _RippleData0 ("RippleData0", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData1 ("RippleData1", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData2 ("RippleData2", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData3 ("RippleData3", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData4 ("RippleData4", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData5 ("RippleData5", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData6 ("RippleData6", Vector) = (6, 1.0, 1.5, 0.6)
        _RippleData7 ("RippleData7", Vector) = (6, 1.0, 1.5, 0.6)
        // x = maxRadius, y = amplitude, z = frequency, w = damping
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_Normal1); SAMPLER(sampler_Normal1);
            TEXTURE2D(_Normal2); SAMPLER(sampler_Normal2);

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;

                float4 _NormalTiling;  // xy for N1, zw for N2
                float4 _NormalSpeed;   // xy for N1, zw for N2
                half  _NormalStrength;

                half  _FresnelPower;
                half  _FoamThreshold;
                half  _FoamIntensity;

                half  _DepthStrength;
                half  _OverallAlpha;

                int   _RippleCount;

                float4 _RipplePos0, _RipplePos1, _RipplePos2, _RipplePos3;
                float4 _RipplePos4, _RipplePos5, _RipplePos6, _RipplePos7;
                float4 _RippleData0, _RippleData1, _RippleData2, _RippleData3;
                float4 _RippleData4, _RippleData5, _RippleData6, _RippleData7;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 posCS : SV_POSITION;
                float2 uv1   : TEXCOORD0;
                float2 uv2   : TEXCOORD1;
                float3 posWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            float rippleContribution(float2 worldXZ, float4 pos, float4 data, float t)
            {
                // pos.xy = ripple origin XZ, pos.z = start time
                float start = pos.z;
                float age = max(t - start, 0.0);
                if (age <= 0.0) return 0.0;

                float2 toP = worldXZ - pos.xy;
                float dist = length(toP);

                float maxR = data.x;
                float A    = data.y;
                float freq = data.z;
                float damp = data.w;

                if (dist > maxR) return 0.0;

                float phase = dist * freq - age * (freq * 2.0);
                float att   = saturate(1.0 - dist / maxR) * exp(-age * damp);

                return A * sin(phase) * att;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.posCS = TransformWorldToHClip(o.posWS);
                o.viewDirWS = _WorldSpaceCameraPos.xyz - o.posWS;

                float t = _Time.y;
                o.uv1 = v.uv * _NormalTiling.xy + _NormalSpeed.xy * t;
                o.uv2 = v.uv * _NormalTiling.zw + _NormalSpeed.zw * t;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 N1 = UnpackNormal(SAMPLE_TEXTURE2D(_Normal1, sampler_Normal1, i.uv1));
                float3 N2 = UnpackNormal(SAMPLE_TEXTURE2D(_Normal2, sampler_Normal2, i.uv2));
                float3 N  = normalize(float3(N1.xy + N2.xy, 1.0));
                N = normalize(lerp(float3(0,0,1), N, _NormalStrength));

                // Simple depth tint (requires URP Depth Texture on camera)
                float sceneDepth = 1.0;
                #if defined(REQUIRES_DEPTH_TEXTURE)
                    float4 posCS = TransformWorldToHClip(i.posWS);
                    float2 uv     = posCS.xy / posCS.w * 0.5 + 0.5;
                    sceneDepth = SAMPLE_SCENE_DEPTH(uv);
                    float linearEye = LinearEyeDepth(sceneDepth, _ZBufferParams);
                    float surfEye   = LinearEyeDepth(posCS.w, _ZBufferParams);
                    float depthDiff = saturate((linearEye - surfEye) * _DepthStrength);
                #else
                    float depthDiff = 0.5; // fallback
                #endif

                half4 baseCol = lerp(_DeepColor, _ShallowColor, depthDiff);

                // Fresnel
                float3 V = normalize(i.viewDirWS);
                float  fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // Ripples (world XZ)
                float t = _Time.y;
                float2 xz = float2(i.posWS.x, i.posWS.z);
                float rip = 0.0;

                [unroll] for (int idx=0; idx<8; idx++)
                {
                    if (idx >= _RippleCount) break;

                    float4 p = (idx==0)?_RipplePos0:(idx==1)?_RipplePos1:(idx==2)?_RipplePos2:(idx==3)?_RipplePos3:
                               (idx==4)?_RipplePos4:(idx==5)?_RipplePos5:(idx==6)?_RipplePos6:_RipplePos7;

                    float4 d = (idx==0)?_RippleData0:(idx==1)?_RippleData1:(idx==2)?_RippleData2:(idx==3)?_RippleData3:
                               (idx==4)?_RippleData4:(idx==5)?_RippleData5:(idx==6)?_RippleData6:_RippleData7;

                    rip += rippleContribution(xz, p, d, t);
                }

                // Perturb normal slightly by ripple height derivative approximation (cheap “bump”)
                float rippleN = saturate(0.5 + 0.5 * rip);
                baseCol.rgb += rippleN * 0.05; // tiny brightness pop from ripples

                // Foam (brighten on strong fresnel or big ripple)
                float foamMask = saturate(max(fresnel, abs(rip)) - _FoamThreshold);
                half3 foam = _FoamColor.rgb * foamMask * _FoamIntensity;

                half3 col = baseCol.rgb + foam + fresnel * 0.1;
                return half4(col, baseCol.a * _OverallAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}