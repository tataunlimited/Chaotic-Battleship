
using Core.GridSystem;

using UnityEngine;
using System.Collections.Generic;
using System;
using Random = System.Random;
using Core.Board;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;
using Unity.VisualScripting;


namespace Core.Ship
{

    // 
    public abstract class ShipMovementPattern
    {
        private Random rnd = new Random();

        public float chanceToStayStill = 0.5f;
        public float chanceToRotate = 0.5f;
        public bool canMoveAfterRotating = false;
        public int maxMovementPoints = 1;
        public int movesRemaining = 1;
        public bool hasAlreadyRotated = false;


        public static ShipMovementPattern CreateMovementPattern(ShipType type)
        {
            switch (type)
            {
                case ShipType.Battleship: return new BattleShipMovementPattern();
                case ShipType.Cruiser: return new CruiserMovementPattern();
                case ShipType.Destroyer: return new DestroyerMovementPattern();
                case ShipType.Submarine: return new SubmarineMovementPattern();
                default: return new BattleShipMovementPattern();
            }
        }

        public void Reset()
        {
            movesRemaining = maxMovementPoints;
            hasAlreadyRotated = false;
        }

        // AI Movement Decision Rules:
        // Enemy ships have a 50/50 chance to act:
        // Subs & Destroyers: 50% rotate, 50% move(both may occur).
        // Cruisers & Battleships: 50% chance to act; if acting, 50% rotate or move.
        // If chosen action is invalid, AI attempts the other; if both fail, it stays put.
        //
        // Algorithm:
        //      if random value is under chanceToStayStill, return true
        //      else if random value is under chanceToRotate and it successfully rotates and it can't move after rotating, return true
        //      else try to move and return whether it succesfully found a valid position to move to
        //
        // returns  true - if doesn't move or successfully turns and/or moves
        //          false - if tried to turns and/or move but failed
        public bool RandomlyTurnAndMove(BoardView board, ShipView shipView)
        {
            ShipModel ship = shipView.shipModel;
            Debug.Log("RandomlyTurnAndMove ship: " + ship.id + " starts with orientation: " + ship.orientation + ", pos: " + ship.root);

            Reset();

            if (rnd.NextDouble() < chanceToStayStill)
                return true;

            if (rnd.NextDouble() < chanceToRotate && RandomlyRotateLeftOrRight(board, shipView))
            {
                Debug.Log("rotated to " + ship.orientation);
                if (!canMoveAfterRotating)
                    return true;
            }

            bool isSuccess = MoveToARandomPosition(board, shipView);
            Debug.Log("RandomlyTurnAndMove ship: " + ship.id + " ends at orientation: " + ship.orientation + ", pos: " + ship.root);
            return isSuccess;
        }

        private bool RandomlyRotateLeftOrRight(BoardView board, ShipView shipView)
        {
            ShipModel ship = shipView.shipModel;
            bool hasSuccessfullyTurned = true;
            Orientation originalOrientation = ship.orientation;
            Orientation newOrientation = ship.orientation;
            List<Orientation> validOrientations = new List<Orientation>();     // valid Orientations that the ship can fit

            if (ship.isDestroyed)   // destroyed ships can't rotate
                return false;

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);
            //if (board.revealShips)
            //    board.HideAShip(ship);

            ship.orientation = ship.RotateLeft();
            if (board.Model.ValidateShipPlacement(ship))
            {
                validOrientations.Add(ship.orientation);
            }

            ship.orientation = originalOrientation;
            ship.orientation = ship.RotateRight();
            if (board.Model.ValidateShipPlacement(ship))
            {
                validOrientations.Add(ship.orientation);
            }

            if (validOrientations.Count == 0)
            {
                newOrientation = originalOrientation;
                hasSuccessfullyTurned = false;
            }
            else
            {
                newOrientation = validOrientations[rnd.Next(validOrientations.Count)];
                hasAlreadyRotated = true;
                movesRemaining--;
            }

            // place the ship
            //
            ship.orientation = originalOrientation;
            shipView.UpdatePosition(ship.root, newOrientation, false);

            return hasSuccessfullyTurned;
        }

        private bool MoveToARandomPosition(BoardView board, ShipView shipView)
        {
            ShipModel ship = shipView.shipModel;
            bool hasBeenSuccessfullyPlaced = true;
            GridPos originalPosition = ship.root;
            GridPos newPosition = ship.root;

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);
            //if (board.revealShips)
            //    board.HideAShip(ship);

            // valid GridPos cells that the ship can fit
            List<GridPos> validLocations = GetAllPossibleMovePositions(board, ship);

            if (validLocations.Count == 0)
            {
                newPosition = originalPosition;
                hasBeenSuccessfullyPlaced = false;
            }
            else
            {
                // randomly choose one of the validLocations to set as the ship's root 
                int index = rnd.Next(validLocations.Count);
                newPosition = validLocations[index];
            }

            // place the ship
            shipView.UpdatePosition(newPosition, ship.orientation, false);

