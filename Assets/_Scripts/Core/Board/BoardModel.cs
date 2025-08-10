using Core.GridSystem;

namespace Core.Board
{
    public class BoardModel
    {
        public readonly BoardSide Side;
        public readonly int Width;
        public readonly int Height;
        
        private readonly CellState[,] _cells;

        public BoardModel(BoardSide side, int width, int height)
        {
            Side   = side;
            Width  = width;
            Height = height;
            _cells = new CellState[width, height];
        }

        public bool InBounds(GridPos p) =>
            p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;

        public CellState Get(GridPos p) => _cells[p.x, p.y];

        public bool TryPlaceShip(GridPos root, int length, Orientation o)
        {
            // validate
            if (!ValidateShipPlacement(root, length, o)) return false;
            // commit
            for (int i = 0; i < length; i++)
            {
                var p = o == Orientation.Horizontal ? new GridPos(root.x + i, root.y)
                    : new GridPos(root.x, root.y + i);
                _cells[p.x, p.y] = CellState.Ship;
            }
            return true;
        }

        public bool ValidateShipPlacement(GridPos root, int length, Orientation o)
        {
            for (int i = 0; i < length; i++)
            {
                var p = o == Orientation.Horizontal ? new GridPos(root.x + i, root.y)
                    : new GridPos(root.x, root.y + i);
                if (!InBounds(p) || Get(p) != CellState.Empty) return false;
            }

            return true;
        }

        public bool TryFire(GridPos p, out bool hit)
        {
            hit = false;
            if (!InBounds(p)) return false;

            var s = _cells[p.x, p.y];
            if (s == CellState.Hit || s == CellState.Miss) return false; // already fired

            if (s == CellState.Ship) { _cells[p.x, p.y] = CellState.Hit; hit = true; }
            else                     { _cells[p.x, p.y] = CellState.Miss; }
            return true;
        }
    }
}