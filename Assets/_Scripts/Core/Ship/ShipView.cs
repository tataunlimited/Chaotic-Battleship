using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using NUnit.Framework;
using Core.Ship;
using UnityEngine;

namespace Core.Ship
{
    public class ShipView : MonoBehaviour
    {
        private static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
        public ShipModel shipModel;
        public bool IsPlayer {private set; get;}
        
        private Collider _collider;
        private BoardView _originBoardView;
        private ShipHealth _shipHealth;


        public GameObject defaultState;
        public GameObject brokenState;
        
        public bool IsInInitialPhase { get; private set; }
        public bool IsPlacedInsideTheGrid => shipModel.GetCells().TrueForAll(p=>_originBoardView.Model.InBounds(p));
         
        public void Init(BoardView boardView, ShipModel model, bool isPlayer)
        {
            _originBoardView = boardView;
            _shipHealth = GetComponent<ShipHealth>();
            shipModel = model;
            IsPlayer = isPlayer;
            SetShipOnGrid(!IsPlayer);
            if (shipModel.hp <= shipModel.MaxHP) shipModel.ResetHP();
            if (!isPlayer) Hide();
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
            var coords = shipModel.GetAttackCoordinates(enemyBoard, _originBoardView.IsLastShip);
            foreach (var gridPos in coords)
            {
                if (enemyBoard.Model.TryFire(gridPos, out bool hit))
                {
                    if(shipModel.type == ShipType.Submarine && !IsPlayer && !hit)
                    {

                    }
                    else
                    {
                        VFXManager.Instance.SpawnExplosion(enemyBoard.GridToWorld(gridPos, 0.5f));
                        enemyBoard.Tint(gridPos);
                    }
                        
                    if (hit)
                    {
                        //Find which enemy ship we hit
                        if (enemyBoard.TryGetShipAt(gridPos, out var enemyShip))
                        {
                            int damage = 1;
                            if (shipModel.type == ShipType.Destroyer && _originBoardView.IsLastShip)
                            {
                                damage = 9999;
                            }
                            bool justSunk = enemyShip.ApplyDamage(damage);
                            
                            enemyBoard.SpawnPersistentHitFire(enemyShip, gridPos, 0.5f);

                             //award per-segment points for HITS (player → enemy only)
                            if (IsPlayer && enemyBoard.side == BoardSide.Enemy)
                            {
                                GameEvents.RaiseHitSegment(enemyShip.shipModel.type);
                            }


                            if (justSunk)
                            {
                                //sunk 
                                VFXManager.Instance.SpawnSunkEffect(enemyBoard.GridToWorld(gridPos, 0.5f));
                                enemyBoard.RevealAShip(enemyShip.shipModel);
                                enemyBoard.OnShipSunk(enemyShip);
                                enemyShip.defaultState.SetActive(false);
                                enemyShip.brokenState.SetActive(true);

                                //award SINK bonus (player → enemy only)
                                if (IsPlayer && enemyBoard.side == BoardSide.Enemy)
                                {
                                    GameEvents.RaiseDestroyedShip(enemyShip.shipModel.type);
                                }
                            }
                            else
                            {
                                //hit 
                                VFXManager.Instance.SpawnHitEffect(enemyBoard.GridToWorld(gridPos, 0.5f));
                                
                            }
                        }
                    }
                    
                }

               
                yield return _waitForSeconds0_1;
            }
        }

        public bool ApplyDamage(int damage)
        {
            bool isSunk = shipModel.ApplyDamage(damage);
            if(IsPlayer)
                _shipHealth.UpdateHealthBar();
            else if(!IsPlayer && isSunk)
            {
                _shipHealth.EnableDestroyedState(true);
            }
            return isSunk;
        }


        public void UpdatePosition(GridPos newPos, Orientation newOrientation, bool showCells = true)
        {

             _originBoardView.Model.ResetShipCells(shipModel);
             if (showCells)
                 _originBoardView.Tint(shipModel.GetCells());

            shipModel.orientation = newOrientation;
            shipModel.root = newPos;
            _originBoardView.Model.TryPlaceShip(shipModel);
            if (showCells)
                _originBoardView.Tint(shipModel.GetCells());

            SetPosition();
        }

        private void SetPosition()
        {
            transform.position = _originBoardView.GridToWorld(shipModel.root);
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
            return _originBoardView.Model.ValidateShipPlacement(shipModelCopy, new List<GridPos> { shipModelCopy.root });
        }


        public void SetShipOnGrid(bool b)
        {
            IsInInitialPhase = b;
        }
    }
}