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

    [Header("Item Grid")]
    public Transform gridParent;
    public GameObject itemCardPrefab;

    private void Awake()
    {
        if (shopPanel == null || gridParent == null || itemCardPrefab == null)
            Debug.LogError("ShopUI: Missing references in Inspector!", this);

        shopPanel.SetActive(false);

        if (closeShopButton != null)
            closeShopButton.SetActive(false);
    }

    private void OnEnable()
    {
        CoinManager.OnCoinsChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
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
    }

    public void CloseShop()
    {
        GameObject startPanel = GameObject.Find("StartScreenPanel");
        if (startPanel == null || !startPanel.activeSelf)
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

    private void RefreshBalance()
    {
        if (balanceText != null)
            balanceText.text = CoinManager.Instance.CurrentCoins.ToString();
    }

    private void OnCoinsChanged(int newAmount)
    {
        if (balanceText != null)
            balanceText.text = newAmount.ToString();
    }
    
}