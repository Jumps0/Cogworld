using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public void StartUp()
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
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
        //Debug.Log("Interface Enter");
    }

    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
        //Debug.Log("Interface Exit");
    }

    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreatetempItem(obj);
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
            rt.GetComponent<InvMovingDisplayItem>().Setup(obj.GetComponent<InvDisplayItem>()._assignedItem.itemData.itemName);
            tempItem.transform.SetParent(inventoryArea.transform.parent);

            // Is there an actual item there?
            if (slotsOnInterface[obj].item.Id >= 0)
            {
                rt.GetComponent<InvMovingDisplayItem>().SetUnRaycast();
            }
        }

        return tempItem;
    }

    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);
        Debug.Log(MouseData.interfaceMouseIsOver);
        if(MouseData.interfaceMouseIsOver == null)
        {
            // Drop the item on the floor
            InventoryControl.inst.DropItemOnFloor(obj.GetComponent<InvDisplayItem>()._assignedItem, PlayerData.inst.GetComponent<Actor>(), _inventory);

            if (this.GetComponent<StaticInterface>())
            {  
                PlayerData.inst.currentInvCount -= 1; // If this item was in the player's inventory we need to decrement the count.
            }
            else if (this.GetComponent<DynamicInterface>()) // If this item was equipped we need to change the player's stats
            {
                PlayerData.inst.currentWeight -= obj.GetComponent<InvDisplayItem>()._assignedItem.itemData.mass;
                if (obj.GetComponent<InvDisplayItem>()._assignedItem.itemData.propulsion.Count > 0)
                {
                    PlayerData.inst.maxWeight -= obj.GetComponent<InvDisplayItem>()._assignedItem.itemData.propulsion[0].support;
                }
                
            }

            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateInventory();
            UIManager.inst.UpdateParts();

            slotsOnInterface[obj].RemoveItem();
            InventoryControl.inst.UpdateInterfaceInventories();
            return;
        }

        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver]; // Get data from slot hovered over
            _inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
            InventoryControl.inst.UpdateInterfaceInventories();
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
        t._assignedItem = s._assignedItem;
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
                    _slot.Key.GetComponent<InvDisplayItem>()._assignedItem = _slot.Value.item;
                    _slot.Key.GetComponent<InvDisplayItem>().SetUnEmpty();
                    _slot.Key.GetComponent<InvDisplayItem>().UpdateDisplay();
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
