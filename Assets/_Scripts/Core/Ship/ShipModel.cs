using System;
using System.Collections.Generic;
using UnityEngine;

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
    public class EnemyShipModel : ShipModel
    {
        public int movementLevel;
        public int attackLevel;
        public int specialAbilityLevel;
        public int armorLevel;

        public EnemyShipModel() { }

        // This is the key constructor for your factory!
        public EnemyShipModel(ShipModel baseModel, int movementLevel, int attackLevel, int specialAbilityLevel, int armorLevel) 
            : base(baseModel) // This copies all properties from ShipModel!
        {
            this.movementLevel = movementLevel;
            this.attackLevel = attackLevel;
            this.specialAbilityLevel = specialAbilityLevel;
            this.armorLevel = armorLevel;
        
            // Now, initialize the attack pattern with the enemy's specific levels
            this.InitAttackPattern(this.attackLevel, this.specialAbilityLevel);
        }
    
        // Add a constructor to copy another EnemyShipModel
        public EnemyShipModel(EnemyShipModel other) : base(other)
        {
            this.movementLevel = other.movementLevel;
            this.attackLevel = other.attackLevel;
            this.specialAbilityLevel = other.specialAbilityLevel;
            this.armorLevel = other.armorLevel;

            this.InitAttackPattern(this.attackLevel, this.specialAbilityLevel);
        }

        // Override the Copy method
        public override ShipModel Copy()
        {
            return new EnemyShipModel(this);
        }
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
        public bool IsSunk => isDestroyed || hp <= 0;

        // armor
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

        public ShipModel() { }

        // Add this protected copy constructor
        protected ShipModel(ShipModel other)
        {
            this.id = other.id;
            this.type = other.type;
            this.length = other.length;
            this.submerged = other.submerged;
            this.hp = other.hp;
            this.isDestroyed = other.isDestroyed;
            this.armor = other.armor;
            this.armorDestroyerChance = other.armorDestroyerChance;
            this.armorCruiserChance = other.armorCruiserChance;
            this.orientation = other.orientation;
            this.root = other.root;
            this.reserved = other.reserved;
            this._round = other._round;
        
            this.MovementPattern = ShipMovementPattern.CreateMovementPattern(other.type);
            if(other.MovementPattern != null)
            {
                this.MovementPattern.moveData = other.MovementPattern.moveData; // Assuming moveData is a struct or needs copying
            }
        }

        // Change the existing Copy() method to be virtual
        public virtual ShipModel Copy()
        {
            // Now the Copy method simply uses our new copy constructor!
            var copy = new ShipModel(this);
        
            // We still need to initialize the attack pattern after copying
            int attackLevel = this.AttackPattern?.AttackLevel ?? 0;
            int specialLevel = this.AttackPattern?.SpecialAbilityLevel ?? 0;
            copy.InitAttackPattern(attackLevel, specialLevel);

            return copy;
        }
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
            if (isSpecialAttack)
            {
                AttackPattern.EnableSpecialAttackState();
            }
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

        public AreaOfAttack GetPossibleAreaOfAttack(BoardView enemyBoard)
        {
            if(reserved.x < 0 || reserved.y < 0)
            {
                reserved = root;
            }

            return AttackPattern.GetAreaOfAttack(enemyBoard);
        }

        internal List<GridPos> GetPossibleAreaOfAttack(BoardView boardView, out List<GridPos> selectedCoords,
            out bool chance)
        {
            List<GridPos> coords = new List<GridPos>();
            selectedCoords = new List<GridPos>();
            chance = false;
            if(reserved.x < 0 || reserved.y < 0)
            {
                reserved = root;
            }

            switch (type)
            {
                case ShipType.Destroyer:

                    coords.AddRange(boardView.GetAllPositions());
                    selectedCoords.Add(reserved);
                    chance = true;
                    break;
                case ShipType.Battleship:
                    coords.AddRange(boardView.GetAllPositions());
                    //selectedCoords.AddRange(AttackPattern.GetAreaOfAttack(boardView));
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

        public void SetMovementLevelData(MovementUpgrade movementData)
        {
            MovementPattern.moveData = movementData;
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

        public bool CanTarget()
        {
            switch (type)
            {
                case ShipType.Destroyer:
                case ShipType.Battleship when AttackPattern.AttackLevel > 2:
                    return true;
                default:
                    return false;
            }
        }
        
        public void CheckToRevealShips(BoardView enemyBoard, GridPos targetPos)
        {
            if(enemyBoard.Model.Get(targetPos) != CellState.NearMiss)
                return;
            
            if (AttackPattern.NearmissRevealRadius > 0)
            {
                var neighbors = enemyBoard.GetNeighbors(targetPos, AttackPattern.NearmissRevealRadius);
                foreach (var neighbor in neighbors)
                {
                    if (enemyBoard.TryGetShipAt(neighbor, out var ship))
                    {
                        ship.Show();
                        Debug.Log("Revealing A Ship!");
                    }
                }
            }
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
        public static EnemyShipModel CreateEnemyModel(WaveDefinition.ShipEntry shipEntry)
        {
            ShipModel baseModel = DefaultShips[shipEntry.type];

            return new EnemyShipModel(baseModel, shipEntry.movementLevel, shipEntry.attackLevel, shipEntry.specialAbilityLevel, shipEntry.armorLevel);
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

    public class AreaOfAttack
    {
        public List<GridPos> LineOfFireCells = new();
        public List<GridPos> TargetableCells = new();
        public List<GridPos> PossibleCells = new();
    }
}