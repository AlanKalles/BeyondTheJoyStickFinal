Shader "FishAndFisher/CrosshairUnlit"
{
    Properties
    {
        _MainTex ("准心纹理", 2D) = "white" {}
        _Color ("准心颜色", Color) = (1,1,1,1)
        _Alpha ("透明度", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        LOD 100

        // 启用透明混合
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "CrosshairPass"

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Alpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 转换顶点位置到裁剪空间
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                // 应用纹理的缩放和偏移
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 采样纹理
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // 应用颜色和透明度
                half4 finalColor = texColor * _Color;
                finalColor.a *= _Alpha;

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
