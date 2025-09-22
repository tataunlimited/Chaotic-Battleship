
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.GridSystem;
using UnityEngine;
using Random = System.Random;


namespace Core.Ship
{

    public struct PositionAndOrientation
    {
        public GridPos position;
        public Orientation orientation;
    }


    // just making it Serializable, so can expose it in the BoardController to change intelligenceLevel
    [System.Serializable]

    // EnemyWaveManager handles enemy AI behavior
    //
    // IntelligenceMode is described here:
    //      https://docs.google.com/document/d/1GcoqxzFUJKC-komFlCzL5_cIS60NXxgO4zf4hF_iFTg/edit?tab=t.0
    //
    public class EnemyWaveManager
    {
        public enum IntelligenceMode  { AttackAvoidance, Targeting }

        public int intelligenceLevel = 0;
        public IntelligenceMode  intelligenceMode ;

        // for intelligenceLevel's 0 to 3, the Percentage of moves avoided
        private static readonly float[] avoidancePercent = { 0f, 0.1f, 0.3f, 0.5f };
        private static readonly float[] targetingPercent = { 0f, 0.2f, 0.4f, 0.7f };

        // for intelligenceLevel's 0 to 3, the Destroyer's Percentage chance of targeting a ship's bow 
        private static readonly float[] destroyerTargetingPercent = { 1f/9, 1f/4, 2f/7, 1f/3 };


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
                isValid = ShipFactory.DefaultShips.TryGetValue(DEFAULT_SHIP_TYPES[i], out ship);
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

                //Array orientations = Enum.GetValues(typeof(Orientation));
                Array orientations = new[] { Orientation.North, Orientation.East, Orientation.South, Orientation.West };
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

        public bool MoveEnemyShips(BoardView AIBoard, BoardView playerBoard)
        {
            Debug.Log("MoveEnemyShips");

            // get list of locations that the player ships occupy on the playerBoard
            List<GridPos> playerShipsLocations = GetPlayerShipsLocations(playerBoard);

            if (intelligenceLevel == 0)
                return RandomlyMoveShips(AIBoard, playerBoard, playerShipsLocations);

            ComputeIntelligenceMode(AIBoard, playerBoard);      // sets intelligenceMode

            // get list of locations the player ships might hit
            List<GridPos> playerImminentHitSet = CalculatePlayerImminentHitSet(AIBoard, playerBoard);


            bool haveAllSuccessfullyMoved = true;
            foreach (ShipView shipView in AIBoard.SpawnedShips.Values)
            {
                haveAllSuccessfullyMoved &= IntelligentlyTurnAndMove(shipView, AIBoard, playerBoard, playerImminentHitSet, playerShipsLocations);
            }

            return haveAllSuccessfullyMoved;
        }



        /* ComputeIntelligenceMode
         * https://docs.google.com/document/d/1GcoqxzFUJKC-komFlCzL5_cIS60NXxgO4zf4hF_iFTg/edit?tab=t.0
         * 
         * ratio < 0.26 -> Targeting
         * 0.26 <= ratio >= 1.00 -> Attack Avoidance
         * ratio > 1.01 -> Targeting
         * 
         * Boundary gap handling (1.00 < ratio ? 1.01 or 0.25 ? ratio < 0.26): default to Attack Avoidance.
         * 
         */
        private void ComputeIntelligenceMode(BoardView AIBoard, BoardView playerBoard)
        {
            float ratio = AIBoard.ComputeTotalHealth() / playerBoard.ComputeTotalHealth();
            if (ratio >= 0.25 && ratio <= 1.01)
                intelligenceMode = IntelligenceMode.AttackAvoidance;
            else
                intelligenceMode = IntelligenceMode.Targeting;

            Debug.Log("ComputeIntelligenceMode ratio: " + ratio + ", intelligenceMode: " + intelligenceMode);
        }

