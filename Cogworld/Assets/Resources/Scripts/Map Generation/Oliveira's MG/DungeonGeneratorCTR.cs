using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
using DungeonResources;
using STType = StructureCTR.STType;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

// Originally made by: Ariel Oliveira [https://github.com/ArielOliveira/DungeonGenerator]
// Modified by: Cody Jackson | cody@krselectric.com

/*
 Original description from Github:
 * A C# implementation of Dr. Peter Henningsen's Dungeon Maker algorithm, which had further contribution from Aaron Dalton. 
    The link to the source forge project http://dungeonmaker.sourceforge.net/.
 * Basically, the algorithm makes use of AI to procedurally generate rooms and connects them with tunnels\corridors. 
    There are 3 basic structures and for each structure a type of builder: Tunnelers build tunnels, 
    wall crawlers build walls (which can turn into mazes) and Roomies build rooms.
 * The builders are like creatures living in a given map and they have a limited life, for every turn they will do their stuff and grow older, 
    when they get to their max age they are eliminated and there is a chance of them 
    spawning babies (except for the roomies which are spawned by tunnelers and will have only one run) in 
    different (or the same) direction and thats one of the main ideas on how this algorithm works. 
    You can see all the information on how it works, including the original source code, at the sourceforge project linked above.
 */

public class DungeonGeneratorCTR : MonoBehaviour {
    private bool _isCustom;

    [HideInInspector] public List<Vector2Int> validSpawnLocations;

    #region (Unused?) Dungeon Gen Settings
    [Header("Dungeon Generation Settings")]
    [SerializeField] private Transform dungeon;
    public static Random random;
    private static DungeonGeneratorCTR instance = null;
    private SquareData[,] map;
    private bool[,] hasLightAt;
    private FlagsDirs[,] MapFlagsDirs;
    private SquareData background;
    private List<Direction> openings;
    private List<RectFill> design; // Pre-placed rooms
    private CrawlerData tunnelCrawlerStats;
    public int sizeX;
    public int sizeY;
    private int activeGeneration;
    private int seedCrawlersInTunnels;   //will be seeded after the completion of the dungeon run if this is > 0
    private int joinDist;   //a wall that will close the gap does so when distance to wall is less than this
    private int minRoomSz , medRoomSz , larRoomSz , maxRoomSz;  //minRS <= smallRoom < medRS <= medRoom < larRS <= larRoom < maxRS
    private int numSmallRoomsL , numMediumRoomsL , numLargeRoomsL;   //maximum wanted numbers of rooms in LABYRINTH
    private int currSmallRoomsL , currMediumRoomsL , currLargeRoomsL; //used for counting, updated by Roomies
    private int numSmallRoomsD , numMediumRoomsD , numLargeRoomsD;   //maximum wanted numbers of rooms in DUNGEON
    private int currSmallRoomsD , currMediumRoomsD , currLargeRoomsD; //used for counting, updated by Roomies
    private List<int> babyDelayProbsForGenerationC;
    private List<int> babyDelayProbsForGenerationT;
    private List<int> babyDelayProbsForGenerationR;
    private List<TripleInt> roomSizeProbSideways;
    private List<TripleInt> roomSizeProbBranching;
    private List<int> maxAgesC;
    private List<int> maxAgesT;
    private List<int> stepLengths;
    private List<int> corrWidths;

    #endregion

    #region Tunnelers
    private List<int> joinPref;
    private List<int> sizeUpProbs, sizeDownProbs;
    private List<int> anteRoomProbs;
    private bool crawlersInTunnels;    //if true, Crawlers can enter Tunnels and build walls in there
    private bool crawlersInAnterooms;    //if true, Crawlers can enter Anterooms and build walls in there
    private bool columnsInTunnels;
    private int patience;
    private int tunnelCrawlerGeneration;   //the generation in which Crawlers are seeded into tunnels;
    private int tunnelCrawlerClosedProb;
    private int tunnelJoinDistance;
    private double roomAspectRatio;
    private TunnelerCTR lastChanceTunneler;
    private int genDelayLastChance;
    private int genSpeedUpOnAnteRooms;
    
    private int sizeUpGenDelay;
    #endregion

    #region Setup & Map Params
    private List<Builder> dungeonBuilders;
    public List<RoomCTR> rooms;
    public List<Tunnel> tunnels;
    public List<AnteRoom> anteRooms;
    private int mutator;
    private int noHeadProb;

