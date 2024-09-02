using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// Recycle components by loading them from your inventory and initiating the process. Recycling units collect matter until they reach a certain quota (500), 
/// after which it is transferred away to a central system. Instruct the unit to report matter to examine its current stores. 
/// Hack the retrieve matter command to have it eject its contents.
///
/// Similarly, unprocessed parts can be listed and retrieved via other system commands.
/// </summary>
public class RecyclingUnit : MonoBehaviour
{
    public Vector2Int _size;

    [Header("Identification")]
    public string fullName;
    /// <summary>
    /// EX: Recycling vF.08n
    /// </summary>
    public string systemName;

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
    public ItemObject targetPart = null;
    public bool working = false;
    [Tooltip("Where completed components get spawned.")]
    public Transform ejectionSpot;
    public int storedMatter;
    public InventoryObject storedComponents; // Need a unique inventory to track this

    public void Init()
    {
        // Setup component inventory
        storedComponents = new InventoryObject(10, systemName + "'s component Inventory");

        // We need to load this machine with the following commands:

    }

    public void Check()
    {

    }

    private void Update()
    {
        if (!locked)
        {
            OverflowCheck();
        }
    }

    // If the stored matter goes above 500 it is reset. If components stored goes above 10 that inventory is emptied.
    private void OverflowCheck()
    {
        if(storedMatter > 500)
        {
            storedMatter = 0;
        }
        if(storedComponents.Container.Items.Length > 10)
        {
            // Reset the inventory but keep the most recently added item
            Item final = storedComponents.Container.Items[0].item;

            storedComponents.Container.Clear();
            storedComponents.AddItem(final);
        }
    }
}
