using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class Overdrive : MonoBehaviour
{
    //header
    [Header("Overdrive Parameters")]
    [SerializeField] private float speedThreshold = 10f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 1f;
    [SerializeField] private LayerMask explosiveLayer;
    
    [SerializeField] private float cooldownDuration = 2f; // 冷却时间
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    private Rigidbody rb;
    
    //header
    [Header("VFX")]
    [SerializeField] private GameObject overdriveVFXPrefab;
    [SerializeField] private VFXController vfxController;
    
    [Header("Time Parameters")]
    [SerializeField] private TimeLerp timeLerp;
    // [SerializeField] private AnimationCurve timeCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float timeScale = 0.5f; // 目标时间缩放
    // [SerializeField] private float duration = 1f; // 变化持续时间
    
    [Header("Post Processing")]
    [SerializeField] private float lensDistortionIntensity = -0.7f;
    [SerializeField] private float distortionFadeIn = 0.1f;
    [SerializeField] private float distortionFadeOut = 0.5f;
    [SerializeField] private float depthOFieldIntensity = 0.5f;
    [SerializeField] private float depthOFieldFadeIn = 0.1f;
    [SerializeField] private float depthOFieldFadeOut = 0.5f;
    
    [SerializeField] private Volume _volume;
    
    private DepthOfField _depthOField;
    private LensDistortion _lensDistortion;

    private int c = 1;
    
    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        _volume.profile.TryGet<DepthOfField>(out _depthOField);
        _volume.profile.TryGet<LensDistortion>(out _lensDistortion);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if collision object is in the explosive layer and speed is above threshold
        if (((1 << collision.gameObject.layer) & explosiveLayer) != 0 && rb.linearVelocity.magnitude >= speedThreshold && !isOnCooldown)
        {
            
            // Find all nearby rigidbodies within explosion radius
            Collider[] colliders = Physics.OverlapSphere(collision.contacts[0].point, explosionRadius, explosiveLayer);
            
            Debug.Log($"Explosion triggered at {collision.contacts[0].point} with speed: {rb.linearVelocity.magnitude}");
            
            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody targetRb = nearbyObject.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    targetRb.AddExplosionForce(explosionForce * (rb.linearVelocity.magnitude / 5), collision.contacts[0].point, explosionRadius, upwardsModifier);
                    // Debug.Log($"Object affected: {nearbyObject.gameObject.name}");
                }
            }

            // debug info about chromatic aberration and lens distortion
            timeLerp.StartTimeLerp(timeScale);
            postProcessTrigger();
            vfxController.SpawnVFX(collision.contacts[0].point, rb.linearVelocity, overdriveVFXPrefab, false);
        }
    }

    private void postProcessTrigger()
    {
        if (isOnCooldown) return;
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
        StartCoroutine(LerpDepthOfField(1f, depthOFieldIntensity, depthOFieldFadeIn, depthOFieldFadeOut));
        StartCoroutine(LerpLensDistortion(0f, lensDistortionIntensity, distortionFadeIn, distortionFadeOut));
        _depthOField.focalLength.value = 1f;
    }
    
    private IEnumerator LerpDepthOfField(float startValue, float endValue, float fadeInDuration,
        float fadeOutDuration = 0.1f)
    {
        lock (_depthOField)
        {
            
            // fade in to startvalue
            float elapsed = 0f;
            while (elapsed < fadeInDuration || _depthOField.focalLength.value < endValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeInDuration;
                _depthOField.focalLength.value = Mathf.Lerp(startValue, endValue, normalizedTime);
                yield return null;
            }

            //fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration || _depthOField.focalLength.value > startValue)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeOutDuration;
                _depthOField.focalLength.value = Mathf.Lerp(endValue, startValue, normalizedTime);
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