using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// https://www.gridsagegames.com/blog/2013/12/scanalyzers-fabricators/
public class Fabricator : MonoBehaviour 
{
    [Header("Operation")]
    public ItemObject targetPart = null;
    public BotObject targetBot = null;
    [Tooltip("How long it will take to build the specified componenet.")]
    public int buildTime;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;

    [Header("Special Flags")]
    public bool flag_overload = false;

    // TODO FUTURE WORK: AUTHCHIPS
    // https://www.gridsagegames.com/blog/2021/11/design-overhaul-4-fabrication-2-0/

    public void Init()
    {
        

    }

    #region Operation
    


    // -- Load -- //
    

    // -- Build -- //
    

    

    
    #endregion

    
}
