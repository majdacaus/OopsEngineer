using UnityEngine;

public class SimpleGoalZone : MonoBehaviour
{
    public float targetX = -118f; 
    private bool _hasFinished = false;

    void Update()
    {
        if (_hasFinished || CarIdentifier.ActiveCar == null) return;

        if (CarIdentifier.ActiveCar.position.x <= targetX)
        {
            _hasFinished = true;
            WinGame();
        }
    }

    void WinGame()
    {
        StressSimulator sim = FindFirstObjectByType<StressSimulator>();
        if (sim != null)
        {
            sim.NotifySimulationPassed(sim.LastAnalysis);
        }

        GameObject panel = GameObject.Find("StartScreenPanel");
        if (panel != null) 
        {
            panel.SetActive(true);
            Time.timeScale = 0f; 
        }
    }
}