        /* CalculatePlayerImminentHitSet
         * - finds all cells that the player Destroyers, Subs, and Cruisers may hit
         */
        private List<GridPos> CalculatePlayerImminentHitSet(BoardView AIBoard, BoardView playerBoard)
        {
            Debug.Log("CalculatePlayerImminentHitSet");

            ShipModel ship;
            List<GridPos> coords;
            List<GridPos> playerImminentHitSet = new List<GridPos>();
            List<GridPos> destroyerCoords;  // not used. Just needed for GetPossibleAreaOfAttack call
            bool chance;                    // not used. Just needed for GetPossibleAreaOfAttack call

            // Note: future improvement: a smarter approach is to actually order the ships by Destroyers, 
            //      Submarines, and then Cruisers since this is the order of accuracy of the ships attacks
            foreach (ShipView shipView in playerBoard.SpawnedShips.Values)
            {
                ship = shipView.shipModel;
                if (ship.isDestroyed)
                    continue;

                switch (ship.type)
                {
                    case ShipType.Battleship:
                        continue;    // ignore the Battleship since it can hit the entire board

                    case ShipType.Cruiser:
                        coords = ship.GetPossibleAreaOfAttack(AIBoard, out destroyerCoords, out chance);
                        break;

                    case ShipType.Destroyer:
                        ship.GetPossibleAreaOfAttack(AIBoard, out coords, out chance);  // destroyer returns its coords through the out parameter
                        break;

                    case ShipType.Submarine:
                        if (!ship.SubIsFiringThisRound(AIBoard.IsLastShip))
                            continue;       // currently Sub only fires on rounds % 2 == 0 or if IsLastShip, but this may change with upgrades

                        coords = ship.GetPossibleAreaOfAttack(AIBoard, out destroyerCoords, out chance);
                        break;

                    default:
                        Debug.LogError("CalculatePlayerImminentHitSet found unhandled ship type: " + ship.type);
                        continue;
                }

                playerImminentHitSet.AddRange(coords);
            }

            return playerImminentHitSet;
        }


        /* GetPlayerShipsLocations
         * - finds all cells that the players ships occupy
         */
        private List<GridPos> GetPlayerShipsLocations(BoardView playerBoard)
        {
            Debug.Log("GetPlayerShipsLocations");

            List<GridPos> coords;
            List<GridPos> playerShipsLocations = new List<GridPos>();

            foreach (ShipView shipView in playerBoard.SpawnedShips.Values)
            {
                coords = shipView.shipModel.GetCells();
                playerShipsLocations.AddRange(coords);
            }

            return playerShipsLocations;
        }

        public bool IntelligentlyTurnAndMove(ShipView shipView,
                                             BoardView AIBoard,
                                             BoardView playerBoard,
                                             List<GridPos> playerImminentHitSet,
                                             List<GridPos> playerShipsLocations)
        {
            ShipModel ship = shipView.shipModel;
            Debug.Log("IntelligentlyTurnAndMove ship: " + shipView.name + " starts with orientation: " + ship.orientation + ", pos: " + ship.root);

            List<PositionAndOrientation> possibleMoves = GetAllPossibleMoveAndTurnLocations(AIBoard, shipView);
            string alllMoves = "";
            foreach (PositionAndOrientation possibleMove in possibleMoves)
            {
                alllMoves += "(" + possibleMove.position.x + "," + possibleMove.position.y + "," + possibleMove.orientation + "), ";
            }
            Debug.Log(possibleMoves.Count + " Moves: " + alllMoves);

            if (possibleMoves.Count == 0)       // this should never happen, since the original position and rotation should always be a valid choice
                return false;

            // Check if applying AttackAvoidance and if so reduce possibleMoves to avoid the most dangerous
            if (intelligenceMode == IntelligenceMode.AttackAvoidance || 
                ship.type == ShipType.Battleship || 
                ship.type == ShipType.Destroyer || 
                (ship.type == ShipType.Submarine && !ship.SubIsFiringThisRound(AIBoard.IsLastShip)))
            {
                AvoidMostDangerousMoves(ship, playerImminentHitSet, possibleMoves);
            }

            // Destroyer always applies its own specific type of targeting
            if (ship.type == ShipType.Destroyer)
                ship.reserved = GetDestroyerAttackCellForAI(playerBoard, playerShipsLocations);

            // Check if applying Targeting (only for Sub and Cruiser) and if so reduce possibleMoves to be the most advantageous
            if (intelligenceMode == IntelligenceMode.Targeting && 
                (ship.type == ShipType.Cruiser ||
                ship.type == ShipType.Submarine && ship.SubIsFiringThisRound(AIBoard.IsLastShip)))
            { 
                TargetEnemyShips(ship, AIBoard, playerBoard, playerShipsLocations, possibleMoves);
            }

            // for debugging, reporting remaining moves to console
            alllMoves = "";
            foreach (PositionAndOrientation possibleMove in possibleMoves)
            {
                alllMoves += "(" + possibleMove.position.x + "," + possibleMove.position.y + "," + possibleMove.orientation + "), ";
            }
            Debug.Log(possibleMoves.Count + " remaining moves: " + alllMoves);

            // randomly choose one of the validLocations to set as the ship's root 
            PositionAndOrientation move = possibleMoves[rnd.Next(possibleMoves.Count)];

            // move the ship
            Debug.Log("Updating ship: " + shipView.name + " with orientation: " + move.orientation + ", pos: " + move.position);
            shipView.UpdatePosition(move.position, move.orientation, false);

            return true;
        }