    public void InitFromSetup(Setup setup, int seed = 0) {
        if (instance == null)
            instance = this;
        else 
            Debug.Assert(false);
        if (seed == 0)
            random = new Random();
        else
            random = new Random(seed);

        _isCustom = setup.isCustom;
        mapGenComplete = false;

        if (!_isCustom)
        {
            if (!setup.map.IsConsistent)
                Debug.Assert(false);
            if (!setup.builders.IsConsistent)
                Debug.Assert(false);
            if (!setup.roomParameters.IsConsistent())
                Debug.Assert(false);
            if (!setup.crawlerGenes.IsConsistent)
                Debug.Assert(false);
            if (!setup.tunnelerGenes.IsConsistent)
                Debug.Assert(false);
            if (!setup.generalGenes.IsConsistent)
                Debug.Assert(false);
        }

        //Map Initialization
        dungeon = new GameObject("Dungeon").transform;
        transform.position = new Vector3(sizeX/2, 0f, sizeY/2);
        sizeX = setup.map.SizeX;
        sizeY = setup.map.SizeY;
        map = new SquareData[sizeX, sizeY];
        hasLightAt = new bool[sizeX, sizeY];
        MapFlagsDirs = new FlagsDirs[sizeX, sizeY];
        background = setup.map.BackgroundType;
        openings = setup.map.Openings;
        design = setup.map.DesignElements; // Pre-placed rooms
        tunnelJoinDistance = setup.map.TunnelJoinDist;
        joinDist = setup.map.JoinDistance;

        //Structures
        rooms = new List<RoomCTR>();
        tunnels = new List<Tunnel>();
        anteRooms = new List<AnteRoom>();

        //Set border and map based on background
        SetRect(1, 1, sizeX-2, sizeY-2, background);
        SetRect(0, 0, sizeX-1, 0, SquareData.G_CLOSED);
        SetRect(0, 0, 0, sizeY-1, SquareData.G_CLOSED);
        SetRect(sizeX-1, 0, sizeX-1, sizeY-1, SquareData.G_CLOSED);
        SetRect(0, sizeY-1, sizeX-1, sizeY-1, SquareData.G_CLOSED);

        //Empty spawn locations list
        validSpawnLocations = new List<Vector2Int>();

        //Set design elements (pre-placed rooms)
        foreach (RectFill element in setup.map.DesignElements)
        {
            if(element.type == SquareData.VAR) // We need to do special stuff (like overriding the list) for rotation & random placement
            {
                // TODO: Rotation doesn't work properly, fix it!

                // First we need to find out if the prefab should be placed/rotated randomly.
                // To do this we need to search for an "r" after the prefab's size.
                string _name = element.prefab.gameObject.name;
                if (_isCustom) // Simple override, name will be different
                {
                    _name = "test-test)test-test";
                }
                string[] left = _name.Split(")");
                string[] right = left[1].Split("-");
                if (right[0].Contains("r")) // It should be random
                {
                    Vector2Int oldStart = new Vector2Int(element.startX, element.startY);
                    Vector2Int oldEnd = new Vector2Int(element.endX, element.endY);

                    // 1) We need to rotate it randomly
                    float _rotation = 0f;
                    int random = UnityEngine.Random.Range(0, 3);
                    if(random == 0)
                    {
                        _rotation = 0f;
                    }
                    else if(random == 1)
                    {
                        _rotation = 90f;
                    }
                    else if(random == 2)
                    {
                        _rotation = 180f;
                    }
                    else
                    {
                        _rotation = 270f;
                    }

                    Vector2Int newStart = Vector2Int.zero;
                    Vector2Int newEnd = Vector2Int.zero;
                    (newStart, newEnd) = HF.RotateCoordinates(oldStart, oldEnd, _rotation);
                    // 2) We need to place it randomly
                    (newStart, newEnd) = HF.ShiftCoordinates(newStart, newEnd, new Vector2Int(sizeX / 2, sizeY / 2), 8f);

                    // 3) We need to do some jank to override the list
                    RectFill[] designArray = design.ToArray();
                    for (int i = 0; i < designArray.Length; i++)
                    {
                        if (designArray[i].startX == oldStart.x && designArray[i].startY == oldStart.y && designArray[i].endX == oldEnd.x && designArray[i].endY == oldEnd.y) // Check if it matches
                        {
                            // If it does, override its values.
                            designArray[i].startX = newStart.x;
                            designArray[i].startY = newStart.y;
                            designArray[i].endX = newEnd.x;
                            designArray[i].endY = newEnd.y;
                            designArray[i].rotation = _rotation;
                            break; // Finished
                        }
                    }
                    design = designArray.ToList();

                    // 4) Finish up
                    SetRect(element);
                    Debug.Log("Randomized - pos: " + newStart + ", " + newEnd + " r:" + _rotation);
                }
                else // This needs to be placed in its specified position (not random)
                {
                    SetRect(element);
                }
            }
            else
            {
                // Goes to | SetRect -> SetMapData -> map | and sets points directly
                SetRect(element);
            }
        }
           
            

        foreach (Direction opening in setup.map.Openings) {
            switch(opening) {
                case Direction.NO: SetRect(0, sizeY/2 - 1, 2, sizeY/2 + 1, SquareData.G_OPEN);
                    break;
                case Direction.WE: SetRect(sizeX/2 - 1, 0, sizeX/2 + 1, 2, SquareData.G_OPEN);
                    break;
                case Direction.EA: SetRect(sizeX/2 - 1, sizeY - 3, sizeX/2 + 1, sizeY - 1, SquareData.G_OPEN);
                    break;
                case Direction.SO: SetRect(sizeX - 3, sizeY/2 - 1, sizeX - 1, sizeY/2 + 1, SquareData.G_OPEN);
                    break;
                case Direction.NW: SetRect(0, 0, 2, 2, SquareData.G_OPEN);
                    break;
                case Direction.NE: SetRect(0, sizeY - 3, 2, sizeY - 1, SquareData.G_OPEN);
                    break;
                case Direction.SW: SetRect(sizeX - 3, 0, sizeX - 1, 2, SquareData.G_OPEN);
                    break;
                case Direction.SE: SetRect(sizeX - 3, sizeY - 3, sizeX - 1, sizeY - 1, SquareData.G_OPEN);
                    break;
                default: Debug.Assert(false);
                    break;
            }
            
        }

        //End map initialization

        //Miscellaneous
        patience = setup.miscellaneous.Patience;
        mutator = setup.miscellaneous.Mutator;
        noHeadProb = setup.miscellaneous.NoHeadProb;
        sizeUpGenDelay = setup.miscellaneous.SizeUpGenDelay;
        columnsInTunnels = setup.miscellaneous.ColumnsInTunnels;
        roomAspectRatio = setup.miscellaneous.RoomAspectRatio;
        if (roomAspectRatio >= 1 || roomAspectRatio <= 0)
            roomAspectRatio = 0.6f;
        genSpeedUpOnAnteRooms = setup.miscellaneous.GenSpeedUpOnAnteRoom;
        if (genSpeedUpOnAnteRooms <= 0)
            genSpeedUpOnAnteRooms = 1;
        crawlersInTunnels = setup.miscellaneous.ColumnsInTunnels;
        crawlersInTunnels = setup.miscellaneous.CrawlersInAnteRooms;
        seedCrawlersInTunnels = setup.miscellaneous.SeedCrawlersInTunnels;

        //RoomCTR Parameters
        minRoomSz = setup.roomParameters.MinRoomSz;
        medRoomSz = setup.roomParameters.MedRoomSz;
        larRoomSz = setup.roomParameters.LarRoomSz;
        maxRoomSz = setup.roomParameters.MaxRoomSz;

        numSmallRoomsL = setup.roomParameters.NumSmallL;
        numMediumRoomsL = setup.roomParameters.NumMedL;
        numLargeRoomsL = setup.roomParameters.NumLarL;
        
        numSmallRoomsD = setup.roomParameters.NumSmallD;
        numMediumRoomsD = setup.roomParameters.NumMedD;
        numLargeRoomsD = setup.roomParameters.NumLarD;


        //Crawler Genes
        babyDelayProbsForGenerationC = setup.crawlerGenes.BabyGenerationProbsC;
        tunnelCrawlerGeneration = setup.crawlerGenes.TunnelCrawlerGeneration;
        tunnelCrawlerClosedProb = setup.crawlerGenes.TunnelCrawlerClosedProb;
        tunnelCrawlerStats = setup.crawlerGenes.TunnelCrawlerStats;

        //TunnelerCTR Genes
        babyDelayProbsForGenerationT = setup.tunnelerGenes.BabyGenerationProbsT;
        joinPref = setup.tunnelerGenes.JoinPref;
        roomSizeProbSideways = setup.tunnelerGenes.RoomSizeProbS;
        roomSizeProbBranching = setup.tunnelerGenes.RoomSizeProbB;
        sizeUpProbs = setup.tunnelerGenes.SizeUpProb;
        sizeDownProbs = setup.tunnelerGenes.SizeDownProb;
        anteRoomProbs = setup.tunnelerGenes.AnteRoomProb;

        //General Genes
        babyDelayProbsForGenerationR = setup.generalGenes.BabyGenerationProbsR;
        maxAgesC = setup.generalGenes.MaxAgeC;
        maxAgesT = setup.generalGenes.MaxAgeT;
        stepLengths = setup.generalGenes.StepLengths;
        corrWidths = setup.generalGenes.CorridorWidth;
        
        TunnelerData lCT = setup.generalGenes.LastChanceTunneler;
        lastChanceTunneler = new TunnelerCTR(ref instance, Vector2Int.zero, Vector2Int.zero, lCT.age, lCT.maxAge, lCT.generation, Vector2Int.zero, lCT.stepLength, lCT.tunnelWidth, lCT.straightDoubleSpawnProb, lCT.turnDoubleSpawnProb, lCT.changeDirProb, lCT.makeRoomRightProb, lCT.makeRoomLeftProb, lCT.joinPref);
        genDelayLastChance = setup.generalGenes.GenDelayLastChance;

        //Builders
        dungeonBuilders = new List<Builder>();

        foreach (CrawlerData c in setup.builders.Crawlers) 
            CreateCrawler(c.location, Directions.Transform(c.direction), -c.age, c.maxAge, c.generation, Directions.Transform(c.desiredDirection), c.stepLength, c.opening, c.corridorWidth, c.straightSingleSpawnProb, c.straightDoubleSpawnProb, c.turnSingleSpawnProb, c.turnDoubleSpawnProb, c.changeDirectionProb);
        
        foreach (Setup.Pair<CrawlerData, CrawlerData> pair in setup.builders.CrawlerPairs) {
            bool firstIsOpen = true;
            CrawlerData c = pair.First;
            if (random.Next(2) == 0)
                firstIsOpen = false;
            if (firstIsOpen)
                c.opening = 1;
            else
                c.opening = 0;
            CreateCrawler(c.location, Directions.Transform(c.direction), -c.age, c.maxAge, c.generation, Directions.Transform(c.desiredDirection), c.stepLength, c.opening, c.corridorWidth, c.straightSingleSpawnProb, c.straightDoubleSpawnProb, c.turnSingleSpawnProb, c.turnDoubleSpawnProb, c.changeDirectionProb);
            SetMapData(c.location.x, c.location.y, SquareData.CLOSED);
            c = pair.Second;
            if (firstIsOpen)
                c.opening = 0;
            else
                c.opening = 1;
            CreateCrawler(c.location, Directions.Transform(c.direction), -c.age, c.maxAge, c.generation, Directions.Transform(c.desiredDirection), c.stepLength, c.opening, c.corridorWidth, c.straightSingleSpawnProb, c.straightDoubleSpawnProb, c.turnSingleSpawnProb, c.turnDoubleSpawnProb, c.changeDirectionProb);
            SetMapData(c.location.x, c.location.y, SquareData.CLOSED);    
        }

        foreach(TunnelerData t in setup.builders.Tunnelers) {
            CreateTunneler(t.location, Directions.Transform(t.direction), -t.age, t.maxAge, t.generation, Directions.Transform(t.desiredDirection), t.stepLength, t.tunnelWidth, t.straightDoubleSpawnProb, t.turnDoubleSpawnProb, t.changeDirProb, t.makeRoomRightProb, t.makeRoomLeftProb, t.joinPref);
        }

        //Rand Crawlers
        for (int gen = 0; gen < setup.randCrawlerGenes.RandCrawlerPerGen.Count; gen++) {
            int crawlersPer1000Squares = setup.randCrawlerGenes.RandCrawlerPerGen[gen];
            if (crawlersPer1000Squares > 0) {
                int crawlersPerTopBottomWall = (sizeY * crawlersPer1000Squares) / 1000;
                if (crawlersPerTopBottomWall == 0) {
                    if (random.Next(1000) < (sizeY * crawlersPer1000Squares))
                        crawlersPerTopBottomWall = 1;
                }
                int yIndex = 0;
                for (int i = 0; i < crawlersPerTopBottomWall; i++) {
                    yIndex = 2 + random.Next(sizeY - 4);
                    Vector2Int locNorth = new Vector2Int(0, yIndex);
                    Vector2Int fwdNorth = new Vector2Int(1, 0);
                    Vector2Int desiredFwd = fwdNorth;
                    CreateCrawler(locNorth, fwdNorth, 0, GetMaxAgeC(gen), gen, desiredFwd, GetStepLength(gen), 1, GetCorrWidth(gen), Mutate2(setup.randCrawlerGenes.RandC_sSSP), Mutate2(setup.randCrawlerGenes.RandC_sDSP), Mutate2(setup.randCrawlerGenes.RandC_tSSP), Mutate2(setup.randCrawlerGenes.RandC_tDSP), Mutate2(setup.randCrawlerGenes.RandC_cDP));

                    yIndex = 2 + random.Next(sizeY - 4);
                    Vector2Int locSouth = new Vector2Int(sizeX-1, yIndex);
                    Vector2Int fwdSouth = new Vector2Int(-1, 0);
                    desiredFwd = fwdSouth;
                    CreateCrawler(locSouth, fwdSouth, 0, GetMaxAgeC(gen), gen, desiredFwd, GetStepLength(gen), 1, GetCorrWidth(gen), Mutate2(setup.randCrawlerGenes.RandC_sSSP), Mutate2(setup.randCrawlerGenes.RandC_sDSP), Mutate2(setup.randCrawlerGenes.RandC_tSSP), Mutate2(setup.randCrawlerGenes.RandC_tDSP), Mutate2(setup.randCrawlerGenes.RandC_cDP));
                }
                int crawlersPerLeftRightWall = (sizeX * crawlersPer1000Squares) / 1000;
                if (crawlersPerLeftRightWall == 0) {
                    if (random.Next(1000) < (sizeX * crawlersPer1000Squares))
                        crawlersPerLeftRightWall = 1;
                }
                int xIndex = 0;
                for (int i = 0; i < crawlersPerLeftRightWall; i++) {
                    xIndex = 2 + random.Next(sizeX - 4);
                    Vector2Int locWest = new Vector2Int(xIndex, 0);
                    Vector2Int fwdWest = new Vector2Int(0, 1);
                    Vector2Int desiredFwd = fwdWest;
                    CreateCrawler(locWest, fwdWest, 0, GetMaxAgeC(gen), gen, desiredFwd, GetStepLength(gen), 1, GetCorrWidth(gen), Mutate2(setup.randCrawlerGenes.RandC_sSSP), Mutate2(setup.randCrawlerGenes.RandC_sDSP), Mutate2(setup.randCrawlerGenes.RandC_tSSP), Mutate2(setup.randCrawlerGenes.RandC_tDSP), Mutate2(setup.randCrawlerGenes.RandC_cDP));

                    xIndex = 2 + random.Next(sizeX - 4);
                    Vector2Int locEast = new Vector2Int(xIndex, sizeY-1);
                    Vector2Int fwdEast = new Vector2Int(0, -1);
                    desiredFwd = fwdEast;
                    CreateCrawler(locEast, fwdEast, 0, GetMaxAgeC(gen), gen, desiredFwd, GetStepLength(gen), 1, GetCorrWidth(gen), Mutate2(setup.randCrawlerGenes.RandC_sSSP), Mutate2(setup.randCrawlerGenes.RandC_sDSP), Mutate2(setup.randCrawlerGenes.RandC_tSSP), Mutate2(setup.randCrawlerGenes.RandC_tDSP), Mutate2(setup.randCrawlerGenes.RandC_cDP));
                }
            }
        }
    }
    #endregion

