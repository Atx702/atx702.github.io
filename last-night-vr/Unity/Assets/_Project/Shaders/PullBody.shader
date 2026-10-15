// The Pull: a white, glowing, faintly shimmering silhouette. Brightens on every compression beat
// (_Beat, set by PullController) and fades out with _Alpha. Ignores fog so it cuts through it.
Shader "LastNight/PullBody"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Beat ("Beat", Range(0, 1)) = 0
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Shimmer ("Shimmer", Float) = 0.006
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "PullBody"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            Cull Back
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Beat;
                half _Alpha;
                float _Shimmer;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float wobble = sin(_Time.y * 23.0 + v.positionOS.y * 31.0) * _Shimmer * (1.0 + _Beat * 3.0);
                float3 pos = v.positionOS.xyz + v.normalOS * wobble;
                float3 positionWS = TransformObjectToWorld(pos);
                o.positionCS = TransformWorldToHClip(positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half rim = 1.0 - saturate(dot(normalize(i.normalWS), normalize(i.viewDirWS)));
                rim = rim * rim;
                half glow = (0.35 + rim * 1.4) * (1.0 + _Beat * 0.8);
                return half4(_Color.rgb * glow, saturate(_Alpha * (0.45 + rim * 0.55)));
            }
            ENDHLSL
        }
    }
    FallBack Off
}
