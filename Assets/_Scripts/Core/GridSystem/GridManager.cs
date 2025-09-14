using UnityEngine;

namespace Core.GridSystem
{
    public class GridManager : MonoBehaviour
    {
        // TODO Make this a singleton or find a way to make it easily accessible.
        public static GridManager Instance { get; private set; }

        // The generic type `CellType` is defined above.
        private Grid2D<CellType> grid;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            // Initialize the grid.
            grid = new Grid2D<CellType>(10, 10, 1.0f, Vector3.zero, CellType.Empty);
        }

        // Pass-through method to check bounds using the grid instance.
        public bool InBounds(GridPos p)
        {
            return grid.InBounds(p);
        }

        // Pass-through method to get the cell type at a given position.
        public CellType GetCellType(GridPos p)
        {
            return grid.Get(p);
        }

        // Pass-through method to convert a world position to a grid position.
        public bool WorldToGrid(Vector3 worldPosition, out GridPos gridPos)
        {
            return grid.WorldToGrid(worldPosition, out gridPos);
        }

        // A method to simulate placing a ship on the grid for testing.
        public void PlaceShip(GridPos startPos)
        {
            // For demonstration, place a single-cell ship.
            if (grid.InBounds(startPos))
            {
                grid.Set(startPos, CellType.Ship);
            }
        }
    }
}