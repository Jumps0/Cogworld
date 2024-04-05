using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garrison : MonoBehaviour
{
    public Vector2Int _size;
    public bool doorRevealed = false; // The player can now ENTER the garrison.
    public bool g_sealed = false; // This garrison is permanently closed.

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Garrison Terminal
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
    //
    public bool trojan_broadcast = false;
    public bool trojan_decoy = false;
    public bool trojan_redirect = false;
    public bool trojan_reprogram = false;

    public void Init()
    {
        // We need to load this machine with the following commands:

    }

    public void UnlockAccess()
    {
        doorRevealed = true;
    }

    public void SealAccess()
    {
        g_sealed = true;
        this.GetComponentInChildren<AudioSource>().loop = false;
        this.GetComponentInChildren<AudioSource>().playOnAwake = false;
        this.GetComponentInChildren<AudioSource>().PlayOneShot(AudioManager.inst.DOOR_Clips[3]);
    }

    public void CouplerStatus()
    {

    }
}
