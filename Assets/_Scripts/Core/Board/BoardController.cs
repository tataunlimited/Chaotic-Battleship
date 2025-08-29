
using System;
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
        public HighlightAttackArea highlightAttackArea;

        public Action<bool> OnShipSelected;

        public List<ShipView> shipPrefabs;
        public ShipView SelectedShip {get; private set;}
        
        private Camera _camera;


        private EnemyWaveManager _enemyWaveManager;
        public static BoardController Instance;

       
        void Awake()
        {
            _camera = Camera.main;
            Instance = this;
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
            if (Input.GetMouseButtonDown(0))
            {
                if (TryHitBoard(enemyView, out var eCell))  // left-click fires at enemy
                {
                    // if (enemyView.Model.TryFire(eCell, out _))
                    //     enemyView.Tint(eCell);
                    if (SelectedShip != null && SelectedShip.shipModel.type == ShipType.Destroyer)
                    {
                        SelectedShip.shipModel.reserved = eCell;
                        highlightAttackArea.SpawnHighlights(SelectedShip.shipModel.GetPossibleAreaOfAttack(enemyView, out var selectedCoords, out var chance), selectedCoords, chance);
                    }
                }
                if (TrySelectShip(out var shipView)) // right-click to test on player
                {
                    UpdatePlayerSelectedShip(shipView);
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                foreach (var enemyShip in enemyView.SpawnedShips)
                {
                    enemyShip.Value.ApplyDamage(100);
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
            List<GridPos> cellPositions;
            if (SelectedShip.IsInInitialPhase)
            {
                cellPositions = shipView.shipModel.GetMovablePositions(playerView);
            }
            else
            {
                cellPositions = playerView.GetAllPossiblePositions(SelectedShip.shipModel);
            }
            movementCellManager.ClearCells();
            foreach (var cell in cellPositions)
            {
                movementCellManager.SpawnCell(cell, () =>
                {
                    SelectedShip.UpdatePosition(shipView.shipModel.MoveTo(cell), shipView.shipModel.orientation);
                    ClearSelectedShip();
                });
            }
            if(playerView.Model.InBounds(SelectedShip.shipModel.root))
                highlightAttackArea.SpawnHighlights(SelectedShip.shipModel.GetPossibleAreaOfAttack(enemyView, out var selectedCoords, out var chance), selectedCoords, chance);
            
            OnShipSelected?.Invoke(true);

        }

        public void ClearSelectedShip()
        {
            if (SelectedShip != null)
            {
                movementCellManager.ClearCells();
                SelectedShip.DeselectShip();
                highlightAttackArea.ClearHighlight();
                OnShipSelected?.Invoke(false);
                SelectedShip = null;
            }

            foreach (var ship in playerView.SpawnedShips)
            {
                ship.Value.DeselectShip();
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
            SpawnShip(ShipType.Cruiser, new GridPos(-1,2), Orientation.North, playerView);
            SpawnShip(ShipType.Destroyer, new GridPos(-2,1), Orientation.North, playerView);
            SpawnShip(ShipType.Battleship, new GridPos(-3,3), Orientation.North, playerView);
            SpawnShip(ShipType.Submarine, new GridPos(-1,3), Orientation.North, playerView);
        }

        public void PlayerAttack()
        {
            foreach (var ship in playerView.SpawnedShips)
            {
                if (ship.Value.shipModel.IsSunk) continue;  // skip sunk ships
                ship.Value.Attack(enemyView);
            }
        }
        
        public void EnemyAttack()
        {
            foreach (var ship in enemyView.SpawnedShips)
            {
                if (ship.Value.shipModel.IsSunk) continue;  // skip sunk ships
                ship.Value.Attack(playerView);
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

        public void ClearUI()
        {
            ClearSelectedShip();
        }
    }
}