using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    public BridgeManager bridgeManager; 
    public Button hintButton;           
    public int maxHints = 10;          
    private int hintsUsed = 0;

    void Start()
    {
        if (hintButton != null)
            hintButton.onClick.AddListener(ToggleHints);
        
        InitializeNodeVisibility();
    }

    void InitializeNodeVisibility()
    {
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);

        foreach (Node node in allNodes)
        {
            if (!node.IsAnchor && !node.IsStartNode)
            {
                node.Hide();
            }
            else
            {
                node.Reveal();
            }
        }
    }

    public void ToggleHints()
    {
        if (hintsUsed >= maxHints)
        {
            Debug.Log("Nema više hintova!");
            return;
        }

        bridgeManager.ShowHintTemporarily(30.0f); 
        hintsUsed++;
        
        Debug.Log($"Iskorišten hint {hintsUsed}/{maxHints}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHints();
        }
    }
}