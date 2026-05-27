using System;
using System.Collections.Generic;
using UnityEngine;

public class SeedInventory : MonoBehaviour
{
    [SerializeField] private List<SeedInventoryEntry> startingSeeds = new List<SeedInventoryEntry>();
    [SerializeField] private List<SeedInventoryEntry> seeds = new List<SeedInventoryEntry>();

    public IReadOnlyList<SeedInventoryEntry> StartingSeeds => startingSeeds;
    public IReadOnlyList<SeedInventoryEntry> Seeds => seeds;

    public bool HasAnySeed()
    {
        foreach (SeedInventoryEntry entry in seeds)
        {
            if (entry != null && entry.seedData != null && entry.count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void EnsureStartingSeedsIfEmpty()
    {
        if (!HasAnySeed())
        {
            ResetToStartingSeeds();
        }
    }

    public void ResetToStartingSeeds()
    {
        seeds.Clear();

        foreach (SeedInventoryEntry entry in startingSeeds)
        {
            if (entry == null || entry.seedData == null || entry.count <= 0)
            {
                continue;
            }

            seeds.Add(new SeedInventoryEntry(entry.seedData, entry.count));
        }
    }

    public int GetCount(string seedId)
    {
        SeedInventoryEntry entry = FindEntry(seedId);
        return entry != null ? entry.count : 0;
    }

    public bool HasSeed(string seedId)
    {
        return GetCount(seedId) > 0;
    }

    public bool TryConsumeSeed(string seedId, int amount = 1)
    {
        if (amount <= 0)
        {
            return false;
        }

        SeedInventoryEntry entry = FindEntry(seedId);
        if (entry == null || entry.count < amount)
        {
            return false;
        }

        entry.count -= amount;
        return true;
    }

    public void AddSeed(SeedData seedData, int amount)
    {
        if (seedData == null || string.IsNullOrEmpty(seedData.SeedId) || amount <= 0)
        {
            return;
        }

        SeedInventoryEntry entry = FindEntry(seedData.SeedId);
        if (entry == null)
        {
            seeds.Add(new SeedInventoryEntry(seedData, amount));
            return;
        }

        entry.count += amount;
    }

    public List<SeedInventorySaveEntry> CaptureSaveData()
    {
        List<SeedInventorySaveEntry> saveEntries = new List<SeedInventorySaveEntry>();

        foreach (SeedInventoryEntry entry in seeds)
        {
            if (entry != null && entry.seedData != null)
            {
                saveEntries.Add(new SeedInventorySaveEntry(entry.seedData.SeedId, entry.count));
            }
        }

        return saveEntries;
    }

    public void LoadSaveData(List<SeedInventorySaveEntry> saveEntries)
    {
        foreach (SeedInventoryEntry entry in seeds)
        {
            if (entry != null)
            {
                entry.count = 0;
            }
        }

        if (saveEntries == null)
        {
            return;
        }

        foreach (SeedInventorySaveEntry saveEntry in saveEntries)
        {
            SeedInventoryEntry entry = FindEntry(saveEntry.seedId);
            if (entry != null)
            {
                entry.count = Mathf.Max(0, saveEntry.count);
            }
        }
    }

    private SeedInventoryEntry FindEntry(string seedId)
    {
        if (string.IsNullOrEmpty(seedId))
        {
            return null;
        }

        foreach (SeedInventoryEntry entry in seeds)
        {
            if (entry != null && entry.seedData != null && entry.seedData.SeedId == seedId)
            {
                return entry;
            }
        }

        return null;
    }
}

[Serializable]
public class SeedInventoryEntry
{
    public SeedData seedData;
    [Min(0)] public int count;

    public SeedInventoryEntry()
    {
    }

    public SeedInventoryEntry(SeedData seedData, int count)
    {
        this.seedData = seedData;
        this.count = Mathf.Max(0, count);
    }
}
