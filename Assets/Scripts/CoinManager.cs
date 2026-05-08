using System;
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public static event Action<int> OnCoinsChanged;

    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private int startingCoins = 200;

    private int currentCoins;

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    public bool SpendCoins(int amount)
    {
        if (currentCoins < amount)
            return false;

        currentCoins -= amount;
        UpdateUI();
        return true;
    }

    public bool HasEnough(int amount)
    {
        return currentCoins >= amount;
    }

    private void UpdateUI()
    {
        coinText.text = $"<sprite name=\"coin\"> {currentCoins}";
        OnCoinsChanged?.Invoke(currentCoins);
    }
}