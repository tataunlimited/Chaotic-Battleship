using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using Core.Pathfinding;
using DG.Tweening;
using UnityEngine;

namespace Core.Ship
{
    /// <summary>
    /// This component goes on your Ship prefab. It handles the physical movement
    /// and rotation of the ship along a path provided by the Pathfinder.
    /// </summary>
    [RequireComponent(typeof(ShipView))]
    public class ShipMovement : MonoBehaviour
    {
        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 6.5f;

        [SerializeField] private float rotationSpeed = 180f;

        [Header("Submarine Settings")] [SerializeField]
        private float submergeDepth = -1.5f; // How far below the surface to go
        [SerializeField] private float submergeDuration = .5f;
        
        [Header("Collision Settings")]
        [SerializeField] private float collisionPushForce = 0.5f; // How far the other ship gets pushed
        [SerializeField] private float collisionPushDuration = 0.6f;

        private bool _isMoving = false;

        private ShipView _shipView;
        private static Coroutine _movementCoroutine;
        private bool _isBeingPushed = false; // Prevents a ship from being pushed multiple times at once

        public Vector3 GetOriginalPosition() => _shipView.Board.GridToWorld(_shipView.shipModel.root);

        private void Awake()
        {
            _shipView = GetComponent<ShipView>();
        }

        public void StartMove(GridPos targetPosition, Orientation finalOrientation)
        {
            if (_shipView == null || _shipView.Board == null)
            {
                Debug.LogError("ShipView or Board not initialized!");
                return;
            }


            // A ship cannot move to its own current location with the same orientation.
            if (targetPosition.Equals(_shipView.shipModel.root) &&
                finalOrientation == _shipView.shipModel.orientation) return;

            var path = PathfinderController.Instance.FindPathForShip(
                _shipView.Board,
                _shipView.shipModel,
                _shipView.shipModel.root,
                targetPosition
            );
            if (path == null) path = new List<GridPos>();

            if (_movementCoroutine != null)
            {
                //StartCoroutine(WaitIfBusy(path, finalOrientation));
                return;
            }


            _movementCoroutine = StartCoroutine(FollowPathCoroutine(path, finalOrientation));
        }

        private IEnumerator WaitIfBusy(List<GridPos> path, Orientation finalOrientation)
        {
            yield return new WaitUntil(() => _movementCoroutine == null);
            StartCoroutine(FollowPathCoroutine(path, finalOrientation));
        }

        // In ShipMovement.cs, replace the entire method with this one

        private IEnumerator FollowPathCoroutine(List<GridPos> path, Orientation finalOrientation)
        {
            Debug.Log($"Starting movement along path with {path.Count} steps.");
            BoardModel boardModel = _shipView.Board.Model;
            _isMoving = true;
            boardModel.ResetShipCells(_shipView.shipModel);

            // --- New Submarine Logic ---
            // Check if this ship is a submarine and store its starting height
            bool isSubmarine = _shipView.shipModel.type == ShipType.Submarine; // Assumes ShipType enum
            float originalY = transform.position.y;
            float submergedY = originalY + submergeDepth;

            // 1. SUBMERGE (if submarine)
            if (isSubmarine)
            {
                Debug.Log("Submarine submerging...");
                // Using DOTween for a smooth animation
                yield return transform.DOMoveY(submergedY, submergeDuration).SetEase(Ease.InOutSine)
                    .WaitForCompletion();
            }

            GridPos currentGridPos = _shipView.shipModel.root;

            foreach (var nextNode in path)
            {
                Vector3 moveDirection = (_shipView.Board.GridToWorld(nextNode) - transform.position).normalized;

                if (moveDirection == Vector3.zero) continue;

                float forwardAngle = Vector3.Angle(transform.forward, moveDirection);
                float backwardAngle = Vector3.Angle(transform.forward, -moveDirection);

                Quaternion targetRotation;
                Vector3 facingDirection;

                if (backwardAngle < forwardAngle)
                {
                    facingDirection = -moveDirection;
                    targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);
                }
                else
                {
                    facingDirection = moveDirection;
                    targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);
                }

                // 2. Rotation
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1.0f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                    yield return null;
                }

                transform.rotation = targetRotation;
                _shipView.shipModel.orientation = DirectionToOrientation(facingDirection);

