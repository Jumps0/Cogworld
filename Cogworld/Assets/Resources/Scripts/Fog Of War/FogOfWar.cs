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

    public void UpdateFogMap(List<Vector3Int> playerFOV)
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
         */

        // We will create a "Union" list which contains ALL the unique tiles between both lists so we can interact with everything at once.
        List<Vector3Int> allTiles = visibleTiles.Union<Vector3Int>(playerFOV).ToList<Vector3Int>();

        // Go through all the tiles
        foreach (Vector3Int pos in allTiles.ToList())
        {
            // Ensure that this tile is actually set
            bool tileInDict = !MapManager.inst.mapdata[pos.x, pos.y].Equals(default(WorldTile));

            if (tileInDict)
            {
                // Variables
                WorldTile tile = MapManager.inst.mapdata[pos.x, pos.y];

                // Is this tile no longer visible?
                bool tileNoLongerVisible = visibleTiles.Contains(pos) && !playerFOV.Contains(pos);
                if (tileNoLongerVisible)
                {
                    tile.vis = 1; // UNSEEN & EXPLORED
                }

                // Is this tile in both lists? (No vis change)
                bool tileStillVisible = visibleTiles.Contains(pos) && playerFOV.Contains(pos);
                if (tileStillVisible)
                {
                    tile.vis = 2; // SEEN & EXPLORED
                }

                // We just saw this tile
                bool tileNewlyVisible = !visibleTiles.Contains(pos) && playerFOV.Contains(pos);
                if (tileNewlyVisible)
                {
                    tile.vis = 2; // SEEN & EXPLORED

                    // Do the reveal animation if needed
                    if (!tile.doneRevealAnimation)
                    {
                        tile.doneRevealAnimation = true;
                        MapManager.inst.TileInitialReveal((Vector2Int)pos);
                    }
                }

                // !! DEBUG - NO FOG !!
                if (debug_nofog)
                {
                    // For full vision
                    tile.vis = 2;
                }

                // Update the actual value
                MapManager.inst.mapdata[pos.x, pos.y].vis = tile.vis;
                MapManager.inst.mapdata[pos.x, pos.y].doneRevealAnimation = tile.doneRevealAnimation;

                // Last step: Remove unseen tiles
                if (tileNoLongerVisible)
                {
                    allTiles.Remove(pos);
                }
            }

            // Update the visibility of this tile
            MapManager.inst.TileUpdateVis((Vector2Int)pos);
        }

        // Update visibleTiles
        visibleTiles = allTiles;
    }

    /// <summary>
    /// Do a vision update for the ENTIRE MAP. Use this sparingly! There are a lot of tiles out there!
    /// </summary>
    public void FullMapVisUpdate()
    {
        for (int x = 0; x < MapManager.inst.mapsize.x; x++)
        {
            for (int y = 0; y < MapManager.inst.mapsize.y; y++)
            {
                MapManager.inst.TileUpdateVis(new Vector2Int(x, y));
            }
        }
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

    [SerializeField] private bool debug_nofog = false;
    public void DEBUG_ToggleFog()
    {
        debug_nofog = !debug_nofog;

        if (debug_nofog) // DEBUG mode, show everything
        {
            // Tiles
            for (int x = 0; x < MapManager.inst.mapsize.x; x++)
            {
                for (int y = 0; y < MapManager.inst.mapsize.y; y++)
                {
                    MapManager.inst.mapdata[x, y].vis = 2;
                    MapManager.inst.TileUpdateVis(new Vector2Int(x, y));
                }
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