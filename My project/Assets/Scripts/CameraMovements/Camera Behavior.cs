using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class CameraBehavior : MonoBehaviour
{
    
    [Header("Camera Settings")]
    [SerializeField] private Transform target;           // Object to follow
    [SerializeField] private float distance = 10.0f;     // Distance from target
    [SerializeField] private float orbitSpeed = 5.0f;    // Horizontal rotation speed
    [SerializeField] private float verticalSpeed = 3.0f; // Vertical rotation speed
    [SerializeField] private float minVerticalAngle = -30f; // Minimum look angle
    [SerializeField] private float maxVerticalAngle = 60f;  // Maximum look angle
    [SerializeField] private Vector3 orbitOffset = new Vector3(0, 2f, 0); // Orbit point offset

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    
    private bool isBound = true;
    private Quaternion frozenRotation;
    private Vector3 frozenPosition;
    
    private Vector3 dashStartPosition;
    private Vector3 dashTargetPosition;
    private float dashProgress = 0f;
    private bool isDashing = false;
    private float dashDuration;
    
    
    [Header("Post Processing")]
    [SerializeField] private float lensDistortionIntensity = -0.7f;
    [SerializeField] private float distortionFadeIn = 0.1f;
    [SerializeField] private float distortionFadeOut = 0.5f;
    [SerializeField] private float chromaticAberrationIntensity = 0.5f;
    [SerializeField] private float chromaticAberrationFadeIn = 0.1f;
    [SerializeField] private float chromaticAberrationFadeOut = 0.5f;
    
    [SerializeField] private Volume _volume;
    
    
    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;

    public void UnbindCamera()
    {
        isBound = false;
        
        frozenRotation = transform.rotation;
        dashStartPosition = transform.position;
        isDashing = false;
        GetComponent<Follow>().enabled = false;
        
        StartCoroutine(LerpChromaticAberration(0f, chromaticAberrationIntensity, chromaticAberrationFadeIn, chromaticAberrationFadeOut));
        StartCoroutine(LerpLensDistortion(0f, lensDistortionIntensity, distortionFadeIn, distortionFadeOut));
        //debug info about chromatic aberration and lens distortion
        Debug.Log("For dashing: Chromatic Aberration: " + _chromaticAberration.intensity.value + " Lens Distortion: " + _lensDistortion.intensity.value);
    }
    
    public void StartDashMovement(Vector3 dashDirection, float distance, float duration)
    {
        isDashing = true;
        dashProgress = 0f;
        dashDuration = duration;
        dashStartPosition = transform.position;
        dashTargetPosition = dashStartPosition + (dashDirection * distance);
    
        // 启动协程来处理插值
        StartCoroutine(DashMovementCoroutine());
    }

    private IEnumerator DashMovementCoroutine()
    {
        while (dashProgress < 1f)
        {
            dashProgress += Time.deltaTime / dashDuration;
            transform.position = Vector3.Lerp(dashStartPosition, dashTargetPosition, dashProgress);
            yield return null;
        }
        isDashing = false;
    }
    
    public void RebindCamera()
    {
        isBound = true;
        GetComponent<Follow>().enabled = true;
        // StartCoroutine(LerpChromaticAberration(chrom, 0f, 0.15f));
        // StartCoroutine(LerpLensDistortion(lensDistortionIntensity, 0f, distortionTime * 0.1f));
    }

    private void Start()
    {
        
        _volume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        _volume.profile.TryGet<LensDistortion>(out _lensDistortion);
        
        if (target == null)
        {
            Debug.LogWarning("No target assigned to CameraBehavior!");
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        Vector3 angles = transform.eulerAngles;
        currentRotationY = angles.y;
        currentRotationX = angles.x;
    }

    private void LateUpdate()
    {
        if (target == null || !isBound)
        {
            if (isDashing)
            {
                dashProgress += Time.deltaTime / dashDuration;
                transform.position = Vector3.Lerp(dashStartPosition, dashTargetPosition, dashProgress);
            }
 //           transform.rotation = frozenRotation;
            return;
        }
        
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * orbitSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;

        currentRotationY += mouseX;
        currentRotationX -= mouseY;
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        Vector3 orbitPoint = target.position + orbitOffset;
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 position = orbitPoint - (rotation * Vector3.forward * distance);
        
        if (target == null || !isBound) 
        {
            transform.rotation = frozenRotation;
            return;
        }

        transform.position = position;
        transform.rotation = rotation;
        
        // chromatic aberration and lens distortion
        // Debug.Log("Chromatic Aberration: " + _chromaticAberration.intensity.value + " Lens Distortion: " + _lensDistortion.intensity.value);
    }

    private IEnumerator LerpChromaticAberration(float startValue, float endValue, float fadeInDuration,
        float fadeOutDuration = 0.1f)
    {
        lock (_chromaticAberration)
        {
            // fade in to startvalue
            float elapsed = 0f;
            while (elapsed < fadeInDuration || _chromaticAberration.intensity.value < endValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeInDuration;
                _chromaticAberration.intensity.value = Mathf.Lerp(startValue, endValue, normalizedTime);
                yield return null;
            }

            //fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration || _chromaticAberration.intensity.value > startValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeOutDuration;
                _chromaticAberration.intensity.value = Mathf.Lerp(endValue, startValue, normalizedTime);
                yield return null;
            }
        }
    }

    private IEnumerator LerpLensDistortion(float startValue, float endValue, float fadeInDuration, float fadeOutDuration = 0.1f)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration || _lensDistortion.intensity.value < endValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeInDuration;
                _lensDistortion.intensity.value = Mathf.Lerp(startValue, endValue, normalizedTime);
                yield return null;
            }
            // fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration || _lensDistortion.intensity.value > startValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeOutDuration;
                _lensDistortion.intensity.value = Mathf.Lerp(endValue, startValue, normalizedTime);
                yield return null;
            }
        }
    
    
}