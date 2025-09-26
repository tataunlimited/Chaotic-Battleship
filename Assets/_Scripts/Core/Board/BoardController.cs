
using System;
using System.Collections;
using Core.GridSystem;
using Core.Ship;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utility;


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
        public ShipView SelectedShip { get; private set; }

        private Camera _camera;
        public LayerMask shipLayer;
        // exposing this for debugging purposes
        [SerializeField]
        private EnemyWaveManager _enemyWaveManager;
        public static BoardController Instance;
        //SFX
        public AudioSource shipSelectMovementPhaseSFX;

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

        private ShipView SpawnShip(ShipType shipType, GridPos pos, Orientation orientation, BoardView board)
        {
            var prefab = shipPrefabs.Find(s => s.shipModel.type == shipType);
            if (prefab == null) { Debug.LogError($"Ship type {shipType} not found"); return null; }

            if (board.TryPlaceShip(prefab, pos, orientation, out var instance))
                return instance;

            Debug.LogError("TryPlaceShip failed");
            return null;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && 
                (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS ||
                 GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_MOVING))
            {
                if (TryHitBoard(enemyView, out var eCell))  // left-click fires at enemy
                {
                    // if (enemyView.Model.TryFire(eCell, out _))
                    //     enemyView.Tint(eCell);
                    if (SelectedShip != null && SelectedShip.shipModel.CanTarget())
                    {
                        SelectedShip.shipModel.reserved = eCell;
                        HighlightAttackArea();
                        //highlightAttackArea.SpawnHighlights(SelectedShip.shipModel.GetPossibleAreaOfAttack(enemyView, out var selectedCoords, out var chance), selectedCoords, chance);
                    }
                }
                if (TrySelectShip(out var shipView)) // right-click to test on player
                {
                    UpdatePlayerSelectedShip(shipView);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                ClearSelectedShip();
            }

        }

        public void UpdatePlayerSelectedShip(ShipView shipView)
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
                    if (SelectedShip.UpdatePosition(cell, shipView.shipModel.orientation))
                    {
                        shipView.shipModel.UpdateMovementStatus();
                        UpdatePlayerSelectedShip(SelectedShip);
                        HighlightAttackArea();
                    }

                });
            }

            HighlightAttackArea();

            OnShipSelected?.Invoke(true);
            shipSelectMovementPhaseSFX.Play();

            // enable/disable the rotate buttons
            GameManager.instance.rotateLeftButton.interactable = shipView.shipModel.canRotate;
            GameManager.instance.rotateRightButton.interactable = shipView.shipModel.canRotate;
        }

        public void HighlightAttackArea()
        {
            if (SelectedShip == null)
            {
                return;
            }
            highlightAttackArea.SpawnHighlights(SelectedShip.shipModel);
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
                if (ship.Value == null)
                {
                    
                    continue;
                }
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
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 500f, shipLayer))
                return false;
            shipView = hit.collider.GetComponentInParent<ShipView>();
            return shipView != null && shipView.IsPlayer;
        }

        public void UpdateEnemyShips()
        {
            Debug.Log("UpdateEnemyShips");

            enemyView.BeginMovementPhase();

            EnsureEnemyWaveManager();
            // update the enemy ship locations and orientations, and place them on the enemyView board
            _enemyWaveManager.MoveEnemyShips(enemyView, playerView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (enemyView.revealShips)
                enemyView.RevealShips();
        }

        // public void SpawnEnemyShips()
        // {
        //     _enemyWaveManager = new EnemyWaveManager();

        //     // DEBUG ONLY: do not check in with the next 2 lines
        //     _enemyWaveManager.intelligenceLevel = 0;
        //     Debug.Log("FOR DEBUGGING: _enemyWaveManager.intelligenceLevel set to " + _enemyWaveManager.intelligenceLevel);

        //     List<ShipModel> enemyShips = _enemyWaveManager.CreateDefaultWaveOfShips();  // create a default list of enemy ships

        //     // randomly set the enemy ship locations and orientations, and place them on the enemyView board
        //     _enemyWaveManager.RandomlySetShipsLocations(enemyView, enemyShips);

        //     // Note: the designer said not to move intelligently during enemy placement
        //     // Give the AI a chance to move to "smarter" locations
        //     //_enemyWaveManager.MoveEnemyShips(enemyView, playerView);

        //     enemyView.Model.ResetAllCells();    // have to clear the previously set BoardModel in order to SpawnShips in those locations
        //     foreach (ShipModel ship in enemyShips)
        //         SpawnShip(ship.type, ship.root, ship.orientation, enemyView);

        //     // use revealShips for testing purposes to show where the enemy ships are placed
        //     if (enemyView.revealShips)
        //         enemyView.RevealShips();
        // }

        public ShipView SpawnPlayerShip(ShipType shipType)
        {
            return shipType switch
            {
                ShipType.Submarine => SpawnShip(ShipType.Submarine, new GridPos(-100, 1), Orientation.North, playerView),
                ShipType.Cruiser => SpawnShip(ShipType.Cruiser, new GridPos(-100, 3), Orientation.North, playerView),
                ShipType.Destroyer => SpawnShip(ShipType.Destroyer, new GridPos(-100, 2), Orientation.North, playerView),
                ShipType.Battleship => SpawnShip(ShipType.Battleship, new GridPos(-100, 4), Orientation.North, playerView),
                _ => throw new ArgumentOutOfRangeException(nameof(shipType), shipType, null),
            };
        }

        public IEnumerator PlayerAttack()
        {
            foreach (var ship in playerView.SpawnedShips)
            {
                if (ship.Value.shipModel.IsSunk) continue;  // skip sunk ships
                yield return StartCoroutine(ship.Value.AttackSequence(enemyView));
                yield return new WaitForSeconds(0.5f);
            }
            playerView.Model.UpdateScorchedCells();
        }

        public IEnumerator EnemyAttack()
        {
            var randomizedEnemyShipList = enemyView.SpawnedShips.Values.ToList();
            randomizedEnemyShipList.Shuffle();
            foreach (var ship in randomizedEnemyShipList)
            {
                if (ship.shipModel.IsSunk) continue;  // skip sunk ships
                yield return StartCoroutine(ship.AttackSequence(playerView));
                yield return new WaitForSeconds(0.5f);
            }
            enemyView.Model.UpdateScorchedCells();
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

        public void EnsureEnemyWaveManager()
        {
            if (_enemyWaveManager == null)
                _enemyWaveManager = new EnemyWaveManager();
        }


        public void ClearUI()
        {
            ClearSelectedShip();
        }

        public void UnfreezeFrozenShips()
        {
            enemyView.Unfreeze();
            playerView.Unfreeze();
        }

        public void SpawnEnemyShipsFromModels(List<ShipModel> ships, bool reveal = false)
        {
            if (ships == null || ships.Count == 0)
            {
                Debug.LogWarning("SpawnEnemyShipsFromModels: No ships to spawn.");
                return;
            }

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            _enemyWaveManager.RandomlySetShipsLocations(enemyView, ships);
            enemyView.Model.ResetAllCells();    // have to clear the previously set BoardModel in order to SpawnShips in those locations
            foreach (ShipModel ship in ships)
                SpawnShip(ship.type, ship.root, ship.orientation, enemyView);

            // use revealShips for testing purposes to show where the enemy ships are placed
            if (reveal || enemyView.revealShips)
            {
                enemyView.RevealShips();
            }
        }
        
        public void SetIntelligenceLevel(int level)
        {
            EnsureEnemyWaveManager();
            _enemyWaveManager.intelligenceLevel = Mathf.Clamp(level, 0, 3);
        }

    }
}