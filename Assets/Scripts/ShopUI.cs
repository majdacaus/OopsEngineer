using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject shopPanel;
    public Animator panelAnimator;
    private static readonly int OpenHash = Animator.StringToHash("Open");

    [Header("Buttons")]
    public GameObject closeShopButton;
    public GameObject openShopButton;

    [Header("Coin Display")]
    public TextMeshProUGUI balanceText;

    // [Header("Tabs")]
    // public Button tabAll;
    // public Button tabWood;
    // public Button tabMetal;
    // public Button tabAdvanced;

    // [Header("Tab Colors")]
    // public Color tabActiveColor = new(0.22f, 0.55f, 0.95f, 0.15f);
    // public Color tabInactiveColor = Color.clear;
    // public Color tabActiveTextColor = new(0.18f, 0.47f, 0.87f);
    // public Color tabInactiveTextColor = new(0.5f, 0.5f, 0.5f);
    //
    [Header("Item Grid")]
    public Transform gridParent;
    public GameObject itemCardPrefab;
    //
    // [Header("Materials")]
    // public List<MaterialData> allMaterials;

    // [Header("Toast")]
    // public GameObject toastObject;
    // public TextMeshProUGUI toastText;
    // public float toastDuration = 2f;

    // private MaterialCategory? _activeFilter;
    // private readonly List<ShopItemUI> _spawnedCards = new();
    // private Coroutine _toastCoroutine;

    private void Awake()
    {
        // if (shopPanel == null || gridParent == null || itemCardPrefab == null)
            Debug.LogError("ShopUI: Missing references in Inspector!", this);

        shopPanel.SetActive(false);

        if (closeShopButton != null)
            closeShopButton.SetActive(false);

        // tabAll.onClick.AddListener(() => SetFilter(null));
        // tabWood.onClick.AddListener(() => SetFilter(MaterialCategory.Wood));
        // tabMetal.onClick.AddListener(() => SetFilter(MaterialCategory.Metal));
        // tabAdvanced.onClick.AddListener(() => SetFilter(MaterialCategory.Advanced));
    }

    private void OnEnable()
    {
        CoinManager.OnCoinsChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;

        // if (toastObject != null)
        //     toastObject.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0f;
        if (closeShopButton != null)
        {
            closeShopButton.SetActive(true);
            openShopButton.SetActive(false);
        }

        if (panelAnimator != null)
            panelAnimator.SetBool(OpenHash, true);

        RefreshBalance();
        SetFilter(null);
    }

    public void CloseShop()
    {
        Time.timeScale = 1f; 

        if (closeShopButton != null)
        {
            closeShopButton.SetActive(false);
            openShopButton.SetActive(true);
        }

        if (panelAnimator != null)
            panelAnimator.SetBool(OpenHash, false);
        else
            shopPanel.SetActive(false);
    }

    public void OnCloseAnimationFinished()
    {
        shopPanel.SetActive(false);
    }

    private void SetFilter(MaterialCategory? category)
    {
        // _activeFilter = category;
       // UpdateTabVisuals();
        //PopulateGrid();
    }

   /* private void UpdateTabVisuals()
    {
        SetTab(tabAll, _activeFilter == null);
        SetTab(tabWood, _activeFilter == MaterialCategory.Wood);
        SetTab(tabMetal, _activeFilter == MaterialCategory.Metal);
        SetTab(tabAdvanced, _activeFilter == MaterialCategory.Advanced);
    }*/

    // private void SetTab(Button tab, bool active)
    // {
    //     if (tab == null) return;
    //
    //     var img = tab.GetComponent<Image>();
    //     if (img != null)
    //         img.color = active ? tabActiveColor : tabInactiveColor;
    //
    //     var text = tab.GetComponentInChildren<TextMeshProUGUI>();
    //     if (text != null)
    //         text.color = active ? tabActiveTextColor : tabInactiveTextColor;
    // }

    // private void PopulateGrid()
    // {
    //     foreach (var card in _spawnedCards)
    //         if (card != null)
    //             Destroy(card.gameObject);
    //
    //     _spawnedCards.Clear();
    //
    //     foreach (var mat in allMaterials)
    //     {
    //         if (_activeFilter.HasValue && mat.category != _activeFilter.Value)
    //             continue;
    //
    //         var go = Instantiate(itemCardPrefab, gridParent);
    //         var ui = go.GetComponent<ShopItemUI>();
    //
    //         ui.Setup(mat, this);
    //         _spawnedCards.Add(ui);
    //     }
    // }

    // private void RefreshAllCards()
    // {
    //     foreach (var card in _spawnedCards)
    //         if (card != null)
    //             card.Refresh();
    // }

    public void TryPurchase(MaterialData material)
    {
        if (MaterialInventory.Instance.IsOwned(material)) return;
        if (MaterialInventory.Instance.IsLocked(material)) return;

        if (!material.freeStarter &&
            !CoinManager.Instance.SpendCoins(material.coinPrice))
        {
            //ShowToast("Not enough coins!");
            return;
        }

        MaterialInventory.Instance.Unlock(material);

        RefreshBalance();
       // RefreshAllCards();

       // ShowToast($"{material.displayName} unlocked!");
    }

    private void RefreshBalance()
    {
        if (balanceText != null)
            balanceText.text = CoinManager.Instance.CurrentCoins.ToString();
    }

    private void OnCoinsChanged(int newAmount)
    {
        if (balanceText != null)
            balanceText.text = newAmount.ToString();

        // RefreshAllCards();
    }

    // private void ShowToast(string message)
    // {
    //     if (_toastCoroutine != null)
    //         StopCoroutine(_toastCoroutine);
    //
    //     _toastCoroutine = StartCoroutine(ToastRoutine(message));
    // }
    //
    // private IEnumerator ToastRoutine(string message)
    // {
    //     if (toastText != null)
    //         toastText.text = message;
    //
    //     if (toastObject != null)
    //         toastObject.SetActive(true);
    //
    //     yield return new WaitForSeconds(toastDuration);
    //
    //     if (toastObject != null)
    //         toastObject.SetActive(false);
    // }
}