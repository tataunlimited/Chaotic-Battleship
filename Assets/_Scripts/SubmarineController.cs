using UnityEngine;
using Core.GridSystem;

public class SubmarineController : MonoBehaviour
{
    // A reference to your game's grid manager.
    // Assign this in the Unity Inspector.
    public GridManager gridManager;

    // A public method to be called when the submarine should fire.
    // The direction is a grid direction, e.g., (1, 0) for right.
    public void FireTorpedo(GridPos fireDirection)
    {
        // Get the submarine's current position on the grid.
        if (!gridManager.WorldToGrid(transform.position, out GridPos currentGridPos))
        {
            Debug.LogError("Submarine is not on the grid!");
            return;
        }

        // Loop through the grid cells in the specified direction.
        while (gridManager.InBounds(currentGridPos))
        {
            // Get the type of the current cell.
            CellType cellContent = gridManager.GetCellType(currentGridPos);

            if (cellContent == CellType.Ship)
            {
                // We found a ship!
                Debug.Log($"Torpedo hit an enemy ship at grid position: ({currentGridPos.x}, {currentGridPos.y})");

                // TODO Add hit logic here:
                // - Deal damage to the ship at this position.
                // - Play an explosion effect.
                // - Stop the torpedo animation/simulation.
                return; // Stop the loop and exit the method on the first hit.
            }

            // Move to the next cell in the firing direction.
            currentGridPos.x += fireDirection.x;
            currentGridPos.y += fireDirection.y;
        }

        // If the loop completes without finding a ship, it's a miss.
        Debug.Log("Torpedo missed all ships in the line.");
        // TODO Add "miss" logic here.
    }
}