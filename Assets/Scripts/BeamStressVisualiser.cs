using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeamStressVisualizer : MonoBehaviour
{
    [SerializeField] Gradient stressGradient;
    [SerializeField] float    reportInterval = 0.5f;

    private float lastReport;

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

    public void UpdateVisuals(List<Beam> beams)
    {
        foreach (Beam beam in beams)
        {
            if (beam == null || beam.IsBroken()) continue;
            Renderer r = beam.GetComponent<Renderer>();
            if (r != null) r.material.color = stressGradient.Evaluate(beam.StressRatio);
        }

        if (Time.time - lastReport < reportInterval) return;
        lastReport = Time.time;
        LogReport(beams);
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
}