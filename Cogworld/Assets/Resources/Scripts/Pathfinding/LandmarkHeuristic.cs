using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PathfindingTestControl;

public class LandmarkHeuristic : MonoBehaviour
{
    public Dictionary<Vector2Int, Dictionary<Vector2Int, float>> landmarkDistances;
    public List<Vector2Int> landmarks;
    public Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),  // Right
        new Vector2Int(-1, 0), // Left
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(1, -1), // Down-Right
        new Vector2Int(-1, -1),// Down-Left
        new Vector2Int(1, 1),  // Up-Right
        new Vector2Int(-1, 1), // Up-Left
    };

    /*
    // Constructor
    public LandmarkHeuristic()
    {
        landmarkDistances = new Dictionary<Vector2Int, Dictionary<Vector2Int, float>>();
        CreateLandmarks();
    }

    int landmarkSpacing = 15;
    private void CreateLandmarks()
    {
        // Set variables
        GameObject prefab = PathfindingTestControl.inst.prefab_Landmark;
        Vector2Int mapsize = PathfindingTestControl.inst.mapSize;
        landmarks = new List<Vector2Int>();

        // Instantiate landmarks
        for (int x = 0; x < mapsize.x / landmarkSpacing; x++)
        {
            for (int y = 0; y < mapsize.y / landmarkSpacing; y++)
            {
                var newLandmark = Instantiate(prefab, new Vector3(x * landmarkSpacing, y * landmarkSpacing, 5), Quaternion.identity); // Instantiate
                newLandmark.name = $"Landmark {x * landmarkSpacing} {y * landmarkSpacing} - "; // Give grid based name
                newLandmark.transform.parent = PathfindingTestControl.inst.parent_landmarks.transform; // Set parent

                landmarks.Add(new Vector2Int(x * landmarkSpacing, y * landmarkSpacing));
            }
        }

        // Calculate initial precompute distances
        //foreach (var landmark in landmarks)
        //{
        //    RecomputeDistances(landmark, mapsize.x, mapsize.y, PathfindingTestControl.inst.grid);
        //}

        // Calculate precompute distances
        PrecomputeDistances(PathfindingTestControl.inst.grid, landmarks);
    }

    // Set up landmarks with precomputed distances
    public void SetLandmarks(List<Vector2Int> landmarks, Dictionary<Vector2Int, Dictionary<Vector2Int, float>> precomputedDistances)
    {
        foreach (var landmark in landmarks)
        {
            if (precomputedDistances.ContainsKey(landmark))
            {
                landmarkDistances[landmark] = precomputedDistances[landmark];
            }
            else
            {
                Debug.LogError($"Precomputed distances not found for landmark at {landmark}");
            }
        }
    }

    // Heuristic function
    public float GetHeuristic(Vector2Int currentNode, Vector2Int goalNode)
    {
        float maxHeuristic = 0f;

        foreach (var landmark in landmarkDistances)
        {
            if (landmark.Value.ContainsKey(currentNode) && landmark.Value.ContainsKey(goalNode))
            {
                float distToCurrent = landmark.Value[currentNode];
                float distToGoal = landmark.Value[goalNode];
                float heuristic = Mathf.Abs(distToCurrent - distToGoal);
                maxHeuristic = Mathf.Max(maxHeuristic, heuristic);
            }
            else
            {
                Debug.LogError($"Distances not found for landmark at {landmark.Key}.");
            }
        }

        return maxHeuristic;
    }

    // Function to recompute distances from a specific landmark using BFS (or Dijkstra for variable costs)
    public void RecomputeDistances(Vector2Int landmark, int gridWidth, int gridHeight, GameObject[,] walkableGrid)
    {
        // Initialize the distance dictionary for this landmark
        Dictionary<Vector2Int, float> distances = new Dictionary<Vector2Int, float>();

        // BFS setup
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(landmark);
        distances[landmark] = 0f;  // Distance to itself is 0

        // Process the grid using BFS
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            float currentDistance = distances[current];

            // Explore neighbors (up, down, left, right)
            foreach (var direction in directions)
            {
                Vector2Int neighbor = current + direction;

                // Ensure neighbor is within bounds and walkable
                if (IsValidPosition(neighbor, gridWidth, gridHeight) && (walkableGrid[neighbor.x, neighbor.y] && walkableGrid[neighbor.x, neighbor.y].GetComponent<SpriteRenderer>().color == Color.white))
                {
                    if (!distances.ContainsKey(neighbor) || currentDistance + 1f < distances[neighbor])
                    {
                        distances[neighbor] = currentDistance + 1f;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Store the recomputed distances in the landmarkDistances dictionary
        landmarkDistances[landmark] = distances;
    }

    // Helper function to check if a position is valid on the grid
    private bool IsValidPosition(Vector2Int position, int gridWidth, int gridHeight)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight;
    }

    public Dictionary<Vector2Int, Dictionary<Vector2Int, float>> precomputedDistances = new Dictionary<Vector2Int, Dictionary<Vector2Int, float>>();
    public Dictionary<Vector2Int, Dictionary<Vector2Int, float>> PrecomputeDistances(GameObject[,] grid, List<Vector2Int> landmarks)
    {
        precomputedDistances = new Dictionary<Vector2Int, Dictionary<Vector2Int, float>>();

        foreach (Vector2Int landmark in landmarks)
        {
            // Compute the shortest distance from this landmark to all other nodes
            precomputedDistances[landmark] = DijkstraFromLandmark(grid, landmark);
        }

        return precomputedDistances;
    }

    // Function to perform Dijkstra's algorithm from a given landmark
    private Dictionary<Vector2Int, float> DijkstraFromLandmark(GameObject[,] grid, Vector2Int landmark)
    {
        Dictionary<Vector2Int, float> distances = new Dictionary<Vector2Int, float>();
        int gridWidth = grid.GetLength(0);
        int gridHeight = grid.GetLength(1);

        // Initialize distances with infinity for all positions
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                distances[pos] = float.MaxValue;
            }
        }

        // Distance to the landmark itself is 0
        distances[landmark] = 0;

        // Priority queue to hold nodes to visit (sorted by distance)
        PriorityQueue<Vector2Int> queue = new PriorityQueue<Vector2Int>();
        queue.Enqueue(landmark, new Vector2(0f, 0f)); // Use Vector2 with y = 0

        // Perform Dijkstra's algorithm
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            float currentDistance = distances[current];

            // Visit neighbors
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;

                if (IsValidPosition(neighbor, gridWidth, gridHeight) && IsWalkable(neighbor, grid))
                {
                    float newDistance = currentDistance + 1; // Assuming uniform cost for all grid moves

                    if (newDistance < distances[neighbor])
                    {
                        distances[neighbor] = newDistance;
                        queue.Enqueue(neighbor, new Vector2(newDistance, 0f)); // Enqueue with Vector2 where y = 0
                    }
                }
            }
        }

        return distances;
    }


    // Helper function to check if a position is walkable
    private bool IsWalkable(Vector2Int position, GameObject[,] grid)
    {
        return grid[position.x, position.y] && grid[position.x, position.y].GetComponent<SpriteRenderer>().color == Color.white;
    }
    */
}