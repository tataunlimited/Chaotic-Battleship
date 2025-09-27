using System.Collections.Generic;
using Core.GridSystem;
using Core.Ship;

namespace Core.Board
{
    public class BoardModel
    {
        public readonly BoardSide Side;
        public readonly int Width;
        public readonly int Height;

        private readonly Dictionary<GridPos, int> ScorchedCells = new();



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
                for (int y = 0; y < Height; y++)
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

        public CellState Get(GridPos p)
        {
            if (!InBounds(p))
                return CellState.Empty;           
            return _cells[p.x, p.y];
        }

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

        public bool TryScorchCell(GridPos p, int lifeTime)
        {
            if (!InBounds(p)) return false;
            if (_cells[p.x, p.y] == CellState.Ship) return false;
            ScorchedCells[p] = lifeTime;
            return true;

        }

        public void UpdateScorchedCells()
        {
            List<GridPos> keys = new List<GridPos>(ScorchedCells.Keys);
            List<GridPos> toRemove = new List<GridPos>();

            foreach (GridPos key in keys)
            {
                ScorchedCells[key]--; 
                if (ScorchedCells[key] <= 0)
                {
                    toRemove.Add(key);
                }
            }

            foreach (GridPos key in toRemove)
            {
                ScorchedCells.Remove(key);
            }
        }
        public bool ValidateShipPlacement(ShipModel shipModel, List<GridPos> positionsToIgnore = null)
        {
            foreach (var c in shipModel.GetCells())
            {
                if (positionsToIgnore != null && positionsToIgnore.Contains(c))
                {
                    continue;
                }

                if (!InBounds(c) || ScorchedCells.ContainsKey(c) || (_cells[c.x, c.y] != CellState.Empty && _cells[c.x, c.y] != CellState.Miss && _cells[c.x, c.y] != CellState.NearMiss))
                    return false;
            }
            return true;
        }

        public void ResetShipCells(ShipModel shipModel)
        {
            foreach (var c in shipModel.GetCells())
            {
                if(InBounds(c))
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

        public bool TryFire(GridPos p, out bool hit, bool onlyUpdateHit = false)
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
                case CellState.Miss:
                case CellState.NearMiss:
                    if (onlyUpdateHit) return false;
                    Set(p, IsOrthogonallyAdjacentToShip(p) ? CellState.NearMiss : CellState.Miss);
                    return true;
                case CellState.Hit:
                    hit = true;
                    return true;

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

        public bool IsScorched(GridPos gridPos)
        {
            return ScorchedCells.ContainsKey(gridPos);
        }
    }
}