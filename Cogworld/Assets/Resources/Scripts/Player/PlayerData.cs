using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Stores all data related to the player.
/// </summary>
public class PlayerData : MonoBehaviour
{
    public static PlayerData inst;
    public void Awake()
    {
        inst = this;
        playerGameObject = this.gameObject;
    }

    private GameObject playerGameObject;

    [Header("Inventory")]
    public int powerSlots;
    public int propulsionSlots;
    public int utilitySlots;
    public int weaponSlots;
    //

    [Header("KeyValues")]
    [Header("  Top Right")]
    public int maxHealth;
    public int currentHealth;
    public int currentCoreExposure;
    //
    public int maxEnergy;
    public int currentEnergy;
    public float energyOutput1;
    public float energyOutput2;
    public int maxInternalEnergy = 0;
    public int currentInternalEnergy = 0;
    //
    public int maxMatter;
    public int currentMatter;
    public int maxInternalMatter = 0;
    public int currentInternalMatter = 0;
    //
    public int maxCorruption = 100;
    [Tooltip("Affects many things:" +
        "-Hacking: Every 3 points of corruption reduces the success rate by 1.")]
    public int currentCorruption;
    //
    public int currentHeat;
    public float heatRate1;
    public float heatRate2;
    public float naturalHeatDissipation = 25;
    //
    public int moveSpeed1;
    public int moveSpeed2;
    public BotMoveType moveType;
    [Tooltip("100 = No Siege, 5-1 Siege Imminent, 0 = In Siege Mode, <negative>1-5 Siege end")]
    public int timeTilSiege = 100;
    //
    //
    public float currentAvoidance;
    [Tooltip("Flight/Hover bonus")]
    public int evasion1;
    [Tooltip("Heat level (This value is negative)")]
    public int evasion2;
    [Tooltip("Movement speed (and whether recently moved)")]
    public int evasion3;
    [Tooltip("Evasion modifiers from utilities (e.g. Manuevering Thrusters)")]
    public int evasion4;
    [Tooltip("Cloaking modifiers from utilities (e.g. Cloaking Devices")]
    public int evasion5;
    public bool lockedInStasis = false;

    [Header("  Parts")]
    public int maxWeight;
    public int currentWeight;
    public int baseWeightSupport = 3;

    [Header("  Inventory")]
    public int maxInvSize;
    public int currentInvCount;

    //
    [Header("Movement")]
    public int currentSupport = 3;

    [Header("Alert Level")]
    [Tooltip("<0 = Low Sec, 1-5 = Lvl 1-5, 6 = High Security>")]
    public int alertLevel = 0;

    [Header("Hacking")]
    [Tooltip("The number of botnets. The first provides +6% bonus, the second 3%, and all remaining provide +1% cummulative success rate to all hacks.")]
    public int linkedTerminalBotnet = 0;
    [Tooltip("The number of active operator allies within 20 spaces. " +
        "The first provides +10%, the second 5%, the third 2%, and all remaining provide +1% cummulative success rate to all hacks.")]
    public int linkedOperators = 0;
    [Tooltip("The total bonus of all offensive hackware. e.g. A standard *Hacking Suite* provides a +10 bonus.")]
    public float hack_successBonus = 0f; // Success chance to do a hack
    public float hack_detectBonus = 0f;  // Chance to get detected while hacking (negative)
    //
    public List<TerminalCustomCode> customCodes = new List<TerminalCustomCode>();

    [Header("Allies")]
    [Tooltip("Allies are bots that follow the player, and that the player can order around. They are blue.")]
    public List<Actor> allies = new List<Actor>();
    [Tooltip("Followers are bots that follow the player, but that the player can't directly control. They are usually white, red, purple, etc.")]
    public List<Actor> followers = new List<Actor>();

    [Header("Unique Alignments")]
    public bool hasRIF = false;
    public bool hasImprinted = false;
    public bool hasFARCOM = false;

