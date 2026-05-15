using UnityEngine;
using System.Collections.Generic;

public class RoadBuilder : MonoBehaviour
{
    [Header("Road Vizual")]
    [SerializeField] Material roadMaterial;
    [SerializeField] float roadWidth = 2.8f;  
    [SerializeField] float roadThickness = 0.08f;

    [Header("Detalji ceste")]
    [SerializeField] bool addRoadLines = true;
    [SerializeField] Material roadLineMaterial; 

    private List<GameObject> roadSegments = new List<GameObject>();

    public void AddRoadSegment(Vector3 nodeA, Vector3 nodeB, Vector3 widthDirection)
    {
        GameObject segment = CreateRoadMesh(nodeA, nodeB, widthDirection);
        segment.transform.SetParent(this.transform);
        segment.name = $"Road_{roadSegments.Count}";
        roadSegments.Add(segment);

        if (addRoadLines)
            CreateCentreLine(nodeA, nodeB, widthDirection);
    }

    GameObject CreateRoadMesh(Vector3 a, Vector3 b, Vector3 widthDir)
    {
        Vector3 w = widthDir.normalized * roadWidth;

        Vector3 v0 = a;             
        Vector3 v1 = a + w;         
        Vector3 v2 = b + w;        
        Vector3 v3 = b;           

        Vector3 up = Vector3.up * 0.12f;
        v0 += up; v1 += up; v2 += up; v3 += up;

        Mesh mesh = new Mesh();
        mesh.name = "RoadMesh";

        mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
        mesh.triangles = new int[] { 0, 1, 2,   0, 2, 3 }; 

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject obj = new GameObject("RoadSegment");
        obj.AddComponent<MeshFilter>().mesh = mesh;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material = roadMaterial != null ? roadMaterial : CreateDefaultRoadMaterial();
        MeshCollider mc = obj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = true;
        obj.tag = "Road";
        return obj;
    }

    void CreateCentreLine(Vector3 a, Vector3 b, Vector3 widthDir)
    {
        Vector3 halfW = widthDir.normalized * (roadWidth * 0.5f);
        Vector3 up = Vector3.up * 0.13f; 

        Vector3 lineStart = a + halfW + up;
        Vector3 lineEnd   = b + halfW + up;

        GameObject lineObj = new GameObject("CentreLine");
        lineObj.transform.SetParent(this.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, lineStart);
        lr.SetPosition(1, lineEnd);
        lr.startWidth = 0.25f;
        lr.endWidth   = 0.25f;
        lr.material   = roadLineMaterial != null ? roadLineMaterial : CreateDefaultLineMaterial();

        lr.textureMode = LineTextureMode.Tile;
    }

    Material CreateDefaultRoadMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.25f, 0.25f, 0.25f); 
        return mat;
    }

    Material CreateDefaultLineMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = Color.white;
        return mat;
    }

    public void RemoveLastSegment()
    {
        if (roadSegments.Count == 0) return;

        GameObject last = roadSegments[roadSegments.Count - 1];
        roadSegments.RemoveAt(roadSegments.Count - 1);
        Destroy(last);
    }

    public void ClearAllRoad()
    {
        foreach (var seg in roadSegments)
            if (seg) Destroy(seg);
        roadSegments.Clear();
    }
}