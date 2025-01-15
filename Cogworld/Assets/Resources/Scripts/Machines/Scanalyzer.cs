using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanalyzer : InteractableMachine
{
    [Header("Operation")]
    public ItemObject targetPart = null;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;

    public void Init()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Scanalyzer;

        // We need to load this machine with the following commands:

    }

    public void Check()
    {

    }
}
