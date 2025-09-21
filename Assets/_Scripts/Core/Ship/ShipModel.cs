using System;
using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.GridSystem;
using UnityEngine;

namespace Core.Ship
{
    public enum ShipType
    {
        Destroyer = 1,
        Battleship = 2,
        Cruiser = 3,
        Submarine = 4
    }

    [System.Serializable]
    public class ShipModel
    {
        public string id;
        public ShipType type;
        public int length;
        public bool submerged = false;
        public int hp;
        public int MaxHP => length;
        public int currentArmor;
        public bool IsSunk => isDestroyed || hp <= 0;
        public float armor = 0f;
        public float armorDestroyerChance = 0f;
        public float armorCruiserChance = 0f;
        public Orientation orientation = Orientation.North;
        public GridPos root; // bow (front) position   
        public GridPos reserved = new GridPos(-1000, -1000); // Destroyer's attack position   
        public bool isDestroyed = false;
        private int _round = 0;
        public ShipMovementPattern movementPattern = null;

        public bool canMove => !isDestroyed && movementPattern != null && movementPattern.canMove;
        public bool canRotate => !isDestroyed && movementPattern != null && movementPattern.canRotate;


        /// <summary>Apply damage and return true if the ship just sunk.</summary>
        public bool ApplyDamage(int damage = 1)
        {
            if (IsSunk) return false;
            hp = Math.Max(0, hp - damage);
            if (hp == 0)
            {
                isDestroyed = true;
                return true;
            }

            return false;
        }

        public List<GridPos> GetCells()
        {
            return GetCells(root);
        }

        private List<GridPos> GetCells(GridPos rootCell)
        {
            var cells = new List<GridPos>();
            for (int i = 0; i < length; i++)
            {
                cells.Add(orientation switch
                {
                    Orientation.North => new GridPos(rootCell.x, rootCell.y - i),
                    Orientation.East => new GridPos(rootCell.x - i, rootCell.y),
                    Orientation.South => new GridPos(rootCell.x, rootCell.y + i),
                    Orientation.West => new GridPos(rootCell.x + i, rootCell.y),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }

            return cells;
        }

        public void ResetHP()
        {
            hp = length;
            isDestroyed = false;
        }

        internal List<GridPos> GetAttackCoordinates(BoardView boardView, bool isSpecialAttack)
        {
            List<GridPos> coords = new List<GridPos>();
            switch (type)
            {
                case ShipType.Destroyer:
                    if (reserved.x < 0 || reserved.y < 0)
                    {
                        reserved = root;
                    }

                    // AI destroyer target is set during AI movement/placement now

                    coords.Add(reserved);
                    SFXManager.Instance?.PlayDestroyerAttack();
                    break;
                case ShipType.Battleship:
                    int count = isSpecialAttack ? 12 : 4;
                    coords.AddRange(boardView.GetRandomPositions(count));
                    SFXManager.Instance?.PlayBattleshipAttack();
                    break;
                case ShipType.Submarine:
                {
                    if (SubIsFiringThisRound(isSpecialAttack))
                    {
                        submerged = true;
                        List<GridPos> line = orientation is Orientation.West or Orientation.East
                            ? boardView.GetRow(root.y, orientation)
                            : boardView.GetColumn(root.x, orientation);

                        foreach (var pos in line)
                        {
                            coords.Add(pos);
                            // Stop if this grid cell has a ship
                            if (boardView.HasShipAt(pos))
                                break;
                        }
                            SFXManager.Instance?.PlaySubmarineAttack();
                        }
                    else
                    {
                        // Submarine is reloading
                        submerged = false;
                    }

                    _round++;
                    break;
                }
                case ShipType.Cruiser:
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation));
                    if (isSpecialAttack)
                    {
                        var randomRoots = boardView.GetRandomPositions(2);
                        coords.AddRange(boardView.CruiserAttack(GetCells(randomRoots[0]), orientation));
                        coords.AddRange(boardView.CruiserAttack(GetCells(randomRoots[1]), orientation));
                    }
                    SFXManager.Instance?.PlayCruiserAttack();

                    break;
            }

            return coords;
        }

