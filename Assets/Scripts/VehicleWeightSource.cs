using UnityEngine;

public class VehicleWeightSource : MonoBehaviour
{
    [SerializeField] float vehicleMass = 1500f;
    [SerializeField] float contactRadius = 7.2f;

    public float ContactRadius => contactRadius;
    private StressSimulator _sim;
    public float WeightForce => vehicleMass * Mathf.Abs(Physics.gravity.y);

    private bool _hasNotified = false;

    public Vector3 GroundPoint
    {
        get
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 15f))
            {
                if (!_hasNotified && _sim != null)
                {
                    if (hit.collider.CompareTag("Road") || hit.collider.GetComponent<Beam>() != null)
                    {
                        _sim.NotifyVehicleOnBridge();
                        _hasNotified = true;
                    }
                }
                return hit.point;
            }
            return transform.position;
        }
    }

    void Start()
    {
        _sim = FindFirstObjectByType<StressSimulator>();
        if (_sim != null)
        {
            _sim.RegisterVehicle(this);
            _hasNotified = false;
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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * 15f);
    }
}