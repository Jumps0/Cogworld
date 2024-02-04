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
    public AudioClip openSound;
    public AudioClip closedSound;


    public void LoadActivationTiles()
    {
        // We want all (non-diagonal) neighboring tiles + this door's location
        //
        //    *
        //  * D *
        //    *
        //

        activationTiles.Add(MapManager.inst._allTilesRealized[_location]); // Center
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(0, 1)]);  // Up
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(0, -1)]); // Down
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(-1, 0)]); // Left
        activationTiles.Add(MapManager.inst._allTilesRealized[_location + new Vector2Int(1, 0)]);  // Right

        this.GetComponent<SpriteRenderer>().sprite = _closed;
        source.spatialBlend = 1;
    }

    /// <summary>
    /// Check if the door should open/close.
    /// </summary>
    public void StateCheck()
    {
        state = false;
        foreach (TileBlock T in activationTiles)
        {
            
            if (T.GetBotOnTop() != null) // Is there a bot nearby?
            {
                state = true;
            }
        }

        if (state) // If there is a bot nearby
        {
            if (this.GetComponent<SpriteRenderer>().sprite == _closed) // If we're currently closed, we need to open
            {
                Open();
            }
        }
        else if(!state) // If there isn't a bot nearby
        {
            if (this.GetComponent<SpriteRenderer>().sprite == _open) // If we're currently open, we need to close
            {
                Close();
            }
        }
    }

    public void Open()
    {
        // Change Sprite
        this.GetComponent<SpriteRenderer>().sprite = _open;

        // Play Sound
        source.PlayOneShot(openSound);

        // Change Vis
        this.GetComponent<TileBlock>().specialNoBlockVis = true;

        // Need to update FOV!
        TurnManager.inst.AllEntityVisUpdate();
    }

    public void Close()
    {
        // Change Sprite
        this.GetComponent<SpriteRenderer>().sprite = _closed;

        // Play Sound
        source.PlayOneShot(closedSound);

        // Change Vis
        this.GetComponent<TileBlock>().specialNoBlockVis = false;

        // Need to update FOV!
        TurnManager.inst.AllEntityVisUpdate();
    }

}
