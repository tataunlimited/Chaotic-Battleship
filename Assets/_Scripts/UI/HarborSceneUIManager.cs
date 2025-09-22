using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HarborSceneUIManager : MonoBehaviour
{
    private PlayerData playerData;
    private int current_points;

    public enum ShipType { Submarine, Destroyer, Cruiser, Battleship }
    [Header("Harbor Testing - Selected Ship")]
    [SerializeField] private ShipType selectedShip = ShipType.Submarine;

    [Header("Armor Panel UI")]
    [SerializeField] private GameObject armorPanelRoot;
    [SerializeField] private TMP_Text currentLevelLabel;
    [SerializeField] private TMP_Text currentStatsLabel;
    [SerializeField] private TMP_Text nextNameLabel;
    [SerializeField] private TMP_Text nextDescLabel;
    [SerializeField] private TMP_Text nextCostLabel;
    [SerializeField] private TMP_Text nextGainsLabel;
    [SerializeField] private Button upgradeButton;

    [Header("Optional hooks")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip sfxUpgradeSuccess;

    private const string ARMOR_SUB_KEY = "armor_submarine";
    private const string ARMOR_DES_KEY = "armor_destroyer";
    private const string ARMOR_CRU_KEY = "armor_cruiser";
    private const string ARMOR_BAT_KEY = "armor_battleship";

    private int GetArmorTier(ShipType ship)
    {
        string key = ship switch
        {
            ShipType.Submarine => ARMOR_SUB_KEY,
            ShipType.Destroyer => ARMOR_DES_KEY,
            ShipType.Cruiser => ARMOR_CRU_KEY,
            ShipType.Battleship => ARMOR_BAT_KEY,
            _ => ARMOR_SUB_KEY
        };
        return PlayerPrefs.GetInt(key, 0);
    }

    private void SetArmorTier(ShipType ship, int tier)
    {
        string key = ship switch
        {
            ShipType.Submarine => ARMOR_SUB_KEY,
            ShipType.Destroyer => ARMOR_DES_KEY,
            ShipType.Cruiser => ARMOR_CRU_KEY,
            ShipType.Battleship => ARMOR_BAT_KEY,
            _ => ARMOR_SUB_KEY
        };
        PlayerPrefs.SetInt(key, Mathf.Clamp(tier, 0, 3));
        PlayerPrefs.Save();
    }

    #region upgrade prices
    private int submarine_armor_level1_price = 400;
    private int submarine_armor_level2_price = 800;
    private int submarine_armor_level3_price = 1600;

    private int destroyer_armor_level1_price = 1100;
    private int destroyer_armor_level2_price = 1600;
    private int destroyer_armor_level3_price = 2100;

    private int cruiser_armor_level1_price = 1200;
    private int cruiser_armor_level2_price = 1700;
    private int cruiser_armor_level3_price = 2200;

    private int battleship_armor_level1_price = 1300;
    private int battleship_armor_level2_price = 1800;
    private int battleship_armor_level3_price = 2300;
    #endregion


#if UNITY_EDITOR
    private void Reset() { AutoBind(); }
    private void OnValidate() { AutoBind(); }

    private void AutoBind()
    {
        // Only fill missing fields; keeps manual assignments intact
        if (armorPanelRoot == null) armorPanelRoot = transform.Find("GROUP_UpgradeInfo")?.gameObject;
        if (currentLevelLabel == null) currentLevelLabel = transform.Find("GROUP_UpgradeInfo/TXT_CurrentLevel")?.GetComponent<TMPro.TMP_Text>();
        if (currentStatsLabel == null) currentStatsLabel = transform.Find("GROUP_UpgradeInfo/TXT_CurrentStats")?.GetComponent<TMPro.TMP_Text>();
        if (nextNameLabel == null) nextNameLabel = transform.Find("GROUP_UpgradeInfo/TXT_NextName")?.GetComponent<TMPro.TMP_Text>();
        if (nextDescLabel == null) nextDescLabel = transform.Find("GROUP_UpgradeInfo/LABEL_UpgradeDescription/TXT_NextDescription")?.GetComponent<TMPro.TMP_Text>();
        if (nextCostLabel == null) nextCostLabel = transform.Find("GROUP_UpgradeInfo/TXT_NextCost")?.GetComponent<TMPro.TMP_Text>();
        if (nextGainsLabel == null) nextGainsLabel = transform.Find("GROUP_UpgradeInfo/TXT_NextGains")?.GetComponent<TMPro.TMP_Text>();
        if (upgradeButton == null) upgradeButton = transform.Find("GROUP_UpgradeInfo/BUTTON_Upgrade")?.GetComponent<UnityEngine.UI.Button>();
    }
#endif
    
    private void Start()
    {
        playerData = PlayerData.Instance;
        current_points = playerData != null ? playerData.currentScore : 0;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        RefreshArmorPanel();
    }

    public void SetSelectedShipType(ShipType ship)
    {
        selectedShip = ship;
        RefreshArmorPanel();
    }

    public void ShowArmorTab()
    {
        if (armorPanelRoot != null) armorPanelRoot.SetActive(true);
        RefreshArmorPanel();
    }

    private void RefreshArmorPanel()
    {
        if (armorPanelRoot == null) return;

        int currentTier = GetArmorTier(selectedShip);
        int nextTier;
        int nextCost;
        bool hasNext = TryGetNextArmorCost(selectedShip, out nextTier, out nextCost);

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"Level {currentTier}";

        if (currentStatsLabel != null)
            currentStatsLabel.text = GetCurrentStatsText(selectedShip, currentTier);

        if (hasNext)
        {
            if (nextNameLabel != null) nextNameLabel.text = GetTierDisplayName(selectedShip, nextTier);
            if (nextDescLabel != null) nextDescLabel.text = GetTierDescription(selectedShip, nextTier);
            if (nextCostLabel != null) nextCostLabel.text = $"Cost: {nextCost}";
            if (nextGainsLabel != null) nextGainsLabel.text = GetTierGainsText(selectedShip, nextTier);

            bool canAfford = (playerData != null) && (playerData.currentScore >= nextCost);
            SetUpgradeButtonState(canAfford, true);
        }
        else
        {
            if (nextNameLabel != null) nextNameLabel.text = "MAXED";
            if (nextDescLabel != null) nextDescLabel.text = "Armor path complete.";
            if (nextCostLabel != null) nextCostLabel.text = "—";
            if (nextGainsLabel != null) nextGainsLabel.text = "—";
            SetUpgradeButtonState(false, false);
        }
    }

    private void SetUpgradeButtonState(bool canAfford, bool interactable)
    {
        if (upgradeButton == null) return;
        upgradeButton.interactable = canAfford && interactable;
    }

    #region button events
    public void SubmarineArmorUpgradePressed()
    {
        selectedShip = ShipType.Submarine;
        ShowArmorTab();
    }
    public void DestroyerArmorUpgradePressed()
    {
        selectedShip = ShipType.Destroyer;
        ShowArmorTab();
    }
    public void CruiserArmorUpgradePressed()
    {
        selectedShip = ShipType.Cruiser;
        ShowArmorTab();
    }
    public void BattleshipArmorUpgradePressed()
    {
        selectedShip = ShipType.Battleship;
        ShowArmorTab();
    }
    #endregion

    public void ToBattle()
    {
        SceneTypes.SceneType nextScene = SceneTypes.SceneType.Game;
        SceneManager.Instance.LoadScene(nextScene);
    }

    private void OnUpgradeClicked()
    {
        if (!TryGetNextArmorCost(selectedShip, out int nextTier, out int cost))
        {
            ShowToast($"{selectedShip} Armor is MAXED");
            return;
        }

        if (playerData == null) return;

        if (playerData.currentScore < cost)
        {
            ShowToast($"Need {cost} points (have {playerData.currentScore})");
            RefreshArmorPanel();
            return;
        }

        string title = "Confirm Upgrade";
        string body = $"Upgrade {selectedShip} Armor to Tier {nextTier} for {cost} points?";
        ShowConfirm(title, body, () =>
        {
            playerData.currentScore = Mathf.Max(0, playerData.currentScore - cost);
            SetArmorTier(selectedShip, nextTier);
            SaveIfPossible();
            PlaySuccess();
            ShowToast($"{selectedShip} Armor → Tier {nextTier}!");
            RefreshArmorPanel();
        });
    }

    private bool TryGetNextArmorCost(ShipType ship, out int nextTier, out int cost)
    {
        int cur = GetArmorTier(ship);
        nextTier = cur + 1;
        cost = 0;
        if (nextTier > 3) return false;

        switch (ship)
        {
            case ShipType.Submarine:
                cost = nextTier == 1 ? submarine_armor_level1_price :
                       nextTier == 2 ? submarine_armor_level2_price :
                       submarine_armor_level3_price;
                return true;
            case ShipType.Destroyer:
                cost = nextTier == 1 ? destroyer_armor_level1_price :
                       nextTier == 2 ? destroyer_armor_level2_price :
                       destroyer_armor_level3_price;
                return true;
            case ShipType.Cruiser:
                cost = nextTier == 1 ? cruiser_armor_level1_price :
                       nextTier == 2 ? cruiser_armor_level2_price :
                       cruiser_armor_level3_price;
                return true;
            case ShipType.Battleship:
                cost = nextTier == 1 ? battleship_armor_level1_price :
                       nextTier == 2 ? battleship_armor_level2_price :
                       battleship_armor_level3_price;
                return true;
        }
        return false;
    }

    private string GetCurrentStatsText(ShipType ship, int tier)
    {
        int baseHP = ship switch
        {
            ShipType.Submarine => 100,
            ShipType.Destroyer => 120,
            ShipType.Cruiser => 140,
            ShipType.Battleship => 160,
            _ => 120
        };
        int hpGainPerTier = 20;

        int baseDR = ship switch
        {
            ShipType.Submarine => 5,
            ShipType.Destroyer => 8,
            ShipType.Cruiser => 10,
            ShipType.Battleship => 12,
            _ => 8
        };
        int drGainPerTier = 5;

        int hp = baseHP + tier * hpGainPerTier;
        int dr = baseDR + tier * drGainPerTier;

        return $"HP {hp}  |  DR {dr}%";
    }

    private string GetTierDisplayName(ShipType ship, int tier)
    {
        return $"{ship} Armor Tier {tier}";
    }

    private string GetTierDescription(ShipType ship, int tier)
    {
        return $"Reinforced plating level {tier} improves survivability.";
    }

    private string GetTierGainsText(ShipType ship, int tier)
    {
        int hpGainPerTier = 20;
        int drGainPerTier = 5;
        return $"+{hpGainPerTier} HP  |  +{drGainPerTier}% DR";
    }

    private void ShowConfirm(string title, string body, Action onConfirm)
    {
        try
        {
            var modalType = Type.GetType("SimpleModal");
            if (modalType != null)
            {
                MethodInfo mi = modalType.GetMethod("Show",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(string), typeof(Action) },
                    null);

                if (mi != null)
                {
                    mi.Invoke(null, new object[] { title, body, onConfirm });
                    return;
                }
            }
        }
        catch { }

        onConfirm?.Invoke();
    }

    private void ShowToast(string msg)
    {
        Debug.Log("[HarborUI] " + msg);
    }

    private void SaveIfPossible()
    {
        try
        {
            var saveType = Type.GetType("SaveManager");
            var mi = saveType?.GetMethod("SaveGame", BindingFlags.Public | BindingFlags.Static);
            mi?.Invoke(null, null);
        }
        catch { }
    }

    private void PlaySuccess()
    {
        if (uiAudioSource != null && sfxUpgradeSuccess != null)
            uiAudioSource.PlayOneShot(sfxUpgradeSuccess);
    }
}
