
using Core.GridSystem;
using Core.Ship;

using UnityEngine;
using System.Collections.Generic;

namespace Core.Board
{
    public class BoardController : MonoBehaviour
    {
        public BoardView playerView;
        public BoardView enemyView;
        public MovementCellManager movementCellManager;

        public List<ShipView> shipPrefabs;
        public static ShipView SelectedShip;
        
        private Camera _camera;


        private EnemyWaveManager _enemyWaveManager;

        public static BoardController Get()
        {
            return GameObject.Find("BoardController").GetComponent<BoardController>();
        }

       
        void Awake()
        {
            _camera = Camera.main;
        }

        public void Reset()
        {
            playerView.Reset();
            enemyView.Reset();
            movementCellManager.ClearCells();
            SelectedShip = null;
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

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (TryHitBoard(enemyView, out var eCell))  // left-click fires at enemy
                {
                    if (enemyView.Model.TryFire(eCell, out _))
                        enemyView.Tint(eCell);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (TrySelectShip(out var shipView)) // right-click to test on player
                {
                    UpdatePlayerSelectedShip(shipView);
                }
                
            }

        }

        private void UpdatePlayerSelectedShip(ShipView shipView)
        {
            shipView.SelectShip();
            if (SelectedShip != null)
            {
                SelectedShip.DeselectShip();
            }
            SelectedShip = shipView;
            var cellPositions = shipView.shipModel.GetMovablePositions(playerView);
            movementCellManager.ClearCells();
            foreach (var cell in cellPositions)
            {
                movementCellManager.SpawnCell(cell, () =>
                {
                    shipView.UpdatePosition(shipView.shipModel.MoveTo(cell), shipView.shipModel.orientation);
                    shipView.DeselectShip();
                    SelectedShip = null;
                    movementCellManager.ClearCells();
                });
            }
        
        }

        private bool TryHitBoard(BoardView view, out GridPos cell)
        {
            cell = default;
            return Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 500f) && view.WorldToGrid(hit.point, out cell);
        }

        private bool TrySelectShip(out ShipView shipView)
        {
            shipView = null;
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 500f))
                return false;
            shipView = hit.collider.GetComponentInParent<ShipView>();
            return shipView != null && shipView.IsPlayer;
        }

        public void UpdateEnemyShips()
        {
            Debug.Log("UpdateEnemyShips");

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            _enemyWaveManager.RandomlyMoveShips(enemyView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips();
        }

        public void SpawnEnemyShips()
        {
            _enemyWaveManager = new EnemyWaveManager();
            List<ShipModel> enemyShips = _enemyWaveManager.CreateDefaultWaveOfShips();  // create a default list of enemy ships

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            _enemyWaveManager.RandomlySetShipsLocations(enemyView, enemyShips);

            enemyView.Model.ResetAllCells();    // have to clear the previously set BoardModel in order to SpawnShips in those locations
            foreach (ShipModel ship in enemyShips)
                SpawnShip(ship.type, ship.root, ship.orientation, enemyView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips();
        }

        public void SpawnPlayerShips()
        {
            SpawnShip(ShipType.Cruiser, new GridPos(0,0), Orientation.South, playerView);
            SpawnShip(ShipType.Destroyer, new GridPos(1,0), Orientation.South, playerView);
            SpawnShip(ShipType.Battleship, new GridPos(2,0), Orientation.South, playerView);
            SpawnShip(ShipType.Submarine, new GridPos(3,0), Orientation.South, playerView);
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
            foreach (var ship in boardView.SpawnedShips)
            {
                ship.Value.Attack(boardView);
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