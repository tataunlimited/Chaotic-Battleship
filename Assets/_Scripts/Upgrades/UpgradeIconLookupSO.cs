using System;
using System.Collections.Generic;
using Core.Ship;          // ShipType
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Icon Lookup", fileName = "UpgradeIconLookup")]
public class UpgradeIconLookupSO : ScriptableObject
{
    [Serializable]
    public class IconRow
    {
        public ShipType ship;
        public UpgradeType upgradeType;     // SpecialAttack, Movement, AttackPattern, Armor
        public List<Sprite> iconsByLevel = new(); // index 0 = level 0, etc.
    }

    [SerializeField] private List<IconRow> rows = new();
    [SerializeField] private Sprite placeholder;

    public Sprite GetIcon(ShipType ship, UpgradeType type, int level)
    {
        var row = rows.Find(r => r.ship == ship && r.upgradeType == type);
        if (row == null || level < 0 || level >= row.iconsByLevel.Count) return placeholder;
        return row.iconsByLevel[level] ? row.iconsByLevel[level] : placeholder;
    }
}
