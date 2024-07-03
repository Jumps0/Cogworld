using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This is the interface over the player's / PARTS /.
/// </summary>
public class DynamicInterface : UserInterface
{
    public List<InventoryObject> inventories = new List<InventoryObject>();

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        // -- POWER -- //

        var newPower = Instantiate(UIManager.inst.powerPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Power Header
        UIManager.inst.instPower = newPower;

        for (int i = 0; i < InventoryControl.inst.p_inventoryPower.Container.Items.Length; i++)
        {
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>().item = null;
            obj.GetComponent<InvDisplayItem>().SetEmpty();
            obj.GetComponent<InvDisplayItem>().my_interface = this;

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnterInterface(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExitInterface(obj); });

            InventoryControl.inst.p_inventoryPower.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryPower.Container.Items[i].AllowedItems[0] = ItemSlot.Power; // Restrict the slot to only *Power* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryPower.Container.Items[i]);
        }

        // -- PROPULSION -- //

        var newProp = Instantiate(UIManager.inst.propulsionPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Propulsion Header
        UIManager.inst.instPropulsion = newProp;

        for (int i = 0; i < InventoryControl.inst.p_inventoryPropulsion.Container.Items.Length; i++)
        {
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>().item = null;
            obj.GetComponent<InvDisplayItem>().SetEmpty();
            obj.GetComponent<InvDisplayItem>().my_interface = this;

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnterInterface(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExitInterface(obj); });

            InventoryControl.inst.p_inventoryPropulsion.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryPropulsion.Container.Items[i].AllowedItems[0] = ItemSlot.Propulsion; // Restrict the slot to only *Propulsion* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryPropulsion.Container.Items[i]);
        }

        // -- UTILITY -- //

        var newUtility = Instantiate(UIManager.inst.utilitiesPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Utility Header
        UIManager.inst.instUtilities = newUtility;

        for (int i = 0; i < InventoryControl.inst.p_inventoryUtilities.Container.Items.Length; i++)
        {
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>().item = null;
            obj.GetComponent<InvDisplayItem>().SetEmpty();
            obj.GetComponent<InvDisplayItem>().my_interface = this;

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnterInterface(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExitInterface(obj); });

            InventoryControl.inst.p_inventoryUtilities.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryUtilities.Container.Items[i].AllowedItems[0] = ItemSlot.Utilities; // Restrict the slot to only *Utilities* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryUtilities.Container.Items[i]);
        }

        // -- WEAPON -- //

        var newWeapon = Instantiate(UIManager.inst.weaponPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Weapon Header
        UIManager.inst.instWeapons = newWeapon;

        for (int i = 0; i < InventoryControl.inst.p_inventoryWeapons.Container.Items.Length; i++)
        {
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>().item = null;
            obj.GetComponent<InvDisplayItem>().SetEmpty();
            obj.GetComponent<InvDisplayItem>().my_interface = this;

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnterInterface(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExitInterface(obj); });

            InventoryControl.inst.p_inventoryWeapons.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryWeapons.Container.Items[i].AllowedItems[0] = ItemSlot.Weapons; // Restrict the slot to only *Weapon* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryWeapons.Container.Items[i]);
        }
    }

    public void UpdateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>(); // Reset list

        foreach (Transform child in inventoryArea.transform) // Clear out the old gameobjects (headers included)
        {
            Destroy(child.gameObject);
        }

        // -- POWER -- //

        var newPower = Instantiate(UIManager.inst.powerPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Power Header
        UIManager.inst.instPower = newPower;

        for (int i = 0; i < InventoryControl.inst.p_inventoryPower.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = CreateNewEmptySlot();

            // Set parent and store it
            InventoryControl.inst.p_inventoryPower.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryPower.Container.Items[i].AllowedItems[0] = ItemSlot.Power; // Restrict the slot to only *Power* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryPower.Container.Items[i]);
        }

        // -- PROPULSION -- //

        var newProp = Instantiate(UIManager.inst.propulsionPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Propulsion Header
        UIManager.inst.instPropulsion = newProp;

        for (int i = 0; i < InventoryControl.inst.p_inventoryPropulsion.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = CreateNewEmptySlot();

            // Set parent and store it
            InventoryControl.inst.p_inventoryPropulsion.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryPropulsion.Container.Items[i].AllowedItems[0] = ItemSlot.Propulsion; // Restrict the slot to only *Propulsion* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryPropulsion.Container.Items[i]);
        }

        // -- UTILITY -- //

        var newUtility = Instantiate(UIManager.inst.utilitiesPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Utility Header
        UIManager.inst.instUtilities = newUtility;

        for (int i = 0; i < InventoryControl.inst.p_inventoryUtilities.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = CreateNewEmptySlot();

            // Set parent and store it
            InventoryControl.inst.p_inventoryUtilities.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryUtilities.Container.Items[i].AllowedItems[0] = ItemSlot.Utilities; // Restrict the slot to only *Utilities* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryUtilities.Container.Items[i]);
        }

        // -- WEAPON -- //

        var newWeapon = Instantiate(UIManager.inst.weaponPrefab, new Vector3(), Quaternion.identity, inventoryArea.transform); // Instantiate Weapon Header
        UIManager.inst.instWeapons = newWeapon;

        for (int i = 0; i < InventoryControl.inst.p_inventoryWeapons.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = CreateNewEmptySlot();

            // Set parent and store it
            InventoryControl.inst.p_inventoryWeapons.Container.Items[i].parent = this;
            InventoryControl.inst.p_inventoryWeapons.Container.Items[i].AllowedItems[0] = ItemSlot.Weapons; // Restrict the slot to only *Weapon* items
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventoryWeapons.Container.Items[i]);
        }
    }

    public GameObject CreateNewEmptySlot()
    {
        // Create a new *InvDisplayItem* object
        var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);

        // Set it to empty
        obj.GetComponent<InvDisplayItem>().item = null;
        obj.GetComponent<InvDisplayItem>().SetEmpty();
        obj.GetComponent<InvDisplayItem>().my_interface = this;

        // Add events
        AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
        AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
        AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
        AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

        AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnterInterface(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExitInterface(obj); });

        return obj;
    }
}
