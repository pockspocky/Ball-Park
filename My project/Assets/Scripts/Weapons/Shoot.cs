using UnityEngine;

public class Shoot : MonoBehaviour
{
    #region 序列化字段
    [Header("射击设置")]
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;

    [Header("蓄力设置")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float baseExplosionMultiplier = 1f;
    [SerializeField] private float maxExplosionMultiplier = 3f;

    [Header("射线设置")]
    [SerializeField] private float rayLength = 100f;
    [SerializeField] private float rayDuration = 0.5f;
    [SerializeField] private LayerMask targetLayers;

    [Header("爆炸设置")]
    [SerializeField] private float baseExplosionForce = 500f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 1f;
    #endregion

    #region 私有变量
    private bool canShoot = true;
    private bool isCharging;
    private float shootTimer;
    private float chargeTimer;
    private float currentExplosionMultiplier;
    #endregion

    #region Unity生命周期
    private void Update()
    {
        UpdateShootCooldown();
        HandleChargeInput();
    }
    #endregion

    #region 冷却系统
    private void UpdateShootCooldown()
    {
        if (!canShoot)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootCooldown)
            {
                ResetShootState();
            }
        }
    }

    private void ResetShootState()
    {
        canShoot = true;
        shootTimer = 0f;
    }
    #endregion
    
    #region 蓄力系统
    private void HandleChargeInput()
    {
        if (!canShoot) return;

        if (Input.GetKey(shootKey))
        {
            ChargeShot();
        }
        else if (Input.GetKeyUp(shootKey) && isCharging)
        {
            ReleaseShot();
        }
    }

    private void ChargeShot()
    {
        isCharging = true;
        chargeTimer = Mathf.Min(chargeTimer + Time.deltaTime, maxChargeTime);
        currentExplosionMultiplier = Mathf.Lerp(baseExplosionMultiplier, maxExplosionMultiplier, chargeTimer / maxChargeTime);
        
        Debug.Log($"蓄力中: {(chargeTimer / maxChargeTime * 100):F0}% - 倍率: {currentExplosionMultiplier:F1}x");
    }

    private void ReleaseShot()
    {
        HandleShoot();
        ResetChargeState();
        canShoot = false;
        shootTimer = 0f;
    }

    private void ResetChargeState()
    {
        isCharging = false;
        chargeTimer = 0f;
        currentExplosionMultiplier = baseExplosionMultiplier;
    }
    #endregion

    #region 射击系统
    private void HandleShootInput()
    {
        if (Input.GetKeyDown(shootKey) && canShoot)
        {
            HandleShoot();
            canShoot = false;
            shootTimer = 0f;
        }
    }

    private void HandleShoot()
    {
        Ray shootRay = CreateShootRay();
        RaycastHit hitInfo;

        bool hitTarget = Physics.Raycast(shootRay.origin, shootRay.direction, out hitInfo, rayLength, targetLayers);

        DrawDebugRay(shootRay);

        if (hitTarget)
        {
            ProcessHit(hitInfo.point);
        }
        else
        {
            ProcessMiss(shootRay);
        }
    }

    private Ray CreateShootRay()
    {
        return new Ray(transform.position, transform.forward);
    }

    private void DrawDebugRay(Ray ray)
    {
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, rayDuration);
    }

    private void LogShootInfo(Vector3 start, Vector3 end)
    {
        Debug.Log($"射线起点：{start}");
        Debug.Log($"射线终点：{end}");
    }

    private void ProcessHit(Vector3 hitPoint)
    {
        CreateExplosion(hitPoint);
    }

    private void ProcessMiss(Ray ray)
    {
        Vector3 endPoint = ray.origin + ray.direction * rayLength;
        // CreateLaser(ray.origin, endPoint);
    }
    #endregion
    
    #region 爆炸系统
    private void CreateExplosion(Vector3 explosionPoint)
    {
        float currentExplosionForce = baseExplosionForce * currentExplosionMultiplier;
        Collider[] affectedColliders = Physics.OverlapSphere(explosionPoint, explosionRadius, targetLayers);
        ApplyExplosionForce(affectedColliders, explosionPoint, currentExplosionForce);
    }

    private void ApplyExplosionForce(Collider[] colliders, Vector3 explosionPoint, float force)
    {
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(force, explosionPoint, explosionRadius, upwardsModifier);
            }
        }
    }
    #endregion
}