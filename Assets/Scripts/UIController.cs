using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("UI Elementi")]
    [SerializeField] private GameObject toolbar;

    public void SetToolbarVisible(bool visible)
    {
        if (toolbar != null)
        {
            toolbar.SetActive(visible);
        }
    }

    public void HideToolbar()
    {
        SetToolbarVisible(false);
    }

    public void ShowToolbar()
    {
        SetToolbarVisible(true);
    }
}