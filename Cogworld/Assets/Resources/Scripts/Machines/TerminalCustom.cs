using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalCustom : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Outpost Terminal (Limited) | Zhirov's Terminal (Local) | DSF Access (Limited) | WAR.Sys (Local) | SHOP.Sys
    /// </summary>
    public string systemType;

    [Header("Commands")]
    public List<TerminalCommand> avaiableCommands;

    [Header("Security")]
    public bool restrictedAccess = true;
    [Tooltip("0, 1, 2, 3. 0 = Open System")]
    public int secLvl = 1;
    public float detectionChance;
    public float traceProgress;
    public bool detected;
    public bool locked = false; // No longer accessable
    [Tooltip("(Optional) Where completed components get spawned.")]
    public Transform ejectionSpot;

    [Header("Trojans")]
    public List<TrojanType> trojans = new List<TrojanType>();

    public CustomTerminalType type;

    [Header("Prototypes")]
    public List<ItemObject> prototypes = new List<ItemObject>();

    #region Door Control
    [Header("Door Control")]
    [Tooltip("Coordinates to the wall(s) that will dissapear if this *door* is opened.")]
    public List<Vector2Int> wallRevealCoordinates = new List<Vector2Int>();
    public List<TileBlock> wallRevealObjs = new List<TileBlock>();
    public AudioSource _doorSource;
    [SerializeField] private TileObject replaceTile;

    public void OpenDoor()
    {
        // Essentially we want to:
        // - Play the door open "sliding" sound
        _doorSource.Play();
        // - Replace the walls with floor tiles (that have rubble texture).
        foreach (TileBlock W in wallRevealObjs)
        {
            // TODO
        }
    }

    #endregion

    #region Hideout Cache
    public void SetupAsCache()
    {
        // TODO
    }

    #endregion

}

public enum CustomTerminalType
{
    Shop,
    WarlordCamp,
    LoreEntry,
    DoorLock,
    PrototypeData,
    HideoutCache,
    Misc
}
