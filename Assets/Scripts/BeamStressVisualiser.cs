using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeamStressVisualizer : MonoBehaviour
{
    [SerializeField] Gradient stressGradient;
    [SerializeField] float    reportInterval = 0.5f;

    [SerializeField] float    shakeThreshold = 0.7f;
    [SerializeField] float    shakeAmount = 0.05f;
    
    private float lastReport;

    private Dictionary<Beam, Vector3> originalPositions = new Dictionary<Beam, Vector3>();
    
    public void UpdateVisuals(List<Beam> beams)
    {
        if (beams == null || beams.Count == 0) return;
        
        foreach (Beam beam in beams)
        {
            if (beam == null || !beam.gameObject) continue;
            
            if (!originalPositions.ContainsKey(beam))
                originalPositions[beam] = beam.transform.localPosition;

            if (beam.IsBroken()) continue;
            Renderer r = beam.GetComponent<Renderer>();
            if (r != null) r.material.color = stressGradient.Evaluate(beam.StressRatio);

            HandleBeamShake(beam);
        }

        if (Time.time - lastReport < reportInterval) return;
        lastReport = Time.time;
        LogReport(beams);
    }
    
    private void HandleBeamShake(Beam beam)
    {
        float ratio = beam.StressRatio;

        if (ratio > shakeThreshold)
        {float currentShake = (ratio - shakeThreshold) * shakeAmount;
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake)
            );

            beam.transform.localPosition = originalPositions[beam] + randomOffset;
        }
        else
        {
            beam.transform.localPosition = originalPositions[beam];
        }
    }
            
    void Reset()
    {
        stressGradient = new Gradient();
        stressGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.1f, 0.9f, 0.2f), 0.00f),
                new GradientColorKey(new Color(1.0f, 0.9f, 0.0f), 0.55f),
                new GradientColorKey(new Color(1.0f, 0.3f, 0.0f), 0.85f),
                new GradientColorKey(new Color(0.9f, 0.0f, 0.0f), 1.00f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }

    public void RegisterBeam(Beam beam)
    {
        if(beam != null && !originalPositions.ContainsKey(beam))
            originalPositions[beam] = beam.transform.localPosition;
    }
    public void TriggerCollapse(List<Beam> beams, List<Node> nodes)
    {
        foreach (Beam beam in beams)
        {
            if (beam == null) continue;
            Rigidbody rb = beam.GetComponent<Rigidbody>();
            if (rb == null) rb = beam.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity  = true;
    
            rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
    
        foreach (Node node in nodes)
        {
            if (node == null || node.IsAnchor) continue; 
            Rigidbody rb = node.GetComponent<Rigidbody>();
            if (rb == null) rb = node.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity  = true;
        }
    }
  
    void LogReport(List<Beam> beams)
    {
        Beam worst = beams
            .Where(b => b != null && !b.IsBroken())
            .OrderByDescending(b => b.StressRatio)
            .FirstOrDefault();

        if (worst == null || worst.StressRatio < 0.05f) return;

        string level = worst.StressRatio switch
        {
            >= 1.0f  => "KRITIČNO",
            >= 0.75f => "OPASNO",
            >= 0.5f  => "UPOZORENJE",
            _        => "OK"
        };

        Debug.Log($"[Vizualizer] [{level}] '{worst.name}' ({worst.MaterialType}) | {worst.CurrentStress:F0}/{worst.MaxLoad:F0} | {worst.StressRatio * 100f:F1}%");
    }
    
    public void ClearData()
    {
        originalPositions.Clear();
        lastReport = 0;
    }
}