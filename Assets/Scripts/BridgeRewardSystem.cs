using UnityEngine;
using System;

public class BridgeRewardSystem : MonoBehaviour
{
    public static BridgeRewardSystem Instance { get; private set; }

    public static event Action<int, bool> OnRewardGranted; 

    [Header("Reward Settings")]
    [SerializeField] private ConstructionAnalyzer analyzer;
    [SerializeField] private int baseReward = 100;
    [SerializeField] private float healthRewardScale = 0.5f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EvaluateAndReward(AnalysisResult result)
    {
        if (result == null) return;

        bool passed = result.PathExists && !result.IsFloating;
        int earned  = passed ? CalculateReward(result) : 0;

        if (earned > 0)
            CoinManager.Instance.AddCoins(earned);

        OnRewardGranted?.Invoke(earned, passed);

        Debug.Log($"[BridgeReward] Health={result.StructuralHealth:F0}% | Earned={earned} coins | Passed={passed}");
    }

    private int CalculateReward(AnalysisResult result)
    {
        float healthBonus   = result.StructuralHealth * healthRewardScale;
        float savingsBonus  = Mathf.Max(0f, (analyzer.maxBudget - result.CurrentCost) * 0.001f);
        return Mathf.RoundToInt(baseReward + healthBonus + savingsBonus);
    }
}