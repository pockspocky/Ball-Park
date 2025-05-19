using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private bool isResetting;

    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private float resetDuration = 1f;
    [SerializeField] private AnimationCurve resetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey) && !isResetting)
        {
            ResetObject();
        }
    }

    private void ResetObject()
    {
        if (isResetting) return;
        
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        StartCoroutine(SmoothReset());
    }

    private IEnumerator SmoothReset()
    {
        isResetting = true;
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = resetCurve.Evaluate(elapsedTime / resetDuration);

            transform.position = Vector3.Lerp(startPosition, initialPosition, t);
            transform.rotation = Quaternion.Lerp(startRotation, initialRotation, t);

            yield return null;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isResetting = false;
    }

    public void TriggerReset()
    {
        ResetObject();
    }
}