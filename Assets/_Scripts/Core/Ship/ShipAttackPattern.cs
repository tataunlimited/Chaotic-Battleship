using System;
using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.GridSystem;

namespace Core.Ship
{
    public abstract class ShipAttackPattern
    {
        protected readonly ShipModel ShipModel;
        protected readonly int AttackLevel;
        protected readonly int SpecialAbilityLevel;

        public bool IsInCapacitated { get; set; }

        protected ShipAttackPattern(ShipModel shipModel, int attackLevel, int specialAbilityLevel)
        {
            ShipModel = shipModel;
            AttackLevel = attackLevel;
            SpecialAbilityLevel = specialAbilityLevel;
            IsInCapacitated = false;
        }

        public event Action<TorpedoData> OnTorpedoFired;
        protected static int Round => GameManager.instance.RoundNumber;
        protected bool IsSpecialAttack => BoardController.Instance.playerView.IsLastShip;


        public abstract List<GridPos> GetAttackPositions(BoardView enemyBoard);

        public virtual int GetAttackDamage(ShipType targetType)
        {
            return 1;
        }

        protected bool CalculateChance(float chance)
        {
            return UnityEngine.Random.Range(0, 1f) <= chance;
        }

        // if true prevent the target from attacking next round
        public virtual bool CanIncapacitate(ShipType targetType)
        {
            return false;
        }

        // if true prevent the target from moving next round
        public virtual bool CanFreezeTarget(ShipType targetType)
        {
            return false;
        }

        public virtual bool CanScorchMissedCells()
        {
            return false;
        }

        protected void FireTorpedo(TorpedoData data)
        {
            OnTorpedoFired?.Invoke(data);
        }

        protected List<GridPos> GetAttackLinePositions(BoardView enemyBoard, Orientation orientation)
        {
            List<GridPos> line = orientation is Orientation.West or Orientation.East
                ? enemyBoard.GetRow(ShipModel.root.y, orientation)
                : enemyBoard.GetColumn(ShipModel.root.x, orientation);

            var coords = new List<GridPos>();
            foreach (var pos in line)
            {
                coords.Add(pos);
                if (enemyBoard.HasShipAt(pos))
                    break;
            }

            return coords;
        }

    }

    public class SubAttackPattern : ShipAttackPattern
    {
        public SubAttackPattern(ShipModel shipModel, int attackLevel, int specialAbilityLevel) : base(
            shipModel, attackLevel, specialAbilityLevel)
        {
        }


