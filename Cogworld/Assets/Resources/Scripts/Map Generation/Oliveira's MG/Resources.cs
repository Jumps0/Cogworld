using UnityEngine;

namespace DungeonResources {
    public struct Coordinate {
        public int first;
        public int second;
        public Coordinate(int x = 0, int y = 0) {first = x; second = y;}
        public Coordinate(ref Coordinate coord) {first = coord.first; second = coord.second;}
        public int getX {get {return first;}}
        public int getY {get {return second;}}
        public void SetCoordinate(ref Coordinate _coord) {first = _coord.first;}

        public static Coordinate operator +(Coordinate lhs, Coordinate rhs) => new Coordinate(lhs.first + rhs.first, lhs.second + rhs.second);
        public static Coordinate operator -(Coordinate lhs, Coordinate rhs) => new Coordinate(lhs.first - rhs.first, lhs.second - rhs.second);
        public static Coordinate operator -(Coordinate coord) => new Coordinate(-coord.first, -coord.second);
        public static Coordinate operator *(int lhs, Coordinate rhs) => new Coordinate(lhs * rhs.first, lhs * rhs.second);

        public static bool operator ==( Coordinate lhs, Coordinate rhs) => ((lhs.first == rhs.first) && (lhs.second == rhs.second));

        public static bool operator !=( Coordinate lhs, Coordinate rhs) => ((lhs.first != rhs.first) || (lhs.second != rhs.second));

        public override bool Equals(object obj) {
            if ((obj == null) || (!this.GetType().Equals(obj.GetType()))) 
                return false;
            else {
                Coordinate coord = (Coordinate) obj;
                return (first == coord.first) && (second == coord.second);
            }
        }

        public override int GetHashCode() {
            return -1;
        }

        public static Coordinate TransformDirection(Direction dir) {
            Coordinate coordinate = new Coordinate();
            switch(dir) {
                case(Direction.NO): coordinate = new Coordinate(-1 , 0); break;
                case(Direction.EA): coordinate = new Coordinate(0 , 1); break;
                case(Direction.SO): coordinate = new Coordinate(1 , 0); break;
                case(Direction.WE): coordinate = new Coordinate(0 , -1); break;
                case(Direction.NE): coordinate = new Coordinate(-1 , 1); break;
                case(Direction.SE): coordinate = new Coordinate(1 , 1); break;
                case(Direction.SW): coordinate = new Coordinate(1 , -1); break;
                case(Direction.NW): coordinate = new Coordinate(-1 , -1); break;
                case(Direction.XX): coordinate = new Coordinate(0 , 0); break;
            }
	        return coordinate;
        }
    };

    public class Directions {
        public static Vector2Int Transform(Direction dir) {
            Vector2Int direction = Vector2Int.zero;

            switch(dir) {
                case(Direction.NO): direction = new Vector2Int(-1 , 0); break;
                case(Direction.EA): direction = new Vector2Int(0 , 1); break;
                case(Direction.SO): direction = new Vector2Int(1 , 0); break;
                case(Direction.WE): direction = new Vector2Int(0 , -1); break;
                case(Direction.NE): direction = new Vector2Int(-1 , 1); break;
                case(Direction.SE): direction = new Vector2Int(1 , 1); break;
                case(Direction.SW): direction = new Vector2Int(1 , -1); break;
                case(Direction.NW): direction = new Vector2Int(-1 , -1); break;
                case(Direction.XX): direction = new Vector2Int(0 , 0); break;
            }
	        return direction;
        }

        public static Direction Transform(Vector2Int dir) {
                if (Transform(Direction.NO) == dir) return Direction.NO;
                if (Transform(Direction.EA) == dir) return Direction.EA;
                if (Transform(Direction.SO) == dir) return Direction.SO;
                if (Transform(Direction.WE) == dir) return Direction.WE;
                if (Transform(Direction.NE) == dir) return Direction.NE;
                if (Transform(Direction.SE) == dir) return Direction.SE;
                if (Transform(Direction.SW) == dir) return Direction.SW;
                if (Transform(Direction.NW) == dir) return Direction.NW;
                else return Direction.XX;
        }
    }
    

    public enum Direction {
        NO=0, EA, SO, WE, NE, SE, SW, NW, XX //XX = no intended direction
    };

    public enum RoomSize {
        NULL = -1,
        SMALL, MEDIUM, LARGE
    };
    
