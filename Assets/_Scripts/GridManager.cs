using UnityEngine;
using Core.GridSystem;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

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

        grid = new Grid2D<CellType>(10, 10, 1.0f, Vector3.zero, CellType.Empty);
    }

    public bool WorldToGrid(Vector3 worldPosition, out GridPos gridPos)
    {
        return grid.WorldToGrid(worldPosition, out gridPos);
    }

    public void PlaceShip(GridPos startPos)
    {
        if (grid.InBounds(startPos))
        {
            grid.Set(startPos, CellType.Ship);
        }
    }
}