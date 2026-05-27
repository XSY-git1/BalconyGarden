using System;
using System.Collections.Generic;
using UnityEngine;

public class HarvestInventory : MonoBehaviour
{
    [SerializeField] private List<HarvestInventoryEntry> items = new List<HarvestInventoryEntry>();

    public IReadOnlyList<HarvestInventoryEntry> Items => items;

    public void AddItem(string itemId, string displayName, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
        {
            return;
        }

        HarvestInventoryEntry entry = FindEntry(itemId);
        if (entry == null)
        {
            items.Add(new HarvestInventoryEntry(itemId, displayName, amount));
            return;
        }

        entry.count += amount;
        if (!string.IsNullOrEmpty(displayName))
        {
            entry.displayName = displayName;
        }
    }

    public List<HarvestInventorySaveEntry> CaptureSaveData()
    {
        List<HarvestInventorySaveEntry> saveEntries = new List<HarvestInventorySaveEntry>();

        foreach (HarvestInventoryEntry entry in items)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.itemId))
            {
                saveEntries.Add(new HarvestInventorySaveEntry(entry.itemId, entry.displayName, entry.count));
            }
        }

        return saveEntries;
    }

    public void LoadSaveData(List<HarvestInventorySaveEntry> saveEntries)
    {
        items.Clear();

        if (saveEntries == null)
        {
            return;
        }

        foreach (HarvestInventorySaveEntry saveEntry in saveEntries)
        {
            if (!string.IsNullOrEmpty(saveEntry.itemId) && saveEntry.count > 0)
            {
                items.Add(new HarvestInventoryEntry(saveEntry.itemId, saveEntry.displayName, saveEntry.count));
            }
        }
    }

    private HarvestInventoryEntry FindEntry(string itemId)
    {
        foreach (HarvestInventoryEntry entry in items)
        {
            if (entry != null && entry.itemId == itemId)
            {
                return entry;
            }
        }

        return null;
    }
}

[Serializable]
public class HarvestInventoryEntry
{
    public string itemId;
    public string displayName;
    [Min(0)] public int count;

    public HarvestInventoryEntry()
    {
    }

    public HarvestInventoryEntry(string itemId, string displayName, int count)
    {
        this.itemId = itemId;
        this.displayName = string.IsNullOrEmpty(displayName) ? itemId : displayName;
        this.count = Mathf.Max(0, count);
    }
}
