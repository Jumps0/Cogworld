using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Color = UnityEngine.Color;
using System;
using System.Linq;
using PathfindingDSL;
using Random = UnityEngine.Random;
using System.IO;


/// <summary>
/// Controls the main aspects of the PathfindingTest Scene, not used in main game.
/// </summary>
public class PathfindingTestControl : MonoBehaviour
{
    public static PathfindingTestControl inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Map Values")]
    public Vector2Int mapSize;
    public GameObject[,] grid; // I don't like using an array here but the A* uses it so here we go.
    public float percentWalls;

    [Header("Pathfinding")]
    public GameObject start;
    public GameObject finish;
    public Astar pathing;
    public bool useLandmark = false;

    [Header("UI")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI statusText;
    public Image statusImage;
    public TextMeshProUGUI setsText;

    [Header("Prefabs")]
    public GameObject prefab_Tile;
    public GameObject prefab_Landmark;

    [Header("Parents")]
    public GameObject parent_tiles;
    public GameObject parent_landmarks;
    public GameObject parent_actor;

    [Header("Colors")]
    public Color openColor;
    public Color closedColor;
    public Color pathColor;

    /*
    [Header("D* Lite")]
    public DStarLite<Vector2> dslite;
    public Node<Vector2> startNode;
    public Node<Vector2> goalNode;
    public List<Node<Vector2>> allNodes = new List<Node<Vector2>>();
    */

    private Node[,] nodes;
    private Node startNode;
    private Node goalNode;
    private PriorityQueue openList;
    private Vector2Int[] directions = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    private void Start()
    {
        // Create the map
        GenerateMap();
        StartCoroutine(SnapNodes());
        statusText.text = "No route.";
        infoText.text = "No route.";
    }

    #region attempt 4
    public void CalculateGraph()
    {
        InitializeNodes();
        InitializeDStarLite();
        ComputeShortestPath();
        VisualizeFinalPath();
    }

    // Initializes the nodes grid based on the GameObject[,] grid
    private void InitializeNodes()
    {
        GameObject goal = finish;
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        nodes = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWalkable = grid[x, y] != null && grid[x, y].GetComponent<SpriteRenderer>().color == Color.white;
                nodes[x, y] = new Node(new Vector2Int(x, y), isWalkable, grid[x, y]);
            }
        }

