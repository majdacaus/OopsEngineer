using UnityEngine;

public class VehicleWeightSource : MonoBehaviour
{
    [SerializeField] float vehicleMass   = 1500f;
    [SerializeField] float contactRadius = 3.5f;

    public float   ContactRadius => contactRadius;
    public float   WeightForce   => vehicleMass * Mathf.Abs(Physics.gravity.y);
    public Vector3 GroundPoint
    {
        get
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f))
                return hit.point;
            return transform.position;
        }
    }

    void Start()
    {
        StressSimulator sim = FindObjectOfType<StressSimulator>();
        if (sim != null)
        {
            sim.RegisterVehicle(this);
            Debug.Log($"[Vehicle] {gameObject.name} | masa={vehicleMass}kg | sila={WeightForce:F0}N");
        }
        else
        {
            Debug.LogWarning($"[Vehicle] {gameObject.name}: StressSimulator nije pronađen u sceni!");
        }
    }

    void OnDestroy()
    {
        FindObjectOfType<StressSimulator>()?.UnregisterVehicle(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
}