using UnityEngine;

public class VehicleWeightSource : MonoBehaviour
{
    [SerializeField] float vehicleMass   = 1500f;
    [SerializeField] float contactRadius = 7.2f;

    public float   ContactRadius => contactRadius;
    private StressSimulator _sim;
    public float   WeightForce   => vehicleMass * Mathf.Abs(Physics.gravity.y);
    public Vector3 GroundPoint
    {
        get
        {
            if (Physics.Raycast(transform.position + Vector3.up*0.5f, Vector3.down, out RaycastHit hit, 10f))
                return hit.point;
            return transform.position;
        }
    }

    void Start()
    {
        Debug.Log($"[Vehicle] Start() pozvan na {gameObject.name}");
        _sim = FindObjectOfType<StressSimulator>();
        Debug.Log($"[Vehicle] Simulator pronađen: {(_sim != null ? "DA" : "NE")}");
        
        _sim = FindObjectOfType<StressSimulator>();
        if (_sim != null)
        {
            _sim.RegisterVehicle(this);
            Debug.Log($"[Vehicle] {gameObject.name} | masa={vehicleMass}kg | sila={WeightForce:F0}N");
        }
        else
        {
            Debug.LogWarning($"[Vehicle] {gameObject.name}: StressSimulator nije pronađen u sceni!");
        }
    }

    void OnDestroy()
    {
        _sim?.UnregisterVehicle(this);
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
}