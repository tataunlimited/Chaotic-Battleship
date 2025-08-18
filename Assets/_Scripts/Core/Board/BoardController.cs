
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
        public static ShipView SelectedShip;

        private EnemyWaveManager enemyWaveManager;
        private List<ShipModel> enemyShips; 
        
        void Start()
        {
            // Example placements (pure logic via Model)
            


            enemyWaveManager = new EnemyWaveManager();
            enemyShips = enemyWaveManager.CreateDefaultWaveOfShips();  // create a default list of enemy ships

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlySetShipsLocations(enemyView, enemyShips);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips(enemyShips);
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
                if (SelectedShip == shipView)
                {
                    SelectedShip.DeselectShip();
                    movementCellManager.ClearCells();
                    SelectedShip = null;
                    return false;
                }
                shipView.SelectShip(movementCellManager);
                
                if (SelectedShip != null)
                {
                    SelectedShip.DeselectShip();
                }
                
                SelectedShip = shipView;
                return true;
            }

            return false;
        }

        public void UpdateEnemyShips()
        {
            Debug.Log("UpdateEnemyShips");

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlyMoveShips(enemyView, enemyShips);

            // uncomment next line for testing purposes to show where the enemy ships are placed
            //enemyView.RevealShips(enemyShips);
        }

        public void SpawnEnemyShips()
        {
            enemyWaveManager = new EnemyWaveManager();

            // create a list of enemy ships with given lengths
            List<ShipModel> ships = enemyWaveManager.CreateDefaultWaveOfShips();

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlySetShipsLocations(enemyView, ships);
                
            // test ship placement below
                
            SpawnShip(ShipType.Cruiser, new GridPos(0,0), Orientation.North, enemyView);
            SpawnShip(ShipType.Destroyer, new GridPos(5,5), Orientation.North, enemyView);
            SpawnShip(ShipType.Battleship, new GridPos(6,7), Orientation.South, enemyView);
            SpawnShip(ShipType.Submarine, new GridPos(9,5), Orientation.East, enemyView);
        }

        public void SpawnPlayerShips()
        {
            SpawnShip(ShipType.Cruiser, new GridPos(0,0), Orientation.North, playerView);
            SpawnShip(ShipType.Destroyer, new GridPos(1,0), Orientation.North, playerView);
            SpawnShip(ShipType.Battleship, new GridPos(2,0), Orientation.North, playerView);
            SpawnShip(ShipType.Submarine, new GridPos(3,0), Orientation.North, playerView);
        }

        public void PlayerAttack()
        {
            Attack(playerView);

            
        }
        public void EnemyAttack()
        {
            Attack(enemyView);
        }

        private void Attack(BoardView boardView)
        {
            foreach (var ship in boardView.SpawnedShipes)
            {
                ship.Value.Attack();
            }
        }

        public void ResetGridIndicators()
        {
            playerView.ResetIndicators();
            enemyView.ResetIndicators();
        }

        public void UpdateBoards()
        {
            playerView.UpdateBoard();
        }
    }
}