        /*
         * possibleMoves is altered by this method (namely by changing the order and removing some moves)
         */
        private void TargetEnemyShips(ShipModel ship,
                                      BoardView AIBoard,
                                      BoardView playerBoard,
                                      List<GridPos> playerShipsLocations,
                                      List<PositionAndOrientation> possibleMoves)
        {
            Debug.Log("TargetEnemyShips");

            int numHits;
            GridPos originalLocation = ship.root;
            Orientation originalOrientation = ship.orientation;
            List<GridPos> cellsAttacked;
            PositionAndOrientation removedMove;
            int numMovesToAvoid = (int)MathF.Ceiling(possibleMoves.Count * targetingPercent[intelligenceLevel]); 
            List<GridPos> destroyerCoords;  // not used. Just needed for GetPossibleAreaOfAttack call
            bool chance;                    // not used. Just needed for GetPossibleAreaOfAttack call


            if (numMovesToAvoid <= 0)
                return;

            // For each possibleMove, see how dangerous it is
            Dictionary<PositionAndOrientation, int> weightedMoves = new();
            foreach (PositionAndOrientation possibleMove in possibleMoves)
            {
                numHits = 0;

                // temporarily move the ship to possibleMove's position and orientation
                ship.root = possibleMove.position;
                ship.orientation = possibleMove.orientation;

                // get cells our ship is threatening
                switch (ship.type)
                {
                    case ShipType.Cruiser:
                        cellsAttacked = ship.GetPossibleAreaOfAttack(playerBoard, out destroyerCoords, out chance);
                        break;

                    case ShipType.Submarine:
                        if (!ship.SubIsFiringThisRound(playerBoard.IsLastShip))
                        {
                            // currently Sub only fires on rounds % 2 == 0 or if IsLastShip, but this may change with upgrades
                            Debug.Log("TargetEnemyShips shouldn't be called when Submarine is not firing");
                            return;      
                        }

                        cellsAttacked = ship.GetPossibleAreaOfAttack(playerBoard, out destroyerCoords, out chance);
                        break;

                    case ShipType.Battleship:
                        Debug.Log("TargetEnemyShips shouldn't be called for Battleship");
                        return;    // ignore the Battleship since it can hit the entire board

                    case ShipType.Destroyer:
                        Debug.Log("TargetEnemyShips shouldn't be called for Destroyer");
                        return;    // ignore the Destroyer since it has its own targetting method

                    default:
                        Debug.LogError("TargetEnemyShips found unhandled ship type: " + ship.type);
                        continue;
                }

                // for each cell that's potentially attacked, see if it hits a player ship and if so increment numHits
                foreach (GridPos pos in cellsAttacked)
                {
                    if (playerShipsLocations.Contains(pos))
                    { 
                        numHits++;

                        // sub can only hit 1 cell before its attack stops
                        if (ship.type == ShipType.Submarine)
                            break;
                    }
                }

                weightedMoves.Add(possibleMove, numHits);
            }

            // put the ship back at its original position and orientation
            ship.root = originalLocation;
            ship.orientation = originalOrientation;

            // change possibleMoves so that it's sorted by numHits
            possibleMoves.Clear();
            var sortedByValue = weightedMoves.OrderByDescending(pair => pair.Value).ToList();
            foreach (var pair in sortedByValue)
            {
                possibleMoves.Add(pair.Key);
            }

            // removing the last numMovesToAvoid moves
            string avoidedMoves = "";        // for debugging, reporting avoidedMoves to console
            for (int i = 1; i <= numMovesToAvoid; ++i)
            {
                // have to leave at least one move
                if (possibleMoves.Count <= 1)
                {
                    numMovesToAvoid = i - 1;    // for reporting/debugging purposes
                    break;
                }

                removedMove = possibleMoves[possibleMoves.Count - 1]; // Get the last element
                avoidedMoves += "(" + removedMove.position.x + "," + removedMove.position.y + "," + removedMove.orientation + "), ";
                
                possibleMoves.RemoveAt(possibleMoves.Count - 1);
            }

            Debug.Log(numMovesToAvoid + " avoided moves: " + avoidedMoves);
        }


