using System;
using UnityEngine;

public class BridgeManager : MonoBehaviour
{
    public GameObject beamPrefab;

    GameObject previewBeam;
    bool isDragging = false;
    Transform startNode;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Node"))
            {
                startNode = hit.transform;
                isDragging = true;

                previewBeam = Instantiate(beamPrefab);

                Debug.Log("START DRAG: " + hit.collider.name);
            }
        }

        if (isDragging)
        {
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 currentPoint = hit.point;

                UpdateBeam(previewBeam, startNode.position, currentPoint);
            }
        }

        // 🔴 MOUSE UP (finalizacija)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Node"))
            {
                if (hit.transform != startNode) // da ne spaja sam sa sobom
                {
                    MakeBeam(startNode.position, hit.transform.position);
                    Debug.Log("END DRAG: " + hit.collider.name);
                }
            }

            Destroy(previewBeam);
            isDragging = false;
        }
    }

    void MakeBeam(Vector3 a, Vector3 b)
    {
        GameObject beam = Instantiate(beamPrefab);
        UpdateBeam(beam, a, b);
    }

    void UpdateBeam(GameObject beam, Vector3 a, Vector3 b)
    {
        Vector3 direction = b - a;
        float distance = direction.magnitude;

        beam.transform.position = a + direction / 2f;
        beam.transform.rotation = Quaternion.LookRotation(direction);
        beam.transform.localScale = new Vector3(0.2f, 0.2f, distance);
    }
    
    
}