    #region Map Generation
    public void SetRect(int startX, int startY, int endX, int endY, SquareData type) {
        if (endX < startX || endY < startY) {
            Debug.Assert(false);
            return;
        } else {
            for (int i = startX; i <= endX; i++) {
                for (int j = startY; j <= endY; j++) {
                    SetMapData(i, j, type);
                }
            }
        }
    }
    
    public void SetRect(RectFill rect) {
        if (rect.endX < rect.startX || rect.endY < rect.startY) {
            Debug.Assert(false);
            return;
        } else {
            if (rect.type == SquareData.VAR) // For placing doors, random positions, and random locations
            {
                // Unsure if this is relevant anymore
                /*
                float _rotation = rect.rotation;
                
                GameObject prefab = rect.prefab;
                prefab.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, _rotation));
                Dictionary<Vector2Int, GameObject> objects = new Dictionary<Vector2Int, GameObject>(); // Dictionary of every object to place
                foreach (Transform child in prefab.transform) // Go through all children and add them to the list of objects
                {
                    Debug.Log(child.gameObject.name);
                    objects.Add(HF.V3_to_V2I(child.transform.position), child.gameObject);
                }

                for (int i = rect.startX; i <= rect.endX; i++)
                {
                    for (int j = rect.startY; j <= rect.endY; j++)
                    {
                        if (objects.ContainsKey(new Vector2Int(i, j)) && objects[new Vector2Int(i, j)].tag == "Door") // Unique check for doors
                        {
                            bool direction = IdentifyHV_Door(objects, new Vector2Int(i, j));
                            if (direction) // H_DOOR
                            {
                                SetMapData(i, j, SquareData.H_DOOR);
                            }
                            else // V_DOOR
                            {
                                SetMapData(i, j, SquareData.V_DOOR);
                            }
                        }
                        else
                        {
                            SetMapData(i, j, rect.type);
                        }
                    }
                }
                */
            }
            else
            {
                for (int i = rect.startX; i <= rect.endX; i++)
                {
                    for (int j = rect.startY; j <= rect.endY; j++)
                    {
                        SetMapData(i, j, rect.type);
                    }
                }
            }
        }

    }

    public bool AdvanceGeneration() {

        bool thereAreBuilders = false;
        int highestNegativeAge = 0;

        foreach (Builder builder in dungeonBuilders) 
        {
            if (builder != null) 
            {
                thereAreBuilders = true;
                if (builder.Generation == activeGeneration) 
                {
                    int a = builder.Age;
                    if (a >= 0)
                        return true;
                    else if (highestNegativeAge == 0 || highestNegativeAge < a)
                        highestNegativeAge = a;
                }
            }
        }

        if (highestNegativeAge == 0) {
            activeGeneration++;
            return thereAreBuilders;
        } 
        else 
        {
            Debug.Assert(highestNegativeAge < 0);
            foreach (Builder builder in dungeonBuilders) 
            {
                if (builder != null) 
                {
                    if (builder.Generation == activeGeneration)
                        builder.AdvanceAge(-highestNegativeAge);
                }
            }
            return thereAreBuilders;
        }
    }

    public bool MakeIteration() {
        bool stillActive = false;
        List<Builder> currGeneration = new List<Builder>();
        foreach (Builder builder in dungeonBuilders)
            currGeneration.Add(builder);

        foreach (Builder builder in currGeneration) {
            if (builder.Generation == ActiveGeneration && builder.Age <= builder.MaxAge)
                stillActive = true;
            if (!builder.StepAhead()) 
                dungeonBuilders.Remove(builder);
        }
    
        currGeneration.Clear();
        return stillActive;
    }

    public void OverrideIteration() {
        List<Builder> currGeneration = new List<Builder>();
        foreach (Builder builder in dungeonBuilders)
            currGeneration.Add(builder);

        foreach (Builder builder in currGeneration)
            if (!builder.StepAhead())
                dungeonBuilders.Remove(builder);

        currGeneration.Clear();
    }

    public void SeedCrawlersInTunnels() {
        int numberFound = 0;
        int tries = 0;
        while ((numberFound < seedCrawlersInTunnels) && (tries < sizeX*sizeY)) {
            tries++;
            int startX = 1 + random.Next(sizeX-4);
            int startY = 1 + random.Next(sizeY-4);
            Vector2Int test = new Vector2Int(startX, startY);

            if (random.Next(100) < 50)
                startX = 0;
            else
                startY = 0;
            if (startX == 0) {
                if (random.Next(100) < 50)
                    startY = -1;
                else 
                    startY = 1;
            } else {
                Debug.Assert(startY == 0);
                if (random.Next(100) < 50)
                    startX = -1;
                else
                    startX = 1;
            }

            Vector2Int dir = new Vector2Int(startX, startY);
            Vector2Int ortho = Vector2Int.zero;

            if (dir.x == 0)
                ortho = new Vector2Int(dir.y, 0);
            else if (dir.y == 0)
                ortho = new Vector2Int(0, -dir.x);
                
            bool notFound = true;
            while (notFound) {
                test = test + dir;
                if (test.x < 2 || test.y < 2 || test.x > sizeX - 3 || test.y > sizeY - 3)
                    break;
                
                if (GetMapData(test) != SquareData.IT_OPEN)
                    continue;
                
                if ((GetMapData(test + dir) != SquareData.IT_OPEN) || (GetMapData(test - dir) != SquareData.IT_OPEN) ||
                    (GetMapData(test + ortho) != SquareData.IT_OPEN) || (GetMapData(test - ortho) != SquareData.IT_OPEN) ||
                    (GetMapData(test + dir + ortho) != SquareData.IT_OPEN) || (GetMapData(test - dir + ortho) != SquareData.IT_OPEN) ||
                    (GetMapData(test + dir - ortho) != SquareData.IT_OPEN) || (GetMapData(test - dir - ortho) != SquareData.IT_OPEN))
                    continue;
                
                SetMapData(test, SquareData.CLOSED);

                CreateCrawler(test, dir, 0, tunnelCrawlerStats.maxAge, activeGeneration+1, dir, tunnelCrawlerStats.stepLength, 1, 1, tunnelCrawlerStats.straightSingleSpawnProb, tunnelCrawlerStats.straightDoubleSpawnProb, tunnelCrawlerStats.turnSingleSpawnProb, tunnelCrawlerStats.turnDoubleSpawnProb, tunnelCrawlerStats.changeDirectionProb);
                CreateCrawler(test, ortho, 0, tunnelCrawlerStats.maxAge, activeGeneration+1, dir, tunnelCrawlerStats.stepLength, 1, 1, tunnelCrawlerStats.straightSingleSpawnProb, tunnelCrawlerStats.straightDoubleSpawnProb, tunnelCrawlerStats.turnSingleSpawnProb, tunnelCrawlerStats.turnDoubleSpawnProb, tunnelCrawlerStats.changeDirectionProb);
                CreateCrawler(test, -ortho, 0, tunnelCrawlerStats.maxAge, activeGeneration+1, dir, tunnelCrawlerStats.stepLength, 1, 1, tunnelCrawlerStats.straightSingleSpawnProb, tunnelCrawlerStats.straightDoubleSpawnProb, tunnelCrawlerStats.turnSingleSpawnProb, tunnelCrawlerStats.turnDoubleSpawnProb, tunnelCrawlerStats.changeDirectionProb);

                if (random.Next(100) < tunnelCrawlerClosedProb)
                    CreateCrawler(test, -dir, 0, tunnelCrawlerStats.maxAge, activeGeneration+1, dir, tunnelCrawlerStats.stepLength, 0, 1, tunnelCrawlerStats.straightSingleSpawnProb, tunnelCrawlerStats.straightDoubleSpawnProb, tunnelCrawlerStats.turnSingleSpawnProb, tunnelCrawlerStats.turnDoubleSpawnProb, tunnelCrawlerStats.changeDirectionProb);
                else
                    CreateCrawler(test, ortho, 0, tunnelCrawlerStats.maxAge, activeGeneration+1, dir, tunnelCrawlerStats.stepLength, 1, 1, tunnelCrawlerStats.straightSingleSpawnProb, tunnelCrawlerStats.straightDoubleSpawnProb, tunnelCrawlerStats.turnSingleSpawnProb, tunnelCrawlerStats.turnDoubleSpawnProb, tunnelCrawlerStats.changeDirectionProb);
                
                notFound = false;
                numberFound++;
            }
        }
    }

