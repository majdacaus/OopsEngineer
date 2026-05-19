using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructionAnalyzer : MonoBehaviour
{
    [Header("Budget Settings")]
    public float maxBudget = 300000f;

    private const float TOLERANCE = 0.1f;
    
    // private bool CheckTopHeavyDistribution(List<Beam> beams, List<Node> nodes)
    // {
    //     if (nodes.Count == 0) return false;
    //
    //     float avgY = 0f;
    //     foreach (Node n in nodes) avgY += n.transform.position.y;
    //     avgY /= nodes.Count;
    //
    //     float heavyAbove = 0f;
    //     float heavyBelow = 0f;
    //
    //     foreach (Beam b in beams)
    //     {
    //         float beamY    = (b.StartNode.transform.position.y + b.EndNode.transform.position.y) / 2f;
    //         float weight   = b.MaterialType == BeamMaterialType.Steel ? 3f :
    //             b.MaterialType == BeamMaterialType.Cable ? 1f : 1.5f;
    //
    //         if (beamY > avgY) heavyAbove += weight;
    //         else              heavyBelow += weight;
    //     }
    //
    //     if (heavyAbove + heavyBelow == 0f) return false;
    //     float ratio = heavyAbove / (heavyAbove + heavyBelow);
    //     return ratio > 0.65f;
    //     // -----------------------
    // }
    
    public AnalysisResult PerformFullAnalysis(List<Node> nodes, List<Beam> beams)
    {
        
     
            
        // nodes = nodes.FindAll(n => n.gameObject.activeInHierarchy && n.IsRevealed);
        // beams = beams.FindAll(b => b.gameObject.activeInHierarchy);

        if (nodes.Count == 0) return new AnalysisResult();
        
        float frontX = nodes[0].transform.position.x;

        nodes = nodes.FindAll(n => n.gameObject.activeInHierarchy &&
                                   n.IsRevealed && Mathf.Abs(n.transform.position.x - frontX) < TOLERANCE);
        
        beams = beams.FindAll(b => b.gameObject.activeInHierarchy &&
                                    Mathf.Abs(b.transform.position.x - frontX) < TOLERANCE);
        // var activeNodes = nodes.Where(n => n != null && n.gameObject.activeInHierarchy).ToList();
        // var activeBeams = beams.Where(b => b != null && b.gameObject.activeInHierarchy).ToList();
        
        
        // nodes = nodes.FindAll(n => n != null && n.gameObject.activeInHierarchy && n.IsRevealed);
        // beams = beams.FindAll(b => b != null && b.gameObject.activeInHierarchy);
        
        AnalysisResult res = new AnalysisResult();
        if (nodes.Count < 2) return res;
        // res.IsTopHeavy = CheckTopHeavyDistribution(beams, nodes);
        // if (res.IsTopHeavy)
        //     res.AdviceMessages.Add("The steel on top is destabilizing us! It's wobbling like a jelly!");
        
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
        Debug.Log($"<color=cyan>[ANALYSIS REPORT]</color> " +
                  $"Path: {res.PathExists} | " +
                  $"Floating: {res.IsFloating} | " +
                  $"Triangles: {!res.IsLackingTriangles} | " +
                  $"Health: {res.StructuralHealth}%");
        return res;
    }

    private float CalculateCost(List<Beam> beams)
    {
        float total = 0f;
        foreach (Beam b in beams)
        {
            float budgetWeight = (b.Length < 2.5f) ? 1.0f : 2.0f;

            float multiplier = b.MaterialType switch
            {
                BeamMaterialType.Steel => 50f,
                BeamMaterialType.Cable => 30f,
                _                      => 10f
            };
            total += budgetWeight * multiplier;
        }
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

    // private bool CheckIfFloating(List<Node> allNodes)
    // {
    //     HashSet<Node> connected = new();
    //     Queue<Node> queue = new();
    //
    //     foreach (Node node in allNodes)
    //     {
    //         if (!node.IsAnchor) continue;
    //         queue.Enqueue(node);
    //         connected.Add(node);
    //     }
    //
    //     while (queue.Count > 0)
    //     {
    //         // Node current = queue.Dequeue();
    //         // foreach (Beam beam in current.GetConnectedBeams())
    //         // {
    //         //     Node next = beam.StartNode == current ? beam.EndNode : beam.StartNode;
    //         //     if (next != null && !connected.Contains(next))
    //         //     {
    //         //         connected.Add(next);
    //         //         queue.Enqueue(next);
    //         //     }
    //         // }
    //
    //         Node current = queue.Dequeue();
    //         foreach (Beam beam in current.GetConnectedBeams())
    //         {
    //             if (beam.StartNode == null || beam.EndNode == null) continue;
    //
    //             Node next = beam.StartNode == current ? beam.EndNode : beam.StartNode;
    //             if (next != null && !connected.Contains(next))
    //             {
    //                 connected.Add(next);
    //                 queue.Enqueue(next);
    //             }
    //         }
    //
    //         if (connected.Count < allNodes.Count)
    //         {
    //             foreach (Node n in allNodes)
    //             {
    //                 if (!connected.Contains(n))
    //                     Debug.LogError(
    //                         $"[ANALYSIS] Čvor {n.gameObject.name} na {n.transform.position} NIJE POVEZAN sa temeljima!");
    //             }
    //
    //         }
    //             return true;
    //
    //         // return connected.Count < allNodes.Count;
    //     }
    //         return false;
    // }
    
    private bool CheckIfFloating(List<Node> allNodes)
    {
        HashSet<Node> connected = new();
        Queue<Node> queue = new();

        foreach (Node node in allNodes)
        {
            if (node.IsAnchor)
            {
                queue.Enqueue(node);
                connected.Add(node);
            }
        }

        if (connected.Count == 0) return true;

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Beam beam in current.GetConnectedBeams())
            {
                if (beam == null || beam.StartNode == null || beam.EndNode == null) continue;

                Node next = beam.StartNode == current ? beam.EndNode : beam.StartNode;
                if (next != null && !connected.Contains(next))
                {
                    connected.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        if (connected.Count < allNodes.Count)
        {
            foreach (Node n in allNodes)
            {
                if (!connected.Contains(n))
                    Debug.LogError($"[ANALYSIS] Čvor {n.gameObject.name} NIJE POVEZAN!");
            }
            return true;
        }

        return false; 
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
        if (nodes.Count == 0) return false;
        float frontX = nodes[0].transform.position.x;
        const float xTolerance = 0.15f;
        
        List<Node> nonAnchors = nodes.FindAll(n => !n.IsAnchor
                                                   &&
                                                   Mathf.Abs(n.transform.position.x - frontX) < xTolerance);
        if (nonAnchors.Count == 0) return false;

        bool IsOnFrontPlane(Beam b) =>
            b != null && b.StartNode != null && b.EndNode != null &&
            Mathf.Abs(b.StartNode.transform.position.x - frontX) < xTolerance &&
            Mathf.Abs(b.EndNode.transform.position.x - frontX) < xTolerance;
        
        foreach (Node node in nodes)
        {
            List<Node> neighbors = new();
            
            foreach (Beam b in node.GetConnectedBeams())
            {
                //if (b == null || b.StartNode == null || b.EndNode == null) continue;
                if (!IsOnFrontPlane(b)) continue;
                Node other = b.StartNode == node ? b.EndNode : b.StartNode;
                if (other != null && nodes.Contains(other))
                    neighbors.Add(other);
            }

            bool inTriangle = false;
            for (int i = 0; i < neighbors.Count && !inTriangle; i++)
            {
                for (int j = i + 1; j < neighbors.Count && !inTriangle; j++)
                {
                    bool connected = neighbors[i].GetConnectedBeams()
                        .Exists(b=> IsOnFrontPlane(b) &&
                                     (b.StartNode == neighbors[j] || b.EndNode == neighbors[j]));
                
                    if (connected) inTriangle = true; 
                }
            }
            
            if(!inTriangle) return true;
        } 

        return false; 
    }

    public float CalculateStructuralHealth(AnalysisResult res)
    {
        float health = 100f;

        if (!res.PathExists)             health -= 40f;
        if (res.IsFloating)              health -= 30f;
        if (res.IsLackingTriangles)      health -= 60f;
        if (res.HasWeakMaterials)        health -= 15f;
        if (res.CurrentCost > maxBudget) health -= 10f;
      //  if (res.IsTopHeavy) health -= 20f;

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
    
   // public bool IsTopHeavy;


    public float StructuralHealth;
    public int   PotentialPoints;

    public bool IsReadyForTest() => PathExists && !IsFloating && !HasWeakMaterials && !IsLackingTriangles;
}