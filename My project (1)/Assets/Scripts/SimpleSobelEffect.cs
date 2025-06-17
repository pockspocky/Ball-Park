using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleSobelEffect : MonoBehaviour
{
    [Header("Sobel边缘检测设置")]
    public Material sobelMaterial;
    
    void Start()
    {
        // 确保深度纹理可用
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (sobelMaterial != null)
        {
            Graphics.Blit(source, destination, sobelMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
} 