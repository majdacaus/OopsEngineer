using UnityEngine;

public class CameraLock : MonoBehaviour
{
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;

    void Start()
    {
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;
    }
}