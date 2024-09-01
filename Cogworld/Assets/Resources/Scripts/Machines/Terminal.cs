using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Terminal : MonoBehaviour
{
    public Vector2Int centerLoc; // Where this terminal is in the grid.
    public Vector2Int _size;
    public int securityLevel; // 1, 2, 3
    public int accessLevel; // 1 = 0b10, 2 = Free
    public bool limited; // Limited = for doors only
    public bool locked = false; // No longer accessable

    public TerminalZone zone;
    //public Bot assignedBot; (Technician, Administrator, etc)

    [Header("Commands")]
    public List<TerminalCommand> avaiableCommands = new List<TerminalCommand>();
    public List<TerminalCustomCode> customCodes = new List<TerminalCustomCode>();

    public List<ItemObject> storedObjects;

    [Header("Idenifier")] // Terminal ?### - ? Access
    private string name1;
    private string name2;
    public string fullName;
    /// <summary>
    /// EX: Outpost Terminal (Limited) | Terminal vFe.01a | Terminal vTi.06n
    /// </summary>
    public string systemType;

    [Header("Security")]
    public bool restrictedAccess = true;
    [Tooltip("0, 1, 2, 3. 0 = Open System")]
    public int secLvl = 1;
    public float detectionChance;
    public float traceProgress;
    public bool detected;
    public bool databaseLockout = false;

    [Header("Trojans")]
    public List<TrojanType> trojans = new List<TrojanType>();

    [Header("Audio")]
    public AudioSource _source;
    public AudioClip _ambient;



    public void UseCustomCode(TerminalCustomCode code)
    {
        // idk how this is gonna get parsed
        // Create log messages
        string message = "";
        UIManager.inst.CreateNewLogMessage(message, UIManager.inst.highlightGreen, UIManager.inst.dullGreen, false, true);
    }
}

[System.Serializable]
public class TerminalCommand
{
    public string assignedChar;
    public string command;
    public TerminalCommandType subType; //
    [Tooltip("Record, Analysis, etc (The stuff in darker text)")]
    public string secondaryText;
    public bool available = true; // Has this been used yet? True = no, False = yes

    [Header("Related Hack")]
    public HackObject hack;

    [Header("Reward")]
    public KnowledgeObject knowledge;
    public GlobalActions specialAction;
    public BotObject bot;       // Bot
    public ItemObject item;     // Item (Prototype)

    /// <summary>
    /// Create a new terminal command.
    /// </summary>
    /// <param name="letter">The letter that will represent this command (to the side), and the keybind that can automatically attempt it.</param>
    /// <param name="displayText">The display text of this command (what text you see in the commands list).</param>
    /// <param name="type">The command type (see list).</param>
    /// <param name="subText">Dark green text that will appear before the command's text (ex. Schematic, Record, etc.)</param>
    /// <param name="hackN">The assigned HackObject for this command. MUST BE ASSIGNED!!!</param>
    /// <param name="knowN">A knowledge reward object for this command. Optional.</param>
    /// <param name="action">A global action that happens if this hack succeeds. Optional.</param>
    /// <param name="botNew">An assigned bot object to this command. Optional.</param>
    /// <param name="itemNew">An assigned item object to this command. Optional.</param>
    public TerminalCommand(string letter, string displayText, TerminalCommandType type, string subText = "", HackObject hackN = null, KnowledgeObject knowN = null, GlobalActions action = null, BotObject botNew = null, ItemObject itemNew = null)
    {
        assignedChar = letter;
        command = displayText;
        subType = type;
        secondaryText = subText;
        hack = hackN;
        knowledge = knowN;
        specialAction = action;
        bot = botNew; 
        item = itemNew;
    }
}

