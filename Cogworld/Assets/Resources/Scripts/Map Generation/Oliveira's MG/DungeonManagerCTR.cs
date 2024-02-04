using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Resources;
using System.Collections.Generic;
using System.Collections;

// Originally made by: Ariel Oliveira [https://github.com/ArielOliveira/DungeonGenerator]
// Modified by: Cody Jackson | codyj@nevada.unr.edu

public class DungeonManagerCTR : MonoBehaviour
{   
    public static DungeonManagerCTR instance;
    bool placeDesigns;
    bool stillActive1;
    bool stillActive2;

    bool setEdges;
    bool backGround;
    bool firstSeed;
    bool secondSeed;
    bool iteration = false;
    int number, count, index;
    RectFill rect;

    [Header("Design")]
    //[SerializeField] Text text;
    //[SerializeField] Text text2;
    [SerializeField] string designFileName;
    public DungeonGeneratorCTR generator;
    //
    public bool useFileData = true;
    public MapGen_DataCTR genData;

    [Header("~ Begin Dungeon Generation ~")]
    [Tooltip("Check this to initaite the dungeon generation.")]
    public bool doDungeonGen = false;

    // Start is called before the first frame update
    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!doDungeonGen)
        {
            return;
        }

        if (!doOnce)
        {
            DungeonGenSetup();
            doOnce = true;
        }

        if (runningGen)
        {
            if (!timeOut)
            {
                StartCoroutine(DungeonGenTimeout());
            }

            DungeonGenLive();
        }
    }

    public void forceStartGen(bool value = true)
    {
        doDungeonGen = value; // This is a setter but looks worse
    }


    [Tooltip("Is the dungeon generator currently running?")]
    [SerializeField] private bool runningGen = false;
    private bool timeOut = false;
    private bool doOnce = false;


    public void DungeonGenSetup()
    {

        Setup setup = null;
        if (useFileData) // Read setup data from file
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Setup));

            FileStream file = new FileStream(designFileName, FileMode.Open);
            setup = (Setup)serializer.Deserialize(file);
        }
        else // Read it from MapGen_DataCTR
        {
            if (genData != null)
            {
                setup = readDataFromDataCTR(genData);
            }
            else
            {
                Debug.LogError("ERROR: MapGen_DataCTR in DungeonManagerCTR is null!");
                return;
            }
        }

        placeDesigns = false;
        stillActive1 = true;
        stillActive2 = false;
        setEdges = false;
        backGround = false;
        firstSeed = true;
        secondSeed = true;

        count = 0;
        index = 0;
        generator = GetComponent<DungeonGeneratorCTR>();
        generator.InitFromSetup(setup, setup.map.Seed);
        rect = new RectFill(0, 0, generator.SizeX, generator.SizeY, generator.Background);
        number = generator.SizeX * generator.SizeY;
        if (MapManager.inst)
        {
            MapManager.inst._mapSizeX = generator.SizeX;
            MapManager.inst._mapSizeX = generator.SizeY;
        }

        runningGen = true;
    }

    public void DungeonGenLive()
    {
        bool input = Input.GetButtonDown("Jump");
        //if (input) {
        if (stillActive1)
        {
            if (firstSeed && generator.ActiveGeneration == generator.TunnelCrawlerGeneration)
            {
                generator.SeedCrawlersInTunnels();
                firstSeed = false;
            }

            iteration = generator.MakeIteration();
            if (!iteration)
            {
                stillActive1 = generator.AdvanceGeneration();
            }
            if (!stillActive1)
                stillActive2 = true;
        }

        if (stillActive2)
        {
            if (secondSeed && (generator.TunnelCrawlerGeneration < 0 || generator.ActiveGeneration < generator.TunnelCrawlerGeneration))
            {
                generator.SeedCrawlersInTunnels();
                secondSeed = false;
            }

            iteration = generator.MakeIteration();
            if (!iteration)
            {
                stillActive2 = generator.AdvanceGeneration();
            }
            if (!stillActive2)
                backGround = true;
        }

        if (generator.Background == SquareData.OPEN && backGround)
        {
            if (generator.WantsMoreRoomsL())
            {
                Debug.Log("wants more rooms L");
                if (!generator.CreateRoom(rect))
                {
                    Debug.Log("Count = " + count + " number = " + number);
                    count++;
                }
            }
            if (count > number)
            {
                backGround = false;
                placeDesigns = true;
                count = 0;
            }
        }
        else if (backGround)
        {
            setEdges = true;
            backGround = false;
        }

        if (placeDesigns)
        {
            if (index < generator.Design.Count)
            {
                if (generator.Design[index].type != SquareData.OPEN)
                {
                    index++;
                }

                number = (generator.Design[index].endX - generator.Design[index].startX) * (generator.Design[index].endY - generator.Design[index].startY);
                if (generator.WantsMoreRoomsL())
                {
                    if (!generator.CreateRoom(generator.Design[index]))
                        count++;
                    if (count > number)
                        index++;
                }
            }
            else
            {
                placeDesigns = false;
                setEdges = true;
            }
        }
        //}

        if (setEdges)
        {
            setEdges = false;
            generator.SetEdges();
            generator.SetCeiling();
            generator.SetLights();
            generator.PlaceTunnels();
            generator.PlaceAnteRooms();
            generator.PlaceRooms();
        }

        //text.text = "Active Generation: " + generator.ActiveGeneration;
        //text2.text = "Active Tunnelers: " + generator.GetTunnelerNum() + " Active Roomies: " + generator.GetRoomieNum();
    }

    IEnumerator DungeonGenTimeout()
    {
        timeOut = true;

        yield return new WaitForSeconds(25f);

        runningGen = false;
        doDungeonGen = false;
    }

    /// <summary>
    /// Reads map generation data directly from a serialized object that exists somewhere in the files.
    /// You can find more info about map generation and what the terms mean inside *MapGen_DataCTR*.
    /// </summary>
    /// <param name="data">A serialized object which defines the type of map and variables of how it should be generated.</param>
    /// <returns>A formatted "Setup" object which the map generator can use to create the dungeon.</returns>
    public Setup readDataFromDataCTR(MapGen_DataCTR data)
    {
        Setup s = new Setup();

        // - Map - //
        s.isCustom = data.isCustomMap;
        s.map = new Setup.Map();
        s.map.Seed = (int)System.DateTime.Now.Ticks;
        //s.map.Seed = data.seed;
        s.map.SizeX = data.sizeX;
        s.map.SizeY = data.sizeY;
        s.map.JoinDistance = data.joinDistance;
        s.map.TunnelJoinDist = data.tunnelJoinDistance;
        s.map.BackgroundType = data.background;
        s.map.Openings = data.openings;
        // ~ RectFill lists aren't viewable from editor so we used a custom one
        // ~ that means we need to convert it back here
        s.map.DesignElements = ConvertToRectFill(data.preplaced);

        // - General genes - //
        s.generalGenes = new Setup.GeneralGenes();
        s.generalGenes.BabyGenerationProbsR = data.babyDelayProbsForGenerationRoomies;
        s.generalGenes.MaxAgeC = data.maxAgesCrawlers;
        s.generalGenes.MaxAgeT = data.maxAgesTunnelers;
        s.generalGenes.StepLengths = data.stepLengths;
        s.generalGenes.CorridorWidth = data.corridorWidths;
        s.generalGenes.LastChanceTunneler = data.lastChanceTunneler;
        s.generalGenes.GenDelayLastChance = data.genDelayLastChance;

        // - Builders - //
        s.builders = new Setup.Builders();
        s.builders.Crawlers = data.crawlers;
        s.builders.Tunnelers = data.tunnelers;
        s.builders.CrawlerPairs = data.crawlerPairs;

        // - RoomCTR Parameters - //
        s.roomParameters = new Setup.RoomParameters();
        s.roomParameters.MinRoomSz = data.minRoomSize;
        s.roomParameters.MedRoomSz = data.medRoomSize;
        s.roomParameters.LarRoomSz = data.largeRoomSize;
        s.roomParameters.MaxRoomSz = data.MaxRoomSize;
        s.roomParameters.NumSmallL = data.numSmallL;
        s.roomParameters.NumMedL = data.numMedL;
        s.roomParameters.NumLarL = data.numLargeL;
        s.roomParameters.NumSmallD = data.numSmallD;
        s.roomParameters.NumMedD = data.numMedD;
        s.roomParameters.NumLarD = data.numLargeD;

        // - Crawlers Genes - //
        s.crawlerGenes = new Setup.CrawlerGenes();
        s.crawlerGenes.BabyGenerationProbsC = data.BabyGenerationProbsCrawler;
        s.crawlerGenes.TunnelCrawlerGeneration = data.tunnelCrawlerGeneration;
        s.crawlerGenes.TunnelCrawlerClosedProb = data.tunnelCrawlerClosedProb;
        s.crawlerGenes.TunnelCrawlerStats = data.tunnelCrawlerStats;

        // - TunnelerCTR Genes - //
        s.tunnelerGenes = new Setup.TunnelerGenes();
        s.tunnelerGenes.BabyGenerationProbsT = data.babyGenerationProbsTunneler;
        s.tunnelerGenes.JoinPref = data.joinPref;
        s.tunnelerGenes.RoomSizeProbS = ConvertToTripleInt(data.roomSizeProbS);
        s.tunnelerGenes.RoomSizeProbB = ConvertToTripleInt(data.roomSizeProbB);
        s.tunnelerGenes.SizeUpProb = data.SizeUpProb;
        s.tunnelerGenes.SizeDownProb = data.SizeDownProb;
        s.tunnelerGenes.AnteRoomProb = data.anteRoomProb;

        // - Random Crawler Genes - //
        s.randCrawlerGenes = new Setup.RandCrawlerGenes();
        s.randCrawlerGenes.RandCrawlerPerGen = data.randomCrawlerPerGen;
        s.randCrawlerGenes.RandC_sSSP = data.RandomCrawler_sSSP;
        s.randCrawlerGenes.RandC_sDSP = data.RandomCrawler_sDSP;
        s.randCrawlerGenes.RandC_tSSP = data.RandomCrawler_tSSP;
        s.randCrawlerGenes.RandC_tDSP = data.RandomCrawler_tDSP;
        s.randCrawlerGenes.RandC_cDP = data.RandomCrawler_cDP;

        // - Mobs & Treasure (unused?) - //

        // - Miscellaneous - //
        s.miscellaneous.Patience = data.patience;
        s.miscellaneous.Mutator = data.mutator;
        s.miscellaneous.NoHeadProb = data.noHeadProb;
        s.miscellaneous.SizeUpGenDelay = data.sizeUpGenDelay;
        s.miscellaneous.ColumnsInTunnels = data.columnsInTunnels;
        s.miscellaneous.RoomAspectRatio = data.roomAspectRatio;
        s.miscellaneous.GenSpeedUpOnAnteRoom = data.GenSpeedUpOnAnteRoom;
        s.miscellaneous.CrawlersInTunnels = data.crawlersInTunnels;
        s.miscellaneous.CrawlersInAnteRooms = data.crawlersInAnteRooms;
        s.miscellaneous.SeedCrawlersInTunnels = data.seedCrawlersInTunnels;

        return s;
    }

    public List<RectFill> ConvertToRectFill(List<MapGen_DataCTR.PreplacedRoom> rooms)
    {
        List<RectFill> converted = new List<RectFill>();

        foreach (MapGen_DataCTR.PreplacedRoom room in rooms)
        {
            RectFill newRect = new RectFill(room.startX, room.startY, room.endX, room.endY, room.type, room.prefab);

            converted.Add(newRect);
        }

        return converted;
    }

    public List<TripleInt> ConvertToTripleInt(List<Vector3Int> values)
    {
        List<TripleInt> converted = new List<TripleInt>();

        foreach (Vector3Int t in values)
        {
            TripleInt triple = new TripleInt(t.x, t.y, t.z);

            converted.Add(triple);
        }

        return converted;
    }
}
