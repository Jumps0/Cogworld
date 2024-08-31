using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking.Types;
using static StructureCTR;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    public static MapManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Prefas")]
    public GameObject _playerPrefab;
    [SerializeField] private TileBlock _tilePrefab;
    [SerializeField] private GameObject _accessPrefab;
    [SerializeField] private GameObject debugPrefab;
    [SerializeField] private GameObject minePrefab;
    public GameObject prefab_highlightedTile;

    [Header("References")]
    public GameObject cameraRef;
    public GameObject playerRef;
    [SerializeField] private GameObject levelLoadCover;



    [Header("Databases")]
    public TileDatabaseObject tileDatabase;
    public BotDatabaseObject botDatabase;
    public ItemDatabaseObject itemDatabase;
    public HackDatabaseObject hackDatabase;
    public KnowledgeDatabaseObject knowledgeDatabase;

    [Header("DEBUG")]
    // -- Debug --
    //
    public bool debugDisabled = false;
    //
    // --       --

    // - Level Data -
    [Header("Level Data")]
    public int currentLevel; // Goes from -10 to -1
    public int currentBranch;// Goes from 0 (no branch) to ~5
    [Tooltip("Is the current level a branch?")]
    public bool currentLevelIsBranch = false;
    public LevelName levelName;
    public string currentLevelName;
    public int mapSeed = 0;
    //
    [Tooltip("Successful indirect hacking of central *database-related* targets (queries, schematics, analysis, prototypes) " +
        "incurs a 25% chance to trigger a database lockout, preventing indirect access to those types of targets at every terminal on the same map.")]
    public bool centerDatabaseLockout = false;
    //
    [Tooltip("Used by AI to patrol to.")]
    public List<Vector2Int> pointsOfInterest = new List<Vector2Int>();
    public List<Vector2Int> initialAISpawnPositions = new List<Vector2Int>();

    [Header("Misc")]
    [SerializeField] private Vector3 originalPlayerSpawnLocation;
    [Tooltip("Used by engineers to find things to repair. This list is updated by TileBlocks that get damaged.")]
    public List<GameObject> damagedStructures = new List<GameObject>();

    [Header("MapGen Debug")]
    [Tooltip("1 = Caves | 2 = Normal | 3 = Custom")]
    public int mapType = 0;
    [Tooltip("-1 = Starting Cave | 0 = Exiles")]
    public int customMapType = -1;
    [Tooltip("!! Must be an Odd number !!")]
    public Vector2Int setMapSize = new Vector2Int(21, 21);
    Vector3 playerSpawnLocation = Vector3.zero;
    public bool loaded = false;

    #region InitNewLevel
    public IEnumerator InitNewLevel()
    {
        levelLoadCover.SetActive(true); // Enable the Level Load cover
        Vector2Int mapSize = Vector2Int.zero;

        //Debug.Log($"Creating a map of type [{mapType}].");

        if (currentLevel == -11)
            GlobalSettings.inst.SetStartingValues();

        // - Generate the Level -
        //

        /*
        if(CreateHauberkDungeon && !CreatePerlinDungeon && !CreateCaveDungeon)
        {
            setMapSize = new Vector2Int((int)(setMapSize.x / 2) + 1, (int)(setMapSize.y / 2) + 1); // Halved cause it gets x2'd
            DungeonGenerator.instance.GenerateHauberkDungeon(setMapSize.x, setMapSize.y);
            mapType = 1;
        }
        else if (!CreateHauberkDungeon && CreatePerlinDungeon && !CreateCaveDungeon)
        {
            DungeonGenerator.instance.GeneratePerlinDungeon(setMapSize.x, setMapSize.y);
            mapType = 2;
        }
        */

        if (mapType == 1) // Cave Dungeon
        {
            DungeonGenerator.instance.GenerateCaveDungeon(setMapSize.x, setMapSize.y);
            GenerateByGrid(DungeonGenerator._dungeon);
            mapSize.x = DungeonGenerator._dungeon.GetLength(0);
            mapSize.y = DungeonGenerator._dungeon.GetLength(1);
        }
        else if (mapType == 2) // Normal (0b10 Complex) Dungeon
        {
            DungeonManagerCTR.instance.genData = mapGenSpecifics[(currentLevel + 10)];

            while (!DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().mapGenComplete)
                yield return null; // Wait for loading to finish...

            GenerateByCTR();
            yield return null;
        }
        else // Custom
        {
            switch (customMapType)
            {
                case -1: // Starting cave, nothing to do here

                    break;
                case 0: // EXILEs cave
                    DungeonManagerCTR.instance.doDungeonGen = true;

                    while (!DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().mapGenComplete)
                        yield return null; // Wait for loading to finish...

                    GenerateByCTR();
                    yield return null;
                    break;
            }
        }


        // - Place the Player -
        // (Depends on Map Gen)
        Vector2Int sl = Vector2Int.zero;
        if (DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().validSpawnLocations.Count > 0)
        {
            sl = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().validSpawnLocations[Random.Range(0, DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().validSpawnLocations.Count - 1)];
        }
        originalPlayerSpawnLocation = new Vector3(sl.x, sl.y, 0f);
        playerSpawnLocation = originalPlayerSpawnLocation;

        PreInitializedItems(); // This is here because if its any lower "FillWithRock" will put walls everywhere and break many things.

        if (mapType == 1) // Cave
        {
            // Then dirty up the tiles a bit
            foreach (KeyValuePair<Vector2Int, TileBlock> floor in _allTilesRealized)
            {
                float rand = Random.Range(0f, 1f);
                if (rand > 0.4 && floor.Value.tileInfo.type == TileType.Floor) // 60% chance
                {
                    floor.Value.SetToDirty();
                }
            }
        }
        else if (mapType == 2) // Complex
        {
            mapSize = new Vector2Int(DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeX, DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeY);

            FillWithRock(mapSize);

        }
        else
        {
            switch (customMapType)
            {
                case -1:
                    mapSize = new Vector2Int(120, 120);
                    GridManager.inst.grid = new GameObject[mapSize.x + 1, mapSize.y + 1];

                    CustomMap_StartingCave(); // Overrides spawn position in here so we good
                    FillWithRock(mapSize);
                    break;

                case 0: // EXILEs cave
                    mapSize = new Vector2Int(DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeX, DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeY);
                    GridManager.inst.grid = new GameObject[mapSize.x + 1, mapSize.y + 1];

                    FillWithRock(mapSize);
                    break;
                default:

                    break;
            }
        }

        PlaceBranchNExits(); // Place exits (map type logic handled inside)

        DrawBorder(mapSize.x, mapSize.y); // Draw the border

        foreach (KeyValuePair<Vector2Int, GameObject> door in _layeredObjsRealized) // Setup all the doors
        {
            if (door.Value.GetComponent<DoorLogic>())
            {
                door.Value.GetComponent<DoorLogic>().LoadActivationTiles();
            }
        }

        // Set mapSize var
        _mapSizeX = mapSize.x;
        _mapSizeY = mapSize.y;

        CreateRegions();
        TurnManager.inst.SetAllUnknown(); // Also fills the regions

        // Spawn the player
        var spawnedPlayer = PlacePlayer();

        // Sync the player's stats
        if (tempPlayer == null)
        {
            // - First apply defaults
            spawnedPlayer.GetComponent<PlayerData>().SetDefaults();
        }
        else
        {
            LoadFromTempPlayer(spawnedPlayer.gameObject);
        }

        // Setup player's UI
        if (!firstTimeUISetup)
        {
            UIManager.inst.FirstTimeStartup(); // This also plays the UI animation

            /*
            InventoryControl.inst.ClearInterfacesInventories();
            InventoryControl.inst.p_inventoryPower.Container.Items = new InventorySlot[1];
            InventoryControl.inst.p_inventoryPropulsion.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventoryUtilities.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventoryWeapons.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventory.Container.Items = new InventorySlot[5];
            */

            InventoryControl.inst.SetInterfaceInventories();
            firstTimeUISetup = true;
        }

        yield return null;

        QuestManager.inst.Init();

        // -- Place Machines (Static & Interactable) --
        //
        PlaceMachines();
        //
        // --                                        --

        // -- Place Random Items --
        //

        PlaceRandomItems(mapType);

        //
        // --

        // -- AI / Bot Related --
        //
        CalculatePointsOfInterest(); // Find POIs

        initialAISpawnPositions.Clear();

        PlacePassiveBots(mapSize);
        PlaceHostileBots(mapSize);

        AssignMachineNames(); // Assign names to all placed machines
        AssignMachineCommands(); // Assign commands (for terminal interaction) to all placed machines.
        ZoneTerminals(); // Create terminal zones
        UIManager.inst.GetComponent<BorderIndicators>().CreateIndicators(); // Create indicators for all (interactable) machines

        //
        // --            --

        //if (mapType == 2)
        //{
        TurnManager.inst.LoadActors();
        //}

        yield return new WaitForEndOfFrame();

        levelLoadCover.SetActive(false); // Disable the Level Load cover
        UIManager.inst.NewFloor_BeginAnimate(); // Do the new floor "scanning" animation

        if (mapType == 3)
        {
            switch (customMapType) // Starting Map
            {
                case -1:
                    UIManager.inst.CreateNewLogMessage("Systems online...", UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
                    UIManager.inst.CreateNewLogMessage("Loading variables...", UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
                    UIManager.inst.CreateNewLogMessage("CORE=STABLE", UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
                    UIManager.inst.CreateNewLogMessage("INTEGRATION=OK", UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
                    break;

                default:
                    break;
            }
        }

        if (logEvoChanges) // If player evolved, display those changes
        {
            if (evoChanges[0] > 0) // Power
            {
                UIManager.inst.CreateNewLogMessage("PARAMETERS=POWER+" + evoChanges[0].ToString(), UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
            }
            if (evoChanges[1] > 0) // Propulsion
            {
                UIManager.inst.CreateNewLogMessage("PARAMETERS=PROPULSION+" + evoChanges[1].ToString(), UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
            }
            if (evoChanges[2] > 0) // Utility
            {
                UIManager.inst.CreateNewLogMessage("PARAMETERS=UTILITY+" + evoChanges[2].ToString(), UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
            }
            if (evoChanges[3] > 0) // Weapon
            {
                UIManager.inst.CreateNewLogMessage("PARAMETERS=WEAPON+" + evoChanges[3].ToString(), UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
            }

            logEvoChanges = false;
            evoChanges = new List<int>();
        }

        LocationLog(currentLevelName.ToUpper());

        if (mapType == 2)
        {
            if (!rogueBotArrivalMessage)
            {
                UIManager.inst.CreateNewLogMessage("ALERT: A rogue bot has emerged from the junkyard. Terminate on contact.", UIManager.inst.complexWhite, UIManager.inst.inactiveGray, false, true);
                rogueBotArrivalMessage = false;
            }
        }

        TurnManager.inst.AllEntityVisUpdate(); // Update vis

        // Load stored intel (non-branches)
        if (!currentLevelIsBranch)
            GameManager.inst.RevealStoredIntel();

        loaded = true;
    }

    public void FreezePlayer(bool frozen)
    {
        if (playerRef)
        {
            playerRef.GetComponent<PlayerGridMovement>().playerMovementAllowed = !frozen;
        }
    }

    #endregion

    #region Hideout Related
    public IEnumerator InitNewHideout()
    {
        Vector2Int mapSize = Vector2Int.zero;

        GlobalSettings.inst.SetStartingValues();
        currentLevelName = BaseManager.inst.data.layerName.ToUpper();

        // Change this later !!!
        DungeonGenerator.instance.GenerateCaveDungeon(121, 121);
        GenerateByGrid(DungeonGenerator._dungeon);
        mapSize.x = DungeonGenerator._dungeon.GetLength(0);
        mapSize.y = DungeonGenerator._dungeon.GetLength(1);
        // Set mapSize var
        _mapSizeX = mapSize.x;
        _mapSizeY = mapSize.y;

        yield return new WaitForSeconds(1f); // --- DELAY ---

        // - Place the Player -
        // Pick a random cave and spot in that cave to spawn in (change later)

        int random = Random.Range(0, DungeonGenerator.instance.Caves.Count - 1);

        List<Vector2Int> spawnPoints = HF.LIST_IV2_to_V2I(DungeonGenerator.instance.Caves[random]);
        List<Vector2Int> validSpawnPoints = new List<Vector2Int>();

        // Iterate through each spawn point
        foreach (Vector2Int spawnPoint in spawnPoints)
        {
            int minDistanceFromEdge = 12;

            // Check if the spawn point is at least 'minDistanceFromEdge' units away from the map border
            if (spawnPoint.x >= minDistanceFromEdge && spawnPoint.x < mapSize.x - minDistanceFromEdge &&
                spawnPoint.y >= minDistanceFromEdge && spawnPoint.y < mapSize.y - minDistanceFromEdge)
            {
                validSpawnPoints.Add(spawnPoint);
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            //Debug.LogError("ERROR: No valid spawn points found!");
            //Debug.Break();
            // Just spawn at (50,50) for now
            validSpawnPoints.Add(new Vector2Int(50, 50));
        }

        Vector2Int spawnLoc = validSpawnPoints[Random.Range(0, validSpawnPoints.Count - 1)];

        playerSpawnLocation = new Vector3(spawnLoc.x, spawnLoc.y, 0);
        originalPlayerSpawnLocation = playerSpawnLocation;

        PlaceGenericOutpost(spawnLoc); // Place the outpost
        FillWithRock(mapSize);

        // Then dirty up the tiles a bit
        foreach (KeyValuePair<Vector2Int, TileBlock> floor in _allTilesRealized)
        {
            float rand = Random.Range(0f, 1f);
            if (rand > 0.6 && floor.Value.tileInfo.type == TileType.Floor) // 40% chance
            {
                floor.Value.SetToDirty();
            }
        }

        DrawBorder(mapSize.x, mapSize.y); // Draw the border

        CreateRegions();
        TurnManager.inst.SetAllUnknown(); // Also fills the regions

        foreach (KeyValuePair<Vector2Int, GameObject> door in _layeredObjsRealized) // Setup all the doors
        {
            if (door.Value.GetComponent<DoorLogic>())
            {
                door.Value.GetComponent<DoorLogic>().LoadActivationTiles();
            }
        }

        AssignMachineNames(); // Assign names to all placed machines
        AssignMachineCommands(); // Assign commands (for terminal interaction) to all placed machines.
        ZoneTerminals(); // Create terminal zones
        UIManager.inst.GetComponent<BorderIndicators>().CreateIndicators(); // Create indicators for all (interactable) machines

        // Spawn the player
        var spawnedPlayer = PlacePlayer();

        QuestManager.inst.Init();

        // Sync the player's stats
        if (tempPlayer == null)
        {
            // - First apply defaults
            spawnedPlayer.GetComponent<PlayerData>().SetDefaults();
        }
        else
        {
            LoadFromTempPlayer(spawnedPlayer.gameObject);
        }

        // Setup player's UI
        if (!firstTimeUISetup)
        {
            UIManager.inst.FirstTimeStartup();

            /*
            InventoryControl.inst.ClearInterfacesInventories();
            InventoryControl.inst.p_inventoryPower.Container.Items = new InventorySlot[1];
            InventoryControl.inst.p_inventoryPropulsion.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventoryUtilities.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventoryWeapons.Container.Items = new InventorySlot[2];
            InventoryControl.inst.p_inventory.Container.Items = new InventorySlot[5];
            */

            InventoryControl.inst.SetInterfaceInventories();
            firstTimeUISetup = true;
        }

        TurnManager.inst.AllEntityVisUpdate(); // Update vis
        //
        // --            --

        // Enable Control
        spawnedPlayer.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;

        // - Completion Effects -
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.INTRO_Clips[4], 0.25f); // DONE
        PlayAmbientMusic(); // Ambient music

        UIManager.inst.CreateNewLogMessage("Arrived at Hideout...", UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
        UIManager.inst.CreateNewLogMessage("LOCATION=" + BaseManager.inst.data.layerName.ToUpper(), UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);

        loaded = true;
    }

    public void PlaceGenericOutpost(Vector2Int center)
    {
        /*
         * An outpost is a 16x16 "base" found in caves, occupied by AI and has a few things inside.
         * The base itself is made completely out of cave walls and is 16x16.
         * It has a clear outer ring of empty space 2 wide.
         * There are 1-2 rooms inside the outpost.
         * All doors in/out are double doors.
         */

        // The dungeon has already been generated, we just need to place this outpost where needed.

        // 1 - Place 20x20 flat ground.
        Vector2Int offset = new Vector2Int(center.x - 10, center.y - 10);
        for (int x = 0 + offset.x; x < 20 + offset.x; x++)
        {
            for (int y = 0 + offset.y; y < 20 + offset.y; y++)
            {
                CreateBlock(new Vector2Int(x, y), HF.IDbyTheme(TileType.Floor));
            }
        }

        // 2 - Place 16x16 outer walls
        int wallID = HF.IDbyTheme(TileType.Wall);
        offset = new Vector2Int(center.x - 8, center.y - 8);
        for (int x = 0 + offset.x; x < 16 + offset.x; x++)
        {
            CreateBlock(new Vector2Int(x, offset.y), wallID);
            CreateBlock(new Vector2Int(x, offset.y + 15), wallID);
        }
        for (int y = 1 + offset.y; y < 15 + offset.y; y++)
        {
            CreateBlock(new Vector2Int(offset.x, y), wallID);
            CreateBlock(new Vector2Int(offset.x + 15, y), wallID);
        }


        // 3 - Create the rooms (this should be changed later)
        // For now its gonna be pre-set
        Vector2Int bl = new Vector2Int(center.x - 8, center.y - 8); // Bottom left corner

        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 1), wallID);
        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 2), wallID);
        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 3), wallID);
        //
        CreateBlock(new Vector2Int(bl.x + 7, bl.y + 3), wallID);
        CreateBlock(new Vector2Int(bl.x + 8, bl.y + 3), wallID);
        CreateBlock(new Vector2Int(bl.x + 9, bl.y + 3), wallID);
        //
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 3), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 4), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 5), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 6), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 9), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 10), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 11), wallID);
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 12), wallID);
        //
        CreateBlock(new Vector2Int(bl.x + 7, bl.y + 12), wallID);
        CreateBlock(new Vector2Int(bl.x + 8, bl.y + 12), wallID);
        CreateBlock(new Vector2Int(bl.x + 9, bl.y + 12), wallID);
        //
        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 12), wallID);
        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 13), wallID);
        CreateBlock(new Vector2Int(bl.x + 5, bl.y + 14), wallID);

        // 4 - Place the doors
        CreateBlock(new Vector2Int(bl.x + 7, bl.y), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x + 7, bl.y + 15), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x, bl.y + 5), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x, bl.y + 9), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 7), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x + 10, bl.y + 8), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x + 15, bl.y + 6), HF.IDbyTheme(TileType.Door));
        CreateBlock(new Vector2Int(bl.x + 15, bl.y + 7), HF.IDbyTheme(TileType.Door));

        // 5 - Place custom machines
        PlaceIndividualMachine(new Vector2Int(bl.x + 2, bl.y + 3), 1, 4); // Terminal 4x3 "Pipeworks"
        PlaceIndividualMachine(new Vector2Int(bl.x + 4, bl.y + 7), 0, 11); // Static Machine (Outpost Generator)
        PlaceIndividualMachine(new Vector2Int(bl.x + 8, bl.y + 4), 0, 0); // Static Machine (Recharging Bay)

        PlaceIndividualMachine(new Vector2Int(bl.x + 3, bl.y + 13), 2, 2); // Fabricator 4x2 "Alice"

        // 6 - Place Cache
        PlaceHideoutCache(new Vector2Int(bl.x + 8, bl.y + 10));

        // # - Test bot
        Actor testBot = PlaceBot(new Vector2Int(bl.x + 12, bl.y + 5), 9);
        // Test QUEST Bot
        Actor questBot = PlaceBot(new Vector2Int(bl.x + 5, bl.y + 16), 13);

        // test trap
        PlaceTrap(MapManager.inst.itemDatabase.Items[103], new Vector2Int(bl.x + 5, bl.y + 11));
    }

    /// <summary>
    /// Places the object/machine prefab which the player can use to access their hideout's inventory storage.
    /// </summary>
    private void PlaceHideoutCache(Vector2 pos)
    {
        // Set prefab
        GameObject prefab = imp_customTerminals[0];

        // ~~Adjust position since its 2x2 (We use the bottom left tile as the center)~~
        Vector2 offset = Vector2.zero; //new Vector2(-0.5f, -0.5f);
        pos += offset;

        // Instantiate it at the correct position
        GameObject cache = Instantiate(prefab, pos, Quaternion.identity, mapParent);

        // Add to layers
        MachinePart[] machines = cache.GetComponentsInChildren<MachinePart>();
        foreach (MachinePart M in machines)
        {
            M.GetComponent<SpriteRenderer>().sortingOrder = 7;
            Vector2Int loc = new Vector2Int((int)(M.gameObject.transform.position.x + offset.x), (int)(M.gameObject.transform.position.y + offset.y));
            _allTilesRealized[loc].occupied = true;
            _layeredObjsRealized[loc] = M.gameObject;
        }
        /*
         * NOTE:
         * In the machine prefabs, all components my be perfectly aligned on exact numbers. If there are any decimals in the numbers (eg. -1.9999) there
         * is a high chance that the spawning will break and the machine part will be spawned in the incorrect space due to rounding.
         */

        // Setup the machine's script
        cache.GetComponentInChildren<TerminalCustom>().SetupAsCache();
    }

    #endregion

    bool rogueBotArrivalMessage = false;
    bool firstTimeUISetup = false;
    // Recent evo changes
    public bool logEvoChanges = false;
    public List<int> evoChanges = new List<int>(); // 0 = Power, 1 = Propulsion, 2 = Utility, 3 = Weapon

    #region Random Item Placement
    private void PlaceRandomItems(int mapType)
    {
        switch (mapType)
        {
            case 1: // Caves

                break;

            case 2: // Complex
                List<RoomCTR> rooms = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms;
                int itemsPlaced = 0;

                List<int> validitems = new List<int>();
                foreach (ItemObject item in InventoryControl.inst._itemDatabase.Items)
                {
                    // Items that can spawn on at this level
                    if (item.schematicDetails.hackable && item.schematicDetails.location[0].y == currentLevel)
                    {
                        validitems.Add(item.data.Id);
                    }
                }

                while (itemsPlaced < 50)
                {
                    RoomCTR aRoom = rooms[Random.Range(0, rooms.Count - 1)];
                    List<DTile> spots = aRoom.GetInterior;

                    Vector2Int loc = spots[Random.Range(0, spots.Count - 1)].position;

                    InventoryControl.inst.CreateItemInWorld(validitems[Random.Range(0, validitems.Count - 1)], loc, true);
                    itemsPlaced++;
                }

                break;

            case 3: // Custom

                if (customMapType == -1) // Starting map
                {
                    // We want to spawn the starting items centered in the first room
                    // All in all, we should spawn:
                    // - 4 movement parts (2 of one type, 2 of another | Lets just do legs/treads)
                    // - 2 armor parts
                    // - 2 engines
                    // - 1 storage (small/medium)
                    // - 1 device
                    // - 1 processor
                    // - 3 random weapon (2 of one type, 1 of other)
                    // - And finally, 200 matter

                    Vector2Int spawnArea = new Vector2Int(50, 50); // Bottom Left Corner of our 8x8 room
                    List<Vector2Int> occupiedLocations = new List<Vector2Int>(); // We don't want to stack things
                    occupiedLocations.Add(new Vector2Int((int)playerSpawnLocation.x, (int)playerSpawnLocation.y));

                    // And for the matter RNG (this is overly complex)
                    int random = Random.Range(75, 150);
                    List<int> matterRNG = new List<int>
                    {
                        random,
                        200 - random
                    };
                    int[] idsToSpawn = { 4, 4, 9, 9, Random.Range(10, 13), Random.Range(10, 13), 2, 3, 5, 15, 14, 16, 6, 8, 17, 17 };

                    SpawnItems(idsToSpawn, spawnArea, matterRNG);

                    InventoryControl.inst.CreateItemInWorld(45, new Vector2Int(57, 57), true); // ONLY FOR TESTING. REMOVE LATER  (Beamcaster)
                    InventoryControl.inst.CreateItemInWorld(102, new Vector2Int(56, 57), true); // ONLY FOR TESTING. REMOVE LATER (Rocket Launcher)
                    InventoryControl.inst.CreateItemInWorld(101, new Vector2Int(55, 57), true); // ONLY FOR TESTING. REMOVE LATER (Vibroblade)
                    InventoryControl.inst.CreateItemInWorld(91, new Vector2Int(54, 57), true); // ONLY FOR TESTING. REMOVE LATER (Exp. Target Analysis Processor)
                    InventoryControl.inst.CreateItemInWorld(105, new Vector2Int(53, 57), true); // ONLY FOR TESTING. REMOVE LATER (Hvy. Siege Treads)
                    InventoryControl.inst.CreateItemInWorld(66, new Vector2Int(52, 57), true); // ONLY FOR TESTING. REMOVE LATER (Ex. Chip 1)
                }


                break;

            default:

                break;
        }
    }

    void SpawnItems(int[] itemIds, Vector2Int spawnArea, List<int> randomValues = null)
    {
        HashSet<Vector2Int> usedLocations = new HashSet<Vector2Int>();
        int i = 0;

        foreach (int itemId in itemIds)
        {
            Vector2Int location = GetRandomLocation(spawnArea, usedLocations);

            if (itemId == 17) // This is *MATTER*, we need a random amount.
            {
                InventoryControl.inst.CreateItemInWorld(itemId, location, true, randomValues[i]);
                i++;
            }
            else // Just normal item.
            {
                InventoryControl.inst.CreateItemInWorld(itemId, location, true);
            }

            usedLocations.Add(location);
        }
    }

    Vector2Int GetRandomLocation(Vector2Int spawnArea, HashSet<Vector2Int> usedLocations)
    {
        Vector2Int location;

        do
        {
            int x = Random.Range(spawnArea.x + 1, spawnArea.x + 7);
            int y = Random.Range(spawnArea.y + 1, spawnArea.y + 7);

            location = new Vector2Int(x, y);
        } while (usedLocations.Contains(location));

        return location;
    }
    #endregion

    #region Map Realization

    [Header("New MapGen Details")]
    public int xOff = 0;
    public int yOff = 0;

    [SerializeField] private Transform mapParent;
    [SerializeField] private Transform botParent;
    public void GenerateByGrid(Tile[,] grid)
    {

        int xSize = grid.GetLength(0);
        int ySize = grid.GetLength(1);

        GridManager.inst.grid = new GameObject[xSize, ySize]; // Grid gets made here

        mapParent = new GameObject().transform;
        mapParent.name = "~ Grid Parent ~";

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                CreateBlock(new Vector2(x + xOff, y + yOff), HF.IDbyTheme(HF.Tile_to_TileType(grid[x, y])));
            }
        }
    }

    [Header("Regions")]
    public int regionSize = 15;
    public Dictionary<Vector2Int, Region> regions = new Dictionary<Vector2Int, Region>();

    /// <summary>
    /// Creates a number of regions determined by the map size, each TileBlock within that region gets assigned to it.
    /// </summary>
    public void CreateRegions()
    {
        // Calculate the number of regions in each axis
        int numRegionsX = Mathf.CeilToInt((float)_mapSizeX / regionSize);
        int numRegionsY = Mathf.CeilToInt((float)_mapSizeY / regionSize);

        // Iterate through each region
        for (int x = 0; x < numRegionsX; x++)
        {
            for (int y = 0; y < numRegionsY; y++)
            {
                // Calculate the position of the region
                Vector2Int regionPosition = new Vector2Int(x, y);

                // Create a new region
                Region region = new Region()
                {
                    size = regionSize,
                    pos = regionPosition,
                    objects = new List<GameObject>()
                };

                // Add the region to the dictionary
                regions.Add(regionPosition, region);
            }
        }

        // Example: Display the regions in the console
        foreach (KeyValuePair<Vector2Int, Region> entry in regions)
        {
            
        }
    }

    private void CreateCaveSpawnpoints()
    {
        // We are going to pick a few random locations. 4-6
        int random = UnityEngine.Random.Range(4, 6);

        for (int i = 0; i < random; i++)
        {
            int _rand = Random.Range(0, DungeonGenerator.instance.Caves.Count - 1);

            List<IntVector2> spawnPoints = DungeonGenerator.instance.Caves[_rand];

            int randomPoint = Random.Range(0, spawnPoints.Count - 1);

            IntVector2 spawnLoc = spawnPoints[randomPoint];

            DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().validSpawnLocations.Add(HF.IV2_to_V2I(spawnLoc));
        }
    }

    /// <summary>
    /// Generate the map using a pre-created CTR dungeon.
    /// </summary>
    public void GenerateByCTR()
    {
        int borderSizeX = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeX;
        int borderSizeY = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().sizeY;
        GridManager.inst.grid = new GameObject[borderSizeX + 1, borderSizeY + 1];

        // First the tiles
        foreach (KeyValuePair<Vector3, GameObject> tile in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedTiles)
        {
            // Floor
            if (tile.Value.tag == "Floor")
            {
                bool isTileDirty = false;

                if (tile.Value.name.Contains("*"))
                {
                    if(Random.Range(0f, 1f) < 0.07f) // 7% chance
                    {
                        isTileDirty = true;
                    }
                }

                CreateBlock(new Vector2(tile.Key.x + xOff, tile.Key.y + yOff), HF.IDbyTheme(TileType.Floor), isTileDirty);
            }
            else if (tile.Value.tag == "Wall")
            {
                if (tile.Value.name.Contains("*")) // Specific wall tile
                {
                    string[] split = tile.Value.name.Split("*"); // we want right side
                    string tileName = split[1];

                    CreateBlock(new Vector2(tile.Key.x + xOff, tile.Key.y + yOff), HF.GetTileByString(tileName).Id);
                }
                else // Generic themed wall tile
                {
                    CreateBlock(new Vector2(tile.Key.x + xOff, tile.Key.y + yOff), HF.IDbyTheme(TileType.Wall));
                }
            }
        }

        // Then the doors
        foreach (KeyValuePair<Vector3, GameObject> tile in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedDoors.ToList())
        {
            CreateBlock(new Vector2(tile.Key.x + xOff, tile.Key.y + yOff), HF.IDbyTheme(TileType.Door));
        }

        // And any pre-placed objects (mostly just machines)
        if(DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().prePlacedObjects.Count > 0)
        {
            foreach (GameObject obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().prePlacedObjects)
            {
                /*
                if (_layeredObjsRealized.ContainsKey(HF.V3_to_V2I(obj.transform.position)))
                {
                    //Debug.Log("Problem! - Dupe [" + HF.V3_to_V2I(obj.transform.position) + "]");
                    Debug.Log("KEYERROR: [" + HF.V3_to_V2I(obj.transform.position) + "]" +
                        " when trying to place: " + obj.name + ", Dict<> already contains: " + _layeredObjsRealized[HF.V3_to_V2I(obj.transform.position)]);
                }
                else
                {
                    
                }
                */

                // We need to place a floor tile below this (since its a layered object)
                CreateBlock(new Vector2(obj.transform.position.x + xOff, obj.transform.position.y + yOff), HF.IDbyTheme(TileType.Floor));

                if (!obj.GetComponent<Actor>()) // Sometimes there are bots in here
                {
                    // This is an awkward bypass instead of using .Add because for some reason neighboring machines get assigned the same key???
                    _layeredObjsRealized[HF.V3_to_V2I(obj.transform.position + new Vector3(xOff, yOff, 0f))] = obj;
                }
            }
        }
    }

    private void PreInitializedItems()
    {
        // We interpret what type of object it is by its TAG,
        // and what to actually place by its NAME.
        //
        // Items: Category-PoolType (ex. Storage-Normal)
        // Bots: Bot Type (matched closest to player depth)
        // Access: "Access"-Branch/Stairs-LevelName
        // Machine: "Machine" 

        foreach (GameObject obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().preInitObjects)
        {
            Vector3 spawnLocation = obj.transform.position + new Vector3(xOff, yOff, 0f);
            if (obj.tag.Contains("Item"))
            {

                // There needs to be a floor under this
                CreateBlock(spawnLocation, HF.IDbyTheme(TileType.Floor));

                if (obj.gameObject.name.Contains("*")) // Specific Item Reference
                {
                    string[] itemName = obj.gameObject.name.Split("*");
                    InventoryControl.inst.CreateItemInWorld(HF.GetItemByString(itemName[0]).data.Id, HF.V3_to_V2I(spawnLocation));
                }
                else if (obj.gameObject.GetComponent<MiscItemPool>()) // Specific Item Pool
                {
                    InventoryControl.inst.CreateItemInWorld(obj.gameObject.GetComponent<MiscItemPool>().itemPool[Random.Range(0, obj.gameObject.GetComponent<MiscItemPool>().itemPool.Count - 1)].data.Id, HF.V3_to_V2I(spawnLocation));
                }
                else // Generic Reference
                {
                    string[] words = obj.gameObject.name.Split('-');
                    string category = words[0];
                    string _rating = words[1];

                    ItemType _type = HF.GetItemTypeByString(category);

                    // Failsafe
                    if (_rating.Contains("(Clone)"))
                    {
                        string[] temp = _rating.Split("(");
                        _rating = temp[0];
                    }

                    // Generate a list of valid items to spawn based on the rating
                    List<int> validitems = new List<int>();
                    foreach (ItemObject item in InventoryControl.inst._itemDatabase.Items)
                    {
                        if (item.type == _type && item.rating == int.Parse(_rating))
                        {
                            validitems.Add(item.data.Id);
                        }
                    }

                    InventoryControl.inst.CreateItemInWorld(validitems[Random.Range(0, validitems.Count - 1)], HF.V3_to_V2I(spawnLocation));
                }
            }
            else if (obj.tag.Contains("Bot"))
            {
                // There needs to be a floor under this
                CreateBlock(spawnLocation, HF.IDbyTheme(TileType.Floor));

                // First check to see if this is a specific bot to place
                BotObject bot = HF.GetBotByString(obj.gameObject.name);
                
                // If a bot wasn't found, try the generic method
                if (bot == null)
                {
                    string[] words = obj.gameObject.name.Split('-');
                    //string type = words[0]; // "Bot-"
                    string rating = words[1];

                    bot = HF.FindBotOfTier(int.Parse(rating));

                    PlaceBot(HF.V3_to_V2I(spawnLocation), bot.Id, obj.gameObject);
                }
                else // Bot was found, place it
                {
                    PlaceBot(HF.V3_to_V2I(spawnLocation), bot.Id, obj.gameObject);
                }
            }
            else if (obj.tag == "Trigger" || obj.gameObject.name.Contains("Trigger"))
            {
                // There needs to be a floor under this
                CreateBlock(spawnLocation, HF.IDbyTheme(TileType.Floor));

                obj.transform.position += spawnLocation; // Janky nudge
                obj.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                obj.gameObject.transform.parent = mapParent;
                triggers.Add(obj.gameObject);
            }
            else if (obj.tag == "Event" || obj.gameObject.name.Contains("Event"))
            {
                obj.transform.position += spawnLocation; // Janky nudge
                obj.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                obj.gameObject.transform.parent = mapParent;
                events.Add(obj.gameObject);
            }
        }

        foreach (GameObject obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().prePlacedObjects.ToList())
        {
            Vector3 spawnLocation = obj.transform.position + new Vector3(xOff, yOff, 0f);

            if (obj.tag.Contains("Access") || obj.gameObject.name.Contains("Access"))
            {
                // There needs to be a floor under this
                CreateBlock(spawnLocation, HF.IDbyTheme(TileType.Floor));

                string[] words = obj.gameObject.name.Split('-');
                string exitType = words[1];
                string exitTarget = words[2];

                int _target = int.Parse(exitTarget);

                if (exitType.Contains("Branch"))
                {
                    PlaceLevelExit(HF.V3_to_V2I(spawnLocation), true, _target);
                }
                else // Stairs (up)
                {
                    PlaceLevelExit(HF.V3_to_V2I(spawnLocation), false, _target);
                }

            }
            else if (obj.gameObject.name.Contains("Machine"))
            {
                // We are just going to create an exact copy of this part and copy over its component data
                GameObject M = Instantiate(obj.gameObject);
                M.transform.position = spawnLocation;
                M.transform.rotation = obj.transform.rotation;

                CopyComponentData<SpriteRenderer>(obj.gameObject, M);
                CopyComponentData<MachinePart>(obj.gameObject, M);

                #region Specific Machine Types
                if (obj.gameObject.GetComponent<Terminal>())
                {
                    CopyComponentData<Terminal>(obj.gameObject, M);
                    machines_terminals.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<TerminalCustom>())
                {
                    CopyComponentData<TerminalCustom>(obj.gameObject, M);
                    machines_customTerminals.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<Scanalyzer>())
                {
                    CopyComponentData<Scanalyzer>(obj.gameObject, M);
                    machines_scanalyzers.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<Garrison>())
                {
                    CopyComponentData<Garrison>(obj.gameObject, M);
                    machines_garrisons.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<RepairStation>())
                {
                    CopyComponentData<RepairStation>(obj.gameObject, M);
                    machines_repairStation.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<Fabricator>())
                {
                    CopyComponentData<Fabricator>(obj.gameObject, M);
                    machines_fabricators.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<RecyclingUnit>())
                {
                    CopyComponentData<RecyclingUnit>(obj.gameObject, M);
                    machines_recyclingUnits.Add(M.gameObject);
                }
                else if (obj.gameObject.GetComponent<StaticMachine>())
                {
                    CopyComponentData<StaticMachine>(obj.gameObject, M);
                    machines_static.Add(M.gameObject);
                }
                #endregion

                M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                Vector2Int loc = HF.V3_to_V2I(M.transform.position);
                M.gameObject.transform.parent = mapParent;
                _allTilesRealized[loc].occupied = true;
                _layeredObjsRealized[loc] = M.gameObject;
            }
        }

        #region Old List Clearing
        // Clear the old lists
        foreach (GameObject obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().preInitObjects.ToList())
        {
            Destroy(obj.gameObject);
        }
        DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().preInitObjects.Clear();

        foreach (GameObject obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().prePlacedObjects.ToList())
        {
            Destroy(obj.gameObject);
        }
        DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().prePlacedObjects.Clear();

        // -- 
        foreach (KeyValuePair<Vector3, GameObject> obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedTiles.ToList())
        {
            Destroy(obj.Value.gameObject);
        }
        DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedTiles.Clear();

        foreach (KeyValuePair<Vector3, GameObject> obj in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedDoors.ToList())
        {
            Destroy(obj.Value.gameObject);
        }
        DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().placedDoors.Clear();
        #endregion
    }

    private void DrawBorder(int sizeX, int sizeY)
    {
        int xSize = sizeX;
        int ySize = sizeY;

        for (int x = -1; x < xSize; x++) // Bottom lane & Top Lane
        {
            CreateBlock(new Vector2(x + xOff, -1 + yOff), 0);
            CreateBlock(new Vector2(x + xOff, ySize + -1 + yOff), 0);
        }
        for (int y = -1; y < ySize; y++) // Left and Right Lanes
        {
            CreateBlock(new Vector2(-1 + xOff, y + yOff), 0);
            CreateBlock(new Vector2(xSize + -1 + xOff, y + yOff), 0);
        }
    }

    private void CreateBlock(Vector2 pos, int type, bool dirty = false)
    {

        int _tileID = type;

        var spawnedTile = Instantiate(_tilePrefab, new Vector3(pos.x * GridManager.inst.globalScale, pos.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedTile.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedTile.name = $"Tile {pos.x} {pos.y} - "; // Give grid based name

        spawnedTile.tileInfo = MapManager.inst.tileDatabase.Tiles[_tileID]; // Assign tile data from database by ID

        spawnedTile.name += spawnedTile.tileInfo.type.ToString(); // Modify name with type

        spawnedTile.tileInfo.currentVis = TileVisibility.Unknown; // All tiles start hidden
        spawnedTile.isExplored = false;
        spawnedTile.isVisible = false;

        if(dirty) // There's probably a better way of doing this
            spawnedTile.SetToDirty();

        FogOfWar.inst.unseenTiles.Add(spawnedTile); // Add to unseen tiles

        spawnedTile._highlightPerm.GetComponent<SpriteRenderer>().color = Color.white;

        spawnedTile.locX = (int)pos.x; // Assign X location
        spawnedTile.locY = (int)pos.y; // Assign Y location


        if (_tileID != 0)
        {  // Don't add impassible tiles

            // VVV Expand this later, it sucks! VVV
            if (MapManager.inst.tileDatabase.Tiles[_tileID].type != TileType.Door) // Things that arent doors
            {
                if (_allTilesRealized.ContainsKey(new Vector2Int((int)pos.x, (int)pos.y)))
                {
                    // Something already exists here, overwrite it and destroy newest instantiation
                    _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].tileInfo = MapManager.inst.tileDatabase.Tiles[_tileID];
                    _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].name = $"Tile {pos.x} {pos.y} - " + spawnedTile.tileInfo.type.ToString();
                    _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].tileInfo.currentVis = TileVisibility.Unknown;
                    _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].StartCheck();
                }
                else
                {
                    _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)] = spawnedTile; // Add to Dictionary
                }

                // In some scenarios, when we are placing a tile at a position, there may be another one there, so we want to overwrite it.
                // Simply deleting the old tile DOES NOT WORK because it already exists in a bunch of other lists and would cause massive problems.
                // So we need to update the already existing tile's data with the new data and simply delete the newly instantiated tile so it is never added.

                if (GridManager.inst.grid[(int)pos.x, (int)pos.y] == null)
                {
                    GridManager.inst.grid[(int)pos.x, (int)pos.y] = spawnedTile.gameObject; // Fill grid
                }
                else
                {
                    CopyComponentData<PlayerData>(spawnedTile.gameObject, GridManager.inst.grid[(int)pos.x, (int)pos.y]); // Copy over the values    
                    Destroy(spawnedTile.gameObject);
                }
            }
            else if (MapManager.inst.tileDatabase.Tiles[_tileID].type == TileType.Door) // Doors
            {
                _layeredObjsRealized[new Vector2Int((int)pos.x, (int)pos.y)] = spawnedTile.gameObject;
                spawnedTile.AddComponent<DoorLogic>();
                spawnedTile.GetComponent<DoorLogic>()._tile = spawnedTile.GetComponent<TileBlock>();
                spawnedTile.GetComponent<DoorLogic>()._location = new Vector2Int((int)pos.x, (int)pos.y);
                spawnedTile.GetComponent<DoorLogic>()._open = MapManager.inst.tileDatabase.Tiles[_tileID].altSprite;
                spawnedTile.GetComponent<DoorLogic>()._closed = MapManager.inst.tileDatabase.Tiles[_tileID].displaySprite;
                spawnedTile.AddComponent<AudioSource>();
                spawnedTile.GetComponent<AudioSource>().playOnAwake = false;
                spawnedTile.GetComponent<AudioSource>().loop = false;
                spawnedTile.GetComponent<DoorLogic>().source = spawnedTile.GetComponent<AudioSource>();
                spawnedTile.GetComponent<DoorLogic>().source.spatialBlend = 1f;
                spawnedTile.GetComponent<DoorLogic>().source.volume = 0.5f;
                spawnedTile.GetComponent<SpriteRenderer>().sortingOrder = 5;

                GridManager.inst.grid[(int)pos.x, (int)pos.y] = spawnedTile.gameObject; // Fill grid

                // As a failsafe, if this door is being placed on top of a wall, we need to turn that wall into a floor tile.
                if (_allTilesRealized.ContainsKey(new Vector2Int((int)pos.x, (int)pos.y)))
                {
                    TileBlock T = _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)];
                    if(T.tileInfo.type == TileType.Wall) // Is it a wall? We need to change that
                    {
                        // First get what type of floor tile we need to place
                        int id = HF.IDbyTheme(TileType.Floor);

                        _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].gameObject.name = $"Tile {pos.x} {pos.y} - "; // Give grid based name
                        _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].tileInfo = MapManager.inst.tileDatabase.Tiles[id]; // Assign tile data from database by ID
                        _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].gameObject.name += _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].tileInfo.type.ToString(); // Modify name with type
                        _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].StartCheck();

                        // And update the grid too
                        if (GridManager.inst.grid[(int)pos.x, (int)pos.y] != null)
                        {
                            GridManager.inst.grid[(int)pos.x, (int)pos.y] = _allTilesRealized[new Vector2Int((int)pos.x, (int)pos.y)].gameObject;
                        }
                    }

                }
            }
            //
        }

        //spawnedTile.gameObject.transform.SetParent(GridManager.inst.floorParent.transform); // Set parent
        spawnedTile.transform.parent = mapParent;
    }

    [Header("Auto Mapgen Settings")]
    public List<MapGen_DataCTR> mapGenSpecifics = new List<MapGen_DataCTR>();
    // Global Size
    public int _mapSizeX;
    public int _mapSizeY;
    [Header("Collected Map Data")]
    public Dictionary<Vector2Int, TileBlock> _allTilesRealized = new Dictionary<Vector2Int, TileBlock>();         // ~ This stuff 
    public Dictionary<Vector2Int, GameObject> _layeredObjsRealized = new Dictionary<Vector2Int, GameObject>();    // ~ is important
    //
    public List<GameObject> triggers = new List<GameObject>();
    public List<GameObject> events = new List<GameObject>();

    public List<AudioClip> mapRelatedSounds = new List<AudioClip>();

    public void CreateTileHere(int x, int y, int _tileID)
    {

        var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * GridManager.inst.globalScale, y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedTile.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedTile.name = $"Tile {x} {y} - "; // Give grid based name

        spawnedTile.tileInfo = MapManager.inst.tileDatabase.Tiles[_tileID]; // Assign tile data from database by ID

        spawnedTile.name += spawnedTile.tileInfo.type.ToString(); // Modify name with type

        spawnedTile.tileInfo.currentVis = TileVisibility.Unknown; // All tiles start hidden
        //FogOfWar.inst.unseenTiles.Add(spawnedTile); // Add to unseen tiles

        spawnedTile._highlightPerm.GetComponent<SpriteRenderer>().color = Color.white;

        spawnedTile.locX = x; // Assign X location
        spawnedTile.locY = y; // Assign Y location

        _allTilesRealized[new Vector2Int(x, y)] = spawnedTile; // Add to Dictionary

        spawnedTile.gameObject.transform.SetParent(mapParent.transform); // Set parent
    }

    public void FillWithRock(Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!CheckDictionaryEntry(_allTilesRealized, new Vector2Int(x, y)))
                {
                    CreateBlock(new Vector2Int(x, y), 2);
                }
            }
        }
    }

    public bool CheckDictionaryEntry(Dictionary<Vector2Int, TileBlock> dict, Vector2Int key)
    {
        return dict.ContainsKey(key);
    }

    #endregion

    #region Access - (Exit/Branches)
    public bool firstExitFound = false;
    [Header("Exits")]
    public List<GameObject> placedExits = new List<GameObject>();    // <Main> Access points
    public List<GameObject> placedBranches = new List<GameObject>(); // <Branch> Access points


    public void PlaceBranchNExits()
    {
        if (mapType == 1) // Caves
        {
            // TODO: Make this better later V
            CreateCaveSpawnpoints();
        }
        else if (mapType == 2) // Complex
        {
            List<Vector2Int> exitLocations = new List<Vector2Int>();
            List<Tunnel> halls = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().tunnels;

            // Determine valid locations for exits
            foreach (Tunnel hall in halls)
            {
                if (Vector2Int.Distance(hall.Center, Action.V3_to_V2I(playerSpawnLocation)) > 50f && hall.Width >= 2 && hall.Length >= 2)
                {
                    exitLocations.Add(hall.Center);
                }
            }

            exitLocations = exitLocations.OrderBy(x => Random.value).ToList(); // Shuffle the list

            // Place exits randomly in valid locations
            int numExits = Random.Range(3, 4);
            for (int i = 0; i < numExits && exitLocations.Count > 0; i++)
            {
                int exitIndex = Random.Range(0, exitLocations.Count);
                Vector2Int exitLocation = exitLocations[exitIndex];
                exitLocations.RemoveAt(exitIndex);
                PlaceLevelExit(exitLocation, false, 0);
            }
        }
        else // Custom Maps
        {

        }
    }

    public void PlaceLevelExit(Vector2Int loc, bool isBranch, int targetDestination) // Can be branch or +1 access
    {
        /*
        int type = 6;
        if (isBranch)
        {
            type = 7;
        }
        */

        var spawnedAccess = Instantiate(_accessPrefab, new Vector3(loc.x * GridManager.inst.globalScale, loc.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedAccess.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedAccess.name = $"ACCESS {loc.x} {loc.y} - "; // Give grid based name

        // Add details
        spawnedAccess.GetComponent<AccessObject>().Setup(targetDestination, isBranch);

        spawnedAccess.GetComponent<AccessObject>().isExplored = false;
        spawnedAccess.GetComponent<AccessObject>().isVisible = false;

        //spawnedAccess.GetComponent<AccessObject>().tileInfo = MapManager.inst.tileDatabase.Tiles[type]; // Assign tile data from database by ID
        //spawnedAccess.GetComponent<TileBlock>().tileInfo = MapManager.inst.tileDatabase.Tiles[type]; // Dumb workaround

        //FogOfWar.inst.unseenTiles.Add(spawnedAccess.GetComponent<TileBlock>()); // Add to unseen tiles

        spawnedAccess.GetComponent<AccessObject>().locX = (int)loc.x; // Assign X location
        spawnedAccess.GetComponent<AccessObject>().locY = (int)loc.y; // Assign Y location



        _layeredObjsRealized[new Vector2Int((int)loc.x, (int)loc.y)] = spawnedAccess;

        spawnedAccess.GetComponent<SpriteRenderer>().sortingOrder = 5; // +4 layer so not hidden in floor

        GridManager.inst.grid[(int)loc.x, (int)loc.y] = spawnedAccess; // Fill grid

        spawnedAccess.transform.parent = mapParent;

        if (isBranch)
        {
            placedBranches.Add(spawnedAccess);
        }
        else
        {
            placedExits.Add(spawnedAccess);
        }
    }

    #endregion

    #region Machine Placement

    public void PlaceMachines()
    {
        if (mapType == 1) // Caves [Different spawning method]
        {

        }
        else if (mapType == 2) // Complex [Normal spawning method]
        {
            PlaceInteractableMachines();
            PlaceStaticMachines();
        }
        else // Custom
        {

        }
    }

    [Header("All Machines")]
    public List<GameObject> machines_static = new List<GameObject>();
    public List<GameObject> machines_terminals = new List<GameObject>();
    public List<GameObject> machines_fabricators = new List<GameObject>();
    public List<GameObject> machines_recyclingUnits = new List<GameObject>();
    public List<GameObject> machines_garrisons = new List<GameObject>();
    public List<GameObject> machines_repairStation = new List<GameObject>();
    public List<GameObject> machines_scanalyzers = new List<GameObject>();
    public List<GameObject> machines_customTerminals = new List<GameObject>();

    [Header("Machine Prefabs")]
    public List<GameObject> imp_terminals = new List<GameObject>();
    public List<GameObject> imp_fabricators = new List<GameObject>();
    public List<GameObject> imp_recyclingunits = new List<GameObject>();
    public List<GameObject> imp_garrisons = new List<GameObject>();
    public List<GameObject> imp_repairstations = new List<GameObject>();
    public List<GameObject> imp_scanalyzers = new List<GameObject>();
    public List<GameObject> imp_customTerminals = new List<GameObject>();
    public List<GameObject> staticMachinePrefabs = new List<GameObject>();

    private void PlaceInteractableMachines()
    {
        /*
        int term = 0, scan = 0, recy = 0, fab = 0, garr = 0;

        // We want to spawn in the following machines:
        // - Terminals
        // - Scanalyzers
        // - Recyclers
        // - Fabricators
        // - Garrisons (Layer dependant)

        
        while (term < 12)
        {
            GameObject toPlace = interactableMachinePrefabs_terminals[Random.Range(0, interactableMachinePrefabs_terminals.Count - 1)];
            PlaceMachineInRooms(toPlace, DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms, GetMachineSizeFromName(toPlace));
            term++;
        }
        */
        // The spread of machines depends on the current layer
        List<RoomCTR> rooms = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms;

        int toSpawn = ((_mapSizeX * _mapSizeX) / 100 / 4);

        for (int i = 0; i < toSpawn; i++)
        {
            RoomCTR room = rooms[Random.Range(0, rooms.Count - 1)];
            if (room.machineCount == 0) // No machines already
            {
                Vector2Int roomSize = new Vector2Int(room.Width, room.Length);
                int random = Random.Range(0, imp_terminals.Count - 1);
                Terminal stat = imp_terminals[random].GetComponentInChildren<Terminal>();

                if ((stat._size.x + 2) < roomSize.x && (stat._size.y + 2) < roomSize.y) // Will machine fit? + padding
                {
                    // If so spawn it in the center
                    GameObject machine = Instantiate(imp_terminals[random], new Vector3(room.GetCenter.x, room.GetCenter.y, 0), Quaternion.identity);
                    machine.transform.SetParent(mapParent);
                    machine.transform.position = new Vector3(room.GetCenter.x, room.GetCenter.y, 0f);
                    machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * 0);
                    MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
                    foreach (MachinePart M in machines)
                    {
                        M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                        Vector2Int loc = new Vector2Int((int)M.gameObject.transform.position.x, (int)M.gameObject.transform.position.y);
                        _allTilesRealized[loc].occupied = true;
                        _layeredObjsRealized[loc] = M.gameObject;
                    }

                    room.machineCount += 1;
                    machines_terminals.Add(machine);
                }
                else if ((stat._size.x + 2) < roomSize.y && (stat._size.y + 2) < roomSize.x) // Try rotating it
                {
                    // If so spawn it in the center
                    GameObject machine = Instantiate(imp_terminals[random], new Vector3(room.GetCenter.x, room.GetCenter.y, 0), Quaternion.identity);
                    machine.transform.SetParent(mapParent);
                    machine.transform.position = new Vector3(room.GetCenter.x, room.GetCenter.y, 0f);
                    machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * 1);
                    if (machine.GetComponentInChildren<Canvas>())
                        machine.GetComponentInChildren<Canvas>().transform.rotation = Quaternion.Euler(0f, 0f, 90f * -1); // Don't rotate the canvas
                    MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
                    foreach (MachinePart M in machines)
                    {
                        M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                        Vector2Int loc = new Vector2Int((int)M.gameObject.transform.position.x, (int)M.gameObject.transform.position.y);
                        _allTilesRealized[loc].occupied = true;
                        _layeredObjsRealized[loc] = M.gameObject;
                    }

                    room.machineCount += 1;
                    machines_terminals.Add(machine);
                }
            }
        }
    }

    Vector2Int GetMachineSizeFromName(GameObject machine)
    {
        string[] nameParts = machine.name.Split(' ');
        string sizeString = nameParts[1];
        string[] sizeParts = sizeString.Split('x');
        int width = int.Parse(sizeParts[0]);
        int height = int.Parse(sizeParts[1]);
        return new Vector2Int(width + 2, height + 2);
    }

    private void PlaceStaticMachines()
    {
        // The spread of machines depends on the current layer
        List<RoomCTR> rooms = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms;

        int toSpawn = ((_mapSizeX * _mapSizeX) / 100);

        for (int i = 0; i < toSpawn; i++)
        {
            RoomCTR room = rooms[Random.Range(0, rooms.Count - 1)];
            if (room.machineCount == 0) // No machines already
            {
                Vector2Int roomSize = new Vector2Int(room.Width, room.Length);
                int random = Random.Range(0, staticMachinePrefabs.Count - 1);
                StaticMachine stat = staticMachinePrefabs[random].GetComponentInChildren<StaticMachine>();

                if ((stat._size.x + 2) < roomSize.x && (stat._size.y + 2) < roomSize.y) // Will machine fit? + padding
                {
                    // If so spawn it in the center
                    GameObject machine = Instantiate(staticMachinePrefabs[random], new Vector3(room.GetCenter.x, room.GetCenter.y, 0), Quaternion.identity);
                    machine.transform.SetParent(mapParent);
                    machine.transform.position = new Vector3(room.GetCenter.x, room.GetCenter.y, 0f);
                    machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * 0);
                    MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
                    foreach (MachinePart M in machines)
                    {
                        M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                        Vector2Int loc = new Vector2Int((int)M.gameObject.transform.position.x, (int)M.gameObject.transform.position.y);
                        _allTilesRealized[loc].occupied = true;
                        _layeredObjsRealized[loc] = M.gameObject;
                    }

                    room.machineCount += 1;

                }
                else if ((stat._size.x + 2) < roomSize.y && (stat._size.y + 2) < roomSize.x) // Try rotating it
                {
                    // If so spawn it in the center
                    GameObject machine = Instantiate(staticMachinePrefabs[random], new Vector3(room.GetCenter.x, room.GetCenter.y, 0), Quaternion.identity);
                    machine.transform.SetParent(mapParent);
                    machine.transform.position = new Vector3(room.GetCenter.x, room.GetCenter.y, 0f);
                    machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * 1);
                    if (machine.GetComponentInChildren<Canvas>())
                        machine.GetComponentInChildren<Canvas>().transform.rotation = Quaternion.Euler(0f, 0f, 90f * -1); // Don't rotate the canvas
                    MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
                    foreach (MachinePart M in machines)
                    {
                        M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                        Vector2Int loc = new Vector2Int((int)M.gameObject.transform.position.x, (int)M.gameObject.transform.position.y);
                        _allTilesRealized[loc].occupied = true;
                        _layeredObjsRealized[loc] = M.gameObject;
                    }

                    room.machineCount += 1;
                }
            }
        }
    }

    void PlaceMachineInRooms(GameObject machineToPlace, List<RoomCTR> rooms, Vector2Int machineSize)
    {
        foreach (RoomCTR room in rooms)
        {
            RoomCTR roomCtr = room;

            if (roomCtr.machineCount == 0)
            {
                List<DTile> floorTiles = roomCtr.GetInterior;
                List<DTile> doors = roomCtr.GetDoors;
                bool placed = false;
                foreach (DTile floorTile in floorTiles)
                {
                    for (int rotation = 0; rotation < 4; rotation++)
                    {
                        if (CanPlaceMachineAtTile(floorTile, roomCtr, rotation, machineSize.x, machineSize.y))
                        {
                            bool blocksDoor = false;
                            foreach (DTile door in doors)
                            {
                                Vector2Int doorPos = door.DTilePos;
                                Vector2Int rotatedDoorPos = RotateVector2Int(doorPos - floorTile.DTilePos, -rotation);
                                if (rotatedDoorPos.x >= 0 && rotatedDoorPos.x < machineSize.x && rotatedDoorPos.y >= 0 && rotatedDoorPos.y < machineSize.y)
                                {
                                    // The machine overlaps the door
                                    blocksDoor = true;
                                    break;
                                }
                            }
                            if (!blocksDoor)
                            {
                                GameObject machine = Instantiate(machineToPlace, new Vector3(room.GetCenter.x, room.GetCenter.y, 0), Quaternion.identity);
                                machine.transform.SetParent(mapParent);
                                machine.transform.position = new Vector3(floorTile.DTilePos.x, floorTile.DTilePos.y, 0f);
                                machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * rotation);
                                MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
                                foreach (MachinePart M in machines)
                                {
                                    M.GetComponent<SpriteRenderer>().sortingOrder = 7;
                                    Vector2Int loc = new Vector2Int((int)M.gameObject.transform.position.x, (int)M.gameObject.transform.position.y);
                                    _allTilesRealized[loc].occupied = true;
                                    _layeredObjsRealized[loc] = M.gameObject;
                                }

                                roomCtr.machineCount += 1;

                                placed = true;
                                return;
                                //break;
                            }
                        }
                    }
                    if (placed) break;
                }
            }
        }
    }

    bool CanPlaceMachineAtTile(DTile tile, RoomCTR room, int rotation, int machineWidth, int machineHeight)
    {
        for (int x = 0; x < machineWidth; x++)
        {
            for (int y = 0; y < machineHeight; y++)
            {
                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int tilePos = tile.DTilePos + RotateVector2Int(offset, rotation);
                if (!room.ContainsTile(tilePos)) return false;
                if (room.HasWall(tilePos)) return false;
                //if (room.HasMachine(tilePos)) return false;
            }
        }
        return true;
    }

    Vector2Int RotateVector2Int(Vector2Int v, int rotation)
    {
        switch (rotation)
        {
            case 0: return v;
            case 1: return new Vector2Int(-v.y, v.x);
            case 2: return new Vector2Int(-v.x, -v.y);
            case 3: return new Vector2Int(v.y, -v.x);
            default: return v;
        }
    }
    /// <summary>
    /// Place an individual machine somewhere in the world.
    /// </summary>
    /// <param name="location">Where the machine should be placed.</param>
    /// <param name="type">The type of machine to be placed. [0 = Static, 1 = Terminal, 2 = Fabricator, 3 = Scanalyzer, 4 = Repair Station, 5 = Recycling Unit, 6 = Garrison, 7 = Custom]</param>
    /// <param name="id">The id (list based) of the item to be placed.</param>
    public void PlaceIndividualMachine(Vector2Int location, int type, int id)
    {
        Vector2Int size = Vector2Int.zero;
        Vector2 offset = Vector2.zero;
        GameObject machine = null;

        // Offset if its even in size
        if (size.x % 2 == 0 && size.x > 1)
        {
            offset.x = 0.5f;
        }
        if (size.y % 2 == 0 && size.y > 1)
        {
            offset.y = 0.5f;
        }

        switch (type)
        {
            case 0: // Static
                StaticMachine stat = staticMachinePrefabs[id].GetComponentInChildren<StaticMachine>();
                size = stat._size;
                machine = Instantiate(staticMachinePrefabs[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_static.Add(machine);
                break;
            case 1: // Terminal
                Terminal term = imp_terminals[id].GetComponentInChildren<Terminal>();
                size = term._size;
                machine = Instantiate(imp_terminals[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_terminals.Add(machine);
                break;
            case 2: // Fabricator
                Fabricator fab = imp_fabricators[id].GetComponentInChildren<Fabricator>();
                size = fab._size;
                machine = Instantiate(imp_fabricators[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_fabricators.Add(machine);
                break;
            case 3: // Scanalyzer
                Scanalyzer scan = imp_scanalyzers[id].GetComponentInChildren<Scanalyzer>();
                size = scan._size;
                machine = Instantiate(imp_scanalyzers[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_scanalyzers.Add(machine);
                break;
            case 4: // Repair Station
                RepairStation rep = imp_repairstations[id].GetComponentInChildren<RepairStation>();
                size = rep._size;
                machine = Instantiate(imp_repairstations[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_repairStation.Add(machine);
                break;
            case 5: // Recycling Unit
                RecyclingUnit recy = imp_recyclingunits[id].GetComponentInChildren<RecyclingUnit>();
                size = recy._size;
                machine = Instantiate(imp_recyclingunits[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_recyclingUnits.Add(machine);
                break;
            case 6: // Garrison
                Garrison garr = imp_garrisons[id].GetComponentInChildren<Garrison>();
                size = garr._size;
                machine = Instantiate(imp_garrisons[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_garrisons.Add(machine);
                break;
            case 7: // Custom
                TerminalCustom termC = imp_customTerminals[id].GetComponentInChildren<TerminalCustom>();
                size = termC._size;
                machine = Instantiate(imp_customTerminals[id], new Vector3(location.x + offset.x, location.y + offset.y, 0), Quaternion.identity);
                machines_customTerminals.Add(machine);
                break;
            default:
                Debug.LogError("ERROR: Invalid Machine Selection.");
                break;
        }

        machine.transform.SetParent(mapParent);
        machine.transform.position = new Vector3(location.x + offset.x, location.y + offset.y, 0f);
        machine.transform.rotation = Quaternion.Euler(0f, 0f, 90f * 0);
        MachinePart[] machines = machine.GetComponentsInChildren<MachinePart>();
        foreach (MachinePart M in machines)
        {
            M.GetComponent<SpriteRenderer>().sortingOrder = 7;
            Vector2Int loc = new Vector2Int((int)(M.gameObject.transform.position.x + offset.x), (int)(M.gameObject.transform.position.y + offset.y));
            _allTilesRealized[loc].occupied = true;
            _layeredObjsRealized[loc] = M.gameObject;
            /*
            GameObject debug = Instantiate(debugPrefab, new Vector3(loc.x, loc.y, 0), Quaternion.identity); // Debug vis check
            debug.GetComponent<SpriteRenderer>().sortingOrder = 8;
            */
        }
        /*
         * NOTE:
         * In the machine prefabs, all components my be perfectly aligned on exact numbers. If there are any decimals in the numbers (eg. -1.9999) there
         * is a high chance that the spawning will break and the machine part will be spawned in the incorrect space due to rounding.
         */
    }

    private void AssignMachineNames()
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        if (machines_terminals.Count > 0)
        {
            int amount = 1;
            foreach (var term in machines_terminals)
            {
                // -- Naming -- 
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }
                string letter1 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToUpper();
                string letter2 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();
                string letter3 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();

                term.GetComponentInChildren<Terminal>().systemType = "Terminal v" + letter1 + letter2 + ".0" + Random.Range(1, 9) + letter3;
                term.GetComponentInChildren<Terminal>().fullName = "Terminal " + currentLevelName[0] + fill + amount.ToString();

                amount++;
                // -- Security --
                term.GetComponentInChildren<Terminal>().restrictedAccess = true;
                term.GetComponentInChildren<Terminal>().secLvl = HF.MachineSecLvl();
            }
        }

        if (machines_fabricators.Count > 0)
        {
            int amount = 1;
            foreach (var fab in machines_fabricators)
            {
                // -- Naming --
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }

                string letter1 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToUpper();
                string letter2 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();
                string letter3 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();

                fab.GetComponentInChildren<Fabricator>().systemType = "Fabricator v" + letter1 + letter2 + ".0" + Random.Range(1, 9) + letter3;

                fab.GetComponentInChildren<Fabricator>().fullName = "Fabricator " + currentLevelName[0] + fill + amount.ToString();
                amount++;
                // -- Security --
                fab.GetComponentInChildren<Fabricator>().restrictedAccess = true;
                fab.GetComponentInChildren<Fabricator>().secLvl = HF.MachineSecLvl();
            }
        }

        if (machines_recyclingUnits.Count > 0)
        {
            int amount = 1;
            foreach (var recy in machines_recyclingUnits)
            {
                // -- Naming --
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }

                string letter1 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToUpper();
                string letter2 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();
                string letter3 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();

                recy.GetComponentInChildren<RecyclingUnit>().systemType = "Recycling v" + letter1 + letter2 + ".0" + Random.Range(1, 9) + letter3;

                recy.GetComponentInChildren<RecyclingUnit>().fullName = "Recycling Unit " + currentLevelName[0] + fill + amount.ToString();
                amount++;
                // -- Security --
                recy.GetComponentInChildren<RecyclingUnit>().restrictedAccess = true;
                recy.GetComponentInChildren<RecyclingUnit>().secLvl = HF.MachineSecLvl();
            }
        }

        if (machines_garrisons.Count > 0)
        {
            int amount = 1;
            foreach (var garr in machines_garrisons)
            {
                // -- Naming --
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }

                garr.GetComponentInChildren<Garrison>().systemType = "Garrison Terminal";

                garr.GetComponentInChildren<Garrison>().fullName = "Garrison " + currentLevelName[0] + fill + amount.ToString();
                amount++;
                // -- Security --
                garr.GetComponentInChildren<Garrison>().restrictedAccess = true;
                garr.GetComponentInChildren<Garrison>().secLvl = HF.MachineSecLvl();
            }
        }

        if (machines_repairStation.Count > 0)
        {
            int amount = 1;
            foreach (var reps in machines_repairStation)
            {
                // -- Naming --
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }

                string letter1 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToUpper();
                string letter2 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();
                string letter3 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();

                reps.GetComponentInChildren<RepairStation>().systemType = "Repair Station v" + letter1 + letter2 + ".0" + Random.Range(1, 9) + letter3;

                reps.GetComponentInChildren<RepairStation>().fullName = "Repair Station " + currentLevelName[0] + fill + amount.ToString();
                amount++;
                // -- Security --
                reps.GetComponentInChildren<RepairStation>().restrictedAccess = true;
                reps.GetComponentInChildren<RepairStation>().secLvl = HF.MachineSecLvl();
            }
        }

        if (machines_scanalyzers.Count > 0)
        {
            int amount = 1;
            foreach (var scan in machines_scanalyzers)
            {
                // -- Naming --
                string fill = "";
                if (amount <= 9)
                {
                    fill = "00";
                }
                else if (amount <= 99)
                {
                    fill = "0";
                }

                string letter1 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToUpper();
                string letter2 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();
                string letter3 = alphabet[Random.Range(0, alphabet.Count - 1)].ToString().ToLower();

                scan.GetComponentInChildren<Scanalyzer>().systemType = "Scanalyzer v" + letter1 + letter2 + ".0" + Random.Range(1, 9) + letter3;

                scan.GetComponentInChildren<Scanalyzer>().fullName = "Scanalyzer " + currentLevelName[0] + fill + amount.ToString();
                amount++;
                // -- Security --
                scan.GetComponentInChildren<Scanalyzer>().restrictedAccess = true;
                scan.GetComponentInChildren<Scanalyzer>().secLvl = HF.MachineSecLvl();
            }
        }

        // Custom Terminals get custom names

        // Static Machines don't get name assignments
    }

    private void AssignMachineCommands()
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We do terminals here, all other machines will self load
        if (machines_terminals.Count > 0)
        {
            foreach (var term in machines_terminals)
            {
                int amount = 0;

                // -- We want to generate:
                // - 1-4 Lore entries
                // - 0-2 Access+Alert entries
                // - 0-1 Analysis entries (scaled to level)
                // - 0-2 Enumerate entries
                // - 0-1 Index entries
                // - 0-1 Layout(Zone) entries
                // - 0-1 Recall entries (make sure to check if there is one first)
                // - 0-1 Schematic (item) entries (scaled to level)
                // - 0-1 Schematic (bot) entries (scaled to level)
                // - 0-1 Traps entries

                // Lore
                for (int i = 0; i < Random.Range(1, 4); i++)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);
                    //
                    KnowledgeObject data = knowledgeDatabase.Data[Random.Range(0, knowledgeDatabase.Data.Length)];
                    string displayText = "\"" + data.name + "\"";

                    HackObject hack = hackDatabase.Hack[152];

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Query, "Record", hack, data);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);

                    amount++;
                }

                // Access [73-75] Alert [76-77]
                if (Random.Range(0f, 1f) >= 0.4f)
                {
                    // Access here
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    HackObject hack = hackDatabase.Hack[Random.Range(73, 75)];
                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Access, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    // Alert here
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    HackObject hack = hackDatabase.Hack[Random.Range(76, 77)];
                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Alert, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Analysis [78-87]
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
                    int tier = currentLevel + 11;
                    if(tier <= 0)
                    {
                        tier = 1;
                    }

                    HackObject hack = hackDatabase.Hack[tier + 77];

                    BotObject bot = HF.FindBotOfTier(tier);

                    string displayText = bot.name;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Analysis, "Analysis", hack, null, null, bot);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Enumerate [91-103]
                int rE = Random.Range(91, 103);
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    HackObject hack = hackDatabase.Hack[rE];

                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    int rand = Random.Range(91, 103);
                    if (rand == rE) // Try again
                    {
                        rand = Random.Range(91, 103);
                    }

                    HackObject hack = hackDatabase.Hack[rand];

                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Index [104-110]
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    int rand = Random.Range(104, 110);
                    HackObject hack = hackDatabase.Hack[rand];

                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Index, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Layout (Zone)
                if (Random.Range(0f, 1f) >= 0.6f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    HackObject hack = hackDatabase.Hack[113];

                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Layout, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Recall
                if (GameManager.inst.activeAssaults.Count > 0 || GameManager.inst.activeExterminations.Count > 0 || GameManager.inst.activeInvestigations.Count > 0 || GameManager.inst.activeReinforcements.Count > 0)
                {
                    if ((Random.Range(0f, 1f) >= 0.4f))
                    {
                        string letter = alphabet[0].ToString().ToLower();
                        alphabet.Remove(alphabet[0]);

                        HackObject hack = null;

                        if (GameManager.inst.activeAssaults.Count > 0)
                        {
                            hack = hackDatabase.Hack[116];
                        }
                        else if (GameManager.inst.activeExterminations.Count > 0)
                        {
                            hack = hackDatabase.Hack[117];
                        }
                        else if (GameManager.inst.activeInvestigations.Count > 0)
                        {
                            hack = hackDatabase.Hack[118];
                        }
                        else if (GameManager.inst.activeReinforcements.Count > 0)
                        {
                            hack = hackDatabase.Hack[119];
                        }

                        string displayText = hack.trueName;

                        TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Recall, "", hack, null);

                        term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                    }
                }

                // Schematic (Item) [129-145]
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
                    int tier = currentLevel + 11;
                    if (tier <= 2) // temp fix
                    {
                        tier = 3;
                    }

                    HackObject hack = hackDatabase.Hack[tier + 128 + Random.Range(0, 1)];

                    ItemObject item = HF.FindItemOfTier(tier);

                    string displayText = item.itemName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, null, item);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }

                // Schematic (Bot) [120-128]
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
                    int tier = currentLevel + 11;
                    if (tier <= 0)
                    {
                        tier = 1;
                    }

                    HackObject hack = hackDatabase.Hack[tier + 119];

                    BotObject bot = HF.FindBotOfTier(tier);

                    string displayText = bot.name;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, bot);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }
                // Traps [146-148]
                if (Random.Range(0f, 1f) >= 0.5f)
                {
                    string letter = alphabet[0].ToString().ToLower();
                    alphabet.Remove(alphabet[0]);

                    HackObject hack = hackDatabase.Hack[Random.Range(146, 148)];

                    string displayText = hack.trueName;

                    TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "", hack, null);

                    term.GetComponentInChildren<Terminal>().avaiableCommands.Add(newCommand);
                }
            }
        }

        // Plus assign wall reveal tiles for Custom Terminals
        foreach (GameObject obj in machines_customTerminals)
        {
            if(obj.GetComponentInChildren<TerminalCustom>().type == CustomTerminalType.DoorLock)
            {
                foreach (Vector2Int loc in obj.GetComponentInChildren<TerminalCustom>().wallRevealCoordinates)
                {
                    if (MapManager.inst._allTilesRealized.ContainsKey(loc))
                    {
                        obj.GetComponentInChildren<TerminalCustom>().wallRevealObjs.Add(MapManager.inst._allTilesRealized[loc]);
                    }
                }
            }
        }

        foreach(GameObject obj in machines_fabricators)
        {
            obj.GetComponentInChildren<Fabricator>().Init();
        }

        foreach (GameObject obj in machines_garrisons)
        {
            obj.GetComponentInChildren<Garrison>().Init();
        }

        foreach (GameObject obj in machines_recyclingUnits)
        {
            obj.GetComponentInChildren<RecyclingUnit>().Init();
        }

        foreach (GameObject obj in machines_repairStation)
        {
            obj.GetComponentInChildren<RepairStation>().Init();
        }

        foreach (GameObject obj in machines_scanalyzers)
        {
            obj.GetComponentInChildren<Scanalyzer>().Init();
        }
    }

    /// <summary>
    /// Creates "zones" around terminals. Used for Layout(Zone) command & sometimes derilect knowledge.
    /// </summary>
    private void ZoneTerminals()
    {
        // And now (ouch), go through every placed tile, and assign its position to the nearest terminal.
        foreach (KeyValuePair<Vector2Int, TileBlock> tile in MapManager.inst._allTilesRealized)
        {
            Vector2Int tilePos = tile.Key;
            Terminal nearestTerminal = FindNearestTerminal(tilePos);
            if (nearestTerminal != null)
            {
                nearestTerminal.zone.assignedArea.Add(tilePos);

                // and add mines too
                if(_layeredObjsRealized.ContainsKey(tilePos) && _layeredObjsRealized[tilePos].GetComponent<FloorTrap>())
                {
                    _layeredObjsRealized[tilePos].GetComponent<FloorTrap>().zone = nearestTerminal.zone;
                }
            }
        }
    }

    private Terminal FindNearestTerminal(Vector2Int tilePos)
    {
        Terminal nearestTerminal = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject terminal in machines_terminals)
        {
            Vector2Int terminalPos = HF.V3_to_V2I(terminal.transform.position);
            float distance = Vector2Int.Distance(tilePos, terminalPos);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTerminal = terminal.GetComponentInChildren<Terminal>();
            }
        }

        return nearestTerminal;
    }

    #endregion

    #region Bot Spawning
    private GameObject PlacePlayer()
    {
        var spawnedPlayer = Instantiate(_playerPrefab, new Vector3(playerSpawnLocation.x * GridManager.inst.globalScale, playerSpawnLocation.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedPlayer.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedPlayer.GetComponent<PlayerGridMovement>().playerMovementAllowed = false; // Disable movement for the time being
        playerRef = spawnedPlayer; // Set playerRef in CameraController
        spawnedPlayer.GetComponent<Actor>().ClearFieldOfView();

        return spawnedPlayer;
    }

    private void PlacePassiveBots(Vector2Int mapSize)
    {
        int haulers = 0, serfs = 0, engis = 0, scavs = 0, drills = 0;

        // We want to spawn the following passive bots:
        // - Haulers
        // - Serfs
        // - Engineers
        // - Scanvergers
        // - Drillers
        //
        // Though this all depends on the map type

        if (mapType == 1) // Caves
        {
            int botsToSpawn = ((mapSize.x * mapSize.y) / 100) / 2; // Halved

            // This is caves so we want to prioritize:
            // - Haulers
            // - Drillers
            // - Scavengers

        }
        else if (mapType == 2) // Complex
        {
            int botsToSpawn = ((mapSize.x * mapSize.y) / 100 / 8);
            // This is Complex so we want to prioritize:
            // - Serfs
            // - Engineers
            // - Haulers

            // First get some possible spawn locations
            // For now we will just place bots on the edges of rooms
            List<Vector2Int> possibleSpawnLocations = new List<Vector2Int>();
            List<RoomCTR> rooms = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms;
            foreach (var room in rooms)
            {
                possibleSpawnLocations.Add(room.Edges[Random.Range(0, 7)].position);
            }

            // Don't wanna spawn bots on-top of each-other
            foreach (Vector2Int loc in possibleSpawnLocations.ToList())
            {
                if (initialAISpawnPositions.Contains(loc))
                {
                    possibleSpawnLocations.Remove(loc);
                }
            }

            // Now spawn the bots
            int t = botsToSpawn;
            int minLow = Mathf.FloorToInt(botsToSpawn / 6); // Minimum amount of each to have

            while (t > 0)
            {
                if (possibleSpawnLocations.Count == 0)
                {
                    // Out of locations? Stop
                    break;
                }

                // Pick a bot to spawn
                int ID = Random.Range(0, 4);

                if (haulers < minLow)
                {
                    ID = 0;
                }

                if (serfs < minLow)
                {
                    ID = 1;
                }

                if (engis < minLow)
                {
                    ID = 2;
                }

                if (scavs < minLow - 2) // Lower Prio
                {
                    ID = 3;
                }

                if (drills < minLow - 2) // Lower Prio
                {
                    ID = 4;
                }



                // Spawn the bot
                int random = Random.Range(0, possibleSpawnLocations.Count - 1);
                PlaceBot(possibleSpawnLocations[random], ID);
                initialAISpawnPositions.Add(possibleSpawnLocations[random]);
                possibleSpawnLocations.Remove(possibleSpawnLocations[random]);


                // Decrement
                t--;
                switch (ID)
                {
                    case 0:
                        haulers += 1;
                        break;
                    case 1:
                        serfs += 1;
                        break;
                    case 2:
                        engis += 1;
                        break;
                    case 3:
                        scavs += 1;
                        break;
                    case 4:
                        drills += 1;
                        break;
                }
            }

        }
        else // Custom maps have custom spawning
        {

        }
    }

    private void PlaceHostileBots(Vector2Int mapSize)
    {
        if (mapType == 1) // Caves
        {

        }
        else if (mapType == 2) // Complex
        {
            PlacePatrols();
            PlaceSentries();
        }
        else // Custom maps have custom spawning
        {

        }
    }

    private void PlacePatrols()
    {
        // Pick amount of squads to spawn based on map size
        //int toSpawn = ((_mapSizeX * _mapSizeY) / 100 / 10);
        int toSpawn = 7;
        int squads = 0;

        // We will spawn them in the hallways, so get a list of them
        List<Tunnel> halls = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().tunnels;

        while (squads < toSpawn)
        {
            Tunnel hall = halls[Random.Range(0, halls.Count - 1)];

            if (hall.Width >= 2 && hall.Length >= 2) // Don't wanna spawn bots in cramped hallways (W/L is indexed at 0 for some reason, 3x3+ is goal)
            {
                float random = Random.Range(0f, 1f);
                // Randomly choose which type of squad to spawn, expand on this later!

                if (random >= 0.3f)
                {
                    // Spawn a grunt squad (of 3)
                    List<DTile> edges = hall.Edges;

                    if (edges.Count >= 2)
                    {
                        // Spawn them all next to each-other
                        GameObject squadLead = PlaceBotSquadLead(edges[0].position, 5);
                        if (edges.Count >= 2)
                            squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[1].position, 5));
                        if (edges.Count >= 3)
                            squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[2].position, 5));
                    }
                }
                else
                {
                    // Spawn a swarmer squad (of 4)
                    List<DTile> edges = hall.Edges;

                    if (edges.Count >= 2)
                    {
                        // Spawn them all next to each-other
                        GameObject squadLead = PlaceBotSquadLead(edges[0].position, 8);
                        squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[0].position, 8));

                        if (edges.Count >= 2)
                            squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[1].position, 8));
                        if (edges.Count >= 3)
                            squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[2].position, 8));
                        if (edges.Count >= 4)
                            squadLead.GetComponent<GroupLeader>().AddBotToPatrol(PlaceBot(edges[3].position, 8));
                    }
                }

                squads++;
            }
        }


    }

    private void PlaceSentries()
    {

    }

    public List<GameObject> bots = new List<GameObject>();
    public Actor PlaceBot(Vector2Int pos, int type, GameObject _reference = null)
    {
        var spawnedBot = Instantiate(bots[type], new Vector3(pos.x * GridManager.inst.globalScale, pos.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedBot.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedBot.name = ($"{bots[type].name} @ ({pos.x},{pos.y})"); // Give grid based name
        spawnedBot.GetComponent<Actor>().isVisible = false;
        spawnedBot.GetComponent<Actor>().isExplored = false;

        spawnedBot.GetComponent<Actor>().maxHealth = spawnedBot.GetComponent<Actor>().botInfo.coreIntegrity;
        spawnedBot.GetComponent<Actor>().currentHealth = spawnedBot.GetComponent<Actor>().maxHealth;

        
        spawnedBot.transform.SetParent(botParent, true);
        if(_reference)
            CopyComponentData<BotAI>(_reference, spawnedBot.gameObject);
        

        if(_reference)
            _reference.GetComponent<Actor>().TransferStates(spawnedBot.GetComponent<Actor>()); // Transfer any states

        return spawnedBot.GetComponent<Actor>();
    }

    private GameObject PlaceBotSquadLead(Vector2Int pos, int type)
    {

        var spawnedBot = Instantiate(bots[type], new Vector3(pos.x * GridManager.inst.globalScale, pos.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedBot.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedBot.name = ($"{bots[type].name}* @ ({pos.x},{pos.y})"); // Give grid based name
        spawnedBot.GetComponent<Actor>().isVisible = false;
        spawnedBot.GetComponent<Actor>().isExplored = false;

        spawnedBot.GetComponent<Actor>().maxHealth = spawnedBot.GetComponent<Actor>().botInfo.coreIntegrity;
        spawnedBot.GetComponent<Actor>().currentHealth = spawnedBot.GetComponent<Actor>().maxHealth;

        // Make it a squad leader
        spawnedBot.AddComponent<GroupLeader>();

        spawnedBot.transform.SetParent(botParent, true);

        return spawnedBot;
    }

    #endregion

    #region [Custom Maps] Starting Cave

    // INFO: Looking for where the starting items spawn?
    //       GOTO "Random Item Placement" section.

    public void CustomMap_StartingCave()
    {
        // The starting cave the player spawns in is:
        // An 8x8 open area,
        // connected to another 8x8 area,
        // by a 2x5 shaft. (To the right)

        Vector2Int offset = new Vector2Int(50, 50); // Start in the middle

        // Start with the 8x8 Area
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                CreateBlock(new Vector2(x + offset.x, y + offset.y), 4);
            }
        }

        // While we're at it, set the players spawn location
        Vector2Int temp = offset;
        Vector3 spawnLoc = new Vector3(temp.x += Random.Range(0, 7), temp.y += Random.Range(0, 7));
        playerSpawnLocation = spawnLoc;

        // Adjust the offset
        offset.x += 8;
        offset.y += 3;
        // Then make the connector
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                CreateBlock(new Vector2(x + offset.x, y + offset.y), 4);
            }
        }

        // Adjust the offset
        offset.x += 5;
        offset.y += Random.Range(-5, 0); // Random y offset
        // Then make the second cave
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                CreateBlock(new Vector2(x + offset.x, y + offset.y), 4);
            }
        }

        // Place the exit randomly in the second room (target = Materials)

        Vector2Int exitLoc = new Vector2Int(offset.x += Random.Range(4, 6), offset.y += Random.Range(1, 6));

        PlaceLevelExit(exitLoc, false, 0);

        // Then dirty up the tiles a bit
        foreach (KeyValuePair<Vector2Int, TileBlock> floor in _allTilesRealized)
        {
            float rand = Random.Range(0f, 1f);
            if (rand > 0.4) // 60% chance
            {
                floor.Value.SetToDirty();
            }
        }

    }



    #endregion

    #region > Change Levels <

    public void ChangeMap(int target, bool isBranch, bool keepStats = true)
    {
        loaded = false;
        DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().mapGenComplete = false;

        playerRef.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;
        playerRef.GetComponent<Actor>().FieldofView.Clear();

        // Stop all current audio!
        AudioManager.inst.StopAmbient();
        AudioManager.inst.StopMisc();
        AudioManager.inst.StopMiscSpecific();
        AudioManager.inst.StopMusic();

        // Clear Center + Bottom Messages
        UIManager.inst.ClearAllBottomMessages();
        UIManager.inst.ClearCenterMessages();

        firstExitFound = false;
        PlayerData.inst.linkedTerminalBotnet = 0;
        centerDatabaseLockout = false;

        if (isBranch)
        {
            // Only change the name
        }
        else
        {
            // Change name and increase level
            currentLevel += 1;
            playerRef.GetComponent<PlayerData>().NewLevelRestore();
        }
        currentLevelIsBranch = isBranch;

        // FLAG - UPDATE NEW LEVELS
        switch (target)
        {
            // TODO: In each of these statements (or those relevent) add: DungeonManagerCTR.instance.genData =
            case 0:
                currentLevelName = "MATERIALS";
                levelName = LevelName.Materials;
                mapType = 2;
                break;
            case 1:
                currentLevelName = "LOWER CAVES";
                levelName = LevelName.Lower_Caves;
                mapType = 1;
                break;
            case 2:
                currentLevelName = "STORAGE";
                levelName = LevelName.Storage;
                mapType = 2;
                break;
            case 3:
                currentLevelName = "DSF";
                levelName = LevelName.DSF;
                mapType = 2;
                break;
            case 4:
                currentLevelName = "GARRISON";
                levelName = LevelName.Garrison;
                mapType = 2;
                break;
            case 5:
                currentLevelName = "FACTORY";
                levelName = LevelName.Factory;
                mapType = 2;
                break;
            case 6:
                currentLevelName = "EXTENSION";
                levelName = LevelName.Extension;
                mapType = 2;
                break;
            case 7:
                currentLevelName = "UPPER CAVES";
                levelName = LevelName.Upper_Caves;
                mapType = 1;
                break;
            case 8:
                currentLevelName = "RESEARCH";
                levelName = LevelName.Research;
                mapType = 2;
                break;
            case 9:
                currentLevelName = "ACCESS";
                levelName = LevelName.Access;
                mapType = 2;
                break;
            case 10:
                currentLevelName = "COMMAND";
                levelName = LevelName.Command;
                mapType = 2;
                break;
            case 11:
                currentLevelName = "ARMORY";
                levelName = LevelName.Armory;
                mapType = 2;
                break;
            case 12:
                currentLevelName = "ARCHIVES";
                levelName = LevelName.Archives;
                mapType = 2;
                break;
            case 13:
                currentLevelName = "ZHIROV";
                levelName = LevelName.Zhirov;
                mapType = 2;
                break;
            case 14:
                currentLevelName = "DATA MINER";
                levelName = LevelName.Data_Miner;
                mapType = 2;
                break;
            case 15:
                currentLevelName = "ARCHITECT";
                levelName = LevelName.Architect;
                mapType = 2;
                break;
            case 16:
                currentLevelName = "EXILES";
                levelName = LevelName.Exiles;
                mapType = 3;
                DungeonManagerCTR.instance.genData = mapGenSpecifics[1];
                customMapType = 0;
                break;
            case 17:
                currentLevelName = "WARLORD";
                levelName = LevelName.Warlord;
                mapType = 2;
                break;
            case 18:
                currentLevelName = "SECTION 7";
                levelName = LevelName.Section_7;
                mapType = 2;
                break;
            case 19:
                currentLevelName = "TESTING";
                levelName = LevelName.Testing;
                mapType = 2;
                break;
            case 20:
                currentLevelName = "QUARANTINE";
                levelName = LevelName.Quarantine;
                mapType = 2;
                break;
            case 21:
                currentLevelName = "LAB";
                levelName = LevelName.Lab;
                mapType = 2;
                break;
            case 22:
                currentLevelName = "HUB_04(d)";
                levelName = LevelName.Hub_04d;
                mapType = 2;
                break;
            case 23:
                currentLevelName = "ZION";
                levelName = LevelName.Zion;
                mapType = 2;
                break;
            case 24:
                currentLevelName = "ZDC";
                levelName = LevelName.Zion_Deep_Caves;
                mapType = 2;
                break;
            case 25:
                currentLevelName = "MINES";
                levelName = LevelName.Mines;
                mapType = 2;
                break;
            case 26:
                currentLevelName = "RECYCLING";
                levelName = LevelName.Recycling;
                mapType = 2;
                break;
            case 27:
                currentLevelName = "SUBCAVES";
                levelName = LevelName.Subcaves;
                mapType = 2;
                break;
            case 28:
                currentLevelName = "WASTES";
                levelName = LevelName.Wastes;
                mapType = 2;
                break;
            case 29:
                currentLevelName = "SCRAPTOWN";
                levelName = LevelName.Scraptown;
                mapType = 2;
                break;
            default: // CUSTOM
                currentLevelName = "UNKNOWN";
                levelName = LevelName.Default;
                //
                mapType = 3;
                customMapType = target;
                break;
        }

        // We want to clear out the current map

        // Remove all the bots
        foreach (var bot in TurnManager.inst.actors.ToList())
        {
            if (!bot.GetComponent<PlayerData>()) // Destroy the player last
            {
                Destroy(bot);
            }
        }
        TurnManager.inst.actors.Clear();

        // Remove all things on layer 1
        foreach (var L in _layeredObjsRealized.ToList())
        {
            Destroy(L.Value);
        }
        _layeredObjsRealized.Clear();

        // Remove all things on layer 0
        foreach (var L in _allTilesRealized.ToList())
        {
            Destroy(L.Value.gameObject);
        }
        _allTilesRealized.Clear();
        regions.Clear();

        // Clear triggers
        foreach (GameObject T in triggers.ToList())
        {
            Destroy(T.gameObject);
        }
        triggers.Clear();
        // And events
        foreach (GameObject E in events.ToList())
        {
            Destroy(E.gameObject);
        }
        events.Clear();

        // Remove all world items
        foreach (var item in InventoryControl.inst.worldItems.ToList())
        {
            Destroy(item.Value.gameObject);
        }
        InventoryControl.inst.worldItems.Clear();

        foreach (Transform child in mapParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        GameManager.inst.Entities.Clear();

        // --- Player Related --- //
        if (keepStats)
        {
            GameObject tempPlayerHolder = new GameObject(); // Make a
            tempPlayerHolder.AddComponent<PlayerData>();    // temporary copy
                                                            //playerRef.GetComponent<PlayerData>().SavePlayerInventory(); // Save the inventory
            CopyComponentData<PlayerData>(playerRef, tempPlayerHolder); // Copy over the values
            tempPlayer = tempPlayerHolder;
        }
        else
        {
            tempPlayer = null;
        }

        // - Save the player's data - (expand this later, .json sucks)
        //Debug.Log(">> Saving player data...");
        GameManager.inst.SavePlayerStatus(currentLevel, currentLevelName, mapSeed, TurnManager.inst.globalTime, new Vector2Int(PlayerData.inst.powerSlots, PlayerData.inst.propulsionSlots),
            new Vector2Int(PlayerData.inst.utilitySlots, PlayerData.inst.weaponSlots), PlayerData.inst.robotsKilled);

        playerRef.GetComponentInChildren<CameraController>().SetCamFree(); // Free the camera so it isn't deleted
        Destroy(playerRef); // Destroy the player
        playerRef = null;
        // ---------------------- //

        if (isBranch)
        {
            // Is this a branch? We continue.
        }
        else
        {
            // This isn't a branch? Load up the evolution screen
            GameManager.inst.OpenEvolutionScreen(currentLevel);
            return;
        }


        // And finally, regenerate the level
        StartCoroutine(InitNewLevel());

    }

    public GameObject tempPlayer = null;

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    /// <summary>
    /// Copies data of one component type from a source GameObject to a target GameObject
    /// </summary>
    /// <typeparam name="T">The type of componenet (script) to copy the data of.</typeparam>
    /// <param name="sourceObject">The source GameObject</param>
    /// <param name="targetObject">The target GameObject</param>
    public void CopyComponentData<T>(GameObject sourceObject, GameObject targetObject) where T : Component
    {
        // Get the component from the source GameObject
        T sourceComponent = sourceObject.GetComponent<T>();

        // Check if the source component is not null
        if (sourceComponent != null)
        {
            // Get the component from the target GameObject
            T targetComponent = targetObject.GetComponent<T>();

            // Check if the target component is not null
            if (targetComponent != null)
            {
                // Copy over all fields from the source component to the target component
                System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    field.SetValue(targetComponent, field.GetValue(sourceComponent));
                }
            }
            else
            {
                // Add the component to the target GameObject and copy over all fields from the source component to the target component
                targetComponent = targetObject.AddComponent<T>();
                System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    field.SetValue(targetComponent, field.GetValue(sourceComponent));
                }
            }
        }
    }

    private void LoadFromTempPlayer(GameObject player)
    {
        CopyComponentData<PlayerData>(tempPlayer, player);
        //player.GetComponent<PlayerData>().LoadPlayerInventory();

        Destroy(tempPlayer);
        tempPlayer = null;
    }

    #endregion

    #region Misc

    public void PlaceTrap(ItemObject trapData, Vector2Int location, BotAlignment alignment = BotAlignment.Complex)
    {
        var spawnedMine = Instantiate(minePrefab, new Vector3(location.x * GridManager.inst.globalScale, location.y * GridManager.inst.globalScale), Quaternion.identity); // Instantiate
        spawnedMine.transform.localScale = new Vector3(GridManager.inst.globalScale, GridManager.inst.globalScale, GridManager.inst.globalScale); // Adjust scaling
        spawnedMine.name = $"Floor Trap {location.x} {location.y} - {trapData.deployableItem.trapType}"; // Give grid based name

        spawnedMine.GetComponent<FloorTrap>().Setup(trapData, location, _allTilesRealized[location], alignment);

        _layeredObjsRealized[location] = spawnedMine.gameObject;

        spawnedMine.GetComponentInChildren<SpriteRenderer>().sortingOrder = 12;

        spawnedMine.transform.parent = mapParent;
    }

    public void LocationLog(string location, string goal = "GOAL=ESCAPE")
    {
        string locationMessage = "LOCATION=";
        locationMessage += location;

        UIManager.inst.CreateNewLogMessage(locationMessage, UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
        UIManager.inst.CreateNewLogMessage(goal, UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, true);
    }

    public void CalculatePointsOfInterest()
    {
        if (mapType == 1) // Caves
        {

        }
        else if (mapType == 2) // Complex
        {
            // First check for junctions
            foreach (var room in DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms)
            {
                if (IsJunctionCTR(room))
                {
                    pointsOfInterest.Add(room.Center);
                }
            }

            // Then rooms with (interactive) machines


            // Then exits


            // Then chutes


        }
        else // Custom
        {

        }
    }

    /// <summary>
    /// Tries to determine if a room is a junction (because they aren't automatically categorized).
    /// </summary>
    /// <param name="room">The room to check</param>
    /// <returns></returns>
    private bool IsJunctionCTR(RoomCTR room)
    {
        // Should have the same width/length & smaller than 8 but greater than 2
        return (room.Width == room.Length && room.Width < 8 && room.Width > 2);
    }

    public void PlayAmbientMusic()
    {
        // FLAG - UPDATE NEW LEVELS

        int ambID = 0;

        switch (currentLevelName)
        {
            case "MATERIALS":
                ambID = 15;
                break;
            case "LOWER CAVES":
                ambID = 4;
                break;
            case "STORAGE":
                ambID = 21;
                break;
            case "DSF":
                ambID = 8;
                break;
            case "GARRISON":
                ambID = 12;
                break;
            case "FACTORY":
                ambID = 11;
                break;
            case "EXTENSION":
                ambID = 10;
                break;
            case "UPPER CAVES":
                ambID = 4;
                break;
            case "RESEARCH":
                ambID = 19;
                break;
            case "ACCESS":
                ambID = 1;
                break;
            case "COMMAND":
                ambID = 6;
                break;
            case "ARMORY":
                ambID = 3;
                break;
            case "WASTE":
                ambID = 4;
                break;
            case "HUB":
                ambID = 4;
                break;
            case "ARCHIVES":
                ambID = 2;
                break;
            case "CETUS":
                ambID = 5;
                break;
            case "ARCHITECT":
                ambID = 0;
                break;
            case "ZHIROV":
                ambID = 26;
                break;
            case "DATA MINER":
                ambID = 16;
                break;
            case "EXILES":
                ambID = 9;
                break;
            case "WARLORD":
                ambID = 24;
                break;
            case "SECTION 7":
                ambID = 20;
                break;
            case "TESTING":
                ambID = 23;
                break;
            case "QUARANTINE":
                ambID = 17;
                break;
            case "LAB":
                ambID = 14;
                break;
            case "HUB_04(d)":
                ambID = 13;
                break;
            case "ZION":
                ambID = 27;
                break;
            case "ZDC":
                ambID = 7;
                break;
            case "MINES":
                ambID = 16;
                break;
            case "RECYCLING":
                ambID = 18;
                break;
            case "SUBCAVES":
                ambID = 22;
                break;
            case "WASTES":
                ambID = 25;
                break;
            case "SCRAPTOWN":
                ambID = 4; 
                break;
            default:
                ambID = 4;
                break;
                // EXPAND THIS LATER
        }

        AudioManager.inst.PlayAmbient(ambID, 0.4f);
    }

    /// <summary>
    /// Updates the visuals of all Tiles
    /// </summary>
    public void AllTileVisUpdate()
    {
        foreach (KeyValuePair<Vector2Int, TileBlock> T in _allTilesRealized)
        {
            if (T.Value)
            {
                T.Value.CheckVisibility();
            }
        }
    }

    /// <summary>
    /// Updates the visuals of all Tiles within the specified *radius* variable (in a square) around a specified location.
    /// </summary>
    /// <param name="radius">The radius (in a square) to update the nearby squares specified by the center variable.</param>
    /// <param name="center">The location in the world where the square of tiles to check is.</param>
    public void NearTileVisUpdate(int radius, Vector2Int center)
    {
        Vector2Int corner = new Vector2Int(center.x - radius, center.y - radius); // Bottom right corner

        for (int x = corner.x; x < corner.x + (radius * 2); x++)
        {
            for (int y = corner.y; y < corner.y + (radius * 2); y++)
            {
                if(_allTilesRealized.ContainsKey(new Vector2Int(x, y)))
                {
                    if (_allTilesRealized[new Vector2Int(x, y)])
                    {
                        _allTilesRealized[new Vector2Int(x, y)].CheckVisibility();
                    }
                }
            }
        }
    }

    #endregion
}

public class Region
{
    [Tooltip("The size of this region, ?x?.")]
    public int size { get; set; }
    [Tooltip("The positions of this region among the other regions.")]
    public Vector2Int pos { get; set; }
    [Tooltip("All objects currently in this region.")]
    public List<GameObject> objects { get; set; }

    /// <summary>
    /// Updates vision of all objects stored when called.
    /// </summary>
    public void UpdateVis()
    {
        foreach (var obj in objects)
        {
            if (obj.GetComponent<TileBlock>())
            {
                obj.GetComponent<TileBlock>().CheckVisibility();
            }
        }
    }
}