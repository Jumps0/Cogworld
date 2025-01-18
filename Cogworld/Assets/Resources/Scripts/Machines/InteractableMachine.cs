using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parent class for all Interactable Machines (Terminals, Garrisons, Fabricators, etc.). Contains shared information used across all TYPES of machines.
/// </summary>
public class InteractableMachine : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Fabricator vF.08n
    /// </summary>
    public string specialName;
    public MachineType type;

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
    public int timesAccessed = 0;

    [Header("Trojans")]
    public List<HackObject> trojans = new List<HackObject>();
}
