using System.Collections.Generic;
using UnityEngine;
using Resources;

[System.Serializable]
public abstract class StructureCTR {
    public enum STType {
            FLOOR,
            EDGE,
            WALL,
            CEILING,
            CORNER,
            DOOR,
            COLUMN,
            LIGHT,
        }

    [System.Serializable]
    public struct DTile {
        public STType type;

        public Vector2Int position;
        public Vector2Int facing;
        public DTile(Vector2Int _pos, Vector2Int _face, STType _type) {
            position = _pos;
            facing = _face;
            type = _type;
        }

        public Vector2Int DTilePos
        {
            get { return position; }
        }

        public Vector2Int DTileFace
        {
            get { return facing; }
        }
    }
    protected List<DTile> interior;
    protected List<DTile> ceiling;
    protected List<DTile> edges;
    protected List<DTile> walls;
    protected List<DTile> corners;
    protected List<DTile> doors;
    protected List<DTile> columns;
    protected List<DTile> lights;
    protected bool inDungeon;
    protected int width;
    protected int length;
    protected Vector2Int center;
    protected Vector2Int start;
    protected Vector2Int end;
    public int machineCount = 0;

    public List<DTile> GetInterior
    {
        get { return interior; }
    }

    public List<DTile> GetDoors
    {
        get { return doors; }
    }

    public Vector2Int GetCenter
    {
        get { return center; }
    }

    public StructureCTR(List<DTile> interior, List<DTile> edges, List<DTile> walls, List<DTile> ceiling, List<DTile> corners, List<DTile> doors, List<DTile> columns, List<DTile> lights, Vector2Int center, Vector2Int start, Vector2Int end, bool inDungeon, int width, int length) {
        this.interior = interior;
        this.walls = walls;
        this.edges = edges;
        this.ceiling = ceiling;
        this.corners = corners;
        this.doors = doors;
        this.columns = columns;
        this.lights = lights;
        this.inDungeon = inDungeon;
        this.width = width;
        this.length = length;
        this.center = center;
        this.start = start;
        this.end = end;
    }

    public StructureCTR() {
        interior = new List<DTile>();
        edges = new List<DTile>();
        walls = new List<DTile>();
        ceiling = new List<DTile>();
        corners = new List<DTile>();
        doors = new List<DTile>();
        columns = new List<DTile>();
        lights = new List<DTile>();
        inDungeon = false;
        width = 0;
        length = 0;
        center = Vector2Int.zero;
        start = Vector2Int.zero;
        end = Vector2Int.zero;
    }

    public bool ContainsTile(Vector2Int pos)
    {
        foreach (DTile tile in interior)
        {
            if(tile.position == pos)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasWall(Vector2Int pos)
    {
        foreach (DTile tile in walls)
        {
            if (tile.position == pos)
            {
                return true;
            }
        }
        return false;
    }

    public void AddTile(Vector2Int position, Vector2Int facing, STType type) {   
        switch(type) {
            case STType.FLOOR:
                interior.Add(new DTile(position, facing, type));
                break;
            case STType.EDGE:
                edges.Add(new DTile(position, facing, type));
                break;
            case STType.WALL:
                walls.Add(new DTile(position, facing, type));
                break;
            case STType.CEILING:
                ceiling.Add(new DTile(position, facing, type));
                break;
            case STType.CORNER:
                corners.Add(new DTile(position, facing, type));
                break;
            case STType.DOOR:
                doors.Add(new DTile(position, facing, type));
                break;
            case STType.COLUMN:
                columns.Add(new DTile(position, facing, type));
                break;
            case STType.LIGHT:
                lights.Add(new DTile(position, facing, type));
                break;
        } 
    }

    public List<DTile> GetTiles(STType type) {   
        List<DTile> tiles = null;
        switch(type) {
            case STType.FLOOR:
                tiles = interior;
                break;
            case STType.EDGE:
                tiles = edges;
                break;                
            case STType.WALL:
                tiles = walls;
                break;
            case STType.CEILING:
                tiles = ceiling;
            break;
            case STType.CORNER:
                tiles = corners;
                break;
            case STType.DOOR:
                tiles = doors;
                break;
            case STType.COLUMN:
                tiles = columns;
                break;
            case STType.LIGHT:
                tiles = lights;
                break;
        } 
        return tiles;
    }

    protected bool IsEdge(DungeonGeneratorCTR map, Vector2Int placement, Vector2Int direction, out STType edge) {
        Vector2Int right = Vector2Int.zero;
        if (direction.x == 0)
            right.x = direction.y;
        else if (direction.y == 0)
            right.y = -direction.x;
        if (map.GetMapData(placement + direction) == SquareData.CLOSED || map.GetMapData(placement + direction) == SquareData.G_CLOSED || map.GetMapData(placement + direction) == SquareData.NJ_CLOSED || map.GetMapData(placement + direction) == SquareData.NJ_G_CLOSED) {
            edge = STType.EDGE;
            return true;
        } else if (map.GetMapData(placement + direction) == SquareData.H_DOOR || map.GetMapData(placement + direction) == SquareData.V_DOOR) {
            edge = STType.DOOR;
            return true;
        }
        edge = STType.FLOOR;
        return false;
    }

    public abstract void SetEdges(DungeonGeneratorCTR map);
    public abstract void SetCorners(DungeonGeneratorCTR map);
    public abstract void SetLights(DungeonGeneratorCTR map);

    public virtual void SetCeiling(DungeonGeneratorCTR map) {
        foreach (DTile floor in interior)
            AddTile(floor.position, floor.facing, STType.CEILING);

        foreach (DTile edge in edges)
            AddTile(edge.position, edge.facing, STType.CEILING);

        foreach (DTile corner in corners)
            AddTile(corner.position, corner.facing, STType.CEILING);
    }

    public int Width {get => width; set {width = value;}}
    public int Length {get => length; set {length = value;}}
    public Vector2Int Center {get => center; set {center = value;}}
    public Vector2Int Start {get => start; set {start = value;}}
    public Vector2Int End {get => end; set {end = value;}}
    public List<DTile> Edges { get => edges; }
    public List<DTile> Doors { get => doors; }
    public bool InDungeon {get => inDungeon; set {inDungeon = value;}}

    public static bool Compare(StructureCTR first, StructureCTR second) {return first.GetTiles(STType.FLOOR).Count > second.GetTiles(STType.FLOOR).Count;}
}
