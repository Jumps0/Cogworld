using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Color = UnityEngine.Color;

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

    [Header("Colors")]
    public Color openColor;
    public Color closedColor;
    public Color pathColor;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        StartCoroutine(SnapNodes());
        statusText.text = "No route.";
        infoText.text = "No route.";
    }

    #region MapGeneration

    private void GenerateMap()
    {
        // - We need to generate a random map for pathfinding based on the map size -

        // Set array
        grid = new GameObject[mapSize.x,mapSize.y];

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
                if(random > percentWalls)
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

    #endregion

    #region Pathfinding

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
        foreach(GameObject T in grid)
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Pathfind();
        }
    }
}