[System.Serializable]
public enum TerminalCommandType
{
    // - Terminal
    Access,
    Alert,
    Enumerate,
    Index,
    Inventory,
    Recall,
    Traps,
    Manifests,
    Open, // ---- Terminal (Door)
    Prototypes,
    Query,
    Layout,
    Analysis,
    Schematic,
    Download,
    // - Garrison
    Couplers,
    Seal,
    Unlock,
    // - Fabricator
    LoadIndirect,
    Load,
    Build,
    Network,
    // - Repair Station
    Refit,
    Repair,
    Scan,
    // - Recycling
    Retrieve,
    Recycle,
    // - Scanalyzer
    Insert,
    Scanalyze,
    // - Hacking https://gridsagegames.com/wiki/Unauthorized_Hacking
    Trojan,
    Force,
    // - Hideout Storage
    Submit,
    NONE
}

[System.Serializable]
public class TerminalZone
{
    [Header("Assigned Terminal")]
    public Terminal assignedTerminal;

    [Header("Content")]
    [Tooltip("A collection of points in a map that are assigned to this zone.")]
    public List<Vector2Int> assignedArea = new List<Vector2Int>();
    public List<FloorTrap> trapList = new List<FloorTrap>();
    public List<DoorLogic> emergencyAccessDoors = new List<DoorLogic>();

    public void RevealArea()
    {
        Debug.Log("Revealing area of terminal.");
        foreach (Vector2Int T in assignedArea)
        {
            if (MapManager.inst._allTilesRealized.ContainsKey(T))
            {
                TileBlock tile = MapManager.inst._allTilesRealized[T];
                // Set all the tiles as explored and if not in direct LOS make them green
                // - Don't do cave walls
                if(!(tile.tileInfo.Id == 0 || tile.tileInfo.Id == 2 || tile.tileInfo.Id == 18))
                {
                    // play an animation (once terminal is closed)
                    tile.RevealViaZone();
                }

            }

            if (MapManager.inst._layeredObjsRealized.ContainsKey(T))
            {
                // Set all the tiles as explored and if not in direct LOS make them green

                // - If there are any exits reveal them
            }
        }
    }

    public void RevealLocalEAccess()
    {

    }

    public void RevealTraps()
    {

    }
}

[System.Serializable]
public class TerminalCustomCode
{
    // These are custom codes given to the player by NPCs or data files that they can use in specific instances while hacking.
    // If doing a manual hack on something, if the player knows one or more codes, a custom window should appear.

    [Tooltip("In the form of \\???? (Ex. \\6RCT)")]
    public string code;
    [Tooltip("Where/who this code came from (usually a name). (Ex. EX-BIN)")]
    public string source;
    [Tooltip("Where this code can be used at. (Ex. EX-Vault Control)")]
    public string target;

    public TerminalCustomCode(string nSource, string nTarget)
    {
        code = GenerateRandomString();
        source = nSource;
        target = nTarget;
    }

    /// <summary>
    /// For custom pre-existing codes (Exiles, warlord, etc...)
    /// </summary>
    /// <param name="id">The unique ID to target.</param>
    public TerminalCustomCode(int id)
    {
        switch (id)
        {
            case 0: // Exiles Vault Control
                code = GenerateRandomString();
                source = "EX-BIN";
                target = "EX-Vault Control";
                break;
            case 1:

                break;

            default:

                break;
        }
    }

    private const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string GenerateRandomString()
    {
        string randomString = "";

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, characters.Length);
            randomString += characters[randomIndex];
        }

        return randomString;
    }
}

public enum TrojanType // https://noemica.github.io/cog-minder/hacks.html
{ // This doesn't include FORCE hacks for now
    Track,
    Assimilate,
    Botnet,
    Detonate,
    Broadcast,
    Decoy,
    Redirect,
    Reprogram,
    Disrupt,
    Fabnet,
    Haulers,
    Intercept,
    Mask,
    Mechanics,
    Monitor,
    Operators,
    Prioritize,
    Recyclers,
    Reject,
    Report,
    Researchers,
    Restock,
    Watchers,
    Liberate
}