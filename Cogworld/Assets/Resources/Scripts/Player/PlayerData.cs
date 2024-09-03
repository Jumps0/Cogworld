using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;

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
    [SerializeField] private GameObject mouseTracker;

    [Header("Inventory")]
    public int powerSlots;
    public int propulsionSlots;
    public int utilitySlots;
    public int weaponSlots;
    
    #region Key Values
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
    [Tooltip("Goes from 0 to 100. Affects many things:" +
        "-Hacking: Every 3 points of corruption reduces the success rate by 1.")]
    public int currentCorruption;
    //
    public int currentHeat; // HUD shows full heat breakdown including stationary upkeep, when mobile, and injector/ablative cooling
    [Tooltip("Heat rate while not moving")]
    public float heatRate1;
    [Tooltip("Heat rate while moving")]
    public float heatRate2;
    [Tooltip("Heat rate soley from cooling that loses HP")]
    public float heatRate3;
    public int naturalHeatDissipation = 25;
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

    [Header("Used Artifacts")]
    public List<Item> artifacts_used = new List<Item>(); // TODO - When functionality for consumable items is added later, make sure to add used artifacts to this list.

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
    #endregion
    [Header("Allies")]
    [Tooltip("Allies are bots that follow the player, and that the player can order around. They are blue.")]
    public List<Actor> allies = new List<Actor>();
    [Tooltip("Followers are bots that follow the player, but that the player can't directly control. They are usually white, red, purple, etc.")]
    public List<Actor> followers = new List<Actor>();

    #region Stat Variables
    [Header("Unique Alignments")]
    public bool hasRIF = false;
    public bool hasImprinted = false;
    public bool hasFARCOM = false;

    [Header("RIF Values")] // TODO: Expand on this later when RIF is worked on. It's got its own sub menu now to show stats (BETA 14)
    public bool rif_immuneToCorruption = false;

    [Header("*STATS THIS RUN*")]
    public int robotsKilled = 0;
    public List<BotObject> robotsKilledData = new List<BotObject>();
    public List<BotAlignment> robotsKilledAlignment = new List<BotAlignment>();
    [Header("Unique Kills")]
    public bool uk_imprinter = false;
    public bool uk_warlord = false;
    public bool uk_dataminer = false;
    public bool uk_MAINC = false;
    public bool uk_architect = false;
    public bool uk_r17 = false;

    public SpecialTrait specialTrait; // FARCOM, CRM, Imprinted, RIF, etc.
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.GetComponent<PartInventory>())
        {
            CombatInputs();
            InventoryInputDetection();
            UpdateStats();
            HandleMouseHighlight();
        }
    }


    #region Stat Related
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
        heatRate3 = GlobalSettings.inst.cogCoreHeat3;
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
        if(timeTilSiege == 0)
        {
            moveType = BotMoveType.SIEGE;
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
    #endregion

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
        if (weaponInUse && !UIManager.inst.dataMenu.data_parent.activeInHierarchy && UIManager.inst.terminal_targetTerm == null && !HF.MouseBoundsCheck())
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

                CheckForMouseAttack();
            }
            else
            {
                if(targetLine.Count > 0)
                {
                    ClearTargeting();
                }
            }
        }
        else
        {
            canDoTargeting = false;
        }
    }

    public void ClearTargeting()
    {
        ClearAllHighlights();
        LTH_Clear();
        UIManager.inst.Evasion_Volley(false); // Close the /VOLLEY/ window
        canDoTargeting = false;
    }

    Vector3 oldMouseTarget = Vector3.zero;
    public void DoTargeting()
    {
        // - First off get where the mouse is. We don't want to keep re-drawing the same line every frame, only if the mouse position has changed.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // End point correction due to sneaky rounding
        mousePosition = new Vector3(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));

        if(mousePosition != oldMouseTarget) // This is causing some issues. Why?
        {
            oldMouseTarget = mousePosition;

            ClearAllHighlights();

            // We want to draw a line from the player to their mouse cursor.


            #region A) Line Drawing via Raycast
            /*
            #region Basic Line Drawing
            // Cast a ray from the player to the mouse position
            Vector3 direction = mousePosition - this.gameObject.transform.position;

            float distance = Vector3.Distance(new Vector3Int((int)mousePosition.x, (int)mousePosition.y, 0), this.gameObject.transform.position);
            //distance = Mathf.Clamp(distance, 0f, Action.GetWeaponRange(this.GetComponent<Actor>()));
            direction.Normalize();
            RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position, direction, distance);

            // Loop through all the hits and set the targeting highlight on each tile
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                if (!targetLine.ContainsKey(HF.V3_to_V2I(hit.transform.position)))
                { // Only make a new highlight if one doesn't exist at that location
                    CreateHighlightTile(HF.V3_to_V2I(hit.transform.position));
                }
            }
            #endregion

            // Clear specific tiles to make highlight thinner
            #region Line visual correction
            foreach (var T in targetLine)
            {
                Vector2Int loc = T.Key;
                
                // Horizontal Line
                if (targetLine.ContainsKey(Vector2Int.left + loc) && //  - * -
                    targetLine.ContainsKey(Vector2Int.right + loc))
                {
                    if (targetLine.ContainsKey(Vector2Int.up + loc) && targetLine[Vector2Int.up + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.up + loc);
                        targetLine[Vector2Int.up + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else if (targetLine.ContainsKey(Vector2Int.down + loc) && targetLine[Vector2Int.down + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.left + loc);
                        targetLine[Vector2Int.down + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                
                // Vertical Line
                if (targetLine.ContainsKey(Vector2Int.up + loc) &&
                    targetLine.ContainsKey(Vector2Int.down + loc))
                {
                    if (targetLine.ContainsKey(Vector2Int.up + Vector2Int.left + loc) && targetLine[Vector2Int.up + Vector2Int.left + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.up + loc);
                        targetLine[Vector2Int.up + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else if (targetLine.ContainsKey(Vector2Int.up + Vector2Int.right + loc) && targetLine[Vector2Int.up + Vector2Int.right + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.up + loc);
                        targetLine[Vector2Int.up + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (targetLine.ContainsKey(Vector2Int.down + Vector2Int.left + loc) && targetLine[Vector2Int.down + Vector2Int.left + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.down + loc);
                        targetLine[Vector2Int.down + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else if (targetLine.ContainsKey(Vector2Int.down + Vector2Int.right + loc) && targetLine[Vector2Int.down + Vector2Int.right + loc].GetComponent<SpriteRenderer>().enabled)
                    {
                        //DestroyHighlightTile(Vector2Int.down + loc);
                        targetLine[Vector2Int.down + loc].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                
                // Diagonal Line
                // NOTE: This doesn't actually do what we want, but im okay with it as is.
                if (targetLine.ContainsKey(Vector2Int.up + Vector2Int.left + loc) &&
                    targetLine.ContainsKey(Vector2Int.down + Vector2Int.right + loc)) 
                {
                    //  \
                    //   *
                    //    \


                    //DestroyHighlightTile(Vector2Int.up + Vector2Int.right + loc);
                    //DestroyHighlightTile(Vector2Int.down + Vector2Int.left + loc);
                    ////DestroyHighlightTile(Vector2Int.up + Vector2Int.right + loc); // no?
                    ////DestroyHighlightTile(Vector2Int.down + Vector2Int.right + loc); // no?

                    if (targetLine.ContainsKey(Vector2Int.up + Vector2Int.right + loc))
                        targetLine[Vector2Int.up + Vector2Int.right + loc].GetComponent<SpriteRenderer>().enabled = false;
                    if (targetLine.ContainsKey(Vector2Int.down + Vector2Int.left + loc))
                        targetLine[Vector2Int.down + Vector2Int.left + loc].GetComponent<SpriteRenderer>().enabled = false;
                }
                else if (targetLine.ContainsKey(Vector2Int.up + Vector2Int.right + loc) && targetLine[Vector2Int.up + Vector2Int.right + loc].GetComponent<SpriteRenderer>().enabled &&
                    targetLine.ContainsKey(Vector2Int.down + Vector2Int.left + loc) && targetLine[Vector2Int.down + Vector2Int.left + loc].GetComponent<SpriteRenderer>().enabled)    
                {                                                                          
                    //     /
                    //    *
                    //   /


                    //DestroyHighlightTile(Vector2Int.up + Vector2Int.left + loc);
                    ////DestroyHighlightTile(Vector2Int.down + Vector2Int.left + loc); // no?
                    ////DestroyHighlightTile(Vector2Int.up + Vector2Int.right + loc); // no?
                    //DestroyHighlightTile(Vector2Int.down + Vector2Int.right + loc);

                    if(targetLine.ContainsKey(Vector2Int.up + Vector2Int.left + loc))
                        targetLine[Vector2Int.up + Vector2Int.left + loc].GetComponent<SpriteRenderer>().enabled = false;
                    if (targetLine.ContainsKey(Vector2Int.down + Vector2Int.right + loc))
                        targetLine[Vector2Int.down + Vector2Int.right + loc].GetComponent<SpriteRenderer>().enabled = false;
                }

            }
            #endregion
            */
            #endregion

            #region B) Line Drawing via "Dart"
            // I'm not 100% happy with this method, but as implemented, it works better than raycasting (especially the line trimming).
            
            // - Explainer -
            // With this method we are imagining a "dart" which starts at the player, and is shot towards the target position.
            // The closest tile to the dart's closest position is added to the path.
            // We finish when we "reach" the end point.
            // There are some safety rails in place to make sure the "dart" doesn't shoot past the edge of the map, but honestly I
            // don't 100% trust it, so there is probably like a 0.1% chance when the player is aiming an error happens.

            path = new List<Vector3>();

            Vector2 start = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y));
            Vector2 finish = mousePosition;

            Vector2 currentPos = start;
            Vector2 direction = (finish - start).normalized;
            Vector2 lastDirection = direction; // Store the last direction
            float distance = Vector2.Distance(start, finish);

            while (Vector2.Distance(currentPos, finish) > 0.1f)
            {

                // Add current point to path
                path.Add(currentPos);

                // Move towards finish point in the calculated direction
                currentPos += direction;

                // Check if current position is out of bounds, if so, break the loop
                if (currentPos.x < 0 || currentPos.y < 0 || currentPos.x >= MapManager.inst._mapSizeX - 2 || currentPos.y >= MapManager.inst._mapSizeY - 2)
                    break;

                // Update direction towards finish point
                direction = (finish - currentPos).normalized;

                // Check if direction has changed (passed the finish point)
                if (Vector2.Dot(direction, lastDirection) < 0)
                    break; // Stop if the direction changes

                lastDirection = direction; // Update last direction
            }

            // Add the finish point to the path
            path.Add(finish);

            // - Now "draw" the path -

            foreach (var P in path) // Go through the path and mark each tile
            {
                if (!targetLine.ContainsKey(HF.V3_to_V2I(P)))
                {
                    CreateHighlightTile(HF.V3_to_V2I(P));
                }
            }

            if (GapCheckHelper(HF.V3_to_V2I(finish)))
            {
                GapCheck();
            }
            //CleanPath(); // Clean the path
            
            #endregion

            #region LOS Color check & Melee adjustment
            // If the player is using a melee weapon, we want to visually let them know it has a super short range.
            if (Action.FindMeleeWeapon(this.GetComponent<Actor>()) != null)
            {
                foreach (var T in targetLine)
                {
                    if (Vector2.Distance(this.transform.position, T.Key) > 1.55f)
                    {
                        SetHighlightColor(T.Key, highlightRed);
                    }
                }
            }

            // Here we check if the player actually has line-of-sight on the target.
            // -If yes, then the line is green, no changes necessary.
            // -If no, then the line, starting PAST the blocking object, is red.
            GameObject blocker = HF.ReturnObstacleInLOS(this.gameObject, mousePosition, true);
            if (blocker != null) // There is an obstacle!
            {
                float dist = Vector2.Distance(this.transform.position, blocker.transform.position);
                foreach (var T in targetLine)
                {
                    // We only want to change the line colors past the blocking object
                    if (Vector2.Distance(this.transform.position, T.Key) - 0.4f > dist) // -0.4f added because distances are weird?
                    {
                        SetHighlightColor(T.Key, highlightRed);
                    }
                }

                if(!TargetLineContainsHighlight())
                    SetHighlightColor(HF.V3_to_V2I(blocker.transform.position), highlightGreen); // Set the blocker highlight
            }

            // Only the final target light is bright green. Everything else is darker
            for (int i = 0; i < targetLine.ToList().Count; i++)
            {
                if (targetLine.ToList()[i].Value.GetComponent<SpriteRenderer>().color == highlightGreen) // Only change bright green tiles
                {
                    Color darkGreen = new Color(highlightGreen.r, 0.7f, highlightGreen.b, highlightGreen.a);
                    SetHighlightColor(targetLine.ToList()[i].Key, darkGreen);

                    
                    // Is this the last tile before some kind of blocker? Change it back to bright green
                    if (i + 1 < targetLine.ToList().Count && targetLine.ToList()[i + 1].Value.GetComponent<SpriteRenderer>().color == highlightRed)
                    {
                        SetHighlightColor(targetLine.ToList()[i].Key, highlightGreen); // Set the last highlight to the bright green.
                    }
                    
                }
            }

            #endregion

            #region Launcher Indicator
            // Here we handle the additional indicator needed for launcher weapons.
            // If the player's weapon:
            // 1) Is a launcher
            // 2) Has LOS to wherever they are targeting
            // 3) Is within the weapons range
            // Then and only then will we show the special indicator.
            Item launcher = Action.HasLauncher(this.GetComponent<Actor>());
            if (launcher != null & blocker == null && distance <= launcher.itemData.shot.shotRange)
            {
                // Success! Lets draw it.

                // =============================================================================================================== //
                // This comprises of:                                                                                              //
                // -A spheric, green highlighted area around the player's target tile, that indicates the radius of an explosion.  //
                //      -Each tile becomes darker as it moves away from the target tile.                                           //
                //      -The default brightness is about half of the normal target tile brightness.                                //
                // -Two "brackets" which appear around the above area, indicating the overall size of the weapon (range * 2 + 1).  //
                //      -These brackets appear on the sides furthest away from the Player                                          //
                // -A "scan" effect of the green highlighted tiles that happens every couple of seconds                            //
                // =============================================================================================================== //

                LTH_Clear(); // Clear any pre-existing tiles

                // - First off lets gather all the tiles (in a square) that are around the target tile, and within range.
                int range = launcher.itemData.explosionDetails.radius;
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));
                Vector2Int BL_corner = new Vector2Int(target.x - range, target.y - range);

                List<GameObject> tiles = new List<GameObject>();
                for (int x = 0 + BL_corner.x; x < (range * 2) + 1 + BL_corner.x; x++)
                {
                    for (int y = 0 + BL_corner.y; y < (range * 2) + 1 + BL_corner.y; y++)
                    {
                        if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(x, y)))
                        {
                            tiles.Add(MapManager.inst._allTilesRealized[new Vector2Int(x, y)].gameObject);
                        }
                    }
                }

                // - Now we need to go through the very fun process of refining this square into a circle. While we're at it, we spawn the prefabs aswell.
                foreach (GameObject T in tiles)
                {
                    float tileDistiance = Vector2Int.Distance(HF.V3_to_V2I(T.transform.position), target);
                    Vector2Int pos = HF.V3_to_V2I(T.transform.position);

                    // Check if the distance is within the radius of the circle
                    if (tileDistiance <= range)
                    {
                        // Tile is within the circle, create a prefab of it.
                        var spawnedTile = Instantiate(UIManager.inst.prefab_basicTile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
                        spawnedTile.name = $"LTH Tile: {pos.x},{pos.y}"; // Give grid based name
                        spawnedTile.transform.parent = this.transform;
                        spawnedTile.GetComponent<SpriteRenderer>().sortingOrder = 31;
                        lth_tiles.Add(spawnedTile);
                    }
                }

                // - We also need to assign them a unique color, as - starting from the center - the tiles will get darker and more transparent as it gets closer to the edge.
                foreach (GameObject T in lth_tiles)
                {
                    // Calculate the distance of the tile from the center
                    float tileDistiance = Vector2Int.Distance(HF.V3_to_V2I(T.transform.position), HF.V3_to_V2I(mousePosition));

                    // Calculate the normalized distance (0 to 1)
                    float normalizedDistance = Mathf.Clamp01(tileDistiance / (range + 1));

                    // Calculate the new color based on the normalized distance
                    Color A = Color.Lerp(UIManager.inst.highlightGreen, Color.black, normalizedDistance);

                    // Adjust transparency based on distance
                    float alpha = 1f - normalizedDistance;
                    A.a = alpha;

                    // We also need to pre-assign some values for the animation
                    float animationTime = 0.2f;

                    // - Set the starting color
                    T.GetComponent<SpriteRenderer>().color = A;

                    Color B = UIManager.inst.highlightGreen; // Mid point is green highlight

                    // - And assign the animation values.
                    // Starts from set color, goes to high green, then goes back to set color.
                    T.GetComponent<SimpleTileAnimator>().Init(A, B, animationTime);
                    T.GetComponent<SimpleTileAnimator>().InitChain(B, A, animationTime);
                }

                // ?



                // - Now we need to add the outside brackets
                // First lets find the two sides we need to put the brackets on.
                Vector2 sideA = Vector2.zero, sideB = Vector2.zero;
                (sideA, sideB) = HF.GetRelativePosition(HF.V3_to_V2I(this.transform.position), mousePosition);
                // !!! REMEMBER THAT THESE NEED TO BE INVERTED !!!

                // We now need to assemble these two brackets with our two prefabs stored in UIManager.
                // We need to carefully rotate these to make sure they look correct.
                // Remember that the default state of these prefabs is:
                //
                //       |            |
                //       |     and    |
                //   ____|            |
                //

                Vector2Int TL_corner = new Vector2Int(target.x - range, target.y + range);
                Vector2Int BR_corner = new Vector2Int(target.x + range, target.y - range);
                Vector2Int TR_corner = new Vector2Int(target.x + range, target.y + range);
                //Debug.Log("Center: " + target + " -- TL: " + TL_corner + " -- TR: " + TR_corner + " -- BL: " + BL_corner + " -- BR: " + BR_corner);
                int length = (range * 2) + 1; // How long each bracket needs to be. Reminder that each end is a corner

                // First we'll place the vertical line (left or right side)
                if (sideA == Vector2.left) // Targeting is to the LEFT of the place. So we need to face RIGHT.
                {
                    // First place the TOP corner, it needs to face right and down.
                    LTH_PlaceCorner(TL_corner + new Vector2Int(-1, 0), 180f);
                    // Then place the [length - 2] lines
                    for (int i = 0; i < length - 2; i++)
                    {
                        // No rotation needed
                        LTH_PlaceLine(BL_corner + new Vector2Int(-1, i + 1), 0f);
                    }
                    // Lastly, place the BOTTOM corner. It needs to face right and up.
                    LTH_PlaceCorner(BL_corner + new Vector2Int(-1, 0), -90f);
                }
                else if (sideA == Vector2.right || sideA == Vector2.zero) // Targeting is to the RIGHT of the place. So we need to face LEFT.
                {
                    // First place the TOP corner, it needs to face left and down.
                    LTH_PlaceCorner(TR_corner + new Vector2Int(1, 0), 90f);
                    // Then place the [length - 2] lines
                    for (int i = 0; i < length - 2; i++)
                    {
                        // No rotation needed
                        LTH_PlaceLine(BR_corner + new Vector2Int(1, i + 1), 0f);
                    }
                    // Lastly, place the BOTTOM corner. It needs to face left and up.
                    LTH_PlaceCorner(BR_corner + new Vector2Int(1, 0), 0f);
                }

                // Then we place the horizontal line (top or bottom)

                if (sideB == Vector2.up) // Targeting is ABOVE of the place. So we need to face DOWN.
                {
                    // First place the LEFT corner, it needs to place right and down.
                    LTH_PlaceCorner(TL_corner + new Vector2Int(0, 1), 180f);
                    // Then place the [length - 2] lines
                    for (int i = 0; i < length - 2; i++)
                    {
                        LTH_PlaceLine(TL_corner + new Vector2Int(i + 1, 1), 90f);
                    }
                    // Lastly, place the RIGHT corner. It needs to face left and down.
                    LTH_PlaceCorner(TR_corner + new Vector2Int(0, 1), 90f);
                }
                else if (sideB == Vector2.down || sideB == Vector2.zero) // Targeting is BELOW of the place. So we need to face UP.
                {
                    // First place the LEFT corner, it needs to place right and up.
                    LTH_PlaceCorner(BL_corner + new Vector2Int(0, -1), -90f);
                    // Then place the [length - 2] lines
                    for (int i = 0; i < length - 2; i++)
                    {
                        LTH_PlaceLine(BL_corner + new Vector2Int(i + 1, -1), 90f);
                    }
                    // Lastly, place the RIGHT corner. It needs to face left and up.
                    LTH_PlaceCorner(BR_corner + new Vector2Int(0, -1), 0f);
                }

                // - Finally, start the scan animation, and timer
                if(LTH_timerRoutine == null)
                {
                    LTH_timerRoutine = StartCoroutine(LTH_ScanTimer());
                }

                // - When the player fires, ALL targeting effects should dissapear until the projectile they fired detonates
                //Debug.Break();
            }
            else
            {
                LTH_Clear(); // Clear any pre-existing tiles
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

            if (wall && wall.GetComponent<TileBlock>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, wall);
            }
            else if (exit && exit.GetComponent<AccessObject>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, exit);
            }
            else if (bot && bot.GetComponent<Actor>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, bot);
            }
            else if (item && item.GetComponent<Part>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, item);
            }
            else if (door && door.GetComponent<TileBlock>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, door);
            }
            else if (machine && machine.GetComponent<MachinePart>().isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, machine);
            }
            else if (trap && trap.GetComponent<FloorTrap>().knowByPlayer && MapManager.inst._allTilesRealized[HF.V3_to_V2I(trap.transform.position)].isExplored)
            {
                UIManager.inst.Scan_FlipSubmode(true, trap);
            }
            else
            {
                UIManager.inst.Scan_FlipSubmode(false);
            }
            #endregion

            UIManager.inst.Evasion_Volley(true); // Open the /VOLLEY/ window
        }
    }

    #region Highlight Helper Functions
    [SerializeField] private Color highlightGreen;
    [SerializeField] private Color highlightRed;
    private Dictionary<Vector2Int, GameObject> targetLine = new Dictionary<Vector2Int, GameObject>();
    List<Vector3> path = new List<Vector3>();
    private void CreateHighlightTile(Vector2Int pos)
    {
        var spawnedTile = Instantiate(MapManager.inst.prefab_highlightedTile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"TargetLine: {pos.x},{pos.y}"; // Give grid based name
        spawnedTile.transform.parent = this.transform;
        spawnedTile.GetComponent<SpriteRenderer>().color = highlightGreen; // Default green color
        targetLine.Add(pos, spawnedTile);
    }

    private void DestroyHighlightTile(Vector2Int pos)
    {
        if (targetLine.ContainsKey(pos))
        {
            Destroy(targetLine[pos]);

            if (targetLine.ContainsKey(pos))
            { // ^ Safety check
                targetLine.Remove(pos);
            }
        }
    }

    private void SetHighlightColor(Vector2Int loc, Color _color)
    {
        if (targetLine.ContainsKey(loc))
        {
            targetLine[loc].GetComponent<SpriteRenderer>().color = _color;
        }
    }

    private void ClearAllHighlights()
    {
        foreach (var T in targetLine.ToList())
        {
            Destroy(T.Value);
        }

        targetLine.Clear();
    }

    private bool TargetLineContainsHighlight()
    {
        foreach(var T in targetLine)
        {
            if(T.Value.GetComponent<SpriteRenderer>().color == highlightGreen)
            {
                return true;
            }
        }

        return false;
    }

    #region "Dart" Target-line Helpers
    private void GapCheck()
    {
        // Due to a quirk in the line generation, there is occasionally a gap in the path between the last tile along the path and the end tile.
        // Here we will simply fill that 1 tile gap.

        // This issue doesn't appear to show up in diagonal lines, so we won't check the diagonal directions here.

        if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1]), HF.V3_to_V2I(path[0])) == Vector2.up)
        {
            CreateHighlightTile(HF.V3_to_V2I(path[path.Count - 1] + new Vector3(0, 1)));
        }
        else if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1]), HF.V3_to_V2I(path[0])) == Vector2.down)
        {
            CreateHighlightTile(HF.V3_to_V2I(path[path.Count - 1] + new Vector3(0, -1)));
        }
        else if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1]), HF.V3_to_V2I(path[0])) == Vector2.left)
        {
            CreateHighlightTile(HF.V3_to_V2I(path[path.Count - 1] + new Vector3(1, 0)));
        }
        else if (HF.GetDirection(HF.V3_to_V2I(path[path.Count - 1]), HF.V3_to_V2I(path[0])) == Vector2.right)
        {
            CreateHighlightTile(HF.V3_to_V2I(path[path.Count - 1] + new Vector3(-1, 0)));
        }
    }

    private bool GapCheckHelper(Vector2Int end)
    {
        // Checks around in a circle of the finish point.
        // If there are no tiles surrounding the finish tile (island), returns true.
        if (!targetLine.ContainsKey(end + Vector2Int.up)
            && !targetLine.ContainsKey(end + Vector2Int.down)
            && !targetLine.ContainsKey(end + Vector2Int.left)
            && !targetLine.ContainsKey(end + Vector2Int.right)
            && !targetLine.ContainsKey(end + Vector2Int.up + Vector2Int.left)
            && !targetLine.ContainsKey(end + Vector2Int.up + Vector2Int.right)
            && !targetLine.ContainsKey(end + Vector2Int.down + Vector2Int.left)
            && !targetLine.ContainsKey(end + Vector2Int.down + Vector2Int.right))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CleanPath()
    {
        Vector2Int C = HF.V3_to_V2I(this.transform.position); // the player's position

        // Sometimes the path can be a bit messy, lets fix that...
        // We usually have 2 cases to fix.

        // 1. Sometimes an additional tile is highlighted next to the player.
        // - We need to check diagonals around the player
        if (targetLine.ContainsKey(new Vector2Int(C.x + 1, C.y - 1))) // [ DOWN-RIGHT ]
        {
            // Destroy (Right & Down)
            DestroyHighlightTile(new Vector2Int(C.x + 1, C.y));
            DestroyHighlightTile(new Vector2Int(C.x, C.y - 1));
        }
        if (targetLine.ContainsKey(new Vector2Int(C.x + 1, C.y + 1))) // [ UP-RIGHT ]
        {
            // Destroy (Right & Up)
            DestroyHighlightTile(new Vector2Int(C.x + 1, C.y));
            DestroyHighlightTile(new Vector2Int(C.x, C.y + 1));
        }
        if (targetLine.ContainsKey(new Vector2Int(C.x - 1, C.y - 1))) // [ DOWN-LEFT ]
        {
            // Destroy (Left & Down)
            DestroyHighlightTile(new Vector2Int(C.x - 1, C.y));
            DestroyHighlightTile(new Vector2Int(C.x, C.y - 1));
        }
        if (targetLine.ContainsKey(new Vector2Int(C.x - 1, C.y + 1))) // [ UP-LEFT ]
        {
            // Destroy (Left & Up)
            DestroyHighlightTile(new Vector2Int(C.x - 1, C.y));
            DestroyHighlightTile(new Vector2Int(C.x, C.y + 1));
        }

        // 2. Sometimes an additional tile is highlighted along a diagonal.
        // - This is a bit more tricky to do as we need to check two tiles for each diagonal
        /* - Like this:
         *       ?
         *   * []
         *   [] *
         *  ?
         */

        // This is also tricky because we need to be able to modify the list LIVE.
        foreach (var P in targetLine)
        {
            Vector2Int loc = P.Key;

            if (targetLine.ContainsKey(new Vector2Int(loc.x + 1, loc.y - 1)) && targetLine[new Vector2Int(loc.x + 1, loc.y - 1)].GetComponent<SpriteRenderer>().enabled) // [ DOWN-RIGHT ]
            {
                // Destroy (Right & Down)
                //DestroyHighlightTile(new Vector2Int(loc.x + 1, loc.y));
                //DestroyHighlightTile(new Vector2Int(loc.x, loc.y - 1));
                if (targetLine.ContainsKey(new Vector2Int(loc.x + 1, loc.y)))
                    targetLine[new Vector2Int(loc.x + 1, loc.y)].GetComponent<SpriteRenderer>().enabled = false;
                if (targetLine.ContainsKey(new Vector2Int(loc.x, loc.y - 1)))
                    targetLine[new Vector2Int(loc.x, loc.y - 1)].GetComponent<SpriteRenderer>().enabled = false;
            }
            if (targetLine.ContainsKey(new Vector2Int(loc.x + 1, loc.y + 1)) && targetLine[new Vector2Int(loc.x + 1, loc.y + 1)].GetComponent<SpriteRenderer>().enabled) // [ UP-RIGHT ]
            {
                // Destroy (Right & Up)
                //DestroyHighlightTile(new Vector2Int(loc.x + 1, loc.y));
                //DestroyHighlightTile(new Vector2Int(loc.x, loc.y + 1));
                if (targetLine.ContainsKey(new Vector2Int(loc.x + 1, loc.y)))
                    targetLine[new Vector2Int(loc.x + 1, loc.y)].GetComponent<SpriteRenderer>().enabled = false;
                if (targetLine.ContainsKey(new Vector2Int(loc.x, loc.y + 1)))
                    targetLine[new Vector2Int(loc.x, loc.y + 1)].GetComponent<SpriteRenderer>().enabled = false;
            }
            if (targetLine.ContainsKey(new Vector2Int(loc.x - 1, loc.y - 1)) && targetLine[new Vector2Int(loc.x - 1, loc.y - 1)].GetComponent<SpriteRenderer>().enabled) // [ DOWN-LEFT ]
            {
                // Destroy (Left & Down)
                //DestroyHighlightTile(new Vector2Int(loc.x - 1, loc.y));
                //DestroyHighlightTile(new Vector2Int(loc.x, loc.y - 1));
                if (targetLine.ContainsKey(new Vector2Int(loc.x - 1, loc.y)))
                    targetLine[new Vector2Int(loc.x - 1, loc.y)].GetComponent<SpriteRenderer>().enabled = false;
                if (targetLine.ContainsKey(new Vector2Int(loc.x, loc.y - 1)))
                    targetLine[new Vector2Int(loc.x, loc.y - 1)].GetComponent<SpriteRenderer>().enabled = false;
            }
            if (targetLine.ContainsKey(new Vector2Int(loc.x - 1, loc.y + 1)) && targetLine[new Vector2Int(loc.x - 1, loc.y + 1)].GetComponent<SpriteRenderer>().enabled) // [ UP-LEFT ]
            {
                // Destroy (Left & Up)
                //DestroyHighlightTile(new Vector2Int(loc.x - 1, loc.y));
                //DestroyHighlightTile(new Vector2Int(loc.x, loc.y + 1));
                if (targetLine.ContainsKey(new Vector2Int(loc.x - 1, loc.y)))
                    targetLine[new Vector2Int(loc.x - 1, loc.y)].GetComponent<SpriteRenderer>().enabled = false;
                if (targetLine.ContainsKey(new Vector2Int(loc.x, loc.y + 1)))
                    targetLine[new Vector2Int(loc.x, loc.y + 1)].GetComponent<SpriteRenderer>().enabled = false;
            }
        }

    }
    #endregion


    #endregion

    #region Launcher Target Helper Functions
    public List<GameObject> lth_brackets = new List<GameObject>();
    public List<GameObject> lth_tiles = new List<GameObject>();

    /// <summary>
    /// This triggers the "scan" animation for the squares. Code stolen from UIManager
    /// </summary>
    private IEnumerator LTH_ScanSquare()
    {
        // First organize all the pre-existing tiles into a new Dictionary
        Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
        foreach (var tile in lth_tiles)
        {
            tiles.Add(HF.V3_to_V2I(tile.transform.position), tile);
        }

        float pingDuration = 0.25f;

        // Calculate how many rows there are
        int rowCount = 0;
        HashSet<int> rowIndices = new HashSet<int>();

        foreach (Vector2Int key in tiles.Keys)
        {
            // Adding the y-coordinate to the HashSet to keep only unique rows
            rowIndices.Add(key.y);
        }

        rowCount = rowIndices.Count;

        // Organize the tiles by *y* coordinate.
        // Initialize the new dictionary
        Dictionary<int, List<GameObject>> sortedDictionary = new Dictionary<int, List<GameObject>>();

        // Iterate through the original dictionary
        foreach (var kvp in tiles)
        {
            // Get the y-coordinate
            int yCoordinate = kvp.Key.y;

            // Check if the y-coordinate is already a key in the sorted dictionary
            if (!sortedDictionary.ContainsKey(yCoordinate))
            {
                sortedDictionary[yCoordinate] = new List<GameObject>();
            }

            // Add the GameObject to the list associated with the y-coordinate
            sortedDictionary[yCoordinate].Add(kvp.Value);
        }

        // Create a sorted dictionary with keys sorted in descending order
        var sortedKeys = new List<int>(sortedDictionary.Keys);
        sortedKeys.Sort((a, b) => b.CompareTo(a));

        // Create a new dictionary with sorted keys
        Dictionary<int, List<GameObject>> sortedResult = new Dictionary<int, List<GameObject>>();
        foreach (var key in sortedKeys)
        {
            sortedResult[key] = sortedDictionary[key];
        }


        // Play the scanline sound. ("UI/SCAN 5")
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[92]); // SCAN_5

        // Now go through each row, and ping every block in the row
        foreach (var kvp in sortedResult)
        {
            foreach (GameObject entry in kvp.Value) // Loop through each object in the micro-list
            {
                if(entry == null) // Lovely race condition here
                {
                    yield break;
                }

                SimpleTileAnimator animObj = entry.GetComponent<SimpleTileAnimator>();

                if (animObj != null)
                {
                    animObj.Animate(); // Make it play the scan animation
                }
            }

            yield return new WaitForSeconds(pingDuration / rowCount); // Equal time between rows
        }

        foreach (KeyValuePair<Vector2Int, GameObject> obj in tiles) // Probably uneccessary. Emergency stop
        {
            obj.Value.GetComponent<SimpleTileAnimator>().Stop();
        }
    }

    private void LTH_PlaceLine(Vector2Int pos, float rotation)
    {
        // Remember, default start state is:
        //
        //       |
        //       |  
        //       |
        //

        var spawnedTile = Instantiate(UIManager.inst.prefab_launcherTargetLine, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"LTH Line: {pos.x},{pos.y}"; // Give grid based name
        spawnedTile.transform.parent = this.transform;
        lth_brackets.Add(spawnedTile);

        // Now we need to rotate this so it faces the correct direction. 
        spawnedTile.transform.eulerAngles = new Vector3(spawnedTile.transform.eulerAngles.x, spawnedTile.transform.eulerAngles.y, rotation);
    }

    /// <summary>
    /// Places the corner for the Launcher Targeting indicator's brackets.
    /// </summary>
    /// <param name="pos">The position to place this prefab.</param>
    /// <param name="rotation">The amount to rotate the object by. Should be in 90 degree increments.</param>
    private void LTH_PlaceCorner(Vector2Int pos, float rotation)
    {
        // Remember, default start state is:
        //
        //       |
        //       |  
        //   ____|
        //

        var spawnedTile = Instantiate(UIManager.inst.prefab_launcherTargetCorner, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"LTH Corner: {pos.x},{pos.y}"; // Give grid based name
        spawnedTile.transform.parent = this.transform;
        lth_brackets.Add(spawnedTile);

        // Now we need to rotate this so it faces the correct direction. 
        spawnedTile.transform.eulerAngles = new Vector3(spawnedTile.transform.eulerAngles.x, spawnedTile.transform.eulerAngles.y, rotation);

    }

    private void LTH_Clear()
    {
        LTH_Stop();

        foreach (GameObject item in lth_brackets.ToList())
        {
            Destroy(item);
        }

        lth_brackets.Clear();

        foreach (GameObject item in lth_tiles.ToList())
        {
            Destroy(item);
        }

        lth_tiles.Clear();
    }

    private Coroutine LTH_timerRoutine;
    private IEnumerator LTH_ScanTimer()
    {
        // Get current mouse position (this could change later we need to track it)
        Vector3 lastTile = oldMouseTarget;

        yield return new WaitForSeconds(0.5f);

        Coroutine LTH_animationRoutine = null;

        // Get the current mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector3(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));

        // Has the mouse moved?
        if (lastTile == mousePosition)
        {
            // No? Continue, and play the animation.
            LTH_animationRoutine = StartCoroutine(LTH_ScanSquare());
        }
        else
        {
            // It has, stop.
            LTH_Stop();
        }

        yield return new WaitForSeconds(3f);

        // Get the current mouse position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector3(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));

        // Has the mouse moved?
        if(lastTile == mousePosition)
        {
            // No? Go again!
            LTH_timerRoutine = StartCoroutine(LTH_ScanTimer());
        }
        else
        {
            // It has, stop.
            LTH_Stop();
            StopCoroutine(LTH_animationRoutine);
        }
    }

    private void LTH_Stop()
    {
        if (LTH_timerRoutine != null)
        {
            StopCoroutine(LTH_timerRoutine);
            LTH_timerRoutine = null;
        }
    }

    #endregion

    
    private Actor GetMouseTarget()
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

    public void CheckForMouseAttack()
    {
        if (TurnManager.inst.isPlayerTurn)
        {
            Item equippedWeapon = Action.FindActiveWeapon(this.GetComponent<Actor>());

            // TODO: Have UIManager perform a little animation or whatever in the player's Matter/Energy bars to indicate how much this attack will cost
            if(equippedWeapon != null)
            {

            }

            if (Input.GetKey(KeyCode.Mouse0)) // Leftclick
            {
                if (equippedWeapon != null && !attackBuffer)
                {
                    Vector2Int mousePosition = HF.V3_to_V2I(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                    if (!Action.IsTargetWithinRange(HF.V3_to_V2I(this.transform.position), mousePosition, equippedWeapon)) // Does this weapon have the range to reach the target?
                    {
                        return; // Don't attack if the target is not within range.
                    }

                    // Does the player have the required resources to perform this attack?
                    if (!Action.HasResourcesToAttack(this.GetComponent<Actor>(), equippedWeapon))
                    {
                        return;
                    }

                    StartCoroutine(AttackBuffer()); // Activate the attacking (interaction) cooldown.

                    if (Action.IsMeleeWeapon(equippedWeapon))
                    {
                        GameObject target = HF.GetTargetAtPosition(mousePosition); // Use ray-line to get target
                        Action.MeleeAction(this.GetComponent<Actor>(), target);
                    }
                    else
                    {
                        // Is this is standard ranged attack or an AOE attack?
                        if (equippedWeapon.itemData.explosionDetails.radius > 0) // It's an AOE attack, we need to handle things slightly differently.
                        {
                            // Firstly, the target is wherever the player's mouse is.
                            GameObject target = HF.GetTargetAtPosition(HF.V3_to_V2I(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
                            // Second, the attack only attack happens when the projectile we are firing reaches the target.

                            // - Calculate the travel time
                            float distance = Vector3.Distance(this.transform.position, target.transform.position);
                            float travelTime = distance / 20f; // distance / speed

                            // Now we need to launch the projectile, and stall until it reaches its target.
                            StartCoroutine(StalledAOEAttack(travelTime, target, equippedWeapon));
                        }
                        else // Normal attack
                        {
                            GameObject target = HF.DetermineAttackTarget(this.gameObject, Camera.main.ScreenToWorldPoint(Input.mousePosition)); // Use ray-line to get target

                            Action.RangedAttackAction(this.GetComponent<Actor>(), target, equippedWeapon);
                        }
                    }

                    ClearAllHighlights();
                    LTH_Clear();
                    doTargeting = false;

                    // TODO: Tell UIManager to stop doing the cost "animation"

                }
            }
        }
    }

    private IEnumerator StalledAOEAttack(float waitTime, GameObject target, Item equippedWeapon)
    {
        // - Create an in-world projectile that goes to the target
        UIManager.inst.CreateLauncherProjectile(this.transform, target.transform, equippedWeapon.itemData);

        yield return new WaitForSeconds(waitTime); // Wait until it gets there

        Action.RangedAttackAction(this.GetComponent<Actor>(), target, equippedWeapon);
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
        foreach (InventorySlot slot in this.GetComponent<PartInventory>().inv_weapon.Container.Items)
        {
            if(slot.item.Id > -1 && slot.item.state && slot.item.disabledTimer <= 0) // There is an item here, and its active
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
        this.GetComponent<PartInventory>().inv_power.Container.Clear();
        this.GetComponent<PartInventory>().inv_propulsion.Container.Clear();
        this.GetComponent<PartInventory>().inv_utility.Container.Clear();
        this.GetComponent<PartInventory>().inv_weapon.Container.Clear();
        //this.GetComponent<PartInventory>()._inventory.Container.Items = new InventorySlot[24];
    }

    private void OnApplicationQuit()
    {
        ClearInventory();
    }

    public void SavePlayerInventory()
    {
        GetComponent<PartInventory>()._inventory.Save();
        GetComponent<PartInventory>().inv_power.Save();
        GetComponent<PartInventory>().inv_propulsion.Save();
        GetComponent<PartInventory>().inv_utility.Save();
        GetComponent<PartInventory>().inv_weapon.Save();
    }

    public void LoadPlayerInventory()
    {
        GetComponent<PartInventory>()._inventory.Load();
        GetComponent<PartInventory>().inv_power.Load();
        GetComponent<PartInventory>().inv_propulsion.Load();
        GetComponent<PartInventory>().inv_utility.Load();
        GetComponent<PartInventory>().inv_weapon.Load();
    }

    /// <summary>
    /// For items in the /PARTS/ menu. If the corresponding letter is pressed on the keyboard, that item should be toggled. WE IGNORE INVENTORY ITEMS.
    /// </summary>
    private void InventoryInputDetection()
    {
        // Check for player input
        if (Input.anyKey && !UIManager.inst.terminal_targetresultsAreaRef.gameObject.activeInHierarchy && !InventoryControl.inst.awaitingSort)
        {
            // Go through all the interfaces
            foreach (var I in InventoryControl.inst.interfaces)
            {
                string detect = "";
                InvDisplayItem reference = null;

                // Get the letter
                if (I.GetComponent<DynamicInterface>()) // Includes all items found in /PARTS/ menus (USES LETTER)
                {
                    foreach (var item in I.GetComponent<DynamicInterface>().slotsOnInterface)
                    {
                        reference = item.Key.GetComponent<InvDisplayItem>();
                        if (reference.item != null && reference.item.Id >= 0)
                        {
                            detect = reference._assignedChar;

                            KeyCode parse = KeyCode.None;
                            try
                            {
                                parse = (KeyCode)System.Enum.Parse(typeof(KeyCode), detect); // Make sure this is an actual key we can press
                            }
                            catch (Exception e)
                            {
                                // do nothing
                                return;
                            }

                            if (detect != "" && parse != KeyCode.None && Input.GetKeyDown(parse)) // Is that key currenlty down?
                            {
                                // Toggle!
                                if (reference != null)
                                {
                                    reference.Click();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Highlighted Mouse Tile
    [SerializeField] private GameObject mouseTile;

    public void HandleMouseHighlight()
    {
        // There are probably other cases where this shouldn't be enabled. Consider them here and add more when needed
        if(UIManager.inst.terminal_targetTerm == null)
        {
            mouseTile.SetActive(true);

            if (!mouseTile.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("FlashWhite")) // If the animator isn't flashing make it so.
            {
                mouseTile.GetComponent<Animator>().Play("FlashWhite");
            }

            // Get mouse position
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            // Snap to nearest (by converting to V2I)
            mousePos = new Vector3(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y), 0);

            mouseTile.transform.position = mousePos;
        }
        else // Disable it
        {
            mouseTile.SetActive(false);
        }
    }
    #endregion

    private Coroutine overheatwarning;
    private bool overheatcooldown = false;
    public void OverheatWarning()
    {
        if(!overheatcooldown)
        {
            // Play the sound
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[9]); // UI | ALARM_RESOURCES
            // Start the cooldown
            if(overheatwarning != null)
            {
                StopCoroutine(overheatwarning);
            }
            overheatwarning = StartCoroutine(OverheatCooldown());
        }
    }

    private IEnumerator OverheatCooldown()
    {
        overheatcooldown = true;

        yield return new WaitForSeconds(10f);

        overheatcooldown = false;
    }
}