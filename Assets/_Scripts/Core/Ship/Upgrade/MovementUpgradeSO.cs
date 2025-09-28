using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Ship.Upgrade
{
    [CreateAssetMenu(fileName = "MovementUpgradeSO", menuName = "Scriptable Objects/Ship Upgrade/MovementUpgradeSO")]
    public class MovementUpgradeSO : BaseShipUpgradeSO
    {
        public List<MovementUpgradeList> Upgrades;
        
        public MovementUpgrade GetUpgrade(ShipType shipType, int level)
        {
            if(level < 0)
                return null;

            foreach (var upgradeList in Upgrades)
            {
                if (upgradeList.ShipType == shipType)
                {
                    return upgradeList.MovementUpgrades[level];
                }
            }
            return null;
        }
    }

    [Serializable]
    public class MovementUpgradeList : UpgradeList
    {
        public List<MovementUpgrade> MovementUpgrades;
    }

    [Serializable]
    public class MovementUpgrade : BaseShipUpgrade
    {
        [Tooltip("Movement points to add to ship.")]
        public int OrthogonalMovementPoints;
        public int DiagonalMovementPoints;
        public int AnyDirectionMovementPoints;
        public int ForwardMovementPoints;
        public int BackwardMovementPoints;
        public int ForwardDiagonalMovementPoints;
        public int BackwardDiagonalMovementPoints;
        public bool CanRotateAndMove;
    }

}