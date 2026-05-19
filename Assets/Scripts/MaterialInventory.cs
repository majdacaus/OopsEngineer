using System.Collections.Generic;
using UnityEngine;

public class MaterialInventory : MonoBehaviour
{
    public static MaterialInventory Instance { get; private set; }

    private const string PrefsKey      = "OwnedMaterials";
    // -----------------------
    private const string QuantityPrefix = "MatQty_";
    // -----------------------

    private HashSet<string> _ownedIds = new();

    public MaterialData EquippedMaterial { get; private set; }

    // -----------------------
    public static event System.Action<string> OnQuantityChanged;
// -----------------------
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public bool IsOwned(MaterialData material)    => _ownedIds.Contains(material.materialId);

    public bool IsLocked(MaterialData material)
    {
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        return playerLevel < material.requiredLevel;
    }

    public void Unlock(MaterialData material)
    {
        if (IsOwned(material)) return;
        _ownedIds.Add(material.materialId);
        Save();
    }

    // -----------------------
    public void AddQuantity(MaterialData material, int amount)
    {
        if (!IsOwned(material)) Unlock(material);
        int current = GetQuantity(material);
        PlayerPrefs.SetInt(QuantityPrefix + material.materialId, current + amount);
        PlayerPrefs.Save();
        
        // -----------------------
        OnQuantityChanged?.Invoke(material.materialId);
        // ---------------
    }

    public int GetQuantity(MaterialData material)
    {
        return PlayerPrefs.GetInt(QuantityPrefix + material.materialId, 0);
    }

    public bool ConsumeUnit(MaterialData material)
    {
        int qty = GetQuantity(material);
        if (qty <= 0) return false;
        PlayerPrefs.SetInt(QuantityPrefix + material.materialId, qty - 1);
        PlayerPrefs.Save();
        
        // -----------------------
        OnQuantityChanged?.Invoke(material.materialId);
        // ----
        return true;
    }
    // -----------------------

    public void Equip(MaterialData material)
    {
        if (!IsOwned(material)) return;
        EquippedMaterial = material;
        PlayerPrefs.SetString("EquippedMaterial", material.materialId);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string raw = PlayerPrefs.GetString(PrefsKey, "");
        if (!string.IsNullOrEmpty(raw))
            foreach (var id in raw.Split(','))
                _ownedIds.Add(id);
    }

    private void Save()
    {
        PlayerPrefs.SetString(PrefsKey, string.Join(",", _ownedIds));
        PlayerPrefs.Save();
    }
}