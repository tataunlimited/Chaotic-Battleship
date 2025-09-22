using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using USM = UnityEngine.SceneManagement.SceneManager; // alias to avoid name clash

namespace UI
{
    public class HarborSceneUIManager : MonoBehaviour
    {
        private PlayerData playerData;

        public enum ShipType
        {
            None,
            Submarine,
            Destroyer,
            Cruiser,
            Battleship
        }

        public enum UpgradeCategory
        {
            Armor,
            Movement,
            Attack,
            SpecialAbility
        }

        [Header("Data (drag ScriptableObjects)")] [SerializeField]
        private ScriptableObject armorData;

        [SerializeField] private ScriptableObject movementData;
        [SerializeField] private ScriptableObject attackData;
        [SerializeField] private ScriptableObject specialData;

        [Header("Selection")] [SerializeField] private ShipType selectedShip = ShipType.None;
        [SerializeField] private UpgradeCategory selectedCategory = UpgradeCategory.Armor;

        [Header("Upgrade Panel UI")] [SerializeField]
        private GameObject panelRoot;

        [SerializeField] private TMP_Text nextNameLabel;
        [SerializeField] private TMP_Text nextDescLabel;
        [SerializeField] private TMP_Text nextCostLabel;
        [SerializeField] private TMP_Text nextGainsLabel;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private ShipUpgradeLabelHandler[] labelHandlers;

        [Header("Points Label (optional)")] [SerializeField]
        private TMP_Text pointsLabel; // binds to LABEL_CurrentPoints/TEXT_PointsLabel if present

        [Header("Options")] [SerializeField] private bool hidePanelOnStart = true;
        [SerializeField] private bool autoWireLeftButtons = true;

        [Header("Optional Audio")] [SerializeField]
        private AudioSource uiAudioSource;

        [SerializeField] private AudioClip sfxUpgradeSuccess;

        // --------- PlayerPrefs tier keys ----------


#if UNITY_EDITOR
        private void Reset()
        {
            EditorAutobind();
        }

        private void OnValidate()
        {
            EditorAutobind();
        }

        private void EditorAutobind()
        {
            TryFindUIRefs(transform);
        }
#endif

        private void Awake()
        {
            TryFindUIRefs(transform);
        }

        private void Start()
        {
            playerData = EnsurePlayerData(); // make sure it exists in Harbor
            if (panelRoot) panelRoot.SetActive(!hidePanelOnStart);

            if (upgradeButton)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (autoWireLeftButtons) RuntimeWireLeftButtons();

            UpdatePointsLabel();
            if (!hidePanelOnStart && selectedShip != ShipType.None) RefreshPanel();
        }

