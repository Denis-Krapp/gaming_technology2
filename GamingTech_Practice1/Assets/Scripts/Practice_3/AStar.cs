using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public static List<GridNode> FindPath(Grid grid, GridNode startNode, GridNode targetNode, bool allowDiagonals, HeuristicType heuristic)
    {
        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        openSet.Add(startNode);

        int nodesExpanded = 0;

        while (openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            nodesExpanded++;

            if (currentNode == targetNode)
            {
                Debug.Log($"<color=green>[A* Done]</color> Nodes Expanded: {nodesExpanded}, Total Cost: {targetNode.gCost}");
                return RetracePath(startNode, targetNode);
            }

            foreach (GridNode neighbor in grid.GetNeighbors(currentNode, allowDiagonals))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetRealDistance(currentNode, neighbor);
                
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    
                    neighbor.hCost = CalculateHeuristic(neighbor, targetNode, heuristic);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<GridNode>();
    }

    private static int GetRealDistance(GridNode nodeA, GridNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);
        return (dstX == 1 && dstY == 1) ? 14 : 10;
    }

    private static int CalculateHeuristic(GridNode node, GridNode target, HeuristicType type)
    {
        int dx = Mathf.Abs(node.x - target.x);
        int dbY = Mathf.Abs(node.y - target.y);

        switch (type)
        {
            case HeuristicType.Manhattan:
                return 10 * (dx + dbY);
            case HeuristicType.Euclidean:
                return Mathf.RoundToInt(10f * Mathf.Sqrt(dx * dx + dbY * dbY));

            case HeuristicType.Diagonal:
                if (dx > dbY)
                    return 14 * dbY + 10 * (dx - dbY);
                return 14 * dx + 10 * (dbY - dx);

            default:
                return 0;
        }
    }

    private static List<GridNode> RetracePath(GridNode startNode, GridNode targetNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = targetNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
}