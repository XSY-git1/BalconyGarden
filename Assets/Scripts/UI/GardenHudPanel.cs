using UnityEngine;

public class GardenHudPanel : MonoBehaviour
{
    [SerializeField] private BalconyPotLayoutManager layoutManager;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private HarvestInventory harvestInventory;
    [SerializeField] private bool showPanel = true;
    [SerializeField] private Rect panelRect = new Rect(320f, 16f, 300f, 420f);
    [SerializeField] private Vector2 minimumPanelSize = new Vector2(300f, 420f);

    private string statusText = "Ready";
    private SeedData selectedSeedData;
    private Vector2 scrollPosition;

    private void Awake()
    {
        ResolveReferences();
        EnsurePanelSize();
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

        ResolveReferences();
        EnsurePanelSize();
        panelRect = GUI.Window(GetInstanceID(), panelRect, DrawPanel, "Garden");
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
            GUILayout.Label("No balcony layout found.");
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
            return;
        }

        DrawSummary();
        DrawPotSelection();
        DrawPlacedPotSelection();
        DrawSeedSelection();
        DrawActions();
        DrawHarvestInventory();

        GUILayout.Space(6f);
        GUILayout.Label(statusText);

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
    }

    private void DrawSummary()
    {
        GUILayout.Label($"Selected Pot Type: {GetSelectedPotDataName()}");
        GUILayout.Label($"Selected Pot: {GetSelectedPotName()}");
        GUILayout.Label($"Plant: {GetSelectedPotPlantName()}");
        GUILayout.Label($"Growth: {layoutManager.GetSelectedPlantStageText()}");
        GUILayout.Label($"Selected Seed: {GetSelectedSeedName()}");
    }

    private void DrawPotSelection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Pots");

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
    }

    private void DrawPlacedPotSelection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Placed Pots");

        if (layoutManager.PlacedPots.Count == 0)
        {
            GUILayout.Label("None");
            return;
        }

        foreach (PotInstance pot in layoutManager.PlacedPots)
        {
            if (pot == null)
            {
                continue;
            }

            string plantText = pot.plant != null ? $"{pot.plant.plantId} ({pot.plant.currentStage})" : "Empty";
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Slot {pot.startSlotIndex}: {pot.potId} - {plantText}"))
            {
                layoutManager.SelectPot(pot);
                statusText = $"Selected pot @ Slot {pot.startSlotIndex}";
            }

            GUI.enabled = selectedSeedData != null && pot.plant == null;
            if (GUILayout.Button("Plant Here", GUILayout.Width(90f)))
            {
                TryPlantSeedInPot(pot);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }

    private void DrawSeedSelection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Seeds");

        if (seedInventory == null)
        {
            GUILayout.Label("No seed inventory found.");
            return;
        }

        foreach (SeedInventoryEntry entry in seedInventory.Seeds)
        {
            if (entry == null || entry.seedData == null)
            {
                continue;
            }

            GUI.enabled = entry.count > 0;
            if (GUILayout.Button($"{entry.seedData.DisplayName} x{entry.count}"))
            {
                selectedSeedData = entry.seedData;
                statusText = $"Selected {entry.seedData.DisplayName}";
            }
            GUI.enabled = true;
        }
    }

    private void DrawActions()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Actions");

        GUI.enabled = selectedSeedData != null && layoutManager.SelectedPot != null;
        if (GUILayout.Button("Plant Seed"))
        {
            TryPlantSelectedSeed();
        }
        GUI.enabled = true;

        GUI.enabled = layoutManager.CanHarvestSelectedPlant();
        if (GUILayout.Button("Harvest"))
        {
            TryHarvestSelectedPlant();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Refresh Growth"))
        {
            layoutManager.RefreshAllPlantGrowth();
            statusText = "Growth refreshed";
        }

        GUI.enabled = layoutManager.SelectedPot != null;
        if (GUILayout.Button("Remove Selected Pot"))
        {
            statusText = layoutManager.RemoveSelectedPot() ? "Removed selected pot" : "No selected pot to remove";
        }
        GUI.enabled = true;
    }

    private void TryPlantSelectedSeed()
    {
        if (seedInventory == null)
        {
            statusText = "No seed inventory found";
            return;
        }

        if (selectedSeedData == null)
        {
            statusText = "Select a seed first";
            return;
        }

        if (!seedInventory.HasSeed(selectedSeedData.SeedId))
        {
            statusText = $"No {selectedSeedData.DisplayName} left";
            return;
        }

        if (layoutManager.SelectedPot == null)
        {
            statusText = "Select a pot first";
            return;
        }

        TryPlantSeedInPot(layoutManager.SelectedPot);
    }

    private void TryPlantSeedInPot(PotInstance targetPot)
    {
        if (seedInventory == null)
        {
            statusText = "No seed inventory found";
            return;
        }

        if (selectedSeedData == null)
        {
            statusText = "Select a seed first";
            return;
        }

        if (targetPot == null)
        {
            statusText = "Select a pot first";
            return;
        }

        if (!seedInventory.HasSeed(selectedSeedData.SeedId))
        {
            statusText = $"No {selectedSeedData.DisplayName} left";
            return;
        }

        string failureReason = layoutManager.GetPlantSeedFailureReason(targetPot, selectedSeedData);
        if (!string.IsNullOrEmpty(failureReason))
        {
            layoutManager.SelectPot(targetPot);
            statusText = failureReason;
            return;
        }

        if (!seedInventory.TryConsumeSeed(selectedSeedData.SeedId))
        {
            statusText = $"No {selectedSeedData.DisplayName} left";
            return;
        }

        if (layoutManager.PlantSeed(targetPot, selectedSeedData, out _))
        {
            layoutManager.SelectPot(targetPot);
            statusText = $"Planted {selectedSeedData.DisplayName} @ Slot {targetPot.startSlotIndex}";
            return;
        }

        seedInventory.AddSeed(selectedSeedData, 1);
        statusText = "Planting failed";
    }

    private void TryHarvestSelectedPlant()
    {
        if (harvestInventory == null)
        {
            ResolveReferences();
        }

        if (!layoutManager.HarvestSelectedPlant(out string itemId, out string displayName))
        {
            statusText = "Selected plant is not mature";
            return;
        }

        if (harvestInventory != null)
        {
            harvestInventory.AddItem(itemId, displayName, 1);
        }

        statusText = $"Harvested {displayName}";
    }

    private void DrawHarvestInventory()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Harvested");

        if (harvestInventory == null || harvestInventory.Items.Count == 0)
        {
            GUILayout.Label("None");
            return;
        }

        foreach (HarvestInventoryEntry entry in harvestInventory.Items)
        {
            if (entry != null)
            {
                GUILayout.Label($"{entry.displayName} x{entry.count}");
            }
        }
    }

    private void ResolveReferences()
    {
        if (layoutManager == null)
        {
            layoutManager = FindObjectOfType<BalconyPotLayoutManager>();
        }

        if (seedInventory == null)
        {
            seedInventory = FindObjectOfType<SeedInventory>();
        }

        if (harvestInventory == null)
        {
            harvestInventory = FindObjectOfType<HarvestInventory>();
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

    private string GetSelectedSeedName()
    {
        return selectedSeedData != null ? selectedSeedData.DisplayName : "None";
    }
}
