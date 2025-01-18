using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Prefabs")]
    // - Prefabs -
    //
    public GameObject powerPrefab;
    public GameObject propulsionPrefab;
    public GameObject utilitiesPrefab;
    public GameObject weaponPrefab;
    //
    public GameObject itemPrefab;
    //
    public GameObject prefab_launcherTargetLine;
    public GameObject prefab_launcherTargetCorner;
    public GameObject prefab_basicTile;
    //
    public GameObject prefab_meleeIndicator;
    //
    public GameObject prefab_machineTimer;
    //
    // -         -

    #region Color Pallet
    [Header("Color Pallet")]
    // - Colors -
    //
    public Color highSecRed;
    public Color dangerRed;
    //
    public Color corruptOrange;
    public Color corruptOrange_faded;
    //
    public Color activeGreen;
    public Color highlightGreen;
    public Color highGreen;
    public Color normalGreen;
    public Color dullGreen;
    public Color subGreen;
    //
    public Color cautiousYellow;
    //
    public Color deepInfoBlue;
    public Color infoBlue;
    public Color brightBlue;
    public Color energyBlue;
    //
    public Color complexWhite;
    public Color inactiveGray;
    public Color specialPurple;
    #endregion

    /*
     *  Sorting Layer Summary:
     *  
     *  12 - Mines
     *  
     *  20 - Player / Bots !!!
     *  23 - Projectiles
     *  24 - Sensor Bot Indicator
     *  25 - Alert Indicator
     *  26 - Targeting Indicator(s)
     *  27 - Item Popups / Bot Popups
     *  29 - Exit Popups
     *  
     *  30 - Mouse Highlight
     *  40 - Perma Highlight
     *  41/42/43 - New Level AnimTileBlocks
     */

    #region > New Floor & Fresh Start Animations <

    [Header("Fresh Start Animation")]
    [SerializeField] private Animator FSA_LOG;
    [SerializeField] private Animator FSA_LAIC;
    [SerializeField] private Animator FSA_TopRight;
    [SerializeField] private Animator FSA_ScanEvasion;
    [SerializeField] private Animator FSA_Parts;
    [SerializeField] private Animator FSA_Inventory;

    /// <summary>
    /// This is where the UI appears in its neat little starting animation.
    /// </summary>
    private void FreshStart_BeginAnimate()
    {
        StartCoroutine(FreshStart_Animation());
    }

    private IEnumerator FreshStart_Animation()
    {
        // This intro animation has multiple sequences, and takes part over a period of X seconds.


        // 1 - Darkness
        // - In the start, everything is black except a few key features, these are;
        // - The [+] of the Log, {L\A\I\C}, CORE ###/###, Parts total [ ##/## ], {c\e\w\q}, Inventory total [ #/# ], {t\m\i}, <MAP> <ESC/?>
        // - Which are their normal colors

        // Start the / LOG / animation
        FSA_LOG.enabled = true;
        FSA_LOG.Play("FSUI_Log");
        // And start the / LAIC / animation at the same time
        FSA_LAIC.enabled = true;
        FSA_LAIC.Play("FSUI_LAIC");
        // And start the [TopRightData] animation at the same time
        FSA_TopRight.enabled = true;
        FSA_TopRight.Play("FSUI_TopRight");
        // And [Scan/Evasion] too
        FSA_ScanEvasion.enabled = true;
        FSA_ScanEvasion.Play("FSUI_ScanEvasion");
        // [/ Parts /]
        FSA_Parts.enabled = true;
        FSA_Parts.Play("FSUI_Parts");
        // [/ Inventory /]
        yield return new WaitForSeconds(0.2f);
        FSA_Inventory.enabled = true;
        FSA_Inventory.Play("FSUI_Inventory");

        // Play the "MANUAL" sound (56)
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["MANUAL"]); // UI - MANUAL

        yield return null;

    }

    [Header("New Floor Animation")]
    [SerializeField] private GameObject NFA_prefab;
    public Dictionary<Vector2Int, GameObject> NFA_squares = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, GameObject> NFA_secondary = new Dictionary<Vector2Int, GameObject>();

    /// <summary>
    /// This is where the player "scans" around the and the floor appears.
    /// </summary>
    public void NewFloor_BeginAnimate()
    {
        if (GlobalSettings.inst.showNewLevelAnimation)
        {
            StartCoroutine(NewFloor_Animation());
        }
        else
        {
            // - Completion Effects -
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_intro["DONE"], 0.45f); // "INTRO/DONE" this audio clip is too loud for its own good
            MapManager.inst.PlayAmbientMusic(); // Ambient music
            MapManager.inst.FreezePlayer(false); // Unfreeze the player, we are done!
        }
    }

    private IEnumerator NewFloor_Animation()
    {
        // First off, we want to get a collection of all the tiles within the player's FOV. This is easy to do.

        List<Vector3Int> fovD = PlayerData.inst.GetComponent<Actor>().FieldofView; // These are effectively Vector2Int's, ignore the Z.
        List<Vector3Int> fov = fovD.Distinct().ToList(); // Remove any possible duplicates

        // We now need to collect the objects in the world based on these coordinates.
        // We could either:
        // -Go through every list of all objects, items, machines, bots, etc. and check if they are in any of these locations
        //    OR
        // -Raycast through every point, and collect anything we find.


        // Once we find what we are looking for, we will instantiate our prefab, and add it to the list with some important data.
        // - We will set the parent to the UIManager

        foreach (var coords in fov)
        {
            // We want to draw a line from above the location to below the location.

            Vector3 lowerPosition = new Vector3(coords.x, coords.y, 2);
            Vector3 upperPosition = new Vector3(coords.x, coords.y, -2);
            Vector3 direction = lowerPosition - upperPosition;
            float distance = Vector3.Distance(new Vector3Int((int)lowerPosition.x, (int)lowerPosition.y, 0), upperPosition);
            direction.Normalize();
            RaycastHit2D[] hits = Physics2D.RaycastAll(upperPosition, direction, distance);

            // - Flags -
            GameObject wall = null;
            GameObject bot = null;
            GameObject item = null;
            GameObject door = null;
            GameObject machine = null;
            GameObject trashTile = null;
            GameObject cleanFloor = null;

            // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                // PROBLEM!!! This list of hits is unsorted and contains multiple things that violate the heirarchy below. This MUST be fixed!

                // There is a heirarchy of what we want to display:
                // -A wall
                // -A bot
                // -An item
                // -A door
                // -A machine
                // -A floor tile WITH *trash*
                // -A floor tile

                // We will solve this problem by setting flags. And then going back afterwards and using our heirarchy.

                #region Hierarchy Flagging
                if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Wall")
                && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // A wall
                    wall = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<Actor>() && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // A bot
                    bot = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<Part>()
                    && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // An item
                    item = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Door")
                    && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Door
                    door = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<MachinePart>()
                    && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y))) // maybe refine this later
                {
                    // Machine
                    machine = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor") && hit.collider.GetComponent<TileBlock>()._debrisSprite.activeInHierarchy
                    && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Dirty floor tile
                    trashTile = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor") && !hit.collider.GetComponent<TileBlock>()._debrisSprite.activeInHierarchy
                    && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Clean floor tile
                    cleanFloor = hit.collider.gameObject;
                }
                #endregion
            }

            #region Hit Management

            if (wall && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // A wall
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("WALL", wall);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);

            }
            else if (bot && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // A bot

                if (bot.GetComponent<PlayerData>()) // Is this the player?
                {
                    GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                    newSquare.transform.SetParent(this.gameObject.transform);
                    newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                    newSquare.GetComponent<AnimTileBlock>().Init("BOT", null, null, null, true);
                    NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
                }
                else // This isn't the player
                {
                    GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                    newSquare.transform.SetParent(this.gameObject.transform);
                    newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                    newSquare.GetComponent<AnimTileBlock>().Init("BOT", null, null, bot.GetComponent<Actor>().botInfo, false);
                    NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
                }
            }
            else if (item && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // An item
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("ITEM", null, item.GetComponent<Part>()._item);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (door && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Door
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("DOOR", door);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (machine && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y))) // maybe refine this later
            {
                // Machine
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("MACHINE", null, null, null, false, machine.gameObject.GetComponent<SpriteRenderer>().sprite);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (trashTile && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Dirty floor tile
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("TRASH", trashTile);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (cleanFloor && !NFA_squares.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Clean floor tile
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("FLOOR", cleanFloor);
                NFA_squares.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            #endregion
        }

        // - Bonus! Now we have to collect anything that is known by the player but not in their FOV. This gets a semi-different animation.
        //   And because we don't want to include the entire map, we need to limit included tiles based on distance from the player.
        //   We can do this by knowing that by default: The camera zoom is at 20, and this allows the player to see 25 tiles away from themselves in each direction.

        // So, we begin
        // - First calculate distance based on camera zoom
        float cam = Camera.main.orthographicSize;
        int dist = (int)(cam * 1.25f);
        // - then calcuate the bottom left corner to search from
        Vector2Int bottomLeft = HF.V3_to_V2I(PlayerData.inst.transform.position) - new Vector2Int(dist, dist);

        // - Then create another FOV list, we are basically just gonna do the raycast stuff again.
        List<Vector3Int> nonFOV = new List<Vector3Int>();

        #region Secondary List Loading
        // The secondary list

        for (int x = bottomLeft.x; x < bottomLeft.x + (dist * 2); x++)
        {
            for (int y = bottomLeft.y; y < bottomLeft.y + (dist * 2); y++)
            {
                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(x, y)))
                {
                    bool _e, _v = false;
                    (_e, _v) = HF.GetGenericTileVis(MapManager.inst._allTilesRealized[new Vector2Int(x, y)].bottom.gameObject);

                    if (_e && !_v)
                    {
                        //NFA_secondary.Add(new Vector2Int(x, y), kvp.Value.gameObject);
                        nonFOV.Add(new Vector3Int(x, y, 0));
                    }
                }
            }
        }

        // Remove duplicates
        List<Vector3Int> temp = nonFOV.Distinct().ToList();
        nonFOV.Clear();
        // Remove objects that are already within FOV
        foreach (Vector3Int point in temp)
        {
            if (!fov.Contains(point))
            {
                nonFOV.Add(point);
            }
        }
        // And finally, do the same deal again
        foreach (var coords in nonFOV)
        {
            // We want to draw a line from above the location to below the location.

            Vector3 lowerPosition = new Vector3(coords.x, coords.y, 2);
            Vector3 upperPosition = new Vector3(coords.x, coords.y, -2);
            Vector3 direction = lowerPosition - upperPosition;
            float distance = Vector3.Distance(new Vector3Int((int)lowerPosition.x, (int)lowerPosition.y, 0), upperPosition);
            direction.Normalize();
            RaycastHit2D[] hits = Physics2D.RaycastAll(upperPosition, direction, distance);

            // - Flags -
            GameObject wall = null;
            GameObject bot = null;
            GameObject item = null;
            GameObject door = null;
            GameObject machine = null;
            GameObject trashTile = null;
            GameObject cleanFloor = null;

            // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                // PROBLEM!!! This list of hits is unsorted and contains multiple things that violate the heirarchy below. This MUST be fixed!

                // There is a heirarchy of what we want to display:
                // -A wall
                // -A bot
                // -An item
                // -A door
                // -A machine
                // -A floor tile WITH *trash*
                // -A floor tile

                // We will solve this problem by setting flags. And then going back afterwards and using our heirarchy.

                #region Hierarchy Flagging
                if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Wall")
                && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // A wall
                    wall = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<Actor>() && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // A bot
                    bot = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<Part>()
                    && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // An item
                    item = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Door")
                    && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Door
                    door = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<MachinePart>()
                    && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y))) // maybe refine this later
                {
                    // Machine
                    machine = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor") && hit.collider.GetComponent<TileBlock>()._debrisSprite.activeInHierarchy
                    && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Dirty floor tile
                    trashTile = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<TileBlock>() && hit.collider.gameObject.name.Contains("Floor") && !hit.collider.GetComponent<TileBlock>()._debrisSprite.activeInHierarchy
                    && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
                {
                    // Clean floor tile
                    cleanFloor = hit.collider.gameObject;
                }
                #endregion
            }

            #region Hit Management

            if (wall && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // A wall
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("WALL", wall);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);

            }
            else if (bot && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // A bot

                if (bot.GetComponent<PlayerData>()) // Is this the player?
                {
                    GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                    newSquare.transform.SetParent(this.gameObject.transform);
                    newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                    newSquare.GetComponent<AnimTileBlock>().Init("BOT", null, null, null, true);
                    NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
                }
                else // This isn't the player
                {
                    GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                    newSquare.transform.SetParent(this.gameObject.transform);
                    newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                    newSquare.GetComponent<AnimTileBlock>().Init("BOT", null, null, bot.GetComponent<Actor>().botInfo, false);
                    NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
                }
            }
            else if (item && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // An item
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("ITEM", null, item.GetComponent<Part>()._item);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (door && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Door
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("DOOR", door);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (machine && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y))) // maybe refine this later
            {
                // Machine
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("MACHINE", null, null, null, false, machine.gameObject.GetComponent<SpriteRenderer>().sprite);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (trashTile && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Dirty floor tile
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("TRASH", trashTile);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            else if (cleanFloor && !NFA_secondary.ContainsKey(new Vector2Int(coords.x, coords.y)))
            {
                // Clean floor tile
                GameObject newSquare = Instantiate(NFA_prefab, new Vector3(coords.x, coords.y, 0), Quaternion.identity);
                newSquare.transform.SetParent(this.gameObject.transform);
                newSquare.transform.position = new Vector3(coords.x, coords.y, 0f);
                newSquare.GetComponent<AnimTileBlock>().Init("FLOOR", cleanFloor);
                NFA_secondary.Add(new Vector2Int(coords.x, coords.y), newSquare);
            }
            #endregion
        }

        //Debug.ClearDeveloperConsole();
        //Debug.Log("Main: " + NFA_squares.Count + " / Secondary: " + NFA_secondary.Count + " / Points: " + nonFOV.Count);

        #endregion

        // Start animating the player's tile, it gets about 1 second by itself in the darkness.
        foreach (KeyValuePair<Vector2Int, GameObject> entry in NFA_squares)
        {
            if (entry.Value.GetComponent<AnimTileBlock>().player)
            {
                entry.Value.GetComponent<AnimTileBlock>().Animate();
                break;
            }
        }

        yield return new WaitForSeconds(1f);

        // Start animating all non wall/doors. They get animated in the next step.
        foreach (KeyValuePair<Vector2Int, GameObject> entry in NFA_squares)
        {
            if(entry.Value.GetComponent<AnimTileBlock>().type != "WALL" && entry.Value.GetComponent<AnimTileBlock>().type != "DOOR" && !entry.Value.GetComponent<AnimTileBlock>().player)
            {
                entry.Value.GetComponent<AnimTileBlock>().Animate();
            }
        }


        // Scan darts
        // - We want to shoot out multiple scan "darts" out directly from the players location (in random directions towards any walls/doors).
        // - Starting with 1 dart, after that we will shoot out 2 each time. The sequence is: 1, 2, 2, 2, 1, 2.
        // - We do this over a period of 3 seconds, with 6 rounds total, 1 per 0.5 seconds.
        StartCoroutine(NFA_Darts());

        // Scan wave
        // - We want to (over the period of 5 seconds), "scan" horizontally from top to bottom, each blocks (by activating their scan function). A total of 5 times (once per second).
        StartCoroutine(NFA_Scan());

        // Then after the 3 seconds of animating
        yield return new WaitForSeconds(3f);
        // We start the final 2 second animation of everything happening TOGETHER.
        foreach (KeyValuePair<Vector2Int, GameObject> entry in NFA_squares)
        {
            entry.Value.GetComponent<AnimTileBlock>().FinalizeAnimation();
        }

        // - We don't need to worry about removing/destroying the tiles because they do it themselves.
        yield return new WaitForSeconds(2f);

        // -- END --
        // - Completion Effects -
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_intro["DONE"], 0.45f); // "UI/DONE" this audio clip is too loud for its own good
        MapManager.inst.PlayAmbientMusic(); // Ambient music
        MapManager.inst.FreezePlayer(false); // Unfreeze the player, we are done!

        yield return null;

        foreach (KeyValuePair<Vector2Int, GameObject> sub in NFA_secondary.ToList())
        {
            Destroy(sub.Value); // Done here because we don't call the FinalizeAnimation() function
        }

        // Clear list(s)
        NFA_squares.Clear();
        NFA_secondary.Clear();
    }

    private IEnumerator NFA_Darts()
    {
        // First assemble a list of valid targets, we want WALLs and DOORs only.
        List<GameObject> targets = new List<GameObject>();
        foreach (KeyValuePair<Vector2Int, GameObject> entry in NFA_squares)
        {
            if(entry.Value.GetComponent<AnimTileBlock>().type == "WALL" || entry.Value.GetComponent<AnimTileBlock>().type == "DOOR")
            {
                targets.Add(entry.Value);
            }
        }

        //List<GameObject> copyList = targets; // Just incase we run out of targets

        // Given our sequence (1, 2, 2, 2, 1, 2) we want to also avoid hitting similar targets. - This is all taking place over a period of *3 SECONDS*.
        float timeDelay = 0.33f;

        // We are going to raycast from the player to our target, this will not only ping the WALL/DOOR, but also draw a line of highlights to it briefly.

        for (int i = 0; i < 6; i++)
        {
            // Play a sound ("UI/IntelON")
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_ui["INTEL_ON"]); // UI - INTEL_ON

            if (i == 0 || i == 4) // Only one dart
            {
                // We want to draw a line from above the location to below the location.
                #region Random Darting

                int randomIndex = Random.Range(0, targets.Count - 1); // Pick a random target from the list

                Vector3 playerPos = PlayerData.inst.gameObject.transform.position;
                Vector3 targetPos = new Vector3(targets[randomIndex].transform.position.x, targets[randomIndex].transform.position.y, 0);
                Vector3 direction = playerPos - targetPos;
                float distance = Vector3.Distance(new Vector3Int((int)playerPos.x, (int)playerPos.y, 0), targetPos);
                direction.Normalize();
                RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, direction, distance); // might need to swap these?

                List<GameObject> newPath = new List<GameObject>();
                
                // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
                for (int j = 0; j < hits.Length; j++)
                {
                    RaycastHit2D hit = hits[j];
                    
                    if (hit.collider.GetComponent<AnimTileBlock>() && hit.collider.gameObject.transform.position != playerPos) // Don't light up the player's tile
                    {
                        newPath.Add(hit.collider.gameObject);
                    }
                }
                
                NFA_TrimRaycast(newPath);

                // And finally
                if (targets[randomIndex].GetComponent<AnimTileBlock>().type == "WALL" || targets[randomIndex].GetComponent<AnimTileBlock>().type == "DOOR")
                {
                    if (!targets[randomIndex].GetComponent<AnimTileBlock>().pinged)
                    {
                        targets[randomIndex].GetComponent<AnimTileBlock>().Animate(); // Animate the wall if it hasn't been already
                    }
                }
                //targets.Remove(targets[randomIndex]); // Remove from targets // NOTE: It's simpler this way to just not to :/

                #endregion

                yield return new WaitForSeconds(timeDelay);
            }
            else // Two darts
            {
                #region Double Darting

                int randomIndex = Random.Range(0, targets.Count - 1); // Pick a random target from the list

                Vector3 playerPos = PlayerData.inst.gameObject.transform.position;
                Vector3 targetPos = new Vector3(targets[randomIndex].transform.position.x, targets[randomIndex].transform.position.y, 0);
                Vector3 direction = playerPos - targetPos;
                float distance = Vector3.Distance(new Vector3Int((int)playerPos.x, (int)playerPos.y, 0), targetPos);
                direction.Normalize();
                RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, direction, distance); // might need to swap these?

                List<GameObject> newPath = new List<GameObject>();

                // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
                for (int j = 0; j < hits.Length; j++)
                {
                    RaycastHit2D hit = hits[j];

                    if (hit.collider.GetComponent<AnimTileBlock>() && hit.collider.gameObject.transform.position != playerPos) // Don't light up the player's tile
                    {
                        newPath.Add(hit.collider.gameObject);
                    }
                }

                NFA_TrimRaycast(newPath);

                // And then
                if (targets[randomIndex].GetComponent<AnimTileBlock>().type == "WALL" || targets[randomIndex].GetComponent<AnimTileBlock>().type == "DOOR")
                {
                    if (!targets[randomIndex].GetComponent<AnimTileBlock>().pinged)
                    {
                        targets[randomIndex].GetComponent<AnimTileBlock>().Animate(); // Animate the wall if it hasn't been already
                    }
                }

                //yield return new WaitForSeconds(0.33f);
                
                // Play the sound again ("UI/IntelON")
                //AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[46]);

                // -- And again! --

                randomIndex = Random.Range(0, targets.Count - 1); // Pick a random target from the list

                targetPos = new Vector3(targets[randomIndex].transform.position.x, targets[randomIndex].transform.position.y, 0);
                direction = playerPos - targetPos;
                distance = Vector3.Distance(new Vector3Int((int)playerPos.x, (int)playerPos.y, 0), targetPos);
                direction.Normalize();
                RaycastHit2D[] hits2 = Physics2D.RaycastAll(targetPos, direction, distance); // might need to swap these?

                newPath = new List<GameObject>();

                // Loop through all the hits and set the targeting highlight on each tile (ideally shouldn't loop that many times)
                for (int j = 0; j < hits2.Length; j++)
                {
                    RaycastHit2D hit = hits2[j];

                    if (hit.collider.GetComponent<AnimTileBlock>() && hit.collider.gameObject.transform.position != playerPos) // Don't light up the player's tile
                    {
                        newPath.Add(hit.collider.gameObject);
                    }
                }

                NFA_TrimRaycast(newPath);

                // And finally
                if (targets[randomIndex].GetComponent<AnimTileBlock>().type == "WALL" || targets[randomIndex].GetComponent<AnimTileBlock>().type == "DOOR")
                {
                    if (!targets[randomIndex].GetComponent<AnimTileBlock>().pinged)
                    {
                        targets[randomIndex].GetComponent<AnimTileBlock>().Animate(); // Animate the wall if it hasn't been already
                    }
                }

                yield return new WaitForSeconds(timeDelay);
                #endregion
            }
        }

        yield return null;
    }

    /// <summary>
    /// Stolen from PlayerData.cs, this is still terrible, but is slightly worse here because
    /// we are dealing with (at MAX) ~200 tiles.
    /// This function exists to trim down highlight raycasts on tiles to make them look thinner.
    /// </summary>
    private void NFA_TrimRaycast(List<GameObject> path)
    {
        /*
        // Prune the path, this list contains all the tiles that SHOULD be active.
        List<GameObject> prunedTiles = HF.PrunePath(path);
        Debug.Log("OG:" + path.Count + " Pruned:" + prunedTiles.Count);

        foreach (GameObject go in prunedTiles)
        {
            go.GetComponent<AnimTileBlock>().Dart();
        }
        */
        // Above disabled for now :(
        foreach (GameObject go in path)
        {
            go.GetComponent<AnimTileBlock>().Dart();
        }
    }

    private IEnumerator NFA_Scan()
    {
        float pingDuration = 0.35f;

        // Calculate how many rows there are
        int rowCount = 0;
        HashSet<int> rowIndices = new HashSet<int>();

        // Combine the NFA_squares dictionary with the secondary dictionary
        Dictionary<Vector2Int, GameObject> NFA_merged = NFA_secondary;
        NFA_squares.ToList().ForEach(x => NFA_merged[x.Key] = x.Value);

        foreach (Vector2Int key in NFA_merged.Keys)
        {
            // Adding the y-coordinate to the HashSet to keep only unique rows
            rowIndices.Add(key.y);
        }

        rowCount = rowIndices.Count;

        // Organize the NFA_merged by *y* coordinate.
        // Initialize the new dictionary
        Dictionary<int, List<GameObject>> sortedDictionary = new Dictionary<int, List<GameObject>>();

        // Iterate through the original dictionary
        foreach (var kvp in NFA_merged)
        {
            // Get the y-coordinate
            int yCoordinate = kvp.Key.y;

            // Check if the y-coordinate is already a key in the sorted dictionary
            if (!sortedDictionary.ContainsKey(yCoordinate))
            {
                sortedDictionary[yCoordinate] = new List<GameObject>();
            }

            // Add the GameObject to the list associated with the y-coordinate
            sortedDictionary[yCoordinate].Add(kvp.Value);
        }

        // Create a sorted dictionary with keys sorted in descending order
        var sortedKeys = new List<int>(sortedDictionary.Keys);
        sortedKeys.Sort((a, b) => b.CompareTo(a));

        // Create a new dictionary with sorted keys
        Dictionary<int, List<GameObject>> sortedResult = new Dictionary<int, List<GameObject>>();
        foreach (var key in sortedKeys)
        {
            sortedResult[key] = sortedDictionary[key];
        }

        for (int i = 0; i < 5; i++) // 5 times total
        {
            // Play the scanline sound. ("INTRO/MAPSCAN")
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_intro["MAPSCAN"]); // INTRO - MAPSCAN

            // Now go through each row, and ping every block in the row
            foreach (var kvp in sortedResult)
            {
                foreach (GameObject entry in kvp.Value) // Loop through each object in the micro-list
                {
                    AnimTileBlock pingObj = entry.GetComponent<AnimTileBlock>();

                    if (pingObj != null)
                    {
                        pingObj.Scan(); // Make it play the scan animation
                    }
                }

                yield return new WaitForSeconds(pingDuration / rowCount); // Equal time between rows
            }

            foreach (KeyValuePair<Vector2Int, GameObject> obj in NFA_merged) // Probably uneccessary. Emergency stop
            {
                obj.Value.GetComponent<AnimTileBlock>().HaltScan();
            }

            yield return new WaitForSeconds(0.3f); // Short delay between scans
        }

        yield return null;
    }


    #endregion

    #region Center Message Top
    [Header("Center Message Top")]
    // - Center Message -
    //
    public GameObject centerMessage;
    public GameObject centerMessage_prefab;
    public GameObject centerMessageArea;
    public List<GameObject> centerMessages = new List<GameObject>();
    Coroutine centerMessageCO;
    //
    // -               -

    /// <summary>
    /// Show a message at the top center of the screen.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="displayType">The visual (color) of the message.</param>
    /// <param name="timeToShow">How long the message appears for, default is 4 seconds.</param>
    public void ShowCenterMessageTop(string message, Color textColor, Color backColor, float timeToShow = 4f)
    {
        // Only want to play the co-routine if nothing else is running, if something is, stop it.
        if (centerMessageCO != null)
        {
            StopCoroutine(centerMessageCO);
            if (centerMessage != null)
                ClearCenterMessages();
        }
        centerMessageCO = StartCoroutine(DoCenterMessage(message, textColor, backColor, timeToShow));
    }

    IEnumerator DoCenterMessage(string message, Color textColor, Color backColor, float timeToShow = 4f)
    {
        // Instiatiate new message
        GameObject newCenterMessage = Instantiate(centerMessage_prefab, centerMessageArea.transform.position, Quaternion.identity);
        newCenterMessage.transform.SetParent(centerMessageArea.transform);
        newCenterMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        centerMessage = newCenterMessage;
        centerMessages.Add(centerMessage);

        // Assign Details
        newCenterMessage.GetComponent<UICenterMessage>().Setup(message, textColor, backColor);
        newCenterMessage.GetComponent<UICenterMessage>().Appear();

        yield return new WaitForSeconds(timeToShow);

        if (centerMessage != null)
            centerMessages.Remove(centerMessage);
            Destroy(centerMessage);

    }

    public void ClearCenterMessages()
    {
        foreach (var item in centerMessages.ToList())
        {
            Destroy(item.gameObject);
        }
    }

    #endregion

    #region Left Messages (ALERT)

    [Header("Left Message(s)")]
    // - Left Message(s) - mostly ALERT:
    //
    public GameObject leftMessageArea;
    public GameObject leftMessage_prefab;
    public List<GameObject> leftMessages = new List<GameObject>();
    //
    // -                 -

    public void CreateLeftMessage(string message, float timeToShow = 10f, AudioClip clip = null)
    {
        StartCoroutine(DoLeftMessage(message, timeToShow, clip));
    }

    IEnumerator DoLeftMessage(string message, float timeToShow = 10f, AudioClip clip = null)
    {
        // Instantiate it & Assign it to left area
        GameObject newLeftMessage = Instantiate(leftMessage_prefab, leftMessageArea.transform.position, Quaternion.identity);
        newLeftMessage.transform.SetParent(leftMessageArea.transform);
        newLeftMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        leftMessages.Add(newLeftMessage);
        // Assign Details
        newLeftMessage.GetComponent<UILeftMessage>().SetText(message);
        newLeftMessage.GetComponent<UILeftMessage>().AppearAnim();

        // Play a sound if necessary
        if(clip != null)
        {
            AudioManager.inst.CreateTempClip(this.transform.position, clip, 0.5f);
            //AudioManager.inst.PlayMiscSpecific(clip, 0.5f);
        }

        yield return new WaitForSeconds(timeToShow);

        // Play dissapear animation
        newLeftMessage.GetComponent<UILeftMessage>().RemoveAnim();
        // Remove it from list
        leftMessages.Remove(newLeftMessage);
        // Destroy it (done internally)

    }

    public void ClearAllLeftMessages()
    {
        foreach (GameObject LM in leftMessages.ToList())
        {
            Destroy(LM);
        }

        leftMessages.Clear();
    }

    #endregion

    #region Bottom Messages

    [Header("Bottom Message(s)")]
    public GameObject bottomMessage_prefab;
    public GameObject bottomMessageArea;
    public List<GameObject> bottomMessages;

    public void CreateBottomMessage(string message, string color, float displayTime = 10f)
    {
        // ~ This type of message doesn't play a sound
        StartCoroutine(DoCreateBottomMessage(message, color, displayTime));
    }

    IEnumerator DoCreateBottomMessage(string message, string color, float displayTime = 10f)
    {
        // Instantiate it & Assign it to left area
        GameObject newBottomMessage = Instantiate(bottomMessage_prefab, bottomMessageArea.transform.position, Quaternion.identity);
        newBottomMessage.transform.SetParent(bottomMessageArea.transform);
        newBottomMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        bottomMessages.Add(newBottomMessage);
        // Assign Details
        newBottomMessage.GetComponent<UIBottomMessage>().Setup(message, color);
        newBottomMessage.GetComponent<UIBottomMessage>().AppearAnim();

        yield return new WaitForSeconds(displayTime);

        // Play dissapear animation
        newBottomMessage.GetComponent<UIBottomMessage>().RemoveAnim();
        // Remove it from list
        bottomMessages.Remove(newBottomMessage);
        // Destroy it (done internally)

    }

    public void ClearAllBottomMessages()
    {
        foreach (GameObject BM in bottomMessages.ToList())
        {
            Destroy(BM);
        }

        bottomMessages.Clear();
    }


    #endregion

    #region Exit Popup
    [Header("Exit Popups")]
    public List<GameObject> exitPopups = new List<GameObject>();
    public GameObject exitPopup_prefab;

    public void CreateExitPopup(GameObject _parent, string setName)
    {
        StartCoroutine(ExitPopup(_parent, setName));
    }

    IEnumerator ExitPopup(GameObject _parent, string setName)
    {
        // Instantiate it & Assign it to parent
        GameObject newExitPopup = Instantiate(exitPopup_prefab, _parent.transform.position, Quaternion.identity);
        newExitPopup.transform.SetParent(_parent.transform);
        newExitPopup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newExitPopup.GetComponent<Canvas>().sortingOrder = 29;
        // Add it to list
        exitPopups.Add(newExitPopup);
        // Assign Details
        newExitPopup.GetComponentInChildren<UIExitPopup>().Setup(setName, _parent);


        yield return new WaitForSeconds(3f);

        // If the player has their mouse over it keep it up
        if (newExitPopup != null)
        {
            while (newExitPopup.GetComponentInChildren<UIExitPopup>().mouseOver)
            {
                yield return null;
            }

            newExitPopup.GetComponentInChildren<UIExitPopup>().Disappear();
        }


        exitPopups.Remove(newExitPopup);
        

    }

    public void ClearExitPopups()
    {
        foreach (var item in exitPopups.ToList())
        {
            item.GetComponent<UIExitPopup>().Disappear();
        }
    }


    #endregion

    #region Item Pop-ups
    [Header("Item Pop-ups")]

    public List<GameObject> itemPopups = new List<GameObject>();
    public GameObject itemPopup_prefab;

    public void CreateItemPopup(GameObject _parent, string setName, Color textColor, Color backingColor, Color edgeColor)
    {
        StartCoroutine(ItemPopup(_parent, setName, textColor, backingColor, edgeColor));
    }

    IEnumerator ItemPopup(GameObject _parent, string setName, Color a, Color b, Color c)
    {
        // Instantiate it & Assign it to parent
        GameObject newItemPopup = Instantiate(itemPopup_prefab, _parent.transform.position, Quaternion.identity);
        newItemPopup.transform.SetParent(_parent.transform);
        newItemPopup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newItemPopup.GetComponent<Canvas>().sortingOrder = 27;
        // Add it to list
        itemPopups.Add(newItemPopup);
        // Assign Details
        newItemPopup.GetComponentInChildren<UIItemPopup>().Setup(setName, _parent, a, b, c);

        yield return null;
    }

    public void ClearItemPopup()
    {
        foreach (var item in itemPopups.ToList())
        {
            item.GetComponent<UIItemPopup>().MessageOut();
        }
    }


    /*
    public void ShowItemPopup(Part item)
    {
        string displayString = item._item.itemName + " [" + item._item.rating.ToString() + "]";
        // Display text should usually be black
        Color edgeColor = item.realColor; // Use item color for edge
        Color barColor = item.realColor * (1f - 0.15f); // Item Color for bar (but 15% darker)

        Vector3 itemOrigin = item.transform.position;

        // Instantiate the item popup prefab
        GameObject popupObject = Instantiate(itemPopupPrefab, itemOrigin, Quaternion.identity);
        popupObject.transform.SetParent(item.transform);

        // Set the text of the item popup
        TextMeshProUGUI popupText = popupObject.GetComponentInChildren<TextMeshProUGUI>();
        popupText.text = item._item.itemName;

        // Set the position of the item popup above the item origin
        RectTransform popupRect = popupObject.GetComponentInChildren<RectTransform>();
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(itemOrigin);
        popupRect.position = screenPoint;

        // Create a line renderer for the popup line
        LineRenderer lineRenderer = popupObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        // Set the positions of the line renderer
        Vector3[] positions = new Vector3[2];
        positions[0] = itemOrigin;
        positions[1] = popupRect.position;
        lineRenderer.SetPositions(positions);

        // Add the popup object to the list of active popups
        activePopups.Add(popupObject);

        // Adjust the position of the new popup to avoid overlaps
        foreach (GameObject existingPopup in activePopups)
        {
            if (existingPopup != popupObject)
            {
                RectTransform existingRect = existingPopup.GetComponentInChildren<RectTransform>();
                if (RectOverlap(popupRect, existingRect))
                {
                    float newY = existingRect.position.y - existingRect.rect.height - popupRect.rect.height;
                    popupRect.position = new Vector2(popupRect.position.x, newY);
                    positions[1] = popupRect.position;
                    lineRenderer.SetPositions(positions);
                }
            }
        }

        // Destroy the popup object and line renderer after a set time
        StartCoroutine(DestroyPopup(popupObject));
    }

    private IEnumerator DestroyPopup(GameObject popupObject)
    {
        yield return new WaitForSeconds(GlobalSettings.inst.itemPopupLifetime);
        activePopups.Remove(popupObject);
        Destroy(popupObject);
    }

    private bool RectOverlap(RectTransform rect1, RectTransform rect2)
    {
        Rect r1 = new Rect(rect1.position.x, rect1.position.y, rect1.rect.width, rect1.rect.height);
        Rect r2 = new Rect(rect2.position.x, rect2.position.y, rect2.rect.width, rect2.rect.height);
        return r1.Overlaps(r2);
    }
    */

    #endregion

    #region Combat Pop-ups
    [Header("Combat Pop-ups")]

    public List<GameObject> combatPopups = new List<GameObject>();
    public GameObject combatPopup_prefab;
    public GameObject combatPopupShort_prefab;

    public void CreateCombatPopup(GameObject _parent, string setName, Color textColor, Color backingColor, Color edgeColor)
    {
        StartCoroutine(CombatPopup(_parent, setName, textColor, backingColor, edgeColor));
    }

    IEnumerator CombatPopup(GameObject _parent, string setName, Color a, Color b, Color c)
    {
        // Instantiate it & Assign it to parent
        GameObject newCombatPopup = Instantiate(combatPopup_prefab, _parent.transform.position, Quaternion.identity);
        newCombatPopup.transform.SetParent(_parent.transform);
        newCombatPopup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newCombatPopup.GetComponent<Canvas>().sortingOrder = 27;
        // Add it to list
        combatPopups.Add(newCombatPopup);
        // Assign Details
        newCombatPopup.GetComponentInChildren<UIItemPopup>().Setup(setName, _parent, a, b, c);


        yield return new WaitForSeconds(3f);

        combatPopups.Remove(newCombatPopup);

        if (newCombatPopup != null)
            newCombatPopup.GetComponentInChildren<UIItemPopup>().MessageOut();

    }

    public void ClearCombatPopup()
    {
        foreach (var item in combatPopups.ToList())
        {
            item.GetComponent<UIItemPopup>().MessageOut();
        }
    }

    /// <summary>
    /// Create a (stubby) UI popup adjacent to a specified object. If an actor is assigned at the end the colors will instead vary on the actor's current health.
    /// </summary>
    /// <param name="_parent">The parent object the popup is centered on.</param>
    /// <param name="message">The message to show.</param>
    /// <param name="mainColor">The primary color of the banner.</param>
    /// <param name="edgeColor">The "edge" color that connects the banner to the stick to the player.</param>
    /// <param name="actor">(Optional) An actor that we can check the health of. If set, the color will instead vary based on the bot's health (green for higher health, red for lower).</param>
    public void CreateShortCombatPopup(GameObject _parent, string message, Color mainColor, Color edgeColor, Actor actor = null)
    {
        // Instantiate it & Assign it to parent
        GameObject newCombatPopup = Instantiate(combatPopup_prefab, _parent.transform.position, Quaternion.identity);
        newCombatPopup.transform.SetParent(_parent.transform);
        newCombatPopup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newCombatPopup.GetComponent<Canvas>().sortingOrder = 27;

        // Consider if an actor has been assigned. If so, color is related to the bot's health.
        if(actor != null)
        {
            // Set color related to current item health
            float HP = actor.currentHealth / actor.maxHealth;
            if (HP >= 0.75) // Healthy
            {
                mainColor = UIManager.inst.activeGreen;
                edgeColor = new Color(UIManager.inst.activeGreen.r, UIManager.inst.activeGreen.g, UIManager.inst.activeGreen.b, 0.7f);
            }
            else if (HP < 0.75 && HP >= 0.5) // Minor Damage
            {
                mainColor = UIManager.inst.cautiousYellow;
                edgeColor = new Color(UIManager.inst.cautiousYellow.r, UIManager.inst.cautiousYellow.g, UIManager.inst.cautiousYellow.b, 0.7f);
            }
            else if (HP < 0.5 && HP >= 0.25) // Medium Damage
            {
                mainColor = UIManager.inst.slowOrange;
                edgeColor = new Color(UIManager.inst.slowOrange.r, UIManager.inst.slowOrange.g, UIManager.inst.slowOrange.b, 0.7f);
            }
            else // Heavy Damage
            {
                mainColor = UIManager.inst.dangerRed;
                edgeColor = new Color(UIManager.inst.dangerRed.r, UIManager.inst.dangerRed.g, UIManager.inst.dangerRed.b, 0.7f);
            }
        }

        // Assign Details
        newCombatPopup.GetComponentInChildren<UICombatPopupShort>().Setup(message, mainColor, edgeColor);
    }

    #endregion

    #region Bot Pop-ups
    [Header("Bot Pop-ups")]

    public List<GameObject> botPopups = new List<GameObject>();
    public GameObject botPopup_prefab;

    public void CreateBotPopup(GameObject _parent, string setName, Color textColor, Color backingColor, Color edgeColor)
    {
        StartCoroutine(BotPopup(_parent, setName, textColor, backingColor, edgeColor));
    }

    IEnumerator BotPopup(GameObject _parent, string setName, Color a, Color b, Color c)
    {
        // Instantiate it & Assign it to parent
        GameObject newBotPopup = Instantiate(itemPopup_prefab, _parent.transform.position, Quaternion.identity);
        newBotPopup.transform.SetParent(_parent.transform);
        newBotPopup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newBotPopup.GetComponent<Canvas>().sortingOrder = 27;
        // Add it to list
        botPopups.Add(newBotPopup);
        // Assign Details
        newBotPopup.GetComponentInChildren<UIItemPopup>().Setup(setName, _parent, a, b, c);


        yield return new WaitForSeconds(3f);

        botPopups.Remove(newBotPopup);

        if (newBotPopup != null)
            newBotPopup.GetComponentInChildren<UIItemPopup>().MessageOut();

    }

    public void ClearBotPopup()
    {
        foreach (var item in botPopups.ToList())
        {
            item.GetComponent<UIItemPopup>().MessageOut();
        }
    }

    #endregion

    #region Combat Projectiles

    [Header("Combat Projectiles")]
    public GameObject projectilePrefab_generic;
    public GameObject projectilePrefab_launcher;
    public List<GameObject> projectiles = new List<GameObject>();

    public void CreateGenericProjectile(Transform origin, Transform target, ItemProjectile weapon, float speed, bool accurate)
    {
        // Instantiate it & Assign it to parent
        GameObject newProjectile = Instantiate(projectilePrefab_generic, origin.position, Quaternion.identity);
        newProjectile.transform.SetParent(origin.transform);
        newProjectile.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newProjectile.GetComponent<Canvas>().sortingOrder = 23;
        // Add it to list
        projectiles.Add(newProjectile);
        // Assign Details
        newProjectile.GetComponentInChildren<Projectile_Generic>().Setup(origin, target, weapon, speed, accurate);
    }

    public void CreateLauncherProjectile(Transform origin, Transform target, ItemObject weapon)
    {
        // Instantiate it & Assign it to parent
        GameObject newProjectile = Instantiate(projectilePrefab_launcher, origin.position, Quaternion.identity);
        newProjectile.transform.SetParent(origin.transform);
        newProjectile.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newProjectile.GetComponent<Canvas>().sortingOrder = 23;
        // Add it to list
        projectiles.Add(newProjectile);
        // Assign Details
        newProjectile.GetComponentInChildren<Projectile_Launcher>().Setup(origin, target, weapon);
    }

    #endregion

    #region Dialogue Box
    [Header("Dialogue Box")]
    public GameObject dialogueBoxRef;
    public GameObject dialogueCloseBox; // Covers the entire box (show upon exit)
    public GameObject dialogueTransmitBox; // Covers the "transmission" (show upon open)
    public GameObject dialogueGuider; // Points to who is speaking
    public TextMeshProUGUI dialogue_text; // The core text itself (the dialogue)
    public TextMeshProUGUI diagloue_textBacker; // The blackness behind the main text
    public TextMeshProUGUI dialogue_speaker; // The text name of who is speaking
    public TextMeshProUGUI dialogueLower_text; // The "..." on the bottom. Only shows if more text will show after current text.
    public TextMeshProUGUI dialogueBackingBinary; // The changing binary in the background.
    public Actor dialogueActor = null;
    public bool dialogue_readyToDisplay = false;

    public void Dialogue_OpenBox(string speaker, Actor speakerA)
    {
        // Freeze the player
        PlayerData.inst.GetComponent<PlayerGridMovement>().inDialogueSequence = true;

        // Set dialogue box position
        Vector3 adjust = new Vector3(-170, -48, 0);
        dialogueBoxRef.transform.position = Camera.main.WorldToScreenPoint(speakerA.transform.position);
        dialogueBoxRef.transform.position += adjust;

        // Assign who is speaking
        dialogueActor = speakerA;

        // Enable & Position dialogue box
        dialogueBoxRef.SetActive(true);

        // Binary (Background)
        InvokeRepeating("DialogueBinary", 0f, 1f);

        // Set name
        dialogue_speaker.text = speaker;

        // Play the background audio
        AudioManager.inst.PlayDialogueAmbient(2, 0.5f); // BKG (background)

        // This should be the only use case?
        if (dialogueActor.GetComponent<BotAI>().uniqueDialogueSound != null)
        {
            // Play a specific sound if this is a unique NPC /w unique dialogue audio
            AudioManager.inst.PlayMiscSpecific(dialogueActor.GetComponent<BotAI>().uniqueDialogueSound);
        }

        StartCoroutine(Dialogue_OpenAnim(speaker));
    }

    private IEnumerator Dialogue_OpenAnim(string speaker)
    {
        dialogueTransmitBox.SetActive(true);
        dialogueTransmitBox.GetComponent<Image>().color = new Color(highlightGreen.r, highlightGreen.g, highlightGreen.b, 1f);
        dialogueGuider.SetActive(false);
        dialogueGuider.transform.parent.gameObject.SetActive(false);

        // For name display flavour
        // Create a StringBuilder to store the display text
        System.Text.StringBuilder displayText = new System.Text.StringBuilder(speaker.Length);

        // Initialize the display text with random 0/1 characters
        for (int i = 0; i < speaker.Length; i++)
        {
            displayText.Append(Random.value < 0.5f ? "0" : "1");
        }

        dialogue_speaker.text = displayText.ToString();

        // Reveal characters over time
        float revealDuration = 0.01f;
        float removeA = 1f / speaker.Length;
        float setA = 1f;

        while (displayText.ToString() != speaker)
        {
            // Gradually fade out the TransmitBox cover
            setA -= removeA;
            dialogueTransmitBox.GetComponent<Image>().color = new Color(highlightGreen.r, highlightGreen.g, highlightGreen.b, setA);

            // Randomly select a character to reveal
            int index = Random.Range(0, speaker.Length);

            // If the character is already revealed, skip to the next iteration
            if (displayText[index] == speaker[index])
                continue;

            // Reveal the character
            displayText[index] = speaker[index];
            dialogue_speaker.text = displayText.ToString();

            // Wait for the specified duration
            yield return new WaitForSeconds(revealDuration);
        }

        // Turn on the guider
        dialogueGuider.SetActive(true);
        dialogueGuider.transform.parent.gameObject.SetActive(true);

        dialogueTransmitBox.SetActive(false);

        dialogue_readyToDisplay = true;
    }

    public void Dialogue_DisplayText(DialogueC text, bool hasMoreDialogue)
    {
        StartCoroutine(DisplayDialogue(text, hasMoreDialogue));
    }

    private IEnumerator DisplayDialogue(DialogueC text, bool hasMoreDialogue)
    {
        // Set the backer text
        diagloue_textBacker.text = "<mark=#000000>" + text.speech + "</mark>"; // Mark highlights it as pure black

        if (hasMoreDialogue)
        {
            dialogueLower_text.transform.parent.gameObject.SetActive(true);
            dialogueLower_text.text = ToBinaryString("...");
        }
        else
        {
            dialogueLower_text.transform.parent.gameObject.SetActive(false);
        }

        /*
         * NOTE:
         *  There is no answer here that is not dissapointing.
         *  
         *  As it stands, through my testing, there is no possible
         *  way to emulate Cogmind's quick and snappy binary to text
         *  dialogue animations. I've tried many many things but
         *  it seems that the speed at which text change be changed
         *  has a cap at ~0.5 seconds per character. Nothing I have
         *  done has been able to break that barrier, and for it
         *  I am extremely dissapointed.
         *  
         *  For now, the animation will just have a couple states
         *  between full binary, semi, and then full text.
         *  This is the best I can do given the circumstances.
         */

        #region Old Reveal Code
        /*
        // Create a StringBuilder to store the display text
        System.Text.StringBuilder displayText = new System.Text.StringBuilder(text.speech.Length);

        // Initialize the display text with random 0/1 characters
        for (int i = 0; i < text.speech.Length; i++)
        {
            displayText.Append(Random.value < 0.5f ? "0" : "1");
        }

        dialogue_text.text = displayText.ToString();

        // Reveal characters over time
        float revealDuration = 0.5f / (float)text.speech.Length;

        // Create a list of indices to reveal in a deterministic order
        List<int> revealOrder = new List<int>(text.speech.Length);
        for (int i = 0; i < text.speech.Length; i++)
        {
            revealOrder.Add(i);
        }

        // Shuffle the reveal order list
        revealOrder = revealOrder.OrderBy(x => Random.value).ToList();
        Debug.Log("Delay: " + revealDuration + " for " + revealOrder.Count + " characters.");
        foreach (int index in revealOrder)
        {
            // If the character is already revealed, skip to the next iteration
            if (displayText[index] == text.speech[index])
                continue;

            // Reveal the character
            displayText[index] = text.speech[index];
            dialogue_text.text = displayText.ToString();

            if (hasMoreDialogue)
            {
                string dot = "...";
                dialogueLower_text.text = ReplaceCharAtIndex(dialogueLower_text.text, Random.Range(0, 2), dot[0]);
            }

            Debug.Log(Time.time + "- Character replaced: " + text.speech[index] + " Character revealed: " + displayText.ToString());

            // Wait for the specified duration
            yield return new WaitForSeconds(revealDuration);
        }
        */
        #endregion

        #region Temp Code
        /*
        List<string> revealVariants = GenerateRevealVariants(text.speech);

        float delay = 0.1f; // This value doesn't really matter

        dialogue_text.text = revealVariants[0];

        yield return new WaitForSeconds(delay);

        dialogue_text.text = revealVariants[revealVariants.Count / 2];
        dialogueLower_text.text = "...";

        yield return new WaitForSeconds(delay);

        dialogue_text.text = text.speech;
        */
        #endregion

        // Look at that, a solution that works!

        List<string> revealVariants = GenerateRevealVariants(text.speech);

        float delay = 0f;
        float perDelay = 0.25f / revealVariants.Count;

        foreach (string variant in revealVariants)
        {
            StartCoroutine(DelayedTextReveal(dialogue_text, variant, delay += perDelay));

            // Lower Dots
            if (hasMoreDialogue)
            {
                string dot = "...";
                dialogueLower_text.text = ReplaceCharAtIndex(dialogueLower_text.text, Random.Range(0, 2), dot[0]);
            }

            // Wait
            //yield return new WaitForSeconds(0.5f / revealVariants.Count);
        }

        yield return new WaitForSeconds(delay);

        // Just in case it hasn't finished yet
        if (hasMoreDialogue)
        {
            dialogueLower_text.text = "...";
        }
        dialogue_text.text = text.speech;
    }

    private IEnumerator DelayedTextReveal(TextMeshProUGUI UI, string text, float delay)
    {
        yield return new WaitForSeconds(delay);

        UI.text = text;
    }

    // Function to generate reveal variants of the input text
    private List<string> GenerateRevealVariants(string originalText)
    {
        // Create a StringBuilder to store the display text
        StringBuilder displayText = new StringBuilder(originalText.Length);

        List<int> revealOrder = new List<int>(originalText.Length);
        for (int i = 0; i < originalText.Length; i++)
        {
            revealOrder.Add(i); // Create a list of indices to reveal in a deterministic order
            displayText.Append(Random.value < 0.5f ? "0" : "1"); // Initialize the display text with random 0/1 characters
        }
        revealOrder = revealOrder.OrderBy(x => Random.value).ToList(); // Shuffle the reveal order list

        List<string> variants = new List<string>();
        string[] arrVariants = new string[originalText.Length + 1];
        for (int i = 0; i < originalText.Length + 1; i++)
        {
            arrVariants[i] = displayText.ToString(); // Fill the array
        }

        for (int i = 0; i < arrVariants.Length; i++)
        {
            if (i != 0) // Skip the first one since its nothing revealed yet
            {
                string variant = arrVariants[i - 1]; // Start off with the previous string
                //Debug.Log(variant + " : " + revealOrder[i - 1] + " : " + originalText[revealOrder[i - 1]].ToString());
                variant = variant.Remove(revealOrder[i - 1], 1).Insert(revealOrder[i - 1], originalText[revealOrder[i - 1]].ToString()); // Replace the character

                arrVariants[i] = variant; // Modify array
            }
        }

        variants = arrVariants.ToList(); // Convert back to list

        return variants;
    }

    private string ToBinaryString(string text)
    {
        string binaryText = "";
        foreach (char c in text)
        {
            binaryText += Random.value < 0.5f ? "0" : "1";
        }
        return binaryText;
    }

    private string ReplaceCharAtIndex(string originalString, int index, char newChar)
    {
        char[] chars = originalString.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }

    [SerializeField] private int dialogue_binaryLen = 368; // Binary [46 digits per line, 8 lines total (368)]
    private void DialogueBinary()
    {
        var randomBinaryChars = new char[dialogue_binaryLen];
        for (int i = 0; i < dialogue_binaryLen; i++)
        {
            randomBinaryChars[i] = (Random.Range(0, 2) == 0) ? '0' : '1';
        }
        dialogueBackingBinary.text = new string(randomBinaryChars);
    }

    public void Dialogue_MoveToNext()
    {
        // This should be the only use case?
        if (dialogueActor.GetComponent<BotAI>())
        {
            dialogueActor.GetComponent<BotAI>().moveToNextDialogue = true; // Move on to the next dialogue
        }
        else
        {
            Debug.LogError("ERROR: Speaker isn't a friendly AI!"); // Change this later if necessary.
        }
    }

    /// <summary>
    /// Quits out of the dialogue box.
    /// </summary>
    public void Dialogue_Quit()
    {
        dialogueActor = null;
        StartCoroutine(Dialogue_QuitAnim());

        // Un-Freeze the player
        PlayerData.inst.GetComponent<PlayerGridMovement>().inDialogueSequence = false;

        // Stop the background audio
        AudioManager.inst.StopDialogueAmbient();

        // Play the close sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["CLOSE"]); // UI - CLOSE
    }

    private IEnumerator Dialogue_QuitAnim()
    {
        dialogueCloseBox.SetActive(true);
        dialogueGuider.SetActive(false);
        dialogueCloseBox.GetComponent<Image>().color = dullGreen;

        yield return new WaitForSeconds(0.1f);

        dialogueBoxRef.SetActive(false);
        dialogueCloseBox.SetActive(false);

        dialogue_readyToDisplay = false;
    }

    #endregion

    #region Generic Machine / Terminal
    [HideInInspector] public InteractableMachine terminal_targetTerm;
    [Header("Terminal/Generic Machines")]
    public bool terminal_isAnimating = false;
    public GameObject terminal_hackingAreaRef;
    public GameObject terminal_targetresultsAreaRef;
    public GameObject terminal_hackinfoArea1;
    public List<GameObject> terminal_hackinfoBorders = new List<GameObject>();
    public Image terminal_TARGETBorders;
    public Image terminal_RESULTSBorders;
    public Image terminal_hackTitleBacker;
    public Image terminal_TARGETheaderbacker;
    public Image terminal_RESULTSheaderbacker;
    public ScrollRect terminal_resultsScrollrect;
    public List<KeyValuePair<string, TerminalCommand>> terminal_manualBuffer = new List<KeyValuePair<string, TerminalCommand>>(); // Past used terminal codes
    //
    public GameObject terminal_hackOptionsArea; // Where the hacking option prefabs will be put (Target)
    public GameObject terminal_hackResultsArea; // Where the hacking result prefabs will be put (Results)
    //
    [Tooltip("Input Field gameObject when using a Terminal")]
    public GameObject terminal_activeIField = null;
    //
    public TextMeshProUGUI terminal_backingBinary;
    public TextMeshProUGUI terminal_name; // Terminal ?### - ? Access
    public Image terminal_secLvl_backing;
    public TextMeshProUGUI terminal_secLvl;
    [SerializeField] private UIHackCloseEffect terminal_closeEffect;
    [SerializeField] private GameObject terminal_static;
    private GameObject terminal_staticAudio = null;
    // - Prefabs
    public GameObject terminal_hackinfoV1_prefab;
    public GameObject terminal_hackinfoV2_prefab;
    public GameObject terminal_hackinfoV3_prefab;
    public GameObject terminal_hackinfoSpacer_prefab;
    public GameObject terminal_hackResultsSpacer_prefab;
    public GameObject terminal_trace_prefab;
    public GameObject terminal_locked_prefab;
    public GameObject terminal_hackoption_prefab;
    public GameObject terminal_hackResults_prefab;
    public GameObject terminal_input_prefab;
    // - Active Lines
    public List<GameObject> terminal_hackinfoList = new List<GameObject>();
    public List<GameObject> terminal_hackTargetsList = new List<GameObject>();
    public List<GameObject> terminal_hackResultsList = new List<GameObject>();
    public List<GameObject> terminal_hackCodesList = new List<GameObject>();

    [Header("    Codes Window")]
    public GameObject codes_window;
    public GameObject codes_setArea; // Where prefabs are assigned to
    public GameObject codes_prefab;

    public void Terminal_OpenGeneric(InteractableMachine target)
    {
        terminal_isAnimating = true;

        // Freeze player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;

        // Change interfacing mode
        PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.TYPING);

        // Set target
        terminal_targetTerm = target;

        // Binary (Background)
        InvokeRepeating("Terminal_Binary", 0f, 1f);

        // Also take the time to track how many times this machine has been interacted with for later use.
        terminal_targetTerm.timesAccessed++;

        // Assign name + get Sec Level / Restricted access
        int secLvl = UIManager.inst.terminal_targetTerm.secLvl;
        bool restrictedAccess = true;
        string terminalNameSpacer = " "; // Needed due to how UI alignment works

        terminal_name.text = terminalNameSpacer + terminal_targetTerm.fullName;
        restrictedAccess = terminal_targetTerm.restrictedAccess;

        // Restricted access?
        if (restrictedAccess)
        {
            terminal_name.text += " - Restricted Access";
        }
        else
        {
            terminal_name.text += " - Unrestricted Access";
        }

        // Security level + color
        switch(secLvl) {
            case 0: // OPEN SYSTEM
                terminal_secLvl_backing.color = highlightGreen;
                terminal_secLvl.text = "OPEN SYSTEM";
                break;
            case 1: // SECURITY LEVEL #
                terminal_secLvl_backing.color = warmYellow;
                terminal_secLvl.text = "SECURITY LEVEL 1";
                break;
            case 2:
                terminal_secLvl_backing.color = warningOrange;
                terminal_secLvl.text = "SECURITY LEVEL 2";
                break;
            case 3:
                terminal_secLvl_backing.color = highSecRed;
                terminal_secLvl.text = "SECURITY LEVEL 3";
                break;
        }


        // //
        // Now That General Setup is done, we do the animation / reveal part
        // //

        StartCoroutine(Terminal_OpenAnim());
    }

    private IEnumerator Terminal_OpenAnim()
    {
        // In-case this menu is being re-opened, we need to make all the images un-transparent again
        #region Image Transparency Reset
        Image[] i1 = terminal_hackingAreaRef.GetComponentsInChildren<Image>();
        Image[] i2 = terminal_targetresultsAreaRef.GetComponentsInChildren<Image>();
        Image[] i3 = terminal_hackinfoArea1.GetComponentsInChildren<Image>();

        var i12 = i1.Concat(i2).ToArray();
        var iFinal = i12.Concat(i3).ToArray();

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 1f);
        }
        #endregion

        float delay = 0.05f;

        // First, the hacking window opens
        terminal_hackingAreaRef.SetActive(true);
        terminal_hackingAreaRef.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["OPEN_1"], 0.7f); // UI - OPEN_1
        StartCoroutine(Terminal_HackBorderAnim());

        // Play (typing) sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["PRINT_2"]); // UI - PRINT_2

        // Next, the "Utilities" text appears at the top of the hacking window
        GameObject hackUtilitiesMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackUtilitiesMessage.transform.SetParent(terminal_hackinfoArea1.transform);
        hackUtilitiesMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackUtilitiesMessage);
        // Assign Details
        hackUtilitiesMessage.GetComponent<UIHackinfoV2>().Setup("Utilities");

        // Add a spacer
        GameObject hackSpacer = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer);

        // Shortly after
        yield return new WaitForSeconds(delay);
        // The hacking related parts (if the player has any) will appear
        // -Get the parts
        List<ItemObject> hackware = new List<ItemObject>();
        bool hasHackware = false;
        (hasHackware, hackware) = Action.FindPlayerHackware();
        if (hasHackware) // Display the hackware
        {
            foreach (ItemObject item in hackware)
            {
                GameObject hackinfoMessage = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
                hackinfoMessage.transform.SetParent(terminal_hackinfoArea1.transform);
                hackinfoMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                terminal_hackinfoList.Add(hackinfoMessage);
                // Assign Details
                hackinfoMessage.GetComponent<UIHackinfoV1>().Setup(item.itemName, item);
            }
        }
        else // Type out "(None)"
        {
            GameObject hackinfoMessage = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackinfoMessage.transform.SetParent(terminal_hackinfoArea1.transform);
            hackinfoMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackinfoMessage);
            // Assign Details
            hackinfoMessage.GetComponent<UIHackinfoV1>().Setup("(NONE)");
        }

        // Add a spacer
        GameObject hackSpacer2 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer2.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer2.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer2);

        yield return new WaitForSeconds(delay);

        // Next, the "System" text appears
        GameObject hackSystemMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSystemMessage.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSystemMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSystemMessage);
        // Assign Details
        hackSystemMessage.GetComponent<UIHackinfoV2>().Setup("System");

        // Add a spacer
        GameObject hackSpacer3 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer3.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer3.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer3);

        // -- System Name Printout --
        GameObject hackSystemName = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSystemName.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSystemName.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSystemName);
        // Assign Details
        hackSystemName.GetComponent<UIHackinfoV1>().Setup(terminal_targetTerm.specialName);

        // Add a spacer
        GameObject hackSpacer4 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer4.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer4.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer4);

        yield return new WaitForSeconds(delay);

        // Next, the "Status" text appears
        GameObject hackStatusMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackStatusMessage.transform.SetParent(terminal_hackinfoArea1.transform);
        hackStatusMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackStatusMessage);
        // Assign Details
        hackStatusMessage.GetComponent<UIHackinfoV2>().Setup("Status");

        // Add a spacer
        GameObject hackSpacer5 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer5.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer5.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer5);

        yield return new WaitForSeconds(delay);
        // -- If the player has any linked botnet terminals --
        if (PlayerData.inst.linkedTerminalBotnet > 0)
        {
            GameObject hackTerminalBotnet = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackTerminalBotnet.transform.SetParent(terminal_hackinfoArea1.transform);
            hackTerminalBotnet.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackTerminalBotnet);
            // Assign Details
            hackTerminalBotnet.GetComponent<UIHackinfoV1>().Setup("Linking terminal botnet (" + PlayerData.inst.linkedTerminalBotnet + ")...");
        }

        yield return new WaitForSeconds(delay);
        // -- If the player has any linked operators --
        if (PlayerData.inst.linkedOperators > 0)
        {
            GameObject hackOperatorBotnet = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackOperatorBotnet.transform.SetParent(terminal_hackinfoArea1.transform);
            hackOperatorBotnet.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackOperatorBotnet);
            // Assign Details
            hackOperatorBotnet.GetComponent<UIHackinfoV1>().Setup("Linking terminal botnet (" + PlayerData.inst.linkedOperators + ")...");
        }

        yield return new WaitForSeconds(delay);
        // -- "Scanning nodes..." --
        GameObject hackScanningNodes = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackScanningNodes.transform.SetParent(terminal_hackinfoArea1.transform);
        hackScanningNodes.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackScanningNodes);
        // Assign Details
        hackScanningNodes.GetComponent<UIHackinfoV1>().Setup("Scanning nodes...");

        yield return new WaitForSeconds(delay);
        // -- "Network defenses at 100%..." --
        GameObject hackNetworkDef = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackNetworkDef.transform.SetParent(terminal_hackinfoArea1.transform);
        hackNetworkDef.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackNetworkDef);
        // Assign Details
        hackNetworkDef.GetComponent<UIHackinfoV1>().Setup("Network defenses at 100%...");

        yield return new WaitForSeconds(delay);
        // -- "Building attack tree..." --
        GameObject hackAttackTree = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackAttackTree.transform.SetParent(terminal_hackinfoArea1.transform);
        hackAttackTree.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackAttackTree);
        // Assign Details
        hackAttackTree.GetComponent<UIHackinfoV1>().Setup("Building attack tree...");

        yield return new WaitForSeconds(delay);
        // -- "Bypassing authorization..." --
        GameObject hackBypassAuth = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackBypassAuth.transform.SetParent(terminal_hackinfoArea1.transform);
        hackBypassAuth.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackBypassAuth);
        // Assign Details
        hackBypassAuth.GetComponent<UIHackinfoV1>().Setup("Bypassing authorization...");

        // Add a spacer
        GameObject hackSpacer6 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer6.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer6.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer6);

        yield return new WaitForSeconds(delay);

        // --------
        // Here is where the other window opens
        // --------
        StartCoroutine(Terminal_OpenTargetResults());

        // -- Chance of Detection --
        GameObject hackDetChance = Instantiate(terminal_hackinfoV3_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackDetChance.transform.SetParent(terminal_hackinfoArea1.transform);
        hackDetChance.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackDetChance);
        // Assign Details
        hackDetChance.GetComponent<UIHackinfoV3>().Setup(terminal_targetTerm);

        // Add a spacer
        GameObject hackSpacer7 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer7.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer7.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer7);

        yield return new WaitForSeconds(delay);

        // Stop the typing sound
        AudioManager.inst.StopMiscSpecific();

        terminal_isAnimating = false;
    }

    private IEnumerator Terminal_HackBorderAnim()
    {
        float delay = 0.1f;

        Color usedColor = highlightGreen;

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);

        yield return new WaitForSeconds(delay);

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        terminal_hackinfoBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_hackinfoBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_hackinfoBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_hackinfoBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_hackTitleBacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
    }

    private IEnumerator Terminal_TargetResultBorderAnim()
    {
        float delay = 0.1f;

        Color usedColor = highlightGreen;

        float value = 0f;
        float headerValue = 0.25f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        value = 0.4f;
        headerValue = 0.75f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        value = 0.2f;
        headerValue = 1f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        value = 0.6f;
        headerValue = 0.75f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        value = 0.4f;
        headerValue = 0.25f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        value = 1f;
        headerValue = 0f;
        terminal_TARGETBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);
        terminal_RESULTSBorders.color = new Color(usedColor.r, usedColor.g, usedColor.b, value);

        terminal_TARGETheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
        terminal_RESULTSheaderbacker.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);
    }

    public void Terminal_InitTrace()
    {
        // We want to open up the trance progress
        // -- "Estimated Trace Progress" --
        GameObject hackEstTrace = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackEstTrace.transform.SetParent(terminal_hackinfoArea1.transform);
        hackEstTrace.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackEstTrace);
        // Assign Details
        hackEstTrace.GetComponent<UIHackinfoV1>().Setup("Estimated Trace Progress");

        // -- Trace Progress Bar --
        GameObject hackTrace = Instantiate(terminal_trace_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackTrace.transform.SetParent(terminal_hackinfoArea1.transform);
        hackTrace.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackTrace);
        // Assign Details
        hackTrace.GetComponent<UITraceBar>().Setup(terminal_targetTerm);

        // Add a spacer
        GameObject hackSpacer8 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer8.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer8.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer8);
    }

    public void Terminal_DoConsequences(Color setColor, string displayString, bool doSound = true, bool summonInvestigationSquad = true, bool forceExit = false)
    {
        StartCoroutine(Terminal_InitConsequences(setColor, displayString, doSound, summonInvestigationSquad, forceExit));
    }

    private IEnumerator Terminal_InitConsequences(Color setColor, string displayString, bool doSound = true, bool summonInvestigationSquad = true, bool forceExit = false)
    {
        // We want to init the consequences thingy
        GameObject hackLock = Instantiate(terminal_locked_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackLock.transform.SetParent(terminal_hackinfoArea1.transform);
        hackLock.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackLock);
        // Assign Details
        hackLock.GetComponent<UIHackLocked>().Setup(setColor, displayString, doSound);

        // Force disabled all hacking options
        foreach (var T in terminal_hackTargetsList.ToList())
        {
            T.GetComponent<UIHackTarget>().ForceDisabled();
        }

        // Do an animation over the hacking /TARGET/ window (this effect is greatly simplified since I don't know how to optimally achieve the true effect)
        // This also enables the static
        terminal_static.transform.parent.gameObject.SetActive(true);
        StartCoroutine(ImageTransparentAnim(terminal_static.GetComponent<Image>(), 1f, true));
        terminal_closeEffect.Close();

        // Create the looping static sound
        terminal_staticAudio = AudioManager.inst.MakeLoopingEffect(AudioManager.inst.dict_ui["HACK_STATIC"], 0.6f); // UI - HACK_STATIC

        if (summonInvestigationSquad)
        {
            string name = "";
            name = terminal_targetTerm.fullName;

            // Also consider secondary consequences
            HF.TerminalFailConsequence(name);

            string alertString = "ALERT: Suspicious activity at " + name + ". Dispatching Investigation squad.";
            GameManager.inst.DeploySquadTo("Investigation", terminal_targetTerm);
            UIManager.inst.CreateLeftMessage(alertString);
            UIManager.inst.CreateNewLogMessage(alertString, UIManager.inst.complexWhite, UIManager.inst.inactiveGray, false, true);

            // We now need to display this message in the terminal's results area aswell (without the header though).
            UIManager.inst.Terminal_CreateResult($"[{alertString}]", inactiveGray, "", false);
        }

        yield return null;

        if (forceExit)
        {
            StartCoroutine(Terminal_CloseAnim()); // Force close if asked to
        }
    }

    private IEnumerator Terminal_OpenTargetResults()
    {
        terminal_targetresultsAreaRef.SetActive(true); // Enable the window
        terminal_targetresultsAreaRef.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["OPEN_1"], 0.7f); // UI - OPEN_1
        StartCoroutine(Terminal_TargetResultBorderAnim()); // Do the opener animation

        yield return new WaitForSeconds(0.4f);

        // Generate the hacking target options
        List<TerminalCommand> commands = terminal_targetTerm.avaiableCommands;
        int secLvl = terminal_targetTerm.secLvl;

        // Create & Setup the pre-set options the player can choose (Targets)
        int i = 0;
        foreach (TerminalCommand command in commands)
        {
            GameObject targetCommand = Instantiate(terminal_hackoption_prefab, terminal_hackOptionsArea.transform.position, Quaternion.identity);
            targetCommand.transform.SetParent(terminal_hackOptionsArea.transform);
            targetCommand.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackTargetsList.Add(targetCommand);
            // Assign Details
            bool drawLine = false;
            if (i % 2 == 0)
                drawLine = true;

            // (Off topic) -- Calculate Chance of Success --
            float chance = 0f;
            if (secLvl == 0)
            {
                chance = 1f; // Open System
            }
            else
            {
                HackObject hack = command.hack;
                float baseChance = 1f;
                if (secLvl == 1) // We are using direct chance because indirect is done somewhere else
                {
                    baseChance = (float)((float)hack.directChance.x / 100f);
                }
                else if (secLvl == 2)
                {
                    baseChance = (float)((float)hack.directChance.y / 100f);
                }
                else if (secLvl == 3)
                {
                    baseChance = (float)((float)hack.directChance.z / 100f);
                }
                chance = HF.CalculateHackSuccessChance(baseChance);
            }

            targetCommand.GetComponent<UIHackTarget>().Setup(command, drawLine, chance, !command.available);
            i++;
            
        }

        // Lastly, initiate the manual command
        GameObject manualCommand = Instantiate(terminal_hackoption_prefab, terminal_hackOptionsArea.transform.position, Quaternion.identity);
        manualCommand.transform.SetParent(terminal_hackOptionsArea.transform);
        manualCommand.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackTargetsList.Add(manualCommand);
        // Assign Details
        bool drawLine2 = false;
        if (i % 2 == 0)
            drawLine2 = true;
        manualCommand.GetComponent<UIHackTarget>().SetupAsManualCommand(drawLine2);

        yield return null;

        // Start detail checking
        Terminal_ExtraDetail(true);
    }

    public void Terminal_RefreshHackingOptions()
    {
        // First we need to clear up all the old options
        foreach (var option in terminal_hackTargetsList.ToList())
        {
            if (option.GetComponent<UIHackTarget>())
            {
                option.GetComponent<UIHackTarget>().ShutDown();
            }
        }

        terminal_hackTargetsList.Clear();

        // Then we need to put in the new ones again
        #region New options
        // Generate the hacking target options
        List<TerminalCommand> commands = terminal_targetTerm.avaiableCommands;
        int secLvl = terminal_targetTerm.secLvl;

        int i = 0;
        foreach (TerminalCommand command in commands)
        {
            GameObject targetCommand = Instantiate(terminal_hackoption_prefab, terminal_hackOptionsArea.transform.position, Quaternion.identity);
            targetCommand.transform.SetParent(terminal_hackOptionsArea.transform);
            targetCommand.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackTargetsList.Add(targetCommand);
            // Assign Details
            bool drawLine = false;
            if (i % 2 == 0)
                drawLine = true;

            // (Off topic) -- Calculate Chance of Success --
            float chance = 0f;
            if (secLvl == 0)
            {
                chance = 1f; // Open System
            }
            else
            {
                HackObject hack = command.hack;
                float baseChance = 1f;
                if (secLvl == 1) // We are using direct chance because indirect is done somewhere else
                {
                    baseChance = (float)((float)hack.directChance.x / 100f);
                }
                else if (secLvl == 2)
                {
                    baseChance = (float)((float)hack.directChance.y / 100f);
                }
                else if (secLvl == 3)
                {
                    baseChance = (float)((float)hack.directChance.z / 100f);
                }
                chance = HF.CalculateHackSuccessChance(baseChance);
            }

            targetCommand.GetComponent<UIHackTarget>().Setup(command, drawLine, chance);
            i++;
        }

        // Lastly, initiate the manual command
        GameObject manualCommand = Instantiate(terminal_hackoption_prefab, terminal_hackOptionsArea.transform.position, Quaternion.identity);
        manualCommand.transform.SetParent(terminal_hackOptionsArea.transform);
        manualCommand.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackTargetsList.Add(manualCommand);
        // Assign Details
        bool drawLine2 = false;
        if (i % 2 == 0)
            drawLine2 = true;
        manualCommand.GetComponent<UIHackTarget>().SetupAsManualCommand(drawLine2);
        #endregion
    }

    public void Terminal_CreateResult(string text, Color setColor, string whiteText, bool tryDetection = false, float levelOfFailure = 0f)
    {
        if (whiteText != "")
        {
            GameObject header = Instantiate(terminal_hackResults_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
            header.transform.SetParent(terminal_hackResultsArea.transform);
            header.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackResultsList.Add(header);
            // Assign Details
            header.GetComponent<UIHackResults>().Setup(whiteText, header.GetComponent<UIHackResults>().headerWhite);
        }

        GameObject result = Instantiate(terminal_hackResults_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
        result.transform.SetParent(terminal_hackResultsArea.transform);
        result.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackResultsList.Add(result);
        // Assign Details
        result.GetComponent<UIHackResults>().Setup(text, setColor, true);

        if (tryDetection)
        {
            // Possibly increase detection chance
            HF.TraceHacking(UIManager.inst.terminal_targetTerm, levelOfFailure);
        }
    }

    public void Terminal_CreateManualInput()
    {
        if(terminal_activeIField != null) // Destroy it if a clone exists
        {
            Destroy(terminal_activeIField);
            terminal_activeIField = null;
        }

        // We want to insert the input field prefab and set the focus to that.
        terminal_activeIField = Instantiate(terminal_input_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
        terminal_activeIField.transform.SetParent(terminal_hackResultsArea.transform);
        terminal_activeIField.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Assign Details
        terminal_activeIField.GetComponent<UIHackInputfield>().Setup();
    }

    public void Terminal_InstantFinishResults()
    {
        foreach (var obj in terminal_hackResultsList)
        {
            obj.GetComponent<UIHackResults>().InstantFinish();
        }
    }

    public void Terminal_OpenCodes(float yValue)
    {
        codes_window.SetActive(true);
        
        Vector3[] v = new Vector3[4];
        codes_window.GetComponent<RectTransform>().GetWorldCorners(v);
        float currentY = v[1].y;
        float otherAdj = 10f;

        Vector3 pos = codes_window.transform.position;
        float adjustment = Mathf.Abs(currentY - yValue);
        if(yValue > currentY) // The input field is higher up than the codes window
        {
            pos = new Vector3(pos.x, pos.y + adjustment + otherAdj, pos.z);
            codes_window.transform.position = pos;
        }
        else // The input field is below the codes window
        {
            pos = new Vector3(pos.x, pos.y - adjustment + otherAdj, pos.z);
            codes_window.transform.position = pos;
        }

        Terminal_FillCodes();
    }

    private void Terminal_FillCodes()
    {
        int i = 0;
        foreach (var cCode in PlayerData.inst.customCodes)
        {
            GameObject code = Instantiate(codes_prefab, codes_setArea.transform.position, Quaternion.identity);
            code.transform.SetParent(codes_setArea.transform);
            code.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackCodesList.Add(code);
            // Assign Details
            code.GetComponent<UIHackCustomCode>().Setup(cCode, alphabet[i].ToString());
            //
            i++;
        }
    }

    public void Terminal_CloseCodes()
    {
        foreach (var i in terminal_hackCodesList.ToList())
        {
            if (i != null && i.GetComponent<UIHackCustomCode>())
            {
                i.GetComponent<UIHackCustomCode>().ShutDown();
            }
        }

        codes_window.GetComponent<UIHackCodes>().ShutDown();

        codes_window.SetActive(false);
    }

    [SerializeField] private int terminal_binaryLen = 1776; // Binary [48 digits per line, 37 lines total (1776)]
    private void Terminal_Binary()
    {
        var randomBinaryChars = new char[terminal_binaryLen];
        for (int i = 0; i < terminal_binaryLen; i++)
        {
            randomBinaryChars[i] = (Random.Range(0, 2) == 0) ? '0' : '1';
        }
        terminal_backingBinary.text = new string(randomBinaryChars);
    }

    /// <summary>
    /// A generic way of closing terminals, will try to close either the normal OR custom terminal window.
    /// </summary>
    public void Terminal_CloseAny()
    {
        if (terminal_targetTerm.GetComponent<TerminalCustom>())
        {
            CTerminal_Close(); // Custom Terminal
        }
        else
        {
            Terminal_Close(); // Normal Machine
        }
    }

    public void Terminal_Close()
    {
        if (terminal_isAnimating) // Safety flag
        {
            return;
        }

        AudioManager.inst.CreateTempClip(terminal_targetTerm.transform.position, AudioManager.inst.dict_ui["CLOSE"]); // UI - CLOSE

        StartCoroutine(Terminal_CloseAnim());
    }

    private IEnumerator Terminal_CloseAnim()
    {
        // Stop detail checking
        Terminal_ExtraDetail(false);

        // Null out main reference
        terminal_targetTerm = null;

        // Do a fade out animation
        #region Image Fade-out
        Image[] i1 = terminal_hackingAreaRef.GetComponentsInChildren<Image>();
        Image[] i2 = terminal_targetresultsAreaRef.GetComponentsInChildren<Image>();
        Image[] i3 = terminal_hackinfoArea1.GetComponentsInChildren<Image>();

        var i12 = i1.Concat(i2).ToArray();
        var iFinal = i12.Concat(i3).ToArray();
        float delay = 0.1f;

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 0.8f);
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 0.6f);
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 0.4f);
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 0.2f);
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 0f);
        }
        #endregion

        yield return null;

        // Stop static effects
        terminal_closeEffect.ResetEffect();
        if (terminal_staticAudio != null)
        {
            Destroy(terminal_staticAudio);
        }

        // Shut down all the lines
        foreach (var i in terminal_hackinfoList.ToList())
        {
            if (i.GetComponent<UIHackinfoV1>())
            {
                i.GetComponent<UIHackinfoV1>().ShutDown();
            }
            else if (i.GetComponent<UIHackinfoV2>())
            {
                i.GetComponent<UIHackinfoV2>().ShutDown();
            }
            else if (i.GetComponent<UIHackinfoV3>())
            {
                i.GetComponent<UIHackinfoV3>().ShutDown();
            }
            else if (i.GetComponent<UITraceBar>())
            {
                i.GetComponent<UITraceBar>().ShutDown();
            }
            else if (i.GetComponent<UIHackLocked>())
            {
                i.GetComponent<UIHackLocked>().ShutDown();
            }
        }

        foreach(var i in terminal_hackTargetsList.ToList())
        {
            if (i.GetComponent<UIHackTarget>())
            {
                i.GetComponent<UIHackTarget>().ShutDown();
            }
        }

        foreach (var i in terminal_hackResultsList.ToList())
        {
            if (i.GetComponent<UIHackResults>())
            {
                i.GetComponent<UIHackResults>().ShutDown();
            }
        }

        foreach (var i in terminal_hackCodesList.ToList())
        {
            if (i != null && i.GetComponent<UIHackCustomCode>())
            {
                i.GetComponent<UIHackCustomCode>().ShutDown();
            }
        }

        codes_window.GetComponent<UIHackCodes>().ShutDown();
        

        yield return null;

        terminal_static.transform.parent.gameObject.SetActive(false);

        terminal_hackinfoList.Clear(); // Clear the list
        terminal_hackTargetsList.Clear();
        terminal_hackResultsList.Clear();
        terminal_hackCodesList.Clear();
        // Most of the prefabs will delete themselves

        // The spacers however do not, so this is a fallback.
        foreach (Transform child in terminal_hackinfoArea1.transform)
        {
            Destroy(child.gameObject);
        }

        // Close window
        terminal_hackingAreaRef.SetActive(false);
        terminal_targetresultsAreaRef.SetActive(false);
        codes_window.SetActive(false);

        // Un-Freeze the player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;

        // Change interfacing mode
        PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.COMBAT);

    }

    private IEnumerator ImageTransparentAnim(Image target, float duration, bool fadeIn)
    {
        float startValue, endValue;

        if (fadeIn)
        {
            startValue = 0f;
            endValue = 1f;
        }
        else
        {
            startValue = 1f;
            endValue = 0f;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration) // Green -> Black
        {
            target.color = new Color(target.color.r, target.color.g, target.color.b, Mathf.Lerp(startValue, endValue, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        target.color = new Color(target.color.r, target.color.g, target.color.b, endValue);
    }

    /// <summary>
    /// Handles logic for the ExtraDetail tooltip when hovering over hacking options.
    /// </summary>
    private void Terminal_ExtraDetail(bool check)
    {
        if (check)
        {
            // We need to start the checking loop.
            if(terminal_extradetail_co != null)
            {
                StopCoroutine(terminal_extradetail_co);
            }

            terminal_extradetail_co = StartCoroutine(Terminal_ExtraDetailLoop());
        }
        else
        {
            // We need to stop the checking loop.
            StopCoroutine(terminal_extradetail_co);
            terminal_extradetail_co = null;
        }
    }

    private Coroutine terminal_extradetail_co = null;
    private string terminal_extradetail_string = "";
    [Tooltip("The hacking option the mouse is currently hovering over. Updated via function.")]
    private GameObject terminal_hoveredChoice = null;
    public void Terminal_UpdateHoveredChoice(GameObject option, bool enteredBounds)
    {
        if (enteredBounds) // If this option has just been entered, it is now the new hovered choice.
        {
            terminal_hoveredChoice = option;
        }
        else // If this option has just been exited, and nothing new has been set to replace it, it needs to be null.
        {
            if(terminal_hoveredChoice == option)
            {
                terminal_hoveredChoice = null;
            }
        }
    }

    /// <summary>
    /// Called upon the Terminal menu being over and the player right clicking.
    /// </summary>
    public void Terminal_TryExtraDetail()
    {
        // The user has just right clicked. Should we do anything?
        // - Is the detail menu closed?
        if (!dataMenu.data_extraDetail.gameObject.activeInHierarchy)
        {
            // It's not so lets try and open it

            // - Are we over a viable command option?
            if(terminal_hoveredChoice != null && terminal_extradetail_string != "")
            {
                // We are, open the menu and display the string
                dataMenu.data_extraDetail.SetActive(true);
                dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().ShowExtraDetail(terminal_extradetail_string);
                dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().myGameObject = terminal_hoveredChoice;
            }
        }
    }

    private IEnumerator Terminal_ExtraDetailLoop()
    {
        while (terminal_targetresultsAreaRef.gameObject.activeInHierarchy) // Pretty much a forever loop but this is just here for safety
        {
            // Is the player's mouse currently over a hacking option?
            if(terminal_hoveredChoice != null)
            {
                // It is! Lets get the display string for that command.
                string display = "";
                UIHackTarget hack = terminal_hoveredChoice.GetComponent<UIHackTarget>();
                TerminalCommand comm = terminal_hoveredChoice.GetComponent<UIHackTarget>().command;
                if (hack.isManualCommand)
                { // - If it's the manual command we show something different
                    display = "Enter special manual hacking codes prefixed by \\\\, or manually enter any known trojans, brute force hacks, or regular hacks." +
                        " Start typing and press Spacebar or Tab to complete the current text, or Up/Down to select from among Multiple autocomplete matches." +
                        " Up/Down arrows on an empty line cycler through past manual commands.";
                }
                else
                {
                    display = comm.hack.description;
                }

                // -- MAIN LOGIC --
                if (terminal_extradetail_string == "") // Have we got a set string yet?
                {
                    // No? Set it
                    terminal_extradetail_string = display;
                }
                else if(display == terminal_extradetail_string)
                {
                    // We already have this string set.
                    // Just to be safe, check and see if the gameObjects are different
                    if(dataMenu.data_extraDetail.gameObject.activeInHierarchy && dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().myGameObject != terminal_hoveredChoice)
                    {
                        // They are different, we need to close the window.
                        dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().HideExtraDetail();
                        terminal_extradetail_string = "";
                    }
                }
                else
                {
                    // It's a different one, we should close the window.
                    dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().HideExtraDetail();
                    terminal_extradetail_string = "";
                }

                yield return null;
            }
            else // Not hovering over anything? Hide the detail
            {
                dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().HideExtraDetail();
            }

            yield return null;
        }
    }

    #endregion

    #region Custom Terminal
    [HideInInspector] public TerminalCustom cTerminal_machine;
    [Header("Custom Terminal")]
    public GameObject cTerminal_gibberishPrefab;
    public bool cTerminal_animating = false;

    public void CTerminal_Open(InteractableMachine target)
    {
        cTerminal_animating = true;

        // Freeze player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;

        // Set target
        cTerminal_machine = target.GetComponent<TerminalCustom>();
        terminal_targetTerm = target;

        // If this is the Hideout Cache, we need to change the / PARTS / header to / CACHE /
        if(cTerminal_machine.customType == CustomTerminalType.HideoutCache)
        {
            partsHeaderText.text = "/CACHE/";
            PartsFlashHeader(); // Flash it too so the player can recognize the change

            // Force a UI update so the inventory is displayed on the right side
            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateParts();
            InventoryControl.inst.UpdateInterfaceInventories();
        }

        // Binary (Background)
        InvokeRepeating("Terminal_Binary", 0f, 1f);

        // Assign name + get Sec Level / Restricted access
        int secLvl = 0;
        bool restrictedAccess = true;

        string cTerminalNameSpacer = " "; // Needed due to text alignment
        terminal_name.text = cTerminalNameSpacer + cTerminal_machine.fullName;
        secLvl = cTerminal_machine.secLvl;
        restrictedAccess = cTerminal_machine.restrictedAccess;


        // Restricted access? (In most cases no)
        if (restrictedAccess)
        {
            terminal_name.text += " - Restricted Access";
        }
        else
        {
            terminal_name.text += " - Unrestricted Access";
        }

        // Security level + color
        switch (secLvl)
        {
            case 0: // OPEN SYSTEM
                terminal_secLvl_backing.color = highlightGreen;
                terminal_secLvl.text = "OPEN SYSTEM";
                break;
            case 1: // SECURITY LEVEL #
                terminal_secLvl_backing.color = warmYellow;
                terminal_secLvl.text = "SECURITY LEVEL 1";
                break;
            case 2:
                terminal_secLvl_backing.color = warningOrange;
                terminal_secLvl.text = "SECURITY LEVEL 2";
                break;
            case 3:
                terminal_secLvl_backing.color = highSecRed;
                terminal_secLvl.text = "SECURITY LEVEL 3";
                break;
        }


        // //
        // Now That General Setup is done, we do the animation / reveal part
        // //

        StartCoroutine(CTerminal_OpenAnim());
    }

    private IEnumerator CTerminal_OpenAnim()
    {
        float delay = 0.05f;

        // In-case this menu is being re-opened, we need to make all the images un-transparent again
        #region Image Transparency Reset
        Image[] i1 = terminal_hackingAreaRef.GetComponentsInChildren<Image>();
        Image[] i2 = terminal_targetresultsAreaRef.GetComponentsInChildren<Image>();
        Image[] i3 = terminal_hackinfoArea1.GetComponentsInChildren<Image>();

        var i12 = i1.Concat(i2).ToArray();
        var iFinal = i12.Concat(i3).ToArray();

        foreach (Image I in iFinal)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 1f);
        }
        #endregion

        // First, the hacking window opens
        terminal_hackingAreaRef.SetActive(true);
        StartCoroutine(Terminal_HackBorderAnim());

        // Play (typing) sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["PRINT_2"]); // UI - PRINT_2

        if (cTerminal_machine.restrictedAccess)
        {
            // Next, the "Utilities" text appears at the top of the hacking window
            GameObject hackUtilitiesMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackUtilitiesMessage.transform.SetParent(terminal_hackinfoArea1.transform);
            hackUtilitiesMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackUtilitiesMessage);
            // Assign Details
            hackUtilitiesMessage.GetComponent<UIHackinfoV2>().Setup("Utilities");

            // Add a spacer
            GameObject hackSpacer = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackSpacer.transform.SetParent(terminal_hackinfoArea1.transform);
            hackSpacer.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackSpacer);

            // Shortly after
            yield return new WaitForSeconds(delay);

            // The hacking related parts (if the player has any) will appear
            // -Get the parts
            List<ItemObject> hackware = new List<ItemObject>();
            bool hasHackware = false;
            (hasHackware, hackware) = Action.FindPlayerHackware();
            if (hasHackware) // Display the hackware
            {
                foreach (ItemObject item in hackware)
                {
                    GameObject hackinfoMessage = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
                    hackinfoMessage.transform.SetParent(terminal_hackinfoArea1.transform);
                    hackinfoMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    // Add it to list
                    terminal_hackinfoList.Add(hackinfoMessage);
                    // Assign Details
                    hackinfoMessage.GetComponent<UIHackinfoV1>().Setup(item.itemName, item);
                }
            }
            else // Type out "(None)"
            {
                GameObject hackinfoMessage = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
                hackinfoMessage.transform.SetParent(terminal_hackinfoArea1.transform);
                hackinfoMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                terminal_hackinfoList.Add(hackinfoMessage);
                // Assign Details
                hackinfoMessage.GetComponent<UIHackinfoV1>().Setup("(NONE)");
            }

            // Add a spacer
            GameObject hackSpacer2 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
            hackSpacer2.transform.SetParent(terminal_hackinfoArea1.transform);
            hackSpacer2.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Add it to list
            terminal_hackinfoList.Add(hackSpacer2);

            yield return new WaitForSeconds(delay);
        }

        // Next, the "System" text appears
        GameObject hackSystemMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSystemMessage.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSystemMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSystemMessage);
        // Assign Details
        hackSystemMessage.GetComponent<UIHackinfoV2>().Setup("System");

        // Add a spacer
        GameObject hackSpacer3 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer3.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer3.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer3);

        // -- System Name Printout --
        GameObject hackSystemName = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSystemName.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSystemName.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSystemName);
        // Assign Details
        hackSystemName.GetComponent<UIHackinfoV1>().Setup(terminal_targetTerm.specialName);

        // Add a spacer
        GameObject hackSpacer4 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer4.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer4.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer4);

        yield return new WaitForSeconds(delay);

        // Next, the "Status" text appears
        GameObject hackStatusMessage = Instantiate(terminal_hackinfoV2_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackStatusMessage.transform.SetParent(terminal_hackinfoArea1.transform);
        hackStatusMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackStatusMessage);
        // Assign Details
        hackStatusMessage.GetComponent<UIHackinfoV2>().Setup("Status");

        // Add a spacer
        GameObject hackSpacer5 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer5.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer5.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer5);

        yield return new WaitForSeconds(delay);
        // -- "Accessing entry node" --
        GameObject hackScanningNodes = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackScanningNodes.transform.SetParent(terminal_hackinfoArea1.transform);
        hackScanningNodes.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackScanningNodes);
        // Assign Details
        hackScanningNodes.GetComponent<UIHackinfoV1>().Setup("Accessing entry node");

        // Add a spacer
        GameObject hackSpacer6 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer6.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer6.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer6);

        yield return new WaitForSeconds(delay);


        // -- Now the random gibberish and highlight
        GameObject hackGibb = Instantiate(cTerminal_gibberishPrefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackGibb.transform.SetParent(terminal_hackinfoArea1.transform);
        hackGibb.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackGibb);

        // Add a spacer
        GameObject hackSpacer7 = Instantiate(terminal_hackinfoSpacer_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackSpacer7.transform.SetParent(terminal_hackinfoArea1.transform);
        hackSpacer7.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackSpacer7);

        yield return new WaitForSeconds(delay);

        // -- "Connection established..." --
        GameObject hackAttackTree = Instantiate(terminal_hackinfoV1_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackAttackTree.transform.SetParent(terminal_hackinfoArea1.transform);
        hackAttackTree.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackAttackTree);
        // Assign Details
        hackAttackTree.GetComponent<UIHackinfoV1>().Setup("Connection established");

        // --------
        // Here is where the other window opens
        // --------
        StartCoroutine(Terminal_OpenTargetResults());


        // Stop the typing sound
        AudioManager.inst.StopMiscSpecific();

        yield return null;

        cTerminal_animating = false;
    }

    public void CTerminal_Close()
    {
        if (cTerminal_animating) // Safety flag
        {
            return;
        }

        AudioManager.inst.CreateTempClip(terminal_targetTerm.transform.position, AudioManager.inst.dict_ui["CLOSE"]); // UI - CLOSE

        // Did we just close the cache window?
        bool wasCache = (cTerminal_machine.customType == CustomTerminalType.HideoutCache);

        // Un-assign target
        terminal_targetTerm = null;
        cTerminal_machine = null;

        // Change back the header
        if (wasCache)
        {
            partsHeaderText.text = "/PARTS/";
            PartsFlashHeader(); // Flash it too so the player can recognize the change

            // Force a UI update so the inventory is set to normal again
            UIManager.inst.UpdatePSUI();
            UIManager.inst.UpdateParts();
            InventoryControl.inst.UpdateInterfaceInventories();

            InventoryControl.inst.HideoutSortingCheck();
        }

        StartCoroutine(Terminal_CloseAnim()); // - Both use the same resources so its safe to do this
    }

    #endregion

    #region Schematics

    [Header("Schematics")]
    public List<GameObject> schematics_objects = new List<GameObject>();
    public GameObject schematics_prefab;
    public GameObject schematics_parent;
    public GameObject schematics_contentArea;
    [SerializeField] private GameObject schematics_closer;
    [SerializeField] private Image schematics_backer;

    public void Schematics_Open()
    {
        // Enable the menu
        schematics_parent.SetActive(true);

        // Populate it with know schematics (items then bots)
        List<ItemObject> knownItems = HF.GetKnownItemSchematics();
        List<BotObject> knownBots = HF.GetKnownBotSchematics();

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        alphabet = alpha.ToList(); // Fill alphabet list

        float delay = 0f;
        float delaySpacing = 0.08f;

        foreach (ItemObject item in knownItems)
        {
            if(schematics_objects.Count < 25) // Don't go over 26, we don't have that many letters
            {
                // Instantiate it & Assign it to the area
                GameObject schematicOption = Instantiate(schematics_prefab, schematics_contentArea.transform.position, Quaternion.identity);
                schematicOption.transform.SetParent(schematics_contentArea.transform);
                schematicOption.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                schematics_objects.Add(schematicOption);
                // Assign Details
                schematicOption.GetComponent<UISchematicOption>().Init(alphabet[0].ToString().ToLower(), delay += delaySpacing, item);

                alphabet.Remove(alphabet[0]);
            }
        }

        foreach (BotObject bot in knownBots)
        {
            if (schematics_objects.Count < 25) // Don't go over 26, we don't have that many letters
            {
                // Instantiate it & Assign it to the area
                GameObject schematicOption = Instantiate(schematics_prefab, schematics_contentArea.transform.position, Quaternion.identity);
                schematicOption.transform.SetParent(schematics_contentArea.transform);
                schematicOption.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                schematics_objects.Add(schematicOption);
                // Assign Details
                schematicOption.GetComponent<UISchematicOption>().Init(alphabet[0].ToString().ToLower(), delay += delaySpacing, null, bot);

                alphabet.Remove(alphabet[0]);
            }
        }

        // Play the animation
        StartCoroutine(Schematics_OpenAnim());
    }

    private IEnumerator Schematics_OpenAnim()
    {
        // Animate the header box
        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration) // Green -> Black
        {
            schematics_backer.color = Color.Lerp(highlightGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        schematics_backer.color = Color.black;

        // Update the X's position
        schematics_closer.GetComponent<UIRectAligner>().AlignToBottomRight();
    }

    public void Schematics_Close()
    {
        // Remove all the options
        foreach (GameObject prefab in schematics_objects.ToList())
        {
            Destroy(prefab);
        }
        schematics_objects.Clear();

        // Disable the menu
        schematics_parent.SetActive(false);
    }

    #endregion

    [Header("Bar Collection")]
    [Tooltip("Collection of all bar images that make up the UI boxes.")]
    public List<Image> boxBars = new List<Image>();

    #region Log Messages

    [Header("Log Area")]
    // - Log -
    //
    public GameObject logMessage_prefab;
    public GameObject logTextArea;
    public List<string> logFull = new List<string>(); // The full message log
    public List<GameObject> logMessages = new List<GameObject>(); // All of the GameObjects
    //
    // -     -

    /// <summary>
    /// Creates a message in the top left "Log" section
    /// </summary>
    /// <param name="newMessage">The message to display (will be edited)</param>
    /// <param name="desiredColor">The color this message should be.</param>
    /// <param name="noTime">If a timestamp should be added to this message</param>
    public void CreateNewLogMessage(string newMessage, Color desiredColor, Color desiredHighlight, bool noTime = false, bool hasAudio = false)
    {
        // First determine the TIME of when this message happened
        int currentTime = TurnManager.inst.globalTime;
        int digitCount = currentTime.ToString().Length;

        string displayTime = "";

        switch (digitCount) // 5 digits max, convert it to a string
        {
            case 1: // 1 Digit
                displayTime = "0000";
                displayTime += currentTime.ToString();
                break;

            case 2: // 2 Digits
                displayTime = "000";
                displayTime += currentTime.ToString();
                break;

            case 3: // 3 Digits
                displayTime = "00";
                displayTime += currentTime.ToString();
                break;

            case 4: // 4 Digits
                displayTime = "0";
                displayTime += currentTime.ToString();
                break;

            case 5: // 5 Digits
                displayTime = currentTime.ToString();
                break;
        }

        displayTime += "_ "; // Add an "_ " at the end

        string finalMessage = "";
        if (!noTime)
        {
            finalMessage = displayTime += newMessage; // Combine for final message
        }
        else
        {
            finalMessage = newMessage;
        }

        // -- Now Actually Create the Message --
        // Instantiate it & Assign it to left area
        GameObject newLogMessage = Instantiate(logMessage_prefab, logTextArea.transform.position, Quaternion.identity);
        newLogMessage.transform.SetParent(logTextArea.transform);
        newLogMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        logMessages.Add(newLogMessage);
        logFull.Add(finalMessage);
        // Assign Details
        newLogMessage.GetComponent<UILogMessage>().Setup(finalMessage, desiredColor, desiredHighlight, hasAudio);

    }


#endregion

    #region LAIC Data Variables

    [Header("L / A / I / C")]
    // - L \ A \ I \ C -
    //
    public GameObject laicArea;
    //
    public TextMeshProUGUI L_text;
    public Button L_button;
    public TextMeshProUGUI A_text;
    public Button A_button;
    public TextMeshProUGUI I_text;
    public Button I_button;
    public TextMeshProUGUI C_text;
    public Button C_button;
    //
    public TextMeshProUGUI topText_LAIC;
    //
    public string currentActiveMenu_LAIC; // [ L / A / I / C ]
    //
    // -              -

    #endregion

    #region RightSide Variables

    [Header("Top Right Data")]
    [Header("Core")]
    // - Core -
    //
    public Image coreBarMain;
    public Image coreBarSub;
    //
    public TextMeshProUGUI coreAmount_text; // ###/### (##% exposed)
    //
    public GameObject warningCRef;
    public Image warningCImage;
    public TextMeshProUGUI warningCText;
    //
    public Color warningYellow;
    public Color alertRed;
    //
    public Slider core_slider;
    public Slider coreD_slider;
    //
    // -     -
    [Header("Energy")]
    // - Energy -
    //
    public Image energyBarMain;
    public Image energyBarSub;
    //
    public TextMeshProUGUI energyAmount_text;
    //
    public GameObject warningERef;
    public Image warningEImage;
    public TextMeshProUGUI warningEText;
    //
    public Slider energy_slider;
    public Slider energyD_slider;
    //
    // -       -
    [Header("Matter")]
    // - Matter -
    //
    public Image matterBarMain;
    public Image matterBarSub;
    //
    public TextMeshProUGUI matterAmount_text;
    //
    public GameObject warningMRef;
    public Image warningMImage;
    public TextMeshProUGUI warningMText;
    //
    public Slider matter_slider;
    public Slider matterD_slider;
    //
    // -       -
    [Header("Corruption")]
    // - Corruption -
    //
    public GameObject corruptionBar;
    //
    public TextMeshProUGUI corruptionAmountText;
    //
    public Slider corruption_slider;
    public Slider corruptionD_slider;
    //
    // -            -
    [Header("Heat")]
    // - Heat -
    //
    public GameObject heaDiffGO;
    public Image heaDiffImage;
    public TextMeshProUGUI heaDiffText;
    //
    public TextMeshProUGUI currentHeatText;
    public Image heaWarningBacker;
    // [0 to 100 -> Cool] [100 to 200 -> Warm] [200 to 300 -> Hot] [300 to 400 -> Warning] [400+ -> Critical]
    public Color coolBlue;
    public Color warmYellow;
    public Color hotOrange;
    public Color warningOrange;
    //
    // -      -
    [Header("Movement")]
    // - Movement -
    //
    public GameObject flashRef;
    public Image flashImage;
    //
    public TextMeshProUGUI moveTypeText;
    //
    public GameObject modeGO;
    public Image modeImage;
    public TextMeshProUGUI modeText;
    //
    public TextMeshProUGUI moveNumberText; //: (###)
    public bool usingSpecialMovement = false;
    //
    public Color slowOrange;
    public Color superSlowOrange;
    //
    // -          -
    [Header("Time/Location")]
    // - Time / Location -
    //
    public TextMeshProUGUI timeText; // Aka the turn timer
    //
    public TextMeshProUGUI clock_text; //: ##:##
    //
    public TextMeshProUGUI run_text; //: ##:##
    //
    public TextMeshProUGUI locationText; //: -#/???
    [SerializeField] private GameObject locationBoxWarning;
    [SerializeField] private TextMeshProUGUI locationBoxWarning_text;
    [SerializeField] private Image locationBoxWarning_backer;
    //
    public GameObject influenceGO;
    public TextMeshProUGUI influence_text; //: ### #-#
    //
    // -                 -

    [Header("SCAN")]
    // - SCAN - 
    //
    public TextMeshProUGUI scanHeader_text; //: / S C A N /
    public GameObject scanButtonsParent; // Parent for the 4 buttons in the scan box
    public List<TextMeshProUGUI> scanButtonText = new List<TextMeshProUGUI>();
    // - Scan subinfo components
    [Tooltip("The box health indicator")] public Image scanSubImage;
    [Tooltip("The background images that highlight during animations.")] public List<Image> scanSubBackerImages = new List<Image>();
    [Tooltip("The upper text")] public TextMeshProUGUI scanSubTextA;
    [Tooltip("The lower text")] public TextMeshProUGUI scanSubTextB;
    [Tooltip("The red !")] public TextMeshProUGUI scanSubDangerNotify;
    public GameObject scanSubParent;
    //
    // -      -

    [Header("EVASION")]
    // - EVASION -
    //
    public TextMeshProUGUI evasionHeader_text; //: / E V A S I O N /
    //
    public Image avoidanceIndicator_image; // the square
    public TextMeshProUGUI avoidanceNum_text; //: ##%
    public TextMeshProUGUI avoidanceDetail1_text; // The five
    public TextMeshProUGUI avoidanceDetail2_text; // numbers
    public TextMeshProUGUI avoidanceDetail3_text; // along
    public TextMeshProUGUI avoidanceDetail4_text; // the
    public TextMeshProUGUI avoidanceDetail5_text; // bottom
    //
    // -         -

    [Header("Parts")]
    // - Parts -
    //
    public Image borderT;
    public Image borderB;
    public Image borderL;
    public Image borderR;
    //
    public TextMeshProUGUI partsHeaderText;
    //
    public TextMeshProUGUI weightText;
    //
    public GameObject botTypeGO;
    public Image botTypeImage;
    public TextMeshProUGUI botTypeText;
    public bool useBotType = false;
    //
    public GameObject miscNumGO;
    public Image miscNumImage;
    [Tooltip("Overencumberance")]
    public TextMeshProUGUI miscNumText; // # x #
    //
    public GameObject partContentArea;
    //
    // - CEWQ -
    //
    public TextMeshProUGUI cText;
    public TextMeshProUGUI eText;
    public TextMeshProUGUI wText;
    public TextMeshProUGUI qText;
    public GameObject cewq_main;
    public TextMeshProUGUI cewq_text;
    [Tooltip("0 = COVERAGE | 1 = ENERGY | 2 = INTEGRITY | 3 = INFO")]
    public int cewq_mode = 0;
    //
    // - TMI -
    //
    public TextMeshProUGUI tText;
    public TextMeshProUGUI mText;
    public TextMeshProUGUI iText;
    //
    // - Overflow -
    //
    public TextMeshProUGUI overFlowTop;
    public TextMeshProUGUI overFlowBottom;
    //
    // - Map / Esc? -
    //
    public TextMeshProUGUI mapText;
    public TextMeshProUGUI escapeText;
    //
    // - Refs To Spawned Inv Things -
    //
    public GameObject instPower;
    public GameObject instPropulsion;
    public GameObject instUtilities;
    public GameObject instWeapons;
    //
    public List<GameObject> instParts = new List<GameObject>();
    public List<char> alphabet = new List<char>();
    // -       -

    [Header("  Inventory")]
    // - Inventory -
    //
    public TextMeshProUGUI inventorySize_text;
    public GameObject inventoryArea;
    //
    public List<GameObject> instInv = new List<GameObject>();
    //
    // -           -

    #endregion

    #region RightSide UI

    public void FirstTimeStartup()
    {
        if (GlobalSettings.inst.showNewLevelAnimation)
        {
            FreshStart_BeginAnimate();
        }
        else
        {
            FSA_LOG.enabled = false;
            FSA_LAIC.enabled = false;
            FSA_TopRight.enabled = false;
            FSA_ScanEvasion.enabled = false;
            FSA_Parts.enabled = false;
            FSA_Inventory.enabled = false;
        }

        InitDefaultPartsMenu(); // Set the parts menu (empty)
    }

    [SerializeField] private List<GameObject> initAnimObjects = new List<GameObject>();
    public void InitDefaultPartsMenu()
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        alphabet = alpha.ToList(); // Fill alphabet list

        AssignPartKeys(); // Assign each slot keybinds
    }

    #region Rightside - Player Stats UI
    /// <summary>
    /// Update player stats UI (top right)
    /// </summary>
    public void UpdatePSUI()
    {
        // ---------- CORE ------------

        // ####/####
        coreAmount_text.text = PlayerData.inst.currentHealth.ToString() + "/" + PlayerData.inst.maxHealth.ToString();
        // now append the exposure amount (this part is a different color)
        // (##% exposed)
        string exposure_text = "<color=#009700>" + " (" + PlayerData.inst.currentCoreExposure.ToString() + "% exposed)" + "</color>";
        coreAmount_text.text += exposure_text;

        // ---------- ENERGY ------------

        // ###/###
        energyAmount_text.text = PlayerData.inst.currentEnergy.ToString() + "/" + PlayerData.inst.maxEnergy.ToString() + " ";
        // now expand the other details

        // - Energy Main - //
        energyAmount_text.text += "<color=#00ABE3>" + "(" + "</color>";

        float energy1 = Mathf.Round(PlayerData.inst.energyOutput1 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.energyOutput1 < 0)
        {
            // Negative energy, so red
            energyAmount_text.text += "<color=#B20000>" + energy1.ToString() + "</color>" + " ";
        }
        else
        {
            // Positive energy, so blue
            energyAmount_text.text += "<color=#00ABE3>" + "+" + energy1.ToString() + "</color>" + " ";
        }

        // - Energy Secondary - //
        float energy2 = Mathf.Round(PlayerData.inst.energyOutput2 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.energyOutput2 < 0)
        {
            // Negative energy, so red
            energyAmount_text.text += "<color=#B20000>" + energy2.ToString() + "</color>";
        }
        else
        {
            // Positive energy, so blue
            energyAmount_text.text += "<color=#00ABE3>" + "+" + energy2.ToString() + "</color>";
        }
        energyAmount_text.text += "<color=#00ABE3>" + ")" + "</color>";

        // - Internal Energy - //

        if (PlayerData.inst.maxInternalEnergy > 0)
        {
            energyAmount_text.text += "<color=#00ABE3>" + " I:" + PlayerData.inst.currentInternalEnergy.ToString() + "</color>";
        }

        // ---------- MATTER ------------

        matterAmount_text.text = PlayerData.inst.currentMatter.ToString() + "/" + PlayerData.inst.maxMatter.ToString();
        if (PlayerData.inst.maxInternalMatter > 0)
        {
            matterAmount_text.text += " I:" + PlayerData.inst.currentInternalMatter.ToString();
        }

        // ---------- CORRUPTION ------------

        corruptionAmountText.text = PlayerData.inst.currentCorruption + "%";
        if (PlayerData.inst.rif_immuneToCorruption) // Player may be immune to the effects of corruption
        {
            corruptionAmountText.text += " (Immune)";
        }

        // ---------- HEAT ------------
        // [0 to 100 -> Cool] [100 to 200 -> Warm] [200 to 300 -> Hot] [300 to 400 -> Warning] [400+ -> Critical]
        if (PlayerData.inst.currentHeat < 100) // Cool
        {
            heaDiffText.text = "Cool";
            heaDiffImage.color = coolBlue;
        }
        else if (PlayerData.inst.currentHeat >= 100 && PlayerData.inst.currentHeat < 200) // Warm
        {
            heaDiffText.text = "Warm";
            heaDiffImage.color = warmYellow;
        }
        else if(PlayerData.inst.currentHeat >= 200 && PlayerData.inst.currentHeat < 300) // Hot
        {
            heaDiffText.text = "Hot";
            heaDiffImage.color = hotOrange;
        }
        else if (PlayerData.inst.currentHeat >= 300 && PlayerData.inst.currentHeat < 400) // Warning
        {
            heaDiffText.text = "Warning";
            heaDiffImage.color = warningOrange;
        }
        else // Critical
        {
            heaDiffText.text = "Critical";
            heaDiffImage.color = alertRed;
        }

        // Just the heat number
        int playerHeat = PlayerData.inst.currentHeat;
        // If the heat is negative, than it is from some item doing it (reducing the base), so we want to show the text in a different color to let the player know
        currentHeatText.text = "";
        if(playerHeat < 0)
        {
            currentHeatText.text = "<color=#A9B628>" + playerHeat.ToString() + "</color>" + " ";
        }
        else
        {
            currentHeatText.text = playerHeat.ToString() + " ";
        }

        // The two secondary numbers
        currentHeatText.text += "(";

        float heat1 = Mathf.Round(PlayerData.inst.heatRate1 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.heatRate1 <= 0) // Negative (Green)
        {
            currentHeatText.text += "<color=#009700>" + heat1.ToString() + "</color>" + " ";
        }
        else // Positive (Red)
        {
            currentHeatText.text += "<color=#B20000>" + "+" + heat1.ToString() + "</color>" + " ";
        }
        float heat2 = Mathf.Round(PlayerData.inst.heatRate2 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.heatRate2 <= 0) // Negative (Green)
        {
            currentHeatText.text += "<color=#009700>" + heat2.ToString() + "</color>";
        }
        else // Positive (Red)
        {
            currentHeatText.text += "<color=#B20000>" + "+" + heat2.ToString() + "</color>";
        }
        if(PlayerData.inst.heatRate3 != 0) // This is not usually shown unless the player has the neccessary items
        {
            float heat3 = Mathf.Round(PlayerData.inst.heatRate3 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
            if (PlayerData.inst.heatRate3 <= 0) // Negative (Green)
            {
                currentHeatText.text += "<color=#009700>" + heat3.ToString() + "</color>";
            }
            else // Positive (Red)
            {
                currentHeatText.text += "<color=#B20000>" + "+" + heat3.ToString() + "</color>";
            }
        }
                                                                   
        currentHeatText.text += ")";

        // - Turn on the warning flash backer if needed (>300 & >400)
        if(playerHeat >= 400) // Red warning flash
        {
            heaWarningBacker.enabled = true;
            heaWarningBacker.gameObject.GetComponent<Animator>().enabled = true;
            heaWarningBacker.gameObject.GetComponent<Animator>().Play("HeatWarningFlash");
            heaWarningBacker.color = alertRed;
        }
        else if (playerHeat < 400 && playerHeat >= 300) // Orange warning flash
        {
            heaWarningBacker.enabled = true;
            heaWarningBacker.gameObject.GetComponent<Animator>().enabled = true;
            heaWarningBacker.gameObject.GetComponent<Animator>().Play("HeatWarningFlash");
            heaWarningBacker.color = warningOrange;
        }
        else // No flasher
        {
            heaWarningBacker.enabled = false;
            heaWarningBacker.gameObject.GetComponent<Animator>().enabled = false;
        }

        // Add "ITN" (Integrated Thermoelectric Network) if needed
        if(PlayerData.inst.artifacts_used.Count > 0)
        {
            foreach (var A in PlayerData.inst.artifacts_used) // Should be relatively quick
            {
                foreach (var E in A.itemData.itemEffects)
                {
                    if(E.alienBonus.hasEffect && E.alienBonus.itn_effect)
                    {
                        currentHeatText.text += " ITN";
                    }
                }
            }
        }

        // Lastly, if this map has a different ambient heat level, we need to display that too
        int ambientHeat = MapManager.inst.ambientHeat;
        // This should be displayed as " (amb. ?##)"
        if (ambientHeat > 0) // HOT. Make the text orange (#A65806)
        {
            currentHeatText.text += $" (amb. <color=#A65806>+{ambientHeat}</color>)";
        }
        else if(ambientHeat < 0) // COOL. Make the text blue (#00C7FF)
        {
            currentHeatText.text += $" (amb. <color=#00C7FF>{ambientHeat}</color>)";
        }

        // ---------- MOVEMENT ------------

        if (GlobalSettings.inst.useAdvMoveDisNumbers) // Advanced move display: (###) ##
        {
            moveNumberText.text = "(" + PlayerData.inst.moveSpeed1.ToString() + ") "; // Time "units"
            moveNumberText.text += PlayerData.inst.moveSpeed2.ToString(); // 1 move = X turns
        }
        else // Basic move display: ##%
        {
            float moveAmount = PlayerData.inst.moveSpeed2;
            string displayAmount = Mathf.RoundToInt(moveAmount).ToString();

            moveNumberText.text = "(" + displayAmount + "%)";
        }

        /* This can be:
         * Rolling [SLOW] <yellow>
         * Treading [SLOW] <yellow>
         * Hovering [FAST] x0/1/2/3 <none,blue,blue,purple>
         * Immobile (Siege in #) OR [SIEGE] <yellow>
         * Running (Core) <none> <-- Default
         * Walking <none>
         */

        int speed = PlayerData.inst.moveSpeed1;
        switch (PlayerData.inst.moveType)
        {
            case BotMoveType.Running:
                moveTypeText.text = "Core";
                modeGO.SetActive(false);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.Walking:
                moveTypeText.text = "Walking";
                modeGO.SetActive(false);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.Treading:
                moveTypeText.text = "Treading";
                modeGO.SetActive(true);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.Flying:
                moveTypeText.text = "Flying";
                modeGO.SetActive(true);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.Hovering:
                moveTypeText.text = "Hovering";
                modeGO.SetActive(true);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.Rolling:
                moveTypeText.text = "Rolling";
                modeGO.SetActive(true);
                flashRef.SetActive(false);
                if (speed <= 25) // FASTx3
                {
                    modeText.text = "FASTx3";
                    modeImage.color = specialPurple;
                }
                else if (speed > 25 && speed <= 50) // FASTx2
                {
                    modeText.text = "FASTx2";
                    modeImage.color = coolBlue;
                }
                else if (speed > 50 && speed <= 75) // FAST
                {
                    modeText.text = "FAST";
                    modeImage.color = coolBlue;
                }
                else if (speed >= 300) // SLOWx3
                {
                    modeText.text = "SLOWx3";
                    modeImage.color = alertRed;
                }
                else if (speed >= 200 && speed < 300) // SLOWx2
                {
                    modeText.text = "SLOWx2";
                    modeImage.color = warningOrange;
                }
                else if (speed >= 150 && speed < 200) // SLOW
                {
                    modeText.text = "SLOW";
                    modeImage.color = slowOrange;
                }
                else // Nothing
                {
                    modeGO.SetActive(false);
                }
                break;
            case BotMoveType.SIEGE:
                flashRef.SetActive(true);
                flashRef.GetComponent<Animator>().Play("siegeModeFlash");
                moveNumberText.gameObject.SetActive(false);
                moveNumberText.text = "";
                if (PlayerData.inst.timeTilSiege == 100) // No siege
                {
                    //
                }
                else if(PlayerData.inst.timeTilSiege >= 1 && PlayerData.inst.timeTilSiege <= 5) // Siege in #
                {
                    moveTypeText.text = "Immobile (Siege in " + PlayerData.inst.timeTilSiege + ")";
                    modeGO.SetActive(false);
                }
                else if(PlayerData.inst.timeTilSiege >= -5 && PlayerData.inst.timeTilSiege <= -1) // Siege end in #
                {
                    moveTypeText.text = "Immobile (Siege end in " + PlayerData.inst.timeTilSiege + ")";
                    modeGO.SetActive(false);
                    
                }
                else // SIEGE MODE
                {
                    moveTypeText.text = "Immobile";
                    modeText.text = "SIEGE";
                    modeGO.SetActive(true);
                    modeImage.color = slowOrange;
                }

                break;
            default:
                break;
        }

        // - Location - (-##/???)

        string locationString = "";

        locationString = (MapManager.inst.currentLevel.ToString() + "/" + MapManager.inst.currentLevelName);
        locationText.text = locationString;

        // Also enable the box warning if needed (high sec & steralization)
        if (GameManager.inst.alert_steralization) // - STERALIZATION
        {
            locationBoxWarning.SetActive(true);
            locationBoxWarning_backer.color = specialPurple;
            locationBoxWarning_text.text = "Strlztn"; // idk how to concat this better. What do you think?
        }
        else if(GameManager.inst.alertLevel >= 6) // - HIGH SEC
        {
            locationBoxWarning.SetActive(true);
            locationBoxWarning_backer.color = highSecRed;
            locationBoxWarning_text.text = "HiSec";
        }
        else
        {
            locationBoxWarning.SetActive(false);
        }

        // Evasion
        if (evasionHeader_text.gameObject.activeInHierarchy)
        {
            avoidanceNum_text.gameObject.SetActive(true);
            avoidanceIndicator_image.gameObject.SetActive(true);
            avoidanceDetail1_text.gameObject.SetActive(true);
            avoidanceDetail2_text.gameObject.SetActive(true);
            avoidanceDetail3_text.gameObject.SetActive(true);
            avoidanceDetail4_text.gameObject.SetActive(true);
            avoidanceDetail5_text.gameObject.SetActive(true);

            int avoidance = (int)(PlayerData.inst.currentAvoidance);
            avoidanceNum_text.text = avoidance.ToString() + "% Avoidance";
            SetAvoidanceImageColor(PlayerData.inst.currentAvoidance, avoidanceIndicator_image);

            // Flight/Hover Bonus
            if (PlayerData.inst.evasion1 > 0)
            {
                if (!PlayerData.inst.lockedInStasis && !(PlayerData.inst.currentWeight > PlayerData.inst.maxWeight)) // Not held in stasis or overweight
                {
                    avoidanceDetail1_text.text = "<color=#295BA0>" + "+" + PlayerData.inst.evasion1 + "</color>"; // Blue
                }
                else
                {
                    avoidanceDetail1_text.text = "<color=#5B5B5B>" + "+" + PlayerData.inst.evasion1 + "</color>"; // Gray
                }
            }
            else
            {
                avoidanceDetail1_text.text = "<color=#5B5B5B>" + "-0" + "</color>"; // Gray
            }
            // Heat level (Inverted)
            if (PlayerData.inst.evasion2 > 0)
            {
                avoidanceDetail2_text.text = "<color=#B53E00>" + "+" + PlayerData.inst.evasion2 + "</color>"; // Orange
            }
            else
            {
                avoidanceDetail2_text.text = "<color=#5B5B5B>" + "-0" + "</color>"; // Gray
            }
            // Movement speed (and whether recently moved)
            if (PlayerData.inst.evasion3 > 0)
            {
                avoidanceDetail3_text.text = "<color=#00FFF5>" + "+" + PlayerData.inst.evasion3 + "</color>"; // Cyan
            }
            else
            {
                avoidanceDetail3_text.text = "<color=#5B5B5B>" + "-0" + "</color>"; // Gray
            }
            // Evasion modifiers from utilities (e.g. Maneuvering Thrusters)
            if (PlayerData.inst.evasion4 > 0)
            {
                avoidanceDetail4_text.text = "<color=#B2B200>" + "+" + PlayerData.inst.evasion4 + "</color>"; // Yellow
            }
            else
            {
                avoidanceDetail4_text.text = "<color=#5B5B5B>" + "-0" + "</color>"; // Gray
            }
            // Cloaking modifiers from utilities (e.g. Cloaking Devices)
            if (PlayerData.inst.evasion5 > 0)
            {
                avoidanceDetail5_text.text = "<color=#8600B2>" + "+" + PlayerData.inst.evasion5 + "</color>"; // Purple
            }
            else
            {
                avoidanceDetail5_text.text = "<color=#5B5B5B>" + "-0" + "</color>"; // Gray
            }

            // And update the expanded menu if necessary
            if (evasionExtra.transform.GetChild(0).gameObject.activeInHierarchy)
                Evasion_UpdateUI();
        }

        // Warning Bars
        float math = (float)PlayerData.inst.currentHealth / (float)PlayerData.inst.maxHealth;
        if (math > 0.15f && math <= 0.4f) // Warning (40%-15%)
        {
            warningCRef.SetActive(true);
            warningCImage.color = warningYellow;
            warningCText.text = "Warning";
        }
        else if (math < 0.15f) // Alert
        {
            warningCRef.SetActive(true);
            warningCImage.color = alertRed;
            warningCText.text = "Alert";
        }
        else // No issues!
        {
            warningCRef.SetActive(false);
        }
        math = (float)PlayerData.inst.currentEnergy / (float)PlayerData.inst.maxEnergy;
        if (math > 0.15f && math <= 0.4f) // Warning (40%-15%)
        {
            warningERef.SetActive(true);
            warningEImage.color = warningYellow;
            warningEText.text = "Warning";
        }
        else if (math < 0.15f) // Alert
        {
            warningERef.SetActive(true);
            warningEImage.color = alertRed;
            warningEText.text = "Alert";
        }
        else // No issues!
        {
            warningERef.SetActive(false);
        }
        math = (float)PlayerData.inst.currentMatter / (float)PlayerData.inst.maxMatter;
        if (math > 0.15f && math <= 0.4f) // Warning (40%-15%)
        {
            warningMRef.SetActive(true);
            warningMImage.color = warningYellow;
            warningMText.text = "Warning";
        }
        else if (math < 0.15f) // Alert
        {
            warningMRef.SetActive(true);
            warningMImage.color = alertRed;
            warningMText.text = "Alert";
        }
        else // No issues!
        {
            warningMRef.SetActive(false);
        }

        // ~ Special Indicators ~
        if (PlayerData.inst & PlayerData.inst.hasRIF) // (Influence)
        {
            influence_text.gameObject.SetActive(true);
            influenceGO.SetActive(true);
            influence_text.text = "Influence: ";

            switch (GameManager.inst.alertLevel)
            {
                case 0: // Low Security
                    influence_text.text += GameManager.inst.alertValue + " LOW";
                    break;
                case 1:
                    influence_text.text += GameManager.inst.alertValue + " " + GameManager.inst.alertLevel + "-" + HF.AssignInfluenceLetter(GameManager.inst.alertValue - 100);
                    break;
                case 2:
                    influence_text.text += GameManager.inst.alertValue + " " + GameManager.inst.alertLevel + "-" + HF.AssignInfluenceLetter(GameManager.inst.alertValue - 300);
                    break;
                case 3:
                    influence_text.text += GameManager.inst.alertValue + " " + GameManager.inst.alertLevel + "-" + HF.AssignInfluenceLetter(GameManager.inst.alertValue - 500);
                    break;
                case 4:
                    influence_text.text += GameManager.inst.alertValue + " " + GameManager.inst.alertLevel + "-" + HF.AssignInfluenceLetter(GameManager.inst.alertValue - 700);
                    break;
                case 5:
                    influence_text.text += GameManager.inst.alertValue + " " + GameManager.inst.alertLevel + "-" + HF.AssignInfluenceLetter(GameManager.inst.alertValue - 900);
                    break;
                case 6: // High Security
                    influence_text.text += GameManager.inst.alertValue + " HIGH";
                    break;

                default:
                    break;
            }
        }
        else
        {
            influence_text.gameObject.SetActive(false);
            influenceGO.SetActive(false);
        }

        if (usingSpecialMovement)
        {
            moveNumberText.text += " // NEM";
            // TODO: NEM
        }
        else
        {
            //
        }

        UpdateCEMCUI();
        UpdateParts();
    }

    public void UpdateTimer(int time)
    {
        timeText.text = time.ToString();
    }

    /// <summary>
    /// Updates the [ Core, Energy, Matter, Corruption ] Slides on the UI
    /// </summary>
    private void UpdateCEMCUI()
    {
        // ~ Values ~
        int maxHealth = PlayerData.inst.maxHealth;
        int currentHealth = PlayerData.inst.currentHealth;
        int maxEnergy = PlayerData.inst.maxEnergy;
        int currentEnergy = PlayerData.inst.currentEnergy;
        int maxMatter = PlayerData.inst.maxMatter;
        int currentMatter = PlayerData.inst.currentMatter;
        int maxCorruption = PlayerData.inst.maxCorruption;
        int currentCorruption = PlayerData.inst.currentCorruption;

        // Core
        if (!core_indicating)
        {
            core_slider.maxValue = PlayerData.inst.maxHealth;
            core_slider.value = PlayerData.inst.currentHealth;
        }
        // Energy
        if (!energy_indicating)
        {
            energy_slider.maxValue = PlayerData.inst.maxEnergy;
            energy_slider.value = PlayerData.inst.currentEnergy;
        }
        // Matter
        if (!matter_indicating)
        {
            matter_slider.maxValue = PlayerData.inst.maxMatter;
            matter_slider.value = PlayerData.inst.currentMatter;
        }
        // Corruption
        if (!corruption_indicating)
        {
            corruption_slider.maxValue = PlayerData.inst.maxCorruption;
            corruption_slider.value = PlayerData.inst.currentCorruption;
        }

        // -- Duplicate Sliders -- //
        // Core
        if (!core_indicating)
        {
            coreD_slider.maxValue = maxHealth;
            coreD_slider.value = currentHealth;
        }
        // Energy
        if (!energy_indicating)
        {
            energyD_slider.maxValue = maxEnergy;
            energyD_slider.value = currentEnergy;
        }
        // Matter
        if (!matter_indicating)
        {
            matterD_slider.maxValue = maxMatter;
            matterD_slider.value = currentMatter;
        }
    }

    #region Stat Bar Changes
    public bool core_indicating = false;
    public bool matter_indicating = false;
    public bool energy_indicating = false;
    public bool corruption_indicating = false;
    Coroutine cBarChange;
    Coroutine mBarChange;
    Coroutine eBarChange;
    Coroutine ccBarChange;

    // -- INCREASES -- (Simpler, Black -> Main Color)
    public void Core_AnimateIncrease(int amount)
    {
        if(cBarChange == null)
        {
            core_indicating = true;

            int newCore = PlayerData.inst.currentHealth + amount;
            if(newCore > PlayerData.inst.maxHealth)
                newCore = PlayerData.inst.maxHealth;
            int oldCore = PlayerData.inst.currentHealth;

            // The true core slide is ON TOP of the duplicate slider

            cBarChange = StartCoroutine(Core_AnimateIncrease(oldCore, newCore));
        }
    }

    private IEnumerator Core_AnimateIncrease(int oldC, int newC)
    {
        // 1. Advace the duplicate bar to the new amount & set its color to black
        coreD_slider.value = newC;
        core_slider.value = oldC;
        coreBarSub.color = Color.black;

        // 2. Animate the duplicate bar's color from Black -> Main Green
        Color startGreen = new Color(0 / 255f, 64 / 255f, 0 / 255f);

        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration) // Black -> Main Green
        {
            coreBarSub.color = Color.Lerp(Color.black, startGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        coreBarSub.color = startGreen;

        // 3. Set both bars to the new value
        coreD_slider.value = newC;
        core_slider.value = newC;

        core_indicating = false;

        cBarChange = null;
    }

    public void Matter_AnimateIncrease(int amount)
    {
        if (mBarChange == null)
        {
            matter_indicating = true;

            int newMatter = PlayerData.inst.currentMatter + amount;
            if (newMatter > PlayerData.inst.maxMatter)
                newMatter = PlayerData.inst.maxMatter;
            int oldMatter = PlayerData.inst.currentMatter;

            // The true matter slide is ON TOP of the duplicate slider

            mBarChange = StartCoroutine(Matter_AnimateIncrease(oldMatter, newMatter));
        }
    }

    private IEnumerator Matter_AnimateIncrease(int oldM, int newM)
    {
        // 1. Advace the duplicate bar to the new amount & set its color to black
        matterD_slider.value = newM;
        matter_slider.value = oldM;
        matterBarSub.color = Color.black;

        // 2. Animate the duplicate bar's color from Black -> Main Purple
        Color startPurple = new Color(77 / 255f, 0 / 255f, 101 / 255f);

        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration) // Black -> Main Cyan
        {
            matterBarSub.color = Color.Lerp(Color.black, startPurple, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        matterBarSub.color = startPurple;

        // 3. Set both bars to the new value
        matterD_slider.value = newM;
        matter_slider.value = newM;

        matter_indicating = false;

        mBarChange = null;
    }

    public void Energy_AnimateIncrease(int amount)
    {
        if (eBarChange == null)
        {
            energy_indicating = true;

            int newEnergy = PlayerData.inst.currentEnergy + amount;
            if (newEnergy > PlayerData.inst.maxEnergy)
                newEnergy = PlayerData.inst.maxEnergy;
            int oldEnergy = PlayerData.inst.currentEnergy;

            // The true energy slide is ON TOP of the duplicate slider

            eBarChange = StartCoroutine(Energy_AnimateIncrease(oldEnergy, newEnergy));
        }
    }

    private IEnumerator Energy_AnimateIncrease(int oldE, int newE)
    {
        // 1. Advace the duplicate bar to the new amount & set its color to black
        energyD_slider.value = newE;
        energy_slider.value = oldE;
        energyBarSub.color = Color.black;

        // 2. Animate the duplicate bar's color from Black -> Main Cyan
        Color startCyan = new Color(4 / 255f, 76 / 255f, 103 / 255f);

        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration) // Black -> Main Cyan
        {
            energyBarSub.color = Color.Lerp(Color.black, startCyan, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        energyBarSub.color = startCyan;

        // 3. Set both bars to the new value
        energyD_slider.value = newE;
        energy_slider.value = newE;

        energy_indicating = false;

        eBarChange = null;
    }

    public void Corruption_AnimateIncrease(int amount)
    {
        if (ccBarChange == null)
        {
            // Does the corruption bar have an animation? I'm not sure so we'll keep it simple for now & let Update_PSUI handle it.
            

            /*
            corruption_indicating = true;

            int newCore = PlayerData.inst.currentHealth + amount;
            if (newCore > PlayerData.inst.maxHealth)
                newCore = PlayerData.inst.maxHealth;
            int oldCore = PlayerData.inst.currentHealth;

            // The true core slide is ON TOP of the duplicate slider

            ccBarChange = StartCoroutine(Corruption_AnimateIncrease(oldCore, newCore));
            */
        }
    }

    /*
    private IEnumerator Corruption_AnimateIncrease(int oldC, int newC)
    {
        // 1. Advace the duplicate bar to the new amount & set its color to black
        corruptionD_slider.value = newC;
        corruption_slider.value = oldC;
        corruptionBarSub.color = Color.black;

        // 2. Animate the duplicate bar's color from White -> Main Green

        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration) // Green -> Bright Gren
        {
            corruptionBarSub.color = Color.Lerp(Color.black, Color.white, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        corruptionBarSub.color = Color.white;

        // 3. Set both bars to the new value
        corruptionD_slider.value = newC;
        corruption_slider.value = newC;

        corruption_indicating = false;

        ccBarChange = null;
    }
    */

    // -- DECREASES -- (Complex, Main Color -> Bright Main Color -> Black)
    public void Core_AnimateDecrease(int cost)
    {
        if(cBarChange == null)
        {
            core_indicating = true;

            int newCore = PlayerData.inst.currentHealth + cost;
            int oldCore = PlayerData.inst.currentHealth;

            // The true core slide is BELOW the duplicate slider

            cBarChange = StartCoroutine(Core_AnimateDecreaseC(oldCore, newCore));
        }
    }

    private IEnumerator Core_AnimateDecreaseC(int oldC, int newC)
    {
        // 1. Reduce the duplicate bar
        coreD_slider.value = newC;
        core_slider.value = oldC;

        // 2. We animate the main bar (Green -> bright Green -> black)
        Color startGreen = new Color(0 / 255f, 64 / 255f, 0 / 255f);
        Color brightGreen = new Color(0 / 255f, 186 / 255f, 0 / 255f);

        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Green -> Bright Gren
        {
            coreBarMain.color = Color.Lerp(startGreen, brightGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        coreBarMain.color = brightGreen;

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright Green -> Black
        {
            coreBarMain.color = Color.Lerp(brightGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        coreBarMain.color = Color.black;

        // 3. Reduce the main bar & reset its color
        core_slider.value = newC;
        coreBarMain.color = startGreen;

        core_indicating = false;

        cBarChange = null;
    }

    /// <summary>
    /// Changes the UI matter bar to animate going down.
    /// </summary>
    /// <param name="cost">The cost to indicate.</param>
    public void Matter_AnimateDecrease(int cost)
    {
        if(mBarChange == null)
        {
            matter_indicating = true;

            int newMatter = PlayerData.inst.currentMatter + cost;
            int oldMatter = PlayerData.inst.currentMatter;

            // The true matter slide is BELOW the duplicate slider

            mBarChange = StartCoroutine(Matter_AnimateDecreaseC(oldMatter, newMatter));
        }
    }

    private IEnumerator Matter_AnimateDecreaseC(int oldM, int newM)
    {
        // 1. Reduce the duplicate bar
        matterD_slider.value = newM;
        matter_slider.value = oldM;

        // 2. We animate the main bar (purple -> bright purple -> black)
        Color startPurple = new Color(77 / 255f, 0 / 255f, 101 / 255f);
        Color brightPurple = new Color(144 / 255f, 2 / 255f, 192 / 255f);

        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Purple -> Bright Purple
        {
            matterBarMain.color = Color.Lerp(startPurple, brightPurple, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        matterBarMain.color = brightPurple;

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright Purple -> Black
        {
            matterBarMain.color = Color.Lerp(brightPurple, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        matterBarMain.color = Color.black;

        // 3. Reduce the main bar & reset its color
        matter_slider.value = newM;
        matterBarMain.color = startPurple;

        matter_indicating = false;

        mBarChange = null;
    }

    public void Energy_AnimateDecrease(int cost)
    {
        if(eBarChange == null)
        {
            energy_indicating = true;

            int newEnergy = PlayerData.inst.currentEnergy + cost;
            int oldEnergy = PlayerData.inst.currentEnergy;

            // The true energy slide is BELOW the duplicate slider

            eBarChange = StartCoroutine(Energy_AnimateDecreaseC(oldEnergy, newEnergy));
        }
    }

    private IEnumerator Energy_AnimateDecreaseC(int oldE, int newE)
    {
        // 1. Reduce the duplicate bar
        energyD_slider.value = newE;
        energy_slider.value = oldE;

        // 2. We animate the main bar (Cyan -> bright Cyan -> black)
        Color startCyan = new Color(4 / 255f, 76 / 255f, 103 / 255f);
        Color brightCyan = new Color(4 / 255f, 162 / 255f, 214 / 255f);

        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Cyan -> Bright Cyan
        {
            energyBarMain.color = Color.Lerp(startCyan, brightCyan, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        energyBarMain.color = brightCyan;

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright Cyan -> Black
        {
            energyBarMain.color = Color.Lerp(brightCyan, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        energyBarMain.color = Color.black;

        // 3. Reduce the main bar & reset its color
        energy_slider.value = newE;
        energyBarMain.color = startCyan;

        energy_indicating = false;

        eBarChange = null;
    }

    #endregion

    #endregion

    // ----- SCAN ------
    #region Rightside - Scan

    public void Scan_DetailTextFlip(bool detailedMode)
    {
        if (detailedMode) // Colored text
        {
            string displayText = "";
            // (Dark green) [
            displayText += "<color=#006C00>" + "[" + "</color>";
            // (White)      Number
            displayText += "<color=#CCC9CC>" + "1" + "</color>";
            // (High green) - Text
            displayText += "<color=#00D900>" + " - Hostile" + "</color>";
            // (Dark green) ]
            displayText += "<color=#006C00>" + "]" + "</color>";
            scanButtonText[0].text = displayText;

            displayText = "";
            // (Dark green) [
            displayText += "<color=#006C00>" + "[" + "</color>";
            // (White)      Number
            displayText += "<color=#CCC9CC>" + "2" + "</color>";
            // (High green) - Text
            displayText += "<color=#00D900>" + " - Friendly" + "</color>";
            // (Dark green) ]
            displayText += "<color=#006C00>" + "]" + "</color>";
            scanButtonText[1].text = displayText;

            displayText = "";
            // (Dark green) [
            displayText += "<color=#006C00>" + "[" + "</color>";
            // (White)      Number
            displayText += "<color=#CCC9CC>" + "3" + "</color>";
            // (High green) - Text
            displayText += "<color=#00D900>" + " - Parts" + "</color>";
            // (Dark green) ]
            displayText += "<color=#006C00>" + "]" + "</color>";
            scanButtonText[2].text = displayText;

            displayText = "";
            // (Dark green) [
            displayText += "<color=#006C00>" + "[" + "</color>";
            // (White)      Number
            displayText += "<color=#CCC9CC>" + "4" + "</color>";
            // (High green) - Text
            displayText += "<color=#00D900>" + " - Exits" + "</color>";
            // (Dark green) ]
            displayText += "<color=#006C00>" + "]" + "</color>";
            scanButtonText[3].text = displayText;
        }
        else // Dim green text
        {
            string displayText = "";
            displayText += "<color=#006C00>" + "[1 - Hostile]" + "</color>";
            scanButtonText[0].text = displayText;

            displayText = "";
            displayText += "<color=#006C00>" + "[2 - Friendly]" + "</color>";
            scanButtonText[1].text = displayText;

            displayText = "";
            displayText += "<color=#006C00>" + "[3 - Parts]" + "</color>";
            scanButtonText[2].text = displayText;

            displayText = "";
            displayText += "<color=#006C00>" + "[4 - Exits]" + "</color>";
            scanButtonText[3].text = displayText;
        }
    }

    private bool scanIndicateCooldown = false;
    private IEnumerator Scan_Cooldown()
    {
        scanIndicateCooldown = true;

        yield return new WaitForSeconds(GlobalSettings.inst.itemPopupLifetime + 1);

        scanIndicateCooldown = false;
    }

    public void Scan_IndicateHostiles() // 1
    {
        if (!scanIndicateCooldown) // We don't want the player to be able to spam these
        {
            StartCoroutine(Scan_Cooldown());
        }

        List<GameObject> hostiles = new List<GameObject>();

        foreach (Actor B in GameManager.inst.entities) // Go through all entities
        {
            if(HF.DetermineRelation(PlayerData.inst.GetComponent<Actor>(), B) == BotRelation.Hostile && B.isVisible) // Search for hostile aligned bots
            {
                hostiles.Add(B.gameObject); // Add it to the list
            }
        }

        // - We now want to create indicators for all these objects -




        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // UI - ACCESS
    }

    public void Scan_IndicateFriendlies() // 2
    {
        if (!scanIndicateCooldown) // We don't want the player to be able to spam these
        {
            StartCoroutine(Scan_Cooldown());
        }

        List<GameObject> friendlies = new List<GameObject>();

        foreach (Actor B in GameManager.inst.entities) // Go through all entities
        {
            if (HF.DetermineRelation(PlayerData.inst.GetComponent<Actor>(), B) == BotRelation.Friendly && B.isVisible) // Search for friendly aligned bots
            {
                friendlies.Add(B.gameObject); // Add it to the list
            }
        }

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // UI - ACCESS
    }

    public void Scan_IndicateParts() // 3
    {
        if (!scanIndicateCooldown) // We don't want the player to be able to spam these
        {
            StartCoroutine(Scan_Cooldown());
        }

        List<GameObject> parts = new List<GameObject>();

        foreach (var item in InventoryControl.inst.worldItems)
        {
            if (item.Value.GetComponent<Part>().isExplored)
            {
                parts.Add(item.Value);
            }
        }

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // UI - ACCESS
    }

    public void Scan_IndicateExits() // 4
    {
        if (!scanIndicateCooldown) // We don't want the player to be able to spam these
        {
            StartCoroutine(Scan_Cooldown());
        }

        List<GameObject> exits = new List<GameObject>();

        foreach (var B in MapManager.inst.placedBranches)
        {
            if (B.gameObject.GetComponent<AccessObject>().isExplored)
            {
                exits.Add(B.gameObject);
            }
        }

        foreach (var E in MapManager.inst.placedExits)
        {
            if (E.gameObject.GetComponent<AccessObject>().isExplored)
            {
                exits.Add(E.gameObject);
            }
        }

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // UI - ACCESS
    }

    public void Scan_FlipSubmode(bool state, GameObject focusObj = null)
    {
        if (state) // Enable Submode
        {
            // Enable the sub text blocker
            scanSubParent.SetActive(true);
            scanSubTextA.enabled = true;
            scanSubTextB.enabled = true;
            scanSubBackerImages[0].enabled = true;
            scanSubBackerImages[1].enabled = true;
            scanSubBackerImages[0].color = Color.black;
            scanSubBackerImages[1].color = Color.black;
            // Disable the text
            scanButtonText[0].enabled = false;
            scanButtonText[1].enabled = false;
            scanButtonText[2].enabled = false;
            scanButtonText[3].enabled = false;

            // NOTE: The highlight backing animation only plays for "important" objects like; actors, items, exits, and traps.

            // Now figure out what the focus object is
            if (focusObj.GetComponent<Actor>())
            {
                StartCoroutine(Scan_SubmodeAnimate());

                // - The square - here it represents the actor's current health
                scanSubImage.enabled = true;
                float currentIntegrity = (float)focusObj.GetComponent<Actor>().currentHealth / (float)focusObj.GetComponent<Actor>().maxHealth;
                if (currentIntegrity >= 0.75f) // Green (>=75%)
                {
                    scanSubImage.color = activeGreen;
                }
                else if (currentIntegrity < 0.75f && currentIntegrity <= 0.50f) // Yellow (75-50%)
                {
                    scanSubImage.color = cautiousYellow;
                }
                else if (currentIntegrity < 0.5f && currentIntegrity <= 0.25f) // Orange (50-25%)
                {
                    scanSubImage.color = corruptOrange;
                }
                else // Red (25-0%)
                {
                    scanSubImage.color = dangerRed;
                }
                // - The text - here line A is the actors's name and line B is the base change to hit the actor (dark green)
                scanSubTextA.text = focusObj.name;
                if (HF.DetermineRelation(PlayerData.inst.GetComponent<Actor>(), focusObj.GetComponent<Actor>()) == BotRelation.Hostile)
                {
                    // Display it as red if the bot is hostile
                    scanSubTextA.color = dangerRed;
                }
                else
                {
                    // Display it as green if not
                    scanSubTextA.color = highGreen;
                }

                float toHitChance = 0f;
                bool noCrit = false;
                List<ArmorType> types = new List<ArmorType>();
                Item activeWeapon = Action.FindActiveWeapon(PlayerData.inst.GetComponent<Actor>());
                if (activeWeapon != null)
                {
                    (toHitChance, noCrit, types) = Action.CalculateRangedHitChance(PlayerData.inst.GetComponent<Actor>(), focusObj.GetComponent<Actor>(), activeWeapon);
                }

                scanSubTextB.text = "Base Hit " + (int)(toHitChance * 100);
                scanSubTextB.color = subGreen;

                // And the "danger notification". This tells the player if the enemy bot knows the Player is there.
                if (HF.ActorInBotFOV(focusObj.GetComponent<Actor>(), PlayerData.inst.GetComponent<Actor>()) && focusObj.GetComponent<BotAI>().memory > 0)
                {
                    scanSubDangerNotify.enabled = true;
                    scanSubDangerNotify.text = "!";
                }
                else
                {
                    scanSubDangerNotify.enabled = false;
                    scanSubDangerNotify.text = "!";
                }

            }
            else if (focusObj.GetComponent<AccessObject>())
            {
                // - The square - here it's disabled
                scanSubImage.enabled = false;

                // - The text - here line A is the name primary or branch access, and the secondary is where it leads
                if (focusObj.GetComponent<AccessObject>().isBranch)
                {
                    scanSubTextA.text = "Branch Access";
                }
                else
                {
                    scanSubTextA.text = "Level Access";
                }
                scanSubTextA.color = highGreen;

                scanSubTextB.text = "< ";
                if (focusObj.GetComponent<AccessObject>().playerKnowsDestination)
                {
                    scanSubTextB.text += focusObj.GetComponent<AccessObject>().destName;
                }
                else
                {
                    scanSubTextB.text += "???";
                }
                scanSubTextB.color = highGreen;
                
            }
            else if (focusObj.GetComponent<Part>())
            {
                StartCoroutine(Scan_SubmodeAnimate());

                if (focusObj.GetComponent<Part>()._item.Id != 17)
                {
                    Item _item = focusObj.GetComponent<Part>()._item;

                    // - The square - here it represents the item's current health
                    scanSubImage.enabled = true;
                    float currentIntegrity = (float)focusObj.GetComponent<Part>()._item.integrityCurrent / (float)focusObj.GetComponent<Part>()._item.itemData.integrityMax;
                    if (currentIntegrity >= 0.75f) // Green (>=75%)
                    {
                        scanSubImage.color = activeGreen;
                    }
                    else if (currentIntegrity < 0.75f && currentIntegrity <= 0.50f) // Yellow (75-50%)
                    {
                        scanSubImage.color = cautiousYellow;
                    }
                    else if (currentIntegrity < 0.5f && currentIntegrity <= 0.25f) // Orange (50-25%)
                    {
                        scanSubImage.color = corruptOrange;
                    }
                    else // Red (25-0%)
                    {
                        scanSubImage.color = dangerRed;
                    }
                    // - The text - here line A is the item's name (in full green) and line B is the items mechanical description (dark green)
                    string _message = "";
                    // Is this item a prototype? If so we don't show the true name
                    if (_item.itemData.knowByPlayer)
                    {
                        _message = _item.itemData.itemName;
                    }
                    else
                    {
                        _message = HF.ItemPrototypeName(_item);
                    }
                    scanSubTextA.text = _message;
                    scanSubTextA.color = highGreen;
                }
                else // We do something different for matter
                {
                    // The square is green
                    scanSubImage.color = activeGreen;
                    // The text is grayed out (## Matter)
                    scanSubTextA.text = focusObj.GetComponent<Part>()._item.amount + " " + focusObj.GetComponent<Part>()._item.Name;
                    scanSubTextA.color = inactiveGray;

                    scanSubTextB.enabled = false;
                    scanSubBackerImages[1].enabled = false;
                }

                if(focusObj.GetComponent<Part>()._item.itemData.mechanicalDescription.Length > 0)
                {
                    scanSubTextB.text = focusObj.GetComponent<Part>()._item.itemData.mechanicalDescription;
                    scanSubTextB.color = subGreen;
                }
                else // Disable it if the item doesn't have a mechanical description
                {
                    scanSubTextB.enabled = false;
                    scanSubBackerImages[1].enabled = false;
                }
            }
            else if (focusObj.GetComponent<TileBlock>())
            {
                // - The square - here it's disabled
                scanSubImage.enabled = false;
                if (focusObj.GetComponent<DoorLogic>()) // Is this a door?
                {
                    scanSubTextA.text = focusObj.GetComponent<DoorLogic>()._tile.tileInfo.tileName;
                    scanSubTextA.color = highGreen;

                    scanSubTextB.enabled = false;
                    scanSubBackerImages[1].enabled = false;
                }
                else // Its not a door
                {
                    // - The text - here line A is the tile's name (in full green) and line B is the tile's armor value (dark green)
                    scanSubTextA.text = focusObj.GetComponent<TileBlock>().tileInfo.tileName;
                    scanSubTextA.color = highGreen;

                    scanSubTextB.text = focusObj.GetComponent<TileBlock>().tileInfo.armor.ToString();
                    scanSubTextB.color = subGreen;
                }
            }
            else if (focusObj.GetComponent<MachinePart>())
            {
                // Here the square is off
                scanSubImage.enabled = false;

                // The text shows its name, and then armor
                scanSubTextA.text = focusObj.GetComponent<MachinePart>().displayName;
                scanSubTextA.color = highGreen;

                scanSubTextB.text = focusObj.GetComponent<MachinePart>().armor.ToString();
                scanSubTextB.color = subGreen;
            }
            else if (focusObj.GetComponent<FloorTrap>())
            {
                StartCoroutine(Scan_SubmodeAnimate());

                // - The square - here it indicates if this trap is friendly or not
                if (HF.RelationToTrap(PlayerData.inst.GetComponent<Actor>(), focusObj.GetComponent<FloorTrap>()) != BotRelation.Friendly) 
                {
                    scanSubImage.color = dangerRed;
                    scanSubTextA.color = dangerRed;
                }
                else if (HF.RelationToTrap(PlayerData.inst.GetComponent<Actor>(), focusObj.GetComponent<FloorTrap>()) != BotRelation.Hostile)
                {
                    scanSubImage.color = activeGreen;
                    scanSubTextA.color = highGreen;
                }
                // - The text - here line A is the actors's name
                scanSubTextA.text = focusObj.GetComponent<FloorTrap>().fullName;

                scanSubTextB.enabled = false;
                scanSubBackerImages[1].enabled = false;
            }


            // Trim the two display strings, as we can only display at most 26 characters per line.
            if (scanSubTextA.text.Length > 26)
            {
                scanSubTextA.text = scanSubTextA.text.Substring(0, 26);
            }
            if (scanSubBackerImages[1].enabled && scanSubTextB.text.Length > 26)
            {
                scanSubTextB.text = scanSubTextB.text.Substring(0, 26);
            }
        }
        else // Disable Submode
        {
            // Disable sub text blocker
            scanSubParent.SetActive(false);
            // Re-enable the text
            scanButtonText[0].enabled = true;
            scanButtonText[1].enabled = true;
            scanButtonText[2].enabled = true;
            scanButtonText[3].enabled = true;
        }

        
    }

    private IEnumerator Scan_SubmodeAnimate()
    {
        // Basically just make the backer images go from black to high green to black
        scanSubBackerImages[0].color = Color.black;
        scanSubBackerImages[1].color = Color.black;

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            scanSubBackerImages[0].color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            scanSubBackerImages[1].color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        scanSubBackerImages[0].color = highGreen;
        scanSubBackerImages[1].color = highGreen;

        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            scanSubBackerImages[0].color = Color.Lerp(highGreen, Color.black, elapsedTime / duration);
            scanSubBackerImages[1].color = Color.Lerp(highGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        scanSubBackerImages[0].color = Color.black;
        scanSubBackerImages[1].color = Color.black;
    }

    #endregion

    #region Rightside - Evasion
    // -- EVASION -- //
    [Header("RSUI - Evasion")]
    public GameObject evasionExtra;
    public GameObject evasion_hoverOpener;
    public GameObject evasionAnimator;
    public TextMeshProUGUI evasionText1;
    public TextMeshProUGUI evasionText2;
    public TextMeshProUGUI evasionText3;
    public TextMeshProUGUI evasionText4;
    public TextMeshProUGUI evasionText5;
    public TextMeshProUGUI evasionMainText;
    public Image evasionImage;
    [Header("RSUI - Evasion (Volley)")]
    public GameObject volleyMain;
    public TextMeshProUGUI volleyHeader;
    public TextMeshProUGUI volleyTextA;
    public TextMeshProUGUI volleyTextB;
    public GameObject volleyV;
    [SerializeField] private TextMeshProUGUI volleyVText;
    [SerializeField] private GameObject volley_prefab;

    public void Evasion_ExpandMenu()
    {
        if (PlayerData.inst && terminal_targetTerm == null) // Should only be able to open the menu when the player exists & not in terminal menu
        {
            //StopCoroutine(Evasion_ExpandMenu_Animation());

            //evasionExtra.transform.GetChild(0).gameObject.SetActive(true);
            evasionExtra.GetComponent<UIMouseBounds>().disabled = true; // Disable opening detection
            evasion_hoverOpener.GetComponent<UIMouseBounds>().disabled = false; // Allow closing

            evasionAnimator.GetComponent<UIEvasionAnimation>().OpenMenu();
            //StartCoroutine(Evasion_ExpandMenu_Animation());
        }
    }

    /*
    private IEnumerator Evasion_ExpandMenu_Animation()
    {
        evasionAnimator.enabled = true;

        evasionAnimator.Play("Evasion_ExpandMenu");

        yield return new WaitForSeconds(1f);

        evasionAnimator.enabled = false;
        Evasion_UpdateUI();
    }
    */

    public void Evasion_UpdateUI()
    {

        // Flight/Hover Bonus
        if (PlayerData.inst.evasion1 > 0)
        {
            if (PlayerData.inst.lockedInStasis)
            {
                evasionText1.text = "<color=#5B5B5B>" + Action.DetermineBotMoveType(PlayerData.inst.GetComponent<Actor>()).ToString() + " (in stasis) + " + PlayerData.inst.evasion1 + "</color>"; // Gray
            }
            else if (Action.IsOverweight(PlayerData.inst.GetComponent<Actor>()))
            {
                evasionText1.text = "<color=#5B5B5B>" + Action.DetermineBotMoveType(PlayerData.inst.GetComponent<Actor>()).ToString() + " (overweight) + " + PlayerData.inst.evasion1 + "</color>"; // Gray
            }
            else
            {
                evasionText1.text = "<color=#295BA0>" + Action.DetermineBotMoveType(PlayerData.inst.GetComponent<Actor>()).ToString() + " + " + PlayerData.inst.evasion1 + "</color>"; // Gray
            }
        }
        else
        {
            evasionText1.text = "<color=#5B5B5B>" + Action.DetermineBotMoveType(PlayerData.inst.GetComponent<Actor>()).ToString() + " - 0" + "</color>"; // Gray
        }
        // Heat level (Inverted)
        if (PlayerData.inst.evasion2 > 0)
        {
            evasionText2.text = "<color=#B53E00>" + "Heat - " + PlayerData.inst.evasion2 + "</color>"; // Orange
        }
        else
        {
            evasionText2.text = "<color=#5B5B5B>" + "Heat - 0" + "</color>"; // Gray
        }
        // Movement speed (and whether recently moved)
        if (PlayerData.inst.evasion3 > 0)
        {
            string mainText = "Speed + ";
            if(PlayerData.inst.timeTilSiege == 0)
            {
                mainText = "Siege + "; // Special text for Siege mode
            }

            evasionText3.text = "<color=#00FFF5>" + mainText + PlayerData.inst.evasion3 + "</color>"; // Cyan
        }
        else
        {
            evasionText3.text = "<color=#5B5B5B>" + "Speed + 0" + "</color>"; // Gray
        }
        // Evasion modifiers from utilities (e.g. Maneuvering Thrusters)
        if (PlayerData.inst.evasion4 > 0)
        {
            evasionText4.text = "<color=#B2B200>" + "Evasion + " + PlayerData.inst.evasion4 + "</color>"; // Yellow
        }
        else
        {
            evasionText4.text = "<color=#5B5B5B>" + "Evasion + 0" + "</color>"; // Gray
        }
        // Cloaking modifiers from utilities (e.g. Cloaking Devices)
        if (PlayerData.inst.evasion5 > 0)
        {
            evasionText5.text = "<color=#8600B2>" + "Phasing + " + PlayerData.inst.evasion5 + "</color>"; // Purple
        }
        else
        {
            evasionText5.text = "<color=#5B5B5B>" + "Phasing + 0" + "</color>"; // Gray
        }

        SetAvoidanceImageColor(PlayerData.inst.currentAvoidance, evasionImage);

        evasionMainText.text = PlayerData.inst.currentAvoidance.ToString() + "%";
    }

    public void Evasion_ShrinkMenu()
    {
        //StopCoroutine(Evasion_ExpandMenu_Animation());

        //evasionAnimator.enabled = false;
        evasionExtra.GetComponent<UIMouseBounds>().disabled = false; // Allow opening
        evasion_hoverOpener.GetComponent<UIMouseBounds>().disabled = true; // Disabled closing detection
        evasionExtra.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Evasion_Volley(bool showMenu)
    {
        volleyMain.SetActive(showMenu);
        volleyV.SetActive(showMenu); // idk what this is for
        evasionExtra.GetComponent<UIMouseBounds>().disabled = showMenu; // Disable/Enable opening detection

        if (showMenu)
        {
            volleyHeader.text = "/VOLLEY/";

            // Now we need to get a bunch of info from the mouse and what the player is actually targeting.
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // End point correction due to sneaky rounding
            mousePosition = new Vector3(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));

            #region Volley TLDR
            /*
             *    Description from the Cogmind manual:
             * While in firing mode, the evasion window is replaced with the volley window, 
             * showing summary data for the volley if there are any active weapons. 
             * 'R' indicates the range from your current position to the cursor. 
             * Letters in brackets refer to all active weapons (using their parts list letter), 
             * where those which are not currently within range of the cursor target (and therefore will not fire) are grayed out. 
             * For all weapons that will fire, the total resource costs are listed in the second line: 
             * time required to fire along with energy ('E'), matter ('M') and heat ('H'). 
             * Energy and matter are displayed as total costs, 
             * while heat is instead the amount generated per turn over the volley duration.
             * 
             * At the bottom of that window is a button for toggling a visualization of the volley range 
             * (alternatively use the 'v' key) to highlight the area reachable by the current volley, 
             * using different levels of brightness if multiple ranges are involved. Any visible terrain destroyable using the volley 
             * (including generally taking into account all applicable damage modification from active utilities, 
             * current momentum, resistances, etc.) will glow red, while any other terrain that could at least theoretically 
             * be destroyed by currently inactive weapons or those in inventory will instead glow yellow. 
             * The appearance of terrain you don't likely have the means to destroy at the moment remains unchanged. 
             * Note that even in such a case, it may sometimes still be possible to destroy the terrain using other means, 
             * since the system does not take into account inactive utilities or those in inventory, 
             * special weapons, the environment, or other means to creatively destroy terrain, 
             * but it is a useful way to quickly confirm that you already do have the capability.
             */
            #endregion

            // -- This is what we need to display:
            // Line 1) R=# [?]
            // - Where # = range and the letter inside changes to represent different things.
            // - The letter itself changes, so does its color.
            // - This line is always displayed
            int range = Mathf.RoundToInt(Vector2.Distance(HF.V3_to_V2I(PlayerData.inst.transform.position), mousePosition));

            volleyTextA.text = "R=" + range + " [";
            List<KeyValuePair<string, Item>> weaponChars = new List<KeyValuePair<string, Item>>(); // List of individual characters that represent weapons.
            // Gather up all active weapons (normally there should at least be one)
            foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.state && item.item.disabledTimer <= 0) // Only active weapons
                    {
                        string letter = HF.FindItemUILetter(item.item);
                        if(letter != null)
                        {
                            weaponChars.Add(new KeyValuePair<string, Item>(letter, item.item));
                        }
                    }
                }
            }

            // Go through all active weapons we previously gathered, and add in the letters + colors based on range
            foreach (var W in weaponChars)
            {
                // "<color=#5B5B5B>" // gray
                // "<color=#00D900>" // green

                if ((W.Value.itemData.meleeAttack.isMelee && range > 2) || (W.Value.itemData.shot.shotRange > range))
                {
                    volleyTextA.text += "<color=#00D900>" + W.Key.ToLower() + "</color>"; // In range, so Green
                }
                else
                {
                    volleyTextA.text += "<color=#5B5B5B>" + W.Key.ToLower() + "</color>"; // Out of range, so Gray
                }
            }

            volleyTextA.text += "]";

            // Line 2) Time ### E-## M-## H+##
            // - Where we display the time to shoot, energy/matter/heat costs.
            // - This line is only displayed when atleast one weapon is in ranged.
            // - All weapons that are in range (see line 1), will have their costs added to this line.
            int time = 0, energy = 0, matter = 0, heat = 0;
            volleyTextB.enabled = false;
            foreach (var W in weaponChars)
            {
                if ((W.Value.itemData.meleeAttack.isMelee && range > 2) || (W.Value.itemData.shot.shotRange > range))
                {
                    volleyTextB.enabled = true;

                    if (W.Value.itemData.meleeAttack.isMelee) // Melee weapon
                    {
                        time += W.Value.itemData.meleeAttack.delay;
                        energy += W.Value.itemData.meleeAttack.energy;
                        matter += W.Value.itemData.meleeAttack.matter;
                        heat += W.Value.itemData.meleeAttack.heat;
                    }
                    else // Ranged weapon
                    {
                        time += W.Value.itemData.shot.shotDelay;
                        energy += W.Value.itemData.shot.shotEnergy;
                        matter += W.Value.itemData.shot.shotMatter;
                        heat += W.Value.itemData.shot.shotHeat;
                    }
                }
            }

            // Now assemble the string (if the text is active)
            if(volleyTextB.enabled == true)
            {
                volleyTextB.text = "Time ";
                volleyTextB.text += time.ToString();
                

                if(energy >= 0)
                {
                    volleyTextB.text += " E+" + energy;
                }
                else if (energy == 0)
                {
                    volleyTextB.text += " E-" + energy;
                }
                else
                {
                    volleyTextB.text += " E" + energy;
                }

                if(matter > 0)
                {
                    volleyTextB.text += " M+" + matter;
                }
                else if (matter == 0)
                {
                    volleyTextB.text += " M-" + matter;
                }
                else
                {
                    volleyTextB.text += " M" + matter;
                }

                if(heat >= 0)
                {
                    volleyTextB.text += " H+" + heat;
                }
                else
                {
                    volleyTextB.text += " H" + heat;
                }
            }

            // <color=#009700> // darker green
        }
        else
        {
            volleyHeader.text = "/EVASION/";
            // I don't think there is anything else we need to do here.
        }
    }

    #region /VOLLEY/ Animation
    [HideInInspector] public bool volleyMode = false;
    [HideInInspector] public bool volleyAnimating = false;
    public void Evasion_VolleyModeFlip()
    {
        volleyMode = !volleyMode; // Flip

        if (volleyMode)
        {
            // Enable the special volley mode
            volleyVText.color = highlightGreen; // Change the "v" color

            if(!volleyAnimating)
                StartCoroutine(Evasion_VolleyAnimation()); // Do the animation

        }
        else
        {
            // Disable the special volley mode
            volleyVText.color = subGreen; // Change the "v" color

            Evasion_VolleyCleanup(); // Clean up the animation

            // Play the sound "RANGE_OFF" (ui-86)
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["RANGE_OFF"]); // UI - RANGE_OFF
        }
    }

    [HideInInspector] public Dictionary<Vector2Int, GameObject> volleyTiles = new Dictionary<Vector2Int, GameObject>();

    private IEnumerator Evasion_VolleyAnimation()
    {
        volleyAnimating = true;

        // Basically what we want to do here is a simple "scan" animation.
        // This takes place entirely within the player's FOV.

        // We are going make two scan lines (one going up, one going down) from the players position.
        // The lines start full red but quickly go transparent.
        // When the animation ends the transparent red squares stay within the player FOV.

        // TLDR, we are basically just gonna steal from NFA_Scan() but make it slightly more complex.

        // Gather up the player's FOV
        List<Vector3Int> fovD = PlayerData.inst.GetComponent<Actor>().FieldofView;
        List<Vector3Int> fov = fovD.Distinct().ToList();

        // Create a new tile for each FOV
        foreach (Vector3Int L in fov)
        {
            Evasion_VolleyMakeTile(new Vector2Int(L.x, L.y));
        }

        Vector2Int playerLocation = HF.V3_to_V2I(PlayerData.inst.transform.position);

        float pingDuration = 0.15f;

        // Calculate how many rows there are
        int rowCount = 0;
        HashSet<int> rowIndices = new HashSet<int>();

        foreach (Vector2Int key in volleyTiles.Keys)
        {
            // Adding the y-coordinate to the HashSet to keep only unique rows
            rowIndices.Add(key.y);
        }

        rowCount = rowIndices.Count;

        // Organize the volleyTiles by *y* coordinate.
        // Initialize the new dictionary
        Dictionary<int, List<GameObject>> sortedDictionary = new Dictionary<int, List<GameObject>>();

        // Iterate through the original dictionary
        foreach (var kvp in volleyTiles)
        {
            // Get the y-coordinate
            int yCoordinate = kvp.Key.y;

            // Check if the y-coordinate is already a key in the sorted dictionary
            if (!sortedDictionary.ContainsKey(yCoordinate))
            {
                sortedDictionary[yCoordinate] = new List<GameObject>();
            }

            // Add the GameObject to the list associated with the y-coordinate
            sortedDictionary[yCoordinate].Add(kvp.Value);
        }

        // Determine rows above and below the player's position
        int playerRow = playerLocation.y;
        int maxRow = sortedDictionary.Keys.Max();
        int minRow = sortedDictionary.Keys.Min();

        int rowsAbovePlayer = maxRow - playerRow;
        int rowsBelowPlayer = playerRow - minRow;

        // Create a sorted list of rows based on distance from the player
        List<int> sortedRows = new List<int>();
        sortedRows.Add(playerRow); // Start with the player's row

        for (int i = 1; i <= Mathf.Max(rowsAbovePlayer, rowsBelowPlayer); i++)
        {
            if (playerRow + i <= maxRow)
            {
                sortedRows.Add(playerRow + i); // Add rows below the player
            }

            if (playerRow - i >= minRow)
            {
                sortedRows.Add(playerRow - i); // Add rows above the player
            }
        }

        // Play the sound "SCAN_4" (ui-91)
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["SCAN_4"]); // SCAN_4

        // Now go through each row, and ping every block in the row
        foreach (int row in sortedRows)
        {
            if (sortedDictionary.ContainsKey(row))
            {
                foreach (GameObject entry in sortedDictionary[row])
                {
                    entry.GetComponent<Animator>().enabled = true;
                    entry.GetComponent<Animator>().Play("VolleyTile");
                }
            }

            yield return new WaitForSeconds(pingDuration / rowCount); // Equal time between rows
        }

        yield return null;

        // Now we replace all the animation tiles will non-animation tiles

        Evasion_VolleyNonAnimation();
        volleyAnimating = false;
    }

    public void Evasion_VolleyNonAnimation()
    {
        // No animation here we just fill the FOV with the prefabs
        Evasion_VolleyCleanup();

        // Gather up the player's FOV
        List<Vector3Int> fovD = PlayerData.inst.GetComponent<Actor>().FieldofView;
        List<Vector3Int> fov = fovD.Distinct().ToList();

        // Create a new tile for each FOV
        foreach (Vector3Int L in fov)
        {
            Evasion_VolleyMakeTile(new Vector2Int(L.x, L.y));
        }
    }

    private void Evasion_VolleyCleanup()
    {
        // Nothing fancy here, just remove all the squares.

        foreach (var item in volleyTiles.ToList())
        {
            Destroy(item.Value);
        }

        volleyTiles.Clear();
    }

    private void Evasion_VolleyMakeTile(Vector2Int pos)
    {
        var spawnedTile = Instantiate(volley_prefab, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"TargetLine: {pos.x},{pos.y}"; // Give grid based name
        spawnedTile.transform.parent = this.transform;
        volleyTiles.Add(pos, spawnedTile);
    }

    #endregion

    #endregion

    /// <summary>
    /// Updates the [ Parts ] area on the UI
    /// </summary>
    public void UpdateParts()
    {
        // - Weight -
        string weight = "";

        // Do different weight display if using the Hideout Cache
        if(cTerminal_machine != null)
        {
            InventoryObject inv = InventoryControl.inst.hideout_inventory;
            int current = inv.ItemCount, max = inv.Container.Items.Length;

            weight = current + "/" + max;
            weightText.text = weight;

            if (current >= max)
            {
                weightText.color = dangerRed;
            }
            else
            {
                weightText.color = normalGreen;
            }
        }
        else // Normal text
        {
            weight = PlayerData.inst.currentWeight.ToString() + "/" + PlayerData.inst.maxWeight.ToString();

            weightText.text = weight;
            if (PlayerData.inst.currentWeight > PlayerData.inst.maxWeight)
            {
                weightText.color = dangerRed;
            }
            else
            {
                weightText.color = normalGreen;
            }
        }

        // - # x # -   ~ Overencumberance (Balance) ~
        // - This will only appear if the player's current weight is > max weight
        // - The right # is by how much times X it is over
        // - The left # is ???
        if (PlayerData.inst.currentWeight > PlayerData.inst.maxWeight)
        {
            miscNumGO.SetActive(true);

            string leftNumber = "0";
            string rightNumber = "0";

            // - Right Number Calculation -
            float difference = (float)PlayerData.inst.currentWeight / (float)PlayerData.inst.maxWeight;
            int displayNum = Mathf.RoundToInt(difference); // Round to nearest whole number
            rightNumber = displayNum.ToString();

            // - Left Number Calculation -

            miscNumText.text = leftNumber + "x" + rightNumber;
        }
        else
        {
            miscNumGO.SetActive(false);
        }

        // - Bot Type -
        if (useBotType)
        {
            botTypeGO.SetActive(true);
            string botTypeS = "";
            botTypeText.text = botTypeS;
        }
        else
        {
            botTypeGO.SetActive(false);
        }
    }

    private Coroutine partsHeaderFlash = null;
    /// <summary>
    /// Briefly flash the /PARTS/ header text
    /// </summary>
    private void PartsFlashHeader()
    {
        if(partsHeaderFlash != null)
        {
            StopCoroutine(partsHeaderFlash);
        }
        partsHeaderFlash = StartCoroutine(PartsFlashHeaderAnim());
    }

    private IEnumerator PartsFlashHeaderAnim()
    {
        // We want to do the following
        // 1. Set the text to DARK green
        // 2. Set the text to BRIGHT green
        // 3. Set the text to BLACK
        // 4. Set the text to its normal color
        Color normal = HF.HexToRGB("15AC13");

        // 1
        partsHeaderText.color = dullGreen;

        // 2
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Dark Green -> Bright Green
        {
            partsHeaderText.color = Color.Lerp(dullGreen, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright Green -> Black
        {
            partsHeaderText.color = Color.Lerp(highGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Black -> Normal
        {
            partsHeaderText.color = Color.Lerp(Color.black, normal, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #region Rightside - CEWQ
    public void CEWQ_Click(int value) // Separate function for the buttons because it won't show up in the list for functions with more than 1 input
    {
        CEWQ_SetMode(value, true);
    }

    public void CEWQ_HoverEnter(int id)
    {
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HOVER"], 0.8f); // UI - HOVER

        switch (id)
        {
            case 0:
                cText.text = "<color=#00FF00>c</color>";
                break;
            case 1:
                eText.text = "<color=#00FF00>e</color>";
                break;
            case 2:
                wText.text = "<color=#00FF00>w</color>";
                break;
            case 3:
                qText.text = "<color=#00FF00>q</color>";
                break;
        }
    }

    public void CEWQ_HoverExit(int id)
    {
        // Only difference here is that if the mode is selected we don't set it back to dark green, we keep it light green.
        switch (id)
        {
            case 0:
                if(cewq_mode == id)
                {
                    cText.text = "<color=#00FF00>c</color>";
                    // Unmark everything else
                    eText.text = "<color=#008100>e</color>";
                    wText.text = "<color=#008100>w</color>";
                    qText.text = "<color=#008100>q</color>";
                }
                else
                {
                    cText.text = "<color=#008100>c</color>";
                }
                break;
            case 1:
                if (cewq_mode == id)
                {
                    eText.text = "<color=#00FF00>e</color>";
                    // Unmark everything else
                    cText.text = "<color=#008100>c</color>";
                    wText.text = "<color=#008100>w</color>";
                    qText.text = "<color=#008100>q</color>";
                }
                else
                {
                    eText.text = "<color=#008100>e</color>";
                }
                break;
            case 2:
                if (cewq_mode == id)
                {
                    wText.text = "<color=#00FF00>w</color>";
                    // Unmark everything else
                    eText.text = "<color=#008100>e</color>";
                    cText.text = "<color=#008100>c</color>";
                    qText.text = "<color=#008100>q</color>";
                }
                else
                {
                    wText.text = "<color=#008100>w</color>";
                }
                break;
            case 3:
                if (cewq_mode == id)
                {
                    qText.text = "<color=#00FF00>q</color>";
                    // Unmark everything else
                    eText.text = "<color=#008100>e</color>";
                    wText.text = "<color=#008100>w</color>";
                    cText.text = "<color=#008100>c</color>";
                }
                else
                {
                    qText.text = "<color=#008100>q</color>";
                }
                break;
        }
    }

    public void CEWQ_SetMode(int value, bool showType = true)
    {
        // Set the mode
        cewq_mode = value;
        CEWQ_HoverExit(cewq_mode); // Change the text

        // Do the animation
        if (cewq_coroutine != null)
        {
            StopCoroutine(cewq_coroutine); // Stop it if one is already going
            cewq_main.SetActive(false);
        }

        if (showType)
        {
            cewq_coroutine = StartCoroutine(CEWG_Animate());
        }

        // Update the UI
        foreach (var I in InventoryControl.inst.interfaces)
        {
            foreach (KeyValuePair<GameObject, InventorySlot> kvp in I.slotsOnInterface)
            {
                InvDisplayItem idi = kvp.Key.GetComponent<InvDisplayItem>();

                if(idi.item != null)
                {
                    idi.SetRightSideDisplay(true);
                }
            }
        }
    }

    private Coroutine cewq_coroutine;
    private IEnumerator CEWG_Animate()
    {
        // Enable the main object
        cewq_main.SetActive(true);

        // Set the text
        string text = "";
        switch (cewq_mode)
        {
            case 0:
                text = "COVERAGE";
                break;
            case 1:
                text = "ENERGY";
                break;
            case 2:
                text = "INTEGRITY";
                break;
            case 3:
                text = "INFO";
                break;
        }
        cewq_text.text = text;

        // Wait some time
        yield return new WaitForSeconds(2f);

        // Disable the main object
        cewq_main.SetActive(false);
    }

    #endregion

    /// <summary>
    /// Updates the [ Inventory ] area on the UI
    /// </summary>
    public void UpdateInventory()
    {
        // - Inventory Size -
        string invSize = PlayerData.inst.currentInvCount.ToString() + " " + PlayerData.inst.maxInvSize.ToString();
        inventorySize_text.text = invSize;


    }

    private void SetAvoidanceImageColor(float amount, Image image)
    {
        amount = amount / 100f;

        if(amount >= 0.6 && amount <= 0.8) // Yellow (60-80%)
        {
            image.color = cautiousYellow;
        }
        else if(amount < 0.6 && amount > 0.3) // Orange (30-60%)
        {
            image.color = corruptOrange;
        }
        else if (amount < 0.3) // Red (< 30%)
        {
            image.color = dangerRed;
        }
        else // Blue (> 80%)
        {
            image.color = coolBlue;
        }
    }

    /// <summary>
    /// Assign keybinds to use each part in the part menu.
    /// </summary>
    public void AssignPartKeys()
    {
        InventoryControl.inst.SetInterfaceInvKeys();
    }

    #region Rightside - MAP
    [Header("MAP")]
    public TextMeshProUGUI map_buttonText;

    public void Map_HoverEnter()
    {
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HOVER"], 0.8f); // UI - HOVER
    }

    public void Map_HoverExit()
    {

    }

    public void Map_Open()
    {

    }

    public void Map_Close()
    {

    }
    #endregion

    #region Rightside - ESC
    [Header("ESC")]
    public TextMeshProUGUI esc_buttonText;

    public void ESC_HoverEnter()
    {
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HOVER"], 0.8f); // UI - HOVER
    }

    public void ESC_HoverExit()
    {

    }

    public void ESC_Open()
    {

    }

    public void ESC_Close()
    {

    }
    #endregion

    #endregion

    #region LAIC

    public void UpdateLAIC()
    {
        // - Set the Top Text -
        switch (currentActiveMenu_LAIC)
        {
            case "l": // Log
                topText_LAIC.text = "/LOG/";
                L_text.color = highlightGreen;
                A_text.color = dullGreen;
                I_text.color = dullGreen;
                C_text.color = dullGreen;
                secondLogParent.SetActive(true);
                intelAreaParent.SetActive(false);
                alliesAreaParent.SetActive(false);
                calcAreaParent.SetActive(false);
                break;

            case "a": // Allies
                topText_LAIC.text = "/ALLIES/";
                L_text.color = dullGreen;
                A_text.color = highlightGreen;
                I_text.color = dullGreen;
                C_text.color = dullGreen;
                secondLogParent.SetActive(false);
                intelAreaParent.SetActive(true);
                alliesAreaParent.SetActive(false);
                calcAreaParent.SetActive(false);
                break;

            case "i": // Intel
                topText_LAIC.text = "/INTEL/";
                L_text.color = dullGreen;
                A_text.color = dullGreen;
                I_text.color = highlightGreen;
                C_text.color = dullGreen;
                secondLogParent.SetActive(false);
                intelAreaParent.SetActive(false);
                alliesAreaParent.SetActive(true);
                calcAreaParent.SetActive(false);
                break;

            case "c": // Calculator
                topText_LAIC.text = "/CALC/";
                L_text.color = dullGreen;
                A_text.color = dullGreen;
                I_text.color = dullGreen;
                C_text.color = highlightGreen;
                secondLogParent.SetActive(false);
                intelAreaParent.SetActive(false);
                alliesAreaParent.SetActive(false);
                calcAreaParent.SetActive(true);
                break;
        }
    }

    public void SetActiveLAICMenu(string identifier)
    {
        currentActiveMenu_LAIC = identifier;
        UpdateLAIC();
    }
    #endregion

    #region LOG
    // - LOG -
    [Header("LOG")]
    public GameObject secondLogArea;
    public GameObject secondLogParent;
    public List<GameObject> logMessages2 = new List<GameObject>();

    
    /// <summary>
    /// Creates a message in the top middle "Log" section
    /// </summary>
    /// <param name="newMessage">The message to display (will be edited)</param>
    /// <param name="desiredColor">The color this message should be.</param>
    /// <param name="noTime">If a timestamp should be added to this message</param>
    public void CreateNewSecondaryLogMessage(string newMessage, Color desiredColor, Color desiredHighlight, bool noTime = false, bool hasAudio = false)
    {
        // First determine the TIME of when this message happened
        int currentTime = TurnManager.inst.globalTime;
        int digitCount = currentTime.ToString().Length;

        string displayTime = "";

        switch (digitCount) // 5 digits max, convert it to a string
        {
            case 1: // 1 Digit
                displayTime = "0000";
                displayTime += currentTime.ToString();
                break;

            case 2: // 2 Digits
                displayTime = "000";
                displayTime += currentTime.ToString();
                break;

            case 3: // 3 Digits
                displayTime = "00";
                displayTime += currentTime.ToString();
                break;

            case 4: // 4 Digits
                displayTime = "0";
                displayTime += currentTime.ToString();
                break;

            case 5: // 5 Digits
                displayTime = currentTime.ToString();
                break;
        }

        displayTime += "_ "; // Add an "_ " at the end

        string finalMessage = "";
        if (!noTime)
        {
            finalMessage = displayTime += newMessage; // Combine for final message
        }
        else
        {
            finalMessage = newMessage;
        }

        // -- Now Actually Create the Message --
        // Instantiate it & Assign it to left area
        GameObject newLogMessage = Instantiate(logMessage_prefab, secondLogArea.transform.position, Quaternion.identity);
        newLogMessage.transform.SetParent(secondLogArea.transform);
        newLogMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        logMessages2.Add(newLogMessage);
        //logFull.Add(finalMessage);
        // Assign Details
        newLogMessage.GetComponent<UILogMessage>().Setup(finalMessage, desiredColor, desiredHighlight, hasAudio);

    }
    // -     -
    #endregion

    #region INTEL
    // - INTEL -
    [Header("INTEL")]
    public GameObject intelArea;
    public GameObject intelAreaParent;
    public List<GameObject> intelMessages = new List<GameObject>();


    // -       -
    #endregion

    #region ALLIES
    // - ALLIES -
    [Header("ALLIES")]
    public GameObject alliesArea;
    public GameObject alliesAreaParent;
    public List<GameObject> alliesMessages = new List<GameObject>();


    // -        -
    #endregion

    #region CALC
    // - CALC -
    [Header("Calc")]
    public GameObject calcArea;
    public GameObject calcAreaParent;
    public List<GameObject> calcMessages = new List<GameObject>();

    /// <summary>
    /// Creates a message in the top middle "Calc" section
    /// </summary>
    /// <param name="newMessage">The message to display (will be edited)</param>
    /// <param name="desiredColor">The color this message should be.</param>
    /// <param name="noTime">If a timestamp should be added to this message</param>
    public void CreateNewCalcMessage(string newMessage, Color desiredColor, Color desiredHighlight, bool noTime = false, bool hasAudio = false)
    {
        // First determine the TIME of when this message happened
        int currentTime = TurnManager.inst.globalTime;
        int digitCount = currentTime.ToString().Length;

        string displayTime = "";

        switch (digitCount) // 5 digits max, convert it to a string
        {
            case 1: // 1 Digit
                displayTime = "0000";
                displayTime += currentTime.ToString();
                break;

            case 2: // 2 Digits
                displayTime = "000";
                displayTime += currentTime.ToString();
                break;

            case 3: // 3 Digits
                displayTime = "00";
                displayTime += currentTime.ToString();
                break;

            case 4: // 4 Digits
                displayTime = "0";
                displayTime += currentTime.ToString();
                break;

            case 5: // 5 Digits
                displayTime = currentTime.ToString();
                break;
        }

        displayTime += "_ "; // Add an "_ " at the end

        string finalMessage = "";
        if (!noTime)
        {
            finalMessage = displayTime += newMessage; // Combine for final message
        }
        else
        {
            finalMessage = newMessage;
        }

        // -- Now Actually Create the Message --
        // Instantiate it & Assign it to left area
        GameObject newLogMessage = Instantiate(logMessage_prefab, calcArea.transform.position, Quaternion.identity);
        newLogMessage.transform.SetParent(calcArea.transform);
        newLogMessage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        calcMessages.Add(newLogMessage);
        //logFull.Add(finalMessage);
        // Assign Details
        newLogMessage.GetComponent<UILogMessage>().Setup(finalMessage, desiredColor, desiredHighlight, hasAudio);

    }
    // -      -
    #endregion

    #region Ambient SFX Menu
    // Not yet implemented, see: https://www.gridsagegames.com/blog/2020/06/audio-accessibility-features-roguelikes/
    #endregion

    #region DATA Menu (Item/Bot info display)
    [Header("/ DATA / Menu")]
    public UIDataDisplay dataMenu;

    public void Data_OpenMenu(Item item = null, GameObject other = null, Actor itemOwner = null)
    {
        // Firstly assign values
        Actor bot = null;
        MachinePart machine = null;
        TileBlock tile = null;

        if(other != null)
        {
            if (other.GetComponent<Actor>())
            {
                bot = other.GetComponent<Actor>();
            }
            else if (other.GetComponent<MachinePart>())
            {
                machine = other.GetComponent<MachinePart>().parentPart;
            }
            else if (other.GetComponent<TileBlock>())
            {
                tile = other.GetComponent<TileBlock>();
                // But we only want walls & doors
                if (tile.tileInfo.type == TileType.Floor || tile.tileInfo.type == TileType.Machine || tile.tileInfo.type == TileType.Exit || tile.tileInfo.type == TileType.Default)
                {
                    tile = null;
                }
            }
        }

        // Emergency stop incase this menu is being spammed
        dataMenu.data_superImageParent.SetActive(false);

        StopCoroutine(Data_CloseMenuAnimation());
        StopCoroutine(DataAnim_TitleOpen());
        StopCoroutine(DataAnim_SmallImageOpen());
        StopCoroutine(DataAnim_TitleTextClose());

        // Before we assign the selection objects, we need to check if this menu is already open.
        // Because if it is, we need to do a quick clear and refresh animation instead.
        // And if its and item, and we previous had an item, we need to open the comparison menu.
        if (dataMenu.data_parent.activeInHierarchy)
        {
            // Clear out all detail text that was spawned in (no animations)
            foreach (var O in dataMenu.data_objects.ToList())
            {
                Destroy(O.gameObject);
            }
            dataMenu.data_objects.Clear();

            // Was the previous selected object an item? (and its not the exact same thing, and its not a unique item)
            if(item != null && dataMenu.selection_item != null && dataMenu.selection_item != item && dataMenu.selection_item.itemData.slot == item.itemData.slot
                && !item.itemData.instantUnique && item.itemData.knowByPlayer && dataMenu.selection_item.itemData.knowByPlayer)
            {
                dataMenu.selected_comparison_item = item; // Assign the item

                // Then we need to open the comparison sidebar!
                dataMenu.data_comparisonParent.SetActive(true);

                // Activate the animation
                dataMenu.data_ComparisonAnimator.Play("Data_ComparisonOpen");

                // Populate the comparison area with details
                #region Comparison Logic
                Item item1 = dataMenu.selection_item;
                Item item2 = dataMenu.selected_comparison_item;
                /*
                 *  ====== HOW THIS WORKS =====
                 *  We are comaring the stats of item1 against item2.
                 *  What this means is that if item1 has an integrity of 100,
                 *  and item2 has an integrity of 45.
                 *  We display a "-55" in the comparison sidebar,
                 *  since the 2nd item (which we are now viewing),
                 *  has a lower integrity.
                 *  
                 *  If the values are the same, we don't display anything.
                 */


                // - Firstly, when comparing two items, we start at mass in the overview section, which all items have.
                int diff = 0;
                string text = "";

                #region Overview
                // Mass
                (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.mass, item2.itemData.mass); // inverted (positive is worse)
                if (diff < 0) // Red (item1 is better)
                {
                    UIManager.inst.Data_CreateComparison(false, text);
                }
                else if (diff > 0) // Green (item2 is better)
                {
                    UIManager.inst.Data_CreateComparison(true, text);
                }
                else
                {
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Rating
                (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.rating, item2.itemData.rating);
                if (diff > 0) // Red (item1 is better)
                {
                    UIManager.inst.Data_CreateComparison(false, text);
                }
                else if (diff < 0) // Green (item2 is better)
                {
                    UIManager.inst.Data_CreateComparison(true, text);
                }
                else
                {
                    // Consider other special types
                    int A = 0;
                    int B = 0;
                    if(item1.itemData.ratingType == ItemRatingType.Standard)
                    {
                        A = 0;
                    }
                    else if (item1.itemData.ratingType == ItemRatingType.Prototype)
                    {
                        A = 1;
                    }
                    else if (item1.itemData.ratingType == ItemRatingType.Alien)
                    {
                        A = 2;
                    }

                    if (item2.itemData.ratingType == ItemRatingType.Standard)
                    {
                        B = 0;
                    }
                    else if (item2.itemData.ratingType == ItemRatingType.Prototype)
                    {
                        B = 1;
                    }
                    else if (item2.itemData.ratingType == ItemRatingType.Alien)
                    {
                        B = 2;
                    }

                    if(A > B)
                    {
                        UIManager.inst.Data_CreateComparison(false, "*");
                    }
                    else if(B > A)
                    {
                        UIManager.inst.Data_CreateComparison(true, "*");
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                }
                // Integrity
                (text, diff) = HF.GetDifferenceAsStringAndValue(item1.integrityCurrent, item2.integrityCurrent);
                if (diff > 0) // Red (item1 is better)
                {
                    UIManager.inst.Data_CreateComparison(false, text);
                }
                else if(diff < 0) // Green (item2 is better)
                {
                    UIManager.inst.Data_CreateComparison(true, text);
                }
                else
                {
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Coverage
                (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.coverage, item2.itemData.coverage);
                if (diff > 0) // Red (item1 is better)
                {
                    UIManager.inst.Data_CreateComparison(false, text);
                }
                else if (diff < 0) // Green (item2 is better)
                {
                    UIManager.inst.Data_CreateComparison(false, text);
                }
                else
                {
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                if (item2.itemData.schematicDetails.hasSchematic) // Need another spacer
                {
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // State (empty)
                UIManager.inst.Data_CreateComparison(false, "EMPTY");
                // (Leave two more empties as spacers for the next category)
                UIManager.inst.Data_CreateComparison(false, "EMPTY");
                UIManager.inst.Data_CreateComparison(false, "EMPTY");
                #endregion
                // - Then after overview, we need to consider if the two items have similar subsections (we don't do effects)
                // Upkeep
                if (item1.itemData.hasUpkeep && item2.itemData.hasUpkeep)
                {
                    // Energy
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.energyUpkeep, item2.itemData.energyUpkeep); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Matter
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.matterUpkeep, item2.itemData.matterUpkeep); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Heat
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.heatUpkeep, item2.itemData.heatUpkeep); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    // and 2 spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.hasUpkeep)
                {
                    // Create 5 spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // (Power) Supply
                if (item1.itemData.supply > 0 && item2.itemData.supply > 0)
                {
                    // Supply
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.supply, item2.itemData.supply);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Storage
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.storage, item2.itemData.storage);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Stability
                    if(item1.itemData.power_HasStability && item2.itemData.power_HasStability)
                    {
                        (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.power_stability, item2.itemData.power_stability);
                        if (diff > 0) // Red (item1 is better)
                        {
                            UIManager.inst.Data_CreateComparison(false, text);
                        }
                        else if (diff < 0) // Green (item2 is better)
                        {
                            UIManager.inst.Data_CreateComparison(true, text);
                        }
                        else
                        {
                            UIManager.inst.Data_CreateComparison(false, "EMPTY");
                        }
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.supply > 0)
                {
                    // Create 5 spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Propulsion
                if (item1.itemData.propulsion.Count > 0 && item2.itemData.propulsion.Count > 0)
                {
                    // Time/Move
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].timeToMove, item2.itemData.propulsion[0].timeToMove); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Drag
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].drag, item2.itemData.propulsion[0].drag);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Energy
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].propEnergy, item2.itemData.propulsion[0].propEnergy); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Heat
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].propHeat, item2.itemData.propulsion[0].propHeat); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Support
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].support, item2.itemData.propulsion[0].support);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Penalty
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].penalty, item2.itemData.propulsion[0].penalty); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Burnout
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].burnout, item2.itemData.propulsion[0].burnout);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Siege (conditional)
                    if(item1.itemData.type == ItemType.Treads && item1.itemData.type == ItemType.Treads)
                    {
                        (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.propulsion[0].canSiege, item2.itemData.propulsion[0].canSiege);
                        if (diff > 0) // Red (item1 is better)
                        {
                            UIManager.inst.Data_CreateComparison(false, "*");
                        }
                        else if (diff < 0) // Green (item2 is better)
                        {
                            UIManager.inst.Data_CreateComparison(true, "*");
                        }
                        else
                        {
                            UIManager.inst.Data_CreateComparison(false, "EMPTY");
                        }
                    }
                    else if (item.itemData.type == ItemType.Treads) // Don't wanna mess up the spacing
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.propulsion.Count > 0)
                {
                    // Create spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    if(item.itemData.type == ItemType.Treads) // Siege
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Cover the gap + header
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Shot
                if (item1.itemData.shot.shotRange > 0 && item2.itemData.shot.shotRange > 0)
                {
                    // Range
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotRange, item2.itemData.shot.shotRange);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Energy
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotEnergy, item2.itemData.shot.shotEnergy); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Matter
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotMatter, item2.itemData.shot.shotMatter); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Heat
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotHeat, item2.itemData.shot.shotHeat); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Recoil
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotRecoil, item2.itemData.shot.shotRecoil);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Targeting
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotTargeting, item2.itemData.shot.shotTargeting);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Delay
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotDelay, item2.itemData.shot.shotDelay); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Stability
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotStability, item2.itemData.shot.shotStability);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Arc
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.shot.shotArc, item2.itemData.shot.shotArc);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.shot.shotRange > 0)
                {
                    // Create spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Projectile
                if (item1.itemData.projectile.damage.x > 0 && item2.itemData.projectile.damage.x > 0)
                {
                    // Damage (semi different text since its a range)
                    Vector2Int diffV2 = new Vector2Int((int)item1.itemData.projectile.damage.x - (int)item2.itemData.projectile.damage.x, (int)item1.itemData.projectile.damage.y - (int)item2.itemData.projectile.damage.y);
                    text = "";
                    if(diffV2.x > 0)
                    {
                        text = "+";
                    }
                    text += diffV2.x + "/";
                    if (diffV2.y > 0)
                    {
                        text += "+";
                    }
                    text += diffV2.y;

                    if (diffV2.x > 0 || diffV2.y > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diffV2.x < 0 || diffV2.y < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Type (special text)
                    if(item1.itemData.projectile.damageType == item2.itemData.projectile.damageType)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    else
                    {
                        text = HF.ShortenDamageType(item1.itemData.projectile.damageType);
                        UIManager.inst.Data_CreateComparison(true, text, true);
                    }
                    // Critical (special text)
                    if (item1.itemData.projectile.critChance == item2.itemData.projectile.critChance)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    else
                    {
                        string typeC = item1.itemData.projectile.critType.ToString();
                        if(typeC.Length > 5)
                            typeC = typeC.Substring(0, 5); // Shorten crit type
                        text = typeC;
                        UIManager.inst.Data_CreateComparison(true, text, true);
                    }
                    // Penetration
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.projectile.penetrationCapability, item2.itemData.projectile.penetrationCapability);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Heat Transfer (conditional)
                    if (item1.itemData.projectile.heatTrasfer > 0 && item2.itemData.projectile.heatTrasfer > 0) // inverted (positive is worse)
                    {
                        (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.projectile.heatTrasfer, item2.itemData.projectile.heatTrasfer);
                        if (diff < 0) // Red (item1 is better)
                        {
                            UIManager.inst.Data_CreateComparison(false, text);
                        }
                        else if (diff > 0) // Green (item2 is better)
                        {
                            UIManager.inst.Data_CreateComparison(true, text);
                        }
                    }
                    else if (item.itemData.projectile.heatTrasfer > 0)
                    {
                        UIManager.inst.Data_CreateComparison(true, "*");
                    }
                    // Spectrum (conditional & special text)
                    if (item1.itemData.projectile.hasSpectrum && item2.itemData.projectile.hasSpectrum)
                    {
                        text = ((item2.itemData.projectile.spectrum * 100) - (item1.itemData.projectile.spectrum * 100)).ToString(); // not too sure why this is swapped?

                        UIManager.inst.Data_CreateComparison(false, text, true);
                    }
                    else if (item.itemData.projectile.hasSpectrum)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    // Disruption
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.projectile.disruption, item2.itemData.projectile.disruption);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Salvage
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.projectile.salvage, item2.itemData.projectile.salvage);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.projectile.damage.x > 0)
                {
                    // Create spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Explosion
                if (item1.itemData.explosionDetails.radius > 0 && item2.itemData.explosionDetails.radius > 0)
                {
                    // Radius
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.explosionDetails.radius, item2.itemData.explosionDetails.radius);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Damage (semi different text since its a range)
                    Vector2Int diffV2 = new Vector2Int((int)item1.itemData.explosionDetails.damage.x - (int)item2.itemData.explosionDetails.damage.x, (int)item1.itemData.explosionDetails.damage.y - (int)item2.itemData.explosionDetails.damage.y);
                    if (diffV2.x > 0)
                    {
                        text = "+ ";
                    }
                    text += diffV2.x + "/";
                    if (diffV2.y > 0)
                    {
                        text = "+ ";
                    }
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Falloff
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.explosionDetails.fallOff, item2.itemData.explosionDetails.fallOff);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Type (special text)
                    if (item1.itemData.explosionDetails.damageType == item2.itemData.explosionDetails.damageType)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    else
                    {
                        text = HF.ShortenDamageType(item1.itemData.explosionDetails.damageType);
                        UIManager.inst.Data_CreateComparison(true, text, true);
                    }

                    // Spectrum (conditional & special text)
                    if (item1.itemData.explosionDetails.hasSpectrum && item2.itemData.explosionDetails.hasSpectrum)
                    {
                        text = "(" + ((item2.itemData.explosionDetails.spectrum * 100) - (item1.itemData.explosionDetails.spectrum * 100)) + ")"; // not too sure why this is swapped?

                        UIManager.inst.Data_CreateComparison(false, text, true);
                    }
                    else if (item.itemData.explosionDetails.hasSpectrum)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }

                    // Disruption
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.explosionDetails.disruption, item2.itemData.explosionDetails.disruption);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Salvage
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.explosionDetails.salvage, item2.itemData.explosionDetails.salvage);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.explosionDetails.radius > 0)
                {
                    // Create spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                // Melee (Attack & Hit)
                if (item1.itemData.meleeAttack.isMelee && item2.itemData.meleeAttack.isMelee)
                {
                    // Energy
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.energy, item2.itemData.meleeAttack.energy); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Matter
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.matter, item2.itemData.meleeAttack.matter); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Heat
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.heat, item2.itemData.meleeAttack.heat); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Targeting
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.targeting, item2.itemData.meleeAttack.targeting);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // Delay
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.delay, item2.itemData.meleeAttack.delay); // inverted (positive is worse)
                    if (diff < 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff > 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    else
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    // --- Two spacers ---
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    // Damage (semi different text since its a range)
                    Vector2Int diffV2 = new Vector2Int((int)item1.itemData.meleeAttack.damage.x - (int)item2.itemData.meleeAttack.damage.x, (int)item1.itemData.meleeAttack.damage.y - (int)item2.itemData.meleeAttack.damage.y);
                    if (diffV2.x > 0)
                    {
                        text = "+ ";
                    }
                    text += diffV2.x + "/";
                    if (diffV2.y > 0)
                    {
                        text = "+ ";
                    }
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Type (special text)
                    if (item1.itemData.meleeAttack.damageType == item2.itemData.meleeAttack.damageType)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    else
                    {
                        text = HF.ShortenDamageType(item1.itemData.meleeAttack.damageType);
                        UIManager.inst.Data_CreateComparison(true, text, true);
                    }
                    // Critical (special text)
                    if (item1.itemData.meleeAttack.critical == item2.itemData.meleeAttack.critical)
                    {
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    }
                    else
                    {
                        string typeC = item1.itemData.meleeAttack.critType.ToString();
                        typeC = typeC.Substring(0, 5); // Shorten crit type
                        text = typeC;
                        UIManager.inst.Data_CreateComparison(true, text, true);
                    }

                    // Heat Transfer (conditional)
                    if (HF.GetHeatTransfer(item1) > 0 && HF.GetHeatTransfer(item2) > 0) // inverted (positive is worse)
                    {
                        (text, diff) = HF.GetDifferenceAsStringAndValue(HF.GetHeatTransfer(item1), HF.GetHeatTransfer(item2));
                        if (diff < 0) // Red (item1 is better)
                        {
                            UIManager.inst.Data_CreateComparison(false, text);
                        }
                        else if (diff > 0) // Green (item2 is better)
                        {
                            UIManager.inst.Data_CreateComparison(true, text);
                        }
                    }
                    else if (HF.GetHeatTransfer(item) > 0)
                    {
                        UIManager.inst.Data_CreateComparison(true, "*");
                    }
                    // Disruption
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.disruption, item2.itemData.meleeAttack.disruption);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }
                    // Salvage
                    (text, diff) = HF.GetDifferenceAsStringAndValue(item1.itemData.meleeAttack.salvage, item2.itemData.meleeAttack.salvage);
                    if (diff > 0) // Red (item1 is better)
                    {
                        UIManager.inst.Data_CreateComparison(false, text);
                    }
                    else if (diff < 0) // Green (item2 is better)
                    {
                        UIManager.inst.Data_CreateComparison(true, text);
                    }

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                else if(item.itemData.meleeAttack.isMelee)
                {
                    // Create spacers
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    if(item.itemData.meleeAttack.isMelee && HF.GetHeatTransfer(item) > 0)
                        UIManager.inst.Data_CreateComparison(false, "EMPTY");

                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                    UIManager.inst.Data_CreateComparison(false, "EMPTY");
                }
                #endregion
            }
        }
        else
        {
            dataMenu.selection_item = item;
        }

        dataMenu.selection_obj = other;

        // Enable the menu
        dataMenu.data_parent.SetActive(true);
        dataMenu.data_contentArea.SetActive(true);
        dataMenu.data_smallImage.transform.parent.gameObject.SetActive(true);

        // Play a sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["OPEN_1"]); // UI - OPEN_1

        // There isn't actually an opening animation for the menu itself (thankfully).
        // But most items inside the menu do have their own animations, which we will handle inside their own scripts.

        // What are we focusing on, an item, bot, machine, or tile? (Spawn in things respective to what info we are given)
        if(item != null)
        {
            // Set the big image (and enable it)
            dataMenu.data_superImageParent.gameObject.SetActive(true);
            Sprite itemSprite = item.itemData.bigDisplay; // Get the sprite art
            Sprite itemSprite_bw = HF.GetBlackAndWhiteSprite(itemSprite); // Get the black & white variant for the opening animation

            dataMenu.data_superImage.sprite = itemSprite_bw; // Swapped cause i messed something up
            dataMenu.data_superImageBW.sprite = itemSprite;

            // Set title to item's name
            dataMenu.data_mainTitle.text = HF.GetFullItemName(item);
            // Set title image to item's (world) image
            dataMenu.data_smallImage.sprite = item.itemData.inventoryDisplay;
            dataMenu.data_smallImage.color = item.itemData.itemColor;

            if (!item.itemData.knowByPlayer) // Prototype considerations
            {
                dataMenu.data_smallImage.color = Color.white;
                dataMenu.data_superImageBW.sprite = null;
                dataMenu.data_superImage.sprite = null;
                dataMenu.data_mainTitle.text = HF.ItemPrototypeName(item);
            }

            // Before we get into the item detail displaying, lets check if this is a "special" item, like matter, scrap, a data cache, etc.
            // Because if so we don't really display anything, and there are things likes N/A's and whatnot.
            if (item.itemData.instantUnique)
            {
                // Alter the title to include the amount (if it has any)
                if(item.amount > 1)
                    dataMenu.data_mainTitle.text = item.amount + " " + item.Name;
                if (item.itemData.type == ItemType.Alien && !item.itemData.knowByPlayer) // Unknown alien artifact
                    dataMenu.data_mainTitle.text = "Alien Artifact";

                // Overview
                UIManager.inst.Data_CreateHeader("Overview"); // Overview =========================================================================

                // -- A bit of variation here --
                if(item.itemData.type == ItemType.Alien && !item.itemData.knowByPlayer) // Alien artifacts
                {
                    // Type
                    UIDataGenericDetail iType = UIManager.inst.Data_CreateGeneric();
                    iType.Setup(true, false, false, "Type", Color.white, "General classification of this item.", "", false, "Artifact");
                    // Slot
                    UIDataGenericDetail iSlot = UIManager.inst.Data_CreateGeneric();
                    string xtra = "";
                    if (item.itemData.slotsRequired > 1)
                    {
                        xtra = " x" + item.itemData.slotsRequired;
                    }
                    iSlot.Setup(true, false, false, "Slot", Color.white, "Type of slot the part can be attached to, if applicable. Larger items may occupy multipel slots, both in the inventory and when attached.", "", false, item.itemData.slot + xtra);

                    // Also turn off the big image and change the null text
                    dataMenu.data_superImage.sprite = null;
                    dataMenu.data_superImageNull.GetComponent<TextMeshProUGUI>().text = "No Analysis Data";
                }
                else // Other stuff
                {
                    // Type
                    UIDataGenericDetail iType = UIManager.inst.Data_CreateGeneric();
                    iType.Setup(true, false, false, "Type", Color.white, "General classification of this item.", "", false, item.itemData.itemName);
                    // Slot
                    UIDataGenericDetail iSlot = UIManager.inst.Data_CreateGeneric();
                    iSlot.Setup(true, false, false, "Slot", Color.white, "Type of slot the part can be attached to, if applicable. Larger items may occupy multipel slots, both in the inventory and when attached.", "", false, "N/A", true);
                    // Mass
                    UIDataGenericDetail iMass = UIManager.inst.Data_CreateGeneric();
                    string extra = "Mass of an attached item contributes to a robot's total mass and affects its movement speed. Items held in inventory do not count towards total mass for movement calculation purposes.";
                    iMass.Setup(true, false, false, "Mass", Color.white, extra, "", false, "N/A", true);
                }

                // Then the textwall, which we take from the item's description.
                Data_CreateSpacer();

                if (item.itemData.type == ItemType.Alien && !item.itemData.knowByPlayer)
                { // Unknown alien artifact
                    Data_CreateTextWall("This is an unidentified alien artifact. Its properties are discovered by attaching it.");
                }
                else
                {
                    Data_CreateTextWall(item.itemData.description);
                }
            }
            else
            {

                // -- We have quite an extensive checklist here, there is a massive variance in what data an item can have -- //
                #region Data Displaying
                #region Overview
                UIManager.inst.Data_CreateHeader("Overview"); // Overview =========================================================================
                // Type
                UIDataGenericDetail iType = UIManager.inst.Data_CreateGeneric();
                iType.Setup(true, false, false, "Type", Color.white, "General classification of this item.", "", false, item.itemData.type.ToString());
                // Slot
                UIDataGenericDetail iSlot = UIManager.inst.Data_CreateGeneric();
                string xtra = "";
                if (item.itemData.slotsRequired > 1)
                {
                    xtra = " x" + item.itemData.slotsRequired;
                }
                iSlot.Setup(true, false, false, "Slot", Color.white, "Type of slot the part can be attached to, if applicable. Larger items may occupy multipel slots, both in the inventory and when attached.", "", false, item.itemData.slot.ToString() + xtra);

                if (item.itemData.knowByPlayer)
                {
                    // Mass
                    UIDataGenericDetail iMass = UIManager.inst.Data_CreateGeneric();
                    string extra = "Mass of an attached item contributes to a robot's total mass and affects its movement speed. Items held in inventory do not count towards total mass for movement calculation purposes.";
                    if (item.itemData.mass > 0)
                    {
                        iMass.Setup(true, false, true, "Mass", highlightGreen, extra, item.itemData.mass.ToString(), false, "", false, "", item.itemData.mass / 100f, true); // Uses the bar for some reason?
                    }
                    else // If the mass is 0, we display "N/A"
                    {
                        iMass.Setup(true, false, false, "Mass", Color.white, extra, "", false, "N/A", true);
                    }

                    // Rating - this guy actually has some variance
                    UIDataGenericDetail iRating = UIManager.inst.Data_CreateGeneric();
                    string rText = item.itemData.rating.ToString();
                    extra = "Rating is a relative indicator of the item's usefulness and/or value. When comparing items, the quickest and easiest method is to go with whatever has the highest rating. However, prototypes are almost always better than common parts with a similar rating (generally implying a +1 to their listed ratings)."; ;
                    if (item.itemData.star)
                        rText += "*";
                    // Options:
                    // -Standard, no box, faded green text
                    // -Prototype, green vBox
                    // -Alien, green vBox
                    // -Faulty (Prototype), red vBox
                    // -Construct, green vBox (see https://www.gridsagegames.com/blog/2022/10/the-scrap-engine/)
                    // -And potentially more
                    switch (item.itemData.ratingType)
                    {
                        case ItemRatingType.Standard: // Faded out "Standard"
                            iRating.Setup(true, false, false, "Rating", Color.white, extra, rText, false, "Standard", true);
                            break;
                        case ItemRatingType.Prototype: // Bright green vBox "Prototype"
                            iRating.Setup(true, true, false, "Rating", highlightGreen, extra, rText, false, "", false, "Prototype");
                            break;
                        case ItemRatingType.Alien: // Bright green vBox "Alien"
                            iRating.Setup(true, true, false, "Rating", highlightGreen, extra, rText, false, "", false, "Alien");
                            break;
                        case ItemRatingType.Faulty:
                            iRating.Setup(true, true, false, "Faulty Prototype", highSecRed, extra, rText, false, "", false, "Faulty Prototype");
                            break;
                        case ItemRatingType.Construct:
                            iRating.Setup(true, true, false, "Construct", highlightGreen, extra, "?", false, "", false, "Construct");
                            break;
                        default:
                            break;
                    }
                    // Integrity
                    UIDataGenericDetail iInteg = UIManager.inst.Data_CreateGeneric();
                    extra = "Integrity reflects the amount of damage a part can sustain before it is destroyed or rendered useless. Maximum integrity for a part type that cannot be repaired is preceded by a *.";
                    string integS = "";
                    if (item.itemData.repairable)
                    {
                        integS = item.integrityCurrent + " / " + item.itemData.integrityMax;
                    }
                    else
                    {
                        integS = item.integrityCurrent + " /*" + item.itemData.integrityMax;
                    }
                    iInteg.Setup(true, false, true, "Integrity", Color.white, extra, integS, false, "", false, "", item.integrityCurrent / item.itemData.integrityMax);
                    // Coverage
                    UIDataGenericDetail iCoverage = UIManager.inst.Data_CreateGeneric();
                    extra = "Coverage is a relative indicator of how likely this part is to be hit by a successful incoming attack compared to other attached parts. For example, a part with 100 coverage is twice" +
                        " as likely to be hit as another part with a value of 50. Armor, protective, and large parts tend to have higher coverage values while small subsystems provide less coverage but are therefore less likely to be damage.";
                    if (item.itemData.coverage > 0 && itemOwner != null)
                    { // CalculateItemCoverage
                        float coverage = HF.CalculateItemCoverage(itemOwner, item);
                        iCoverage.Setup(true, false, true, "Coverage", highlightGreen, extra, "", false, item.itemData.coverage.ToString() + " (" + coverage * 100 + "%)", false, "", coverage, true);
                    }
                    else
                    {
                        iCoverage.Setup(true, false, false, "Coverage", Color.white, extra, "", false, item.itemData.coverage.ToString()); // Just 0, you won't hit this. Nothing fancy. (Its probably in the inventory)
                    }
                    // Schematic
                    if (item.itemData.schematicDetails.hasSchematic)
                    {
                        extra = "A schematic allows this item to be fabricated at any fabricator provided you have enough matter to start the process.";
                        UIDataGenericDetail iSchematic = UIManager.inst.Data_CreateGeneric();
                        iSchematic.Setup(true, false, false, "Schematic", Color.white, extra, "", false, "Known", true);
                    }
                    // State - there is some variance in this
                    /*
                     *  NON-FUNCTIONAL : 
                     *  UNSTABLE       :
                     *  DISPOSABLE     :
                     *  CHARGING       :
                     *  OVERLOADED     :
                     *  DETERIORATING  :
                     *  RIGGED         :
                     *  FRAGILE        :
                     *  ACTIVE         :
                     *  INACTIVE       :
                     */
                    UIDataGenericDetail iState = UIManager.inst.Data_CreateGeneric();
                    if (item.isBroken) // NON-FUNCTIONAL (Red)
                    {
                        extra = "This component is completely non-functional, and must be repaired before being able to properly operate again.";
                        iState.Setup(true, true, false, "State", highSecRed, extra, "", false, "", false, "NON-FUNCTIONAL");
                    }
                    else if (item.unstable > 0) // Unstable
                    {
                        extra = "Unstable weapons implode after the indicated remaining number of shots.";
                        iState.Setup(true, true, false, "State", energyBlue, extra, "", false, "", false, "UNSTABLE " + item.unstable);
                    }
                    else if (item.disposable > 0) // Disposable
                    {
                        extra = "Disposable weapons become defunct after a certain number of uses.";
                        iState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "DISPOSABLE " + item.disposable);
                    }
                    else if (item.chargable > 0) // Charging
                    {
                        extra = "Chargable parts automatically recharge while attached, draining your energy at their charge rate for as many turns as necessary to fully recharge, but will never reduce your energy below 50% capacity.";
                        iState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "CHARGING");
                    }
                    else if (item.isOverloaded) // Overloaded (yellow)
                    {
                        extra = "Current state of this item. This item is currently overloaded, providing double effectiveness, while requiring double energy, and producing three times the normal heat.";
                        iState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "OVERLOADED");
                    }
                    else if (item.isDeteriorating) // Deteriorating (yellow)
                    {
                        extra = "Current state of this item. This item is currently deteriorating. While active, it will lose integrity until eventually becoming permanently disabled. ";
                        iState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "DETERIORATING");
                    }
                    else if (item.isRigged) // Rigged (yellow)
                    {
                        extra = "A rigged power source has been converted into a proximity mine set to detonate when anyone hostile to the creator moves adjacent to it. These power sources " +
                            "can no longer be attached or used for their normal purpose, but can be safely picked up and relocated by anyone not deemed a threat. ";
                        iState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "RIGGED");
                    }
                    else if (item.itemData.destroyOnRemove) // Fragile (pear green)
                    {
                        string hex = "86B200";
                        Color pearGreen = Color.white;
                        if (ColorUtility.TryParseHtmlString(hex, out pearGreen))
                        {
                            // Convert hex to a usable color
                        }

                        extra = "Fragile parts are destroyed if removed after attaching them for use. While attached, these are also marked with a colon next to their letter in the parts list, as a reminder.";
                        iState.Setup(true, true, false, "State", pearGreen, extra, "", false, "", false, "FRAGILE");
                    }
                    else if (item.disabledTimer > 0)
                    {
                        extra = "Current state of this item. This item is currently disabled, likely from overheating. This item will soon finish rebooting.";
                        iState.Setup(true, true, false, "State", warningOrange, extra, "", false, "", false, "DISABLED");
                    }
                    else if (item.state) // Active (green)
                    {
                        extra = "Current state of this item. This message will provide more context for special states.";
                        iState.Setup(true, true, false, "State", highlightGreen, extra, "", false, "", false, "ACTIVE");
                    }
                    else // Inactive (gray)
                    {
                        extra = "Current state of this item. This item is inactive, and will not provide any useful effects, or function as normal.";
                        iState.Setup(true, true, false, "State", inactiveGray, extra, "", false, "", false, "INACTIVE");
                    }
                    #endregion

                    Data_CreateSpacer();

                    if (item.itemData.hasUpkeep)
                    {
                        UIManager.inst.Data_CreateHeader("Active Upkeep"); // Active Upkeep =========================================================================
                                                                           // Energy
                        UIDataGenericDetail iEnergyUpkeep = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy consumed each turn by this part while active.";
                        string iEU = "";
                        if (item.itemData.energyUpkeep < 0f)
                        {
                            iEU = "-" + Mathf.Abs(item.itemData.energyUpkeep).ToString() + "%";
                            iEnergyUpkeep.Setup(true, false, true, "Energy", Color.white, extra, iEU, false, "", false, "", item.itemData.energyUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.energyUpkeep > 0f)
                        {
                            iEU = "+" + Mathf.Abs(item.itemData.energyUpkeep).ToString() + "%";
                            iEnergyUpkeep.Setup(true, false, true, "Energy", Color.white, extra, iEU, false, "", false, "", item.itemData.energyUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.energyUpkeep == 0)
                        {
                            // Grayed out
                            iEnergyUpkeep.Setup(true, false, true, "Energy", Color.white, extra, "0", true, "", false, "", 0f); // Bar
                        }
                        // Matter
                        UIDataGenericDetail iMatterUpkeep = UIManager.inst.Data_CreateGeneric();
                        extra = "Matter consumed each turn by this part while active.";
                        string iMU = "";
                        if (item.itemData.matterUpkeep < 0f)
                        {
                            iMU = "-" + Mathf.Abs(item.itemData.matterUpkeep).ToString() + "%";
                            iMatterUpkeep.Setup(true, false, true, "Matter", Color.white, extra, iMU, false, "", false, "", item.itemData.matterUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.matterUpkeep > 0f)
                        {
                            iMU = "+" + Mathf.Abs(item.itemData.matterUpkeep).ToString() + "%";
                            iMatterUpkeep.Setup(true, false, true, "Matter", Color.white, extra, iMU, false, "", false, "", item.itemData.matterUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.matterUpkeep == 0)
                        {
                            // Grayed out
                            iMatterUpkeep.Setup(true, false, true, "Matter", Color.white, extra, "0", true, "", false, "", 0f); // Bar
                        }
                        // Heat
                        UIDataGenericDetail iHeatUpkeep = UIManager.inst.Data_CreateGeneric();
                        extra = "Heat produced each turn by this part while active.";
                        string iHU = "";
                        if (item.itemData.heatUpkeep < 0f)
                        {
                            iHU = "-" + Mathf.Abs(item.itemData.heatUpkeep).ToString();
                            iHeatUpkeep.Setup(true, false, true, "Heat", Color.white, extra, iHU, false, "", false, "", item.itemData.heatUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.heatUpkeep > 0f)
                        {
                            iHU = "+" + Mathf.Abs(item.itemData.heatUpkeep).ToString();
                            iHeatUpkeep.Setup(true, false, true, "Heat", Color.white, extra, iHU, false, "", false, "", item.itemData.heatUpkeep / 36f); // Bar
                        }
                        else if (item.itemData.heatUpkeep == 0)
                        {
                            // Grayed out
                            iHeatUpkeep.Setup(true, false, true, "Heat", Color.white, extra, "0", true, "", false, "", 0f); // Bar
                        }

                        Data_CreateSpacer();
                    }

                    if (item.itemData.supply > 0)
                    {
                        UIManager.inst.Data_CreateHeader("Power"); // Power =========================================================================
                                                                   // Supply
                        UIDataGenericDetail iSupply = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy generated each turn by this power source.";
                        iSupply.Setup(true, false, true, "Supply", highlightGreen, extra, "+" + item.itemData.supply, false, "", false, "", item.itemData.supply / 36f, true); // Bar
                                                                                                                                                                               // Storage
                        UIDataGenericDetail iStorage = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy storage capacity of this power source. Excess energy can be stored, but requires additional utilities.";
                        iStorage.Setup(true, false, true, "Storage", highlightGreen, extra, item.itemData.storage.ToString(), false, "", false, "", item.itemData.storage / 500f, true); // Bar
                                                                                                                                                                                         // Stability
                        UIDataGenericDetail iStability = UIManager.inst.Data_CreateGeneric();
                        extra = "Stability represents the ability of this power source to generate energy while overloaded without suffering any negative side-effects. " +
                            "Overloading doubles output and heat. If N/A, this power source cannot be overloaded; only cooled power sources support overloading.";
                        if (item.itemData.power_HasStability)
                        {
                            iStability.Setup(true, false, true, "Stability", Color.white, extra, (item.itemData.power_stability * 100) + "%", false, "", false, "", item.itemData.power_stability); // Bar
                        }
                        else
                        {
                            iStability.Setup(true, false, true, "Stability", Color.white, extra, "N/A", true, "", false, "", 0f, false); // Bar
                        }

                        Data_CreateSpacer();
                    }

                    if (item.itemData.propulsion.Count > 0)
                    {
                        UIManager.inst.Data_CreateHeader("Propulsion"); // Propulsion =========================================================================
                                                                        // Time/Move
                        UIDataGenericDetail iTimeMove = UIManager.inst.Data_CreateGeneric();
                        extra = "The amount of time required to move on space when unburdened and using only this type of propulsion. Where multiple active propulsion modules have different values, the average is used."; ;
                        Color iTMC = highlightGreen; // Color is inverted based on amount
                        if (item.itemData.propulsion[0].timeToMove > 100)
                        {
                            iTMC = highSecRed;
                        }
                        else if (item.itemData.propulsion[0].timeToMove < 50)
                        {
                            iTMC = highlightGreen;
                        }
                        else
                        {
                            iTMC = cautiousYellow;
                        }
                        iTimeMove.Setup(true, false, true, "Time/Move", iTMC, extra, item.itemData.propulsion[0].timeToMove.ToString(), false, "", false, "", item.itemData.propulsion[0].timeToMove / 200f, true); // Bar
                                                                                                                                                                                                                    // Drag
                        UIDataGenericDetail iDrag = UIManager.inst.Data_CreateGeneric();
                        extra = "Inactive non-airborne propulsion modify the movement time cost by this amouunt while airborne. However, inactive propulsion has no adverse effective on the speed of non-airborne propulsion, including core movement.";
                        iDrag.Setup(true, false, false, "Drag", Color.white, extra, item.itemData.propulsion[0].drag.ToString()); // No bar (simple)
                                                                                                                                  // Energy
                        UIDataGenericDetail iEnergyProp = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy consumed by this part each move, if active.";
                        Color iEP = highlightGreen;
                        if (item.itemData.propulsion[0].propEnergy <= 0)
                        {
                            iEP = highlightGreen;
                        }
                        else if (item.itemData.propulsion[0].propEnergy > 15)
                        {
                            iEP = highSecRed;
                        }
                        else
                        {
                            iEP = cautiousYellow;
                        }
                        iEnergyProp.Setup(true, false, true, "Energy", iEP, extra, item.itemData.propulsion[0].propEnergy.ToString(), false, "", false, "", item.itemData.propulsion[0].propEnergy + 10 / 20f, true); // Bar
                                                                                                                                                                                                                      // Heat
                        UIDataGenericDetail iHeatProp = UIManager.inst.Data_CreateGeneric();
                        extra = "Heat produced by this part each move, if active.";
                        Color iHP = highlightGreen;
                        if (item.itemData.propulsion[0].propHeat <= 5)
                        {
                            iHP = highlightGreen;
                        }
                        else if (item.itemData.propulsion[0].propHeat > 15)
                        {
                            iHP = highSecRed;
                        }
                        else
                        {
                            iHP = cautiousYellow;
                        }
                        if (item.itemData.propulsion[0].propHeat > 0)
                        {
                            iHeatProp.Setup(true, false, true, "Heat", iHP, extra, item.itemData.propulsion[0].propHeat.ToString(), false, "", false, "", item.itemData.propulsion[0].propHeat + 1 / 20f, true); // Bar
                        }
                        else
                        {
                            iHeatProp.Setup(true, false, true, "Heat", iHP, extra, "0", true, "", false, "", 0f, true); // Bar
                        }
                        // Support
                        UIDataGenericDetail iSupportProp = UIManager.inst.Data_CreateGeneric();
                        extra = "Mass supported by this part, if active.";
                        if (item.itemData.propulsion[0].support > 0)
                        {
                            iSupportProp.Setup(true, false, true, "Support", Color.white, extra, item.itemData.propulsion[0].support.ToString(), false, "", false, "", item.itemData.propulsion[0].support / 22f); // Bar
                        }
                        else
                        {
                            iSupportProp.Setup(true, false, true, "Support", Color.white, extra, "0", true, "", false, "", 0f); // Bar
                        }
                        // Penalty
                        UIDataGenericDetail iPenaltyProp = UIManager.inst.Data_CreateGeneric();
                        extra = "Movement time penalty for being overweight, applied once if overweight at all. Penalty values for multiple different propulsion modules are averaged for calculation purposes. " +
                            "Further exceeding the mass support limit gradually continue to reduce speed depending on the amount of excess mass.";
                        Color iPP = highlightGreen;
                        if (item.itemData.propulsion[0].penalty <= 10)
                        {
                            iPP = highlightGreen;
                        }
                        else if (item.itemData.propulsion[0].penalty > 45)
                        {
                            iPP = highSecRed;
                        }
                        else
                        {
                            iPP = cautiousYellow;
                        }
                        if (item.itemData.propulsion[0].penalty > 0)
                        {
                            iPenaltyProp.Setup(true, false, true, " Penalty", iPP, extra, item.itemData.propulsion[0].penalty.ToString(), false, "", false, "", item.itemData.propulsion[0].penalty / 60f, true); // Bar
                        }
                        else
                        {
                            iPenaltyProp.Setup(true, false, true, " Penalty", iPP, extra, "0", true, "", false, "", 0f, true); // Bar
                        }
                        // Burnout
                        if (item.itemData.propulsion[0].hasBurnout)
                        {
                            UIDataGenericDetail iBurnout = UIManager.inst.Data_CreateGeneric();
                            extra = "If this system is overloaded, it will gradually lose integrity determined by its burnout rate due to the strain of operating in this unintended state.";
                            Color iBP = highlightGreen;
                            if (item.itemData.propulsion[0].penalty <= 15)
                            {
                                iBP = highlightGreen;
                            }
                            else if (item.itemData.propulsion[0].penalty > 50)
                            {
                                iBP = highSecRed;
                            }
                            else
                            {
                                iBP = cautiousYellow;
                            }
                            if (item.itemData.propulsion[0].penalty > 0)
                            {
                                iBurnout.Setup(true, false, true, "Burnout", iBP, extra, item.itemData.propulsion[0].burnout.ToString(), false, "", false, "", item.itemData.propulsion[0].burnout / 70f, true); // Bar
                            }
                            else
                            {
                                iBurnout.Setup(true, false, true, "Burnout", iBP, extra, "N/A", true, "", false, "", 0f); // Bar
                            }
                        }
                        // Siege
                        if (item.itemData.type == ItemType.Treads)
                        {
                            UIDataGenericDetail iSiege = UIManager.inst.Data_CreateGeneric();
                            extra = "Entering or exiting siege mode requires 5 turns. During the transition, and for as long as the mode is active, Cogmind is immobile and the treads cannot be disabled or removed." +
                                " While in siege mode, non-melee attacks have a +20% accuracy, coverage for all armor and heavy treads is doubled, any treads in siege mode get a free 25% damage reduction, no " +
                                "weapons suffer from recoil effects, and Cogmind is immune to instant part destruction from critical hits. Treads capable of High siege mode instead give + 30% accuracy and have 50% damage resist.";
                            if (item.itemData.propulsion[0].canSiege == 1)
                            {
                                iSiege.Setup(true, false, false, "Siege", Color.white, extra, "", false, "Standard");
                            }
                            else if (item.itemData.propulsion[0].canSiege == 2)
                            {
                                iSiege.Setup(true, false, false, "Siege", Color.white, extra, "", false, "High");
                            }
                            else
                            {
                                iSiege.Setup(true, false, false, "Siege", Color.white, extra, "N/A", true);
                            }
                        }


                        // And then consider if there is a text wall after this
                        foreach (var E in item.itemData.itemEffects)
                        {
                            string textWall = "";

                            if (E.hasLegEffect) // Leg effect
                            {
                                if (E.extraKickChance > 0)
                                {
                                    textWall += " Each active leg slot provides a ";
                                    textWall += (E.extraKickChance * 100).ToString() + "% ";
                                    textWall += "chance to kick ";

                                    if (E.appliesToLargeTargets)
                                    {
                                        textWall += "non-huge ";
                                    }
                                    textWall += "targets out of the way. ";
                                }

                                if (E.conferToRunningState)
                                {
                                    textWall += "Moving on legs also confers the Running state, which increases evasion but decreases accuracy, each by ";
                                    textWall += (E.evasionNaccuracyChange * 100).ToString() + "% per level of momentup (up to ";
                                    textWall += E.maxMomentumAmount.ToString() + ").";
                                }

                                Data_CreateSpacer();
                                Data_CreateTextWall(textWall);
                            }

                            if (E.chainExplode) // Chain reaction explosion
                            {
                                textWall += " " + item.itemData.itemName + ": ";
                                textWall += "If triggered by chain reaction or rigged proximity response, explodes for ";
                                textWall += item.itemData.explosionDetails.damage.x + "-" + item.itemData.explosionDetails.damage.y;
                                textWall += " " + item.itemData.explosionDetails.damageType.ToString().ToLower();
                                textWall += " damage with a radius of ";
                                textWall += item.itemData.explosionDetails.radius + " (falloff: ";
                                textWall += item.itemData.explosionDetails.fallOff + "; chunks: ";
                                textWall += item.itemData.explosionDetails.chunks.x + "-" + item.itemData.explosionDetails.chunks.y;
                                textWall += "; salvage: " + item.itemData.explosionDetails.salvage.ToString();
                                textWall += "). ";
                                if (E.heatTransfer == 0)
                                {

                                }
                                else if (E.heatTransfer == 1)
                                {
                                    textWall += "Low heat transfer.";
                                }
                                else if (E.heatTransfer == 2)
                                {
                                    textWall += "Medium heat transfer.";
                                }
                                else if (E.heatTransfer == 3)
                                {
                                    textWall += "High heat transfer.";
                                }
                                else if (E.heatTransfer == 4)
                                {
                                    textWall += "Massive heat transfer.";
                                }

                                Data_CreateSpacer();
                                Data_CreateTextWall(textWall);
                            }

                            // TODO: Consider a couple more effects that appear like this
                        }

                        Data_CreateSpacer();
                    }

                    if (item.itemData.shot.shotRange > 0)
                    {
                        ItemShot shot = item.itemData.shot;
                        UIManager.inst.Data_CreateHeader("Shot"); // Shot =========================================================================
                                                                  // Range
                        UIDataGenericDetail iSRange = UIManager.inst.Data_CreateGeneric();
                        extra = "Maximum effective range of this weapon.";
                        iSRange.Setup(true, false, true, "Range", Color.white, extra, item.itemData.shot.shotRange.ToString(), false, "", false, "", item.itemData.shot.shotRange / 22f); // Bar
                                                                                                                                                                                          // Energy
                        UIDataGenericDetail iSEnergy = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy required to fire this weapon.";
                        if (shot.shotEnergy < 0) // Negative
                        {
                            float clampedValue = Mathf.Clamp(shot.shotEnergy, 0, -50);
                            float normalizedValue = Mathf.InverseLerp(0, -50, clampedValue);

                            iSEnergy.Setup(true, false, true, "Energy", Color.white, extra, shot.shotEnergy.ToString(), false, "", false, "", normalizedValue); // Bar
                        }
                        else
                        {
                            iSEnergy.Setup(true, false, true, "Energy", Color.white, extra, shot.shotEnergy.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Matter
                        UIDataGenericDetail iSMatter = UIManager.inst.Data_CreateGeneric();
                        extra = "Matter consumed when firing this weapon.";
                        if (shot.shotMatter < 0) // Negative
                        {
                            float clampedValue = Mathf.Clamp(shot.shotMatter, 0, -50);
                            float normalizedValue = Mathf.InverseLerp(0, -50, clampedValue);

                            iSMatter.Setup(true, false, true, "Matter", Color.white, extra, shot.shotMatter.ToString(), false, "", false, "", normalizedValue); // Bar
                        }
                        else
                        {
                            iSMatter.Setup(true, false, true, "Matter", Color.white, extra, shot.shotMatter.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Heat
                        UIDataGenericDetail iSHeat = UIManager.inst.Data_CreateGeneric();
                        extra = "Heat produced by firing this weapon. This value is averaged over the number of turns it takes to fire, and therefore not applied all at once.";
                        if (shot.shotHeat > 0)
                        {
                            Color iSC = highlightGreen;
                            if (shot.shotHeat > 60)
                            {
                                iSC = highSecRed;
                            }
                            else if (shot.shotHeat <= 20)
                            {
                                iSC = highlightGreen;
                            }
                            else
                            {
                                iSC = cautiousYellow;
                            }

                            iSHeat.Setup(true, false, true, "Heat", iSC, extra, "+" + shot.shotHeat.ToString(), false, "", false, "", shot.shotHeat / 100f, true); // Bar
                        }
                        else
                        {
                            iSHeat.Setup(true, false, true, "Heat", Color.white, extra, shot.shotHeat.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Recoil
                        UIDataGenericDetail iSR = UIManager.inst.Data_CreateGeneric();
                        extra = "Recoil causes any other weapons fired in the same volley to suffer this penalty to their accuracy.";
                        if (shot.shotRecoil > 0)
                        {
                            iSR.Setup(true, false, false, "Recoil", Color.white, extra, shot.shotRecoil.ToString()); // No bar (simple)
                        }
                        else
                        {
                            iSR.Setup(true, false, false, "Recoil", Color.white, extra, "0", true); // No bar (simple)
                        }
                        // Targeting
                        UIDataGenericDetail iST = UIManager.inst.Data_CreateGeneric();
                        extra = "This is a direct modifier to the weapon's accuracy calculation when firing. Some weapons are inherently easier or more difficult to accurately target with.";
                        if (shot.shotTargeting != 0)
                        {
                            iST.Setup(true, false, false, "Targeting", Color.white, extra, (shot.shotTargeting * 100) + "%"); // No bar (simple)
                        }
                        else
                        {
                            iST.Setup(true, false, false, "Targeting", Color.white, extra, "0%", true); // No bar (simple)
                        }
                        // Delay
                        UIDataGenericDetail iSD = UIManager.inst.Data_CreateGeneric();
                        extra = "This is a direct modifier to the time it takes to fire the weapon. Some weapons are inherently faster or slower to fire.";
                        if (shot.shotDelay > 0)
                        {
                            string iSD_text = "";
                            if (shot.shotDelay > 0)
                            {
                                iSD_text = "+";
                            }

                            iSD.Setup(true, false, false, "Delay", Color.white, extra, iSD_text += shot.shotDelay); // No bar (simple)
                        }
                        else
                        {
                            iSD.Setup(true, false, false, "Delay", Color.white, extra, "0", true); // No bar (simple)
                        }
                        // Stability
                        UIDataGenericDetail iSS = UIManager.inst.Data_CreateGeneric();
                        extra = "Stability represents the ability of this weapon to fire while overloaded without suffering any negative side-effects. Overloading doubles damage and energy cost, " +
                            "and generates triple the heat. If N/A, this weapon cannot be overloaded; only some energy weapons support overloading. While overloaded, heat transfer is one level high than" +
                            " usual where applicable.";
                        if (shot.hasStability)
                        {
                            iSS.Setup(true, false, true, "Stability", Color.white, extra, (shot.shotStability * 100) + "%", false, "", false, "", shot.shotStability); // Bar
                        }
                        else
                        {
                            iSS.Setup(true, false, false, "Stability", Color.white, extra, "N/A", true, "", false, "", 0f); // Empty bar
                        }
                        // Arc
                        UIDataGenericDetail iSA = UIManager.inst.Data_CreateGeneric();
                        extra = "Total angle within which projectiles are randomly distributed around the target, spreading them along an arc of a circle centered on the shot origin. Improved " +
                            "targeting has no effect on the spread, which is always centered around the line of fire.";
                        if (shot.hasArc)
                        {
                            iSA.Setup(true, false, false, "Arc", Color.white, extra, shot.shotArc.ToString()); // No bar (simple)
                        }
                        else
                        {
                            iSA.Setup(true, false, false, "Arc", Color.white, extra, "N/A", true); // No bar (simple)
                        }

                        Data_CreateSpacer();
                    }

                    if (item.itemData.projectile.damage.x > 0)
                    {
                        ItemProjectile proj = item.itemData.projectile;
                        string bonus = "";
                        if (item.itemData.projectileAmount > 1)
                        {
                            bonus = "x" + item.itemData.projectileAmount;
                        }

                        UIManager.inst.Data_CreateHeader("Projectile", bonus); // Projectile =========================================================================
                                                                               // Damage
                        UIDataGenericDetail pDamage = UIManager.inst.Data_CreateGeneric();
                        extra = "Range of potential damage at the point of impact.";
                        pDamage.Setup(true, false, true, "Damage", highlightGreen, extra, proj.damage.x + "-" + proj.damage.y, false, "", false, "", ((proj.damage.x + proj.damage.y) / 2) / 100f, true);
                        // Type
                        UIDataGenericDetail pType = UIManager.inst.Data_CreateGeneric();
                        extra = HF.DamageTypeToString(proj.damageType);
                        pType.Setup(true, false, false, "Type", Color.white, extra, "", false, proj.damageType.ToString());
                        // Critical
                        UIDataGenericDetail pCrit = UIManager.inst.Data_CreateGeneric();
                        extra = HF.CriticalEffectsToString(proj.critType);
                        if (proj.critChance > 0f)
                        {
                            pCrit.Setup(true, false, false, "Critical", Color.white, extra, (proj.critChance * 100).ToString() + "%", false, proj.critType.ToString());
                        }
                        else
                        {
                            pCrit.Setup(true, false, false, "Critical", Color.white, extra, "0%", true);
                        }
                        // Penetration
                        UIDataGenericDetail pPen = UIManager.inst.Data_CreateGeneric();
                        extra = "Chance this projectile may penetrate each consecutive object it hits, e.g. 100 / 80 /50 may pass through up to three objects, with a 100% chance for the first," +
                            " followed by 80% and 50^ for the remaining two. Huge robots count as a single object for penetration purposes.";
                        if (proj.penetrationCapability > 0)
                        {
                            string s = "";
                            foreach (var P in proj.penetrationChances)
                            {
                                s += (P * 100) + " / ";
                            }
                            s = s.Substring(0, s.Length - 2); // Remove the extra "/ "

                            pPen.Setup(true, false, false, "Penetration", Color.white, extra, "x" + proj.penetrationCapability, false, s);
                        }
                        else
                        {
                            pPen.Setup(true, false, false, "Penetration", Color.white, extra, "x0", true);
                        }
                        // Heat Transfer
                        if (item.itemData.itemEffects.Count > 0 && item.itemData.itemEffects[0].heatTransfer > 0)
                        {
                            UIDataGenericDetail iPHT = UIManager.inst.Data_CreateGeneric();
                            string s = "";
                            extra = "Thermal weapons attempt to transfer some % of the maximum heat transfer rating of the weapon, and also check for possible robot meltdown (the effect of which might be delayed until the robot's next turn).";
                            int ht = item.itemData.itemEffects[0].heatTransfer;
                            if (ht == 1)
                            {
                                s = "Low (25)";
                            }
                            else if (ht == 2)
                            {
                                s = "Medium (37)";
                            }
                            else if (ht == 3)
                            {
                                s = "High (50)";
                            }
                            else if (ht == 4)
                            {
                                s = "Massive (80)";
                            }

                            iPHT.Setup(true, false, false, "Heat Transfer", Color.white, extra, "", false, s);
                        }
                        // Spectrum
                        UIDataGenericDetail iPS = UIManager.inst.Data_CreateGeneric();
                        extra = "Range of electromagnetic spectrum, with narrower ranges more likely to trigger chain explosions in power sources. (Cogmind's power sources are immune to this effect from projectiles.)";
                        if (proj.hasSpectrum)
                        {
                            string s = "";
                            if (proj.spectrum <= 0.1f)
                            {
                                s = "Wide (10%)";
                            }
                            else if (proj.spectrum == 0.3f)
                            {
                                s = "Intermediate (30%)";
                            }
                            else if (proj.spectrum == 0.5f)
                            {
                                s = "Narrow (50%)";
                            }
                            else if (proj.spectrum == 1f)
                            {
                                s = "Fine (100%)";
                            }

                            iPS.Setup(true, false, false, "Specturm", Color.white, extra, s); // No bar (simple)
                        }
                        else
                        {
                            iPS.Setup(true, false, false, "Specturm", Color.white, extra, "N/A", true); // No bar (simple)
                        }
                        // Disruption
                        UIDataGenericDetail iPD = UIManager.inst.Data_CreateGeneric();
                        extra = "Chance this projectile may temporarily disable an active part on impact. If a robot core is struck, there is half thsi chance the entire robot may be disabled.";
                        if (proj.disruption > 0)
                        {
                            Color iPDC = highlightGreen;
                            if (proj.disruption < 0.33)
                            {
                                iPDC = highlightGreen;
                            }
                            else if (proj.disruption >= 0.66)
                            {
                                iPDC = highSecRed;
                            }
                            else
                            {
                                iPDC = cautiousYellow;
                            }

                            iPD.Setup(true, false, true, "Disruption", iPDC, extra, (proj.disruption * 100) + "%", false, "", false, "", proj.disruption, true); // Bar
                        }
                        else
                        {
                            iPD.Setup(true, false, true, "Disruption", Color.white, extra, "0%", true, "", false, "", 0f); // Bar

                        }
                        // Salvage
                        UIDataGenericDetail iPR = UIManager.inst.Data_CreateGeneric();
                        extra = "Amount by which the salvage potential of a given robot is affected each time shot by a projectile from this weapon. While usually negative, reducing the amount of " +
                            "usable salvage, some special weapons may increase salvage by disabling a robot with minimal collateral damage.";
                        if (proj.salvage > 0)
                        {
                            iPR.Setup(true, false, false, "Salvage", Color.white, extra, "+" + proj.salvage.ToString()); // No bar (simple)
                        }
                        else if (proj.salvage == 0)
                        {
                            iPR.Setup(true, false, false, "Salvage", Color.white, extra, "0", true); // No bar (simple)
                        }
                        else
                        {
                            iPR.Setup(true, false, false, "Salvage", Color.white, extra, proj.salvage.ToString()); // No bar (simple)
                        }

                        Data_CreateSpacer();
                    }

                    if (item.itemData.explosionDetails.radius > 0)
                    {
                        UIManager.inst.Data_CreateHeader("Explosion"); // Explosion =========================================================================
                        ExplosionGeneric detail = item.itemData.explosionDetails;
                        // Radius
                        UIDataGenericDetail iERadius = UIManager.inst.Data_CreateGeneric();
                        extra = "Maximum radius of the explosion from its origin.";
                        iERadius.Setup(true, false, true, "Radius", highlightGreen, extra, detail.radius.ToString(), false, "", false, "", detail.radius / 9f, true); // Bar
                                                                                                                                                                      // Damage
                        int average = Mathf.RoundToInt((detail.damage.x + detail.damage.y) / 2);
                        extra = "Range of potential damage at the explosion's origin.";
                        UIDataGenericDetail iEDamage = UIManager.inst.Data_CreateGeneric();
                        iEDamage.Setup(true, false, true, "Damage", highlightGreen, extra, detail.damage.x + "-" + detail.damage.y, false, "", false, "", average / 120f, true); // Bar
                                                                                                                                                                                 // Falloff
                        UIDataGenericDetail iEFalloff = UIManager.inst.Data_CreateGeneric();
                        extra = "Amount of damage potential lost per space as the explosion expands from its origin. While targeting, this falloff is represented visually by the AOE color's brightness " +
                            "relative to the origin (this feature can be toggled via Explosion Predictions option).";
                        if (detail.fallOff <= 0)
                        {
                            iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, detail.fallOff.ToString());
                        }
                        else
                        {
                            iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, "0", true);
                        }
                        // Chunks
                        if (detail.chunks.y > 1)
                        {
                            UIDataGenericDetail iEChunks = UIManager.inst.Data_CreateGeneric();
                            extra = "AOE damage is often spread across each target in the area of effect, dividing the damage into separate chunks before affecting a robot," +
                                " where each chunk of damage selects its own target part (though they may overlap). Some explosive effects have a static number of chunks, while " +
                                "others randomly select from within a range for each attack.";
                            iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, detail.fallOff.ToString());
                        }
                        // Type
                        UIDataGenericDetail iEType = UIManager.inst.Data_CreateGeneric();
                        extra = "While powerful, explosives generally spread damage across each target in the area of effect.\nExplosions also tend to reduce the amount of salvage remaining after destroying a target.";
                        iEType.Setup(true, false, false, "Type", Color.white, extra, "", false, detail.damageType.ToString());
                        // Spectrum
                        UIDataGenericDetail iESpectrum = UIManager.inst.Data_CreateGeneric();
                        extra = "Range of electromagnetic spectrum, with narrower ranges more likely to trigger chain explosions in power sources. (AOE spectrum only affects power sources being used by Cogmind, or those exposed on the ground.)";
                        if (detail.hasSpectrum)
                        {
                            string s = "";
                            if (detail.spectrum <= 0.1f)
                            {
                                s = "Wide (10%)";
                            }
                            else if (detail.spectrum == 0.3f)
                            {
                                s = "Intermediate (30%)";
                            }
                            else if (detail.spectrum == 0.5f)
                            {
                                s = "Narrow (50%)";
                            }
                            else if (detail.spectrum == 1f)
                            {
                                s = "Fine (100%)";
                            }

                            iESpectrum.Setup(true, false, false, "Specturm", Color.white, extra, s); // No bar (simple)
                        }
                        else
                        {
                            iESpectrum.Setup(true, false, false, "Specturm", Color.white, extra, "N/A", true); // No bar (simple)
                        }
                        // Disruption
                        UIDataGenericDetail iEDisruption = UIManager.inst.Data_CreateGeneric();
                        extra = "Chance this explosion may temporarily disable an active part it damages. If a robot core is struck, there is half this chance the entire robot may be disabled.";
                        if (detail.disruption > 0)
                        {
                            Color iPDC = highlightGreen;
                            if (detail.disruption < 0.33)
                            {
                                iPDC = highlightGreen;
                            }
                            else if (detail.disruption >= 0.66)
                            {
                                iPDC = highSecRed;
                            }
                            else
                            {
                                iPDC = cautiousYellow;
                            }

                            iEDisruption.Setup(true, false, true, "Disruption", iPDC, extra, (detail.disruption * 100) + "%", false, "", false, "", detail.disruption, true); // Bar
                        }
                        else
                        {
                            iEDisruption.Setup(true, false, true, "Disruption", Color.white, extra, "0%", true, "", false, "", 0f); // Bar

                        }
                        // Salvage
                        UIDataGenericDetail iESalvage = UIManager.inst.Data_CreateGeneric();
                        extra = "Amount by which the salvage potential of a given robot is affected by being engulfed in this explosion. While usually negative, reducing the amount of usable salvage, some special weapons may increase salvage by disabling a robot with minimal collateral damage.";
                        if (detail.fallOff <= 0)
                        {
                            iESalvage.Setup(true, false, false, "Salvage", Color.white, extra, detail.salvage.ToString());
                        }
                        else
                        {
                            iESalvage.Setup(true, false, false, "Salvage", Color.white, extra, "0", true);
                        }
                    }

                    if (item.itemData.meleeAttack.isMelee)
                    {
                        // This is split up into "attack" and "hit"
                        ItemMeleeAttack melee = item.itemData.meleeAttack;
                        UIManager.inst.Data_CreateHeader("Attack"); // Attack =========================================================================
                                                                    // Energy
                        UIDataGenericDetail iAEnergy = UIManager.inst.Data_CreateGeneric();
                        extra = "Energy required to attack with this weapon.";
                        if (melee.energy < 0) // Negative
                        {
                            float clampedValue = Mathf.Clamp(melee.energy, 0, -50);
                            float normalizedValue = Mathf.InverseLerp(0, -50, clampedValue);

                            iAEnergy.Setup(true, false, true, "Energy", Color.white, extra, melee.energy.ToString(), false, "", false, "", normalizedValue); // Bar
                        }
                        else
                        {
                            iAEnergy.Setup(true, false, true, "Energy", Color.white, extra, melee.energy.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Matter
                        UIDataGenericDetail iAMatter = UIManager.inst.Data_CreateGeneric();
                        extra = "Matter consumed when attacking with this weapon.";
                        if (melee.matter < 0) // Negative
                        {
                            float clampedValue = Mathf.Clamp(melee.matter, 0, -50);
                            float normalizedValue = Mathf.InverseLerp(0, -50, clampedValue);

                            iAMatter.Setup(true, false, true, "Matter", Color.white, extra, melee.matter.ToString(), false, "", false, "", normalizedValue); // Bar
                        }
                        else
                        {
                            iAMatter.Setup(true, false, true, "Matter", Color.white, extra, melee.matter.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Heat
                        UIDataGenericDetail iAHeat = UIManager.inst.Data_CreateGeneric();
                        extra = "Heat produced by attack with this weapon.";
                        if (melee.heat > 0)
                        {
                            Color iSC = highlightGreen;
                            if (melee.heat > 60)
                            {
                                iSC = highSecRed;
                            }
                            else if (melee.heat <= 20)
                            {
                                iSC = highlightGreen;
                            }
                            else
                            {
                                iSC = cautiousYellow;
                            }

                            iAHeat.Setup(true, false, true, "Heat", iSC, extra, "+" + melee.heat.ToString(), false, "", false, "", melee.heat / 100f, true); // Bar
                        }
                        else
                        {
                            iAHeat.Setup(true, false, true, "Heat", Color.white, extra, melee.heat.ToString(), true, "", false, "", 0f); // Bar
                        }
                        // Targeting
                        UIDataGenericDetail iATargeting = UIManager.inst.Data_CreateGeneric();
                        extra = "This is a direct modifier to the weapon's accuracy when attacking with it. Some weapons are inherently easier or more difficult to accurately strike with.";
                        if (melee.targeting != 0)
                        {
                            iATargeting.Setup(true, false, false, "Targeting", Color.white, extra, (melee.targeting * 100) + "%"); // No bar (simple)
                        }
                        else
                        {
                            iATargeting.Setup(true, false, false, "Targeting", Color.white, extra, "0%", true); // No bar (simple)
                        }
                        // Delay
                        UIDataGenericDetail iADelay = UIManager.inst.Data_CreateGeneric();
                        extra = "This is a modifier to the time it takes to attack with this weapon. Some weapons are inherently faster or slower to attack with.";
                        if (melee.delay > 0)
                        {
                            string iSD_text = "";
                            if (melee.delay > 0)
                            {
                                iSD_text = "+";
                            }

                            iADelay.Setup(true, false, false, "Delay", Color.white, extra, iSD_text += melee.delay); // No bar (simple)
                        }
                        else
                        {
                            iADelay.Setup(true, false, false, "Delay", Color.white, extra, "0", true); // No bar (simple)
                        }

                        Data_CreateSpacer();
                        if (!item.itemData.specialAttack.specialMelee)
                        {
                            UIManager.inst.Data_CreateHeader("Hit"); // Hit =========================================================================
                                                                     // Damage
                            UIDataGenericDetail hDamage = UIManager.inst.Data_CreateGeneric();
                            extra = "Range of potential damage at the point of impact.";
                            hDamage.Setup(true, false, true, "Damage", highlightGreen, extra, melee.damage.x + "-" + melee.damage.y, false, "", false, "", ((melee.damage.x + melee.damage.y) / 2) / 100f, true);
                            // Type
                            UIDataGenericDetail hType = UIManager.inst.Data_CreateGeneric();
                            extra = HF.DamageTypeToString(item.itemData.meleeAttack.damageType);
                            hType.Setup(true, false, false, "Type", Color.white, extra, "", false, melee.damageType.ToString());
                            // Critical
                            UIDataGenericDetail hCrit = UIManager.inst.Data_CreateGeneric();
                            extra = HF.CriticalEffectsToString(item.itemData.meleeAttack.critType);
                            if (melee.critical > 0f)
                            {
                                hCrit.Setup(true, false, false, "Critical", Color.white, extra, (melee.critical * 100).ToString() + "%", false, melee.critical.ToString());
                            }
                            else
                            {
                                hCrit.Setup(true, false, false, "Critical", Color.white, extra, "0%", true);
                            }
                            // Heat Transfer
                            if (item.itemData.itemEffects.Count > 0 && item.itemData.itemEffects[0].heatTransfer > 0)
                            {
                                UIDataGenericDetail hHT = UIManager.inst.Data_CreateGeneric();
                                extra = "Thermal weapons attempt to transfer some % of the maximum heat transfer rating of the weapon, and also check for possible robot meltdown (the effect of which might be delayed until the robot's next turn).";
                                string s = "";
                                int ht = item.itemData.itemEffects[0].heatTransfer;
                                if (ht == 1)
                                {
                                    s = "Low (25)";
                                }
                                else if (ht == 2)
                                {
                                    s = "Medium (37)";
                                }
                                else if (ht == 3)
                                {
                                    s = "High (50)";
                                }
                                else if (ht == 4)
                                {
                                    s = "Massive (80)";
                                }

                                hHT.Setup(true, false, false, "Heat Transfer", Color.white, extra, "", false, s);
                            }
                            // Disruption
                            UIDataGenericDetail iHDisruption = UIManager.inst.Data_CreateGeneric();
                            extra = "Chance for this weapon to temporarily disable an active part on impact. If a robot core is struck, there is half this chance the entire robot may be disabled.";
                            if (melee.disruption > 0)
                            {
                                Color iPDC = highlightGreen;
                                if (melee.disruption < 0.33)
                                {
                                    iPDC = highlightGreen;
                                }
                                else if (melee.disruption >= 0.66)
                                {
                                    iPDC = highSecRed;
                                }
                                else
                                {
                                    iPDC = cautiousYellow;
                                }

                                iHDisruption.Setup(true, false, true, "Disruption", iPDC, extra, (melee.disruption * 100) + "%", false, "", false, "", melee.disruption, true); // Bar
                            }
                            else
                            {
                                iHDisruption.Setup(true, false, true, "Disruption", Color.white, extra, "0%", true, "", false, "", 0f); // Bar

                            }
                            // Salvage
                            UIDataGenericDetail iHSalvage = UIManager.inst.Data_CreateGeneric();
                            extra = "Amount by which the salvage potential of a given robot is affected each time shot by a projectile from this weapon. While usually negative, reducing the amount of " +
                            "usable salvage, some special weapons may increase salvage by disabling a robot with minimal collateral damage.";
                            if (melee.salvage > 0)
                            {
                                iHSalvage.Setup(true, false, false, "Salvage", Color.white, extra, "+" + melee.salvage.ToString()); // No bar (simple)
                            }
                            else if (melee.salvage == 0)
                            {
                                iHSalvage.Setup(true, false, false, "Salvage", Color.white, extra, "0", true); // No bar (simple)
                            }
                            else
                            {
                                iHSalvage.Setup(true, false, false, "Salvage", Color.white, extra, melee.salvage.ToString()); // No bar (simple)
                            }
                        }
                    }

                    // Effect is above fabrication
                    if (item.itemData.itemEffects.Count > 0)
                    {
                        if (!item.itemData.itemEffects[0].hasLegEffect && !item.itemData.itemEffects[0].chainExplode)
                        {
                            UIManager.inst.Data_CreateHeader("Effect"); // Effect =========================================================================

                            foreach (var E in item.itemData.itemEffects)
                            {
                                // There is A LOT to consider here and its gonna take a while to fill it all out
                                string textWall = " ";
                                #region Effects

                                if (E.heatDissipation.hasEffect)
                                {
                                    ItemHeatDissipation h = E.heatDissipation;
                                    if (h.ablativeArmorEffect)
                                    {
                                        // Can be two types
                                        if(h.type == HeatDissipationType.AblativeIndividual)
                                        {
                                            textWall += $"Dissipates heat each turn, losing {h.integrityLossPerTurn} integrity per {h.dissipationPerTurn} heat dissipated, applied after all standard heat dissipation and any injectors, and only when heat levels rise above {h.minTempToActivate}. If multiple similar parts attached, heat is distributed among them equally where possible.";
                                        }
                                        else if (h.type == HeatDissipationType.AblativeBroad)
                                        {
                                            textWall += $"Dissipates heat each turn, causing {h.ablativeDamage} damage per {h.dissipationPerTurn} heat dissipated, applied after all standard heat dissipation and any injectors, and only when heat levels rise above {h.minTempToActivate}. The damage is applied to other attached parts, selected randomly, in chunks of {h.ablativeChunks}. Also reduces effect of incoming heat transfer by one level.";
                                        }
                                    }
                                    else if (E.heatDissipation.dissipationPerTurn > 0)
                                    {
                                        textWall += "Dissipates " + E.heatDissipation.dissipationPerTurn + " heat per turn";
                                        if (E.heatDissipation.integrityLossPerTurn > 0)
                                        {
                                            textWall += ", losing " + E.heatDissipation.integrityLossPerTurn + " integrity in the process";
                                        }
                                        if (E.heatDissipation.minTempToActivate > 0)
                                        {
                                            textWall += ", but is applied after all standard heat dissipation, and only when heat levels rise above ";
                                            textWall += E.heatDissipation.minTempToActivate.ToString();
                                        }
                                        textWall += ".";
                                    }
                                    else if (E.heatDissipation.lowerBaseTemp > 0) // Mostly just cryofiber webs
                                    {
                                        textWall += "Lowers base temperature to " + E.heatDissipation.lowerBaseTemp + ".";
                                        textWall += " Also doubles the cooling effect of all active heats sinks (includes Regular/Imp/Adv/Exp). ";
                                        if (E.heatDissipation.preventPowerShutdown)
                                        {
                                            textWall += "Protects power sources from forced shutdown while overheating";
                                            if (E.heatDissipation.preventOverheatSideEffects > 0)
                                            {
                                                textWall += ", and has a " + E.heatDissipation.preventOverheatSideEffects * 100 + "% chance to prevent other types of overheating side effects";
                                            }
                                            textWall += ".";
                                        }
                                    }
                                    else if (E.heatDissipation.heatToEnergyAmount > 0)
                                    {
                                        textWall += "Generates " + E.heatDissipation.energyGeneration + " energy per ";
                                        textWall += E.heatDissipation.heatToEnergyAmount + " surplus heat every turn.";
                                    }


                                    if (E.heatDissipation.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else if (E.heatDissipation.parallel)
                                    {
                                        textWall += "\n <parallel_ok>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }
                                else if (E.ramCrushEffect)
                                {
                                    textWall += E.ramCrushChance * 100 + "% chance to crush robots when ramming them.";
                                    if (E.ramCrush_canDoLarge && E.ramCrush_canDoGigantic)
                                    {
                                        textWall += "Cannot crush targets of large or greater size";
                                        if (E.ramCrush_integLimit > 0)
                                        {
                                            textWall += ", or those with more than " + E.ramCrush_integLimit + " core integrity";
                                        }
                                        textWall += ".";
                                    }

                                    if (E.ramCrush_canStack)
                                    {
                                        if (E.ramCrush_stackCap > 0)
                                        {
                                            textWall += "\n <stacks, capped at " + E.ramCrush_stackCap * 100 + "%>";
                                        }
                                        else
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                    textWall += "\n\n";
                                }

                                if (E.hasStabEffect)
                                {
                                    textWall += E.stab_recoilPerWeapon + " recoil from each weapon";
                                    if (E.stab_KnockbackImmune)
                                    {
                                        textWall += ", and immunity to knockback";
                                    }
                                    textWall += ".";
                                }

                                if (E.armorProtectionEffect.hasEffect)
                                {
                                    ItemProtectionEffect p = E.armorProtectionEffect;

                                    if (p.armorEffect_absorbtion > 0)
                                    {
                                        textWall += "Absorbs " + p.armorEffect_absorbtion * 100 + "% of damage that would otherwise affect ";
                                        string type = "";
                                        switch (p.armorEffect_slotType)
                                        {
                                            case ArmorType.Power:
                                                type = "Power sources.";
                                                break;
                                            case ArmorType.Propulsion:
                                                type = "Propulsion units.";
                                                break;
                                            case ArmorType.Utility:
                                                type = "Utilities.";
                                                break;
                                            case ArmorType.Weapon:
                                                type = "Weapons.";
                                                break;
                                            case ArmorType.General:
                                                type = "other parts.";
                                                break;
                                            case ArmorType.None:
                                                type = "Core.";
                                                break;
                                        }
                                        textWall += type;
                                        if (p.armorEffect_preventCritStrikesVSSlot)
                                        {
                                            textWall += "Also negates extra effects of critical strikes against " + type;
                                            if (p.armorEffect_preventChainReactions)
                                            {
                                                textWall += ", and prevents chain reactions due to electromagnetic damage";
                                                if (!p.armorEffect_preventOverflowDamage)
                                                {
                                                    textWall += ", but cannot protect against overflow damage";
                                                }
                                            }
                                        }
                                        textWall += ".";

                                        if (p.stacks)
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                        else
                                        {
                                            textWall += "\n <no_stack>";
                                        }
                                    }
                                    else if (p.projectionExchange)
                                    {
                                        textWall += "Blocks " + p.pe_blockPercent * 100 + "% of damage ";
                                        if (p.pe_global)
                                        {
                                            textWall += "in exchange for energy loss at a ";
                                        }
                                        else if (p.pe_includeVisibileAllies)
                                        {
                                            textWall += "to self and any visibile allies within a range of ";
                                            textWall += p.pe_alliesDistance + " in exchange for energy loss at a ";
                                        }
                                        else
                                        {
                                            textWall += "to this part in exchange for energy loss at a ";
                                        }
                                        textWall += p.pe_exchange.x + ":" + p.pe_exchange.y + " ratio";
                                        if (p.pe_requireEnergy)
                                            textWall += " (no effect if insufficient energy).";
                                        textWall += ".";

                                        if (p.stacks)
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                        else
                                        {
                                            textWall += "\n <no_stack>";
                                        }
                                    }
                                    else if (p.highCoverage)
                                    {
                                        textWall += "Protects other parts via high coverage.";
                                    }
                                    else if (p.type_hasTypeResistance)
                                    {
                                        textWall += "Enables ";
                                        if (p.type_allTypes)
                                        {
                                            textWall += "general damage resistance: ";
                                        }
                                        else
                                        {
                                            textWall += p.type_damageType.ToString().ToLower() + " damage resistance: ";
                                        }
                                        textWall += p.type_percentage * 100 + "%";
                                        if (p.type_includeAllies)
                                        {
                                            textWall += ", applying benefit to both self and any allies within a range of ";
                                            textWall += p.type_alliesRange.ToString();
                                        }
                                        textWall += ".";

                                        if (p.stacks)
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                        else
                                        {
                                            textWall += "\n <no_stack>";
                                        }
                                    }
                                    else if (p.selfRepair)
                                    {
                                        textWall += "Regenerates integrity at a rate of ";
                                        textWall += p.selfRepair_amount + " per " + p.selfRepairTurns + " turn";
                                        if (p.selfRepairTurns > 1)
                                            textWall += "s";
                                        textWall += ".";

                                        if (p.parallel && p.resume)
                                        {
                                            textWall += "\n <parallel_ok, resume_ok>";
                                        }
                                        else if (p.parallel)
                                        {
                                            textWall += "\n <parallel_ok>";
                                        }
                                        else if (p.resume)
                                        {
                                            textWall += "\n <resume_ok>";
                                        }
                                    }
                                    else if (p.critImmunity)
                                    {
                                        textWall += "100% chance to negate effects of incoming critical strikes.";
                                    }
                                }

                                if (E.detectionEffect.hasEffect)
                                {
                                    ItemDetectionEffect d = E.detectionEffect;

                                    if (d.botDetection)
                                    {
                                        textWall += "Enables robots scanning up to a distance of " + d.range + ", once per turn";

                                        if (d.bd_maxInterpreterEffect)
                                        {
                                            textWall += ", in addition to all effects of a maximum-strength signal interpreter.";

                                            if (d.bd_immuneToScramblingCloaking)
                                            {
                                                textWall += " Immune to all scrambling and cloaking effects.";
                                            }

                                            if (d.bd_previousActivity)
                                            {
                                                textWall += " Also detects long-term residual evidence of prior robot activity within field of view.";
                                            }

                                            if (d.bd_botReporting)
                                            {
                                                textWall += " 0b10 combat robots scanned by this device will report the event, once per bot.";
                                            }
                                        }
                                        else
                                        {
                                            textWall += ".";
                                        }
                                    }
                                    else if (d.terrainScanning)
                                    {
                                        textWall += "Enabled terrain scanning up to a distance of " + d.range + ", once per turn.";
                                    }
                                    else if (d.haulerTracking)
                                    {
                                        textWall += "Enables real-time tracking of 0b10 Haulers across the entire floor";
                                        if (d.ht_viewInventories)
                                        {
                                            textWall += ", and gives access to their current manifest. Toggle active state to temporarily list all inventories in view.";
                                        }
                                        else
                                        {
                                            textWall += ".";
                                        }
                                        textWall += " Only applies in 0b10-controlled areas.";
                                    }
                                    else if (d.seismic)
                                    {
                                        textWall += "Enables real-time seismic detection and analysis up to a distance of ";
                                        textWall += d.range + ".";

                                        if (d.stacks)
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                        else
                                        {
                                            textWall += "\n <no_stack>";
                                        }
                                    }
                                    else if (d.machine)
                                    {
                                        textWall += "Analyzes visible machines to locate others on the 0b10 network. Also determines whether an explosive machine has been destabilized and how long until detonation.";
                                    }
                                    else if (d.structural)
                                    {
                                        textWall += "Scans all visible walls to analyze the structure behind them, out to a depth of ";
                                        textWall += d.structural_depth;
                                        textWall += ". Also identifies hidden doorways and highlights areas that will soon cave in due to instability even without further stimulation.";
                                    }
                                    else if (d.warlordComms)
                                    {
                                        textWall += "Enables long-distance communication with Warlord forces to determine their composition and hiding locations. Toggle active state to temporarily list all squads in the area. If active while near a squad, will signal it to emerge and assist.";
                                    }
                                }

                                if (E.hasSignalInterp)
                                {
                                    // Unlike other effects, each text wall is different for the different strengths
                                    // 1 -> 4
                                    switch (E.signalInterp_strength)
                                    {
                                        case 1:
                                            textWall += "Strength 1: Robot scan signals differentiate between object sizes. (Requires data from a Sensor Array.) Also enables deciphering of signals emitted from adjacent exits, revealing where they lead, and at garrisons know the time until the next response dispatch, if any. When multiple exits lead to the same destination, after having identified the first, subsequent ones will be identified on sight even without an interpreter.";
                                            break;
                                        case 2:
                                            textWall += "Strength 2: Robot scan signals accurately reflect target size and class, and are unaffected by system corruption. (Requires data from a Sensor Array.) Also enables deciphering of signals emitted from adjacent exits, revealing where they lead, and at garrisons know the time until the next response dispatch, if any. When multiple exits lead to the same destination, after having identified the first, subsequent ones will be identified on sight even without an interpreter.";
                                            break;
                                        case 3:
                                            textWall += "Strength 3: Robot scan signals accurately reflect target size and specific class rating, and are unaffected by system corruption. (Requires data from a Sensor Array.) Also enables deciphering of signals emitted from adjacent exits, revealing where they lead, and at garrisons know the time until the next response dispatch, if any. When multiple exits lead to the same destination, after having identified the first, subsequent ones will be identified on sight even without an interpreter.";
                                            break;
                                        case 4:
                                            textWall += "Strength 4: Robot scan signals accurately reflect target size and specific class rating, are unaffected by system corruption, and can discern dormant, unpowered, disabled, and broken robots. (Requires data from a Sensor Array.) Also enables deciphering of signals emitted from adjacent exits, revealing where they lead, and at garrisons know the time until the next response dispatch, if any. When multiple exits lead to the same destination, after having identified the first, subsequent ones will be identified on sight even without an interpreter.";
                                            break;
                                    }
                                }

                                if (E.hasMassSupport)
                                {
                                    textWall += "Increase mass support by " + E.massSupport + ".";

                                    if (E.massSupport_stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.collectsMatterBeam)
                                {
                                    textWall += "Automatically collects matter within a range of " + E.matterCollectionRange + ".";
                                }

                                if (E.internalStorageEffect.hasEffect)
                                {
                                    textWall += "Increases ";
                                    if (E.internalStorageEffect.internalStorageType == 0) // Matter
                                    {
                                        textWall += "matter storage capcity by " + E.internalStorageEffect.capacity + ".";
                                        textWall += " Also stores surplus collected matter while in inventory, but cannot be accessed for use until attached. Stored matter can also be extracted directly if on the ground at current position.";
                                    }
                                    else if (E.internalStorageEffect.internalStorageType == 1) // Power
                                    {
                                        textWall += "energy storage capcity by " + E.internalStorageEffect.capacity + ".";
                                        textWall += " Also stores surplus collected energy while in inventory, but cannot be accessed for use until attached. Stored energy can also be extracted directly if on the ground at current position.";
                                    }

                                    if (E.internalStorageEffect.internalStorageType_stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.hackBonuses.hasHackBonus || E.hackBonuses.hasSystemShieldBonus)
                                {
                                    HackBonus h = E.hackBonuses;

                                    if (h.hasHackBonus)
                                    {
                                        textWall += "Increases chance of successful machine hack by " + h.hackSuccessBonus * 100;
                                        textWall += ". Also provides a +" + h.rewireBonus * 100 + "% bonus to rewiring traps and disrupted robots, and applies a ";
                                        textWall += h.programmerHackDefenseBonus * 100 + "% penalty to hostile programmers attempting to defend their allies against your hacks.";
                                    }
                                    else if (h.hasSystemShieldBonus)
                                    {
                                        textWall += "While hacking machines, reduces both chance of detection and rate of detection chance increase by ";
                                        textWall += h.hackDetectRateBonus + ". Reduces tracing progress advances by the same amount. Also a lower chance that hacking machines will be considered serious enough to trigger an increase in security level, and reduces central database lockout chance by ";
                                        textWall += h.databaseLockoutBonus * 100 + "%. Blocks hacking feedback side effects ";
                                        textWall += h.hackFeedbackPreventBonus * 100 + "% of the time, and repels " + h.allyHackDefenseBonus * 100 + "% of hacking attempts against allies within a range of ";
                                        textWall += h.allyHackRange + ".";
                                    }

                                    if (h.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.emitEffect.hasEffect)
                                {
                                    textWall += "Every " + E.emitEffect.turnTime + " turns emits a powerful signal tuned to power down all ";
                                    if (E.emitEffect.target)
                                    {
                                        textWall += "0b10 ";
                                    }
                                    else
                                    {
                                        textWall += "Assembled ";
                                    }
                                    textWall += "within a range of " + E.emitEffect.range + ".";
                                }

                                if (E.fireTimeEffect.hasEffect)
                                {
                                    ItemFiretimeEffect f = E.fireTimeEffect;

                                    if (f.launchersOnly)
                                    {
                                        textWall += "Reduces firing time for any launcher by " + f.fireTimeReduction * 100 + "%, if fired alone.";
                                    }
                                    else
                                    {
                                        textWall += "Reduces collective firing time of all guns, cannons, and launchers by " + f.fireTimeReduction * 100 + "%.";
                                    }

                                    if (!f.compatability)
                                    {
                                        textWall += " Incompatible with Weapon Cyclers and Autonomous or overloaded weapons.";
                                    }

                                    if (f.stacks)
                                    {
                                        if (f.capped)
                                        {
                                            textWall += "\n <stacks, capped at " + f.cap * 100 + "%>";
                                        }
                                        else
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.transmissionJammingEffect.hasEffect)
                                {
                                    ItemTransmissionJamming f = E.transmissionJammingEffect;

                                    textWall += "Blocks local transmissions from visible hostiles within a range of ";
                                    textWall += f.range + ", making it impossible for them to share information about your current position.";

                                    if (f.preventReinforcementCall)
                                    {
                                        textWall += " Also prevents calls for reinforcements";
                                        if (f.suppressAlarmTraps)
                                        {
                                            textWall += ", and suppresses alarm traps";
                                        }
                                        textWall += ".";
                                    }

                                    textWall += " Visibility for jamming purposes includes indirect knowledge via drones, Watcher Feeds, and so on, but must still be within the indicated range from you personally.";
                                }

                                if (E.effectiveCorruptionEffect.hasEffect)
                                {
                                    ItemEffectiveCorruptionPrevention f = E.effectiveCorruptionEffect;

                                    textWall += "Reduces effective system corruption by " + f.amount + ".";

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.partRestoreEffect.hasEffect)
                                {
                                    ItemPartRestore f = E.partRestoreEffect;

                                    textWall += f.percentChance * 100 + "% chance each turn to restore a broken or malfunctioning part, attached or in inventory, to functionality.";
                                    if (!f.canRepairAlienTech)
                                    {
                                        textWall += " Unable to repair alien technology";
                                        if (f.protoRepairCap > 0)
                                        {
                                            textWall += ", or prototypes above rating " + f.protoRepairCap + ".";
                                        }
                                        else
                                        {
                                            textWall += ".";
                                        }
                                    }
                                    else
                                    {
                                        textWall += " Able to repair alien technology";
                                        if (f.protoRepairCap > 0)
                                        {
                                            textWall += ", but unable to repair prototypes above rating " + f.protoRepairCap + ".";
                                        }
                                        else
                                        {
                                            textWall += ".";
                                        }
                                    }

                                    if (f.canRunInParralel)
                                    {
                                        textWall += "\n <parallel_ok>";
                                    }
                                }

                                if (E.exilesEffects.hasEffect)
                                {
                                    ItemExilesSpecific f = E.exilesEffects;

                                    if (f.isAutoWeapon)
                                    {
                                        textWall += "Selects its own targets and attacks them if in range, at no time cost to you.";
                                    }

                                    if (f.chronoWheelEffect)
                                    {
                                        textWall += "Sets a temporal reversion point when attached, then loses 1 integrity per turn. Once integrity is depleted naturally, you are forced back to that point in time. If destroyed prematurely the reversion will not occur.";
                                    }

                                    if (f.lifetimeDecay)
                                    {
                                        textWall += "\n\n Eventually breaks down.";
                                    }

                                    // add more later
                                }

                                if (E.toHitBuffs.hasEffect)
                                {
                                    ItemToHitEffects f = E.toHitBuffs;

                                    if (f.bonusCritChance)
                                    {
                                        textWall += "Increases weapon critical chances by " + f.amount * 100 + "%.";
                                        textWall += " Only affects weapons that are already capable of critical hits, and not applicable for the Meltdown critical.";
                                    }

                                    if (f.bypassArmor)
                                    {
                                        textWall += "Enables " + f.amount * 100 + "% chance to bypass target armor. Does not apply to AOE attacks.";
                                    }

                                    if (f.coreExposureEffect)
                                    {
                                        textWall += "Increases target core exposure by " + f.amount * 100 + "%.";
                                        if (f.coreExposureGCM_only)
                                        {
                                            textWall += " Applies to only gun, cannon, and melee attacks.";
                                        }
                                    }

                                    if (f.flatBonus)
                                    {
                                        textWall += "Increase non-melee weapon accuracy by " + f.amount * 100 + "%";
                                    }

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else if (f.halfStacks)
                                    {
                                        textWall += "\n <half_stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.partIdent.hasEffect)
                                {
                                    textWall += E.partIdent.amount * 100 + "% chance to identify a random unidentified part in inventory each turn. ";
                                    if (!E.partIdent.canIdentifyAlien)
                                    {
                                        textWall += "Cannot identify alien technology.";
                                    }

                                    if (E.partIdent.parallel)
                                    {
                                        textWall += "\n <parallel_ok>";
                                    }
                                }

                                if (E.antiCorruption.hasEffect)
                                {
                                    textWall += E.antiCorruption.amount * 100 + "% chance each turn to purge 1% of system corruption";
                                    if (E.antiCorruption.integrityLossPer > 0)
                                    {
                                        textWall += ", losing " + E.antiCorruption.integrityLossPer + " integrity each time the effect is applied";
                                    }

                                    textWall += ".";

                                    if (E.antiCorruption.parallel)
                                    {
                                        textWall += "\n <parallel_ok>";
                                    }
                                }

                                if (E.rcsEffect.hasEffect)
                                {
                                    ItemRCS f = E.rcsEffect;

                                    textWall += "Enables responsive movement to avoid direct attacks, " + f.percentage * 100 + "% to dodge while on legs, hovering or flying (no effect on tracked or wheeled movement). Same chance to evade triggered traps";
                                    if (f.momentumBonus > 0)
                                    {
                                        textWall += ", and a +" + f.momentumBonus + " to effective momentum for melee attacks and ramming. No effects while overweight";
                                    }
                                    textWall += ".";

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.phasingEffect.hasEffect)
                                {
                                    textWall += "Reduces enemy ranged targeting accuracy by " + E.phasingEffect.percentage * 100 + "%.";
                                    if (E.phasingEffect.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.cloakingEffect.hasEffect)
                                {
                                    ItemCloaking f = E.cloakingEffect;

                                    textWall += "Effective sight range of robots attempting to spot you reduced by " + f.rangedReduction + ". ";
                                    if (f.noticeReduction > 0)
                                    {
                                        textWall += "Also " + f.noticeReduction * 100 + "% chance of being noticed by hostiles if passing through their field of view when not their turn.";
                                    }

                                    if (f.halfStacks)
                                    {
                                        textWall += "\n <half_stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.meleeBonus.hasEffect)
                                {
                                    ItemMeleeBonus f = E.meleeBonus;

                                    if (f.melee_followUpChance > 0)
                                    {
                                        textWall += "Increases per-weapon chance of follow-up melee attacks by" + f.melee_followUpChance * 100 + "%";
                                    }

                                    if (f.melee_maxDamageBoost > 0)
                                    {
                                        textWall += "Increases melee weapons maximum damage by " + f.melee_maxDamageBoost * 100 + "%";
                                        if (f.melee_accuracyDecrease > 0)
                                        {
                                            textWall += ", and decreases melee attack accuracy by " + f.melee_accuracyDecrease * 100 + "%";
                                        }
                                        textWall += ".";
                                    }

                                    if (f.melee_accuracyIncrease > 0)
                                    {
                                        textWall += "Increases melee attack accuracy by " + f.melee_accuracyIncrease * 100 + "%";
                                        if (f.melee_minDamageBoost > 0)
                                        {
                                            textWall += ", and minimum damage by " + f.melee_minDamageBoost + " (cannot exceed weapon's maximum damage)"; ;
                                        }
                                        textWall += ".";
                                    }

                                    if (f.melee_attackTimeDecrease > 0)
                                    {
                                        textWall += "Increases per-weapon chance of follow-up melee attacks by " + f.melee_attackTimeDecrease * 100 + "%";
                                    }

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stack>";
                                    }
                                    else if (f.halfStacks)
                                    {
                                        textWall += "\n <half_stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.launcherBonus.hasEffect)
                                {
                                    ItemLauncherBonuses f = E.launcherBonus;

                                    if (f.launcherAccuracy > 0)
                                    {
                                        textWall += "Increases launcher accuracy by " + f.launcherAccuracy * 100 + "%. Also prevents launcher misfires caused by system corruption.";
                                    }

                                    if (f.launcherLoading > 0)
                                    {
                                        if (f.forEnergyOrCannon)
                                        {
                                            textWall += "Reduces firing time for an energy gun or cannon by 50%, if fired alone. Incompatible with Weapon Cyclers and autonomous or overloaded weapons.";
                                        }
                                        else
                                        {
                                            textWall += "Reduces firing time for any launcher by 50%, if fired alone. Incompatible with Weapon Cyclers and autonomous or overloaded weapons.";
                                        }
                                    }

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.bonusSlots.hasEffect)
                                {
                                    ItemBonusSlots f = E.bonusSlots;

                                    // ?
                                }

                                if (E.flatDamageBonus.hasEffect)
                                {
                                    ItemFlatDamageBonus f = E.flatDamageBonus;

                                    string types = "";
                                    foreach (var T in f.types)
                                    {
                                        types += T.ToString() + "/";
                                    }
                                    types = types.Substring(0, types.Length - 1); // Remove the spare "/"

                                    textWall += "Increases " + types + " damage by " + f.damageBonus * 100 + "%.";

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else if (f.halfStacks)
                                    {
                                        textWall += "\n <half_stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.salvageBonus.hasEffect)
                                {
                                    ItemSalvageBonuses f = E.salvageBonus;

                                    textWall += "Increases salvage recovered from targets, +" + f.bonus + " modifier.";

                                    if (f.gunTypeOnly)
                                    {
                                        textWall += " Compatible only with gun-type weapons that fire a single projectile.";
                                    }

                                    if (f.stacks)
                                    {
                                        textWall += "\n <stacks>";
                                    }
                                    else
                                    {
                                        textWall += "\n <no_stack>";
                                    }
                                }

                                if (E.alienBonus.hasEffect)
                                {
                                    ItemAlienBonuses f = E.alienBonus;

                                    if (f.singleDamageToCore)
                                    {
                                        textWall += "While active, " + f.amount * 100 + "% of damage to this part is intead passed along to core.";
                                    }
                                    else if (f.allDamageToCore)
                                    {
                                        textWall += f.amount * 100 + "% of damage to parts is instead transferred directly to the core.";

                                        if (f.stacks)
                                        {
                                            textWall += "\n <stacks>";
                                        }
                                        else
                                        {
                                            textWall += "\n <no_stack>";
                                        }
                                    }
                                    else if (f.cpc_evolutionFreedom)
                                    {
                                        textWall += "Enables greater freedom during assignment of evolution parameters. No effect after reaching maximum slot count (26).";
                                    }

                                    // TODO - Expand this later
                                }

                                #endregion

                                Data_CreateTextWall(textWall);
                                Data_CreateSpacer();
                            }
                        }


                    }

                    // Fabrication goes at the very bottom
                    if (item.itemData.fabricationInfo.canBeFabricated && item.itemData.schematicDetails.hasSchematic)
                    {
                        string mult = "";
                        ItemFabInfo info = item.itemData.fabricationInfo;

                        if (info.amountMadePer > 1)
                        {
                            mult = " x" + info.amountMadePer;
                        }

                        UIManager.inst.Data_CreateHeader("Fabrication" + mult); // Fabrication =========================================================================

                        UIDataGenericDetail fTime = UIManager.inst.Data_CreateGeneric();
                        extra = "The time in turns required to fabricate this item at a fabricator. Fabricators with higher security levels fabricator quicker, as indicated by the times. Security level: 1 / 2 / 3.";
                        fTime.Setup(true, false, false, "Time", Color.white, extra, "", false, info.fabTime.x + "/" + info.fabTime.y + "/" + info.fabTime.z);
                        UIDataGenericDetail fComp = UIManager.inst.Data_CreateGeneric();
                        extra = "Any extra components required to fabricate this item. Normally only matter is required. Some special items may require extra components.";
                        if (info.componenetsRequired.Count > 0)
                        {
                            string comps = "";
                            foreach (var C in info.componenetsRequired)
                            {
                                comps += C.itemName + "/";
                            }
                            comps = comps.Substring(0, comps.Length - 1); // Remove the extra "/"
                            fComp.Setup(true, false, false, "Components", Color.white, extra, "", false, comps);
                        }
                        else
                        {
                            fComp.Setup(true, false, false, "Components", Color.white, extra, "", false, "None", true);
                        }
                    }

                    // TODO?
                    // [MORE] https://www.gridsagegames.com/blog/2024/01/full-ui-upscaling-part-2-holy-mockups/
                }
                else
                {
                    dataMenu.data_superImageNull.GetComponent<TextMeshProUGUI>().text = "No Analysis Data";

                    Data_CreateSpacer();
                    Data_CreateTextWall("This is an unidentified prototype. Attaching it before proper analysis will identify its functions, but also carries the risk of negative side-effects if it is a faulty version.");
                }
                #endregion
            }
        }
        else if(bot != null)
        {
            // Disable the big image because we don't use it
            dataMenu.data_superImageParent.gameObject.SetActive(false);
            // Set title to bot's name
            dataMenu.data_mainTitle.text = bot.botInfo.botName;
            // Set title image to bot's (world) image
            dataMenu.data_smallImage.sprite = bot.botInfo.displaySprite;

            // -- Unlike items, bots are more consistent in the data we have to display, but there is a little bit of variance in there -- //
            #region Data Displaying
            UIManager.inst.Data_CreateHeader("Overview"); // Overview
            // Class
            UIDataGenericDetail botClass = UIManager.inst.Data_CreateGeneric();
            string extra = "Robots are divided into classes based on their purpose.";
            botClass.Setup(true, false, false, "Class", Color.white, extra, "", false, HF.BotClassParse(bot.botInfo._class));
            // Size
            UIDataGenericDetail botSize = UIManager.inst.Data_CreateGeneric();
            extra = "Robot size category determines their likeliness of being struck by both targeted and stray shots, as well as their chance to suffer knockback. " +
                "Smaller robots do not necessarily occupy their entire position, see the manual under Combat > Targeting for more about how this affects lines of fire.";
            botSize.Setup(true, false, false, "Size", Color.white, extra, "", false, bot.botInfo._size.ToString());
            // Rating
            UIDataGenericDetail botRating = UIManager.inst.Data_CreateGeneric();
            extra = "Rating summarizes the robot's overall effectiveness in combat situations, thus highly-rated robots are far more dangerous.";
            Color bRC = highlightGreen;
            if(bot.botInfo.rating > 90)
            {
                bRC = highSecRed;
            }
            else if(bot.botInfo.rating <= 25)
            {
                bRC = highlightGreen;
            }
            else
            {
                bRC = cautiousYellow;
            }
            botRating.Setup(true, false, true, "Rating", bRC, extra, bot.botInfo.rating.ToString(), false, "", false, "", bot.botInfo.rating / 100f, true); // Uses the bar to indicate threat
            // ID
            UIDataGenericDetail botID = UIManager.inst.Data_CreateGeneric();
            Color idColor = Color.white;
            string idText = "";
            (idColor, idText, extra) = HF.VisualsFromBotRelation(bot);
            botID.Setup(true, true, false, "ID", idColor, extra, "", false, "", false, idText);
            // Movement
            UIDataGenericDetail botMovement = UIManager.inst.Data_CreateGeneric();
            extra = "Indicates the current propulsion type, and speed as a percentage of 'average' speed (100%). In tactical HUD mode, an additional value represents the actual time cost to move one space (thus higher values are slower).";
            botMovement.Setup(true, false, false, "Movement", Color.white, extra, "", false, bot.botInfo._movement.moveType.ToString() + " (" + bot.botInfo._movement.moveTileRange + ")");
            // Core Integrity
            UIDataGenericDetail botInteg = UIManager.inst.Data_CreateGeneric();
            extra = "Remaining core integrity. A robot is destroyed once its core integrity reaches zero.";
            botInteg.Setup(true, false, true, "Core Integrity", Color.white, extra, bot.currentHealth.ToString(), false, "", false, "", bot.currentHealth / 100f);
            // Core Temp
            UIDataGenericDetail botTemp = UIManager.inst.Data_CreateGeneric();
            Color tempColor = Color.white;
            string tempText = "";
            extra = "Current temperature. At the danger threshold and above, this robot is susceptible to meltdowns from additional thermal damage.";
            // [0 to 100 -> Cool] [100 to 200 -> Warm] [200 to 300 -> Hot] [300 to 400 -> Warning] [400+ -> Critical]
            if (bot.currentHeat < 100) // Cool
            {
                tempText = "Cool";
                tempColor = coolBlue;
            }
            else if (bot.currentHeat >= 100 && bot.currentHeat < 200) // Warm
            {
                tempText = "Warm";
                tempColor = warmYellow;
            }
            else if (bot.currentHeat >= 200 && bot.currentHeat < 300) // Hot
            {
                tempText = "Hot";
                tempColor = hotOrange;
            }
            else if (bot.currentHeat >= 300 && bot.currentHeat < 400) // Warning
            {
                tempText = "Warning";
                tempColor = warningOrange;
            }
            else // Critical
            {
                tempText = "Critical";
                tempColor = alertRed;
            }
            botTemp.Setup(true, true, false, "Core Temp", tempColor, extra, "", false, "", false, tempText);
            // Core Explosure
            UIDataGenericDetail botExposure = UIManager.inst.Data_CreateGeneric();
            extra = "Exposure represents the chance that a successful attack will hit the core.";
            botExposure.Setup(true, false, true, "Core Exposure", Color.white, extra, (bot.botInfo.coreExposure * 100) + "%", false, "", false, "", bot.botInfo.coreExposure);
            // Salvage Potential
            UIDataGenericDetail botSalvage = UIManager.inst.Data_CreateGeneric();
            extra = "Salvage potential represents the amount of matter that could be retrieved from the robot, assuming it can be disabled without excessive collateral damage.";
            botSalvage.Setup(true, false, false, "Salvage Potential", Color.white, extra, "", false, bot.botInfo.salvagePotential.x + "-" + bot.botInfo.salvagePotential.y);

            Data_CreateSpacer();

            // Armament
            UIManager.inst.Data_CreateHeader("Armament");
            if(bot.armament.Container.Items.Length <= 0)
            {
                UIDataGenericDetail aNone = UIManager.inst.Data_CreateGeneric();
                aNone.Setup(false, false, false, "None", Color.white, "");
            }
            else
            {
                List<ItemObject> tracker = new List<ItemObject>(); // Track all items so we can stack duplicates
                foreach (var I in bot.armament.Container.Items)
                {
                    if (tracker.Contains(I.item.itemData)) // Duplicate
                    {
                        // Modify the last object
                        UIDataGenericDetail obj = dataMenu.data_objects[dataMenu.data_objects.Count - 1].GetComponent<UIDataGenericDetail>();
                        string displayString = obj.primary_text.text;
                        if (displayString[displayString.Length - 1].ToString() == "x" || displayString[displayString.Length - 2].ToString() == "x" 
                            || displayString[displayString.Length - 3].ToString() == "x" || displayString[displayString.Length - 4].ToString() == "x")
                        { // Does this string already contain an x## at the end?
                            // Yes, we need to modify that number
                            string[] split = displayString.Split("x");
                            int amount = HF.StringToInt(split[split.Length - 1]);
                            displayString += "x" + amount++; // Increase the number and add it back in
                        }
                        else
                        {
                            // No, add in the x
                            displayString += " x2";
                        }

                        obj.Setup(false, false, false, displayString, Color.white, "");
                    }
                    else
                    {
                        UIDataGenericDetail itemC = UIManager.inst.Data_CreateGeneric();
                        string cName = "";
                        // Only show the true name if the player has prototype knowledge about it. If not, append "Prototype" and call it by its type.
                        if (I.item.itemData.knowByPlayer)
                        {
                            cName = I.item.itemData.itemName;
                        }
                        else
                        {
                            cName = HF.ItemPrototypeName(I.item);
                        }

                        string displayString = cName + " (" + HF.FindExposureInBotObject(bot.botInfo.armament, I.item.itemData) * 100 + "%)";

                        itemC.Setup(false, false, false, displayString, Color.white, "");

                        tracker.Add(I.item.itemData);
                    }
                }

                Data_CreateSpacer();
            }

            // Components
            UIManager.inst.Data_CreateHeader("Components");
            if (bot.components.Container.Items.Length <= 0)
            {
                UIDataGenericDetail aNone = UIManager.inst.Data_CreateGeneric();
                aNone.Setup(false, false, false, "None", Color.white, "");
            }
            else
            {
                List<ItemObject> tracker = new List<ItemObject>(); // Track all items so we can stack duplicates
                foreach (var I in bot.components.Container.Items)
                {
                    if (tracker.Contains(I.item.itemData)) // Duplicate
                    {
                        // Modify the last object
                        UIDataGenericDetail obj = dataMenu.data_objects[dataMenu.data_objects.Count - 1].GetComponent<UIDataGenericDetail>();
                        string displayString = obj.primary_text.text;
                        if (displayString[displayString.Length - 1].ToString() == "x" || displayString[displayString.Length - 2].ToString() == "x"
                            || displayString[displayString.Length - 3].ToString() == "x" || displayString[displayString.Length - 4].ToString() == "x")
                        { // Does this string already contain an x## at the end?
                            // Yes, we need to modify that number
                            string[] split = displayString.Split("x");
                            int amount = HF.StringToInt(split[split.Length - 1]);
                            displayString += "x" + amount++; // Increase the number and add it back in
                        }
                        else
                        {
                            // No, add in the x
                            displayString += " x2";
                        }

                        obj.Setup(false, false, false, displayString, Color.white, "");
                    }
                    else
                    {
                        UIDataGenericDetail itemC = UIManager.inst.Data_CreateGeneric();
                        string cName = "";
                        // Only show the true name if the player has prototype knowledge about it. If not, append "Prototype" and call it by its type.
                        if (I.item.itemData.knowByPlayer)
                        {
                            cName = I.item.itemData.itemName;
                        }
                        else
                        {
                            cName = HF.ItemPrototypeName(I.item);
                        }

                        string displayString = cName + " (" + HF.FindExposureInBotObject(bot.botInfo.components, I.item.itemData) * 100 + "%)";

                        itemC.Setup(false, false, false, displayString, Color.white, "");

                        tracker.Add(I.item.itemData);
                    }
                }

                Data_CreateSpacer();
            }

            // Resistances
            UIManager.inst.Data_CreateHeader("Resistances");
            BotResistancesExtra x = bot.botInfo.resistancesExtra;
            if (bot.botInfo.resistances.Count <= 0 && !x.coring && !x.criticals && !x.dismemberment && !x.disruption && !x.hacking && !x.jamming && !x.meltdown)
            {
                UIDataGenericDetail rNone = UIManager.inst.Data_CreateGeneric();
                rNone.Setup(false, false, false, "None", Color.white, "");
            }

            foreach (var R in bot.botInfo.resistances)
            {
                UIDataGenericDetail resBar = UIManager.inst.Data_CreateGeneric();
                extra = "Depending on their design, robots may be more or less likely to be affected by a certain type of damage. Negative resistances represent a weakness," +
                    " and therefore greater damage from that source. For example, 25% resistance would decrease the damage from an incoming attack of that type by 25%, " +
                    "while -25% would instead increase the damage sustained by 25%.";

                if (R.immune)
                {
                    resBar.Setup(true, false, false, "Class", Color.white, "This bot is IMMUNE to the damage type. Attempting to damage this bot with the designated weapon type will do ZERO damage.", "", false, "Immune", true);
                }
                else
                {
                    string vA = "";
                    Color vC;
                    if (R.resistanceAmount < 0f)
                    {
                        vA = "-" + Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                        vC = highSecRed;
                    }
                    else
                    {
                        vA = Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                        vC = highlightGreen;
                    }

                    resBar.Setup(true, false, true, R.damageType.ToString(), vC, extra, vA, false, "", false, "", R.resistanceAmount, true); // Bar
                }
            }
            // - and then go through resisitancesExtra too
            if (x.dismemberment)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to dismemberment from melee and other related sources.";
                rX.Setup(true, false, false, "Dismemberment", Color.white, extra, "", false, "Immune", true);
            }
            if (x.disruption)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to core disruption from electromagnetic sources.";
                rX.Setup(true, false, false, "Disruption", Color.white, extra, "", false, "Immune", true);
            }
            if (x.criticals)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to ALL critical effects.";
                rX.Setup(true, false, false, "Criticals", Color.white, extra, "", false, "Immune", true);
            }
            if (x.coring)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to any core-affecting effects from critical strikes, including Destroy, Blast, Smash, and Phase.";
                rX.Setup(true, false, false, "Coring", Color.white, extra, "", false, "Immune", true);
            }
            if (x.hacking)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to any remote hacking by Programmers. Cannot be rewired while core disrupted.";
                rX.Setup(true, false, false, "Hacking", Color.white, extra, "", false, "Immune", true);
            }
            if (x.jamming)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to transmission jamming.";
                rX.Setup(true, false, false, "Jamming", Color.white, extra, "", false, "Immune", true);
            }
            if (x.meltdown)
            {
                UIDataGenericDetail rX = UIManager.inst.Data_CreateGeneric();
                extra = "Immune to meltdown destruction and other side effects of overheating, as well as the Meltdown critical effect.";
                rX.Setup(true, false, false, "Metldown", Color.white, extra, "", false, "Immune", true);
            }

            // Traits (most bots don't have this so we'll do it later) TODO
            if(bot.botInfo.traits.Count > 0)
            {
                Data_CreateSpacer(); // Spacer

                // Create the object
                GameObject go = Instantiate(dataMenu.data_traitPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
                go.transform.SetParent(dataMenu.data_contentArea.transform);
                go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                dataMenu.data_objects.Add(go);
                go.name = "[Traits]";

                // This has its own dropdown menu!
                string combiText = "";
                bool isFirstTrait = true; // Flag to check if it's the first trait

                foreach (var T in bot.botInfo.traits)
                {
                    // Add "\n\n" before adding the next trait description if it's not the first trait
                    if (!isFirstTrait)
                    {
                        combiText += "\n\n";
                    }

                    if (T.trait_sensorJamming)
                    {
                        combiText += "Sensor Jamming: Prevents Sensor Arrays within range from pinpointing signals, but gives away its position in the process.";
                        isFirstTrait = false;
                    }

                    if (T.trait_energyEmmission)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Energy Emission ({T.eeAmount}): Each turn transfers [{T.eeAmount}-range] energy to each robot in view and within a range of {T.eeRange}.";
                        isFirstTrait = false;
                    }

                    if (T.trait_coreRegeneration)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Core Regeneration ({T.crAmount}): Regenerates {T.crAmount} core integrity every turn.";
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_partRegeneration)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Part Regeneration ({T.prAmount}): All attached parts regenerate {T.prAmount} integrity every turn. ";
                        if (T.prIncludeMissing)
                        {
                            combiText += $"Also regenerates one missing part every {T.prMissingDelay} turns.";
                        }
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_corruptionEmission)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Corruption Emission ({T.ceChance * 100}): {T.ceChance * 100}% chance each turn to cause anywhere from {T.ceAmount.x * 100}-{T.ceAmount.y * 100}% corruption in each robot in view and within a range of {T.ceRange}. (Cogmind is less susceptible to the effect and only suffers 0-1% corruption.)";
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_energyDrain)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Energy Drain ({T.edAmount}): Each turn drains [{T.edAmount}-(range*2)] energy from each robot in view and within a range of {T.edRange}. ";
                        combiText += $"Also drains an equivalent amount of heat.";
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_heatEmission)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Heat Emission ({T.heAmount}): Each turn transfers [{T.heAmount}-(range*{T.heRange})] heat to each robot in view and within a range of {T.heRange}.";
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_scanCloak)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Scan Cloak ({T.scStrength}): Hides this robot from sensors without a Signal Interpreter of at least strength {T.scStrength}.";
                        isFirstTrait = false;
                    }
                    
                    if (T.trait_selfDestruct)
                    {
                        //if (!isFirstTrait) combiText += "\n\n";
                        combiText += $"Self-destructing: Leaves no parts on destruction, unless self-destruct mechanism fails due to system corruption.";
                        isFirstTrait = false;
                    }
                }

                dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Setup(combiText, true);
            }

            // Analysis
            if (bot.botInfo.playerHasAnalysisData)
            {
                Data_CreateSpacer(); // Spacer

                // Create the object
                GameObject go = Instantiate(dataMenu.data_analysisPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
                go.transform.SetParent(dataMenu.data_contentArea.transform);
                go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                // Add it to list
                dataMenu.data_objects.Add(go);
                go.name = "[Analysis]";

                // This has its own dropdown menu!
                string combiText = "";

                // First the description of the bot
                combiText = bot.botInfo.description;
                // And what we get for knowing this
                combiText += "\n ";
                combiText += "<weaknesses identified: +5% hit, +10% dmg>"; // (pretty sure this doesn't change)

                dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Setup(combiText, false);
            }

            #endregion
        }
        else if(machine != null)
        {
            // Pretty simple, not much to show here besides overview and resistances

            // Disable the big image because we don't use it
            dataMenu.data_superImageParent.gameObject.SetActive(false);
            // Set title to machine's name
            dataMenu.data_mainTitle.text = machine.displayName;
            // Disable the mini-image
            dataMenu.data_smallImage.transform.parent.gameObject.SetActive(false);

            #region Details
            UIManager.inst.Data_CreateHeader("Overview"); // Overview ============================================================
            // Class
            UIDataGenericDetail mArmor = UIManager.inst.Data_CreateGeneric();
            string extra = "Amount of concentrated damage required to overcome this object's armor to disable or destroy it. After resistances are factored in, " +
                "the damage from a single hit must be greater than or equal to this value.";
            Color aC = Color.white;
            if (machine.armor.y >= 40)
            {
                aC = highSecRed;
            }
            else if (machine.armor.y <= 12)
            {
                aC = highlightGreen;
            }
            else if (machine.armor.y < 40 && machine.armor.y >= 20)
            {
                aC = warningOrange;
            }
            else
            {
                aC = cautiousYellow;
            }
            string a = machine.armor.y.ToString();
            if(machine.armor.y >= 999) // No chance buster
            {
                a = "**";
            }
            mArmor.Setup(true, false, true, "Armor", aC, extra, a, false, "", false, "", machine.armor.y / 75f, true);

            // State - there is some variance in this
            InteractableMachine interactable = HF.GetInteractableMachine(machine.gameObject);

            // https://www.gridsagegames.com/blog/2015/11/garrison-access/
            UIDataGenericDetail mState = UIManager.inst.Data_CreateGeneric();
            /*
             *  == There are quite a few states a machine can be in ==
             *  ACTIVE - Normal state
             *  INACTIVE - Destroyed or inoperable
             *  PROCESSING T-(time) - [Fabricator/Repair Bay] Time until finished
             *  TRACING T-(time) - (All?) ???
             *  UNSTABLE (time) - [Static that will explode because of external factors] Time until detonation
             *  DETONATE (time) - [Static that was *told* to explode] Time until detonation
             *  OVERLOAD - [Fabricator] "Render the fabricator inoperable, but cause it to send high corruption-causing arcs of electromagnetic energy at nearby bots for a short while. Also, locks the machine and summons an investigation squad like all force hacks."
             *  COMPROMISED - [Terminal] - If you trigger the central database lockout 
             *  TRANSMITTING - [Garrison] Calling in more dudes
             *  REDEPLOYING - [Garrison?] Recalling squad
             */
            if (machine.GetComponent<Fabricator>() && machine.GetComponent<Fabricator>().flag_overload) // OVERLOAD
            {
                extra = "This machine is currently inoperable and is randomly send high corruption arcs of electromagnetic energy at any nearby bots. An investigation squad is likely to be on its way.";
                mState.Setup(true, true, false, "State", highSecRed, extra, "", false, "", false, "OVERLOAD");
            }
            else if ((machine.GetComponent<Fabricator>() && machine.GetComponent<Fabricator>().working) || (machine.GetComponent<RepairStation>() && machine.GetComponent<RepairStation>().working)) // PROCESSING
            {
                int time = 0;
                if (machine.GetComponent<Fabricator>())
                {
                    time = TurnManager.inst.globalTime - machine.GetComponent<Fabricator>().begunBuildTime;
                }
                else if (machine.GetComponent<RepairStation>())
                {
                    time = machine.GetComponent<RepairStation>().timeToComplete;
                }
                extra = "This machine is currently in operation and will finish after the specified time.";
                mState.Setup(true, true, false, "State", energyBlue, extra, "", false, "", false, "PROCESSING T-" + time.ToString());
            }
            else if (machine.GetComponent<Garrison>() && (machine.GetComponent<Garrison>().s_redeploying || machine.GetComponent<Garrison>().s_transmitting))
            {
                if (machine.GetComponent<Garrison>().s_redeploying)
                {
                    extra = "Forces summoned by this Garrison Access are currently moving to redeploy to a different area. They will arrive here shortly.";
                    mState.Setup(true, true, false, "State", warningOrange, extra, "", false, "", false, "REDEPLOYING");
                }
                else if (machine.GetComponent<Garrison>().s_transmitting)
                {
                    extra = "This Garrison Access is communicating with additional reinforcements preparing for dispatch. Using a Signal Interpreter provides the precise number of turns remaining until the next dispatch.";
                    mState.Setup(true, true, false, "State", warningOrange, extra, "", false, "", false, "TRANSMITTING");
                }
            }
            else if (machine.GetComponent<Terminal>() && machine.GetComponent<Terminal>().databaseLockout)
            {
                extra = "This terminal's access to the central database has been compromised due to frequenty incursion attempts, and has thus been denied access to make further attempts.";
                mState.Setup(true, true, false, "State", warningOrange, extra, "", false, "", false, "COMPROMISED");
            }
            else if (machine.GetComponent<StaticMachine>() && (machine.GetComponent<StaticMachine>().s_detonate || machine.GetComponent<StaticMachine>().s_unstable))
            {
                if (machine.GetComponent<StaticMachine>().s_detonate)
                {
                    extra = "This machine is about to explode due to external interference.";
                    mState.Setup(true, true, false, "State", highSecRed, extra, "", false, "", false, "DETONATE " + machine.GetComponent<StaticMachine>().detonate_timer);
                }
                else if (machine.GetComponent<StaticMachine>().s_unstable)
                {
                    extra = "This machine has become extremely unstable due to external circumstances, and will explode very soon.";
                    mState.Setup(true, true, false, "State", highSecRed, extra, "", false, "", false, "UNSTABLE " + machine.GetComponent<StaticMachine>().detonate_timer);
                }
            }
            else if (!machine.GetComponent<StaticMachine>() && interactable != null && interactable.detected) // Tracing (this is some kind of interactable machine)
            {
                extra = "Due to recently detected hacking attempts, the central control system is currently attempting to trace the approximate location of the intrusion. Once successful, and investigate squad will be sent to this position.";
                mState.Setup(true, true, false, "State", cautiousYellow, extra, "", false, "", false, "TRACING " + (interactable.traceProgress * 100).ToString());
            }
            else if (machine.state) // Active (green)
            {
                extra = "Current state of this machine. This message will provide more context for special states.";
                mState.Setup(true, true, false, "State", highlightGreen, extra, "", false, "", false, "ACTIVE");
            }
            else if(!machine.state) // Inactive (gray)
            {
                extra = "This machine is inactive and will not function.";
                mState.Setup(true, true, false, "State", inactiveGray, extra, "", false, "", false, "INACTIVE");
            }
            else if (machine.destroyed) // Disabled (gray)
            {
                extra = "Critical components of this machine have been destroyed and thus this machine is disabled.";
                mState.Setup(true, true, false, "State", inactiveGray, extra, "", false, "", false, "DISABLED");
            }

            // Building/Repair progress
            if (machine.GetComponent<Fabricator>() && machine.GetComponent<Fabricator>().working)
            {
                UIDataGenericDetail mTrojans = UIManager.inst.Data_CreateGeneric();
                if (machine.state)
                {
                    extra = "This machine is currently working on fabricating the specified object. It will finish after the specified time.";
                    string Fname = "";
                    if (machine.GetComponent<Fabricator>().targetBot != null)
                    {
                        Fname = machine.GetComponent<Fabricator>().targetBot.botName;
                    }
                    else if (machine.GetComponent<Fabricator>().targetPart != null)
                    {
                        Fname = machine.GetComponent<Fabricator>().targetPart.itemName;
                    }
                    mTrojans.Setup(true, false, false, "Building...", Color.white, extra, "", false, Fname);
                }
                else
                {
                    extra = "This machine is currently disabled.";
                    mTrojans.Setup(true, false, false, "Building...", Color.white, extra, "", false, "Disabled");
                }
            }
            else if (machine.GetComponent<RepairStation>() && machine.GetComponent<RepairStation>().working)
            {
                UIDataGenericDetail mTrojans = UIManager.inst.Data_CreateGeneric();
                if (machine.state)
                {
                    extra = "This machine is currently working on repairing the following item. It will finish after the specified time.";
                    mTrojans.Setup(true, false, false, "Repairing...", Color.white, extra, "", false, machine.GetComponent<RepairStation>().targetPart.itemName);
                }
                else
                {
                    extra = "This machine is currently disabled.";
                    mTrojans.Setup(true, false, false, "Repairing...", Color.white, extra, "", false, "Disabled");
                }
            }

            // Trojans
            if (!machine.GetComponent<StaticMachine>())
            {
                if (machine.state)
                {
                    List<HackObject> trojans = machine.gameObject.GetComponent<InteractableMachine>().trojans;
                    bool first = true;
                    if (trojans.Count > 0)
                    {
                        foreach (var T in trojans)
                        {
                            UIDataGenericDetail trojan = UIManager.inst.Data_CreateGeneric();
                            if (first)
                            {
                                trojan.Setup(true, false, false, "Trojans", Color.white, T.description, "", false, T.ToString());
                            }
                            else
                            {
                                trojan.Setup(true, false, false, "", Color.white, T.description, "", false, T.ToString());
                            }

                            first = false;
                        }
                    }
                    else
                    {
                        UIDataGenericDetail mTrojans = UIManager.inst.Data_CreateGeneric();
                        mTrojans.Setup(true, false, false, "Trojans", Color.white, "Lists any active trojans loaded on this system", "", false, "None", true);
                    }
                }
                else
                {
                    UIDataGenericDetail mTrojans = UIManager.inst.Data_CreateGeneric();
                    mTrojans.Setup(true, false, false, "Trojans", Color.white, "This machine is currently disabled.", "", false, "Disabled");
                }
            }

            Data_CreateSpacer();

            // Resistances
            if(machine.resistances.Count > 0)
            {
                UIManager.inst.Data_CreateHeader("Resistances"); // Overview ============================================================

                foreach (var R in machine.resistances)
                {
                    UIDataGenericDetail resBar = UIManager.inst.Data_CreateGeneric();
                    extra = "Depending on their design, some installations may be more or less likely to be affected by a certain type of damage. Negative resistances represent a weakness," +
                        " and therefore greater damage from that source. For example, 25% resistance would decrease the damage from an incoming attack of that type by 25%, " +
                        "while -25% would instead increase the damage sustained by 25%.";

                    if (R.immune)
                    {
                        resBar.Setup(true, false, false, "Class", Color.white, "This machine is IMMUNE to the damage type. Attempting to damage this machine with the designated weapon type will do ZERO damage.", "", false, "Immune", true);
                    }
                    else
                    {
                        string vA = "";
                        Color vC;
                        if (R.resistanceAmount < 0f)
                        {
                            vA = "-" + Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                            vC = highSecRed;
                        }
                        else
                        {
                            vA = Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                            vC = highlightGreen;
                        }

                        resBar.Setup(true, false, true, R.damageType.ToString(), vC, extra, vA, false, "", false, "", R.resistanceAmount, true); // Bar
                    }
                }

                Data_CreateSpacer();
            }

            // Explosive Potential
            if(machine.GetComponent<StaticMachine>() && machine.GetComponent<StaticMachine>().explosive)
            {
                UIManager.inst.Data_CreateHeader("Explosive Potential"); // Explosive Potential ============================================================
                ExplosionGeneric detail = machine.GetComponent<MachinePart>().explosiveDetails;

                // Stability
                UIDataGenericDetail mStability = UIManager.inst.Data_CreateGeneric();
                extra = "Ability of this potentially explosive machine to avoid being destabilized by a sutained attack, even those that do note pentrate its armor.";
                if (detail.stability > 0)
                {
                    mStability.Setup(true, false, true, "Stability", Color.white, extra, (detail.stability * 100) + "%", false, "", false, "", detail.stability); // Bar
                }
                else
                {
                    mStability.Setup(true, false, true, "Stability", Color.white, extra, "N/A", true, "", false, "", 0f, false); // Bar
                }
                // Delay
                UIDataGenericDetail mDelay = UIManager.inst.Data_CreateGeneric();
                extra = "Approximate number of turns after a destabilization event that this machine will likely explode.";
                mDelay.Setup(true, false, false, "Delay", Color.white, extra, "", false, detail.delay.x + "-" + detail.delay.y);
                // Radius
                UIDataGenericDetail iERadius = UIManager.inst.Data_CreateGeneric();
                extra = "Maximum radius of the explosion from its origin.";
                iERadius.Setup(true, false, true, "Radius", highlightGreen, extra, detail.radius.ToString(), false, "", false, "", detail.radius / 9f, true); // Bar
                // Damage
                int average = Mathf.RoundToInt((detail.damage.x + detail.damage.y) / 2);
                extra = "Range of potential damage at the explosion's origin.";
                UIDataGenericDetail iEDamage = UIManager.inst.Data_CreateGeneric();
                iEDamage.Setup(true, false, true, "Damage", highlightGreen, extra, detail.damage.x + "-" + detail.damage.y, false, "", false, "", average / 120f, true); // Bar
                // Falloff
                UIDataGenericDetail iEFalloff = UIManager.inst.Data_CreateGeneric();
                extra = "Amount of damage potential lost per space as the explosion expands from its origin. While targeting, this falloff is represented visually by the AOE color's brightness " +
                    "relative to the origin (this feature can be toggled via Explosion Predictions option).";
                if (detail.fallOff <= 0)
                {
                    iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, detail.fallOff.ToString());
                }
                else
                {
                    iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, "0", true);
                }
                // Chunks
                if (detail.chunks.y > 1)
                {
                    UIDataGenericDetail iEChunks = UIManager.inst.Data_CreateGeneric();
                    extra = "AOE damage is often spread across each target in the area of effect, dividing the damage into separate chunks before affecting a robot," +
                        " where each chunk of damage selects its own target part (though they may overlap). Some explosive effects have a static number of chunks, while " +
                        "others randomly select from within a range for each attack.";
                    iEFalloff.Setup(true, false, false, " Falloff", Color.white, extra, detail.fallOff.ToString());
                }
                // Type
                UIDataGenericDetail iEType = UIManager.inst.Data_CreateGeneric();
                extra = "While powerful, explosives generally spread damage across each target in the area of effect.\nExplosions also tend to reduce the amount of salvage remaining after destroying a target.";
                iEType.Setup(true, false, false, "Type", Color.white, extra, "", false, detail.damageType.ToString());
                // Spectrum
                UIDataGenericDetail iESpectrum = UIManager.inst.Data_CreateGeneric();
                extra = "Range of electromagnetic spectrum, with narrower ranges more likely to trigger chain explosions in power sources. (AOE spectrum only affects power sources being used by Cogmind, or those exposed on the ground.)";
                if (detail.hasSpectrum)
                {
                    string s = "";
                    if (detail.spectrum <= 0.1f)
                    {
                        s = "Wide (10%)";
                    }
                    else if (detail.spectrum == 0.3f)
                    {
                        s = "Intermediate (30%)";
                    }
                    else if (detail.spectrum == 0.5f)
                    {
                        s = "Narrow (50%)";
                    }
                    else if (detail.spectrum == 1f)
                    {
                        s = "Fine (100%)";
                    }

                    iESpectrum.Setup(true, false, false, "Specturm", Color.white, extra, s); // No bar (simple)
                }
                else
                {
                    iESpectrum.Setup(true, false, false, "Specturm", Color.white, extra, "N/A", true); // No bar (simple)
                }
                // Heat transfer
                int ht = machine.heatTransfer;
                UIDataGenericDetail mHeatTransfer = UIManager.inst.Data_CreateGeneric();
                if (ht > 0)
                {
                    string s = "";
                    extra = "Some explosions attempt to transfer some % of the maximum heat transfer rating of the explosion, and also check for possible robot meltdown (the effect of which might be delayed until the robot's next turn).";
                    if (ht == 1)
                    {
                        s = "Low (25)";
                    }
                    else if (ht == 2)
                    {
                        s = "Medium (37)";
                    }
                    else if (ht == 3)
                    {
                        s = "High (50)";
                    }
                    else if (ht == 4)
                    {
                        s = "Massive (80)";
                    }

                    mHeatTransfer.Setup(true, false, false, "Heat Transfer", Color.white, extra, "", false, s);
                }
                else
                {
                    mHeatTransfer.Setup(true, false, false, "Heat Transfer", Color.white, extra, "", false, "N/A", true);
                }
                // Disruption
                UIDataGenericDetail iEDisruption = UIManager.inst.Data_CreateGeneric();
                extra = "Chance this explosion may temporarily disable an active part it damages. If a robot core is struck, there is half this chance the entire robot may be disabled.";
                if (detail.disruption > 0)
                {
                    Color iPDC = highlightGreen;
                    if (detail.disruption < 0.33)
                    {
                        iPDC = highlightGreen;
                    }
                    else if (detail.disruption >= 0.66)
                    {
                        iPDC = highSecRed;
                    }
                    else
                    {
                        iPDC = cautiousYellow;
                    }

                    iEDisruption.Setup(true, false, true, "Disruption", iPDC, extra, (detail.disruption * 100) + "%", false, "", false, "", detail.disruption, true); // Bar
                }
                else
                {
                    iEDisruption.Setup(true, false, true, "Disruption", Color.white, extra, "0%", true, "", false, "", 0f); // Bar

                }
                // Salvage
                UIDataGenericDetail iESalvage = UIManager.inst.Data_CreateGeneric();
                extra = "Amount by which the salvage potential of a given robot is affected by being engulfed in this explosion. While usually negative, reducing the amount of usable salvage, some special weapons may increase salvage by disabling a robot with minimal collateral damage.";
                if (detail.fallOff <= 0)
                {
                    iESalvage.Setup(true, false, false, "Salvage", Color.white, extra, detail.salvage.ToString());
                }
                else
                {
                    iESalvage.Setup(true, false, false, "Salvage", Color.white, extra, "0", true);
                }
            }
            

            #endregion
        }
        else if(tile != null)
        {
            // Pretty simple, not much to show here besides overview and resistances

            // Disable the big image because we don't use it
            dataMenu.data_superImageParent.gameObject.SetActive(false);
            // Set title to tile's name
            dataMenu.data_mainTitle.text = tile.tileInfo.tileName;
            // Disable the mini-image
            dataMenu.data_smallImage.transform.parent.gameObject.SetActive(false);

            #region Details
            UIManager.inst.Data_CreateHeader("Overview"); // Overview ============================================================
            // Class
            UIDataGenericDetail mArmor = UIManager.inst.Data_CreateGeneric();
            int armor = tile.tileInfo.armor;
            string extra = "Amount of concentrated damage required to overcome this object's armor to disable or destroy it. After resistances are factored in, " +
                "the damage from a single hit must be greater than or equal to this value.";
            Color aC = Color.white;
            if (armor >= 40)
            {
                aC = highSecRed;
            }
            else if (armor <= 12)
            {
                aC = highlightGreen;
            }
            else if (armor < 40 && armor >= 20)
            {
                aC = warningOrange;
            }
            else
            {
                aC = cautiousYellow;
            }
            string a = armor.ToString();
            if (armor >= 999) // No chance buster
            {
                a = "**";
            }
            mArmor.Setup(true, false, true, "Armor", aC, extra, a, false, "", false, "", armor / 75f, true);

            // Resistances
            if (tile.tileInfo.resistances.Count > 0)
            {
                UIManager.inst.Data_CreateHeader("Resistances"); // Overview ============================================================

                foreach (var R in tile.tileInfo.resistances)
                {
                    UIDataGenericDetail resBar = UIManager.inst.Data_CreateGeneric();
                    extra = "Depending on their design, some structures may be more or less likely to be affected by a certain type of damage. Negative resistances represent a weakness," +
                        " and therefore greater damage from that source. For example, 25% resistance would decrease the damage from an incoming attack of that type by 25%, " +
                        "while -25% would instead increase the damage sustained by 25%.";

                    if (R.immune)
                    {
                        resBar.Setup(true, false, false, "Class", Color.white, "This structure is IMMUNE to the damage type. Attempting to damage this structure with the designated weapon type will do ZERO damage.", "", false, "Immune", true);
                    }
                    else
                    {
                        string vA = "";
                        Color vC;
                        if (R.resistanceAmount < 0f)
                        {
                            vA = "-" + Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                            vC = highSecRed;
                        }
                        else
                        {
                            vA = Mathf.Abs(R.resistanceAmount * 100).ToString() + "%";
                            vC = highlightGreen;
                        }

                        resBar.Setup(true, false, true, R.damageType.ToString(), vC, extra, vA, false, "", false, "", R.resistanceAmount, true); // Bar
                    }
                }

                Data_CreateSpacer();
            }

            #endregion
        }

        // -- Animation time --
        // We need to trigger all the animations at the same time

        // Animate the big image (if its enabled)
        if (dataMenu.data_superImage.gameObject.activeInHierarchy)
        {
            if(dataMenu.data_superImage.sprite != null)
            {
                dataMenu.data_superImageNull.gameObject.SetActive(false);
                dataMenu.data_superNullBrackets.gameObject.SetActive(false);
                dataMenu.data_SuperImageAnimator.Play("Data_SuperImageAppear");
            }
            else // In some rare cases, some items don't have images. So we display some text indicating so.
            {
                dataMenu.data_superImageNull.gameObject.SetActive(true);
                dataMenu.data_superNullBrackets.gameObject.SetActive(true);
            }
        }

        // Animate the title (type out, include the [ ] too)
        StartCoroutine(DataAnim_TitleOpen());

        // Animate the small [image] if its active
        if (dataMenu.data_smallImage.transform.parent.gameObject.activeInHierarchy)
        {
            // Animate the little image
            StartCoroutine(DataAnim_SmallImageOpen());
        }
        
        // Animate the data lines
        foreach (var O in dataMenu.data_objects.ToList())
        {
            if (O.GetComponent<UIDataHeader>())
            {
                O.GetComponent<UIDataHeader>().Open();
            }
            else if (O.GetComponent<UIDataGenericDetail>())
            {
                O.GetComponent<UIDataGenericDetail>().Open();
            }
            else if (O.GetComponent<UIDataTextWall>())
            {
                O.GetComponent<UIDataTextWall>().Open();
            }
            else if (O.GetComponent<UIDataTraits>())
            {
                O.GetComponent<UIDataTraits>().AppearAnimate();
            }
            else if (O.GetComponent<UIDataAnalysis>())
            {
                O.GetComponent<UIDataAnalysis>().AppearAnimate();
            }
        }

    }

    public void Data_CloseMenu()
    {
        // Play a sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["CLOSE"]); // UI - CLOSE

        dataMenu.selection_item = null;
        dataMenu.selection_obj = null;
        dataMenu.selected_comparison_item = null;

        StartCoroutine(Data_CloseMenuAnimation());
    }

    private IEnumerator Data_CloseMenuAnimation()
    {
        // There is a closing animation:
        // - The menu itself just goes from its set color to all black (DONT FORGET TO RESET THEIR COLORS AT THE END THIS TIME!!!)
        // - Each object has their own closing animations too.

        // Disable the super image
        dataMenu.data_superImageParent.gameObject.SetActive(false);

        // Disable the comparison bar
        dataMenu.data_comparisonParent.SetActive(false);

        // Fade out the title text
        StartCoroutine(DataAnim_TitleTextClose());

        dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Close();
        dataMenu.data_onAnalysis = false;
        dataMenu.data_onTraits = false;

        // Animate out & Destroy all the prefabs we create
        foreach (var O in dataMenu.data_objects.ToList())
        {
            if (O.GetComponent<UIDataHeader>())
            {
                O.GetComponent<UIDataHeader>().Close();
            }
            else if (O.GetComponent<UIDataGenericDetail>())
            {
                O.GetComponent<UIDataGenericDetail>().Close();
            }
            else if (O.GetComponent<UIDataTextWall>())
            {
                O.GetComponent<UIDataTextWall>().Close();
            }
            else if (O.GetComponent<UIDataTraits>())
            {
                O.GetComponent<UIDataTraits>().Close();
            }
            else if (O.GetComponent<UIDataAnalysis>())
            {
                O.GetComponent<UIDataAnalysis>().Close();
            }
        }

        // Transparency out the menu (0.5s delay)
        #region Image Fade-out

        float delay = 0.1f;

        foreach (Image I in dataMenu.bits)
        {
            if (I.gameObject != null)
            {
                Color setColor = I.color;
                I.color = new Color(setColor.r, setColor.g, setColor.b, 0.8f);
            }
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in dataMenu.bits)
        {
            if (I.gameObject != null)
            {
                Color setColor = I.color;
                I.color = new Color(setColor.r, setColor.g, setColor.b, 0.6f);
            }
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in dataMenu.bits)
        {
            if (I.gameObject != null)
            {
                Color setColor = I.color;
                I.color = new Color(setColor.r, setColor.g, setColor.b, 0.4f);
            }
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in dataMenu.bits)
        {
            if (I.gameObject != null)
            {
                Color setColor = I.color;
                I.color = new Color(setColor.r, setColor.g, setColor.b, 0.2f);
            }
        }

        yield return new WaitForSeconds(delay);

        foreach (Image I in dataMenu.bits)
        {
            if(I.gameObject != null)
            {
                Color setColor = I.color;
                I.color = new Color(setColor.r, setColor.g, setColor.b, 0f);
            }
        }

        yield return new WaitForSeconds(delay);
        #endregion

        foreach (var O in dataMenu.data_objects.ToList()) // Clear up any remaining things (like spacers)
        {
            Destroy(O.gameObject);
        }

        dataMenu.data_objects.Clear();

        // Disable the menu
        dataMenu.data_parent.SetActive(false);

        // Reset the transparency
        #region Image Transparency Reset

        foreach (Image I in dataMenu.bits)
        {
            Color setColor = I.color;
            I.color = new Color(setColor.r, setColor.g, setColor.b, 1f);
        }
        #endregion
    }

    #region Prefab Creation
    private void Data_CreateHeader(string text, string bonus = "")
    {
        GameObject go = Instantiate(dataMenu.data_headerPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
        go.transform.SetParent(dataMenu.data_contentArea.transform);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        dataMenu.data_objects.Add(go);
        // Assign Details
        go.GetComponent<UIDataHeader>().Setup(text, bonus);
    }

    private UIDataGenericDetail Data_CreateGeneric()
    {
        GameObject go = Instantiate(dataMenu.data_genericPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
        go.transform.SetParent(dataMenu.data_contentArea.transform);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        dataMenu.data_objects.Add(go);

        return go.GetComponent<UIDataGenericDetail>();
    }

    private void Data_CreateSpacer()
    {
        GameObject go = Instantiate(dataMenu.data_spacerPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
        go.transform.SetParent(dataMenu.data_contentArea.transform);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        dataMenu.data_objects.Add(go);
        go.name = "<Spacer>";
    }

    private void Data_CreateTextWall(string text)
    {
        GameObject go = Instantiate(dataMenu.data_textWallPrefab, dataMenu.data_contentArea.transform.position, Quaternion.identity);
        go.transform.SetParent(dataMenu.data_contentArea.transform);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        dataMenu.data_objects.Add(go);
        // Set value
        go.GetComponent<UIDataTextWall>().Setup(text);
    }

    private void Data_CreateComparison(bool green, string text, bool alt = false)
    {
        GameObject go = Instantiate(dataMenu.data_comparisonPrefab, dataMenu.data_comparisonArea.transform.position, Quaternion.identity);
        go.transform.SetParent(dataMenu.data_comparisonArea.transform);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        dataMenu.data_objects.Add(go);
        // Assign Details
        go.GetComponent<UIDataComparisonDetail>().Setup(green, text, alt);
        go.name = "[Comp]: " + text;
    }
    #endregion

    #region Animations
    private IEnumerator DataAnim_TitleOpen()
    {
        dataMenu.isAnimating = true;

        // Set brackets to black
        dataMenu.data_bracketLeft.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(Color.black)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"["}</color></mark>";
        dataMenu.data_bracketRight.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(Color.black)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"["}</color></mark>";

        string primaryStart = dataMenu.data_mainTitle.text;
        int len = primaryStart.Length;

        float delay = 0f;
        float perDelay = 0.75f / len;
        dataMenu.data_mainTitle.text = "";

        List<string> segments = HF.StringToList(primaryStart);

        foreach (string segment in segments)
        {
            string s = segment;
            string last = HF.GetLastCharOfString(s);
            if (last == " ")
            {
                last = "_"; // Janky workaround because mark doesn't highlight spaces
            }

            if (s.Length > 0)
            {
                s = segment.Remove(segment.Length - 1, 1); // Remove the last character
                if (last == "_")
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(UIManager.inst.subGreen)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{last}</color></mark>"; // Add it back with the highlight
                }
                else
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(UIManager.inst.subGreen)}aa><color=#{ColorUtility.ToHtmlStringRGB(UIManager.inst.highlightGreen)}>{last}</color></mark>"; // Add it back with the highlight
                }
            }

            StartCoroutine(HF.DelayedSetText(dataMenu.data_mainTitle, s, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        dataMenu.data_mainTitle.text = primaryStart;

        // And we need to manual deal with the remaining brackets
        // - Highlight the bracket
        dataMenu.data_bracketLeft.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(UIManager.inst.subGreen)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"["}</color></mark>";

        yield return new WaitForSeconds(perDelay);

        // - Unhighlight the left bracket & highlight the right one

        dataMenu.data_bracketLeft.text = "[";
        dataMenu.data_bracketRight.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(UIManager.inst.subGreen)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"]"}</color></mark>";

        yield return new WaitForSeconds(perDelay);

        // - Unhighlight the right bracket
        dataMenu.data_bracketRight.text = "]";

        dataMenu.isAnimating = false;
    }

    private IEnumerator DataAnim_SmallImageOpen()
    {
        yield return new WaitForSeconds(0.1f);

        dataMenu.data_smallImage_anim.gameObject.SetActive(true);

        Color transparent = new Color(0f, 0f, 0f, 0f);

        float elapsedTime = 0f;
        float duration = 1f;
        while (elapsedTime < duration) // Bright green -> Fully transparent
        {
            Color color = Color.Lerp(UIManager.inst.highGreen, transparent, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            dataMenu.data_smallImage_anim.color = color;

            yield return null;
        }

        dataMenu.data_smallImage_anim.gameObject.SetActive(false);
    }

    private IEnumerator DataAnim_TitleTextClose()
    {
        dataMenu.isAnimating = true;

        string oldtext = dataMenu.data_mainTitle.text;

        float elapsedTime = 0f;
        float duration = 0.45f;
        while (elapsedTime < duration) // Dark green -> Black
        {
            Color color = Color.Lerp(UIManager.inst.dullGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            // Set the highlights for the text
            dataMenu.data_mainTitle.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{oldtext}</mark>";
            dataMenu.data_bracketLeft.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{"["}</mark>";
            dataMenu.data_bracketRight.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{"]"}</mark>";

            yield return null;
        }

        dataMenu.isAnimating = false;
    }

    #endregion

    #endregion

}

[System.Serializable]
public class UIDataDisplay
{
    // This time we will be smart and store all our variables in a subclass so that
    // we can hide it in the inspector later in a dropdown menu (unlike the wall of Terminal variables, my bad).

    [Header("References")]
    [Tooltip("Parent of the entire menu. Disabled/Enable this to Close/Open the menu.")]
    public GameObject data_parent;
    [Tooltip("The giant image that appears at the top of the menu.")]
    public Image data_superImage;
    public Image data_superImageBW;
    public GameObject data_superImageParent;
    [Tooltip("In cases where an item has no image, we display this text instead.")]
    public GameObject data_superImageNull;
    public GameObject data_superNullBrackets;
    public TextMeshProUGUI data_mainTitle;
    [Tooltip("The small little image right next to the item/bot's name inside the [ ]")]
    public Image data_smallImage;
    public Image data_smallImage_anim;
    public TextMeshProUGUI data_bracketLeft;
    public TextMeshProUGUI data_bracketRight;
    [Tooltip("The object where all the prefabs should be spawned in.")]
    public GameObject data_contentArea;
    public GameObject data_extraDetail;
    //
    public GameObject data_traitBox;

    [Header("Prefabs")]
    public GameObject data_headerPrefab;
    public GameObject data_genericPrefab;
    public GameObject data_spacerPrefab;
    public GameObject data_textWallPrefab;
    public GameObject data_traitPrefab;
    public GameObject data_analysisPrefab;
    public GameObject data_comparisonPrefab;

    [Header("Objects")]
    [Tooltip("All the objects (prefabs) that we spawned in.")]
    public List<GameObject> data_objects = new List<GameObject>();
    public UIDataGenericDetail data_focusObject = null;
    public Item selection_item;
    [Tooltip("A second item we compare the first against.")]
    public Item selected_comparison_item;
    public GameObject selection_obj;
    public List<Image> bits = new List<Image>(); // Gets faded out/in when the menu closes

    [Header("Animation")]
    public bool isAnimating = false;
    public Animator data_SuperImageAnimator;
    public Animator data_ComparisonAnimator;

    public bool data_onTraits = false;
    public bool data_onAnalysis = false;

    [Header("Comparison")]
    public GameObject data_comparisonParent;
    public GameObject data_comparisonArea;
}