    [Header("*STATS THIS RUN*")]
    public int robotsKilled = 0;
    public int kills_0b10 = 0;
    public int kills_derelict = 0;
    public int kills_zion = 0;
    public int kills_warlord = 0;
    [Header("Unique Kills")]
    public bool uk_imprinter = false;
    public bool uk_warlord = false;
    public bool uk_dataminer = false;
    public bool uk_MAINC = false;
    public bool uk_architect = false;
    public bool uk_r17 = false;

    public SpecialTrait specialTrait; // FARCOM, CRM, Imprinted, RIF, etc.

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.GetComponent<PartInventory>())
        {
            CombatInputs();
            UpdateStats();
        }
    }



    public void SetDefaults()
    {
        powerSlots = GlobalSettings.inst.startingPowerSlots;
        propulsionSlots = GlobalSettings.inst.startingPropulsionSlots;
        utilitySlots = GlobalSettings.inst.startingUtilitySlots;
        weaponSlots = GlobalSettings.inst.startingWeaponSlots;

        // Player Stats
        maxHealth = GlobalSettings.inst.maxHealth_start;
        maxEnergy = GlobalSettings.inst.maxEnergy_start;
        maxMatter = GlobalSettings.inst.maxMatter_start;
        currentHealth = maxHealth;
        currentCoreExposure = 100;
        currentEnergy = GlobalSettings.inst.startingEnergy;
        energyOutput1 = GlobalSettings.inst.cogCoreOutput1;
        energyOutput2 = GlobalSettings.inst.cogCoreOutput2;
        currentMatter = GlobalSettings.inst.startingMatter;
        currentCorruption = GlobalSettings.inst.startingCorruption;
        currentHeat = 0;
        heatRate1 = GlobalSettings.inst.cogCoreHeat1;
        heatRate2 = GlobalSettings.inst.cogCoreHeat2;
        moveSpeed1 = GlobalSettings.inst.cogCoreDefaultMovement;
        moveSpeed2 = GlobalSettings.inst.cogCoreDefMovement2;
        // Evasion
        currentAvoidance = GlobalSettings.inst.cogCoreAvoidance;
        evasion1 = GlobalSettings.inst.startingEvasionWide;
        evasion2 = GlobalSettings.inst.startingEvasionWide;
        evasion3 = GlobalSettings.inst.startingEvasion;
        evasion4 = GlobalSettings.inst.startingEvasionWide;
        evasion5 = GlobalSettings.inst.startingEvasionWide;

        moveType = BotMoveType.Running; // aka Core
        

        // Parts
        currentWeight = 0;
        maxWeight = GlobalSettings.inst.startingWeight;

        // Inventory
        currentInvCount = 0;
        maxInvSize = GlobalSettings.inst.startingInvSize;

        // Update Inventory
        this.gameObject.GetComponent<PartInventory>().SetNewInventorySizes(powerSlots, propulsionSlots, utilitySlots, weaponSlots, maxInvSize);

        // Call UIMgr functions
        UIManager.inst.UpdatePSUI();
        UIManager.inst.UpdateParts();
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateTimer(0);

        // -----------
        // DEBUG
        // -----------
        customCodes.Add(new TerminalCustomCode(0));
        //customCodes.Add(new TerminalCustomCode("E-TEST", "TEST-TARGET"));
    }

    public void UpdateStats()
    {


        #region Update Stats - Movement
        bool R = false, T = false, H = false, F = false, WA = false;
        R = Action.HasWheels(this.GetComponent<Actor>());
        T = Action.HasTreads(this.GetComponent<Actor>());
        H = Action.HasHover(this.GetComponent<Actor>());
        F = Action.HasFlight(this.GetComponent<Actor>());
        WA = Action.HasLegs(this.GetComponent<Actor>());

        if (H)
        {
            moveType = BotMoveType.Hovering;
        }
        if (F)
        {
            moveType = BotMoveType.Flying;
        }
        if (WA)
        {
            moveType = BotMoveType.Walking;
        }
        if (R)
        {
            moveType = BotMoveType.Rolling;
        }
        if (T)
        {
            moveType = BotMoveType.Treading;
        }

        float energyUse, heatUse;
        (moveSpeed1, energyUse, heatUse) = Action.GetSpeed(this.GetComponent<Actor>());
        moveSpeed2 = 100 * (100 / moveSpeed1); // Get it into the percentage value
        #endregion

        #region Update Stats - Evasion/Avoidance
        List<int> individualEAvalues = new List<int>();

        (currentAvoidance, individualEAvalues) = Action.CalculateAvoidance(this.GetComponent<Actor>());
        evasion1 = individualEAvalues[0];
        evasion2 = individualEAvalues[1];
        evasion3 = individualEAvalues[2];
        evasion4 = individualEAvalues[3];
        evasion5 = individualEAvalues[4];

        #endregion

        #region Update Stats - Weight
        currentWeight = Action.GetTotalMass(this.GetComponent<Actor>());
        maxWeight = Action.GetTotalSupport(this.GetComponent<Actor>());
        #endregion

        // And finally update the UI
        UIManager.inst.UpdatePSUI();
    }

    public void NewLevelRestore()
    {
        currentHealth = maxHealth;
        currentCorruption = 0;
        currentEnergy = maxEnergy;
        currentHeat = 0;
    }

    #region Combat

    bool canDoTargeting = false;
    bool doTargeting = false;

    // Checks for combat inputs
    public void CombatInputs()
    {
        if(this.gameObject == null)
        {
            return;
        }

        // First we only want to do this if the player actually has weapons and atleast one is active
        ItemObject weaponInUse = HasActiveWeapon();
        if (weaponInUse)
        {
            canDoTargeting = true;

            // Now look for key inputs
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                doTargeting = !doTargeting;
            }

            if (doTargeting)
            {
                DoTargeting();
                Actor target = GetMouseTarget();
                CheckForMouseAttack(target);
            }
            else
            {
                ClearTargeting();
            }
        }
        else
        {
            canDoTargeting = false;
        }
    }

    public void DoTargeting()
    {
        ClearTargeting();

        // We want to draw a line from the player to their mouse cursor.
        
        // Cast a ray from the player to the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - this.gameObject.transform.position;
        float distance = Vector3.Distance(new Vector3Int((int)mousePosition.x, (int)mousePosition.y, 0), this.gameObject.transform.position);
        direction.Normalize();
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position, direction, distance);

        // Loop through all the hits and set the targeting highlight on each tile
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            if (tile != null && MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int((int)tile.transform.position.x, (int)tile.transform.position.y)))
            {
                if (!tile.targetingHighlight.activeInHierarchy)
                {
                    tile.targetingHighlight.SetActive(true);
                }
            }
        }

        // NOTE: This method looks awful but it works, any way to make this better in the future? Maybe not loop through every single tile on the map?

        // Clear specific tiles to make highlight thinner
        #region Awful Line visual correction
        foreach (TileBlock tile in MapManager.inst._allTilesRealized.Values)
        {
            if (tile.targetingHighlight.activeInHierarchy)
            {
                Vector2Int tileCell = new Vector2Int(tile.locX, tile.locY);
                TileBlock leftTile = null;
                TileBlock rightTile = null;
                TileBlock upTile = null;
                TileBlock downTile = null;
                TileBlock TL = null;
                TileBlock TR = null;
                TileBlock BL = null;
                TileBlock BR = null;

                // Check for adjacent highlighted tiles
                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x - 1, tileCell.y)))
                {
                    leftTile = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x - 1, tileCell.y)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x + 1, tileCell.y)))
                {
                    rightTile = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x + 1, tileCell.y)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x, tileCell.y + 1)))
                {
                    upTile = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x, tileCell.y + 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x, tileCell.y - 1)))
                {
                    downTile = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x, tileCell.y - 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x - 1, tileCell.y + 1)))
                {
                    TL = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x - 1, tileCell.y + 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x + 1, tileCell.y + 1)))
                {
                    TR = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x + 1, tileCell.y + 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x - 1, tileCell.y - 1)))
                {
                    BL = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x - 1, tileCell.y - 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }

                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(tileCell.x + 1, tileCell.y - 1)))
                {
                    BR = MapManager.inst._allTilesRealized[new Vector2Int(tileCell.x + 1, tileCell.y - 1)];
                }
                else
                {
                    ClearTargeting();
                    return;
                }
                //


                // Horizontal Line
                if (leftTile.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy &&
                    rightTile.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                {
                    if (TL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        leftTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    else if (BL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        leftTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    if (BR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        rightTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    else if (TR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        rightTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }

                }

                // Vertical Line
                if (upTile.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy &&
                    downTile.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                {
                    if (TL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        upTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    else if (TR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        upTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    if (BL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        downTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                    else if (BR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                    {
                        downTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    }
                }

                // Diagonal Line
                if (TL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy &&
                    BR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                {
                    leftTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    rightTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    upTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    downTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                }
                else if (TR.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy &&
                    BL.gameObject.GetComponent<TileBlock>().targetingHighlight.activeInHierarchy)
                {
                    leftTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    rightTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    upTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                    downTile.gameObject.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
                }
            }
        }
        #endregion

        #region Scan Window Indicator
        // - Show what's being targeted up in the / SCAN / window. -

        // We need to figure out what the player has their mouse over.
        // Objects of interest are:
        // -Walls, Doors, Bots, Machines, Floor items, traps

        // We are going to copy over the raycast down method from UIManager
        Vector3 lowerPosition = new Vector3(mousePosition.x, mousePosition.y, 2);
        Vector3 upperPosition = new Vector3(mousePosition.x, mousePosition.y, -2);
        direction = lowerPosition - upperPosition;
        distance = Vector3.Distance(new Vector3Int((int)lowerPosition.x, (int)lowerPosition.y, 0), upperPosition);
        direction.Normalize();
        RaycastHit2D[] hits2 = Physics2D.RaycastAll(upperPosition, direction, distance);

        // - Flags -
        GameObject wall = null;
        GameObject exit = null;
        GameObject bot = null;
        GameObject item = null;
        GameObject door = null;
        GameObject machine = null;
        GameObject trap = null;

        // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
        for (int i = 0; i < hits2.Length; i++)
        {
            RaycastHit2D hit = hits2[i];
            // PROBLEM!!! This list of hits is unsorted and contains multiple things that violate the heirarchy below. This MUST be fixed!

            // There is a heirarchy of what we want to display:
            // -A wall
            // -An exit
            // -A bot
            // -An item
            // -A door
            // -A machine
            // -A trap

            // We will solve this problem by setting flags. And then going back afterwards and using our heirarchy.

            #region Hierarchy Flagging
            if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Wall"))
            {
                // A wall
                wall = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<AccessObject>())
            {
                // An exit
                exit = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<Actor>() && hit.collider.GetComponent<Actor>() != PlayerData.inst.GetComponent<Actor>())
            {
                // A bot
                bot = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<Part>())
            {
                // An item
                item = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Door"))
            {
                // Door
                door = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<MachinePart>())
            {
                // Machine
                machine = hit.collider.gameObject;
            }
            else if (hit.collider.GetComponent<FloorTrap>())
            {
                // Trap
                trap = hit.collider.gameObject;
            }
            #endregion
        }

        if (wall)
        {
            UIManager.inst.Scan_FlipSubmode(true, wall);
            return;
        }
        else if (exit)
        {
            UIManager.inst.Scan_FlipSubmode(true, exit);
            return;
        }
        else if (bot)
        {
            UIManager.inst.Scan_FlipSubmode(true, bot);
            return;
        }
        else if (item)
        {
            UIManager.inst.Scan_FlipSubmode(true, item);
            return;
        }
        else if (door)
        {
            UIManager.inst.Scan_FlipSubmode(true, door);
            return;
        }
        else if (machine)
        {
            UIManager.inst.Scan_FlipSubmode(true, machine);
            return;
        }
        else if (trap)
        {
            UIManager.inst.Scan_FlipSubmode(true, trap);
            return;
        }
        else
        {
            UIManager.inst.Scan_FlipSubmode(false);
        }
        #endregion
    }

    public void ClearTargeting()
    {
        foreach (KeyValuePair<Vector2Int, TileBlock> T in MapManager.inst._allTilesRealized)
        {
            T.Value.targetingHighlight.SetActive(false);
        }

        foreach (KeyValuePair<Vector2Int, GameObject> T in MapManager.inst._layeredObjsRealized)
        {
            if (T.Value.GetComponent<TileBlock>())
            {
                T.Value.GetComponent<TileBlock>().targetingHighlight.SetActive(false);
            }
        }
    }

    public Actor GetMouseTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Actor actor = hit.collider.GetComponent<Actor>();

            if (actor != null && actor != this.GetComponent<Actor>())
            {
                return actor;
            }
        }

        return null;
    }

    public void CheckForMouseAttack(Actor target)
    {
        if (TurnManager.inst.isPlayerTurn)
        {
            if (Input.GetKey(KeyCode.Mouse0)) // Leftclick
            {
                Item equippedWeapon = Action.FindRangedWeapon(this.GetComponent<Actor>());
                if (equippedWeapon != null && !attackBuffer)
                {
                    StartCoroutine(AttackBuffer());
                    Action.RangedAttackAction(this.GetComponent<Actor>(), target, equippedWeapon);

                    ClearTargeting();
                    doTargeting = false;
                }
            }
        }
    }

    bool attackBuffer = false;
    IEnumerator AttackBuffer()
    {
        attackBuffer = true;

        yield return new WaitForSeconds(1f);

        attackBuffer = false;
    }

    public ItemObject HasActiveWeapon()
    {
        foreach (InventorySlot slot in this.GetComponent<PartInventory>()._invWeapon.Container.Items)
        {
            if(slot.item.Id > -1 && slot.item.state) // There is an item here, and its active
            {
                return slot.item.itemData;
            }
        }

        return null;
    }



    #endregion

    #region Inventory Related

    public void ClearInventory()
    {
        this.GetComponent<PartInventory>()._inventory.Container.Clear();
        this.GetComponent<PartInventory>()._invPower.Container.Clear();
        this.GetComponent<PartInventory>()._invPropulsion.Container.Clear();
        this.GetComponent<PartInventory>()._invUtility.Container.Clear();
        this.GetComponent<PartInventory>()._invWeapon.Container.Clear();
        //this.GetComponent<PartInventory>()._inventory.Container.Items = new InventorySlot[24];
    }

    private void OnApplicationQuit()
    {
        ClearInventory();
    }

    public void SavePlayerInventory()
    {
        GetComponent<PartInventory>()._inventory.Save();
        GetComponent<PartInventory>()._invPower.Save();
        GetComponent<PartInventory>()._invPropulsion.Save();
        GetComponent<PartInventory>()._invUtility.Save();
        GetComponent<PartInventory>()._invWeapon.Save();
    }

    public void LoadPlayerInventory()
    {
        GetComponent<PartInventory>()._inventory.Load();
        GetComponent<PartInventory>()._invPower.Load();
        GetComponent<PartInventory>()._invPropulsion.Load();
        GetComponent<PartInventory>()._invUtility.Load();
        GetComponent<PartInventory>()._invWeapon.Load();
    }

    #endregion
}