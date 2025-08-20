
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
        
        void Start()
        {

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
            enemyWaveManager.RandomlyMoveShips(enemyView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips();
        }

        public void SpawnEnemyShips()
        {
            enemyWaveManager = new EnemyWaveManager();
            List<ShipModel> enemyShips = enemyWaveManager.CreateDefaultWaveOfShips();  // create a default list of enemy ships

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlySetShipsLocations(enemyView, enemyShips);

            enemyView.Model.ResetAllCells();    // have to clear the previously set BoardModel in order to SpawnShips in those locations
            foreach (ShipModel ship in enemyShips)
                SpawnShip(ship.type, ship.root, ship.orientation, enemyView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips();
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
            enemyView.ResetIndicators(enemyView.revealShips);
        }

        public void UpdateBoards()
        {
            playerView.UpdateBoard();
        }
    }
}