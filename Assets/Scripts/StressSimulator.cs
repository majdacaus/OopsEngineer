using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class StressSimulator : MonoBehaviour
{
    [Header("Simulacija")] 
    [SerializeField] float tickRate = 0.05f;
    [SerializeField] float breakThreshold = 1.15f;
    [SerializeField] bool enableBreaking = true;

    [Header("Reference")]
    [SerializeField] RoadBuilder roadBuilder;
    [SerializeField] ConstructionAnalyzer analyzer;

    private readonly List<Beam> registeredBeams = new();
    private readonly List<Node> registeredNodes = new();
    private readonly List<VehicleWeightSource> vehicles = new();

    private readonly Dictionary<Node, int> depthMap = new();
    private readonly Dictionary<Node, float> nodeForces = new();

    private BeamStressVisualizer visualizer;
    private AnalysisResult lastAnalysis;
    private bool hasCollapsed = false;
    private bool simulationStarted = false;
    private bool _vehicleOnBridge = false;

    public static event Action<string> OnSimulationBlocked;
    public static event Action<AnalysisResult> OnSimulationPassed;
    public static event Action OnSimulationFailed;

    public bool SimulationStarted => simulationStarted;

    void Start()
    {
        visualizer = GetComponent<BeamStressVisualizer>() ?? gameObject.AddComponent<BeamStressVisualizer>();
        if (analyzer == null) analyzer = GetComponent<ConstructionAnalyzer>();
        InvokeRepeating(nameof(RunSimulation), 0.3f, tickRate);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) StartTestMode();
        if (Input.GetKeyDown(KeyCode.B)) ResetToBuildMode();
    }

    public void StartTestMode()
    {
        if (simulationStarted) return;
        if (analyzer == null) return;

        simulationStarted = true;
        hasCollapsed = false;
        _vehicleOnBridge = false;
    }

    public bool TryStartTestMode()
    {
        if (simulationStarted) return false;
        if (analyzer == null) return false;

        AnalysisResult check = analyzer.PerformFullAnalysis(registeredNodes, registeredBeams);
        if (!check.PathExists)
        {
            OnSimulationBlocked?.Invoke("Most ne spaja obje strane!");
            return false;
        }

        simulationStarted = true;
        hasCollapsed = false;
        _vehicleOnBridge = false;
        
        FindFirstObjectByType<BridgeAdvisor>()?.StartStressMonitoring();
        return true;
    }

    public void RegisterBeam(Beam beam)
    {
        if (beam != null && !registeredBeams.Contains(beam))
        {
            registeredBeams.Add(beam);
            visualizer.RegisterBeam(beam);
        }
    }

    public void RegisterNode(Node node)
    {
        if (node != null && !registeredNodes.Contains(node))
            registeredNodes.Add(node);
    }

    public void RegisterVehicle(VehicleWeightSource v) => vehicles.Add(v);
    public void UnregisterVehicle(VehicleWeightSource v) => vehicles.Remove(v);
    public void NotifyVehicleOnBridge() => _vehicleOnBridge = true;

    void RunSimulation()
    {
        if (!simulationStarted || hasCollapsed || analyzer == null || registeredBeams.Count == 0) return;
        lastAnalysis = analyzer.PerformFullAnalysis(registeredNodes, registeredBeams);

        ResetForces();
        BuildDepthMap();
        ApplyVehicleLoads();
        PropagateForces();
        ComputeAndApplyStress();
        
        if (enableBreaking) CheckBreakage();
        if (!hasCollapsed)
            visualizer.UpdateVisuals(registeredBeams);
    }

    void ResetForces()
    {
        nodeForces.Clear();
        foreach (Node n in registeredNodes) nodeForces[n] = 0f;
        foreach (Beam b in registeredBeams)
        {
            if (b != null && !b.IsBroken()) b.UpdateStress(0f);
        }
    }

    void BuildDepthMap()
    {
        depthMap.Clear();
        Queue<Node> queue = new();

        foreach (Node n in registeredNodes.Where(n => n != null && n.IsAnchor))
        {
            depthMap[n] = 0;
            queue.Enqueue(n);
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Beam beam in current.GetConnectedBeams())
            {
                if (beam == null || beam.IsBroken()) continue;
                Node neighbour = beam.GetOtherNode(current);
                if (neighbour == null || depthMap.ContainsKey(neighbour)) continue;
                depthMap[neighbour] = depthMap[current] + 1;
                queue.Enqueue(neighbour);
            }
        }
    }

    void ApplyVehicleLoads()
    {
        foreach (VehicleWeightSource vehicle in vehicles)
        {
            if (vehicle == null) continue;

            Vector3 pos = vehicle.GroundPoint;
            float force = vehicle.WeightForce;
            float radius = vehicle.ContactRadius;

            var nearby = registeredNodes
                .Where(n => n != null && depthMap.ContainsKey(n) && Vector3.Distance(pos, n.transform.position) < radius)
                .Select(n => (node: n, invDist: 1f / Mathf.Max(Vector3.Distance(pos, n.transform.position), 0.05f)))
                .ToList();

            float totalWeight = nearby.Sum(x => x.invDist);
            if (totalWeight <= 0f) continue;

            foreach (var (node, invDist) in nearby)
                nodeForces[node] += force * (invDist / totalWeight);
        }
    }

    void PropagateForces()
    {
        if (depthMap.Count == 0) return;
        int maxDepth = depthMap.Values.Max();

        for (int depth = maxDepth; depth > 0; depth--)
        {
            foreach (Node node in registeredNodes.Where(n => n != null && depthMap.ContainsKey(n) && depthMap[n] == depth))
            {
                if (!nodeForces.TryGetValue(node, out float force) || force <= 0f) continue;

                var outgoing = node.GetConnectedBeams()
                    .Where(b => b != null && !b.IsBroken() && depthMap.ContainsKey(b.GetOtherNode(node))
                                              && depthMap[b.GetOtherNode(node)] < depth)
                    .ToList();

                if (outgoing.Count == 0) continue;

                float totalStiffness = outgoing.Sum(b => GetEffectiveStiffness(b, node));
                if (totalStiffness <= 0f) continue;

                foreach (Beam beam in outgoing)
                {
                    float eff = GetEffectiveStiffness(beam, node);
                    float share = eff / totalStiffness;
                    float beamForce = force * share;

                    Node next = beam.GetOtherNode(node);
                    nodeForces[next] = nodeForces.GetValueOrDefault(next, 0f) + beamForce;
                }
            }
        }
    }

    float GetEffectiveStiffness(Beam beam, Node fromNode)
    {
        if (!BeamMaterialProperties.IsCable(beam.MaterialType))
            return BeamMaterialProperties.GetStiffness(beam.MaterialType);

        Vector3 toOther = beam.GetOtherNode(fromNode).transform.position - fromNode.transform.position;
        return toOther.y > 0.05f ? BeamMaterialProperties.GetStiffness(beam.MaterialType) : 0f;
    }

    void ComputeAndApplyStress()
    {
        
        foreach (Beam beam in registeredBeams)
        {
            if (beam == null || beam.IsBroken()) continue;
            float forceA = nodeForces.GetValueOrDefault(beam.StartNode, 0f);
            float forceB = nodeForces.GetValueOrDefault(beam.EndNode, 0f);
            beam.UpdateStress((forceA + forceB) / 2f);
        }
    }

    void CheckBreakage()
    {
        if (!simulationStarted || hasCollapsed || lastAnalysis == null || !_vehicleOnBridge) return;
    
        if (lastAnalysis.StructuralHealth < 60f)
        {
            TriggerCollapse();
        }
    }

    // void TriggerCollapse()
    // {
    //     hasCollapsed = true;
    //     simulationStarted = false;
    //
    //     foreach (var beam in registeredBeams)
    //         if (beam != null) StartCoroutine(SinkAndFade(beam.gameObject));
    //
    //     foreach (var node in registeredNodes)
    //         if (node != null && !node.IsAnchor) StartCoroutine(SinkAndFade(node.gameObject));
    //
    //     if (roadBuilder != null) roadBuilder.ClearAllRoad(); 
    //     OnSimulationFailed?.Invoke();
    //     Invoke(nameof(ResetToBuildMode), 4f);
    // }
    //
    void TriggerCollapse()
    {
        hasCollapsed = true;
        simulationStarted = false;

        
        if (visualizer != null) visualizer.ClearData();
        List<Beam> beamsCopy = new List<Beam>(registeredBeams);
        List<Node> nodesCopy = new List<Node>(registeredNodes);

        registeredBeams.Clear();
        registeredNodes.Clear();

        foreach (var beam in beamsCopy)
        {
            if (beam != null) StartCoroutine(SinkAndFade(beam.gameObject));
        }

        foreach (var node in nodesCopy)
        {
            if (node != null && !node.IsAnchor) 
                StartCoroutine(SinkAndFade(node.gameObject));
        }
        
        foreach (var vehicle in vehicles)
        {
            if (vehicle != null)
            {
                if (vehicle.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;rb.useGravity = true;
                }
                var controller = vehicle.GetComponent<MonoBehaviour>(); 
                if (controller != null) controller.enabled = false;
            }
        }

        if (roadBuilder != null) roadBuilder.ClearAllRoad();
    
        OnSimulationFailed?.Invoke();
        Invoke(nameof(ResetToBuildMode), 4f);
    }

    private IEnumerator SinkAndFade(GameObject obj)
    {
        float duration = 3.0f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 startScale = obj.transform.localScale;
        Quaternion startRot = obj.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(-45f, 45f), Random.Range(-45f, 45f));
        Vector3 targetPos = startPos + Vector3.down * 7f + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

        if (obj.TryGetComponent(out Collider col)) col.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothPercent = Mathf.SmoothStep(0, 1, elapsed / duration);
            obj.transform.position = Vector3.Lerp(startPos, targetPos, smoothPercent);
            obj.transform.rotation = Quaternion.Lerp(startRot, targetRot, smoothPercent);
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, smoothPercent);
            yield return null;
        }
        Destroy(obj);
    }

    public void ResetToBuildMode()
    {
        FindFirstObjectByType<BridgeAdvisor>()?.StopStressMonitoring();

        StopAllCoroutines();

        foreach (var beam in registeredBeams)
        {
            if (beam != null) 
            {
                beam.UpdateStress(0f);
                Destroy(beam.gameObject);
            }
        }
        
        foreach (var node in registeredNodes)
            if (node != null && !node.IsAnchor) Destroy(node.gameObject);
        
        registeredBeams.Clear();
        registeredNodes.Clear();
        simulationStarted = false;
        hasCollapsed = false;
        _vehicleOnBridge = false;
    }
}