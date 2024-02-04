using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnteRoom : StructureCTR {
    public AnteRoom(List<DTile> interior, List<DTile> edges, List<DTile> walls, List<DTile> ceiling, List<DTile> corners, List<DTile> doors, List<DTile> columns, List<DTile> lights, Vector2Int center, Vector2Int start, Vector2Int end, bool inDungeon, int sizeX, int sizeY) : base(interior, edges, walls, ceiling, corners, doors, columns, lights, center, start, end, inDungeon, sizeX, sizeY) {}

    public AnteRoom () : base() {}

     public override void SetEdges(DungeonGeneratorCTR map) {
      Vector2Int[] directions = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
         List<DTile> cleanEdges = new List<DTile>();
         foreach (DTile tile in interior) {
            Vector2Int facing = Vector2Int.zero;
            STType edge = STType.FLOOR;
            bool isEdge = false;
            for (int i = 0; i < 4; i++) {
               if (IsEdge(map, tile.position, directions[i], out edge)) {
                  if (edge != STType.DOOR) {
                     isEdge = true;
                     facing = -directions[i];
                     AddTile(tile.position+directions[i], facing, STType.WALL);
                  }
               }
            }
            if (isEdge) {
               AddTile(tile.position, facing, STType.EDGE);
               cleanEdges.Add(tile);
            }
         }
         foreach (DTile tile in cleanEdges) 
            interior.Remove(tile);
         
         cleanEdges.Clear();
   }
    public override void SetLights(DungeonGeneratorCTR map) {}
    public override void SetCorners(DungeonGeneratorCTR map) {}
}
