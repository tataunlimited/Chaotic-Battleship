using System;
using System.Collections.Generic;
using Core.Ship;              // Uses your existing ShipType enum
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Singleton
    public static PlayerData Instance { get; private set; }

    // Core values
    public int waveNumber = 1;
    public int currentScore = 0;
    public enum Phase {Placement, Attack, Movement}
    public Phase currentPhase;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // NEW: score baseline captured at the start of each wave
    public int scoreAtWaveStart = 0;

    // Upgrade data container
    [System.Serializable]
    public class UpgradeLevels
    {
        public int SpecialAttack = 0;
        public int Movement = 0;
        public int AttackPattern = 0;
        public int Armor = 0;
    }

    // Per-ship upgrades
    [SerializeField] private Dictionary<ShipType, UpgradeLevels> upgrades = new Dictionary<ShipType, UpgradeLevels>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Ensure we have upgrade records and then load saved values
        EnsureUpgradeDefaults();
        SaveManager.LoadGame();
    }

    // Ensure every ship we care about has an UpgradeLevels record
    public void EnsureUpgradeDefaults()
    {
        foreach (var ship in AllShipTypesForUpgrades())
        {
            if (!upgrades.ContainsKey(ship))
                upgrades[ship] = new UpgradeLevels();
        }
    }

    // List all ship types we want to track upgrades for
    public IEnumerable<ShipType> AllShipTypesForUpgrades()
    {
        // If you don't want every enum member, explicitly list the ones you ship with:
        foreach (ShipType s in Enum.GetValues(typeof(ShipType)))
            yield return s;
    }

    public int GetUpgrade(ShipType ship, UpgradeType type)
    {
        if (!upgrades.TryGetValue(ship, out var lvl))
            return 0;

        switch (type)
        {
            case UpgradeType.SpecialAttack: return lvl.SpecialAttack;
            case UpgradeType.Movement:      return lvl.Movement;
            case UpgradeType.AttackPattern: return lvl.AttackPattern;
            case UpgradeType.Armor:         return lvl.Armor;
            default: return 0;
        }
    }

    public void SetUpgrade(ShipType ship, UpgradeType type, int newLevel)
    {
        EnsureUpgradeDefaults();
        if (!upgrades.TryGetValue(ship, out var lvl))
        {
            lvl = new UpgradeLevels();
            upgrades[ship] = lvl;
        }

        switch (type)
        {
            case UpgradeType.SpecialAttack: lvl.SpecialAttack = newLevel; break;
            case UpgradeType.Movement:      lvl.Movement      = newLevel; break;
            case UpgradeType.AttackPattern: lvl.AttackPattern = newLevel; break;
            case UpgradeType.Armor:         lvl.Armor         = newLevel; break;
        }
    }

    // Expose upgrades for SaveManager
    public IReadOnlyDictionary<ShipType, UpgradeLevels> UpgradesReadonly => upgrades;
}
