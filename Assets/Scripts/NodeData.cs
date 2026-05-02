using UnityEngine;
using System.Collections.Generic;

public class NodeData : MonoBehaviour
{
    [Header("Tip čvora")]
    public bool isAnchor = false; // Ankorirani čvorovi (tlo, obala) - ne mogu se micati
    public bool isStartNode = false;

    [Header("Runtime load")]
    public float accumulatedLoad = 0f; // koliko tereta nosi ovaj čvor
    public float externalLoad = 0f;    // npr. težina vozila na ovom čvoru

    [HideInInspector] public List<BeamData> connectedBeams = new List<BeamData>();
    [HideInInspector] public bool isRevealed = false;

    private Renderer nodeRenderer;

    void Awake()
    {
        nodeRenderer = GetComponent<Renderer>();

        if (isStartNode || isAnchor)
            Reveal();
        else
            SetVisible(false);
    }

    public void Reveal()
    {
        isRevealed = true;
        SetVisible(true);
    }

    public void SetVisible(bool v)
    {
        if (nodeRenderer) nodeRenderer.enabled = v;
    }

    public bool IsConnectedTo(NodeData other)
    {
        foreach (var beam in connectedBeams)
        {
            if (beam.startNode == other || beam.endNode == other)
                return true;
        }
        return false;
    }

    // Broj greda koje ovaj čvor drže gore (ne-horizontalne)
    public int SupportBeamCount()
    {
        int count = 0;
        foreach (var b in connectedBeams)
            if (b.angle > 15f) count++;
        return count;
    }
}