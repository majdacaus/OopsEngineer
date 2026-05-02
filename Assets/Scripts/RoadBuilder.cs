using UnityEngine;
using System.Collections.Generic;

public class RoadBuilder : MonoBehaviour
{
    [Header("Road Vizual")]
    [SerializeField] Material roadMaterial;
    [SerializeField] float roadWidth = 2.8f;   // malo uži od bridgeWidth
    [SerializeField] float roadThickness = 0.08f;

    [Header("Detalji ceste")]
    [SerializeField] bool addRoadLines = true;
    [SerializeField] Material roadLineMaterial; // bijeli isprekidani

    // Interno čuvamo sve cestovne segmente
    private List<GameObject> roadSegments = new List<GameObject>();

    // ─────────────────────────────────────────────────
    // Poziva BridgeManager nakon svake nove grede
    // a i b su pozicije krajnjih čvorova JEDNOG BOKA mosta
    // ─────────────────────────────────────────────────
    public void AddRoadSegment(Vector3 nodeA, Vector3 nodeB, Vector3 widthDirection)
    {
        // Cesta ide od (nodeA) do (nodeB) u dužinu,
        // i od (nodeA) do (nodeA + widthDirection * roadWidth) u širinu

        GameObject segment = CreateRoadMesh(nodeA, nodeB, widthDirection);
        segment.transform.SetParent(this.transform);
        segment.name = $"Road_{roadSegments.Count}";
        roadSegments.Add(segment);

        if (addRoadLines)
            CreateCentreLine(nodeA, nodeB, widthDirection);
    }

    GameObject CreateRoadMesh(Vector3 a, Vector3 b, Vector3 widthDir)
    {
        // 4 ugla ceste
        Vector3 w = widthDir.normalized * roadWidth;

        Vector3 v0 = a;             // lijevo-start
        Vector3 v1 = a + w;         // desno-start
        Vector3 v2 = b + w;         // desno-end
        Vector3 v3 = b;             // lijevo-end

        // Pomakni gore za tolašinu da ne probija grede
        Vector3 up = Vector3.up * 0.05f;
        v0 += up; v1 += up; v2 += up; v3 += up;

        Mesh mesh = new Mesh();
        mesh.name = "RoadMesh";

        mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
        mesh.triangles = new int[] { 0, 1, 2,   0, 2, 3 }; // dva trokuta = jedan quad

        // UV koordinate za texturu ceste (opciono)
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

        return obj;
    }

    void CreateCentreLine(Vector3 a, Vector3 b, Vector3 widthDir)
    {
        // Linija ide sredinom ceste duž dužine
        Vector3 halfW = widthDir.normalized * (roadWidth * 0.5f);
        Vector3 up = Vector3.up * 0.06f; // malo iznad ceste

        Vector3 lineStart = a + halfW + up;
        Vector3 lineEnd   = b + halfW + up;

        // Koristi LineRenderer za isprekidanu liniju
        GameObject lineObj = new GameObject("CentreLine");
        lineObj.transform.SetParent(this.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, lineStart);
        lr.SetPosition(1, lineEnd);
        lr.startWidth = 0.06f;
        lr.endWidth   = 0.06f;
        lr.material   = roadLineMaterial != null ? roadLineMaterial : CreateDefaultLineMaterial();

        // Isprekidana linija pomoću texture offseta
        lr.textureMode = LineTextureMode.Tile;
    }

    Material CreateDefaultRoadMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.25f, 0.25f, 0.25f); // tamnosivi asfalt
        return mat;
    }

    Material CreateDefaultLineMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = Color.white;
        return mat;
    }

    // Ako se greda uništi, ukloni i cestovni segment
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