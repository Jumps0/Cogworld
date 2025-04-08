using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Terminal : MonoBehaviour
{
    public Vector2Int centerLoc; // Where this terminal is in the grid.

    public TerminalZone zone;
    public Actor assignedBot; // (Technician, Administrator, etc)

    public List<TerminalCustomCode> customCodes = new List<TerminalCustomCode>();

    public List<ItemObject> storedObjects;

    public bool databaseLockout = false;

    [Header("Audio")]
    public AudioSource _source;
    public AudioClip _ambient;

    public void Init()
    {
        
    }

    public void UseCustomCode(TerminalCustomCode code)
    {

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
    [Tooltip("Has this been used yet? True = no, False = yes")]
    public bool available = true;
    [Tooltip("Can this command be used multiple times? (Re-open terminal to see it again) Normally false, a few commands can be done again.")]
    public bool repeatable = false;

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
    public Vector2Int assignedTerminal;

    [Header("Content")]
    [Tooltip("A collection of points in a map that are assigned to this zone.")]
    public List<Vector2Int> assignedArea = new List<Vector2Int>();
    public List<WorldTile> trapList = new List<WorldTile>();
    public List<WorldTile> emergencyAccessDoors = new List<WorldTile>();

    public void RevealArea()
    {
        Debug.Log("Revealing area of terminal.");
        foreach (Vector2Int T in assignedArea)
        {
            WorldTile tile = MapManager.inst.mapdata[T.x, T.y];
            // Set all the tiles as explored and if not in direct LOS make them green
            // - Don't do cave walls
            if (!tile.tileInfo.isCaveWall)
            {
                // play an animation (once terminal is closed)
                //tile.RevealViaZone(); // TODO: Rework this (old code in TileObject.s)
            }

            // TODO

            // Set all the tiles as explored and if not in direct LOS make them green

            // - If there are any exits reveal them
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