        internal List<GridPos> GetPossibleAreaOfAttack(BoardView boardView, out List<GridPos> selectedCoords,
            out bool chance)
        {
            List<GridPos> coords = new List<GridPos>();
            selectedCoords = new List<GridPos>();
            chance = false;

            switch (type)
            {
                case ShipType.Destroyer:

                    coords.AddRange(boardView.GetAllPositions());
                    selectedCoords.Add(reserved);
                    chance = true;
                    break;
                case ShipType.Battleship:
                    coords.AddRange(boardView.GetAllPositions());
                    chance = true;
                    break;
                case ShipType.Submarine:
                    coords = orientation is Orientation.West or Orientation.East
                        ? boardView.GetRow(root.y, orientation)
                        : boardView.GetColumn(root.x, orientation);
                    chance = false;
                    break;
                case ShipType.Cruiser:
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation, true));
                    chance = true;
                    break;
            }

            return coords;
        }

        public ShipModel Copy()
        {
            ShipModel copy = new ShipModel
            {
                id = id,
                type = type,
                length = length,
                root = root,
                orientation = orientation,
                hp = hp,
                isDestroyed = isDestroyed,
                _round = _round,
                movementPattern = ShipMovementPattern.CreateMovementPattern(type)
            };
            return copy;
        }

        public bool MoveTowards(Orientation direction, int count = 1)
        {
            root = direction switch
            {
                Orientation.North => new GridPos(root.x, root.y + count),
                Orientation.East => new GridPos(root.x + count, root.y),
                Orientation.South => new GridPos(root.x, root.y - count),
                Orientation.West => new GridPos(root.x - count, root.y),
                _ => throw new ArgumentOutOfRangeException()
            };

            return true;
        }

        public void UpdateMovementStatus()
        {
            if (GameManager.instance.phaseState != GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
                movementPattern.hasAlreadyMoved = true;
        }

        public Orientation RotateLeft()
        {
            int orientationNumber = (int)orientation;
            if (orientationNumber == 0)
            {
                orientationNumber = 3;
            }
            else
            {
                orientationNumber--;
            }

            return (Orientation)orientationNumber;
        }

        public Orientation RotateRight()
        {
            int orientationNumber = (int)orientation;
            if (orientationNumber == 3)
            {
                orientationNumber = 0;
            }
            else
            {
                orientationNumber++;
            }

            return (Orientation)orientationNumber;
        }

        public List<GridPos> GetMovablePositions(BoardView playerView)
        {
            return movementPattern.GetAllPossibleMovePositions(playerView, this);
        }

        public bool CanReceiveDamage(ShipType attackerType)
        {
            if (armorDestroyerChance > 0 && attackerType == ShipType.Destroyer)
            {
                var chance = UnityEngine.Random.Range(0, 1f);
                if (chance <= armorDestroyerChance)
                {
                    return false;
                }
            }
            else if (armorCruiserChance > 0 && attackerType == ShipType.Cruiser)
            {
                var chance = UnityEngine.Random.Range(0, 1f);
                if (chance <= armorCruiserChance)
                {
                    return false;
                }
            }

            if (armor >= 1)
            {
                armor -= 1;
                return false;
            }

            if (armor > 0)
            {
                var chance = UnityEngine.Random.Range(0, 1f);
                bool result = !(chance <= armor);

                armor = 0;
                return result;
            }

            return true;
        }

        public void SetArmorLevelData(ArmorUpgrade armorData)
        {
            armor = armorData.ArmorPoints;
            armorDestroyerChance = armorData.DestroyerArmorChance;
            armorCruiserChance = armorData.CruiserArmorChance;
        }

        // currently Sub only fires on rounds % 2 == 0 or if IsLastShip, but this may change with upgrades
        public bool SubIsFiringThisRound(bool isSpecialAttack)
        {
            return (_round % 2 == 0 || isSpecialAttack);
        }
    }

    public static class ShipDatabase
    {
        public static readonly Dictionary<ShipType, ShipModel> DefaultShips = new()
        {
            {
                ShipType.Battleship,
                new ShipModel
                {
                    id = "battleship", type = ShipType.Battleship, length = 4,
                    movementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Battleship)
                }
            },
            {
                ShipType.Submarine,
                new ShipModel
                {
                    id = "submarine", type = ShipType.Submarine, length = 1,
                    movementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Submarine)
                }
            },
            {
                ShipType.Destroyer,
                new ShipModel
                {
                    id = "destroyer", type = ShipType.Destroyer, length = 2,
                    movementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Destroyer)
                }
            },
            {
                ShipType.Cruiser,
                new ShipModel
                {
                    id = "cruiser", type = ShipType.Cruiser, length = 3,
                    movementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Cruiser)
                }
            }
        };
    }
}