        private Core.Ship.ShipType GetShipType(ShipType ship)
        {
            switch (ship)
            {
                case ShipType.None:
                    throw new ArgumentOutOfRangeException(nameof(ship), ship, null);
                case ShipType.Submarine:
                    return Core.Ship.ShipType.Submarine;
                case ShipType.Destroyer:
                    return Core.Ship.ShipType.Destroyer;
                case ShipType.Cruiser:
                    return Core.Ship.ShipType.Cruiser;
                case ShipType.Battleship:
                    return Core.Ship.ShipType.Battleship;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ship), ship, null);
            }
        }

        private UpgradeType GetUpgradeType(UpgradeCategory upgradeCategory)
        {
            switch (upgradeCategory)
            {
                case UpgradeCategory.Armor:
                    return UpgradeType.Armor;
                case UpgradeCategory.Movement:
                    return UpgradeType.Movement;
                case UpgradeCategory.Attack:
                    return UpgradeType.AttackPattern;
                case UpgradeCategory.SpecialAbility:
                    return UpgradeType.SpecialAttack;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgradeCategory), upgradeCategory, null);
            }
        }

        // ===== Selection entry points (match existing button wiring) =====
        public void SubmarineArmorUpgradePressed() => Select(ShipType.Submarine, UpgradeCategory.Armor);
        public void DestroyerArmorUpgradePressed() => Select(ShipType.Destroyer, UpgradeCategory.Armor);
        public void CruiserArmorUpgradePressed() => Select(ShipType.Cruiser, UpgradeCategory.Armor);
        public void BattleshipArmorUpgradePressed() => Select(ShipType.Battleship, UpgradeCategory.Armor);

        public void SubmarineMovementUpgradePressed() => Select(ShipType.Submarine, UpgradeCategory.Movement);
        public void DestroyerMovementUpgradePressed() => Select(ShipType.Destroyer, UpgradeCategory.Movement);
        public void CruiserMovementUpgradePressed() => Select(ShipType.Cruiser, UpgradeCategory.Movement);
        public void BattleshipMovementUpgradePressed() => Select(ShipType.Battleship, UpgradeCategory.Movement);

        public void SubmarineAttackButtonPressed() => Select(ShipType.Submarine, UpgradeCategory.Attack);
        public void DestroyerAttackButtonPressed() => Select(ShipType.Destroyer, UpgradeCategory.Attack);
        public void CruiserAttackButtonPressed() => Select(ShipType.Cruiser, UpgradeCategory.Attack);
        public void BattleshipAttackButtonPressed() => Select(ShipType.Battleship, UpgradeCategory.Attack);

        public void SubmarineSpecialAbilityButtonPressed() => Select(ShipType.Submarine, UpgradeCategory.SpecialAbility);
        public void DestroyerSpecialAbilityButtonPressed() => Select(ShipType.Destroyer, UpgradeCategory.SpecialAbility);
        public void CruiserSpecialAbilityButtonPressed() => Select(ShipType.Cruiser, UpgradeCategory.SpecialAbility);
        public void BattleshipSpecialAbilityButtonPressed() => Select(ShipType.Battleship, UpgradeCategory.SpecialAbility);

        private void Select(ShipType ship, UpgradeCategory cat)
        {
            selectedShip = ship;
            selectedCategory = cat;
            if (panelRoot) panelRoot.SetActive(true);
            RefreshPanel();
            Debug.Log($"[HarborUI] Selected {ship} / {cat}");
        }

        private void RefreshPanel()
        {
            if (panelRoot == null || selectedShip == ShipType.None) return;

            int cur = PlayerData.Instance.GetUpgrade(GetShipType(selectedShip), GetUpgradeType(selectedCategory));
            int next = cur + 1;

            if (labelHandlers != null)
                foreach (var handler in labelHandlers)
                {
                    handler.UpdateLabels();
                }

            if (TryGetUpgradeFromSO(GetDataFor(selectedCategory), selectedShip, next, out var u))
            {
                if (nextNameLabel) nextNameLabel.text = u.Name;
                if (nextDescLabel) nextDescLabel.text = u.Description;
                if (nextCostLabel) nextCostLabel.text = $"Cost: {u.Cost}";
                if (nextGainsLabel) nextGainsLabel.text = u.Gains;

                bool canAfford = (EnsurePlayerData() != null) && (playerData.currentScore >= u.Cost);
                SetUpgradeButtonState(canAfford, true);
            }
            else
            {
                if (nextNameLabel) nextNameLabel.text = "MAXED";
                if (nextDescLabel) nextDescLabel.text = $"{selectedCategory} path complete.";
                if (nextCostLabel) nextCostLabel.text = "—";
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

            int cur = PlayerData.Instance.GetUpgrade(GetShipType(selectedShip), GetUpgradeType(selectedCategory));
            int next = cur + 1;

            if (!TryGetUpgradeFromSO(GetDataFor(selectedCategory), selectedShip, next, out var u))
            {
                Debug.Log("[HarborUI] Already maxed.");
                return;
            }

            var pd = EnsurePlayerData();
            if (pd == null)
            {
                Debug.LogWarning("[HarborUI] PlayerData not available; cannot spend points.");
                return;
            }

            if (pd.currentScore < u.Cost)
            {
                Debug.Log($"[HarborUI] Need {u.Cost} points (have {pd.currentScore}).");
                RefreshPanel();
                return;
            }

            // Confirm then apply
            ShowConfirm("Confirm Upgrade",
                $"Upgrade {selectedShip} {selectedCategory} to {u.Name} for {u.Cost} points?",
                () =>
                {
                    int before = pd.currentScore;
                    pd.currentScore = Mathf.Max(0, pd.currentScore - u.Cost);
                    //SetTier(selectedCategory, selectedShip, next);
                    PlayerData.Instance.SetUpgrade(GetShipType(selectedShip), GetUpgradeType(selectedCategory), next);
                    UpdatePointsLabel();
                    SaveManager.SaveGame();
                    //SaveIfPossible();
                    PlaySuccess();

                    Debug.Log(
                        $"[HarborUI] Purchase OK: {before} → {pd.currentScore}, {selectedShip} {selectedCategory} tier {next}");
                    RefreshPanel();
                });
        }

        // --------- SO access ----------
        [Serializable]
        private struct UpgradeInfo
        {
            public string Name;
            public string Description;
            public int Cost;
            public string Gains;
        }

        private ScriptableObject GetDataFor(UpgradeCategory cat) => cat switch
        {
            UpgradeCategory.Armor => armorData,
            UpgradeCategory.Movement => movementData,
            UpgradeCategory.Attack => attackData,
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
                if (p0.IsEnum)
                {
                    try
                    {
                        shipArg = Enum.Parse(p0, ship.ToString(), true);
                    }
                    catch
                    {
                    }
                }

                shipArg ??= (p0 == typeof(int)) ? (object)(int)ship :
                    (p0 == typeof(string)) ? (object)ship.ToString() : null;

                object lvlArg = (p1 == typeof(int)) ? level : (object)level.ToString();

                try
                {
                    upgradeObj = getUpgrade.Invoke(so, new[] { shipArg, lvlArg });
                }
                catch
                {
                    upgradeObj = null;
                }
            }

            // Fallback: arrays/lists per ship name (e.g., SubmarineUpgrades[level-1])
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
            int cost = ReadInt(upgradeObj, new[] { "Cost", "Price", "cost" }, 0);

            string gains = ReadString(upgradeObj, new[] { "Gains", "gains" });
            if (string.IsNullOrWhiteSpace(gains))
            {
                int armorPts = ReadInt(upgradeObj, new[] { "ArmorPoints", "armorPoints", "Armor", "armor" }, 0);
                int speedPts = ReadInt(upgradeObj, new[] { "SpeedPoints", "speedPoints", "Speed", "speed" }, 0);
                int attackPts = ReadInt(upgradeObj, new[] { "AttackPoints", "attackPoints", "Attack", "attack" }, 0);
                var parts = new List<string>(3);
                if (armorPts != 0) parts.Add($"+{armorPts} armor");
                if (speedPts != 0) parts.Add($"+{speedPts} speed");
                if (attackPts != 0) parts.Add($"+{attackPts} attack");
                gains = parts.Count > 0 ? string.Join("  |  ", parts) : desc;
            }

            info = new UpgradeInfo { Name = name, Description = desc, Cost = cost, Gains = gains };
            return true;
        }

        // --------- UI binding / wiring ----------
        private void TryFindUIRefs(Transform root)
        {
            panelRoot ??= root.Find("GROUP_UpgradeInfo")?.gameObject;
            nextNameLabel ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextName");
            nextDescLabel ??= FindTMP(root, "GROUP_UpgradeInfo/LABEL_UpgradeDescription/TXT_NextDescription");
            nextCostLabel ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextCost");
            nextGainsLabel ??= FindTMP(root, "GROUP_UpgradeInfo/TXT_NextGains");

            if (!upgradeButton)
            {
                var bt = root.Find("GROUP_UpgradeInfo/BUTTON_Upgrade");
                if (bt) upgradeButton = bt.GetComponent<Button>();
            }

            // optional score label
            if (!pointsLabel)
            {
                pointsLabel = FindTMP(root, "LABEL_CurrentPoints/TEXT_PointsLabel")
                              ?? FindTMP(root, "TXT_CurrentPoints");
            }
        }

        private void RuntimeWireLeftButtons()
        {
            var left = transform.Find("GROUP_Left");
            if (!left) return;

            Wire(left, "Sub", UpgradeCategory.Armor, ShipType.Submarine, new[] { "armor" });
            Wire(left, "Sub", UpgradeCategory.Movement, ShipType.Submarine, new[] { "move", "movement", "speed" });
            Wire(left, "Sub", UpgradeCategory.Attack, ShipType.Submarine, new[] { "attack" });
            Wire(left, "Sub", UpgradeCategory.SpecialAbility, ShipType.Submarine, new[] { "spec", "special" });

            Wire(left, "Destroyer", UpgradeCategory.Armor, ShipType.Destroyer, new[] { "armor" });
            Wire(left, "Destroyer", UpgradeCategory.Movement, ShipType.Destroyer, new[] { "move", "movement", "speed" });
            Wire(left, "Destroyer", UpgradeCategory.Attack, ShipType.Destroyer, new[] { "attack" });
            Wire(left, "Destroyer", UpgradeCategory.SpecialAbility, ShipType.Destroyer, new[] { "spec", "special" });

            Wire(left, "Cruiser", UpgradeCategory.Armor, ShipType.Cruiser, new[] { "armor" });
            Wire(left, "Cruiser", UpgradeCategory.Movement, ShipType.Cruiser, new[] { "move", "movement", "speed" });
            Wire(left, "Cruiser", UpgradeCategory.Attack, ShipType.Cruiser, new[] { "attack" });
            Wire(left, "Cruiser", UpgradeCategory.SpecialAbility, ShipType.Cruiser, new[] { "spec", "special" });

            Wire(left, "Battleship", UpgradeCategory.Armor, ShipType.Battleship, new[] { "armor" });
            Wire(left, "Battleship", UpgradeCategory.Movement, ShipType.Battleship, new[] { "move", "movement", "speed" });
            Wire(left, "Battleship", UpgradeCategory.Attack, ShipType.Battleship, new[] { "attack" });
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
                foreach (var t in tokens)
                {
                    if (path.Contains(t))
                    {
                        matches = true;
                        break;
                    }
                }

                if (!matches) continue;

                if (b.onClick.GetPersistentEventCount() == 0)
                    b.onClick.AddListener(() => Select(ship, cat));
            }
        }

        private void UpdatePointsLabel()
        {
            if (pointsLabel && EnsurePlayerData() != null)
                pointsLabel.text = EnsurePlayerData().currentScore.ToString();
            if (labelHandlers == null) return;
            foreach (var handler in labelHandlers)
            {
                handler.UpdateLabels();
            }
        }

        // --------- Scene navigation ----------
        public void ToBattle()
        {
            try
            {
                // Prefer project SceneManager if available
                var sm = SceneManager.Instance; // project singleton
                if (sm != null)
                {
                    sm.LoadScene(SceneTypes.SceneType.Game);
                    return;
                }
            }
            catch
            {
                /* fall through to Unity fallback */
            }

            // Fallbacks using Unity SceneManager
            if (Application.CanStreamedLevelBeLoaded("Game"))
            {
                USM.LoadScene("Game");
                return;
            }

            if (USM.sceneCountInBuildSettings > 1) USM.LoadScene(1); // assume Game is index 1
            else Debug.LogWarning("[HarborUI] Could not load Game scene. Check Build Settings.");
        }

        // --------- Helpers ----------
        private PlayerData EnsurePlayerData()
        {
            if (playerData != null) return playerData;

            // Try singleton
            playerData = PlayerData.Instance;
            if (playerData != null) return playerData;

            // Try find in scene
            playerData = FindObjectOfType<PlayerData>();
            if (playerData != null) return playerData;

            // Last resort: create one (works if PlayerData is a MonoBehaviour singleton)
            try
            {
                var go = new GameObject("PlayerData");
                playerData = go.AddComponent<PlayerData>();
                DontDestroyOnLoad(go);
                Debug.Log("[HarborUI] PlayerData created in Harbor scene.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HarborUI] Could not create PlayerData: {e.Message}");
            }

            return playerData;
        }

        private void ShowConfirm(string title, string body, Action onConfirm)
        {
            try
            {
                var modalType = Type.GetType("SimpleModal");
                if (modalType != null)
                {
                    var mi = modalType.GetMethod("Show",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string), typeof(Action) }, null);
                    mi?.Invoke(null, new object[] { title, body, onConfirm });
                    return;
                }
            }
            catch
            {
            }

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
            catch
            {
            }
        }

        private void PlaySuccess()
        {
            if (uiAudioSource && sfxUpgradeSuccess) uiAudioSource.PlayOneShot(sfxUpgradeSuccess);
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
            foreach (var o in enumerable)
            {
                if (i++ == index) return o;
            }

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
    }
}