
using Core.GridSystem;
using Core.Board;

using UnityEngine;
using System.Collections.Generic;
using System;
using Random = System.Random;
using UnityEngine.UIElements;
using System.Drawing;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;


namespace Core.Ship
{

    public class EnemyWaveManager
    {
        public const int DEFAULT_NUM_SHIPS = 4;
        public ShipType[] DEFAULT_SHIP_TYPES = { ShipType.Battleship, 
                                                 ShipType.Cruiser, 
                                                 ShipType.Destroyer, 
                                                 ShipType.Submarine };

        private Random rnd = new Random();


        public List<ShipModel> CreateDefaultWaveOfShips()
        {
            List<ShipModel> ships = new List<ShipModel>();
            ShipModel ship;
            bool isValid;

            for (int i=0; i < DEFAULT_NUM_SHIPS; i++)
            {
                isValid = ShipDatabase.DefaultShips.TryGetValue(DEFAULT_SHIP_TYPES[i], out ship);
                if (isValid)
                {
                    ships.Add(ship.Copy());
                }
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
            int index;

            foreach (ShipModel ship in ships)
            {
                validLocations.Clear();

                Array orientations = Enum.GetValues(typeof(Orientation));
                index = rnd.Next(orientations.Length);
                ship.orientation = (Orientation)orientations.GetValue(index);

                int firstCol = 0;
                int firstRow = 0;
                int lastCol = board.width;
                int lastRow = board.height;

                // TODO: if needed,
                // HEURISTICS to reduce the number of cells that are checked with ValidateShipPlacement()
                // based on where a ship of each length and orientation could fit
                //
                //// if the ship is horizontal, we know a ship of size > 1 won't fit in the rightmost columns, so don't bother testing those
                //if (ship.orientation == Orientation.Horizontal)
                //    lastCol = lastCol - ship.length + 1;

                //// if the ship is vertical, we know a ship of size > 1 won't fit in the bottommost rows, so don't bother testing those
                //else   
                //    lastRow = lastRow - ship.length + 1;

                GridPos originalRoot = ship.root;

                // test every position on the board to see if it is a valid GridPos, and if so, add to validLocations
                for (int col = firstCol; col < lastCol; col++)
                {
                    for (int row = firstRow; row < lastRow; row++)
                    {
                        GridPos root = new GridPos(col, row);
                        ship.root = root;
                        if (board.Model.ValidateShipPlacement(ship))
                        {
                            validLocations.Add(root);
                        }
                    }
                }

                if (validLocations.Count == 0)
                {
                    ship.root = originalRoot;
                    haveBeenSuccessfullyPlaced = false;
                }
                else 
                { 
                    // randomly choose one of the validLocations to set as the ship's root 
                    index = rnd.Next(validLocations.Count);
                    ship.root = validLocations[index];
                    board.Model.TryPlaceShip(ship);

                    Debug.Log("Placing enemy ship: " + ship.id + ", orientation: " + ship.orientation + ", pos: " + ship.root);
                }
            }

            return haveBeenSuccessfullyPlaced;
        }

        //public bool RandomlyMoveShips(BoardView board, List<ShipModel> ships)
        //{
        //    bool haveBeenSuccessfullyMoved = true;

        //    foreach (ShipModel ship in ships)
        //    {
        //        haveBeenSuccessfullyMoved &= RandomlyMoveAShip(board, ship);
        //    }
            
        //    return haveBeenSuccessfullyMoved;
        //}

        public bool RandomlyMoveShips(BoardView board)
        {
            bool haveBeenSuccessfullyMoved = true;

            foreach (ShipView shipView in board.SpawnedShips.Values)
            {
                haveBeenSuccessfullyMoved &= RandomlyMoveAShip(board, shipView);
            }

            return haveBeenSuccessfullyMoved;

        }

        private bool RandomlyMoveAShip(BoardView board, ShipView shipView)
        {
            ShipMovementPattern pattern = ShipMovementPattern.CreateMovementPattern(shipView.shipModel.type);
            return pattern.RandomlyTurnAndMove(board, shipView);
        }

    }
}
