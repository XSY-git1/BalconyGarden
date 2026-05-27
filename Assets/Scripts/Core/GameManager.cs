using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BalconyPotLayoutManager layoutManager;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private HarvestInventory harvestInventory;
    [SerializeField] private GameLocalSaveStore saveStore;
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool saveOnApplicationPause = true;
    [SerializeField] private bool saveOnApplicationQuit = true;

    public bool HasSave => saveStore != null && saveStore.HasSave;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        bool loadedSave = false;

        if (autoLoadOnStart)
        {
            loadedSave = LoadGame();
        }

        if (!loadedSave && seedInventory != null)
        {
            seedInventory.EnsureStartingSeedsIfEmpty();
        }
    }

    public void SaveGame()
    {
        ResolveReferences();

        if (saveStore == null || layoutManager == null)
        {
            return;
        }

        saveStore.Save(layoutManager, seedInventory, harvestInventory);
    }

    public bool LoadGame()
    {
        ResolveReferences();

        if (saveStore == null || layoutManager == null)
        {
            return false;
        }

        return saveStore.Load(layoutManager, seedInventory, harvestInventory);
    }

    public void ClearSave()
    {
        ResolveReferences();

        if (saveStore != null)
        {
            saveStore.ClearSave();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && saveOnApplicationPause)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
        {
            SaveGame();
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

        if (saveStore == null)
        {
            saveStore = GetComponent<GameLocalSaveStore>();
        }

        if (saveStore == null)
        {
            saveStore = gameObject.AddComponent<GameLocalSaveStore>();
        }
    }
}
