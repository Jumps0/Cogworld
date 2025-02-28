using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TestMgr : MonoBehaviour
{
    public static TestMgr inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("References")]
    public GameObject refA;
    public GameObject refB;
    public Tilemap tilemap;

    private void Start()
    {
        //StartCoroutine(Delay(2.5f));
        TilemapTest();
    }

    private IEnumerator Delay(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        /*
        refA.GetComponent<UIDataHeader>().Setup("Overview");
        refA.GetComponent<UIDataHeader>().Open();

        //refB.GetComponent<UIDataGenericDetail>().Setup(true, true, false, "This is a text test", Color.cyan, "", false, "", false, "STATE"); // Variable Box
        refB.GetComponent<UIDataGenericDetail>().Setup(false, false, false, "This is a text test", Color.white); // Basic (no secondary)
        //refB.GetComponent<UIDataGenericDetail>().Setup(true, false, true, "This is a text test", Color.white, "", false, "", false, "", 0.9f); // Bar
        refB.GetComponent<UIDataGenericDetail>().Open();
        */
    }

    private void Update()
    {
        /*
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(Delay());
        }
        else if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            refA.GetComponent<UIDataHeader>().Close();
            refB.GetComponent<UIDataGenericDetail>().Close();
        }
        */
    }

    private void TilemapTest()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap reference is not assigned!");
            return;
        }

        // Get the bounds of the Tilemap (the area covered by tiles)
        BoundsInt bounds = tilemap.cellBounds;

        // Loop through all the tiles within the bounds of the Tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            // Get the tile at the current position
            TileBase tile = tilemap.GetTile(position);

            // If the tile exists, print its sprite
            if (tile != null)
            {
                // Check if the tile is a Tile object and has a sprite
                if (tile is Tile tileObject && tileObject.sprite != null)
                {
                    Debug.Log($"Tile at {position}: {tileObject.sprite.name}");
                }
                else
                {
                    Debug.Log($"Tile at {position}: No sprite found");
                }
            }
            else
            {
                Debug.Log($"No tile at {position}");
            }
        }
    }
}
