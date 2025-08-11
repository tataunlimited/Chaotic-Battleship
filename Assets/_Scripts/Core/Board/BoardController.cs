
using Core.GridSystem;
using Core.Ship;

using UnityEngine;
using System.Collections.Generic;

namespace Core.Board
{
    public class BoardController : MonoBehaviour
    {
        public BoardView playerView;
        public BoardView enemyView;

        void Start()
        {
            // Example placements (pure logic via Model)
            
            playerView.TryPlaceShip( new ShipModel
            {
                root = new GridPos(0,0),
                length = 5,
                orientation = Orientation.Horizontal,
                
            });
            playerView.TryPlaceShip(new ShipModel
            {
                root = new GridPos(3,2),
                length = 4,
                orientation = Orientation.Horizontal,
            });


            // uncomment next line for testing purposes to show where the enemy ships are placed
            //enemyView.revealShips = true;

            EnemyWaveManager enemyWaveManager = new EnemyWaveManager();

            // create a list of enemy ships with given lengths
            List<ShipModel> ships = enemyWaveManager.CreateDefaultWaveOfShips();

            // randomly set the enemy ship locations and orientations, and place them on the enemyView board
            enemyWaveManager.RandomlySetShipsLocations(enemyView, ships);

        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (TryHitBoard(enemyView, out var eCell))  // left-click fires at enemy
                {
                    if (enemyView.Model.TryFire(eCell, out bool hit))
                        enemyView.Tint(eCell, hit ? Color.red : Color.blue);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (TryHitBoard(playerView, out var pCell)) // right-click to test on player
                {
                    if (playerView.Model.TryFire(pCell, out bool hit))
                        playerView.Tint(pCell, hit ? Color.red : Color.blue);
                }
            }
        }

        bool TryHitBoard(BoardView view, out GridPos cell)
        {
            cell = default;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 500f))
                return false;
            return view.WorldToGrid(hit.point, out cell);
        }

    }
}