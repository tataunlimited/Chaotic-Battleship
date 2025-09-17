using UnityEngine;

public class HarborSceneUIManager : MonoBehaviour
{
    //helpful values
    PlayerData playerData;
    private int current_points;

    #region upgrade prices:
    private int submarine_armor_level1_price = 400;
    private int submarine_armor_level2_price = 800;
    private int submarine_armor_level3_price = 1600;
    private int submarine_movement_level1_price = 1000;
    private int submarine_movement_level2_price = 1500;
    private int submarine_movement_level3_price = 2000;
    private int submarine_attack_level1_price = 1000;
    private int submarine_attack_level2_price = 1500;
    private int submarine_attack_level3_price = 3000;
    private int submarine_special_ability_level1_price = 1000;
    private int submarine_special_ability_level2_price = 1500;
    private int submarine_special_ability_level3_price = 3000;
    private int destroyer_armor_level1_price = 1100;
    private int destroyer_armor_level2_price = 1600;
    private int destroyer_armor_level3_price = 2100;
    private int destroyer_movement_level1_price = 500;
    private int destroyer_movement_level2_price = 1000;
    private int destroyer_movement_level3_price = 2000;
    private int destroyer_attack_level1_price = 1000;
    private int destroyer_attack_level2_price = 1500;
    private int destroyer_attack_level3_price = 3000;
    private int destroyer_special_ability_level1_price = 1000;
    private int destroyer_special_ability_level2_price = 1500;
    private int destroyer_special_ability_level3_price = 3000;
    private int cruiser_armor_level1_price = 1200;
    private int cruiser_armor_level2_price = 1700;
    private int cruiser_armor_level3_price = 2200;
    private int cruiser_movement_level1_price = 700;
    private int cruiser_movement_level2_price = 1400;
    private int cruiser_movement_level3_price = 2800;
    private int cruiser_attack_level1_price = 1000;
    private int cruiser_attack_level2_price = 1500;
    private int cruiser_attack_level3_price = 3000;
    private int cruiser_special_ability_level1_price = 1000;
    private int cruiser_special_ability_level2_price = 1500;
    private int cruiser_special_ability_level3_price = 3000;
    private int battleship_armor_level1_price = 1300;
    private int battleship_armor_level2_price = 1800;
    private int battleship_armor_level3_price = 2300;
    private int battleship_movement_level1_price = 800;
    private int battleship_movement_level2_price = 1600;
    private int battleship_movement_level3_price = 3200;
    private int battleship_attack_level1_price = 1000;
    private int battleship_attack_level2_price = 1500;
    private int battleship_attack_level3_price = 3000;
    private int battleship_special_ability_level1_price = 1000;
    private int battleship_special_ability_level2_price = 1500;
    private int battleship_special_ability_level3_price = 3000;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerData = PlayerData.Instance;
        current_points = playerData.currentScore;
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region useful button events
    public void SubmarineArmorUpgradePressed()
    {
        Debug.Log("Submarine Armor Upgrade Pressed");
    }

    public void SubmarineMovementUpgradePressed()
    {
        Debug.Log("Submarine Speed Upgrade Pressed");
    }

    public void SubmarineAttackButtonPressed()
    {
        Debug.Log("Submarine Attack Button Pressed");
    }

    public void SubmarineSpecialAbilityButtonPressed()
    {
        Debug.Log("Submarine Special Ability Button Pressed");
    }

    public void DestroyerArmorUpgradePressed()
    {
        Debug.Log("Destroyer Armor Upgrade Pressed");
    }

    public void DestroyerMovementUpgradePressed()
    {
        Debug.Log("Destroyer Speed Upgrade Pressed");
    }

    public void DestroyerAttackButtonPressed()
    {
        Debug.Log("Destroyer Attack Button Pressed");
    }

    public void DestroyerSpecialAbilityButtonPressed()
    {
        Debug.Log("Destroyer Special Ability Button Pressed");
    }

    public void CruiserArmorUpgradePressed()
    {
        Debug.Log("Cruiser Armor Upgrade Pressed");
    }

    public void CruiserMovementUpgradePressed()
    {
        Debug.Log("Cruiser Speed Upgrade Pressed");
    }

    public void CruiserAttackButtonPressed()
    {
        Debug.Log("Cruiser Attack Button Pressed");
    }

    public void CruiserSpecialAbilityButtonPressed()
    {
        Debug.Log("Cruiser Special Ability Button Pressed");
    }

    public void BattleshipArmorUpgradePressed()
    {
        Debug.Log("Battleship Armor Upgrade Pressed");
    }

    public void BattleshipMovementUpgradePressed()
    {
        Debug.Log("Battleship Speed Upgrade Pressed");
    }

    public void BattleshipAttackButtonPressed()
    {
        Debug.Log("Battleship Attack Button Pressed");
    }

    public void BattleshipSpecialAbilityButtonPressed()
    {
        Debug.Log("Battleship Special Ability Button Pressed");
    }

    #endregion

    public void UpgradePressed()
    {
        Debug.Log("Upgrade Pressed");
    }

    public void ToBattle()
    {
        Debug.Log("Transitioning to Battle Scene");
        SceneTypes.SceneType nextScene = SceneTypes.SceneType.Game;
        SceneManager.Instance.LoadScene(nextScene);
    }
}