        /*
         * possibleMoves is altered by this method (namely by changing the order and removing some moves)
         */
        private void AvoidMostDangerousMoves(ShipModel ship, 
                                             List<GridPos> playerImminentHitSet,
                                             List<PositionAndOrientation> possibleMoves)
        {
            Debug.Log("AvoidMostDangerousMoves");

            int numHits;
            PositionAndOrientation removedMove;
            GridPos originalLocation = ship.root;
            Orientation originalOrientation = ship.orientation;
            List <GridPos> shipCells;
            int numMovesToAvoid = (int)MathF.Ceiling(possibleMoves.Count * avoidancePercent[intelligenceLevel]);

            if (numMovesToAvoid <= 0) 
                return;

            // For each possibleMove, see how dangerous it is
            Dictionary<PositionAndOrientation, int> weightedMoves = new();
            foreach (PositionAndOrientation possibleMove in possibleMoves)
            {
                numHits = 0;

                // temporarily move the ship to possibleMove's position and orientation
                ship.root = possibleMove.position;
                ship.orientation = possibleMove.orientation;
                shipCells = ship.GetCells();

                // for each player targeted position that would hit the ship if it made this possibleMove, increment numHits
                foreach (GridPos pos in playerImminentHitSet)
                {
                    if (shipCells.Contains(pos))
                        numHits++;
                }

                weightedMoves.Add(possibleMove, numHits);
            }

            // put the ship back at its original position and orientation
            ship.root = originalLocation;
            ship.orientation = originalOrientation;

            // change possibleMoves so that it's sorted by numHits
            possibleMoves.Clear();
            var sortedByValue = weightedMoves.OrderBy(pair => pair.Value).ToList();
            foreach (var pair in sortedByValue)
            {
                possibleMoves.Add(pair.Key);
            }

            // removing the last numMovesToAvoid moves
            string avoidedMoves = "";        // for debugging, reporting avoidedMoves to console
            for (int i = 1;  i <= numMovesToAvoid; ++i)
            {
                // have to leave at least one move
                if (possibleMoves.Count <= 1)
                {
                    numMovesToAvoid = i - 1;    // for reporting/debugging purposes
                    break;
                }
                removedMove = possibleMoves[possibleMoves.Count - 1]; // Get the last element

                // if the next move to remove was not threatened, no reason to remove it. 
                if (weightedMoves.GetValueOrDefault(removedMove) < 1)
                {
                    Debug.Log("This move was not threatened, so no reason to avoid.");
                    numMovesToAvoid = i - 1;    // for reporting/debugging purposes
                    break;
                }
                avoidedMoves += "(" + removedMove.position.x + "," + removedMove.position.y + "," + removedMove.orientation + "), ";
                
                possibleMoves.RemoveAt(possibleMoves.Count - 1);
            }

            Debug.Log(numMovesToAvoid + " avoided moves: " + avoidedMoves);
        }

