using UnityEngine;

[CreateAssetMenu(fileName = "SeedData", menuName = "BalconyGarden/Seed Data")]
public class SeedData : ScriptableObject
{
    [SerializeField] private string seedId = "seed_id";
    [SerializeField] private string displayName = "Seed";
    [SerializeField] private string plantId = "plant_id";

    public string SeedId => seedId;
    public string DisplayName => displayName;
    public string PlantId => plantId;
}
