using Unity.VisualScripting;

namespace Core.GridSystem
{
    public enum CellState { Empty, Ship, Hit, Miss }
    public enum Orientation { North, East , South, West }
    
    public enum BoardSide { Player, Enemy }

    [System.Serializable]
    public struct GridPos
    {
        public int x, y;
        public GridPos(int x, int y) { this.x = x; this.y = y; }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }
}