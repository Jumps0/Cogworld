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
    public bool locked = false; // No longer accessable

    [Header("Trojans")]
    public List<TrojanType> trojans = new List<TrojanType>();

    [Header("Operation")]
    [Tooltip("Where arriving bots are spawned, or the access point is created.")]
    public Transform ejectionSpot;

    [Header("Special Flags")]
    [Tooltip("This Garrison Access is communicating with additional reinforcements preparing for dispatch. Using a Signal Interpreter provides the precise number of turns remaining until the next dispatch.")]
    public bool s_transmitting = false;
    public bool s_redeploying = false;

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
        this.GetComponentInChildren<AudioSource>().PlayOneShot(AudioManager.inst.DOOR_Clips[3]); // GARRISON_SEAL
    }

    public void CouplerStatus()
    {

    }
}
