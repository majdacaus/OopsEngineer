using UnityEngine;

public class StartScreenUI : MonoBehaviour
{
    public GameObject startScreenPanel;

    private void Awake()
    {
        startScreenPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        startScreenPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}