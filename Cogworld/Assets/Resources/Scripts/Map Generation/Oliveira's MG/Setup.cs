using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using DungeonResources;

[XmlRootAttribute]
public class Setup {

    public bool isCustom;
    public Map map;
    public GeneralGenes generalGenes;
    public Builders builders;
    public RoomParameters roomParameters;
    public CrawlerGenes crawlerGenes;
    public TunnelerGenes tunnelerGenes;
    public RandCrawlerGenes randCrawlerGenes;
    //public MobsAndTreasure mobsAndTreasure;
    public Miscellaneous miscellaneous;

    public Setup() {
        map = new Map();
        generalGenes = new GeneralGenes();
        builders = new Builders();
        roomParameters = new RoomParameters();
        crawlerGenes = new CrawlerGenes();
        tunnelerGenes = new TunnelerGenes();
        randCrawlerGenes = new RandCrawlerGenes();
      //  mobsAndTreasure = new MobsAndTreasure();
        miscellaneous = new Miscellaneous();
    }

    public Setup(int x, int y) {
        map = new Map();
       // mobsAndTreasure = new MobsAndTreasure();
    }

    public static bool ValidateDirections(Direction d, Direction iD) {
        if(iD == Direction.XX)
            return(true); 
        if(iD == Direction.NO)
            if((d == Direction.WE) || (d == Direction.NW) || (d == Direction.NO) || (d == Direction.NE) || (d == Direction.EA))
                return(true);
        if(iD == Direction.EA)
            if( (d == Direction.NO) || (d == Direction.NE) || (d == Direction.EA) || (d == Direction.SE) || (d == Direction.SO))
                return(true);
        if(iD == Direction.SO)
            if( (d == Direction.WE) || (d == Direction.SW) || (d == Direction.SO) || (d == Direction.SE) || (d == Direction.EA))
                return(true);
        if(iD == Direction.WE)
            if((d == Direction.NO) || (d == Direction.NW) || (d == Direction.WE) || (d == Direction.SW) || (d == Direction.SO))
                return(true);
        if(iD == Direction.NW)
            if((d == Direction.NO) || (d == Direction.WE))
                return(true);
        if(iD == Direction.SW)
            if((d == Direction.SO) || (d == Direction.WE))
                return(true);
        if(iD == Direction.NE)
            if((d == Direction.NO) || (d == Direction.EA))
                return(true);
        if(iD == Direction.SE)
            if((d == Direction.SO) || (d == Direction.EA))
                return(true);
  //WELL, TOO BAD, if we get here we are inconsistsnt
  return(false);
    }

    [System.Serializable]
    public class Pair <T, U> {
        public T First {get; set;}
        public U Second {get; set;}
        public Pair() {}
        public Pair(T first, U second) {
            this.First = first;
            this.Second = second;
        }
    };

    public class Map {
        bool consistent;
        int seed;
        int sizeX, sizeY;
        int joinDistance;
        int tunnelJoinDist;
        SquareData backgroundType;
        List<Direction> openings;
        List<RectFill> designElements;
        public Map() {
            BackgroundType = SquareData.NULL;
            Openings = new List<Direction>();
            DesignElements = new List<RectFill>();
            consistent = true;
        }
        public int Seed {get => seed; set {seed = value;}}
        public int SizeX {get => sizeX; set {sizeX = value;}}
        public int SizeY {get => sizeY; set {sizeY = value;}}
        public SquareData BackgroundType {get => backgroundType; set {if (value == SquareData.OPEN || value == SquareData.CLOSED) backgroundType = value; else {backgroundType = SquareData.NULL; consistent = false;}}}
        public int JoinDistance {get => joinDistance; set {joinDistance = value;}}
        public int TunnelJoinDist {get => tunnelJoinDist; set {tunnelJoinDist = value;}}
        public List<Direction> Openings {get => openings; set {openings = value;}}
        public List<RectFill> DesignElements {get => designElements; set {designElements = value;}}
        public bool IsConsistent {get => consistent; set { consistent = value; } }
    }
    
    public class GeneralGenes {
        bool consistent;
        List<int> babyGenerationProbsR;
        List<int> maxAgeC, maxAgeT;
        List<int> stepLengths;
        List<int> corridorWidth;
        TunnelerData lastChanceTunneler;
        int genDelayLastChance;

        public GeneralGenes() {
            babyGenerationProbsR = new List<int>();
            maxAgeC = new List<int>();
            maxAgeT = new List<int>();
            stepLengths = new List<int>();
            corridorWidth = new List<int>();
            lastChanceTunneler = new TunnelerData();
            consistent = true;
        }
        public List<int> BabyGenerationProbsR {get => babyGenerationProbsR; set {int sum = 0; foreach (int prob in value) sum = sum + prob; if (sum == 100) babyGenerationProbsR = value; else {consistent = false;}}}
        public List<int> MaxAgeC {get => maxAgeC; set {maxAgeC = value;}}
        public List<int> MaxAgeT {get => maxAgeT; set {maxAgeT = value;}}
        public List<int> StepLengths {get => stepLengths; set {stepLengths = value;}}
        public List<int> CorridorWidth {get => corridorWidth; set {corridorWidth = value;}}
        public TunnelerData LastChanceTunneler {get => lastChanceTunneler; set {if (ValidateDirections(value.direction, value.desiredDirection)) lastChanceTunneler = value; else {consistent = false;}}}
        public int GenDelayLastChance {get => genDelayLastChance; set {genDelayLastChance = value;}}
        public bool IsConsistent {get => consistent; set { consistent = value; } }
    }
    
   

