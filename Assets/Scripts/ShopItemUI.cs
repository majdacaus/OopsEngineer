using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("References")]
    public Image swatchImage;
    public Image swatchBorder;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public GameObject priceGroup; 
    [Header("Stat Bars")]
    public RectTransform strengthBar;
    public RectTransform weightBar;
    public RectTransform costBar;

    [Header("Badges")]
    public GameObject badgeOwned;
    public GameObject badgeNew;
    public GameObject badgeLocked;

    [Header("Buttons")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;
    public Image buyButtonImage;

    [Header("State Tint")]
    public CanvasGroup canvasGroup;

    [Header("Colors")]
    public Color ctaColor = new Color(0.22f, 0.55f, 0.95f, 0.15f);
    public Color defaultButtonColor = Color.white;

    private MaterialData _data;
    private ShopUI _shopUI;

    private const float MaxBarWidth = 80f;

    public void Setup(MaterialData data, ShopUI shopUI)
    {
        _data = data;
        _shopUI = shopUI;
        Refresh();
    }

    public void Refresh()
    {
        bool owned = MaterialInventory.Instance.IsOwned(_data);
        bool locked = MaterialInventory.Instance.IsLocked(_data);
        bool canAfford = CoinManager.Instance.CurrentCoins >= _data.coinPrice;

        swatchImage.color = _data.swatchColor;
        if (swatchBorder != null) swatchBorder.color = _data.swatchBorderColor;

        nameText.text = _data.displayName;
        descriptionText.text = _data.description;

        SetBarWidth(strengthBar, _data.strength);
        SetBarWidth(weightBar, _data.weight);
        SetBarWidth(costBar, _data.costPerUnit);

        bool showPrice = !owned && !_data.freeStarter;
        priceGroup.SetActive(showPrice);
        if (showPrice) priceText.text = _data.coinPrice.ToString();

        badgeOwned.SetActive(owned);
        badgeNew.SetActive(!owned && !locked && _data.isNew);
        badgeLocked.SetActive(locked);

        buyButton.gameObject.SetActive(!owned);
        if (!owned)
        {
            buyButton.interactable = !locked && (canAfford || _data.freeStarter);
            buyButtonText.text = _data.freeStarter ? "Free" : locked ? "Locked" : "Buy";
            buyButtonImage.color = (!locked && canAfford) ? ctaColor : defaultButtonColor;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = (!owned && !locked && !canAfford) ? 0.55f : 1f;
    }

   /* public void OnBuyClicked()
    {
        _shopUI.TryPurchase(_data);
    }*/

    private void SetBarWidth(RectTransform bar, int value)
    {
        if (bar == null) return;
        var sd = bar.sizeDelta;
        sd.x = (value / 10f) * MaxBarWidth;
        bar.sizeDelta = sd;
    }
}
