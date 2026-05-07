using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
    [SerializeField] private float positionSmoothing = 5f;
    [SerializeField] private float rotationSmoothing = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmoothing * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target.rotation,
            rotationSmoothing * Time.deltaTime
        );
    }
}