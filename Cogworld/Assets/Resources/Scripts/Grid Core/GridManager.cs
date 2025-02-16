/*
 * Originally Created by: TaroDev
 * Youtube Link: https://www.youtube.com/watch?v=kkAjpQAM-jE
 * 
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{

    public static GridManager inst;
    public void Awake()
    {
        inst = this;
    }

    [SerializeField] public int _width, _height;

    [SerializeField] private TileBlock _tilePrefab;

    [SerializeField] private Transform _cam;

    public Dictionary<Vector2, TileBlock> _tiles;
    public GameObject[,] grid;

    public GameObject floorParent;

    [Tooltip("Value to scale all sprites.")]
    public float globalScale = 1f;

    public Astar astar;

    Coroutine getMouseLocation;

    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, TileBlock>(); // Init Dictionary
        InitGrid();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * globalScale, y * globalScale), Quaternion.identity); // Instantiate
                spawnedTile.transform.localScale = new Vector3(globalScale, globalScale, globalScale); // Adjust scaling
                spawnedTile.name = $"Tile {x} {y} - ";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                //spawnedTile.Init(isOffset);

                //
                // - DO MAP GENERATION HERE
                //

                int random = Random.Range(0, 10);
                if(random == 0)
                    spawnedTile.tileInfo = MapManager.inst.tileDatabase.Tiles[2];
                else
                    spawnedTile.tileInfo = MapManager.inst.tileDatabase.Tiles[1];

                //

                spawnedTile.name += spawnedTile.tileInfo.type.ToString(); // Modify name with type

                spawnedTile.tileInfo.currentVis = TileVisibility.Unknown; // All tiles start hidden
                FogOfWar.inst.unseenTiles.Add(spawnedTile); // Add to unseen tiles

                spawnedTile.location.x = x;
                spawnedTile.location.y = y;

                _tiles[new Vector2(x, y)] = spawnedTile; // Add to Dictionary
                grid[x, y] = spawnedTile.gameObject; // Add to Grid Array

                spawnedTile.gameObject.transform.SetParent(floorParent.transform);
            }
        }

        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);

        RegenerateAstar(); // Set up the A*
    }

    public void InitGrid()
    {
        grid = new GameObject[_width, _height]; // Init Grid
    }

    public TileBlock GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    private void Update()
    {
        if(GameManager.inst.allowMouseMovement)
            HandlePlayerMouseNavigation();
    }

    /// <summary>
    /// The current tile the mouse is hovering over.
    /// </summary>
    public TileBlock mouseTargetTile;
    public TileBlock oldTargetTile = null;
    private bool running = false;

    private void HandlePlayerMouseNavigation()
    {
        //mouseTargetTile = null;

        /*
        if(getMouseLocation != null)
        {
            StopCoroutine(CheckMouseLocation());
        }
        getMouseLocation = StartCoroutine(CheckMouseLocation());
        */

        CheckMouseLocation();

    }

    void CheckMouseLocation()
    {
        //Debug.Log("Checking...");
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

        if (hit.collider != null)
        {
            TileBlock tile = hit.collider.GetComponent<TileBlock>();

            if (tile != null && tile.GetComponent<SpriteRenderer>().color != Color.black && !EventSystem.current.IsPointerOverGameObject())
            {
                // Only update the mouseTargetTile variable if a valid tile was hit
                mouseTargetTile = tile;
            }
        }

        // If the mouseTargetTile variable is null, return without doing anything else
        if (mouseTargetTile == null)
        {
            return;
        }

        // If the mouseTargetTile variable has changed, regenerate the A* pathfinding and highlight the path
        if (oldTargetTile != mouseTargetTile)
        {
            ClearGridOfHighlightColor(UIManager.inst.dullGreen); // Clear the path

            // Get current player tile
            TileBlock currentPlayerTile = PlayerData.inst.GetComponent<PlayerGridMovement>().GetCurrentPlayerTile();

            // Create a path from the current player tile to the mouse target tile
            RegenerateAstar();

            astar.CreatePath(grid, currentPlayerTile.location.x, currentPlayerTile.location.y, mouseTargetTile.location.x, mouseTargetTile.location.y);

            if (astar.searchStatus == AStarSearchStatus.Failure)
            {
                running = false;
                return;
            }

            // Highlight the path
            for (int i = 1; i < astar.path.Count; i++) // i = 1, don't display the last one
            {
                if (i != astar.path.Count - 1) // Don't display the first one
                {
                    grid[astar.path[i].X, astar.path[i].Y].GetComponent<TileBlock>()._highlightPerm.SetActive(true);
                    grid[astar.path[i].X, astar.path[i].Y].GetComponent<TileBlock>()._highlightPerm.GetComponent<SpriteRenderer>().color = UIManager.inst.dullGreen;
                }
            }

            // Update the oldTargetTile variable
            oldTargetTile = mouseTargetTile;
        }
    }

    void CheckMouseLocationOLD()
    {
        mouseTargetTile = null;
        TileBlock toReturn = null;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.GetComponent<TileBlock>() != null && !EventSystem.current.IsPointerOverGameObject())
            {
                if (hit.collider.gameObject.GetComponent<SpriteRenderer>().color != Color.black) // Don't want to path to unseen tiles
                {
                    toReturn = hit.collider.gameObject.GetComponent<TileBlock>();
                }
            }
        }

        mouseTargetTile = toReturn;

        if (oldTargetTile != null && oldTargetTile != mouseTargetTile) // Same tile as last time? Don't bother.
        {
            ClearGridOfHighlightColor(grid[0, 0].GetComponent<TileBlock>().intel_green);

            if (mouseTargetTile != null)
            {
                // Get current player tile
                TileBlock currentPlayerTile = PlayerData.inst.GetComponent<PlayerGridMovement>().GetCurrentPlayerTile();

                // Create a path from:
                // - Current player tile (X, Y)
                // - Mouse target tile (X, Y)
                RegenerateAstar(); // May be performance heavy (Here to prevent old bad Spot arrays)
                astar.CreatePath(grid, currentPlayerTile.location.x, currentPlayerTile.location.y, mouseTargetTile.location.x, mouseTargetTile.location.y);

            }

            if (astar.searchStatus == AStarSearchStatus.Failure)
            {
                running = false;
                return;
            }

            // Visualize it
            for (int i = 1; i < astar.path.Count; i++) // i = 1, don't display the last one
            {
                if (i != astar.path.Count - 1) // Don't display the first one
                    grid[astar.path[i].X, astar.path[i].Y].GetComponent<TileBlock>()._highlight.SetActive(true);
                grid[astar.path[i].X, astar.path[i].Y].GetComponent<TileBlock>()._highlight.GetComponent<SpriteRenderer>().color = grid[0, 0].GetComponent<TileBlock>().intel_green;
            }



            // (misc vis)
            /*
             * 
            if (astar.end != null)
            {
                grid[astar.end.X, astar.end.Y]._highlight.SetActive(true);
                grid[astar.end.X, astar.end.Y]._highlight.GetComponent<SpriteRenderer>().color = grid[0, 0].GetComponent<Tile>().intel_green;
            }

            if (astar.start != null)
            {
                grid[astar.start.X, astar.start.Y]._highlight.SetActive(true);
                grid[astar.start.X, astar.start.Y]._highlight.GetComponent<SpriteRenderer>().color = grid[0, 0].GetComponent<Tile>().trojan_Blue;
            }
            for (int i = 0; i < astar.closedSet.Count; i++)
            {
                grid[astar.closedSet[i].X, astar.closedSet[i].Y]._highlight.SetActive(true);
                grid[astar.closedSet[i].X, astar.closedSet[i].Y]._highlight.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
            for (int i = 0; i < astar.openSet.Count; i++)
            {
                grid[astar.openSet[i].X, astar.openSet[i].Y]._highlight.SetActive(true);
                grid[astar.openSet[i].X, astar.openSet[i].Y]._highlight.GetComponent<SpriteRenderer>().color = grid[0, 0].GetComponent<Tile>().unstableCollapse_red;
            }
            */
        }

        //yield return new WaitForEndOfFrame();

        if (oldTargetTile != null && mouseTargetTile != null)
        {
            oldTargetTile = mouseTargetTile;
        }

        if (oldTargetTile != mouseTargetTile) // New target? Wipe the map.
        {

            ClearGridOfHighlightColor(grid[0, 0].GetComponent<TileBlock>().intel_green);
        }
        else
        {

        }

        if (mouseTargetTile != null)
        {
            oldTargetTile = mouseTargetTile;
        }


        running = false;

    }

    public void RegenerateAstar()
    {
        astar = new Astar(grid); // Set up the A*
    }

    public void ClearGridOfHighlightColor(Color desiredColor)
    {
        foreach (GameObject t in grid)
        {
            if(t != null && t.GetComponent<TileBlock>())
            {
                if (t.GetComponent<TileBlock>()._highlightPerm.GetComponent<SpriteRenderer>().color == desiredColor)
                { // Does the color match? Turn it off.
                    t.GetComponent<TileBlock>()._highlightPerm.GetComponent<SpriteRenderer>().color = Color.white; // Set it back to white
                    t.GetComponent<TileBlock>()._highlightPerm.SetActive(false);
                }
            }
        }
    }

    
    public void TileVisFlip(TileVisibility old, TileVisibility desired)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if(grid[x, y].GetComponent<TileBlock>().tileInfo.currentVis == old)
                {
                    grid[x, y].GetComponent<TileBlock>().tileInfo.currentVis = desired;
                }
            }
        }
    }

    [SerializeField] private bool debugFlip = false;
    private void DEBUG()
    {
        if (debugFlip)
        {
            TileVisFlip(TileVisibility.Visible, TileVisibility.Unknown);
            debugFlip = false;
        }
    }
    
}