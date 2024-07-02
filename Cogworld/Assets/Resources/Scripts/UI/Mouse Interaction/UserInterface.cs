using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UserInterface : MonoBehaviour
{
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    public InventoryObject _inventory;

    [Tooltip("The object that only displays the item's name in gray while its being moved around the menu.")]
    public GameObject prefab_movingItem;
    [Tooltip("The actual gameObject that represents the item on the UI.")]
    public GameObject prefab_item;
    public GameObject inventoryArea;

    public void StartUp()
    {
        for (int i = 0; i < _inventory.Container.Items.Length; i++)
        {
            _inventory.Container.Items[i].parent = this;
        }

        if (this.GetComponent<DynamicInterface>())
        {
            foreach (var I in this.GetComponent<DynamicInterface>().inventories)
            {
                foreach (var slot in I.Container.Items)
                {
                    slot.parent = this;
                }
            }
        }

        slotsOnInterface.UpdateSlotDisplay();
    }

    public abstract void CreateSlots();

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = null;

        if (obj.GetComponent<InvDisplayItem>())
        {
            Button _b = obj.GetComponent<InvDisplayItem>()._button; // Convoluted workaround due to null errors
            trigger = _b.GetComponent<EventTrigger>();
        }
        else
        {
            trigger = this.GetComponent<EventTrigger>();
        }

        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        MouseData.slotHoveredOver = obj;
    }

    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
    }

    public void OnEnterInterface(GameObject obj)
    {
        UserInterface found = null;
        if (obj.GetComponent<InvDisplayItem>()) // Check if this is an item, and we can get the interface from that.
        {
            found = obj.GetComponent<InvDisplayItem>().my_interface;
        }
        else // Interface is probably on the object itself
        {
            found = obj.GetComponent<UserInterface>();
        }

        MouseData.interfaceMouseIsOver = found;
    }

    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnDragStart(GameObject obj)
    {
        if(obj.GetComponent<InvDisplayItem>().item != null) // Don't drag empty items.
        {
            if (obj.GetComponent<InvDisplayItem>().isSecondaryItem) // If we start dragging the secondary item, set the obj to be the parent instead.
            {
                obj = obj.GetComponent<InvDisplayItem>().secondaryParent;
            }

            MouseData.tempItemBeingDragged = CreatetempItem(obj);
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[27]);
        }
            
    }

    public GameObject CreatetempItem(GameObject obj)
    {
        GameObject tempItem = null;

        if (slotsOnInterface[obj].item.Id >= 0) // If this object does exist on our inventory
        {
            // Create a (visual) copy of the item that gets attached to the mouse
            tempItem = Instantiate(prefab_movingItem, Vector3.zero, Quaternion.identity, inventoryArea.transform);

            var rt = tempItem;
            //CopyInvDisplayItem(obj.GetComponent<InvDisplayItem>(), rt.GetComponent<InvDisplayItem>());

            bool destroyOnRemove = false;
            if (slotsOnInterface[obj].item.itemData.destroyOnRemove && obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<DynamicInterface>()) // It should be red if it will be destroyed on remove
            {
                destroyOnRemove = true;
            }

            rt.GetComponent<InvMovingDisplayItem>().Setup(obj.GetComponent<InvDisplayItem>().item.itemData.itemName, destroyOnRemove);
            tempItem.transform.SetParent(inventoryArea.transform.parent.transform.parent); // Set parent to *[RightCore] - Area* so that it isn't layered under any of the menus.

            // Is there an actual item there?
            if (slotsOnInterface[obj].item.Id >= 0)
            {
                rt.GetComponent<InvMovingDisplayItem>().SetUnRaycast();
            }
        }

        return tempItem;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj">The InventorySlot which holds the item we are currently dragging (aka Origin). Most data is inside the [InvDisplayItem] attached to it.</param>
    public void OnDragEnd(GameObject obj)
    {
        if (obj.GetComponent<InvDisplayItem>().item == null || obj.GetComponent<InvDisplayItem>().isSecondaryItem) // Don't drag empty or secondary items.
            return;

        Destroy(MouseData.tempItemBeingDragged);

        if(MouseData.interfaceMouseIsOver == null && obj.GetComponent<InvDisplayItem>().item != null) // If we're not over an inventory & we are dragging a real item
        {
            #region Item Dropping
            // Before we actually try to drop the item, we want to consider if attempting to do so would destroy it.
            if (obj.GetComponent<InvDisplayItem>().item.itemData.destroyOnRemove && obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<DynamicInterface>()) // Also only do this in /PARTS/ menu
            {
                // NOTE: Currently there exists no multi-slot force discardable items IN THE GAME. So we don't have to worry about it ;)
                #region Item Force Discarding
                // First of all, if this is the first time we try to do this, we want to warn the player.
                if (!obj.GetComponent<InvDisplayItem>().discard_readyToDestroy) // First time, give a warning, and start the reset time
                {
                    obj.GetComponent<InvDisplayItem>().StartDiscardTimeout();
                    UIManager.inst.ShowCenterMessageTop($"Will discard {obj.GetComponent<InvDisplayItem>().item.itemData.itemName}, repeat to confirm", UIManager.inst.dangerRed, Color.black);
                }
                else // Not the first time, destroy the item
                {
                    string key = obj.GetComponent<InvDisplayItem>()._assignedChar;
                    string name = obj.GetComponent<InvDisplayItem>().nameUnmodified;

                    // Stop the animation
                    obj.GetComponent<InvDisplayItem>().DiscardForceStop();

                    // Make a log message
                    UIManager.inst.CreateNewLogMessage("Discarded " + obj.GetComponent<InvDisplayItem>().item.itemData.itemName + ".", UIManager.inst.cautiousYellow, UIManager.inst.dullGreen, false, true); // Do a UI message

                    // Play the discard sound
                    AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[82]); // UI/PT_DISC

                    _inventory.RemoveItem(obj.GetComponent<InvDisplayItem>().item); // Remove item from inventory

                    // Update player mass
                    PlayerData.inst.currentWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.mass;
                    if (obj.GetComponent<InvDisplayItem>().item.itemData.propulsion.Count > 0)
                    {
                        PlayerData.inst.maxWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.propulsion[0].support;
                    }

                    // Subtract 10 energy
                    PlayerData.inst.currentEnergy -= 10;
                    PlayerData.inst.currentEnergy = Mathf.Clamp(PlayerData.inst.currentEnergy, 0, PlayerData.inst.maxEnergy);

                    // Update UI
                    UIManager.inst.UpdatePSUI();
                    UIManager.inst.UpdateInventory();
                    UIManager.inst.UpdateParts();

                    slotsOnInterface[obj].RemoveItem(); // Remove from slot
                    InventoryControl.inst.animatedItems.Remove(slotsOnInterface[obj]); // Remove from animation tracking HashSet
                    InventoryControl.inst.UpdateInterfaceInventories(); // Update UI

                    // Lastly, have the reference object do a little animation to show the item is gone.
                    obj.GetComponent<InvDisplayItem>().DiscardedAnimation(key, name);
                }

                // We also want to turn off any other warnings that may be on right now (we want to focus on this).
                foreach (Transform IDI in obj.GetComponent<InvDisplayItem>().my_interface.inventoryArea.transform)
                {
                    if (IDI.gameObject.GetComponent<InvDisplayItem>())
                    {
                        InvDisplayItem reference = IDI.gameObject.GetComponent<InvDisplayItem>();
                        if (reference.item != null && reference.item.Id >= 0 && reference != obj.GetComponent<InvDisplayItem>())
                        {
                            if (reference.item.itemData.destroyOnRemove && reference.discard_readyToDestroy)
                            {
                                reference.DiscardSetAsNormal();
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                // Drop the item on the floor
                InventoryControl.inst.DropItemOnFloor(obj.GetComponent<InvDisplayItem>().item, PlayerData.inst.GetComponent<Actor>(), _inventory);

                #region Multi-Slot items
                if (obj.GetComponent<InvDisplayItem>().item.itemData.slotsRequired > 1)
                {
                    foreach (var C in obj.GetComponent<InvDisplayItem>().secondaryChildren.ToList())
                    {
                        InventoryControl.inst.DropItemOnFloor(C.GetComponent<InvDisplayItem>().item, PlayerData.inst.GetComponent<Actor>(), _inventory);
                        InventoryControl.inst.animatedItems.Remove(slotsOnInterface[C]); // Remove from animation tracking HashSet
                        slotsOnInterface[C].RemoveItem(); // Remove from slots
                    }
                }
                #endregion

                // Update player mass
                PlayerData.inst.currentWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.mass;
                if (obj.GetComponent<InvDisplayItem>().item.itemData.propulsion.Count > 0)
                {
                    PlayerData.inst.maxWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.propulsion[0].support;
                }

                // Subtract 10 energy
                PlayerData.inst.currentEnergy -= 10;
                PlayerData.inst.currentEnergy = Mathf.Clamp(PlayerData.inst.currentEnergy, 0, PlayerData.inst.maxEnergy);

                // Update UI
                UIManager.inst.UpdatePSUI();
                UIManager.inst.UpdateInventory();
                UIManager.inst.UpdateParts();

                InventoryControl.inst.animatedItems.Remove(slotsOnInterface[obj]); // Remove from animation tracking HashSet
                slotsOnInterface[obj].RemoveItem(); // Remove from slots
                InventoryControl.inst.UpdateInterfaceInventories(); // Update UI
            }
            return;
            #endregion
        }

        if (MouseData.slotHoveredOver && obj.GetComponent<InvDisplayItem>().item != null 
            && MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item.Id >= 0) // Are we hovering over a slot with an item? Lets attempt to swap the two parts.
        {
            if(MouseData.slotHoveredOver == obj) // If we are attempting to swap the item with itself, cancel and create a message.
            {
                UIManager.inst.ShowCenterMessageTop("Swap cancelled", UIManager.inst.highlightGreen, Color.black);
                return;
            }

            #region Item Swapping
            /* == IDENTIFICATION GUIDE ==
            * 
            *  MouseData.slotHoveredOver                                                  | This is the DESTINATION *gameObject* (since we are currently hovering over it)
            *  MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver] | This is the DESTINATION *InventorySlot*
            *                                                                             |
            *  obj.GetComponent<InvDisplayItem>()                                         | This is the ORIGIN's *gameObject* (that we are dragging from)
            *  obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj]      | This is the ORIGIN's *InventorySlot*
            */

            // Get data from the two things we are trying to swap
            #region Information Grabbing
            GameObject obj_origin = obj;
            GameObject obj_destination = MouseData.slotHoveredOver;

            Item originItem = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj].item;
            Item destinationItem = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item;

            InventorySlot originSlot = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj]; // Get data from the slot we are swapping with
            InventorySlot destinationSlot = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver]; // Get data from slot hovered over

            int size_origin = originItem.itemData.slotsRequired;
            int size_destination = destinationItem.itemData.slotsRequired;

            int free_origin = 0;
            int free_destination = 0;

            switch (originItem.itemData.slot)
            {
                case ItemSlot.Power:
                    free_origin = PlayerData.inst.GetComponent<PartInventory>()._invPower.EmptySlotCount;
                    break;
                case ItemSlot.Propulsion:
                    free_origin = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.EmptySlotCount;
                    break;
                case ItemSlot.Utilities:
                    free_origin = PlayerData.inst.GetComponent<PartInventory>()._invUtility.EmptySlotCount;
                    break;
                case ItemSlot.Weapons:
                    free_origin = PlayerData.inst.GetComponent<PartInventory>()._invWeapon.EmptySlotCount;
                    break;
                default: // Inventory
                    free_origin = PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount;
                    break;
            }

            switch (destinationItem.itemData.slot)
            {
                case ItemSlot.Power:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invPower.EmptySlotCount;
                    break;
                case ItemSlot.Propulsion:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.EmptySlotCount;
                    break;
                case ItemSlot.Utilities:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invUtility.EmptySlotCount;
                    break;
                case ItemSlot.Weapons:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invWeapon.EmptySlotCount;
                    break;
                default: // Inventory
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount;
                    break;
            }

            // If the item is multi-slot we need to gather up its children and their objects
            List<KeyValuePair<GameObject, InventorySlot>> children_destination = null, children_origin = null;
            if (size_origin > 1 || size_destination > 1)
            {
                if(size_origin > 1)
                {
                    children_origin = new List<KeyValuePair<GameObject, InventorySlot>>();
                    foreach (var C in obj_origin.GetComponent<InvDisplayItem>().secondaryChildren) // [ORIGIN]
                    {
                        children_origin.Add(new KeyValuePair<GameObject, InventorySlot>(C, obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[C]));
                    }
                }
                    
                if (size_destination > 1)
                {
                    children_destination = new List<KeyValuePair<GameObject, InventorySlot>>();
                    foreach (var C in obj_destination.GetComponent<InvDisplayItem>().secondaryChildren) // [DESTINATION]
                    {
                        children_destination.Add(new KeyValuePair<GameObject, InventorySlot>(C, MouseData.interfaceMouseIsOver.slotsOnInterface[C]));
                    }
                }
            }
            #endregion

            #region Can this item be swapped into this slot?
            bool canSwap = true;

            // First check if both items will be compatible with their new slots (there are a couple different cases here).
            // 1) Check to see if the items are actually in the slot "area" (aka both in inventory/both in power,propulsion,utility,weapon)
            if ((originSlot.AllowedItems.Count == 0 && destinationSlot.AllowedItems.Count == 0)) // Both items share the same (inventory) slot type, obviously we shouldn't swap these.
            {
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
                return; // Bail out
            }
            else if(originSlot.AllowedItems.Count > 0 && destinationSlot.AllowedItems.Count > 0 && originSlot.AllowedItems[0] == destinationSlot.AllowedItems[0])
            { // Both items share the same (parts) slot type, we shouldn't swap these either.
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
                return; // Bail out
            }

            // 2) Since the slots don't share the same area/type, we need to make sure the items will be compatible with their new slot.
            // We need to check the following:

            // -If the [DESTINATION] slot is in the inventory, that it has enough free space.
            // -If the [ORIGIN] slot is in the inventory, that it has enough free space.
            canSwap = ((obj_destination.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>() && free_destination >= size_destination)
                || (obj_origin.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>() && free_origin >= size_origin));

            // -If the [DESTINATION] slot is NOT in the inventory, that the [ORIGIN] item type is allowed by the slot.
            if (!obj_destination.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>())
            {
                if ((destinationSlot.AllowedItems.Count > 0 && destinationSlot.AllowedItems.Contains(originItem.itemData.slot)) || destinationSlot.AllowedItems.Count == 0)
                {
                    canSwap = true;
                }
                else
                {
                    canSwap = false;
                }
            }
            // -If the [ORIGIN] slot is NOT in the inventory, that the [DESTINATION] item type is allowed by the slot.
            if (!obj_origin.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>())
            {
                if ((originSlot.AllowedItems.Count > 0 && originSlot.AllowedItems.Contains(destinationItem.itemData.slot)) || originSlot.AllowedItems.Count == 0)
                {
                    canSwap = true;
                }
                else
                {
                    canSwap = false;
                }
            }

            #endregion

            if (canSwap) // We can swap the items!
            {
                // With multi-slot items there may be cases where we need to force auto-sort the items on display so a new item can "fit". We do this check here.
                if(free_destination > 1 && size_origin > 1 && !obj_destination.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>())
                {
                    InventoryObject inv = null;
                    switch (originItem.itemData.slot)
                    {
                        case ItemSlot.Power:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPower;
                            break;
                        case ItemSlot.Propulsion:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion;
                            break;
                        case ItemSlot.Utilities:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invUtility;
                            break;
                        case ItemSlot.Weapons:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invWeapon;
                            break;
                        default:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                            break;
                    }

                    if (AutoSortCheck(inv))
                    {
                        AutoSortSection(inv, false);
                    }
                }
                if (free_origin > 1 && size_destination > 1 && !obj_origin.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>())
                {
                    InventoryObject inv = null;
                    switch (originItem.itemData.slot)
                    {
                        case ItemSlot.Power:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPower;
                            break;
                        case ItemSlot.Propulsion:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion;
                            break;
                        case ItemSlot.Utilities:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invUtility;
                            break;
                        case ItemSlot.Weapons:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invWeapon;
                            break;
                        default:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                            break;
                    }

                    if (AutoSortCheck(inv))
                    {
                        AutoSortSection(inv, false);
                    }
                }

                // Now perform the swap, we need to be careful because both items could be of different sizes. Although as we have checked before, there WILL be space for them both since we just did an auto sort.
                // To handle this we will gather up two lists of what we need to swap, and since the sizes could be different, the smaller item will include the former's empty slots (so we match the largest amount).
                List<KeyValuePair<GameObject, InventorySlot>> slots_origin = new List<KeyValuePair<GameObject, InventorySlot>>();
                List<KeyValuePair<GameObject, InventorySlot>> slots_destination = new List<KeyValuePair<GameObject, InventorySlot>>();

                #region Data Matching
                // - First add the parents because its simple
                slots_origin.Add(new KeyValuePair<GameObject, InventorySlot>(obj_origin, originSlot));
                slots_destination.Add(new KeyValuePair<GameObject, InventorySlot>(obj_destination, destinationSlot));
                
                // - Then do the children
                if(children_origin != null && children_destination != null && children_origin.Count == children_destination.Count) // X to X | Same amount (more than 1 each), not that complicated, we do a direct swap
                {
                    foreach (var O in children_origin)
                    {
                        slots_origin.Add(O);
                    }
                    foreach (var O in children_destination)
                    {
                        slots_destination.Add(O);
                    }
                }
                else if (children_origin == null && children_destination == null) // 1 to 1
                {
                    // nothing to do here
                }
                else if (children_origin == null && children_destination != null) // 1 to X (fill from [ORIGIN])
                {
                    // Fill destination
                    foreach (var O in children_destination)
                    {
                        slots_destination.Add(O);
                    }
                    // Now we need to fill up the difference between these two. We do that by getting the *NEXT* available empty slots (that do exist since we checked for that earlier).
                    int diff = children_destination.Count; // (Parent(1) + Children(?)) - (Parent(1)) = Children(?)
                    int start_index = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface.ToList().IndexOf(slots_origin[0]) + 1; // Get starting index
                    // We need this many empty slots, and we will need to get them from the origin's slots.
                    for (int i = start_index; i < diff + start_index; i++)
                    {
                        /* NOTE: It is important to remember that (with how we are currently displaying it) the inventory currently is not filled with empty slot objects!
                        *  The inventory is set dynamically with need, so in cases where we ask for an empty slot (object) from the inventory, we may not get one!
                        *  So in that case, we will need to quickly make a new one.
                        */
                        if (obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>()) // /INVENTORY/
                        {
                            // This is very cheeky (and potentially dangerous) but since we will update the UI right after this by destroying (via parent not the array!) & creating new slots, it should be fine.
                            GameObject empty = obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>().CreateNewEmptySlot();
                            slots_origin.Add(new KeyValuePair<GameObject, InventorySlot>(empty, InventoryControl.inst.p_inventory.Container.Items[i]));
                        }
                        else // /PARTS/
                        {
                            slots_origin.Add(obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface.ToList()[i]);
                        }
                    }
                }
                else if (children_origin != null && children_destination == null) // X to 1 (fill from [DESTINATION])
                {
                    // Fill origin
                    foreach (var O in children_origin)
                    {
                        slots_origin.Add(O);
                    }
                    // Now we need to fill up the difference between these two. We do that by getting the *NEXT* available empty slots (that do exist since we checked for that earlier).
                    int diff = children_origin.Count; // (Parent(1) + Children(?)) - (Parent(1)) = Children(?)
                    int start_index = MouseData.interfaceMouseIsOver.slotsOnInterface.ToList().IndexOf(slots_destination[0]) + 1; // Get starting index
                    // We need this many empty slots, and we will need to get them from the destinations's slots.
                    for (int i = start_index; i < diff + start_index; i++)
                    {
                        /* NOTE: It is important to remember that (with how we are currently displaying it) the inventory currently is not filled with empty slot objects!
                        *  The inventory is set dynamically with need, so in cases where we ask for an empty slot (object) from the inventory, we may not get one!
                        *  So in that case, we will need to quickly make a new one.
                        */
                        if (MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>()) // /INVENTORY/
                        {
                            // This is very cheeky (and potentially dangerous) but since we will update the UI right after this by destroying (via parent not the array!) & creating new slots, it should be fine.
                            GameObject empty = MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>().CreateNewEmptySlot();
                            slots_destination.Add(new KeyValuePair<GameObject, InventorySlot>(empty, InventoryControl.inst.p_inventory.Container.Items[i]));
                        }
                        else // /PARTS/
                        {
                            slots_destination.Add(MouseData.interfaceMouseIsOver.slotsOnInterface.ToList()[i]);
                        }
                    }
                }
                else if (children_origin.Count > children_destination.Count) // X to Y | [ORIGIN] has more than [DESTINATION], so we need empty slots from [DESTINATION]
                {
                    foreach (var O in children_origin)
                    {
                        slots_origin.Add(O);
                    }
                    foreach (var O in children_destination)
                    {
                        slots_destination.Add(O);
                    }

                    int diff = children_origin.Count - children_destination.Count;
                    int start_index = MouseData.interfaceMouseIsOver.slotsOnInterface.ToList().IndexOf(slots_destination[0]) + 1; // Get starting index
                    // We need this many empty slots, and we will need to get them from the destinations's slots.
                    for (int i = start_index; i < diff + start_index; i++)
                    {
                        /* NOTE: It is important to remember that (with how we are currently displaying it) the inventory currently is not filled with empty slot objects!
                        *  The inventory is set dynamically with need, so in cases where we ask for an empty slot (object) from the inventory, we may not get one!
                        *  So in that case, we will need to quickly make a new one.
                        */
                        if (MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>()) // /INVENTORY/
                        {
                            // This is very cheeky (and potentially dangerous) but since we will update the UI right after this by destroying (via parent not the array!) & creating new slots, it should be fine.
                            GameObject empty = MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>().CreateNewEmptySlot();
                            slots_destination.Add(new KeyValuePair<GameObject, InventorySlot>(empty, InventoryControl.inst.p_inventory.Container.Items[i]));
                        }
                        else // /PARTS/
                        {
                            slots_destination.Add(MouseData.interfaceMouseIsOver.slotsOnInterface.ToList()[i]);
                        }
                    }
                }
                else if (children_destination.Count > children_origin.Count) // Y to X | [DESTINATION] has more than [ORIGIN], so we need empty slots from [ORIGIN]
                {
                    foreach (var O in children_origin)
                    {
                        slots_origin.Add(O);
                    }
                    foreach (var O in children_destination)
                    {
                        slots_destination.Add(O);
                    }

                    int diff = children_destination.Count - children_origin.Count;
                    int start_index = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface.ToList().IndexOf(slots_origin[0]) + 1; // Get starting index
                    // We need this many empty slots, and we will need to get them from the origin's slots.
                    for (int i = start_index; i < diff + start_index; i++)
                    {
                        /* NOTE: It is important to remember that (with how we are currently displaying it) the inventory currently is not filled with empty slot objects!
                        *  The inventory is set dynamically with need, so in cases where we ask for an empty slot (object) from the inventory, we may not get one!
                        *  So in that case, we will need to quickly make a new one.
                        */
                        if (obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>()) // /INVENTORY/
                        {
                            // This is very cheeky (and potentially dangerous) but since we will update the UI right after this by destroying (via parent not the array!) & creating new slots, it should be fine.
                            GameObject empty = obj.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>().CreateNewEmptySlot();
                            slots_origin.Add(new KeyValuePair<GameObject, InventorySlot>(empty, InventoryControl.inst.p_inventory.Container.Items[i]));
                        }
                        else // /PARTS/
                        {
                            slots_origin.Add(obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface.ToList()[i]);
                        }
                    }
                }
                #endregion

                // Prototype discovery
                if(originSlot.AllowedItems.Count > 0)
                {
                    if (!originItem.itemData.knowByPlayer)
                        originItem.itemData.knowByPlayer = true;
                    UIManager.inst.CreateNewLogMessage("Identified " + originItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else if (destinationSlot.AllowedItems.Count > 0)
                {
                    if (!destinationItem.itemData.knowByPlayer)
                        destinationItem.itemData.knowByPlayer = true;
                    UIManager.inst.CreateNewLogMessage("Identified " + destinationItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }

                // Now that we have all our data, we go through both at the same time and start swapping data (they will both have the same length).
                for (int i = 0; i < slots_origin.Count; i++)
                {
                    _inventory.SwapItems(slots_origin[i].Value, slots_destination[i].Value); // Swap the slots

                    // Swap item data on objects
                    slots_origin[i].Key.GetComponent<InvDisplayItem>().item = slots_destination[i].Value.item; // Origin -> Destination
                    slots_destination[i].Key.GetComponent<InvDisplayItem>().item = slots_origin[i].Value.item; // Destination -> Origin

                    // Force Enable both items, and animate them
                    if (!slots_origin[i].Key.GetComponent<InvDisplayItem>().item.state) // [ORIGIN]
                    {
                        slots_origin[i].Key.GetComponent<InvDisplayItem>().UIEnable();
                    }
                    if (!slots_destination[i].Key.GetComponent<InvDisplayItem>().item.state) // [DESTINATION]
                    {
                        slots_destination[i].Key.GetComponent<InvDisplayItem>().UIEnable();
                    }

                    // Flash both item's images
                    slots_origin[i].Key.GetComponent<InvDisplayItem>().FlashItemDisplay(); // [ORIGIN]
                    slots_destination[i].Key.GetComponent<InvDisplayItem>().FlashItemDisplay(); // [DESTINATION]
                }

                UIManager.inst.ShowCenterMessageTop("Attached " + originItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                UIManager.inst.CreateNewLogMessage("Attached " + originItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
            }
            else // We can't swap the items, do a message.
            {
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
            }
            #endregion
        }
        else if (((MouseData.slotHoveredOver && MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item.Id < 0) 
            || MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>()) && obj.GetComponent<InvDisplayItem>().item != null) // Are we hovering over a slot thats empty? Attempt to move this item
        {
            #region Item Relocation

            #region Information Grabbing
            GameObject obj_origin = obj;
            GameObject obj_destination = MouseData.slotHoveredOver;

            Item originItem = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj].item;

            InventorySlot originSlot = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj]; // Get data from the slot we are swapping with
            InventorySlot destinationSlot = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver]; // Get data from slot hovered over

            int size_origin = originItem.itemData.slotsRequired;

            int free_destination = 0;

            switch (originSlot.item.itemData.slot) // Get empty slots of the destination area
            {
                case ItemSlot.Power:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invPower.EmptySlotCount;
                    break;
                case ItemSlot.Propulsion:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.EmptySlotCount;
                    break;
                case ItemSlot.Utilities:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invUtility.EmptySlotCount;
                    break;
                case ItemSlot.Weapons:
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._invWeapon.EmptySlotCount;
                    break;
                default: // Inventory
                    free_destination = PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount;
                    break;
            }

            // If the item is multi-slot we need to gather up its children and their objects
            List<KeyValuePair<GameObject, InventorySlot>> children_origin = null;
            if (size_origin > 1)
            {
                children_origin = new List<KeyValuePair<GameObject, InventorySlot>>();
                foreach (var C in obj_origin.GetComponent<InvDisplayItem>().secondaryChildren) // [ORIGIN]
                {
                    children_origin.Add(new KeyValuePair<GameObject, InventorySlot>(C, obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[C]));
                }
            }
            #endregion

            // Firstly, is there enough space for our item to fit into its new destination?
            if(free_destination < size_origin)
            {
                // Nope. Stop early.
                UIManager.inst.ShowCenterMessageTop("Not enough space", UIManager.inst.dangerRed, Color.black);
                return;
            }

            // Is this item allowed to be placed in this slot?
            if(destinationSlot.AllowedItems.Count == 0 || destinationSlot.AllowedItems.Contains(originItem.itemData.slot)) // Yes! Move the item
            {
                // Now that we have space and are allowed to move the item there, do we need to sort the slots at all?
                // With multi-slot items there may be cases where we need to force auto-sort the items on display so a new item can "fit". We do this check here.
                if (free_destination > 1 && size_origin > 1 && !obj_destination.GetComponent<InvDisplayItem>().my_interface.GetComponent<StaticInterface>())
                {
                    InventoryObject inv = null;
                    switch (originItem.itemData.slot)
                    {
                        case ItemSlot.Power:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPower;
                            break;
                        case ItemSlot.Propulsion:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invPropulsion;
                            break;
                        case ItemSlot.Utilities:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invUtility;
                            break;
                        case ItemSlot.Weapons:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._invWeapon;
                            break;
                        default:
                            inv = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                            break;
                    }

                    if (AutoSortCheck(inv))
                    {
                        AutoSortSection(inv, false);
                    }
                }

                // Now that everything is sorted, we need to collect up the slots we need to interact with
                List<KeyValuePair<GameObject, InventorySlot>> slots_origin = new List<KeyValuePair<GameObject, InventorySlot>>();
                List<KeyValuePair<GameObject, InventorySlot>> slots_destination = new List<KeyValuePair<GameObject, InventorySlot>>();

                // - First add the parents because its simple
                slots_origin.Add(new KeyValuePair<GameObject, InventorySlot>(obj_origin, originSlot));
                slots_destination.Add(new KeyValuePair<GameObject, InventorySlot>(obj_destination, destinationSlot));

                // - Add any child origin slots
                if(children_origin != null && children_origin.Count > 0)
                {
                    foreach (var C in children_origin)
                    {
                        slots_origin.Add(C);
                    }

                    // Then since it is necessary, we need to identify some extra free slots in the [DESTINATION].
                    // Now we need to fill up the difference between these two. We do that by getting the *NEXT* available empty slots (that do exist since we checked for that earlier).
                    int diff = children_origin.Count; // (Parent(1) + Children(?)) - (Parent(1)) = Children(?)
                    int start_index = MouseData.interfaceMouseIsOver.slotsOnInterface.ToList().IndexOf(slots_destination[0]) + 1; // Get starting index

                    // We need this many empty slots, and we will need to get them from the destinations's slots.
                    for (int i = start_index; i < diff + start_index; i++)
                    {
                        /* NOTE: It is important to remember that (with how we are currently displaying it) the inventory currently is not filled with empty slot objects!
                        *  The inventory is set dynamically with need, so in cases where we ask for an empty slot (object) from the inventory, we may not get one!
                        *  So in that case, we will need to quickly make a new one.
                        */
                        if (MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>()) // /INVENTORY/
                        {
                            // This is very cheeky (and potentially dangerous) but since we will update the UI right after this by destroying (via parent not the array!) & creating new slots, it should be fine.
                            GameObject empty = MouseData.interfaceMouseIsOver.GetComponent<StaticInterface>().CreateNewEmptySlot();
                            slots_destination.Add(new KeyValuePair<GameObject, InventorySlot>(empty, InventoryControl.inst.p_inventory.Container.Items[i]));
                        }
                        else // /PARTS/
                        {
                            slots_destination.Add(MouseData.interfaceMouseIsOver.slotsOnInterface.ToList()[i]);
                        }
                    }
                }

                // - Figure out what to print out
                if (destinationSlot.parent.GetComponent<StaticInterface>()) // Moving TO the /INVENTORY/
                {
                    UIManager.inst.ShowCenterMessageTop("Detached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    UIManager.inst.CreateNewLogMessage("Detached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else if (destinationSlot.parent.GetComponent<DynamicInterface>())// Moving TO a /PARTS/ slot
                {
                    // Prototype discovery (equip only)
                    if (!originItem.itemData.knowByPlayer)
                    {
                        originItem.itemData.knowByPlayer = true;
                        UIManager.inst.CreateNewLogMessage("Identified " + originItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }

                    if (originSlot.item.itemData.slot == ItemSlot.Power)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                    else if (originSlot.item.itemData.slot == ItemSlot.Propulsion)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                    else if (originSlot.item.itemData.slot == ItemSlot.Utilities)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                    else if (originSlot.item.itemData.slot == ItemSlot.Weapons)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + originSlot.item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                    else
                    {
                        Debug.LogError(destinationSlot + " has no assigned interface!");
                        return;
                    }
                }

                // Now that we have all our data, we go through both at the same time and start swapping data (they will both have the same length).
                for (int i = 0; i < slots_origin.Count; i++)
                {
                    _inventory.SwapItems(slots_origin[i].Value, slots_destination[i].Value); // Swap the slots

                    // Swap item data on objects
                    slots_origin[i].Key.GetComponent<InvDisplayItem>().item = slots_destination[i].Value.item; // Origin -> Destination
                    slots_destination[i].Key.GetComponent<InvDisplayItem>().item = slots_origin[i].Value.item; // Destination -> Origin

                    // Force Enable both items, and animate them
                    if (!slots_origin[i].Key.GetComponent<InvDisplayItem>().item.state) // [ORIGIN]
                    {
                        slots_origin[i].Key.GetComponent<InvDisplayItem>().UIEnable();
                    }
                    if (!slots_destination[i].Key.GetComponent<InvDisplayItem>().item.state) // [DESTINATION]
                    {
                        slots_destination[i].Key.GetComponent<InvDisplayItem>().UIEnable();
                    }

                    // Flash both item's images
                    slots_origin[i].Key.GetComponent<InvDisplayItem>().FlashItemDisplay(); // [ORIGIN]
                    slots_destination[i].Key.GetComponent<InvDisplayItem>().FlashItemDisplay(); // [DESTINATION]
                }
            }
            else // No. Don't move the item.
            {
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
            }
            #endregion
        }

        // Update Inventory Count
        PlayerData.inst.currentInvCount = PlayerData.inst.GetComponent<PartInventory>()._inventory.ItemCount;

        // Update UI
        UIManager.inst.UpdatePSUI();
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
        InventoryControl.inst.UpdateInterfaceInventories();
    }

    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
        {
            MouseData.tempItemBeingDragged.transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Attempt to directly equip this item (from the inventory), to an empty slot that matches this item's type. Called externally from an InvDisplayItem object upon a double click.
    /// </summary>
    /// <param name="obj">A reference to the InvDisplayObject.</param>
    /// <param name="item">A reference to the IDO's item.</param>
    public void TryDirectEquip(GameObject obj, Item item)
    {
        // If the item is multi-slot we need to gather up its children and their objects
        List<KeyValuePair<GameObject, Item>> children = null;
        if(item.itemData.slotsRequired > 1)
        {
            children = new List<KeyValuePair<GameObject, Item>>();

            foreach (var C in obj.GetComponent<InvDisplayItem>().secondaryChildren)
            {
                children.Add(new KeyValuePair<GameObject, Item>(C, C.GetComponent<InvDisplayItem>().item));
            }
        }

        // We are essentially doing item relocation here. So we can copy most of the code from above.

        // First we need to try and find a free slot of the matching type.
        int itemSize = item.itemData.slotsRequired;
        int freeSlotsFound = 0;
        List<GameObject> destinationObjects = new List<GameObject>();

        foreach (var I in InventoryControl.inst.interfaces)
        {
            if (I.GetComponent<DynamicInterface>()) // This double loop shouldn't be too bad. v
            {
                foreach (KeyValuePair<GameObject, InventorySlot> S in I.GetComponent<DynamicInterface>().slotsOnInterface)
                {
                    if(S.Value.item != null && S.Value.item.Id == -1) // An empty slot
                    {
                        if(S.Value.AllowedItems.Count > 0 && S.Value.AllowedItems.Contains(item.itemData.slot)) // And allows for our item's slot type
                        { // Note: ^ Since we are adding to /PARTS/ slots this should always have (usually) 1 type and not be == 0.
                            // We've found our slot
                            destinationObjects.Add(S.Key);
                            freeSlotsFound++;
                            if (freeSlotsFound == itemSize)
                            {
                                break; // We have space!
                            }
                        }
                    }
                }
            }
        }

        // Did we succeed on finding a slot(s)?
        if(freeSlotsFound < itemSize) // No. Display a message
        {
            UIManager.inst.ShowCenterMessageTop($"No free {item.itemData.slot.ToString()} slots", UIManager.inst.dangerRed, Color.black);
        }
        else // Yes! Relocate the item.
        {
            bool once = false;
            foreach (var dobj in destinationObjects)
            {
                // If this is a multi-slot item, we need to start modifying the children
                if(dobj != destinationObjects[0]) // Not the first item (parent). This will only happen if we need to move more than 1 slot!
                {
                    // We need to re-assign our input variables to the children
                    int index = destinationObjects.IndexOf(dobj) - 1; // -1 because destinationObjects also includes the parent item (at index 0)
                    obj = children[index].Key;
                    item = children[index].Value;
                }

                // Prototype discovery
                if (!item.itemData.knowByPlayer)
                {
                    item.itemData.knowByPlayer = true;
                    UIManager.inst.CreateNewLogMessage("Identified " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }

                if (item.itemData.slot == ItemSlot.Power)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invPower.AddItem(item, 1);
                    if (!once)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                }
                else if (item.itemData.slot == ItemSlot.Propulsion)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.AddItem(item, 1);
                    if (!once)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                }
                else if (item.itemData.slot == ItemSlot.Utilities)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invUtility.AddItem(item, 1);
                    if (!once)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                }
                else if (item.itemData.slot == ItemSlot.Weapons)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invWeapon.AddItem(item, 1);
                    if (!once)
                    {
                        UIManager.inst.ShowCenterMessageTop("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                        UIManager.inst.CreateNewLogMessage("Attached " + item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                    }
                }

                // Remove the old item from wherever its stored
                Action.FindRemoveItemFromPlayer(item);

                obj.GetComponent<InvDisplayItem>().item = new Item(); // Set [ORIGIN]'s item to be empty in object
                obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj].item = new Item(); // Set [ORIGIN]'s item slot to be empty so we don't get errors

                dobj.GetComponent<InvDisplayItem>().item = item; // Set the item on the [DESTINATION] object

                // Force enable the item and animate it (if its disabled)
                if (!dobj.GetComponent<InvDisplayItem>().item.state) // [DESTINATION]
                {
                    dobj.GetComponent<InvDisplayItem>().UIEnable();
                }

                // Flash the item's display square
                dobj.GetComponent<InvDisplayItem>().FlashItemDisplay();

                once = true;
            }

            // Update Inventory Count
            PlayerData.inst.currentInvCount = PlayerData.inst.GetComponent<PartInventory>()._inventory.ItemCount;

            // Update UI
            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateInventory();
            UIManager.inst.UpdateParts();
            InventoryControl.inst.UpdateInterfaceInventories();
        }
    }

    #region Auto-Sorting
    /// <summary>
    /// Called periodically by other functions. Checks to see if an inventory needs to be auto-sorted. Returns True/False.
    /// <param name="inventory"/>The inventory to check.</param>
    /// <returns>True/False if a sort needs to happen.</returns>
    /// </summary>
    public bool AutoSortCheck(InventoryObject inventory)
    {
        // First of all: 1.) Are there even any items in this inventory? 2.) Is this inventory full?
        if(inventory.ItemCount <= 0 || inventory.EmptySlotCount <= 0)
        {
            return false;
        }

        // Now that we know that there is atleast 1 item in here, and atleast 1 free space.
        // There are a few scenarios where we want to sort:
        // 1. There is a free space at the top. (We want all items to be pushed to the top)
        if (inventory.Container.Items[0].item == null || inventory.Container.Items[0].item.Id == -1)
        {
            return true;
        }
        // 2. There is a gap inbetween two items.
        if(HF.FindGapInList(HF.InventoryToSimple(inventory)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// When called, will attempt to auto-sort an inventory.
    /// <param name="inventory"/>The inventory to check.</param>
    /// <param name="animate"/>If true, when the UI update is performed, sorted items will do a little "move" animation to show they have been sorted. If false, they will just auto-reposition with no fanfare.</param>
    /// </summary>
    public void AutoSortSection(InventoryObject inventory, bool animate = true)
    {
        // The whole idea here is to push everything to the top, and leave the empty space at the bottom. (There WILL be empty space thanks to our checks)
        // We will move the items around in the inventory first, then tell the UserInterface to redraw itself.

        inventory.Sort();

        if (animate) // == ANIMATION ==
        {
            InventoryControl.inst.awaitingSort = true;

            // Now that the inventory is sorted (but the UI still hasn't been updated) we need to find the difference between the two, and use that for our animation.

            // Gather up the UI GameObjects
            List<KeyValuePair<GameObject, InventorySlot>> UIslots = new List<KeyValuePair<GameObject, InventorySlot>>();
            if (inventory.Container.Items[0].AllowedItems.Count > 0) // /PARTS/
            {
                foreach (KeyValuePair<GameObject, InventorySlot> pair in UIManager.inst.partContentArea.GetComponent<DynamicInterface>().slotsOnInterface)
                {
                    if (pair.Value.AllowedItems[0] == inventory.Container.Items[0].AllowedItems[0]) // Want to make sure we only get the same type
                    {
                        UIslots.Add(pair);
                    }
                }
            }
            else // /INVENTORY/
            {
                foreach (KeyValuePair<GameObject, InventorySlot> pair in UIManager.inst.inventoryArea.GetComponent<StaticInterface>().slotsOnInterface)
                {
                    UIslots.Add(pair);
                }
            }
            
            // Save the old positions & Find the new positions
            List<Vector3> oldPositions = new List<Vector3>();
            List<Vector3> newPositions = new List<Vector3>();
            List<GameObject> toBeSorted = new List<GameObject>();
            foreach (KeyValuePair<GameObject, InventorySlot> kvp in UIslots)
            {
                if (kvp.Key.GetComponent<InvDisplayItem>().item != null && kvp.Key.GetComponent<InvDisplayItem>().item.Id >= 0) // Don't sort the empty slots
                {
                    oldPositions.Add(kvp.Key.transform.position);

                    foreach (InventorySlot slot in inventory.Container.Items) // Find where this slot *SHOULD BE* in the new arrangement
                    {
                        if (slot.item == kvp.Key.GetComponent<InvDisplayItem>().item)
                        {
                            newPositions.Add(kvp.Key.transform.position);
                            toBeSorted.Add(kvp.Key);
                            break;
                        }
                    }
                }
            }

            // We will use this information to create temporary duplicates that we will move around to the place they need to be.
            // OR we could just use the originals (since we delete them anyways on update) and then stall the interface refresh
            int distance = 21; // The UI elements are around this distance apart from each other. 
            Debug.Log($"SortInfo: UIslots:{UIslots.Count} | op:{oldPositions.Count} | np:{newPositions.Count}");
            // Now go through and perform the movement, we should only be moving slots that NEED to be moved.
            for (int i = 0; i < toBeSorted.Count; i++)
            {
                Debug.Log($"Old: {oldPositions[i]} | New: {newPositions[i]}");
                if (oldPositions[i] != newPositions[i]) // Only move ones that need to be moved.
                {
                    GameObject obj = toBeSorted[i]; // Get the object that needs to be moved

                    Debug.Log($"Sorting: {obj.name}");
                    obj.GetComponent<InvDisplayItem>().Sort_StaggeredMove(newPositions[i], distance);
                }
            }
        }

        // Redraw the UI Display
        //InventoryControl.inst.UpdateInterfaceInventories();
    }
    #endregion

    public void CopyInvDisplayItem(InvDisplayItem s, InvDisplayItem t) // Source | Target
    {
        t.activeGreen = s.activeGreen;
        t.inActiveGreen = s.inActiveGreen;
        t.wideBlue = s.wideBlue;
        t.hurtYellow = s.hurtYellow;
        t.badOrange = s.badOrange;
        t.dangerRed = s.dangerRed;
        t.emptyGray = s.emptyGray;
        t.letterWhite = s.letterWhite;
        t.item = s.item;
    }

    public void ClearSlots()
    {
        foreach (KeyValuePair<GameObject, InventorySlot> S in slotsOnInterface.ToList())
        {
            Destroy(S.Key);
        }
        slotsOnInterface.Clear();
    }
}

public static class MouseData { 

    public static UserInterface interfaceMouseIsOver;
    public static GameObject tempItemBeingDragged;
    public static GameObject slotHoveredOver;
}

public static class ExtensionMethods
{
    public static void UpdateSlotDisplay(this Dictionary<GameObject, InventorySlot> _slotsOnInterface)
    {
        foreach (KeyValuePair<GameObject, InventorySlot> _slot in _slotsOnInterface)
        {
            if (_slot.Key.GetComponent<InvDisplayItem>())
            {
                if (_slot.Value.item.Id != -1)
                {
                    bool oneTime = false;
                    if (!InventoryControl.inst.animatedItems.Contains(_slot.Value)) // Only want to start the text black once
                        oneTime = true;

                    _slot.Key.GetComponent<InvDisplayItem>().item = _slot.Value.item;
                    _slot.Key.GetComponent<InvDisplayItem>().isSecondaryItem = _slot.Value.item.isDuplicate;
                    if (!_slot.Value.item.isDuplicate)
                    { // Duplicates get setup via the parent
                        _slot.Key.GetComponent<InvDisplayItem>().SetAsFilled();
                        _slot.Key.GetComponent<InvDisplayItem>().UpdateDisplay(oneTime);
                    }
                    else
                    {
                        _slot.Key.GetComponent<InvDisplayItem>().SecondaryCompleteSetup();
                    }

                    // Play the initial animation (only once tho)
                    if (!InventoryControl.inst.animatedItems.Contains(_slot.Value))
                    {
                        // Add the item to the set of animated items
                        InventoryControl.inst.animatedItems.Add(_slot.Value);
                        _slot.Key.GetComponent<InvDisplayItem>().InitialReveal();
                    }
                }
                else
                {
                    _slot.Key.GetComponent<InvDisplayItem>().SetEmpty();
                }
            }
        }

        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
    }
}
