using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StressSimulator : MonoBehaviour
{
    [Header("Simulacija")]
    [Tooltip("Koliko puta iteriramo distribuciju tereta (više = preciznije)")]
    [SerializeField] int iterations = 5;

    [Tooltip("Vlastita težina svake grede po jedinici dužine")]
    [SerializeField] float beamWeightPerUnit = 0.1f;

    [Tooltip("Koliko traje animacija pucanja grede")]
    [SerializeField] float breakAnimDuration = 0.5f;

    [Header("Događaji")]
    public System.Action<BeamData> onBeamBroken;
    public System.Action onBridgeCollapsed; // pozvati game over

    // ─────────────────────────────────────────────────────────
    // GLAVNA METODA — zovi je svaki put kad se most promijeni
    // ili kad se doda novo opterećenje (vozilo)
    // ─────────────────────────────────────────────────────────
    public void RunSimulation(List<NodeData> nodes, List<BeamData> beams)
    {
        
        // Ukloni sve uništene beame iz liste prije simulacije
        beams.RemoveAll(b => b == null || b.gameObject == null);
        nodes.RemoveAll(n => n == null || n.gameObject == null);
        
        foreach (var node in nodes)
        {
            if (node == null) continue;  // dodatna zaštita
            node.accumulatedLoad = node.externalLoad;
        }

        foreach (var beam in beams)
        {
            if (beam == null) continue;
            beam.stressRatio = 0f;
        }

        // KORAK 2: Dodaj vlastitu težinu greda kao load na njihove čvorove
        foreach (var beam in beams)
        {
            float beamWeight = beam.length * beamWeightPerUnit;
            // Polovina težine ide na svaki kraj
            if (beam.startNode) beam.startNode.accumulatedLoad += beamWeight * 0.5f;
            if (beam.endNode)   beam.endNode.accumulatedLoad   += beamWeight * 0.5f;
        }

        // KORAK 3: Iterativna propagacija
        // Čvorovi koji su slobodni (ne-ankorirani) distribuiraju load
        // na susjedne grede proporcionalno njihovoj efikasnosti
        for (int i = 0; i < iterations; i++)
        {
            PropagateLoads(nodes);
        }

        // KORAK 4: Izračunaj stress svake grede
        foreach (var beam in beams)
        {
            float totalLoad = 0f;
            if (beam.startNode) totalLoad += beam.startNode.accumulatedLoad * 0.5f;
            if (beam.endNode)   totalLoad += beam.endNode.accumulatedLoad   * 0.5f;

            // Dulja greda = manji stress per unit, ali manja snaga
            float capacity = beam.StrengthFactor;
            beam.stressRatio = totalLoad / Mathf.Max(capacity, 0.01f);

            beam.UpdateVisual();
        }

        // KORAK 5: Provjeri pucanja
        CheckForBreaks(beams);
    }

    void PropagateLoads(List<NodeData> nodes)
    {
        foreach (var node in nodes)
        {
            
            if (node == null) continue;  // ← dodaj ovo

            if (node.isAnchor) continue;
            if (node.connectedBeams.Count == 0) continue;

            // Ukloni uništene beame i s čvora
            node.connectedBeams.RemoveAll(b => b == null);
            // Ankorirani čvorovi upijaju load — ne propagiraju dalje


            // Izračunaj ukupnu efikasnost svih greda na ovom čvoru
            float totalEfficiency = 0f;
            foreach (var beam in node.connectedBeams)
                totalEfficiency += beam.EfficiencyFactor;

            if (totalEfficiency <= 0f) continue;

            // Distribuiraj load proporcionalno efikasnosti
            float loadToDistribute = node.accumulatedLoad * 0.15f; // 40% se širi dalje

            foreach (var beam in node.connectedBeams)
            {
                float share = (beam.EfficiencyFactor / totalEfficiency) * loadToDistribute;

                // Šalji load na suprotni kraj grede
                NodeData other = (beam.startNode == node) ? beam.endNode : beam.startNode;
                if (other != null)
                    other.accumulatedLoad += share;
            }
        }
    }

    void CheckForBreaks(List<BeamData> beams)
    {
        List<BeamData> broken = new List<BeamData>();

        foreach (var beam in beams)
            if (beam.IsBroken()) broken.Add(beam);

        if (broken.Count > 0)
            StartCoroutine(BreakBeams(broken));
    }

    IEnumerator BreakBeams(List<BeamData> beams)
    {
        foreach (var beam in beams)
        {
            if (beam == null) continue; 
            
            onBeamBroken?.Invoke(beam);

            // Animacija: greda pada/nestaje
            float t = 0f;
            Vector3 originalPos = beam.transform.position;

            while (t < breakAnimDuration)
            {
                if (beam == null) break;  
                t += Time.deltaTime;
                float progress = t / breakAnimDuration;

                // Pada dolje i nestaje
                beam.transform.position = originalPos + Vector3.down * progress * 3f;

                Renderer r = beam.GetComponent<Renderer>();
                if (r)
                {
                    Color c = r.material.color;
                    c.a = 1f - progress;
                    r.material.color = c;
                }

                yield return null;
            }
            if (beam != null)
            Destroy(beam.gameObject);
        }

        onBridgeCollapsed?.Invoke();
    }
}