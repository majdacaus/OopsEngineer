using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    public BridgeManager bridgeManager; // Prevuci BridgeManager ovdje
    public Button hintButton;           // Tvoj UI Button
    public int maxHints = 10;            // Koliko puta igrač smije tražiti pomoć
    private int hintsUsed = 0;

    void Start()
    {
        if (hintButton != null)
            hintButton.onClick.AddListener(ToggleHints);
    }

    public void ToggleHints()
    {
        if (hintsUsed >= maxHints)
        {
            Debug.Log("Nema više hintova!");
            return;
        }

        // Pozivamo metodu u BridgeManageru koju smo ranije definisali
        bridgeManager.ShowHintTemporarily(30.0f); // Hint traje 3 sekunde
        hintsUsed++;
        
        Debug.Log($"Iskorišten hint {hintsUsed}/{maxHints}");
    }
}