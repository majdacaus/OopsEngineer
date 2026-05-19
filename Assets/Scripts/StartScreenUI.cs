using UnityEngine;

public class StartScreenUI : MonoBehaviour
{
    public GameObject startScreenPanel;
    [SerializeField] private UIController uiController; 

    private void Awake()
    {
        startScreenPanel.SetActive(true);
        Time.timeScale = 0f;

        if (uiController != null)
            uiController.HideToolbar();
    }

    public void StartGame()
    {
        startScreenPanel.SetActive(false);
        Time.timeScale = 1f;

        if (uiController != null)
            uiController.ShowToolbar();
    }
}