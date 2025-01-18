using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : InteractableMachine
{
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
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.RepairStation;

        // We need to load this machine with the following commands:

    }

    public void Scan(ItemObject item, int time)
    {
        targetPart = item;
        timeToComplete = time;
    }

    // https://www.gridsagegames.com/blog/2014/01/recycling-units-repair-stations/
    // TODO: Repair stations cannot repair faulty prototypes or deteriorating parts
    public void Repair()
    {

    }

    public void Check()
    {

    }
    
}
