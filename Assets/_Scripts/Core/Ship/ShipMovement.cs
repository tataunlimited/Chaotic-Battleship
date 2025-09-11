using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using Core.Pathfinding;
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
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float rotationSpeed = 180f;

        private ShipView _shipView;
        private Coroutine _movementCoroutine;

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
            if (targetPosition.Equals(_shipView.shipModel.root) && finalOrientation == _shipView.shipModel.orientation) return;

            var path = PathfinderController.Instance.FindPathForShip(
                _shipView.Board,
                _shipView.shipModel,
                _shipView.shipModel.root,
                targetPosition
            );

            // Even if there is no path (e.g., rotating in place), we still might need to run the coroutine.
            if (path == null) path = new List<GridPos>();

            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
            }
            _movementCoroutine = StartCoroutine(FollowPathCoroutine(path, finalOrientation));
        }

        private IEnumerator FollowPathCoroutine(List<GridPos> path, Orientation finalOrientation)
        {
            Debug.Log($"Starting movement along path with {path.Count} steps.");
            BoardModel boardModel = _shipView.Board.Model;

            // First, remove the ship from its starting position in the model
            // This frees up the cells so other ships can pathfind around it while it moves.
            boardModel.ResetShipCells(_shipView.shipModel);

            GridPos currentGridPos = _shipView.shipModel.root;

            foreach (var nextNode in path)
            {
                // --- 1. Rotation ---
                Vector3 direction = (_shipView.Board.GridToWorld(nextNode) - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    
                    // This is where you would check if the rotation is blocked.
                    // For simplicity, we'll skip that check, but the logic would be:
                    // while(IsRotationBlocked(targetRotation)) { yield return null; }

                    while (Quaternion.Angle(transform.rotation, targetRotation) > 1.0f)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                        yield return null;
                    }
                    transform.rotation = targetRotation; // Snap to final rotation
                    
                    // UPDATE THE MODEL: Sync the ship model's orientation with the visual rotation
                    _shipView.shipModel.orientation = DirectionToOrientation(direction);
                }


                // --- 2. Movement ---
                Vector3 targetWorldPos = _shipView.Board.GridToWorld(nextNode);
                while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                transform.position = targetWorldPos; // Snap to final position
                currentGridPos = nextNode;
            }
            
            // --- 3. Final Rotation ---
            // After reaching the destination, perform the final rotation to match the desired orientation.
            Quaternion finalTargetRotation = OrientationToRotation(finalOrientation);
            while (Quaternion.Angle(transform.rotation, finalTargetRotation) > 1.0f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, finalTargetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = finalTargetRotation; // Snap to final rotation

            // --- 4. Finalize State ---
            // Update the ship's logical position and orientation in the model
            _shipView.shipModel.root = currentGridPos;
            _shipView.shipModel.orientation = finalOrientation;
            
            // Place the ship back in the model at its final destination.
            boardModel.TryPlaceShip(_shipView.shipModel);
            _shipView.Board.UpdateBoard(); // Visually update the board tints

            Debug.Log("Movement finished.");
            _movementCoroutine = null;
        }

        /// <summary>
        /// Converts a world-space direction vector to a grid Orientation.
        /// </summary>
        private Orientation DirectionToOrientation(Vector3 direction)
        {
            // Use Vector3.Dot to find the dominant axis. This is more robust than checking x/z values.
            float dotForward = Vector3.Dot(direction, Vector3.forward);
            float dotBack = Vector3.Dot(direction, Vector3.back);
            float dotRight = Vector3.Dot(direction, Vector3.right);
            float dotLeft = Vector3.Dot(direction, Vector3.left);

            if (dotForward > 0.9f) return Orientation.North;
            if (dotBack > 0.9f) return Orientation.South;
            if (dotRight > 0.9f) return Orientation.East;
            if (dotLeft > 0.9f) return Orientation.West;
            
            // Fallback in case of a non-cardinal direction (should not happen in grid movement)
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
                Orientation.East => 90,
                Orientation.South => 180,
                Orientation.West => -90, // Or 270
                _ => 0
            };
            return Quaternion.Euler(0f, yAngle, 0f);
        }
    }
}

