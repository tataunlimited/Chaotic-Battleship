using Core.Board;
using Core.Ship;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DockUIScript : MonoBehaviour
{
    public GameObject HUD;
    public Button sellShipButton;
    public ShipPurchaser shipPurchaser;
    public TextMeshProUGUI sellText;
    public BoardView playerView;
    public ShipPlacementUI shipPlacementUI;
    public Button startBattleBtn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startBattleBtn.interactable = false;
        shipPlacementUI.OnAllShipsSpawned += OnAllShipsSpawned;
    }

    private void OnAllShipsSpawned(bool value)
    {
        startBattleBtn.interactable = value;
    }

    // Update is called once per frame
    void Update()
    {
        // only enable sell button if a ship is selected
        if (BoardController.Instance.SelectedShip != null)
        {
            sellShipButton.interactable = true;
            switch (BoardController.Instance.SelectedShip.shipModel.type)
            {
                case ShipType.Submarine:
                    sellText.text = "Sell for " + (int)(shipPurchaser.default_submarine_price * 0.75f) + " pts";
                    break;
                case ShipType.Destroyer:
                    sellText.text = "Sell for " + (int)(shipPurchaser.default_destroyer_price * 0.75f) + " pts";
                    break;
                case ShipType.Cruiser:
                    sellText.text = "Sell for " + (int)(shipPurchaser.default_cruiser_price * 0.75f) + " pts";
                    break;
                case ShipType.Battleship:
                    sellText.text = "Sell for " + (int)(shipPurchaser.default_battleship_price * 0.75f) + " pts";
                    break;
                default:
                    Debug.LogError("Unknown ship type selected!");
                    break;
            }
        }
        else
        {
            sellShipButton.interactable = false;
            sellText.text = "Sell Ship";
        }

        Debug.Log("Sell button interactable: " + sellShipButton.interactable);
    }

    public void StartWave()
    {
        if (!shipPlacementUI.AreAllShipsSpawnedAndPlaced)
        {
            return;
        }
        // go to next phase
        GameManager.instance.NextPhaseButton();
        // show HUD
        HUD.SetActive(true);
        //populate player ship info panel
        if (GameManager.instance.playerShipInfoPanel != null)
        {
            GameManager.instance.playerShipInfoPanel.Populate();
        } else
        {
            Debug.LogWarning("Player Ship Info Panel reference is null in GameManager!");
        }
        if (GameManager.instance.enemyShipInfoPanel != null)
        {
            GameManager.instance.enemyShipInfoPanel.Populate();
        } else
        {
            Debug.LogWarning("Enemy Ship Info Panel reference is null in GameManager!");
        }
        // hide dock UI
        this.gameObject.SetActive(false);

    }

    public void SellShip()
    {
        // sell selected ship
        if (BoardController.Instance.SelectedShip != null)
        {
            var selectedShip = BoardController.Instance.SelectedShip;
            if (selectedShip != null)
            {
                //figure out what ship we just sold and decrement the appropriate counter in PlayerData
                switch (selectedShip.shipModel.type)
                {
                    case ShipType.Submarine:
                        PlayerData.Instance.numberSubsInDock--;
                        PlayerData.Instance.currentScore += (int)(shipPurchaser.default_submarine_price * 0.75f);
                        break;
                    case ShipType.Destroyer:
                        PlayerData.Instance.numberDestroyersInDock--;
                        PlayerData.Instance.currentScore += (int)(shipPurchaser.default_destroyer_price * 0.75f);
                        break;
                    case ShipType.Cruiser:
                        PlayerData.Instance.numberCruisersInDock--;
                        PlayerData.Instance.currentScore += (int)(shipPurchaser.default_cruiser_price * 0.75f);
                        break;
                    case ShipType.Battleship:
                        PlayerData.Instance.numberBattleshipsInDock--;
                        PlayerData.Instance.currentScore += (int)(shipPurchaser.default_battleship_price * 0.75f);
                        break;
                    default:
                        Debug.LogError("Unknown ship type sold!");
                        break;
                }
                playerView.RemoveShip(selectedShip);
                BoardController.Instance.ClearSelectedShip();
                //update player board
                BoardController.Instance.playerView.UpdateBoard();
                //BoardController.Instance.playerView.ResetMovementPhase();
            }
        }
        else
        {
            sellShipButton.interactable = false;
        }
        SaveManager.SaveGame();
    }
}
