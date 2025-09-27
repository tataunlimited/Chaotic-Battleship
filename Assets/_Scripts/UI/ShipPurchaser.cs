using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipPurchaser : MonoBehaviour
{
    public ShipPlacementUI shipPlacementUI;
    public GameManagerScore gameManagerScore;
    public TextMeshProUGUI slot_1_text;
    public TextMeshProUGUI slot_2_text;
    public TextMeshProUGUI slot_1_price_text;
    public TextMeshProUGUI slot_2_price_text;
    public TextMeshProUGUI current_points_text;
    public TextMeshProUGUI wave_number_text;
    public Button slot_1_button;
    public Button slot_2_button;
    public int max_ships_in_dock = 10;
    public int current_number_of_ships_in_dock = 0;    


    private int slot_1_price = 100;
    private int slot_2_price = 200;

    public readonly int default_submarine_price = 1000;
    public readonly int default_destroyer_price = 1500;
    public readonly int default_cruiser_price = 2000;
    public readonly int default_battleship_price = 2500;
    public readonly int default_price_increase_per_upgrade = 200;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManagerScore = FindFirstObjectByType<GameManagerScore>();
        if (gameManagerScore == null)
        {
            Debug.LogError("GameManagerScore not found in the scene.");
            return;
        }
        //select a random ship for purchase in slot 1 and slot 2
        int slot_1_ship = Random.Range(0, 4);
        int slot_2_ship = Random.Range(0, 4);
        switch (slot_1_ship)
        {
            case 0:
                slot_1_text.text = "Submarine";
                //set the price based on the player's upgrade level for the submarine
                int sub_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.AttackPattern);
                int sub_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.Movement);
                int sub_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.SpecialAttack);
                int sub_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.Armor);
                int sub_upgrade_level = sub_attack_upgrade_level + sub_movement_upgrade_level + sub_special_upgrade_level + sub_armor_upgrade_level;
                Debug.Log($"Sub Upgrade Level: {sub_upgrade_level}");
                slot_1_price = (int)((default_submarine_price + (sub_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 1:
                slot_1_text.text = "Destroyer";
                //set the price based on the player's upgrade level for the destroyer
                int des_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.AttackPattern);
                int des_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.Movement);
                int des_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.SpecialAttack);
                int des_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.Armor);
                int des_upgrade_level = des_attack_upgrade_level + des_movement_upgrade_level + des_special_upgrade_level + des_armor_upgrade_level;
                Debug.Log($"Des Upgrade Level: {des_upgrade_level}");
                slot_1_price = (int)((default_destroyer_price + (des_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 2:
                slot_1_text.text = "Cruiser";
                //set the price based on the player's upgrade level for the cruiser
                int cru_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.AttackPattern);
                int cru_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.Movement);
                int cru_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.SpecialAttack);
                int cru_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.Armor);
                int cru_upgrade_level = cru_attack_upgrade_level + cru_movement_upgrade_level + cru_special_upgrade_level + cru_armor_upgrade_level;
                Debug.Log($"Cru Upgrade Level: {cru_upgrade_level}");
                slot_1_price = (int)((default_cruiser_price + (cru_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 3:
                slot_1_text.text = "Battleship";
                //set the price based on the player's upgrade level for the battleship
                int bat_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.AttackPattern);
                int bat_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.Movement);
                int bat_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.SpecialAttack);
                int bat_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.Armor);
                int bat_upgrade_level = bat_attack_upgrade_level + bat_movement_upgrade_level + bat_special_upgrade_level + bat_armor_upgrade_level;
                Debug.Log($"Bat Upgrade Level: {bat_upgrade_level}");
                slot_1_price = (int)((default_battleship_price + (bat_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
        }

        switch (slot_2_ship)
        {
            case 0:
                slot_2_text.text = "Submarine";
                //set the price based on the player's upgrade level for the submarine
                int sub_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.AttackPattern);
                int sub_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.Movement);
                int sub_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.SpecialAttack);
                int sub_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Submarine, UpgradeType.Armor);
                int sub_upgrade_level = sub_attack_upgrade_level + sub_movement_upgrade_level + sub_special_upgrade_level + sub_armor_upgrade_level;
                Debug.Log($"Sub Upgrade Level: {sub_upgrade_level}");
                slot_2_price = (int)((default_submarine_price + (sub_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 1:
                slot_2_text.text = "Destroyer";
                //set the price based on the player's upgrade level for the destroyer
                int des_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.AttackPattern);
                int des_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.Movement);
                int des_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.SpecialAttack);
                int des_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Destroyer, UpgradeType.Armor);
                int des_upgrade_level = des_attack_upgrade_level + des_movement_upgrade_level + des_special_upgrade_level + des_armor_upgrade_level;
                Debug.Log($"Des Upgrade Level: {des_upgrade_level}");
                slot_2_price = (int)((default_destroyer_price + (des_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 2:
                slot_2_text.text = "Cruiser";
                //set the price based on the player's upgrade level for the cruiser
                int cru_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.AttackPattern);
                int cru_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.Movement);
                int cru_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.SpecialAttack);
                int cru_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Cruiser, UpgradeType.Armor);
                int cru_upgrade_level = cru_attack_upgrade_level + cru_movement_upgrade_level + cru_special_upgrade_level + cru_armor_upgrade_level;
                Debug.Log($"Cru Upgrade Level: {cru_upgrade_level}");
                slot_2_price = (int)((default_cruiser_price + (cru_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
            case 3:
                slot_2_text.text = "Battleship";
                //set the price based on the player's upgrade level for the battleship
                int bat_attack_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.AttackPattern);
                int bat_movement_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.Movement);
                int bat_special_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.SpecialAttack);
                int bat_armor_upgrade_level = PlayerData.Instance.GetUpgrade(Core.Ship.ShipType.Battleship, UpgradeType.Armor);
                int bat_upgrade_level = bat_attack_upgrade_level + bat_movement_upgrade_level + bat_special_upgrade_level + bat_armor_upgrade_level;
                Debug.Log($"Bat Upgrade Level: {bat_upgrade_level}");
                slot_2_price = (int)((default_battleship_price + (bat_upgrade_level * default_price_increase_per_upgrade)) * Random.Range(0.75f, 1.5f));
                break;
        }

        slot_1_price_text.text = slot_1_price.ToString();
        slot_2_price_text.text = slot_2_price.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        //set the purchase sot buttons highlight color to RED if the player does not have enough score to purchase the ship
        if (PlayerData.Instance.currentScore < slot_1_price)
        {
            slot_1_button.GetComponent<Image>().color = Color.red;
        }
        else
        {
            slot_1_button.GetComponent<Image>().color = Color.white;
        }

        if (PlayerData.Instance.currentScore < slot_2_price)
        {
            slot_2_button.GetComponent<Image>().color = Color.red;
        }
        else
        {
            slot_2_button.GetComponent<Image>().color = Color.white;
        }

        
        current_number_of_ships_in_dock = PlayerData.Instance.numberSubsInDock + PlayerData.Instance.numberDestroyersInDock + PlayerData.Instance.numberCruisersInDock + PlayerData.Instance.numberBattleshipsInDock;
        if(current_number_of_ships_in_dock >= max_ships_in_dock)
        {
            slot_1_button.interactable = false;
            slot_2_button.interactable = false;
        }
        
        current_points_text.text = "Current Points: " + PlayerData.Instance.currentScore.ToString();
        wave_number_text.text = "Wave: " + PlayerData.Instance.waveNumber.ToString();
    }

    public void PurchaseSlot1()
    {
        //add a check that total number of ships in dock is less than max allowed ships in dock (10)
        if (current_number_of_ships_in_dock < max_ships_in_dock && PlayerData.Instance.currentScore >= slot_1_price)
        {
            //add the selected ship to the ship placement UI for placement
            switch (slot_1_text.text)
            {
                case "Submarine":
                    PlayerData.Instance.numberSubsInDock++;
                    shipPlacementUI.subs_left++;
                    break;
                case "Destroyer":
                    PlayerData.Instance.numberDestroyersInDock++;
                    shipPlacementUI.destroyers_left++;
                    break;
                case "Cruiser":
                    PlayerData.Instance.numberCruisersInDock++;
                    shipPlacementUI.cruisers_left++;
                    break;
                case "Battleship":
                    PlayerData.Instance.numberBattleshipsInDock++;
                    shipPlacementUI.battleships_left++;
                    break;
            }
            PlayerData.Instance.currentScore -= slot_1_price;
            slot_1_text.text = "SOLD OUT";
            slot_1_button.interactable = false;
            slot_1_price_text.text = "-";
            SaveManager.SaveGame();
        }
    }

    public void PurchaseSlot2()
    {
        if (current_number_of_ships_in_dock < max_ships_in_dock && PlayerData.Instance.currentScore >= slot_2_price)
        {

            //add the selected ship to the ship placement UI for placement
            switch (slot_2_text.text)
            {
                case "Submarine":
                    PlayerData.Instance.numberSubsInDock++;
                    shipPlacementUI.subs_left++;
                    break;
                case "Destroyer":
                    PlayerData.Instance.numberDestroyersInDock++;
                    shipPlacementUI.destroyers_left++;
                    break;
                case "Cruiser":
                    PlayerData.Instance.numberCruisersInDock++;
                    shipPlacementUI.cruisers_left++;
                    break;
                case "Battleship":
                    PlayerData.Instance.numberBattleshipsInDock++;
                    shipPlacementUI.battleships_left++;
                    break;
            }
            PlayerData.Instance.currentScore -= slot_2_price;
            slot_2_text.text = "SOLD OUT";
            slot_2_button.interactable = false;
            slot_2_price_text.text = "-";
            SaveManager.SaveGame();
        }
    }
        
}
