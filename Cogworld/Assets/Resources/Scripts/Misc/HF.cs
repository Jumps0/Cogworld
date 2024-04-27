using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using TMPro;
using DungeonResources;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;
using UnityEngine.UIElements;
using UnityEngine.Windows;

/// <summary>
/// Contains helper functions to be used globally.
/// </summary>
public static class HF
{
    #region Vector Conversions
    public static Vector2Int V3_to_V2I(Vector3 v3)
    {
        Vector2Int vector = new Vector2Int(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y));
        return vector;

        //return new Vector2Int((int)v3.x, (int)v3.y);
    }

    public static Vector2Int IV2_to_V2I(IntVector2 v2)
    {
        return new Vector2Int((int)v2.x, (int)v2.y);
    }

    public static List<Vector2Int> LIST_IV2_to_V2I(List<IntVector2> v2)
    {
        List<Vector2Int> retList = new List<Vector2Int>();

        foreach (IntVector2 V in v2)
        {
            retList.Add(HF.IV2_to_V2I(V));
        }

        return retList;
    }
    #endregion

    public static TileType Tile_to_TileType(Tile type)
    {
        switch (type)
        {
            case Tile.Wall:
                return TileType.Wall;
            case Tile.Floor:
                return TileType.Floor;
            case Tile.Door:
                return TileType.Door;
            default:
                return TileType.Floor;
        }
    }

    public static int IDbyTheme(TileType type)
    {
        // This is going to suck
        if (type == TileType.Wall)
        {
            #region Tile ID Guide
            /* Guide:
            0 = Impassible DEV wall (Map border)
            1 = Materials walls
            2 = Cave walls
            3 = Industrial cave walls
            4 = Materials Floor Tile
            5 = Materials Door
            6 = ACCESS_MAIN (Stairs)
            7 = ACCESS_BRANCH (Door)
            8 = Factory wall
            9 = DSF Wall (Orange)
            10 = Waste wall (Brown)
            11 = Garrison wall (Red)
            12 = Factory Door
            13 = Emergency Access Door
            14 = Zhirov Wall (Blue)
            15 = Research wall (Purple)
            16 = Research Door
            17 = Triangle Door
            18 = Upper cave walls (the other rocky texture)
            19 = Testing (Green)
            20 = Architect wall (White) also for access and lab
            21 = Cave vault wall (Orange)
            22 = Cave floor (Smooth floor with no rivits in the corners)
            */
            #endregion

            switch (MapManager.inst.currentLevelName)
            {
                case "MATERIALS":
                    return 1;
                case "LOWER CAVES":
                    return 2;
                case "STORAGE":
                    return 1;
                case "DSF":
                    return 9;
                case "GARRISON":
                    return 11;
                case "FACTORY":
                    return 8;
                case "EXTENSION":
                    return 1;
                case "UPPER CAVES":
                    return 2;
                case "RESEARCH":
                    return 1;
                case "ACCESS":
                    return 20;
                case "COMMAND":
                    return 1;
                case "ARMORY":
                    return 11;
                case "WASTE":
                    return 10;
                case "HUB":
                    return 1;
                case "ARCHIVES":
                    return 1;
                case "CETUS":
                    return 14;
                case "ARCHITECT":
                    return 20;
                case "ZHIROV":
                    return 14;
                case "DATA MINER":
                    return 1;
                case "EXILES":
                    return 18;
                case "WARLORD":
                    return 18;
                case "SECTION 7":
                    return 19;
                case "TESTING":
                    return 19;
                case "QUARANTINE":
                    return 19;
                case "LAB":
                    return 20;
                case "HUB_04(d)":
                    return 21;
                default:
                    return 1;
                    // EXPAND THIS LATER
            }
        }
        else if (type == TileType.Floor)
        {
            switch (MapManager.inst.currentLevelName)
            { // We have two types of floor tiles at the moment. One thats clean (for caves), and one thats industrial (for everythign else)
                case "MATERIALS":
                    return 4;
                case "LOWER CAVES":
                    return 22;
                case "STORAGE":
                    return 4;
                case "DSF":
                    return 4;
                case "GARRISON":
                    return 4;
                case "FACTORY":
                    return 4;
                case "EXTENSION":
                    return 4;
                case "UPPER CAVES":
                    return 22;
                case "RESEARCH":
                    return 4;
                case "ACCESS":
                    return 4;
                case "COMMAND":
                    return 4;
                case "ARMORY":
                    return 4;
                case "WASTE":
                    return 4;
                case "HUB":
                    return 4;
                case "ARCHIVES":
                    return 4;
                case "CETUS":
                    return 4;
                case "ARCHITECT":
                    return 22;
                case "ZHIROV":
                    return 22;
                case "DATA MINER":
                    return 22;
                case "EXILES":
                    return 22;
                case "WARLORD":
                    return 22;
                case "SECTION 7":
                    return 4;
                case "TESTING":
                    return 4;
                case "QUARANTINE":
                    return 4;
                case "LAB":
                    return 4;
                case "HUB_04(d)":
                    return 4;
                default:
                    return 4;
                    // EXPAND THIS LATER
            }
        }
        else if (type == TileType.Door)
        {
            switch (MapManager.inst.currentLevelName)
            {
                case "MATERIALS":
                    return 5;
                case "LOWER CAVES":
                    return 5;
                case "STORAGE":
                    return 5;
                case "DSF":
                    return 5;
                case "GARRISON":
                    return 5;
                case "FACTORY":
                    return 12;
                case "EXTENSION":
                    return 5;
                case "UPPER CAVES":
                    return 5;
                case "RESEARCH":
                    return 16;
                case "ACCESS":
                    return 5;
                case "COMMAND":
                    return 5;
                case "ARMORY":
                    return 5;
                case "WASTE":
                    return 5;
                case "HUB":
                    return 5;
                case "ARCHIVES":
                    return 5;
                case "CETUS":
                    return 5;
                case "ARCHITECT":
                    return 5;
                case "ZHIROV":
                    return 5;
                case "DATA MINER":
                    return 5;
                case "EXILES":
                    return 18;
                case "WARLORD":
                    return 18;
                case "SECTION 7":
                    return 16;
                case "TESTING":
                    return 16;
                case "QUARANTINE":
                    return 16;
                case "LAB":
                    return 19;
                case "HUB_04(d)":
                    return 5;
                default:
                    return 5;
                    // EXPAND THIS LATER
            }
        }
        else
        {
            return 1;
        }
    }

    #region Machines/Hacking
    public static string HackDescription(TrojanType type)
    {
        switch (type)
        {
            case TrojanType.Track:
                return "Continuously report the position of all robots within a short distance of the terminal. The radius is 6 for a level 1 terminal, 8 for a level 2, and 10 for a level 3.";
            case TrojanType.Assimilate:
                return "Causes any operator attempting to report Cogmind to instead become a controllable (blue) ally.";
            case TrojanType.Botnet:
                return "Increase the success rate of hacking attempts on all other machines on the floor. The first hack increases success by 6%, the second by 3%, and all others by 1%.";
            case TrojanType.Detonate:
                return "Rig a nearby explosive machine to detonate when a hostile passes nearby.";
            case TrojanType.Broadcast:
                return "Enable reporting of any garrison dispatch type, robot composition, and position on the floor.";
            case TrojanType.Decoy:
                return "Redirect the next dispatch's target to another location, includes dispatches with otherwise perfect tracking like exterminations and assaults.";
            case TrojanType.Redirect:
                return "Redirect all dispatch targets to another location, includes dispatches with otherwise perfect tracking like exterminations and assaults. Each dispatch after the first has an increasing 25% chance of removing the hack, dispatching an extra investigation squad, and disabling the garrison machine.";
            case TrojanType.Reprogram:
                return "Reprogram the next dispatch to be allied but noncontrollable to Cogmind (purple color). Doesn't work against special dispatches like Lightnings.";
            case TrojanType.Disrupt:
                return "Reduce the targeting of all nearby hostile robots by 10%.";
            case TrojanType.Fabnet:
                return "Adds a 3% chance per 0b10 combat bot on the next depth up to temporarily turn friendly (purple) when alerted of any enemy (not just Cogmind) similar to the RIF hack overwrite IFF. This chance decreases to 1% on the following depth, and reaches 0% after that. Caps at 15%. There is a chance that the machine network on the floor will be permanently locked when this effect activates.";
            case TrojanType.Haulers:
                return "Continuously report the position of all Hauler class robots on the current floor.";
            case TrojanType.Intercept:
                return "Increases accuracy against nearby MAIN.C-controlled bot by 15% for all friendly bots.";
            case TrojanType.Mask:
                return "Prevent Recycler robots from picking up any parts in a 15-tile square area around the machine.";
            case TrojanType.Mechanics:
                return "Continuously report the position of all Mechanic class robots on the current floor.";
            case TrojanType.Monitor:
                return "Enable reporting of any parts inserted into any Recycling Machine, as well as their integrity.";
            case TrojanType.Operators:
                return "Continuously report the position of all Operator class robots on the current floor.";
            case TrojanType.Prioritize:
                return "Double the speed of fabrication for the current fabricator.";
            case TrojanType.Recyclers:
                return "Continuously report the position of all Recycler class robots on the current floor.";
            case TrojanType.Reject:
                return "Prevent parts from being deposited into the recycling machine. Instead, any inserted parts are deposited onto the floor.";
            case TrojanType.Report:
                return "Reports when any fabricator on the floor is finished building something, as well as its position.";
            case TrojanType.Researchers:
                return "Continuously report the position of all Researcher class robots on the current floor.";
            case TrojanType.Restock:
                return "Dispatches a programmer to the current garrison carrying relay couplers.";
            case TrojanType.Watchers:
                return "Continuously report the position of all Watcher class robots on the current floor.";
            case TrojanType.Liberate:
                return "Any bots created by this fabricator by 0b10 will be allied but noncontrollable to Cogmind (purple color). If on a factory floor, the bots will head for the closest caves exit and leave the map. Otherwise the bots will follow you as regular purple allies.";
            default:
                return "";
        }
    }

    public static int MachineSecLvl()
    {
        float random = Random.Range(0f, 1f);
        if (MapManager.inst.currentLevel < -9) // -11 to -10
        {
            if (random >= 0.95) // 5% - High Sec
            {
                return 3;
            }
            else if (random < 0.95 && random >= 0.60) // 35% - Medium Sec
            {
                return 2;
            }
            else // 60% - Low Sec
            {
                return 1;
            }
        }
        else if (MapManager.inst.currentLevel < -6) // -9 to -7
        {
            if (random >= 0.80) // 20% - High Sec
            {
                return 3;
            }
            else if (random < 0.80 && random >= 0.45) // 35% - Medium Sec
            {
                return 2;
            }
            else // 45% - Low Sec
            {
                return 1;
            }
        }
        else if (MapManager.inst.currentLevel < -3) // -6 to -4
        {
            if (random >= 0.70) // 30% - High Sec
            {
                return 3;
            }
            else if (random < 0.70 && random >= 0.25) // 50% - Medium Sec
            {
                return 2;
            }
            else // 25% - Low Sec
            {
                return 1;
            }
        }
        else // -3 to -1
        {
            if (random >= 0.60) // 40% - High Sec
            {
                return 3;
            }
            else if (random < 0.60 && random >= 0.15) // 45% - Medium Sec
            {
                return 2;
            }
            else // 15% - Low Sec
            {
                return 1;
            }
        }
    }

    public static string GetTerminalSystemName(GameObject machine)
    {
        if (machine != null)
        {
            if (machine.GetComponent<Terminal>()) // Open Terminal
            {
                return machine.GetComponent<Terminal>().systemType;
            }
            else if (machine.GetComponent<Fabricator>()) // Open Fabricator
            {
                return machine.GetComponent<Fabricator>().systemType;
            }
            else if (machine.GetComponent<Scanalyzer>()) // Open Scanalyzer
            {
                return machine.GetComponent<Scanalyzer>().systemType;
            }
            else if (machine.GetComponent<RepairStation>()) // Open Repair Station
            {
                return machine.GetComponent<RepairStation>().systemType;
            }
            else if (machine.GetComponent<RecyclingUnit>()) // Open Recycling Unit
            {
                return machine.GetComponent<RecyclingUnit>().systemType;
            }
            else if (machine.GetComponent<Garrison>()) // Open Garrison
            {
                return machine.GetComponent<Garrison>().systemType;
            }
            else if (machine.GetComponent<TerminalCustom>()) // Open Custom Terminal
            {
                return machine.GetComponent<TerminalCustom>().systemType;
            }
        }

        return "Unknown Terminal";
    }

    public static int GetMachineSecLvl(GameObject machine)
    {
        if (machine != null)
        {
            if (machine.GetComponent<Terminal>()) // Open Terminal
            {
                return machine.GetComponent<Terminal>().secLvl;
            }
            else if (machine.GetComponent<Fabricator>()) // Open Fabricator
            {
                return machine.GetComponent<Fabricator>().secLvl;
            }
            else if (machine.GetComponent<Scanalyzer>()) // Open Scanalyzer
            {
                return machine.GetComponent<Scanalyzer>().secLvl;
            }
            else if (machine.GetComponent<RepairStation>()) // Open Repair Station
            {
                return machine.GetComponent<RepairStation>().secLvl;
            }
            else if (machine.GetComponent<RecyclingUnit>()) // Open Recycling Unit
            {
                return machine.GetComponent<RecyclingUnit>().secLvl;
            }
            else if (machine.GetComponent<Garrison>()) // Open Garrison
            {
                return machine.GetComponent<Garrison>().secLvl;
            }
            else if (machine.GetComponent<TerminalCustom>()) // Open Custom Terminal
            {
                return machine.GetComponent<TerminalCustom>().secLvl;
            }
        }

        return 0;
    }

    public static string GetMachineType(GameObject machine)
    {
        if (machine != null)
        {
            if (machine.GetComponent<Terminal>()) // Open Terminal
            {
                return "Terminal";
            }
            else if (machine.GetComponent<Fabricator>()) // Open Fabricator
            {
                return "Fabricator";
            }
            else if (machine.GetComponent<Scanalyzer>()) // Open Scanalyzer
            {
                return "Scanalyzer";
            }
            else if (machine.GetComponent<RepairStation>()) // Open Repair Station
            {
                return "Repair Bay";
            }
            else if (machine.GetComponent<RecyclingUnit>()) // Open Recycling Unit
            {
                return "Recycling Unit";
            }
            else if (machine.GetComponent<Garrison>()) // Open Garrison
            {
                return "Garrison";
            }
            else if (machine.GetComponent<TerminalCustom>()) // Open Custom Terminal
            {
                return "Terminal";
            }
        }

        return "Unknown";
    }

    public static float CalculateHackSuccessChance(float baseChance)
    {
        float success = baseChance;

        // -- Hack Bonus --
        List<ItemObject> hackware = new List<ItemObject>();
        bool hasHackware = false;
        (hasHackware, hackware) = Action.FindPlayerHackware();
        float hackwareBonus = 0f;

        if (hasHackware)
        {
            foreach (ItemObject item in hackware)
            {
                if (item.itemEffects.Count > 0)
                {
                    foreach (ItemEffect effect in item.itemEffects)
                    {
                        if (effect.hackBonuses.hasHackBonus)
                        {
                            hackwareBonus += effect.hackBonuses.hackSuccessBonus;
                        }
                    }
                }
            }
        }

        success -= hackwareBonus;
        // -- Corruption --
        float corruptionMod = 0f;
        if (PlayerData.inst.currentCorruption >= 3) // 1% per every 3 points
        {
            int amount = PlayerData.inst.currentCorruption / 3;
            corruptionMod = amount / 100;
        }
        success -= corruptionMod;

        // -- Operators --
        // The number of active operator allies within 20 spaces.
        // The first provides +10%, the second 5%, the third 2%, and all remaining provide +1% cummulative success rate to all hacks.
        float operatorBonus = 0f;
        PlayerData.inst.linkedOperators = 0;
        if (PlayerData.inst.allies.Count > 0)
        {
            foreach (Actor ally in PlayerData.inst.allies)
            {
                if (ally.botInfo != null && ally.botInfo._class == BotClass.Operator)
                {
                    // Now do the distance check
                    float distance = Vector3.Distance(PlayerData.inst.gameObject.transform.position, ally.gameObject.transform.position);
                    if (distance <= 20f)
                    {
                        // Success!
                        PlayerData.inst.linkedOperators++;
                    }
                }
            }
        }

        int linked = PlayerData.inst.linkedOperators;
        if (linked > 0)
        {
            if (linked == 1)
            {
                operatorBonus += -0.1f;
            }
            else if (linked == 2)
            {
                operatorBonus += -0.1f;
                operatorBonus += -0.05f;
            }
            else if (linked == 3) {
                operatorBonus += -0.1f;
                operatorBonus += -0.05f;
                operatorBonus += -0.02f;
            }
            else if (linked > 3)
            {
                operatorBonus += -0.1f;
                operatorBonus += -0.05f;
                operatorBonus += -0.02f;
                operatorBonus += (-0.01f * (linked - 3));
            }
        }

        success -= operatorBonus;

        // -- Botnets --
        // The number of botnets. The first provides +6% bonus, the second 3%, and all remaining provide +1% cummulative success rate to all hacks.
        float botnetBonus = 0f;
        int botnet = PlayerData.inst.linkedOperators;
        if (botnet > 0)
        {
            if (botnet == 1)
            {
                botnetBonus += -0.06f;
            }
            else if (botnet == 2)
            {
                botnetBonus += -0.06f;
                botnetBonus += -0.03f;
            }
            else if (botnet > 2)
            {
                botnetBonus += -0.06f;
                botnetBonus += -0.03f;
                botnetBonus += (-0.01f * (botnet - 2));
            }
        }

        success -= botnetBonus;

        return success;
    }

    public static string MachineReward_PrintPLUSAction(TerminalCommand command, ItemObject item = null, BotObject bot = null)
    {
        string result = "";

        if (command.knowledge != null) // This is a knowledge reward
        {
            if (command.bot != null) // This is a bot knowledge reward
            {
                command.bot.playerHasAnalysisData = true;
                return "Downloading analysis...\n    " + command.bot.name + "\n    Tier: " + command.bot.tier + "\n" + command.bot.description;
            }
            else if (command.item != null) // This is an item (prototype) knowledge reward
            {
                command.item.knowByPlayer = true;
                return "Downloading analysis...\n    " + command.item.name + "\n    Rating: " + command.item.rating + "\n" + command.item.description;
            }
            else // This is (probably) a lore reward
            {
                result = command.knowledge.lore;
            }
        }
        else // This is a special action reward
        {
            string parsedName = HF.ParseHackName(command.hack);

            switch (command.hack.hackType)
            {
                case TerminalCommandType.Access:
                    if (command.secondaryText == "main")
                    {
                        string print = "";
                        if (MapManager.inst.placedExits.Count > 0)
                        {
                            GameManager.inst.AccessMain();
                            print = $"Found {MapManager.inst.placedExits.Count} main access points:\n";
                            foreach (GameObject exit in MapManager.inst.placedExits)
                            {
                                Vector2Int location = HF.V3_to_V2I(exit.transform.position);
                                string locationName = exit.GetComponent<AccessObject>().destName;
                                locationName = locationName.Substring(0, 1).ToUpper() + locationName.Substring(1).ToLower(); // Reformat (all caps to norm /w capitalization)

                                print += $"  ({location.x},{location.y}) {locationName}\n";
                            }

                            print += "Map Updated.\n";
                        }
                        else
                        {
                            print = "No primary access points found.";
                        }

                        return print;
                    }
                    else if (command.secondaryText == "branch")
                    {
                        string print = "";
                        if (MapManager.inst.placedBranches.Count > 0)
                        {
                            GameManager.inst.AccessBranch();
                            print = $"Found {MapManager.inst.placedBranches.Count} branch access points:\n";
                            foreach (GameObject exit in MapManager.inst.placedBranches)
                            {
                                Vector2Int location = HF.V3_to_V2I(exit.transform.position);
                                string locationName = exit.GetComponent<AccessObject>().destName;
                                locationName = locationName.Substring(0, 1).ToUpper() + locationName.Substring(1).ToLower(); // Reformat (all caps to norm /w capitalization)

                                print += $"  ({location.x},{location.y}) {locationName}\n";
                            }

                            print += "Map Updated.\n";
                        }
                        else
                        {
                            print = "No primary access points found.";
                        }

                        return print;
                    }
                    else if (command.secondaryText == "emergency")
                    {
                        GameManager.inst.AccessEmergency(UIManager.inst.terminal_targetTerm);
                        return "Local emergency access data updated.";
                    }
                    break;
                case TerminalCommandType.Alert:
                    if (parsedName.Contains("Purge"))
                    {
                        if (PlayerData.inst.alertLevel > 0)
                        {
                            PlayerData.inst.alertLevel--;
                            return ("Purged threat record, alert level lowered.");
                        }
                        else
                        {
                            return ("Alert level is already at its lowest.");
                        }
                    }
                    else if (parsedName.Contains("Check"))
                    {
                        int alertLvl = PlayerData.inst.alertLevel;
                        if (alertLvl == 0)
                        {
                            return "Current Alert Level: Low Security";
                        }
                        else if (alertLvl != 6)
                        {
                            string characters2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // i dont understand this?
                            string rstring = characters2[Random.Range(0, characters2.Length - 1)].ToString();

                            return "Current Alert Level: " + alertLvl + "-" + rstring;
                        }
                        else
                        {
                            return "Current Alert Level: High Security";
                        }
                    }
                    break;
                case TerminalCommandType.Enumerate:
                    break;
                case TerminalCommandType.Index:
                    if (parsedName.Contains("Fabricators"))
                    {
                        GameManager.inst.IndexMachinesGeneric(0);
                        return ("Found " + MapManager.inst.machines_fabricators.Count + " fabricators.\nDownloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Garrisons"))
                    {
                        GameManager.inst.IndexMachinesGeneric(1);
                        return ("Found " + MapManager.inst.machines_garrisons.Count + " garrisons.\nDownloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Machines")) // aka all interactable
                    {
                        GameManager.inst.IndexMachinesGeneric(2);
                        string mrs = "Found " + (MapManager.inst.machines_fabricators.Count + MapManager.inst.machines_repairStation.Count + MapManager.inst.machines_recyclingUnits.Count + MapManager.inst.machines_scanalyzers.Count + MapManager.inst.machines_garrisons.Count) +
                            " machines: \n";
                        if(MapManager.inst.machines_terminals.Count > 0)
                        {
                            if(MapManager.inst.machines_terminals.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_terminals.Count + " Terminals\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_terminals.Count + " Terminals\n";
                            }
                        }
                        if (MapManager.inst.machines_fabricators.Count > 0)
                        {
                            if (MapManager.inst.machines_fabricators.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_fabricators.Count + " Fabricators\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_fabricators.Count + " Fabricators\n";
                            }
                        }
                        if (MapManager.inst.machines_repairStation.Count > 0)
                        {
                            if (MapManager.inst.machines_repairStation.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_repairStation.Count + " Repair Stations\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_repairStation.Count + " Repair Stations\n";
                            }
                        }
                        if (MapManager.inst.machines_recyclingUnits.Count > 0)
                        {
                            if (MapManager.inst.machines_recyclingUnits.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_recyclingUnits.Count + " Recycling Units\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_recyclingUnits.Count + " Recycling Units\n";
                            }
                        }
                        if (MapManager.inst.machines_scanalyzers.Count > 0)
                        {
                            if (MapManager.inst.machines_scanalyzers.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_scanalyzers.Count + " Scanalyzers\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_scanalyzers.Count + " Scanalyzers\n";
                            }
                        }
                        if (MapManager.inst.machines_garrisons.Count > 0)
                        {
                            if (MapManager.inst.machines_garrisons.Count > 9) // 2 digits
                            {
                                mrs += "  " + MapManager.inst.machines_garrisons.Count + " Garrison Accesses\n";
                            }
                            else
                            {
                                mrs += "   " + MapManager.inst.machines_garrisons.Count + " Garrison Accesses\n";
                            }
                        }

                        return (mrs + "Downloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Recycling Units"))
                    {
                        GameManager.inst.IndexMachinesGeneric(3);
                        return ("Found " + MapManager.inst.imp_recyclingunits.Count + " recycling units.\nDownloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Repair Stations"))
                    {
                        GameManager.inst.IndexMachinesGeneric(4);
                        return ("Found " + MapManager.inst.machines_repairStation.Count + " repair stations.\nDownloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Scanalyzers"))
                    {
                        GameManager.inst.IndexMachinesGeneric(5);
                        return ("Found " + MapManager.inst.machines_scanalyzers.Count + " scanalyzers.\nDownloaded coordinate data.");
                    }
                    else if (parsedName.Contains("Terminals"))
                    {
                        GameManager.inst.IndexMachinesGeneric(6);
                        return ("Found " + MapManager.inst.machines_terminals.Count + " terminals.\nDownloaded coordinate data.");
                    }
                    break;
                case TerminalCommandType.Inventory:
                    break;
                case TerminalCommandType.Recall:
                    break;
                case TerminalCommandType.Traps:
                    if (parsedName.Contains("Disarm"))
                    {
                        List<FloorTrap> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            foreach (FloorTrap trap in traps)
                            {
                                trap.DeActivateTrap();
                            }
                            return $"Disarmed {traps.Count} nearby traps.";
                        }
                        else
                        {
                            return "No nearby traps found to disarm.";
                        }
                    }
                    else if (parsedName.Contains("Locate"))
                    {
                        List<FloorTrap> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            // Group the traps and count how many of each are available.
                            var groupedTraps = traps.GroupBy(f => f.fullName).ToDictionary(g => g.Key, g => g.Count());
                            string print = "";

                            // Print the merged version of the items.
                            foreach (var trap in groupedTraps)
                            {
                                string plural = trap.Value > 1 ? "s" : "";
                                print += $"Located {trap.Value} {trap.Key}{plural}.\n";
                            }
                            foreach (FloorTrap T in traps)
                            {
                                T.LocateTrap();
                            }

                            return HF.RemoveTrailingNewline(print);
                        }
                        else
                        {
                            return "No traps found in local area.";
                        }
                    }
                    else if (parsedName.Contains("Reprogram"))
                    {
                        List<FloorTrap> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            // Group the traps and count how many of each are available.
                            var groupedTraps = traps.GroupBy(f => f.fullName).ToDictionary(g => g.Key, g => g.Count());
                            string print = "";

                            // Print the merged version of the items.
                            foreach (var trap in groupedTraps)
                            {
                                string plural = trap.Value > 1 ? "s" : "";
                                print += $"Located {trap.Value} {trap.Key}{plural}.\n";
                            }
                            foreach (FloorTrap T in traps)
                            {
                                T.SetAlignment(BotAlignment.Player); // Delightfully devilish Seymour
                            }

                            return HF.RemoveTrailingNewline(print);
                        }
                        else
                        {
                            return "No traps found in local area.";
                        }
                    }
                    break;
                case TerminalCommandType.Manifests:
                    break;
                case TerminalCommandType.Open:
                    break;
                case TerminalCommandType.Prototypes:
                    break;
                case TerminalCommandType.Query: // This already happens in "lore"
                    break;
                case TerminalCommandType.Layout:
                    UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.RevealArea();
                    string print1 = "";

                    string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    string randomString = "";

                    for (int i = 0; i < 2; i++)
                    {
                        int randomIndex = Random.Range(0, characters.Length);
                        randomString += characters[randomIndex];
                    }

                    print1 = $"Retrieving Zone {randomString} layout...\nDownloaded map data.";

                    return print1;
                case TerminalCommandType.Analysis:
                    break;
                case TerminalCommandType.Schematic:
                    if (item != null)
                    {
                        command.item.knowByPlayer = true;

                        return "Downloading schematic...\n    " + HF.ExtractText(parsedName) + "\n    Rating: "
                            + item.rating + "\n    Schematic downloaded.";
                    }
                    else if (bot != null)
                    {
                        command.bot.schematicDetails.hasSchematic = true;

                        return "Downloading schematic...\n    " + HF.ExtractText(parsedName) + "\n    Rating: "
                            + bot.rating + "\n    Schematic downloaded.";
                    }
                    Debug.LogError("ERROR: No <Item> or <Bot> has been set for this command!");
                    return null;

                case TerminalCommandType.Download:
                    break;
                case TerminalCommandType.Couplers:
                    break;
                case TerminalCommandType.Seal:
                    // TODO
                    // We want to:
                    // -print the statement (see below)
                    // -play the closing sound
                    // -init the red consequences bar /w "GARRISON ACCESS SHUTDOWN"
                    // -forbid any further access to this garrison (via hacking)
                    UIManager.inst.Terminal_DoConsequences(UIManager.inst.highSecRed, "GARRISON ACCESS SHUTDOWN", false);
                    UIManager.inst.terminal_targetTerm.GetComponent<Garrison>().SealAccess();

                    return "Access door sealed.";
                case TerminalCommandType.Unlock:
                    break;
                case TerminalCommandType.LoadIndirect: // This opens up the schematic menu
                    if (!UIManager.inst.schematics_parent.activeInHierarchy)
                        UIManager.inst.Schematics_Open();

                    break;
                case TerminalCommandType.Load:

                    int matterCost = 0;
                    string name = "";
                    if (item != null)
                    {
                        matterCost = item.fabricationInfo.matterCost;
                        name = item.itemName;
                    }
                    else if (bot != null)
                    {
                        matterCost = bot.fabricationInfo.matterCost;
                        name = bot.name;
                    }

                    if (PlayerData.inst.currentMatter >= matterCost)
                    {
                        int buildTime = 0;
                        int secLvl = GetMachineSecLvl(UIManager.inst.terminal_targetTerm);

                        if (item != null)
                        {
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
                            string p = "";
                            if (item.star)
                            {
                                p = "p";
                            }

                            UIManager.inst.terminal_targetTerm.GetComponent<Fabricator>().Load(buildTime, item);
                            UIManager.inst.Schematics_Close();

                            return "Uploading " + name + " schematic...\nLoaded successfully:\n    " + name + "\n    Rating: "
                                + item.rating + p + "\n    Time: " + buildTime.ToString() + "\nInitiate build sequence.";
                        }
                        else if (bot != null)
                        {
                            if (secLvl == 1)
                            {
                                buildTime = bot.fabricationInfo.fabTime.x;
                            }
                            else if (secLvl == 2)
                            {
                                buildTime = bot.fabricationInfo.fabTime.y;
                            }
                            else if (secLvl == 3)
                            {
                                buildTime = bot.fabricationInfo.fabTime.z;
                            }
                            string p = "";
                            if (bot.star)
                            {
                                p = "p";
                            }

                            UIManager.inst.terminal_targetTerm.GetComponent<Fabricator>().Load(buildTime, null, bot);
                            UIManager.inst.Schematics_Close();

                            return "Uploading " + name + " schematic...\nLoaded successfully:\n    " + name + "\n    Rating: "
                                + bot.rating + p + "\n    Time: " + buildTime.ToString() + "\nInitiate build sequence.";
                        }
                    }
                    else
                    {
                        return "Not enough matter to consturct selected schematic. \n Missing " + (matterCost - PlayerData.inst.currentMatter) + " matter.";
                    }

                    Debug.LogError("ERROR: No <Item> or <Bot> has been set for this command!");
                    return null;
                case TerminalCommandType.Build:
                    string bName = "";
                    if (item != null)
                    {
                        bName = item.itemName;
                    }
                    else if (bot != null)
                    {
                        bName = bot.name;
                    }

                    UIManager.inst.terminal_targetTerm.GetComponent<Fabricator>().Build();

                    return "Building " + bName + "...\nETC: " + UIManager.inst.terminal_targetTerm.GetComponent<Fabricator>().buildTime;
                case TerminalCommandType.Network:
                    break;
                case TerminalCommandType.Refit:
                    break;
                case TerminalCommandType.Repair:
                    UIManager.inst.terminal_targetTerm.GetComponent<RepairStation>().Repair();

                    return "Repairing " + HF.ExtractText(parsedName) + "...\nETC: " + UIManager.inst.terminal_targetTerm.GetComponent<RepairStation>().timeToComplete;
                case TerminalCommandType.Scan:
                    int buildTime3 = 0;
                    int secLvl3 = GetMachineSecLvl(UIManager.inst.terminal_targetTerm);
                    if (secLvl3 == 1)
                    {
                        buildTime3 = item.fabricationInfo.fabTime.x;
                    }
                    else if (secLvl3 == 2)
                    {
                        buildTime3 = item.fabricationInfo.fabTime.y;
                    }
                    else if (secLvl3 == 3)
                    {
                        buildTime3 = item.fabricationInfo.fabTime.z;
                    }

                    string p2 = "";
                    if (item.star)
                    {
                        p2 = "p";
                    }

                    UIManager.inst.terminal_targetTerm.GetComponent<RepairStation>().Scan(command.item, buildTime3);

                    return "Scanning " + HF.ExtractText(parsedName) + "...\nReady to Repair:\n    " + HF.ExtractText(parsedName) + "\n    Rating: "
                        + item.rating + p2 + "\n    Time: " + buildTime3.ToString();
                case TerminalCommandType.Retrieve:
                    break;
                case TerminalCommandType.Recycle:
                    break;
                case TerminalCommandType.Insert:
                    break;
                case TerminalCommandType.Scanalyze:
                    break;
                case TerminalCommandType.NONE:
                    break;
                case TerminalCommandType.Trojan:
                    if (parsedName.Contains("Track"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nTracking enabled and active.";
                    }
                    else if (parsedName.Contains("Assimilate"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nSystem interface override in place.";
                    }
                    else if (parsedName.Contains("Botnet"))
                    {
                        // hack
                        string print = "Trojan loaded successfully.\nTesting...\nTerminal linked with " + PlayerData.inst.linkedTerminalBotnet + " systems.\nAwaiting botnet instructions.";
                        PlayerData.inst.linkedTerminalBotnet++;
                        GameObject target = UIManager.inst.terminal_targetTerm;
                        if (target.GetComponent<Terminal>())
                        {
                            target.GetComponent<Terminal>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<Fabricator>())
                        {
                            target.GetComponent<Fabricator>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<Scanalyzer>())
                        {
                            target.GetComponent<Scanalyzer>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<RepairStation>())
                        {
                            target.GetComponent<RepairStation>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<RecyclingUnit>())
                        {
                            target.GetComponent<RecyclingUnit>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<Garrison>())
                        {
                            target.GetComponent<Garrison>().trojans.Add(TrojanType.Botnet);
                        }
                        else if (target.GetComponent<TerminalCustom>())
                        {
                            target.GetComponent<TerminalCustom>().trojans.Add(TrojanType.Botnet);
                        }
                        return print;

                    }
                    else if (parsedName.Contains("Detonate"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Broadcast"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Decoy"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Redirect"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Reprogram"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Disrupt"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Fabnet"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Haulers"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Intercept"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Liberate"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Mask"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nMasking routine running.";
                    }
                    else if (parsedName.Contains("Mechanics"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Monitor"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Operators"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nOperator tracking enabled and active.";
                    }
                    else if (parsedName.Contains("Prioritize"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Recyclers"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Reject"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Report"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Researchers"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Restock"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Watchers"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }

                    break; // ---------- NOT DONE: TODO
                case TerminalCommandType.Force:
                    if (parsedName.Contains("Extract"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Jam"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Overload"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Download"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Eject"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Sabotage"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Search"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Tunnel"))
                    {
                        return "";
                    }
                    break;  // ---------- NOT DONE: TODO
            }
        }

        return result;
    }

    public static string ParseHackName(HackObject hack, string fill = "")
    {
        string result = "";

        result = hack.name;

        // Now parse it
        // Check for the presence of the "-" character
        int dashIndex = result.IndexOf('-');
        if (dashIndex != -1)
        {
            // Extract the substring before the dash
            result = result.Substring(0, dashIndex).Trim();
        }

        // Check for the presence of "[" and "]"
        int bracketStartIndex = result.IndexOf('[');
        int bracketEndIndex = result.IndexOf(']');
        if (bracketStartIndex != -1 && bracketEndIndex != -1 && bracketEndIndex > bracketStartIndex)
        {
            // Remove the brackets and their content
            result = result.Remove(bracketStartIndex, bracketEndIndex - bracketStartIndex + 1);
        }

        // Remove any left over "()"
        string[] split = result.Split("(");
        result = split[0];

        // Trim any leading or trailing spaces
        result = result.Trim();

        if (fill != "")
        {
            result = result + "(" + fill + ")";
        }

        return result;
    }

    public static string GenericHackFailure(float percent)
    {
        string result = "Failed " + ((int)(percent * 100)).ToString() + "% (";

        List<string> errors = new List<string>();

        if (percent >= 0.70) // Failed by not that much
        {
            errors.Add("near success, primary interm node flashed");
            errors.Add("near success, sudden abort of session control");
            errors.Add("near success, session timeout");
            errors.Add("near success, encountered trap node");
            errors.Add("near success, triggered node purge");
            errors.Add("near success, failed to establish connection");
            errors.Add("near success, receiving potentially harmful encrypted stream");
            errors.Add("near success, suspicious null response");
            errors.Add("near success, triggered system alarm");
            errors.Add("near success, unexpected hard line switch");
            errors.Add("near success, followed decoy data trail");
            errors.Add("near success, suspicious activity detected by network sentry");
            errors.Add("near success, encountered local connection sweep");
            errors.Add("near success, dynamic firewall re-routed connection");
        }
        else if (percent < 0.70 && percent >= 0.30) // Failed by a bit
        {
            errors.Add("primary interm node flashed");
            errors.Add("sudden abort of session control");
            errors.Add("session timeout");
            errors.Add("encountered trap node");
            errors.Add("triggered node purge");
            errors.Add("failed to establish connection");
            errors.Add("receiving potentially harmful encrypted stream");
            errors.Add("suspicious null response");
            errors.Add("triggered system alarm");
            errors.Add("unexpected hard line switch");
            errors.Add("followed decoy data trail");
            errors.Add("suspicious activity detected by network sentry");
            errors.Add("encountered local connection sweep");
            errors.Add("dynamic firewall re-routed connection");
        }
        else // oof
        {
            errors.Add("catastrophic failure, primary interm node flashed");
            errors.Add("catastrophic failure, sudden abort of session control");
            errors.Add("catastrophic failure, session timeout");
            errors.Add("catastrophic failure, encountered trap node");
            errors.Add("catastrophic failure, triggered node purge");
            errors.Add("catastrophic failure, failed to establish connection");
            errors.Add("catastrophic failure, receiving potentially harmful encrypted stream");
            errors.Add("catastrophic failure, suspicious null response");
            errors.Add("catastrophic failure, triggered system alarm");
            errors.Add("catastrophic failure, unexpected hard line switch");
            errors.Add("catastrophic failure, followed decoy data trail");
            errors.Add("catastrophic failure, suspicious activity detected by network sentry");
            errors.Add("catastrophic failure, encountered local connection sweep");
            errors.Add("catastrophic failure, dynamic firewall re-routed connection");
        }

        result += errors[Random.Range(0, errors.Count)];

        result += ").";
        return result;

    }

    /// <summary>
    /// When given a gameObject (machine) returns the 3 values related to tracing. Detection Chance, Trace Progress, and if the hack has been detected.
    /// </summary>
    /// <param name="machine">The machine gameObject to investigate</param>
    /// <returns>(float, float, bool) Detection Chance, Trace Progress, and if the hack has been detected.</returns>
    public static (float, float, bool) GetTraceValues(GameObject machine)
    {
        float detectionChance = 0f;
        float traceProgress = 0f;
        bool detected = false;

        if (machine.GetComponent<Terminal>()) // Open Terminal
        {
            detectionChance = machine.GetComponent<Terminal>().detectionChance;
            traceProgress = machine.GetComponent<Terminal>().traceProgress;
            detected = machine.GetComponent<Terminal>().detected;
        }
        else if (machine.GetComponent<Fabricator>()) // Open Fabricator
        {
            detectionChance = machine.GetComponent<Fabricator>().detectionChance;
            traceProgress = machine.GetComponent<Fabricator>().traceProgress;
            detected = machine.GetComponent<Fabricator>().detected;
        }
        else if (machine.GetComponent<Scanalyzer>()) // Open Scanalyzer
        {
            detectionChance = machine.GetComponent<Scanalyzer>().detectionChance;
            traceProgress = machine.GetComponent<Scanalyzer>().traceProgress;
            detected = machine.GetComponent<Scanalyzer>().detected;
        }
        else if (machine.GetComponent<RepairStation>()) // Open Repair Station
        {
            detectionChance = machine.GetComponent<RepairStation>().detectionChance;
            traceProgress = machine.GetComponent<RepairStation>().traceProgress;
            detected = machine.GetComponent<RepairStation>().detected;
        }
        else if (machine.GetComponent<RecyclingUnit>()) // Open Recycling Unit
        {
            detectionChance = machine.GetComponent<RecyclingUnit>().detectionChance;
            traceProgress = machine.GetComponent<RecyclingUnit>().traceProgress;
            detected = machine.GetComponent<RecyclingUnit>().detected;
        }
        else if (machine.GetComponent<Garrison>()) // Open Garrison
        {
            detectionChance = machine.GetComponent<Garrison>().detectionChance;
            traceProgress = machine.GetComponent<Garrison>().traceProgress;
            detected = machine.GetComponent<Garrison>().detected;
        }
        else if (machine.GetComponent<TerminalCustom>()) // Open Custom Terminal
        {
            detectionChance = machine.GetComponent<TerminalCustom>().detectionChance;
            traceProgress = machine.GetComponent<TerminalCustom>().traceProgress;
            detected = machine.GetComponent<TerminalCustom>().detected;
        }

        return (detectionChance, traceProgress, detected);
    }

    public static void TraceHacking(GameObject machine)
    {
        float detectionChance = 0f;
        float traceProgress = 0f;
        bool detected = false;

        (detectionChance, traceProgress, detected) = HF.GetTraceValues(machine);

        /*
        Detection: Initially your presence is unknown, but with each subsequent action there is a chance your activity will be detected. 
            Detection happens more quickly at higher security machines, and becomes more likely with each failed hack, but can be mitigated by defensive hackware. 
            Accessing the same machine multiple times also increases the chance of detection.
        Tracing: As soon as suspicious activity is detected, the system attempts to locate it. 
            Failing hacks increases the speed of the trace, more quickly for worse failures. 
            If a session is terminated while a trace is in progress, that trace will resume from where it left off if a connection is reestablished.
        Feedback: Once traced, the system is permanently locked and may attempt to counterattack the source of the unauthorized access, 
            which either causes system corruption or disables connected hackware.
         */

        int secLvl = HF.GetMachineSecLvl(machine);

        // Get any possible bonuses from system sheields
        List<float> bonuses = HF.SystemShieldBonuses();

        detectionChance -= bonuses[1];

        if (!detected)
        {
            // Do detection rolls
            if (Random.Range(0f, 1f) < detectionChance)
            {
                // Detected!
                detected = true;
                UIManager.inst.Terminal_InitTrace();
                AudioManager.inst.CreateTempClip(UIManager.inst.transform.position, AudioManager.inst.UI_Clips[39]);
            }
            else
            {
                // Potentially increase detection chance
                float increaseChance = 0.3f * secLvl; // probably wrong
                if (Random.Range(0f, 1f) < (increaseChance - bonuses[0]))
                {
                    // Increase detection chance
                    detectionChance += (increaseChance - bonuses[0]);
                }
            }

            foreach (var i in UIManager.inst.terminal_hackinfoList.ToList())
            {
                if (i.GetComponent<UIHackinfoV3>())
                {
                    i.GetComponent<UIHackinfoV3>().UpdateText(); // update the text
                }
            }
        }
        else
        {
            // Increase trace amount
            float old = traceProgress;
            traceProgress += detectionChance;

            foreach (var item in UIManager.inst.terminal_hackinfoList.ToList())
            {
                if (item.GetComponent<UITraceBar>())
                {
                    item.GetComponent<UITraceBar>().ExpandByPercent(traceProgress - old);
                }
            }
        }

        // Now update values
        detectionChance += bonuses[1]; // add it back so spam open/closing doesn't cheese the detecting

        if (machine.GetComponent<Terminal>()) // Open Terminal
        {
            machine.GetComponent<Terminal>().detectionChance = detectionChance;
            machine.GetComponent<Terminal>().traceProgress = traceProgress;
            machine.GetComponent<Terminal>().detected = detected;
        }
        else if (machine.GetComponent<Fabricator>()) // Open Fabricator
        {
            machine.GetComponent<Fabricator>().detectionChance = detectionChance;
            machine.GetComponent<Fabricator>().traceProgress = traceProgress;
            machine.GetComponent<Fabricator>().detected = detected;
        }
        else if (machine.GetComponent<Scanalyzer>()) // Open Scanalyzer
        {
            machine.GetComponent<Scanalyzer>().detectionChance = detectionChance;
            machine.GetComponent<Scanalyzer>().traceProgress = traceProgress;
            machine.GetComponent<Scanalyzer>().detected = detected;
        }
        else if (machine.GetComponent<RepairStation>()) // Open Repair Station
        {
            machine.GetComponent<RepairStation>().detectionChance = detectionChance;
            machine.GetComponent<RepairStation>().traceProgress = traceProgress;
            machine.GetComponent<RepairStation>().detected = detected;
        }
        else if (machine.GetComponent<RecyclingUnit>()) // Open Recycling Unit
        {
            machine.GetComponent<RecyclingUnit>().detectionChance = detectionChance;
            machine.GetComponent<RecyclingUnit>().traceProgress = traceProgress;
            machine.GetComponent<RecyclingUnit>().detected = detected;
        }
        else if (machine.GetComponent<Garrison>()) // Open Garrison
        {
            machine.GetComponent<Garrison>().detectionChance = detectionChance;
            machine.GetComponent<Garrison>().traceProgress = traceProgress;
            machine.GetComponent<Garrison>().detected = detected;
        }
        else if (machine.GetComponent<TerminalCustom>()) // Open Custom Terminal
        {
            machine.GetComponent<TerminalCustom>().detectionChance = detectionChance;
            machine.GetComponent<TerminalCustom>().traceProgress = traceProgress;
            machine.GetComponent<TerminalCustom>().detected = detected;
        }
    }

    /// <summary>
    /// A quick and clunky way of converting a tier and potential star to the relative hack command object.
    /// </summary>
    /// <param name="tier">The tier number of this part.</param>
    /// <param name="star">If this part is a * part.</param>
    /// <returns>The relevant HackObject to build for.</returns>
    public static HackObject HackBuildParser(int tier, bool star)
    {
        switch (tier)
        {
            case 1:
                return MapManager.inst.hackDatabase.Hack[0];
            case 2:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[1];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[2];
                }
            case 3:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[3];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[4];
                }
            case 4:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[5];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[6];
                }
            case 5:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[7];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[8];
                }
            case 6:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[9];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[10];
                }
            case 7:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[11];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[12];
                }
            case 8:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[13];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[14];
                }
            case 9:
                if (!star)
                {
                    return MapManager.inst.hackDatabase.Hack[15];
                }
                else
                {
                    return MapManager.inst.hackDatabase.Hack[16];
                }
            default:
                return MapManager.inst.hackDatabase.Hack[0];
        }
    }

    /// <summary>
    /// Determines if the specified machine is locked and unusuable. Returns true/false.
    /// </summary>
    /// <param name="machine">The machine gameObject to investigate.</param>
    /// <returns>True/False if the machine is locked.</returns>
    public static bool IsMachineLocked(GameObject machine)
    {
        if (machine.GetComponent<Terminal>()) // Terminal
        {
            return machine.GetComponent<Terminal>().locked;
        }
        else if (machine.GetComponent<Fabricator>()) // Fabricator
        {
            return machine.GetComponent<Fabricator>().locked;
        }
        else if (machine.GetComponent<Scanalyzer>()) // Scanalyzer
        {
            return machine.GetComponent<Scanalyzer>().locked;
        }
        else if (machine.GetComponent<RepairStation>()) // Repair Station
        {
            return machine.GetComponent<RepairStation>().locked;
        }
        else if (machine.GetComponent<RecyclingUnit>()) // Recycling Unit
        {
            return machine.GetComponent<RecyclingUnit>().locked;
        }
        else if (machine.GetComponent<Garrison>()) // Garrison
        {
            return machine.GetComponent<Garrison>().locked || machine.GetComponent<Garrison>().g_sealed;
        }
        else if (machine.GetComponent<TerminalCustom>()) // Custom Terminal
        {
            return machine.GetComponent<TerminalCustom>().locked;
        }

        return true;
    }

    #endregion

    #region Find & Get

    public static Part TryFindPartAtLocation(Vector2Int pos)
    {
        Vector3 lowerPosition = new Vector3(pos.x, pos.y, 2);
        Vector3 upperPosition = new Vector3(pos.x, pos.y, -2);
        Vector3 direction = lowerPosition - upperPosition;
        float distance = 4f;
        direction.Normalize();
        RaycastHit2D[] hits = Physics2D.RaycastAll(upperPosition, direction, distance);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<Part>())
            {
                return hit.collider.gameObject.GetComponent<Part>();
            }
        }

        return null;
    }

    public static List<GameObject> GetAllInteractableMachines()
    {
        List<GameObject> machines = new List<GameObject>();

        // Remember that we are returning the parent gameObject. The actual scripts are on the child of the parent.
        foreach (var M in MapManager.inst.machines_fabricators)
        {
            machines.Add(M);
        }
        foreach (var M in MapManager.inst.machines_garrisons)
        {
            machines.Add(M);
        }
        foreach (var M in MapManager.inst.machines_recyclingUnits)
        {
            machines.Add(M);
        }
        foreach (var M in MapManager.inst.machines_repairStation)
        {
            machines.Add(M);
        }
        foreach (var M in MapManager.inst.machines_scanalyzers)
        {
            machines.Add(M);
        }
        foreach (var M in MapManager.inst.machines_terminals)
        {
            machines.Add(M);
        }

        return machines;
    }

    public static int GetHeatTransfer(Item item)
    {
        int ht = 0;

        if (item.itemData.projectile.damage.x > 0)
        {
            ht = item.itemData.projectile.heatTrasfer;
        }

        foreach (var E in item.itemData.itemEffects)
        {
            if (E.heatTransfer > ht)
            {
                ht = E.heatTransfer;
            }
        }

        return ht;
    }

    /// <summary>
    /// Determines the relations between the "source" Actor and "target" actor. Returns the relation.
    /// </summary>
    /// <param name="source">The original actor to focus on.</param>
    /// <param name="target">The target being compared to.</param>
    /// <returns>A BotRelation.</returns>
    public static BotRelation DetermineRelation(Actor source, Actor target)
    {
        BotAlignment sourceAlignment = BotAlignment.None;
        BotAlignment targetAlignment = BotAlignment.None;
        bool isSourcePlayer = false;
        bool isTargetPlayer = false;

        if (source.botInfo)
        {
            sourceAlignment = source.botInfo.locations.alignment;
        }
        else
        {
            // This is the player
            isSourcePlayer = true;
        }

        if (target.botInfo)
        {
            targetAlignment = target.botInfo.locations.alignment;
        }
        else
        {
            // This is the player
            isTargetPlayer = true;
        }

        BotRelation relationToTarget = BotRelation.Neutral;

        if (isSourcePlayer) // Player vs Bot
        {
            foreach ((BotAlignment, BotRelation) T in source.allegances.alleganceTree)
            {
                if (T.Item1 == targetAlignment)
                {
                    relationToTarget = T.Item2;
                }
            }
        }
        else if (isTargetPlayer) // Bot vs Player
        {
            foreach ((BotAlignment, BotRelation) T in source.allegances.alleganceTree)
            {
                if (T.Item1 == BotAlignment.Player)
                {
                    relationToTarget = T.Item2;
                }
            }
        }
        else // Bot vs Bot
        {
            foreach ((BotAlignment, BotRelation) T in source.allegances.alleganceTree) // Source
            {
                if (T.Item1 == targetAlignment)
                {
                    relationToTarget = T.Item2;
                }
            }
        }

        return relationToTarget;
    }

    public static BotRelation RelationToTrap(Actor bot, FloorTrap trap)
    {
        BotRelation relationToTarget = BotRelation.Neutral;

        BotAlignment trapAlignment = trap.alignment;
        Allegance tree = bot.allegances;

        foreach ((BotAlignment, BotRelation) T in tree.alleganceTree)
        {
            if (T.Item1 == trapAlignment)
            {
                relationToTarget = T.Item2;
            }
        }

        return relationToTarget;
    }

    /// <summary>
    /// Attempts to find bots within a specified ranged of a specified bot. Returns a list of any found bots.
    /// </summary>
    /// <param name="source">The actor we are searching around. This actor is NOT included in returned list.</param>
    /// <param name="range">The radius around which we are searching the source bot.</param>
    /// <returns>A list of actors that are within range of the specified bot.</returns>
    public static List<Actor> FindBotsWithinRange(Actor source, int range)
    {
        List<Actor> bots = new List<Actor>();

        foreach (var bot in GameManager.inst.entities)
        {
            if (bot != source && Vector2.Distance(source.transform.position, bot.transform.position) <= range)
            {
                bots.Add(bot.GetComponent<Actor>());
            }
        }

        return bots;
    }

    public static Item DoesBotHaveSheild(Actor actor)
    {
        List<Item> items = Action.CollectAllBotItems(actor);

        foreach (var item in items)
        {
            if (item.Id >= 0)
            {
                foreach (var E in item.itemData.itemEffects)
                {
                    if (E.armorProtectionEffect.hasEffect)
                    {
                        if (E.armorProtectionEffect.projectionExchange)
                        {
                            return item;
                        }
                    }
                }
            }
        }

        return null;
    }

    public static List<TrojanType> GetMachineTrojans(GameObject target)
    {
        if (target.GetComponent<Terminal>())
        {
            return target.GetComponent<Terminal>().trojans;
        }
        else if (target.GetComponent<Fabricator>())
        {
            return target.GetComponent<Fabricator>().trojans;
        }
        else if (target.GetComponent<Scanalyzer>())
        {
            return target.GetComponent<Scanalyzer>().trojans;
        }
        else if (target.GetComponent<RepairStation>())
        {
            return target.GetComponent<RepairStation>().trojans;
        }
        else if (target.GetComponent<RecyclingUnit>())
        {
            return target.GetComponent<RecyclingUnit>().trojans;
        }
        else if (target.GetComponent<Garrison>())
        {
            return target.GetComponent<Garrison>().trojans;
        }
        else if (target.GetComponent<TerminalCustom>())
        {
            return target.GetComponent<TerminalCustom>().trojans;
        }

        return new List<TrojanType>();
    }

    public static ItemDamageType GetDamageType(ItemObject weapon)
    {
        if (weapon.meleeAttack.isMelee)
        {
            return weapon.meleeAttack.damageType;
        }
        else
        {
            return weapon.projectile.damageType;
        }
    }

    /// <summary>
    /// Determines if the actor in question can enter into siege mode, and if so, standard or high siege mode.
    /// </summary>
    /// <param name="actor">The actor in question.</param>
    /// <returns>The *value* of siege mode. 0 = Can't | 1 = Standard | 2 = High</returns>
    public static int DetermineSiegeType(Actor actor)
    {
        List<Item> items = Action.CollectAllBotItems(actor);

        int mode = 0;

        foreach (var item in items)
        {
            if (item.Id > -1 && item.itemData.type == ItemType.Treads)
            {
                if (mode == 2) // Stop early if we can reach High siege mode
                {
                    break;
                }

                mode = item.itemData.propulsion[0].canSiege;
            }
        }

        return mode;
    }

    /// <summary>
    /// Finds a random bot (in the database) of a specified tier (1-10).
    /// </summary>
    /// <param name="tier">The specified tier to search for. 1 to 10</param>
    /// <returns></returns>
    public static BotObject FindBotOfTier(int tier)
    {
        List<BotObject> bots = new List<BotObject>();

        foreach (BotObject bot in MapManager.inst.botDatabase.Bots)
        {
            if (bot.tier == tier)
            {
                bots.Add(bot);
            }
        }

        if (bots.Count > 0)
        {
            return bots[Random.Range(0, bots.Count - 1)];
        }

        return MapManager.inst.botDatabase.Bots[0]; // Failsafe
    }

    /// <summary>
    /// Finds a random item (in the database) of a specified tier (1-10).
    /// </summary>
    /// <param name="tier">The specified tier to search for. 1 to 10</param>
    /// <param name="isUnknown">Whether the item shouldn't be known to the player or not (aka a Prototype).</param>
    /// <returns></returns>
    public static ItemObject FindItemOfTier(int tier, bool isUnknown = true)
    {
        List<ItemObject> items = new List<ItemObject>();

        foreach (ItemObject item in MapManager.inst.itemDatabase.Items)
        {
            if (isUnknown)
            {
                if (item.rating == tier && !item.knowByPlayer) // Prototypes only
                {
                    items.Add(item);
                }
            }
            else
            {
                if (item.rating == tier)
                {
                    items.Add(item);
                }
            }
        }

        if (items.Count > 0)
        {
            return items[Random.Range(0, items.Count - 1)];
        }

        return MapManager.inst.itemDatabase.Items[0]; // Failsafe
    }

    /// <summary>
    /// Returns any possible bonuses if the player has a system shield equipped and enabled.
    /// </summary>
    /// <returns>Returns a list of 6 values (bonuses). Detection Rate Bonus, Detection Chance Bonus, Security Level Increase Bonus, Database lockout bonus, feedback prevention bonus, ally hack defense bonus.</returns>
    public static List<float> SystemShieldBonuses()
    {
        //[Tooltip("Reduces Chances of Detection while hacking machines")]
        float hackDetectRateBonus = 0f; // Reduces Chances of Detection
        //[Tooltip("Reduces Rate of Detection Chance increases while hacking machines")]
        float hackDetectChanceBonus = 0f;
        //[Tooltip("Lowers the chance that hacking machines will be considered serious enough to trigger an increase in security")]
        float securityLevelIncreaseChance = 0f;
        //[Tooltip("Reduces central database lockout chance")]
        float databaseLockoutBonus = 0f;
        //[Tooltip("Blocks hacking feedback side effects ##% of the time")]
        float hackFeedbackPreventBonus = 0f;
        //[Tooltip("Repels ##% of hacking attempts against allies within a range of ##")]
        float allyHackDefenseBonus = 0f;

        foreach (ItemObject item in MapManager.inst.itemDatabase.Items)
        {
            if (item.itemEffects.Count > 0)
            {
                foreach (var E in item.itemEffects)
                {
                    if (E.hackBonuses.hasSystemShieldBonus && item.data.state)
                    {
                        hackDetectRateBonus += E.hackBonuses.hackDetectRateBonus;
                        hackDetectChanceBonus += E.hackBonuses.hackDetectChanceBonus;
                        securityLevelIncreaseChance += E.hackBonuses.securityLevelIncreaseChance;
                        databaseLockoutBonus += E.hackBonuses.databaseLockoutBonus;
                        hackFeedbackPreventBonus += E.hackBonuses.hackFeedbackPreventBonus;
                        allyHackDefenseBonus += E.hackBonuses.allyHackDefenseBonus;
                    }
                }
            }
        }

        List<float> returns = new List<float>();
        returns.Add(hackDetectRateBonus);
        returns.Add(hackDetectChanceBonus);
        returns.Add(securityLevelIncreaseChance);
        returns.Add(databaseLockoutBonus);
        returns.Add(hackFeedbackPreventBonus);
        returns.Add(allyHackDefenseBonus);

        return returns;
    }

    public static (HackObject, TerminalCommand) ParseHackString(string str)
    {
        HackObject hack = null;
        TerminalCommand command = new TerminalCommand("x", "x", TerminalCommandType.NONE);

        if (str.Contains("("))
        {
            // command(inside)
            string left = HF.GetLeftSubstring(str); // command
            string right = HF.GetRightSubstring(str); // inside)
            //right = right.Substring(0, right.Length - 1); // Remove the ")", now: inside
            right = right.ToLower();

            command.command = left;
            command.secondaryText = right;

            // Now identify if an item/bot is involved
            ItemObject item = GetItemByString(right);
            BotObject bot = GetBotByString(right);

            command.bot = bot;
            command.item = item;

            /* The following hacks are in this category:
             * -Load, Network, Recycle(part,process,report), Retrieve(comp,mat), Scan, Insert, Access(b,e,m), Alert(purge,check), Analysis(bots)
             * [26]   [27]     [31,32,33]                    [34,35]            [54]   [55]    [73,74,75]     [77,76]             [77-87]
             * -Download(N,R,S), Enumerate, Index, Inventory, Layout, Recall, Schematic(Bot&Parts), Traps(D,L,R), Query, Trojan,         Force
             * [88,89,90]        [91-103]  [104-110][111,112] [113]  [116-119][120-128][129-145]   [146,147,148] [152]  [153-160,169-184] [161,168]
             */

            string c = left.ToLower();

            if (c == "load")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.Hack[26];
                }
                else
                {
                    Debug.LogError("ERROR: `item` is null! Cannot parse.");
                }
            }
            else if (c == "network")
            {
                hack = MapManager.inst.hackDatabase.Hack[27];
            }
            else if (c == "recycle")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.Hack[31];
                }
                else if (right.ToLower() == "process")
                {
                    hack = MapManager.inst.hackDatabase.Hack[32];
                }
                else if (right.ToLower() == "report")
                {
                    hack = MapManager.inst.hackDatabase.Hack[33];
                }
                else
                {
                    Debug.LogError("ERROR: Problem parsing Recycle(" + right.ToLower() + ") hack.");
                }
            }
            else if (c == "retrieve")
            {
                if (right.ToLower() == "components")
                {
                    hack = MapManager.inst.hackDatabase.Hack[34];
                }
                else if (right.ToLower() == "matter")
                {
                    hack = MapManager.inst.hackDatabase.Hack[35];
                }
            }
            else if (c == "scan")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.Hack[54];
                }
                else
                {
                    Debug.LogError("ERROR: `item` is null! Cannot parse.");
                }
            }
            else if (c == "insert")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.Hack[55];
                }
                else
                {
                    Debug.LogError("ERROR: `item` is null! Cannot parse.");
                }
            }
            else if (c == "access")
            {
                if (right.ToLower() == "branch")
                {
                    hack = MapManager.inst.hackDatabase.Hack[73];
                }
                else if (right.ToLower() == "emergency")
                {
                    hack = MapManager.inst.hackDatabase.Hack[74];
                }
                else if (right.ToLower() == "main")
                {
                    hack = MapManager.inst.hackDatabase.Hack[75];
                }
            }
            else if (c == "alert")
            {
                if (right.ToLower() == "check")
                {
                    hack = MapManager.inst.hackDatabase.Hack[76];
                }
                else if (right.ToLower() == "purge")
                {
                    hack = MapManager.inst.hackDatabase.Hack[77];
                }
            }
            else if (c == "analysis")
            {
                if (bot != null)
                {
                    int rating = bot.rating;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[78];
                            break;
                        case 2:
                            hack = MapManager.inst.hackDatabase.Hack[79];
                            break;
                        case 3:
                            hack = MapManager.inst.hackDatabase.Hack[80];
                            break;
                        case 4:
                            hack = MapManager.inst.hackDatabase.Hack[81];
                            break;
                        case 5:
                            hack = MapManager.inst.hackDatabase.Hack[82];
                            break;
                        case 6:
                            hack = MapManager.inst.hackDatabase.Hack[83];
                            break;
                        case 7:
                            hack = MapManager.inst.hackDatabase.Hack[84];
                            break;
                        case 8:
                            hack = MapManager.inst.hackDatabase.Hack[85];
                            break;
                        case 9:
                            hack = MapManager.inst.hackDatabase.Hack[86];
                            break;
                        case 10:
                            hack = MapManager.inst.hackDatabase.Hack[87];
                            break;
                    }
                }
                else
                {
                    Debug.LogError("ERROR: `bot` is null! Cannot parse.");
                }
            }
            else if (c == "download")
            {
                if (right.ToLower() == "navigation")
                {
                    hack = MapManager.inst.hackDatabase.Hack[88];
                }
                else if (right.ToLower() == "registry")
                {
                    hack = MapManager.inst.hackDatabase.Hack[89];
                }
                else if (right.ToLower() == "security")
                {
                    hack = MapManager.inst.hackDatabase.Hack[90];
                }
            }
            else if (c == "enumerate")
            {
                if (right.ToLower() == "assaults")
                {
                    hack = MapManager.inst.hackDatabase.Hack[91];
                }
                else if (right.ToLower() == "coupling")
                {
                    hack = MapManager.inst.hackDatabase.Hack[92];
                }
                else if (right.ToLower() == "exterminations")
                {
                    hack = MapManager.inst.hackDatabase.Hack[93];
                }
                else if (right.ToLower() == "garrison")
                {
                    hack = MapManager.inst.hackDatabase.Hack[94];
                }
                else if (right.ToLower() == "gaurds")
                {
                    hack = MapManager.inst.hackDatabase.Hack[95];
                }
                else if (right.ToLower() == "intercept")
                {
                    hack = MapManager.inst.hackDatabase.Hack[96];
                }
                else if (right.ToLower() == "investigations")
                {
                    hack = MapManager.inst.hackDatabase.Hack[97];
                }
                else if (right.ToLower() == "maintenance")
                {
                    hack = MapManager.inst.hackDatabase.Hack[98];
                }
                else if (right.ToLower() == "patrols")
                {
                    hack = MapManager.inst.hackDatabase.Hack[99];
                }
                else if (right.ToLower() == "reinforcements")
                {
                    hack = MapManager.inst.hackDatabase.Hack[100];
                }
                else if (right.ToLower() == "squads")
                {
                    hack = MapManager.inst.hackDatabase.Hack[101];
                }
                else if (right.ToLower() == "surveillance")
                {
                    hack = MapManager.inst.hackDatabase.Hack[102];
                }
                else if (right.ToLower() == "transport")
                {
                    hack = MapManager.inst.hackDatabase.Hack[103];
                }
            }
            else if (c == "index")
            {
                if (right.ToLower() == "fabricators")
                {
                    hack = MapManager.inst.hackDatabase.Hack[104];
                }
                else if (right.ToLower() == "garrisons")
                {
                    hack = MapManager.inst.hackDatabase.Hack[105];
                }
                else if (right.ToLower() == "machines")
                {
                    hack = MapManager.inst.hackDatabase.Hack[106];
                }
                else if (right.ToLower() == "recycling units")
                {
                    hack = MapManager.inst.hackDatabase.Hack[107];
                }
                else if (right.ToLower() == "repair stations")
                {
                    hack = MapManager.inst.hackDatabase.Hack[108];
                }
                else if (right.ToLower() == "scanalyzers")
                {
                    hack = MapManager.inst.hackDatabase.Hack[109];
                }
                else if (right.ToLower() == "terminals")
                {
                    hack = MapManager.inst.hackDatabase.Hack[110];
                }
            }
            else if (c == "inventory")
            {
                if (right.ToLower() == "component")
                {
                    hack = MapManager.inst.hackDatabase.Hack[111];
                }
                else if (right.ToLower() == "prototype")
                {
                    hack = MapManager.inst.hackDatabase.Hack[112];
                }
            }
            else if (c == "layout")
            {
                hack = MapManager.inst.hackDatabase.Hack[113];
            }
            else if (c == "recall")
            {
                if (right.ToLower() == "assault")
                {
                    hack = MapManager.inst.hackDatabase.Hack[116];
                }
                else if (right.ToLower() == "extermination")
                {
                    hack = MapManager.inst.hackDatabase.Hack[117];
                }
                else if (right.ToLower() == "investigation")
                {
                    hack = MapManager.inst.hackDatabase.Hack[118];
                }
                else if (right.ToLower() == "reinforcements")
                {
                    hack = MapManager.inst.hackDatabase.Hack[119];
                }
            }
            else if (c == "schematic")
            {
                if (bot != null)
                {
                    int rating = bot.rating;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[120];
                            break;
                        case 2:
                            hack = MapManager.inst.hackDatabase.Hack[121];
                            break;
                        case 3:
                            hack = MapManager.inst.hackDatabase.Hack[122];
                            break;
                        case 4:
                            hack = MapManager.inst.hackDatabase.Hack[123];
                            break;
                        case 5:
                            hack = MapManager.inst.hackDatabase.Hack[124];
                            break;
                        case 6:
                            hack = MapManager.inst.hackDatabase.Hack[125];
                            break;
                        case 7:
                            hack = MapManager.inst.hackDatabase.Hack[126];
                            break;
                        case 8:
                            hack = MapManager.inst.hackDatabase.Hack[127];
                            break;
                        case 9:
                            hack = MapManager.inst.hackDatabase.Hack[128];
                            break;
                    }

                }
                else if (item != null)
                {
                    int rating = item.rating;
                    bool starred = item.star;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[129];
                            break;
                        case 2:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[130];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[131];
                            }
                            break;
                        case 3:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[132];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[133];
                            }
                            break;
                        case 4:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[134];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[135];
                            }
                            break;
                        case 5:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[136];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[137];
                            }
                            break;
                        case 6:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[138];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[139];
                            }
                            break;
                        case 7:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[140];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[141];
                            }
                            break;
                        case 8:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[142];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[143];
                            }
                            break;
                        case 9:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[144];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[145];
                            }
                            break;
                    }
                }
                else
                {
                    Debug.LogError("ERROR: Both `bot` and `item` are null! Cannot parse.");
                }
            }
            else if (c == "traps")
            {
                if (right.ToLower() == "disarm")
                {
                    hack = MapManager.inst.hackDatabase.Hack[146];
                }
                else if (right.ToLower() == "locate")
                {
                    hack = MapManager.inst.hackDatabase.Hack[147];
                }
                else if (right.ToLower() == "reprogram")
                {
                    hack = MapManager.inst.hackDatabase.Hack[148];
                }
            }
            else if (c == "query")
            {
                hack = MapManager.inst.hackDatabase.Hack[152];
            }
            else if (c == "trojan")
            {
                if (right.ToLower() == "assimilate")
                {
                    hack = MapManager.inst.hackDatabase.Hack[153];
                }
                else if (right.ToLower() == "botnet")
                {
                    hack = MapManager.inst.hackDatabase.Hack[154];
                }
                else if (right.ToLower() == "broadcast")
                {
                    hack = MapManager.inst.hackDatabase.Hack[155];
                }
                else if (right.ToLower() == "decoy")
                {
                    hack = MapManager.inst.hackDatabase.Hack[156];
                }
                else if (right.ToLower() == "detonate")
                {
                    hack = MapManager.inst.hackDatabase.Hack[157];
                }
                else if (right.ToLower() == "redirect")
                {
                    hack = MapManager.inst.hackDatabase.Hack[158];
                }
                else if (right.ToLower() == "reprogram")
                {
                    hack = MapManager.inst.hackDatabase.Hack[159];
                }
                else if (right.ToLower() == "track")
                {
                    hack = MapManager.inst.hackDatabase.Hack[160];
                }
                else if (right.ToLower() == "disrupt")
                {
                    hack = MapManager.inst.hackDatabase.Hack[169];
                }
                else if (right.ToLower() == "fabnet")
                {
                    hack = MapManager.inst.hackDatabase.Hack[170];
                }
                else if (right.ToLower() == "haulers")
                {
                    hack = MapManager.inst.hackDatabase.Hack[171];
                }
                else if (right.ToLower() == "intercept")
                {
                    hack = MapManager.inst.hackDatabase.Hack[172];
                }
                else if (right.ToLower() == "mask")
                {
                    hack = MapManager.inst.hackDatabase.Hack[173];
                }
                else if (right.ToLower() == "mechanics")
                {
                    hack = MapManager.inst.hackDatabase.Hack[174];
                }
                else if (right.ToLower() == "monitor")
                {
                    hack = MapManager.inst.hackDatabase.Hack[175];
                }
                else if (right.ToLower() == "operators")
                {
                    hack = MapManager.inst.hackDatabase.Hack[176];
                }
                else if (right.ToLower() == "prioritize")
                {
                    hack = MapManager.inst.hackDatabase.Hack[177];
                }
                else if (right.ToLower() == "recyclers")
                {
                    hack = MapManager.inst.hackDatabase.Hack[178];
                }
                else if (right.ToLower() == "reject")
                {
                    hack = MapManager.inst.hackDatabase.Hack[179];
                }
                else if (right.ToLower() == "report")
                {
                    hack = MapManager.inst.hackDatabase.Hack[180];
                }
                else if (right.ToLower() == "researchers")
                {
                    hack = MapManager.inst.hackDatabase.Hack[181];
                }
                else if (right.ToLower() == "restock")
                {
                    hack = MapManager.inst.hackDatabase.Hack[182];
                }
                else if (right.ToLower() == "siphon")
                {
                    hack = MapManager.inst.hackDatabase.Hack[183];
                }
                else if (right.ToLower() == "watchers")
                {
                    hack = MapManager.inst.hackDatabase.Hack[184];
                }
            }
            else if (c == "force")
            {
                if (right.ToLower() == "extract")
                {
                    hack = MapManager.inst.hackDatabase.Hack[161];
                }
                else if (right.ToLower() == "jam")
                {
                    hack = MapManager.inst.hackDatabase.Hack[162];
                }
                else if (right.ToLower() == "overload")
                {
                    hack = MapManager.inst.hackDatabase.Hack[163];
                }
                else if (right.ToLower() == "download")
                {
                    hack = MapManager.inst.hackDatabase.Hack[164];
                }
                else if (right.ToLower() == "eject")
                {
                    hack = MapManager.inst.hackDatabase.Hack[165];
                }
                else if (right.ToLower() == "sabotage")
                {
                    hack = MapManager.inst.hackDatabase.Hack[166];
                }
                else if (right.ToLower() == "search")
                {
                    hack = MapManager.inst.hackDatabase.Hack[167];
                }
                else if (right.ToLower() == "tunnel")
                {
                    hack = MapManager.inst.hackDatabase.Hack[168];
                }
            }
        }
        else
        {
            /* The following hacks are in this category:
             * -Couplers, Seal, Unlock, Manifests, Prototypes, Open (h,m,l)
             * and
             * -Build (Fabricator), Refit (Repair Station), Repair (Repair Station), Scanalyze (Scanalyzer), 
             * [0-16]p----b[17-25], [36]                  , [37-53]                , [56-72]
             */

            // command(inside)
            string left = HF.GetLeftSubstring(str); // command
            string right = HF.GetRightSubstring(str); // inside)
            //right = right.Substring(0, right.Length - 1); // Remove the ")", now: inside
            right = right.ToLower();

            string c = left;

            if (c == "couplers")
            {
                hack = MapManager.inst.hackDatabase.Hack[28];
            }
            else if (c == "seal")
            {
                hack = MapManager.inst.hackDatabase.Hack[29];
            }
            else if (c == "unlock")
            {
                hack = MapManager.inst.hackDatabase.Hack[30];
            }
            else if (c == "manifests")
            {
                hack = MapManager.inst.hackDatabase.Hack[114];
            }
            else if (c == "prototypes")
            {
                hack = MapManager.inst.hackDatabase.Hack[115];
            }
            else if (c == "open")
            {
                hack = MapManager.inst.hackDatabase.Hack[149];
            }

            // Now part 2
            ItemObject item = GetItemByString(right);
            BotObject bot = GetBotByString(right);
            if (c == "build")
            {
                if (bot != null)
                {
                    int rating = bot.rating;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[17];
                            break;
                        case 2:
                            hack = MapManager.inst.hackDatabase.Hack[18];
                            break;
                        case 3:
                            hack = MapManager.inst.hackDatabase.Hack[19];
                            break;
                        case 4:
                            hack = MapManager.inst.hackDatabase.Hack[20];
                            break;
                        case 5:
                            hack = MapManager.inst.hackDatabase.Hack[21];
                            break;
                        case 6:
                            hack = MapManager.inst.hackDatabase.Hack[22];
                            break;
                        case 7:
                            hack = MapManager.inst.hackDatabase.Hack[23];
                            break;
                        case 8:
                            hack = MapManager.inst.hackDatabase.Hack[24];
                            break;
                        case 9:
                            hack = MapManager.inst.hackDatabase.Hack[25];
                            break;
                    }

                }
                else if (item != null)
                {
                    int rating = item.rating;
                    bool starred = item.star;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[0];
                            break;
                        case 2:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[1];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[2];
                            }
                            break;
                        case 3:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[3];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[4];
                            }
                            break;
                        case 4:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[5];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[6];
                            }
                            break;
                        case 5:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[7];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[8];
                            }
                            break;
                        case 6:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[9];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[10];
                            }
                            break;
                        case 7:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[11];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[12];
                            }
                            break;
                        case 8:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[13];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[14];
                            }
                            break;
                        case 9:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[15];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[16];
                            }
                            break;
                    }
                }
                else
                {
                    Debug.LogError("ERROR: Both `bot` and `item` are null! Cannot parse.");
                }
            }
            else if (c == "refit")
            {
                hack = MapManager.inst.hackDatabase.Hack[36];
            }
            else if (c == "repair")
            {
                if (item != null)
                {
                    int rating = item.rating;
                    bool starred = item.star;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[37];
                            break;
                        case 2:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[38];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[39];
                            }
                            break;
                        case 3:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[40];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[41];
                            }
                            break;
                        case 4:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[42];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[43];
                            }
                            break;
                        case 5:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[44];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[45];
                            }
                            break;
                        case 6:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[46];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[47];
                            }
                            break;
                        case 7:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[48];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[49];
                            }
                            break;
                        case 8:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[50];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[51];
                            }
                            break;
                        case 9:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[52];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[53];
                            }
                            break;
                    }
                }
                else
                {
                    Debug.LogError("ERROR: `item` is null! Cannot parse.");
                }
            }
            else if (c == "scanalyze")
            {
                if (item != null)
                {
                    int rating = item.rating;
                    bool starred = item.star;
                    switch (rating)
                    {
                        case 1:
                            hack = MapManager.inst.hackDatabase.Hack[56];
                            break;
                        case 2:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[57];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[58];
                            }
                            break;
                        case 3:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[59];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[60];
                            }
                            break;
                        case 4:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[61];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[62];
                            }
                            break;
                        case 5:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[63];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[64];
                            }
                            break;
                        case 6:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[65];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[66];
                            }
                            break;
                        case 7:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[67];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[68];
                            }
                            break;
                        case 8:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[69];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[70];
                            }
                            break;
                        case 9:
                            if (!starred)
                            {
                                hack = MapManager.inst.hackDatabase.Hack[71];
                            }
                            else
                            {
                                hack = MapManager.inst.hackDatabase.Hack[72];
                            }
                            break;
                    }
                }
                else
                {
                    Debug.LogError("ERROR: `item` is null! Cannot parse.");
                }
            }

            command.item = item;
            command.bot = bot;
        }

        command.hack = hack;

        return (hack, command);
    }

    public static ItemType GetItemTypeByString(string str)
    {

        switch (str)
        {
            case "Default":
                return ItemType.Default;
            case "Engine":
                return ItemType.Engine;
            case "PowerCore":
                return ItemType.PowerCore;
            case "Reactor":
                return ItemType.Reactor;
            case "Treads":
                return ItemType.Treads;
            case "Legs":
                return ItemType.Legs;
            case "Wheels":
                return ItemType.Wheels;
            case "Hover":
                return ItemType.Hover;
            case "Flight":
                return ItemType.Flight;
            case "Storage":
                return ItemType.Storage;
            case "Processor":
                return ItemType.Processor;
            case "Hackware":
                return ItemType.Hackware;
            case "Device":
                return ItemType.Device;
            case "Armor":
                return ItemType.Armor;
            case "Alien":
                return ItemType.Alien;
            case "Gun":
                return ItemType.Gun;
            case "EnergyGun":
                return ItemType.EnergyGun;
            case "Cannon":
                return ItemType.Cannon;
            case "EnergyCannon":
                return ItemType.EnergyCannon;
            case "Launcher":
                return ItemType.Launcher;
            case "Impact":
                return ItemType.Impact;
            case "Special":
                return ItemType.Special;
            case "Melee":
                return ItemType.Melee;
            case "Data":
                return ItemType.Data;
            case "Nonpart":
                return ItemType.Nonpart;
            case "Trap":
                return ItemType.Trap;
            default:
                return ItemType.Default;
        }
    }

    public static ItemObject GetItemByString(string str)
    {
        ItemObject item = null;

        string target = str.ToLower();

        foreach (var I in MapManager.inst.itemDatabase.Items)
        {
            string name = I.itemName.ToLower();
            if (name == target || name == target.ToLower())
            {
                item = I;
                break;
            }
        }

        return item;
    }

    public static TileObject GetTileByString(string str)
    {
        TileObject tile = null;

        string target = str.ToLower();

        foreach (var I in MapManager.inst.tileDatabase.Tiles)
        {
            string name = I.name.ToLower();

            if (name == target || name == target.ToLower())
            {
                tile = I;
                break;
            }
        }

        return tile;
    }

    public static BotObject GetBotByString(string str)
    {
        BotObject bot = null;

        string target = str.ToLower();

        foreach (var B in MapManager.inst.botDatabase.Bots)
        {
            string name = B.name.ToLower();
            if (name == target || name == target.ToLower())
            {
                bot = B;
                break;
            }
        }

        return bot;
    }

    /// <summary>
    /// Checks a generic object which is presumed to be a tile and returns its visibility.
    /// </summary>
    /// <param name="target">The target gameobject to check.</param>
    /// <returns>1. Is this target explored? 2. Is this target currently visible to the player? (Both bools)</returns>
    public static (bool, bool) GetGenericTileVis(GameObject target)
    {
        // - Return values -
        bool _e = false;
        bool _v = false;
        // -               -

        if (target.GetComponent<TileBlock>()) // Walls, Floors, & Door tiles
        {
            _e = target.GetComponent<TileBlock>().isExplored;
            _v = target.GetComponent<TileBlock>().isVisible;
        }
        else if (target.GetComponentInChildren<TileBlock>())
        {
            _e = target.GetComponentInChildren<TileBlock>().isExplored;
            _v = target.GetComponentInChildren<TileBlock>().isVisible;
        }

        if (target.GetComponent<MachinePart>()) // Machines
        {
            _e = target.GetComponent<MachinePart>().isExplored;
            _v = target.GetComponent<MachinePart>().isVisible;
        }
        else if (target.GetComponentInChildren<MachinePart>())
        {
            _e = target.GetComponentInChildren<MachinePart>().isExplored;
            _v = target.GetComponentInChildren<MachinePart>().isVisible;
        }

        // Bots
        if (target.GetComponent<Actor>() && target != PlayerData.inst.gameObject)
        {
            _e = target.GetComponent<Actor>().isExplored;
            _v = target.GetComponent<Actor>().isVisible;
        }
        else if (target.GetComponentInChildren<Actor>() && target != PlayerData.inst.gameObject)
        {
            _e = target.GetComponentInChildren<Actor>().isExplored;
            _v = target.GetComponentInChildren<Actor>().isVisible;
        }

        // Access
        if (target.GetComponent<AccessObject>())
        {
            _e = target.GetComponent<AccessObject>().isExplored;
            _v = target.GetComponent<AccessObject>().isVisible;
        }
        else if (target.GetComponentInChildren<AccessObject>())
        {
            _e = target.GetComponentInChildren<AccessObject>().isExplored;
            _v = target.GetComponentInChildren<AccessObject>().isVisible;
        }

        // Items
        if (target.GetComponent<Part>())
        {
            _e = target.GetComponent<Part>().isExplored;
            _v = target.GetComponent<Part>().isVisible;
        }
        else if (target.GetComponentInChildren<Part>())
        {
            _e = target.GetComponentInChildren<Part>().isExplored;
            _v = target.GetComponentInChildren<Part>().isVisible;
        }

        return (_e, _v);

    }

    /// <summary>
    /// Finds *VALID* neighbors given a current position on a grid.
    /// </summary>
    /// <param name="X">Current X position on the grid.</param>
    /// <param name="Y">Current Y position on the grid.</param>
    /// <returns>Returns a list of *VALID* neighbors that exist.</returns>
    public static List<GameObject> FindNeighbors(int X, int Y)
    {
        // --
        // Copied from "Astar.cs"
        // --

        // NOTE: I hate GridManager, the array sucks. We are going to use _allTilesRealized instead.

        List<GameObject> neighbors = new List<GameObject>();

        // We want to include diagonals into this.
        if (X < MapManager.inst._mapSizeX - 1) // [ RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y)].gameObject);
        }
        if (X > 0) // [ LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y)].gameObject);
        }
        if (Y < MapManager.inst._mapSizeY - 1) // [ UP ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X, Y + 1)].gameObject);
        }
        if (Y > 0) // [ DOWN ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X, Y - 1)].gameObject);
        }
        // -- 
        // Diagonals
        // --
        if (X < MapManager.inst._mapSizeX - 1 && Y < MapManager.inst._mapSizeY - 1) // [ UP-RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y + 1)].gameObject);
        }
        if (Y < MapManager.inst._mapSizeY - 1 && X > 0) // [ UP-LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y + 1)].gameObject);
        }
        if (Y > 0 && X > 0) // [ DOWN-LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y - 1)].gameObject);
        }
        if (Y > 0 && X < MapManager.inst._mapSizeX - 1) // [ DOWN-RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y - 1)].gameObject);
        }

        return neighbors;

    }

    /// <summary>
    /// Finds the assigned letter for an item on the UI Parts display.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The letter assigned to that item.</returns>
    public static string FindItemUILetter(Item item)
    {
        // Roundabout way of doing this since i'm not sure how to access the list directly
        foreach (Transform child in UIManager.inst.partContentArea.transform)
        {
            if (child.GetComponent<InvDisplayItem>() && child.GetComponent<InvDisplayItem>().item != null)
            {
                if (child.GetComponent<InvDisplayItem>().item == item)
                {
                    return child.GetComponent<InvDisplayItem>()._assignedChar.ToString();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to retrieve a structures armor value. This structure could be a machine, a door, a tile, etc.
    /// </summary>
    /// <param name="structure">The structure gameObject to try to retrieve the armor value of.</param>
    /// <returns>The INT armor value of the specified structure.</returns>
    public static int TryGetStructureArmor(GameObject structure)
    {
        int armor = 0;

        if (structure)
        {
            if (structure.GetComponent<MachinePart>())
            {
                armor = structure.GetComponent<MachinePart>().armor.y;
            }
            else if (structure.GetComponent<TileBlock>())
            {
                armor = structure.GetComponent<TileBlock>().tileInfo.armor;
            }
        }

        return armor;
    }

    public static string GetNextLetter(string lastLetter)
    {
        // Convert the last letter to a character
        char lastChar = lastLetter[0];

        // Increment the ASCII value of the last letter to get the next letter
        // If the last letter is 'z', wrap around to 'a'
        char nextChar = (char)(lastChar == 'z' ? 'a' : lastChar + 1);

        // Convert the next character back to a string and return it
        return nextChar.ToString();
    }

    public static List<ItemObject> GetKnownItemSchematics()
    {
        List<ItemObject> items = new List<ItemObject>();

        foreach (var item in MapManager.inst.itemDatabase.Items)
        {
            if (item.schematicDetails.hasSchematic)
            {
                items.Add(item);
            }
        }

        return items;
    }

    public static List<BotObject> GetKnownBotSchematics()
    {
        List<BotObject> bots = new List<BotObject>();

        foreach (var bot in MapManager.inst.botDatabase.Bots)
        {
            if (bot.schematicDetails.hasSchematic)
            {
                bots.Add(bot);
            }
        }

        return bots;
    }

    public static (int, bool) GetTierAndP(string gameObjectName)
    {
        int tier = 0;
        bool hasP = false;

        // Define the regular expression pattern to match "Tier #" or "Rating #P" in the GameObject name.
        string pattern = @"(?:Tier|Rating)\s+(\d+)(P?)";

        // Use Regex to find matches in the GameObject name.
        Match match = Regex.Match(gameObjectName, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            // Extract the tier/rating number and check for "P".
            tier = int.Parse(match.Groups[1].Value);
            hasP = !string.IsNullOrEmpty(match.Groups[2].Value);
        }

        return (tier, hasP);
    }

    /// <summary>
    /// Will attempt to locate the nearest free space to the specified location. Usually used for item placement on the floor.
    /// </summary>
    /// <param name="center">The start location to perform the search.</param>
    /// <returns>A free position (Vector2Int) that has been found.</returns>
    public static Vector2Int LocateFreeSpace(Vector2Int center)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(center);

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            if (IsOpenSpace(currentPos))
            {
                return currentPos;
            }

            // Add adjacent tiles to the queue if they haven't been visited yet
            foreach (Vector2Int adjacentPos in GetAdjacentPositions(currentPos))
            {
                if (!visited.Contains(adjacentPos))
                {
                    visited.Add(adjacentPos);
                    queue.Enqueue(adjacentPos);
                }
            }
        }

        // If no open space is found, return the start position
        return center;
    }

    public static bool IsOpenSpace(Vector2Int position)
    {
        if (MapManager.inst._allTilesRealized.TryGetValue(position, out TileBlock tileBlock))
        {
            return tileBlock._partOnTop == null && !tileBlock.occupied;
        }

        return false;
    }

    public static List<Vector2Int> GetAdjacentPositions(Vector2Int position)
    {
        List<Vector2Int> adjacentPositions = new List<Vector2Int>();
        adjacentPositions.Add(new Vector2Int(position.x + 1, position.y));
        adjacentPositions.Add(new Vector2Int(position.x - 1, position.y));
        adjacentPositions.Add(new Vector2Int(position.x, position.y + 1));
        adjacentPositions.Add(new Vector2Int(position.x, position.y - 1));
        return adjacentPositions;
    }

    public static InventoryObject FindPlayerInventoryFromItem(Item item)
    {
        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>()._invPower.Container.Items.ToList())
        {
            if (I.item.itemData.data.Id >= 0)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>()._invPower;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.Container.Items.ToList())
        {
            if (I.item.itemData.data.Id >= 0)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>()._invPropulsion;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>()._invUtility.Container.Items.ToList())
        {
            if (I.item.itemData.data.Id >= 0)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>()._invUtility;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>()._invWeapon.Container.Items.ToList())
        {
            if (I.item.itemData.data.Id >= 0)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>()._invWeapon;
                }
            }
        }

        return null;
    }

    public static int GetSalvageMod(Item item)
    {
        int salvage = 0;

        // We can find this salvage mod buried in various things:
        // - Melee attacks
        // - Projectile
        // - Explosions

        if (item.itemData.meleeAttack.isMelee)
        {
            salvage = item.itemData.meleeAttack.salvage;
        }
        else if (item.itemData.projectile.damage.x > 0)
        {
            salvage = item.itemData.projectile.salvage;
        }
        else if (item.itemData.explosionDetails.isGeneral || item.itemData.explosionDetails.isDeployable || item.itemData.explosionDetails.isEffect)
        {
            salvage = item.itemData.explosionDetails.salvage;
        }

        return salvage;
    }

    /// <summary>
    /// Attempts to find the parent part of a machine at the specified machine.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    /// <returns>If one is found, returns the gameObject of the machine parent located near the position.</returns>
    public static GameObject GetMachineParentAtPosition(Vector2Int pos)
    {
        Vector3 lowerPosition = new Vector3(pos.x, pos.y, 2);
        Vector3 upperPosition = new Vector3(pos.x, pos.y, -2);
        Vector3 direction = lowerPosition - upperPosition;
        float distance = Vector3.Distance(new Vector3Int((int)lowerPosition.x, (int)lowerPosition.y, 0), upperPosition);
        direction.Normalize();
        RaycastHit2D[] hits = Physics2D.RaycastAll(upperPosition, direction, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];

            if (hit.collider.GetComponent<MachinePart>())
            {
                return hit.collider.GetComponent<MachinePart>().parentPart.gameObject;
            }
        }

        return null;
    }

    public static Sprite GetBlackAndWhiteSprite(Sprite coloredSprite)
    {
        if (coloredSprite == null)
        {
            return null;
        }

        // Get the name of the colored sprite
        string coloredSpriteName = coloredSprite.name;

        // Append "_BW" to the name to get the name of the black and white sprite
        string blackAndWhiteSpriteName = coloredSpriteName + "_BW";
        // Load the black and white sprite from the "BW" folder
        Sprite blackAndWhiteSprite = Resources.Load<Sprite>("Textures/Sprites/Part Art/BW/" + blackAndWhiteSpriteName);

        return blackAndWhiteSprite;
    }

    public static float FindExposureInBotObject(List<BotArmament> inventory, ItemObject item)
    {
        foreach (var I in inventory)
        {
            if (I.item.itemData == item)
            {
                return I.dropChance;
            }
        }

        return 0f;
    }

    #endregion

    #region Floor Traps
    public static void AttemptTriggerTrap(FloorTrap trap, GameObject target)
    {
        /*
         Triggering
            Moving over a trap does not always trigger it. The chance is instead determined by the form of propulsion:

           <Mode>	   <Probability>
            Treads	    100%
            Legs	    75%
            Wheels	    50%
            Hover/Core	40%
            Flight	    20%

            Once triggered, the trap at that position is spent and will not trigger again. (Some traps exhibit special behavior in this regard.)
            Note there are also multiple undocumented circumstances under which a trap may be triggered,
            not simply regular movement. 
            Also, Stasis Traps are a special case and their chance to trigger is the opposite of the values listed above (Flight ex: 100 - 20 = 80%).
         */

        float triggerChance = 0f;

        if (Action.HasTreads(target.GetComponent<Actor>()))
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 1f;
            }
            else
            {
                triggerChance = 1f;
            }
        }
        else if (Action.HasLegs(target.GetComponent<Actor>()))
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.75f;
            }
            else
            {
                triggerChance = 0.75f;
            }
        }
        else if (Action.HasWheels(target.GetComponent<Actor>()))
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.5f;
            }
            else
            {
                triggerChance = 0.5f;
            }
        }
        else if (Action.HasFlight(target.GetComponent<Actor>()))
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.2f;
            }
            else
            {
                triggerChance = 0.2f;
            }
        }
        else if (Action.HasHover(target.GetComponent<Actor>()))
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.4f;
            }
            else
            {
                triggerChance = 0.4f;
            }
        }
        else // Same as hover
        {
            if (trap.type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.4f;
            }
            else
            {
                triggerChance = 0.4f;
            }
        }

        // Now do the roll
        if (Random.Range(0f, 1f) < triggerChance) // Hit!
        {
            trap.TripTrap(target);
        }
        else // Safe!
        {
            return;
        }
    }

    public static void AttemptLocateTrap(List<Vector3Int> FOV)
    {
        /*
         Detection
            Cogmind has a 1% chance to detect each trap in view, checked every turn, where each active Structural Scanner provides a +2% bonus. 
            Hack terminals to reveal entire trap arrays at once. Nearby allied operators will also reveal hidden traps within your field of vision, 
            and local trap locations can also be extracted from a pre-expired operator data core.
         */

        float detChance = 0.01f;

        // Check for structural scanners
        foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>()._invWeapon.Container.Items)
        {
            if (item.item.Id >= 0)
            {
                if (item.item.itemData.itemEffects.Count > 0)
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (E.detectionEffect.structural)
                        {
                            detChance += 0.02f;
                            break;
                        }
                    }
                }
            }
        }

        // Now that we have the full bonus, go through the player's FOV and check for any mines (that the player can't see)
        List<FloorTrap> trapsInView = new List<FloorTrap>();

        foreach (Vector3Int spot in FOV)
        {
            Vector2Int loc = new Vector2Int(spot.x, spot.y);

            if (MapManager.inst._layeredObjsRealized.ContainsKey(loc) && MapManager.inst._layeredObjsRealized[loc].GetComponent<FloorTrap>())
            {
                if (!MapManager.inst._layeredObjsRealized[loc].GetComponent<FloorTrap>().knowByPlayer
                    && !MapManager.inst._layeredObjsRealized[loc].GetComponent<FloorTrap>().tripped
                    && MapManager.inst._layeredObjsRealized[loc].GetComponent<FloorTrap>().active)
                {
                    trapsInView.Add(MapManager.inst._layeredObjsRealized[loc].GetComponent<FloorTrap>());
                }
            }
        }

        // Now go through each trap and determine if it should be located
        foreach (FloorTrap trap in trapsInView)
        {
            if (Random.Range(0f, 1f) < detChance)
            {
                // Detected!
                trap.LocateTrap();
            }
        }
    }

    #endregion

    #region String/Name Manipulation

    /// <summary>
    /// Given an input string, will output a list of strings that when quickly swapped out will show an "animation" of one letter at a time being highlighted,
    /// changing from the start color to the end color of the text.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <param name="highlight">The highlight color.</param>
    /// <param name="start">The color the text starts as.</param>
    /// <param name="end">The color the text ends as.</param>
    /// <returns>A list of strings that when changed together looks like an animated highlight.</returns>
    public static List<string> SteppedStringHighlightAnimation(string text, Color highlight, Color start, Color end)
    {
        List<string> output = new List<string>();

        // We do this using:                                                   (fun fact, adding: aa to the end of mark makes it transparent)
        // = $"<mark=#{ColorUtility.ToHtmlStringRGB(Color.black)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"["}</color></mark>";

        // Starting state:
        output.Add($"<color=#{ColorUtility.ToHtmlStringRGB(start)}>{text}</color>");

        // Transition states:
        for (int i = 0; i < text.Length; i++)
        {
            // Split the string
            string left, middle, right;
            HF.SplitString(text, i, out left, out middle, out right);

            string result = "";

            // Add components with color & highlight
            if(left != "")
                result += $"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{left}</color>";

            result += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlight)}aa><color=#{ColorUtility.ToHtmlStringRGB(end)}>{middle}</color></mark>";

            if (right != "")
                result += $"<color=#{ColorUtility.ToHtmlStringRGB(start)}>{right}</color>";

            // Add result to list
            output.Add(result);
        }

        // End state:
        output.Add($"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{text}</color>");

        return output;
    }

    /// <summary>
    /// Given a single input string, will split it up given a position value within the string. Ex: (apple, 1) -> (a, p, ple)
    /// </summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="position">The position within the string to select as our "split point".</param>
    /// <param name="left">All the characters to the left of the split point.</param>
    /// <param name="middle">The single character at the split point.</param>
    /// <param name="right">All the characters to the right of the split point.</param>
    public static void SplitString(string input, int position, out string left, out string middle, out string right)
    {
        // Check if the position is within the bounds of the string
        if (position < 0 || position >= input.Length)
        {
            // If the position is out of bounds, set all output strings to empty
            left = "";
            middle = "";
            right = "";
            return;
        }

        // Get the substring to the left of the character at the given position
        left = input.Substring(0, position);

        // Get the character at the given position
        middle = input.Substring(position, 1);

        // Get the substring to the right of the character at the given position
        right = input.Substring(position + 1);
    }

    /// <summary>
    /// Extracts text from the inside of (   ). Example Name(This)
    /// </summary>
    /// <param name="input">A string in the format of "Name(This)".</param>
    /// <returns></returns>
    public static string ExtractText(string input)
    {
        int bracketStartIndex = input.IndexOf('(');
        int bracketEndIndex = input.IndexOf(')');
        if (bracketStartIndex != -1 && bracketEndIndex != -1 && bracketEndIndex > bracketStartIndex)
        {
            return input.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1).Trim();
        }

        return string.Empty;
    }
    public static int StringToInt(string input)
    {
        // Parse the string to an integer
        int result = int.Parse(input);
        return result;
    }

    /// <summary>
    /// Access a string in the form of word(word), returns anything to the left of the "(".
    /// </summary>
    /// <param name="input">A string that contains "(", usually in the form of word(word)</param>
    /// <returns>Returns the characters to the left of the "(" character.</returns>
    public static string GetLeftSubstring(string input)
    {
        int index = input.IndexOf("(");
        if (index != -1)
        {
            return input.Substring(0, index);
        }

        return input;
    }

    /// <summary>
    /// Access a string in the form of word(word), returns anything to the right of the "(".
    /// </summary>
    /// <param name="input">A string that contains "(", usually in the form of word(word)</param>
    /// <returns>Returns the characters to the right of the "(" character.</returns>
    public static string GetRightSubstring(string input)
    {
        int index = input.IndexOf("(");
        if (index != -1)
        {
            return input.Substring(index + 1, input.Length - index - 2);
        }

        return string.Empty;
    }

    public static string RemoveTrailingNewline(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            int length = input.Length;
            while (length > 0 && input[length - 1] == '\n')
            {
                length--;
            }

            return input.Substring(0, length);
        }

        return input;
    }

    /// <summary>
    /// Renames a gameObject if a specific word is present. Removes that word.
    /// </summary>
    /// <param name="gameObject">The gameObject to target.</param>
    /// <param name="wordToRemove">The word to look for and remove.</param>
    public static void RemoveWordFromName(GameObject gameObject, string wordToRemove)
    {
        if (gameObject != null && !string.IsNullOrEmpty(wordToRemove))
        {
            if (gameObject.name.Contains(wordToRemove))
            {
                string newName = gameObject.name.Replace(wordToRemove, "");
                gameObject.name = newName;
            }
        }
    }

    /// <summary>
    /// Removes trailing (#) from gameObject's names and returns a new "cleaned" name.
    /// </summary>
    /// <param name="originalName">The original name of the gameObject</param>
    /// <returns>The new name of the gameObject.</returns>
    public static string RemoveTrailingNums(string originalName)
    {
        // Find the index of the opening parenthesis
        int openParenIndex = originalName.IndexOf("(");

        // Check if the opening parenthesis is found and it in the latter half of the name
        if (openParenIndex != -1 && (openParenIndex > originalName.Length / 2))
        {
            // Trim the string to remove trailing characters
            string cleanedName = originalName.Substring(0, openParenIndex).Trim();

            return cleanedName;
        }

        // Return the original name if no trailing characters are found
        return originalName;
    }

    // UNUSED (doesnt work)
    public static string GenerateMarkedString(string input)
    {
        // Split the input string into words
        string[] words = input.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Initialize a StringBuilder to construct the marked string
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Add <mark> tags around each word and append them to the StringBuilder
        foreach (string word in words)
        {
            sb.Append($"<mark=#000000>{word}</mark>");
        }

        // Return the final marked string
        return sb.ToString();
    }

    public static string GetLastCharOfString(string input)
    {
        if (input.Length == 0)
        {
            return "";
        }
        else if (input.Length == 1)
        {
            return input;
        }
        else
        {
            return input[input.Length - 1].ToString();
        }
    }

    public static IEnumerator DelayedSetText(TextMeshProUGUI UI, string text, float delay)
    {
        yield return new WaitForSeconds(delay);

        UI.text = text;
    }

    public static string BotClassParse(BotClass bClass)
    {
        string s = bClass.ToString();
        s = s.Replace("_", " ");

        return s;
    }

    public static (string differenceString, int differenceValue) GetDifferenceAsStringAndValue(object value1, object value2)
    {
        float num1, num2;

        // Check if the values are float percentages
        bool isPercentage = (value1 is float && value2 is float);

        if (isPercentage)
        {
            // Convert the float percentages to standard float values
            num1 = (float)value1 * 100;
            num2 = (float)value2 * 100;
        }
        else
        {
            // Convert values to floats (assuming they are numeric types)
            num1 = System.Convert.ToSingle(value1);
            num2 = System.Convert.ToSingle(value2);
        }

        // Compute the difference
        float difference = num2 - num1;

        // Convert the difference to an integer
        int differenceInt = Mathf.RoundToInt(difference);

        // Format the difference as a string
        string differenceString = differenceInt.ToString();

        // Add a '+' sign if the difference is positive
        if (differenceInt > 0)
        {
            differenceString = "+" + differenceString;
        }

        // Add a "%" sign if the values were float percentages
        if (isPercentage)
        {
            differenceString += "%";
        }

        return (differenceString, differenceInt);
    }


    /// <summary>
    /// Accepts a damage type and returns a shortened two character string respective to that damage type.
    /// </summary>
    /// <param name="type">The damage type (ItemDamageType).</param>
    /// <returns>A (sometimes) two character string. example: "EM"</returns>
    public static string ShortenDamageType(ItemDamageType type)
    {
        switch (type)
        {
            case ItemDamageType.Kinetic:
                return "KI";
            case ItemDamageType.Thermal:
                return "TH";
            case ItemDamageType.Explosive:
                return "EX";
            case ItemDamageType.EMP:
                return "EM";
            case ItemDamageType.Phasic:
                return "PH";
            case ItemDamageType.Impact:
                return "I";
            case ItemDamageType.Slashing:
                return "S";
            case ItemDamageType.Piercing:
                return "P";
            case ItemDamageType.Entropic:
                return "E";
            default:
                return "?";
        }
    }

    public static string HighlightDamageType(string input)
    {
        // Dictionary of colors
        Dictionary<string, string> damageTypeColors = new Dictionary<string, string>
        {
            { "KI", "#BABABA" },
            { "TH", "#B75E00" },
            { "EX", "#980400" },
            { "EM", "#0079C6" },
            { "PH", "#7F00C5" },
            { "I", "#9FC880" },
            { "S", "#AEAEAE" },
            { "P", "#A6006C" },
            { "E", "#C57F00" },
        };


        // Define the regular expression pattern to match damage type segments (and only match at the end)
        string pattern = @"\b(KI|TH|EX|EM|PH|I|S|P|E)\b$";

        // Use Regex.Replace to find and replace the matches
        string output = Regex.Replace(input, pattern, match =>
        {
            // Get the matched damage type
            string damageType = match.Groups[1].Value;

            // Check if the damage type has a color defined in the dictionary
            if (damageTypeColors.ContainsKey(damageType))
            {
                // Use the color defined in the dictionary
                return $"<color={damageTypeColors[damageType]}>{damageType}</color>";
            }
            else
            {
                // Use a default color if no color is defined for the damage type
                return $"<color=#000000>{damageType}</color>";
            }
        });

        return output;
    }

    public static string ItemPrototypeName(Item item)
    {
        string cName = "Prototype ";

        switch (item.itemData.type)
        {
            case ItemType.Default:
                cName += "Part";
                break;
            case ItemType.Engine:
                cName += "Engine";
                break;
            case ItemType.PowerCore:
                cName += "Power Core";
                break;
            case ItemType.Reactor:
                cName += "Reactor";
                break;
            case ItemType.Treads:
                cName += "Treads";
                break;
            case ItemType.Legs:
                cName += "Legs";
                break;
            case ItemType.Wheels:
                cName += "Wheels";
                break;
            case ItemType.Hover:
                cName += "Hover Unit";
                break;
            case ItemType.Flight:
                cName += "Flight Unit";
                break;
            case ItemType.Storage:
                cName += "Storage";
                break;
            case ItemType.Processor:
                cName += "Processor";
                break;
            case ItemType.Hackware:
                cName += "Hackware";
                break;
            case ItemType.Device:
                cName += "Device";
                break;
            case ItemType.Armor:
                cName += "Protection";
                break;
            case ItemType.Alien:
                cName = "Unknown Alien Artifact";
                break;
            case ItemType.Gun:
                cName += "Ballistic Gun";
                break;
            case ItemType.EnergyGun:
                cName += "Energy Gun";
                break;
            case ItemType.Cannon:
                cName += "Ballistic Cannon";
                break;
            case ItemType.EnergyCannon:
                cName += "Energy Cannon";
                break;
            case ItemType.Launcher:
                cName += "Launcher";
                break;
            case ItemType.Impact:
                cName += "Melee Weapon";
                break;
            case ItemType.Special:
                cName += "Melee Weapon";
                break;
            case ItemType.Melee:
                cName += "Melee Weapon";
                break;
            case ItemType.Data:
                cName += "Data Object";
                break;
            case ItemType.Nonpart:
                cName += "Mechanism";
                break;
            case ItemType.Trap:
                cName += "Trap";
                break;
        }

        return cName;
    }

    #endregion

    #region Highlighted Path Pruning
    // Function to prune the highlighted tiles based on A* pathfinding
    public static List<GameObject> PrunePath(List<GameObject> path)
    {
        if (path.Count < 3)
        {
            // No pruning needed for a path with less than 3 tiles
            return path;
        }

        List<GameObject> prunedPath = new List<GameObject>();
        prunedPath.Add(path[0]); // Add the starting tile

        for (int i = 2; i < path.Count; i++)
        {
            // Check if the path from the current tile to the next tile is diagonal
            if (IsDiagonalMove(path[i - 2].transform.position, path[i].transform.position))
            {
                // Prune the intermediate tile for diagonal moves
                continue;
            }

            prunedPath.Add(path[i - 1]); // Add the intermediate tile for non-diagonal moves
        }

        prunedPath.Add(path[path.Count - 1]); // Add the last tile
        return prunedPath;
    }

    // Function to check if two positions are diagonal to each other
    private static bool IsDiagonalMove(Vector3 position1, Vector3 position2)
    {
        return Mathf.Abs(position1.x - position2.x) > 0 && Mathf.Abs(position1.y - position2.y) > 0;
    }
    #endregion

    #region Misc

    public static void RemoveMachineFromList(MachinePart go)
    {
        switch (go.type)
        {
            case MachineType.Fabricator:
                foreach (var P in MapManager.inst.machines_fabricators.ToList())
                {
                    if(P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_fabricators.Remove(P);
                    }
                }
                break;
            case MachineType.Garrison:
                foreach (var P in MapManager.inst.machines_garrisons.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_garrisons.Remove(P);
                    }
                }
                break;
            case MachineType.Recycling:
                foreach (var P in MapManager.inst.machines_recyclingUnits.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_recyclingUnits.Remove(P);
                    }
                }
                break;
            case MachineType.RepairStation:
                foreach (var P in MapManager.inst.machines_repairStation.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_repairStation.Remove(P);
                    }
                }
                break;
            case MachineType.Scanalyzer:
                foreach (var P in MapManager.inst.machines_scanalyzers.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_scanalyzers.Remove(P);
                    }
                }
                break;
            case MachineType.Terminal:
                foreach (var P in MapManager.inst.machines_terminals.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_terminals.Remove(P);
                    }
                }
                break;
            case MachineType.CustomTerminal:
                foreach (var P in MapManager.inst.machines_customTerminals.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_customTerminals.Remove(P);
                    }
                }
                break;
            case MachineType.DoorTerminal:

                break;
            case MachineType.Misc:
                foreach (var P in MapManager.inst.machines_static.ToList())
                {
                    if (P == go.gameObject.transform.parent.gameObject)
                    {
                        MapManager.inst.machines_static.Remove(P);
                    }
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Function to rotate coordinates in 90-degree increments. Used primarily in *DungeonGeneratorCTR*
    /// </summary>
    /// <param name="startCoords">The bottom left corner of the prefab.</param>
    /// <param name="endCoords">The top right corner of the prefab.</param>
    /// <param name="rotation">The rotation angle (in 90* increments) to rotate the coords by.</param>
    /// <returns>The newly rotated pair of coordinates. StartX/Y & EndX/Y</returns>
    public static (Vector2Int, Vector2Int) RotateCoordinates(Vector2Int startCoords, Vector2Int endCoords, float rotation)
    {
        // Calculate the center of the room
        Vector2Int center = new Vector2Int((startCoords.x + endCoords.x) / 2, (startCoords.y + endCoords.y) / 2);

        // Convert rotation to integer representing the number of 90-degree increments
        int rotationIncrements = Mathf.RoundToInt(rotation / 90f) % 4;

        // Rotate coordinates based on the number of 90-degree increments
        for (int i = 0; i < rotationIncrements; i++)
        {
            int tempX = startCoords.x;
            startCoords.x = center.x - (center.y - startCoords.y);
            startCoords.y = center.y + (tempX - center.x);

            int tempEndX = endCoords.x;
            endCoords.x = center.x - (center.y - endCoords.y);
            endCoords.y = center.y + (tempEndX - center.x);
        }

        return (startCoords, endCoords);
    }

    /// <summary>
    /// Function to shift coordinates to a random location within the map
    /// </summary>
    /// <param name="startCoords">The bottom left corner of the prefab.</param>
    /// <param name="endCoords">The top right corner of the prefab.</param>
    /// <param name="mapSize">The size of the map.</param>
    /// <param name="minDistanceFromEdge">Optional value. How far this new location should be from the edge of the map.</param>
    /// <returns>A newly updated coordinate pair of where the prefab should end up.</returns>
    public static (Vector2Int, Vector2Int) ShiftCoordinates(Vector2Int startCoords, Vector2Int endCoords, Vector2Int mapSize, float minDistanceFromEdge = 0f)
    {
        // Calculate the size of the room
        int roomWidth = Mathf.Abs(endCoords.x - startCoords.x) + 1;
        int roomHeight = Mathf.Abs(endCoords.y - startCoords.y) + 1;

        // Calculate the maximum valid position considering the minimum distance from the edge
        int maxX = mapSize.x - roomWidth - 1;
        int maxY = mapSize.y - roomHeight - 1;

        // Ensure the minimum distance from the edge
        int minX = Mathf.RoundToInt(minDistanceFromEdge);
        int minY = Mathf.RoundToInt(minDistanceFromEdge);

        // Randomly shift coordinates within the valid range
        int randomX = Random.Range(minX, maxX + 1);
        int randomY = Random.Range(minY, maxY + 1);

        // Update the start and end coordinates
        startCoords.x += randomX;
        startCoords.y += randomY;
        endCoords.x += randomX;
        endCoords.y += randomY;

        return (startCoords, endCoords);
    }

    /// <summary>
    /// Function to determine the relative position of the target in reference to the specified actor
    /// </summary>
    /// <param name="actorPosition">The actor in question.</param>
    /// <param name="targetPosition">The target reference location.</param>
    /// <returns>([Left or Right] Vector2, [Up or Down] Vector2) the two reference locations in the form of Vector2.???</returns>
    public static (Vector2, Vector2) GetRelativePosition(Vector2 actorPosition, Vector2 targetPosition)
    {
        Vector2 relativeDirectionX = Vector2.zero;
        Vector2 relativeDirectionY = Vector2.zero;

        // Compare x coordinates
        if (targetPosition.x < actorPosition.x)
        {
            relativeDirectionX = Vector2.left; // Target is to the left of the player
        }
        else if (targetPosition.x > actorPosition.x)
        {
            relativeDirectionX = Vector2.right; // Target is to the right of the player
        }

        // Compare y coordinates
        if (targetPosition.y < actorPosition.y)
        {
            relativeDirectionY = Vector2.down; // Target is below the player
        }
        else if (targetPosition.y > actorPosition.y)
        {
            relativeDirectionY = Vector2.up; // Target is above the player
        }

        return (relativeDirectionX, relativeDirectionY);
    }

    public static Sprite GetProjectileSprite(Vector3 origin, Vector3 target)
    {
        // Calculate the direction vector from origin to target
        Vector3 direction = target - origin;

        /*  -- Currently the sprites are organized as follows:
         *  /  \  |  -
         */

        // Flags
        int sprite = 0;
        bool up = false;
        bool left = false;

        if (direction.y > 0)
            up = true;

        if(direction.x < 0)
            left = true;

        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.y) < 2) // This isn't the best but it works
        {
            // - sideways
            sprite = 3;
        }
        else
        {
            if (up)
            {
                if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    // | straight up
                    sprite = 2;
                }
                else // diagonal (/ or \)
                {
                    if (left)
                    {
                        // up-left \
                        sprite = 1;
                    }
                    else
                    {
                        // up-right /
                        sprite = 0;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    // | straight up
                    sprite = 2;
                }
                else // diagonal (/ or \)
                {
                    if (left)
                    {
                        // up-right /
                        sprite = 0;
                    }
                    else
                    {
                        // up-left \
                        sprite = 1;
                    }
                }
            }
        }

        // Return the sprite from the global list based on the determined index
        return MiscSpriteStorage.inst.projectileSprites[sprite];
    }

    // Used in the Influence UI
    public static string AssignInfluenceLetter(int value)
    {
        // Ensure the value is within the range of 0 to 200
        value = Mathf.Clamp(value, 0, 200);

        // Calculate the index of the alphabet character based on the value
        int index = Mathf.RoundToInt(value / 200f * 25);

        // Convert the index to the corresponding character
        char character = (char)('A' + index);

        return character.ToString();
    }

    public static Vector2 GetDirection(Vector2 origin, Vector2 target)
    {
        Vector2 directionVector = target - origin;

        float angle = Vector2.SignedAngle(Vector2.up, directionVector);

        if (angle > -45 && angle <= 45)
        {
            return Vector2.up;
        }
        else if (angle > 45 && angle <= 135)
        {
            return Vector2.right;
        }
        else if (angle > -135 && angle <= -45)
        {
            return Vector2.left;
        }
        else
        {
            return Vector2.down;
        }
    }

    public static List<string> StringToList(string input)
    {
        List<string> resultList = new List<string>();

        // Iterate over each character in the input string
        for (int i = 0; i <= input.Length; i++)
        {
            // Extract the substring from the beginning up to the current character
            string substring = input.Substring(0, i);
            // Add the substring to the result list
            resultList.Add(substring);
        }

        return resultList;
    }

    public static AudioClip RandomClip(List<AudioClip> clips)
    {
        return clips[Random.Range(0, clips.Count - 1)];
    }

    public static Color GetDarkerColor(Color originalColor, float percentage)
    {
        // Make sure the percentage is within the range [0, 100].
        percentage = Mathf.Clamp(percentage, 0f, 100f) / 100f;

        // Calculate the darker color components.
        float r = originalColor.r - originalColor.r * percentage;
        float g = originalColor.g - originalColor.g * percentage;
        float b = originalColor.b - originalColor.b * percentage;

        // Make sure the color components are within the range [0, 1].
        r = Mathf.Clamp01(r);
        g = Mathf.Clamp01(g);
        b = Mathf.Clamp01(b);

        return new Color(r, g, b, originalColor.a);
    }

    public static string RecolorPrefix(ItemObject item, string color)
    {
        // Recolor is made up of two parts:
        // "<color=#009700>"
        // "</color>"

        string name = item.itemName;

        if (item.quality != ItemQuality.Standard)
        {
            string[] split = name.Split(". ");

            name = "<color=#" + color + ">" + split[0] + ". </color>" + split[1];
        }

        return name;
    }

    public static void ModifyBotAllegance(Actor bot, List<BotRelation> rList, BotAlignment newFaction)
    {
        bot.allegances.alleganceTree.Clear();

        bot.allegances.alleganceTree.Add((BotAlignment.Complex, rList[0]));
        bot.allegances.alleganceTree.Add((BotAlignment.Derelict, rList[1]));
        bot.allegances.alleganceTree.Add((BotAlignment.Assembled, rList[2]));
        bot.allegances.alleganceTree.Add((BotAlignment.Warlord, rList[3]));
        bot.allegances.alleganceTree.Add((BotAlignment.Zion, rList[4]));
        bot.allegances.alleganceTree.Add((BotAlignment.Exiles, rList[5]));
        bot.allegances.alleganceTree.Add((BotAlignment.Architect, rList[6]));
        bot.allegances.alleganceTree.Add((BotAlignment.Subcaves, rList[7]));
        bot.allegances.alleganceTree.Add((BotAlignment.SubcavesHostile, rList[8]));
        bot.allegances.alleganceTree.Add((BotAlignment.Player, rList[9]));
        bot.allegances.alleganceTree.Add((BotAlignment.None, rList[10]));

        bot.myFaction = newFaction;
    }

    public static List<Actor> GatherVisibleAllies(Actor source)
    {
        List<Actor> bots = new List<Actor>();

        List<Actor> visibileBots = new List<Actor>();

        // Gather up all visible bots
        foreach (Entity bot in GameManager.inst.entities)
        {
            if(HF.ActorInBotFOV(source, bot.GetComponent<Actor>()))
            {
                visibileBots.Add(bot.GetComponent<Actor>());
            }
        }

        // Check if those bots are allied
        foreach (var bot in visibileBots)
        {
            if(source.myFaction == bot.myFaction)
            {
                bots.Add(bot);
            }
        }

        return bots;
    }

    public static void RemovePartFromBotInventory(Actor bot, Item item)
    {
        if (bot.botInfo) // Bot
        {
            foreach (var I in bot.components.Container.Items.ToList())
            {
                if (I.item.Id >= 0 && I.item == item)
                {
                    bot.components.RemoveItem(I.item); // Remove the item
                    return;
                }
            }

            foreach (var I in bot.armament.Container.Items.ToList())
            {
                if (I.item.Id >= 0 && I.item == item)
                {
                    bot.armament.RemoveItem(I.item); // Remove the item
                    return;
                }
            }

            foreach (var I in bot.inventory.Container.Items.ToList())
            {
                if(I.item == item)
                {
                    bot.inventory.RemoveItem(item);
                    return;
                }
            }
        }
    }

    public static float CalculateItemCoverage(Actor actor, Item item)
    {
        // We aren't going to consider to hit bonuses or any other effects here.
        
        // Gather up all the items
        List<Item> items = Action.CollectAllBotItems(actor);

        // Get the core exposure
        int coreExposure = 0;
        float coreHitChance = 0f;
        if (actor.botInfo)
        {
            //coreExposure = actor.botInfo.coreExposure; // This is a percent value in the game files and I don't know how to calculate the true value. Fun!
            coreExposure = 100;
            coreHitChance = actor.botInfo.coreExposure;
        }
        else
        {
            coreExposure = PlayerData.inst.currentCoreExposure;
        }


        int totalExposure = 0; // Calculate total exposure
        foreach (var I in items)
        {
            totalExposure += I.itemData.coverage;
        }

        return item.itemData.coverage / totalExposure; // And with that, return the item's calculated exposure
    }

    /// <summary>
    /// Checks to see if the actor in question should be able to see through the specified phase wall. Returns true/false.
    /// </summary>
    /// <param name="actor">The actor in question.</param>
    /// <param name="tile">The phase wall tile (a wall).</param>
    /// <returns>True/false, if this wall should be see-through for this specific bot.</returns>
    public static bool PhaseWallVisCheck(Actor actor, TileBlock tile)
    {
        return tile.phaseWallTeam == actor.myFaction;
    }

    public static (int intVal, float floatVal) ParseEffectStackingValues(List<SimplifiedItemEffect> list)
    {
        int totalIntValue = 0;
        float totalFloatValue = 0f;

        foreach (var effect in list)
        {
            if (effect.stacks)
            {
                if (effect.half_stacks)
                {
                    // If only half of the effect can stack, add half of the effect
                    totalIntValue += effect.intVal / 2;
                    totalFloatValue += effect.floatVal / 2f;
                }
                else
                {
                    // If the effect fully stacks, add the full effect
                    totalIntValue += effect.intVal;
                    totalFloatValue += effect.floatVal;
                }
            }
            else
            {
                // If the effect doesn't stack, replace the total effect with the new effect
                totalIntValue = effect.intVal;
                totalFloatValue = effect.floatVal;
            }
        }

        return (totalIntValue, totalFloatValue);
    } 

    /// <summary>
    /// Used in UIManager to get the color and display text for the /DATA/ box next to "ID" for bots.
    /// </summary>
    /// <param name="faction">The faction of the bot.</param>
    /// <returns>(Color, string)</returns>
    public static (Color color, string text, string extra) VisualsFromBotRelation(Actor bot)
    {
        // First consider any special states
        if (bot.state_CLOAKED)
        {
            return (UIManager.inst.deepInfoBlue, "CLOAKED", "This bot is currently moving in stealth, and is only visible to you and its allies.");
        }
        else if (bot.state_DISABLED)
        {
            return (UIManager.inst.warningOrange, "DISABLED", "This bot is currently disabled and will not re-activate unless directly interfered with.");
        }
        else if (bot.state_DISARMED)
        {
            return (UIManager.inst.dullGreen, "DISARMED", "This bot is currently disarmed and unable to fight. It will likely flee and return to its point of origin.");
        }
        else if (bot.state_DORMANT)
        {
            return (UIManager.inst.corruptOrange_faded, "DORMANT", "Robots temporarily resting in this mode are only a problem if they wake up, which may be triggered by alarm traps, being alerted by their nearby allies, or by hostiles hanging around in their field of vision.");
        }
        else if (bot.state_UNPOWERED)
        {
            return (UIManager.inst.warningOrange, "UNPOWERED", "Unpowered robots will never become active. (Though yeah they’re very much real and you can harvest them for parts if you’d like.)");
        }

        // Then consider relations
        BotRelation relation = HF.DetermineRelation(PlayerData.inst.GetComponent<Actor>(), bot);

        switch (relation)
        {
            case BotRelation.Hostile:
                return (UIManager.inst.highSecRed, "HOSTILE", "This robot's relation with you. If inactive for some reason, this message will provide more context.");
            case BotRelation.Neutral:
                return (UIManager.inst.inactiveGray, "NEUTRAL", "This robot's relation with you. If inactive for some reason, this message will provide more context.");
            case BotRelation.Friendly:
                return (UIManager.inst.highlightGreen, "FRIENDLY", "This robot's relation with you. If inactive for some reason, this message will provide more context.");
            case BotRelation.Default:
                return (UIManager.inst.inactiveGray, "NEUTRAL", "This robot's relation with you. If inactive for some reason, this message will provide more context.");
            default:
                return (UIManager.inst.inactiveGray, "NEUTRAL", "This robot's relation with you. If inactive for some reason, this message will provide more context.");
        }
    }

    /// <summary>
    /// Based on a provided inventory that (presumably) contains utility items that increase inventory size, determines how big an inventory should be.
    /// </summary>
    /// <param name="inv">The InventoryObject we will search through. Should contain utility items.</param>
    /// <param name="baseAmount">A starting amount, can be 0.</param>
    /// <returns>The size the inventory should be based on our findings.</returns>
    public static int CalculateMaxInventorySize(InventoryObject inv, int baseAmount = 0)
    {
        int size = baseAmount;

        foreach (var item in inv.Container.Items)
        {
            if(item.item.Id >= 0)
            {
                // Storage items
                if (item.item.itemData.itemEffects.Count > 0)
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (E.inventorySizeEffect)
                        {
                            size += E.sizeIncrease;
                        }
                    }
                    // There may be some other items out there that increase inventory size with some weird effect (maybe alien items?) but we can add that in later.
                }
            }
        }

        return size;
    }

    public static string CriticalEffectsToString(CritType crit)
    {
        switch (crit)
        {
            case CritType.Nothing:
                return "This weapon has no critical effect.";
            case CritType.Burn:
                return "Significantly increase the heat transfer rate of this weapon.";
            case CritType.Meltdown:
                return "Instantly melt target bot regardless of what part was hit.";
            case CritType.Destroy:
                return "Chance for this weapon to instantly destroy the hit component, or even a robot core. (Cogmind is less susceptible to this effect, which can only destroy those" +
                    " parts which are already below 33% integrity when hit.) Armor is immune to this effect, instead taking an additional 20% damage.";
            case CritType.Blast:
                return "Chance for this weapon to instantly destroy the hit component, or even a robot core. Also damage a second part and knock it off target.";
            case CritType.Corrupt:
                return "Maximize system corruption effect on target.";
            case CritType.Smash:
                return "Chance for this weapon to instantly destroy the hit component, or even a robot core. Also apply an equal amount of damage as overflow damage.";
            case CritType.Sever:
                return "Sever target part, or if hit core also damages and severs a different part.";
            case CritType.Puncture:
                return "Half of damage automatically transferred to core.";
            case CritType.Detonate:
                return "Destroy a random utility part.";
            case CritType.Sunder:
                return "Damage and remove a random propulsion component.";
            case CritType.Intensify:
                return "Doubles damage dealt to target.";
            case CritType.Phase:
                return "Mirrors damage done to a single neighboring bot.";
            case CritType.Impale:
                return "Chance to instantly destroy an enemy bot's core.";
        }
        return "This weapon has no critical effect.";
    }

    public static string DamageTypeToString(ItemDamageType type)
    {
        switch (type)
        {
            case ItemDamageType.Kinetic:
                return "Ballistic weapons generally have a longer effective range and higher chance of critical strike, but suffer from less predictable damage and high recoil. Kinetic cannon hits also have a chance to cause knockback depending on damage, range, and size of the target.";
            case ItemDamageType.Thermal:
                return "Thermal weapons generally have a shorter effective range, but benefit from a more easily predictable damage potential and little or no recoil. Thermal damage also generally transfers heat to the target, and may cause meltdowns in hot enough targets.";
            case ItemDamageType.Explosive:
                return "While powerful, explosives generally spread damage across each target in the area of effect, dividing damage into 1~3 chunks before affecting a robot, where each chunk selects its own target part (though they may overlap). Explosions also significantly tend to reduce the amount of salvage remaining after destroying a target.";
            case ItemDamageType.EMP:
                return "EM weapons have less of an impact on integrity, but are capable of corrupting a target’s computer systems. Anywhere from 50 to 150% of damage done is also applied as system corruption, automatically maximized on a critical hit. EM-based explosions only deal half damage to inactive items lying on the ground.";
            case ItemDamageType.Phasic: // TODO
                return "[NO_DATA]";
            case ItemDamageType.Impact:
                return "Impact melee weapons have a damage-equivalent chance to cause knockback, and while incapable of a critical strike, they ignore coverage and are effective at destroying fragile systems. For every component crushed by an impact, its owner’s system is significantly corrupted (+25-150%), though electromagnetic resistance can help mitigate this effect.";
            case ItemDamageType.Slashing:
                return "Slashing melee weapons are generally very damaging, and most are also capable of severing components from a target without destroying them.";
            case ItemDamageType.Piercing:
                return "Piercing melee weapons achieve critical strikes more often, are more likely to hit a robot’s core (doubles core exposure value in hit location calculations), and get double the melee momentum damage bonus.";
            case ItemDamageType.Entropic: // TODO
                return "[NO_DATA]";
        }
        return "";
    }

    public static int CalculateAverageTimeToMove(Actor bot)
    {
        List<Item> items = Action.CollectAllBotItems(bot);
        int total = 0;
        int count = 0;

        foreach (var item in items)
        {
            if (item.itemData.propulsion[0].timeToMove > 0)
            {
                total += item.itemData.propulsion[0].timeToMove;
                count++;
            }
        }

        return Mathf.RoundToInt(total / count);
    }

    #endregion

    #region Spotting

    public static bool LOSOnTarget(GameObject source, GameObject target)
    {
        bool LOS = true;

        // - If line of sight being blocked - // (THIS ALSO GETS USED LATER)
        Vector2 targetDirection = target.transform.position - source.transform.position;
        float distance = Vector2.Distance(Action.V3_to_V2I(source.transform.position), Action.V3_to_V2I(target.transform.position));

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(source.transform.position.x, source.transform.position.y), targetDirection.normalized, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            DoorLogic door = hit.collider.GetComponent<DoorLogic>();
            MachinePart machine = hit.collider.GetComponent<MachinePart>();

            // (TODO: Expand this later when needed)
            // If we encounter:
            // - A wall
            // - A closed door
            // - A machine

            // Then there is no LOS

            if (tile != null && tile.tileInfo.type == TileType.Wall)
            {
                return false;
            }

            if (door != null && tile.specialNoBlockVis == true)
            {
                LOS = true;
            }
            else if (door != null && tile.specialNoBlockVis == false)
            {
                return false;
            }

            if (machine != null)
            {
                return false;
            }
        }

        return LOS;
    }

    /// <summary>
    /// Basically *LOSOnTarget* but returns the first thing blocking line of sight if there is anything.
    /// </summary>
    /// <param name="source">The origin location.</param>
    /// <param name="target">The target location.</param>
    /// <param name="requireVision">If the player needs to be able to see the blocking object.</param>
    /// <returns></returns>
    public static GameObject ReturnObstacleInLOS(GameObject source, Vector3 target, bool requireVision = false)
    {
        GameObject blocker = null;

        Vector2 targetDirection = target - source.transform.position;

        float distance = Vector2.Distance(Action.V3_to_V2I(source.transform.position), Action.V3_to_V2I(target));

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(source.transform.position.x, source.transform.position.y), targetDirection.normalized, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            DoorLogic door = hit.collider.GetComponent<DoorLogic>();
            MachinePart machine = hit.collider.GetComponent<MachinePart>();

            // (TODO: Expand this later when needed)
            // If we encounter:
            // - A wall
            // - A closed door
            // - A machine

            // Then there is no LOS

            if (tile != null && tile.tileInfo.type == TileType.Wall)
            {
                if (requireVision)
                {
                    if (tile.isExplored)
                    {
                        return tile.gameObject;
                    }
                    else
                    {
                        blocker = null;
                    }
                }
                else
                {
                    return tile.gameObject;
                }
            }

            if (door != null && tile.specialNoBlockVis == true)
            {
                blocker = null;
            }
            else if (door != null && tile.specialNoBlockVis == false)
            {
                if (requireVision)
                {
                    if (door.GetComponent<TileBlock>().isExplored)
                    {
                        return door.gameObject;
                    }
                    else
                    {
                        blocker = null;
                    }
                }
                else
                {
                    return door.gameObject;
                }
            }

            if (machine != null)
            {
                if (requireVision)
                {
                    if (machine.isExplored)
                    {
                        return machine.gameObject;
                    }
                    else
                    {
                        blocker = null;
                    }
                }
                else
                {
                    return machine.gameObject;
                }
            }
        }

        return blocker;
    }

    /// <summary>
    /// Basically *ReturnObstacleInLOS* but refined for attack targeting.
    /// </summary>
    /// <param name="source">The origin location.</param>
    /// <param name="target">The target location.</param>
    /// <returns>Returns the thing we are actually going to attack against.</returns>
    public static GameObject DetermineAttackTarget(GameObject source, Vector3 target)
    {
        GameObject blocker = null;

        Vector2 targetDirection = target - source.transform.position;
        float distance = Vector2.Distance(Action.V3_to_V2I(source.transform.position), Action.V3_to_V2I(target));

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(source.transform.position.x, source.transform.position.y), targetDirection.normalized, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            DoorLogic door = hit.collider.GetComponent<DoorLogic>();
            MachinePart machine = hit.collider.GetComponent<MachinePart>();

            // (TODO: Expand this later when needed)
            // List of viable targets:
            // - A wall
            // - A closed door
            // - A machine
            // - A bot

            if (tile != null && tile.tileInfo.type == TileType.Wall)
            {
                blocker = tile.gameObject;
            }

            if (door != null && tile.specialNoBlockVis == true)
            {
                blocker = null;
            }
            else if (door != null && tile.specialNoBlockVis == false)
            {
                blocker = door.gameObject;
            }

            if (machine != null)
            {
                blocker = machine.gameObject;
            }
        }

        if(blocker == null)
        {
            // We have nothing blocking our LOS from player to where this mouse is, so we are probably targeting a floor tile right now.
            // We need to keep going (recursively) past the mouse position until we actually hit something.

            Vector2 direction = targetDirection;
            direction.Normalize();
            blocker = HF.DetermineAttackTarget(source, target + (Vector3)direction); // Recursion!
        }

        return blocker;
    }

    /// <summary>
    /// Attempts to locate the most relevant object to target at the specified position. Walls, bots, doors, machines, floors, etc.
    /// </summary>
    /// <param name="pos">The location to check at.</param>
    /// <returns>The most relevant game object at the specified position. Most common scenario is a floor/wall.</returns>
    public static GameObject GetTargetAtPosition(Vector2Int pos)
    {
        Vector3 lowerPosition = new Vector3(pos.x, pos.y, 2);
        Vector3 upperPosition = new Vector3(pos.x, pos.y, -2);
        Vector3 direction = lowerPosition - upperPosition;
        float distance = Vector3.Distance(new Vector3Int((int)lowerPosition.x, (int)lowerPosition.y, 0), upperPosition);
        direction.Normalize();
        RaycastHit2D[] hits = Physics2D.RaycastAll(upperPosition, direction, distance);

        // - Flags -
        GameObject wall = null;
        GameObject bot = null;
        GameObject door = null;
        GameObject machine = null;
        GameObject floor = null;

        // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            // PROBLEM!!! This list of hits is unsorted and contains multiple things that violate the heirarchy below. This MUST be fixed!

            // There is a heirarchy of what we want to display:
            // -A wall
            // -A bot
            // -A door
            // -A machine
            // -A floor tile

            // We will solve this problem by setting flags. And then going back afterwards and using our heirarchy.

            #region Hierarchy Flagging
            if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Wall"))
            {
                // A wall
                wall = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<Actor>())
            {
                // A bot
                bot = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Door"))
            {
                // Door
                door = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<MachinePart>()) // maybe refine this later
            {
                // Machine
                machine = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor"))
            {
                // Dirty floor tile
                floor = hit.collider.gameObject;
            }

            #endregion
        }

        GameObject retObject = null;

        if(wall != null)
        {
            retObject = wall;
        }
        else if (bot != null)
        {
            retObject = bot;
        }
        else if (door != null)
        {
            retObject = door;
        }
        else if (machine != null)
        {
            retObject = machine;
        }
        else if (floor != null)
        {
            retObject = floor;
        }

        return retObject;
    }

    /// <summary>
    /// Similar to GetTargetAtPosition except it returns the neighboring targets AROUND the specified position as a list.
    /// </summary>
    /// <param name="pos">The center position.</param>
    /// <returns>A list of (most relevant) neighboring gameObjects.</returns>
    public static List<GameObject> GetNeighboringTargetsAtPosition(Vector2Int pos)
    {
        List<GameObject> neighbors = new List<GameObject>();

        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.up));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.down));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.left));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.right));

        // Diagonals
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.up + Vector2Int.left));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.up + Vector2Int.right));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.down + Vector2Int.left));
        neighbors.Add(HF.GetTargetAtPosition(pos + Vector2Int.down + Vector2Int.right));

        return neighbors;
    }

    /// <summary>
    /// Determines if the actor in question is within the players FOV (list).
    /// </summary>
    /// <param name="actor">The actor in question.</param>
    /// <returns>If this actor is in the player's FOV. True/False</returns>
    public static bool InPlayerFOV(GameObject actor)
    {
        return (PlayerData.inst.GetComponent<Actor>().FieldofView.Contains(new Vector3Int((int)actor.transform.position.x, (int)actor.transform.position.y, (int)actor.transform.position.z))
            && actor.GetComponent<Actor>().isVisible);
    }

    /// <summary>
    /// Determines if the target bot is in the spotter bot's FOV.
    /// </summary>
    /// <param name="spotter">The actor doing the spotting.</param>
    /// <param name="target">The target to look for.</param>
    /// <returns></returns>
    public static bool ActorInBotFOV(Actor spotter, Actor target)
    {
        return spotter.FieldofView.Contains(new Vector3Int((int)target.transform.position.x, (int)target.transform.position.y, (int)target.transform.position.z));
    }

    /// <summary>
    /// Tries to locate an entity inside GameManager's entity list based on a specified position. Returns that actor.
    /// </summary>
    /// <param name="pos">The position to search for.</param>
    /// <returns>The actor found at the position (if one is found).</returns>
    public static Actor FindActorAtPosition(Vector2 pos)
    {
        foreach (Entity E in GameManager.inst.entities)
        {
            if(HF.V3_to_V2I(E.transform.position) == pos)
            {
                return E as Actor;
            }
        }

        return null;
    }

    #endregion
}
