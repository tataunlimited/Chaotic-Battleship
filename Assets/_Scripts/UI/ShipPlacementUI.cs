using System;
using Core.Board;
using Core.GridSystem;
using Core.Ship;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipPlacementUI : MonoBehaviour
{
    public BoardController board_controller;
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

    // NEW: cache whatever counts you set in the Inspector as the defaults for each wave
    private int _defSubs;
    private int _defDestroyers;
    private int _defCruisers;
    private int _defBattleships;
    
    public LayerMask gridLayer;

    public bool AreAllShipsSpawned { private set; get; }
    public bool AreAllShipsSpawnedAndPlaced => AreAllShipsSpawned && _spawnedShip == null;
    
    public Action<bool> OnAllShipsSpawned;

    private ShipView _spawnedShip;
    

    //SFX
    
    private void Awake()
    {
        // Capture Inspector defaults so we can restore them each new wave
        _defSubs = PlayerData.Instance.numberSubsInDock;
        _defDestroyers = PlayerData.Instance.numberDestroyersInDock;
        _defCruisers = PlayerData.Instance.numberCruisersInDock;
        _defBattleships = PlayerData.Instance.numberBattleshipsInDock;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        subs_left        = _defSubs;
        destroyers_left  = _defDestroyers;
        cruisers_left    = _defCruisers;
        battleships_left = _defBattleships;
    }


    // Update is called once per frame
    void Update()
    {
        
        if(_spawnedShip != null)
        {
            var gridPos = GetMouseGridPosition();
            _spawnedShip.transform.position = board_controller.playerView.GridToWorld(gridPos);
            if (Input.mouseScrollDelta.y > 0f)
            {
                var or = _spawnedShip.shipModel.RotateLeft();
                RotateSpawnedShip(or);;
            }
            else if(Input.mouseScrollDelta.y < 0f)
            {
                var or = _spawnedShip.shipModel.RotateRight();
                RotateSpawnedShip(or);
            }
        }
        
        if (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
        {
            in_placement_Phase = true;
            Subs_left_to_place.text       = subs_left.ToString();
            Destroyers_left_to_place.text = destroyers_left.ToString();
            Cruisers_left_to_place.text   = cruisers_left.ToString();
            Battleships_left_to_place.text= battleships_left.ToString();
        }
        else
        {
            in_placement_Phase = false;
            sub_selected_to_place        = false;
            destroyer_selected_to_place  = false;
            cruiser_selected_to_place    = false;
            battleship_selected_to_place = false;
        }

        if (subs_left <= 0)        { Subs_left_to_place_Button.interactable        = false; } else { Subs_left_to_place_Button.interactable        = true; }
        if (destroyers_left <= 0)  { Destroyers_left_to_place_Button.interactable  = false; } else { Destroyers_left_to_place_Button.interactable  = true; }
        if (cruisers_left <= 0)    { Cruisers_left_to_place_Button.interactable    = false; } else { Cruisers_left_to_place_Button.interactable    = true; }
        if (battleships_left <= 0) { Battleships_left_to_place_Button.interactable = false; } else { Battleships_left_to_place_Button.interactable = true; }

        
        if (subs_left <= 0 && destroyers_left <= 0 && cruisers_left <= 0 && battleships_left <= 0)
        {
            AreAllShipsSpawned = true;

            if (AreAllShipsSpawnedAndPlaced)
            {
                OnAllShipsSpawned?.Invoke(AreAllShipsSpawnedAndPlaced);
            }
        }
        else
        {
            if (AreAllShipsSpawned)
            {
                AreAllShipsSpawned = false;
                OnAllShipsSpawned?.Invoke(false);
            }
        }

        

    }
    private void RotateSpawnedShip(Orientation or)
    {
        if (_spawnedShip == null) return;
        _spawnedShip.shipModel.orientation = or;
        _spawnedShip.transform.rotation = _spawnedShip.GetRotation(or);
        BoardController.Instance.UpdatePlayerSelectedShip(_spawnedShip);
    }
    private GridPos GetMouseGridPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GridPos pos = new GridPos(100000, 100000);
        if (Physics.Raycast(ray, out hit, 1000, gridLayer))
        {
            var cellPosition = hit.collider.transform.position;
            board_controller.playerView.WorldToGrid(cellPosition , out pos);
        }
        return pos;
    }

    // NEW: called by GameManager at the start of each wave
    public void ResetForNewWave()
    {
        subs_left        = PlayerData.Instance.numberSubsInDock;
        destroyers_left  = PlayerData.Instance.numberDestroyersInDock;
        cruisers_left    = PlayerData.Instance.numberCruisersInDock;
        battleships_left = PlayerData.Instance.numberBattleshipsInDock;

        if (Subs_left_to_place_Button)        Subs_left_to_place_Button.interactable        = true;
        if (Destroyers_left_to_place_Button)  Destroyers_left_to_place_Button.interactable  = true;
        if (Cruisers_left_to_place_Button)    Cruisers_left_to_place_Button.interactable    = true;
        if (Battleships_left_to_place_Button) Battleships_left_to_place_Button.interactable = true;

        sub_selected_to_place        = false;
        destroyer_selected_to_place  = false;
        cruiser_selected_to_place    = false;
        battleship_selected_to_place = false;

        AreAllShipsSpawned = false;
        // placement_group gets shown automatically when the phase flips in Update()
    }

    public void SetSubmarineToNextPlacement()
    {
        if(_spawnedShip)
            return;
        if (subs_left <= 0 || !in_placement_Phase)
        {
            Subs_left_to_place_Button.interactable = false;
            return;
        }
        sub_selected_to_place = true;
        Debug.Log("Sub Selected to place");
        var newShip = board_controller.SpawnPlayerShip(GetShip(ShipType.Submarine));
        
        board_controller.UpdatePlayerSelectedShip(newShip);
        subs_left--;
        StartShipPlacementOnGrid(newShip);
    }


    public void SetDestroyerToNextPlacement()
    {
        if(_spawnedShip)
            return;
        if (destroyers_left <= 0 || !in_placement_Phase)
        {
            Destroyers_left_to_place_Button.interactable = false;
            return;
        }
        destroyer_selected_to_place = true;
        Debug.Log("Destroyer Selected to place");
        var newShip = board_controller.SpawnPlayerShip(GetShip(ShipType.Destroyer));
        board_controller.UpdatePlayerSelectedShip(newShip);
        destroyers_left--;
        StartShipPlacementOnGrid(newShip);

    }

    public void SetCruiserToNextPlacement()
    {
        if(_spawnedShip)
            return;
        if (cruisers_left <= 0 || !in_placement_Phase)
        {
            Cruisers_left_to_place_Button.interactable = false;
            return;
        }
        cruiser_selected_to_place = true;
        Debug.Log("Cruiser Selected to place");
        var newShip = board_controller.SpawnPlayerShip(GetShip(ShipType.Cruiser));
        board_controller.UpdatePlayerSelectedShip(newShip);
        cruisers_left--;
        StartShipPlacementOnGrid(newShip);

    }

    public void SetBattleshipToNextPlacement()
    {
        if(_spawnedShip)
            return;
        if (battleships_left <= 0 || !in_placement_Phase)
        {
            Battleships_left_to_place_Button.interactable = false;
            return;
        }
        battleship_selected_to_place = true;
        Debug.Log("Battleship Selected to place");
        var newShip = board_controller.SpawnPlayerShip(GetShip(ShipType.Battleship));
        board_controller.UpdatePlayerSelectedShip(newShip);
        battleships_left--;
        StartShipPlacementOnGrid(newShip);
    }

    private void StartShipPlacementOnGrid(ShipView shipView)
    {
        SFXManager.Instance.PlayInitialPhaseShipConfirmOnGridSFX();
        _spawnedShip = shipView;
        _spawnedShip.OnBeforeShipPlacedOnGrid += OnShipPlacedOnTheGrid;
    }

    private void OnShipPlacedOnTheGrid(ShipView shipView)
    {
        SFXManager.Instance.PlayShipSelectMovementPhaseSFX();
        shipView.OnBeforeShipPlacedOnGrid -= OnShipPlacedOnTheGrid;
        _spawnedShip = null;
    }
    
    private ShipModel GetShip(ShipType type)
    {
        var model = ShipFactory.CreateShipModel(type);
        model.orientation = Orientation.North;
        model.root = new GridPos(-10000, -10000);
        return model;
    }
}
