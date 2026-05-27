using UnityEngine;

public class PotSlot : MonoBehaviour
{
    private const string DebugVisualName = "SlotDebugVisual";

    [SerializeField, Min(0)] private int slotIndex;
    [SerializeField] private BalconyPotLayoutManager layoutManager;
    [SerializeField] private SpriteRenderer debugRenderer;
    [SerializeField] private bool autoCreateDebugRenderer = true;
    [SerializeField] private bool useManagedDebugVisual = true;
    [SerializeField] private bool hideRootSpriteWhenUsingManagedVisual = true;
    [SerializeField] private Vector2 debugVisualWorldSize = new Vector2(1.25f, 0.45f);
    [SerializeField] private int debugSortingOrder = -10;
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.8f, 0.7f, 0.2f);
    [SerializeField] private Color occupiedColor = new Color(0.9f, 0.55f, 0.25f, 0.24f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.2f, 0.36f);

    private static Sprite fallbackSprite;

    public int SlotIndex => slotIndex;
    public BalconyPotLayoutManager LayoutManager => layoutManager;

    private void Awake()
    {
        EnsureDebugRenderer();
    }

    public void Bind(BalconyPotLayoutManager manager, int index)
    {
        layoutManager = manager;
        slotIndex = Mathf.Max(0, index);
        EnsureDebugRenderer();
    }

    public void SetDebugState(bool occupied, bool selected)
    {
        EnsureDebugRenderer();

        if (debugRenderer == null)
        {
            return;
        }

        debugRenderer.color = selected ? selectedColor : occupied ? occupiedColor : emptyColor;
    }

    private void OnMouseDown()
    {
        if (layoutManager != null)
        {
            layoutManager.HandleSlotClicked(this);
        }
    }

    private void OnValidate()
    {
        slotIndex = Mathf.Max(0, slotIndex);
        debugVisualWorldSize.x = Mathf.Max(0.1f, debugVisualWorldSize.x);
        debugVisualWorldSize.y = Mathf.Max(0.1f, debugVisualWorldSize.y);
    }

    private void Reset()
    {
        debugRenderer = GetComponent<SpriteRenderer>();
    }

    private void EnsureDebugRenderer()
    {
        if (useManagedDebugVisual)
        {
            debugRenderer = GetOrCreateManagedDebugRenderer();
            HideRootSpriteIfNeeded();
        }
        else if (debugRenderer == null)
        {
            debugRenderer = GetComponent<SpriteRenderer>();
        }

        if (debugRenderer == null && autoCreateDebugRenderer)
        {
            debugRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        ApplyDebugRendererDefaults();
    }

    private SpriteRenderer GetOrCreateManagedDebugRenderer()
    {
        Transform visualTransform = transform.Find(DebugVisualName);
        if (visualTransform == null && autoCreateDebugRenderer)
        {
            GameObject visualObject = new GameObject(DebugVisualName);
            visualTransform = visualObject.transform;
            visualTransform.SetParent(transform, false);
        }

        if (visualTransform == null)
        {
            return null;
        }

        SpriteRenderer renderer = visualTransform.GetComponent<SpriteRenderer>();
        if (renderer == null && autoCreateDebugRenderer)
        {
            renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = GetDebugVisualLocalScale();
        return renderer;
    }

    private Vector3 GetDebugVisualLocalScale()
    {
        Vector3 parentScale = transform.lossyScale;
        float scaleX = Mathf.Abs(parentScale.x) > 0.0001f ? debugVisualWorldSize.x / Mathf.Abs(parentScale.x) : debugVisualWorldSize.x;
        float scaleY = Mathf.Abs(parentScale.y) > 0.0001f ? debugVisualWorldSize.y / Mathf.Abs(parentScale.y) : debugVisualWorldSize.y;
        return new Vector3(scaleX, scaleY, 1f);
    }

    private void HideRootSpriteIfNeeded()
    {
        if (!hideRootSpriteWhenUsingManagedVisual)
        {
            return;
        }

        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null && rootRenderer != debugRenderer)
        {
            rootRenderer.enabled = false;
        }
    }

    private void ApplyDebugRendererDefaults()
    {
        if (debugRenderer == null)
        {
            return;
        }

        if (debugRenderer.sprite == null)
        {
            debugRenderer.sprite = GetFallbackSprite();
        }

        debugRenderer.sortingOrder = debugSortingOrder;
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
        fallbackSprite.name = "RuntimeSlotDebugSquare";
        return fallbackSprite;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }
}
