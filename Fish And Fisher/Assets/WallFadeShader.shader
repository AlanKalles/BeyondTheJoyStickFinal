Shader "FishAndFisher/URP/WallFade"
{
    Properties
    {
        // 基础纹理和颜色
        _BaseMap ("基础纹理", 2D) = "white" {}
        _BaseColor ("基础颜色", Color) = (1,1,1,1)

        // 相机距离淡出设置
        _FadeStartDistance ("开始淡出距离", Float) = 5.0
        _FadeEndDistance ("完全透明距离", Float) = 2.0

        // 表面属性
        _Smoothness ("平滑度", Range(0,1)) = 0.5
        _Metallic ("金属度", Range(0,1)) = 0.0

        // 透明度控制
        _MinAlpha ("最小透明度", Range(0,1)) = 0.0

        // 最小亮度（确保背光面也有颜色）
        _MinBrightness ("最小亮度", Range(0,1)) = 0.3

        // 使用自定义相机位置（由脚本设置）
        [Toggle] _UseCustomCamera ("使用指定相机", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 300

        // 主渲染Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off  // 双面渲染

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP关键字
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // 材质属性
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _FadeStartDistance;
                float _FadeEndDistance;
                half _Smoothness;
                half _Metallic;
                half _MinAlpha;
                half _MinBrightness;
                float _UseCustomCamera;
            CBUFFER_END

            // 全局变量：由脚本设置的自定义相机位置
            float3 _CustomCameraPosition;

            // 顶点输入
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // 片元输入
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            // 顶点着色器
            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            // 片元着色器
            half4 frag(Varyings input) : SV_Target
            {
                // 采样基础纹理
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 albedo = baseMap * _BaseColor;

                // 双面法线 - 确保背面也能接收光照
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // 如果法线背对视角，翻转法线（双面光照）
                if (dot(normalWS, viewDirWS) < 0.0)
                {
                    normalWS = -normalWS;
                }

                // 计算光照
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = viewDirWS;
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                // 表面数据
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.alpha = albedo.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1.0;

                // 计算PBR光照
                half4 color = UniversalFragmentPBR(lightingInput, surfaceData);

                // 确保最小亮度 - 即使背光也能看到颜色
                color.rgb = max(color.rgb, albedo.rgb * _MinBrightness);

                // 根据设置选择使用的相机位置
                float3 targetCameraPos = _UseCustomCamera > 0.5 ? _CustomCameraPosition : _WorldSpaceCameraPos;

                // 计算相机距离
                float distanceToCamera = distance(input.positionWS, targetCameraPos);

                // 根据距离计算透明度
                // 使用smoothstep实现平滑过渡
                float fadeFactor = smoothstep(_FadeEndDistance, _FadeStartDistance, distanceToCamera);
                fadeFactor = max(fadeFactor, _MinAlpha);

                // 应用透明度
                color.a *= fadeFactor;

                // 应用雾效
                color.rgb = MixFog(color.rgb, input.fogFactor);

                return color;
            }
            ENDHLSL
        }

        // 阴影投射Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off  // 双面阴影

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
