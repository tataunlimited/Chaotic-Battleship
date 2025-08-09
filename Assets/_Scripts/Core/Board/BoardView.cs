using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Core.Board
{
    public class BoardView : MonoBehaviour
    {
        [Header("Config")]
        public BoardSide side = BoardSide.Player;
        public int width = 10;
        public int height = 10;
        public float cellSize = 1f;
        public Vector3 origin = Vector3.zero;
        public GameObject cellPrefab;

        public BoardModel Model { get; private set; }

        private readonly Dictionary<GridPos, Renderer> _tiles = new();

        void Awake()
        {
            Model = new BoardModel(side, width, height);
            if (cellPrefab) BuildGrid();
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

        public void Tint(GridPos p, Color c)
        {
            if (_tiles.TryGetValue(p, out var r) && r.material.HasProperty("_Color"))
                r.material.color = c;
        }

        // Optional: quick gizmos
        void OnDrawGizmosSelected()
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
    }
}
