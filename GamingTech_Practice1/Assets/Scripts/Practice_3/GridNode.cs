using UnityEngine;

public class GridNode
{
    public int x;
    public int y;
    public bool walkable;
    
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public GridNode parent;

    public GridNode(int _x, int _y, bool _walkable)
    {
        x = _x;
        y = _y;
        walkable = _walkable;
    }
}