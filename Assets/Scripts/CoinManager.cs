using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private int startingCoins = 200;

    private int currentCoins;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentCoins = startingCoins;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateUI();
    }

    public void SpendCoins(int amount)
    {
        currentCoins -= amount;
        UpdateUI();
    }

    public bool HasEnough(int amount)
    {
        return currentCoins >= amount;
    }

    private void UpdateUI()
    {
        coinText.text = $"Gold: {currentCoins}";
    }
}