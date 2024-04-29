using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This is the interface over the player's / INVENTORY /.
/// </summary>
public class StaticInterface : UserInterface
{
    public GameObject[] slots;

    public override void CreateSlots()
    {
        
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < InventoryControl.inst.p_inventory.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            slots[i] = obj;

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

            // Set parent and store it
            InventoryControl.inst.p_inventory.Container.Items[i].parent = this;
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventory.Container.Items[i]);
        }

    }

    public void Update()
    {
        
    }

    public void UpdateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>(); // Reset list

        foreach (Transform child in inventoryArea.transform) // Clear out the old gameobjects
        {
            if (child.gameObject.GetComponent<InvDisplayItem>())
            {
                Destroy(child.gameObject);
            }
        }

        slots = new GameObject[InventoryControl.inst.p_inventory.Container.Items.Length]; // Reset array

        for (int i = 0; i < InventoryControl.inst.p_inventory.Container.Items.Length; i++)
        {
            // Create a new *InvDisplayItem* object
            var obj = Instantiate(prefab_item, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            slots[i] = obj;

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

            // Set parent and store it
            InventoryControl.inst.p_inventory.Container.Items[i].parent = this;
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventory.Container.Items[i]);
        }
    }
}
