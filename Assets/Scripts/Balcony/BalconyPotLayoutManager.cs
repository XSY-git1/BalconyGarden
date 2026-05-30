using System;
using System.Collections.Generic;
using UnityEngine;

public class BalconyPotLayoutManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField, Min(1)] private int slotCount = 5;
    [SerializeField] private PotSlot[] slots = new PotSlot[5];
    [SerializeField] private bool autoFindSlotsInChildren = true;

    [Header("Pot Data")]
    [SerializeField] private PotData[] potCatalog = new PotData[0];
    [SerializeField] private PotData selectedPotData;

    [Header("Plant Data")]
    [SerializeField] private PlantData[] plantCatalog = new PlantData[0];

    [Header("Views")]
    [SerializeField] private Transform potViewRoot = null;
    [SerializeField] private bool placeSelectedPotWhenClickingEmptySlot = true;
    [SerializeField] private bool autoFitPotViewsToSlots = true;
    [SerializeField, Min(0.1f)] private float singleSlotPotViewWidth = 1.25f;

    [Header("Growth")]
    [SerializeField] private bool autoRefreshPlantGrowth = true;
    [SerializeField, Min(0.1f)] private float growthRefreshIntervalSeconds = 1f;

    [Header("Runtime Data")]
    [SerializeField] private List<PotInstance> placedPots = new List<PotInstance>();

    private readonly Dictionary<string, PotView> potViewsByInstanceId = new Dictionary<string, PotView>();
    private PotInstance[] slotOccupancy;
    private string selectedPotInstanceId;
    private float growthRefreshTimer;

    public event Action<PotInstance> PotSelected;
    public event Action LayoutChanged;
    public event Action<PotInstance, PlantInstance> PlantChanged;

    public IReadOnlyList<PotInstance> PlacedPots => placedPots;
    public IReadOnlyList<PotData> PotCatalog => potCatalog;
    public IReadOnlyList<PlantData> PlantCatalog => plantCatalog;
    public PotInstance SelectedPot => GetPotByInstanceId(selectedPotInstanceId);
    public PotData SelectedPotData => selectedPotData;
    public int SlotCount => slotCount;

    private void Awake()
    {
        InitializeLayout();
    }

    private void Reset()
    {
        slotCount = 5;
        CollectSlotsFromChildren();
    }

    private void OnValidate()
    {
        slotCount = Mathf.Max(1, slotCount);
        growthRefreshIntervalSeconds = Mathf.Max(0.1f, growthRefreshIntervalSeconds);
    }

    private void Update()
    {
        if (!autoRefreshPlantGrowth || placedPots.Count == 0)
        {
            return;
        }

        growthRefreshTimer += Time.deltaTime;
        if (growthRefreshTimer < growthRefreshIntervalSeconds)
        {
            return;
        }

        growthRefreshTimer = 0f;
        RefreshAllPlantGrowth();
    }

    public void InitializeLayout()
    {
        if (autoFindSlotsInChildren)
        {
            CollectSlotsFromChildren();
        }

        BindSlots();
        RebuildSlotOccupancy();
        RefreshAllPlantGrowth();
        RebuildPotViews();
    }

    public void SetSelectedPotData(PotData potData)
    {
        selectedPotData = potData;
    }

    public void SelectPotDataById(string potId)
    {
        selectedPotData = FindPotData(potId);
    }

    public void HandleSlotClicked(PotSlot slot)
    {
        if (slot == null)
        {
            return;
        }

        PotInstance potAtSlot = GetPotAtSlot(slot.SlotIndex);
        if (potAtSlot != null)
        {
            SelectPot(potAtSlot);
            return;
        }

        SelectPot(null);

        if (placeSelectedPotWhenClickingEmptySlot && selectedPotData != null)
        {
            PlacePot(selectedPotData, slot.SlotIndex, out _);
        }
    }

    public bool CanPlacePot(PotData potData, int startSlotIndex)
    {
        if (potData == null || string.IsNullOrEmpty(potData.PotId))
        {
            return false;
        }

        return CanPlacePot(potData.SlotSize, startSlotIndex);
    }

    public bool CanPlacePot(int slotSize, int startSlotIndex)
    {
        EnsureSlotOccupancy();

        if (slotSize <= 0 || startSlotIndex < 0 || startSlotIndex + slotSize > slotCount)
        {
            return false;
        }

        for (int i = startSlotIndex; i < startSlotIndex + slotSize; i++)
        {
            if (slotOccupancy[i] != null)
            {
                return false;
            }
        }

        return true;
    }

    public PotInstance PlacePot(PotData potData, int startSlotIndex)
    {
        PlacePot(potData, startSlotIndex, out PotInstance placedPot);
        return placedPot;
    }

    public bool PlacePot(PotData potData, int startSlotIndex, out PotInstance placedPot)
    {
        placedPot = null;

        if (!CanPlacePot(potData, startSlotIndex))
        {
            return false;
        }

        placedPot = new PotInstance(CreatePotInstanceId(), potData.PotId, startSlotIndex, potData.SlotSize);
        placedPots.Add(placedPot);
        MarkSlotsOccupied(placedPot);
        SpawnPotView(placedPot);
        SelectPot(placedPot);
        RefreshSlotDebugStates();
        LayoutChanged?.Invoke();
        return true;
    }

    public bool RemovePot(PotInstance potInstance)
    {
        if (potInstance == null)
        {
            return false;
        }

        return RemovePot(potInstance.potInstanceId);
    }

    public bool RemoveSelectedPot()
    {
        return RemovePot(selectedPotInstanceId);
    }

    public bool CanPlantSeedInSelectedPot(SeedData seedData)
    {
        return CanPlantSeed(SelectedPot, seedData);
    }

    public bool CanPlantSeed(PotInstance pot, SeedData seedData)
    {
        return string.IsNullOrEmpty(GetPlantSeedFailureReason(pot, seedData));
    }

    public string GetPlantSeedFailureReason(PotInstance pot, SeedData seedData)
    {
        if (pot == null)
        {
            return "Select a pot first";
        }

        if (seedData == null)
        {
            return "Select a seed first";
        }

        if (ClearInvalidPlantIfNeeded(pot))
        {
            RefreshPlantView(pot);
        }

        if (pot.plant != null)
        {
            return "This pot already has a plant";
        }

        if (string.IsNullOrEmpty(seedData.SeedId))
        {
            return "Selected seed has no seedId";
        }

        if (string.IsNullOrEmpty(seedData.PlantId))
        {
            return "Selected seed has no plantId";
        }

        if (FindPlantData(seedData.PlantId) == null)
        {
            return $"No PlantData found for plantId '{seedData.PlantId}'";
        }

        return string.Empty;
    }

    public bool PlantSeedInSelectedPot(SeedData seedData, out PlantInstance plant)
    {
        return PlantSeed(SelectedPot, seedData, out plant);
    }

    public bool PlantSeed(PotInstance pot, SeedData seedData, out PlantInstance plant)
    {
        plant = null;

        if (!CanPlantSeed(pot, seedData))
        {
            return false;
        }

        plant = new PlantInstance(CreatePlantInstanceId(), seedData.PlantId, seedData.SeedId, GetCurrentUnixTimeSeconds());
        RefreshPlantGrowth(pot, plant);
        pot.plant = plant;
        RefreshPlantView(pot);
        PlantChanged?.Invoke(pot, plant);
        LayoutChanged?.Invoke();
        return true;
    }

    public void RefreshAllPlantGrowth()
    {
        foreach (PotInstance pot in placedPots)
        {
            if (pot == null || pot.plant == null)
            {
                continue;
            }

            if (ClearInvalidPlantIfNeeded(pot))
            {
                RefreshPlantView(pot);
                continue;
            }

            RefreshPlantGrowth(pot, pot.plant);
            RefreshPlantView(pot);
        }
    }

    public string GetSelectedPlantStageText()
    {
        PotInstance selectedPot = SelectedPot;
        if (selectedPot == null || selectedPot.plant == null)
        {
            return "None";
        }

        if (ClearInvalidPlantIfNeeded(selectedPot))
        {
            RefreshPlantView(selectedPot);
            return "None";
        }

        PlantData plantData = FindPlantData(selectedPot.plant.plantId);
        if (plantData == null)
        {
            return selectedPot.plant.currentStage.ToString();
        }

        double elapsedMinutes = GetElapsedGrowthMinutes(selectedPot.plant);
        return $"{selectedPot.plant.currentStage} ({elapsedMinutes:0.0}/{plantData.TotalGrowthMinutes} min)";
    }

    public PlantData GetSelectedPlantData()
    {
        return GetPlantDataForPot(SelectedPot);
    }

    public PlantData GetPlantDataForPot(PotInstance pot)
    {
        if (pot == null || pot.plant == null)
        {
            return null;
        }

        if (ClearInvalidPlantIfNeeded(pot))
        {
            RefreshPlantView(pot);
            return null;
        }

        return FindPlantData(pot.plant.plantId);
    }

    public bool CanHarvestSelectedPlant()
    {
        return CanHarvestPlant(SelectedPot);
    }

    public bool CanHarvestPlant(PotInstance pot)
    {
        if (pot == null || pot.plant == null)
        {
            return false;
        }

        if (ClearInvalidPlantIfNeeded(pot))
        {
            RefreshPlantView(pot);
            return false;
        }

        RefreshPlantGrowth(pot, pot.plant);
        return pot.plant.currentStage == PlantGrowthStage.Mature;
    }

    public bool HarvestSelectedPlant(out string harvestItemId, out string harvestDisplayName)
    {
        return HarvestPlant(SelectedPot, out harvestItemId, out harvestDisplayName);
    }

    public bool HarvestPlant(PotInstance pot, out string harvestItemId, out string harvestDisplayName)
    {
        harvestItemId = string.Empty;
        harvestDisplayName = string.Empty;

        if (!CanHarvestPlant(pot))
        {
            return false;
        }

        PlantData plantData = FindPlantData(pot.plant.plantId);
        if (plantData != null)
        {
            harvestItemId = plantData.HarvestItemId;
            harvestDisplayName = plantData.HarvestDisplayName;
        }

        pot.plant = null;
        RefreshPlantView(pot);
        PlantChanged?.Invoke(pot, null);
        LayoutChanged?.Invoke();
        return true;
    }

    public void ClearLayout()
    {
        ClearPotViews();
        placedPots.Clear();
        selectedPotInstanceId = string.Empty;
        RebuildSlotOccupancy();
        PotSelected?.Invoke(null);
        LayoutChanged?.Invoke();
    }

    public bool RemovePot(string potInstanceId)
    {
        if (string.IsNullOrEmpty(potInstanceId))
        {
            return false;
        }

        PotInstance pot = GetPotByInstanceId(potInstanceId);
        if (pot == null)
        {
            return false;
        }

        placedPots.Remove(pot);
        RemovePotView(potInstanceId);

        if (selectedPotInstanceId == potInstanceId)
        {
            selectedPotInstanceId = string.Empty;
            PotSelected?.Invoke(null);
            PlantChanged?.Invoke(null, null);
        }

        RebuildSlotOccupancy();
        LayoutChanged?.Invoke();
        return true;
    }

    public PotInstance GetPotAtSlot(int slotIndex)
    {
        EnsureSlotOccupancy();

        if (slotIndex < 0 || slotIndex >= slotOccupancy.Length)
        {
            return null;
        }

        return slotOccupancy[slotIndex];
    }

    public void RebuildSlotOccupancy()
    {
        slotOccupancy = new PotInstance[slotCount];
        List<PotInstance> validPots = new List<PotInstance>();

        foreach (PotInstance pot in placedPots)
        {
            if (!CanRegisterPot(pot))
            {
                Debug.LogWarning($"Skipped invalid pot layout entry: {DescribePot(pot)}", this);
                continue;
            }

            ClearInvalidPlantIfNeeded(pot);
            MarkSlotsOccupied(pot);
            validPots.Add(pot);
        }

        placedPots = validPots;
        RefreshSlotDebugStates();
    }

    public PotLayoutSaveData CaptureSaveData()
    {
        RefreshAllPlantGrowth();
        return new PotLayoutSaveData(placedPots);
    }

    public string CaptureSaveJson(bool prettyPrint = true)
    {
        return JsonUtility.ToJson(CaptureSaveData(), prettyPrint);
    }

    public void LoadPotLayout(PotLayoutSaveData saveData)
    {
        ClearPotViews();
        selectedPotInstanceId = string.Empty;
        placedPots = saveData != null ? new PotLayoutSaveData(saveData.pots).pots : new List<PotInstance>();
        RebuildSlotOccupancy();
        RefreshAllPlantGrowth();
        RebuildPotViews();
        LayoutChanged?.Invoke();
    }

    public void LoadPotLayoutJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            LoadPotLayout(null);
            return;
        }

        LoadPotLayout(JsonUtility.FromJson<PotLayoutSaveData>(json));
    }

    public void SelectPot(PotInstance potInstance)
    {
        selectedPotInstanceId = potInstance != null ? potInstance.potInstanceId : string.Empty;
        RefreshSlotDebugStates();
        RefreshPotViewSelection();
        PotSelected?.Invoke(potInstance);
    }

    private void CollectSlotsFromChildren()
    {
        slots = GetComponentsInChildren<PotSlot>(true);
        Array.Sort(slots, CompareSlots);
    }

    private int CompareSlots(PotSlot a, PotSlot b)
    {
        int indexCompare = a.SlotIndex.CompareTo(b.SlotIndex);
        if (indexCompare != 0)
        {
            return indexCompare;
        }

        return a.transform.position.x.CompareTo(b.transform.position.x);
    }

    private void BindSlots()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length && i < slotCount; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Bind(this, i);
            }
        }
    }

    private void EnsureSlotOccupancy()
    {
        if (slotOccupancy == null || slotOccupancy.Length != slotCount)
        {
            RebuildSlotOccupancy();
        }
    }

    private bool CanRegisterPot(PotInstance pot)
    {
        if (pot == null || string.IsNullOrEmpty(pot.potInstanceId) || string.IsNullOrEmpty(pot.potId))
        {
            return false;
        }

        if (pot.slotSize <= 0 || pot.startSlotIndex < 0 || pot.startSlotIndex + pot.slotSize > slotCount)
        {
            return false;
        }

        for (int i = pot.startSlotIndex; i < pot.startSlotIndex + pot.slotSize; i++)
        {
            if (slotOccupancy[i] != null)
            {
                return false;
            }
        }

        return true;
    }

    private void MarkSlotsOccupied(PotInstance pot)
    {
        for (int i = pot.startSlotIndex; i < pot.startSlotIndex + pot.slotSize; i++)
        {
            slotOccupancy[i] = pot;
        }
    }

    private void SpawnPotView(PotInstance pot)
    {
        PotData potData = FindPotData(pot.potId);
        if (potData == null || potData.PotPrefab == null)
        {
            return;
        }

        Transform root = potViewRoot != null ? potViewRoot : transform;
        Vector3 position = GetPotViewPosition(pot) + (Vector3)potData.ViewOffset;
        GameObject viewObject = Instantiate(potData.PotPrefab, position, Quaternion.identity, root);
        PotView potView = viewObject.GetComponent<PotView>();

        if (potView == null)
        {
            potView = viewObject.AddComponent<PotView>();
        }

        potView.Bind(pot, potData, this);
        if (autoFitPotViewsToSlots)
        {
            potView.FitToWorldWidth(GetPotViewTargetWidth(pot));
        }

        potViewsByInstanceId[pot.potInstanceId] = potView;
        RefreshPlantView(pot);
        potView.SetSelected(pot.potInstanceId == selectedPotInstanceId);
    }

    private void RefreshPlantView(PotInstance pot)
    {
        if (pot == null || string.IsNullOrEmpty(pot.potInstanceId))
        {
            return;
        }

        if (!potViewsByInstanceId.TryGetValue(pot.potInstanceId, out PotView potView) || potView == null)
        {
            return;
        }

        if (pot.plant == null)
        {
            potView.ClearPlant();
            return;
        }

        if (ClearInvalidPlantIfNeeded(pot))
        {
            potView.ClearPlant();
            return;
        }

        RefreshPlantGrowth(pot, pot.plant);
        PlantData plantData = FindPlantData(pot.plant.plantId);
        potView.ShowPlant(pot.plant, plantData);
    }

    private void RefreshPlantGrowth(PotInstance pot, PlantInstance plant)
    {
        if (pot == null || plant == null)
        {
            return;
        }

        PlantData plantData = FindPlantData(plant.plantId);
        if (plantData == null)
        {
            return;
        }

        if (plant.plantedUnixTimeSeconds <= 0)
        {
            plant.plantedUnixTimeSeconds = GetCurrentUnixTimeSeconds();
        }

        plant.currentStage = CalculateGrowthStage(plant, plantData);
    }

    private bool ClearInvalidPlantIfNeeded(PotInstance pot)
    {
        if (pot == null || pot.plant == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(pot.plant.plantId) && FindPlantData(pot.plant.plantId) != null)
        {
            return false;
        }

        pot.plant = null;
        return true;
    }

    private PlantGrowthStage CalculateGrowthStage(PlantInstance plant, PlantData plantData)
    {
        double elapsedMinutes = GetElapsedGrowthMinutes(plant);
        double growthProgress = Mathf.Clamp01((float)(elapsedMinutes / plantData.TotalGrowthMinutes));

        if (growthProgress >= 1f)
        {
            return PlantGrowthStage.Mature;
        }

        if (growthProgress >= 0.5f)
        {
            return PlantGrowthStage.Young;
        }

        if (growthProgress >= 0.25f)
        {
            return PlantGrowthStage.Sprout;
        }

        return PlantGrowthStage.Seed;
    }

    private double GetElapsedGrowthMinutes(PlantInstance plant)
    {
        if (plant == null || plant.plantedUnixTimeSeconds <= 0)
        {
            return 0d;
        }

        long elapsedSeconds = Math.Max(0, GetCurrentUnixTimeSeconds() - plant.plantedUnixTimeSeconds);
        return elapsedSeconds / 60d;
    }

    private float GetPotViewTargetWidth(PotInstance pot)
    {
        if (pot == null || pot.slotSize <= 1)
        {
            return singleSlotPotViewWidth;
        }

        return singleSlotPotViewWidth + GetAverageSlotSpacing() * (pot.slotSize - 1);
    }

    private float GetAverageSlotSpacing()
    {
        if (slots == null || slots.Length < 2)
        {
            return singleSlotPotViewWidth;
        }

        float totalSpacing = 0f;
        int spacingCount = 0;

        for (int i = 1; i < slots.Length && i < slotCount; i++)
        {
            if (slots[i - 1] == null || slots[i] == null)
            {
                continue;
            }

            totalSpacing += Mathf.Abs(slots[i].transform.position.x - slots[i - 1].transform.position.x);
            spacingCount++;
        }

        return spacingCount > 0 ? totalSpacing / spacingCount : singleSlotPotViewWidth;
    }

    private Vector3 GetPotViewPosition(PotInstance pot)
    {
        if (slots == null || slots.Length == 0)
        {
            return transform.position;
        }

        int firstSlotIndex = Mathf.Clamp(pot.startSlotIndex, 0, slots.Length - 1);
        int lastSlotIndex = Mathf.Clamp(pot.EndSlotIndex, 0, slots.Length - 1);

        if (slots[firstSlotIndex] == null)
        {
            return transform.position;
        }

        Vector3 firstPosition = slots[firstSlotIndex].transform.position;
        Vector3 lastPosition = slots[lastSlotIndex] != null ? slots[lastSlotIndex].transform.position : firstPosition;
        return (firstPosition + lastPosition) * 0.5f;
    }

    private void RebuildPotViews()
    {
        ClearPotViews();

        foreach (PotInstance pot in placedPots)
        {
            if (pot != null && pot.plant != null)
            {
                RefreshPlantGrowth(pot, pot.plant);
            }

            SpawnPotView(pot);
        }

        RefreshPotViewSelection();
    }

    private void RemovePotView(string potInstanceId)
    {
        if (!potViewsByInstanceId.TryGetValue(potInstanceId, out PotView potView) || potView == null)
        {
            potViewsByInstanceId.Remove(potInstanceId);
            return;
        }

        DestroyViewObject(potView.gameObject);
        potViewsByInstanceId.Remove(potInstanceId);
    }

    private void ClearPotViews()
    {
        foreach (PotView potView in potViewsByInstanceId.Values)
        {
            if (potView != null)
            {
                DestroyViewObject(potView.gameObject);
            }
        }

        potViewsByInstanceId.Clear();
    }

    private void DestroyViewObject(GameObject viewObject)
    {
        if (Application.isPlaying)
        {
            Destroy(viewObject);
        }
        else
        {
            DestroyImmediate(viewObject);
        }
    }

    private void RefreshSlotDebugStates()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length && i < slotCount; i++)
        {
            PotSlot slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            PotInstance pot = GetPotAtSlot(i);
            bool selected = pot != null && pot.potInstanceId == selectedPotInstanceId;
            slot.SetDebugState(pot != null, selected);
        }
    }

    private void RefreshPotViewSelection()
    {
        foreach (KeyValuePair<string, PotView> pair in potViewsByInstanceId)
        {
            if (pair.Value != null)
            {
                pair.Value.SetSelected(pair.Key == selectedPotInstanceId);
            }
        }
    }

    private PotData FindPotData(string potId)
    {
        if (string.IsNullOrEmpty(potId) || potCatalog == null)
        {
            return null;
        }

        foreach (PotData potData in potCatalog)
        {
            if (potData != null && potData.PotId == potId)
            {
                return potData;
            }
        }

        return null;
    }

    private PlantData FindPlantData(string plantId)
    {
        if (string.IsNullOrEmpty(plantId) || plantCatalog == null)
        {
            return null;
        }

        foreach (PlantData plantData in plantCatalog)
        {
            if (plantData != null && plantData.PlantId == plantId)
            {
                return plantData;
            }
        }

        return null;
    }

    private PotInstance GetPotByInstanceId(string potInstanceId)
    {
        if (string.IsNullOrEmpty(potInstanceId))
        {
            return null;
        }

        foreach (PotInstance pot in placedPots)
        {
            if (pot != null && pot.potInstanceId == potInstanceId)
            {
                return pot;
            }
        }

        return null;
    }

    private string CreatePotInstanceId()
    {
        return $"pot_{Guid.NewGuid():N}";
    }

    private string CreatePlantInstanceId()
    {
        return $"plant_{Guid.NewGuid():N}";
    }

    private long GetCurrentUnixTimeSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private string DescribePot(PotInstance pot)
    {
        if (pot == null)
        {
            return "<null>";
        }

        return $"{pot.potInstanceId} / {pot.potId} / start:{pot.startSlotIndex} / size:{pot.slotSize}";
    }
}
