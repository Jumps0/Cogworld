using System.Collections.Generic;
using UnityEngine;
using Resources;

public class Tunnel : StructureCTR {
   public Tunnel(List<DTile> interior, List<DTile> edges, List<DTile> walls, List<DTile> ceiling, List<DTile> corners, List<DTile> doors, List<DTile> columns, List<DTile> lights, Vector2Int center, Vector2Int start, Vector2Int end, bool inDungeon, int sizeX, int sizeY) : base(interior, edges, walls, ceiling, corners, doors, columns, lights, center, start, end, inDungeon, sizeX, sizeY) {}

   public Tunnel() : base() {}

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
 
   public override void SetCorners(DungeonGeneratorCTR map) {}

   public override void SetLights(DungeonGeneratorCTR map) {
      if (columns.Count == 0) {
         foreach (DTile wall in walls) {
            Vector2Int pos = wall.position;
            Vector2Int fwd = wall.facing;
            Vector2Int right = Vector2Int.zero;

            if (fwd.x == 0)
               right.x = fwd.y;
            else if (fwd.y == 0)
               right.y = -fwd.x;

            bool isDark = true;
            
            if (map.GetMapData(pos+right) == SquareData.IT_OPEN || map.GetMapData(pos-right) == SquareData.IT_OPEN) {
               if (map.GetMapData(pos+right) == SquareData.IT_OPEN)
                  AddTile(pos, right+fwd, STType.COLUMN);
               for (int i = -3; i <= 3; i++)
                  for (int j = 0; j <= 3; j++)
                     if (map.HasLightAt(pos+fwd*j+right*i))
                        isDark = false;
                     if (isDark) {
                        map.SetLightAt(pos);
                        AddTile(pos, fwd, STType.LIGHT);
                     }
            } else if (map.GetMapData(pos+right) == SquareData.H_DOOR || map.GetMapData(pos+right) == SquareData.V_DOOR) {
               AddTile(pos, right+fwd, STType.COLUMN);
            } else if (map.GetMapData(pos-right) == SquareData.H_DOOR || map.GetMapData(pos-right) == SquareData.V_DOOR) {
               AddTile(pos, -right+fwd, STType.COLUMN);
            } else {
               STType edge = STType.CEILING;
               if (IsEdge(map, pos+fwd, right, out edge) || IsEdge(map, pos+fwd, -right, out edge)) {
                  if (IsEdge(map, pos+fwd, right, out edge))
                     if (edge == STType.EDGE)
                        AddTile(pos+right, -right+fwd, STType.COLUMN);
                  isDark = true;
                  for (int i = -3; i <= 3; i++)
                     for (int j = 0; j <= 3; j++)
                        if (map.HasLightAt(pos+fwd*j+right*i))
                           isDark = false;
                        if (isDark) {
                           map.SetLightAt(pos);
                           AddTile(pos, fwd, STType.LIGHT);
                        }
                  } else if (edge == STType.DOOR) {
                     isDark = true;
                     for (int i = 0; i <= 3; i++)
                        for (int j = -3; j <= 3; j++)
                           if (map.HasLightAt(pos+fwd*j+right*i))
                              isDark = false;
                     if (isDark) {
                        map.SetLightAt(pos);
                        AddTile(pos, fwd, STType.LIGHT);
                     }
                  } 
               }   
         }
      } else {
         foreach(DTile column in columns) {
            Vector2Int pos = column.position;
            Vector2Int fwd = column.facing;
            Vector2Int right = Vector2Int.zero;

            if (fwd.x == 0)
               right.x = fwd.y;
            else if (fwd.y == 0)
               right.y = -fwd.x;

            bool isDark = true;
            
            for (int i = -3; i <= 3; i++)
               for (int j = 0; j <= 3; j++)
                  if (map.HasLightAt(pos+fwd*j+right*i))
                     isDark = false;
            
            if (isDark) {
               map.SetLightAt(pos);
               AddTile(pos, fwd, STType.LIGHT);
            }
         }
      }
   }
}
