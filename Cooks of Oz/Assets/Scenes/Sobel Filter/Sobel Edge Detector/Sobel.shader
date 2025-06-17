

Shader "Unlit/Sobel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.5
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthNormalsTexture;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _EdgeThreshold;
            float4 _EdgeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            void DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
            {
                DecodeDepthNormal(enc, depth, normal);
                normal = normalize(normal);
            }

            float SobelSample(float2 uv)
            {
                float2 delta = _MainTex_TexelSize.xy;
                float depth;
                float3 normal;

                float4 dn1 = tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, -delta.y));
                float4 dn2 = tex2D(_CameraDepthNormalsTexture, uv + float2(0, -delta.y));
                float4 dn3 = tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, -delta.y));
                float4 dn4 = tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, 0));
                float4 dn5 = tex2D(_CameraDepthNormalsTexture, uv);
                float4 dn6 = tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, 0));
                float4 dn7 = tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, delta.y));
                float4 dn8 = tex2D(_CameraDepthNormalsTexture, uv + float2(0, delta.y));
                float4 dn9 = tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, delta.y));

                float3 n1, n2, n3, n4, n5, n6, n7, n8, n9;
                float d1, d2, d3, d4, d5, d6, d7, d8, d9;

                DecodeDepthNormal(dn1, d1, n1);
                DecodeDepthNormal(dn2, d2, n2);
                DecodeDepthNormal(dn3, d3, n3);
                DecodeDepthNormal(dn4, d4, n4);
                DecodeDepthNormal(dn5, d5, n5);
                DecodeDepthNormal(dn6, d6, n6);
                DecodeDepthNormal(dn7, d7, n7);
                DecodeDepthNormal(dn8, d8, n8);
                DecodeDepthNormal(dn9, d9, n9);

                float3 sobelX = n1 + 2.0 * n4 + n7 - n3 - 2.0 * n6 - n9;
                float3 sobelY = n1 + 2.0 * n2 + n3 - n7 - 2.0 * n8 - n9;
                float sobelXD = d1 + 2.0 * d4 + d7 - d3 - 2.0 * d6 - d9;
                float sobelYD = d1 + 2.0 * d2 + d3 - d7 - 2.0 * d8 - d9;

                float edge = length(sobelX) + length(sobelY) + abs(sobelXD) + abs(sobelYD);
                return edge;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float edge = SobelSample(i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);
                return lerp(col, _EdgeColor, step(_EdgeThreshold, edge));
            }
            ENDCG
        }
    }
}