using System.Collections.Generic;
using UnityEngine;

public class ConstructionAnalyzer : MonoBehaviour
{
    [Header("Budget Settings")]
    public float maxBudget = 300000f;

    private const float TOLERANCE = 0.1f;
    
    public AnalysisResult PerformFullAnalysis(List<Node> nodes, List<Beam> beams)
    {
        nodes = nodes.FindAll(n => n.gameObject.activeInHierarchy);
        beams = beams.FindAll(b => b.gameObject.activeInHierarchy);
        
        AnalysisResult res = new AnalysisResult();
        if (nodes.Count < 2) return res;
        
        List<Node> anchors = nodes.FindAll(n => n.IsAnchor);
        anchors.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        res.PathExists = anchors.Count >= 2 && CanBuildRoad(anchors[0], anchors[anchors.Count - 1]);
        if (!res.PathExists)
            res.AdviceMessages.Add("We have a problem. The bridge doesn't actually reach the other side!");

        res.IsFloating = CheckIfFloating(nodes);
        if (res.IsFloating)
            res.AdviceMessages.Add("Are you building a bridge or a kite? Some parts are literally floating in the air!");

        res.HasWeakMaterials = CheckMaterialLimits(beams);
        if (res.HasWeakMaterials)
            res.AdviceMessages.Add("Size matters! Some beams are too long and will snap like a toothpick.");

        res.IsLackingTriangles = CheckTriangulation(nodes);
        if (res.IsLackingTriangles)
            res.AdviceMessages.Add("Square shapes are for boxes, not bridges. Add some triangles or watch it fold like an omelet!");

        res.CurrentCost = CalculateCost(beams);
        if (res.CurrentCost > maxBudget)
            res.AdviceMessages.Add($"Our pockets are empty! You're ${res.CurrentCost - maxBudget:F0} over budget. Chill with the steel!");

        res.StructuralHealth = CalculateStructuralHealth(res);
        res.PotentialPoints  = CalculatePotentialPoints(res.StructuralHealth, res.CurrentCost);

        if (res.CurrentCost < maxBudget * 0.5f)
            res.AdviceMessages.Add("Budget King! You're building this for pennies.");
        else if (res.CurrentCost > maxBudget)
            res.AdviceMessages.Add("Overbudget! The city council is going to kill us.");

        return res;
    }

    private float CalculateCost(List<Beam> beams)
    {
        float total = 0f;
        float steelT = 0, woodT = 0, cableT = 0;
        foreach (Beam b in beams)
        {
            float multiplier = b.MaterialType switch
            {
                BeamMaterialType.Steel => 50f,
                BeamMaterialType.Cable => 30f,
                _                      => 10f
            };
            float cost = b.Length * multiplier;
            total += cost;
            
            //zbir za debug
            if (b.MaterialType == BeamMaterialType.Steel) steelT += cost;
            else if (b.MaterialType == BeamMaterialType.Cable) cableT += cost;
            else woodT += cost;
        }
        Debug.Log($"<color=cyan>BUDŽET INFO:</color> Čelik: ${steelT:F0}, Drvo: ${woodT:F0}, Kablovi: ${cableT:F0}");
        return total;
    }

    public bool CanBuildRoad(Node startNode, Node endNode)
    {
        //if (startNode == null || endNode == null) return false;
        if (startNode == null) return false;

        Queue<Node>   queue   = new();
        HashSet<Node> visited = new();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        float targetZ = startNode.transform.position.z + 8.0f;
        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            //if (current == endNode) return true;
            float distanceTravelled = Mathf.Abs(current.transform.position.z - startNode.transform.position.z);

            if (current.IsAnchor && distanceTravelled > 8.0f) 
                return true; 
            foreach (Beam beam in current.GetConnectedBeams())
            {
                // float heightDiff = Mathf.Abs(beam.StartNode.transform.position.y - beam.EndNode.transform.position.y);
                // if (heightDiff >= 3.0f) continue;

                Node next = beam.StartNode == current ? beam.EndNode : beam.StartNode;
                if (next != null && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
               
            }
        }
        return false;
    }

    private bool CheckIfFloating(List<Node> allNodes)
    {
        HashSet<Node> connected = new();
        Queue<Node>   queue     = new();

        foreach (Node node in allNodes)
        {
            if (!node.IsAnchor) continue;
            queue.Enqueue(node);
            connected.Add(node);
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Beam beam in current.GetConnectedBeams())
            {
                Node next = beam.StartNode == current ? beam.EndNode : beam.StartNode;
                if (next != null && !connected.Contains(next))
                {
                    connected.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return connected.Count < allNodes.Count;
    }

    private bool CheckMaterialLimits(List<Beam> beams)
    {
        foreach (Beam b in beams)
        {
            if (b.MaterialType == BeamMaterialType.Wood  && b.Length > 5.5f) return true;
            if (b.MaterialType == BeamMaterialType.Steel && b.Length > 8.5f) return true;
            if (b.MaterialType == BeamMaterialType.Cable && b.Length < 22.5f) return true;
        }
        return false;
    }

    private bool CheckTriangulation(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            if (node.IsAnchor) continue;
            if (node.GetConnectedBeams().Count < 1) return true;
        }
        return false;
    }

    public float CalculateStructuralHealth(AnalysisResult res)
    {
        float health = 100f;

        if (!res.PathExists)             health -= 40f;
        if (res.IsFloating)              health -= 30f;
        if (res.IsLackingTriangles)      health -= 20f;
        if (res.HasWeakMaterials)        health -= 15f;
        if (res.CurrentCost > maxBudget) health -= 10f;

        return Mathf.Clamp(health, 0f, 100f);
    }

    public int CalculatePotentialPoints(float health, float currentCost)
    {
        float savingsBonus = Mathf.Max(0f, (maxBudget - currentCost) * 0.1f);
        return Mathf.RoundToInt((health * 10f) + savingsBonus);
    }
}

public class AnalysisResult
{
    public bool  PathExists;
    public bool  IsFloating;
    public bool  HasWeakMaterials;
    public bool  IsLackingTriangles;
    public float CurrentCost;
    public List<string> AdviceMessages = new();

    public float StructuralHealth;
    public int   PotentialPoints;

    public bool IsReadyForTest() => PathExists && !IsFloating && !HasWeakMaterials && !IsLackingTriangles;
}