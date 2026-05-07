using System.Collections.Generic;
using UnityEngine;

public class BridgeStructure : MonoBehaviour
{
    private List<Node> _nodes = new();
    private List<Beam> _beams = new();
    
    public List<Node> Nodes => _nodes;
    public List<Beam> Beams => _beams;
    
    private void Awake()
    {
        Node[] existingNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        foreach (var node in existingNodes)
        {
            if (!_nodes.Contains(node)) _nodes.Add(node);
        }
    }
    public void AddNode(Node node) => _nodes.Add(node);
    public void AddBeam(Beam beam) => _beams.Add(beam);
    public void ClearStructure()
    {
        foreach (var beam in _beams) Destroy(beam.gameObject);
        foreach (var node in _nodes) Destroy(node.gameObject);
        _nodes.Clear();
        _beams.Clear();
    }
    
}
