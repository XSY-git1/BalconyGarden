using UnityEngine;

public class PotView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer tintRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.92f, 0.45f, 1f);
    [SerializeField] private bool autoFindTintRenderer = true;

    private PotInstance potInstance;
    private PotData potData;
    private PlantView plantView;

    public PotInstance PotInstance => potInstance;
    public PotData PotData => potData;
    public string PotInstanceId => potInstance != null ? potInstance.potInstanceId : string.Empty;

    public void Bind(PotInstance instance, PotData data)
    {
        potInstance = instance;
        potData = data;
        gameObject.name = data != null
            ? $"PotView_{data.PotId}_{instance.startSlotIndex}"
            : $"PotView_{instance.potId}_{instance.startSlotIndex}";
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

    private void EnsureTintRenderer()
    {
        if (tintRenderer == null && autoFindTintRenderer)
        {
            tintRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}
