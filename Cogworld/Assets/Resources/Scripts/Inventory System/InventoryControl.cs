using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;

public class InventoryControl : MonoBehaviour
{
    public static InventoryControl inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Item Database")]
    public ItemDatabaseObject _itemDatabase;
    [Header("Player's Inventories")]
    public InventoryObject p_inventoryPower;
    public InventoryObject p_inventoryPropulsion;
    public InventoryObject p_inventoryUtilities;
    public InventoryObject p_inventoryWeapons;
    public InventoryObject p_inventory;


    [Header("Data Related")]
    // - Data -
    public Dictionary<Vector2Int, GameObject> worldItems = new Dictionary<Vector2Int, GameObject>(); // Spawned in items on the floor in the map
    public List<ItemObject> knownItems = new List<ItemObject>();
    [Header("Prefabs")]
    // -- Prefabs --
    //
    public GameObject _itemHeldPrefab;
    public GameObject _hoverMovingItemPrefab;
    public GameObject _itemGroundPrefab;
    //
    public GameObject powerSpacer;
    public GameObject propulsionSpacer;
    public GameObject utilitySpacer;
    public GameObject weaponSpacer;
    //
    //
    // --         --
    [Header("References")]
    // -- References --
    //
    public GameObject allFloorItems;
    //
    public GameObject inventoryArea;
    public TextMeshProUGUI inventorySizeText;
    //
    public GameObject partArea;
    public TextMeshProUGUI partWeightText;
    //
    // --            --

    [Header("Inventory UI")]
    public Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>();

    public void InitBasicKnownItems()
    {
        
    }

    // Inventory Displaying now happens in UserInterface / DynamicInterface / Static Interface
    #region Inventory Displaying

