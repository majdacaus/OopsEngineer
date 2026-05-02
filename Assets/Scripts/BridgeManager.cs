using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BridgeManager : MonoBehaviour
{
    [Header("Osnovne Postavke")]
    [SerializeField] GameObject beamPrefab;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] float snapRadius = 1.0f;
    [SerializeField] float maxBeamLength = 6.0f;
    [SerializeField] float minBeamLength = 0.5f;

    [Header("3D/2D Hybrid")]
    [SerializeField] float bridgeWidth = 3.0f;
    [SerializeField] bool autoBuildParallel = true;

    [Header("Sistemi")]
    [SerializeField] StressSimulator stressSimulator;
    [SerializeField] RoadBuilder roadBuilder;

    Camera cam;
    GameObject previewBeam;
    bool isDragging = false;
    Transform startNode;
    Plane constructionPlane;

    private List<NodeData> allNodeData = new List<NodeData>();
    private List<BeamData> allBeamData = new List<BeamData>();

    void Awake() => cam = Camera.main;

    void Update()
    {
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();
    }

    void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Node"))
            StartDragging(hit.transform);
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

    void StartDragging(Transform node)
    {
        startNode = node;
        isDragging = true;
        constructionPlane = new Plane(Vector3.right, startNode.position);
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
        point.x = startNode.position.x;
        return point;
    }

    void CreatePreviewBeam()
    {
        if (beamPrefab == null) return;
        previewBeam = Instantiate(beamPrefab);
        Collider col = previewBeam.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void UpdatePreview(Vector3 worldPoint)
    {
        NodeData snapTarget = FindClosestNodeData(worldPoint);
        Vector3 finalPoint = snapTarget ? snapTarget.transform.position : worldPoint;
        float dist = Vector3.Distance(startNode.position, finalPoint);

        if (previewBeam != null)
        {
            SetPreviewColor(dist);
            UpdateBeam(previewBeam, startNode.position, finalPoint);
        }
    }

    void SetPreviewColor(float dist)
    {
        Renderer r = previewBeam.GetComponent<Renderer>();
        if (r != null) r.material.color = (dist > maxBeamLength) ? Color.red : Color.white;
    }

    void FinishBuild(Vector3 endPos)
    {
        NodeData snapTarget = FindClosestNodeData(endPos);
        Vector3 finalPos = snapTarget ? snapTarget.transform.position : endPos;
        float dist = Vector3.Distance(startNode.position, finalPos);

        if (!IsValidBeam(dist)) return;

        NodeData fromData = startNode.GetComponent<NodeData>();
        if (fromData == null) fromData = startNode.gameObject.AddComponent<NodeData>();
        if (!allNodeData.Contains(fromData)) allNodeData.Add(fromData);

        if (snapTarget != null && snapTarget.transform != startNode)
        {
            MakeBridgeSegment(startNode.position, snapTarget.transform.position, fromData, snapTarget);
        }
        else if (snapTarget == null && nodePrefab != null)
        {
            CreateNodeAndConnect(finalPos, fromData);
        }
    }

    bool IsValidBeam(float dist) => dist <= maxBeamLength && dist > minBeamLength;

    void CreateNodeAndConnect(Vector3 pos, NodeData fromData)
    {
        GameObject newNodeObj = Instantiate(nodePrefab, pos, Quaternion.identity);
        newNodeObj.tag = "Node";

        NodeData toData = newNodeObj.GetComponent<NodeData>();
        if (toData == null) toData = newNodeObj.AddComponent<NodeData>();
        
        if (!allNodeData.Contains(toData)) allNodeData.Add(toData);

        MakeBridgeSegment(startNode.position, newNodeObj.transform.position, fromData, toData);
    }

    void MakeBridgeSegment(Vector3 a, Vector3 b, NodeData fromNode, NodeData toNode)
    {
        GameObject frontBeam = CreateBeamInstance(a, b, "Beam_Front");
        RegisterBeam(frontBeam, fromNode, toNode);

        if (autoBuildParallel)
        {
            Vector3 offset = new Vector3(bridgeWidth, 0, 0);
            NodeData backFromNode = GetOrUpdateBackNode(fromNode, offset);
            NodeData backToNode = GetOrUpdateBackNode(toNode, offset);

            GameObject backBeam = CreateBeamInstance(a + offset, b + offset, "Beam_Back");
            RegisterBeam(backBeam, backFromNode, backToNode);

            CreateBeamInstance(a, a + offset, "Beam_Cross_Start");
            CreateBeamInstance(b, b + offset, "Beam_Cross_End");

            // --- IZMJENA ZA CESTU (SAMO Y FIKSIRAN) ---
            float roadY = -5.6f; // Tvoja visina glavnih čvorova
            if (Mathf.Abs(a.y - roadY) < 0.1f && Mathf.Abs(b.y - roadY) < 0.1f)
            {
                roadBuilder?.AddRoadSegment(a, b, offset);
            }
        }
    }

    NodeData GetOrUpdateBackNode(NodeData frontNode, Vector3 offset)
    {
        Vector3 backPos = frontNode.transform.position + offset;
        NodeData backNode = FindClosestNodeData(backPos);

        if (backNode == null || Vector3.Distance(backNode.transform.position, backPos) > 0.1f)
        {
            GameObject obj = Instantiate(nodePrefab, backPos, Quaternion.identity);
            obj.tag = "Node";
            backNode = obj.GetComponent<NodeData>() ?? obj.AddComponent<NodeData>();
            backNode.isAnchor = frontNode.isAnchor; 
            if (!allNodeData.Contains(backNode)) allNodeData.Add(backNode);
        }
        return backNode;
    }

    void RegisterBeam(GameObject beamObj, NodeData from, NodeData to)
    {
        BeamData bd = beamObj.GetComponent<BeamData>();
        if (bd == null) bd = beamObj.AddComponent<BeamData>();

        Vector3 dir = to.transform.position - from.transform.position;
        bd.startNode = from;
        bd.endNode = to;
        bd.length = dir.magnitude;
        bd.angle = Vector3.Angle(dir, new Vector3(dir.x, 0, dir.z));

        if (!from.connectedBeams.Contains(bd)) from.connectedBeams.Add(bd);
        if (!to.connectedBeams.Contains(bd)) to.connectedBeams.Add(bd);

        if (!allBeamData.Contains(bd)) allBeamData.Add(bd);
    }

    GameObject CreateBeamInstance(Vector3 start, Vector3 end, string name)
    {
        GameObject beam = Instantiate(beamPrefab);
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

    NodeData FindClosestNodeData(Vector3 pos)
    {
        NodeData[] allNodes = FindObjectsOfType<NodeData>();
        NodeData closest = null;
        float min = snapRadius;

        foreach (NodeData n in allNodes)
        {
            if (isDragging && n.transform == startNode) continue;
            float d = Vector3.Distance(pos, n.transform.position);
            if (d < min) { closest = n; min = d; }
        }
        return closest;
    }
    
    public void ShowHintTemporarily(float duration)
    {
        NodeData[] allNodes = Object.FindObjectsByType<NodeData>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    
        foreach (NodeData node in allNodes)
        {
            if (!node.isAnchor) 
            {
                MeshRenderer mr = node.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = true; 
                    StartCoroutine(HideNodeAfterDelay(node, duration));
                }
            }
        }
    }

    private System.Collections.IEnumerator HideNodeAfterDelay(NodeData node, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (node != null && !node.isAnchor)
        {
            MeshRenderer mr = node.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false; 
        }
    }
}