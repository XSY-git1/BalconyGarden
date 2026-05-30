using UnityEngine;

public class PotView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer tintRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.92f, 0.45f, 1f);
    [SerializeField] private bool autoFindTintRenderer = true;
    [SerializeField] private bool autoCreateClickCollider = true;
    [SerializeField] private Vector2 clickColliderSize = Vector2.one;

    private PotInstance potInstance;
    private PotData potData;
    private BalconyPotLayoutManager layoutManager;
    private PlantView plantView;
    private BoxCollider2D clickCollider;

    public PotInstance PotInstance => potInstance;
    public PotData PotData => potData;
    public string PotInstanceId => potInstance != null ? potInstance.potInstanceId : string.Empty;

    public void Bind(PotInstance instance, PotData data)
    {
        Bind(instance, data, null);
    }

    public void Bind(PotInstance instance, PotData data, BalconyPotLayoutManager manager)
    {
        potInstance = instance;
        potData = data;
        layoutManager = manager;
        gameObject.name = data != null
            ? $"PotView_{data.PotId}_{instance.startSlotIndex}"
            : $"PotView_{instance.potId}_{instance.startSlotIndex}";
        EnsureClickCollider();
    }

    public void SetSelected(bool selected)
    {
        EnsureTintRenderer();

        if (tintRenderer == null)
        {
            return;
        }

        tintRenderer.color = selected ? selectedColor : normalColor;
    }

    public void FitToWorldWidth(float targetWidth)
    {
        if (targetWidth <= 0f)
        {
            return;
        }

        EnsureTintRenderer();

        if (tintRenderer == null || tintRenderer.bounds.size.x <= 0.0001f)
        {
            return;
        }

        float scaleMultiplier = targetWidth / tintRenderer.bounds.size.x;
        Vector3 localScale = transform.localScale;
        localScale.x *= scaleMultiplier;
        transform.localScale = localScale;
    }

    public void ShowPlant(PlantInstance instance, PlantData data)
    {
        ClearPlant();

        if (instance == null || data == null)
        {
            return;
        }

        GameObject plantObject;
        if (data.PlantPrefab != null)
        {
            plantObject = Instantiate(data.PlantPrefab, transform);
        }
        else
        {
            plantObject = new GameObject("PlantView");
            plantObject.transform.SetParent(transform, false);
        }

        plantObject.transform.localPosition = data.ViewOffset;
        plantObject.transform.localRotation = Quaternion.identity;
        plantObject.transform.localScale = Vector3.one;

        plantView = plantObject.GetComponent<PlantView>();
        if (plantView == null)
        {
            plantView = plantObject.AddComponent<PlantView>();
        }

        plantView.Bind(instance, data);
    }

    public void ClearPlant()
    {
        if (plantView == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(plantView.gameObject);
        }
        else
        {
            DestroyImmediate(plantView.gameObject);
        }

        plantView = null;
    }

    private void Reset()
    {
        tintRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnValidate()
    {
        clickColliderSize.x = Mathf.Max(0.1f, clickColliderSize.x);
        clickColliderSize.y = Mathf.Max(0.1f, clickColliderSize.y);
    }

    private void OnMouseDown()
    {
        if (layoutManager != null && potInstance != null)
        {
            layoutManager.SelectPot(potInstance);
        }
    }

    private void EnsureTintRenderer()
    {
        if (tintRenderer == null && autoFindTintRenderer)
        {
            tintRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void EnsureClickCollider()
    {
        if (!autoCreateClickCollider)
        {
            return;
        }

        if (clickCollider == null)
        {
            clickCollider = GetComponent<BoxCollider2D>();
        }

        if (clickCollider == null)
        {
            clickCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        clickCollider.isTrigger = false;
        clickCollider.size = clickColliderSize;
        clickCollider.offset = Vector2.zero;
    }
}
