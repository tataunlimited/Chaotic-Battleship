using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.GridSystem;
using UnityEngine;

namespace Core.Ship
{
    public class ShipView : MonoBehaviour
    {
        
        public ShipModel shipModel;
        public BoardView playerView;
        public BoardView enemyBoard;
        public bool IsPlayer {private set; get;}

        
        
        public void Init(BoardView boardView, ShipModel model, bool isPlayer)
        {
            playerView = boardView;
            shipModel = model;
            IsPlayer = isPlayer;
            if (!isPlayer)
            {
                Hide();
            }
            SetPosition();

        }

        private void Hide()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void Show()
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void Start()
        {
            // Temporary for testing
            var boards = FindObjectsByType<BoardView>(FindObjectsSortMode.None);
            if (boards != null)
            {
                enemyBoard = boards.First((board) => board.isPlayer != IsPlayer);
            }
        }


        public void Attack()
        {
            var coords = shipModel.GetAttackCoordinates(enemyBoard);
            foreach (var gridPos in coords)
            {
                if (enemyBoard.Model.TryFire(gridPos, out bool hit))
                {
                    enemyBoard.Tint(gridPos);
                }
                VFXManager.Instance.SpawnExplosion(enemyBoard.GridToWorld(gridPos, 0.5f));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Attack();
            }
        }


        void UpdatePosition(GridPos newPos, Orientation newOrientation)
        {
            playerView.Model.ResetShipCells(shipModel);
            playerView.Tint(shipModel.GetCells());
            shipModel.orientation = newOrientation;
            shipModel.root = newPos;
            playerView.Model.TryPlaceShip(shipModel);
            playerView.Tint(shipModel.GetCells());
            SetPosition();
        }

        private void SetPosition()
        {
            transform.position = playerView.GridToWorld(shipModel.root);
            float yAngle = shipModel.orientation switch 
            {
                Orientation.North => 0,
                Orientation.East => 90,
                Orientation.South => 180,
                Orientation.West => -90,
                _ => 0
            };
            
            transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
        }

        public void SelectShip(MovementCellManager movementCellManager)
        {
            movementCellManager.ClearCells();
            var cellPositions = shipModel.GetMovablePositions(playerView);

            foreach (var cell in cellPositions)
            {
                movementCellManager.SpawnCell(cell, () =>
                {
                    UpdatePosition(shipModel.MoveTo(cell), shipModel.orientation);
                });
            }
        }
        
        
        public void DeselectShip()
        {
        }
        
        
        
        public void MoveEast()
        {
            shipModel.MoveTowards(Orientation.East);
            SetPosition();
        }

        public void MoveWest()
        {
            shipModel.MoveTowards(Orientation.West);
            SetPosition();
        }

        public void MoveSouth()
        {
            shipModel.MoveTowards(Orientation.South);
            SetPosition();
        }

        public void MoveNorth()
        {
            shipModel.MoveTowards(Orientation.North);
            SetPosition();
        }

        public void RotateLeft()
        {
            var targetOrientation = shipModel.RotateLeft();
            
            if(ValidateRotation(targetOrientation))
                UpdatePosition(shipModel.root,  targetOrientation);
        }

        public void RotateRight()
        {
            var targetOrientation = shipModel.RotateRight();
            
            if(ValidateRotation(targetOrientation))
                UpdatePosition(shipModel.root, targetOrientation);
        }

        private bool ValidateRotation(Orientation orientation)
        {
            var shipModelCopy = shipModel.Copy();
            shipModelCopy.orientation = orientation;
            return playerView.Model.ValidateShipPlacement(shipModelCopy, new List<GridPos>{ shipModelCopy.root});
        }
    }
}
