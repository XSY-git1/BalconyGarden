using UnityEngine;

[CreateAssetMenu(fileName = "PotData", menuName = "BalconyGarden/Pot Data")]
public class PotData : ScriptableObject
{
    [SerializeField] private string potId = "pot_id";
    [SerializeField] private string displayName = "Pot";
    [SerializeField, Min(1)] private int slotSize = 1;
    [SerializeField] private GameObject potPrefab = null;
    [SerializeField] private Vector2 viewOffset = Vector2.zero;

    public string PotId => potId;
    public string DisplayName => displayName;
    public int SlotSize => Mathf.Max(1, slotSize);
    public GameObject PotPrefab => potPrefab;
    public Vector2 ViewOffset => viewOffset;

    private void OnValidate()
    {
        slotSize = Mathf.Max(1, slotSize);
    }
}
