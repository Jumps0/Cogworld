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
using Transform = UnityEngine.Transform;
using System.Text;
using Color = UnityEngine.Color;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ColorUtility = UnityEngine.ColorUtility;
using static UnityEngine.Rendering.DebugUI;
using static Setup;
using System.Drawing;

/// <summary>
/// Contains helper functions to be used globally.
/// </summary>
public static class HF
{
    public static Vector3 LocationOfPlayer()
    {
        return PlayerData.inst.transform.position;
    }

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

    #region ID Lookup
    public static TileType Tile_to_TileType(TileCG type)
    {
        switch (type)
        {
            case TileCG.Wall:
                return TileType.Wall;
            case TileCG.Floor:
                return TileType.Floor;
            case TileCG.Door:
                return TileType.Door;
            default:
                return TileType.Floor;
        }
    }

    public static int IDbyTheme(TileType type)
    {
        // FLAG - UPDATE NEW LEVELS

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
                case "ZION":
                    return 2;
                case "ZDC":
                    return 18;
                case "MINES":
                    return 2;
                case "RECYCLING":
                    return 9;
                case "SUBCAVES":
                    return 2;
                case "WASTES":
                    return 10;
                case "SCRAPTOWN":
                    return 2;
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
                case "ZION":
                    return 22;
                case "ZDC":
                    return 22;
                case "MINES":
                    return 22;
                case "RECYCLING":
                    return 4;
                case "SUBCAVES":
                    return 22;
                case "WASTES":
                    return 4;
                case "SCRAPTOWN":
                    return 22;
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
                case "ZION":
                    return 5;
                case "ZDC":
                    return 5;
                case "MINES":
                    return 5;
                case "RECYCLING":
                    return 5;
                case "SUBCAVES":
                    return 5;
                case "WASTES":
                    return 5;
                case "SCRAPTOWN":
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
    #endregion

    #region Machines/Hacking

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

    /// <summary>
    /// Given a gameObject references a specific machine, will determine what type of machine that is and return it as a String.
    /// </summary>
    /// <param name="machine">The gameObject which MUST have some kind of machine script attached to it.</param>
    /// <returns>A string of the type of the machine.</returns>
    public static string GetMachineTypeAsString(InteractableMachine machine)
    {
        switch (machine.type)
        {
            case MachineType.Fabricator:
                return "Fabricator";
            case MachineType.Garrison:
                return "Garrison";
            case MachineType.Recycling:
                return "Recycling Unit";
            case MachineType.RepairStation:
                return "Repair Bay";
            case MachineType.Scanalyzer:
                return "Scanalyzer";
            case MachineType.Terminal:
                return "Terminal";
            case MachineType.CustomTerminal:
                return "Terminal";
            case MachineType.DoorTerminal:
                return "Terminal";
            case MachineType.Misc:
                return "Unknown";
            default:
                return "Unknown";
        }
    }

    public static InteractableMachine GetInteractableMachine(GameObject machine)
    {
        return machine.GetComponent<InteractableMachine>() ? machine.GetComponent<InteractableMachine>() : null;
    }

    public static GameObject GetRandomMachineOfType(MachineType type)
    {
        switch (type)
        {
            case MachineType.Fabricator:
                if(MapManager.inst.machines_fabricators.Count > 0)
                {
                    return MapManager.inst.machines_fabricators[Random.Range(0, MapManager.inst.machines_fabricators.Count - 1)];
                }
                break;
            case MachineType.Garrison:
                if (MapManager.inst.machines_garrisons.Count > 0)
                {
                    return MapManager.inst.machines_garrisons[Random.Range(0, MapManager.inst.machines_garrisons.Count - 1)];
                }
                break;
            case MachineType.Recycling:
                if (MapManager.inst.machines_recyclingUnits.Count > 0)
                {
                    return MapManager.inst.machines_recyclingUnits[Random.Range(0, MapManager.inst.machines_recyclingUnits.Count - 1)];
                }
                break;
            case MachineType.RepairStation:
                if (MapManager.inst.machines_repairStation.Count > 0)
                {
                    return MapManager.inst.machines_repairStation[Random.Range(0, MapManager.inst.machines_repairStation.Count - 1)];
                }
                break;
            case MachineType.Scanalyzer:
                if (MapManager.inst.machines_scanalyzers.Count > 0)
                {
                    return MapManager.inst.machines_scanalyzers[Random.Range(0, MapManager.inst.machines_scanalyzers.Count - 1)];
                }
                break;
            case MachineType.Terminal:
                if (MapManager.inst.machines_terminals.Count > 0)
                {
                    return MapManager.inst.machines_terminals[Random.Range(0, MapManager.inst.machines_terminals.Count - 1)];
                }
                break;
            case MachineType.CustomTerminal:
                if (MapManager.inst.machines_customTerminals.Count > 0)
                {
                    return MapManager.inst.machines_customTerminals[Random.Range(0, MapManager.inst.machines_customTerminals.Count - 1)];
                }
                break;
            case MachineType.DoorTerminal:
                break;
            case MachineType.Misc:
                break;
            default:
                break;
        }

        return null;
    }

    public static float CalculateHackSuccessChance(float baseChance)
    {
        float success = baseChance;

        // -- Hack Bonus --
        List<Item> hackware = new List<Item>();
        bool hasHackware = false;
        (hasHackware, hackware) = Action.FindPlayerHackware();
        float hackwareBonus = 0f;

        if (hasHackware)
        {
            foreach (Item item in hackware)
            {
                if (item.itemData.itemEffects.Count > 0)
                {
                    foreach (ItemEffect effect in item.itemData.itemEffects)
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
                return "Downloading analysis...\n    " + command.bot.botName + "\n    Tier: " + command.bot.tier + "\n" + command.bot.description;
            }
            else if (command.item != null) // This is an item (prototype) knowledge reward
            {
                command.item.knowByPlayer = true;
                return "Downloading analysis...\n    " + command.item.itemName + "\n    Rating: " + command.item.rating + "\n" + command.item.description;
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
                        GameManager.inst.AccessEmergency(UIManager.inst.terminal_targetTerm.gameObject);
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
                        List<WorldTile> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            foreach (WorldTile trap in traps)
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
                        List<WorldTile> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            // Group the traps and count how many of each are available.
                            var groupedTraps = traps.GroupBy(f => f.trap_data.trapname).ToDictionary(g => g.Key, g => g.Count());
                            string print = "";

                            // Print the merged version of the items.
                            foreach (var trap in groupedTraps)
                            {
                                string plural = trap.Value > 1 ? "s" : "";
                                print += $"Located {trap.Value} {trap.Key}{plural}.\n";
                            }
                            foreach (WorldTile T in traps)
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
                        List<WorldTile> traps = UIManager.inst.terminal_targetTerm.GetComponent<Terminal>().zone.trapList;
                        if (traps.Count > 0)
                        {
                            // Group the traps and count how many of each are available.
                            var groupedTraps = traps.GroupBy(f => f.trap_data.trapname).ToDictionary(g => g.Key, g => g.Count());
                            string print = "";

                            // Print the merged version of the items.
                            foreach (var trap in groupedTraps)
                            {
                                string plural = trap.Value > 1 ? "s" : "";
                                print += $"Located {trap.Value} {trap.Key}{plural}.\n";
                            }
                            foreach (WorldTile T in traps)
                            {
                                T.SetTrapAlignment(BotAlignment.Player); // Delightfully devilish Seymour
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
                    // !! This can be either opening a garrison or opening some scripted doors. We can easily differentiate which.
                    if(command.hack.relatedMachine == MachineType.Garrison)
                    {
                        // Open the entrance to this garrison

                        // 1. Play the sound
                        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_door["GARRISON_UNLOCK"]); // DOORS - GARRISON_UNLOCK
                                                                                                                                              // 2. Print out in info blue (time) "EXIT=UNLOCKED: GARRISON"
                        UIManager.inst.CreateNewLogMessage("EXIT=UNLOCKED: GARRISON", UIManager.inst.infoBlue, UIManager.inst.dullGreen, true);
                        // 3. Spawn an exit to garrison underneath the player, and ensure that the exit notification appears
                        Vector2Int loc = HF.V3_to_V2I(UIManager.inst.terminal_targetTerm.GetComponent<Garrison>().ejectionSpot.transform.position);
                        MapManager.inst.mapdata[loc.x, loc.y] = MapManager.inst.PlaceLevelExit(loc, true, 4);
                        UIManager.inst.terminal_targetTerm.GetComponent<Garrison>().Open(); // Also let the garrison know
                                                                                            // 4. Print out in deep info blue (no time) "Access door unlocked."
                        return "Access door unlocked.";
                    }
                    else if(command.hack.relatedMachine == MachineType.CustomTerminal)
                    {
                        // TODO
                        // Open these scripted doors
                        UIManager.inst.terminal_targetTerm.GetComponent<TerminalCustom>().OpenDoor();

                        // Play door sound (or do this inside the above function? ^)

                        // Replace this  v  v  v v  with the type of doors
                        return $"Sealed Heavy Doors opened.";
                    }
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
                    // Query systems for current list of installed relay couplers.
                    // Unique for each garrison, this hack is repeatable (if you back out and open it again)
                    // The hack does basically nothing

                    string output = "Installed:\n";

                    Garrison g = UIManager.inst.terminal_targetTerm.GetComponent<Garrison>();
                    if (g.couplers.Count <= 0)
                    {
                        output += "    [None]";
                    }
                    else
                    {
                        for (int i = 0; i < g.couplers.Count; i++)
                        {
                            // Get the info
                            ItemCoupler details = null;
                            foreach (var E in g.couplers[i].itemData.itemEffects)
                            {
                                if (E.couplerDetails.isCoupler)
                                {
                                    details = E.couplerDetails;
                                }
                            }

                            output += "    Relay Coupler [" + details.ToString() + "] (" + g.couplers[i].amount + ")";
                            if (i != g.couplers.Count - 1) // Don't add a newline on the final one
                            {
                                output += "\n";
                            }
                        }
                    }

                    UIManager.inst.terminal_targetTerm.GetComponent<Garrison>().CouplerStatus();

                    return output;
                case TerminalCommandType.Seal:
                    // Seal this garrison's access door, preventing squad dispatches from this location and slowing extermination squad response times across the entire floor.

                    // -print the statement (see below)
                    // 1. Play the closing sound
                    AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_door["SEAL"]); // DOORS - SEAL
                    // 2. Init the red consequences bar /w "GARRISON ACCESS SHUTDOWN" and Forbid any further access to this garrison (via hacking)
                    UIManager.inst.Terminal_DoConsequences(UIManager.inst.highSecRed, "GARRISON ACCESS SHUTDOWN", false, false, false);
                    // 3. Tell the garrison what to do
                    UIManager.inst.terminal_targetTerm.GetComponent<Garrison>().Seal();

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
                        name = bot.botName;
                    }

                    if (PlayerData.inst.currentMatter >= matterCost)
                    {
                        int buildTime = 0;
                        int secLvl = UIManager.inst.terminal_targetTerm.secLvl;

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
                        bName = bot.botName;
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
                    int secLvl3 = UIManager.inst.terminal_targetTerm.secLvl;
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
                    // This command actually has three uses
                    if (MapManager.inst.playerIsInHideout) // - Player is in the hideout, attempting to retrieve (by default 100) matter from their cache
                    {
                        // Get the machine
                        TerminalCustom cache = null;
                        foreach (GameObject machine in MapManager.inst.machines_customTerminals)
                        {
                            if (machine.GetComponentInChildren<TerminalCustom>() && machine.GetComponentInChildren<TerminalCustom>().customType == CustomTerminalType.HideoutCache)
                            {
                                cache = machine.GetComponentInChildren<TerminalCustom>();
                            }
                        }

                        // How much is actualled stored at the moment?
                        int stored = cache.storedMatter;
                        // A couple things can happen here
                        if(stored == 0) // Nothing stored (leave message and quit)
                        {
                            return $"Failed to eject matter. Storage is currently empty.";
                        }
                        else if(stored < 100) // Less than 100 stored (eject all)
                        {
                            cache.storedMatter = 0; // Set value to 0
                            Vector2Int pos = HF.V3_to_V2I(PlayerData.inst.transform.position);

                            // The easiest way to do this is to just drop it under the player and force the pick up check because the logic there is already complete.
                            InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo("Matter", pos, stored, false)); // Spawn some matter
                            PlayerData.inst.GetComponent<Actor>().SpecialPickupCheck(pos); // Do the check

                            // Play a sound
                            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_game["FABRICATION"], 0.5f); // GAME - FABRICATION (is there a better sound for this?)

                            return $"Ejecting {stored} matter...";
                        }
                        else if (stored > 100) // More than 100, eject 100
                        {
                            cache.storedMatter -= 100;
                            Vector2Int pos = HF.V3_to_V2I(PlayerData.inst.transform.position);

                            // The easiest way to do this is to just drop it under the player and force the pick up check because the logic there is already complete.
                            InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo("Matter", HF.LocateFreeSpace(pos), 100, false)); // Spawn some matter
                            PlayerData.inst.GetComponent<Actor>().SpecialPickupCheck(pos); // Do the check

                            // Play a sound
                            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_game["FABRICATION"], 0.5f); // GAME - FABRICATION (is there a better sound for this?)

                            return $"Ejecting 100 matter...";
                        }
                    }
                    else
                    {
                        // Get the machine
                        RecyclingUnit recycler = UIManager.inst.terminal_targetTerm.GetComponentInChildren<RecyclingUnit>();

                        if (parsedName.Contains("Matter")) // - Player is in the world, attempting to steal some amount of stored matter from a recycling machine
                        {
                            // - "Eject all local matter reserves"
                            int toDrop = recycler.storedMatter;

                            if(toDrop > 0)
                            {
                                Vector2Int pos = HF.V3_to_V2I(recycler.ejectionSpot.transform.position);

                                // The easiest way to do this is to just drop it under the player and force the pick up check because the logic there is already complete.
                                InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo("Matter", HF.LocateFreeSpace(pos), toDrop, false)); // Spawn some matter
                                PlayerData.inst.GetComponent<Actor>().SpecialPickupCheck(pos); // Do the check

                                recycler.storedMatter = 0; // Set storage to 0

                                // Play a sound
                                AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_game["FABRICATION"], 0.5f); // GAME - FABRICATION (is there a better sound for this?)

                                return "Ejecting local matter reserves...";
                            }
                            else // No matter stored, unlucky.
                            {
                                return "Local matter reserves are emtpy.";
                            }
                        }
                        else if (parsedName.Contains("Components")) // - Player is in the world, attempting to steal some random stored items
                        {
                            // - "Eject up to 10 parts contained within"

                            if(recycler.storedComponents.Container.Items.Length > 0) // Eject all the stored items
                            {
                                foreach (var I in recycler.storedComponents.Container.Items)
                                {
                                    InventoryControl.inst.DropItemOnFloor(I.item, null, recycler.storedComponents, HF.V3_to_V2I(recycler.ejectionSpot.transform.position));
                                }

                                // Just to be sure, clear the component inventory (unneccessary but just to be safe)
                                recycler.storedComponents.Container.Clear();

                                // Play a sound
                                AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_game["FABRICATION"], 0.5f); // GAME - FABRICATION (is there a better sound for this?)

                                return $"Ejecting stored components...";
                            }
                            else // No stored items, unlucky.
                            {
                                return "No components in local storage.";
                            }
                        }
                    }
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
                        // Reprograms this terminal's Operator when it attempts to use it.
                        return "Trojan loaded successfully.\nTesting...\nSystem interface override in place.";
                    }
                    else if (parsedName.Contains("Botnet"))
                    {
                        // Co-opts this terminal to aid in hacking other machines.
                        // Confers a +6% to hacks, while the benefit of each additional botnet terminal is halved (+3%, +1%),
                        // with each terminal providing no less than +1%.
                        string print = "Trojan loaded successfully.\nTesting...\nTerminal linked with " + PlayerData.inst.linkedTerminalBotnet + " systems.\nAwaiting botnet instructions.";
                        PlayerData.inst.linkedTerminalBotnet++;

                        UIManager.inst.terminal_targetTerm.trojans.Add(MapManager.inst.hackDatabase.dict["Trojan(Botnet)"]);

                        return print;

                    }
                    else if (parsedName.Contains("Detonate"))
                    {
                        // Rigs all nearby potentially explosive machines to detonate when passed by a hostile Complex 0b10 combat robot.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Broadcast"))
                    {
                        // Reports the position and composition of squads emerging from any garrison.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Decoy"))
                    {
                        // Redirects the next squad emerging from this garrison away from its intended target.
                        // No effect on prototypes, though if dispatched from here they do not affect the installed trojan.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Redirect"))
                    {
                        // Redirects all squads emerging from this garrison away from their intended targets,
                        // though the chance for its presence to be detected increases by 25% each time it takes effect.
                        // No effect on prototypes, though if dispatched from there they do not affect the installed trojan.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Reprogram"))
                    {
                        // Reprograms the next squad emerging from this garrison.
                        // No effect on prototypes, though if dispatched from there they do not affect the installed trojan.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Disrupt"))
                    {
                        // Disrupts targeting coordination signals among hostile Complex 0b10 combat robots, reducing their chance to hit targets by 10% while within a range of 10.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Fabnet"))
                    {
                        // Provides a cumulative 3% chance to temporarily trick any common 0b10 combat robot system encountered hostiles
                        // on the depth above this one into believing it is allied with Cogmind.
                        // After 10 turns the network will perform an automated quickboot to restore it to normal, a process which takes anywhere from 5 to 10 turns.
                        // The effect is less likely to occur on the depth above that (only 1% chance), and it is capped at 15%.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Haulers"))
                    {
                        // Reports in real time the position of all haulers across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Intercept"))
                    {
                        // Intercepts tactical planning and coordination transmissions,
                        // providing self and all controllable allies with a +15% accuracy bonus against hostile Complex 0b10 combat robots within a range of 20.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Liberate"))
                    {
                        // Frees any robots built by 0b10 at this location from central system control.
                        return "Trojan loaded successfully.\nTesting...\nReady to liberate!.";
                    }
                    else if (parsedName.Contains("Mask"))
                    {
                        // Prevents recyclers from collecting parts within a 31 x 31 zone.
                        return "Trojan loaded successfully.\nTesting...\nMasking routine running.";
                    }
                    else if (parsedName.Contains("Mechanics"))
                    {
                        // Reports in real time the position of all mechanics across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Monitor"))
                    {
                        // Reports every object inserted into any recycling unit by a registered recycler.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Operators"))
                    {
                        // Reports in real time the position of all operators across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nOperator tracking enabled and active.";
                    }
                    else if (parsedName.Contains("Prioritize"))
                    {
                        // Accelerates fabrication speed to 200% by forcing the network to allocate matter to this system first.
                        // (Effect also applies to builds already in progress.)
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Recyclers"))
                    {
                        // Reports in real time the position of all recyclers across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Redirect"))
                    {
                        // Redirects all squads emerging from this garrison away from their intended targets,
                        // though the chance for its presence to be detect increases by 25% each time it takes effect.
                        // No effect on prototypes, though if dispatched from there they do not affect the installed trojan.

                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Reject"))
                    {
                        // Ejects any object inserted into this recycling unit by a registered recycler.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Reprogram"))
                    {
                        // // Reprograms the next squad emerging from this garrison. No effect on prototypes, though if dispatched from here they do not affect the installed trojan.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Report"))
                    {
                        // hack
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Researchers"))
                    {
                        // Reports in real time the position of all researchers across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Restock"))
                    {
                        // Manipulates coupler status records, prompting a Programmer to come replace them.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Skim"))
                    {
                        // Opportunistically gathers and reports on potentially useful intel remotely extracted from vulnerable Complex 0b10 networks.
                        // Must be installed on multiple terminals before it can take effect.
                        // (After being installed on 5 terminals, gathers 10 pieces of intel over around 100 turns, then is purged from the system.)
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Track"))
                    {
                        // Reports in real time the position and type of all robots near this terminal.
                        // The range of this effect is dependent on the terminal level: 10 for Security Level 3, 8 on level 2 terminals, and 6 on all other terminals.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }
                    else if (parsedName.Contains("Watchers"))
                    {
                        // Reports in real time the position of all watchers across the entire floor.
                        return "Trojan loaded successfully.\nTesting...\nEjection routine running.";
                    }

                    break; // ---------- NOT DONE: TODO
                case TerminalCommandType.Force:
                    if (parsedName.Contains("Extract"))
                    {
                        return "";
                    }
                    else if (parsedName.Contains("Fedlink"))
                    {
                        // Allocates additional resources to the UFD reserves. Also, locks the machines and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Jam"))
                    {
                        // Seal this garrison's access door, preventing squad dispatches from this location and slowing extermination squad response times across the entire floor.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Overload"))
                    {
                        // Render the fabricator inoperable, but cause it to send high corruption-causing arcs of electromagnetic energy at nearby bots for a short while.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Override"))
                    {
                        // Reveals location of all exits in a Garrison and overrides the lockdown effect that prevents Cogmind from leaving the map.
                        // Does nothing if outside a garrison.
                        return "";
                    }
                    else if (parsedName.Contains("Recompile"))
                    {
                        // Ejects an Authchip from the fabricator with a type matching the currently loaded schematics.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                    }
                    else if (parsedName.Contains("Download"))
                    {
                        // Save the current schematic in the player's list of schematics.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Eject"))
                    {
                        // Eject all relay couplers installed in this garrison to the floor.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Extract"))
                    {
                        // Extracts a random number of parts schematics from the machine. Most schematics will be non-prototype.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Sabotage"))
                    {
                        // Attempt to cause a random explosive machine on the floor to detonate.
                        // This hack can succeed but fail to have any effect.
                        // Also, locks the machine and summons an investigations quad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Search"))
                    {
                        // Locate all nearby interactive machines. Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Scrapoids"))
                    {
                        // Summons 5 Scrapoids within around 20 turns of performing the hack while consuming 2 resources from the UFD reserves.
                        // The Scrapoids vary ased on depth. Also, locks the machines and summons an investigation squad like all force hacks.
                        return "";
                    }
                    else if (parsedName.Contains("Tunnel"))
                    {
                        // 
                        return "";
                    }
                    else if (parsedName.Contains("Patch"))
                    {
                        // Repairs multiple parts by 25 % integrity up to 50 % total integrity.
                        // Level 1 stations repair up to 3 parts, level 2 4 parts, and level 3 5 parts.
                        // Also, locks the machine and summons an investigation squad like all force hacks.
                        return "";
                    }
                    break;  // ---------- NOT DONE: TODO
                case TerminalCommandType.Submit: // - Used to store matter in the player's hideout Cache
                    // We will try to submit 100 at a time
                    int toRemove = 100;

                    // - First get the machine we are using
                    TerminalCustom terminal = null;
                    foreach (GameObject machine in MapManager.inst.machines_customTerminals)
                    {
                        if(machine.GetComponentInChildren<TerminalCustom>() && machine.GetComponentInChildren<TerminalCustom>().customType == CustomTerminalType.HideoutCache)
                        {
                            terminal = machine.GetComponentInChildren<TerminalCustom>();
                        }
                    }

                    // Add up all the matter the player currently has, including internal storage since we will take out of that first
                    int matter = PlayerData.inst.currentMatter;
                    int internalMatter = 0;

                    // - Collect up all items
                    List<Item> items = Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>());

                    // - Collect up all *matter* storage items.
                    List<Item> storage = new List<Item>();
                    foreach (var I in items)
                    {
                        foreach (var E in I.itemData.itemEffects)
                        {
                            if (E.internalStorageEffect.hasEffect && E.internalStorageEffect.internalStorageType == 0)
                            {
                                storage.Add(I);
                            }
                        }
                    }
                    // - Now find out how much is actually in these (if any)
                    foreach (var I in storage)
                    {
                        foreach (var E in I.itemData.itemEffects)
                        {
                            if (E.internalStorageEffect.hasEffect && E.internalStorageEffect.internalStorageType == 0)
                            {
                                if(I.storageAmount > 0)
                                {
                                    internalMatter += I.storageAmount;
                                }
                            }
                        }
                    }

                    // - So does the player even have any internal matter?
                    if(internalMatter > 0) // Yes!
                    {
                        foreach(var I in storage) // Go through each storage item and start taking
                        {
                            foreach (var E in I.itemData.itemEffects)
                            {
                                if (E.internalStorageEffect.hasEffect && E.internalStorageEffect.internalStorageType == 0 && I.storageAmount > 0)
                                {
                                    if (I.storageAmount < toRemove) // Not enough here to fulfill all
                                    {
                                        toRemove -= I.storageAmount; // Decrease value
                                        I.storageAmount = 0; // Set empty
                                    }
                                    else // Enough here, we can finish early
                                    {
                                        I.storageAmount -= toRemove;
                                        toRemove = 0;
                                        break; // Done!
                                    }
                                }
                            }
                        }
                    }
                    // - Any left to remove?
                    if(toRemove > 0) // Yes, keep going, this time using the basic storage
                    {
                        if(matter >= toRemove) // Enough to clear it all up
                        {
                            PlayerData.inst.currentMatter -= toRemove; // Update player value
                            toRemove = 0; // Finish up
                        }
                        else // Can't do it all
                        {
                            toRemove -= PlayerData.inst.currentMatter;
                            PlayerData.inst.currentMatter = 0;
                        }
                    }

                    // So how much did we actually add?
                    int added = 100 - toRemove;

                    // Change the value in the cache, and leave a message
                    terminal.storedMatter += added;

                    return $"Submitted {added} matter to be stored in local cache.";
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

    public static void TraceHacking(InteractableMachine machine, float levelOfFailure = 0f)
    {
        #region From the Manual
        /*
        The system responds in three phases:

        * Detection: 
        * Initially your presence is unknown, but with each subsequent action there is a chance your activity will be detected. 
        * Detection happens more quickly at higher security machines, and becomes more likely with each failed hack, 
        * but can be mitigated by defensive hackware. 
        * Accessing the same machine more than once also increases the chance of detection, if it was previously hacked but not traced.

        * Tracing: 
        * As soon as suspicious activity is detected, the system attempts to locate it. 
        * Failing hacks increases the speed of the trace, more quickly for worse failures. 
        * If a session is terminated while a trace is in progress, that trace will resume from where it left off if a connection is reestablished.

        * Feedback: 
        * Once traced, the system is permanently locked and may attempt to counterattack the source of the unauthorized access, 
        * which either causes system corruption or disables connected hackware.

        Note that successful hacks (especially those which were difficult to pull off) have a chance to cause an increase in local alert level, 
        though this result is less likely while using defensive hackware.
         */
        #endregion

        float detectionChance = machine.detectionChance;
        float traceProgress = machine.traceProgress;
        bool detected = machine.detected;
        int timesAccessed = machine.timesAccessed;

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

        int secLvl = machine.secLvl;

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
                AudioManager.inst.CreateTempClip(UIManager.inst.transform.position, AudioManager.inst.dict_ui["HACK_DETECTED"]); // UI - HACK_DETECTED
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

                // Also increase chance if this machine was interacted with before
                if(timesAccessed > 1 && detectionChance > GlobalSettings.inst.defaultHackingDetectionChance)
                {
                    // Increase detection chance
                    detectionChance += 0.2f;
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

            // Include a "bonus" for how badly you failed
            float failureBonus = 0f;
            if(levelOfFailure > 0)
            {
                // The idea here isn't to just add more trace percentage based on what we calculated so far since that seems like too much.
                // Here we are just gonna take a static amount of 35%, and take a portion of that based on the level of failure (which is usually around 50-60%).
                // This will come out to some value around 17% (more or less), which seems fair.
                failureBonus = GlobalSettings.inst.hackingLevelOfFailureBaseBonus * levelOfFailure;
                traceProgress += failureBonus;
            }

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

        machine.detectionChance = detectionChance;
        machine.detected = detected;
        machine.traceProgress = traceProgress;
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

    public static void TerminalFailConsequence(string machineName) // TODO
    {
        // "Getting traced may result in receiving Corruption, your Hackware breaking, but most commonly an Investigation Squad."

        // Random chance to do the following:
        // -Give corruption (see: https://noemica.github.io/cog-minder/wiki/Corruption
        // -Corrupt an equipped part
        // -Nothing

        float rand = Random.Range(0f, 1f);
        if(rand <= 0.2f) // [20%] I don't know the true values for this, figure that out later.
        {
            // Decide what to do





            // Do a printout for what we did
            string failstring = "ALERT: Suspicious activity at " + machineName + ". Dispatching Investigation squad.";
            UIManager.inst.CreateNewLogMessage(failstring, UIManager.inst.complexWhite, UIManager.inst.inactiveGray, false, true);

            // We now need to display this message in the terminal's results area aswell (without the header though).
            UIManager.inst.Terminal_CreateResult($"[{failstring}]", UIManager.inst.inactiveGray, "", false);
        }
        
    }

    /// <summary>
    /// Converts a HackObject's name to a string of what should be printed out in the /RESULTS/ window.
    /// </summary>
    /// <param name="hack">The HackObject we want to display the name of.</param>
    /// <param name="fill">[OPTIONAL] Any fill text that should be inserted between (    ).</param>
    /// <returns>A string which can be printed out to the /RESULTS/ window.</returns>
    public static string HackToPrintout(HackObject hack, string fill = "")
    {
        string hackName = hack.name;

        // Check for the presence of "[" and "]"
        bool removed = false;
        int bracketStartIndex = hackName.IndexOf('[');
        int bracketEndIndex = hackName.IndexOf(']');
        if (bracketStartIndex != -1 && bracketEndIndex != -1 && bracketEndIndex > bracketStartIndex)
        {
            // Remove the brackets and their content
            hackName = hackName.Remove(bracketStartIndex, bracketEndIndex - bracketStartIndex + 1);

            removed = true;
        }

        // If we removed something from [   ] and have a fill, we should put that in now
        if (removed && fill != "")
        {
            string[] split = hackName.Split("(");
            hackName = split[0] + "(" + fill + ")";
        }

        return hackName;
    }

    /// <summary>
    /// Given a machine type, returns what color that machine and its parts should be colored. Color pallet is inside `GameManager.cs` at the bottom.
    /// </summary>
    /// <param name="type">The type of machine this is.</param>
    /// <returns>A color this machine should appear as (with a base color of white).</returns>
    public static Color MachineColor(MachineType type)
    {
        switch (type)
        {
            case MachineType.Fabricator:
                return GameManager.inst.colors.machine_fabricator;
            case MachineType.Garrison:
                return GameManager.inst.colors.machine_garrison;
            case MachineType.Recycling:
                return GameManager.inst.colors.machine_recycler;
            case MachineType.RepairStation:
                return GameManager.inst.colors.machine_repairbay;
            case MachineType.Scanalyzer:
                return GameManager.inst.colors.machine_scanalyzer;
            case MachineType.Terminal:
                return GameManager.inst.colors.machine_terminal;
            case MachineType.CustomTerminal:
                return GameManager.inst.colors.machine_customterminal;
            case MachineType.DoorTerminal:
                return GameManager.inst.colors.machine_customterminal;
            case MachineType.Misc:
                return GameManager.inst.colors.machine_static;
            default:
                return GameManager.inst.colors.machine_static;
        }
    }
    #endregion

    #region Find & Get
    /// <summary>
    /// Used for setting the byte flag in MapManager's `pathdata`.
    /// </summary>
    /// <param name="type">The type of tile.</param>
    /// <returns>The byte which should be set here to be used in pathing.</returns>
    public static byte TileObstructionType(TileType type)
    {
        // Wall[1], bots[2], machines[3], traps[4]
        switch (type)
        {
            case TileType.Floor:
                return 0;
            case TileType.Wall:
                return 1;
            case TileType.Door:
                return 0;
            case TileType.Machine:
                return 3;
            case TileType.Exit:
                return 0;
            case TileType.Phasewall:
                return 0;
            case TileType.Trap:
                return 4;
            case TileType.Default:
                return 0;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Attempts to find and return the current amount of stored matter in the player's hideout cache. If unsuccessful, returns -1.
    /// </summary>
    /// <returns>If successful, returns the true amount of stored matter. If failed, returns -1.</returns>
    public static int TryFindCachedMatter()
    {
        int value = -1;

        // We first need to find the physical cache machine, which is a Custom Terminal
        foreach (var machines in MapManager.inst.machines_customTerminals)
        {
            if(machines.GetComponentInChildren<TerminalCustom>() && machines.GetComponentInChildren<TerminalCustom>().customType == CustomTerminalType.HideoutCache)
            {
                value = machines.GetComponentInChildren<TerminalCustom>().storedMatter; // Get the value
            }
        }

        return value;
    }

    /// <summary>
    /// Attempts to find and set the current amount of stored matter in the player's hideout cache from save data.
    /// </summary>
    public static void TrySetCachedMatter(int value)
    {
        // We first need to find the physical cache machine, which is a Custom Terminal
        foreach (var machines in MapManager.inst.machines_customTerminals)
        {
            if (machines.GetComponentInChildren<TerminalCustom>() && machines.GetComponentInChildren<TerminalCustom>().customType == CustomTerminalType.HideoutCache)
            {
                machines.GetComponentInChildren<TerminalCustom>().storedMatter = value; // Set the value
            }
        }
    }

    /// <summary>
    /// Attempts to find the % coverage value for the specified item belonging to the player.
    /// </summary>
    /// <param name="item">The item we want the coverage value for.</param>
    /// <returns>A % float value representing this items current coverage.</returns>
    public static float FindPercentCoverageFor(Item item)
    {
        // Gather up all player items (not in inventory though)
        List<Item> items = Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>());

        int coreExposure = PlayerData.inst.currentCoreExposure;

        /*
        Example 1: Cogmind's core has an exposure of 100. Say you equip only one part, a weapon which also has a coverage of 100. 
        Their total value is 200 (100+100), so if you are hit by a projectile, each one has a 50% (100/200) chance to be hit.

        Example 2: You have the following parts attached:
          Ion Engine (60)
          Light Treads (120)
          Light Treads (120)
          Medium Laser (60)
          Assault Rifle (100)
        With your core (100), the total is 560, so the chance to hit each location is:
          Ion Engine: 60/560=10.7%
          Light Treads: 120/560=21.4% (each)
          Medium Laser: 60/560=10.7%
          Assault Rifle: 100/560=17.9%
          Core: 100/560=17.9%
         */

        int totalExposure = 0;

        foreach (var I in items)
        {
            if (I.state)
            {
                totalExposure += I.itemData.coverage;

                // All armor and heavy treads have double coverage in *Siege Mode*
                if (PlayerData.inst.timeTilSiege == 0)
                {
                    if (I.itemData.type == ItemType.Armor || I.itemData.propulsion[0].canSiege != 0)
                    {
                        totalExposure += I.itemData.coverage;
                    }
                }
            }
        }
        totalExposure += coreExposure;

        // Then take the item, and divide its coverage value by the total value to find our result
        return (float)item.itemData.coverage / (float)totalExposure;
    }

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

    public static BotRelation RelationToTrap(Actor bot, WorldTile trap)
    {
        BotRelation relationToTarget = BotRelation.Neutral;

        BotAlignment trapAlignment = trap.trap_alignment;
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
                if (item.rating == tier && item.knowByPlayer) // Needs to be known
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
    /// Finds a random item (in the database) of a specified tier (1-10) and fit into the specified slot.
    /// </summary>
    /// <param name="tier">The specified tier to search for. 1 to 10</param>
    /// <param name="slot">The specific slot this item needs to fit in.</param>
    /// <param name="isUnknown">Whether the item shouldn't be known to the player or not (aka a Prototype).</param>
    /// <returns></returns>
    public static ItemObject FindItemOfTierAndSlot(int tier, ItemSlot slot, bool isUnknown = true)
    {
        List<ItemObject> items = new List<ItemObject>();

        foreach (ItemObject item in MapManager.inst.itemDatabase.Items)
        {
            if (isUnknown)
            {
                if (item.rating == tier && item.slot == slot && !item.knowByPlayer) // Prototypes only
                {
                    items.Add(item);
                }
            }
            else
            {
                if (item.rating == tier && item.slot == slot)
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
    /// Finds a random item (in the database) of a specified tier (1-10) and fit into the specified slot.
    /// </summary>
    /// <param name="tier">The specified tier to search for. 1 to 10</param>
    /// <param name="type">The specific type this item should be.</param>
    /// <param name="isUnknown">Whether the item shouldn't be known to the player or not (aka a Prototype).</param>
    /// <returns></returns>
    public static ItemObject FindItemOfTierAndType(int tier, ItemType type, bool isUnknown = true)
    {
        List<ItemObject> items = new List<ItemObject>();

        foreach (ItemObject item in MapManager.inst.itemDatabase.Items)
        {
            if (isUnknown)
            {
                if (item.rating == tier && item.type == type && !item.knowByPlayer) // Prototypes only
                {
                    items.Add(item);
                }
            }
            else
            {
                if (item.rating == tier && item.type == type)
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

        foreach (Item item in Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>()))
        {
            if (item != null && item.Id >= 0 && item.itemData.itemEffects.Count > 0)
            {
                foreach (var E in item.itemData.itemEffects)
                {
                    if (E.hackBonuses.hasSystemShieldBonus && item.state && item.disabledTimer <= 0)
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

    /// <summary>
    /// Given a string, returns a tuple (HackObject, TerminalCommand) if that string matches a valid hack.
    /// </summary>
    /// <param name="str">The input string which should be the specific name of a hack.</param>
    /// <returns>A tuple (HackObject, TerminalCommand) of the hack if one was found. Null on failure.</returns>
    public static (HackObject, TerminalCommand) ParseHackString(string str)
    {
        HackObject hack = null;
        TerminalCommand command = new TerminalCommand("x", "x", TerminalCommandType.NONE);

        #region Validity Check
        // Check to see if this is actually a valid hack and not gibberish
        bool valid = false;
        foreach (HackObject h in MapManager.inst.hackDatabase.Hack)
        {
            if(h.name.ToLower() == str.ToLower())
            {
                valid = true;
                break;
            }
        }

        if (!valid)
        {
            return (null, null);
        }
        #endregion

        if (str.Contains("("))
        {
            // command(inside)
            string left = HF.GetLeftSubstring(str); // command
            string right = HF.GetRightSubstring(str); // inside
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
             * -Download(N,R,S), Enumerate, Index, Inventory, Layout, Recall, Schematic(Bot&Parts), Traps(D,L,R), Query, Trojan, Force
             * -Load(Indir), Store(Matter)
             */

            string c = left.ToLower();

            if (c == "load")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.dict["Load([Part])"];
                }
                else
                {
                    hack = MapManager.inst.hackDatabase.dict["Load(Indir)"];
                }
            }
            else if (c == "network")
            {
                hack = MapManager.inst.hackDatabase.dict["Network(Status)"];
            }
            else if (c == "recycle")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recycle([Part Name])"];
                }
                else if (right.ToLower() == "process")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recycle(Process)"];
                }
                else if (right.ToLower() == "report")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recycle(Report)"];
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
                    hack = MapManager.inst.hackDatabase.dict["Retrieve(Components)"];
                }
                else if (right.ToLower() == "matter")
                {
                    hack = MapManager.inst.hackDatabase.dict["Retrieve(Matter)"];
                }
            }
            else if (c == "scan")
            {
                if (item != null)
                {
                    hack = MapManager.inst.hackDatabase.dict["Scan([Part Name])"];
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
                    hack = MapManager.inst.hackDatabase.dict["Insert([Part Name])"];
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
                    hack = MapManager.inst.hackDatabase.dict["Access(Branch)"];
                }
                else if (right.ToLower() == "emergency")
                {
                    hack = MapManager.inst.hackDatabase.dict["Access(Emergency)"];
                }
                else if (right.ToLower() == "main")
                {
                    hack = MapManager.inst.hackDatabase.dict["Access(Main)"];
                }
            }
            else if (c == "alert")
            {
                if (right.ToLower() == "check")
                {
                    hack = MapManager.inst.hackDatabase.dict["Alert(Check)"];
                }
                else if (right.ToLower() == "purge")
                {
                    hack = MapManager.inst.hackDatabase.dict["Alert(Purge)"];
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
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 1"];
                            break;
                        case 2:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 2"];
                            break;
                        case 3:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 3"];
                            break;
                        case 4:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 4"];
                            break;
                        case 5:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 5"];
                            break;
                        case 6:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 6"];
                            break;
                        case 7:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 7"];
                            break;
                        case 8:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 8"];
                            break;
                        case 9:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 9"];
                            break;
                        case 10:
                            hack = MapManager.inst.hackDatabase.dict["Analysis([Bot Name]) - Tier 10"];
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
                    hack = MapManager.inst.hackDatabase.dict["Download(Navigation)"];
                }
                else if (right.ToLower() == "registry")
                {
                    hack = MapManager.inst.hackDatabase.dict["Download(Registry)"];
                }
                else if (right.ToLower() == "security")
                {
                    hack = MapManager.inst.hackDatabase.dict["Download(Security)"];
                }
            }
            else if (c == "enumerate")
            {
                if (right.ToLower() == "assaults")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Assaults)"];
                }
                else if (right.ToLower() == "coupling")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Coupling)"];
                }
                else if (right.ToLower() == "exterminations")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Exterminations)"];
                }
                else if (right.ToLower() == "garrison")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Garrison)"];
                }
                else if (right.ToLower() == "guards")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Guards)"];
                }
                else if (right.ToLower() == "intercept")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Intercept)"];
                }
                else if (right.ToLower() == "investigations")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Investigations)"];
                }
                else if (right.ToLower() == "maintenance")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Maintenance)"];
                }
                else if (right.ToLower() == "patrols")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Patrols)"];
                }
                else if (right.ToLower() == "reinforcements")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Reinforcements)"];
                }
                else if (right.ToLower() == "squads")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Squads)"];
                }
                else if (right.ToLower() == "surveillance")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Surveillance)"];
                }
                else if (right.ToLower() == "transport")
                {
                    hack = MapManager.inst.hackDatabase.dict["Enumerate(Transport)"];
                }
            }
            else if (c == "index")
            {
                if (right.ToLower() == "fabricators")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Fabricators)"];
                }
                else if (right.ToLower() == "garrisons")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Garrisons)"];
                }
                else if (right.ToLower() == "machines")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Machines)"];
                }
                else if (right.ToLower() == "recycling units")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Recycling Units)"];
                }
                else if (right.ToLower() == "repair stations")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Repair Stations)"];
                }
                else if (right.ToLower() == "scanalyzers")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Scanalyzers)"];
                }
                else if (right.ToLower() == "terminals")
                {
                    hack = MapManager.inst.hackDatabase.dict["Index(Terminals)"];
                }
            }
            else if (c == "inventory")
            {
                if (right.ToLower() == "component")
                {
                    hack = MapManager.inst.hackDatabase.dict["Inventory(Component)"];
                }
                else if (right.ToLower() == "prototype")
                {
                    hack = MapManager.inst.hackDatabase.dict["Inventory(Prototype)"];
                }
            }
            else if (c == "layout")
            {
                hack = MapManager.inst.hackDatabase.dict["Layout(Zone)"];
            }
            else if (c == "recall")
            {
                if (right.ToLower() == "assault")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Assault)"];
                }
                else if (right.ToLower() == "extermination")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Extermination)"];
                }
                else if (right.ToLower() == "investigation")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Investigation)"];
                }
                else if (right.ToLower() == "reinforcements")
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Reinforcements)"];
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
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 1"];
                            break;
                        case 2:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 2"];
                            break;
                        case 3:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 3"];
                            break;
                        case 4:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 4"];
                            break;
                        case 5:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 5"];
                            break;
                        case 6:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 6"];
                            break;
                        case 7:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 7"];
                            break;
                        case 8:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 8"];
                            break;
                        case 9:
                            hack = MapManager.inst.hackDatabase.dict["Schematic([Bot Name]) - Tier 9"];
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
                    hack = MapManager.inst.hackDatabase.dict["Traps(Disarm)"];
                }
                else if (right.ToLower() == "locate")
                {
                    hack = MapManager.inst.hackDatabase.dict["Traps(Locate)"];
                }
                else if (right.ToLower() == "reprogram")
                {
                    hack = MapManager.inst.hackDatabase.dict["Traps(Reprogram)"];
                }
            }
            else if (c == "query")
            {
                hack = MapManager.inst.hackDatabase.dict["Query"];
            }
            else if (c == "trojan")
            {
                if (right.ToLower() == "assimilate")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Assimilate)"];
                }
                else if (right.ToLower() == "botnet")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Botnet)"];
                }
                else if (right.ToLower() == "broadcast")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Broadcast)"];
                }
                else if (right.ToLower() == "decoy")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Decoy)"];
                }
                else if (right.ToLower() == "detonate")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Detonate)"];
                }
                else if (right.ToLower() == "redirect")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Redirect)"];
                }
                else if (right.ToLower() == "reprogram")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Reprogram)"];
                }
                else if (right.ToLower() == "track")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Track)"];
                }
                else if (right.ToLower() == "disrupt")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Disrupt)"];
                }
                else if (right.ToLower() == "fabnet")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Fabnet)"];
                }
                else if (right.ToLower() == "haulers")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Haulers)"];
                }
                else if (right.ToLower() == "intercept")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Intercept)"];
                }
                else if (right.ToLower() == "liberate")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Liberate)"];
                }
                else if (right.ToLower() == "mask")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Mask)"];
                }
                else if (right.ToLower() == "mechanics")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Mechanics)"];
                }
                else if (right.ToLower() == "monitor")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Monitor)"];
                }
                else if (right.ToLower() == "operators")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Operators)"];
                }
                else if (right.ToLower() == "prioritize")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Prioritize)"];
                }
                else if (right.ToLower() == "recyclers")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Recyclers)"];
                }
                else if (right.ToLower() == "reject")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Reject)"];
                }
                else if (right.ToLower() == "report")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Report)"];
                }
                else if (right.ToLower() == "researchers")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Researchers)"];
                }
                else if (right.ToLower() == "restock")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Restock)"];
                }
                else if (right.ToLower() == "skim")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Skim)"];
                }
                else if (right.ToLower() == "watchers")
                {
                    hack = MapManager.inst.hackDatabase.dict["Trojan(Watchers)"];
                }
            }
            else if (c == "force")
            {
                if (right.ToLower() == "extract")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Extract)"];
                }
                else if (right.ToLower() == "jam")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Jam)"];
                }
                else if (right.ToLower() == "overload")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Overload)"];
                }
                else if (right.ToLower() == "download")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Download)"];
                }
                else if (right.ToLower() == "eject")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Eject)"];
                }
                else if (right.ToLower() == "sabotage")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Sabotage)"];
                }
                else if (right.ToLower() == "search")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Search)"];
                }
                else if (right.ToLower() == "tunnel")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Tunnel)"];
                }
                else if (right.ToLower() == "patch")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Patch)"];
                }
                else if (right.ToLower() == "recompile")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Recompile)"];
                }
                else if (right.ToLower() == "fedlink")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Fedlink)"];
                }
                else if (right.ToLower() == "scrapoids")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Scrapoids)"];
                }
                else if (right.ToLower() == "override")
                {
                    hack = MapManager.inst.hackDatabase.dict["Force(Override)"];
                }
            }
            else if (c == "store")
            {
                hack = MapManager.inst.hackDatabase.dict["Store(Matter)"];
            }
        }
        else
        {
            /* The following hacks are in this category:
             * -Couplers, Seal, Unlock, Manifests, Prototypes, Open (h,m,l)
             * and
             * -Build (Fabricator), Refit (Repair Station), Repair (Repair Station), Scanalyze (Scanalyzer), 
             */

            // command(inside)
            string left = HF.GetLeftSubstring(str); // command
            string right = HF.GetRightSubstring(str); // inside)
            //right = right.Substring(0, right.Length - 1); // Remove the ")", now: inside
            right = right.ToLower();

            string c = left;

            if (c == "couplers")
            {
                hack = MapManager.inst.hackDatabase.dict["Couplers"];
            }
            else if (c == "seal")
            {
                hack = MapManager.inst.hackDatabase.dict["Seal"];
            }
            else if (c == "unlock")
            {
                hack = MapManager.inst.hackDatabase.dict["Unlock"];
            }
            else if (c == "manifests")
            {
                hack = MapManager.inst.hackDatabase.dict["Manifests"];
            }
            else if (c == "prototypes")
            {
                hack = MapManager.inst.hackDatabase.dict["Prototypes"];
            }
            else if (c == "open")
            {
                hack = MapManager.inst.hackDatabase.dict["Open - Storage High Value"]; // not sure what to do here since there are 3 of these (High, Medium, Low)
            }
            else // Interacting with some kind of item or bot
            {
                // Now part 2
                ItemObject item = GetItemByString(right);
                BotObject bot = GetBotByString(right);
                if (c == "build")
                {
                    if (bot != null) // - Build Robot
                    {
                        int rating = bot.rating;
                        switch (rating)
                        {
                            case 1:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 1"];
                                break;
                            case 2:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 2"];
                                break;
                            case 3:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 3"];
                                break;
                            case 4:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 4"];
                                break;
                            case 5:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 5"];
                                break;
                            case 6:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 6"];
                                break;
                            case 7:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 7"];
                                break;
                            case 8:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 8"];
                                break;
                            case 9:
                                hack = MapManager.inst.hackDatabase.dict["Build - Robot Tier 9"];
                                break;
                        }

                    }
                    else if (item != null) // Build - Item
                    {
                        int rating = item.rating;
                        bool starred = item.star;
                        switch (rating)
                        {
                            case 1:
                                hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 1"];
                                break;
                            case 2:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 2"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 2P"];
                                }
                                break;
                            case 3:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 3"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 3P"];
                                }
                                break;
                            case 4:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 4"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 4P"];
                                }
                                break;
                            case 5:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 5"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 5P"];
                                }
                                break;
                            case 6:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 6"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 6P"];
                                }
                                break;
                            case 7:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 7"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 7P"];
                                }
                                break;
                            case 8:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 8"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 8P"];
                                }
                                break;
                            case 9:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 9"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Build - Part Rating 9P"];
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
                    hack = MapManager.inst.hackDatabase.dict["Refit"];
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
                                hack = MapManager.inst.hackDatabase.dict["Repair - Rating 1"];
                                break;
                            case 2:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 2"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 2P"];
                                }
                                break;
                            case 3:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 3"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 3P"];
                                }
                                break;
                            case 4:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 4"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 4P"];
                                }
                                break;
                            case 5:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 5"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 5P"];
                                }
                                break;
                            case 6:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 6"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 6P"];
                                }
                                break;
                            case 7:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 7"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 7P"];
                                }
                                break;
                            case 8:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 8"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 8P"];
                                }
                                break;
                            case 9:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 9"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Repair - Rating 9P"];
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
                                hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 1"];
                                break;
                            case 2:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 2"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 2P"];
                                }
                                break;
                            case 3:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 3"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 3P"];
                                }
                                break;
                            case 4:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 4"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 4P"];
                                }
                                break;
                            case 5:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 5"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 5P"];
                                }
                                break;
                            case 6:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 6"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 6P"];
                                }
                                break;
                            case 7:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 7"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 7P"];
                                }
                                break;
                            case 8:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 8"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 8P"];
                                }
                                break;
                            case 9:
                                if (!starred)
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 9"];
                                }
                                else
                                {
                                    hack = MapManager.inst.hackDatabase.dict["Scanalyze - Rating 9P"];
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
        return MapManager.inst.itemDatabase.dict.ContainsKey(str) ? MapManager.inst.itemDatabase.dict[str] : null;
    }

    public static TileObject GetTileByString(string str)
    {
        return MapManager.inst.tileDatabase.dict.ContainsKey(str) ? MapManager.inst.tileDatabase.dict[str] : null;
    }

    public static BotObject GetBotByString(string str)
    {
        return MapManager.inst.botDatabase.dict.ContainsKey(str) ? MapManager.inst.botDatabase.dict[str] : null;
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
    /// A generic method of setting a gameObject's visibility
    /// </summary>
    public static void SetGenericTileVis(GameObject target, byte vis)
    {
        // Identify what we need to change then change it

        if (target.GetComponent<TileBlock>()) // Walls, Floors, & Door tiles
        {
            target.GetComponent<TileBlock>().UpdateVis(vis);
        }
        else if (target.GetComponentInChildren<TileBlock>())
        {
            target.GetComponentInChildren<TileBlock>().UpdateVis(vis);
        }

        if (target.GetComponent<MachinePart>()) // Machines
        {
            target.GetComponent<MachinePart>().UpdateVis(vis);
        }
        else if (target.GetComponentInChildren<MachinePart>())
        {
            target.GetComponentInChildren<MachinePart>().UpdateVis(vis);
        }

        // Bots
        if (target.GetComponent<Actor>() && target != PlayerData.inst.gameObject)
        {
            target.GetComponent<Actor>().UpdateVis(vis);
        }
        else if (target.GetComponentInChildren<Actor>() && target != PlayerData.inst.gameObject)
        {
            target.GetComponentInChildren<Actor>().UpdateVis(vis);
        }

        // Access
        if (target.GetComponent<AccessObject>())
        {
            target.GetComponent<AccessObject>().UpdateVis(vis);
        }
        else if (target.GetComponentInChildren<AccessObject>())
        {
            target.GetComponentInChildren<AccessObject>().UpdateVis(vis);
        }

        // Items
        if (target.GetComponent<Part>())
        {
            target.GetComponent<Part>().UpdateVis(vis);
        }
        else if (target.GetComponentInChildren<Part>())
        {
            target.GetComponentInChildren<Part>().UpdateVis(vis);
        }
    }

    /// <summary>
    /// Finds *VALID* neighbors given a current position on a grid.
    /// </summary>
    /// <param name="X">Current X position on the grid.</param>
    /// <param name="Y">Current Y position on the grid.</param>
    /// <returns>Returns a list of *VALID* neighbors that exist.</returns>
    public static List<WorldTile> FindNeighbors(int X, int Y)
    {
        List<WorldTile> neighbors = new List<WorldTile>();

        // We want to include diagonals into this.
        if (X < MapManager.inst.mapsize.x - 1) // [ RIGHT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X + 1, Y]);
        }
        if (X > 0) // [ LEFT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X - 1, Y]);
        }
        if (Y < MapManager.inst.mapsize.y - 1) // [ UP ]
        {
            neighbors.Add(MapManager.inst.mapdata[X, Y + 1]);
        }
        if (Y > 0) // [ DOWN ]
        {
            neighbors.Add(MapManager.inst.mapdata[X, Y - 1]);
        }
        // -- 
        // Diagonals
        // --
        if (X < MapManager.inst.mapsize.x - 1 && Y < MapManager.inst.mapsize.y - 1) // [ UP-RIGHT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X + 1, Y + 1]);
        }
        if (Y < MapManager.inst.mapsize.y - 1 && X > 0) // [ UP-LEFT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X - 1, Y + 1]);
        }
        if (Y > 0 && X > 0) // [ DOWN-LEFT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X - 1, Y - 1]);
        }
        if (Y > 0 && X < MapManager.inst.mapsize.x - 1) // [ DOWN-RIGHT ]
        {
            neighbors.Add(MapManager.inst.mapdata[X + 1, Y - 1]);
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
    /// <param name="ignoreFloorItems">Do we care about an item being on the floor already? (Used for bot placement)</param>
    /// <returns>A free position (Vector2Int) that has been found.</returns>
    public static Vector2Int LocateFreeSpace(Vector2Int center, bool ignoreFloorItems = false)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(center);

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            if (IsOpenSpaceForItem(currentPos, true, false))
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

    /// <summary>
    /// Is the tile at this position open for an item to be placed here?
    /// </summary>
    /// <param name="position">The location to try and place the item</param>
    /// <param name="ignoreFloorItems">If true, we don't care if there is already a part on the floor at this position (False by default).</param>
    /// <param name="ignoreBots">If true, we don't care if there is a bot at this position (False by default).</param>
    /// <returns>True/False if it is possible to place an item item.</returns>
    public static bool IsOpenSpaceForItem(Vector2Int position, bool ignoreBots = false, bool ignoreFloorItems = false)
    {
        WorldTile tile = MapManager.inst.mapdata[position.x, position.y];
        byte pathdata = MapManager.inst.pathdata[position.x, position.y];

        bool ret = true;

        ret = HF.IsUnoccupiedTile(tile);

        if (ignoreBots == false && pathdata == 4)
            ret = false;

        if (ignoreFloorItems == false && InventoryControl.inst.worldItems.ContainsKey(position))
            ret = false;

        return ret;
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

    /// <summary>
    /// Given an item, will attempt to find that specific item within one of the separate player Inventories. Will return that inventory when found.
    /// </summary>
    /// <param name="item">An item to search for that is held by the player.</param>
    /// <returns>An InventoryObject which the item being searched for is inside of.</returns>
    public static InventoryObject FindPlayerInventoryFromItem(Item item)
    {
        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_power.Container.Items.ToList())
        {
            if (I != null && I.item != null && I.item.Id >= 0 && !I.item.isDuplicate)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>().inv_power;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
        {
            if (I != null && I.item != null && I.item.Id >= 0 && !I.item.isDuplicate)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>().inv_propulsion;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items.ToList())
        {
            if (I != null && I.item != null && I.item.Id >= 0 && !I.item.isDuplicate)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>().inv_utility;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items.ToList())
        {
            if (I != null && I.item != null && I.item.Id >= 0 && !I.item.isDuplicate)
            {
                if (I.item == item)
                {
                    return PlayerData.inst.GetComponent<PartInventory>().inv_weapon;
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

    /// <summary>
    /// Randomly select a part the player has equipped. Returns an InvDisplayItem.
    /// </summary>
    /// <returns>Returns an InvDisplayItem that's currently on the UI.</returns>
    public static InvDisplayItem GetRandomPlayerPart()
    {
        List<InvDisplayItem> valids = new List<InvDisplayItem>();

        foreach (var I in InventoryControl.inst.interfaces)
        {
            if (I.GetComponent<DynamicInterface>())
            {
                foreach (KeyValuePair<GameObject, InventorySlot> S in I.GetComponent<DynamicInterface>().slotsOnInterface)
                {
                    InvDisplayItem idi = S.Key.GetComponent<InvDisplayItem>();

                    if(idi.item != null && idi.item.Id >= 0)
                    {
                        valids.Add(idi);
                    }
                }
            }
        }

        if(valids.Count > 0)
        {
            return valids[Random.Range(0, valids.Count - 1)];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to get a specific items `InvDisplayItem` from the UI.
    /// </summary>
    /// <param name="item">The Item (Part) we want to get the related IDI from.</param>
    /// <returns>The InvDisplayItem belonging to the input item.</returns>
    public static InvDisplayItem GetInvDisplayItemFromPart(Item item)
    {
        foreach (var I in InventoryControl.inst.interfaces)
        {
            if (I.GetComponent<DynamicInterface>())
            {
                foreach (KeyValuePair<GameObject, InventorySlot> S in I.GetComponent<DynamicInterface>().slotsOnInterface)
                {
                    InvDisplayItem idi = S.Key.GetComponent<InvDisplayItem>();

                    if (idi.item != null && idi.item == item)
                    {
                        return idi;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to find a current equipped part (InvDisplayItem) by the name being displayed.
    /// </summary>
    /// <param name="name">The display name to look for.</param>
    /// <returns>An InvDisplayItem that matches the name requested.</returns>
    public static InvDisplayItem GetInvDisplayItemByName(string name)
    {
        foreach (var I in InventoryControl.inst.interfaces)
        {
            if (I.GetComponent<DynamicInterface>())
            {
                foreach (KeyValuePair<GameObject, InventorySlot> S in I.GetComponent<DynamicInterface>().slotsOnInterface)
                {
                    InvDisplayItem idi = S.Key.GetComponent<InvDisplayItem>();

                    if (idi.item != null && idi.nameUnmodified == name)
                    {
                        return idi;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Given a specific location, will attempt to return a list of valid locations (Vector2) to move to.
    /// </summary>
    /// <param name="pos">A position on the grid we want to move FROM.</param>
    /// <returns>A list of neighboring positions where it is valid to move to.</returns>
    public static List<Vector2> GetValidMovementLocations(Vector2 pos)
    {
        List<Vector2> valids = new List<Vector2>();

        // Get neighbors
        List<WorldTile> neighbors = HF.FindNeighbors((int)pos.x, (int)pos.y);

        foreach (var N in neighbors)
        {
            if (HF.IsUnoccupiedTile(N))
            {
                valids.Add(N.location);
            }
        }

        return valids;
    }

    /// <summary>
    /// Checks to see if a specified tile is unoccupied.
    /// </summary>
    /// <param name="tile">The specified tile to check.</param>
    /// <param name="ignoreBots">Should bots be considered as an occupier.</param>
    /// <returns>Returns TRUE if this tile is unoccupied.</returns>
    public static bool IsUnoccupiedTile(WorldTile tile, bool ignoreBots = false)
    {
        bool ret = false;

        TileType type = tile.tileInfo.type;
        switch (type)
        {
            case TileType.Floor: // No blocking at all (unless a bot is there)
                ret = true;
                if(!ignoreBots)
                    ret = MapManager.inst.pathdata[tile.location.x, tile.location.y] != 2; // Bot check
                break;
            case TileType.Wall: // Always blocks (unless destroyed)
                ret = tile.damaged;
                break;
            case TileType.Door: // No blocking at all (unless a bot is there)
                ret = true;
                if (!ignoreBots)
                    ret = MapManager.inst.pathdata[tile.location.x, tile.location.y] != 2; // Bot check
                break;
            case TileType.Machine: // Always blocks (unless destroyed)
                ret = tile.damaged;
                break;
            case TileType.Exit: // NEVER blocks
                return true;
            case TileType.Phasewall: // No blocking at all (unless a bot is there)
                ret = true;
                if (!ignoreBots)
                    ret = MapManager.inst.pathdata[tile.location.x, tile.location.y] != 2; // Bot check
                break;
            case TileType.Trap:
                ret = true;
                break;
            case TileType.Default:
                break;
            default:
                break;
        }

        return ret;
    }

    /// <summary>
    /// Is this tile able to prevent an explosion from passing throught it?
    /// </summary>
    /// <param name="tile">The specified tile to check.</param>
    /// <returns></returns>
    public static bool IsPermiableTile(WorldTile tile)
    {
        // NOTE: This function is essentially just a copy of the above `IsUnoccupiedTile` check.

        bool ret = false;

        TileType type = tile.tileInfo.type;
        switch (type)
        {
            case TileType.Floor:
                ret = true;
                break;
            case TileType.Wall:
                ret = tile.damaged;
                break;
            case TileType.Door:
                if (tile.damaged)
                {
                    ret = true;
                }
                else
                {
                    ret = tile.door_open;
                }
                break;
            case TileType.Machine:
                ret = tile.damaged;
                break;
            case TileType.Exit:
                return true;
            case TileType.Phasewall:
                ret = true;
                break;
            case TileType.Trap:
                ret = true;
                break;
            case TileType.Default:
                ret = true;
                break;
            default:
                ret = true;
                break;
        }

        return ret;
    }
    #endregion

    #region Floor Traps
    public static void AttemptTriggerTrap(WorldTile trap, Actor victim)
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

        if (Action.HasTreads(victim.GetComponent<Actor>()))
        {
            if (trap.trap_type == TrapType.Stasis)
            {
                triggerChance = 1f - 1f;
            }
            else
            {
                triggerChance = 1f;
            }
        }
        else if (Action.HasLegs(victim.GetComponent<Actor>()))
        {
            if (trap.trap_type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.75f;
            }
            else
            {
                triggerChance = 0.75f;
            }
        }
        else if (Action.HasWheels(victim.GetComponent<Actor>()))
        {
            if (trap.trap_type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.5f;
            }
            else
            {
                triggerChance = 0.5f;
            }
        }
        else if (Action.HasFlight(victim.GetComponent<Actor>()))
        {
            if (trap.trap_type == TrapType.Stasis)
            {
                triggerChance = 1f - 0.2f;
            }
            else
            {
                triggerChance = 0.2f;
            }
        }
        else if (Action.HasHover(victim.GetComponent<Actor>()))
        {
            if (trap.trap_type == TrapType.Stasis)
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
            if (trap.trap_type == TrapType.Stasis)
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
            trap.TripTrap(victim);
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
        foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items)
        {
            if (item.item.Id >= 0 && item.item.state && !item.item.isDuplicate && item.item.disabledTimer <= 0)
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
        List<WorldTile> trapsInView = new List<WorldTile>();

        foreach (Vector3Int spot in FOV)
        {
            Vector2Int loc = new Vector2Int(spot.x, spot.y);

            WorldTile tile = MapManager.inst.mapdata[loc.x, loc.y];
            if (tile.type == TileType.Trap)
            {
                if (!tile.trap_knowByPlayer
                    && !tile.trap_tripped
                    && tile.trap_active)
                {
                    trapsInView.Add(tile);
                }
            }
        }

        // Now go through each trap and determine if it should be located
        foreach (WorldTile trap in trapsInView)
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
    /// Helper function for a text highlight "animation". Given an input string, will start out fully highlighted then will randomly "split" into sections until,
    /// there is no highlights left.
    /// </summary>
    /// <param name="text">The text string to be highlighted.</param>
    /// <param name="highlightColor">The highlight color.</param>
    /// <returns>A list of strings which represents each step in the highlight process.</returns>
    public static List<string> RandomHighlightStringAnimation(string text, Color highlightColor)
    {
        List<string> animationSteps = new List<string>();
        StringBuilder sb = new StringBuilder();
        string colorHex = ColorUtility.ToHtmlStringRGB(highlightColor);

        // Start with the entire string highlighted
        string highlightedText = $"<mark=#{colorHex}aa>{text}</mark>";
        animationSteps.Add(highlightedText);

        int length = text.Length;
        HashSet<int> highlightedIndices = new HashSet<int>();
        for (int i = 0; i < length; i++)
        {
            highlightedIndices.Add(i);
        }

        // Randomly remove highlights from the center
        System.Random random = new System.Random();
        while (highlightedIndices.Count > 0)
        {
            int randomIndex = random.Next(highlightedIndices.Count);
            int removeIndex = -1;
            int count = 0;

            foreach (int index in highlightedIndices)
            {
                if (count == randomIndex)
                {
                    removeIndex = index;
                    break;
                }
                count++;
            }

            if (removeIndex != -1)
            {
                highlightedIndices.Remove(removeIndex);
                sb.Clear();

                for (int i = 0; i < length; i++)
                {
                    if (highlightedIndices.Contains(i))
                    {
                        sb.Append($"<mark=#{colorHex}aa>{text[i]}</mark>");
                    }
                    else
                    {
                        sb.Append(text[i]);
                    }
                }

                animationSteps.Add(sb.ToString());
            }
        }

        return animationSteps;
    }

    /// <summary>
    /// Given a single input string, will split it up given a position value within the string. Ex: (apple, 1) -> (a, p, ple)
    /// </summary>
    /// <param name="istring">The input string to split.</param>
    /// <param name="position">The position within the string to select as our "split point".</param>
    /// <param name="left">All the characters to the left of the split point.</param>
    /// <param name="middle">The single character at the split point.</param>
    /// <param name="right">All the characters to the right of the split point.</param>
    public static void SplitString(string istring, int position, out string left, out string middle, out string right)
    {
        // Check if the position is within the bounds of the string
        if (position < 0 || position >= istring.Length)
        {
            // If the position is out of bounds, set all output strings to empty
            left = "";
            middle = "";
            right = "";
            return;
        }

        // Get the substring to the left of the character at the given position
        left = istring.Substring(0, position);

        // Get the character at the given position
        middle = istring.Substring(position, 1);

        // Get the substring to the right of the character at the given position
        right = istring.Substring(position + 1);
    }

    /// <summary>
    /// Extracts text from the inside of (   ). Example Name(This)
    /// </summary>
    /// <param name="istring">A string in the format of "Name(This)".</param>
    /// <returns></returns>
    public static string ExtractText(string istring)
    {
        int bracketStartIndex = istring.IndexOf('(');
        int bracketEndIndex = istring.IndexOf(')');
        if (bracketStartIndex != -1 && bracketEndIndex != -1 && bracketEndIndex > bracketStartIndex)
        {
            return istring.Substring(bracketStartIndex + 1, bracketEndIndex - bracketStartIndex - 1).Trim();
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
    /// Access a string in the form of word(word), returns anything to the left of the "(". No change if "(" is not present.
    /// </summary>
    /// <param name="istring">A string that contains "(", usually in the form of word(word)</param>
    /// <returns>Returns the characters to the left of the "(" character.</returns>
    public static string GetLeftSubstring(string istring)
    {
        int index = istring.IndexOf("(");
        if (index != -1)
        {
            return istring.Substring(0, index);
        }

        return istring;
    }

    /// <summary>
    /// Access a string in the form of word(word), returns anything to the right of the "(".
    /// </summary>
    /// <param name="istring">A string that contains "(", usually in the form of word(word)</param>
    /// <returns>Returns the characters to the right of the "(" character.</returns>
    public static string GetRightSubstring(string istring)
    {
        int index = istring.IndexOf("(");
        if (index != -1)
        {
            return istring.Substring(index + 1, istring.Length - index - 2);
        }

        return string.Empty;
    }

    public static string RemoveTrailingNewline(string istring)
    {
        if (!string.IsNullOrEmpty(istring))
        {
            int length = istring.Length;
            while (length > 0 && istring[length - 1] == '\n')
            {
                length--;
            }

            return istring.Substring(0, length);
        }

        return istring;
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
    public static string GenerateMarkedString(string istring)
    {
        // Split the input string into words
        string[] words = istring.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

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

    public static string GetLastCharOfString(string istring)
    {
        if (istring.Length == 0)
        {
            return "";
        }
        else if (istring.Length == 1)
        {
            return istring;
        }
        else
        {
            return istring[istring.Length - 1].ToString();
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

    /// <summary>
    /// Returns the true (full) name of an item based on its internal conditions like "Broken", "Corrupted", etc. Works for prototypes too.
    /// </summary>
    /// <param name="item">The item we want the full name of</param>
    /// <returns>The full name with all conditions appended.</returns>
    public static string GetFullItemName(Item item) //  TODO: This will need to change with things like trap storage which update based on internal values
    {
        // First consider if its a prototype we don't know
        if (!item.itemData.knowByPlayer)
        {
            return HF.ItemPrototypeName(item);
        }

        // If not we do the normal routine
        string fullName = item.itemData.itemName;
        if (item.corrupted > 0)
        {
            fullName = "Corrupted " + fullName;
        }

        if (item.isBroken)
        {
            fullName = "Broken " + fullName;
        }

        if (item.isFaulty)
        {
            fullName = "Faulty " + fullName;
        }

        return fullName;
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

    public static string StringCoverageGradient(string istring, Color left, Color right, bool fixedLength = false)
    {
        int fixedLengthValue = GlobalSettings.inst.maxCharBarLength;

        int length = istring.Length;
        int gradientLength = fixedLength ? Mathf.Min(fixedLengthValue, length) : length;
        int startIndex = fixedLength ? Mathf.Max(0, length - fixedLengthValue) : 0;

        StringBuilder sb = new StringBuilder();

        if(istring.Length > 1)
        {
            for (int i = 0; i < length; i++)
            {
                Color color;
                if (i < startIndex)
                {
                    color = left;
                }
                else
                {
                    float t = (i - startIndex) / (float)(gradientLength - 1);
                    color = Color.Lerp(left, right, t);
                }

                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{istring[i]}</color>");
            }
        }
        else // Failsafe
        {
            sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(right)}>{istring[0]}</color>");
        }
        

        return sb.ToString();
    }

    public static string ValueToStringBar(float value, float max = 100f)
    {
        // Clamp the value between 0 and the max
        value = Mathf.Clamp(value, 0, max);

        // Calculate the number of "|" characters (12 is (usually) the maximum length)
        int barLength = Mathf.RoundToInt((value / max) * GlobalSettings.inst.maxCharBarLength);

        // Ensure there is at least one "|" if the value is greater than 0
        if (value > 0 && barLength == 0)
        {
            barLength = 1;
        }

        // Generate the bar string
        return new string('|', barLength);
    }

    /// <summary>
    /// Given a value and its cap, along with two colors, will return a color between the original two colors based on the value, in the form of a hex code.
    /// </summary>
    /// <param name="a">Color A</param>
    /// <param name="b">Color B</param>
    /// <param name="cap">The *low* and *high* limits of the value.</param>
    /// <param name="value">The value which determines the color.</param>
    /// <returns>A hex code in the form of a string.</returns>
    public static string ColorGradientByValue(Color a, Color b, Vector2 cap, float value)
    {
        // Clamp the value
        value = Mathf.Clamp(value, cap.x, cap.y);

        // Normalize the value between 0 and 1
        float normalizedValue = (value - cap.x) / (cap.y - cap.x);

        // Interpolate between colorA and colorB based on the normalized value
        Color resultColor = Color.Lerp(a, b, normalizedValue);

        // Convert the color to hex code
        int _r = Mathf.RoundToInt(resultColor.r * 255);
        int _g = Mathf.RoundToInt(resultColor.g * 255);
        int _b = Mathf.RoundToInt(resultColor.b * 255);

        // Return hex string
        return $"#{_r:X2}{_g:X2}{_b:X2}";
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
    public static bool LocationUnoccupied(Vector2Int pos)
    {
        byte info = MapManager.inst.pathdata[pos.x, pos.y];
        return info == 0 || info == 4; // Clear or Trap tile
    }

    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    public static Color HexToRGB(string hex)
    {
        Color output = Color.white;

        if (ColorUtility.TryParseHtmlString("#" + hex, out output)) {}

        return output;
    }

    /// <summary>
    /// Checks to see if the specified bot has the required resources to try and move to a new location.
    /// </summary>
    /// <param name="actor">The bot trying to move.</param>
    /// <returns>True/false if the bot can make the move.</returns>
    public static bool HasResourcesToMove(Actor actor)
    {
        float movecost_energy = 0;

        List<Item> propulsion = new List<Item>();
        if (actor.GetComponent<PlayerData>())
        {
            foreach (var I in PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if(I.item.Id >= 0 && I.item.state && I.item.disabledTimer <= 0)
                {
                    movecost_energy += I.item.itemData.propulsion[0].propEnergy;
                }
            }

            return PlayerData.inst.currentEnergy >= movecost_energy;
        }
        else // Bot
        {
            foreach (var I in actor.components.Container.Items)
            {
                if (I.item.Id >= 0 && I.item.state && I.item.disabledTimer <= 0)
                {
                    movecost_energy += I.item.itemData.propulsion[0].propEnergy;
                }
            }

            return actor.currentEnergy >= movecost_energy;
        }
    }
    public static QuestPoint ActorHasQuestPoint(Actor actor)
    {
        Transform actorTransform = actor.transform;

        foreach (Transform child in actorTransform)
        {
            if (child.gameObject.GetComponent<QuestPoint>())
            {
                return child.gameObject.GetComponent<QuestPoint>();
            }
        }

        return null;
    }

    /// <summary>
    /// A simple but effective way to check if the mouse is over the UI.
    /// </summary>
    /// <returns>True/False. If the mouse is "over the UI area".</returns>
    public static bool MouseBoundsCheck()
    {
        Vector3 position = Mouse.current.position.ReadValue();
        return position.x > 888 || position.y > 555;
    }

    /// <summary>
    /// Converts a list of items to a simple list of *bool*s where true = item & false = no item.
    /// </summary>
    /// <param name="inventory">The inventory to convert.</param>
    /// <returns>A list of bools where: True = an item & False = no item (aka Empty)</returns>
    public static List<bool> InventoryToSimple(InventoryObject inventory)
    {
        List<bool> result = new List<bool>();

        foreach (var item in inventory.Container.Items)
        {
            if(item.item == null || item.item.Id == -1) // Empty (no item)
            {
                result.Add(false);
            }
            else // Item
            {
                result.Add(true);
            }
        }

        return result;
    }

    /// <summary>
    /// Compainion function to *InventoryToSimple*. Identifies a "gap" between true bool's in a list of bools.
    /// </summary>
    /// <returns>Returns true/false if there is a gap between two true values in the list (where the middle one is false).</returns>
    public static bool FindGapInList(List<bool> list)
    {
        if (list.Count < 3)
        {
            // Not enough elements to check for a gap
            return false;
        }

        bool foundFirstTrue = false;
        bool inGap = false;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == true)
            {
                if (inGap)
                {
                    return true;
                }
                foundFirstTrue = true;
                inGap = false;
            }
            else if (list[i] == false && foundFirstTrue)
            {
                inGap = true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sibling function to *FindGapInList*, used in Auto-Sort. Checks to see if a simplified list has the specified "space" in the list. Returns if it exists, and the index of where it starts.
    /// </summary>
    /// <param name="boolList">The simplified list of bools we will search through.</param>
    /// <param name="spaceSize">The size of space we want to find.</param>
    /// <returns>(bool, int). If the space exists, and if so, where it begins.</returns>
    public static (bool, int) HasSpaceInList(List<bool> boolList, int spaceSize)
    {
        if (boolList.Count < spaceSize + 1)
        {
            // Not enough elements to check for a space
            return (false, -1);
        }

        int spaceStartIndex = -1;

        for (int i = 0; i <= boolList.Count - spaceSize; i++)
        {
            bool foundSpace = true;

            // Check the next 'spaceSize' elements
            for (int j = 0; j < spaceSize; j++)
            {
                if (boolList[i + j])
                {
                    // If any of the elements in the range are true, it's not a space
                    foundSpace = false;
                    break;
                }
            }

            if (foundSpace)
            {
                // Found a space of the specified size
                spaceStartIndex = i;
                return (true, spaceStartIndex);
            }
        }

        // No space found
        return (false, -1);
    }

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

    public static List<string> StringToList(string istring)
    {
        List<string> resultList = new List<string>();

        // Iterate over each character in the input string
        for (int i = 0; i <= istring.Length; i++)
        {
            // Extract the substring from the beginning up to the current character
            string substring = istring.Substring(0, i);
            // Add the substring to the result list
            resultList.Add(substring);
        }

        return resultList;
    }

    public static AudioClip RandomClip(List<AudioClip> clips)
    {
        return clips[Random.Range(0, clips.Count - 1)];
    }

    /// <summary>
    /// Given an input color and a percentage value, will "darken" the color by the given percentage.
    /// </summary>
    /// <param name="originalColor">A color to darken.</param>
    /// <param name="percentage">A float percentage value (0.##f).</param>
    /// <returns>A color that is darker than the input color.</returns>
    public static Color GetDarkerColor(Color originalColor, float percentage)
    {
        // Adjust color
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


        int totalExposure = coreExposure; // Calculate total exposure
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
    public static bool PhaseWallVisCheck(Actor actor, WorldTile tile)
    {
        return tile.phase_team == actor.myFaction || tile.phase_revealed;
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
            if(item.item.Id >= 0 && !item.item.isDuplicate)
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

    public static void BreakPart(Item item, InvDisplayItem display)
    {
        // Log a message before the name changes
        string message = $"{HF.GetFullItemName(item)} broken.";
        UIManager.inst.CreateNewLogMessage(message, display.dangerRed, UIManager.inst.alertRed, false, true);
        
        // Change the internal variable
        item.isBroken = true;

        // Update the UI
        display.BreakItem();
    }

    public static void FusePart(Item item, InvDisplayItem display, bool doMessage = true)
    {
        // May or may not want to do the message
        if (doMessage)
        {
            string message = $"{HF.GetFullItemName(item)} fused.";
            UIManager.inst.CreateNewLogMessage(message, display.dangerRed, UIManager.inst.alertRed, false, true);
        }

        // Change the internal variable
        item.isFused = true;

        // Update the UI
        display.FuseItem();
    }

    /// <summary>
    /// Misc logic that happens directly after equipping an item.
    /// </summary>
    /// <param name="item">The item (or primary item) that was just equipped.</param>
    /// <param name="messagePreface">The preface for a message to be displayed. ex: Aquired, Equipped. No message if left unset.</param>
    /// <param name="doInterfaceUpdate">If the interface should be forcefully updated or not. Default is true.</param>
    public static void MiscItemEquipLogic(Item item, string messagePreface = "", bool doInterfaceUpdate = true)
    {
        string fullName = HF.GetFullItemName(item);

        // If this item was unknown to us, now add that data
        if (!item.itemData.knowByPlayer)
            item.itemData.knowByPlayer = true;

        bool DO_CORRUPTION = item.corrupted > 0 && !item.doneCorruptionFeedback;
        bool DO_FAULTY = item.isFaulty && !item.doneFaultyFailure;

        // -- Faulty item consequences --
        if (DO_FAULTY)
        {
            Action.FaultyConsequences(item);
        }

        // -- Corrupted item consequences --
        // see (https://noemica.github.io/cog-minder/wiki/Corruption)
        int corruption = 0;
        if (DO_CORRUPTION)
        {
            item.doneCorruptionFeedback = true;

            // -- Consequences --
            // When attaching a corrupted part, you will suffer some amount of corruption, and then
            // the corrupted item becomes "integrated", and is no longer corrupted.
            // 1% and [corruption % on death / 10] corruption, capped at 15%
            corruption = Random.Range(1, item.corrupted); // (not doing the division here because its already low)
            if(corruption > 15)
            {
                corruption = 15;
            }

            // Modify the player's corruption
            Action.ModifyPlayerCorruption(corruption);

            // Un-corrupt the item.
            item.corrupted = 0;

            // Display a message
        }

        // If the item is faulty or broken we should use a different text color
        Color a = UIManager.inst.activeGreen, b = UIManager.inst.dullGreen;
        if (item.isFaulty || item.isBroken)
        {
            a = UIManager.inst.dangerRed;
            b = UIManager.inst.highSecRed;
        }

        // Update Interface
        if (doInterfaceUpdate)
            InventoryControl.inst.UpdateInterfaceInventories();

        // Display log message
        if (messagePreface != "")
        {
            UIManager.inst.CreateNewLogMessage($"{messagePreface} {fullName}.", a, b, false, true);
        }

        // Other messages
        if (DO_CORRUPTION)
        {
            a = UIManager.inst.dangerRed;
            b = UIManager.inst.highSecRed;
            UIManager.inst.CreateNewLogMessage($"Integrated {fullName} (+{corruption}).", a, b, false, true);
        }
        else if (DO_FAULTY) { /* Messages handled previously in this function. */ }

    }

    /// <summary>
    /// Given a list of `Item`s. Returns a list of ItemObjects without duplicates.
    /// </summary>
    /// <param name="items">The input list of type `Item`.</param>
    /// <returns>A unique list of `ItemObject`s based on the input list.</returns>
    public static List<ItemObject> ItemListToItemObjects(List<Item> items)
    {
        List<ItemObject> itemObjects = new List<ItemObject>();

        foreach (var I in items)
        {
            itemObjects.Add(I.itemData);
        }

        return itemObjects.Distinct().ToList(); // Remove duplicates
    }

    /// <summary>
    /// Does this bot (the player) have the sufficient energy & matter to equip an item? Will remove the amount if true. Displays a message and returns a bool.
    /// </summary>
    /// <param name="actor">The bot trying to equip (the player).</param>
    /// <returns>True/False if they can attach this part.</returns>
    public static bool HasResourcesToAttach(Actor actor)
    {
        // "Attaching a part requires 20 energy and 10 matter. Detaching a part expends 10 energy."
        // (This can change depending on what `GlobalSettings.cs` says
        int eCost = GlobalSettings.inst.partEnergyAttachmentCost;
        int mCost = GlobalSettings.inst.partMatterAttachmentCost;

        bool isPlayer = actor.GetComponent<PlayerData>();

        int energy = 0, matter = 0;

        string qualifier = "";
        if (isPlayer)
        {
            energy = actor.GetComponent<PlayerData>().currentEnergy;
            matter = actor.GetComponent<PlayerData>().currentMatter;
            qualifier = "Insufficient";
        }
        else
        {
            energy = actor.currentEnergy;
            matter = 100; //actor.currentMatter; // Bots don't have matter
            qualifier = $"{actor.uniqueName} has insufficient";
        }

        bool canAttach = true;

        if (matter < mCost)
        {
            UIManager.inst.ShowCenterMessageTop($"{qualifier} matter stored to equip item ({10 - PlayerData.inst.currentMatter})", UIManager.inst.dangerRed, Color.black);
            canAttach = false;
        }
        else if (energy < eCost)
        {
            UIManager.inst.ShowCenterMessageTop($"{qualifier} energy stored to equip item ({20 - PlayerData.inst.currentEnergy})", UIManager.inst.dangerRed, Color.black);
            canAttach = false;
        }

        if (canAttach)
        {
            // Remove the amount
            if (isPlayer)
            {
                actor.GetComponent<PlayerData>().currentEnergy -= eCost;
                actor.GetComponent<PlayerData>().currentMatter -= mCost;
            }
            else
            {
                actor.currentEnergy -= eCost;
                //actor.currentMatter -= mCost; // Bots don't have matter
            }
        }

        return canAttach;
    }

    /// <summary>
    /// Attempt to parse a settings option based on an ID. Will return all important data about that setting so it can be visualized.
    /// </summary>
    /// <param name="id">An int ID refering to a single settings option.</param>
    /// <returns>Currently active setting (SSShort), Name of Setting (string), Bottom display text (string), List<(string, SSShort) of options></returns>
    public static (ScriptableSettingShort, string, string, List<(string, ScriptableSettingShort)>) ParseSettingsOption(int id)
    {
        // Not too pleased with this but unsure of how else to approach it.

        ScriptableSettingShort value = new ScriptableSettingShort();
        string display = "";
        string bottomText = "";
        // (String to display on the box, what that option will change)
        List<(string, ScriptableSettingShort)> options = new List<(string, ScriptableSettingShort)>();

        ScriptableSettings settings = null;

        if (MainMenuManager.inst)
        {
            settings = MainMenuManager.inst.settingsObject;
        }
        else if (GlobalSettings.inst)
        {
            settings = GlobalSettings.inst.settings;
        }

        if(settings == null)
        {
            Debug.LogError("ERROR: No settings object detected. Settings menu will break!");
        }

        // This is going to be very ugly.
        switch (id)
        {
            case 0: // Modal Layout (NOT IMPLEMENTED)
                value.enum_modal = settings.uiLayout;
                display = "UI Layout (X!)";
                options.Add(("Non-modal (smaller text/tiles, widest map area, all windows visibile)", new ScriptableSettingShort(e_m: ModalUILayout.NonModal)));
                options.Add(("Semi-modal (large text/tiles, medium map area, inventory eventually modal", new ScriptableSettingShort(e_m: ModalUILayout.SemiModal)));
                options.Add(("Modal (large text/tiles, large map area, multiple windows hidden)", new ScriptableSettingShort(e_m: ModalUILayout.Modal)));
                bottomText = "General UI layout, to adjust the balance between easy access to info vs text/tile size. Cogmind was originally design for the Full layout," +
                    " but as that arrangement is only suitable for players with physically large displays, other options allow one to hide info in modal windows in exchange " +
                    "for increased text size. Changing this requires manual restart to take effect.";
                break;
            case 1: // Font
                value.value_string = settings.font;
                display = "Font Set/Size (X!)";
                options.Add(("18/TerminusBold", new ScriptableSettingShort(v_s: "18/TerminusBold")));
                options.Add(("18/Cogmind", new ScriptableSettingShort(v_s: "18/Cogmind")));
                bottomText = "Current font set, which indirectly determines the default dimensions of the window/interface. (Note: The smallest size, 10, does not support " +
                    "tilesets and is therefore only available in ASCII mode. Also in modal UI layouts, the minimum size is 14.)";
                break;
            case 2: // Fullscreen Mode
                value.enum_fullscreen = settings.fullScreenMode;
                display = "Fullscreen";
                options.Add(("Borderless Fullscreen", new ScriptableSettingShort(e_fs: FullScreenMode.ExclusiveFullScreen)));
                options.Add(("True Fullscreen", new ScriptableSettingShort(e_fs: FullScreenMode.FullScreenWindow)));
                options.Add(("Windowed", new ScriptableSettingShort(e_fs: FullScreenMode.Windowed)));
                bottomText = "Deactivate fullscreen mode to play in a window. In windowed mode you may want to select a smaller font, or reduce the map size as desired " +
                    "(50x50 map recommended, though with the game closed /user/system.cfg can be edited to adjust the map size). " +
                    "Use borderless fullscreen mode for better multimonitor support.";
                break;
            case 3: // Show Intro
                value.value_bool = settings.showIntro;
                value.canBeGrayedOut = true;
                display = "Show Intro";
                options.Add(("On", new ScriptableSettingShort(v_b:true)));
                options.Add(("Off", new ScriptableSettingShort(v_b:false, grayedOut:true)));
                bottomText = "Shows the intro before starting a new game.";
                break;
            case 4: // Tutorial
                value.value_bool = settings.tutorial;
                value.canBeGrayedOut = true;
                display = "Tutorial";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Shows contextual tutorial messages in the log window. Each message is only shown once, but toggling this off and on again resets all message " +
                    "records, in addition to causing the next three runs to start in the tutorial map layout.";
                break;
            case 5: // Difficulty
                value.enum_difficulty = settings.difficulty;
                display = "Difficulty";
                options.Add(("Explorer", new ScriptableSettingShort(e_d:Difficulty.Explorer)));
                options.Add(("Adventurer", new ScriptableSettingShort(e_d:Difficulty.Adventurer)));
                options.Add(("Rogue", new ScriptableSettingShort(e_d:Difficulty.Rogue)));
                bottomText = "Explorer is relatively easy, Adventurer is fairly challenging, and Rogue is extremely hard (but is the mode Cogmind was designed for). " +
                    "See the manual for details about each mode. Changes to this setting will not take effect until starting a new run.";
                break;
            case 6: // Log Output
                value.value_bool = settings.logOutput;
                display = "Log Output";
                options.Add(("None", new ScriptableSettingShort(v_b:false)));
                options.Add(("Full", new ScriptableSettingShort(v_b:true)));
                bottomText = "Format to which the message log is output when the game ends (has no impact on the stat summary output). Log contents are written to " +
                    "the scores subdirectory. This setting also applies to mid-run stat dumps.";
                break;
            case 7: // Volume - Master
                value.value_float = settings.volume_master;
                display = "Master Volume";
                options.Add(("0%", new ScriptableSettingShort(v_f:0.0f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "Overall volume level.";
                break;
            case 8: // Volume - Interface
                value.value_float = settings.volume_interface;
                display = "  Interface";
                options.Add(("0%", new ScriptableSettingShort(v_f: 0.0f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "Relative volume of interface sounds.";
                break;
            case 9: // Volume - Game
                value.value_float = settings.volume_game;
                display = "  Game";
                options.Add(("0%", new ScriptableSettingShort(v_f: 0.0f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "Relative volume of game sounds (weapons, robots, etc.).";
                break;
            case 10: // Volume - Props
                value.value_float = settings.volume_props;
                display = "  Props";
                options.Add(("0%", new ScriptableSettingShort(v_f: 0.0f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "Relative volume of environment objects like machinens.";
                break;
            case 11: // Volume - Ambient
                value.value_float = settings.volume_ambient;
                display = "  Ambient";
                options.Add(("0%", new ScriptableSettingShort(v_f: 0.0f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "Relative volume of mapwide ambience.";
                break;
            case 12: // Audio Log
                value.value_bool = settings.audioLog;
                value.canBeGrayedOut = true;
                display = "Audio Log";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Meant primarily as an accessibility feature akin to closed captions, this log lists sound effects at the top-right corner of the map view, allowing anyone " +
                    "who keeps their volume low or muted to be able to retain access to important audio knowledge. See the manual's Audio Log section for more information.";
                break;
            case 13: // Tactical HUD
                value.value_bool = settings.tacticalHud;
                value.canBeGrayedOut = true;
                display = "Tactical HUD";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "HUD information is more detailed in tactical mode. Speed-related variables are shown in real time unit costs, energy and heat change predictions shown " +
                    "for both stationary (per turn) and mobile (per move) circumstances (latter suffers short-term innacuracies due to averaging), items display rating as part of their " +
                    "respective map labels, and more. For advanced players.";
                break;
            case 14: // Combat Log Detail
                value.value_bool = settings.combatLogDetail;
                display = "Combat Log Detail";
                options.Add(("High", new ScriptableSettingShort(v_b: false)));
                options.Add(("Max", new ScriptableSettingShort(v_b: true)));
                bottomText = "Level of detail shown in the combat log. There are numerous related features available depending on your layout and detail settings. See the manual under Advanced" +
                    " UI > Combat Log Window for these options.";
                break;
            case 15: // Part Auto Sorting
                value.value_bool = settings.partAutoSorting;
                value.canBeGrayedOut = true;
                display = "Part Autosorting";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically sorts parts once moving again after changes to loadout. (This same effect can be handled manually via the ':' key.)";
                break;
            case 16: // Inventory Auto Sorting
                value.value_bool = settings.inventoryAutoSorting;
                value.canBeGrayedOut = true;
                display = "Inventory Autosorting";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically sort inventory items by type once moving again after changes to inventory. (This same effect can be handled manually via the 't' key.)";
                break;
            case 17: // Edge Panning Speed
                value.value_int = settings.edgePanningSpeed;
                value.canBeGrayedOut = true;
                display = "Edge Panning Speed";
                options.Add(("0", new ScriptableSettingShort(v_i: 0, grayedOut: true)));
                options.Add(("5", new ScriptableSettingShort(v_i: 5)));
                options.Add(("10", new ScriptableSettingShort(v_i: 10)));
                options.Add(("15", new ScriptableSettingShort(v_i: 15)));
                options.Add(("20", new ScriptableSettingShort(v_i: 20)));
                bottomText = "Map panning speed when cursor is at the edge of the screen, measured in milliseconds/cell (therefore high numbers are slower). Set to 0 to " +
                    "deactivate. Only campatible with True Fullscreen mode.";
                break;
            case 18: // Click Walls To Target
                value.value_bool = settings.clickWallsToTarget;
                value.canBeGrayedOut = true;
                display = "Click Walls to Target";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Clicking on a wall currently in view enter targeting mode (no effect in Keyboard Mode).";
                break;
            case 19: // Label Supporter Items (NOT IMPLEMENTED)
                value.value_bool = settings.labelSupporterItems;
                value.canBeGrayedOut = true;
                display = "Label Supporter Items";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Displays alpha supporter item attributions below the item info, and also whether you've attached that item to add it to your art gallery. Even without " +
                    "this option on, labels for items not previously collected are marked with a '!' following the name.";
                break;
            case 20: // Keyboard Mode
                value.value_bool = settings.keyBoardMode;
                value.canBeGrayedOut = true;
                display = "Keyboard Mode";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Hides the cursor and enables keyboard-controlled map look functionality. For hardcore keyboard-only players (playing this way is FAST). Note that in " +
                    "Keyboard Mode, context help in the status/data windows is still avaiable via up/down arrows. Pressing F2 while on the main interface also swaps in and out of this mode for conveniece.";
                break;
            case 21: // Colorblind Adjustment
                value.value_bool = settings.colorblindAdjustment;
                value.canBeGrayedOut = true;
                display = "Colorblind Adjustment";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Makes color-based adjustments to the interface that may help some players. Neutral 0b10 bots show as light gray instead of green, most orange UI colors instead " +
                    "appear azure blue, and most green is converted to light gray. Fully applying this option requires a manual restart. See the manual's Accessibility section for more info.";
                break;
            case 22: // Auto-activate Parts
                value.value_bool = settings.autoActivateParts;
                value.canBeGrayedOut = true;
                display = "Auto-activate Parts";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically activates parts as they are attached (when possible). This option can be toggled in realtime with Ctrl-F10 if you need to temporarily change it for a specific action.";
                break;
            case 23: // Stop on Threats Only
                value.value_bool = settings.stopOnThreatsOnly;
                value.canBeGrayedOut = true;
                display = "Stop on Threats Only";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Stop running/auto-athing only on spotting combat-capable enemies, rather than any hostile.";
                break;
            case 24: // Move Block Duration
                value.value_int = settings.moveBlockDuration;
                display = "Move Block Duration";
                options.Add(("0", new ScriptableSettingShort(v_i: 0)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                options.Add(("750", new ScriptableSettingShort(v_i: 750)));
                options.Add(("1000", new ScriptableSettingShort(v_i: 1000)));
                options.Add(("1500", new ScriptableSettingShort(v_i: 1500)));
                bottomText = "Number of milliseconds for which to block all movement commands after seeing a new enemy (or only non-disarmed threats if Stop on THreats Only is on). " +
                    "Enemies that move in and out of view will not trigger the block until they haven't been seen for at least 10 turns.";
                break;
            case 25: // Playername
                value.value_string = settings.playerName;
                value.inputfield = true;
                display = "Name";
                // Uniquely for Inputfields, their options start with only their current value.
                options.Add((value.value_string, new ScriptableSettingShort(v_s: value.value_string)));
                // This is an input field
                bottomText = "Player name under which score sheets are saved and the core is named.";
                break;
            case 26: // Upload Scores (NOT IMPLEMENTED)
                value.value_bool = settings.uploadScores;
                value.canBeGrayedOut = true;
                display = "Upload Scores";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "THIS FUNCTION IS NOT IMPLEMENTED.";
                break;
            case 27: // Seed
                value.value_string = settings.seed;
                value.inputfield = true;
                value.canBeGrayedOut = true;
                display = "Seed";
                // Uniquely for Inputfields, their options start with only their current value.
                options.Add((value.value_string, new ScriptableSettingShort(v_s: value.value_string, grayedOut: true)));
                // This is an input field
                bottomText = "Enter any combination of numbers and/or letters to \"seed\" the game, making it possible to replay the same world, or play the same one as friends using the same seed. " +
                    "(Setting this only affects future games.) Seeds are not case sensistive. Enter \"0\" or clear the seed to make it random, ensuring a new world every game. Random or not, each run's " +
                    "score sheet contains its seed.";
                break;
            case 28: // News Updates (NOT IMPLEMENTED)
                value.value_bool = settings.newsUpdates;
                value.canBeGrayedOut = true;
                display = "News/Updates";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "THIS FUNCTION IS NOT IMPLEMENTED.";
                break;
            case 29: // Report Errors (NOT IMPLEMENTED)
                value.value_bool = settings.reportErrors;
                value.canBeGrayedOut = true;
                display = "Report Errors";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "THIS FUNCTION IS NOT IMPLEMENTED.";
                break;
            case 30: // Achievements Anywhere
                value.value_bool = settings.achievementsAnywhere;
                value.canBeGrayedOut = true;
                display = "Achievements Anywhere";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Allow achievements to be earned even in challenge modes and special modes. Some are much easier in these modes compared to the regular game, even " +
                    "unintentionally, so a portion of players prefer to block them except when playing normally. (Setting does not affect achievements that can only be earned during challenge modes.)";
                break;
            case 31: // ASCII Mode
                value.value_bool = settings.asciiMode;
                value.canBeGrayedOut = true;
                display = "ASCII Mode";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Replaces all sprites with ASCII keyboard characters. (Note that at the smallest font size, 10, only ASCII is supported.)";
                break;
            case 32: // Show Path
                value.value_bool = settings.showPath;
                value.canBeGrayedOut = true;
                display = "Show Path";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Shows the path to the cursor (no effect in Keyboard Mode). Note that even with this feature off, you can still hold Ctrl-Alt " +
                    "to highlight the path manually when necessary, and while on those keys can be used to brighten the path.";
                break;
            case 33: // Explosion Predictions
                value.value_bool = settings.explosionPredictions;
                value.canBeGrayedOut = true;
                display = "Explosion Predictions";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Displays the expected explosion radius/radii when aiming a volley that contains one or more explosive weapons.";
                break;
            case 34: // Hit Chance Delay
                value.value_int = settings.hitChanceDelay;
                display = "Hit Chance Delay";
                options.Add(("0", new ScriptableSettingShort(v_i: 0)));
                options.Add(("1", new ScriptableSettingShort(v_i: 1)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                options.Add(("1500", new ScriptableSettingShort(v_i: 1500)));
                options.Add(("2000", new ScriptableSettingShort(v_i: 2000)));
                options.Add(("3000", new ScriptableSettingShort(v_i: 3000)));
                bottomText = "Number of milliseconds after entering targeting mode when hit chances automatically appear next to threats in view." +
                    " Setting to zero deactivates hit chance displays.";
                break;
            case 35: // Combat Indicators
                value.value_bool = settings.combatIndicators;
                value.canBeGrayedOut = true;
                display = "Combat Indicators";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Temporarily displays combat effects directly over the map, including indicators for remaining integrity percentage " +
                    "(on core hit), lost parts, and EM disruption.";
                break;
            case 36: // Auto-label Threats
                value.value_bool = settings.autoLabelThreats;
                value.canBeGrayedOut = true;
                display = "Auto-label Threats";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically labels newly identified threats on the map. (Newly encountered friendlies are also labeled.)";
                break;
            case 37: // Auto-label Items
                value.value_bool = settings.autoLabelItems;
                value.canBeGrayedOut = true;
                display = "Auto-label Items";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically labels newly discovered items on the map.";
                break;
            case 38: // Auto-label on Examine
                value.value_bool = settings.autoLabelOnExamine;
                value.canBeGrayedOut = true;
                display = "Auto-label on Examine";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Automatically labels any robot or part under the examine mode cursor (in Keyboard Mode), or under the mouse cursor when not in Keyboard mode " +
                    "(note that with the mouse cursor, holding Ctrl-Shift has the same effect even while this option is disabled).";
                break;
            case 39: // Color Item Labels
                value.value_bool = settings.colorItemLabels;
                value.canBeGrayedOut = true;
                display = "Color Item Labels";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Color item labels by their respective integrity. Labels for items currently out of view appear slightly darker. Note that even while this option is " +
                    "inactive, the gray scheme's colors are darkened for those items with less than 75% integrity remaining.";
                break;
            case 40: // Motion Trail Duration
                value.value_int = settings.motionTrailDuration;
                display = "Motion Trail Duration";
                options.Add(("0", new ScriptableSettingShort(v_i: 0)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                options.Add(("1000", new ScriptableSettingShort(v_i: 1000)));
                options.Add(("1500", new ScriptableSettingShort(v_i: 1500)));
                options.Add(("2000", new ScriptableSettingShort(v_i: 2000)));
                bottomText = "Number of milliseconds for which to highlight each space previously occupied by a robot as part of their motion trail. While active, hostile robots " +
                    "spotting you are indicated by exclamation points rather than glowing backgrounds to avoid color confusion.";
                break;
            case 41: // Floor Gamma
                value.value_int = settings.floorGamma;
                value.canBeGrayedOut = true;
                display = "Floor Gamma";
                options.Add(("+0", new ScriptableSettingShort(v_i: 0, grayedOut: true)));
                options.Add(("+1", new ScriptableSettingShort(v_i: 1)));
                options.Add(("+2", new ScriptableSettingShort(v_i: 2)));
                options.Add(("+3", new ScriptableSettingShort(v_i: 3)));
                bottomText = "On certain monitors, tileset users might benefit from an increase in floor brightness to better differentiate current FOV from explored areas. Each " +
                    "level slightly increases the brightness of floor and debris tiles. Note that while this feature is meant for use in tileset mode, increasing it also " +
                    "adjusts the ASCII floor color (which doesn't really need it).";
                break;
            case 42: // FOV Handling
                value.enum_fov = settings.fovHandling;
                display = "FOV Handling";
                options.Add(("Fade In", new ScriptableSettingShort(e_fov: FOVHandling.FadeIn)));
                options.Add(("Delay", new ScriptableSettingShort(e_fov: FOVHandling.Delay)));
                options.Add(("Instant", new ScriptableSettingShort(e_fov: FOVHandling.Instant)));
                bottomText = "Determines how newly seen areas are added to the FOV. \"Delay\" waits until the end of an action. \"Instant\" updates every frame if obstacles were destroyed, " +
                    "and \"Fade In\" behaves like instant but with a short animation for the transition from unseen to seen.";
                break;
            case 43: // Corruption Glitches
                value.value_bool = settings.corruptionGlitches;
                value.canBeGrayedOut = true;
                display = "Corruption Glitches";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Enables intermittent audiovisual glitch effects while system corrupted (the frequency of glitches is based on corruption level). This is inteded for " +
                    "immersion and as a reminder of corruption, but you can turn it off if it's annoying. Disabling it also disables the window border animation when taking EM damage.";
                break;
            case 44: // Screenshake
                value.value_bool = settings.screenShake;
                value.canBeGrayedOut = true;
                display = "Screenshake";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Enables screenshake effect when core damaged, and window shake due to nearby explosions based on their force and proximity.";
                break;
            case 45: // Alert (Heat)
                value.value_int = settings.alert_heat;
                display = "Heat";
                options.Add(("100", new ScriptableSettingShort(v_i: 100)));
                options.Add(("200", new ScriptableSettingShort(v_i: 200)));
                options.Add(("300", new ScriptableSettingShort(v_i: 300)));
                options.Add(("400", new ScriptableSettingShort(v_i: 400)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                options.Add(("600", new ScriptableSettingShort(v_i: 600)));
                options.Add(("700", new ScriptableSettingShort(v_i: 700)));
                options.Add(("800", new ScriptableSettingShort(v_i: 800)));
                options.Add(("900", new ScriptableSettingShort(v_i: 900)));
                options.Add(("1000", new ScriptableSettingShort(v_i: 1000)));
                bottomText = "Minimum amount of heat required to trigger alarm sound.";
                break;
            case 46: // Alert (Core)
                value.value_float = settings.alert_core;
                display = "Core";
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "If core integrity drops to or below this level an alarm sound is triggered.";
                break;
            case 47: // Alert (Energy)
                value.value_float = settings.alert_energy;
                display = "Energy";
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "If remaining energy drops to or below this level an alarm sound is triggered.";
                break;
            case 48: // Alert (Matter)
                value.value_float = settings.alert_matter;
                display = "Matter";
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "If remaining matter drops to or below this level an alarm sound is triggered.";
                break;
            case 49: // Alert Popups
                value.value_bool = settings.alertPopups;
                value.canBeGrayedOut = true;
                display = "Alert Popups";
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                bottomText = "Flashes on-map warnings and alerts for low core integrity, energy, and matter.";
                break;
        }


        return (value, display, bottomText, options);
    }

    /// <summary>
    /// A (very clumbsy) method that updating a specific setting given its ID and what to set the new value as.
    /// </summary>
    /// <param name="id">The ID of the setting (see switch statement).</param>
    /// <param name="s">The value to update the setting with (in the form of ScriptableSettingShort).</param>
    public static void UpdateSetting(int id, ScriptableSettingShort s)
    {
        ScriptableSettings settings = MainMenuManager.inst ? MainMenuManager.inst.settingsObject : GlobalSettings.inst.settings;

        // This is going to be very ugly.
        switch (id)
        {
            case 0: // Modal Layout (NOT IMPLEMENTED)
                settings.uiLayout = (ModalUILayout)s.enum_modal;
                break;
            case 1: // Font
                settings.font = s.value_string;
                break;
            case 2: // Fullscreen Mode
                settings.fullScreenMode = (FullScreenMode)s.enum_fullscreen;
                break;
            case 3: // Show Intro
                settings.showIntro = (bool)s.value_bool;
                break;
            case 4: // Tutorial
                settings.tutorial = (bool)s.value_bool;
                break;
            case 5: // Difficulty
                settings.difficulty = (Difficulty)s.enum_difficulty;
                break;
            case 6: // Log Output
                settings.logOutput = (bool)s.value_bool;
                break;
            case 7: // Volume - Master
                settings.volume_master = (float)s.value_float;

                break;
            case 8: // Volume - Interface
                settings.volume_interface = (float)s.value_float;

                break;
            case 9: // Volume - Game
                settings.volume_game = (float)s.value_float;

                break;
            case 10: // Volume - Props
                settings.volume_props = (float)s.value_float;

                break;
            case 11: // Volume - Ambient
                settings.volume_ambient = (float)s.value_float;

                break;
            case 12: // Audio Log
                settings.audioLog = (bool)s.value_bool;

                break;
            case 13: // Tactical HUD
                settings.tacticalHud = (bool)s.value_bool;

                break;
            case 14: // Combat Log Detail
                settings.combatLogDetail = (bool)s.value_bool;

                break;
            case 15: // Part Auto Sorting
                settings.partAutoSorting = (bool)s.value_bool;

                break;
            case 16: // Inventory Auto Sorting
                settings.inventoryAutoSorting = (bool)s.value_bool;

                break;
            case 17: // Edge Panning Speed
                settings.edgePanningSpeed = (int)s.value_int;

                break;
            case 18: // Click Walls To Target
                settings.clickWallsToTarget = (bool)s.value_bool;

                break;
            case 19: // Label Supporter Items (NOT IMPLEMENTED)
                settings.labelSupporterItems = (bool)s.value_bool;

                break;
            case 20: // Keyboard Mode
                settings.keyBoardMode = (bool)s.value_bool;

                break;
            case 21: // Colorblind Adjustment
                settings.colorblindAdjustment = (bool)s.value_bool;
                break;
            case 22: // Auto-activate Parts
                settings.autoActivateParts = (bool)s.value_bool;
                break;
            case 23: // Stop on Threats Only
                settings.stopOnThreatsOnly = (bool)s.value_bool;
                break;
            case 24: // Move Block Duration
                settings.moveBlockDuration = (int)s.value_int;
                break;
            case 25: // Playername
                settings.playerName = s.value_string;

                break;
            case 26: // Upload Scores (NOT IMPLEMENTED)
                settings.uploadScores = (bool)s.value_bool;

                break;
            case 27: // Seed
                settings.playerName = s.value_string;

                break;
            case 28: // News Updates (NOT IMPLEMENTED)
                settings.newsUpdates = (bool)s.value_bool;

                break;
            case 29: // Report Errors (NOT IMPLEMENTED)
                settings.reportErrors = (bool)s.value_bool;

                break;
            case 30: // Achievements Anywhere
                settings.achievementsAnywhere = (bool)s.value_bool;
                break;
            case 31: // ASCII Mode
                settings.asciiMode = (bool)s.value_bool;
                break;
            case 32: // Show Path
                settings.showPath = (bool)s.value_bool;
                break;
            case 33: // Explosion Predictions
                settings.explosionPredictions = (bool)s.value_bool;
                break;
            case 34: // Hit Chance Delay
                settings.hitChanceDelay = (int)s.value_int;
                break;
            case 35: // Combat Indicators
                settings.combatIndicators = (bool)s.value_bool;
                break;
            case 36: // Auto-label Threats
                settings.autoLabelThreats = (bool)s.value_bool;
                break;
            case 37: // Auto-label Items
                settings.autoLabelItems = (bool)s.value_bool;
                break;
            case 38: // Auto-label on Examine
                settings.autoLabelOnExamine = (bool)s.value_bool;
                break;
            case 39: // Color Item Labels
                settings.colorItemLabels = (bool)s.value_bool;
                break;
            case 40: // Motion Trail Duration
                settings.motionTrailDuration = (int)s.value_int;
                break;
            case 41: // Floor Gamma
                settings.floorGamma = (int)s.value_int;
                break;
            case 42: // FOV Handling
                settings.fovHandling = (FOVHandling)s.enum_fov;
                break;
            case 43: // Corruption Glitches
                settings.corruptionGlitches = (bool)s.value_bool;
                break;
            case 44: // Screenshake
                settings.screenShake = (bool)s.value_bool;
                break;
            case 45: // Alert (Heat)
                settings.alert_heat = (int)s.value_int;
                break;
            case 46: // Alert (Core)
                settings.alert_core = (float)s.value_float;
                break;
            case 47: // Alert (Energy)
                settings.alert_energy = (float)s.value_float;
                break;
            case 48: // Alert (Matter)
                settings.alert_matter = (float)s.value_float;
                break;
            case 49: // Alert Popups
                settings.alertPopups = (bool)s.value_bool;
                break;
        }
    }

    /// <summary>
    /// Attempt to parse a preferences option based on an ID. Will return all important data about that preference so it can be visualized.
    /// </summary>
    /// <param name="id">An int ID refering to a single preference option.</param>
    /// <returns>Currently active preference (SSShort), Name of Preference (string), Bottom display text (string), List<(string, SSShort) of options></returns>
    public static (ScriptableSettingShort, string, string, List<(string, ScriptableSettingShort)>) ParsePreferencesOption(int id)
    {
        // Not too pleased with this but unsure of how else to approach it.

        ScriptableSettingShort value = new ScriptableSettingShort();
        string display = "";
        string bottomText = "";
        // (String to display on the box, what that option will change)
        List<(string, ScriptableSettingShort)> options = new List<(string, ScriptableSettingShort)>();

        ScriptablePreferences preferences = null;

        if (MainMenuManager.inst)
        {
            preferences = MainMenuManager.inst.preferencesObject;
        }
        else if (GlobalSettings.inst)
        {
            preferences = GlobalSettings.inst.preferences;
        }

        if (preferences == null)
        {
            Debug.LogError("ERROR: No preferences object detected. Preferences menu will break!");
        }

        // This is going to be very ugly.
        switch (id)
        {
            case 0: // Squads - Investigation
                value.value_bool = preferences.squads_investigation;
                value.canBeGrayedOut = true;
                display = "Investigation Squads";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or disables investigation squads entirely.";
                break;
            case 1: // Squads - Extermination
                value.value_bool = preferences.squads_extermination;
                value.canBeGrayedOut = true;
                display = "Extermination Squads";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or disables investigation squads entirely.";
                break;
            case 2: // Squads - Extermination MTTH
                value.value_int = preferences.extermination_mtth;
                display = "Extermination Squad MTTM";
                options.Add(("100", new ScriptableSettingShort(v_i: 100)));
                options.Add(("200", new ScriptableSettingShort(v_i: 200)));
                options.Add(("300", new ScriptableSettingShort(v_i: 300)));
                options.Add(("400", new ScriptableSettingShort(v_i: 400)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                options.Add(("600", new ScriptableSettingShort(v_i: 600)));
                options.Add(("700", new ScriptableSettingShort(v_i: 700)));
                options.Add(("800", new ScriptableSettingShort(v_i: 800)));
                options.Add(("900", new ScriptableSettingShort(v_i: 900)));
                options.Add(("1000", new ScriptableSettingShort(v_i: 1000)));
                bottomText = "The 'mean-time-to-happen' of extermination squad deployments. In practice this value will be the AVERAGE time between extermination squad dispatches.";
                break;
            case 3: // Hacking - Base Detection Chance
                value.value_float = preferences.hacking_baseDetectionChance;
                display = "Base Detection Chance";
                options.Add(("5%", new ScriptableSettingShort(v_f: 0.05f)));
                options.Add(("10%", new ScriptableSettingShort(v_f: 0.1f)));
                options.Add(("20%", new ScriptableSettingShort(v_f: 0.2f)));
                options.Add(("30%", new ScriptableSettingShort(v_f: 0.3f)));
                options.Add(("40%", new ScriptableSettingShort(v_f: 0.4f)));
                options.Add(("50%", new ScriptableSettingShort(v_f: 0.5f)));
                options.Add(("60%", new ScriptableSettingShort(v_f: 0.6f)));
                options.Add(("70%", new ScriptableSettingShort(v_f: 0.7f)));
                options.Add(("80%", new ScriptableSettingShort(v_f: 0.8f)));
                options.Add(("90%", new ScriptableSettingShort(v_f: 0.9f)));
                options.Add(("100%", new ScriptableSettingShort(v_f: 1.0f)));
                bottomText = "The base detection chance while hacking. Increasing this value will make hacking SIGNIFICANTLY more difficult.";
                break;
            case 4: // Evolve - Heal Between Floors
                value.value_bool = preferences.evolve_healBetweenFloors;
                value.canBeGrayedOut = true;
                display = "Heal Between Floors";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or disables the full health restoration effect between floors. Disabling this feature will make gameplay SIGNIFICANTLY more challenging.";
                break;
            case 5: // Evolve - Clear Corruption
                value.value_bool = preferences.evolve_clearCorruption;
                value.canBeGrayedOut = true;
                display = "Clear Corruption";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or disables the clearing of corruption between floors, where the player's corruption level is set to 0.";
                break;
            case 6: // Evolve - New Health Per Level
                value.value_int = preferences.evolve_newHealthPerLevel;
                display = "New Health Per Level";
                options.Add(("50", new ScriptableSettingShort(v_i: 50)));
                options.Add(("100", new ScriptableSettingShort(v_i: 100)));
                options.Add(("150", new ScriptableSettingShort(v_i: 150)));
                options.Add(("200", new ScriptableSettingShort(v_i: 200)));
                options.Add(("300", new ScriptableSettingShort(v_i: 300)));
                options.Add(("400", new ScriptableSettingShort(v_i: 400)));
                options.Add(("500", new ScriptableSettingShort(v_i: 500)));
                bottomText = "The amount of new max health recieved per evolution.";
                break;
            case 7: // Corruption - Enabled
                value.value_bool = preferences.corruption_enabled;
                value.canBeGrayedOut = true;
                display = "Corruption";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or Disabled the Corruption mechanic. WARNING: Corruption is a key aspect of the game, and disabling it will make the game less complex.";
                break;
            case 8: // Corruption - Do Effects
                value.value_bool = preferences.corruption_effects;
                value.canBeGrayedOut = true;
                display = "Corruption Effects";
                options.Add(("On", new ScriptableSettingShort(v_b: true)));
                options.Add(("Off", new ScriptableSettingShort(v_b: false, grayedOut: true)));
                bottomText = "Enables or Disabled the random effects that trigger from having certain levels of corruption. Does not disable corruption outright, but will make the game less interesting.";
                break;
        }


        return (value, display, bottomText, options);
    }

    /// <summary>
    /// A (very clumbsy) method that updating a specific preference given its ID and what to set the new value as.
    /// </summary>
    /// <param name="id">The ID of the preference (see switch statement).</param>
    /// <param name="s">The value to update the preference with (in the form of ScriptableSettingShort).</param>
    public static void UpdatePreferences(int id, ScriptableSettingShort s)
    {
        ScriptablePreferences preferences = MainMenuManager.inst ? MainMenuManager.inst.preferencesObject : GlobalSettings.inst.preferences;

        // This is going to be very ugly.
        switch (id)
        {
            case 0: // Squads - Investigation
                preferences.squads_investigation = (bool)s.value_bool;
                break;
            case 1: // Squads - Extermination
                preferences.squads_extermination = (bool)s.value_bool;
                break;
            case 2: // Squads - Extermination MTTH
                preferences.extermination_mtth = (int)s.value_int;
                break;
            case 3: // Hacking - Base Detection Chance
                preferences.hacking_baseDetectionChance = (float)s.value_float;
                break;
            case 4: // Evolve - Heal Between Floors
                preferences.evolve_healBetweenFloors = (bool)s.value_bool;
                break;
            case 5: // Evolve - Clear Corruption
                preferences.evolve_clearCorruption = (bool)s.value_bool;
                break;
            case 6: // Evolve - New Health Per Level
                preferences.evolve_newHealthPerLevel = (int)s.value_int;
                break;
            case 7: // Corruption - Enabled
                preferences.corruption_enabled = (bool)s.value_bool;
                break;
            case 8: // Corruption - Do Effects
                preferences.corruption_effects = (bool)s.value_bool;
                break;
        }
    }

    /// <summary>
    /// Generates some dummy (random) player save data for use in testing LOAD/SAVE game UI.
    /// </summary>
    public static (string, string, Vector2Int, Vector2Int, Vector2Int, Vector2Int, Vector2Int, Vector2Int, Vector2Int, Vector2Int, (List<ItemObject>, int), List<string>, int, Sprite) DummyPlayerSaveData()
    {
        // Name
        char[] alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char[] nums = "0123456789".ToCharArray();
        string name = $"GAMESAVE{alphabet[Random.Range(0, alphabet.Length - 1)]}{nums[Random.Range(0, nums.Length - 1)]}{alphabet[Random.Range(0, alphabet.Length - 1)]}{nums[Random.Range(0, nums.Length - 1)]}{alphabet[Random.Range(0, alphabet.Length - 1)]}";
        // Location
        string location = $"-{nums[Random.Range(0, nums.Length - 1)]}/STORAGE";
        // State
        int core = 0, energy = 0, matter = 0, corruption = 0;
        int core_max = Random.Range(300, 600), energy_max = Random.Range(300, 600), matter_max = Random.Range(300, 600), corruption_max = 100;
        core = Random.Range(0, core_max);
        energy = Random.Range(0, energy_max);
        matter = Random.Range(0, matter_max);
        corruption = Random.Range(0, corruption_max);

        // Slots
        int sPower = 0, sProp = 0, sUtil = 0, sWep = 0;
        int sPowerM = Random.Range(1, 5), sPropM = Random.Range(2, 6), sUtilM = Random.Range(2, 8), sWepM = Random.Range(2, 5);
        sPower = Random.Range(1, sPowerM);
        sProp = Random.Range(2, sPropM);
        sUtil = Random.Range(2, sUtilM);
        sWep = Random.Range(2, sWepM);

        // Items
        List<ItemObject> items = new List<ItemObject>();
        ItemDatabaseObject database = null;
        if (MainMenuManager.inst)
        {
            database = MainMenuManager.inst.itemDatabase;
        }
        else if (MapManager.inst)
        {
            database = MapManager.inst.itemDatabase;
        }

        for (int i = 0; i < Random.Range(1, 15); i++)
        {
            items.Add(database.Items[Random.Range(0, database.Items.Length - 1)]);
        }
        // and current max inventory size
        int maxInv = Random.Range(3, 15);

        // Special Conditions
        List<string> conditionOptions = new List<string>() { "FARCOM", "RIF", "IMPRINTED", "NEM", "CRM", "", "", "", "" };
        List<string> conditions = new List<string>();
        conditions.Add(conditionOptions[Random.Range(0, conditionOptions.Count - 1)]);

        // Kills
        int kills = Random.Range(0, 100);

        // Preview Image
        Sprite sprite = null;

        // Return it all
        return (
            name, location, 
            new Vector2Int(core, core_max), new Vector2Int(energy, energy_max), new Vector2Int(matter, matter_max), new Vector2Int(corruption, corruption_max), 
            new Vector2Int(sPower, sPowerM), new Vector2Int(sProp, sPropM), new Vector2Int(sUtil, sUtilM), new Vector2Int(sWep, sWepM), 
            (items, maxInv), conditions, kills, sprite
            );
    }
    
    /// <summary>
    /// Generates some dummy (random) hideout save data for use in testing HIDEOUT game UI.
    /// </summary>
    public static (int, string, int, List<ItemObject>, List<BotObject>, int) DummyHideoutSaveData()
    {
        // What are important save values to display here?
        /* -Hideout depth/level (Lower Caves/Upper Caves/Subcaves)
         * -Cached Matter
         * -Cached Items
         * -Bot Allies
         * -0b10 Awareness
         */

        int depth = Random.Range(-10, -4);
        List<string> levels = new List<string>() { "Lower Caves", "Upper Caves", "Subcaves" };
        string levelName = levels[Random.Range(0, levels.Count - 1)];

        int cached_matter = Random.Range(0, 500);

        List<ItemObject> cached_items = new List<ItemObject>();
        ItemDatabaseObject itemDB = MainMenuManager.inst ? MainMenuManager.inst.itemDatabase : MapManager.inst.itemDatabase;
        for (int i = 0; i < Random.Range(1, 8); i++)
        {
            cached_items.Add(itemDB.Items[Random.Range(0, itemDB.Items.Length - 1)]);
        }

        List<BotObject> bot_allies = new List<BotObject>();
        BotDatabaseObject botDB = MainMenuManager.inst ? MainMenuManager.inst.botDatabase : MapManager.inst.botDatabase;
        for (int i = 0; i < Random.Range(1, 8); i++)
        {
            bot_allies.Add(botDB.Bots[Random.Range(0, botDB.Bots.Length - 1)]);
        }

        int awareness = Random.Range(0, 100);

        return (depth, levelName, cached_matter, cached_items, bot_allies, awareness);
    }
    #endregion

    #region Spotting
    /// <summary>
    /// Replacement for the previous Raycast2D for the tilemap.
    /// </summary>
    /// <param name="start">Vector2Int start location.</param>
    /// <param name="end">Vector2Int end location.</param>
    /// <returns>A list of *Vector2Int* tile positions between the start and end position.</returns>
    public static List<Vector2Int> BresenhamLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> line = new List<Vector2Int>();

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int sx = (start.x < end.x) ? 1 : -1;
        int sy = (start.y < end.y) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            line.Add(start);

            if (start.x == end.x && start.y == end.y) break;

            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                start.x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                start.y += sy;
            }
        }

        return line;
    }

    /// <summary>
    /// Basically *LOSOnTarget* but returns the first thing blocking line of sight if there is anything.
    /// </summary>
    /// <param name="source">The origin location.</param>
    /// <param name="target">The target location.</param>
    /// <param name="requireVision">If the player needs to be able to see the blocking object.</param>
    /// <returns>A Vector2Int position of something that is in the way.</returns>
    public static Vector2Int ReturnObstacleInLOS(Vector2Int source, Vector2Int target, bool requireVision = false)
    {
        // Gather up the line of tiles between the source and the target.
        List<Vector2Int> line = BresenhamLine(source, target);

        // Toss out the start and finish
        line.Remove(source);
        line.Remove(target);

        Vector2Int blocker = Vector2Int.zero;

        if(line.Count > 0)
        {
            foreach (Vector2Int T in line)
            {
                // Vision check
                if (!requireVision || (requireVision && MapManager.inst.mapdata[T.x, T.y].vis == 2))
                {
                    // Is there a bot here?
                    if (MapManager.inst.pathdata[T.x, T.y] == 2)
                    {
                        return T; // There is!
                    }

                    // We can use the permiability function to check this
                    if (!HF.IsPermiableTile(MapManager.inst.mapdata[T.x, T.y]))
                    {
                        return T; // This is a blocker
                    }
                }
            }
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
        GameObject part = null;
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
            // -A part (item)
            // -A floor tile

            // We will solve this problem by setting flags. And then going back afterwards and using our heirarchy.

            #region Hierarchy Flagging
            if (hit.collider.GetComponent<Actor>())
            {
                // A bot
                bot = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Wall"))
            {
                // A wall
                wall = hit.collider.gameObject;
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
            else if (hit.collider.GetComponent<Part>())
            {
                // Part (Item)
                part = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor"))
            {
                // Floor tile
                floor = hit.collider.gameObject;
            }

            #endregion
        }

        GameObject retObject = null;

        if (bot != null)
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
        else if (wall != null)
        {
            retObject = wall;
        }
        else if(part != null)
        {
            retObject = part;
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

    /// <summary>
    /// Given an actor, returns all other Actors within this actor's Field-Of-View as a list.
    /// </summary>
    /// <param name="viewer">The Actor we are checking the FOV of.</param>
    /// <param name="onlyFoes">If true, returns only HOSTILE bots in the FOV.</param>
    /// <returns>A list of all Actors that are within the Field Of View of the specified actor.</returns>
    public static List<Actor> BotsInActorFOV(Actor viewer, bool onlyFoes = false)
    {
        List<Actor> inFOV = new List<Actor>();

        foreach (var E in GameManager.inst.Entities) // This is a bit rough (could this function be done better?)
        {
            Actor a = E.GetComponent<Actor>();
            if (viewer != a && ActorInBotFOV(viewer, a))
            {
                if (onlyFoes)
                {
                    if(viewer.allegances.GetRelation(a.myFaction) == BotRelation.Hostile)
                    {
                        inFOV.Add(a);
                    }
                }
                else
                {
                    inFOV.Add(a);
                }
            }
        }

        return inFOV;
    }
    #endregion
}
