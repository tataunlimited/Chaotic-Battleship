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
        public Orientation orientation = Orientation.North;
        public GridPos root;               // bow (front) position   
        public bool isDestroyed = false;

        public List<GridPos> GetCells()
        {
            var cells = new List<GridPos>();
            for (int i = 0; i < length; i++)
            {
                cells.Add( orientation switch
                {
                    Orientation.North => new GridPos(root.x, root.y - i),
                    Orientation.East => new GridPos(root.x - i, root.y),
                    Orientation.South => new GridPos(root.x, root.y + i),
                    Orientation.West => new GridPos(root.x + i, root.y),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            return cells;
        }
        
        internal List<GridPos> GetAttackCoordinates(BoardView boardView)
        {
            

            List < GridPos > coords = new List<GridPos>();
            switch (type)
            {
                case ShipType.Destroyer:
                    coords.Add(root);
                    break;
                case ShipType.Battleship:
                    coords.AddRange(boardView.GetRandomPositions(4));
                    break;
                case ShipType.Submarine:
                    coords = orientation is Orientation.West or Orientation.East
                    ? boardView.GetRow(root.y, orientation): boardView.GetColumn(root.x, orientation);
                    break;
                case ShipType.Cruiser:
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation));
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
                orientation = orientation
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
            { ShipType.Submarine, new ShipModel { id = "submarine", type = ShipType.Submarine, length = 1} },
            { ShipType.Destroyer, new ShipModel { id = "destroyer", type = ShipType.Destroyer, length = 2 } },
            { ShipType.Cruiser, new ShipModel { id = "cruiser", type = ShipType.Cruiser, length = 3 } }
        };
    }
    
    
}