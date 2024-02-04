using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding{

    private GridCore<PathNode> grid;

    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height)
    {
        grid = new GridCore<PathNode>(width, height, 10f, Vector3.zero, (GridCore<PathNode> g, int x, int y) => new PathNode(grid, x, y));

    }
    /*
    private List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = 

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();
    }
    */
}
