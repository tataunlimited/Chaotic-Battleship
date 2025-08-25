using System.Collections.Generic;
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
        public void Set(GridPos p, CellState s) => _cells[p.x, p.y] = s;
        public BoardModel(BoardSide side, int width, int height)
        {
            Side = side;
            Width = width;
            Height = height;
            _cells = new CellState[width, height];
        }

        public BoardModel Copy()
        {
            BoardModel board = new BoardModel(Side, Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; x < Width; x++)
                {
                    board._cells[x, y] = _cells[x, y];
                }
            }

            return board;
        }

        public void Reset()
        {
            ResetAllCells();
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

        public bool ValidateShipPlacement(ShipModel shipModel, List<GridPos> positionsToIgnore = null)
        {
            foreach (var c in shipModel.GetCells())
            {
                if (positionsToIgnore != null && positionsToIgnore.Contains(c))
                {
                    continue;
                }

                if (!InBounds(c) || (_cells[c.x, c.y] != CellState.Empty && _cells[c.x, c.y] != CellState.Miss))
                    return false;
            }
            return true;
        }

        public void ResetShipCells(ShipModel shipModel)
        {
            foreach (var c in shipModel.GetCells())
            {
                _cells[c.x, c.y] = CellState.Empty;
            }
        }

        public void ResetAllCells()
        {
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j] = CellState.Empty;
                }
            }
        }

        public bool TryFire(GridPos p, out bool hit)
        {
            hit = false;
            if (!InBounds(p)) return false;                    // invalid shot

            var state = Get(p);
            switch (state)
            {
                case CellState.Ship:
                    Set(p, CellState.Hit);
                    hit = true;
                    return true;

                case CellState.Empty:
                    Set(p, IsOrthogonallyAdjacentToShip(p) ? CellState.NearMiss : CellState.Miss);
                    return true;

                case CellState.Miss:
                case CellState.NearMiss:
                case CellState.Hit:
                    // already resolved; don’t re-tint / re-animate
                    return false;

                default:
                    return false;
            }
        }

        private bool IsOrthogonallyAdjacentToShip(GridPos p)
        {
            // up, down, left, right only (no diagonals)
            var neighbors = new[]{
                new GridPos(p.x,     p.y+1),
                new GridPos(p.x,     p.y-1),
                new GridPos(p.x-1,   p.y),
                new GridPos(p.x+1,   p.y),
            };

            foreach (var n in neighbors)
            {
                if (InBounds(n) && Get(n) == CellState.Ship)
                    return true;
            }
            return false;
        }
    }
}