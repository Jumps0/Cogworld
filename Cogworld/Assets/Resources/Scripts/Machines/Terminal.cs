using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Terminal : MonoBehaviour
{
    public Vector2Int centerLoc; // Where this terminal is in the grid.

    public TerminalZone zone;
    public Actor assignedBot; // (Technician, Administrator, etc)

    public List<TerminalCustomCode> customCodes = new List<TerminalCustomCode>();

    public List<ItemObject> storedObjects;

    public bool databaseLockout = false;

    [Header("Audio")]
    public AudioSource _source;
    public AudioClip _ambient;

    public void Init()
    {
        
    }

    public void UseCustomCode(TerminalCustomCode code)
    {

    }
}