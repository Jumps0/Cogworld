using System.Collections.Generic;
using UnityEngine;
using Resources;

public class RoomCTR : StructureCTR {    
    private List<RoomCTR> rooms;
    public RoomCTR(List<DTile> interior, List<DTile> edges, List<DTile> walls, List<DTile> ceiling, List<DTile> corners, List<DTile> doors, List<DTile> columns, List<DTile> lights, Vector2Int center, Vector2Int start, Vector2Int end, bool inDungeon, int sizeX, int sizeY) : base(interior, edges, walls, ceiling, corners, doors, columns, lights, center, start, end, inDungeon, sizeX, sizeY) {}

    public RoomCTR() : base() {}

    public DTile GetRandomSquare() {return interior[DungeonGeneratorCTR.random.Next(interior.Count-1)];}

    public bool Contact(Vector2Int contact) {
        foreach (DTile tile in interior) {
            if (tile.position == contact) 
                return true;
        }
        return false;
    }

    public override void SetEdges(DungeonGeneratorCTR map) {
        Vector2Int[] directions = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
        List<DTile> cleanEdges = new List<DTile>();
        foreach (DTile tile in interior) {
            Vector2Int facing = Vector2Int.zero;
            STType edge = STType.FLOOR;
            bool isEdge = false;
            int i = 0;
            int count = 0;
            for (i = 0; i < 4; i++) {
                if (IsEdge(map, tile.position, directions[i], out edge)) {
                    isEdge = true;      
                    facing = -directions[i];
                    count++;
                    if (edge != STType.DOOR) {                       
                        AddTile(tile.position+directions[i], facing, STType.WALL);
                    } 
                }
            }

            if (isEdge) {
                cleanEdges.Add(tile);
                if (count == 1) {
                    AddTile(tile.position, facing, STType.EDGE);
                } else if (count == 2) {
                    if ((map.GetMapData(tile.position) == SquareData.H_DOOR) || (map.GetMapData(tile.position) == SquareData.V_DOOR)) {
                        AddTile(tile.position, facing, STType.EDGE);
                    }
                }
            }
        }

        foreach (DTile tile in cleanEdges)
            interior.Remove(tile);

        cleanEdges.Clear();
    }
    
    public override void SetLights(DungeonGeneratorCTR map) {
        foreach (DTile door in doors) {
            AddTile(door.position, door.facing, STType.LIGHT);
            map.SetLightAt(door.position);
            AddTile(door.position, -door.facing, STType.LIGHT);
            map.SetLightAt(door.position);
        }
    }

    public override void SetCorners(DungeonGeneratorCTR map) {}
}
