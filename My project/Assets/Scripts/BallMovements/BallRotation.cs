using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    private Transform _transform;

    
    private void Start()
    {
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        _transform.position = target.position - new Vector3(0, 0.01f,0);
    }
}