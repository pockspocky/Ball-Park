using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Pause : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private Volume pauseEffect;
    [SerializeField] private float darkIntensity = 0.5f;

    public bool _isPaused;
    private ColorAdjustments _colorAdjustments;

    void Start()
    {
        // 获取后处理组件
        if (pauseEffect != null && pauseEffect.profile.TryGet(out _colorAdjustments))
        {
            _colorAdjustments.postExposure.value = 0f;
        }
        
        // 初始状态锁定光标
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            _isPaused = !_isPaused;
            PauseGame(_isPaused);
        }
    }

    private void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;

        if (_colorAdjustments != null)
        {
            _colorAdjustments.postExposure.value = pause ? -darkIntensity : 0f;
        }

        // 根据暂停状态控制光标
        if (pause)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}