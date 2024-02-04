using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCore<TGridObject> : MonoBehaviour
{

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    [SerializeField] private TileBlock _tilePrefab;

    public GameObject floorParent;

    private Dictionary<Vector2, TileBlock> _tiles;

    [Tooltip("Value to scale all sprites.")]
    public float globalScale = 1f;

    public GridCore(int width, int height, float cellSize, Vector3 originPosition, Func<GridCore<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * globalScale, y * globalScale), Quaternion.identity); // Instantiate
                spawnedTile.transform.localScale = new Vector3(globalScale, globalScale, globalScale); // Adjust scaling
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);

                _tiles[new Vector2(x, y)] = spawnedTile;
                gridArray[x, y] = createGridObject(this, x, y);

                spawnedTile.gameObject.transform.SetParent(floorParent.transform);
            }
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }
}
