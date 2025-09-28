using UnityEngine;

namespace Core.Ship
{
    public class ShipUpgradeVisual : MonoBehaviour
    {
        [SerializeField] private ShipUpgradeElement movementUpgrade;
        [SerializeField] private ShipUpgradeElement attackUpgrade;
        [SerializeField] private ShipUpgradeElement armorUpgrade;
        [SerializeField] private ShipUpgradeElement specialAbilityUpgrade;

        public void Setup(int movementLevel, int attackLevel, int armorLevel, int specialAbilityLevel)
        {
            movementUpgrade.SetLevel(movementLevel);
            attackUpgrade.SetLevel(attackLevel);
            armorUpgrade.SetLevel(armorLevel);
            specialAbilityUpgrade.SetLevel(specialAbilityLevel);
        }
    }

    [System.Serializable]
    public class ShipUpgradeElement
    {
        public GameObject level1;
        public GameObject level2;
        public GameObject level3;

        public void SetLevel(int level)
        {
            level1.SetActive(level > 0);
            level2.SetActive(level > 1);
            level3.SetActive(level > 2);
        }
    }
}