using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    private Transform _transform;

    
    private void Start()
    {
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        _transform.position = target.position;
    }
}