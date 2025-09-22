using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarborSceneUIManager : MonoBehaviour
{
    private PlayerData playerData;

    public enum ShipType { None, Submarine, Destroyer, Cruiser, Battleship }
    public enum UpgradeCategory { Armor, Movement, Attack, SpecialAbility }

    // ---- Data (assign your ScriptableObjects here) ---------------------------------
    [Header("Data (drag ScriptableObjects from Assets/_ScriptableObjects)")]
    [SerializeField] private ScriptableObject armorData;
    [SerializeField] private ScriptableObject movementData;
    [SerializeField] private ScriptableObject attackData;
    [SerializeField] private ScriptableObject specialData;

    // ---- Selection ----------------------------------------------------------------
    [Header("Selection (no auto-select on start)")]
    [SerializeField] private ShipType selectedShip = ShipType.None;
    [SerializeField] private UpgradeCategory selectedCategory = UpgradeCategory.Armor;

    // ---- Minimal Armor/Upgrade Panel UI -------------------------------------------
    [Header("Upgrade Panel UI")]
    [SerializeField] private GameObject panelRoot;           // GROUP_UpgradeInfo
    [SerializeField] private TMP_Text nextNameLabel;         // TXT_NextName
    [SerializeField] private TMP_Text nextDescLabel;         // LABEL_UpgradeDescription/TXT_NextDescription
    [SerializeField] private TMP_Text nextCostLabel;         // TXT_NextCost
    [SerializeField] private TMP_Text nextGainsLabel;        // TXT_NextGains (optional)
    [SerializeField] private Button upgradeButton;           // BUTTON_Upgrade

    // ---- Options ------------------------------------------------------------------
    [Header("Options")]
    [SerializeField] private bool hidePanelOnStart = true;
    [SerializeField] private bool autoWireLeftButtons = true;   // wire 16 left buttons at runtime

    // ---- Optional Audio ------------------------------------------------------------
    [Header("Optional Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip sfxUpgradeSuccess;

    // ---- PlayerPrefs keys ----------------------------------------------------------
    private string TierKey(UpgradeCategory cat, ShipType ship)
    {
        // keys like: armor_submarine, movement_destroyer, attack_cruiser, special_battleship
        string c = cat switch
        {
            UpgradeCategory.Armor => "armor",
            UpgradeCategory.Movement => "movement",
            UpgradeCategory.Attack => "attack",
            _ => "special"
        };
        return $"{c}_{ship.ToString().ToLower()}";
    }

    private int GetTier(UpgradeCategory cat, ShipType ship) =>
        PlayerPrefs.GetInt(TierKey(cat, ship), 0);

    private void SetTier(UpgradeCategory cat, ShipType ship, int tier)
    {
        PlayerPrefs.SetInt(TierKey(cat, ship), Mathf.Clamp(tier, 0, 3));
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    private void Reset() { EditorAutobind(); }
    private void OnValidate() { EditorAutobind(); }

    private void EditorAutobind()
    {
        // Bind UI by common names so the Inspector isn’t full of “Missing”
        TryFindUIRefs(transform);
    }
#endif

    private void Awake()
    {
        TryFindUIRefs(transform); // runtime safety
    }

    private void Start()
    {
        playerData = PlayerData.Instance;

        if (panelRoot) panelRoot.SetActive(!hidePanelOnStart);

        if (upgradeButton)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (autoWireLeftButtons) RuntimeWireLeftButtons();

        // Do not auto-select; if panel is shown and something is preselected, refresh once
        if (!hidePanelOnStart && selectedShip != ShipType.None) RefreshPanel();
    }

    // ===== Public wrappers (keep compatibility if any were wired manually) ==========
    public void SubmarineArmorUpgradePressed()       => Select(ShipType.Submarine,  UpgradeCategory.Armor);
    public void DestroyerArmorUpgradePressed()       => Select(ShipType.Destroyer,  UpgradeCategory.Armor);
    public void CruiserArmorUpgradePressed()         => Select(ShipType.Cruiser,    UpgradeCategory.Armor);
    public void BattleshipArmorUpgradePressed()      => Select(ShipType.Battleship, UpgradeCategory.Armor);

    public void SubmarineMovementUpgradePressed()    => Select(ShipType.Submarine,  UpgradeCategory.Movement);
    public void DestroyerMovementUpgradePressed()    => Select(ShipType.Destroyer,  UpgradeCategory.Movement);
    public void CruiserMovementUpgradePressed()      => Select(ShipType.Cruiser,    UpgradeCategory.Movement);
    public void BattleshipMovementUpgradePressed()   => Select(ShipType.Battleship, UpgradeCategory.Movement);

    public void SubmarineAttackButtonPressed()       => Select(ShipType.Submarine,  UpgradeCategory.Attack);
    public void DestroyerAttackButtonPressed()       => Select(ShipType.Destroyer,  UpgradeCategory.Attack);
    public void CruiserAttackButtonPressed()         => Select(ShipType.Cruiser,    UpgradeCategory.Attack);
    public void BattleshipAttackButtonPressed()      => Select(ShipType.Battleship, UpgradeCategory.Attack);

    public void SubmarineSpecialAbilityButtonPressed()=> Select(ShipType.Submarine,  UpgradeCategory.SpecialAbility);
    public void DestroyerSpecialAbilityButtonPressed()=> Select(ShipType.Destroyer,  UpgradeCategory.SpecialAbility);
    public void CruiserSpecialAbilityButtonPressed() => Select(ShipType.Cruiser,    UpgradeCategory.SpecialAbility);
    public void BattleshipSpecialAbilityButtonPressed()=>Select(ShipType.Battleship, UpgradeCategory.SpecialAbility);

    // ===== Core selection & refresh =================================================
    private void Select(ShipType ship, UpgradeCategory cat)
    {
        selectedShip = ship;
        selectedCategory = cat;

        if (panelRoot) panelRoot.SetActive(true);
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        if (panelRoot == null || selectedShip == ShipType.None) return;

        int cur = GetTier(selectedCategory, selectedShip);
        int next = cur + 1;

        if (TryGetUpgradeFromSO(GetDataFor(selectedCategory), selectedShip, next, out var u))
        {
            if (nextNameLabel)  nextNameLabel.text  = u.Name;
            if (nextDescLabel)  nextDescLabel.text  = u.Description;
            if (nextCostLabel)  nextCostLabel.text  = $"Cost: {u.Cost}";
            if (nextGainsLabel) nextGainsLabel.text = u.Gains;

            bool canAfford = (playerData != null) && (playerData.currentScore >= u.Cost);
            SetUpgradeButtonState(canAfford, true);
        }
        else
        {
            if (nextNameLabel)  nextNameLabel.text  = "MAXED";
            if (nextDescLabel)  nextDescLabel.text  = $"{selectedCategory} path complete.";
            if (nextCostLabel)  nextCostLabel.text  = "—";
            if (nextGainsLabel) nextGainsLabel.text = "—";
            SetUpgradeButtonState(false, false);
        }
    }

    private void SetUpgradeButtonState(bool canAfford, bool interactable)
    {
        if (upgradeButton) upgradeButton.interactable = canAfford && interactable;
    }

    private void OnUpgradeClicked()
    {
        if (selectedShip == ShipType.None) return;

        int cur = GetTier(selectedCategory, selectedShip);
        int next = cur + 1;

        if (!TryGetUpgradeFromSO(GetDataFor(selectedCategory), selectedShip, next, out var u))
        {
            Log("[HarborUI] Already maxed.");
            return;
        }

        if (playerData == null) return;

        if (playerData.currentScore < u.Cost)
        {
            Log($"[HarborUI] Need {u.Cost} points (have {playerData.currentScore}).");
            RefreshPanel();
            return;
        }

        string title = "Confirm Upgrade";
        string body  = $"Upgrade {selectedShip} {selectedCategory} to {u.Name} for {u.Cost} points?";
        ShowConfirm(title, body, () =>
        {
            playerData.currentScore = Mathf.Max(0, playerData.currentScore - u.Cost);
            SetTier(selectedCategory, selectedShip, next);

            SaveIfPossible();
            PlaySuccess();
            Log($"[HarborUI] {selectedShip} {selectedCategory} → {u.Name}");
            RefreshPanel();
        });
    }

    // ===== Data access ==============================================================
    [Serializable] private struct UpgradeInfo { public string Name; public string Description; public int Cost; public string Gains; }

    private ScriptableObject GetDataFor(UpgradeCategory cat) => cat switch
    {
        UpgradeCategory.Armor          => armorData,
        UpgradeCategory.Movement       => movementData,
        UpgradeCategory.Attack         => attackData,
        UpgradeCategory.SpecialAbility => specialData,
        _ => null
    };

    private bool TryGetUpgradeFromSO(ScriptableObject so, ShipType ship, int level, out UpgradeInfo info)
    {
        info = default;
        if (so == null || level <= 0) return false;

        object upgradeObj = null;

        // Preferred: GetUpgrade(ship, level)
        var getUpgrade = so.GetType().GetMethod("GetUpgrade",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        if (getUpgrade != null && getUpgrade.GetParameters().Length == 2)
        {
            var p0 = getUpgrade.GetParameters()[0].ParameterType;
            var p1 = getUpgrade.GetParameters()[1].ParameterType;

            object shipArg = null;
            if (p0.IsEnum) { try { shipArg = Enum.Parse(p0, ship.ToString(), true); } catch { } }
            shipArg ??= (p0 == typeof(int) ? (object)(int)ship : (p0 == typeof(string) ? ship.ToString() : null));

            object lvlArg = (p1 == typeof(int)) ? level : (object)level.ToString();

            try { upgradeObj = getUpgrade.Invoke(so, new[] { shipArg, lvlArg }); } catch { upgradeObj = null; }
        }

        // Fallback: arrays/lists per ship (e.g., SubmarineUpgrades[level-1])
        if (upgradeObj == null)
        {
            var field = so.GetType().GetField($"{ship}Upgrades",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var list = field.GetValue(so) as IEnumerable;
                if (list != null) upgradeObj = GetIndex(list, level - 1);
            }
        }

        if (upgradeObj == null) return false;

        string name = ReadString(upgradeObj, new[] { "UpgradeName", "Name", "name", "title" });
        string desc = ReadString(upgradeObj, new[] { "Description", "Desc", "description" });
        int    cost = ReadInt   (upgradeObj, new[] { "Cost", "Price", "cost" }, 0);

        // Gains: use explicit field if present, else synthesize from common stats, else description
        string gains = ReadString(upgradeObj, new[] { "Gains", "gains" });
        if (string.IsNullOrWhiteSpace(gains))
        {
            int armorPts = ReadInt(upgradeObj, new[] { "ArmorPoints", "armorPoints", "Armor", "armor" }, 0);
            int speedPts = ReadInt(upgradeObj, new[] { "SpeedPoints", "speedPoints", "Speed", "speed" }, 0);
            int attackPts= ReadInt(upgradeObj, new[] { "AttackPoints","attackPoints","Attack","attack" }, 0);

            var parts = new List<string>(3);
            if (armorPts != 0)  parts.Add($"+{armorPts} armor");
            if (speedPts != 0)  parts.Add($"+{speedPts} speed");
            if (attackPts != 0) parts.Add($"+{attackPts} attack");
            gains = parts.Count > 0 ? string.Join("  |  ", parts) : desc;
        }

        info = new UpgradeInfo { Name = name, Description = desc, Cost = cost, Gains = gains };
        return true;
    }

    // ===== Runtime binding & wiring ================================================
    private void TryFindUIRefs(Transform root)
    {
        if (!panelRoot)
        {
            var t = root.Find("GROUP_UpgradeInfo");
            if (t) panelRoot = t.gameObject;
        }

        nextNameLabel  ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextName");
        nextDescLabel  ??= FindTMP(root, "GROUP_UpgradeInfo/LABEL_UpgradeDescription/TXT_NextDescription");
        nextCostLabel  ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextCost");
        nextGainsLabel ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextGains");

        if (!upgradeButton)
        {
            var bt = root.Find("GROUP_UpgradeInfo/BUTTON_Upgrade");
            if (bt) upgradeButton = bt.GetComponent<Button>();
        }
    }

    private void RuntimeWireLeftButtons()
    {
        var left = transform.Find("GROUP_Left");
        if (!left) return;

        // Submarine
        Wire(left, "Sub",        UpgradeCategory.Armor,          ShipType.Submarine,  new[] { "armor" });
        Wire(left, "Sub",        UpgradeCategory.Movement,       ShipType.Submarine,  new[] { "move", "movement", "speed" });
        Wire(left, "Sub",        UpgradeCategory.Attack,         ShipType.Submarine,  new[] { "attack" });
        Wire(left, "Sub",        UpgradeCategory.SpecialAbility, ShipType.Submarine,  new[] { "spec", "special" });

        // Destroyer
        Wire(left, "Destroyer",  UpgradeCategory.Armor,          ShipType.Destroyer,  new[] { "armor" });
        Wire(left, "Destroyer",  UpgradeCategory.Movement,       ShipType.Destroyer,  new[] { "move", "movement", "speed" });
        Wire(left, "Destroyer",  UpgradeCategory.Attack,         ShipType.Destroyer,  new[] { "attack" });
        Wire(left, "Destroyer",  UpgradeCategory.SpecialAbility, ShipType.Destroyer,  new[] { "spec", "special" });

        // Cruiser
        Wire(left, "Cruiser",    UpgradeCategory.Armor,          ShipType.Cruiser,    new[] { "armor" });
        Wire(left, "Cruiser",    UpgradeCategory.Movement,       ShipType.Cruiser,    new[] { "move", "movement", "speed" });
        Wire(left, "Cruiser",    UpgradeCategory.Attack,         ShipType.Cruiser,    new[] { "attack" });
        Wire(left, "Cruiser",    UpgradeCategory.SpecialAbility, ShipType.Cruiser,    new[] { "spec", "special" });

        // Battleship
        Wire(left, "Battleship", UpgradeCategory.Armor,          ShipType.Battleship, new[] { "armor" });
        Wire(left, "Battleship", UpgradeCategory.Movement,       ShipType.Battleship, new[] { "move", "movement", "speed" });
        Wire(left, "Battleship", UpgradeCategory.Attack,         ShipType.Battleship, new[] { "attack" });
        Wire(left, "Battleship", UpgradeCategory.SpecialAbility, ShipType.Battleship, new[] { "spec", "special" });
    }

    private void Wire(Transform leftRoot, string shipKey, UpgradeCategory cat, ShipType ship, string[] tokens)
    {
        var buttons = leftRoot.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            string path = GetPath(b.transform, leftRoot).ToLowerInvariant();
            if (!path.Contains(shipKey.ToLowerInvariant())) continue;

            bool matches = false;
            foreach (var t in tokens) { if (path.Contains(t)) { matches = true; break; } }
            if (!matches) continue;

            if (b.onClick.GetPersistentEventCount() == 0)
                b.onClick.AddListener(() => Select(ship, cat));
        }
    }

    private static TMP_Text FindTMP(Transform root, string path)
    {
        var t = root.Find(path);
        return t ? t.GetComponent<TMP_Text>() : null;
    }

    private static string GetPath(Transform t, Transform stopAt)
    {
        var path = t.name;
        while (t.parent && t.parent != stopAt)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    private static object GetIndex(IEnumerable enumerable, int index)
    {
        if (index < 0) return null;
        int i = 0;
        foreach (var o in enumerable) { if (i++ == index) return o; }
        return null;
    }

    private static string ReadString(object obj, string[] names)
    {
        foreach (var n in names)
        {
            var f = obj.GetType().GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(string)) return (string)f.GetValue(obj);
            var p = obj.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(string)) return (string)p.GetValue(obj, null);
        }
        return string.Empty;
    }

    private static int ReadInt(object obj, string[] names, int fallback)
    {
        foreach (var n in names)
        {
            var f = obj.GetType().GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);
            var p = obj.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj, null);
        }
        return fallback;
    }

    private static float ReadFloat01(object obj, string[] names, float fallback)
    {
        foreach (var n in names)
        {
            var f = obj.GetType().GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(obj);
            var p = obj.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(obj, null);
        }
        return fallback;
    }

    // ===== Navigation / utilities ===================================================
    public void ToBattle()
    {
        var nextScene = SceneTypes.SceneType.Game;
        SceneManager.Instance.LoadScene(nextScene);
    }

    private void ShowConfirm(string title, string body, Action onConfirm)
    {
        try
        {
            var modalType = Type.GetType("SimpleModal");
            var mi = modalType?.GetMethod("Show",
                BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { typeof(string), typeof(string), typeof(Action) }, null);
            mi?.Invoke(null, new object[] { title, body, onConfirm });
            return;
        }
        catch { }
        onConfirm?.Invoke();
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
        if (uiAudioSource && sfxUpgradeSuccess) uiAudioSource.PlayOneShot(sfxUpgradeSuccess);
    }

    private static void Log(string msg) => Debug.Log(msg);
}
