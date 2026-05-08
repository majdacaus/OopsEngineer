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
    [SerializeField] float snapRadius = 1.0f;
    [SerializeField] float maxBeamLength = 6.0f;
    [SerializeField] float minBeamLength = 0.5f;
    [SerializeField] private GameObject _currentBeamPrefab;
    
    [Header("3D/2D Hybrid")]
    [SerializeField] float bridgeWidth = 3.0f;
    [SerializeField] bool autoBuildParallel = true;

    [Header("Sistemi")]
    //[SerializeField] StressSimulator stressSimulator;
    [SerializeField] RoadBuilder roadBuilder;

    Camera cam;
    GameObject previewBeam;
    bool isDragging = false;
    Node startNode;
    Plane constructionPlane;

    private List<Node> allNodeData = new List<Node>();
    private List<Beam> allBeamData = new List<Beam>();

    void Awake()
    {
        cam = Camera.main;
        if (_currentBeamPrefab == null) 
            _currentBeamPrefab = beamPrefab;
    } 

    void Update()
    {
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();
    }

    void HandleMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Node"))
        {
            Node nodeComponent = hit.collider.GetComponent<Node>();

            if (nodeComponent != null)
            {
                StartDragging(nodeComponent);
            }
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
        constructionPlane = new Plane(Vector3.right, startNode.Position);
        CreatePreviewBeam();
    }

    void CleanupDrag()
    {
        if (previewBeam != null) Destroy(previewBeam);
        isDragging = false;
        startNode = null;
    }

    Vector3 GetMousePoint(Ray ray, float enter)
    {
        Vector3 point = ray.GetPoint(enter);
        point.x = startNode.Position.x;
        return point;
    }

    void CreatePreviewBeam()
    {
        if (_currentBeamPrefab == null) return;
        previewBeam = Instantiate(_currentBeamPrefab);
        Collider col = previewBeam.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void UpdatePreview(Vector3 worldPoint)
    {
        Node snapTarget = FindClosestNodeData(worldPoint);
        Vector3 finalPoint = snapTarget ? snapTarget.transform.position : worldPoint;
        float dist = Vector3.Distance(startNode.Position, finalPoint);

        if (previewBeam != null)
        {
            SetPreviewColor(dist);
            UpdateBeam(previewBeam, startNode.Position, finalPoint);
        }
    }

    void SetPreviewColor(float dist)
    {
        Renderer r = previewBeam.GetComponent<Renderer>();
        if (r != null) r.material.color = (dist > maxBeamLength) ? Color.red : Color.white;
    }

    void FinishBuild(Vector3 endPos)
    {
        Node snapTarget = FindClosestNodeData(endPos);
        Vector3 finalPos = snapTarget ? snapTarget.transform.position : endPos;
        float dist = Vector3.Distance(startNode.Position, finalPos);

        if (!IsValidBeam(dist)) return;

        Node fromData = startNode.GetComponent<Node>();
        if (fromData == null) fromData = startNode.gameObject.AddComponent<Node>();
        if (!allNodeData.Contains(fromData)) allNodeData.Add(fromData);

        if (snapTarget != null && snapTarget.transform != startNode)
        {
            MakeBridgeSegment(startNode.Position, snapTarget.transform.position, fromData, snapTarget);
        }
        else if (snapTarget == null && nodePrefab != null)
        {
            CreateNodeAndConnect(finalPos, fromData);
        }
    }

    bool IsValidBeam(float dist) => dist <= maxBeamLength && dist > minBeamLength;

    void CreateNodeAndConnect(Vector3 pos, Node fromData)
    {
        GameObject newNodeObj = Instantiate(nodePrefab, pos, Quaternion.identity);
        newNodeObj.tag = "Node";

        Node toData = newNodeObj.GetComponent<Node>();
        if (toData == null) toData = newNodeObj.AddComponent<Node>();
        
        if (!allNodeData.Contains(toData)) allNodeData.Add(toData);

        MakeBridgeSegment(startNode.Position, newNodeObj.transform.position, fromData, toData);
    }

    void MakeBridgeSegment(Vector3 a, Vector3 b, Node fromNode, Node toNode)
    {
        bool isHorizontal = Mathf.Abs(a.y - b.y) < 0.2f;
        
        float maxRoadHeight = -5.0f;
        
        if (isHorizontal && _currentBeamPrefab.name.ToLower().Contains("cable"))
        {
            Debug.LogWarning("Ne možeš graditi cestu od kablova!");
            return; 
        }
        
        GameObject frontBeam = CreateBeamInstance(a, b, "Beam_Front");
        RegisterBeam(frontBeam, fromNode, toNode);

        if (autoBuildParallel)
        {
            Vector3 offset = new Vector3(bridgeWidth, 0, 0);
            Node backFromNode = GetOrUpdateBackNode(fromNode, offset);
            Node backToNode = GetOrUpdateBackNode(toNode, offset);

            GameObject backBeam = CreateBeamInstance(a + offset, b + offset, "Beam_Back");
            RegisterBeam(backBeam, backFromNode, backToNode);

            CreateBeamInstance(a, a + offset, "Beam_Cross_Start");
            CreateBeamInstance(b, b + offset, "Beam_Cross_End");

            if (isHorizontal && a.y < maxRoadHeight)
            {
                roadBuilder?.AddRoadSegment(a, b, offset);
                Debug.Log("Gradi se cesta na donjem nivou.");
            }
            else if (isHorizontal && a.y >= maxRoadHeight)
            {
                Debug.Log("Gornja greda detektovana - preskačem gradnju ceste.");
            }
        }
    }

    Node GetOrUpdateBackNode(Node frontNode, Vector3 offset)
    {
        Vector3 backPos = frontNode.transform.position + offset;
        Node backNode = FindClosestNodeData(backPos);

        if (backNode == null || Vector3.Distance(backNode.transform.position, backPos) > 0.1f)
        {
            GameObject obj = Instantiate(nodePrefab, backPos, Quaternion.identity);
            obj.tag = "Node";
            backNode = obj.GetComponent<Node>() ?? obj.AddComponent<Node>();
            backNode.CopySettingsFrom(frontNode);
            if (!allNodeData.Contains(backNode)) allNodeData.Add(backNode);
        }
        return backNode;
    }

    void RegisterBeam(GameObject beamObj, Node from, Node to)
    {
        Beam bd = beamObj.GetComponent<Beam>();
        if (bd == null) bd = beamObj.AddComponent<Beam>();

        bd.Initialize(from, to); 

        if (!allBeamData.Contains(bd)) allBeamData.Add(bd);
    }

    GameObject CreateBeamInstance(Vector3 start, Vector3 end, string name)
    {
        GameObject beam = Instantiate(_currentBeamPrefab);
        beam.name = name;
        beam.tag = "Beam";
        UpdateBeam(beam, start, end);
        return beam;
    }

    void UpdateBeam(GameObject beam, Vector3 a, Vector3 b)
    {
        if (beam == null) return;
        Vector3 dir = b - a;
        if (dir.magnitude <= 0.01f) return;

        beam.transform.position = a + dir / 2f;
        beam.transform.right = dir; 
        beam.transform.localScale = new Vector3(dir.magnitude, 0.2f, 0.2f);
    }

    Node FindClosestNodeData(Vector3 pos)
    {
        Node[] allNodes = FindObjectsOfType<Node>();
        Node closest = null;
        float min = snapRadius;

        foreach (Node n in allNodes)
        {
            if (isDragging && n.transform == startNode) continue;
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
            if (!node.IsAnchor) 
            {
               node.Reveal();
               StartCoroutine(HideNodeAfterDelay(node, duration));
                
            }
        }
    }

    private System.Collections.IEnumerator HideNodeAfterDelay(Node node, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (node != null && !node.IsAnchor)
        {
            node.Hide(); 
        }
    }
    
    public void SetBeamPrefab(GameObject newPrefab)
    {
        _currentBeamPrefab = newPrefab;
        Debug.Log("Materijal promijenjen na: " + newPrefab.name);
    }
}