    public class Builders {
        bool consistent;
        List<CrawlerData> crawlers;
        List<TunnelerData> tunnelers;
        List<Pair<CrawlerData, CrawlerData>> crawlerPairs;
        public Builders() {
            crawlers = new List<CrawlerData>();
            tunnelers = new List<TunnelerData>();
            crawlerPairs = new List<Pair<CrawlerData, CrawlerData>>();
            consistent = true;
        }
        public List<CrawlerData> Crawlers {get => crawlers; set {foreach (CrawlerData crawler in value) if (ValidateDirections(crawler.direction, crawler.desiredDirection)) crawlers.Add(crawler); else consistent = false;}}
        public List<TunnelerData> Tunnelers {get => tunnelers; set {foreach (TunnelerData tunneler in value) if (ValidateDirections(tunneler.direction, tunneler.desiredDirection)) tunnelers.Add(tunneler); else consistent = false;}}
        public List<Pair<CrawlerData, CrawlerData>> CrawlerPairs {
            get => crawlerPairs; 
            set {
                foreach (Pair<CrawlerData, CrawlerData> crawlerPair in value)
                    if (ValidateDirections(crawlerPair.First.direction, crawlerPair.First.desiredDirection) && ValidateDirections(crawlerPair.Second.direction, crawlerPair.Second.desiredDirection))
                        crawlerPairs.Add(crawlerPair);
                    else 
                        consistent = false;
                }
            }

        public bool IsConsistent{get => consistent;}
    }     

    

    public class RoomParameters {
        public RoomParameters() {}
        public int MinRoomSz {get; set;}
        public int MedRoomSz {get; set;} 
        public int LarRoomSz {get; set;}
        public int MaxRoomSz {get; set;}
        public int NumSmallL {get; set;}
        public int NumMedL {get; set;}
        public int NumLarL {get; set;}
        public int NumSmallD {get; set;}
        public int NumMedD {get; set;}
        public int NumLarD {get; set;}

        public bool IsConsistent() {
            if (MinRoomSz > MedRoomSz)
                return false;
            else if (MedRoomSz > LarRoomSz)
                return false;
            else if (LarRoomSz > MaxRoomSz)
                return false;
            return true;
        }
    }

    public class CrawlerGenes {
        bool consistent;
        List<int> babyGenerationProbsC;
        
        int tunnelCrawlerGeneration;
        int tunnelCrawlerClosedProb;
        CrawlerData tunnelCrawlerStats;

        public CrawlerGenes() {
            babyGenerationProbsC = new List<int>();
            consistent = true;
        }
        public List<int> BabyGenerationProbsC {get => babyGenerationProbsC; set {int sum = 0; foreach (int prob in value) sum = sum + prob; if (sum == 100) babyGenerationProbsC = value; else consistent = false;}}
        
        public CrawlerData TunnelCrawlerStats {get => tunnelCrawlerStats; set {tunnelCrawlerStats = value;}}
        public int TunnelCrawlerGeneration {get => tunnelCrawlerGeneration; set {tunnelCrawlerGeneration = value;}}
        public int TunnelCrawlerClosedProb {get => tunnelCrawlerClosedProb; set {tunnelCrawlerClosedProb = value;}}

        public bool IsConsistent {get => consistent;}
    }

    public class TunnelerGenes {
        bool consistent;
        List<int> babyGenerationProbsT;
        List<int> joinPref;
        List<TripleInt> roomSizeProbS;
        List<TripleInt> roomSizeProbB;
        List<int> sizeUpProb, sizeDownProb;
        List<int> anteRoomProb;

        public TunnelerGenes() {
            babyGenerationProbsT = new List<int>();
            joinPref = new List<int>();
            roomSizeProbS = new List<TripleInt>();
            roomSizeProbB = new List<TripleInt>();
            sizeUpProb = new List<int>();
            sizeDownProb = new List<int>();
            anteRoomProb = new List<int>();
            consistent = true;
        }
        public List<int> BabyGenerationProbsT {get => babyGenerationProbsT; set {int sum = 0; foreach (int prob in value) sum = sum + prob; if (sum == 100) babyGenerationProbsT = value; else consistent = false;}}
        public List<int> JoinPref {get => joinPref; set {joinPref = value;}}
        public List<TripleInt> RoomSizeProbS {get => roomSizeProbS; set {foreach (TripleInt roomProb in value) if (roomProb.small+roomProb.medium+roomProb.large == 100) roomSizeProbS.Add(roomProb); else consistent = false;}}
        public List<TripleInt> RoomSizeProbB {get => roomSizeProbB; set {foreach (TripleInt roomProb in value) if (roomProb.small+roomProb.medium+roomProb.large == 100) roomSizeProbB.Add(roomProb); else consistent = false;}}
        public List<int> SizeUpProb {get => sizeUpProb; set {sizeUpProb = value;}}
        public List<int> SizeDownProb {get => sizeDownProb; set {sizeDownProb = value;}}
        public List<int> AnteRoomProb {get => anteRoomProb; set {anteRoomProb = value;}}
        public bool IsConsistent {get => consistent;}
        
    }

