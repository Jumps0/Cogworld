using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class HackObject : ScriptableObject
{
   
    [Header("Overview")]
    public int Id;
    public string trueName;
    public MachineType relatedMachine;
    public TerminalCommandType hackType;

    [Header("Success Chance")]
    [Header("    Direct")]
    public Vector3Int directChance;
    [Header("    Indirect")]
    [Tooltip("In most cases there is no data on this, so if its unset we will just -10% from each part.")]
    public Vector3Int indirectChance;
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