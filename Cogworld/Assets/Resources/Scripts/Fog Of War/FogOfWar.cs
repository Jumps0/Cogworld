using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class FogOfWar : MonoBehaviour
{
    public static FogOfWar inst;
    public void Awake()
    {
        inst = this;
    }

    public List<Vector3Int> visibleTiles;
    public List<TileBlock> unseenTiles = new List<TileBlock>();
    public List<TileBlock> knownTiles = new List<TileBlock>();


    public void UpdateFogMap(List<Vector3Int> playerFOV) // TODO: This is better, but find a way to not have to do two vision updates
    {
        foreach (Vector3Int pos in visibleTiles)
        {
            // Ensure that this tile is actually in the world dictionary
            if (MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos))
            {
                TData T = MapManager.inst._allTilesRealized[(Vector2Int)pos];

                if (GlobalSettings.inst.cheat_fullVision)
                {
                    // For full vision cheat
                    T.vis = 2;
                }
                else
                {
                    // -- isExplored Check, make sure we haven't actually seen this tile before --
                    if (T.vis == 0) // We have now seen this tile (for the first time)
                    {
                        T.vis = 1; // Make it explored, but not visible

                        StartCoroutine(MapManager.inst._allTilesRealized[(Vector2Int)pos].bottom.RevealAnim());
                    }
                }

                // Update the vis since all changes has been made
                MapManager.inst._allTilesRealized[(Vector2Int)pos] = T;

                // Now that the vis has been decided, we will actually update the objects
                byte final_vis = MapManager.inst._allTilesRealized[(Vector2Int)pos].vis;
                MapManager.inst._allTilesRealized[(Vector2Int)pos].bottom.UpdateVis(final_vis); // Update the vis for the bottom
                if (MapManager.inst._allTilesRealized[(Vector2Int)pos].top != null) // And if it exists, update the vis for the top
                    HF.SetGenericTileVis(MapManager.inst._allTilesRealized[(Vector2Int)pos].top, final_vis);
            }
        }

        visibleTiles.Clear();

        foreach(Vector3Int pos in playerFOV)
        {
            // Make sure the tile actually exists in the world dictionary
            if (MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos))
            {
                TData T = MapManager.inst._allTilesRealized[(Vector2Int)pos];

                // Set it to visible seen its now within our FOV
                T.vis = 2;

                // Update the vis since all changes has been made
                MapManager.inst._allTilesRealized[(Vector2Int)pos] = T;

                // Now that the vis has been decided, we will actually update the objects
                byte final_vis = MapManager.inst._allTilesRealized[(Vector2Int)pos].vis;
                MapManager.inst._allTilesRealized[(Vector2Int)pos].bottom.UpdateVis(final_vis); // Update the vis for the bottom
                if (MapManager.inst._allTilesRealized[(Vector2Int)pos].top != null) // And if it exists, update the vis for the top
                    HF.SetGenericTileVis(MapManager.inst._allTilesRealized[(Vector2Int)pos].top, final_vis);
            }

            visibleTiles.Add(pos);
        }

        visibleTiles.Sort((a, b) => a.x.CompareTo(b.x));
    }

    public void SetEntitiesVisibilities()
    {
        foreach (Actor actor in GameManager.inst.Entities)
        {
            if (actor.GetComponent<PlayerData>())
            {
                continue;
            }

            Vector3 location = actor.transform.position;
            Vector3Int entityPosition = new Vector3Int((int)location.x, (int)location.y, (int)location.z);

            if (visibleTiles.Contains(entityPosition))
            {
                actor.UpdateVis(2);
                actor.isVisible = true;
                actor.isExplored = true;
            }
            else
            {
                actor.UpdateVis(1);
            }
        }

        foreach (var qp in QuestManager.inst.questPoints)
        {
            if(qp != null)
            {
                Vector3Int pos = new Vector3Int((int)qp.transform.position.x, (int)qp.transform.position.y, (int)qp.transform.position.z);

                if (visibleTiles.Contains(pos))
                {
                    qp.GetComponent<QuestPoint>().CheckVisibility();
                    qp.GetComponent<QuestPoint>().isVisible = true;
                    qp.GetComponent<QuestPoint>().isExplored = true;
                }
                else
                {
                    qp.GetComponent<QuestPoint>().CheckVisibility();
                    qp.GetComponent<QuestPoint>().isVisible = false;
                }
            }
        }
    }

    public void DEBUG_RevealAll()
    {
        foreach (var T in MapManager.inst._allTilesRealized)
        {
            TData TD = T.Value;
            TD.vis = 2;

            MapManager.inst._allTilesRealized[T.Key] = TD;

            MapManager.inst._allTilesRealized[T.Key].bottom.UpdateVis(TD.vis); // Update the vis for the bottom
            if (MapManager.inst._allTilesRealized[T.Key].top != null) // And if it exists, update the vis for the top
                HF.SetGenericTileVis(MapManager.inst._allTilesRealized[T.Key].top, TD.vis);
        }

        // Bots
        foreach (Entity E in GameManager.inst.entities)
        {
            if (E)
            {
                E.GetComponent<Actor>().isVisible = true;
                E.GetComponent<Actor>().isExplored = true;
            }
        }

        // Items
        foreach (var item in InventoryControl.inst.worldItems)
        {
            if (item.Value != null)
            {
                item.Value.GetComponent<Part>().isVisible = true;
                item.Value.GetComponent<Part>().isExplored = true;
            }
        }

        // Quest points
        foreach (var qp in QuestManager.inst.questPoints)
        {
            if (qp != null)
            {
                Vector3Int pos = new Vector3Int((int)qp.transform.position.x, (int)qp.transform.position.y, (int)qp.transform.position.z);

                qp.GetComponent<QuestPoint>().isVisible = true;
                qp.GetComponent<QuestPoint>().isExplored = true;
            }
        }
    }

}