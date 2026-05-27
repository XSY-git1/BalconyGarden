using UnityEngine;

public class PlantView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static Sprite fallbackSprite;
    private PlantInstance plantInstance;
    private PlantData plantData;

    public PlantInstance PlantInstance => plantInstance;
    public PlantData PlantData => plantData;

    public void Bind(PlantInstance instance, PlantData data)
    {
        plantInstance = instance;
        plantData = data;
        gameObject.name = data != null ? $"PlantView_{data.PlantId}" : $"PlantView_{instance.plantId}";
        RefreshStage(instance.currentStage);
    }

    public void RefreshStage(PlantGrowthStage stage)
    {
        EnsurePlaceholderRenderer(stage);
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void EnsurePlaceholderRenderer(PlantGrowthStage stage)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetFallbackSprite();
        }

        if (plantData == null)
        {
            return;
        }

        spriteRenderer.color = plantData.GetStageColor(stage);
        spriteRenderer.sortingOrder = plantData.SortingOrder;
        FitToWorldSize(plantData.GetStageWorldSize(stage));
    }

    private void FitToWorldSize(Vector2 targetSize)
    {
        if (spriteRenderer == null || spriteRenderer.bounds.size.x <= 0.0001f || spriteRenderer.bounds.size.y <= 0.0001f)
        {
            return;
        }

        transform.localScale = Vector3.one;
        Vector3 localScale = transform.localScale;
        localScale.x *= targetSize.x / spriteRenderer.bounds.size.x;
        localScale.y *= targetSize.y / spriteRenderer.bounds.size.y;
        transform.localScale = localScale;
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        Rect rect = new Rect(0f, 0f, texture.width, texture.height);
        fallbackSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), texture.width);
        fallbackSprite.name = "RuntimePlantPlaceholderSquare";
        return fallbackSprite;
    }
}
