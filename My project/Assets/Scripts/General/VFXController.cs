using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private bool autoDestroy = true;

    private VisualEffect activeVFX;

    public void SpawnVFX(Vector3 position, Vector3 rotation, GameObject vfxPrefab, bool isBehind = true)
    {
        GameObject vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);
        activeVFX = vfxInstance.GetComponent<VisualEffect>();
        
        Debug.Log($"VFX spawned at {position} with rotation {rotation.normalized * 180}");
        
        // set the rotation of the VFX to the opposite direction of the parameter rotation
        if (isBehind)
        {
            vfxInstance.transform.forward = rotation.normalized * 180 * -1;
        }
        else
        {
            vfxInstance.transform.forward = rotation.normalized;
        }

        if (activeVFX == null)
        {
            Debug.LogWarning("VFX prefab does not contain a VisualEffect component!");
            return;
        }

        if (autoDestroy)
        {
            Destroy(vfxInstance, lifetime);
        }
    }

    public void StopVFX()
    {
        if (activeVFX != null)
        {
            activeVFX.Stop();
        }
    }

    public VisualEffect GetActiveVFX()
    {
        return activeVFX;
    }
}