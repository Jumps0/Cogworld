using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Repair Station vF.08n
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
    public List<TrojanType> trojans = new List<TrojanType>();

    [Header("Operation")]
    public ItemObject targetPart = null;
    public int timeToComplete;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;
    // -- Build -- //
    public int begunBuildTime = 0;
    public GameObject timerObject = null;

    public void Init()
    {
        // We need to load this machine with the following commands:

    }

    public void Scan(ItemObject item, int time)
    {
        targetPart = item;
        timeToComplete = time;
    }

    public void Repair()
    {

    }

    public void Check()
    {

    }
    
}
