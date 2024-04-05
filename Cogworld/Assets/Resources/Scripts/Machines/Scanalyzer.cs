using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanalyzer : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Scanalyzer vHe.07a
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

    [Header("Items")]
    public ItemObject targetPart = null;


    public void Init()
    {
        // We need to load this machine with the following commands:

    }
}
