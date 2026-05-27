using System;

[Serializable]
public class PotInstance
{
    public string potInstanceId;
    public string potId;
    public int startSlotIndex;
    public int slotSize;
    public PlantInstance plant;

    public PotInstance()
    {
    }

    public PotInstance(string potInstanceId, string potId, int startSlotIndex, int slotSize)
    {
        this.potInstanceId = potInstanceId;
        this.potId = potId;
        this.startSlotIndex = startSlotIndex;
        this.slotSize = slotSize;
    }

    public int EndSlotIndex => startSlotIndex + slotSize - 1;

    public bool OccupiesSlot(int slotIndex)
    {
        return slotIndex >= startSlotIndex && slotIndex <= EndSlotIndex;
    }

    public PotInstance Clone()
    {
        PotInstance clone = new PotInstance(potInstanceId, potId, startSlotIndex, slotSize);
        clone.plant = plant != null ? plant.Clone() : null;
        return clone;
    }
}
