using UnityEngine;

// Use PascalCase for class names as per the design doc
public class SubmarineController : MonoBehaviour
{
    private GridManager gridManager;

    // Public method to be called when the submarine should fire.
    // The direction would be determined by the submarine's orientation.
    // Need to call the FireTorpedo method from a different script that handles the "every other turn"
    public void FireTorpedo(Vector3 fireDirection)
    {
        // Get the starting position for the raycast.
        Vector3 startPosition = transform.position;

        // The maximum distance the ray can travel.
        float maxDistance = gridManager.GetGridSize();

        // This will find the first object with a collider in its path.
        // Need to set up appropriate layers for enemy ships.
        RaycastHit2D hit = Physics2D.Raycast(startPosition, fireDirection, maxDistance, LayerMask.GetMask("EnemyShip"));

        // Check if the raycast hit an enemy ship.Call the FireTorpedo method from a different script that handles the "every other turn"
        if (hit.collider != null)
        {
            // If a hit is detected, log the hit point.
            // TODO Add hit logic here (e.g., deal damage, play an explosion effect).
            Debug.Log("Torpedo hit an enemy ship at: " + hit.point);
            // Get a reference to the enemy ship component.
            // Example: EnemyShip enemy = hit.collider.GetComponent<EnemyShip>();
        }
        else
        {
            // If no hit is detected, the torpedo passes through the entire row/column.
            Debug.Log("Torpedo missed all ships in the line.");
            // TODO Add "miss" logic here (e.g., play a splash effect at the end of the line).
        }
    }
}