using System;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;

namespace Core.Ship
{
    public enum ShipType
    {
        Destroyer = 1,
        Battleship = 2,
        Cruiser = 3,
        Submarine = 4
    }

    [System.Serializable]
    public class ShipModel
    {
        public string id;
        public ShipType type;
        public int length = 3;

        public int hp;
        public int MaxHP => length;
        public bool IsSunk => isDestroyed || hp <= 0;
        public Orientation orientation = Orientation.North;
        public GridPos root; // bow (front) position   
        public GridPos reserved = new GridPos(-1000,-1000); // bow (front) position   
        public bool isDestroyed = false;
        

        private int _round = 0;

        /// <summary>Apply damage and return true if the ship just sunk.</summary>
        public bool ApplyDamage(int damage = 1)
        {
            if (IsSunk) return false;
            hp = Math.Max(0, hp - damage);
            if (hp == 0)
            {
                isDestroyed = true;
                return true;
            }

            return false;
        }

        public List<GridPos> GetCells()
        {
            return GetCells(root);
        }

        private List<GridPos> GetCells(GridPos rootCell)
        {
            var cells = new List<GridPos>();
            for (int i = 0; i < length; i++)
            {
                cells.Add(orientation switch
                {
                    Orientation.North => new GridPos(rootCell.x, rootCell.y - i),
                    Orientation.East => new GridPos(rootCell.x - i, rootCell.y),
                    Orientation.South => new GridPos(rootCell.x, rootCell.y + i),
                    Orientation.West => new GridPos(rootCell.x + i, rootCell.y),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }

            return cells;
        }
        public void ResetHP()
        {
            hp = length;
            isDestroyed = false;
        }

        internal List<GridPos> GetAttackCoordinates(BoardView boardView, bool isSpecialAttack)
        {
            List<GridPos> coords = new List<GridPos>();
            switch (type)
            {
                case ShipType.Destroyer:
                    if (reserved.x < 0 || reserved.y < 0)
                    {
                        reserved = root;
                    }
                    coords.Add(reserved);
                    break;
                case ShipType.Battleship:
                    int count = isSpecialAttack ? 12 : 4;
                    coords.AddRange(boardView.GetRandomPositions(count));
                    break;
                case ShipType.Submarine:
                {
                    if (_round % 2 == 0 || isSpecialAttack)
                    {
                        List<GridPos> line = orientation is Orientation.West or Orientation.East
                            ? boardView.GetRow(root.y, orientation)
                            : boardView.GetColumn(root.x, orientation);

                        foreach (var pos in line)
                        {
                            coords.Add(pos);
                            // Stop if this grid cell has a ship
                            if (boardView.HasShipAt(pos))
                                break;
                        }
                    }
                    _round++;
                    break;
                }
                case ShipType.Cruiser:
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation));
                    if (isSpecialAttack)
                    {
                        var randomRoots = boardView.GetRandomPositions(2);
                        coords.AddRange(boardView.CruiserAttack(GetCells(randomRoots[0]), orientation));
                        coords.AddRange(boardView.CruiserAttack(GetCells(randomRoots[1]), orientation));
                    }
                    break;
            }

            return coords;
        }

        internal List<GridPos> GetPossibleAreaOfAttack(BoardView boardView, out List<GridPos> selectedCoords,out bool chance)
        {
            List<GridPos> coords = new List<GridPos>();
            selectedCoords = new List<GridPos>();
            chance = false;

            switch (type)
            {
                case ShipType.Destroyer:

                    coords.AddRange(boardView.GetAllPositions());
                    selectedCoords.Add(reserved);
                    chance = true;
                    break;
                case ShipType.Battleship:
                    coords.AddRange(boardView.GetAllPositions());
                    chance = true;
                    break;
                case ShipType.Submarine:
                    coords = orientation is Orientation.West or Orientation.East
                        ? boardView.GetRow(root.y, orientation)
                        : boardView.GetColumn(root.x, orientation);
                    chance = false;
                    break;
                case ShipType.Cruiser:
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation, true));
                    chance = true;
                    break;
            }

            return coords;
        }

        public ShipModel Copy()
        {
            ShipModel copy = new ShipModel
            {
                id = id,
                type = type,
                length = length,
                root = root,
                orientation = orientation,
                hp = hp,
                isDestroyed = isDestroyed,
                _round = _round,
            };
            return copy;
        }

        public bool MoveTowards(Orientation direction, int count = 1)
        {
            root = direction switch
            {
                Orientation.North => new GridPos(root.x, root.y + count),
                Orientation.East => new GridPos(root.x + count, root.y),
                Orientation.South => new GridPos(root.x, root.y - count),
                Orientation.West => new GridPos(root.x - count, root.y),
                _ => throw new ArgumentOutOfRangeException()
            };

            return true;
        }

        public GridPos MoveTo(GridPos point)
        {
            return point;
        }

        public Orientation RotateLeft()
        {
            int orientationNumber = (int)orientation;
            if (orientationNumber == 0)
            {
                orientationNumber = 3;
            }
            else
            {
                orientationNumber--;
            }

            return (Orientation)orientationNumber;
        }

        public Orientation RotateRight()
        {
            int orientationNumber = (int)orientation;
            if (orientationNumber == 3)
            {
                orientationNumber = 0;
            }
            else
            {
                orientationNumber++;
            }

            return (Orientation)orientationNumber;
        }

        public List<GridPos> GetMovablePositions(BoardView playerView)
        {
            ShipMovementPattern pattern = ShipMovementPattern.CreateMovementPattern(type);
            return pattern.GetAllPossibleMovePositions(playerView, this);
        }
    }

    public static class ShipDatabase
    {
        public static readonly Dictionary<ShipType, ShipModel> DefaultShips = new()
        {
            { ShipType.Battleship, new ShipModel { id = "battleship", type = ShipType.Battleship, length = 4 } },
            { ShipType.Submarine, new ShipModel { id = "submarine", type = ShipType.Submarine, length = 1 } },
            { ShipType.Destroyer, new ShipModel { id = "destroyer", type = ShipType.Destroyer, length = 2 } },
            { ShipType.Cruiser, new ShipModel { id = "cruiser", type = ShipType.Cruiser, length = 3 } }
        };
    }
}