    /*
    public void CreateSlots()
    {
        itemsDisplayed = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < p_inventory.Container.Items.Length; i++)
        {
            var obj = Instantiate(_itemHeldPrefab, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>()._assignedItem = null;
            obj.GetComponent<InvDisplayItem>().SetEmpty();

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

            itemsDisplayed.Add(obj, p_inventory.Container.Items[i]);
        }
    }

    public void UpdateSlots()
    {
        foreach (KeyValuePair<GameObject, InventorySlot> _slot in itemsDisplayed)
        {
            if (_slot.Value.item.Id >= 0)
            {
                _slot.Key.GetComponent<InvDisplayItem>()._part = p_inventory.database.GetItem[_slot.Value.item.Id];
                _slot.Key.GetComponent<InvDisplayItem>().SetUnEmpty();
                _slot.Key.GetComponent<InvDisplayItem>().UpdateDisplay();
            }
            else
            {
                _slot.Key.GetComponent<InvDisplayItem>().SetEmpty();
            }
        }

    }

    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
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
        Debug.Log("Mouse Enter");
        mouseItem.hoverObj = obj;
        if (itemsDisplayed.ContainsKey(obj))
        {
            mouseItem.hoverItem = itemsDisplayed[obj];
        }
    }

    public void OnExit(GameObject obj)
    {
        Debug.Log("Mouse Exit");
        mouseItem.obj = null;
        mouseItem.item = null;
    }

    public void OnDragStart(GameObject obj)
    {
        Debug.Log("Start Drag");
        // Create a (visual) copy of the item that gets attached to the mouse
        var mouseObject = Instantiate(_itemHeldPrefab, Vector3.zero, Quaternion.identity, inventoryArea.transform);
        var rt = mouseObject;
        CopyInvDisplayItem(obj.GetComponent<InvDisplayItem>(), rt.GetComponent<InvDisplayItem>());
        rt.GetComponent<InvDisplayItem>().SetEmpty();
        mouseObject.transform.SetParent(inventoryArea.transform.parent);

        // Is there an actual item there?
        if (itemsDisplayed[obj].item.Id >= 0)
        {
            rt.GetComponent<InvDisplayItem>().SetUnRaycast();
        }
        mouseItem.obj = mouseObject;
        mouseItem.item = itemsDisplayed[obj];
    }

    public void OnDragEnd(GameObject obj)
    {
        Debug.Log("Drag End");
        if (mouseItem.hoverObj)
        {
            if (itemsDisplayed[obj].item.Id > -1) // Don't wanna move empty slots
                p_inventory.MoveItem(itemsDisplayed[obj], itemsDisplayed[mouseItem.hoverObj]);
        }
        else
        {
            // Dropping it outside of the inventory
            // -Currently deletes the item, change this later to just drop (or zap) it!
            p_inventory.RemoveItem(itemsDisplayed[obj].item);
        }
        Destroy(mouseItem.obj);
        mouseItem.item = null;
    }

    public void OnDrag(GameObject obj)
    {
        Debug.Log("Dragging...");
        if(mouseItem.obj != null)
        {
            mouseItem.obj.transform.position = Input.mousePosition;
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
        t._part = s._part;
    }

    
    public void CreateInvDisplay()
    {
        if(itemsDisplayed.Count > 0) // Clear the inventory
        {
            //Destroy each gameObject but don't modify the dictionary
            foreach (KeyValuePair<InventorySlot, GameObject> kvp in itemsDisplayed)
            {
                Destroy(kvp.Value);
            }

            itemsDisplayed.Clear(); //To Clear out the dictionary
        }

        for (int i = 0; i < p_inventory.Container.Items.Count; i++)
        {
            InventorySlot slot = p_inventory.Container.Items[i];

            var obj = Instantiate(_itemHeldPrefab, Vector3.zero, Quaternion.identity, inventoryArea.transform);
            obj.GetComponent<InvDisplayItem>()._assignedItem = slot.item;
            itemsDisplayed.Add(slot, obj);
        }
    }

    public void UpdateInvDisplay()
    {
        for (int i = 0; i < p_inventory.Container.Items.Count; i++) // For all the items there are
        {
            if (itemsDisplayed.ContainsKey(p_inventory.Container.Items[i]))
            {
                // Already exists, don't display (duplicate) it
            }
            else
            {
                InventorySlot slot = p_inventory.Container.Items[i];

                var obj = Instantiate(_itemHeldPrefab, Vector3.zero, Quaternion.identity, inventoryArea.transform); // Create a new instance of the prefab
                obj.GetComponent<InvDisplayItem>()._assignedItem = slot.item; // Assign the display its item
                obj.GetComponent<InvDisplayItem>().UpdateDisplay(); // Update the display (internally)
                itemsDisplayed.Add(slot, obj); // Add it to the dictionary of existing displayed items
            }
            
        }
    }
    */
    #endregion

