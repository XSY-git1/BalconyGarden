using UnityEngine;

public class PotLayoutDebugPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BalconyPotLayoutManager layoutManager;
    [SerializeField] private GameLocalSaveStore saveStore;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private HarvestInventory harvestInventory;
    [SerializeField] private bool showPanel = true;
    [SerializeField] private Rect panelRect = new Rect(16f, 16f, 290f, 390f);
    [SerializeField] private Vector2 minimumPanelSize = new Vector2(300f, 420f);

    private string statusText = "Ready";
    private SeedData selectedSeedData;
    private Vector2 scrollPosition;

    private void Awake()
    {
        EnsurePanelSize();

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (layoutManager == null)
        {
            layoutManager = FindObjectOfType<BalconyPotLayoutManager>();
        }

        if (saveStore == null)
        {
            saveStore = GetComponent<GameLocalSaveStore>();
        }

        if (saveStore == null)
        {
            saveStore = gameObject.AddComponent<GameLocalSaveStore>();
        }

        if (seedInventory == null)
        {
            seedInventory = FindObjectOfType<SeedInventory>();
        }

        if (harvestInventory == null)
        {
            harvestInventory = GetComponent<HarvestInventory>();
        }

        if (harvestInventory == null)
        {
            harvestInventory = gameObject.AddComponent<HarvestInventory>();
        }
    }

    private void OnValidate()
    {
        minimumPanelSize.x = Mathf.Max(220f, minimumPanelSize.x);
        minimumPanelSize.y = Mathf.Max(260f, minimumPanelSize.y);
    }

    private void OnGUI()
    {
        if (!showPanel)
        {
            return;
        }

        EnsurePanelSize();
        panelRect = GUI.Window(GetInstanceID(), panelRect, DrawPanel, "Pot Layout Debug");
        ClampPanelToScreen();
    }

    private void DrawPanel(int windowId)
    {
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            false,
            true,
            GUILayout.Width(panelRect.width - 10f),
            GUILayout.Height(panelRect.height - 38f));

        if (layoutManager == null)
        {
            GUILayout.Label("No BalconyPotLayoutManager found.");
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
            return;
        }

        DrawPanelContent();
        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
    }

    private void DrawPanelContent()
    {
        GUILayout.Label($"Selected Pot Data: {GetSelectedPotDataName()}");
        GUILayout.Label($"Selected Pot: {GetSelectedPotName()}");
        GUILayout.Label($"Selected Pot Plant: {GetSelectedPotPlantName()}");
        GUILayout.Label($"Selected Plant Stage: {GetSelectedPlantStageName()}");
        GUILayout.Label($"Selected Seed: {GetSelectedSeedName()}");

        GUILayout.Space(6f);
        GUILayout.Label("Select Pot Type");

        foreach (PotData potData in layoutManager.PotCatalog)
        {
            if (potData == null)
            {
                continue;
            }

            if (GUILayout.Button($"{potData.DisplayName} ({potData.SlotSize} slot)"))
            {
                layoutManager.SetSelectedPotData(potData);
                statusText = $"Selected {potData.DisplayName}";
            }
        }

        GUILayout.Space(6f);
        DrawSeedControls();

        GUILayout.Space(6f);

        if (GUILayout.Button("Remove Selected Pot"))
        {
            statusText = layoutManager.RemoveSelectedPot() ? "Removed selected pot" : "No selected pot to remove";
        }

        if (GUILayout.Button("Refresh Growth"))
        {
            layoutManager.RefreshAllPlantGrowth();
            statusText = "Refreshed plant growth";
        }

        GUI.enabled = layoutManager.CanHarvestSelectedPlant();
        if (GUILayout.Button("Harvest Selected Plant"))
        {
            TryHarvestSelectedPlant();
        }
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            SaveGame();
            statusText = "Saved game";
        }

        if (GUILayout.Button("Load"))
        {
            statusText = LoadGame() ? "Loaded game" : "No saved game";
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Layout"))
        {
            layoutManager.ClearLayout();
            statusText = "Cleared scene layout";
        }

        if (GUILayout.Button("Clear Save"))
        {
            ClearSave();
            statusText = "Cleared saved game";
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        DrawHarvestInventory();

        GUILayout.Space(6f);
        GUILayout.Label(statusText);
    }

    private void EnsurePanelSize()
    {
        float maxWidth = Mathf.Max(220f, Screen.width - 32f);
        float maxHeight = Mathf.Max(260f, Screen.height - 32f);
        float minWidth = Mathf.Min(minimumPanelSize.x, maxWidth);
        float minHeight = Mathf.Min(minimumPanelSize.y, maxHeight);
        panelRect.width = Mathf.Clamp(panelRect.width, minWidth, maxWidth);
        panelRect.height = Mathf.Clamp(panelRect.height, minHeight, maxHeight);
    }

    private void ClampPanelToScreen()
    {
        panelRect.x = Mathf.Clamp(panelRect.x, 0f, Mathf.Max(0f, Screen.width - panelRect.width));
        panelRect.y = Mathf.Clamp(panelRect.y, 0f, Mathf.Max(0f, Screen.height - panelRect.height));
    }

    private void DrawSeedControls()
    {
        GUILayout.Label("Select Seed");

        if (seedInventory == null)
        {
            GUILayout.Label("No SeedInventory found.");
            return;
        }

        if (GUILayout.Button("Reset Test Seeds"))
        {
            seedInventory.ResetToStartingSeeds();
            selectedSeedData = null;
            statusText = "Reset seed inventory";
        }

        foreach (SeedInventoryEntry entry in seedInventory.Seeds)
        {
            if (entry == null || entry.seedData == null)
            {
                continue;
            }

            string label = $"{entry.seedData.DisplayName} x{entry.count}";
            if (GUILayout.Button(label))
            {
                selectedSeedData = entry.seedData;
                statusText = $"Selected {entry.seedData.DisplayName}";
            }
        }

        bool canTryPlant = selectedSeedData != null && layoutManager.SelectedPot != null;
        GUI.enabled = canTryPlant;
        if (GUILayout.Button("Plant Selected Seed"))
        {
            TryPlantSelectedSeed();
        }
        GUI.enabled = true;
    }

    private void TryPlantSelectedSeed()
    {
        if (selectedSeedData == null)
        {
            statusText = "Select a seed first";
            return;
        }

        if (layoutManager.SelectedPot == null)
        {
            statusText = "Select a pot first";
            return;
        }

        if (!seedInventory.HasSeed(selectedSeedData.SeedId))
        {
            statusText = $"No {selectedSeedData.DisplayName} left";
            return;
        }

        string failureReason = layoutManager.GetPlantSeedFailureReason(layoutManager.SelectedPot, selectedSeedData);
        if (!string.IsNullOrEmpty(failureReason))
        {
            statusText = failureReason;
            return;
        }

        if (!seedInventory.TryConsumeSeed(selectedSeedData.SeedId))
        {
            statusText = $"No {selectedSeedData.DisplayName} left";
            return;
        }

        if (layoutManager.PlantSeedInSelectedPot(selectedSeedData, out _))
        {
            statusText = $"Planted {selectedSeedData.DisplayName}";
            return;
        }

        seedInventory.AddSeed(selectedSeedData, 1);
        statusText = "Planting failed";
    }

    private void SaveGame()
    {
        if (gameManager != null)
        {
            gameManager.SaveGame();
            return;
        }

        saveStore.Save(layoutManager, seedInventory, harvestInventory);
    }

    private bool LoadGame()
    {
        if (gameManager != null)
        {
            return gameManager.LoadGame();
        }

        return saveStore.Load(layoutManager, seedInventory, harvestInventory);
    }

    private void ClearSave()
    {
        if (gameManager != null)
        {
            gameManager.ClearSave();
            return;
        }

        saveStore.ClearSave();
    }

    private void TryHarvestSelectedPlant()
    {
        if (!layoutManager.HarvestSelectedPlant(out string itemId, out string displayName))
        {
            statusText = "Selected plant is not mature";
            return;
        }

        harvestInventory.AddItem(itemId, displayName, 1);
        statusText = $"Harvested {displayName}";
    }

    private void DrawHarvestInventory()
    {
        GUILayout.Label("Harvested Items");

        if (harvestInventory == null || harvestInventory.Items.Count == 0)
        {
            GUILayout.Label("None");
            return;
        }

        foreach (HarvestInventoryEntry entry in harvestInventory.Items)
        {
            if (entry == null)
            {
                continue;
            }

            GUILayout.Label($"{entry.displayName} x{entry.count}");
        }
    }

    private string GetSelectedPotDataName()
    {
        PotData selectedPotData = layoutManager.SelectedPotData;
        return selectedPotData != null ? selectedPotData.DisplayName : "None";
    }

    private string GetSelectedPotName()
    {
        PotInstance selectedPot = layoutManager.SelectedPot;
        return selectedPot != null ? $"{selectedPot.potId} @ Slot {selectedPot.startSlotIndex}" : "None";
    }

    private string GetSelectedPotPlantName()
    {
        PotInstance selectedPot = layoutManager.SelectedPot;
        return selectedPot != null && selectedPot.plant != null ? selectedPot.plant.plantId : "None";
    }

    private string GetSelectedPlantStageName()
    {
        return layoutManager != null ? layoutManager.GetSelectedPlantStageText() : "None";
    }

    private string GetSelectedSeedName()
    {
        return selectedSeedData != null ? $"{selectedSeedData.DisplayName} -> {selectedSeedData.PlantId}" : "None";
    }
}
