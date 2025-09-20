//Torpedo
using UnityEngine;
using Core.Board;
using Core.GridSystem;
using System.Collections;
using DG.Tweening;

public class TorpedoVisual : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    public float diveDepth = 10f;
    public float diveDuration = 1f;

    private Transform _playerBoardTransform;
    private BoardView _enemyBoard;
    private GridPos _targetGridPos;
    private bool _isHit;
    private bool _isMovingToEnemyBoard = false;
    private Vector3 _startPos;

    public void Init(BoardView enemyBoard, Transform playerBoardTransform, GridPos targetGridPos, bool isHit)
    {
        _enemyBoard = enemyBoard;
        _playerBoardTransform = playerBoardTransform;
        _targetGridPos = targetGridPos;
        _isHit = isHit;
        _startPos = transform.position;
    }

    void Update()
    {
        if (!_isMovingToEnemyBoard)
        {
            // Calculate the total distance the torpedo should travel on the player's board.
            // A simple approximation is the distance to the edge of the board.
            // We use the player's board's transform and a point at its far edge.
            Vector3 playerBoardEndPos = _playerBoardTransform.position + _playerBoardTransform.forward * 10f; // Assuming a 10x10 grid size
            float distanceToDive = Vector3.Distance(_startPos, playerBoardEndPos);

            // Move the torpedo
            transform.position += transform.forward * speed * Time.deltaTime;

            // Check if the torpedo has reached the edge
            if (Vector3.Distance(_startPos, transform.position) >= distanceToDive)
            {
                _isMovingToEnemyBoard = true;
                StartCoroutine(TransitionToEnemyBoard());
            }
        }
    }

    private IEnumerator TransitionToEnemyBoard()
    {
        // Dive down
        Vector3 diveTarget = transform.position - new Vector3(0, diveDepth, 0);
        yield return transform.DOMove(diveTarget, diveDuration).WaitForCompletion();

        // Reappear at the starting edge of the enemy board
        // We'll create a starting point on the enemy board that is aligned with the torpedo's X-axis
        Vector3 enemyBoardEntryPos = _enemyBoard.GridToWorld(new GridPos(_targetGridPos.x, -5)); // Assuming a vertical orientation and 10x10 board
        transform.position = enemyBoardEntryPos;
        transform.rotation = Quaternion.LookRotation(_enemyBoard.GridToWorld(_targetGridPos) - transform.position);

        // Calculate final destination
        Vector3 finalTargetPos = _enemyBoard.GridToWorld(_targetGridPos, 0.5f);

        if (!_isHit)
        {
            // If it's a miss, extend the journey past the target.
            Vector3 farEdgePos = _enemyBoard.GridToWorld(new GridPos(_targetGridPos.x, 15)); // Extend past the board
            finalTargetPos = farEdgePos;
        }

        // Move to final destination and destroy
        transform.DOMove(finalTargetPos, Vector3.Distance(transform.position, finalTargetPos) / speed)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}