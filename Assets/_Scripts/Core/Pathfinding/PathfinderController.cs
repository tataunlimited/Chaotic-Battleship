using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using Core.Ship;
using UnityEngine;

namespace Core.Pathfinding
{
    /// <summary>
    /// Manages pathfinding requests by considering the current board state.
    /// This class acts as a bridge between the generic Pathfinder and the game's BoardModel.
    /// </summary>
    public class PathfinderController : MonoBehaviour
    {
        public static PathfinderController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        
        /// <summary>
        /// Finds a path for a ship, treating other ships as obstacles.
        /// </summary>
        /// <param name="boardView">The board on which to find the path.</param>
        /// <param name="movingShip">The ship that is moving.</param>
        /// <param name="start">The starting grid position.</param>
        /// <param name="end">The target grid position.</param>
        /// <returns>A list of GridPos representing the path, or null if no path is found.</returns>
        public List<GridPos> FindPathForShip(BoardView boardView, ShipModel movingShip, GridPos start, GridPos end)
        {
            var unwalkablePositions = new HashSet<GridPos>();
            
            // Add all currently occupied ship cells to the unwalkable set
            foreach (var shipView in boardView.SpawnedShips.Values)
            {
                // IMPORTANT: Ignore the ship that is currently trying to move
                if (shipView.shipModel.id == movingShip.id)
                {
                    continue;
                }

                foreach (var cell in shipView.shipModel.GetCells())
                {
                    unwalkablePositions.Add(cell);
                }
            }
            
            // You could also add other unwalkable tile types here if you had them (e.g., islands)
            // for (int x = 0; x < boardView.width; x++)
            // {
            //     for (int y = 0; y < boardView.height; y++)
            //     {
            //         var p = new GridPos(x, y);
            //         if (boardView.Model.Get(p) == CellState.Island)
            //         {
            //             unwalkablePositions.Add(p);
            //         }
            //     }
            // }

            return Pathfinder.FindPath(start, end, boardView.width, boardView.height, unwalkablePositions);
        }
    }
}
