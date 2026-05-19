using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Bridge/Material Data")]
public class MaterialData : ScriptableObject
{
    [Header("Identity")]
    public string materialId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("Prefab")]
// -----------------------
    public GameObject beamPrefab;
// -----------------------

    [Header("Category")]
    public MaterialCategory category;

    [Header("Stats (1-10)")]
    [Range(1, 10)] public int strength;
    [Range(1, 10)] public int weight;
    [Range(1, 10)] public int costPerUnit;

    [Header("Shop")]
    public int coinPrice;
    public bool freeStarter;
    public bool isNew;
    public int requiredLevel;

    // -----------------------
    [Header("Quantity")]
    [Tooltip("How many units the player receives per purchase")]
    public int purchaseQuantity = 10;
    // -----------------------

    [Header("Visuals")]
    public Color swatchColor  = Color.white;
    public Color swatchBorderColor = Color.gray;
}

public enum MaterialCategory { Wood, Metal, Advanced }