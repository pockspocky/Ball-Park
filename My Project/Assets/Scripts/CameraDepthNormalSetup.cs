using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDepthNormalSetup : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // 启用深度和法线纹理
        cam.depthTextureMode = DepthTextureMode.DepthNormals;
    }
} 