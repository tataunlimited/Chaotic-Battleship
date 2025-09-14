using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using UnityEngine;
using System;
using DG.Tweening;

namespace Core.Ship
{
    public class ShipView : MonoBehaviour
    {
        private static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
        public ShipModel shipModel;
        public bool IsPlayer { private set; get; }

        private Collider _collider;
        public BoardView Board { private set; get; }
        private ShipHealth _shipHealth;


        public GameObject defaultState;
        public GameObject brokenState;

        private ShipMovementComponent _movementComponent;

        public Action<ShipView> OnBeforeShipPlacedOnGrid;

        public bool IsInInitialPhase { get; private set; }
        public bool IsPlacedInsideTheGrid => shipModel.GetCells().TrueForAll(p => Board.Model.InBounds(p));

        //rocking
        // --- Rocking/Bobbing State (Model-scope, no Unity deps) ---
        public bool rockingEnabled = true;

        // Degrees around the "roll" axis (e.g., Z in 2D top-down)
        public float rockAmplitudeDeg = 2.0f; // gentle roll
        public float rockFrequencyHz = 0.20f; // 0.2 cycles/sec

        // Vertical (Y) offset units; scale in view if needed
        public float bobAmplitude = 0.06f;
        public float bobFrequencyHz = 0.33f;

        // Outputs (read by view)
        public float RockAngleDeg { get; private set; } // roll angle in degrees
        public float BobOffset { get; private set; } // vertical offset

        [Header("View Smoothing (optional)")] [Tooltip("Higher values = snappier response. 0 to disable smoothing.")]
        public float rotationLerp = 12f;

        [Tooltip("Higher values = snappier response. 0 to disable smoothing.")]
        public float bobLerp = 12f;

        private float _rockTime;
        private float _bobTime;
        private float _rockPhase;
        private float _bobPhase;

        // Base grid pose (we add rocking/bobbing on top of this)
        private Vector3 _basePos;
        private Quaternion _baseRot;
        private Vector3 _defaultStateLocalPos;
        private ShipMovement _shipMovement;

        private void Awake()
        {
            EnsureComponents();
            InitializeRocking();
        }

        private void Start()
        {
            _baseRot = defaultState.transform.localRotation;
            _basePos = defaultState.transform.localPosition;
            _defaultStateLocalPos = defaultState.transform.localPosition;

            if (IsPlayer)
                _shipMovement = gameObject.AddComponent<ShipMovement>();
        }

        private void EnsureComponents()
        {
            if (_shipHealth == null) _shipHealth = GetComponent<ShipHealth>();
            if (_collider == null) _collider = GetComponentInChildren<Collider>(true);
        }

