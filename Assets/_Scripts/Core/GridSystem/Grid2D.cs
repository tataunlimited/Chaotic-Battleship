using UnityEngine;

namespace Core.GridSystem
{
    public class Grid2D<T>
    {
        public readonly int Width;
        public readonly int Height;
        public readonly float CellSize;
        public readonly Vector3 Origin; // bottom-left (x right, y up, z forward)
        private readonly T[,] _cells;

        public Grid2D(int width, int height, float cellSize, Vector3 origin, T defaultValue = default)
        {
            Width = width; Height = height; CellSize = cellSize; Origin = origin;
            _cells = new T[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _cells[x, y] = defaultValue;
        }

        public bool InBounds(GridPos p) => p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;

        public Vector3 GridToWorld(GridPos p, float yOffset = 0f)
        {
            return Origin + new Vector3((p.x + 0.5f) * CellSize, yOffset, (p.y + 0.5f) * CellSize);
        }

        public bool WorldToGrid(Vector3 world, out GridPos p)
        {
            Vector3 local = world - Origin;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.z / CellSize);
            p = new GridPos(x, y);
            return InBounds(p);
        }

        public T Get(GridPos p) => _cells[p.x, p.y];
        public void Set(GridPos p, T value) => _cells[p.x, p.y] = value;
    }
}