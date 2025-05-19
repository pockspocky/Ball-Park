using System;
using UnityEngine;

public class TimeLerp : MonoBehaviour
{
    [Header("进入慢动作")]
    [SerializeField] private AnimationCurve enterCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float enterDuration = 1f;

    [Header("退出慢动作")]
    [SerializeField] private AnimationCurve exitCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float exitDuration = 1f;

    private float currentTime = 0f;
    private bool isLerping = false;
    private float startTimeScale = 1f;
    private float targetTimeScale = 1f;
    private bool isReturning = false;
    private AnimationCurve currentCurve;
    private float currentDuration;
    
    private float fixedDeltaTime;
    
    private float originalFixedDelta = 0.02f;

    public void StartTimeLerp(float target)
    {
        startTimeScale = Time.timeScale;
        targetTimeScale = target;
        currentTime = 0f;
        isLerping = true;
        isReturning = false;
        currentCurve = enterCurve;
        currentDuration = enterDuration;
    }

    private void Start()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {

        if (!isLerping) return;

        currentTime += Time.unscaledDeltaTime;
        float progress = currentTime / currentDuration;
        

        if (progress >= 1f)
        {
            if (!isReturning)
            {
                // 开始返回正常时间
                startTimeScale = targetTimeScale;
                targetTimeScale = 1f;
                currentTime = 0f;
                isReturning = true;
                currentCurve = exitCurve;
                currentDuration = exitDuration;
            }
            else
            {
                // 完全结束
                Time.timeScale = 1f;
                isLerping = false;
                Time.fixedDeltaTime = Time.timeScale * originalFixedDelta;

            }
            return;
        }

        float curveValue = currentCurve.Evaluate(progress);
        Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, curveValue);
        Time.fixedDeltaTime = Time.timeScale * originalFixedDelta;
    }
}