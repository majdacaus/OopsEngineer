using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StressSimulator : MonoBehaviour
{
    [Header("Simulacija")]
    [SerializeField] float tickRate       = 0.05f;
    [SerializeField] float breakThreshold = 1.15f;
    [SerializeField] bool  enableBreaking = true;

    //[Header("Efekti")]
    //[SerializeField] GameObject breakParticlePrefab;
    
    [SerializeField] ConstructionAnalyzer analyzer;

    private readonly List<Beam>              registeredBeams = new();
    private readonly List<Node>              registeredNodes = new();
    private readonly List<VehicleWeightSource> vehicles      = new();

    private readonly Dictionary<Node, int>   depthMap   = new();
    private readonly Dictionary<Node, float> nodeForces = new();

    private BeamStressVisualizer visualizer;
    private AnalysisResult lastAnalysis;
    private bool hasCollapsed = false;

    void Start()
    {
        visualizer = GetComponent<BeamStressVisualizer>() ?? gameObject.AddComponent<BeamStressVisualizer>();
        
        if (analyzer == null) analyzer = GetComponent<ConstructionAnalyzer>();
        InvokeRepeating(nameof(RunSimulation), 0.3f, tickRate);
        Debug.Log("[StressSimulator] Pokrenut.");
    }

    public void RegisterBeam(Beam beam)
    {
        if (beam != null && !registeredBeams.Contains(beam))
        {
            registeredBeams.Add(beam);
            visualizer.RegisterBeam(beam);
            Debug.Log($"[StressSimulator] Gred: {beam.name} | {beam.MaterialType} | MaxLoad={beam.MaxLoad:F0}");
        }
    }

    public void RegisterNode(Node node)
    {
        if (node != null && !registeredNodes.Contains(node))
            registeredNodes.Add(node);
    }

    public void RegisterVehicle(VehicleWeightSource v)
    {
        if (!vehicles.Contains(v)) vehicles.Add(v);
    }

    public void UnregisterVehicle(VehicleWeightSource v) => vehicles.Remove(v);

    void RunSimulation()
    {
        Debug.Log($"[Sim] Tick | Beams={registeredBeams.Count} | Nodes={registeredNodes.Count} | Vehicles={vehicles.Count} | Analyzer={analyzer != null}");
        
        // if (registeredBeams.Count == 0) { Debug.Log("Nema registrovanih greda!"); return; }
        // if (analyzer == null) { Debug.Log("Analyzer je još uvijek NULL!"); return; }
        if (analyzer == null) return;
        if (registeredBeams.Count == 0) return;

        lastAnalysis = analyzer.PerformFullAnalysis(registeredNodes, registeredBeams);
        
        ResetForces();
        BuildDepthMap();
        ApplyVehicleLoads();
        PropagateForces();
        
        ComputeAndApplyStress();
        if (enableBreaking) CheckBreakage();

        visualizer.UpdateVisuals(registeredBeams);
    }

    void ResetForces()
    {
        nodeForces.Clear();
        foreach (Node n in registeredNodes) nodeForces[n] = 0f;
        foreach (Beam b in registeredBeams) { if (!b.IsBroken()) b.UpdateStress(0f); }
    }

    // void BuildDepthMap()
    // {
    //     depthMap.Clear();
    //     Queue<Node> queue = new();
    //
    //     foreach (Node n in registeredNodes.Where(n => n.IsAnchor))
    //     {
    //         depthMap[n] = 0;
    //         queue.Enqueue(n);
    //     }
    //
    //     while (queue.Count > 0)
    //     {
    //         Node current = queue.Dequeue();
    //         foreach (Beam beam in current.GetConnectedBeams())
    //         {
    //             if (beam.IsBroken()) continue;
    //             Node neighbour = beam.GetOtherNode(current);
    //             if (neighbour == null || depthMap.ContainsKey(neighbour)) continue;
    //             depthMap[neighbour] = depthMap[current] + 1;
    //             queue.Enqueue(neighbour);
    //         }
    //     }
    // }
    
    void BuildDepthMap()
    {
        depthMap.Clear();
        Queue<Node> queue = new();

        foreach (Node n in registeredNodes.Where(n => n.IsAnchor))
        {
            depthMap[n] = 0;
            queue.Enqueue(n);
            Debug.Log($"[DepthMap] Anchor: {n.name} | ConnectedBeams={n.GetConnectedBeams().Count}");
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Beam beam in current.GetConnectedBeams())
            {
                if (beam.IsBroken()) continue;
                Node neighbour = beam.GetOtherNode(current);
                if (neighbour == null || depthMap.ContainsKey(neighbour)) continue;
                depthMap[neighbour] = depthMap[current] + 1;
                queue.Enqueue(neighbour);
            }
        }
    
        Debug.Log($"[DepthMap] ZAVRŠEN: {depthMap.Count}/{registeredNodes.Count} nodova dosegnuto");
        foreach(Node n in registeredNodes.Where(n => !depthMap.ContainsKey(n)))
            Debug.Log($"[DepthMap] NIJE DOSEGNUT: {n.name} | Beams={n.GetConnectedBeams().Count}");
    }

    void ApplyVehicleLoads()
    {
        foreach (VehicleWeightSource vehicle in vehicles)
        {
            if (vehicle == null) continue;

            Vector3 pos    = vehicle.GroundPoint;
            float   force  = vehicle.WeightForce;
            float   radius = vehicle.ContactRadius;

            var nearby = registeredNodes
                .Where(n => depthMap.ContainsKey(n) && Vector3.Distance(pos, n.transform.position) < radius)
                .Select(n => (node: n, invDist: 1f / Mathf.Max(Vector3.Distance(pos, n.transform.position), 0.05f)))
                .ToList();
            
            Debug.Log($"[Vehicle] GroundPoint={pos} | WeightForce={force:F0}N | Radius={radius} | NearbyNodes={nearby.Count}");
            Debug.Log($"[Vehicle] Ukupno registrovanih nodova: {registeredNodes.Count} | U depthMap: {depthMap.Count}");
            foreach (var (node, invDist) in nearby)
                Debug.Log($"  → Node '{node.name}' na dist={1f/invDist:F2} prima dio sile");

            float totalWeight = nearby.Sum(x => x.invDist);
            if (totalWeight <= 0f) continue;

            foreach (var (node, invDist) in nearby)
                nodeForces[node] += force * (invDist / totalWeight);

            //Debug.Log($"[StressSimulator] {vehicle.gameObject.name}: {force:F0}N → {nearby.Count} čvorova");
        }
    }

    void PropagateForces()
    {
        if (depthMap.Count == 0) return;

        int maxDepth = depthMap.Values.Max();

        for (int depth = maxDepth; depth > 0; depth--)
        {
            foreach (Node node in registeredNodes.Where(n => depthMap.ContainsKey(n) && depthMap[n] == depth))
            {
                if (!nodeForces.TryGetValue(node, out float force) || force <= 0f) continue;

                var outgoing = node.GetConnectedBeams()
                    .Where(b => !b.IsBroken() && depthMap.ContainsKey(b.GetOtherNode(node))
                                && depthMap[b.GetOtherNode(node)] < depth)
                    .ToList();

                if (outgoing.Count == 0) continue;

                float totalStiffness = outgoing.Sum(b => GetEffectiveStiffness(b, node));
                if (totalStiffness <= 0f) continue;

                foreach (Beam beam in outgoing)
                {
                    float eff = GetEffectiveStiffness(beam, node);
                    if (eff <= 0f) continue;

                    float share = eff / totalStiffness;
                    float beamForce = force * share;

                   // beam.UpdateStress(beamForce);

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
        if (toOther.y > 0.05f)
            return BeamMaterialProperties.GetStiffness(beam.MaterialType);

        Debug.Log($"[StressSimulator] Kabel '{beam.name}' preskočen – kablovi prenose samo vlak.");
        return 0f;
    }

    void ComputeAndApplyStress()
    {
        foreach (Beam beam in registeredBeams)
        {
            if (beam.IsBroken()) continue;
            float forceA = nodeForces.GetValueOrDefault(beam.StartNode, 0f);
            float forceB = nodeForces.GetValueOrDefault(beam.EndNode, 0f);
            float totalForce = (forceA + forceB) / 2f;
            beam.UpdateStress(totalForce);
            
            if(totalForce >10f)
                Debug.Log($"BEAM '{beam.name}' ForceA '{forceA:F0}' ForceB '{forceB:F0}'");
        }
    }

    void CheckBreakage()
    {
        foreach (Beam beam in registeredBeams)
        {
            if (beam.IsBroken()) continue;

            float healthFactor = (lastAnalysis !=  null && lastAnalysis.StructuralHealth<60f) ? 2.0f : 1.0f;
            if (beam.StressRatio > 0.05f)
                Debug.Log($"[Break?] '{beam.name}' | Ratio={beam.StressRatio:F3} | HealthFactor={healthFactor} " +
                          $"| Effective={(beam.StressRatio * healthFactor):F3} | Threshold={breakThreshold}");
            if ((beam.StressRatio * healthFactor) < breakThreshold) continue;
            
            beam.UpdateStress(beam.MaxLoad + 1f);
            Debug.LogWarning($"[StressSimulator] *** GRED PUKLA *** {beam.name} | {beam.MaterialType} | Napon: {beam.CurrentStress:F0}/{beam.MaxLoad:F0}");

            if (!hasCollapsed)
            {
                hasCollapsed = true;
                visualizer.TriggerCollapse(registeredBeams, registeredNodes);
            }
            
            Renderer r = beam.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.12f, 0.04f, 0f);

            // if (breakParticlePrefab != null)
            //     Instantiate(breakParticlePrefab, beam.transform.position, Quaternion.identity);
        }
    }
}