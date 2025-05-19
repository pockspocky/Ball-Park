using UnityEngine;

public class DashFunction : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float cameraUnbindDuration = 0.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("References")]
    [SerializeField] private CameraBehavior cameraScript;
    [SerializeField] private BallMovements ballMovements;
    [SerializeField] private MeshRenderer ballMesh;
    [SerializeField] private VFXController vfxController;
    [SerializeField] private GameObject dashVFXPrefab;
    [SerializeField] private Material dashMaterial;
    
    [Header("Material Properties")]
    [SerializeField] private string powerPropertyName = "_Power";
    
    private bool canDash = true;
    private bool isCameraUnbound;
    private bool isDashing;
    private float dashTimer;
    private float unbindTimer;
    private Camera mainCamera;
    
    private void Start()
    {
        InitializeReferences();
    }

    private void Update()
    {
        UpdateDashCooldown();
        UpdateCameraUnbind();
        HandleDashInput();
    }

    private void InitializeReferences()
    {
        mainCamera = Camera.main;
        cameraScript ??= mainCamera.GetComponent<CameraBehavior>();
        ballMovements ??= GetComponent<BallMovements>();
        ballMesh ??= GetComponent<MeshRenderer>();
    }

    private void UpdateDashCooldown()
    {
        if (!canDash)
        {
            dashTimer += Time.deltaTime;
            float cooldownProgress = dashTimer / dashCooldown;
            UpdateDashMaterialPower(cooldownProgress);
            
            if (dashTimer >= dashCooldown)
            {
                ResetDashState();
            }
        }
    }

    private void UpdateDashMaterialPower(float cooldownProgress)
    {
        if (dashMaterial != null)
        {
            float power = !canDash ? 50f - 45f * cooldownProgress : 5f;
            dashMaterial.SetFloat(powerPropertyName, power);
        }
    }

    private void ResetDashState()
    {
        canDash = true;
        dashTimer = 0f;
        isDashing = false;
    }

    private void UpdateCameraUnbind()
    {
        if (!isCameraUnbound) return;
        
        unbindTimer += Time.deltaTime;
        if (unbindTimer >= cameraUnbindDuration)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        cameraScript.RebindCamera();
        ballMovements.UnfreezeMovement();
        if (ballMesh) ballMesh.enabled = true;
        isCameraUnbound = false;
        isDashing = false;
        dashTimer = 0f;
    }

    private void HandleDashInput()
    {
        if (!ShouldDash()) return;
        
        StartDash();
        ExecuteDash();
    }

    private bool ShouldDash()
    {
        bool hasMovementInput = Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f;
        bool isOppositeKeysPressed = (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) || (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D));
        return Input.GetKeyDown(dashKey) && canDash && hasMovementInput && !isOppositeKeysPressed;
    }

    private void StartDash()
    {
        cameraScript?.UnbindCamera();
        ballMovements.FreezeMovement();
        isCameraUnbound = true;
        unbindTimer = 0f;
        if (dashMaterial != null)
        {
            dashMaterial.SetFloat(powerPropertyName, 0f);
        }
    }

    private void ExecuteDash()
    {
        Vector3 dashDirection = CalculateDashDirection();
        Vector3 targetPosition = CalculateTargetPosition(dashDirection);
        
        if (ballMesh) ballMesh.enabled = false;
        cameraScript.StartDashMovement(dashDirection, dashDistance, cameraUnbindDuration);
        ballMovements.SetDashDirection(dashDirection);
        
        vfxController.SpawnVFX(transform.position, dashDirection, dashVFXPrefab);
        transform.position = targetPosition;
        
        canDash = false;
        isDashing = true;
    }

    private Vector3 CalculateDashDirection()
    {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = cameraRight.y = 0f;
        
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        return (cameraForward.normalized * verticalInput + cameraRight.normalized * horizontalInput).normalized;
    }

    private Vector3 CalculateTargetPosition(Vector3 dashDirection)
    {
        Vector3 targetPosition = transform.position + (dashDirection * dashDistance);
        if (Physics.Raycast(transform.position, dashDirection, out RaycastHit hit, dashDistance, obstacleLayer))
        {
            targetPosition = hit.point - (dashDirection * 0.5f);
        }
        return targetPosition;
    }
}