using System.Collections.Generic;
using Core.GridSystem;

namespace Core.Ship
{
    [System.Serializable]
    public class ShipModel
    {
        public string id;
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
    }
}