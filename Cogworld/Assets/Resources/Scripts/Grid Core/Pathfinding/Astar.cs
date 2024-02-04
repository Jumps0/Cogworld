using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar
{
    public Spot[,] Spots;
    public List<Spot> openSet = new List<Spot>();
    public List<Spot> closedSet = new List<Spot>();
    public List <Spot> path = new List<Spot>();
    public Spot start;
    public Spot end;
    public AStarSearchStatus searchStatus;

    public Astar(GameObject[,] grid)
    {
        Spots = new Spot[grid.GetLength(0), grid.GetLength(1)];
    }

    public void CreatePath(GameObject[,] grid, int startX = 0, int startY = 0, int endX = 10, int endY = 5)
    {
        searchStatus = AStarSearchStatus.Searching;

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] != null)
                {
                    bool height;
                    if (grid[i, j].GetComponent<TileBlock>())
                    {
                        height = grid[i, j].GetComponent<TileBlock>().occupied;
                    }
                    else // Probably a door, access, or something like that
                    {
                        height = true;
                    }

                    Spots[i, j] = new Spot(i, j, height); // < < Here > >
                }
            }
        }

        start = Spots[startX, startY];
        end = Spots[endX, endY];
        openSet.Add(start);

        while(openSet.Count > 0)
        {
            int winner = 0;
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].F < openSet[winner].F)
                {
                    winner = i;
                }
                else if (openSet[i].F == openSet[winner].F)
                {
                    if (openSet[i].H < openSet[winner].H)
                    {
                        winner = i;
                    }
                }
            }
            Spot current = openSet[winner];

            path = new List<Spot>();
            var temp = current;
            path.Add(temp);
            while (temp.previous != null)
            {
                path.Add(temp.previous);
                temp = temp.previous;
            }

            if(current == end)
            {
                searchStatus = AStarSearchStatus.Success;
                break; // We are done, stop.
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if(current.neighbors.Count == 0)
            {
                current.AddNeighbors(Spots);
            }

            var neighbors = current.neighbors;
            for (int i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                if (!closedSet.Contains(n) && n.Height < 1)
                {
                    int tempG = current.G + 1;
                    bool newPath = false;
                    if (openSet.Contains(n))
                    {
                        if(tempG < n.G)
                        {
                            n.G = tempG;
                            newPath = true;
                        }
                    }
                    else
                    {
                        n.G = tempG;
                        newPath = true;
                        openSet.Add(n);
                    }
                    if (newPath)
                    {
                        n.H = Heuristic(n, end);
                        n.F = n.G + n.H;
                        n.previous = current;
                    }
                }
            }
        }

        // If we reached here, we (most likely) failed to find a path
        if(openSet.Count == 0)
            searchStatus = AStarSearchStatus.Failure;

    }

    private int Heuristic(Spot a, Spot b)
    {
        // http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html

        /* (Manhattan)
         * dx = abs(node.x - goal.x)
           dy = abs(node.y - goal.y)
           return D * (dx + dy)
         */

        int D = 1;

        // Diagonal
        var dx = Mathf.Abs(a.X - b.X);
        var dy = Mathf.Abs(a.Y - b.Y);
        //return D * (dx + dy) + ((D * 2) - 2 * D) * Mathf.Min(dx, dy);
        return D * (int)Mathf.Sqrt(dx * dx + dy * dy); // This is more resource intensive but (maybe?) better. [Euclidean Distance]
    }
}

public class Spot
{
    public int X;
    public int Y;

    public int F;
    public int G;
    public int H;
    public int Height;

    public List<Spot> neighbors = new List<Spot>();
    public Spot previous;

    public Spot(int x, int y, bool height)
    {
        F = 0;
        G = 0;
        H = 0;
        X = x;
        Y = y;
        if (height)
        {
            Height = 1;
        }
        else
        {
            Height = 0;
        }
    }

    public void AddNeighbors(Spot[,] grid)
    {
        // We want to include diagonals into this.

        if(X < grid.GetLength(0) - 1) // [ RIGHT ]
        {
            neighbors.Add(grid[X + 1, Y]);
        }
        if (X > 0) // [ LEFT ]
        {
            neighbors.Add(grid[X - 1, Y]);
        }
        if (Y < grid.GetLength(1) - 1) // [ UP ]
        {
            neighbors.Add(grid[X, Y + 1]);
        }
        if (Y > 0) // [ DOWN ]
        {
            neighbors.Add(grid[X, Y - 1]);
        }
        // -- 
        // Diagonals
        // --
        if (X < grid.GetLength(0) - 1 && Y < grid.GetLength(1) - 1) // [ UP-RIGHT ]
        {
            neighbors.Add(grid[X + 1, Y + 1]);
        }
        if (Y < grid.GetLength(1) - 1 && X > 0) // [ UP-LEFT ]
        {
            neighbors.Add(grid[X - 1, Y + 1]);
        }
        if (Y > 0 && X > 0) // [ DOWN-LEFT ]
        {
            neighbors.Add(grid[X - 1, Y - 1]);
        }
        if (Y > 0 && X < grid.GetLength(0) - 1) // [ DOWN-RIGHT ]
        {
            neighbors.Add(grid[X + 1, Y - 1]);
        }
    }
}

public enum AStarSearchStatus
{
    Searching,
    Success,
    Failure
}
