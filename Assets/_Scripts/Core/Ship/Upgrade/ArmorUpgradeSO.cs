using System;
using System.Collections.Generic;
using Core.Ship;
using UnityEngine;

namespace Core.Ship.Upgrade
{
    [CreateAssetMenu(fileName = "ArmorUpgradeSO", menuName = "Scriptable Objects/Ship Upgrade/ArmorUpgradeSO")]
    public class ArmorUpgradeSO : BaseShipUpgradeSO
    {
        public List<ArmorUpgradeList> Upgrades;
        
        public ArmorUpgrade GetUpgrade(ShipType shipType, int level)
        {
            int index = level - 1;
            if(index < 0)
                return null;
            foreach (var upgradeList in Upgrades)
            {
                if (upgradeList.ShipType == shipType)
                {
                    return upgradeList.ArmorUpgrades[index];
                }
            }
            return null;
        }
    }
}
[Serializable]
public class UpgradeList
{
    public ShipType ShipType;
}
[Serializable]
public class ArmorUpgradeList: UpgradeList
{
    public List<ArmorUpgrade> ArmorUpgrades;
}
[Serializable]
public class BaseShipUpgrade{
    public int Cost;
    public Sprite Icon;
    public string UpgradeName; 
    [TextArea]
    public string Description;
}
[Serializable]
public class ArmorUpgrade : BaseShipUpgrade
{
    [Tooltip("Armor points to add to ship. Decimal numbers are allowed and act as percentage of being hit or not.")]
    public float ArmorPoints;
    public float DestroyerArmorChance;
    public float CruiserArmorChance;
}