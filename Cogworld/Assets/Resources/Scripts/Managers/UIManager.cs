using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Drawing;
using Color = UnityEngine.Color;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System;
using Random = UnityEngine.Random;
using Unity.Burst.CompilerServices;
using System.Text;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;
//using static UnityEditor.Progress;

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

    private void Update()
    {
        // - Check to close Terminal window via Escape Key -
        if(terminal_targetTerm != null) // Window is open
        {
            if (terminal_activeInput == null) // And the player isn't in the input window
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Terminal_Close();
                }
            }
        }
    }

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

        // Play the "MANUAL" sound (51)
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[51]);

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
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.INTRO_Clips[4], 0.45f); // "UI/DONE" this audio clip is too loud for its own good
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
                    (_e, _v) = HF.GetGenericTileVis(MapManager.inst._allTilesRealized[new Vector2Int(x, y)].gameObject);

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
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.INTRO_Clips[4], 0.45f); // "UI/DONE" this audio clip is too loud for its own good
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
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[46]);

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
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.INTRO_Clips[6]);

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


        yield return new WaitForSeconds(5f);

        // If the player has their mouse over it keep it up
        while (newItemPopup != null && newItemPopup.GetComponentInChildren<UIItemPopup>().mouseOver)
        {

            yield return null;
        }

        itemPopups.Remove(newItemPopup);

        if(newItemPopup != null)
            newItemPopup.GetComponentInChildren<UIItemPopup>().MessageOut();

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
    public GameObject genericProjectile_prefab;
    public List<GameObject> genericProjectiles = new List<GameObject>();

    public void CreateGenericProjectile(Transform origin, Transform target, Color projColor, Color highlightColor, float speed, bool accurate)
    {
        // Instantiate it & Assign it to parent
        GameObject newProjectile = Instantiate(genericProjectile_prefab, origin.position, Quaternion.identity);
        newProjectile.transform.SetParent(origin.transform);
        newProjectile.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newProjectile.GetComponent<Canvas>().sortingOrder = 23;
        // Add it to list
        genericProjectiles.Add(newProjectile);
        // Assign Details
        newProjectile.GetComponentInChildren<Projectile_Generic>().Setup(origin, target, projColor, highlightColor, speed, accurate);

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
        AudioManager.inst.PlayDialogueAmbient(2, 0.5f);

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

        List<string> revealVariants = GenerateRevealVariants(text.speech);

        float delay = 0.1f; // This value doesn't really matter

        dialogue_text.text = revealVariants[0];

        yield return new WaitForSeconds(delay);

        dialogue_text.text = revealVariants[revealVariants.Count / 2];
        dialogueLower_text.text = "...";

        yield return new WaitForSeconds(delay);

        dialogue_text.text = text.speech;


        // Just in case it hasn't finished yet
        if (hasMoreDialogue)
        {
            dialogueLower_text.text = "...";
        }
        dialogue_text.text = text.speech;
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
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[17]);
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
    [HideInInspector] public GameObject terminal_targetTerm;
    [Header("Terminal/Generic Machines")]
    public GameObject terminal_hackingAreaRef;
    public GameObject terminal_targetresultsAreaRef;
    public GameObject terminal_hackinfoArea1;
    public List<GameObject> terminal_hackinfoBorders = new List<GameObject>();
    public List<GameObject> terminal_targetResultBorders = new List<GameObject>();
    public Image terminal_hackTitleBacker;
    public Image terminal_targetResultBorder1;
    public Image terminal_targetResultBorder2;
    public List<KeyValuePair<string, TerminalCommand>> terminal_manualBuffer = new List<KeyValuePair<string, TerminalCommand>>(); // Past used terminal codes
    //
    public GameObject terminal_hackOptionsArea; // Where the hacking option prefabs will be put (Target)
    public GameObject terminal_hackResultsArea; // Where the hacking result prefabs will be put (Results)
    //
    public GameObject terminal_activeInput = null;
    //
    public TextMeshProUGUI terminal_backingBinary;
    public TextMeshProUGUI terminal_name; // Terminal ?### - ? Access
    public Image terminal_secLvl_backing;
    public TextMeshProUGUI terminal_secLvl;
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

    public void Terminal_OpenGeneric(GameObject target)
    {
        // Freeze player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;

        // Set target
        terminal_targetTerm = target;

        // Binary (Background)
        InvokeRepeating("Terminal_Binary", 0f, 1f);

        // Reference
        int type = 0;
        Terminal term = null;
        Fabricator fab = null;
        Scanalyzer scan = null;
        RepairStation rep = null;
        RecyclingUnit rec = null;
        Garrison gar = null;
        TerminalCustom custom = null;
        if (target.GetComponent<Terminal>())
        {
            term = (Terminal)target.GetComponent<Terminal>();
            type = 1;
        }
        else if (target.GetComponent<Fabricator>())
        {
            fab = target.GetComponent<Fabricator>();
            type = 2;
        }
        else if (target.GetComponent<Scanalyzer>())
        {
            scan = target.GetComponent<Scanalyzer>();
            type = 3;
        }
        else if (target.GetComponent<RepairStation>())
        {
            rep = target.GetComponent<RepairStation>();
            type = 4;
        }
        else if (target.GetComponent<RecyclingUnit>())
        {
            rec = target.GetComponent<RecyclingUnit>();
            type = 5;
        }
        else if (target.GetComponent<Garrison>())
        {
            gar = target.GetComponent<Garrison>();
            type = 6;
        }
        else if (target.GetComponent<TerminalCustom>())
        {
            custom = target.GetComponent<TerminalCustom>();
            type = 7;
        }

        // Assign name + get Sec Level / Restricted access
        int secLvl = 0;
        bool restrictedAccess = true;
        switch (type)
        {
            case 0:
                Debug.LogError("ERROR: Failed to indentify terminal type.");
                break;
            case 1:
                terminal_name.text = term.fullName;
                secLvl = term.secLvl;
                restrictedAccess = term.restrictedAccess;
                break;
            case 2:
                terminal_name.text = fab.fullName;
                secLvl = fab.secLvl;
                restrictedAccess = fab.restrictedAccess;
                break;
            case 3:
                terminal_name.text = scan.fullName;
                secLvl = scan.secLvl;
                restrictedAccess = scan.restrictedAccess;
                break;
            case 4:
                terminal_name.text = rep.fullName;
                secLvl = rep.secLvl;
                restrictedAccess = rep.restrictedAccess;
                break;
            case 5:
                terminal_name.text = rec.fullName;
                secLvl = rec.secLvl;
                restrictedAccess = rec.restrictedAccess;
                break;
            case 6:
                terminal_name.text = gar.fullName;
                secLvl = gar.secLvl;
                restrictedAccess = gar.restrictedAccess;
                break;
            case 7:
                terminal_name.text = custom.fullName;
                secLvl = custom.secLvl;
                restrictedAccess = custom.restrictedAccess;
                break;
            default:
                Debug.LogError("ERROR: Failed to indentify terminal type.");
                break;
        }

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
        float delay = 0.05f;

        // First, the hacking window opens
        terminal_hackingAreaRef.SetActive(true);
        terminal_hackingAreaRef.GetComponent<AudioSource>().PlayOneShot(terminal_hackingAreaRef.GetComponent<AudioSource>().clip, 0.7f); // Play the opening sound
        StartCoroutine(Terminal_HackBorderAnim());

        // Play (typing) sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[67]);

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
        hackSystemName.GetComponent<UIHackinfoV1>().Setup(HF.GetTerminalSystemName(terminal_targetTerm));

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

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);

        yield return new WaitForSeconds(delay);

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        terminal_targetResultBorders[0].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[1].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[2].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[3].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorder1.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        terminal_targetResultBorders[4].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[5].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[6].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorders[7].GetComponent<Image>().color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        terminal_targetResultBorder2.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
    }

    public void Terminal_InitTrace()
    {
        StartCoroutine(Terminal_TraceOpener());
    }

    private IEnumerator Terminal_TraceOpener()
    {
        Debug.Log("Tracing");
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

        yield return null;
    }

    public void Terminal_DoConsequences(Color setColor, string displayString, bool forceExit = true)
    {
        StartCoroutine(Terminal_InitConsequences(setColor, displayString, forceExit));
    }

    private IEnumerator Terminal_InitConsequences(Color setColor, string displayString, bool forceExit = true)
    {
        // We want to init the consequences thingy
        GameObject hackLock = Instantiate(terminal_locked_prefab, terminal_hackinfoArea1.transform.position, Quaternion.identity);
        hackLock.transform.SetParent(terminal_hackinfoArea1.transform);
        hackLock.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackinfoList.Add(hackLock);
        // Assign Details
        hackLock.GetComponent<UIHackLocked>().Setup(setColor, displayString);

        yield return null;

        if(forceExit)
        {
            Terminal_FailClose();
        }
    }

    private IEnumerator Terminal_OpenTargetResults()
    {
        terminal_targetresultsAreaRef.SetActive(true); // Enable the window
        terminal_targetresultsAreaRef.GetComponent<AudioSource>().PlayOneShot(terminal_targetresultsAreaRef.GetComponent<AudioSource>().clip, 0.7f); // Play the opening sound
        StartCoroutine(Terminal_TargetResultBorderAnim()); // Do the opener animation

        yield return new WaitForSeconds(0.4f);

        // Generate the hacking target options
        List<TerminalCommand> commands = new List<TerminalCommand>();
        int secLvl = 0;
        if (terminal_targetTerm.GetComponent<Terminal>()) // Open Terminal
        {
            commands = terminal_targetTerm.GetComponent<Terminal>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<Terminal>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<Fabricator>()) // Open Fabricator
        {
            commands = terminal_targetTerm.GetComponent<Fabricator>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<Fabricator>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<Scanalyzer>()) // Open Scanalyzer
        {
            commands = terminal_targetTerm.GetComponent<Scanalyzer>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<Scanalyzer>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<RepairStation>()) // Open Repair Station
        {
            commands = terminal_targetTerm.GetComponent<RepairStation>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<RepairStation>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<RecyclingUnit>()) // Open Recycling Unit
        {
            commands = terminal_targetTerm.GetComponent<RecyclingUnit>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<RecyclingUnit>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<Garrison>()) // Open Garrison
        {
            commands = terminal_targetTerm.GetComponent<Garrison>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<Garrison>().secLvl;
        }
        else if (terminal_targetTerm.GetComponent<TerminalCustom>()) // Open Custom Terminal
        {
            commands = terminal_targetTerm.GetComponent<TerminalCustom>().avaiableCommands;
            secLvl = terminal_targetTerm.GetComponent<TerminalCustom>().secLvl;
        }


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
            if(secLvl == 0)
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
                else if(secLvl == 3)
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

        yield return null;
    }

    public void Terminal_CreateResult(string text, Color setColor, string whiteText, bool tryDetection = false)
    {
        GameObject header = Instantiate(terminal_hackResults_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
        header.transform.SetParent(terminal_hackResultsArea.transform);
        header.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackResultsList.Add(header);
        // Assign Details
        header.GetComponent<UIHackResults>().Setup(whiteText, header.GetComponent<UIHackResults>().headerWhite);

        GameObject result = Instantiate(terminal_hackResults_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
        result.transform.SetParent(terminal_hackResultsArea.transform);
        result.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Add it to list
        terminal_hackResultsList.Add(result);
        // Assign Details
        result.GetComponent<UIHackResults>().Setup(text, setColor, true);

        //if (tryDetection)
        //{
            // Possibly increase detection chance
            HF.TraceHacking(UIManager.inst.terminal_targetTerm);
        //}
    }

    public void Terminal_CreateManualInput()
    {
        if(terminal_activeInput != null) // Destroy it if a clone exists
        {
            Destroy(terminal_activeInput);
            terminal_activeInput = null;
        }

        // We want to insert the input field prefab and set the focus to that.
        terminal_activeInput = Instantiate(terminal_input_prefab, terminal_hackResultsArea.transform.position, Quaternion.identity);
        terminal_activeInput.transform.SetParent(terminal_hackResultsArea.transform);
        terminal_activeInput.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        // Assign Details
        terminal_activeInput.GetComponent<UIHackInputfield>().Setup();
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

    public void Terminal_Close()
    {
        StartCoroutine(Terminal_CloseAnim());
    }

    private IEnumerator Terminal_CloseAnim()
    {
        // Do a fade out animation
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

        yield return null;

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

        terminal_hackinfoList.Clear(); // Clear the list
        terminal_hackTargetsList.Clear();
        terminal_hackResultsList.Clear();
        terminal_hackCodesList.Clear();
        // We won't go through and delete them because they will delete themselves

        // Close window
        terminal_hackingAreaRef.SetActive(false);
        terminal_targetresultsAreaRef.SetActive(false);
        codes_window.SetActive(false);

        // Un-assign target
        terminal_targetTerm = null;

        // Un-Freeze the player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;

    }

    public void Terminal_FailClose()
    {
        string name = "";
        if (terminal_targetTerm.GetComponent<Terminal>()) // Open Terminal
        {
            name = terminal_targetTerm.GetComponent<Terminal>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<Fabricator>()) // Open Fabricator
        {
            name = terminal_targetTerm.GetComponent<Fabricator>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<Scanalyzer>()) // Open Scanalyzer
        {
            name = terminal_targetTerm.GetComponent<Scanalyzer>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<RepairStation>()) // Open Repair Station
        {
            name = terminal_targetTerm.GetComponent<RepairStation>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<RecyclingUnit>()) // Open Recycling Unit
        {
            name = terminal_targetTerm.GetComponent<RecyclingUnit>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<Garrison>()) // Open Garrison
        {
            name = terminal_targetTerm.GetComponent<Garrison>().fullName;
        }
        else if (terminal_targetTerm.GetComponent<TerminalCustom>()) // Open Custom Terminal
        {
            name = terminal_targetTerm.GetComponent<TerminalCustom>().fullName;
        }
        string alertString = "ALERT: Suspicious activity at " + name + ". Dispatching Investigation squad.";
        GameManager.inst.DeploySquadTo("Investigation", terminal_targetTerm);
        UIManager.inst.CreateLeftMessage(alertString);
        UIManager.inst.CreateNewLogMessage(alertString, UIManager.inst.complexWhite, UIManager.inst.inactiveGray, false, true);
        
        StartCoroutine(Terminal_CloseAnim());
    }

    #endregion

    #region Custom Terminal
    [HideInInspector] public TerminalCustom cTerminal_machine;
    [Header("Custom Terminal")]
    public GameObject cTerminal_gibberishPrefab;

    public void CTerminal_Open(GameObject target)
    {
        // Freeze player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;

        // Set target
        cTerminal_machine = target.GetComponent<TerminalCustom>();
        terminal_targetTerm = target.gameObject;

        // Binary (Background)
        InvokeRepeating("Terminal_Binary", 0f, 1f);

        // Assign name + get Sec Level / Restricted access
        int secLvl = 0;
        bool restrictedAccess = true;

        terminal_name.text = cTerminal_machine.fullName;
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

        // First, the hacking window opens
        terminal_hackingAreaRef.SetActive(true);
        StartCoroutine(Terminal_HackBorderAnim());

        // Play (typing) sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[67]);

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
        hackSystemName.GetComponent<UIHackinfoV1>().Setup(HF.GetTerminalSystemName(terminal_targetTerm));

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
    }

    public void CTerminal_Close()
    {
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
            else if (i.GetComponent<UIHackGibb>())
            {
                i.GetComponent<UIHackGibb>().ShutDown();
            }
        }

        foreach (var i in terminal_hackTargetsList.ToList())
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
            if (i.GetComponent<UIHackCustomCode>())
            {
                i.GetComponent<UIHackCustomCode>().ShutDown();
            }
        }

        codes_window.GetComponent<UIHackCodes>().ShutDown();

        terminal_hackinfoList.Clear(); // Clear the list
        terminal_hackTargetsList.Clear();
        terminal_hackResultsList.Clear();
        terminal_hackCodesList.Clear();
        // We won't go through and delete them because they will delete themselves

        // Close window
        terminal_hackingAreaRef.SetActive(false);
        terminal_targetresultsAreaRef.SetActive(false);
        codes_window.SetActive(false);

        // Un-assign target
        cTerminal_machine = null;

        // Un-Freeze the player
        PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;
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
    //
    public Color coolBlue;
    public Color warmYellow;
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
    public Color siegeYellow;
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
    [Tooltip("Can be [T(ype)] [M(ass)] [I(ntegrity]")]
    public string inventoryDisplayMode = "I";
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


        // ---------- HEAT ------------
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
        else if(PlayerData.inst.currentHeat >= 200 && PlayerData.inst.currentHeat < 300) // HOT
        {
            heaDiffText.text = "Hot";
            heaDiffImage.color = warningOrange;
        }
        else // Critical
        {
            heaDiffText.text = "Critical";
            heaDiffImage.color = alertRed;
        }

        // Just the heat number
        currentHeatText.text = PlayerData.inst.currentHeat.ToString() + " ";

        // The two secondary numbers
        currentHeatText.text += "(";

        float heat1 = Mathf.Round(PlayerData.inst.heatRate1 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.heatRate1 < 0) // Negative (Green)
        {
            currentHeatText.text += "<color=#009700>" + heat1.ToString() + "</color>" + " ";
        }
        else // Positive (Red)
        {
            currentHeatText.text += "<color=#B20000>" + "+" + heat1.ToString() + "</color>" + " ";
        }
        float heat2 = Mathf.Round(PlayerData.inst.heatRate2 * 10.0f) * 0.1f; // sneaky method of rounding to 1 decimal point "##.#"
        if (PlayerData.inst.heatRate2 < 0) // Negative (Green)
        {
            currentHeatText.text += "<color=#009700>" + heat2.ToString() + "</color>";
        }
        else // Positive (Red)
        {
            currentHeatText.text += "<color=#B20000>" + "+" + heat2.ToString() + "</color>";
        }
        currentHeatText.text += ")";

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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
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
                    modeImage.color = siegeYellow;
                }

                break;
            default:
                break;
        }

        // - Location - (-##/???)

        string locationString = "";

        locationString = (MapManager.inst.currentLevel.ToString() + "/" + MapManager.inst.currentLevelName);
        locationText.text = locationString;
        

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
        core_slider.maxValue = PlayerData.inst.maxHealth;
        core_slider.value = PlayerData.inst.currentHealth;
        // Energy
        energy_slider.maxValue = PlayerData.inst.maxEnergy;
        energy_slider.value = PlayerData.inst.currentEnergy;
        // Matter
        matter_slider.maxValue = PlayerData.inst.maxMatter;
        matter_slider.value = PlayerData.inst.currentMatter;
        // Corruption
        corruption_slider.maxValue = PlayerData.inst.maxCorruption;
        corruption_slider.value = PlayerData.inst.currentCorruption;

        // -- Duplicate Sliders -- //
        // Core
        coreD_slider.maxValue = maxHealth;
        coreD_slider.value = currentHealth;
        // Energy
        energyD_slider.maxValue = maxEnergy;
        energyD_slider.value = currentEnergy;
        // Matter
        matterD_slider.maxValue = maxMatter;
        matterD_slider.value = currentMatter;
    }

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




        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0]);
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

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0]);
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

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0]);
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

        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0]);
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
                    scanSubTextA.text = "Primary Access";
                }
                scanSubTextA.color = highGreen;

                scanSubTextB.text = "> / ";
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
                    scanSubTextA.text = focusObj.GetComponent<Part>()._item.Name;
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
                if (focusObj.GetComponent<FloorTrap>().alignment != BotRelation.Friendly) 
                {
                    scanSubImage.color = dangerRed;
                    scanSubTextA.color = dangerRed;
                }
                else if (focusObj.GetComponent<FloorTrap>().alignment != BotRelation.Hostile)
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

    public void Evasion_ExpandMenu()
    {
        //StopCoroutine(Evasion_ExpandMenu_Animation());

        //evasionExtra.transform.GetChild(0).gameObject.SetActive(true);
        evasionExtra.GetComponent<UIMouseBounds>().disabled = true; // Disable opening detection
        evasion_hoverOpener.GetComponent<UIMouseBounds>().disabled = false; // Allow closing

        evasionAnimator.GetComponent<UIEvasionAnimation>().OpenMenu();
        //StartCoroutine(Evasion_ExpandMenu_Animation());
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
            evasionText3.text = "<color=#00FFF5>" + "Speed + " + PlayerData.inst.evasion3 + "</color>"; // Cyan
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

    #endregion

    /// <summary>
    /// Updates the [ Parts ] area on the UI
    /// </summary>
    public void UpdateParts()
    {
        // - Weight -
        string weight = PlayerData.inst.currentWeight.ToString() + "/" + PlayerData.inst.maxWeight.ToString();

        weightText.text = weight;
        if (PlayerData.inst.currentWeight > PlayerData.inst.maxWeight)
        {
            weightText.color = dangerRed;
        }
        else
        {
            weightText.color = normalGreen;
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

    /// <summary>
    /// Updates the [ Inventory ] area on the UI
    /// </summary>
    public void UpdateInventory()
    {
        // - Inventory Size -
        string invSize = PlayerData.inst.currentInvCount.ToString() + " " + PlayerData.inst.maxInvSize.ToString();
        inventorySize_text.text = invSize;


    }

    private string PlusMinusHelper(float num)
    {
        if(num >= 0)
        {
            return "+";
        }
        else
        {
            return ""; // Already negative so "-" is already there
        }
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

    


}
