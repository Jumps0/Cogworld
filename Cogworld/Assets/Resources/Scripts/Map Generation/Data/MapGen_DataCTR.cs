using DungeonResources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapGenCTRParams_",menuName = "PCG/MapGen_dataCTR")]
public class MapGen_DataCTR : ScriptableObject
{
    /// <summary>
    /// This exists to replicate the original functionality of taking data from an .XML file and using it to generate a map.
    /// </summary>

    [Header("Major Map Settings")]
    public bool isCustomMap = false;
    public bool isConsistent = true; // ?
    public int seed;
    public int sizeX;
    public int sizeY;
    [Tooltip("Where the background of the dungeon is open, Builders of subclass WallCrawler (referred to as Crawlers) are active building walls. Where the background is closed, Tunnelers do their thing. While Crawlers only spawn other Crawlers, Tunnelers also spawn Roomies, which (surprise!) make rooms.")]
    public SquareData background; // Open or Closed
    [Tooltip("A wall that will close the gap does so when distance to wall is less than this.")]
    public int joinDistance;
    public int tunnelJoinDistance;
    public List<Direction> openings;
    //public List<RectFill> designElements;
    [Header("   *Pre-Placed Structures*")]
    [Tooltip("This is one of the most important aspects in this file. You can define here multiple different types of structures to exist before" +
        "anything gets generated. The map gen will then happen AROUND these objects.\n\nTerms:\n" +
        "        NULL = -1, // INVALID VALUE\n" +
        "        OPEN = 0, CLOSED, G_OPEN, G_CLOSED, //GUARANTEED-OPEN AND GUARANTEED-CLOSED\n" +
        "        NJ_OPEN, NJ_CLOSED, NJ_G_OPEN, NJ_G_CLOSED, //NJ = non-join, these cannot be joined by Builders with others of their own kind \n" +
        "        IR_OPEN, IT_OPEN, IA_OPEN, //(TLDR: An empty space where nothing should generate) inside-room, open; inside-tunnel, open; inside anteroom, open\n" +
        "        H_DOOR, V_DOOR, //horizontal door, varies over y-axis , vertical door, over x-axis(up and down)\n" +
        "        LIGHT_N, LIGHT_S, LIGHT_E, LIGHT_W,\n" +
        "        MOB1, MOB2, MOB3, //MOBs of different level            - higher is better\n" +
        "        TREAS1, TREAS2, TREAS3, //treasure of different value\n" +
        "        COLUMN,\n" +
        "        VAR // Variable (Custom Prefab)")]
    [SerializeField] public List<PreplacedRoom> preplaced;
    /* << IMPORTANT NOTE >>
        enter rectangles this way: (startX, startY) (endX, endY)   type 
        for possible types see the DungeonMaker.h file
            86        44       99     56     G_CLOSED 
            87        45       98     55     IR_OPEN 
            86        49       86     51     H_DOOR
        later rectangles overwrite earlier ones
        in this manner you can build rooms:
        1) first lay down a G_CLOSED = "guaranteed-closed" rectangle
        2) then inside that another IR_OPEN = "inside-room-open" rect
        3) and finally a horizontal or vertical door (H_DOOR or V_DOOR)
           (vertical V_DOORs extend in the x-direction, 
           doors may need different graphics depending on their orientation

           - If a prefab has an "r" after the size (ex: (9x9)r) then it 
             will be placed randomly on the map at a random rotation
     */

    [Header("General Genes")]
    public bool ggConsistent = true; // ?
    public List<int> babyDelayProbsForGenerationRoomies; // Roomies
    public List<int> maxAgesCrawlers; // Crawlers
    public List<int> maxAgesTunnelers; // Tunnelers
    public List<int> stepLengths;
    public List<int> corridorWidths;

    [Header("Last Chance Tunneler")]
    public TunnelerData lastChanceTunneler;
    //
    public int genDelayLastChance;

    [Header("Builders")]
    [Header("Crawlers")]
    [Header("Tunnelers")]
    public List<CrawlerData> crawlers;
    public List<TunnelerData> tunnelers;
    public List<Setup.Pair<CrawlerData, CrawlerData>> crawlerPairs;

    [Header("Room Parameters")]
    public int minRoomSize;
    public int medRoomSize;
    public int largeRoomSize;
    public int MaxRoomSize;
    public int numSmallL;
    public int numMedL;
    public int numLargeL;
    public int numSmallD;
    public int numMedD;
    public int numLargeD;

    [Header("CrawlerGenes")]
    public List<int> BabyGenerationProbsCrawler;
    public CrawlerData tunnelCrawlerStats;
    public int tunnelCrawlerGeneration;
    public int tunnelCrawlerClosedProb;

    [Header("Tunneler Genes")]
    public List<int> babyGenerationProbsTunneler;
    public List<int> joinPref;
    public List<Vector3Int> roomSizeProbS; // [Small, Medium, Large]
    public List<Vector3Int> roomSizeProbB; // [Small, Medium, Large]
    public List<int> SizeUpProb;
    public List<int> SizeDownProb;
    public List<int> anteRoomProb;

    [Header("Random Crawler Genes")]
    public List<int> randomCrawlerPerGen;
    public int RandomCrawler_sSSP;
    public int RandomCrawler_sDSP;
    public int RandomCrawler_tSSP;
    public int RandomCrawler_tDSP;
    public int RandomCrawler_cDP;

    [Header("Miscellaneous")]
    public float roomAspectRatio;
    public bool crawlersInTunnels;
    public bool crawlersInAnteRooms;
    public bool columnsInTunnels;
    public int seedCrawlersInTunnels;
    public int GenSpeedUpOnAnteRoom;
    public int patience;
    public int mutator;
    public int noHeadProb;
    public int sizeUpGenDelay;

    [System.Serializable]
    public class PreplacedRoom
    {
        public int startX;
        public int startY;
        public int endX;
        public int endY;
        public SquareData type;
        public GameObject prefab; // Used for types of VAR, this is normally null.
    }
}