    public enum SquareData {
        NULL = -1, // INVALID VALUE
        OPEN = 0, CLOSED, G_OPEN, G_CLOSED, //GUARANTEED-OPEN AND GUARANTEED-CLOSED
        NJ_OPEN, NJ_CLOSED, NJ_G_OPEN, NJ_G_CLOSED, //NJ = non-join, these cannot be joined br Builders with others of their own kind 
        IR_OPEN, IT_OPEN, IA_OPEN, //inside-room, open; inside-tunnel, open; inside anteroom, open
        H_DOOR, V_DOOR, //horizontal door, varies over y-axis , vertical door, over x-axis(up and down)
        LIGHT_N, LIGHT_S, LIGHT_E, LIGHT_W,
        MOB1, MOB2, MOB3, //MOBs of different level            - higher is better
        TREAS1, TREAS2, TREAS3, //treasure of different value
        COLUMN,
        VAR // Variable (custom prefab)
    };

    public struct SquareInfo {
        public int xCoord, yCoord;
        public SquareData type;
        public SquareInfo(int x, int y, SquareData _type) {
            xCoord = x;
            yCoord = y;
            type = _type;
        }
        public SquareInfo(ref SquareInfo data) {
            xCoord = data.xCoord;
            yCoord = data.yCoord;
            type = data.type;
        }
        
        public void SetSquare(ref SquareInfo rhs) {
            xCoord = rhs.xCoord;
            yCoord = rhs.yCoord;
            type = rhs.type;
        }
    };

    public struct SpawnInfo {
        public int xCoord, yCoord;
        SquareData type;
        SpawnInfo(int x = 0, int y = 0, SquareData t = SquareData.NULL) {
            xCoord = x;
            yCoord = y;
            type = t;
        }
    };

    public struct TripleInt {
        public int small, medium, large;
        public TripleInt(int _small = 0, int _medium = 0, int _large = 0) {
            small = _small;
            medium = _medium;
            large = _large;
        }
    };

    public struct FlagsDirs {
        public bool _checked;
        public FlagsDirs(bool _in = false) {_checked = _in;}
    };

    public struct RectFill {
        public int startX, startY, endX, endY;
        public SquareData type;
        public GameObject prefab; // Used for Variable pre-placed structures
        [HideInInspector] public float rotation;
        public RectFill(int _startX, int _startY, int _endX, int _endY, SquareData _type, GameObject _prefab = null, float _rotation = 0f) {
            startX = _startX;
            startY = _startY;
            endX = _endX;
            endY = _endY;
            type = _type;
            prefab = _prefab;
            rotation = _rotation;
        }
    };

/*
    public struct CrawlerData {
        Coordinate location, direction, intDirection;
        int age, maxAge, gen, stepLength, opening, corridorWidth, 
            straightSingleSpawnProbability, straightDoubleSpawnProbability,
            turnSingleSpawnProbability, turnDoubleSpawnProbability, changeDirectionProbability;
        
        CrawlerData(Coordinate loc = new Coordinate(), Coordinate dir = new Coordinate(), Coordinate intDir = new Coordinate(),
                    int _age = 0, int _maxAge = 0, int _gen = 0, int _stepLength = 1, int _opening = 1, int _corridorWidth = 1,
                    int _sSSP = 0, int _sDSP = 0, int _tSSP = 0, int _tDSP = 0, int _cDP = 0) {
            location = new Coordinate();
            direction = new Coordinate(-1, 0);
            intDirection = new Coordinate(-1, 0);
            age = _age;
            maxAge = _maxAge;
            gen = _gen;
            stepLength = _stepLength;
            opening = _opening;
            corridorWidth = _corridorWidth;
            straightSingleSpawnProbability = _sSSP;
            straightDoubleSpawnProbability = _sDSP;
            turnSingleSpawnProbability = _tSSP;
            turnDoubleSpawnProbability = _tDSP;
            changeDirectionProbability = _cDP;
        }
    };

    public struct TunnelerData {
        Coordinate location, direction, intDirection;
        int age, maxAge, gen, stepLength, tunnelWidth, corridorWidth, 
            straightDoubleSpawnProbability, turnDoubleSpawnProbability, 
            makeRommsRightProbability, makeRoomsLeftProbability, joinPreference;
        
        TunnelerData(int dummy = 0) {
            location = new Coordinate(0, 0);
            direction = new Coordinate(-1, 0);
            intDirection = new Coordinate(-1, 0);
            age = 0;
            maxAge = 0;
            gen = 0;
            stepLength = 1;
            tunnelWidth = 0;
            corridorWidth = 1;
            straightDoubleSpawnProbability = 0;
            turnDoubleSpawnProbability = 0;
            makeRommsRightProbability = 0;
            makeRoomsLeftProbability = 0;
            joinPreference = 50;
        }
    };
    */
}