using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
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
    public bool locked = false; // No longer accessable

    [Header("Trojans")]
    public int trojans = 0;
    public bool trojan_track = false;
    public bool trojan_assimilate = false;
    public bool trojan_botnet = false;
    public bool trojan_detonate = false;

    [Header("Operation")]
    public ItemObject targetPart = null;
    public BotObject targetBot = null;
    [Tooltip("How long it will take to build the specified componenet.")]
    public int buildTime;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;

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

                if(secLvl == 1)
                {
                    buildTime = bot.fabricationInfo.fabTime.x;
                }
                else if(secLvl == 2)
                {
                    buildTime = bot.fabricationInfo.fabTime.y;
                }
                else if( secLvl == 3)
                {
                    buildTime = bot.fabricationInfo.fabTime.z;
                }
            }
            else // 70% chance to be an item
            {
                item = HF.FindItemOfTier(tier, false);

                displayText += item.itemName;

                hack = HF.HackBuildParser(tier, item.star);

                newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, null, item);

                if (secLvl == 1)
                {
                    buildTime = item.fabricationInfo.fabTime.x;
                }
                else if (secLvl == 2)
                {
                    buildTime = item.fabricationInfo.fabTime.y;
                }
                else if (secLvl == 3)
                {
                    buildTime = item.fabricationInfo.fabTime.z;
                }
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

        // Remove the old preloaded build option if its there
        foreach (var command in avaiableCommands.ToList())
        {
            if (command.subType == TerminalCommandType.Build)
            {
                avaiableCommands.Remove(command);
            }
        }

        if (item != null)
        {
            displayText += item.itemName;

            HackObject hack = HF.HackBuildParser(item.rating, item.star);

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, null, item);

            avaiableCommands.Add(newCommand);
        }
        else if(bot != null)
        {
            displayText += bot.name;

            HackObject hack = MapManager.inst.hackDatabase.Hack[16 + bot.rating];

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, bot);

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
    public GameObject timerObject = null;

    public void Build()
    {
        // Set values
        begunBuildTime = TurnManager.inst.globalTime;
        working = true;

        // Create physical timer
        timerObject = Instantiate(UIManager.inst.prefab_machineTimer, this.transform.position, Quaternion.identity);
        timerObject.transform.SetParent(this.transform);
        timerObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Assign Details
        timerObject.GetComponent<UITimerMachine>().Init(buildTime);

        // Remove old build command
        foreach (var command in avaiableCommands.ToList())
        {
            if (command.subType == TerminalCommandType.Build)
            {
                avaiableCommands.Remove(command);
            }
        }

        // Add placeholder (empty) command
        HackObject hack = MapManager.inst.hackDatabase.Hack[26];

        TerminalCommand newCommand = new TerminalCommand(HF.GetNextLetter(avaiableCommands[avaiableCommands.Count - 1].assignedChar), "No Schematic Loaded", TerminalCommandType.NONE, "", hack);

        avaiableCommands.Add(newCommand);
    }

    public void FinishBuild()
    {
        working = false;

        Vector2Int dropLocation = HF.LocateFreeSpace(HF.V3_to_V2I(ejectionSpot.transform.position));

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.GAME_Clips[29], 0.5f); // Play fabrication complete sound

        if (targetPart != null)
        {
            // Spawn in this part on the floor
            InventoryControl.inst.CreateItemInWorld(targetPart.data.Id, dropLocation, true);
        }
        else if(targetBot != null)
        {
            // Spawn in a new ALLIED bot at this location
            Actor newBot = MapManager.inst.PlaceBot(dropLocation, targetBot.Id);
            newBot.directPlayerAlly = true;

            // Modify relations to be friendly to the player and neutral to some other functions
            List<BotRelation> relationList = new List<BotRelation>();

            relationList.Add(BotRelation.Hostile); // Complex
            relationList.Add(BotRelation.Neutral); // Derelict
            relationList.Add(BotRelation.Hostile); // Assembled
            relationList.Add(BotRelation.Neutral); // Warlord
            relationList.Add(BotRelation.Neutral); // Zion
            relationList.Add(BotRelation.Neutral); // Exiles
            relationList.Add(BotRelation.Hostile); // Architect
            relationList.Add(BotRelation.Neutral); // Subcaves
            relationList.Add(BotRelation.Hostile); // Subcaves Hostile
            relationList.Add(BotRelation.Friendly); // Player
            relationList.Add(BotRelation.Neutral); // None

            HF.ModifyBotAllegance(newBot, relationList);
        }

        PlayerData.inst.GetComponent<Actor>().UpdateFieldOfView();

        Destroy(timerObject);

        targetPart = null;
        targetBot = null;
        buildTime = 0;
        begunBuildTime = 0;
    }

    /// <summary>
    /// Called in GameManager every turn.
    /// </summary>
    public void Check()
    {
        if (working)
        {
            timerObject.GetComponent<UITimerMachine>().Tick();

            if(TurnManager.inst.globalTime >= begunBuildTime + buildTime)
            {
                FinishBuild();
            }
        }
    }

    #region Hacks
    public void Force()
    {
        locked = true;

        // Recolor to gray, this terminal is now locked
        foreach (var P in this.GetComponent<MachinePart>().connectedParts)
        {
            P.GetComponent<SpriteRenderer>().color = Color.white;
        }
        this.GetComponent<MachinePart>().parentPart.GetComponent<SpriteRenderer>().color = Color.white;

        if(targetBot != null || targetPart != null)
        {
            Build();
        }
    }

    #endregion
}
