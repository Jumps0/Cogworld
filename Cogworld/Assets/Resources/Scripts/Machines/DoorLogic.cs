using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLogic : MonoBehaviour
{

    public TileBlock _tile;
    public Vector2Int _location;
    public List<TileBlock> activationTiles = new List<TileBlock>();
    [Tooltip("False = Closed | True = Open")]
    public bool state = false;

    [Header("Sprites")]
    public Sprite _open;
    public Sprite _closed;

    [Header("Audio")]
    public AudioSource source;
    public List<AudioClip> openSound;
    public List<AudioClip> closedSound;

    private void Start()
    {
        openSound = _tile.tileInfo.door_open;
        closedSound = _tile.tileInfo.door_close;
    }

    public void LoadActivationTiles()
    {
        // We want all (non-diagonal) neighboring tiles + this door's location
        //
        //    *
        //  * D *
        //    *
        //

        activationTiles.Add(MapManager.inst._allTilesRealized[_location].bottom); // Center
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(0, 1)].bottom);  // Up
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(0, -1)].bottom); // Down
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(-1, 0)].bottom); // Left
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(1, 0)].bottom);  // Right

        this.GetComponent<SpriteRenderer>().sprite = _closed;
        source.spatialBlend = 1;
    }

    /// <summary>
    /// Check if the door should open/close.
    /// </summary>
    public void StateCheck()
    {
        /*
        bool botNearby = false;
        foreach (TileBlock T in activationTiles)
        {
            if (T.GetBotOnTop() != null) // Is there a bot nearby?
            {
                botNearby = true;
                break;
            }
        }

        if (botNearby) // If there is a bot nearby
        {
            if (!state) // If we're currently closed, we need to open
            {
                Open();
            }
        }
        else if(!botNearby) // If there isn't a bot nearby
        {
            if (state) // If we're currently open, we need to close
            {
                Close();
            }
        }
        */
    }

    public void Open()
    {
        // Change Sprite
        this.GetComponent<SpriteRenderer>().sprite = _open;

        // Change state
        state = true; // open

        // Play Sound
        source.PlayOneShot(_tile.tileInfo.door_open[Random.Range(0, _tile.tileInfo.door_open.Count - 1)]);

        // Change Vis
        this.GetComponent<TileBlock>().specialNoBlockVis = true;

        // Need to update FOV!
        TurnManager.inst.AllEntityVisUpdate(true);
    }

    public void Close()
    {
        // Change Sprite
        this.GetComponent<SpriteRenderer>().sprite = _closed;

        // Change state
        state = false; // closed

        // Play Sound
        source.PlayOneShot(_tile.tileInfo.door_close[Random.Range(0, _tile.tileInfo.door_close.Count - 1)]);

        // Change Vis
        this.GetComponent<TileBlock>().specialNoBlockVis = false;

        // Need to update FOV!
        TurnManager.inst.AllEntityVisUpdate(true);
    }

}
