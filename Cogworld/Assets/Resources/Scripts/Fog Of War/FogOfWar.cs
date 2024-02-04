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

    //public bool doSearching = false;
    //public GameObject player;

    //[SerializeField] private LayerMask layermask;

    /*
    private Mesh mesh;
    public Vector3 origin;
    private float startingAngle;
    [SerializeField] private float fov = 360;
    public float viewDistance;
    [SerializeField] private int rayCount = 70;
    */

    public List<Vector3Int> visibleTiles;
    public List<TileBlock> unseenTiles = new List<TileBlock>();
    public List<TileBlock> knownTiles = new List<TileBlock>();

    //private float angle;
    //private float angleIncrease;

    public void UpdateFogMap(List<Vector3Int> playerFOV)
    {
        foreach (Vector3Int pos in visibleTiles)
        {
            // -- isExplored Check
            if (MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos) && MapManager.inst._allTilesRealized[(Vector2Int)pos])
            { // Safety check
                if (!MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>().isExplored) // We have now seen this tile (for the first time)
                {
                    MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>().isExplored = true; // Make it explored

                    if (MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop) // Items
                    {
                        MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop.isExplored = true;
                    }

                    StartCoroutine(MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>().RevealAnim());

                    if (MapManager.inst._layeredObjsRealized.ContainsKey((Vector2Int)pos)) // -- Extra step for doors, access, and machines
                    {
                        if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>()) // Door
                        {
                            MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>().isExplored = true;
                        }
                        else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>()) // Access
                        {
                            MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>().isExplored = true;
                            MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>().InitialReveal();
                        }
                        else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>()) // Machine
                        {
                            MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>().isExplored = true;
                        }
                    }
                }

                // -- isVisibile Check
                MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>().isVisible = false;

                // -- Item Visibility
                if (MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop) // Items
                {
                    MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop.isVisible = false;
                }
            }

            

            // -- Extra step for doors, access, and machines
            if (MapManager.inst._layeredObjsRealized.ContainsKey((Vector2Int)pos) && MapManager.inst._layeredObjsRealized[(Vector2Int)pos])
            {
                if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>()) // Door
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>().isVisible = false;
                }
                else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>()) // Access
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>().isVisible = false;
                }
                else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>()) // Machine
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>().isVisible = false;
                }
            }
            

        }

        visibleTiles.Clear();

        foreach(Vector3Int pos in playerFOV)
        {
            if (MapManager.inst._allTilesRealized.ContainsKey((Vector2Int)pos) && MapManager.inst._allTilesRealized[(Vector2Int)pos])
            { // Safety Check
                MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>().isVisible = true;

                if (MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop) // Items
                {
                    MapManager.inst._allTilesRealized[(Vector2Int)pos].GetComponent<TileBlock>()._partOnTop.isVisible = true;
                }
            }

            if (MapManager.inst._layeredObjsRealized.ContainsKey((Vector2Int)pos) && MapManager.inst._layeredObjsRealized[(Vector2Int)pos]) // -- Extra step for doors | Will be expanded upon later
            { // Safety Check
                if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>()) // Door
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<TileBlock>().isVisible = true;
                }
                else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>()) // Access
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<AccessObject>().isVisible = true;
                }
                else if (MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>()) // Machine
                {
                    MapManager.inst._layeredObjsRealized[(Vector2Int)pos].GetComponent<MachinePart>().isVisible = true;
                }

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
                actor.CheckVisibility();
                actor.isVisible = true;
                actor.isExplored = true;
                //actor.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                actor.CheckVisibility();
                actor.isVisible = false;
                //actor.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        
    }

    public void DEBUG_RevealAll()
    {
        // Tiles
        foreach (KeyValuePair<Vector2Int,TileBlock> T in MapManager.inst._allTilesRealized)
        {
            T.Value.GetComponent<TileBlock>().isVisible = true;
            T.Value.GetComponent<TileBlock>().isExplored = true;
        }

        // Doors - Exits - Machines
        foreach (KeyValuePair<Vector2Int, GameObject> T in MapManager.inst._layeredObjsRealized)
        {
            if (T.Value)
            {
                if (T.Value.GetComponent<DoorLogic>())
                {
                    T.Value.GetComponent<TileBlock>().isVisible = true;
                    T.Value.GetComponent<TileBlock>().isExplored = true;
                }
                else if (T.Value.GetComponent<AccessObject>())
                {
                    T.Value.GetComponent<AccessObject>().isVisible = true;
                    T.Value.GetComponent<AccessObject>().isExplored = true;
                }
                else if (T.Value.GetComponent<MachinePart>())
                {
                    T.Value.GetComponent<MachinePart>().isVisible = true;
                    T.Value.GetComponent<MachinePart>().isExplored = true;
                }
            }
        }

        // Bots
        foreach (Entity E in GameManager.inst.entities)
        {
            E.GetComponent<Actor>().isVisible = true;
            E.GetComponent<Actor>().isExplored = true;
        }

        // Items
        foreach (var item in InventoryControl.inst.worldItems)
        {
            item.Value.GetComponent<Part>().isVisible = true;
            item.Value.GetComponent<Part>().isExplored = true;
        }
    }

}