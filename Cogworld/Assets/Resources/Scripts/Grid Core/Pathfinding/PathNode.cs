using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridCore<PathNode> grid;
    private int x;
    private int y;

    public int gCost;
    public int hCost;
    public int fCost;

    // Previous Node
    public PathNode cameFromNode;

    public PathNode(GridCore<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}
