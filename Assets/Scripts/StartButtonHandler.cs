using UnityEngine;
using UnityEngine.UI;

public class StartButtonHandler : MonoBehaviour
{
    public StartScreenUI startScreenUI;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            startScreenUI.StartGame();
        });
    }
}