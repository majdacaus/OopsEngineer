using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialSlotUI : MonoBehaviour
{
    // -----------------------
    [Header("Data")]
    [SerializeField] private MaterialData materialData;

    [Header("Visuals")]
    [SerializeField] private Image       swatchImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image       selectionBorder;
    [SerializeField] private Button      slotButton;

    [Header("Colors")]
    [SerializeField] private Color normalBorderColor   = new Color(0.54f, 0.37f, 0.16f);
    [SerializeField] private Color selectedBorderColor = new Color(0.94f, 0.75f, 0.25f);
    [SerializeField] private Color lowQtyColor         = new Color(0.91f, 0.38f, 0.25f);
    [SerializeField] private Color normalQtyColor      = new Color(0.96f, 0.75f, 0.25f);

    private BridgeManager _bridgeManager;

    public string MaterialId => materialData != null ? materialData.materialId : "";

    private void Awake()
    {
        _bridgeManager = FindFirstObjectByType<BridgeManager>();
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
    }

    public void OnSlotClicked()
    {
        if (materialData == null || _bridgeManager == null) return;
        if (!MaterialInventory.Instance.IsOwned(materialData)) return;
        if (MaterialInventory.Instance.GetQuantity(materialData) <= 0) return;

        MaterialInventory.Instance.Equip(materialData);
        _bridgeManager.SetBeamPrefab(GetPrefabForMaterial());

        FindFirstObjectByType<InventoryUI>()?.NotifySelectionChanged(materialData.materialId);
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.color = selected ? selectedBorderColor : normalBorderColor;
    }

    public void Refresh()
    {
        if (materialData == null) return;

        bool owned = MaterialInventory.Instance.IsOwned(materialData);
        int  qty   = MaterialInventory.Instance.GetQuantity(materialData);

        if (swatchImage != null)
            swatchImage.color = materialData.swatchColor;

        if (nameText != null)
            nameText.text = materialData.displayName;

        if (quantityText != null)
        {
            quantityText.text  = owned ? $"×{qty}" : "Locked";
            quantityText.color = (owned && qty <= 3) ? lowQtyColor : normalQtyColor;
        }

        if (slotButton != null)
            slotButton.interactable = owned && qty > 0;

        float alpha = (owned && qty > 0) ? 1f : 0.45f;
        GetComponent<CanvasGroup>().alpha = alpha;
    }

    private GameObject GetPrefabForMaterial()
    {
        return materialData.beamPrefab;
    }
    // -----------------------
}