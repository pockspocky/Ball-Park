using UnityEngine;

public class BallMovements : MonoBehaviour
{
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float groundCheckDistance = 0.7f;
    
    [Range(0, 1)][SerializeField] private float slowDownFactor = 0.98f;
    [Range(1, 10)][SerializeField] private float dashSpeed = 5f;
    
    public Rigidbody rb;
    private Camera mainCamera;
    private Vector3 dashDirection;

    private bool isMovementFrozen = false;
    private Vector3 storedVelocity;
    private Vector3 storedAngularVelocity;

    public void FreezeMovement()
    {
        isMovementFrozen = true;
        storedVelocity = rb.linearVelocity;
        storedAngularVelocity = rb.angularVelocity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public void SetDashDirection(Vector3 direction)
    {
        dashDirection = direction;
    }

    public void UnfreezeMovement()
    {
        isMovementFrozen = false;
        rb.isKinematic = false;
        Debug.Log(storedVelocity.magnitude);
        rb.linearVelocity = dashDirection * (storedVelocity.magnitude * dashSpeed);
        rb.angularVelocity = storedAngularVelocity;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (isMovementFrozen) return;

        if (IsGrounded())
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(moveDirection * pushForce);
            }

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity * slowDownFactor;
            }
        }
    }

    private bool IsGrounded()
    {
        float groundCheckDistance = this.groundCheckDistance; // Adjust this value based on your ball's size
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, obstacleLayer);
    }
}