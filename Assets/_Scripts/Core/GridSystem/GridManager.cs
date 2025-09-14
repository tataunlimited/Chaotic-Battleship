using UnityEngine;
using System.Collections.Generic;

namespace Core.GridSystem
{
    public struct GridPos
    {
        public int x;
        public int y;

        public GridPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum CellType
    {
        Empty,
        Ship
    }

    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        // The generic type `CellType`.
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

        public bool InBounds(GridPos p)
        {
            return grid.InBounds(p);
        }

        public CellType GetCellType(GridPos p)
        {
            return grid.Get(p);
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
}