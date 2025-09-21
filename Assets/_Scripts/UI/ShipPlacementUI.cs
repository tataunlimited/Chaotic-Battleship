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

    // NEW: cache whatever counts you set in the Inspector as the defaults for each wave
    private int _defSubs;
    private int _defDestroyers;
    private int _defCruisers;
    private int _defBattleships;

    public bool AreAllShipsSpawned { private set; get; }
    public Action OnAllShipsSpawned;

    private ShipView _spawnedShip;

    //SFX

    public AudioSource shipConfirmOnGridSFX; 

    private void Awake()
    {
        // Capture Inspector defaults so we can restore them each new wave
        _defSubs = subs_left;
        _defDestroyers = destroyers_left;
        _defCruisers = cruisers_left;
        _defBattleships = battleships_left;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        
        if(_spawnedShip != null)
        {
            var gridPos = GetMouseGridPosition();
            _spawnedShip.transform.position = board_controller.playerView.GridToWorld(gridPos);
        }
        
        if (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
        {
            in_placement_Phase = true;
            placement_group.SetActive(true);
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
            placement_group.SetActive(false);
        }

        if (subs_left <= 0)        { Subs_left_to_place_Button.interactable        = false; }
        if (destroyers_left <= 0)  { Destroyers_left_to_place_Button.interactable  = false; }
        if (cruisers_left <= 0)    { Cruisers_left_to_place_Button.interactable    = false; }
        if (battleships_left <= 0) { Battleships_left_to_place_Button.interactable = false; }

        if (subs_left <= 0 && destroyers_left <= 0 && cruisers_left <= 0 && battleships_left <= 0)
        {
            if (AreAllShipsSpawned) return;
            AreAllShipsSpawned = true;
            OnAllShipsSpawned?.Invoke();
            placement_group.SetActive(false);
        }
        

    }
    
    private GridPos GetMouseGridPosition()
    {
        /*// Create a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GridPos pos = default;
        // We need to know the 'z' position of our ground plane
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // Get the point where the ray intersects the plane
            Vector3 worldPoint = ray.GetPoint(rayDistance);
            // Convert the world point to a cell position on the tilemap
            board_controller.playerView.WorldToGrid(worldPoint , out pos);
        }*/
       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 cellPosition = new Vector3(-1000, 0, -1000);
        GridPos pos = default;
        if (Physics.Raycast(ray, out hit))
        {
            // hit.point contains the world position where the ray intersected with a collider
            cellPosition = hit.collider.transform.position;
            // You can also get the hit object: hit.collider.gameObject
        }
        board_controller.playerView.WorldToGrid(cellPosition , out pos);
        // Return a default value if the ray doesn't hit the plane
        return pos;
    }

    // NEW: called by GameManager at the start of each wave
    public void ResetForNewWave()
    {
        subs_left        = _defSubs;
        destroyers_left  = _defDestroyers;
        cruisers_left    = _defCruisers;
        battleships_left = _defBattleships;

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
        var newShip = board_controller.SpawnPlayerShip(ShipType.Submarine);
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
        var newShip = board_controller.SpawnPlayerShip(ShipType.Destroyer);
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
        var newShip = board_controller.SpawnPlayerShip(ShipType.Cruiser);
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
        var newShip = board_controller.SpawnPlayerShip(ShipType.Battleship);
        board_controller.UpdatePlayerSelectedShip(newShip);
        battleships_left--;
        StartShipPlacementOnGrid(newShip);
    }

    private void StartShipPlacementOnGrid(ShipView shipView)
    {
        _spawnedShip = shipView;
        _spawnedShip.OnBeforeShipPlacedOnGrid += OnShipPlacedOnTheGrid;
    }

    private void OnShipPlacedOnTheGrid(ShipView shipView)
    {
        shipConfirmOnGridSFX.Play();
        shipView.OnBeforeShipPlacedOnGrid -= OnShipPlacedOnTheGrid;
        _spawnedShip = null;
    }
}
