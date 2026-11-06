using System.Collections.Generic;
using UnityEngine;


public enum HeuristicType
{
    Manhattan,
    Euclidean,
    Diagonal
}

public class Grid : MonoBehaviour
{
    [Header("Grid Dimensions")]
    public int width = 15;
    public int height = 10;
    public float nodeSize = 1f;

    [Header("Obstacles Generation")]
    [Range(0, 100)] public int obstaclePercent = 20;

    [Header("Pathfinding Coordinates")]
    public Vector2Int startCoords = new Vector2Int(1, 1);
    public Vector2Int targetCoords = new Vector2Int(13, 8);
    public bool allowDiagonals = false;

    private GridNode[,] grid;
    private List<GridNode> finalPath;

    [Header("Heuristic Settings")]
    public HeuristicType heuristicType = HeuristicType.Manhattan;

    void Awake()
    {
        GenerateGrid();
    }

    void Update()
    {
        CalculatePath();
    }

    void GenerateGrid()
    {
        grid = new GridNode[width, height];
        Random.InitState(42);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if ((x == startCoords.x && y == startCoords.y) || (x == targetCoords.x && y == targetCoords.y))
                {
                    grid[x, y] = new GridNode(x, y, true);
                    continue;
                }

                bool walkable = Random.Range(0, 100) >= obstaclePercent;
                grid[x, y] = new GridNode(x, y, walkable);
            }
        }
    }

    void CalculatePath()
    {
        if (grid == null) return;

        startCoords.x = Mathf.Clamp(startCoords.x, 0, width - 1);
        startCoords.y = Mathf.Clamp(startCoords.y, 0, height - 1);
        targetCoords.x = Mathf.Clamp(targetCoords.x, 0, width - 1);
        targetCoords.y = Mathf.Clamp(targetCoords.y, 0, height - 1);

        GridNode startNode = grid[startCoords.x, startCoords.y];
        GridNode targetNode = grid[targetCoords.x, targetCoords.y];

        finalPath = AStar.FindPath(this, startNode, targetNode, allowDiagonals, heuristicType);
    }

    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) return grid[x, y];
        return null;
    }

    public List<GridNode> GetNeighbors(GridNode node, bool includeDiagonals)
    {
        List<GridNode> neighbors = new List<GridNode>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0) continue;

                if (!includeDiagonals && Mathf.Abs(xOffset) == 1 && Mathf.Abs(yOffset) == 1) continue;

                int checkX = node.x + xOffset;
                int checkY = node.y + yOffset;

                GridNode neighbor = GetNode(checkX, checkY);
                if (neighbor != null) neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    void OnDrawGizmos()
    {
        if (grid == null)
        {
            Gizmos.color = Color.gray;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Gizmos.DrawWireCube(new Vector3(x * nodeSize, 0, y * nodeSize), new Vector3(nodeSize - 0.1f, 0.1f, nodeSize - 0.1f));
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridNode node = grid[x, y];
                Vector3 worldPos = new Vector3(x * nodeSize, 0, y * nodeSize);

                if (!node.walkable) Gizmos.color = Color.black;
                else if (x == startCoords.x && y == startCoords.y) Gizmos.color = Color.blue;
                else if (x == targetCoords.x && y == targetCoords.y) Gizmos.color = Color.red;
                else if (finalPath != null && finalPath.Contains(node)) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.white;
                Gizmos.DrawCube(worldPos, new Vector3(nodeSize - 0.05f, 0.1f, nodeSize - 0.05f));
            }
        }
    }
}