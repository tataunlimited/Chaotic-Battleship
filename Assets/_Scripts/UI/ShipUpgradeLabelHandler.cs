using Core.Ship;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShipUpgradeLabelHandler : MonoBehaviour
    {
        public ShipType shipType;
        public TMP_Text armorLabel;
        public TMP_Text attackLabel;
        public TMP_Text specialAttackLabel;
        public TMP_Text movementLabel;
        
        public void UpdateLabels()
        {
            var pd = PlayerData.Instance;
            if (pd == null) return;
            
            armorLabel.text = $"Armor: {pd.GetUpgrade(shipType, UpgradeType.Armor)}";
            attackLabel.text =  $"Attack: {pd.GetUpgrade(shipType, UpgradeType.AttackPattern)}";
            specialAttackLabel.text =  $"Special Ability: {pd.GetUpgrade(shipType, UpgradeType.SpecialAttack)}";
            movementLabel.text =  $"Movement: {pd.GetUpgrade(shipType, UpgradeType.Movement)}";
            
            
        }
        
        
    }
}
