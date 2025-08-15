
using Core.GridSystem;
using Core.Board;

using UnityEngine;
using System.Collections.Generic;
using System;
using Random = System.Random;
using UnityEngine.UIElements;
using System.Drawing;
using Unity.VisualScripting;


namespace Core.Ship
{

    public class EnemyWaveManager
    {
        public const int DEFAULT_NUM_SHIPS = 4;
        public int[] DEFAULT_SHIP_LENGTHS = { 4, 3, 2, 1 };

        public int waveNumber = 0;

        private Random rnd = new Random();


        public List<ShipModel> CreateDefaultWaveOfShips()
        {
            List<ShipModel> ships = new List<ShipModel>();

            for (int i=0; i < DEFAULT_NUM_SHIPS; i++)
            {
                ships.Add(new ShipModel
                {
                    length = DEFAULT_SHIP_LENGTHS[i]
                });
            }

            return ships;
        }


        // Takes in a wave of ships, randomly sets their rotation, and randomly sets  
        // 		the x & y coordinates to valid positions of the BoardView's grid 
        //
        // The algorithm used is to test all possible locations on the board that the ship can fit
        //      and randomly select one
        //
        // Returns: true - all ships are in valid locations
        //          false - at least one of the ships could not be placed in a valid location
        //
        public bool RandomlySetShipsLocations(BoardView board, List<ShipModel> ships)
        {
            List<GridPos> validLocations = new List<GridPos>();     // valid GridPos cells that the ship can fit
            bool haveBeenSuccessfullyPlaced = true;

            /*foreach (ShipModel ship in ships)
            {
                validLocations.Clear();

                ship.orientation = (Orientation)rnd.Next(2);

                int lastCol = board.width;
                int lastRow = board.height;
                // if the ship is horizontal, we know a ship of size > 1 won't fit in the rightmost columns, so don't bother testing those
                if (ship.orientation == Orientation.Horizontal)
                    lastCol = lastCol - ship.length + 1;

                // if the ship is vertical, we know a ship of size > 1 won't fit in the bottommost rows, so don't bother testing those
                else   
                    lastRow = lastRow - ship.length + 1;

                // test every position on the board to see if it is a valid GridPos, and if so, add to validLocations
                for (int col = 0; col < lastCol; col++) 
                {
                    for (int row = 0; row < lastRow; row++)
                    {
                        GridPos root = new GridPos(col, row);
                        if (board.Model.ValidateShipPlacement(root, ship.length, ship.orientation))
                        {
                            validLocations.Add(root);
                        }
                    }
                }

                if (validLocations.Count == 0)
                    haveBeenSuccessfullyPlaced = false;
                else 
                { 
                    // randomly choose one of the validLocations to set as the ship's root 
                    int index = rnd.Next(validLocations.Count);
                    ship.root = validLocations[index];
                    board.TryPlaceShip(ship);
                }
            }*/

            return haveBeenSuccessfullyPlaced;
        }

    }
}
