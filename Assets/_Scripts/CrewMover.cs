using UnityEngine;

public class CrewMover : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float moveSpeed = 1.0f;
    public float rotationSpeed = 5.0f;
    public float arrivalDistance = 0.1f;

    private Transform[] targetPoints;
    private Vector3 currentGoalLocalPos;
    private int currentTargetIndex = -1;

    /// <summary>
    /// Called by ShipCrewManager to initialize the target points.
    /// </summary>
    public void SetTargets(Transform[] targets)
    {
        targetPoints = targets;
        PickNewTarget();
    }

    void Update()
    {
        if (targetPoints == null || targetPoints.Length == 0)
        {
            return;
        }

        // Move towards the target's local position (ensures no hovering)
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            currentGoalLocalPos,
            moveSpeed * Time.deltaTime);

        // Check if we have arrived
        if (Vector3.Distance(transform.localPosition, currentGoalLocalPos) < arrivalDistance)
        {
            PickNewTarget();
        }

        // Look in the direction of travel
        LookAtGoal();
    }

    void PickNewTarget()
    {
        if (targetPoints.Length > 0)
        {
            // Pick a random target index that is not the current one
            int newTargetIndex = currentTargetIndex;
            while (newTargetIndex == currentTargetIndex)
            {
                newTargetIndex = Random.Range(0, targetPoints.Length);
            }

            currentTargetIndex = newTargetIndex;
            currentGoalLocalPos = targetPoints[currentTargetIndex].localPosition;
        }
    }

    void LookAtGoal()
    {
        // Calculate the direction vector in local space
        Vector3 localDirection = (currentGoalLocalPos - transform.localPosition).normalized;

        // Ensure we don't try to look at a zero vector
        if (localDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(localDirection);

            // Apply the rotation smoothly
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}