    public class RandCrawlerGenes {
        List<int> randCrawlerPerGen;
        int randC_sSSP, randC_sDSP, randC_tSSP, randC_tDSP, randC_cDP;

        public RandCrawlerGenes() {
            RandCrawlerPerGen = new List<int>();
        }

        public List<int> RandCrawlerPerGen {get => randCrawlerPerGen; set {randCrawlerPerGen = value;}}
        public int RandC_sSSP {get => randC_sSSP; set {randC_sSSP = value;}}
        public int RandC_sDSP {get => randC_sDSP; set {randC_sDSP = value;}}
        public int RandC_tSSP {get => randC_tSSP; set {randC_tSSP = value;}}
        public int RandC_tDSP {get => randC_tDSP; set {randC_tDSP = value;}}
        public int RandC_cDP {get => randC_cDP; set {randC_cDP = value;}}
    }
    
    [XmlRootAttribute(IsNullable = false)]
    public class MobsAndTreasure {
        List<int> mobsInLRooms;
        List<int> mobsInLabOpen;
        List<int> mobsInDunRooms;
        List<int> mobsInDunOpen;
        List<int> treasureInLab;
        List<int> treasureInDun;
        int groupSizeMobLabRoom;
        int groupSizeMobDunRoom;
        int groupSizeTreasureInL;
        int groupSizeTreasureInD;
        int groupSizeVarMobInLabRoom;
        int groupSizeVarMobInDunRoom;
        int groupSizeVarTreasureInL;
        int groupSizeVarTreasureInD;
        int inAnteRoomProb;
        public MobsAndTreasure() {
            mobsInLRooms = new List<int>();
            mobsInLabOpen = new List<int>();
            mobsInDunRooms = new List<int>();
            mobsInDunOpen = new List<int>();
            treasureInLab = new List<int>();
            treasureInDun = new List<int>();
        }

        public List<int> MobsInLRooms {get => mobsInLRooms; set {mobsInLRooms = value;}}
        public List<int> MobsInLabOpen {get => mobsInLabOpen; set {}}
        public List<int> MobsInDunRooms {get => mobsInDunRooms; set {}}
        public List<int> MobsInDunOpen {get => mobsInDunOpen; set {}}
        public List<int> TreasureInLab {get => treasureInLab; set {}}
        public List<int> TreasureInDun {get => treasureInDun; set {}}
        public int GroupSizeMobLabRoom {get; set;}
        public int GroupSizeMobDunRoom {get; set;}
        public int GroupSizeTreasureInL {get; set;}
        public int GroupSizeTreasureInD {get; set;}
        public int GroupSizeVarMobInLabRoom {get; set;}
        public int GroupSizeVarMobInDunRoom {get; set;}
        public int GroupSizeVarTreasureInL {get; set;}
        public int GroupSizeVarTreasureInD {get; set;}
        public int InAnteRoomProb {get; set;}
    }

    public class Miscellaneous {
        int patience;
        int mutator;
        int noHeadProb;
        int sizeUpGenDelay;
        bool columnsInTunnels;
        double roomAspectRatio;
        int genSpeedUpOnAnteRoom;
        bool crawlersInTunnels;
        bool crawlersInAnteRooms;
        int seedCrawlersInTunnels;
        public Miscellaneous() {}

        public double RoomAspectRatio {get => roomAspectRatio; set {roomAspectRatio = value;}}       
        public bool CrawlersInTunnels {get => crawlersInTunnels; set {crawlersInTunnels = value;}}
        public bool CrawlersInAnteRooms {get => crawlersInAnteRooms; set {crawlersInAnteRooms = value;}}
        public bool ColumnsInTunnels {get => columnsInTunnels; set {columnsInTunnels = value;}}
        public int SeedCrawlersInTunnels {get => seedCrawlersInTunnels; set {seedCrawlersInTunnels = value;}}
        public int GenSpeedUpOnAnteRoom {get => genSpeedUpOnAnteRoom; set {genSpeedUpOnAnteRoom = value;}}
        public int Patience {get => patience; set {patience = value;}}
        public int Mutator {get => mutator; set {mutator = value;}}
        public int NoHeadProb {get => noHeadProb; set {noHeadProb = value;}}
        public int SizeUpGenDelay {get => sizeUpGenDelay; set {sizeUpGenDelay = value;}}
    }
}