        public List<PositionAndOrientation> GetAllPossibleMoveAndTurnLocations(BoardView board, ShipView shipView)
        {
            ShipModel ship = shipView.shipModel;
            List<PositionAndOrientation> locations = new();
            List<GridPos> coords = ship.MovementPattern.GetAllPossibleMovePositions(board, ship);
            Orientation newOrientation;

            // include the choice of not moving or rotating, or then just rotating left or right without moving
            locations.Add(new PositionAndOrientation { position = ship.root, orientation = ship.orientation });
            if (ship.MovementPattern.CanRotateLeft(board, shipView, out newOrientation))
                locations.Add(new PositionAndOrientation { position = ship.root, orientation = newOrientation });
            if (ship.MovementPattern.CanRotateRight(board, shipView, out newOrientation))
                locations.Add(new PositionAndOrientation { position = ship.root, orientation = newOrientation });

            foreach (GridPos coord in coords)
            {
                // include every move
                locations.Add(new PositionAndOrientation { position = coord, orientation = ship.orientation });

                // if canMoveAfterRotating, include every valid rotation after moving
                if (ship.MovementPattern.moveData.CanRotateAndMove)
                {
                    if (ship.MovementPattern.CanRotateLeft(board, shipView, out newOrientation))
                        locations.Add(new PositionAndOrientation { position = coord, orientation = newOrientation });
                    if (ship.MovementPattern.CanRotateRight(board, shipView, out newOrientation))
                        locations.Add(new PositionAndOrientation { position = coord, orientation = newOrientation });
                }
            }

            return locations;
        }


        private GridPos GetDestroyerAttackCellForAI(BoardView boardView, List<GridPos> playerShipsLocation)
        {
            Debug.Log("GetDestroyerAttackCellForAI");
            
            if (rnd.NextDouble() > destroyerTargetingPercent[intelligenceLevel])
            {
                GridPos target = new GridPos(rnd.Next(10), rnd.Next(10));
                Debug.Log("Random target: " + target.x + ", " + target.y);
                return target;
            }

            var shipViews = boardView.SpawnedShips.Values.ToList();
            int randomIndex = UnityEngine.Random.Range(0, shipViews.Count);
            var rndShip = shipViews[randomIndex];
            var bowPosition = rndShip.shipModel.root;

            Debug.Log("Targeting ship, Type: " + rndShip.shipModel.type + ",  Name: " + rndShip.name + ", shooting at " + bowPosition);
            return bowPosition;
        }

        public bool RandomlyMoveShips(BoardView AIBoard, BoardView playerBoard, List<GridPos> playerShipsLocations)
        {
            Debug.Log("RandomlyMoveShips works the same as before except the Destroyer has a chance to target a ship directly");

            bool haveBeenSuccessfullyMoved = true;

            foreach (ShipView shipView in AIBoard.SpawnedShips.Values)
            {
                haveBeenSuccessfullyMoved &= RandomlyMoveAShip(AIBoard, shipView);

                if (shipView.shipModel.type == ShipType.Destroyer)
                    shipView.shipModel.reserved = GetDestroyerAttackCellForAI(playerBoard, playerShipsLocations);
            }

            return haveBeenSuccessfullyMoved;

        }

        private bool RandomlyMoveAShip(BoardView board, ShipView shipView)
        {
            if (shipView.shipModel.IsSunk) return true;  // don't move sunk ships
            return shipView.shipModel.MovementPattern.RandomlyTurnAndMove(board, shipView);
        }

    }
}
