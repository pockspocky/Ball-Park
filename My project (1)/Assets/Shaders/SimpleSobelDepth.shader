Shader "Custom/SimpleSobelDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _EdgeThickness ("Edge Thickness", Range(0.1, 5)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
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
                float4 _EdgeColor;
                float _EdgeThickness;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 texelSize = _EdgeThickness / _ScreenParams.xy;
                
                // 采样深度值 (3x3 Sobel核心)
                float d00 = SampleSceneDepth(input.uv + float2(-texelSize.x, -texelSize.y));
                float d01 = SampleSceneDepth(input.uv + float2(0, -texelSize.y));
                float d02 = SampleSceneDepth(input.uv + float2(texelSize.x, -texelSize.y));
                float d10 = SampleSceneDepth(input.uv + float2(-texelSize.x, 0));
                float d12 = SampleSceneDepth(input.uv + float2(texelSize.x, 0));
                float d20 = SampleSceneDepth(input.uv + float2(-texelSize.x, texelSize.y));
                float d21 = SampleSceneDepth(input.uv + float2(0, texelSize.y));
                float d22 = SampleSceneDepth(input.uv + float2(texelSize.x, texelSize.y));
                
                // Sobel算子
                float sobelX = d00 + 2*d10 + d20 - d02 - 2*d12 - d22;
                float sobelY = d00 + 2*d01 + d02 - d20 - 2*d21 - d22;
                float edge = sqrt(sobelX*sobelX + sobelY*sobelY);
                
                // 原始颜色
                float4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // 混合边缘
                return lerp(originalColor, _EdgeColor, saturate(edge * 100));
            }
            ENDHLSL
        }
    }
} 