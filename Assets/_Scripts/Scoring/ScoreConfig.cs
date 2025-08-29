using Core.Ship;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreConfig", menuName = "Config/Score Config")]
public class ScoreConfig : ScriptableObject
{
    [Header("Per segment hit")]
    public int segmentHit = 25;

    [Header("Destroy bonuses")]
    public int submarineDestroy = 50;
    public int destroyerDestroy = 100;
    public int battleshipDestroy = 200;

    [Header("Wave clear + speed bonus")]
    public int waveClear = 1000;
    public int speedMax = 1000;   // base used in max(1000 - 50*turns, 0)
    public int speedPerTurn = 50;

    public int DestroyBonus(ShipType t)
    {
        switch (t)
        {
            case ShipType.Submarine: return submarineDestroy;
            case ShipType.Destroyer: return destroyerDestroy;
            case ShipType.Battleship: return battleshipDestroy;
            default: return 0;
        }
    }
}
