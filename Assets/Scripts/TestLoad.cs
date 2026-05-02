using UnityEngine;
using System.Collections.Generic;

public class TestLoad : MonoBehaviour
{
    public StressSimulator simulator;
    public NodeData loadNode;   
    public float testWeight = 2f;

    void Update()
    {
        // T - DODAVANJE TERETA
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (loadNode == null) {
                Debug.LogError("TestLoad: Nisi dodijelila Load Node u Inspector-u!");
                return;
            }

            loadNode.externalLoad += testWeight;
            Debug.Log($"<color=yellow>Load na {loadNode.name}: {loadNode.externalLoad}</color>");
            
            UpdateAllSystems();
        }

        // R - RESET
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (loadNode == null) return;

            loadNode.externalLoad = 0f;
            Debug.Log("<color=green>Teret resetovan na 0!</color>");
            
            UpdateAllSystems();
        }
    }

    void UpdateAllSystems()
    {
        if (simulator == null) {
            Debug.LogError("TestLoad: Simulator slot je prazan!");
            return;
        }

        // Pronalazimo sve aktivne podatke na sceni
        var nodes = new List<NodeData>(FindObjectsOfType<NodeData>());
        var beams = new List<BeamData>(FindObjectsOfType<BeamData>());

        if (nodes.Count == 0) Debug.LogWarning("TestLoad: Nije pronađen nijedan NodeData!");

        simulator.RunSimulation(nodes, beams);
    }
}