using System.Collections.Generic;
using UnityEngine;

public class MaterialInventory : MonoBehaviour
{
    public static MaterialInventory Instance { get; private set; }

    private const string PrefsKey = "OwnedMaterials";

    private HashSet<string> _ownedIds = new();

    public MaterialData EquippedMaterial { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public bool IsOwned(MaterialData material) => _ownedIds.Contains(material.materialId);

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
