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
    public TextMeshProUGUI text_coords;

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

        if (Input.GetMouseButton(1))
        {
            CleanAllTiles();
            FindLinePath();
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

        Vector2Int start = new Vector2Int(Mathf.RoundToInt(playerPosition.x), Mathf.RoundToInt(playerPosition.y));
        Vector2Int finish = new Vector2Int(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));

        text_coords.text = start.ToString() + " to " + finish.ToString();

        //StartCoroutine(Pathfind(HF.V3_to_V2I(playerPosition), HF.V3_to_V2I(mousePosition)));
        FindPathNonIE(start, finish);
    }

    private IEnumerator Pathfind(Vector2 start, Vector2 finish)
    {
        pathFinished = false;
        Vector2 currentPos = start;
        Vector2 direction = (finish - start).normalized;

        text_status.text = "Searching...";
        text_status.color = Color.yellow;

        while (Vector2.Distance(currentPos, finish) > 0.25f)
        {
            // Add current point to path
            path.Add(world[HF.V3_to_V2I(currentPos)]);

            // Move towards finish point in the calculated direction
            currentPos += direction;

            yield return null;
        }

        pathFinished = true;
        MarkPath(finish); // Finish up by drawing the path

        text_status.text = "Finished.";
        text_status.color = Color.green;

        world[HF.V3_to_V2I(finish)].GetComponent<SpriteRenderer>().color = Color.blue;

    }

    private void FindPathNonIE(Vector2 start, Vector2 finish)
    {
        // The function above but just not a Coroutine
        pathFinished = false;
        Vector2 currentPos = start;
        Vector2 direction = (finish - start).normalized;
        Vector2 lastDirection = direction; // Store the last direction

        text_status.text = "Searching...";
        text_status.color = Color.yellow;

        while (Vector2.Distance(currentPos, finish) > 0.1f)
        {
            // Add current point to path
            path.Add(world[HF.V3_to_V2I(currentPos)]);

            // Move towards finish point in the calculated direction
            currentPos += direction;

            // Check if current position is out of bounds, if so, break the loop
            if (currentPos.x < 0 || currentPos.y < 0 || currentPos.x >= mapSize.x - 2 || currentPos.y >= mapSize.y - 2)
                break;

            // Update direction towards finish point
            direction = (finish - currentPos).normalized;

            // Check if direction has changed (passed the finish point)
            if (Vector2.Dot(direction, lastDirection) < 0)
                break; // Stop if the direction changes

            lastDirection = direction; // Update last direction
        }

        // Add the finish point to the path
        path.Add(world[HF.V3_to_V2I(finish)]);

        pathFinished = true;
        MarkPath(finish); // Finish up by drawing the path

        text_status.text = "Finished.";
        text_status.color = Color.green;

        world[HF.V3_to_V2I(finish)].GetComponent<SpriteRenderer>().color = Color.blue;
    }

    #region OLD METHOD
    /*
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
    */
    #endregion

    private void MarkPath(Vector2 end)
    {
        foreach(GameObject P in path) // Go through the path and mark each tile
        {
            MarkTile(HF.V3_to_V2I(P.transform.position));
        }

        if (GapCheckHelper(HF.V3_to_V2I(end)))
        {
            GapCheck();
        }
        CleanPath(); // Clean the path
    }

    private void GapCheck()
    {
        // Due to a quirk in the line generation, there is occasionally a gap in the path between the last tile along the path and the end tile.
        // Here we will simply fill that 1 tile gap.

        // This issue doesn't appear to show up in diagonal lines, so we won't check the diagonal directions here.

        if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1].transform.position), HF.V3_to_V2I(path[0].transform.position)) == Vector2.up)
        {
            world[HF.V3_to_V2I(path[path.Count - 1].transform.position) + Vector2Int.up].GetComponent<SpriteRenderer>().color = lineColor;
        }
        else if(HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1].transform.position), HF.V3_to_V2I(path[0].transform.position)) == Vector2.down)
        {
            world[HF.V3_to_V2I(path[path.Count - 1].transform.position) + Vector2Int.down].GetComponent<SpriteRenderer>().color = lineColor;
        }
        else if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1].transform.position), HF.V3_to_V2I(path[0].transform.position)) == Vector2.left)
        {
            world[HF.V3_to_V2I(path[path.Count - 1].transform.position) + Vector2Int.right].GetComponent<SpriteRenderer>().color = lineColor;
        }
        else if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1].transform.position), HF.V3_to_V2I(path[0].transform.position)) == Vector2.right)
        {
            world[HF.V3_to_V2I(path[path.Count - 1].transform.position) + Vector2Int.left].GetComponent<SpriteRenderer>().color = lineColor;
        }
    }

    private void CleanPath()
    {
        Vector2Int C = HF.V3_to_V2I(player.transform.position); // the player's position

        // Sometimes the path can be a bit messy, lets fix that...
        // We usually have 2 cases to fix.

        // 1. Sometimes an additional tile is highlighted next to the player.
        // - We need to check diagonals around the player
        if(world[new Vector2Int(C.x + 1, C.y - 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ DOWN-RIGHT ]
        {
            // Disable (Right & Down)
            world[new Vector2Int(C.x + 1, C.y)].GetComponent<SpriteRenderer>().color = Color.white;
            world[new Vector2Int(C.x, C.y - 1)].GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (world[new Vector2Int(C.x + 1, C.y + 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ UP-RIGHT ]
        {
            // Disable (Right & Up)
            world[new Vector2Int(C.x + 1, C.y)].GetComponent<SpriteRenderer>().color = Color.white;
            world[new Vector2Int(C.x, C.y + 1)].GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (world[new Vector2Int(C.x - 1, C.y - 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ DOWN-LEFT ]
        {
            // Disable (Left & Down)
            world[new Vector2Int(C.x - 1, C.y)].GetComponent<SpriteRenderer>().color = Color.white;
            world[new Vector2Int(C.x, C.y - 1)].GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (world[new Vector2Int(C.x - 1, C.y + 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ UP-LEFT ]
        {
            // Disable (Left & Up)
            world[new Vector2Int(C.x - 1, C.y)].GetComponent<SpriteRenderer>().color = Color.white;
            world[new Vector2Int(C.x, C.y + 1)].GetComponent<SpriteRenderer>().color = Color.white;
        }

        // 2. Sometimes an additional tile is highlighted along a diagonal.
        // - This is a bit more tricky to do as we need to check two tiles for each diagonal
        /* - Like this:
         *       ?
         *   * []
         *   [] *
         *  ?
         */

        foreach (GameObject P in path)
        {
            Vector2Int loc = HF.V3_to_V2I(P.transform.position);

            if (world[loc].GetComponent<SpriteRenderer>().color == lineColor
            && world[new Vector2Int(loc.x + 1, loc.y - 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ DOWN-RIGHT ]
            {
                // Disable (Right & Down)
                world[new Vector2Int(loc.x + 1, loc.y)].GetComponent<SpriteRenderer>().color = Color.white;
                world[new Vector2Int(loc.x, loc.y - 1)].GetComponent<SpriteRenderer>().color = Color.white;
            }
            if (world[loc].GetComponent<SpriteRenderer>().color == lineColor
            && world[new Vector2Int(loc.x + 1, loc.y + 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ UP-RIGHT ]
            {
                // Disable (Right & Up)
                world[new Vector2Int(loc.x + 1, loc.y)].GetComponent<SpriteRenderer>().color = Color.white;
                world[new Vector2Int(loc.x, loc.y + 1)].GetComponent<SpriteRenderer>().color = Color.white;
            }
            if (world[loc].GetComponent<SpriteRenderer>().color == lineColor
            && world[new Vector2Int(loc.x - 1, loc.y - 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ DOWN-LEFT ]
            {
                // Disable (Left & Down)
                world[new Vector2Int(loc.x - 1, loc.y)].GetComponent<SpriteRenderer>().color = Color.white;
                world[new Vector2Int(loc.x, loc.y - 1)].GetComponent<SpriteRenderer>().color = Color.white;
            }
            if (world[loc].GetComponent<SpriteRenderer>().color == lineColor
            && world[new Vector2Int(loc.x - 1, loc.y + 1)].GetComponent<SpriteRenderer>().color == lineColor) // [ UP-LEFT ]
            {
                // Disable (Left & Up)
                world[new Vector2Int(loc.x - 1, loc.y)].GetComponent<SpriteRenderer>().color = Color.white;
                world[new Vector2Int(loc.x, loc.y + 1)].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

    }

    private bool GapCheckHelper(Vector2Int end)
    {
        // Checks around in a circle of the finish point.
        // If every tile surrounding the finish point is white, returns true.
        if (world[end + Vector2Int.up].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.down].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.left].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.right].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.up + Vector2Int.left].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.up + Vector2Int.right].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.down + Vector2Int.left].GetComponent<SpriteRenderer>().color == Color.white
            && world[end + Vector2Int.down + Vector2Int.right].GetComponent<SpriteRenderer>().color == Color.white)
        {
            return true;
        }
        else
        {
            return false;
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
