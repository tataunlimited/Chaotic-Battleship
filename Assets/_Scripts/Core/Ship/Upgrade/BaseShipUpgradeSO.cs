using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Ship.Upgrade
{
    public class BaseShipUpgradeSO : ScriptableObject
    {

    }

    [Serializable]
    public class BaseShipUpgrade
    {
        public int Cost;
        public Sprite Icon;
        public string UpgradeName;
        [TextArea]
        public string Description;
    }

    [Serializable]
    public class UpgradeList
    {
        public ShipType ShipType;
    }

}
