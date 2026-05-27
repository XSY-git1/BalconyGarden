using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int version = 1;
    public long savedUnixTimeSeconds;
    public List<PotInstance> pots = new List<PotInstance>();
    public List<SeedInventorySaveEntry> seeds = new List<SeedInventorySaveEntry>();
    public List<HarvestInventorySaveEntry> harvestedItems = new List<HarvestInventorySaveEntry>();
}

[Serializable]
public class SeedInventorySaveEntry
{
    public string seedId;
    public int count;

    public SeedInventorySaveEntry()
    {
    }

    public SeedInventorySaveEntry(string seedId, int count)
    {
        this.seedId = seedId;
        this.count = count;
    }
}

[Serializable]
public class HarvestInventorySaveEntry
{
    public string itemId;
    public string displayName;
    public int count;

    public HarvestInventorySaveEntry()
    {
    }

    public HarvestInventorySaveEntry(string itemId, string displayName, int count)
    {
        this.itemId = itemId;
        this.displayName = displayName;
        this.count = count;
    }
}
