using UnityEngine;

public class PotLayoutLocalSaveStore : MonoBehaviour
{
    [SerializeField] private string playerPrefsKey = "balconygarden.potLayout";

    public bool HasSave => PlayerPrefs.HasKey(playerPrefsKey);

    public void Save(BalconyPotLayoutManager layoutManager)
    {
        if (layoutManager == null)
        {
            return;
        }

        PlayerPrefs.SetString(playerPrefsKey, layoutManager.CaptureSaveJson(false));
        PlayerPrefs.Save();
    }

    public bool Load(BalconyPotLayoutManager layoutManager)
    {
        if (layoutManager == null || !HasSave)
        {
            return false;
        }

        layoutManager.LoadPotLayoutJson(PlayerPrefs.GetString(playerPrefsKey));
        return true;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }
}
