using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Toolbar Cards")]
    [SerializeField] private List<MaterialSlotUI> slots;

    [Header("Build Mode Only")]
    [SerializeField] private Button testButton;
    [SerializeField] private GameObject slotsContainer;

    [Header("Test Mode Only")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private GameObject testActionsContainer;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 2.5f;

    private BridgeManager _bridgeManager;
    private Coroutine _feedbackCoroutine;

    private void Awake()
    {
        _bridgeManager = FindFirstObjectByType<BridgeManager>();
    }

    private void OnEnable()
    {
        MaterialInventory.OnQuantityChanged += RefreshSlot;
        StressSimulator.OnSimulationFailed += EnterBuildMode;
        StressSimulator.OnSimulationBlocked += OnSimulationBlocked;
    }

    private void OnDisable()
    {
        MaterialInventory.OnQuantityChanged -= RefreshSlot;
        StressSimulator.OnSimulationFailed -= EnterBuildMode;
        StressSimulator.OnSimulationBlocked -= OnSimulationBlocked;
    }

    private void Start()
    {
        if (testButton != null) testButton.onClick.AddListener(OnTestClicked);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        if (undoButton != null) undoButton.onClick.AddListener(OnUndoClicked);

        RefreshAll();
        EnterBuildMode();
    }

    public void EnterBuildMode()
    {
        if (slotsContainer != null) slotsContainer.SetActive(true);
        if (testButton != null) testButton.gameObject.SetActive(true);
        if (testActionsContainer != null) testActionsContainer.SetActive(false);
    }

    public void EnterTestMode()
    {
        if (slotsContainer != null) slotsContainer.SetActive(false);
        if (testButton != null) testButton.gameObject.SetActive(false);
        if (testActionsContainer != null) testActionsContainer.SetActive(true);
    }

    public void OnTestClicked()
    {
        StressSimulator sim = FindFirstObjectByType<StressSimulator>();
        if (sim == null) return;

        bool started = sim.TryStartTestMode();
        if (started)
            EnterTestMode();
    }

    public void OnResetClicked()
    {
        _bridgeManager?.HardReset();
        EnterBuildMode();
    }

    public void OnUndoClicked()
    {
        FindFirstObjectByType<StressSimulator>()?.ResetToBuildMode();
        EnterBuildMode();
    }

    private void OnSimulationBlocked(string message)
    {
        if (feedbackText == null) return;
        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(ShowFeedback(message));
    }

    private System.Collections.IEnumerator ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        feedbackText.gameObject.SetActive(false);
    }

    private void RefreshAll()
    {
        foreach (var slot in slots) slot.Refresh();
    }

    private void RefreshSlot(string materialId)
    {
        foreach (var slot in slots)
            if (slot.MaterialId == materialId) slot.Refresh();
    }

    public void NotifySelectionChanged(string selectedId)
    {
        foreach (var slot in slots)
            slot.SetSelected(slot.MaterialId == selectedId);
    }
}