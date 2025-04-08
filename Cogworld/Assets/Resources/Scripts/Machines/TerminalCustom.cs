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
        //detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        //type = MachineType.CustomTerminal;
        customType = customtype;

        switch (customType)
        {
            case CustomTerminalType.Shop:
                break;
            case CustomTerminalType.WarlordCamp:
                break;
            case CustomTerminalType.LoreEntry:
                break;
            case CustomTerminalType.DoorLock:
                break;
            case CustomTerminalType.PrototypeData:
                break;
            case CustomTerminalType.HideoutCache:
                SetupAsCache();
                break;
            case CustomTerminalType.Misc:
                break;
            default:
                break;
        }
    }

    public void OpenDoor()
    {
        // Essentially we want to:
        // - Play the door open "sliding" sound
        _doorSource.Play();
        // - Replace the walls with floor tiles (that have rubble texture).
        foreach (TileBlock W in linkedDoors)
        {
            // TODO
        }
    }

    #endregion

    #region Hideout Cache
    public int storedMatter = 0;
    public void SetupAsCache()
    {
        customType = CustomTerminalType.HideoutCache;
        //specialName = "Hideout Cache (Local)";

        // Setup component inventory
        if(InventoryControl.inst.hideout_inventory == null)
        {
            //InventoryControl.inst.hideout_inventory = new InventoryObject(25, specialName + "'s component Inventory");
        }

        #region Add Commands
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We need to populate this machine with the following commands:
        // -Retrieve (Matter)
        // -Submit (Matter)

        // [Retrieve (Matter)]
        string letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        HackObject hack = MapManager.inst.hackDatabase.Hack[35];

        TerminalCommand newCommand = new TerminalCommand(letter, "Retrieve(Matter)", TerminalCommandType.Retrieve, "", hack);

        //avaiableCommands.Add(newCommand);

        // [Submit (Matter)]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[186];

        newCommand = new TerminalCommand(letter, "Store(Matter)", TerminalCommandType.Submit, "", hack);

        //avaiableCommands.Add(newCommand);
        #endregion


    }

    #endregion

}
