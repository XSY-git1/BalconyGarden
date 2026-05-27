using System;

[Serializable]
public class PlantInstance
{
    public string plantInstanceId;
    public string plantId;
    public string seedId;
    public long plantedUnixTimeSeconds;
    public PlantGrowthStage currentStage;

    public PlantInstance()
    {
    }

    public PlantInstance(string plantInstanceId, string plantId, string seedId, long plantedUnixTimeSeconds)
    {
        this.plantInstanceId = plantInstanceId;
        this.plantId = plantId;
        this.seedId = seedId;
        this.plantedUnixTimeSeconds = plantedUnixTimeSeconds;
        currentStage = PlantGrowthStage.Seed;
    }

    public PlantInstance Clone()
    {
        PlantInstance clone = new PlantInstance(plantInstanceId, plantId, seedId, plantedUnixTimeSeconds);
        clone.currentStage = currentStage;
        return clone;
    }
}
