using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Core.Pathfinding
{
    /// <summary>
    /// A generic implementation of the A* pathfinding algorithm.
    /// It's designed to be reusable and doesn't know about ships or specific game rules.
    /// </summary>
    public static class Pathfinder
    {
        private class Node
        {
            public GridPos Position { get; }
            public Node Parent { get; set; }
            public int GCost { get; set; } // Distance from starting node
            public int HCost { get; set; } // Heuristic distance to end node
            public int FCost => GCost + HCost; // Total cost

            public Node(GridPos position)
            {
                Position = position;
            }
        }

        public static List<GridPos> FindPath(GridPos startPos, GridPos endPos, int width, int height, HashSet<GridPos> unwalkablePositions)
        {
            Node startNode = new Node(startPos);
            Node endNode = new Node(endPos);

            List<Node> openList = new List<Node> { startNode };
            HashSet<GridPos> closedSet = new HashSet<GridPos>();

            while (openList.Count > 0)
            {
                // Find the node with the lowest F cost in the open list
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < currentNode.FCost || (openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost))
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedSet.Add(currentNode.Position);

                // Path found
                if (currentNode.Position.Equals(endPos))
                {
                    return RetracePath(startNode, currentNode);
                }

                // Check neighbors
                foreach (Node neighbor in GetNeighbors(currentNode, width, height))
                {
                    if (unwalkablePositions.Contains(neighbor.Position) || closedSet.Contains(neighbor.Position))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, endNode);
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        private static List<GridPos> RetracePath(Node startNode, Node endNode)
        {
            List<GridPos> path = new List<GridPos>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }
            path.Reverse();
            return path;
        }

        private static int GetDistance(Node nodeA, Node nodeB)
        {
            // Manhattan distance for a 4-directional grid
            int dstX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
            int dstY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
            return dstX + dstY;
        }

        private static IEnumerable<Node> GetNeighbors(Node node, int width, int height)
        {
            List<Node> neighbors = new List<Node>();

            // Orthogonal directions
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = node.Position.x + dx[i];
                int checkY = node.Position.y + dy[i];

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(new Node(new GridPos(checkX, checkY)));
                }
            }
            return neighbors;
        }
    }
}
