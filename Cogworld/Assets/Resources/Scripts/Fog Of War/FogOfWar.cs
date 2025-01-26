using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        // Lets fundamentally break this down into how it works
        /*  0. The player has just moved to a new location, and their field of view has just been updated.
         *     We have access to this list of locations in the form of the variable `playerFOV`.
         *     More importantly however, the fog map has yet to be updated, and is still showing what the
         *     map looks like from the player's old position. This is what we need to change.
         *  
         *  1. Before we reveal any previously unseen tiles (explored/unexplored by NOT visible), we need
         *     to make any tiles the player can no longer see NOT visible. Where this is any tiles in the
         *     old `visibileTiles` list that are NOT in the `playerFOV` list.
         * 
         *  2. After being updated the no-longer-visible tiles need to be removed from `visibleTiles`,
         *     and the new tiles need to be set as visibile.
         *  
         *  3. The new tiles (if it is their first time being explored) need to do their reveal animation.
         *  
         *  
         */

        // We will create a "Union" list which contains ALL the unique tiles between both lists so we can interact with everything at once.
        List<Vector3Int> allTiles = visibleTiles.Union<Vector3Int>(playerFOV).ToList<Vector3Int>();

        // Go through all the tiles
        foreach (Vector3Int pos in visibleTiles)
        {
            // Ensure that this tile is actually in the world dictionary
            bool tileInDict = MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos);

            if (tileInDict)
            {
                // Variables
                TData data = MapManager.inst._allTilesRealized[(Vector2Int)pos];
                TileBlock bottom = MapManager.inst._allTilesRealized[(Vector2Int)pos].bottom;
                GameObject top = MapManager.inst._allTilesRealized[(Vector2Int)pos].top;

                // TODO

            }
        }


        foreach (Vector3Int pos in visibleTiles)
        {
            // Ensure that this tile is actually in the world dictionary
            bool tileInDict = MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos);

            if (tileInDict)
            {
                TData T = MapManager.inst._allTilesRealized[(Vector2Int)pos];

                TileBlock TB = MapManager.inst._allTilesRealized[(Vector2Int)pos].bottom;
                GameObject TT = MapManager.inst._allTilesRealized[(Vector2Int)pos].top;

                if (debug_nofog)
                {
                    // For full vision
                    T.vis = 2;
                }
                else
                {
                    // -- isExplored Check
                    if (T.vis == 0 || T.vis == 2)
                    {
                        T.vis = 1; // Make it explored, but not visible

                        // Do the reveal animation if needed
                        if(!TB.firstTimeRevealed)
                            TB.FirstTimeReveal();
                    }
                }

                // Update the vis since all changes has been made
                MapManager.inst._allTilesRealized[(Vector2Int)pos] = T;

                // Now that the vis has been decided, we will actually update the objects
                byte final_vis = MapManager.inst._allTilesRealized[(Vector2Int)pos].vis;
                TB.UpdateVis(final_vis); // Update the vis for the bottom
                if (TT != null) // And if it exists, update the vis for the top
                    HF.SetGenericTileVis(TT, final_vis);
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

    public void SetEntityVisibility()
    {
        foreach (Actor actor in GameManager.inst.Entities)
        {
            if (actor.GetComponent<PlayerData>())
            {
                continue;
            }

            Vector3 location = actor.transform.position;
            Vector3Int entityPosition = new Vector3Int((int)location.x, (int)location.y, (int)location.z);

            if (visibleTiles.Contains(entityPosition)) // We can actively see this bot
            {
                actor.UpdateVis(2);
            }
            else // We can't see this bot
            {
                if (actor.isExplored) // Do we even know of this bots existence?
                { // We've seen it previously so we need to show it.
                    actor.UpdateVis(1);
                }
                else // We don't, so we shouldn't show it.
                {
                    actor.UpdateVis(0);
                }
            }
        }

        foreach (var qp in QuestManager.inst.questPoints)
        {
            if(qp != null)
            {
                Vector3Int pos = new Vector3Int((int)qp.transform.position.x, (int)qp.transform.position.y, (int)qp.transform.position.z);

                if (visibleTiles.Contains(pos)) // We can actively see this quest point
                {
                    qp.GetComponent<QuestPoint>().UpdateVis(2);
                }
                else // We can't see this bot
                {
                    if (qp.GetComponent<QuestPoint>().isExplored) // Do we even know of this quest point's existence?
                    { // We've seen it previously so we need to show it.
                        qp.GetComponent<QuestPoint>().UpdateVis(1);
                    }
                    else // We don't, so we shouldn't show it.
                    {
                        qp.GetComponent<QuestPoint>().UpdateVis(0);
                    }
                }
            }
        }
    }

    private bool debug_nofog = false;
    public void DEBUG_ToggleFog()
    {
        debug_nofog = !debug_nofog;

        if (debug_nofog) // DEBUG mode, show everything
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
        else // Back to normal
        {
            // ?
        }
    }

}