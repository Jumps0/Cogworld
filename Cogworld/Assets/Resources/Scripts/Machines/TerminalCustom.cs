using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerminalCustom : MonoBehaviour
{
    [Tooltip("(Optional) Where completed components get spawned.")]
    public Transform ejectionSpot;

    public CustomTerminalType customType;

    [Header("Prototypes")]
    public List<ItemObject> prototypes = new List<ItemObject>();

    #region Door Control
    [Header("Door Control")]
    [Tooltip("Coordinates to the wall(s) that will dissapear if this *door* is opened.")]
    public List<Vector2Int> wallRevealCoordinates = new List<Vector2Int>();
    public List<TileBlock> linkedDoors = new List<TileBlock>();
    public AudioSource _doorSource;
    [SerializeField] private TileObject replaceTile;

    public void Init(CustomTerminalType customtype = CustomTerminalType.Misc)
    {
        
    }

    public void OpenDoor()
    {
        
    }

    #endregion

    

}
