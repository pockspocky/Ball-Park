Shader "Shaders/ChromaticAberrationShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amount ("Aberration Amount", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "HDRenderPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Amount;
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                float4 redChannel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(_Amount, 0));
                float4 greenChannel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float4 blueChannel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(_Amount, 0));
                
                return float4(redChannel.r, greenChannel.g, blueChannel.b, 1);
            }
            ENDHLSL
        }
    }
}