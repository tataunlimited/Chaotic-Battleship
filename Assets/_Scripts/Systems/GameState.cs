using System;
using System.Collections.Generic;
using Core.Ship;

[Serializable]
public class GameState
{
    public int waveNumber;
    public string phase;        // "PLAYER_FIRING", etc.
    public BoardState playerBoard;
    public BoardState enemyBoard;
}

[Serializable]
public class BoardState
{
    public List<ShipState> ships = new();
}

[Serializable]
public class ShipState
{
    public ShipType type;
    public int length;
    public int rootX;
    public int rootY;
    public string orientation;  // "North"/"East"/"South"/"West"
    public int hp;              // matches ShipModel.hp
    public bool isSunk;         // saved for completeness; logic mainly uses hp
}
