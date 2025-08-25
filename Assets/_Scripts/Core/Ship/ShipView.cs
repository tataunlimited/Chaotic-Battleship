using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using UnityEngine;

namespace Core.Ship
{
    public class ShipView : MonoBehaviour
    {
        public ShipModel shipModel;
        public BoardView playerView;

        public bool IsPlayer { private set; get; }

        private Collider _collider;


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

        public void Hide()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void Show()
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void Start()
        {
            _collider = GetComponentInChildren<Collider>(true);
        }


        public void Attack(BoardView enemyBoard)
        {
            StartCoroutine(AttackSequence(enemyBoard));
        }

        private IEnumerator AttackSequence(BoardView enemyBoard)
        {
            var coords = shipModel.GetAttackCoordinates(enemyBoard);
            foreach (var gridPos in coords)
            {
                if (enemyBoard.Model.TryFire(gridPos, out bool hit))
                {
                    enemyBoard.Tint(gridPos);
                }

                VFXManager.Instance.SpawnExplosion(enemyBoard.GridToWorld(gridPos, 0.5f));
                yield return new WaitForSeconds(0.1f);
            }
        }


        public void UpdatePosition(GridPos newPos, Orientation newOrientation, bool showCells = true)
        {
            playerView.Model.ResetShipCells(shipModel);
            if (showCells)
                playerView.Tint(shipModel.GetCells());

            shipModel.orientation = newOrientation;
            shipModel.root = newPos;
            playerView.Model.TryPlaceShip(shipModel);
            if (showCells)
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

        public void SelectShip()
        {
            _collider.enabled = false;
        }


        public void DeselectShip()
        {
            _collider.enabled = true;
        }

        public bool RotateLeft()
        {
            var targetOrientation = shipModel.RotateLeft();

            if (ValidateRotation(targetOrientation))
            {
                UpdatePosition(shipModel.root, targetOrientation);
                return true;
            }

            return false;
        }

        public bool RotateRight()
        {
            var targetOrientation = shipModel.RotateRight();

            if (ValidateRotation(targetOrientation))
            {
                UpdatePosition(shipModel.root, targetOrientation);
                return true;
            }
            return false;
        }

        private bool ValidateRotation(Orientation orientation)
        {
            var shipModelCopy = shipModel.Copy();
            shipModelCopy.orientation = orientation;
            return playerView.Model.ValidateShipPlacement(shipModelCopy, new List<GridPos> { shipModelCopy.root });
        }
    }
}