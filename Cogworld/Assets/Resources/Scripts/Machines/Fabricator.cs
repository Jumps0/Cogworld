using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class Fabricator : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Fabricator vF.08n
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

    [Header("Trojans")]
    public int trojans = 0;
    public bool trojan_track = false;
    public bool trojan_assimilate = false;
    public bool trojan_botnet = false;
    public bool trojan_detonate = false;

    [Header("Operation")]
    public ItemObject targetPart = null;
    public BotObject targetBot = null;
    public int buildTime;
    public bool working = false;

    public void Init()
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We need to populate this machine with the following commands:
        // -Network Status
        // -Load Schematic
        // -A pre-loaded build command for a bot/item appropriate to the level + 1 (this is random, it can be empty sometimes)

        // [Network Status]
        string letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        HackObject hack = MapManager.inst.hackDatabase.Hack[27];

        TerminalCommand newCommand = new TerminalCommand(letter, "Network Status", TerminalCommandType.Network, "", hack);

        avaiableCommands.Add(newCommand);

        // [Load Schematic]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[185];

        newCommand = new TerminalCommand(letter, "Load Schematic", TerminalCommandType.LoadIndirect, "", hack);

        avaiableCommands.Add(newCommand);

        // Preload
        if(Random.Range(0f, 1f) > 0.5f) // 50/50
        {
            letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string displayText = "Build ";

            // Pick what to show
            // -Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }
            
            if(tier < 10)
            {
                tier++; // +1 for better rewards
            }

            BotObject bot = null;
            ItemObject item = null;

            if(Random.Range(0f, 1f) > 0.7f) // 30% to be a Bot
            {
                bot = HF.FindBotOfTier(tier);

                hack = MapManager.inst.hackDatabase.Hack[16 + tier]; // bot commands actually start at 17 but the lowest tier can be is 1 so 16 + 1 = 17.

                displayText += bot.name;

                newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, bot);
            }
            else // 70% chance to be an item
            {
                item = HF.FindItemOfTier(tier, false);

                displayText += item.itemName;

                hack = HF.HackBuildParser(tier, item.star);

                newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, null, item);
            }

            avaiableCommands.Add(newCommand);
        }
        else // We need to show the "No Schematic Loaded" false command
        {
            hack = MapManager.inst.hackDatabase.Hack[26];

            newCommand = new TerminalCommand(letter, "No Schematic Loaded", TerminalCommandType.NONE, "", hack);

            avaiableCommands.Add(newCommand);
        }

    }

    public void AddBuildCommand(ItemObject item = null, BotObject bot = null)
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        string letter = HF.GetNextLetter(avaiableCommands[avaiableCommands.Count - 1].assignedChar);
        string displayText = "Build ";

        if (item != null)
        {
            displayText += item.itemName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", null, null, null, null, item);

            avaiableCommands.Add(newCommand);
        }
        else if(bot != null)
        {
            displayText += bot.name;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", null, null, null, bot);

            avaiableCommands.Add(newCommand);
        }

        // Remove the [No Schematic Loaded] option if its there
        foreach (var command in avaiableCommands.ToList())
        {
            if(command.subType == TerminalCommandType.NONE)
            {
                avaiableCommands.Remove(command);
            }
        }

        // And refresh the options
        UIManager.inst.Terminal_RefreshHackingOptions();
    }


    // -- Load -- //
    public void Load(int time, ItemObject item = null, BotObject bot = null)
    {
        if(item != null)
        {
            targetPart = item;
        }
        else if(bot != null)
        {
            targetBot = bot;
        }

        buildTime = time;

        AddBuildCommand(item, bot);
    }

    // -- Build -- //
    public int begunBuildTime = 0;

    public void Build()
    {
        begunBuildTime = TurnManager.inst.globalTime;
        working = true;
    }

    public void FinishBuild()
    {
        working = false;

    }

    public void Check()
    {
        if (working)
        {
            if(begunBuildTime + buildTime >= TurnManager.inst.globalTime)
            {
                FinishBuild();
            }
        }
    }
    
}
