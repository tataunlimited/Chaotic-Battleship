using Core.Ship;
using TMPro;
using UnityEngine;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ShipType ship = ShipType.Cruiser;
    [SerializeField] private UpgradeType upgradeType = UpgradeType.Armor;

    [Header("Rules")]
    [SerializeField] private int delta = 1;
    [SerializeField] private int maxLevel = 5;

    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI levelLabel;

    private void OnEnable()
    {
        if (PlayerData.Instance == null) return;
        PlayerData.Instance.EnsureUpgradeDefaults();
        RefreshLabel();
    }

    // Hook to a Button OnClick
    public void Buy()
    {
        var pd = PlayerData.Instance;
        if (pd == null) return;

        int current = pd.GetUpgrade(ship, upgradeType);
        int next = Mathf.Clamp(current + delta, 0, maxLevel);
        if (next == current) return;

        pd.SetUpgrade(ship, upgradeType, next);
        SaveManager.SaveGame();
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (levelLabel == null || PlayerData.Instance == null) return;
        int level = PlayerData.Instance.GetUpgrade(ship, upgradeType);
        levelLabel.text = $"{ship} {upgradeType}: {level}";
    }
}
