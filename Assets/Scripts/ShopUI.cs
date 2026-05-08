using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject shopPanel;
    public Animator panelAnimator;         // optional — hook up a slide-in animator
    private static readonly int OpenHash = Animator.StringToHash("Open");

    [Header("Coin Display")]
    public TextMeshProUGUI balanceText;

    [Header("Tabs")]
    public Button tabAll;
    public Button tabWood;
    public Button tabMetal;
    public Button tabAdvanced;

    [Header("Tab Highlight Colors")]
    public Color tabActiveColor = new Color(0.22f, 0.55f, 0.95f, 0.15f);
    public Color tabInactiveColor = Color.clear;
    public Color tabActiveTextColor = new Color(0.18f, 0.47f, 0.87f);
    public Color tabInactiveTextColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Item Grid")]
    public Transform gridParent;
    public GameObject itemCardPrefab;

    [Header("Materials")]
    public List<MaterialData> allMaterials;

    [Header("Toast")]
    public GameObject toastObject;
    public TextMeshProUGUI toastText;
    public float toastDuration = 2f;

    private MaterialCategory? _activeFilter = null; // null = All
    private readonly List<ShopItemUI> _spawnedCards = new();
    private Coroutine _toastCoroutine;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        shopPanel.SetActive(false);

        tabAll.onClick.AddListener(() => SetFilter(null));
        tabWood.onClick.AddListener(() => SetFilter(MaterialCategory.Wood));
        tabMetal.onClick.AddListener(() => SetFilter(MaterialCategory.Metal));
        tabAdvanced.onClick.AddListener(() => SetFilter(MaterialCategory.Advanced));
    }

    private void OnEnable()
    {
        CoinManager.OnCoinsChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
    }

    // ───────── Open / Close ─────────

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        if (panelAnimator != null) panelAnimator.SetBool(OpenHash, true);
        RefreshBalance();
        SetFilter(null);
    }

    public void CloseShop()
    {
        if (panelAnimator != null)
            panelAnimator.SetBool(OpenHash, false);
        else
            shopPanel.SetActive(false);
    }

    // Called by animator's exit event if you use one
    public void OnCloseAnimationFinished() => shopPanel.SetActive(false);

    // ───────── Tabs ─────────

    private void SetFilter(MaterialCategory? category)
    {
        _activeFilter = category;
        UpdateTabVisuals();
        PopulateGrid();
    }

    private void UpdateTabVisuals()
    {
        SetTabStyle(tabAll,      _activeFilter == null);
        SetTabStyle(tabWood,     _activeFilter == MaterialCategory.Wood);
        SetTabStyle(tabMetal,    _activeFilter == MaterialCategory.Metal);
        SetTabStyle(tabAdvanced, _activeFilter == MaterialCategory.Advanced);
    }

    private void SetTabStyle(Button tab, bool active)
    {
        var img = tab.GetComponent<Image>();
        if (img != null) img.color = active ? tabActiveColor : tabInactiveColor;
        var tmp = tab.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.color = active ? tabActiveTextColor : tabInactiveTextColor;
    }

    // ───────── Grid ─────────

    private void PopulateGrid()
    {
        // Clear old cards
        foreach (var card in _spawnedCards)
            Destroy(card.gameObject);
        _spawnedCards.Clear();

        foreach (var mat in allMaterials)
        {
            if (_activeFilter.HasValue && mat.category != _activeFilter.Value) continue;

            var go = Instantiate(itemCardPrefab, gridParent);
            var itemUI = go.GetComponent<ShopItemUI>();
            itemUI.Setup(mat, this);
            _spawnedCards.Add(itemUI);
        }
    }

    private void RefreshAllCards()
    {
        foreach (var card in _spawnedCards)
            card.Refresh();
    }

    // ───────── Purchase ─────────

    public void TryPurchase(MaterialData material)
    {
        if (MaterialInventory.Instance.IsOwned(material)) return;
        if (MaterialInventory.Instance.IsLocked(material)) return;

        if (!material.freeStarter && !CoinManager.Instance.SpendCoins(material.coinPrice))
        {
            ShowToast("Not enough coins!");
            return;
        }

        MaterialInventory.Instance.Unlock(material);
        RefreshBalance();
        RefreshAllCards();
        ShowToast($"{material.displayName} unlocked!");
    }

    // ───────── Coin Display ─────────

    private void RefreshBalance()
    {
        balanceText.text = CoinManager.Instance.CurrentCoins.ToString();
    }

    private void OnCoinsChanged(int newAmount)
    {
        balanceText.text = newAmount.ToString();
        RefreshAllCards(); // re-evaluate affordability highlights
    }

    // ───────── Toast ─────────

    private void ShowToast(string message)
    {
        if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(ToastRoutine(message));
    }

    private System.Collections.IEnumerator ToastRoutine(string message)
    {
        toastText.text = message;
        toastObject.SetActive(true);
        yield return new WaitForSeconds(toastDuration);
        toastObject.SetActive(false);
    }
}
