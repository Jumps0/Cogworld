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

            slotsOnInterface.Add(obj, InventoryControl.inst.p_inventory.Container.Items[i]);
        }

    }
}
