using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : MonoBehaviour
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
        
    }

    

    
    
}
