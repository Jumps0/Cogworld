using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetingTestMgr : MonoBehaviour
{
    public static TargetingTestMgr inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Space")]
    public Dictionary<Vector2Int, GameObject> world = new Dictionary<Vector2Int, GameObject>();
    public Vector2Int mapSize = new Vector2Int(100, 100);
    public Transform mapParent;

    [Header("Prefabs")]
    public GameObject prefab_tile;

    public GameObject player;

    [Header("Colors")]
    public Color lineColor;

    [Header("UI")]
    public TextMeshProUGUI text_path;
    public TextMeshProUGUI text_status;


    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();

        text_status.text = "Waiting...";
        text_status.color = Color.blue;
    }

    private void Update()
    {
        DrawTargetingLine();
    }

    #region Mapgen

    private void GenerateMap()
    {
        // Create the map
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                CreateTile(new Vector2Int(x, y));
            }
        }

        // Reposition the player
        player.transform.position = new Vector3(mapSize.x / 2, mapSize.y / 2, 0f);
    }

    private void CreateTile(Vector2Int pos)
    {
        var spawnedTile = Instantiate(prefab_tile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"Tile {pos.x} {pos.y} - "; // Give grid based name
        spawnedTile.transform.parent = mapParent;
        world.Add(pos, spawnedTile);
    }

    #endregion

    #region Line Related

    public void DrawTargetingLine()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindLinePath();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            CleanAllTiles();
        }

        text_path.text = path.Count.ToString();
    }

    private void CleanAllTiles()
    {
        path.Clear();
        foreach (var T in world)
        {
            T.Value.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void CleanTile(Vector2Int pos)
    {
        if (world.ContainsKey(pos))
        {
            world[pos].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void MarkTile(Vector2Int pos)
    {
        if (world.ContainsKey(pos))
        {
            world[pos].GetComponent<SpriteRenderer>().color = lineColor;
        }
    }

    private List<GameObject> path = new List<GameObject>();
    private bool pathFinished = false;

    private void FindLinePath()
    {
        path = new List<GameObject>();

        // We want to draw a line FROM *the player* TO the *mouse position*.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 playerPosition = player.transform.position;

        // There are multiple ways of finding a path from point A to point B.
        // -In this method we are going to find the next closest tile (neighbor) and set that as the next tile along the path.

        StartCoroutine(Pathfind(HF.V3_to_V2I(playerPosition), HF.V3_to_V2I(mousePosition)));
    }

    private IEnumerator Pathfind(Vector2Int start, Vector2Int finish)
    {
        pathFinished = false;
        Vector2Int latestPathPos = start;

        text_status.text = "Searching...";
        text_status.color = Color.yellow;

        while(latestPathPos != finish)
        {
            // Add current point to path
            path.Add(world[latestPathPos]);

            // Get neighbors
            List<GameObject> neighbors = FindNeighbors(latestPathPos.x, latestPathPos.y);

            // Find closest neighbor to finish location.
            float closest = float.MaxValue;
            GameObject closestGO = null;
            foreach(GameObject N in neighbors)
            {
                float distance = Vector2.Distance(N.transform.position, finish); // Get distance of this point

                if(distance < closest) // If this distance is closer than the previous closest, overwrite it.
                {
                    closest = distance;
                    closestGO = N;
                }
            }

            // Now that we have the closest tile, that will be our next point.
            latestPathPos = HF.V3_to_V2I(closestGO.transform.position);

            yield return null;
        }

        pathFinished = true;
        MarkPath(); // Finish up by drawing the path

        text_status.text = "Finished.";
        text_status.color = Color.green;
    }

    private void MarkPath()
    {
        foreach(GameObject P in path) // Go through the path and mark each tile
        {
            MarkTile(HF.V3_to_V2I(P.transform.position));
        }
    }

    private List<GameObject> FindNeighbors(int X, int Y)
    {
        List<GameObject> neighbors = new List<GameObject>();

        // We want to include diagonals into this.
        if (X < mapSize.x - 1) // [ RIGHT ]
        {
            neighbors.Add(world[new Vector2Int(X + 1, Y)].gameObject);
        }
        if (X > 0) // [ LEFT ]
        {
            neighbors.Add(world[new Vector2Int(X - 1, Y)].gameObject);
        }
        if (Y < mapSize.y - 1) // [ UP ]
        {
            neighbors.Add(world[new Vector2Int(X, Y + 1)].gameObject);
        }
        if (Y > 0) // [ DOWN ]
        {
            neighbors.Add(world[new Vector2Int(X, Y - 1)].gameObject);
        }
        // -- 
        // Diagonals
        // --
        if (X < mapSize.x - 1 && Y < mapSize.y - 1) // [ UP-RIGHT ]
        {
            neighbors.Add(world[new Vector2Int(X + 1, Y + 1)].gameObject);
        }
        if (Y < mapSize.y - 1 && X > 0) // [ UP-LEFT ]
        {
            neighbors.Add(world[new Vector2Int(X - 1, Y + 1)].gameObject);
        }
        if (Y > 0 && X > 0) // [ DOWN-LEFT ]
        {
            neighbors.Add(world[new Vector2Int(X - 1, Y - 1)].gameObject);
        }
        if (Y > 0 && X < mapSize.x - 1) // [ DOWN-RIGHT ]
        {
            neighbors.Add(world[new Vector2Int(X + 1, Y - 1)].gameObject);
        }

        return neighbors;

    }

    #endregion
}
