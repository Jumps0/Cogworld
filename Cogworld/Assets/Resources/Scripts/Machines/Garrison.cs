using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Garrison : MonoBehaviour
{
    public bool doorRevealed = false; // The player can now ENTER the garrison.
    public bool g_sealed = false; // This garrison is permanently closed.

    [Header("Operation")]
    [Tooltip("Where arriving bots are spawned, or the access point is created.")]
    public Transform ejectionSpot;
    public List<Item> couplers = new List<Item>();

    [Header("Special Flags")]
    [Tooltip("This Garrison Access is communicating with additional reinforcements preparing for dispatch. Using a Signal Interpreter provides the precise number of turns remaining until the next dispatch.")]
    public bool s_transmitting = false;
    public bool s_redeploying = false;

    public void Init()
    {
        
    }

    public void Open()
    {
        
    }

    public void Seal()
    {
        
    }

    public void CouplerStatus()
    {
        // Nothing happens here?
    }

    
}
