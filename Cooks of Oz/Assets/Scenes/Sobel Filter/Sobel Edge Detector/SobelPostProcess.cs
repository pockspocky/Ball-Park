using System;
using Unity.Mathematics;
using UnityEngine;
using UnityCG.cginc;

[RequireComponent(typeof(Camera))]
public class SobelPostProcess : MonoBehaviour
{
    public Material sobelMaterial;
    private Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.depthTextureMode = DepthTextureMode.DepthNormals;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 将滤镜效果应用到相机渲染结果上
        Graphics.Blit(source, destination, sobelMaterial);
    }
    
    inline void DecodeDepthNormal( float4 enc, out float depth, out float3 normal )
    {
        depth = DecodeFloatRG (enc.zw);
        normal = DecodeViewNormalStereo (enc);
    }

}