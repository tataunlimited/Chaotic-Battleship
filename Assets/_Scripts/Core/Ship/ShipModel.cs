using System;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using Core.Ship.Upgrade;

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
        public ShipMovementPattern MovementPattern = null;
        public ShipAttackPattern AttackPattern = null;

        public bool canMove => !isDestroyed && MovementPattern != null && MovementPattern.canMove;
        public bool canRotate => !isDestroyed && MovementPattern != null && MovementPattern.canRotate;


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

        public List<GridPos> GetCells(GridPos rootCell)
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
            var coords = new List<GridPos>();
            if (AttackPattern.IsInCapacitated)
            {
                AttackPattern.IsInCapacitated = false;
                return coords;
            }

            

            coords = AttackPattern.GetAttackPositions(boardView);

            if (coords.Count > 0)
            {
                switch (type)
                {
                    case ShipType.Destroyer:
                        SFXManager.Instance?.PlayDestroyerAttack();
                        break;
                    case ShipType.Battleship:
                        SFXManager.Instance?.PlayBattleshipAttack();
                        break;
                    case ShipType.Submarine:
                        SFXManager.Instance?.PlaySubmarineAttack();
                        break;
                    case ShipType.Cruiser:
                        SFXManager.Instance?.PlayCruiserAttack();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                    coords.AddRange(boardView.CruiserAttack(GetCells(), orientation, 0, true));
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
                submerged = submerged,
                currentArmor = currentArmor,
                armor = armor,
                armorDestroyerChance = armorDestroyerChance,
                armorCruiserChance = armorCruiserChance,
                reserved = reserved,
                MovementPattern = ShipMovementPattern.CreateMovementPattern(type),
            };
            if (AttackPattern != null)
            {
                copy.AttackPattern = AttackPattern.Copy();
            }

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
                MovementPattern.hasAlreadyMoved = true;
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
            return MovementPattern.GetAllPossibleMovePositions(playerView, this);
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

        public void InitAttackPattern(int attackLevel, int specialAbilityLevel)
        {
            AttackPattern = type switch
            {
                ShipType.Destroyer => new DestroyerAttackPattern(this, attackLevel, specialAbilityLevel),
                ShipType.Battleship => new BattleShipAttackPattern(this, attackLevel, specialAbilityLevel),
                ShipType.Cruiser => new CruiserAttackPattern(this, attackLevel, specialAbilityLevel),
                ShipType.Submarine => new SubAttackPattern(this, attackLevel, specialAbilityLevel),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public static class ShipFactory
    {
        public static readonly Dictionary<ShipType, ShipModel> DefaultShips = new()
        {
            {
                ShipType.Battleship,
                CreateBattleship()
            },
            {
                ShipType.Submarine,
                CreateSubmarine()
            },
            {
                ShipType.Destroyer,
                CreateDestroyer()
            },
            {
                ShipType.Cruiser,
                CreateCruiser()
            }
        };

        public static ShipModel CreateShipModel(ShipType type)
        {
            return DefaultShips[type].Copy();
        }

        private static ShipModel CreateBattleship()
        {
            var ship = new ShipModel
            {
                id = "battleship",
                type = ShipType.Battleship,
                length = 4,
                MovementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Battleship)
            };

            ship.AttackPattern = new BattleShipAttackPattern(ship, 0, 0);

            return ship;
        }

        private static ShipModel CreateSubmarine()
        {
            var ship = new ShipModel
            {
                id = "submarine",
                type = ShipType.Submarine,
                length = 1,
                MovementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Submarine)
            };
            ship.AttackPattern = new SubAttackPattern(ship, 0, 0);

            return ship;
        }

        private static ShipModel CreateDestroyer()
        {
            var ship = new ShipModel
            {
                id = "destroyer",
                type = ShipType.Destroyer,
                length = 2,
                MovementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Destroyer)
            };
            ship.AttackPattern = new DestroyerAttackPattern(ship, 0, 0);

            return ship;
        }

        private static ShipModel CreateCruiser()
        {
            var ship = new ShipModel
            {
                id = "cruiser",
                type = ShipType.Cruiser,
                length = 3,
                MovementPattern = ShipMovementPattern.CreateMovementPattern(ShipType.Cruiser)
            };
            ship.AttackPattern = new CruiserAttackPattern(ship, 0, 0);


            return ship;
        }
    }
}