        startNode = nodes[(int)start.transform.position.x, (int)start.transform.position.y];
        goalNode = nodes[(int)goal.transform.position.x, (int)goal.transform.position.y];
    }

    // Initialize the D* Lite algorithm
    private void InitializeDStarLite()
    {
        openList = new PriorityQueue(); // Reset queue

        foreach (Node node in nodes) // Max out all nodes
        {
            node.GCost = float.MaxValue;
            node.RHS = float.MaxValue;
        }

        goalNode.RHS = 0; // Set goal node to 0
        openList.Enqueue(goalNode); // Add goal node to openList
    }

    // Main function to compute the shortest path
    private void ComputeShortestPath()
    {
        /*
        while (openList.Count > 0 && (openList.queue.Min.Item1 < startNode.CalculateKey(startNode) || startNode.GCost > startNode.RHS))
        {
            Node current = openList.Dequeue();
            current.UpdateColor(Color.blue);  // Update color of the node in the open list

            if (current.GCost > current.RHS)
            {
                current.GCost = current.RHS;
            }
            else
            {
                current.GCost = float.MaxValue;
                UpdateNode(current);
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                UpdateNode(neighbor);
            }
        }
        */
    }

    // Updates a node's costs and its position in the priority queue
    private void UpdateNode(Node node)
    {
        if (node != goalNode)
        {
            float minRHS = float.MaxValue;
            Node minNeighbor = null;
            foreach (var neighbor in GetNeighbors(node))
            {
                float cost = neighbor.GCost + 1; // Assuming uniform cost for movement
                if (cost < minRHS)
                {
                    minRHS = cost;
                    minNeighbor = neighbor;
                }
            }
            node.RHS = minRHS;
            node.Parent = minNeighbor;
        }

        if (openList.Contains(node))
        {
            openList.Remove(node);
        }

        if (node.GCost != node.RHS)
        {
            openList.Enqueue(node);
        }
    }

    // Returns a list of walkable neighboring nodes
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = node.Position + direction;
            if (IsPositionValid(neighborPos))
            {
                neighbors.Add(nodes[neighborPos.x, neighborPos.y]);
            }
        }

        return neighbors;
    }

    // Checks if a position is within the bounds of the grid and walkable
    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < nodes.GetLength(0) &&
               position.y >= 0 && position.y < nodes.GetLength(1) &&
               nodes[position.x, position.y].Walkable;
    }

    // Call this method whenever obstacles change to update the path
    public void UpdateGrid(Vector2Int position, bool isWalkable)
    {
        Node node = nodes[position.x, position.y];
        node.Walkable = isWalkable;

        if (!node.Walkable)
        {
            node.RHS = float.MaxValue;
        }

        UpdateNode(node);
        ComputeShortestPath();
    }

    // Visualize the final path from start to goal
    private void VisualizeFinalPath()
    {
        Node current = startNode;
        while (current != null && current != goalNode)
        {
            current.UpdateColor(Color.green);
            current = current.Parent;
        }
        goalNode.UpdateColor(Color.green);  // Mark the goal as part of the path
    }
    #endregion

    #region OLD 3
    /*
    readonly Func<Node<Vector2>, Node<Vector2>, float> cost = (node, neighbor) => Vector2.Distance(node.Data, neighbor.Data);
    readonly Func<Node<Vector2>, Node<Vector2>, float> heuristic = (node, neighbor) => Vector2.Distance(node.Data, neighbor.Data);

    public void CalculateGraph(/*GameObject p, bool b)
    {
        GameObject goal = finish;

        goalNode = new Node<Vector2>(goal.transform.position, cost, heuristic);
        startNode = new Node<Vector2>(start.transform.position, cost, heuristic);
        allNodes.Clear();

        foreach (GameObject waypoint in grid)
        {
            if(waypoint != null)
            {
                Node<Vector2> node = new Node<Vector2>(waypoint.transform.position, cost, heuristic);
                //waypoint.GetComponent<NodeVisualizer>().Setup(node);
                allNodes.Add(node);
            }
        }

        //start.GetComponent<NodeVisualizer>().Setup(startNode);
        //goal.GetComponenet<NodeVisualizer>().Setup(goalNode);

        // What is n???
        Node<Vector2> n = startNode;

        // Set up 3 neighbors for each node from allNodes
        foreach(var node in allNodes)
        {
            node.Neighbors = allNodes
                .OrderBy(node => Vector2.Distance(n.Data, node.Data))
                .Where(node => n != node)
                .Distinct()
                .Take(3)
                .ToList();
        }

        startNode.Neighbors = startNode.Neighbors.Where(n => n != goalNode && n != startNode).ToList();
        goalNode.Neighbors = goalNode.Neighbors.Where(n => n != goalNode && n != startNode).ToList();

        dslite = new DStarLite<Vector2>(startNode, goalNode, allNodes);
        dslite.Initialize();
        dslite.ComputeShortestPath();
        UpdatePathVisual(dslite.GetPath());

    }

    private void UpdatePathVisual(List<Node<Vector2>> path)
    {
        foreach (Node<Vector2> node in path)
        {
            if(node != startNode && node != goalNode)
            {
                Vector2 pos = node.Data;
                grid[(int)pos.x, (int)pos.y].GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
    }
    */
    #endregion

    #region MapGeneration

    private void GenerateMap()
    {
        // - We need to generate a random map for pathfinding based on the map size -

        // Set array
        grid = new GameObject[mapSize.x, mapSize.y];

        // Create border
        for (int x = 0; x < mapSize.x - 1; x++) // Bottom
        {
            PlaceTile(new Vector2Int(x, 0), true, true);
        }
        for (int x = 0; x < mapSize.x - 1; x++) // Top
        {
            PlaceTile(new Vector2Int(x, mapSize.y - 1), true, true);
        }
        for (int y = 0; y < mapSize.y - 1; y++) // Left
        {
            PlaceTile(new Vector2Int(0, y), true, true);
        }
        for (int y = 0; y < mapSize.y - 1; y++) // Right
        {
            PlaceTile(new Vector2Int(mapSize.x - 1, y), true, true);
        }

        // - Now we need to make the main map and the obstacles
        // - we could do something fancy here, but for now, we'll just have it be random
        for (int x = 1; x < mapSize.x - 2; x++) // Bottom
        {
            for (int y = 1; y < mapSize.y - 2; y++) // Left
            {
                float random = Random.Range(0f, 1f);
                bool occupied;
                if (random > percentWalls)
                {
                    occupied = false;
                }
                else
                {
                    occupied = true;
                }

                PlaceTile(new Vector2Int(x, y), occupied);
            }
        }

        // Set camera position
        Camera.main.transform.position = new Vector3(mapSize.x / 2, mapSize.y / 2, -(mapSize.x / 5));
        Camera.main.orthographicSize = mapSize.x / 2;
    }

    private void PlaceTile(Vector2Int loc, bool occupied, bool isBorder = false)
    {
        var spawnedTile = Instantiate(prefab_Tile, new Vector3(loc.x, loc.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"Tile {loc.x} {loc.y} - "; // Give grid based name

        spawnedTile.transform.parent = parent_tiles.transform; // Set parent

        spawnedTile.AddComponent<TileBlock>(); // Since A* uses this we add it here for safety
        spawnedTile.GetComponent<TileBlock>().occupied = occupied; // Set occupied var

        if (occupied)
        {
            spawnedTile.GetComponent<SpriteRenderer>().color = Color.gray;
            spawnedTile.name += "Wall";
        }
        else
        {
            spawnedTile.GetComponent<SpriteRenderer>().color = Color.white;
            spawnedTile.name += "Floor";
        }

        if (isBorder)
        {
            spawnedTile.GetComponent<SpriteRenderer>().color = Color.black;
            spawnedTile.name += "Border";
        }

        // Add to array
        grid[loc.x, loc.y] = spawnedTile;
    }


    // Snap the nodes to nearest int
    private IEnumerator SnapNodes()
    {
        while (true)
        {
            start.transform.position = new Vector3((int)start.transform.position.x, (int)start.transform.position.y);
            finish.transform.position = new Vector3((int)finish.transform.position.x, (int)finish.transform.position.y);

            yield return new WaitForSeconds(5f);
        }
    }
    #endregion

    #region OLD 2

    /*
    // Start is called before the first frame update
    void Start()
    {
        // Create the map
        GenerateMap();
        StartCoroutine(SnapNodes());
        statusText.text = "No route.";
        infoText.text = "No route.";

        Initialize(grid, HF.V3_to_V2I(start.transform.position), HF.V3_to_V2I(finish.transform.position));
    }


    #region Pathfinding (D* Lite)
    public Vector2Int startLocation;
    public Vector2Int goalLocation;

    private int gridWidth;
    private int gridHeight;
    private Dictionary<Vector2Int, float> gScores;
    private Dictionary<Vector2Int, float> rhsScores;
    private PriorityQueue<Vector2Int> openSet;
    private LandmarkHeuristic landmarkHeuristic;

    // Initialize the pathfinding grid and landmarks
    public void Initialize(GameObject[,] grid, Vector2Int start, Vector2Int goal)
    {
        this.grid = grid;
        this.startLocation = start;
        this.goalLocation = goal;

        gridWidth = grid.GetLength(0);
        gridHeight = grid.GetLength(1);

        gScores = new Dictionary<Vector2Int, float>();
        rhsScores = new Dictionary<Vector2Int, float>();
        openSet = new PriorityQueue<Vector2Int>();

        landmarkHeuristic = new LandmarkHeuristic();
        landmarkHeuristic.SetLandmarks(landmarkHeuristic.landmarks, landmarkHeuristic.precomputedDistances);

        // Initialize g and rhs values
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                gScores[pos] = float.MaxValue;
                rhsScores[pos] = float.MaxValue;
            }
        }

        rhsScores[goal] = 0;
        openSet.Enqueue(goal, CalculateKey(goal));
    }

    // Calculate the priority/key for D* Lite (used in the priority queue)
    private Vector2 CalculateKey(Vector2Int node)
    {
        float h = landmarkHeuristic.GetHeuristic(startLocation, node);
        float g = gScores.ContainsKey(node) ? gScores[node] : float.MaxValue;
        float rhs = rhsScores.ContainsKey(node) ? rhsScores[node] : float.MaxValue;
        return new Vector2(Mathf.Min(g, rhs) + h, Mathf.Min(g, rhs));
    }

    // Function to update the grid dynamically and recalculate paths
    public void UpdateMap(Vector2Int updatedPosition)
    {
        // If an obstacle or shortcut was added/removed, update the rhs and g values of the surrounding nodes
        if (grid[updatedPosition.x, updatedPosition.y].GetComponent<SpriteRenderer>().color == Color.gray)
        {
            // Blocked tile (obstacle)
            gScores[updatedPosition] = float.MaxValue;
            rhsScores[updatedPosition] = float.MaxValue;
        }
        else
        {
            // Walkable tile (open space)
            ComputeRHS(updatedPosition);
        }

        UpdateVertex(updatedPosition);
    }

    // Function to compute the rhs value of a node
    private void ComputeRHS(Vector2Int node)
    {
        if (node != goalLocation)
        {
            float minRHS = float.MaxValue;

            foreach (var direction in landmarkHeuristic.directions)
            {
                Vector2Int neighbor = node + direction;

                if (IsValidPosition(neighbor) && IsWalkable(neighbor))
                {
                    float cost = GetTraversalCost(node, neighbor);
                    minRHS = Mathf.Min(minRHS, gScores[neighbor] + cost);
                }
            }

            rhsScores[node] = minRHS;
        }
    }

    // D* Lite pathfinding function
    public void FindPath()
    {
        while (VectorCompare(openSet.PeekPriority(), CalculateKey(startLocation)) || rhsScores[startLocation] != gScores[startLocation])
        {
            Vector2Int currentNode = openSet.Dequeue();

            if (gScores[currentNode] > rhsScores[currentNode])
            {
                gScores[currentNode] = rhsScores[currentNode];
            }
            else
            {
                gScores[currentNode] = float.MaxValue;
                ComputeRHS(currentNode);
            }

            UpdateVertex(currentNode);

            foreach (var direction in landmarkHeuristic.directions)
            {
                Vector2Int neighbor = currentNode + direction;
                if (IsValidPosition(neighbor) && IsWalkable(neighbor))
                {
                    UpdateVertex(neighbor);
                }
            }
        }

        Debug.Log("Path found from start to goal.");
    }

    private bool VectorCompare(Vector2 a, Vector2 b)
    {
        // Compare x values first
        if (a.x < b.x) return true;
        if (a.x > b.x) return false;

        // If x values are equal, compare y values
        return a.y < b.y;
    }

    // Function to update a vertex and reinsert into the priority queue
    private void UpdateVertex(Vector2Int node)
    {
        if (gScores[node] != rhsScores[node])
        {
            if (openSet.Contains(node))
            {
                openSet.UpdatePriority(node, CalculateKey(node));
            }
            else
            {
                openSet.Enqueue(node, CalculateKey(node));
            }
        }
        else if (openSet.Contains(node))
        {
            openSet.Remove(node);
        }
    }

    // Helper function to get the traversal cost between nodes
    private float GetTraversalCost(Vector2Int from, Vector2Int to)
    {
        // Assume all traversal costs are 1 for simplicity
        return 1f;
    }

    // Helper function to check if a node is walkable
    private bool IsWalkable(Vector2Int position)
    {
        return grid[position.x, position.y].GetComponent<SpriteRenderer>().color == Color.white;
    }

    // Helper function to check if a position is valid within the grid
    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight;
    }

    #endregion

    private void SetPainting()
    {
        foreach (var T in pathing.openSet)
        {
            grid[T.X, T.Y].GetComponent<SpriteRenderer>().color = openColor;
        }

        foreach (var T in pathing.closedSet)
        {
            grid[T.X, T.Y].GetComponent<SpriteRenderer>().color = closedColor;
        }

        foreach (var T in pathing.path)
        {
            grid[T.X, T.Y].GetComponent<SpriteRenderer>().color = pathColor;
        }
    }

    private void ResetColors()
    {
        foreach (GameObject T in grid)
        {
            if (T)
            {
                if (T.name.Contains("Border"))
                {
                    T.GetComponent<SpriteRenderer>().color = Color.black;
                }
                else if (T.name.Contains("Wall"))
                {
                    T.GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else if (T.name.Contains("Floor"))
                {
                    T.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }


    public class PriorityQueue<T>
    {
        private List<KeyValuePair<T, Vector2>> elements = new List<KeyValuePair<T, Vector2>>();

        public int Count => elements.Count;
        public List<KeyValuePair<T, Vector2>> E => elements;

        public void Enqueue(T item, Vector2 priority)
        {
            elements.Add(new KeyValuePair<T, Vector2>(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Value.x < elements[bestIndex].Value.x ||
                    (elements[i].Value.x == elements[bestIndex].Value.x && elements[i].Value.y < elements[bestIndex].Value.y))
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Key;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public Vector2 PeekPriority()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Value.x < elements[bestIndex].Value.x ||
                    (elements[i].Value.x == elements[bestIndex].Value.x && elements[i].Value.y < elements[bestIndex].Value.y))
                {
                    bestIndex = i;
                }
            }

            Debug.Log($"E:{elements} | Count: {elements.Count} | i:{bestIndex}");
            return elements[bestIndex].Value;
        }

        public bool Contains(T item)
        {
            return elements.Exists(x => x.Key.Equals(item));
        }

        public void Remove(T item)
        {
            elements.RemoveAll(x => x.Key.Equals(item));
        }

        public void UpdatePriority(T item, Vector2 newPriority)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Key.Equals(item))
                {
                    elements[i] = new KeyValuePair<T, Vector2>(item, newPriority);
                    break;
                }
            }
        }
    }

    private void Update()
    {
        Debug.ClearDeveloperConsole();
        string output = $"QPUEUE STATUS ({openSet.Count}): ";
        foreach (var KVP in openSet.E)
        {
            output += $"{KVP.Key.ToString()} - {KVP.Value.ToString()}";
        }
        Debug.Log(output);
    }

    #region OLD
    /*
    public void Pathfind()
    {
        ResetColors();

        pathing = new Astar(grid); // Set up the A*

        infoText.text = "(" + (int)start.transform.position.x + ", " + (int)start.transform.position.y + ") to (" + (int)finish.transform.position.x + ", " + (int)finish.transform.position.y + ")";
        statusText.text = "Begun...";
        statusImage.color = Color.blue;

        // Call create path function
        pathing.CreatePath(grid, (int)start.transform.position.x, (int)start.transform.position.y, (int)finish.transform.position.x, (int)finish.transform.position.y);

        PlaceLandmarks(); // Place landmarks

        // Set initial status
        setsText.text = "O(" + pathing.openSet.Count + ")" + " C(" + pathing.closedSet.Count + ")" + " P(" + pathing.path.Count + ")"; // Set "sets" text

        statusText.text = "Searching...";
        statusImage.color = Color.yellow;

        StopCoroutine(WaitForPath());
        StartCoroutine(WaitForPath());
    }

    // Things to do while waiting for the path to finish.
    private IEnumerator WaitForPath()
    {
        SetPainting();

        while (pathing.searchStatus != AStarSearchStatus.Success)
        {
            SetPainting();

            if (pathing.searchStatus == AStarSearchStatus.Failure) // Escape case
            {
                statusText.text = "FAILED";
                statusImage.color = Color.red;
                yield break;
            }

            setsText.text = "O(" + pathing.openSet.Count + ")" + " C(" + pathing.closedSet.Count + ")" + " P(" + pathing.path.Count + ")"; // Set "sets" text

            statusText.text = "Searching...";
            statusImage.color = Color.yellow;

            yield return null;
        }

        statusText.text = "Finished.";
        statusImage.color = Color.green;
    }



    private void PlaceLandmarks()
    {
        // Destroy any existing landmarks
        foreach (Transform child in parent_landmarks.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new landmarks
        foreach (var L in pathing.landmarks)
        {
            var newLandmark = Instantiate(prefab_Landmark, new Vector3(L.x, L.y, 5), Quaternion.identity); // Instantiate
            newLandmark.name = $"Landmark {L.x} {L.y} - "; // Give grid based name

            newLandmark.transform.parent = parent_landmarks.transform; // Set parent
        }
    }

    #endregion
    */

    #endregion
}

// Define the Node class to hold information about each cell in the grid
public class Node
{
    public Vector2Int Position;  // Position of the node in the grid
    public float GCost;          // Cost from the start node
    public float RHS;            // One-step lookahead cost
    public float Heuristic;      // Heuristic cost to the goal
    public bool Walkable;        // Whether the node is walkable
    public Node Parent;          // Parent node in the current path
    public GameObject Tile;      // Reference to the GameObject associated with this node
    public float KeyModifier;

    public Node(Vector2Int position, bool walkable, GameObject tile)
    {
        Position = position;
        GCost = float.MaxValue;
        RHS = float.MaxValue;
        Heuristic = 0;
        Walkable = walkable;
        Parent = null;
        Tile = tile;
        KeyModifier = 0;
    }

    public Node(Vector2Int position, float g, float rhs, float h, bool walkable, GameObject tile, float km)
    {
        this.Position = position;
        this.GCost = g;
        this.RHS = rhs;
        this.Heuristic = h;
        this.Walkable = walkable;
        this.Parent = null;
        this.Tile = tile;
        this.KeyModifier = km;
    }

    // Returns the total cost used for priority queue ordering
    public float Priority => Mathf.Min(GCost, RHS) + Heuristic;

    public float CalculateKey(Node node)
    {
        //return new Node(); // ?
        return Mathf.Min(node.GCost, node.RHS) + node.Heuristic + KeyModifier;
    }

    // Updates the tile's color based on the node's state
    public void UpdateColor(Color color)
    {
        if (Tile != null && Tile.GetComponent<SpriteRenderer>() != null)
        {
            Tile.GetComponent<SpriteRenderer>().color = color;
        }
    }
}

// Priority Queue implementation for managing nodes based on priority
public class PriorityQueue
{
    // Custom comparer to handle node priority ordering
    public readonly SortedSet<(Node, float)> queue = new SortedSet<(Node, float)>(Comparer<(Node, float)>.Create((a, b) =>
    {
        if (a.Item2 == b.Item2)
        {
            // Custom comparison of Vector2Int since it lacks IComparable interface
            if (a.Item1.Position.x == b.Item1.Position.x)
                return a.Item1.Position.y.CompareTo(b.Item1.Position.y);
            return a.Item1.Position.x.CompareTo(b.Item1.Position.x);
        }
        return a.Item2.CompareTo(b.Item2);
    }));

    public void Enqueue(Node node)
    {
        queue.Add((node, node.Priority));
    }

    public Node Dequeue()
    {
        var node = queue.Min.Item1;
        queue.Remove(queue.Min);
        return node;
    }

    public bool Contains(Node node) => queue.Any(item => item.Item1 == node);

    public void Remove(Node node)
    {
        queue.RemoveWhere(item => item.Item1 == node);
    }

    public int Count => queue.Count;
}


public static class NE
{
    public static bool Approx(this float f1, float f2) => Mathf.Approximately(f1, f2);
}