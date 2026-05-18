using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class BridgeManager : MonoBehaviour
{
    [Header("Osnovne Postavke")]
    [SerializeField] GameObject beamPrefab;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] float snapRadius    = 1.2f;
    [SerializeField] float maxBeamLength = 3.5f;
    [SerializeField] float minBeamLength = 0.5f;
    [SerializeField] private GameObject _currentBeamPrefab;

    [Header("3D/2D Hybrid")]
    [SerializeField] float bridgeWidth      = 3.0f;
    [SerializeField] bool  autoBuildParallel = true;

    [Header("Sistemi")]
    [SerializeField] RoadBuilder roadBuilder;
    [SerializeField] StressSimulator stressSimulator;
    public ConstructionAnalyzer analyzer;
    
    [Header("Fiksne Dužine (Snapping)")]
    [SerializeField] float shortLength = 1.75f;
    [SerializeField] float longLength = 3.5f;

    Camera     cam;
    GameObject previewBeam;
    GameObject previewNode;
    bool       isDragging = false;
    Node       startNode;
    Plane      constructionPlane;
    
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private List<Node> allNodeData = new();
    private List<Beam> allBeamData = new();
    private BeamMaterialType selectedMaterial = BeamMaterialType.Wood;

    void Awake()
    {
        cam = Camera.main;
        if (_currentBeamPrefab == null)
            _currentBeamPrefab = beamPrefab;

        Node[] existingNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        
        foreach (Node n in existingNodes)
        {
            // if (!allNodeData.Contains(n)) allNodeData.Add(n);
            // stressSimulator?.RegisterNode(n);
            
            if (n.IsAnchor || n.IsRevealed)
            {
                AddNode(n);
            }
            
            //ako dodam predefinisane gredee
            // Beam[] existingBeams = Object.FindObjectsByType<Beam>(FindObjectsSortMode.None);
            // foreach (Beam b in existingBeams)
            // {
            //     if (!allBeamData.Contains(b)) allBeamData.Add(b);
            //     stressSimulator?.RegisterBeam(b);
            // }
            
            Debug.Log($"[BridgeManager] Inicijalizacija završena. Pronađeno {allNodeData.Count} čvorova");
        }
    }

    void Update()
    {
        if (stressSimulator != null && stressSimulator.SimulationStarted) 
            return;
        
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();

        if (Input.GetKeyDown(KeyCode.T))
            TestMyBridge();
        
        if (Input.GetKeyDown(KeyCode.R))
            HardReset();
        
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= doubleClickThreshold)
            {
                TryDeleteElement();
            }
            lastClickTime = Time.time;
        }
    }
  
    void TryDeleteElement()
{
    
    
    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out RaycastHit hit)) return;

    Beam clickedBeam = hit.collider.GetComponent<Beam>();
    
   // Debug.Log($"Clicked beam: {clickedBeam.name}, linkedBeams count: {clickedBeam.linkedBeams?.Count}");
    if (clickedBeam != null)
    {
        HashSet<Beam> toDelete = new HashSet<Beam> { clickedBeam };

        if (clickedBeam.linkedBeams != null)
        {
            foreach (Beam linked in clickedBeam.linkedBeams)
            {
                if (linked != null) toDelete.Add(linked);
            }
        }

        foreach (Beam b in toDelete)
        {
            if (b == null) continue;

            if (b.linkedRoad != null)
            {
                Destroy(b.linkedRoad);
                b.linkedRoad = null;
            }

            allBeamData.Remove(b);
            Destroy(b.gameObject);
        }

        allBeamData.RemoveAll(item => item == null);
        Invoke(nameof(CleanupIsolatedNodes), 0.05f);
        return;
    }

    Node node = hit.collider.GetComponent<Node>();
    if (node != null && !node.IsAnchor)
    {
        HashSet<Beam> toDelete = new HashSet<Beam>();
        foreach (Beam b in allBeamData)
        {
            if (b != null && (b.StartNode == node || b.EndNode == node))
                toDelete.Add(b);
        }

        foreach (Beam b in toDelete)
        {
            if (b == null) continue;
            if (b.linkedRoad != null) { Destroy(b.linkedRoad); b.linkedRoad = null; }
            if (b.linkedBeams != null)
            {
                foreach (Beam linked in b.linkedBeams)
                {
                    if (linked != null && !toDelete.Contains(linked))
                    {
                        if (linked.linkedRoad != null) { Destroy(linked.linkedRoad); linked.linkedRoad = null; }
                        allBeamData.Remove(linked);
                        Destroy(linked.gameObject);
                    }
                }
            }
            allBeamData.Remove(b);
            Destroy(b.gameObject);
        }

        allBeamData.RemoveAll(item => item == null);
        allNodeData.Remove(node);
        Destroy(node.gameObject);
        Invoke(nameof(CleanupIsolatedNodes), 0.05f);
    }
}
    void CleanupIsolatedNodes()
    {
        allBeamData.RemoveAll(b => b == null);
        allNodeData.RemoveAll(n => n == null);
    
        List<Node> nodesToRemove = new List<Node>();

        foreach (Node node in allNodeData)
        {
            if (node == null || node.IsAnchor) continue;

            bool hasConnection = false;
            foreach (Beam beam in allBeamData)
            {
                if (beam != null && (beam.StartNode == node || beam.EndNode == node))
                {
                    hasConnection = true;
                    break;
                }
            }
            if (!hasConnection) nodesToRemove.Add(node);
        }

        foreach (Node node in nodesToRemove)
        {
            allNodeData.Remove(node);
            if (node != null) Destroy(node.gameObject);
        }
    }
    public void HardReset()
    {
        StopAllCoroutines();
    
        foreach (var beam in allBeamData)
        {
            if (beam != null) Destroy(beam.gameObject);
        }
    
        foreach (var node in allNodeData)
        {
            if (node != null && !node.IsAnchor) 
                Destroy(node.gameObject);
        }

        allBeamData.Clear();
        allNodeData.RemoveAll(n => n == null || !n.IsAnchor);
        roadBuilder?.ClearAllRoad();
    }
    Vector3 GetSnappedMousePoint(Vector3 rawMousePoint)
    {
        Vector3 direction = (rawMousePoint - startNode.transform.position).normalized;
        float currentDist = Vector3.Distance(startNode.transform.position, rawMousePoint);

        float threshold = (shortLength + longLength) / 2f; 
    
        float finalDist;
        if (currentDist < threshold)
        {
            finalDist = shortLength;
        }
        else
        {
            finalDist = longLength;
        }

        return startNode.transform.position + direction * finalDist;
    }
    public void SetMaterialWood()  => selectedMaterial = BeamMaterialType.Wood;
    public void SetMaterialSteel() => selectedMaterial = BeamMaterialType.Steel;
    public void SetMaterialCable() => selectedMaterial = BeamMaterialType.Cable;

    void HandleMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Node"))
        {
            Node nodeComponent = hit.collider.GetComponent<Node>();
            if (nodeComponent != null && nodeComponent.IsRevealed)
                StartDragging(nodeComponent);
        }
    }

    void HandleDragging()
    {
        if (!isDragging || startNode == null) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (constructionPlane.Raycast(ray, out float enter))
            UpdatePreview(GetMousePoint(ray, enter));
    }

    void HandleMouseUp()
    {
        if (!Input.GetMouseButtonUp(0) || !isDragging) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (constructionPlane.Raycast(ray, out float enter))
            FinishBuild(GetMousePoint(ray, enter));
        CleanupDrag();
    }

    void StartDragging(Node node)
    {
        startNode = node;
        isDragging = true;
        constructionPlane = new Plane(-cam.transform.forward, startNode.Position);
        
        CreatePreviewBeam();
    }

    void CleanupDrag()
    {
        if (previewBeam != null) Destroy(previewBeam);
      //  if (previewNode != null) Destroy(previewNode);
        isDragging = false;
        startNode = null;
    }

    Vector3 GetMousePoint(Ray ray, float enter)
    {
        Vector3 point = ray.GetPoint(enter);
       // point.y = startNode.Position.y;
        point.x = startNode.Position.x;
        
        float heightdiff = Mathf.Abs(point.y - startNode.Position.y);
        
        if(heightdiff< 0.5f)
            point.y = startNode.Position.y;
        return point;
    }

    void CreatePreviewBeam()
    {
        if (_currentBeamPrefab == null) return;
        previewBeam = Instantiate(_currentBeamPrefab);
       // previewNode = Instantiate(nodePrefab);
        
       // if (previewBeam.TryGetComponent(out Collider colBeam)) colBeam.enabled = false;
       // if (previewNode.TryGetComponent(out Collider colNode)) colNode.enabled = false;
        
        Collider col = previewBeam.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // void UpdatePreview(Vector3 worldPoint)
    // {
    //     Vector3 snappedPoint = GetSnappedMousePoint(worldPoint);
    //     
    //     Node    snapTarget = FindClosestNode(worldPoint);
    //     Vector3 finalPoint = snapTarget ? snapTarget.transform.position : worldPoint;
    //     float   dist       = Vector3.Distance(startNode.Position, finalPoint);
    //
    //     if (previewBeam == null) return;
    //     SetPreviewColor(dist);
    //     UpdateBeamTransform(previewBeam, startNode.Position, finalPoint);
    // }
    void UpdatePreview(Vector3 worldPoint)
    {
        if (previewBeam == null || startNode == null) return;

        Node snapTarget = FindClosestNode(worldPoint);
        Vector3 finalPoint;

        if (snapTarget != null)
        {
            finalPoint = snapTarget.transform.position;
        }
        else
        {
            finalPoint = GetSnappedMousePoint(worldPoint);
        }

        float dist = Vector3.Distance(startNode.Position, finalPoint);
    
        SetPreviewColor(dist);
        UpdateBeamTransform(previewBeam, startNode.Position, finalPoint);
        
        // if (previewNode != null)
        // {
        //     previewNode.transform.position = finalPoint;
        //     previewNode.SetActive(dist <= maxBeamLength); 
        // }
    }

    void FinishBuild(Vector3 endPos)
    {
        if (!isDragging || startNode == null) return;

        Node snapTarget = FindClosestNode(endPos);
        Vector3 finalPos;

        if (snapTarget != null)
        {
            finalPos = snapTarget.transform.position;
        }
        else
        {
            finalPos = GetSnappedMousePoint(endPos);
        }

        float dist = Vector3.Distance(startNode.Position, finalPos);

        if (!IsValidBeam(dist)) return;

        if (!allNodeData.Contains(startNode)) allNodeData.Add(startNode);

        if (snapTarget != null && snapTarget != startNode)
        {
            MakeBridgeSegment(startNode.Position, snapTarget.transform.position, startNode, snapTarget);
        }
        else if (snapTarget == null && nodePrefab != null)
        {
            CreateNodeAndConnect(finalPos, startNode);
        }
    }
    void SetPreviewColor(float dist)
    {
        Renderer r = previewBeam.GetComponent<Renderer>();
        if (r != null) r.material.color = dist > maxBeamLength ? Color.red : Color.white;
    }

    // void FinishBuild(Vector3 endPos)
    // {
    //     Vector3 snappedPoint = GetSnappedMousePoint(endPos);
    //     Node    snapTarget = FindClosestNode(endPos);
    //     Vector3 finalPos   = snapTarget ? snapTarget.transform.position : endPos;
    //     float   dist       = Vector3.Distance(startNode.Position, finalPos);
    //
    //     if (!IsValidBeam(dist)) return;
    //
    //     if (!allNodeData.Contains(startNode)) allNodeData.Add(startNode);
    //
    //     if (snapTarget != null && snapTarget != startNode)
    //         MakeBridgeSegment(startNode.Position, snapTarget.transform.position, startNode, snapTarget);
    //     else if (snapTarget == null && nodePrefab != null)
    //         CreateNodeAndConnect(finalPos, startNode);
    // }

    bool IsValidBeam(float dist) => dist <= maxBeamLength && dist > minBeamLength;

    // void CreateNodeAndConnect(Vector3 pos, Node fromNode)
    // {
    //     GameObject newNodeObj = Instantiate(nodePrefab, pos, Quaternion.identity);
    //     newNodeObj.tag = "Node";
    //
    //     Node toNode = newNodeObj.GetComponent<Node>() ?? newNodeObj.AddComponent<Node>();
    //     //if (!allNodeData.Contains(toNode)) allNodeData.Add(toNode);
    //     AddNode(toNode);
    //     MakeBridgeSegment(startNode.Position, newNodeObj.transform.position, fromNode, toNode);
    // }
    
    void CreateNodeAndConnect(Vector3 pos, Node fromNode)
    {
        Debug.Log($"[BridgeManager] Pokušavam stvoriti Node na: {pos}");
        
        GameObject newNodeObj = Instantiate(nodePrefab, pos, Quaternion.identity);
        if (newNodeObj == null) 
        {
            Debug.LogError("PREFAB NIJE DODIJELJEN U INSPECTORU!");
            return;
        }
        
        newNodeObj.tag = "Node";
    
        newNodeObj.transform.localScale = new Vector3(0.2988206f, 0.2988206f, 0.2988206f);
        //newNodeObj.transform.localScale = Vector3.one * 0.6f;
        Node toNode = newNodeObj.GetComponent<Node>();
        if (toNode == null) toNode = newNodeObj.AddComponent<Node>();
        if (newNodeObj.TryGetComponent(out MeshRenderer mr)) mr.enabled = true;
        if (newNodeObj.TryGetComponent(out SphereCollider sc))
        {
            sc.enabled = true;
            sc.isTrigger = false;
        }
        toNode.Reveal(); 
    
        AddNode(toNode);
        MakeBridgeSegment(fromNode.Position, newNodeObj.transform.position, fromNode, toNode);
        Debug.Log("Node uspješno kreiran i povezan.");
    }
    void AddNode(Node node)
    {
        if (allNodeData.Contains(node)) return;
        allNodeData.Add(node);
        stressSimulator?.RegisterNode(node);
    }
    void MakeBridgeSegment(Vector3 a, Vector3 b, Node fromNode, Node toNode)
    {
        bool isHorizontal = Mathf.Abs(a.y - b.y) < 0.2f;
        float maxRoadHeight = -5.0f;

        if (isHorizontal && selectedMaterial == BeamMaterialType.Cable) return;

        GameObject frontBeam = CreateBeamInstance(a, b, "Beam_Front");
        RegisterBeam(frontBeam, fromNode, toNode);

        if (!autoBuildParallel) return;

        Vector3 offset = new Vector3(bridgeWidth, 0, 0);
        Node backFromNode = GetOrUpdateBackNode(fromNode, offset);
        Node backToNode = GetOrUpdateBackNode(toNode, offset);

        GameObject backBeam = CreateBeamInstance(a + offset, b + offset, "Beam_Back");
        GameObject crossStart = CreateBeamInstance(a, a + offset, "Beam_Cross_Start");
        GameObject crossEnd = CreateBeamInstance(b, b + offset, "Beam_Cross_End");

        RegisterBeam(backBeam, backFromNode, backToNode);
        RegisterBeam(crossStart, fromNode, backFromNode);
        RegisterBeam(crossEnd, toNode, backToNode);

        
        Beam b1 = frontBeam.GetComponent<Beam>();
        Beam b2 = backBeam.GetComponent<Beam>();
        Beam b3 = crossStart.GetComponent<Beam>();
        Beam b4 = crossEnd.GetComponent<Beam>();
        
        if (isHorizontal && a.y < maxRoadHeight)
        {
            if (roadBuilder != null)
            {
                GameObject roadPiece = roadBuilder.AddRoadSegment(a, b, offset);
                if (b1 != null) b1.linkedRoad = roadPiece;
                if (b2 != null) b2.linkedRoad = roadPiece;
                if (b3 != null) b3.linkedRoad = roadPiece;
                if (b4 != null) b4.linkedRoad = roadPiece;
            }
        }
        List<Beam> group = new List<Beam> { b1, b2, b3, b4 };
        foreach (Beam current in group)
        {
            if (current == null) continue;
            foreach (Beam other in group)
            {
                if (other != null && current != other)
                {
                    if (!current.linkedBeams.Contains(other)) 
                        current.linkedBeams.Add(other);
                }
            }
        }
    }

    Node GetOrUpdateBackNode(Node frontNode, Vector3 offset)
    {
        Vector3 backPos  = frontNode.transform.position + offset;
        Node    backNode = FindClosestNode(backPos);

        if (backNode == null || Vector3.Distance(backNode.transform.position, backPos) > 0.1f)
        {
            GameObject obj = Instantiate(nodePrefab, backPos, Quaternion.identity);
            obj.tag  = "Node";
            
            obj.transform.localScale = new Vector3(0.2988206f, 0.2988206f, 0.2988206f);
            
            backNode = obj.GetComponent<Node>() ?? obj.AddComponent<Node>();
            
            if (obj.TryGetComponent(out MeshRenderer mr)) mr.enabled = true;
            if (obj.TryGetComponent(out SphereCollider sc)) { sc.enabled = true; sc.isTrigger = false; }

            backNode.CopySettingsFrom(frontNode);
            backNode.Reveal();
            AddNode(backNode);
            
        }
        return backNode;
    }
    

    void RegisterBeam(GameObject beamObj, Node from, Node to)
    {
        if (beamObj == null) return;

        Beam bd = beamObj.GetComponent<Beam>() ?? beamObj.AddComponent<Beam>();
    
        bd.Initialize(from, to, selectedMaterial, -1f); 
        
        // -----------------------
        if (MaterialInventory.Instance != null)
        {
            MaterialData equipped = MaterialInventory.Instance.EquippedMaterial;
            if (equipped != null)
                MaterialInventory.Instance.ConsumeUnit(equipped);
        }
        // -----------------------

        if (!allBeamData.Contains(bd)) allBeamData.Add(bd);
    
        if (stressSimulator != null)
        {
            stressSimulator.RegisterNode(from);
            stressSimulator.RegisterNode(to);
            stressSimulator.RegisterBeam(bd);
        }

        Debug.Log($"<color=green>[BridgeManager]</color> Greda uspješno registrovana: {from.name} -> {to.name}");
    }
    GameObject CreateBeamInstance(Vector3 start, Vector3 end, string beamName)
    {
        GameObject beam = Instantiate(_currentBeamPrefab);
        beam.name = beamName;
        beam.tag  = "Beam";
        UpdateBeamTransform(beam, start, end);
        return beam;
    }

    void UpdateBeamTransform(GameObject beam, Vector3 a, Vector3 b)
    {
        if (beam == null) return;
        Vector3 dir = b - a;
        if (dir.magnitude <= 0.01f) return;

        beam.transform.position   = a + dir / 2f;
        beam.transform.right      = dir;
        beam.transform.localScale = new Vector3(dir.magnitude, 0.2f, 0.2f);
    }

    Node FindClosestNode(Vector3 pos)
    {
        Node  closest = null;
        float min     = snapRadius;

        foreach (Node n in allNodeData)
        {
            if (isDragging && n == startNode) continue;
            if (!n.IsRevealed) continue;
            float d = Vector3.Distance(pos, n.transform.position);
            if (d < min) { closest = n; min = d; }
        }
        return closest;
    }

    public void ShowHintTemporarily(float duration)
    {
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Node node in allNodes)
        {
            if (node.IsAnchor) continue;
            node.Reveal();
            StartCoroutine(HideNodeAfterDelay(node, duration));
        }
    }

    private System.Collections.IEnumerator HideNodeAfterDelay(Node node, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (node != null && !node.IsAnchor)
            node.Hide();
    }

    public void SetBeamPrefab(GameObject newPrefab)
    {
        _currentBeamPrefab = newPrefab;
    }

    public void TestMyBridge()
    {
        allNodeData = new List<Node>(FindObjectsOfType<Node>())
            .FindAll(n => n.IsRevealed || n.IsAnchor);
        allBeamData = new List<Beam>(FindObjectsOfType<Beam>());

        Debug.Log($"Ukupno nodova: {allNodeData.Count}");
        foreach (Node n in allNodeData)
            Debug.Log($"Node: {n.name} | IsAnchor: {n.IsAnchor} | Pos: {n.Position}");
        
        if (analyzer == null)
        {
            Debug.LogWarning("[BridgeManager] ConstructionAnalyzer nije dodijeljen.");
            return;
        }

        AnalysisResult result = analyzer.PerformFullAnalysis(allNodeData, allBeamData);
        Debug.Log($"--- BRIDGE ANALYSIS --- Health: {result.StructuralHealth}% | Points: {result.PotentialPoints}");

        Debug.Log(result.IsReadyForTest()
            ? "<color=green>Bridge is READY for simulation!</color>"
            : "<color=red>Bridge is NOT ready!</color>");

        foreach (string message in result.AdviceMessages)
            Debug.Log("ADVICE: " + message);
    }
    

    
 
}