namespace Core.GridSystem
{
    public enum CellState { Empty, Ship, Hit, Miss }
    public enum Orientation { Horizontal, Vertical }
    
    public enum BoardSide { Player, Enemy }

    [System.Serializable]
    public struct GridPos
    {
        public int x, y;
        public GridPos(int x, int y) { this.x = x; this.y = y; }
    }
}