using System;
using System.Collections.Generic;
using Core.GridSystem;
using Core.Ship;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Board
{
    public class BoardView : MonoBehaviour
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        [Header("Config")]
        public BoardSide side = BoardSide.Player;
        public bool revealShips;
        public bool isPlayer;
        public int width = 10;
        public int height = 10;
        public float cellSize = 1f;
        public Vector3 origin = Vector3.zero;
        public GameObject cellPrefab;

        public Color baseColor = Color.cyan;

        public BoardModel Model { get; private set; }
        public Dictionary<string, ShipView> SpawnedShips { get; } = new();

        private readonly Dictionary<GridPos, Renderer> _tiles = new();
        private readonly Dictionary<string, ShipView> _spawnedShips = new();

        private Dictionary<string, ShipModel> previousShipPlacements = new();
        
        private int _lastShipId = 0;


        void Awake()
        {
            Model = new BoardModel(side, width, height);
            if (cellPrefab) BuildGrid();
        }

        public void Reset()
        {
            //HideShips();
            DestroyShips();
            _spawnedShips.Clear();
            previousShipPlacements.Clear();
            ResetIndicators();
        }

        internal void SaveShipLocations()
        {
            // TODO: might want to save the BoardModel _cells as well. Can use BoardModel.Copy(), but need to remove the previous ship locations 
            previousShipPlacements.Clear();
            foreach (var pair in _spawnedShips)
            {
                previousShipPlacements.Add(pair.Key, pair.Value.shipModel.Copy());
            }
        }

        // returns true if all ships were moved back to their previous location
        internal bool ResetMovementPhase()
        {
            ShipModel previousShipData;
            bool wasSuccessful = true;

            // need to remove ships so they don't interfere with placing them back where they were
            Model.ResetAllCells();

            foreach (var pair in _spawnedShips)
            {
                wasSuccessful &= previousShipPlacements.TryGetValue(pair.Key, out previousShipData);
                pair.Value.UpdatePosition(previousShipData.root, previousShipData.orientation);
            }

            return wasSuccessful;
        }

        void BuildGrid()
        {
            var parent = new GameObject($"{side}Board").transform;
            parent.SetParent(transform, false);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var p = new GridPos(x, y);
                    var go = Instantiate(cellPrefab, GridToWorld(p), Quaternion.identity, parent);
                    go.name = $"{side}_Cell_{x}_{y}";
                    if (go.TryGetComponent(out Renderer r)) _tiles[p] = r;
                }

            UpdateBoard();
        }

        public Vector3 GridToWorld(GridPos p, float yOffset = 0f) =>
            origin + new Vector3((p.x + 0.5f) * cellSize, yOffset, (p.y + 0.5f) * cellSize);

        public bool WorldToGrid(Vector3 world, out GridPos p)
        {
            Vector3 local = world - origin;
            int x = Mathf.FloorToInt(local.x / cellSize);
            int y = Mathf.FloorToInt(local.z / cellSize);
            p = new GridPos(x, y);
            return Model.InBounds(p);
        }

        private void Tint(GridPos p, Color c)
        {
            if (_tiles.TryGetValue(p, out var r) && r.material.HasProperty(Color1))
                r.material.color = c;
        }

        public void Tint(List<GridPos> positions)
        {
            foreach (var p in positions)
            {
                Tint(p, GetColor(p));
            }
        }

        public void Tint(GridPos p)
        {
            Tint(p, GetColor(p));
        }

        private Color GetColor(GridPos p)
        {
            Color c = Model.Get(p) switch
            {
                CellState.Empty => baseColor,
                CellState.Ship => Color.green,
                CellState.Hit => Color.red,
                CellState.Miss => Color.gray,
                _ => throw new ArgumentOutOfRangeException()
            };
            return c;
        }

        // Optional: quick gizmos
        void OnDrawGizmos()
        {
            Gizmos.color = side == BoardSide.Player ? Color.green : Color.red;
            for (int x = 0; x <= width; x++)
            {
                var a = origin + new Vector3(x * cellSize, 0, 0);
                var b = a + new Vector3(0, 0, height * cellSize);
                Gizmos.DrawLine(a, b);
            }
            for (int y = 0; y <= height; y++)
            {
                var a = origin + new Vector3(0, 0, y * cellSize);
                var b = a + new Vector3(width * cellSize, 0, 0);
                Gizmos.DrawLine(a, b);
            }
        }

        public bool TryPlaceShip(ShipView prefab, GridPos pos, Orientation orientation)
        {

            string id = name + _lastShipId;
            var shipModel = prefab.shipModel.Copy();
            shipModel.id = id;
            shipModel.orientation = orientation;
            shipModel.root = pos;

            bool success = Model.TryPlaceShip(shipModel);
            if (success)
            {
                var shipView = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                shipView.Init(this, shipModel, isPlayer);

                _lastShipId++;
                SpawnedShips.Add(id, shipView);
            }

            if (!revealShips || !success) return success;
            foreach (var gridPos in shipModel.GetCells())
            {
                Tint(gridPos, Color.green);
            }

            return true;
        }

        public void UpdateBoard(bool showShips = true)
        {
            foreach (var ship in SpawnedShips)
            {
                Model.TryPlaceShip(ship.Value.shipModel);
            }
            foreach (var tile in _tiles)
            {
                Tint(tile.Key, GetColor(tile.Key));
            }

            if (!showShips)
            {
                HideShips();
            }
        }

        public List<GridPos> GetRandomPositions(int count)
        {
            var randomPositions = new List<GridPos>();
            for (int i = 0; i < count; i++)
            {
                randomPositions.Add(new GridPos(
                    Random.Range(0, width),
                    Random.Range(0, height)
                    ));
            }
            return randomPositions;
        }
        public List<GridPos> GetRow(int rowIndex, Orientation orientaion)
        {
            var row = new List<GridPos>();
            if (orientaion is Orientation.North or Orientation.South)
            {
                Debug.LogError("Cannot return a row for North or South orientation");
                return row;
            }
            if (orientaion is Orientation.East)
                for (int i = 0; i < height; i++)
                {
                    row.Add(new GridPos(i, rowIndex));
                }
            else
                for (int i = width - 1; i > -1; i--)
                {
                    row.Add(new GridPos(i, rowIndex));
                }
            return row;
        }
        public List<GridPos> GetColumn(int colIndex, Orientation orientaion)
        {
            var col = new List<GridPos>();
            if (orientaion is Orientation.East or Orientation.West)
            {
                Debug.LogError("Cannot return a column for East or West orientation");
                return col;
            }
            if (orientaion is Orientation.North)
                for (int i = 0; i < height; i++)
                {
                    col.Add(new GridPos(colIndex, i));
                }
            else
                for (int i = height - 1; i > -1; i--)
                {
                    col.Add(new GridPos(colIndex, i));
                }

            return col;
        }

        public List<GridPos> CruiserAttack(List<GridPos> shipCells, Orientation orientaion)
        {
            var attackCells = new List<GridPos>();

            // Loop through a 3x3 area centered on the "center" position
            if (orientaion is Orientation.North or Orientation.South)
            {
                for (int i = 0; i < shipCells.Count; i++)
                {
                    var pos1 = new GridPos(shipCells[i].x - 1, shipCells[i].y);
                    var pos3 = new GridPos(shipCells[i].x + 1, shipCells[i].y);
                    if (Model.InBounds(pos1))
                        attackCells.Add(pos1);
                    if (Model.InBounds(shipCells[i]))
                        attackCells.Add(shipCells[i]);
                    if (Model.InBounds(pos3))
                        attackCells.Add(pos3);

                }
            }
            else
            {
                for (int i = 0; i < shipCells.Count; i++)
                {
                    var pos1 = new GridPos(shipCells[i].x, shipCells[i].y - 1);
                    var pos3 = new GridPos(shipCells[i].x, shipCells[i].y + 1);
                    if (Model.InBounds(pos1))
                        attackCells.Add(pos1);
                    if (Model.InBounds(shipCells[i]))
                        attackCells.Add(shipCells[i]);
                    if (Model.InBounds(pos3))
                        attackCells.Add(pos3);

                }
            }

            var randomCells = new List<GridPos>();
            for (int i = 0; i < 3; i++)
            {
                if (attackCells.Count == 0)
                    break;
                int rndCellIndex = Random.Range(0, attackCells.Count);
                randomCells.Add(attackCells[rndCellIndex]);
                attackCells.RemoveAt(rndCellIndex);

            }
            return randomCells;
        }


        public void ResetIndicators(bool showShips = true)
        {
            Model.ResetAllCells();
            UpdateBoard(showShips);
        }

        public void RevealShips()
        {
            foreach (ShipView shipView in SpawnedShips.Values)
            {
                shipView.Show();
                RevealAShip(shipView.shipModel);
            }
        }

        private void RevealAShip(ShipModel shipModel)
        {
            foreach (var gridPos in shipModel.GetCells())
            {
                Tint(gridPos, Color.green);
            }
        }

        private void HideShips()
        {
            foreach (ShipView shipView in SpawnedShips.Values)
            {
                shipView.Hide();
                HideAShip(shipView.shipModel);
            }
        }

        private void HideAShip(ShipModel shipModel)
        {
            foreach (GridPos gp in shipModel.GetCells())
            {
                if (Model.Get(gp) == CellState.Ship)
                    Tint(gp, Color.cyan);
            }
        }

        public void DestroyShips()
        {
            foreach (ShipView shipView in SpawnedShips.Values)
            {
                Destroy(shipView.gameObject);
            }
        }
    }
}

