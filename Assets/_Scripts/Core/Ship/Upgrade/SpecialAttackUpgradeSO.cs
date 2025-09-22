using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;



namespace Core.Ship.Upgrade
{
    [CreateAssetMenu(fileName = "SpecialAttackUpgradeSO", menuName = "Scriptable Objects/Ship Upgrade/SpecialAttackUpgradeSO")]
    public class SpecialAttackUpgradeSO: BaseShipUpgradeSO
    {
        public List<SpecialAttackUpgradeList> specialAttackUpgrades;
        
        public SpecialAttackUpgrade GetUpgrade(ShipType shipType, int level)
        {
            int index = level - 1;
            if(index < 0)
                return null;
            foreach (var upgradeList in specialAttackUpgrades)
            {
                if (upgradeList.ShipType == shipType)
                {
                    return upgradeList.SpecialAttackUpgrade[index];
                }
            }
            return null;
        }
    }
    [Serializable]
    public class SpecialAttackUpgrade : BaseShipUpgrade
    {

    }
    [Serializable]
    public class SpecialAttackUpgradeList: UpgradeList
    {
        public List<SpecialAttackUpgrade> SpecialAttackUpgrade;
    }
}

