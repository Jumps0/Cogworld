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

            var obj = slots[i];
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

            InventoryControl.inst.p_inventory.Container.Items[i].parent = this;
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventory.Container.Items[i]);
        }

    }

    public void Update()
    {
        
    }

    public void UpdateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

        foreach (Transform child in this.gameObject.transform) // Clear out the old gameobjects
        {
            if (child.gameObject.GetComponent<InvDisplayItem>())
            {
                Destroy(child.gameObject);
            }
        }
        slots = new GameObject[InventoryControl.inst.p_inventory.Container.Items.Length];

        for (int i = 0; i < InventoryControl.inst.p_inventory.Container.Items.Length; i++)
        {

            var obj = slots[i];
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

            InventoryControl.inst.p_inventory.Container.Items[i].parent = this;
            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventory.Container.Items[i]);
        }
    }
}
