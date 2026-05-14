using UnityEngine;

public class HelpMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject helpMenuImage;
    public GameObject helpMenuButton;

    void Start()
    {
        if (helpMenuImage != null)
            helpMenuImage.SetActive(false);
            
        if (helpMenuButton != null)
            helpMenuButton.SetActive(true);
    }

    public void ToggleHelpMenu()
    {
        if (helpMenuImage != null)
        {
            bool isNowActive = !helpMenuImage.activeSelf;
            helpMenuImage.SetActive(isNowActive);
            
            Time.timeScale = isNowActive ? 0f : 1f;
        }
    }
}