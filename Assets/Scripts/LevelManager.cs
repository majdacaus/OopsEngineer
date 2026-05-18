using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // -----------------------
    public static event Action<int> OnLevelLoaded;   // level index
    public static event Action      OnLevelComplete;
    public static event Action      OnLevelFailed;

    [Header("Level Settings")]
    [SerializeField] private int coinsOnLevelStart = 0;
    [SerializeField] private BridgeStructure _bridgeStructure;
    [SerializeField] private float _hintDuration = 3f;

    private int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        StressSimulator.OnSimulationPassed += HandleSimulationPassed;
        StressSimulator.OnSimulationFailed += HandleSimulationFailed;
    }

    private void OnDisable()
    {
        StressSimulator.OnSimulationPassed -= HandleSimulationPassed;
        StressSimulator.OnSimulationFailed -= HandleSimulationFailed;
    }

    private void HandleSimulationPassed(AnalysisResult result)
    {
        OnLevelComplete?.Invoke();
        Debug.Log($"[LevelManager] Level {currentLevelIndex} complete!");
    }

    private void HandleSimulationFailed()
    {
        OnLevelFailed?.Invoke();
        Debug.Log($"[LevelManager] Level {currentLevelIndex} failed.");
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;

        if (coinsOnLevelStart > 0 && CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(coinsOnLevelStart);

        SceneManager.LoadScene(levelIndex);
        OnLevelLoaded?.Invoke(levelIndex);
    }

    public void RestartLevel()  => LoadLevel(currentLevelIndex);
    public void NextLevel()     => LoadLevel(currentLevelIndex + 1);
    // -----------------------

    public void ShowHints()
    {
        if (_bridgeStructure != null)
            StartCoroutine(HintRoutine());
    }

    private IEnumerator HintRoutine()
    {
        foreach (Node node in _bridgeStructure.Nodes)
            if (!node.IsAnchor) node.Reveal();

        yield return new WaitForSeconds(_hintDuration);
    }
}