using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class HackObject : ScriptableObject
{
   
    [Header("Overview")]
    public int Id;
    public string trueName;
    [TextArea(3,5)]
    public string description;
    public MachineType relatedMachine;
    public TerminalCommandType hackType;

    [Header("Success Chance")]
    [Header("    Direct")]
    public Vector3Int directChance;
    [Header("    Indirect")]
    [Tooltip("In most cases there is no data on this, so if its unset we will just -10% from each part.")]
    public Vector3Int indirectChance;

    [Tooltip("If true, this will not appear in the suggestions box for manual hacking.")]
    public bool doNotSuggest = false;
}


[System.Serializable]
public enum MachineType
{
    Fabricator,
    Garrison,
    Recycling,
    RepairStation,
    Scanalyzer,
    Terminal,
    CustomTerminal,
    DoorTerminal,
    Misc
}

[System.Serializable]
// https://www.gridsagegames.com/blog/2018/08/65-robot-hacks/
[Tooltip("These are datajack hacks against bots which modify the target in some way, usually affecting behavior or hit chances.")]
public enum ModHacks
{
    [Tooltip("Force system to prioritize attacking Cogmind wherever possible.")]
    focus_fire, // TODO
    [Tooltip("Render propulsion unusable for 15 turns while the control subsystem reboots.")]
    reboot_propulsion, // TODO
    [Tooltip("Rewrite sections of the propulsion control subsystem to permanently halve movement speed.")]
    tweak_propulsion, // TODO
    [Tooltip("Offset targeting algorithms to approximately halve their chance of achieving optimal aim.")]
    scatter_targeting, // Done
    [Tooltip("Mark this bot to make it the most attractive target to all allies, as well as improve accuracy against it by 10%.")]
    mark_system, // TODO (2nd part done)
    [Tooltip("Tap into system’s combat decision-making processes in real time, increasing your accuracy and damage against this bot by 25% while also impairing its accuracy against you by 25%.")]
    link_complan, // Done
    [Tooltip("Openly share the local combat network’s defensive coordination values, giving yourself and all allies 25% better accuracy against this and all 0b10 combat bots within a range of 3.")]
    broadcast_data, // Done
    [Tooltip("Actively interfere with the local combat network’s offensive coordination calculations, reducing the accuracy of this bot by 25%, and that of all 0b10 combat bots within a range of 3.")]
    disrupt_area // Done
}