        public void Init(BoardView boardView, ShipModel model, bool isPlayer)
        {
            Board = boardView;
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

        void Update()
        {
            UpdateRock(Time.deltaTime);
            ApplyRockPose(Time.deltaTime);
        }

        /// <summary>Seed phases & subtle per-ship variation. Call once after creating the model.</summary>
        public void InitializeRocking()
        {
            int s = (shipModel.id?.GetHashCode() ?? 0) ^ ((int)shipModel.type * 73856093) ^
                    (shipModel.length * 19349663);
            var rng = new System.Random(s);

            _rockPhase = (float)rng.NextDouble() * (float)(Math.PI * 2.0);
            _bobPhase = (float)rng.NextDouble() * (float)(Math.PI * 2.0);

            // Tiny amplitude jitter so ships don't look cloned
            float jitterA = 0.90f + (float)rng.NextDouble() * 0.20f; // 0.9–1.1
            float jitterB = 0.90f + (float)rng.NextDouble() * 0.20f;

            rockAmplitudeDeg *= jitterA;
            bobAmplitude *= jitterB;
        }

        public void UpdateRock(float deltaTime)
        {
            if (!rockingEnabled)
            {
                RockAngleDeg = 0f;
                BobOffset = 0f;
                return;
            }

            if (deltaTime <= 0f) return;
            //Debug.Log("ROCKING!");
            _rockTime += deltaTime;
            _bobTime += deltaTime;

            const float TwoPi = (float)(Math.PI * 2.0);

            // Primary + subtle secondary component for natural motion
            float rockPrimary = (float)Math.Sin(TwoPi * rockFrequencyHz * _rockTime + _rockPhase);
            float rockSecondary =
                0.35f * (float)Math.Sin(TwoPi * (rockFrequencyHz * 1.9f) * _rockTime + _rockPhase * 0.37f);
            RockAngleDeg = rockAmplitudeDeg * (rockPrimary + rockSecondary);

            float bobPrimary = (float)Math.Sin(TwoPi * bobFrequencyHz * _bobTime + _bobPhase);
            float bobSecondary = 0.25f * (float)Math.Sin(TwoPi * (bobFrequencyHz * 1.6f) * _bobTime + _bobPhase * 1.7f);
            BobOffset = bobAmplitude * (bobPrimary + bobSecondary);
        }

        /// <summary>Apply the rocking/bobbing to transform on top of the cached grid pose.</summary>
        private void ApplyRockPose(float dt)
        {
            // Target pose layered on base (grid) pose
            Quaternion targetRot =
                _baseRot * Quaternion.AngleAxis(RockAngleDeg, Vector3.forward); // roll around local Z
            Vector3 targetPos = _basePos + new Vector3(0f, BobOffset, 0f); // bob along world Y

            if (rotationLerp > 0f)
            {
                float k = 1f - Mathf.Exp(-rotationLerp * dt);
                defaultState.transform.localRotation =
                    Quaternion.Slerp(defaultState.transform.localRotation, targetRot, k);
            }
            else
            {
                defaultState.transform.localRotation = targetRot;
            }

            if (bobLerp > 0f)
            {
                float k = 1f - Mathf.Exp(-bobLerp * dt);
                defaultState.transform.localPosition = Vector3.Lerp(defaultState.transform.localPosition, targetPos, k);
            }
            else
            {
                defaultState.transform.localPosition = targetPos;
            }
        }


        public IEnumerator AttackSequence(BoardView enemyBoard)
        {
            var coords = shipModel.GetAttackCoordinates(enemyBoard, Board.IsLastShip);
            foreach (var gridPos in coords)
            {
                if (enemyBoard.Model.TryFire(gridPos, out bool hit))
                {
                    bool ignoreSound = false;
                    if (shipModel.type == ShipType.Submarine && !IsPlayer && !hit)
                    {
                        ignoreSound = true;
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
                            if (shipModel.type == ShipType.Destroyer && Board.IsLastShip)
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
                    else if (!ignoreSound)
                    {
                        VFXManager.Instance.PlayFireSound();
                    }
                }


                yield return _waitForSeconds0_1;
            }

            SetPosition(); // reset position
            UpdateSubPosition();
        }

        public bool ApplyDamage(int damage)
        {
            bool isSunk = shipModel.ApplyDamage(damage);
            if (IsPlayer)
                _shipHealth.UpdateHealthBar();
            else if (!IsPlayer && isSunk)
            {
                _shipHealth.EnableDestroyedState(true);
            }

            return isSunk;
        }


        public bool UpdatePosition(GridPos newPos, Orientation newOrientation, bool showCells = true)
        {
            if (shipModel.root.x < 0 || shipModel.root.y < 0)
            {
                OnBeforeShipPlacedOnGrid?.Invoke(this);
                SnapShipOnGrid(newPos, newOrientation, showCells);
                return true;
            }


            Board.Model.ResetShipCells(shipModel);
            if (showCells)
                Board.Tint(shipModel.GetCells());

            if (_shipMovement != null)
            {
               bool success = _shipMovement.TryToStartMovement(newPos, newOrientation);
                UpdateSubPosition();

                return success;
            }


            SnapShipOnGrid(newPos, newOrientation, showCells);
            return true;
        }

        private void UpdateSubPosition()
        {
            if (shipModel.type == ShipType.Submarine && IsPlayer)
            {
                _basePos = shipModel.submerged
                    ? _defaultStateLocalPos - new Vector3(0f, 0.5f, 0f)
                    : _defaultStateLocalPos;
                defaultState.transform.DOLocalMove(_basePos, 0.5f).SetEase(Ease.InSine);

            }
        }

        public void SnapShipOnGrid(GridPos newPos, Orientation newOrientation, bool showCells = false)
        {
            shipModel.orientation = newOrientation;
            shipModel.root = newPos;
            Board.Model.TryPlaceShip(shipModel);
            if (showCells)
                Board.Tint(shipModel.GetCells());

            SetPosition();
        }

        private void SetPosition()
        {
            float yAngle = shipModel.orientation switch
            {
                Orientation.North => 0,
                Orientation.East => 90,
                Orientation.South => 180,
                Orientation.West => -90,
                _ => 0
            };


            transform.position = Board.GridToWorld(shipModel.root);
            transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
        }

        public void SelectShip()
        {
            EnsureComponents();
            if (_collider) _collider.enabled = false;
            else Debug.LogWarning("ShipView.SelectShip(): No Collider found on ship instance.");
        }


        public void DeselectShip()
        {
            EnsureComponents();
            if (_collider) _collider.enabled = true;
            else Debug.LogWarning("ShipView.DeselectShip(): No Collider found on ship instance.");
        }

        public bool RotateLeft()
        {
            var targetOrientation = shipModel.RotateLeft();

            if (ValidateRotation(targetOrientation))
            {
                UpdatePosition(shipModel.root, targetOrientation);

                if (GameManager.instance.phaseState != GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
                    shipModel.movementPattern.hasAlreadyRotated = true;

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

                if (GameManager.instance.phaseState != GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
                    shipModel.movementPattern.hasAlreadyRotated = true;

                return true;
            }

            return false;
        }

        private bool ValidateRotation(Orientation orientation)
        {
            if (!shipModel.canRotate)
                return false;

            var shipModelCopy = shipModel.Copy();
            shipModelCopy.orientation = orientation;
            return Board.Model.ValidateShipPlacement(shipModelCopy, new List<GridPos> { shipModelCopy.root });
        }


        public void SetShipOnGrid(bool b)
        {
            IsInInitialPhase = b;
        }
    }
}