
using Core.GridSystem;
using Core.Ship;

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Core.Board
{
    public class BoardController : MonoBehaviour
    {
        public BoardView playerView;
        public BoardView enemyView;
        public MovementCellManager movementCellManager;

        public List<ShipView> shipPrefabs;
        private ShipView _selectedShip;
        

        void Start()
        {
            // Example placements (pure logic via Model)
            
            SpawnShip(ShipType.Cruiser, new GridPos(0,0), Orientation.North, playerView);
            SpawnShip(ShipType.Destroyer, new GridPos(1,0), Orientation.North, playerView);
            SpawnShip(ShipType.Battleship, new GridPos(2,0), Orientation.North, playerView);
            SpawnShip(ShipType.Submarine, new GridPos(3,0), Orientation.North, playerView);

            // uncomment next line for testing purposes to show where the enemy ships are placed
            //enemyView.revealShips = true;

            EnemyWaveManager enemyWaveManager = new EnemyWaveManager();

            // create a list of enemy ships with given lengths
            List<ShipModel> ships = enemyWaveManager.CreateDefaultWaveOfShips();

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlySetShipsLocations(enemyView, ships);

        }

        private void SpawnShip(ShipType shipType, GridPos pos, Orientation orientation, BoardView board)
        {
            var shipPrefab = shipPrefabs.Find((ship)=>ship.shipModel.type == shipType);
            if (shipPrefab == null)
            {
                Debug.LogError($"Ship type {shipType} not found");
                return;
            }

            board.TryPlaceShip(shipPrefab, pos, orientation);
        }
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (TryHitBoard(enemyView, out var eCell))  // left-click fires at enemy
                {
                    if (enemyView.Model.TryFire(eCell, out bool hit))
                        enemyView.Tint(eCell);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (TrySelectShip(out var shipView)) // right-click to test on player
                {
                    
                }
            }
        }

        bool TryHitBoard(BoardView view, out GridPos cell)
        {
            cell = default;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 500f))
                return false;
            return view.WorldToGrid(hit.point, out cell);
        }

        bool TrySelectShip(out ShipView shipView)
        {
            shipView = null;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 500f))
                return false;
            shipView = hit.collider.GetComponentInParent<ShipView>();
            if (shipView != null && shipView.IsPlayer)
            {
                if (_selectedShip == shipView)
                {
                    _selectedShip.DeselectShip();
                    movementCellManager.ClearCells();
                    _selectedShip = null;
                    return false;
                }
                shipView.SelectShip(movementCellManager);
                
                if (_selectedShip != null)
                {
                    _selectedShip.DeselectShip();
                }
                
                _selectedShip = shipView;
                return true;
            }

            return false;
        }


    }
}