    public bool CreateRoom(RectFill room) {
        if (sizeX < 10 || sizeY < 10)
            return false;
        if (room.endX - room.startX <= 5)
            return false;
        if (room.endY - room.startY <= 5)
            return false;
        
        int startX = room.startX + 1 + random.Next(room.endX - room.startX - 3);
        int startY = room.startY + 1 + random.Next(room.endY - room.startY - 3);
        Vector2Int start = new Vector2Int(startX, startY);

        if (!IsOpen(GetMapData(start)))
            return false;
        if (IsChecked(start))
            return false;
        
        int maxRS = maxRoomSz;
        if (!WantsMoreRoomsL(RoomSize.LARGE))
            maxRS = larRoomSz;
        if (!WantsMoreRoomsL(RoomSize.LARGE) && !WantsMoreRoomsL(RoomSize.MEDIUM))
            maxRS = medRoomSz;
        if (!WantsMoreRoomsL())
            return false;

        Vector2Int NO = new Vector2Int(-1, 0);
        Vector2Int SO = new Vector2Int(1, 0);
        Vector2Int WE = new Vector2Int(0, -1);
        Vector2Int EA = new Vector2Int(0, 1);
        Vector2Int NE = new Vector2Int(-1, 1);
        Vector2Int SE = new Vector2Int(1, 1);
        Vector2Int SW = new Vector2Int(1, -1);
        Vector2Int NW = new Vector2Int(-1, -1);

        List<Vector2Int> dirs = new List<Vector2Int>();
        dirs.Add(NO);
        dirs.Add(SO);
        dirs.Add(WE);
        dirs.Add(EA);
        dirs.Add(NE);
        dirs.Add(SE);
        dirs.Add(SW);
        dirs.Add(NW);

        List<Vector2Int> absDir = new List<Vector2Int>();
        absDir.Add(NO);
        absDir.Add(SO);
        absDir.Add(WE);
        absDir.Add(EA);

        List<Vector2Int> interDir = new List<Vector2Int>();
        interDir.Add(NE);
        interDir.Add(SE);
        interDir.Add(SW);
        interDir.Add(NW);


        bool stillFindingMultiples = true;
        List<Vector2Int> roomSquaresChecked = new List<Vector2Int>();
        List<Vector2Int> roomSquaresActive = new List<Vector2Int>();
        List<Vector2Int> activeFoundThisTurn = new List<Vector2Int>();
        roomSquaresActive.Add(start);

        int numberFound = 0;
        Vector2Int current = Vector2Int.zero;
        while (stillFindingMultiples) {
            stillFindingMultiples = false;
            for (int i = 0; i < roomSquaresActive.Count;) {
                current = roomSquaresActive[i];
                numberFound = 0;

                foreach (Vector2Int direction in dirs)
                    if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn))
                        numberFound++;
                
                if (numberFound > 2) {
                    stillFindingMultiples = true;
                    foreach (Vector2Int direction in dirs)
                        if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn))
                            activeFoundThisTurn.Add(current + direction);
                    
                    if (!IsChecked(current, roomSquaresChecked)) {
                        roomSquaresChecked.Add(current);
                        SetChecked(current);
                    }

