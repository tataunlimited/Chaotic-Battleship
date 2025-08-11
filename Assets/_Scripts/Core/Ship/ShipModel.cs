using System;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;

namespace Core.Ship
{
    public enum ShipType
    {
        Destroyer,
        Battleship,
        Submarine
    }
    [System.Serializable]
    public class ShipModel
    {
        public string id;
        public ShipType type;
        public int length = 3;
        public Orientation orientation = Orientation.Horizontal;
        public GridPos root;               // starting cell (leftmost/topmost)
        public List<GridPos> occupied = new List<GridPos>();

        public IEnumerable<GridPos> EnumerateCells()
        {
            for (int i = 0; i < length; i++)
            {
                yield return orientation == Orientation.Horizontal
                    ? new GridPos(root.x + i, root.y)
                    : new GridPos(root.x, root.y + i);
            }
        }

        internal List<GridPos> GetAttackCoordinates(BoardView boardView)
        {
            

            List < GridPos > coords = new List<GridPos>();
            switch (type)
            {
                case ShipType.Destroyer:
                    var bowPosition = orientation == Orientation.Horizontal
                    ? new GridPos(root.x + length, root.y)
                    : new GridPos(root.x, root.y + length);
                    coords.Add(bowPosition);
                    break;
                case ShipType.Battleship:
                    coords.AddRange(boardView.GetRandomPositions(4));
                    break;
                case ShipType.Submarine:
                    coords = orientation == Orientation.Horizontal
                    ? boardView.GetRow(root.y): boardView.GetColumn(root.x);
                    break;

            }
            return coords;
        }
    }

    public static class ShipDatabase
    {
        public static readonly Dictionary<ShipType, ShipModel> DefaultShips = new Dictionary<ShipType, ShipModel>()
        {

            { ShipType.Battleship, new ShipModel { id = "battleship", type = ShipType.Battleship, length = 4 } },
            { ShipType.Submarine, new ShipModel { id = "submarine", type = ShipType.Submarine, length = 3 } },
            { ShipType.Destroyer, new ShipModel { id = "destroyer", type = ShipType.Destroyer, length = 2 } }
        };
    }
}