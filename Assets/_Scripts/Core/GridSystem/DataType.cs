
using System;

namespace Core.GridSystem
{
    public enum CellState { Empty, Ship, Hit, Miss }
    public enum Orientation { North, East , South, West }
    
    public enum BoardSide { Player, Enemy }

    [System.Serializable]
    public struct GridPos : IEquatable<GridPos>
    {
        public int x, y;
        public GridPos(int x, int y) { this.x = x; this.y = y; }

        public bool Equals(GridPos other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);


        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }
}