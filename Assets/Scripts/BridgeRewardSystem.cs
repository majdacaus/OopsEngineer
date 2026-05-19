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
        Debug.Log($"[DEBUG] Reward System pokrenut!");
    
        if (result == null) {
            Debug.LogError("[DEBUG] Rezultat je NULL!");
            return;
        }

        bool passed = result.PathExists && !result.IsFloating;
        int earned = passed ? CalculateReward(result) : 0;

        Debug.Log($"[DEBUG] Passed: {passed} | Zaradjeno: {earned}");

        if (earned > 0)
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(earned);
                Debug.Log("[DEBUG] AddCoins pozvan uspješno!");
            }
            else
            {
                Debug.LogError("[DEBUG] CoinManager.Instance je NULL! Nije nadjen u sceni!");
            }
        }
        else
        {
            Debug.Log("[DEBUG] Earned je 0 ili manje, pare nisu dodate.");
        }
    }

    private int CalculateReward(AnalysisResult result)
    {
        float healthBonus   = result.StructuralHealth * healthRewardScale;
        float savingsBonus  = Mathf.Max(0f, (analyzer.maxBudget - result.CurrentCost) * 0.001f);
        return Mathf.RoundToInt(baseReward + healthBonus + savingsBonus);
    }
}