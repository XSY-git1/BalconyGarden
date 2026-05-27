using System;
using UnityEngine;

public class GameLocalSaveStore : MonoBehaviour
{
    [SerializeField] private string playerPrefsKey = "balconygarden.gameSave.v1";

    public bool HasSave => PlayerPrefs.HasKey(playerPrefsKey);

    public void Save(BalconyPotLayoutManager layoutManager, SeedInventory seedInventory, HarvestInventory harvestInventory)
    {
        if (layoutManager == null)
        {
            return;
        }

        GameSaveData saveData = new GameSaveData
        {
            savedUnixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            pots = layoutManager.CaptureSaveData().pots,
            seeds = seedInventory != null ? seedInventory.CaptureSaveData() : new System.Collections.Generic.List<SeedInventorySaveEntry>(),
            harvestedItems = harvestInventory != null ? harvestInventory.CaptureSaveData() : new System.Collections.Generic.List<HarvestInventorySaveEntry>()
        };

        PlayerPrefs.SetString(playerPrefsKey, JsonUtility.ToJson(saveData, false));
        PlayerPrefs.Save();
    }

    public bool Load(BalconyPotLayoutManager layoutManager, SeedInventory seedInventory, HarvestInventory harvestInventory)
    {
        if (layoutManager == null || !HasSave)
        {
            return false;
        }

        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(playerPrefsKey));
        if (saveData == null)
        {
            return false;
        }

        layoutManager.LoadPotLayout(new PotLayoutSaveData(saveData.pots));

        if (seedInventory != null)
        {
            seedInventory.LoadSaveData(saveData.seeds);
        }

        if (harvestInventory != null)
        {
            harvestInventory.LoadSaveData(saveData.harvestedItems);
        }

        return true;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }
}