            return hasBeenSuccessfullyPlaced;
        }


        public abstract List<GridPos> GetAllPossibleMovePositions(BoardView board, ShipModel ship);

    }

    // Battleship: 1 space forward; 1 space backward.
    class BattleShipMovementPattern : ShipMovementPattern
    {
        public override List<GridPos> GetAllPossibleMovePositions(BoardView board, ShipModel ship)
        {
            List<GridPos> locations = new();
            if (ship.isDestroyed)   // destroyed ships can't move
                return locations;

            GridPos originalPosition = ship.root;

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);

            ship.MoveTowards(ship.orientation, 1);      // forward 1
            if (board.Model.ValidateShipPlacement(ship))
                locations.Add(ship.root);

            ship.root = originalPosition;
            ship.MoveTowards(ship.orientation, -1);     // back 1
            if (board.Model.ValidateShipPlacement(ship))
                locations.Add(ship.root);

            // place the ship back in its originalPosition
            ship.root = originalPosition;
            board.Model.TryPlaceShip(ship);

            return locations;
        }
    }

    // Cruiser: Up to 2 spaces forward, 1 space backward.
    class CruiserMovementPattern : ShipMovementPattern
    {
        public override List<GridPos> GetAllPossibleMovePositions(BoardView board, ShipModel ship)
        {
            List<GridPos> locations = new();
            if (ship.isDestroyed)   // destroyed ships can't move
                return locations;

            GridPos originalPosition = ship.root;

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);

            ship.MoveTowards(ship.orientation, 1);      // forward 1
            if (board.Model.ValidateShipPlacement(ship))
                locations.Add(ship.root);
            ship.MoveTowards(ship.orientation, 1);      // forward 1 more
            if (board.Model.ValidateShipPlacement(ship))
                locations.Add(ship.root);

            ship.root = originalPosition;
            ship.MoveTowards(ship.orientation, -1);     // back 1
            if (board.Model.ValidateShipPlacement(ship))
                locations.Add(ship.root);

            // place the ship back in its originalPosition
            ship.root = originalPosition;
            board.Model.TryPlaceShip(ship);

            return locations;
        }
    }

    // Destroyer: Up to 2 spaces any direction from the bow. May rotate and move in the same Movement Phase.
    class DestroyerMovementPattern : ShipMovementPattern
    {
        public DestroyerMovementPattern()
        {
            canMoveAfterRotating = true;
            maxMovementPoints = 2;
            movesRemaining = 2;
        }

        public override List<GridPos> GetAllPossibleMovePositions(BoardView board, ShipModel ship)
        {
            List<GridPos> locations = new();
            if (ship.isDestroyed)   // destroyed ships can't move
                return locations;

            GridPos originalPosition = ship.root;     

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);

            // checking east and west moves
            for (int i = originalPosition.x - movesRemaining; i <= originalPosition.x + movesRemaining; i++)
            {
                if (i == originalPosition.x) continue;

                ship.root.x = i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // checking north and south moves
            for (int i = originalPosition.y - movesRemaining; i <= originalPosition.y + movesRemaining; i++)
            {
                if (i == originalPosition.y) continue;

                ship.root.y = i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // checking northeast and southwest diagonal moves
            for (int i = -movesRemaining; i <= movesRemaining; i++)
            {
                if (i == 0) continue;

                ship.root.x = originalPosition.x + i;
                ship.root.y = originalPosition.y + i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // checking northeast and southwest diagonal moves
            for (int i = -movesRemaining; i <= movesRemaining; i++)
            {
                if (i == 0) continue;

                ship.root.x = originalPosition.x + i;
                ship.root.y = originalPosition.y - i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // place the ship back in its originalPosition
            board.Model.TryPlaceShip(ship);

            return locations;
        }
    }

    // Submarine: Up to 3 spaces (N/S/E/W). May rotate and move in the same Movement Phase.
    class SubmarineMovementPattern : ShipMovementPattern
    {
        public SubmarineMovementPattern()
        {
            canMoveAfterRotating = true;
            maxMovementPoints = 3;
            movesRemaining = 3;
        }

        public override List<GridPos> GetAllPossibleMovePositions(BoardView board, ShipModel ship)
        {
            List<GridPos> locations = new();
            if (ship.isDestroyed)   // destroyed ships can't move
                return locations;

            GridPos originalPosition = ship.root;

            // remove the current ship location so it doesn't block possible locations
            board.Model.ResetShipCells(ship);

            // checking east and west moves
            for (int i = originalPosition.x - movesRemaining; i <= originalPosition.x + movesRemaining; i++)
            {
                if (i == originalPosition.x) continue;

                ship.root.x = i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // checking north and south moves
            for (int i = originalPosition.y - movesRemaining; i <= originalPosition.y + movesRemaining; i++)
            {
                if (i == originalPosition.y) continue;

                ship.root.y = i;
                if (board.Model.ValidateShipPlacement(ship))
                    locations.Add(ship.root);
            }
            ship.root = originalPosition;

            // place the ship back in its originalPosition
            board.Model.TryPlaceShip(ship);

            return locations;
        }
    }


}
