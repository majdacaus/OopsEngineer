using UnityEngine;

public class BeamVisualiser : MonoBehaviour
{
    private Beam _beam;
    private Renderer _beamRenderer;
    void Awake()
    {
        _beam = GetComponent<Beam>();
        _beamRenderer = GetComponent<Renderer>();
    }
    
    void OnEnable()
    {
        if (_beam != null)
        {
            _beam.OnStressChanged += HandleStressVisuals;
            HandleStressVisuals(_beam.StressRatio);
        }
    }

    void OnDisable()
    {
        _beam.OnStressChanged -= HandleStressVisuals;
    }
    void HandleStressVisuals(float ratio)
    {
        if(_beamRenderer==null) return;

        Color col;
        if (ratio < 0.5f)
            col = Color.Lerp(Color.green, Color.yellow, ratio * 2f);
        else if (ratio < 1f)
            col = Color.Lerp(Color.yellow, Color.red, (ratio - 0.5f) * 2f);
        else
            col = new Color(0.4f, 0f, 0f);
        
        _beamRenderer.material.color = col;
    }
}
