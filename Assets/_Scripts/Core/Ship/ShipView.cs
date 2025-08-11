using Core.Board;
using Core.Ship;
using UnityEngine;

public class ShipView : MonoBehaviour
{

    public ShipModel shipModel;
    public BoardView enemyBoard;
    public void Attack()
    {
        var coords = shipModel.GetAttackCoordinates(enemyBoard);
        foreach (var coord in coords)
        {
            if (enemyBoard.Model.TryFire(coord, out bool hit))
                enemyBoard.Tint(coord, hit ? Color.red : Color.blue);
        }
    }

}
