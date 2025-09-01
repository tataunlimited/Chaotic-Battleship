
using System;
using Core.Board;
using Core.Ship;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ShipPlacementUI : MonoBehaviour
{
    public BoardController board_controller;
    public GameObject placement_group;
    public TextMeshProUGUI Subs_left_to_place;
    public TextMeshProUGUI Destroyers_left_to_place;
    public TextMeshProUGUI Cruisers_left_to_place;
    public TextMeshProUGUI Battleships_left_to_place;
    public int subs_left;
    public int destroyers_left;
    public int cruisers_left;
    public int battleships_left;

    public Button Subs_left_to_place_Button;
    public Button Destroyers_left_to_place_Button;
    public Button Cruisers_left_to_place_Button;
    public Button Battleships_left_to_place_Button;

    private bool sub_selected_to_place = false;
    private bool destroyer_selected_to_place = false;
    private bool cruiser_selected_to_place = false;
    private bool battleship_selected_to_place = false;

    private bool in_placement_Phase = false;
    
    

    public bool AreAllShipsSpawned {private set; get; }

    public Action OnAllShipsSpawned;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
        {
            in_placement_Phase = true;
            placement_group.SetActive(true);
            Subs_left_to_place.text = subs_left.ToString();
            Destroyers_left_to_place.text = destroyers_left.ToString();
            Cruisers_left_to_place.text = cruisers_left.ToString();
            Battleships_left_to_place.text = battleships_left.ToString();
        }
        else
        {
            in_placement_Phase = false;
            sub_selected_to_place = false;
            destroyer_selected_to_place = false;
            cruiser_selected_to_place = false;
            battleship_selected_to_place = false;
            placement_group.SetActive(false);
        }
        if (subs_left <= 0)
        {
            Subs_left_to_place_Button.interactable = false;
        }
        if (destroyers_left <= 0)
        {
            Destroyers_left_to_place_Button.interactable = false;
        }
        if (cruisers_left <= 0)
        {
            Cruisers_left_to_place_Button.interactable = false;
        }
        if (battleships_left <= 0)
        {
            Battleships_left_to_place_Button.interactable = false;
        }
        if (subs_left <= 0 && destroyers_left <= 0 && cruisers_left <= 0 && battleships_left <= 0)
        {
            if (AreAllShipsSpawned) return;
            AreAllShipsSpawned = true;
            OnAllShipsSpawned?.Invoke();
            placement_group.SetActive(false);
        }
    }

    public void SetSubmarineToNextPlacement()
    {
        if (subs_left <= 0 || !in_placement_Phase)
        {
            Subs_left_to_place_Button.interactable = false;
            return;
        }
        sub_selected_to_place = true;
        Debug.Log("Sub Selected to place");
        var newShip = board_controller.SpawnPlayerShip(ShipType.Submarine);
        board_controller.UpdatePlayerSelectedShip(newShip);
        subs_left--;
    }

    public void SetDestroyerToNextPlacement()
    {
        if (destroyers_left <= 0 || !in_placement_Phase)
        {
            Destroyers_left_to_place_Button.interactable = false;
            return;
        }
        destroyer_selected_to_place = true;
        Debug.Log("Destroyer Selected to place");
        var newShip = board_controller.SpawnPlayerShip(ShipType.Destroyer);
        board_controller.UpdatePlayerSelectedShip(newShip);
        destroyers_left--;
    }

    public void SetCruiserToNextPlacement()
    {
        if (cruisers_left <= 0 || !in_placement_Phase)
        {
            Cruisers_left_to_place_Button.interactable = false;
            return;
        }
        cruiser_selected_to_place = true;
        Debug.Log("Cruiser Selected to place");
        var newShip = board_controller.SpawnPlayerShip(ShipType.Cruiser);
        board_controller.UpdatePlayerSelectedShip(newShip);
        cruisers_left--;
    }

    public void SetBattleshipToNextPlacement()
    {
        if (battleships_left <= 0 || !in_placement_Phase)
        {
            Battleships_left_to_place_Button.interactable = false;
            return;
        }
        battleship_selected_to_place = true;
        Debug.Log("Battleship Selected to place");
        var newShip = board_controller.SpawnPlayerShip(ShipType.Battleship);
        board_controller.UpdatePlayerSelectedShip(newShip);
        battleships_left--;
    }
}