    /// <summary>
    /// Directly creates a NEW item in the world based off of a specified ID.
    /// </summary>
    /// <param name="id">The id of the item to place.</param>
    /// <param name="location">Where in the world to place the item.</param>
    /// <param name="native">Is this item 'native' to 0b10? If false, scavengers will pick this up and recycle it.</param>
    /// <param name="specificAmount">A specific amount of this item to spawn, only really applies to matter</param>
    public void CreateItemInWorld(int id, Vector2Int location, bool native = false, int specificAmount = 1)
    {
        var spawnedItem = Instantiate(_itemGroundPrefab, new Vector3(location.x * GridManager.inst.globalScale, location.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedItem.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedItem.name = $"Floor Item {location.x} {location.y} - "; // Give grid based name

        Item item = new Item(_itemDatabase.Items[id]);

        if (_itemDatabase.Items[id].instantUnique) // For stuff like matter, change this later (this is still probably not the best idea)
        {
            item.amount = specificAmount;
        }

        item.state = false;
        spawnedItem.GetComponent<Part>()._item = item; // Assign part data from database by ID

        spawnedItem.name += spawnedItem.GetComponent<Part>()._item.itemData.itemName.ToString(); // Modify name with type

        spawnedItem.GetComponent<Part>().location.x = (int)location.x; // Assign X location
        spawnedItem.GetComponent<Part>().location.x = (int)location.y; // Assign Y location
        spawnedItem.GetComponent<Part>()._tile = MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)]; // Assign tile

        spawnedItem.GetComponent<Part>().isExplored = false;
        spawnedItem.GetComponent<Part>().isVisible = false;

        worldItems[new Vector2Int((int)location.x, (int)location.y)] = spawnedItem; // Add to Dictionary
        MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)]._partOnTop = spawnedItem.GetComponent<Part>();

        spawnedItem.GetComponentInChildren<SpriteRenderer>().sortingOrder = 6;
        spawnedItem.transform.parent = allFloorItems.transform;
        spawnedItem.GetComponent<Part>().Init(); // Begin part setup
    }

    /// <summary>
    /// Takes an object that was in an inventory and places it into the world.
    /// </summary>
    /// <param name="_item">The item in question.</param>
    /// <param name="location">The place to put it.</param>
    public void PlaceItemIntoWorld(Item _item, Vector2Int location, TileBlock tile)
    {
        var placedItem = Instantiate(_itemGroundPrefab, new Vector3(location.x * GridManager.inst.globalScale, location.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        placedItem.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        placedItem.name = $"Floor Item {location.x} {location.y} - "; // Give grid based name

        _item.state = false;
        placedItem.GetComponent<Part>()._item = _item; // Assign part data from database by ID
        tile._partOnTop = placedItem.GetComponent<Part>();

        placedItem.name += placedItem.GetComponent<Part>()._item.itemData.itemName.ToString(); // Modify name with type

        placedItem.GetComponent<Part>().location.x = (int)location.x; // Assign X location
        placedItem.GetComponent<Part>().location.x = (int)location.y; // Assign Y location
        placedItem.GetComponent<Part>()._tile = MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)]; // Assign tile

        worldItems[new Vector2Int((int)location.x, (int)location.y)] = placedItem; // Add to Dictionary
        MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)]._partOnTop = placedItem.GetComponent<Part>();

        placedItem.GetComponentInChildren<SpriteRenderer>().sortingOrder = 6;
        placedItem.transform.parent = allFloorItems.transform;
        placedItem.GetComponent<Part>().Init(); // Begin part setup
    }

    public void DeleteItemInWorld(Vector2Int location)
    {
        // incomplete
        Destroy(worldItems[location]);
    }

    #region Basic Inventory Logic

    public List<UserInterface> interfaces = new List<UserInterface>();
    public void SetInterfaceInventories()
    {
        foreach (var I in interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>())
            {
                I._inventory = PlayerData.inst.GetComponent<PartInventory>()._invPower;
                I.CreateSlots();
            }
            else if (I.GetComponentInChildren<StaticInterface>())
            {
                I._inventory = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                I.CreateSlots();
            }
            else if(I.GetComponentInChildren<UserInterface>())
            {
                I._inventory = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                I.StartUp();
            }
        }

        SetInterfaceInvKeys();
    }

    public void UpdateInterfaceInventories()
    {
        foreach (var I in interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>())
            {
                I.slotsOnInterface.UpdateSlotDisplay();
            }
            else if (I.GetComponentInChildren<StaticInterface>())
            {
                I.slotsOnInterface.UpdateSlotDisplay();
            }
            else if (I.GetComponentInChildren<UserInterface>())
            {
                I.slotsOnInterface.UpdateSlotDisplay();
            }
        }

        SetInterfaceInvKeys();
    }

    public void SetInterfaceInvKeys()
    {
        int alphabet = 0;
        int numbers = 0;

        foreach (var I in interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>()) // Includes all items found in /PARTS/ menus (USES LETTER)
            {
                foreach (var item in I.GetComponentInChildren<DynamicInterface>().slotsOnInterface)
                {
                    if(item.Key.GetComponent<InvDisplayItem>().item != null)
                    {
                        item.Key.GetComponent<InvDisplayItem>().SetLetter(UIManager.inst.alphabet[alphabet]);
                        alphabet++;

                        if (alphabet >= 26) // Impossible to have more than 26 parts so should be good here.
                        {
                            Debug.LogError("ERROR: Letter overflow in assigning keys to parts.");
                            Debug.Break();
                        }
                    }
                }
            }
            else if (I.GetComponentInChildren<StaticInterface>()) // Includes all items found in /INVENTORY/ menu (USES NUMBERS)
            {
                foreach (var item in I.GetComponentInChildren<StaticInterface>().slotsOnInterface)
                {
                    if (item.Key.GetComponent<InvDisplayItem>().item != null)
                    {
                        item.Key.GetComponent<InvDisplayItem>().SetLetter(UIManager.inst.alphabet[0], numbers + 1);
                        numbers++;
                    }
                }
            }
        }
    }

    public void ClearInterfacesInventories()
    {
        foreach (var I in interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>())
            {
                I.ClearSlots();
            }
            else if (I.GetComponentInChildren<StaticInterface>())
            {
                //I.ClearSlots();
            }
            else if (I.GetComponentInChildren<UserInterface>())
            {
                //I.ClearSlots();
            }
        }
    }

    /// <summary>
    /// Adds an item to either the player's inventory or one of their part slots
    /// </summary>
    /// <param name="part">The part to add.</param>
    /// <param name="inventory">The inventory to target.</param>
    public void AddItemToPlayer(Part part, InventoryObject inventory)
    {
        part._item.state = true;
        Item _item = new Item(part._item.itemData); // surley this couldn't backfire? (instead of Item _item = part._item)
        if(inventory.AddItem(_item, 1))
        {
            // Destroying is handled internally
        }
    }

    public void DropItemOnFloor(Item _item, Actor source, InventoryObject sourceInventory)
    {
        // Drop is as close to the source as possible
        TileBlock dropTile = MapManager.inst._allTilesRealized[HF.V3_to_V2I(source.transform.position)];

        Vector2Int dropLocation = HF.LocateFreeSpace(HF.V3_to_V2I(dropTile.transform.position)); // Find nearest free space
        dropTile = MapManager.inst._allTilesRealized[dropLocation];

        if (source.gameObject.GetComponent<PlayerData>()) // Is player?
        {
            PlayDropSound(_item.itemData.itemName); // Play the drop sound
            UIManager.inst.CreateNewLogMessage("Dropped " + _item.itemData.itemName + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false); // Do a UI message
        }

        // Remove from inventory
        if(sourceInventory != null)
            sourceInventory.RemoveItem(_item);

        // Place the item
        PlaceItemIntoWorld(_item, new Vector2Int(dropTile.locX, dropTile.locY), dropTile);

        return;
    }

    public void PlayDropSound(string iName)
    {
        // First check if this item is light or not
        bool isLight = false;
        if (iName.Contains("Lgt.") || iName.Contains("Lgt") || iName.Contains("LGT") || iName.Contains("LGT.") || iName.Contains("Light"))
        {
            isLight = true;
        }
        else
        {
            isLight = false;
        }

        // Then check if the player is in caves or not
        if (MapManager.inst.currentLevelName == "LOWER CAVES" || MapManager.inst.currentLevelName == "UPPER CAVES" || MapManager.inst.currentLevelName == "UNKNOWN")
        {
            if (isLight)
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dropItem_Clips[Random.Range(2, 3)], 0.7f);
            }
            else
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dropItem_Clips[Random.Range(0, 1)], 0.5f);
            }
        }
        else
        {
            if (isLight)
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dropItem_Clips[Random.Range(8, 10)], 0.7f);
            }
            else
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dropItem_Clips[Random.Range(4, 7)], 0.5f);
            }
        }
    }

    public void RemoveItemFromPlayer(Part part, InventoryObject inventory)
    {

    }

    public Part CloneItem(Part part)
    {
        Part clone = part;
        return clone;
    }

    public void DebugPrintInventory()
    {
        Debug.Log("The Player's inventory currently contains: ");
        Debug.Log(" == Power ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>()._invPower.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Propulsion ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Utility ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>()._invUtility.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Weapons ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>()._invWeapon.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Inventory Storage ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>()._inventory.Container.Items)
        {
            Debug.Log(item.item);
        }
    }

    #endregion
}