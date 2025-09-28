using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Ship.Upgrade
{
    [CreateAssetMenu(fileName = "AttackUpgradeSO", menuName = "Scriptable Objects/Ship Upgrade/AttackUpgradeSO")]
    public class AttackPatternUpgradeSO: BaseShipUpgradeSO
    {
        public List<AttackUpgradeList> attackUpgrades;
        
        public AttackUpgrade GetUpgrade(ShipType shipType, int level)
        {
            int index = level - 1;
            if(index < 0)
                return null;
            foreach (var upgradeList in attackUpgrades)
            {
                if (upgradeList.ShipType == shipType)
                {
                    return upgradeList.AttackUpgrades[index];
                }
            }
            return null;
        }
    }
    [Serializable]
    public class AttackUpgrade : BaseShipUpgrade
    {

    }
    [Serializable]
    public class AttackUpgradeList: UpgradeList
    {
        public List<AttackUpgrade> AttackUpgrades;
    }
}