        public override List<GridPos> GetAttackPositions(BoardView enemyBoard)
        {
            var coords = new List<GridPos>();
            if (Round % 2 == 1 || IsSpecialAttack)
            {
                ShipModel.submerged = false;
                CalculateSubLineOfAttack(enemyBoard, coords, ShipModel.orientation);
                if (CanFireTorpedoPerpendicular())
                {
                    Orientation perpendicularOrientation = ShipModel.orientation switch
                    {
                        Orientation.North => Orientation.East,
                        Orientation.East => Orientation.South,
                        Orientation.South => Orientation.West,
                        Orientation.West => Orientation.North,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    CalculateSubLineOfAttack(enemyBoard, coords, perpendicularOrientation);
                }
            }
            else
            {
                ShipModel.submerged  = true;
            }

            return coords;
        }


        public override int GetAttackDamage(ShipType targetType)
        {
            if (AttackLevel == 0) return 1;
            if (AttackLevel == 1)
            {
                return CalculateChance(0.25f) ? 2 : 1;
            }

            return CalculateChance(0.5f) ? 2 : 1;
        }

        private void CalculateSubLineOfAttack(BoardView enemyBoard, List<GridPos> coords, Orientation orientation)
        {
            var subLineOfFire = GetAttackLinePositions(enemyBoard, orientation);

            FireTorpedo(new TorpedoData
            {
                Orientation = orientation,
                StartPos = subLineOfFire.First(),
                EndPos = subLineOfFire.Last()
            });
            coords.AddRange(subLineOfFire);

            if (AttackLevel > 2)
            {
                Orientation oppositeOrientation = orientation switch
                {
                    Orientation.North => Orientation.South,
                    Orientation.East => Orientation.West,
                    Orientation.South => Orientation.North,
                    Orientation.West => Orientation.East,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var secondSubLineOfFire = GetAttackLinePositions(enemyBoard, oppositeOrientation);
                FireTorpedo(new TorpedoData
                {
                    Orientation = oppositeOrientation,
                    StartPos = secondSubLineOfFire.First(),
                    EndPos = secondSubLineOfFire.Last()
                });
                coords.AddRange(secondSubLineOfFire);
            }
        }

        private bool CanFireTorpedoPerpendicular()
        {
            if (!IsSpecialAttack || SpecialAbilityLevel == 0) return false;
            return SpecialAbilityLevel switch
            {
                1 => CalculateChance(0.1f),
                2 => CalculateChance(0.2f),
                > 2 => CalculateChance(0.3f),
                _ => false
            };
        }
    }

    public class DestroyerAttackPattern : ShipAttackPattern
    {
        public DestroyerAttackPattern(ShipModel shipModel, int attackLevel, int specialAbilityLevel) : base(shipModel,
            attackLevel, specialAbilityLevel)
        {
        }

        public override List<GridPos> GetAttackPositions(BoardView enemyBoard)
        {
            var coords = new List<GridPos>();
            if (ShipModel.reserved.x < 0 || ShipModel.reserved.y < 0)
            {
                ShipModel.reserved = ShipModel.root;
            }

            // AI destroyer target is set during AI movement/placement now

            if (AttackLevel > 0)
            {
                coords.AddRange(enemyBoard.GetRandomPositionAroundThePoint(ShipModel.reserved, 1));
            }

            if (AttackLevel > 2 && Round % 3 == 0)
            {
                // Firing a torpedo 90 degrees clockwise
                Orientation or = ShipModel.orientation switch
                {
                    Orientation.North => Orientation.East,
                    Orientation.East => Orientation.South,
                    Orientation.South => Orientation.West,
                    Orientation.West => Orientation.North,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var torpedoAttackPos = GetAttackLinePositions(enemyBoard, or);
                FireTorpedo(new TorpedoData
                {
                    Orientation = or,
                    StartPos = torpedoAttackPos.First(),
                    EndPos = torpedoAttackPos.Last()
                });
                coords.AddRange(torpedoAttackPos);
            }

            coords.Add(ShipModel.reserved);
            return coords;
        }


        public override bool CanIncapacitate(ShipType targetType)
        {
            return AttackLevel > 1;
        }

        public override int GetAttackDamage(ShipType targetType)
        {
            return IsSpecialAttack ? 1000 : 1;
        }
    }

    public class CruiserAttackPattern : ShipAttackPattern
    {
        public CruiserAttackPattern(ShipModel shipModel, int attackLevel, int specialAbilityLevel) : base(shipModel,
            attackLevel, specialAbilityLevel)
        {
        }

        public override List<GridPos> GetAttackPositions(BoardView enemyBoard)
        {
            var coords = new List<GridPos>();
            int hitCount = GetNumberOfHits();
            coords.AddRange(enemyBoard.CruiserAttack(ShipModel.GetCells(), ShipModel.orientation, hitCount));
            if (IsSpecialAttack)
            {
                int numOfRandomRoots = SpecialAbilityLevel == 0 ? 2 : 3;
                var randomRoots = enemyBoard.GetRandomPositions(numOfRandomRoots);
                foreach (var root in randomRoots)
                {
                    coords.AddRange(enemyBoard.CruiserAttack(ShipModel.GetCells(root), ShipModel.orientation,
                        hitCount));
                }
            }

            return coords;
        }

        private int GetNumberOfHits()
        {
            if(IsSpecialAttack && SpecialAbilityLevel > 2) return 9;
            return AttackLevel switch
            {
                1 => 5,
                > 2 => 7,
                _ => 3
            };
        }

        public override bool CanFreezeTarget(ShipType targetType)
        {
            if (AttackLevel > 1) return true;
            return false;
        }

        public override int GetAttackDamage(ShipType targetType)
        {
            if (AttackLevel > 2 && targetType == ShipType.Destroyer)
            {
                return 2;
            }

            return 1;
        }

        public override bool CanScorchMissedCells()
        {
            if(IsSpecialAttack && SpecialAbilityLevel > 1) return true;
            return false;     
        }
    }

    public class BattleShipAttackPattern : ShipAttackPattern
    {
        public BattleShipAttackPattern(ShipModel shipModel, int attackLevel, int specialAbilityLevel) : base(shipModel,
            attackLevel, specialAbilityLevel)
        {
        }

        public override List<GridPos> GetAttackPositions(BoardView enemyBoard)
        {
            var coords = new List<GridPos>();
            AddAttackPositions(enemyBoard, coords);

            if (AttackLevel <= 2) return coords;
            bool canAttackAgain = coords.All(pos => !enemyBoard.HasShipAt(pos));

            if (canAttackAgain)
            {
                AddAttackPositions(enemyBoard, coords);
            }

            return coords;
        }

        private void AddAttackPositions(BoardView enemyBoard, List<GridPos> coords)
        {
            int count = GetNumberOfHits();
            coords.AddRange(enemyBoard.GetRandomPositions(count));
        }

        private int GetNumberOfHits()
        {
            int numOfHits = AttackLevel switch
            {
                0 => 4,
                1 => 6,
                > 1 => 8,
                _ => 4
            };
            if (IsSpecialAttack)
            {
                numOfHits *= SpecialAbilityLevel switch
                {
                    0 => 3,
                    > 1 => 4,
                    _ => 3
                };
            }
            return numOfHits;
        }

        public override bool CanIncapacitate(ShipType targetType)
        {
            if (SpecialAbilityLevel > 1) return true;
            return false;
        }
    }

    public class TorpedoData
    {
        public Orientation Orientation;
        public GridPos StartPos;
        public GridPos EndPos;
    }
}