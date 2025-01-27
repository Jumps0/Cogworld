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

    [Header("Player's Inventories")]
    public InventoryObject p_inventoryPower;
    public InventoryObject p_inventoryPropulsion;
    public InventoryObject p_inventoryUtilities;
    public InventoryObject p_inventoryWeapons;
    public InventoryObject p_inventory;

    [Header("Hideout Inventory")]
    public InventoryObject hideout_inventory;

    [Header("Data Related")]
    // - Data -
    public Dictionary<Vector2Int, GameObject> worldItems = new Dictionary<Vector2Int, GameObject>(); // Spawned in items on the floor in the map
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
    [Tooltip("By far, the worst thing about this project. For the love of god, find a better way to do this.")]
    public int uuids = 0;
    public Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>();

    public HashSet<InventorySlot> animatedItems = new HashSet<InventorySlot>(); // For tracking InitialAnimation on the InvDisplayObjects
    [Tooltip("Used by the /PARTS/ UI (changes upon every evolution)")]
    public GameObject interfaceDynamic;
    [Tooltip("Used by the /INVENTORY/ UI (changes often based on storage parts)")]
    public GameObject interfaceStatic;

    public void InitBasicKnownItems()
    {
        
    }

    /// <summary>
    /// Directly creates a NEW item in the world based off of a specified ID.
    /// </summary>
    public void CreateItemInWorld(ItemSpawnInfo spawnInfo)
    {
        string itemName = spawnInfo.name;
        Vector2 location = spawnInfo.location;
        int specificAmount = spawnInfo.amount;
        bool native = spawnInfo.native;
        float faultyRoll = spawnInfo.chanceForFaulty;

        int id = MapManager.inst.itemDatabase.dict[itemName].data.Id;

        // GameObject creation
        var spawnedItem = Instantiate(_itemGroundPrefab, new Vector3(location.x * GridManager.inst.globalScale, location.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedItem.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedItem.name = $"Floor Item {location.x} {location.y} - "; // Give grid based name

        // Item creation
        Item item = new Item(MapManager.inst.itemDatabase.Items[id]);

        if (MapManager.inst.itemDatabase.Items[id].instantUnique) // For stuff like matter, change this later (this is still probably not the best idea)
        {
            item.amount = specificAmount;
        }

        item.state = false;

        // Faulty roll
        if (faultyRoll > 0f)
        {
            // To be a faulty prototype it actually needs to be a prototype, duh.
            if (!item.itemData.knowByPlayer)
            {
                float random = Random.Range(0f, 1f);

                if(random <= faultyRoll)
                {
                    item.isFaulty = true;
                }
            }
        }

        // Corruption
        if(spawnInfo.corruptedAmount > 0)
        {
            item.corrupted = spawnInfo.corruptedAmount;
        }

        // Part setup
        spawnedItem.GetComponent<Part>()._item = item; // Assign part data from database by ID

        spawnedItem.name += spawnedItem.GetComponent<Part>()._item.itemData.itemName.ToString(); // Modify name with type

        spawnedItem.GetComponent<Part>().location.x = (int)location.x; // Assign X location
        spawnedItem.GetComponent<Part>().location.x = (int)location.y; // Assign Y location
        spawnedItem.GetComponent<Part>()._tile = MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)].bottom; // Assign tile
        spawnedItem.GetComponent<Part>().native = native;

        // Visibility
        spawnedItem.GetComponent<Part>().isExplored = false;
        spawnedItem.GetComponent<Part>().isVisible = false;

        // Dictionary storage
        worldItems[new Vector2Int((int)location.x, (int)location.y)] = spawnedItem;
        MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)].bottom._partOnTop = spawnedItem.GetComponent<Part>();

        spawnedItem.GetComponentInChildren<SpriteRenderer>().sortingOrder = 6;
        spawnedItem.transform.parent = allFloorItems.transform;

        // Internal Setup
        spawnedItem.GetComponent<Part>().Init();
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
        placedItem.GetComponent<Part>()._tile = MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)].bottom; // Assign tile

        worldItems[new Vector2Int((int)location.x, (int)location.y)] = placedItem; // Add to Dictionary
        MapManager.inst._allTilesRealized[new Vector2Int((int)location.x, (int)location.y)].bottom._partOnTop = placedItem.GetComponent<Part>();

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
                I._inventory = PlayerData.inst.GetComponent<PartInventory>().inv_power;

                I.GetComponentInChildren<DynamicInterface>().inventories.Add(PlayerData.inst.GetComponent<PartInventory>().inv_power);
                I.GetComponentInChildren<DynamicInterface>().inventories.Add(PlayerData.inst.GetComponent<PartInventory>().inv_propulsion);
                I.GetComponentInChildren<DynamicInterface>().inventories.Add(PlayerData.inst.GetComponent<PartInventory>().inv_utility);
                I.GetComponentInChildren<DynamicInterface>().inventories.Add(PlayerData.inst.GetComponent<PartInventory>().inv_weapon);

                I.CreateSlots();
            }
            else if (I.GetComponentInChildren<StaticInterface>())
            {
                I._inventory = PlayerData.inst.GetComponent<PartInventory>()._inventory;
                I.CreateSlots();
            }

            I.StartUp();
            
        }

        SetInterfaceInvKeys();

        // Update the item stats
        UIManager.inst.CEWQ_SetMode(GlobalSettings.inst.defaultItemDataMode, false);
    }

    public void UpdateInterfaceInventories()
    {
        p_inventory.Sort();

        if(uii_coroutine == null)
        {
            uii_coroutine = StartCoroutine(UII_Update());
        }
    }

    private Coroutine uii_coroutine;
    private IEnumerator UII_Update()
    {
        if (awaitingSort) // Stall until the sort animation is done
        {
            while (awaitingSort)
            {
                yield return null;
            }
        }

        foreach (var I in interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>())
            {
                I.GetComponentInChildren<DynamicInterface>().UpdateSlots();
                I.slotsOnInterface.UpdateSlotDisplay();
                I.UpdateObjectNames();
            }
            else if (I.GetComponentInChildren<StaticInterface>())
            {
                I.GetComponentInChildren<StaticInterface>().UpdateSlots();
                I.slotsOnInterface.UpdateSlotDisplay();
                I.UpdateObjectNames();
            }
        }

        SetInterfaceInvKeys();

        uii_coroutine = null;
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
                    item.Key.GetComponent<InvDisplayItem>().SetLetter(UIManager.inst.alphabet[alphabet].ToString());
                    alphabet++;

                    if (alphabet >= 26) // Impossible to have more than 26 parts so should be good here.
                    {
                        Debug.LogError("ERROR: Letter overflow in assigning keys to parts.");
                        Debug.Break();
                    }
                }
            }
            else if (I.GetComponentInChildren<StaticInterface>()) // Includes all items found in /INVENTORY/ menu (USES NUMBERS)
            {
                foreach (var item in I.GetComponentInChildren<StaticInterface>().slotsOnInterface)
                {
                    if (item.Key.GetComponent<InvDisplayItem>().item != null)
                    {
                        item.Key.GetComponent<InvDisplayItem>().SetLetter(UIManager.inst.alphabet[0].ToString(), numbers + 1);
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

    public bool awaitingSort = false;
    /// <summary>
    /// Called every time the player moves X amount of blocks. Checks to see if any of the /PARTS/ inventories (the 4 of them) need to be auto-sorted.
    /// </summary>
    public void PartsSortingCheck()
    {
        if (awaitingSort) // Don't attempt to sort while we are already sorting!
        {
            return;
        }

        bool updateNeeded = false;

        // We should only bother if the inventory has atleast 3 slots.
        if (PlayerData.inst.GetComponent<PartInventory>().inv_power.Container.Items.Length >= 3)
        {
            if (UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortCheck(PlayerData.inst.GetComponent<PartInventory>().inv_power)) // Check if we need to sort this
            {
                UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortSection(PlayerData.inst.GetComponent<PartInventory>().inv_power); // Perform the sort
                updateNeeded = true;
            }
        }
        if (PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items.Length >= 3)
        {
            if (UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortCheck(PlayerData.inst.GetComponent<PartInventory>().inv_propulsion)) // Check if we need to sort this
            {
                UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortSection(PlayerData.inst.GetComponent<PartInventory>().inv_propulsion); // Perform the sort
                updateNeeded = true;
            }
        }
        if (PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items.Length >= 3)
        {
            if (UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortCheck(PlayerData.inst.GetComponent<PartInventory>().inv_utility)) // Check if we need to sort this
            {
                UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortSection(PlayerData.inst.GetComponent<PartInventory>().inv_utility); // Perform the sort
                updateNeeded = true;
            }
        }
        if (PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items.Length >= 3)
        {
            if (UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortCheck(PlayerData.inst.GetComponent<PartInventory>().inv_weapon)) // Check if we need to sort this
            {
                UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortSection(PlayerData.inst.GetComponent<PartInventory>().inv_weapon); // Perform the sort
                updateNeeded = true;
            }
        }

        if (updateNeeded) // If we performed a sort, we now need to update the UI
        {
            UpdateInterfaceInventories();
        }
    }

    /// <summary>
    /// Identical to *PartsSortingCheck* (see above) but for the hideout cache. Called during separate cases.
    /// </summary>
    public void HideoutSortingCheck()
    {
        if (awaitingSort) // Don't attempt to sort while we are already sorting!
        {
            return;
        }

        bool updateNeeded = false;

        if (UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortCheck(hideout_inventory)) // Check if we need to sort this
        {
            UIManager.inst.partContentArea.GetComponent<UserInterface>().AutoSortSection(hideout_inventory); // Perform the sort
            updateNeeded = true;
        }

        if (updateNeeded) // If we performed a sort, we now need to update the UI
        {
            UpdateInterfaceInventories();
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
        Item _item = new Item(part._item);
        inventory.AddItem(_item, 1);

        // If an item takes up more than 1 slot, we add duplicate items (and track using a hashset)
        for (int i = 0; i < _item.itemData.slotsRequired - 1; i++)
        {
            Item duplicate = new Item(part._item);
            int uuid = uuids;

            duplicate.isDuplicate = true;
            duplicate.duplicate_uuid = uuid;

            inventory.AddItem(duplicate, 1);

            _item.duplicates.Add(uuids);
            uuids++;
        }
    }

    /// <summary>
    /// Drop a specific item from a specific inventory / actor around a specific position. Attempts to find the nearest free floor space.
    /// </summary>
    /// <param name="_item">The item to drop.</param>
    /// <param name="source">The source actor, can be null.</param>
    /// <param name="sourceInventory">The source inventory, can be null.</param>
    /// <param name="starting_pos">The center position to drop nearest to. Can be set to Vector2Int.zero as long as *source* is set.</param>
    public void DropItemOnFloor(Item _item, Actor source, InventoryObject sourceInventory, Vector2Int starting_pos)
    {
        if (_item.isDuplicate) // Don't drop duplicate items!
        {
            return;
        }

        if(source != null && starting_pos == Vector2Int.zero)
        {
            starting_pos = HF.V3_to_V2I(source.transform.position);
        }

        // Drop is as close to the source as possible
        TileBlock dropTile = MapManager.inst._allTilesRealized[starting_pos].bottom;

        Vector2Int dropLocation = HF.LocateFreeSpace(HF.V3_to_V2I(dropTile.transform.position)); // Find nearest free space
        dropTile = MapManager.inst._allTilesRealized[dropLocation].bottom;

        if (source != null && source.gameObject.GetComponent<PlayerData>()) // Is player?
        {
            string itemName = HF.GetFullItemName(_item);
            PlayDropSound(itemName); // Play the drop sound
            UIManager.inst.CreateNewLogMessage("Dropped " + itemName + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false); // Do a UI message
        }

        // Remove from inventory
        if(sourceInventory != null)
            sourceInventory.RemoveItem(_item);

        // Place the item
        PlaceItemIntoWorld(_item, new Vector2Int(dropTile.locX, dropTile.locY), dropTile);

        return;
    }

    public void DropItemResiduals(UserInterface itf, GameObject invSlot, bool isPlayer, bool forceful = false)
    {
        if (isPlayer)
        {
            #region Multi-Slot items
            if (invSlot.GetComponent<InvDisplayItem>().item.itemData.slotsRequired > 1)
            {
                foreach (var C in invSlot.GetComponent<InvDisplayItem>().secondaryChildren.ToList())
                {
                    // - DropItemOnFloor doesn't do duplicates!
                    //InventoryControl.inst.DropItemOnFloor(C.GetComponent<InvDisplayItem>().item, PlayerData.inst.GetComponent<Actor>(), _inventory, Vector2Int.zero);
                    InventoryControl.inst.animatedItems.Remove(itf.slotsOnInterface[C]); // Remove from animation tracking HashSet
                    itf.slotsOnInterface[C].RemoveItem(); // Remove from slots
                }
            }
            #endregion

            // Update player mass
            PlayerData.inst.currentWeight -= invSlot.GetComponent<InvDisplayItem>().item.itemData.mass;
            if (invSlot.GetComponent<InvDisplayItem>().item.itemData.propulsion.Count > 0)
            {
                PlayerData.inst.maxWeight -= invSlot.GetComponent<InvDisplayItem>().item.itemData.propulsion[0].support;
            }

            // Subtract the specified amount of energy
            if (!forceful)
            {
                PlayerData.inst.currentEnergy -= GlobalSettings.inst.partEnergyDetachLoss;
                PlayerData.inst.currentEnergy = Mathf.Clamp(PlayerData.inst.currentEnergy, 0, PlayerData.inst.maxEnergy);
            }

            // Update UI
            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateInventory();
            UIManager.inst.UpdateParts();

            InventoryControl.inst.animatedItems.Remove(itf.slotsOnInterface[invSlot]); // Remove from animation tracking HashSet
            itf.slotsOnInterface[invSlot].RemoveItem(); // Remove from slots
            InventoryControl.inst.UpdateInterfaceInventories(); // Update UI
        }
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
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_dropitem[$"EARTH_PART_LGT_0{Random.Range(1, 2)}"], 0.7f); // EARTH_PART_LGT_1/2
            }
            else
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_dropitem[$"EARTH_PART_0{Random.Range(1, 2)}"], 0.5f); // EARTH_PART_1/2
            }
        }
        else
        {
            if (isLight)
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_dropitem[$"STONE_PART_LGT_0{Random.Range(1, 3)}"], 0.7f); // STONE_PART_LGT_1/2/3
            }
            else
            {
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_dropitem[$"STONE_PART_0{Random.Range(1, 4)}"], 0.5f); // STONE_PART_1/2/3/4
            }
        }
    }

    public void RemoveItemFromAnInventory(Item item, InventoryObject inventory)
    {
        inventory.RemoveItem(item);

        // Also remove any duplicates if this item has multiple slots
        if(!item.isDuplicate && item.duplicates.Count > 0)
        {
            foreach (var D in item.duplicates)
            {
                // Check specified inventory for duplicates as specified in the parent item
                foreach (var I in inventory.Container.Items.ToList())
                {
                    if(D == I.item.duplicate_uuid)
                    {
                        inventory.RemoveItem(I.item);
                    }
                }
            }
        }
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
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>().inv_power.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Propulsion ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Utility ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items)
        {
            Debug.Log(item.item);
        }
        Debug.Log(" == Weapons ==");
        foreach (var item in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items)
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

[System.Serializable]
[Tooltip("Contains details regarding the item spawning process of a specific item.")]
public class ItemSpawnInfo
{
    public string name;
    public Vector2 location;
    public int amount = 1;
    [Tooltip("If this item should be considered 'native' to the environment. If false, scavengers will pick this up and recycle it.")]
    public bool native = false;
    [Tooltip("The chance for this item (if it is a prototype) to spawn in as a faulty prototype [0f to 1f].")]
    public float chanceForFaulty;

    // Extra stuff
    [Tooltip("Rarely used. Make an item start with corruption on it.")]
    public int corruptedAmount = 0;

    public ItemSpawnInfo(string name, Vector2 location, int amount, bool native, float rollForFaulty = 0f, int corruption = 0)
    {
        this.name = name;
        this.location = location;
        this.amount = amount;
        this.native = native;
        this.chanceForFaulty = rollForFaulty;
        this.corruptedAmount = corruption;

        // Failsafe for matter
        if (name.ToLower() == "Matter")
        {
            this.chanceForFaulty = 0f;
        }
    }
}