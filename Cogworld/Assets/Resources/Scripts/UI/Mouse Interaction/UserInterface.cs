using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UserInterface : MonoBehaviour
{
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    public InventoryObject _inventory;

    public GameObject _hoverMovingItemPrefab;
    public GameObject _itemHeldPrefab;
    public GameObject inventoryArea;

    bool startUpComplete = false;
    public void StartUp() // at the moment this is never called
    {
        for (int i = 0; i < _inventory.Container.Items.Length; i++)
        {
            _inventory.Container.Items[i].parent = this;
        }
        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

        startUpComplete = true;
    }

    private void Update()
    {
        if (startUpComplete)
        {
            slotsOnInterface.UpdateSlotDisplay();
        }
    }

    public abstract void CreateSlots();

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        Button _b = obj.GetComponent<InvDisplayItem>()._button; // Convoluted workaround due to null errors
        EventTrigger trigger = _b.GetComponent<EventTrigger>();
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
        MouseData.interfaceMouseIsOver = obj.GetComponent<InvDisplayItem>().my_interface;
    }

    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnDragStart(GameObject obj)
    {
        if(obj.GetComponent<InvDisplayItem>().item != null) // Don't drag empty slots!
        {
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
            tempItem = Instantiate(_hoverMovingItemPrefab, Vector3.zero, Quaternion.identity, inventoryArea.transform);

            var rt = tempItem;
            //CopyInvDisplayItem(obj.GetComponent<InvDisplayItem>(), rt.GetComponent<InvDisplayItem>());
            rt.GetComponent<InvMovingDisplayItem>().Setup(obj.GetComponent<InvDisplayItem>().item.itemData.itemName);
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
        if (obj.GetComponent<InvDisplayItem>().item == null) // Don't drag empty slots!
            return;

        Destroy(MouseData.tempItemBeingDragged);

        if(MouseData.interfaceMouseIsOver == null && obj.GetComponent<InvDisplayItem>().item != null) // If we're not over an inventory & we are dragging a real item
        {
            #region Item Dropping
            // Drop the item on the floor
            InventoryControl.inst.DropItemOnFloor(obj.GetComponent<InvDisplayItem>().item, PlayerData.inst.GetComponent<Actor>(), _inventory);

            if (this.GetComponent<StaticInterface>())
            {  
                PlayerData.inst.currentInvCount -= 1; // If this item was in the player's inventory we need to decrement the count.
            }
            else if (this.GetComponent<DynamicInterface>()) // If this item was equipped we need to change the player's stats
            {
                PlayerData.inst.currentWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.mass;
                if (obj.GetComponent<InvDisplayItem>().item.itemData.propulsion.Count > 0)
                {
                    PlayerData.inst.maxWeight -= obj.GetComponent<InvDisplayItem>().item.itemData.propulsion[0].support;
                }

                // Play a little animation to let the player know THIS item was just equipped
                obj.GetComponent<InvDisplayItem>().RecentAttachmentAnimation();
            }

            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateInventory();
            UIManager.inst.UpdateParts();

            slotsOnInterface[obj].RemoveItem();
            InventoryControl.inst.UpdateInterfaceInventories();
            return;
            #endregion
        }

        Debug.Log($"Slot hovered over: {MouseData.slotHoveredOver} | Source item is: {obj.GetComponent<InvDisplayItem>().item.Id} | Destination item is: {MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item.Id}");
        if (MouseData.slotHoveredOver && obj.GetComponent<InvDisplayItem>().item != null 
            && MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item.Id >= 0) // Are we hovering over a slot with an item? Lets attempt to swap the two parts.
        {
            #region Item Swapping

            // Get data from the two things we are trying to swap
            InventorySlot destinationSlot = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver]; // Get data from slot hovered over
            InventorySlot originSlot = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj]; // And get data from the slot we are swapping with

            Item destinationItem = destinationSlot.item;
            Item originItem = originSlot.item;

            bool canSwap = true;

            // First check if both items will be compatible with their new slots (there are a couple different cases here)
            if ((originSlot.AllowedItems.Count == 0 && destinationSlot.AllowedItems.Count == 0)) // Both items share the same slot type, obviously we shouldn't swap these.
            {
                return; // Bail out
            }
            else if(originSlot.AllowedItems.Count > 0 && destinationSlot.AllowedItems.Count > 0 && originSlot.AllowedItems[0] == destinationSlot.AllowedItems[0])
            { // NOTE: This is a bit risky here, but all slots should only have one type
                return; // Bail out
            }

            if (originSlot.AllowedItems.Count == 0) // The origin slot & item is in the inventory (destination item can always go in there)
            {
                if (destinationSlot.AllowedItems.Count > 0)
                {
                    if (destinationSlot.AllowedItems.Contains(originItem.itemData.slot)) // The destination slot allows the origin item to be placed there
                    {
                        canSwap = true;
                    }
                    else
                    {
                        canSwap = false;
                    }
                }
            }

            if(destinationSlot.AllowedItems.Count == 0) // The destination slot is in the inventory
            {
                if(originSlot.AllowedItems.Count > 0)
                {
                    if (originSlot.AllowedItems.Contains(destinationItem.itemData.slot)) // The origin slot allows the destination item to be placed there
                    {
                        canSwap = true;
                    }
                    else
                    {
                        canSwap = false;
                    }
                }
            }

            if (canSwap) // We can swap the items!
            {
                _inventory.SwapItems(slotsOnInterface[obj], destinationSlot); // Swap item positions  !!! MAY BE A PROBLEM HERE, SEPARATE INVENTORIES !!!
                // Swap item data
                obj.GetComponent<InvDisplayItem>().item = destinationItem; // Origin -> Destination
                MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().item = originItem; // Destination -> Origin
                // (Swapping the slot's parents happens inside *SwapItems*)
                // Swap interface assignments
                obj.GetComponent<InvDisplayItem>().my_interface = destinationSlot.parent;
                MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().my_interface = slotsOnInterface[obj].parent;
                // Force Enable both items, and animate them
                if (!obj.GetComponent<InvDisplayItem>().item.state) // [ORIGIN]
                {
                    obj.GetComponent<InvDisplayItem>().UIEnable();
                }
                if (!MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().item.state) // [DESTINATION]
                {
                    MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().UIEnable();
                }
                // Flash both item's images
                obj.GetComponent<InvDisplayItem>().FlashItemDisplay();
                MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().FlashItemDisplay();

                UIManager.inst.ShowCenterMessageTop("Attached " + originItem.itemData.itemName, UIManager.inst.highlightGreen, Color.black);

                InventoryControl.inst.UpdateInterfaceInventories(); // Update the UI
            }
            else // We can't swap the items, do a message.
            {
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
            }
            #endregion
        }
        else if (MouseData.slotHoveredOver && obj.GetComponent<InvDisplayItem>().item != null 
            && MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].item.Id < 0) // Are we hovering over a slot thats empty? Attempt to move this item
        {
            #region Item Relocation
            InventorySlot destinationSlot = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver]; // Get data from slot hovered over
            InventorySlot originSlot = obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj]; // And get data from the slot we are swapping with

            Item originItem = originSlot.item;

            // Is this item allowed to be placed in this slot?
            if(destinationSlot.AllowedItems.Count == 0 || destinationSlot.AllowedItems.Contains(originItem.itemData.slot)) // Yes! Move the item
            {
                // Add the item to the player (aka just make a new item)
                Item _item = new Item(originItem);
                // - Try to find which inventory we need to add it to
                if(_item.itemData.slot == ItemSlot.Power)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invPower.AddItem(_item, 1);
                    UIManager.inst.ShowCenterMessageTop("Attached " + _item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else if (_item.itemData.slot == ItemSlot.Propulsion)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.AddItem(_item, 1);
                    UIManager.inst.ShowCenterMessageTop("Attached " + _item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else if (_item.itemData.slot == ItemSlot.Utilities)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invUtility.AddItem(_item, 1);
                    UIManager.inst.ShowCenterMessageTop("Attached " + _item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else if (_item.itemData.slot == ItemSlot.Weapons)
                {
                    PlayerData.inst.GetComponent<PartInventory>()._invWeapon.AddItem(_item, 1);
                    UIManager.inst.ShowCenterMessageTop("Attached " + _item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }
                else // Add to inventory
                {
                    PlayerData.inst.GetComponent<PartInventory>()._inventory.AddItem(_item, 1);
                    UIManager.inst.ShowCenterMessageTop("Detached " + _item.itemData.itemName, UIManager.inst.highlightGreen, Color.black);
                }

                // Remove the old item from wherever its stored
                Action.FindRemoveItemFromPlayer(originItem);

                // Update parent interfaces
                UserInterface temp = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].parent;
                MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].parent = originSlot.parent;
                obj.GetComponent<InvDisplayItem>().my_interface.slotsOnInterface[obj].parent = temp;

                obj.GetComponent<InvDisplayItem>().my_interface = originSlot.parent; // [ORIGIN]
                MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().my_interface = temp; // [DESTINATION]
                // Force enable the item and animate it (if its disabled)
                if (!MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().item.state) // [DESTINATION]
                {
                    MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().UIEnable();
                }
                // Flash the item's display square
                MouseData.slotHoveredOver.GetComponent<InvDisplayItem>().FlashItemDisplay();
            }
            else // No. Don't move the item.
            {
                UIManager.inst.ShowCenterMessageTop("Item cannot be placed in this slot", UIManager.inst.dangerRed, Color.black);
            }

            InventoryControl.inst.UpdateInterfaceInventories(); // Update the UI
            #endregion
        }
    }

    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
        {
            MouseData.tempItemBeingDragged.transform.position = Input.mousePosition;
        }
    }

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
                    bool first = false;
                    if(_slot.Key.GetComponent<InvDisplayItem>().item == null) // This should only be triggered once
                    {
                        first = true;
                    }

                    _slot.Key.GetComponent<InvDisplayItem>().item = _slot.Value.item;
                    _slot.Key.GetComponent<InvDisplayItem>().SetAsFilled();
                    _slot.Key.GetComponent<InvDisplayItem>().UpdateDisplay();

                    if (first)
                    {
                        _slot.Key.GetComponent<InvDisplayItem>().FlashItemDisplay(); // We do this after we have finished assigning and setting up everything
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
