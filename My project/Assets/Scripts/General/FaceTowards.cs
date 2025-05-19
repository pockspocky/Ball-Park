using UnityEngine;

public class FaceTowards : MonoBehaviour
{
    [SerializeField] private GameObject objectToRotate; // 需要旋转的物体
    [SerializeField] private float maxRayDistance = 100f; // 射线最大距离

    void Update()
    {
        // 从相机中心发射射线
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        // 检查射线是否击中物体
        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            // 如果没有击中，使用最大距离处的点
            targetPoint = ray.origin + ray.direction * maxRayDistance;
        }

        // 使物体朝向目标点
        if (objectToRotate != null)
        {
            objectToRotate.transform.LookAt(targetPoint);
        }
    }
}