                // 3. Movement
                Vector3 targetWorldPos = _shipView.Board.GridToWorld(nextNode);

                // ** CRITICAL: Keep the submarine underwater during movement **
                if (isSubmarine)
                {
                    targetWorldPos.y = submergedY;
                }

                while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
                {
                    transform.position =
                        Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                transform.position = targetWorldPos;
                currentGridPos = nextNode;
            }

            // 4. Final Rotation
            Quaternion finalTargetRotation = OrientationToRotation(finalOrientation);
            while (Quaternion.Angle(transform.rotation, finalTargetRotation) > 1.0f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, finalTargetRotation,
                    rotationSpeed * Time.deltaTime);
                yield return null;
            }

            transform.rotation = finalTargetRotation;

            // 5. RESURFACE (if submarine)
            if (isSubmarine)
            {
                Debug.Log("Submarine resurfacing...");
                yield return transform.DOMoveY(originalY, submergeDuration).SetEase(Ease.InOutSine).WaitForCompletion();
            }

            // 6. Finalize State
            _shipView.shipModel.root = currentGridPos;
            _shipView.shipModel.orientation = finalOrientation;

            boardModel.TryPlaceShip(_shipView.shipModel);
            _shipView.Board.UpdateBoard();

            Debug.Log("Movement finished.");
            _movementCoroutine = null;
            _isMoving = false;
        }

        /// <summary>
        /// Converts a world-space direction vector to a grid Orientation.
        /// </summary>
        private Orientation DirectionToOrientation(Vector3 direction)
        {
            // Normalize to be safe
            direction.Normalize();

            if (direction.z > 0.5f) // North
            {
                if (direction.x > 0.5f) return Orientation.NorthEast;
                if (direction.x < -0.5f) return Orientation.NorthWest;
                return Orientation.North;
            }

            if (direction.z < -0.5f) // South
            {
                if (direction.x > 0.5f) return Orientation.SouthEast;
                if (direction.x < -0.5f) return Orientation.SouthWest;
                return Orientation.South;
            }

            // Must be East or West
            if (direction.x > 0.5f) return Orientation.East;
            if (direction.x < -0.5f) return Orientation.West;

            // Fallback
            return _shipView.shipModel.orientation;
        }

        /// <summary>
        /// Converts a grid Orientation enum to a world-space rotation Quaternion.
        /// </summary>
        private Quaternion OrientationToRotation(Orientation orientation)
        {
            float yAngle = orientation switch
            {
                Orientation.North => 0,
                Orientation.NorthEast => 45, // Add this
                Orientation.East => 90,
                Orientation.SouthEast => 135, // Add this
                Orientation.South => 180,
                Orientation.SouthWest => 225, // Add this
                Orientation.West => 270,
                Orientation.NorthWest => 315, // Add this
                _ => 0
            };
            return Quaternion.Euler(0f, yAngle, 0f);
        }

        void OnTriggerEnter(Collider other)
        {
            // A ship should only initiate a push while it is the one actively moving/rotating.
            if (!_isMoving) return;

            var otherMovement = other.gameObject.GetComponentInParent<ShipMovement>();

            // Check if we collided with another ship that isn't itself and isn't already being pushed.
            if (otherMovement != null && otherMovement != this && !otherMovement._isBeingPushed)
            {
                // 1. Calculate the direction to push the other ship.
                // We get the vector from our center to their center.
                Vector3 pushDirection = other.transform.position - transform.position;
                pushDirection.y = 0; // We only want to push them on the horizontal plane.
                pushDirection.Normalize();

                // 2. Set a flag on the other ship so it doesn't get pushed by multiple things at once.
                otherMovement._isBeingPushed = true;

                // 3. Use DOPunchPosition for a natural effect.
                // This single command handles the push and the return automatically.
                otherMovement.transform.DOPunchPosition(
                        punch: pushDirection * collisionPushForce, // The direction and strength of the punch
                        duration: collisionPushDuration,
                        vibrato: 0,       // How many times to vibrate; 0 is a smooth punch
                        elasticity: 0.1f) // How much the ship will "bounce" back; 0 to 1
                    .OnComplete(() =>
                    {
                        // When the animation is done, reset the flag on the other ship.
                        otherMovement._isBeingPushed = false;
                    });
            }
        }
    }
}