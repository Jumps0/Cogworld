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
        

    }

    
}
