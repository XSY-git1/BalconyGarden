using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "BalconyGarden/Plant Data")]
public class PlantData : ScriptableObject
{
    [SerializeField] private string plantId = "plant_id";
    [SerializeField] private string displayName = "Plant";
    [SerializeField] private string harvestItemId = "item_harvest";
    [SerializeField] private string harvestDisplayName = "Harvest";
    [SerializeField] private GameObject plantPrefab = null;
    [SerializeField, Min(1)] private int totalGrowthMinutes = 30;
    [SerializeField] private Color seedColor = new Color(0.52f, 0.34f, 0.18f, 1f);
    [SerializeField] private Color sproutColor = new Color(0.42f, 0.78f, 0.32f, 1f);
    [SerializeField] private Color youngColor = new Color(0.24f, 0.68f, 0.28f, 1f);
    [SerializeField] private Color matureColor = new Color(0.95f, 0.78f, 0.24f, 1f);
    [SerializeField] private Vector2 seedWorldSize = new Vector2(0.32f, 0.24f);
    [SerializeField] private Vector2 sproutWorldSize = new Vector2(0.34f, 0.45f);
    [SerializeField] private Vector2 youngWorldSize = new Vector2(0.4f, 0.7f);
    [SerializeField] private Vector2 matureWorldSize = new Vector2(0.48f, 0.95f);
    [SerializeField] private Vector2 viewOffset = new Vector2(0f, 0.55f);
    [SerializeField] private int sortingOrder = 20;

    public string PlantId => plantId;
    public string DisplayName => displayName;
    public string HarvestItemId => harvestItemId;
    public string HarvestDisplayName => harvestDisplayName;
    public GameObject PlantPrefab => plantPrefab;
    public int TotalGrowthMinutes => Mathf.Max(1, totalGrowthMinutes);
    public Vector2 ViewOffset => viewOffset;
    public int SortingOrder => sortingOrder;

    public Color GetStageColor(PlantGrowthStage stage)
    {
        switch (stage)
        {
            case PlantGrowthStage.Seed:
                return seedColor;
            case PlantGrowthStage.Sprout:
                return sproutColor;
            case PlantGrowthStage.Young:
                return youngColor;
            case PlantGrowthStage.Mature:
                return matureColor;
            default:
                return seedColor;
        }
    }

    public Vector2 GetStageWorldSize(PlantGrowthStage stage)
    {
        switch (stage)
        {
            case PlantGrowthStage.Seed:
                return seedWorldSize;
            case PlantGrowthStage.Sprout:
                return sproutWorldSize;
            case PlantGrowthStage.Young:
                return youngWorldSize;
            case PlantGrowthStage.Mature:
                return matureWorldSize;
            default:
                return seedWorldSize;
        }
    }

    private void OnValidate()
    {
        totalGrowthMinutes = Mathf.Max(1, totalGrowthMinutes);
        seedWorldSize = ClampWorldSize(seedWorldSize);
        sproutWorldSize = ClampWorldSize(sproutWorldSize);
        youngWorldSize = ClampWorldSize(youngWorldSize);
        matureWorldSize = ClampWorldSize(matureWorldSize);
    }

    private Vector2 ClampWorldSize(Vector2 size)
    {
        size.x = Mathf.Max(0.1f, size.x);
        size.y = Mathf.Max(0.1f, size.y);
        return size;
    }
}
