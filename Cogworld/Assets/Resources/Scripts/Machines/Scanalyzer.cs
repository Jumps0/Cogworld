using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.gridsagegames.com/blog/2013/12/scanalyzers-fabricators/
public class Scanalyzer : MonoBehaviour
{
    [Header("Operation")]
    public ItemObject targetPart = null;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;

    public void Init()
    {
        //detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        //type = MachineType.Scanalyzer;

        // We need to load this machine with the following commands:

    }

    // NOTE: Higher level scanalyzers are required to scan prototypes and more advanced parts, and scanalyzers will reject broken or faulty parts.
    public void Check()
    {

    }
}
