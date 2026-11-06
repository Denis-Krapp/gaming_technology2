using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator
{
    public int width = 64;
    public int height = 36;
    [Range(0, 100)] public int fillPercent = 45;
    public int iterations = 4;
    public int seed;
    public int lastRegionCount;
    public float lastFloorPercent;


    public bool[,] grid;
    public Vector2Int startPos;
    public Vector2Int exitPos;
    public List<Vector2Int> pathNodes = new List<Vector2Int>();
    public Dictionary<Vector2Int, char> items = new Dictionary<Vector2Int, char>();

    public struct ItemStats
    {
        public int common, uncommon, rare, epic, legendary;
    }
    public ItemStats lastItemCounts;

    private System.Random prng;

    private class Node
    {
        public Vector2Int pos;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        public Node parent;
        
        public Node(Vector2Int pos, float g, float h, Node p) 
        { 
            this.pos = pos; 
            gCost = g; 
            hCost = h; 
            parent = p; 
        }
    }

    public LevelGenerator(int width, int height, int fillPercent, int iterations, int seed)
    {
        this.width = width;
        this.height = height;
        this.fillPercent = fillPercent;
        this.iterations = iterations;
        this.seed = seed;
    }

    public bool GenerateAndValidate(out int attemptsNeeded, float minFloorPercent = 0.25f, int minPathLength = 30)
    {
        int currentSeed = seed;
        attemptsNeeded = 0;
        int maxAttempts = 1000;

        while (attemptsNeeded < maxAttempts)
        {
            attemptsNeeded++;
            prng = new System.Random(currentSeed);

            InitializeGrid();
            for (int i = 0; i < iterations; i++)
            {
                SmoothGrid();
            }

            List<List<Vector2Int>> regions = FindRegions();
            lastRegionCount = regions.Count;
            if (regions.Count == 0)
            {
                currentSeed++;
                continue;
            }

            Repair(regions);

            float floorCount = 0;
            int totalCells = width * height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y]) floorCount++;
                }
            }
            float floorPercent = floorCount / totalCells;
            lastFloorPercent = floorPercent * 100f;
            if (floorPercent < minFloorPercent)
            {
                currentSeed++;
                continue;
            }

            if (!FindStartAndExit(out startPos, out exitPos))
            {
                currentSeed++;
                continue;
            }

            pathNodes = FindPathAStar(startPos, exitPos);
            if (pathNodes == null || pathNodes.Count < minPathLength)
            {
                currentSeed++;
                continue;
            }

            ScatterLoot(20);
            seed = currentSeed;
            return true;
        }

        Debug.LogError("Превышен лимит попыток генерации! Условия слишком жесткие.");
        return false;
    }

    private void InitializeGrid()
    {
        grid = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    grid[x, y] = true;
                }
                else
                {
                    grid[x, y] = prng.Next(0, 100) < fillPercent;
                }
            }
        }
    }

    private void SmoothGrid()
    {
        bool[,] newGrid = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    newGrid[x, y] = true;
                    continue;
                }

                int wallNeighbors = CountWallNeighbors(grid, x, y);
                if (grid[x, y])
                {
                    newGrid[x, y] = wallNeighbors >= 4;
                }
                else
                {
                    newGrid[x, y] = wallNeighbors >= 5;
                }
            }
        }
        grid = newGrid;
    }

    private int CountWallNeighbors(bool[,] currentGrid, int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    count++;
                }
                else if (currentGrid[nx, ny])
                {
                    count++;
                }
            }
        }
        return count;
    }

    public List<List<Vector2Int>> FindRegions()
    {
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y] && !visited[x, y])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();

                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int tile = queue.Dequeue();
                        region.Add(tile);

                        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                        foreach (var d in dirs)
                        {
                            int nx = tile.x + d.x;
                            int ny = tile.y + d.y;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (!grid[nx, ny] && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue(new Vector2Int(nx, ny));
                                }
                            }
                        }
                    }
                    regions.Add(region);
                }
            }
        }
        return regions;
    }

    private void Repair(List<List<Vector2Int>> regions)
    {
        if (regions.Count <= 1) return;

        int largestIndex = 0;
        int maxSize = regions[0].Count;
        for (int i = 1; i < regions.Count; i++)
        {
            if (regions[i].Count > maxSize)
            {
                maxSize = regions[i].Count;
                largestIndex = i;
            }
        }

        for (int i = 0; i < regions.Count; i++)
        {
            if (i == largestIndex) continue;
            foreach (var tile in regions[i])
            {
                grid[tile.x, tile.y] = true;
            }
        }
    }

    private bool FindStartAndExit(out Vector2Int start, out Vector2Int exit)
    {
        start = Vector2Int.zero;
        exit = Vector2Int.zero;

        bool foundStart = false;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y])
                {
                    start = new Vector2Int(x, y);
                    foundStart = true;
                    break;
                }
            }
            if (foundStart) break;
        }

        if (!foundStart) return false;

        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        distances[start] = 0;
        int maxDist = 0;
        exit = start;

        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            int currDist = distances[curr];

            if (currDist > maxDist)
            {
                maxDist = currDist;
                exit = curr;
            }

            foreach (var d in dirs)
            {
                Vector2Int next = curr + d;
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                {
                    if (!grid[next.x, next.y] && !distances.ContainsKey(next))
                    {
                        distances[next] = currDist + 1;
                        queue.Enqueue(next);
                    }
                }
            }
        }

        return maxDist > 0;
    }

    private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int target)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start, 0, Mathf.Abs(start.x - target.x) + Mathf.Abs(start.y - target.y), null);
        openSet.Add(startNode);

        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current.pos);

            if (current.pos == target)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node curr = current;
                while (curr != null)
                {
                    path.Add(curr.pos);
                    curr = curr.parent;
                }
                path.Reverse();
                return path;
            }

            foreach (var d in dirs)
            {
                Vector2Int neighborPos = current.pos + d;
                if (neighborPos.x < 0 || neighborPos.x >= width || neighborPos.y < 0 || neighborPos.y >= height) continue;
                if (grid[neighborPos.x, neighborPos.y] || closedSet.Contains(neighborPos)) continue;

                float tentativeGCost = current.gCost + 1f;

                Node neighborNode = openSet.Find(n => n.pos == neighborPos);
                if (neighborNode == null)
                {
                    float h = Mathf.Abs(neighborPos.x - target.x) + Mathf.Abs(neighborPos.y - target.y);
                    neighborNode = new Node(neighborPos, tentativeGCost, h, current);
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = current;
                }
            }
        }
        return null;
    }

    private void ScatterLoot(int count)
    {
        items.Clear();
        lastItemCounts = new ItemStats();

        int[] weights = { 60, 25, 10, 4, 1 };
        char[] letters = { 'C', 'U', 'R', 'E', 'L' };

        List<Vector2Int> availableFloors = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!grid[x, y] && pos != startPos && pos != exitPos)
                {
                    availableFloors.Add(pos);
                }
            }
        }

        for (int i = 0; i < availableFloors.Count; i++)
        {
            int j = prng.Next(i, availableFloors.Count);
            var temp = availableFloors[i];
            availableFloors[i] = availableFloors[j];
            availableFloors[j] = temp;
        }

        int placed = 0;
        for (int i = 0; i < availableFloors.Count && placed < count; i++)
        {
            Vector2Int pos = availableFloors[i];
            char rarityChar = RollRarity(weights, letters);
            items[pos] = rarityChar;

            if (rarityChar == 'C') lastItemCounts.common++;
            else if (rarityChar == 'U') lastItemCounts.uncommon++;
            else if (rarityChar == 'R') lastItemCounts.rare++;
            else if (rarityChar == 'E') lastItemCounts.epic++;
            else if (rarityChar == 'L') lastItemCounts.legendary++;

            placed++;
        }
    }

    private char RollRarity(int[] weights, char[] letters)
    {
        int totalWeight = 0;
        foreach (int w in weights) totalWeight += w;

        int roll = prng.Next(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
            {
                return letters[i];
            }
        }
        return letters[0];
    }

    public string PrintLevelToString()
    {
        char[,] display = new char[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                display[x, y] = grid[x, y] ? '#' : '.';
            }
        }

        if (pathNodes != null)
        {
            foreach (var p in pathNodes)
            {
                if (p != startPos && p != exitPos)
                    display[p.x, p.y] = '*';
            }
        }

        foreach (var kvp in items)
        {
            if (kvp.Key != startPos && kvp.Key != exitPos)
            {
                display[kvp.Key.x, kvp.Key.y] = kvp.Value;
            }
        }

        display[startPos.x, startPos.y] = 'S';
        display[exitPos.x, exitPos.y] = 'E';

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(display[x, y]);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}