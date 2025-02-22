using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Terminal : InteractableMachine
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
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Terminal;

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        int amount = 0;

        // -- We want to generate:
        // - 1-4 Lore entries
        // - 0-2 Access+Alert entries
        // - 0-1 Analysis entries (scaled to level)
        // - 0-2 Enumerate entries
        // - 0-1 Index entries
        // - 0-1 Layout(Zone) entries
        // - 0-1 Recall entries (make sure to check if there is one first)
        // - 0-1 Schematic (item) entries (scaled to level)
        // - 0-1 Schematic (bot) entries (scaled to level)
        // - 0-1 Traps entries

        // Lore
        for (int i = 0; i < Random.Range(1, 4); i++)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);
            //
            KnowledgeObject data = MapManager.inst.knowledgeDatabase.Data[Random.Range(0, MapManager.inst.knowledgeDatabase.Data.Length)];
            string displayText = "\"" + data.name + "\"";

            HackObject hack = MapManager.inst.hackDatabase.dict["Query"];

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Query, "Record", hack, data);

            avaiableCommands.Add(newCommand);

            amount++;
        }

        // Access [73-75] Alert [76-77]
        if (Random.Range(0f, 1f) >= 0.4f)
        {
            // Access here
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string[] options = { "Access(Branch)", "Access(Emergency)", "Access(Main)" };
            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Length - 1)]];
            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Access, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        if (Random.Range(0f, 1f) >= 0.5f)
        {
            // Alert here
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string[] options = { "Alert(Check)", "Alert(Purge)" };
            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Length - 1)]];
            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Alert, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Analysis [78-87]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Analysis([Bot Name]) - Tier {tier}"];

            BotObject bot = HF.FindBotOfTier(tier);

            string displayText = bot.botName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Analysis, "Analysis", hack, null, null, bot);

            avaiableCommands.Add(newCommand);
        }

        // Enumerate [91-103]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Enumerate(Assaults)");
            options.Add("Enumerate(Coupling)");
            options.Add("Enumerate(Exterminations)");
            options.Add("Enumerate(Garrison)");
            options.Add("Enumerate(Guards)");
            options.Add("Enumerate(Intercept)");
            options.Add("Enumerate(Investigations)");
            options.Add("Enumerate(Maintenance)");
            options.Add("Enumerate(Patrols)");
            options.Add("Enumerate(Reinforcements)");
            options.Add("Enumerate(Squads)");
            options.Add("Enumerate(Surveillance)");
            options.Add("Enumerate(Transport)");

            int index = Random.Range(0, options.Count - 1);
            HackObject hack = MapManager.inst.hackDatabase.dict[options[index]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

            avaiableCommands.Add(newCommand);

            // Then do another one
            options.RemoveAt(index);
            index = Random.Range(0, options.Count - 1);
            hack = MapManager.inst.hackDatabase.dict[options[index]];

            displayText = hack.trueName;

            newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Index [104-110]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Index(Fabricators)");
            options.Add("Index(Garrisons)");
            options.Add("Index(Machines)");
            options.Add("Index(Recycling Units)");
            options.Add("Index(Repair Stations)");
            options.Add("Index(Scanalyzers)");
            options.Add("Index(Terminals)");

            int rand = Random.Range(0, options.Count - 1);
            HackObject hack = MapManager.inst.hackDatabase.dict[options[rand]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Index, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Layout (Zone)
        if (Random.Range(0f, 1f) >= 0.6f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            HackObject hack = MapManager.inst.hackDatabase.dict["Layout(Zone)"];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Layout, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Recall
        if (GameManager.inst.activeAssaults.Count > 0 || GameManager.inst.activeExterminations.Count > 0 || GameManager.inst.activeInvestigations.Count > 0 || GameManager.inst.activeReinforcements.Count > 0)
        {
            if ((Random.Range(0f, 1f) >= 0.4f))
            {
                string letter = alphabet[0].ToString().ToLower();
                alphabet.Remove(alphabet[0]);

                HackObject hack = null;

                if (GameManager.inst.activeAssaults.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Assaults)"];
                }
                else if (GameManager.inst.activeExterminations.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Extermination)"];
                }
                else if (GameManager.inst.activeInvestigations.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Investigation)"];
                }
                else if (GameManager.inst.activeReinforcements.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Reinforcements)"];
                }

                string displayText = hack.trueName;

                TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Recall, "", hack, null);

                avaiableCommands.Add(newCommand);
            }
        }

        // Schematic (Item) [129-145]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 2) // temp fix
            {
                tier = 3;
            }

            bool star = false;
            if (Random.Range(0f, 1f) > 0.65f) // Chance to get a "starred" item
            {
                star = true;
            }
            string p = "";
            if (star)
            {
                p = "P";
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Schematic([Part Name]) - Rating {tier}{p}"];

            ItemObject item = HF.FindItemOfTier(tier);

            string displayText = item.itemName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, null, item);

            avaiableCommands.Add(newCommand);
        }

        // Schematic (Bot) [120-128]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Schematic([Bot Name]) - Tier {tier}"];

            BotObject bot = HF.FindBotOfTier(tier);

            string displayText = bot.botName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, bot);

            avaiableCommands.Add(newCommand);
        }
        // Traps [146-148]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Traps(Disarm)");
            options.Add("Traps(Locate)");
            options.Add("Traps(Reprogram)");

            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Count - 1)]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "", hack, null);

            avaiableCommands.Add(newCommand);
        }
    }

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
    public Terminal assignedTerminal;

    [Header("Content")]
    [Tooltip("A collection of points in a map that are assigned to this zone.")]
    public List<Vector2Int> assignedArea = new List<Vector2Int>();
    public List<WorldTile> trapList = new List<WorldTile>();
    public List<DoorLogic> emergencyAccessDoors = new List<DoorLogic>();

    public void RevealArea()
    {
        Debug.Log("Revealing area of terminal.");
        foreach (Vector2Int T in assignedArea)
        {
            if (MapManager.inst._allTilesRealized.ContainsKey(T))
            {
                TileBlock tile = MapManager.inst._allTilesRealized[T].bottom;
                // Set all the tiles as explored and if not in direct LOS make them green
                // - Don't do cave walls
                if(!(tile.tileInfo.Id == 0 || tile.tileInfo.Id == 2 || tile.tileInfo.Id == 18))
                {
                    // play an animation (once terminal is closed)
                    tile.RevealViaZone();
                }

            }

            if (MapManager.inst._allTilesRealized.ContainsKey(T)) // TODO
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