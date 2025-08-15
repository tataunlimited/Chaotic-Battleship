using System.Linq;
using Core.GridSystem;
using Core.Ship;

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

        public bool TryPlaceShip(ShipModel shipModel)
        {
            // validate
            if (!ValidateShipPlacement(shipModel)) return false;
            // commit
            foreach (var c in shipModel.GetCells())
            {
                _cells[c.x, c.y] = CellState.Ship;
            }

            return true;
        }

        public bool ValidateShipPlacement(ShipModel shipModel)
        {
            return shipModel.GetCells().All(cell => InBounds(cell) && Get(cell) == CellState.Empty);
        }

        public void ResetShipCells(ShipModel shipModel)
        {
            foreach (var c in shipModel.GetCells())
            {
                _cells[c.x, c.y] = CellState.Empty;
            }
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