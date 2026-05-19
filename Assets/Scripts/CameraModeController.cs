using UnityEngine;

public class CameraModeController : MonoBehaviour
{
    [SerializeField] private CameraLock lockScript;
    [SerializeField] private CameraFollow followScript;

    void Awake()
    {
        if(lockScript == null) lockScript = GetComponent<CameraLock>();
        if(followScript == null) followScript = GetComponent<CameraFollow>();
        
        SetBuildMode();
    }

    public void SetTestMode(Transform vehicleTransform = null)
    {
        Debug.Log($"[Camera] lockScript: {lockScript}, followScript: {followScript}");
    
        if (lockScript) lockScript.enabled = false;
        if (followScript) followScript.enabled = true;

        var vehicle = FindFirstObjectByType<VehicleWeightSource>();
        Debug.Log($"[Camera] Vehicle found: {vehicle}");
    
        if (vehicle != null && followScript != null)
            followScript.SetTarget(vehicle.transform);
    }

    public void SetBuildMode()
    {
        if (lockScript) lockScript.enabled = true;
        if (followScript) followScript.enabled = false;
    }
}