                    roomSquaresActive.RemoveAt(i++);

                }   else if (numberFound == 2) {
                    int found = 0;
                    foreach (Vector2Int direction in absDir) {
                        if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn)) {
                            activeFoundThisTurn.Add(current + direction);
                            found++;
                        }
                    }

                    if (found == 1) {
                        i++;
                        continue;
                    }
                    
                    foreach (Vector2Int direction in interDir) {
                        if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn))
                            activeFoundThisTurn.Add(current + direction);
                    }
                        
                        if (!IsChecked(current, roomSquaresChecked)) {
                            roomSquaresChecked.Add(current);
                            SetChecked(current);
                        }
                    roomSquaresActive.RemoveAt(i++);
                } else if (numberFound == 1) {
                    i++;
                } else {
                    Debug.Assert(numberFound == 0);
                    if (!IsChecked(current, roomSquaresChecked)) {
                        roomSquaresChecked.Add(current);
                        SetChecked(current);
                    }
                    roomSquaresActive.RemoveAt(i++);
                }
                if (roomSquaresChecked.Count > maxRS)
                    return false;
            }

            foreach (Vector2Int activeFound in activeFoundThisTurn) {
                if (GetMapData(activeFound) == SquareData.G_OPEN || GetMapData(activeFound) == SquareData.NJ_G_OPEN)
                    return false;
                if (!IsChecked(activeFound, roomSquaresChecked) && !IsActive(activeFound, roomSquaresActive))
                    roomSquaresActive.Add(activeFound);
            }
            activeFoundThisTurn.Clear();
        }  

        bool proceeding = true;
        int squaresFindingMultiples = 0;

        while (proceeding) {
            squaresFindingMultiples = 0;
            proceeding = false;
            for (int i = 0; i < roomSquaresActive.Count;) {
                current = roomSquaresActive[i];
                numberFound = 0;
                foreach (Vector2Int direction in dirs) {
                    if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn))
                        numberFound++;
                }

                if (numberFound > 1) {
                    squaresFindingMultiples++;
                    i++;
                } else if (numberFound == 1) {
                    proceeding = true;
                    foreach (Vector2Int direction in dirs) {
                        if (IsOpen(GetMapData(current + direction)) && !IsChecked(current + direction, roomSquaresChecked) && !IsActive(current + direction, roomSquaresActive) && !IsActive(current + direction, activeFoundThisTurn))
                            activeFoundThisTurn.Add(current + direction);
                    }
                    if (!IsChecked(current, roomSquaresChecked)) {
                        roomSquaresChecked.Add(current);
                        SetChecked(current);
                    }
                    roomSquaresActive.RemoveAt(i++);
                } else {
                    Debug.Assert(numberFound == 0);
                    if (!IsChecked(current, roomSquaresChecked)) {
                        roomSquaresChecked.Add(current);
                        SetChecked(current);
                    }
                    roomSquaresActive.RemoveAt(i++);
                }
            }
            foreach (Vector2Int activeFound in activeFoundThisTurn) {
                if (GetMapData(activeFound) == SquareData.G_OPEN || GetMapData(activeFound) == SquareData.NJ_G_OPEN)
                    return false;
                if (!IsChecked(activeFound, roomSquaresChecked) && !IsActive(activeFound, roomSquaresActive))
                    roomSquaresActive.Add(activeFound);
            }
            activeFoundThisTurn.Clear();
        }

        if (squaresFindingMultiples > 1) 
            return false;
        else if (squaresFindingMultiples == 0) {
            Debug.Assert(roomSquaresChecked.Count > 0);
            foreach (Vector2Int squareChecked in roomSquaresChecked) {
                Debug.Assert(GetMapData(squareChecked) == SquareData.OPEN || GetMapData(squareChecked) == SquareData.NJ_OPEN || GetMapData(squareChecked) == SquareData.IT_OPEN || GetMapData(squareChecked) == SquareData.IA_OPEN);
                SetMapData(squareChecked, SquareData.CLOSED);
            }
        } else {
            Debug.Assert(squaresFindingMultiples == 1);
            if (roomSquaresChecked.Count < minRoomSz)
                return false;
            
            bool diffX = false;
            bool diffY = false;
            int sX = roomSquaresChecked[0].x;
            int sY = roomSquaresChecked[0].y;

            foreach (Vector2Int squareChecked in roomSquaresChecked) {
                if (squareChecked.x != sX)
                    diffX = true;
                if (squareChecked.y != sY)
                    diffY = true;
            }

            if (!diffX || !diffY)
                return false;
            
            if ((GetMapData(current + WE) == SquareData.V_DOOR) || (GetMapData(current + EA) == SquareData.V_DOOR) || (GetMapData(current + WE) == SquareData.H_DOOR) || (GetMapData(current + EA) == SquareData.H_DOOR) ||
                (GetMapData(current + NO) == SquareData.V_DOOR) || (GetMapData(current + SO) == SquareData.V_DOOR) || (GetMapData(current + NO) == SquareData.H_DOOR) || (GetMapData(current + SO) == SquareData.H_DOOR))
                return false;

            if (roomSquaresChecked.Count < medRoomSz) {
                if (!WantsMoreRoomsL(RoomSize.SMALL))
                    return false;
                else
                    currSmallRoomsL++;
            } else if (roomSquaresChecked.Count < larRoomSz) {
                if (!WantsMoreRoomsL(RoomSize.MEDIUM))
                    return false;
                else
                    currMediumRoomsL++;
            } else if (roomSquaresChecked.Count < maxRoomSz) {
                if (!WantsMoreRoomsL(RoomSize.LARGE))
                    return false;
                else
                    currLargeRoomsL++;
            } else
                return false;

            Debug.Assert(roomSquaresActive.Count == 1);
            current = roomSquaresActive[0];
            if (IsOpen(GetMapData(current + NO))) {
                Debug.Assert(IsOpen(GetMapData(current + SO)));
                SetMapData(current, SquareData.H_DOOR);
            } else if (IsOpen(GetMapData(current + WE))) {
                Debug.Assert(IsOpen(GetMapData(current + EA)));
                SetMapData(current, SquareData.V_DOOR);
            }

            RoomCTR newRoom = new RoomCTR();

            foreach (Vector2Int squareChecked in roomSquaresChecked) {
                Debug.Assert(GetMapData(squareChecked) == SquareData.OPEN || GetMapData(squareChecked) == SquareData.NJ_OPEN || GetMapData(squareChecked) == SquareData.IT_OPEN || GetMapData(squareChecked) == SquareData.IA_OPEN);
                SetMapData(squareChecked, SquareData.IR_OPEN);
                newRoom.AddTile(squareChecked, Vector2Int.zero, StructureCTR.STType.FLOOR);
            }

            newRoom.InDungeon = false;
            rooms.Add(newRoom);        
        }
        return true;
    }

    public void SetChecked(Vector2Int pos) {Debug.Assert((pos.x < sizeX) && (pos.y < sizeY) && (pos.x >= 0) && (pos.y >= 0));  MapFlagsDirs[pos.x, pos.y]._checked = true;}
    public void SetUnchecked(Vector2Int pos) {Debug.Assert((pos.x < sizeX) && (pos.y < sizeY) && (pos.x >= 0) && (pos.y >= 0));  MapFlagsDirs[pos.x, pos.y]._checked = false;}
    

    public SquareData GetMapData(int x, int y) {
        Debug.Assert((x >= 0)  && (y >= 0) && (x < sizeX) && (y < sizeY));
        return map[x, y];
    }

    public SquareData GetMapData(Vector2Int pos) {
        Debug.Assert((pos.x >= 0)  && (pos.y >= 0) && (pos.x < sizeX) && (pos.y < sizeY));
        return map[pos.x, pos.y];
    }

    public void SetMapData(int x, int y, SquareData data) {
        Debug.Assert((x >= 0 && y >= 0) && (x < sizeX && y < sizeY));
        map[x, y] = data;
    }

    public void SetMapData(Vector2Int pos, SquareData data) {
        Debug.Assert((pos != Vector2Int.zero) && (pos.x < sizeX && pos.y < sizeY));
        map[pos.x, pos.y] = data;
    }

    public void SetLastChanceTunneler(TunnelerCTR tunneler) {
        this.lastChanceTunneler = tunneler;
         int diceRoll = DungeonGeneratorCTR.random.Next(101);
        int gen = tunneler.Generation;
        int summedProbs = 0;
        for (int ind = 0; ind <= 10; ind++) {
            summedProbs = summedProbs + GetBabyDelayProbsForGenerationT(ind);
            if (diceRoll < summedProbs) {
                gen = gen + ind;
                break;
            }
        }
        this.genDelayLastChance = gen;
    }

    public int LastChanceRoomsRightProb {get => lastChanceTunneler.MakeRoomsRightProb;}
    public int LastChanceRoomsLeftProb {get => lastChanceTunneler.MakeRoomsLeftProb;}
    public int LastChanceChangeDirProb {get => lastChanceTunneler.ChangeDirProb;}
    public int LastChanceStraightSpawnProb {get => lastChanceTunneler.StraightDoubleSpawnProb;}
    public int LastChanceTurnSpawnProb {get => lastChanceTunneler.TurnDoubleSpawnProb;}
    public int LastChanceGenDelay {get => genDelayLastChance;}
    public int TunnelCrawlerGeneration {get => tunnelCrawlerGeneration;}
    public int Patiente {get => patience;}
    public int SizeUpGenDelay {get => sizeUpGenDelay;}
    public int GenSpeedUpOnAnteRoom {get => genSpeedUpOnAnteRooms;}
    public int NoHeadProb {get => noHeadProb;}
    public int ActiveGeneration {get => activeGeneration;}
    public int JoinDistance {get => joinDist;}
    public int TunnelJoinDistance {get => tunnelJoinDistance;}
    public int SizeX {get => sizeX;}
    public int SizeY {get => sizeY;}
    public double RoomAspectRatio {get => roomAspectRatio;}
    public bool CrawlersInTunnels {get => crawlersInTunnels;}
    public bool CrawlersInAnterooms {get => crawlersInAnterooms;}
    public bool ColumnsInTunnels {get => columnsInTunnels;}
    public List<RectFill> Design {get => design;}
    public SquareData Background {get => background;}

    public int GetBabyDelayProbsForGenerationC(int gen) {if((gen >= 0) && (gen <= 10)) return babyDelayProbsForGenerationC[gen]; else return 0;}
    public int GetBabyDelayProbsForGenerationT(int gen) {if((gen >= 0) && (gen <= 10)) return babyDelayProbsForGenerationT[gen]; else return 0;}
    public int GetBabyDelayProbsForGenerationR(int gen) {if((gen >= 0) && (gen <= 10)) return babyDelayProbsForGenerationR[gen]; else return 0;}

    public int GetAnteRoomProb(int tunnelWidth) {if (tunnelWidth >= anteRoomProbs.Count) return 100; else return anteRoomProbs[tunnelWidth];}
    public int GetJoinPref(int gen) {if (gen >= joinPref.Count) return joinPref[joinPref.Count-1]; else return joinPref[gen];}
    public int GetSizeUpProb(int gen) {if (gen >= sizeUpProbs.Count) return sizeUpProbs[sizeUpProbs.Count-1]; else return sizeUpProbs[gen];}
    public int GetSizeDownProb(int gen) {if (gen >= sizeDownProbs.Count) return sizeDownProbs[sizeDownProbs.Count-1]; else return sizeDownProbs[gen];}
    public int GetStepLength(int gen) {if(gen >= stepLengths.Count) return stepLengths[stepLengths.Count-1]; else return stepLengths[gen];}
    public int GetCorrWidth(int gen) {if(gen >= corrWidths.Count) return corrWidths[corrWidths.Count-1]; else return corrWidths[gen];}
    public int GetMaxAgeC(int gen) {if (gen >= maxAgesC.Count) return maxAgesC[maxAgesC.Count-1]; else return maxAgesC[gen];}
    public int GetMaxAgeT(int gen) {if (gen >= maxAgesT.Count) return maxAgesT[maxAgesT.Count-1]; else return maxAgesT[gen];}
    public void CreateCrawler(Vector2Int _location, Vector2Int _forward, int _age, int _maxAge, int _generation,
                        Vector2Int desiredDir, int sL, int op, int cW, int sSS, int sDS, int tSS, int tDS, int cDP) {
        dungeonBuilders.Add(new WallCrawler(ref instance, _location, _forward, _age, _maxAge, _generation, desiredDir, sL, op, cW, sSS, sDS, tSS, tDS, cDP));
    }

    public void CreateTunneler(Vector2Int _location, Vector2Int _forward, int _age, int _maxAge, int _generation,
                        Vector2Int desiredDir, int sL, int tW, int sDSP, int tDSP, int cDP, int mRRP, int mRLP, int jP) {
        dungeonBuilders.Add(new TunnelerCTR(ref instance, _location, _forward, _age, _maxAge, _generation, desiredDir, sL, tW, sDSP, tDSP, cDP, mRRP, mRLP, jP));
    }

    public void CreateRoomie(Vector2Int _location, Vector2Int _forward, int _age, int _maxAge, int _generation,
        int dW, RoomSize size, int cat) {
        dungeonBuilders.Add(new Roomie(ref instance, _location, _forward, _age, _maxAge, _generation, dW, size, cat));        
    }

    public bool HasTunnelerAt(Vector2Int location, Vector2Int facing) {
        TunnelerCTR tunneler = dungeonBuilders.Find(x => x.Location == location) as TunnelerCTR;
        if (tunneler != null)
            return true;
        return false;
    }

    public bool WantsMoreRoomsL(RoomSize sz) {if (sz == RoomSize.SMALL) return (numSmallRoomsL > currSmallRoomsL); else if (sz == RoomSize.MEDIUM) return (numMediumRoomsL > currMediumRoomsL); else if (sz == RoomSize.LARGE) return (numLargeRoomsL > currLargeRoomsL); else Debug.Assert(false); return false;}
    public bool WantsMoreRoomsL() {return (WantsMoreRoomsL(RoomSize.SMALL) || WantsMoreRoomsL(RoomSize.MEDIUM) || WantsMoreRoomsL(RoomSize.LARGE));}
    public bool WantsMoreRoomsD(RoomSize sz) {if (sz == RoomSize.SMALL) return (numSmallRoomsD > currSmallRoomsD); else if (sz == RoomSize.MEDIUM) return (numMediumRoomsD > currMediumRoomsD); else if (sz == RoomSize.LARGE) return (numLargeRoomsD > currLargeRoomsD); else Debug.Assert(false); return false;}
    public bool WantsMoreRoomsD() {return (WantsMoreRoomsD(RoomSize.SMALL) || WantsMoreRoomsD(RoomSize.MEDIUM) || WantsMoreRoomsD(RoomSize.LARGE));}
    public void BuiltRoomD(RoomSize sz) {if(RoomSize.SMALL == sz) currSmallRoomsD++; else if(RoomSize.MEDIUM == sz) currMediumRoomsD++; else if(RoomSize.LARGE == sz) currLargeRoomsD++;}
    #endregion

    [Header("Editor Special")]
    public bool doLocalRotation = false;
    public GameObject dungeonParent;

    #region Map Realization
    [Header("Key Map Elements")]
    public Dictionary<Vector2Int, string> placedTiles = new Dictionary<Vector2Int, string>();
    [Tooltip("List containing scripted elements like triggers, and events that cannot be simplified to a string.")]
    [HideInInspector] public List<GameObject> preInitElements = new List<GameObject>();
    [Tooltip("List containing things like bots that have detail which cannot be stored in a string.")]
    [HideInInspector] public List<GameObject> preInitObjects = new List<GameObject>();
    public void PlaceStructure<T>(List<T> structures, string name) where T : StructureCTR {

        /*  === EXPLANATION ===
         * The whole idea here is that we are generating a 'theoretical' map of data and specifics
         * which we will then pass to MapManager which actually realizes the map.
         * 
         * Here we will consider each tile/object to be a string so serialization is easier later.
         * It's data will be separated by commas so its easier to parse.
         * It will take the following form:
         *     [position.x],[position.y],[TILE.TYPE],[DIRTY/CLEAN],[(optional)FULLSPRITENAME],[(optional)MISCINFO]
         * EX: 22,53,FLOOR,DIRTY,floor_cave
         */

        foreach (T s in structures) {
            Transform structure = new GameObject(name).transform;
            structure.transform.position = new Vector3(s.Center.x, 0f, s.Center.y);
            structure.transform.parent = dungeon;
            dungeonParent = dungeon.gameObject;
            Quaternion rotation = Quaternion.identity;

            foreach (StructureCTR.DTile square in s.GetTiles(STType.FLOOR)) 
            {
                string floor = "";

                // Position
                floor += square.position.x + "," + square.position.y + ",";

                // Type
                floor += "TileType.Floor,";

                // Dirty
                if (!placedTiles.ContainsKey(new Vector2Int(square.position.x, square.position.y)))
                    floor += "CLEAN";

                placedTiles.Add(new Vector2Int(square.position.x, square.position.y), floor);
            }

            foreach (StructureCTR.DTile square in s.GetTiles(STType.EDGE))
            {
                string floor = "";

                // Position
                floor += square.position.x + "," + square.position.y + ",";

                // Type
                floor += "TileType.Floor,";

                // Dirty
                if (!placedTiles.ContainsKey(new Vector2Int(square.position.x, square.position.y)))
                    floor += "CLEAN";

                placedTiles.Add(new Vector2Int(square.position.x, square.position.y), floor);
            }

            foreach (StructureCTR.DTile square in s.GetTiles(STType.CORNER)) 
            {
                string floor = "";

                // Position
                floor += square.position.x + "," + square.position.y + ",";

                // Type
                floor += "TileType.Floor,";

                // Dirty
                if (!placedTiles.ContainsKey(new Vector2Int(square.position.x, square.position.y)))
                    floor += "CLEAN";

                placedTiles.Add(new Vector2Int(square.position.x, square.position.y), floor);
            }

            foreach (StructureCTR.DTile square in s.GetTiles(STType.DOOR)) 
            {
                string door = "";

                // Position
                door += square.position.x + "," + square.position.y + ",";

                // Type
                door += "TileType.Door,";

                // Dirty
                door += "False";

                placedTiles.Add(new Vector2Int(square.position.x, square.position.y), door);
            }

            // Although currently not used, this info may be useful later as positional details for specific important objects.
            /*
            foreach (StructureCTR.DTile square in s.GetTiles(STType.COLUMN)) {
                if(doLocalRotation)
                    rotation = Quaternion.LookRotation(new Vector3(square.facing.x, 0f, square.facing.y));
                GameObject columnObj = Instantiate(column, new Vector3(square.position.x* scaling4 + (square.facing.x* scaling23), square.position.y* scaling4 + (square.facing.y* scaling23), 0f), Quaternion.identity);
                columnObj.transform.parent = structure;
            }
            */

            /*
            foreach (StructureCTR.DTile square in s.GetTiles(STType.LIGHT)) {
                float offset = 0f;
                if (GetMapData(square.position) == SquareData.H_DOOR || GetMapData(square.position) == SquareData.V_DOOR)
                    offset = -2f;
                else if (GetMapData(square.position) == SquareData.COLUMN)
                    offset = -1.88f;

                if (doLocalRotation)
                    rotation = Quaternion.LookRotation(new Vector3(square.facing.x, square.facing.y, 0f));
                GameObject value;
                if (!placedLights.TryGetValue(new Vector3(square.position.x * scaling4 + (square.facing.x * offset), square.position.y * scaling4 + (square.facing.y * offset)), out value))
                {
                    GameObject light = Instantiate(lightObj, new Vector3(square.position.x * scaling4 + (square.facing.x * offset), square.position.y * scaling4 + (square.facing.y * offset), 0f), rotation);
                    light.transform.parent = structure;
                    //Debug.Log("New Light placed at: " + new Vector3(square.position.x * scaling4 + (square.facing.x * offset), square.position.y * scaling4 + (square.facing.y * offset), 0f));
                    placedLights.Add(new Vector3(square.position.x * scaling4 + (square.facing.x * offset), square.position.y * scaling4 + (square.facing.y * offset), 0f), light);
                }
            }
            */
        }

        if (!called)
            StartCoroutine(InstWallsWait(placedTiles));
    }

    IEnumerator InstWallsWait(Dictionary<Vector2Int, string> placedTiles)
    {
        called = true;

        yield return null;

        #region Instantiate Walls
        /*
         * This function will generate walls around all floor/door tiles,
         * however it will not do corners.
         */

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (KeyValuePair<Vector2Int, string> tile in placedTiles.ToList())
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = tile.Key + direction;
                if (!placedTiles.ContainsKey(neighborPos))
                {
                    string wall = "";

                    // Position
                    wall += neighborPos.x + "," + neighborPos.y + ",";

                    // Type
                    wall += "TileType.Wall,";

                    // Dirty
                    wall += "False";

                    placedTiles.Add(neighborPos, wall);
                }
            }
        }
        #endregion

        yield return null;

        #region Instantiate Corners
        /*
         * This function handles wall corners. It checks the diagonals of
         * every floor tile, and if no tile (of any type) exists there,
         * it will put down a will tile.
         */

        foreach (KeyValuePair<Vector2Int, string> tile in placedTiles.ToList())
        {
            // We only want to check floor tiles
            if (tile.Value.Contains("Floor"))
            {
                Vector2Int location = tile.Key;

                Vector2Int diagonal = location + Vector2Int.up + Vector2Int.left; // UP - LEFT
                if (!placedTiles.ContainsKey(diagonal))
                {
                    // No tile there, place one
                    string wall = "";

                    // Position
                    wall += diagonal.x + "," + diagonal.y + ",";

                    // Type
                    wall += "TileType.Wall,";

                    // Dirty
                    wall += "False";
                    placedTiles.Add(diagonal, wall);
                }
                diagonal = location + Vector2Int.up + Vector2Int.right; // UP - RIGHT
                if (!placedTiles.ContainsKey(diagonal))
                {
                    // No tile there, place one
                    string wall = "";

                    // Position
                    wall += diagonal.x + "," + diagonal.y + ",";

                    // Type
                    wall += "TileType.Wall,";

                    // Dirty
                    wall += "False";
                    placedTiles.Add(diagonal, wall);
                }
                diagonal = location + Vector2Int.down + Vector2Int.left; // DOWN - LEFT
                if (!placedTiles.ContainsKey(diagonal))
                {
                    // No tile there, place one
                    string wall = "";

                    // Position
                    wall += diagonal.x + "," + diagonal.y + ",";

                    // Type
                    wall += "TileType.Wall,";

                    // Dirty
                    wall += "False";
                    placedTiles.Add(diagonal, wall);
                }
                diagonal = location + Vector2Int.down + Vector2Int.right; // DOWN - RIGHT
                if (!placedTiles.ContainsKey(diagonal))
                {
                    // No tile there, place one
                    string wall = "";

                    // Position
                    wall += diagonal.x + "," + diagonal.y + ",";

                    // Type
                    wall += "TileType.Wall,";

                    // Dirty
                    wall += "False";
                    placedTiles.Add(diagonal, wall);
                }
            }
        }
        #endregion

        yield return null;

        PlacePreGenerated();

        yield return null;

        if (!_isCustom)
        {
            // If this isn't a custom map, we should place our own spawn points.
            PlaceSpawnPoints();
        }

        SanitizeObjectNames(); // Necessary due to how mapgen with prefabs works. Removes the word "(Clone)" if present in gameObjects.

        mapGenComplete = true;
    }

    private bool called = false;

    [Tooltip("DungeonGeneratorCTR has finished generating the map.")]
    public bool mapGenComplete = false;

    // Make this more refined later (spawn "zones")
    private void PlaceSpawnPoints()
    {
        // We are going to pick a few random rooms. 4-6
        int random = UnityEngine.Random.Range(4, 6);

        for (int i = 0; i < random; i++)
        {
            int _rand = UnityEngine.Random.Range(0, DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms.Count-1);

            RoomCTR spawnRoom = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().rooms[_rand];

            validSpawnLocations.Add(new Vector2Int(spawnRoom.Edges[0].position.x, spawnRoom.Edges[0].position.y));
        }
    }

    public void AddRoom(RoomCTR r) {
        rooms.Add(r);
        //Debug.Log("New RoomCTR Added");
    }

    public RoomCTR FindRoom(Vector2Int at) {
        return rooms.Find(x => x.Contact(at));
    }

    public void AddTunnel(Tunnel t) {
        tunnels.Add(t);
    }

    public void AddAnteRoom(AnteRoom aR) {
        anteRooms.Add(aR);
    }

    public void PlaceRooms() {
        PlaceStructure<RoomCTR>(rooms, "Room");
    }
    public void PlaceTunnels() {
        PlaceStructure<Tunnel>(tunnels, "Tunnel");
    }

    public void PlaceAnteRooms() {
        PlaceStructure<AnteRoom>(anteRooms, "AnteRoom");
    }


    public void PlacePreGenerated()
    {
        // Called BEFORE *PlaceStructure*.

        // NOTE:
        // Well, I may be missing it somewhere,
        // but it seems like Oliveira may have not implemented the method for placing down the pre-placed rooms!
        // Or I may have not implemented it in my later Lists -> Sprites method (the 3 functions above this one), oh well.
        // It does seem the dungeon generation respects whatever tries to be placed (in *map*) so nothing takes up that space,
        // this is very helpful.
        // This is now happening here and im going to do it my own way.
        //
        // We have to interpet the nonsense that is the SquareData enum (found in Resources.cs), this is how we will do that:
        // 
        // G_CLOSED: Oops, all walls!
        // IR_OPEN: Oops, all floors!
        // H_DOOR & V_DOOR: Set everything to walls, the tile in the center will be a door instead (since we don't do multi-tile doors here).
        // TODO: Add a special method for unique/oddly shaped rooms (prefabs!)
        //
        // The way we are going to do this is by placing down our wall/floor prefab as normal if there is nothing there.
        // But if there is already something there, we are going to erase it and replace it.

        foreach (RectFill element in design) // For loop here because all rooms are square (for now).
        {
            Vector2Int length = new Vector2Int(element.endX - element.startX, element.endY - element.startY);
            length.x++;
            length.y++;

            if (element.type == SquareData.G_CLOSED) // We are placing walls.
            {
                for (int x = element.startX; x < element.endX; x++)
                {
                    for (int y = element.startY; y < element.endY; y++)
                    {
                        // Check if there is already a tile placed:
                        if (placedTiles.ContainsKey(new Vector2Int(x, y))) // Yes, destory and replace it.
                        {
                            placedTiles.Remove(new Vector2Int(x, y));

                            // Place a new one
                            string wall = "";

                            // Position
                            wall += x + "," + y + ",";

                            // Type
                            wall += "TileType.Wall,";

                            // Dirty
                            wall += "False";
                            placedTiles.Add(new Vector2Int(x, y), wall);
                        }
                        else // No, place a new tile.
                        {
                            // Place a new one
                            string wall = "";

                            // Position
                            wall += x + "," + y + ",";

                            // Type
                            wall += "TileType.Wall,";

                            // Dirty
                            wall += "False";
                            placedTiles.Add(new Vector2Int(x, y), wall);
                        }
                    }
                }
            }
            else if (element.type == SquareData.IR_OPEN) // We are placing floors.
            {
                for (int x = element.startX; x < element.endX; x++)
                {
                    for (int y = element.startY; y < element.endY; y++)
                    {
                        // Check if there is already a tile placed:
                        if (placedTiles.ContainsKey(new Vector2Int(x, y))) // Yes, destory and replace it.
                        {
                            placedTiles.Remove(new Vector2Int(x, y));

                            // Place a new one
                            string floor = "";

                            // Position
                            floor += x + "," + y + ",";

                            // Type
                            floor += "TileType.Floor,";

                            // Dirty
                            floor += "False";
                            placedTiles.Add(new Vector2Int(x, y), floor);
                        }
                        else // No, place a new tile.
                        {
                            // Place a new one
                            string floor = "";

                            // Position
                            floor += x + "," + y + ",";

                            // Type
                            floor += "TileType.Floor,";

                            // Dirty
                            floor += "False";
                            placedTiles.Add(new Vector2Int(x, y), floor);
                        }
                    }
                }
            }
            else if(element.type == SquareData.H_DOOR || element.type == SquareData.V_DOOR)
            {
                // We want to place down a line of walls, and have the center tile be a door.
                Vector2Int center = Vector2Int.zero;

                if(element.type == SquareData.H_DOOR)
                {
                    // *
                    // |
                    // *
                    int extra = 0;
                    if(length.y > 1)
                    {
                        extra++;
                    }

                    center = new Vector2Int(element.startX + extra, (length.y / 2) + element.startY);
                }
                else
                {
                    // *-*
                    int extra = 0;
                    if (length.x > 1)
                    {
                        extra++;
                    }
                    center = new Vector2Int((length.x / 2) + element.startX, element.startY + extra);
                }
                center.y += 2; // odd janky fix
                center.x -= 1;

                for (int x = element.startX; x <= element.endX; x++)
                {
                    for (int y = element.startY + 2; y <= element.endY + 2; y++) // +2 -> odd janky fix
                    {
                        // Check if there is already a tile placed:
                        if (placedTiles.ContainsKey(new Vector2Int(x, y))) // Yes, destory and replace it.
                        {
                            placedTiles.Remove(new Vector2Int(x, y));

                            // Place a new one
                            string wall = "";

                            // Position
                            wall += x + "," + y + ",";

                            // Type
                            wall += "TileType.Wall,";

                            // Dirty
                            wall += "False";
                            placedTiles.Add(new Vector2Int(x, y), wall);
                        }
                        else // No, place a new tile.
                        {
                            // Place a new one
                            string wall = "";

                            // Position
                            wall += x + "," + y + ",";

                            // Type
                            wall += "TileType.Wall,";

                            // Dirty
                            wall += "False";
                            placedTiles.Add(new Vector2Int(x, y), wall);
                        }
                    }
                }

                // And lastly, place the door by replacing one of the walls.
                if (placedTiles.ContainsKey(center))
                {
                    placedTiles.Remove(center);
                }

                // Place a new one
                string door = "";

                // Position
                door += center.x + "," + center.y + ",";

                // Type
                door += "TileType.Door,";

                // Dirty
                door += "False";
                placedTiles.Add(center, door);
            }
            else if(element.type == SquareData.VAR) // Variable rooms (aka Prefabs). This is the tricky stuff
            {
                // NOTICE: The following information is outdated and may be innacurate due to recent changes in how map data is stored and formatted.
                // We have to balance between placing our tiles here and in MapManager.
                // This is because all tiles placed here are interepreted by MapManager and turned into real tiles
                // that then get STORED inside MapManager's lists.
                //
                // We will do this by doing everything normally (by interpreting the prefab into walls, floor, and doors)
                // AND THEN by taking the other items, putting them into a list, and copying that list over to MapManager.
                // This likely does not include walls, floors, or doors, so we can load it into the LAYERED objects list.

                float _rotation = element.rotation;

                GameObject prefab = element.prefab;

                // Grab all the info from the container class
                PMapInfo info = prefab.GetComponent<PMapInfo>();
                Tilemap tilemap = info.tilemap;
                // Then put all the tiles in a list
                List<KeyValuePair<Vector2Int, Tile>> tiles = new List<KeyValuePair<Vector2Int, Tile>>();
                for (int bx = info.bounds_BL.x; bx < info.bounds_TR.x; bx++)
                {
                    for (int by = info.bounds_BL.y; by < info.bounds_TR.y; by++)
                    {
                        Tile T = (Tile)tilemap.GetTile(new Vector3Int(bx, by, 0));

                        if(T != null)
                        {
                            // Also shift tiles so that the Bottom Left corner is at the origin
                            tiles.Add(new KeyValuePair<Vector2Int, Tile>(new Vector2Int(bx - info.bounds_BL.x, by - info.bounds_BL.y), T));
                        }
                    }
                }

                // First go through the tiles
                foreach (var kvp in tiles)
                {
                    Vector2Int pos = kvp.Key;
                    Tile T = kvp.Value;

                    // We go based on the sprite name
                    if (T.sprite.name.Contains("floor"))
                    {
                        // Place a new one
                        string floor = "";

                        // Position
                        floor += pos.x + "," + pos.y + ",";

                        // Type
                        floor += "TileType.Floor,";

                        // Dirty
                        if (info.hasDirtyFloor)
                        {
                            floor += "DIRTY";
                        }
                        else
                        {
                            floor += "CLEAN";
                        }

                        // Sprite name
                        floor += $",{T.sprite.name}";

                        placedTiles.Add(new Vector2Int(pos.x, pos.y), floor);
                    }
                    else if (T.sprite.name.Contains("wall"))
                    {
                        // Place a new one
                        string wall = "";

                        // Position
                        wall += pos.x + "," + pos.y + ",";

                        // Type
                        wall += "TileType.Wall,";

                        // Dirty
                        wall += "False";

                        // Sprite name
                        wall += $",{T.sprite.name}";

                        placedTiles.Add(new Vector2Int(pos.x, pos.y), wall);
                    }
                    else if (T.sprite.name.Contains("pipe") || T.sprite.name.Contains("machine"))
                    {
                        // These are all DECORATIVE machines with no logic
                        string mach = "";

                        // Position
                        mach += pos.x + "," + pos.y + ",";

                        // Type
                        mach += "TileType.Machine,";

                        // Dirty
                        mach += "False";

                        // Sprite name
                        mach += $",{T.sprite.name}";

                        placedTiles.Add(new Vector2Int(pos.x, pos.y), mach);
                    }
                    else if (T.sprite.name.Contains("door"))
                    {
                        string door = "";

                        // Position
                        door += pos.x + "," + pos.y + ",";

                        // Type
                        door += "TileType.Door,";

                        // Dirty
                        door += "False";

                        // Sprite name
                        door += $",{T.sprite.name}";

                        placedTiles.Add(new Vector2Int(pos.x, pos.y), door);
                    }
                }
                // Then go through all the misc objects in the lists
                // Remember, string info has the format: [position.x],[position.y],[TILE.TYPE],[DIRTY/CLEAN],[(optional)FULLSPRITENAME],[(optional)MISCINFO]
                // Items
                foreach (var I in info.objs_item)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    // No stringification here because it may have the item pool attached to it
                    GameObject obj = GameObject.Instantiate(I, new Vector3(pos.x, pos.y), Quaternion.identity);
                    // Object name (Used to determine thing being spawned)
                    // Some examples are: Storage-1, Med. Battery*, EX-Vault Item, Rigged Nuclear Core*
                    obj.name = $"{I.gameObject.name}";

                    preInitObjects.Add(obj);
                }
                // Bots
                foreach (var I in info.objs_bot)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    // We don't "string-ify" these since they have a lot of important info.
                    // In this case the data is help in an `Actor` script.

                    preInitObjects.Add(Instantiate(I, new Vector3(pos.x, pos.y), Quaternion.identity));
                }
                // Triggers
                foreach (var I in info.objs_trigger)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    // We don't "string-ify" these since they have a lot of important info.
                    // In this case the data is help in an `TriggerArea` script.

                    preInitElements.Add(Instantiate(I, new Vector3(pos.x, pos.y), Quaternion.identity));
                }
                // Events
                foreach (var I in info.objs_event)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    // We don't "string-ify" these since they have a lot of important info.
                    // In this case the data is help in an `EventTile` script.

                    preInitElements.Add(Instantiate(I, new Vector3(pos.x, pos.y), Quaternion.identity));
                }
                // Entrances
                foreach (var I in info.objs_entrance)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    // Not much to this
                    
                    string entrance = "";

                    // Position
                    entrance += pos.x + "," + pos.y + ",";

                    // Type
                    entrance += "TileType.Arrival,"; // Not a real tiletype but used for parsing

                    // Dirty
                    entrance += "False";

                    if (!placedTiles.ContainsKey(pos))
                    {
                        placedTiles.Add(pos, entrance);
                    }
                }
                // Exits
                foreach (var I in info.objs_exit)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    string exit = "";

                    // Position
                    exit += pos.x + "," + pos.y + ",";

                    // Type
                    exit += "TileType.Exit,";

                    // Dirty
                    exit += "False";

                    // Sprite name
                    exit += $",{I.GetComponent<SpriteRenderer>().sprite.name}";

                    // Object name (Used to determine destination)
                    exit += $",{I.gameObject.name}";

                    if (!placedTiles.ContainsKey(pos))
                    {
                        placedTiles.Add(pos, exit);
                    }
                }
                // Interactable Machines
                foreach (var I in info.objs_machine)
                {
                    // Get the position, and adjust it based on the bottom left corner
                    Vector2Int pos = new Vector2Int((int)(I.transform.position.x) - info.bounds_BL.x, (int)(I.transform.position.y) - info.bounds_BL.y);

                    string machine = "";

                    // Position
                    machine += pos.x + "," + pos.y + ",";

                    // Type
                    machine += "TileType.Machine,";

                    // Dirty
                    machine += "False";

                    // Sprite name
                    machine += $",{I.GetComponent<SpriteRenderer>().sprite.name}";

                    // Object name (Used to determine what the machine does)
                    machine += $",{I.gameObject.name}";

                    if (!placedTiles.ContainsKey(pos))
                    {
                        placedTiles.Add(pos, machine);
                    }
                }

            }
        }
    }
    #endregion

    #region Helper Functions
    // Identifies if a door is an H_DOOR (true) or a V_DOOR (false)
    private bool IdentifyHV_Door(Dictionary<Vector2Int, GameObject> objects, Vector2Int doorPos)
    {
        // We are just going to check each tile adjacent to the door's position (excluding corners)
        // and if we don't find anything on one side, that is the exit, so we can use it to determine
        // which way the door is facing.

        //   H_DOOR (true)
        //   *
        //   |
        //   *
        //
        //   V_DOOR (false)
        //   *-*

        if (!objects.ContainsKey(doorPos + new Vector2Int(0, 1))) // UP
        {
            return false;
        }
        else if (!objects.ContainsKey(doorPos + new Vector2Int(0, -1))) // Down
        {
            return false;
        }
        else if (!objects.ContainsKey(doorPos + new Vector2Int(1, 0))) // Right
        {
            return true;
        }
        else if (!objects.ContainsKey(doorPos + new Vector2Int(-1, 0))) // Left
        {
            return true;
        }

        return true;
    }


    /// <summary>
    /// Used in certain cases for naming pre-decided objects in level generation.
    /// </summary>
    /// <returns>OPTIONAL. Returns a special name to append.</returns>
    private string AddSpecialName(string name)
    {
        string ret = "";

        if (name.Contains("*"))
        {
            string[] split = name.Split('*');
            string right = split[1];
            right = HF.RemoveTrailingNums(right); // Remove trailing "(#)"

            ret = "*" + right; // right side
        }

        return ret;
    }

    public bool HasLightAt(Vector2Int square) {
        if (square.x < 0 || square.x >= sizeX || square.y < 0 || square.y >= sizeY)
            return false;
        return hasLightAt[square.x, square.y];
    }

    public bool HasLightAt(int x, int y) {
        if (x < 0 || x > sizeX || y < 0 || y > sizeY)
            return false;
        return hasLightAt[x, y];
    }

    public void SetLightAt(Vector2Int square) {
        hasLightAt[square.x, square.y] = true;
    }

    public void SetLightAt(int x, int y) {
        hasLightAt[x, y] = true;
    }

    public void SetEdges() {
        foreach(Tunnel t in tunnels)
           t.SetEdges(this);

        foreach(AnteRoom aR in anteRooms)
            aR.SetEdges(this);

        foreach(RoomCTR r in rooms)
            r.SetEdges(this);
    }

    public void SetCeiling() {
        foreach(Tunnel t in tunnels)
           t.SetCeiling(this);

        foreach(AnteRoom aR in anteRooms)
            aR.SetCeiling(this);

        foreach(RoomCTR r in rooms)
            r.SetCeiling(this);
    }

    public void SetLights() {
        foreach(Tunnel t in tunnels)
           t.SetLights(this);

        foreach(AnteRoom aR in anteRooms)
            aR.SetLights(this);

        foreach(RoomCTR r in rooms)
            r.SetLights(this);
    }

    public int GetTunnelerNum() {
        int count = 0;
        foreach (Builder builder in dungeonBuilders) {
            TunnelerCTR tunneler = builder as TunnelerCTR;
            if (tunneler != null)
                count++;
        }
        return count;
    }

    public int GetRoomieNum() {
        int count = 0;
        foreach (Builder builder in dungeonBuilders) {
            Roomie roomie = builder as Roomie;
            if (roomie != null)
                count++;
        }
        return count;
    }

    public int GetMinRoomSize(RoomSize sz) {
        if (sz == RoomSize.SMALL) 
            return minRoomSz;
        else if (sz == RoomSize.MEDIUM)
            return medRoomSz;
        else {
            Debug.Assert(sz == RoomSize.LARGE);
            return larRoomSz;
        }
    }

    public int GetMaxRoomSize(RoomSize sz) {
        if (sz == RoomSize.SMALL) 
            return medRoomSz-1;
        else if (sz == RoomSize.MEDIUM)
            return maxRoomSz-1;
        else {
            Debug.Assert(sz == RoomSize.LARGE);
            return maxRoomSz-1;
        }
    }

    public int GetRoomSizeProbSideways(int tunnelWidth, RoomSize size) {
        if (tunnelWidth >= roomSizeProbSideways.Count) {
            if (size == RoomSize.LARGE)
                return 100;
            else
                return 0;
        } else if (size == RoomSize.LARGE)
            return roomSizeProbSideways[tunnelWidth].large;
          else if (size == RoomSize.MEDIUM)
            return roomSizeProbSideways[tunnelWidth].medium;
          else if (size == RoomSize.SMALL) {
            Debug.Assert(size == RoomSize.SMALL);
            return roomSizeProbSideways[tunnelWidth].small;
          }
        return 0;
    }

    public int GetRoomSizeProbBranching(int tunnelWidth, RoomSize size) {
        if (tunnelWidth >= roomSizeProbBranching.Count) {
            if (size == RoomSize.LARGE)
                return 100;
            else
                return 0;
        } else if (size == RoomSize.LARGE)
            return roomSizeProbBranching[tunnelWidth].large;
          else if (size == RoomSize.MEDIUM)
            return roomSizeProbBranching[tunnelWidth].medium;
          else if (size == RoomSize.SMALL) {
            Debug.Assert(size == RoomSize.SMALL);
            return roomSizeProbBranching[tunnelWidth].small;
          }
        return 0;
    }

    public int Mutate(int input) {
        int output = input - mutator + (DungeonGeneratorCTR.random.Next(2*mutator+1)); 
        if(output < 0)
            return 0;
        else
            return output;
    }

    public int Mutate2(int input) {
        if(input <= 50) {
            if(input < 0)
	            return 0;
            else
	            return DungeonGeneratorCTR.random.Next(2*input+1); 
        } else  {
            if(input > 100)
                return 100;
            else
                return (2 * input - 100 + DungeonGeneratorCTR.random.Next(200-2*input+1));
        }
    }

    private void SanitizeObjectNames()
    {
        foreach (Transform child in dungeonParent.transform)
        {
            HF.RemoveWordFromName(child.gameObject, "(Clone)");
        }
    }

    public bool IsOpen(SquareData square) { if (square == SquareData.OPEN || square == SquareData.NJ_OPEN || square == SquareData.IT_OPEN || square == SquareData.IA_OPEN || square == SquareData.G_OPEN || square == SquareData.NJ_G_OPEN) return true; else return false; }
    public bool IsChecked(Vector2Int pos) { Debug.Assert((pos.x < sizeX) && (pos.y < sizeY) && (pos.x >= 0) && (pos.y >= 0)); return MapFlagsDirs[pos.x, pos.y]._checked; }
    public bool IsChecked(Vector2Int pos, List<Vector2Int> check)
    {
        foreach (Vector2Int square in check)
            if (pos.x == square.x && pos.y == square.y)
                return true;
        return false;
    }

    public bool IsActive(Vector2Int pos, List<Vector2Int> active)
    {
        foreach (Vector2Int square in active)
            if (pos.x == square.x && pos.y == square.y)
                return true;
        return false;
    }
    #endregion
}
