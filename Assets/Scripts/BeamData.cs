using UnityEngine;

// Ova komponenta ide na beamPrefab!
public class BeamData : MonoBehaviour
{
    [Header("Strukturni podaci")]
    public NodeData startNode;
    public NodeData endNode;
    public float length;
    public float angle; // kut od horizontale (0° = horizontalan, 90° = vertikalan)

    [Header("Stress (0 = nema, 1 = maksimum, >1 = pukao)")]
    [Range(0f, 1.5f)] public float stressRatio = 0f;
    public float maxLoad = 100f; // koliko može nositi prije pucanja

    // Koliko efektivno prenosi load ovisno o kutu
    // Horizontalne grede su najefikasnije za most
    // Dijagonalne grede prenose manji udio vertikalnog tereta
    public float EfficiencyFactor => Mathf.Cos(angle * Mathf.Deg2Rad);

    // Duže grede su slabije (realna mehanika)
    public float StrengthFactor => maxLoad / Mathf.Max(length, 0.1f);

    private Renderer beamRenderer;

    void Awake()
    {
        beamRenderer = GetComponent<Renderer>();
    }

    // Poziva StressSimulator svaki put kad se most promijeni
    public void UpdateVisual()
    {
        if (beamRenderer == null) return;

        // Interpoliraj boju: zelena(0) → žuta(0.5) → crvena(1) → tamnocrvena(>1)
        Color col;
        if (stressRatio < 0.5f)
            col = Color.Lerp(Color.green, Color.yellow, stressRatio * 2f);
        else if (stressRatio < 1f)
            col = Color.Lerp(Color.yellow, Color.red, (stressRatio - 0.5f) * 2f);
        else
            col = new Color(0.4f, 0f, 0f); // tamnocrvena = puknuta

        beamRenderer.material.color = col;
    }

    public bool IsBroken() => stressRatio >= 1f;
}