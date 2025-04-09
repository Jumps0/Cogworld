// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Tile = UnityEngine.Tilemaps.Tile;

/// <summary>
/// A script used for the physical *real world* tiles used to build the world. What this tile is gets determined by its "tileInfo" (a TileObject variable).
/// </summary>
public class TileBlock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Color _baseColor, _offsetColor;
    public SpriteRenderer _renderer;
    [SerializeField] public GameObject _highlight;
    public GameObject _highlightPerm; // Used for map border mostly
    public GameObject _debrisSprite;
    [SerializeField] private GameObject _collapseSprite;
    [SerializeField] private Animator _collapseAnim;
    [Tooltip("The current item laying on top of this Tile, if none then = null.")]
    public Part _partOnTop = null;
    public GameObject targetingHighlight;
    public Animator flashWhite;
    [Tooltip("What this tile would look in DOS mode.")]
    public GameObject dosLetter;
    [Tooltip("Alternate state for this tile.")]
    public GameObject subTile;

    [Header("Details")]
    [Tooltip("The specific details of what this tile is, set upon spawning.")]
    public TileObject tileInfo;
    public bool isDirty = false;
    [Tooltip("Should this tile's highlight be blinking?")]
    public bool blinkMode = false;
    [Tooltip("Can this tile be walked on?")]
    public bool walkable;
    [Tooltip("Is something currently occupying this space?")]
    public bool occupied = false;
    [Tooltip("This tile has been damaged/destroyed, and is displaying a different sprite.")]
    public bool damaged = false;
    public Vector2Int location;

    [Header("Variants")]
    public bool isDoor = false;
    public bool isAccess = false;
    public bool isPhaseWall = false;
    public bool isImpassible = false;

    [Header("Door")]
    public bool door_open = false;

    [Header("ACCESS")]
    public int access_destination;
    public bool access_isBranch = false;

    [Header("Visibility")]
    public bool isExplored;
    public bool isVisible;
    [Tooltip("If true, this tile won't block LOS like normal. Used for open doors.")]
    public bool specialNoBlockVis = false;
    [SerializeField] private Color visc_white;
    [SerializeField] private Color visc_gray;

    [Header("Colors")]
    public Color intel_green;
    [SerializeField] private Color unstableCollapse_red;
    [SerializeField] private Color trojan_Blue;
    [SerializeField] private Color highlight_white;
    [SerializeField] private Color caution_yellow;

    [Header("Phase Wall")]
    [Tooltip("Which team can *use* this phase wall?")]
    public BotAlignment phaseWallTeam = BotAlignment.Complex;
    public Sprite phaseWallSprite;
    [Tooltip("Does the player know this tile is actually a phase wall?")]
    public bool phaseWall_revealed = false;

    public void GInit(bool isOffset)
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.sprite = tileInfo.displaySprite.sprite; // Set the sprite
        if (tileInfo.type == TileType.Wall || tileInfo.type == TileType.Machine) // Walls/Machines take up space
        {
            occupied = true;
            this.gameObject.layer = LayerMask.NameToLayer("BlockVision");
        }
        else if (tileInfo.type == TileType.Floor)
        {
            occupied = false; // This is a risky assumption!
            this.gameObject.layer = LayerMask.NameToLayer("Floor");
        }
        //this.tileInfo.currentVis = TileVisibility.Visible;
        //isDirty = tileInfo.isDirty; // Set dirt flag
        SetHighlightPerma(this.tileInfo.impassable); // Set perma highlight

        TurnManager.inst.turnEvents.onTurnTick += TurnTick; // Begin listening to this event

        // Set sprite vis colors
        visc_white = tileInfo.asciiColor;
        visc_gray = HF.GetDarkerColor(visc_white, 0.3f);
    }

    #region Vision/Display
    public void UpdateVis(byte update)
    {
        // See https://www.youtube.com/watch?v=XNcEZHqtC0g for the many ways to finding problems when optimizing in Unity
        // this too https://www.youtube.com/watch?v=CJ94gOzKqsM (step 1 is to move vis out of blocks)

        if (update == 0) // UNSEEN/UNKNOWN
        {
            isExplored = false;
            isVisible = false;
        }
        else if(update == 1) // UNSEEN/EXPLORED
        {
            isExplored = true;
            isVisible = false;
        }
        else if(update == 2) // SEEN/EXPLORED
        {
            isExplored = true;
            isVisible = true;
        }

        if (recentlyRevealedViaIntel)
        {
            if (!isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = UIManager.inst.dullGreen;
            }
            return;
        }

        if (isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = visc_white;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = visc_white;
            }
            recentlyRevealedViaIntel = false;
        }
        else if (isExplored && isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = visc_white;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = visc_white;
            }
            recentlyRevealedViaIntel = false;
        }
        else if (isExplored && !isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = visc_gray;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = visc_gray;
            }
        }
        else if (!isExplored)
        {
            this.GetComponent<SpriteRenderer>().color = Color.black;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

        // Part on top check
        if(_partOnTop != null)
        {
            //HF.SetGenericTileVis(_partOnTop.gameObject, update);
        }
    }

    private Coroutine firstTimeReveal = null;
    public bool firstTimeRevealed = false;
    public void FirstTimeReveal()
    {
        if (firstTimeReveal != null)
        {
            StopCoroutine(firstTimeReveal);
        }
        firstTimeReveal = StartCoroutine(RevealAnim());
    }

    public IEnumerator RevealAnim()
    {
        // The animator method doesn't work anymore so we're doing this instead.

        // We will just hijack the highlight object and breifly flash it from green -> green 0% opacity
        float startPercent = 0.3f;
        Color start = new Color(UIManager.inst.dullGreen.r, UIManager.inst.dullGreen.g, UIManager.inst.dullGreen.b, startPercent);
        Color end = new Color(UIManager.inst.dullGreen.r, UIManager.inst.dullGreen.g, UIManager.inst.dullGreen.b, 0f);

        _highlight.SetActive(true);
        _highlight.GetComponent<SpriteRenderer>().color = start;

        float elapsedTime = 0f;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            _highlight.GetComponent<SpriteRenderer>().color = new Color(start.r, start.g, start.b, Mathf.Lerp(startPercent, 0f, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _highlight.GetComponent<SpriteRenderer>().color = end;
        _highlight.SetActive(false);

        firstTimeRevealed = true;
    }

    bool recentlyRevealedViaIntel = false;

    public void RevealViaZone(bool doSound = false)
    {
        StartCoroutine(RevealZone(doSound));
    }

    private IEnumerator RevealZone(bool doSound = false)
    {
        while (/*UIManager.inst.terminal_targetTerm*/ false)
        {
            yield return null; // Wait until the terminal screen is closed
        }

        if (doSound)
        {
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["ACCESS"], 0.5f); // UI - ACCESS
        }

        isExplored = true;

        /*
         *  NOTE: The animator being enabled breaks the lighting (Fog of War).
         *  So we only want it to be on when needed.
         */

        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play("TileRevealZone");

        recentlyRevealedViaIntel = true;

        yield return new WaitForSeconds(1f);

        this.GetComponent<SpriteRenderer>().color = UIManager.inst.dullGreen; // Set to dull green at the end

        if (this != null) // contingency
        {
            this.GetComponent<Animator>().enabled = false;
        }
    }


    public void SetToDirty()
    {
        // Activate the debris and set it to a random sprite
        this.isDirty = true;
        this._debrisSprite.SetActive(true);
        _debrisSprite.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.debrisSprites[Random.Range(0, MiscSpriteStorage.inst.debrisSprites.Count)].sprite;
    }

    public void CleanSpriteDebris()
    {
        this.isDirty = false;
        this._debrisSprite.SetActive(false);
        _debrisSprite.GetComponent<SpriteRenderer>().sprite = null;
    }

    public void SetHighlightPerma(bool state)
    {
        Color halfColor = new Color((Color.white.r / 2) / 255f, (Color.white.g / 2) / 255f, (Color.white.b / 2) / 255f);
        _highlightPerm.GetComponent<SpriteRenderer>().color = halfColor;
        _highlightPerm.SetActive(state);
    }

    [SerializeField] private Animator _secretDoorAnimator;
    public void SecretDoorReveal()
    {
        _secretDoorAnimator.gameObject.SetActive(true);
        _secretDoorAnimator.enabled = true;
        _secretDoorAnimator.Play("TileSecretDoorReveal");
    }

    #endregion

    #region Creation & Destruction

    /// <summary>
    /// Sets this tile to its destroyed state.
    /// </summary>
    public void DestroyMe()
    {
        damaged = true;

        // Change the sprite
        this.GetComponent<SpriteRenderer>().sprite = tileInfo.destroyedSprite.sprite;

        if(tileInfo.type == TileType.Floor)
        {
            // Clean your floors with this simple trick!
            CleanSpriteDebris();
        }

        // Activate the "danger roof will collapse" red indicator (if needed)
        if (tileInfo.type == TileType.Wall)
        {
            _collapseSprite.SetActive(true);
            _collapseAnim.enabled = true;
            _collapseAnim.Play("TileAnimCollapse");

            specialNoBlockVis = true;
        }

        // Change walkablility if needed
        if(tileInfo.type == TileType.Wall || tileInfo.type == TileType.Machine || tileInfo.type == TileType.Door)
        {
            walkable = true;
            occupied = false;
        }

        // Play a sound

        AudioClip clip = HF.RandomClip(tileInfo.destructionClips);
        if(!AudioManager.inst.activeTempClips.Contains(clip))
        { // We do this so we don't blow the player's ears out by stacking up the same clip
            AudioManager.inst.CreateTempClip(this.transform.position, clip);
        }

            // Add to MapManager list
        if (tileInfo.type != TileType.Machine) // Machines don't get repaired
            MapManager.inst.damagedStructures.Add(this.gameObject);

    }

    /// <summary>
    /// Sets this tile to its repaired state from its damaged state.
    /// </summary>
    public void RepairMe()
    {
        damaged = false;

        // Change the sprite
        this.GetComponent<SpriteRenderer>().sprite = tileInfo.displaySprite.sprite;

        if(tileInfo.type == TileType.Wall)
        {
            specialNoBlockVis = false; // Revert special state
        }

        // De-active the "danger roof will collapse" red indicator (if its active)
        if (_collapseSprite.activeInHierarchy)
        {
            _collapseSprite.SetActive(false);
            _collapseAnim.enabled = false;
        }

        // Change walkablility if needed
        if (tileInfo.type == TileType.Wall || tileInfo.type == TileType.Machine || tileInfo.type == TileType.Door)
        {
            walkable = false;
            occupied = true;
        }

        // Remove from MapManager list
        if(MapManager.inst.damagedStructures.Contains(this.gameObject))
            MapManager.inst.damagedStructures.Remove(this.gameObject);

    }

    #endregion

    #region Phase Wall

    /// <summary>
    /// Reveal that this tile is actually a phase wall to the player.
    /// </summary>
    public void PhaseWallReveal()
    {
        // Play an animation?
        // Play a sound?
        phaseWall_revealed = true;
        _renderer.sprite = phaseWallSprite;
        specialNoBlockVis = true; // Let the player see through it
    }

    /// <summary>
    /// Make this phase wall passible & play a sound.
    /// </summary>
    public void PhaseWall_Open()
    {
        // Play the open sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_door[$"HEAVY_OPEN_{Random.Range(1, 2)}"]); // HEAVY_OPEN_1/2

        walkable = true;

        // Reveal to the player if the player can see this happening
        if (!phaseWall_revealed && phaseWallTeam != BotAlignment.Player)
        {
            if(PlayerData.inst.GetComponent<Actor>().FieldofView.Contains(new Vector3Int((int)this.transform.position.x, (int)this.transform.position.y)))
            {
                PhaseWallReveal();
            }
        }
    }

    public void PhaseWall_Close()
    {
        // Play the close sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_door[$"PHASE_CLOSE_{Random.Range(1, 3)}"]); // PHASE_CLOSE_1/2/3

        walkable = false;

        // Reveal to the player if the player can see this happening
        if (!phaseWall_revealed && phaseWallTeam != BotAlignment.Player)
        {
            if (PlayerData.inst.GetComponent<Actor>().FieldofView.Contains(new Vector3Int((int)this.transform.position.x, (int)this.transform.position.y)))
            {
                PhaseWallReveal();
            }
        }
    }

    #endregion

    #region Events
    private void OnDisable()
    {
        if (GameManager.inst)
        {
            TurnManager.inst.turnEvents.onTurnTick -= TurnTick; // Stop listening to this event
        }
    }

    private void OnDestroy()
    {
        if (TurnManager.inst)
        {
            TurnManager.inst.turnEvents.onTurnTick -= TurnTick; // Stop listening to this event
        }
    }

    private void TurnTick()
    {
        // There are a couple instances where we need to do something or check something

    }
    #endregion
}

/// <summary>
/// Contains all information regarding a tile at a specific position.
/// </summary>
public struct WorldTile
{
    [Header("Info")]
    public Vector2Int location;
    public TileObject tileInfo;
    [Tooltip("There are 3 visibility states. 0 = UNSEEN/UNEXPLORED | 1 = UNSEEN/EXPLORED | 2 = SEEN/EXPLORED ")]
    public byte vis;
    public bool doneRevealAnimation;
    public bool revealedViaIntel;

    [Header("Tile Resistances")]
    public List<BotResistances> resistances;

    [Header("States")]
    [Tooltip("Where -1 = Not dirty, and any other number indicates the ID of the debris sprite.")]
    public int isDirty;
    public bool isImpassible;
    [Tooltip("Is this tile currently damaged? Default is FALSE. Does not refer to machines.")]
    public bool isDamaged;

    [Header("Variants")]
    public TileType type;
    [Header(" Door")]
    [Tooltip("Is this door currently open?")]
    public bool door_open;
    public bool isSecretDoor;
    [Header(" Access")]
    public bool access_branch;
    public int access_destination;
    [Tooltip("Does the player know where this leads? If not will display as '???'")]
    public bool access_knownDestination;
    public string access_destinationName;
    [Header(" Phase")]
    public bool isPhaseWall;
    [Tooltip("Which team can *use* this phase wall?")]
    public BotAlignment phase_team;
    [Tooltip("Has this phase wall been revealed to the player?")]
    public bool phase_revealed;
    [Header(" Trap")]
    public TileTrapData trap_data;
    [Tooltip("What faction this trap belongs to, will detonate vs all other hostile factions via player's tree.")]
    public TrapType trap_type;
    public bool trap_tripped;
    public bool trap_active;
    public bool trap_knowByPlayer;
    public BotAlignment trap_alignment;

    [Header("Machine")]
    public MachineData machinedata;

    #region Destruction
    public void SetDestroyed(bool destroyed, string message = "")
    {
        if (destroyed == isDamaged) { return; } // Don't do anything if we are already at this state!
        
        isDamaged = destroyed;
        MapManager.inst.mapdata[location.x, location.y].isDamaged = isDamaged; // This is annoying. Without this the value doesn't actually change

        if (destroyed)
        {
            // Modify pathing (can now be free)
            if (type != TileType.Floor && MapManager.inst.pathdata[location.x, location.y] > 0) { MapManager.inst.pathdata[location.x, location.y] = 0; }

            // Create a CALC message
            if(message != "")
                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);

            // If this is a wall tile we need to warn the player it could collapse
            if(type == TileType.Wall)
            {
                GameManager.inst.WarningPulseAdd(location);
            }

            // TODO
        }
        else
        {
            // Modify pathing (it should now go back to being obstructed)
            if (type != TileType.Floor && MapManager.inst.pathdata[location.x, location.y] != 0) { MapManager.inst.pathdata[location.x, location.y] = HF.TileObstructionType(type); }

            // Create a LOG message indicating this tile has been repaired (TODO)
            if (message != "")
                UIManager.inst.CreateNewLogMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);

            // Reset doors
            if (type == TileType.Door) { door_open = false; }

            // If this is a wall tile we should remove its warning pulser if it exists
            if (type == TileType.Wall)
            {
                GameManager.inst.WarningPulseRemove(location);
            }

            // TODO
        }

        // Update this tile's visibility
        MapManager.inst.UpdateTile(this, location);
        // And update the Fog of War since this probably changes local visibility in some way.
        TurnManager.inst.AllEntityVisUpdate(true);

    }
    #endregion

    #region Doors
    /// <summary>
    /// An array containing 9 positions, where if ANY bot is at one of those positions, the door should be open.
    /// </summary>
    private Vector2Int[] doorTiles;
    /// <summary>
    /// Preload what tiles will open this door if a bot is inside of one.
    /// </summary>
    public void PreloadDoorTiles()
    {
        doorTiles = new Vector2Int[5];
        doorTiles[0] = location; // Center

        // Neighbors
        doorTiles[1] = location + Vector2Int.up;
        doorTiles[2] = location + Vector2Int.down;
        doorTiles[3] = location + Vector2Int.left;
        doorTiles[4] = location + Vector2Int.right;

        /* // No diagonals!
        doorTiles[5] = location + Vector2Int.up + Vector2Int.left;
        doorTiles[6] = location + Vector2Int.up + Vector2Int.right;
        doorTiles[7] = location + Vector2Int.down + Vector2Int.left;
        doorTiles[8] = location + Vector2Int.down + Vector2Int.right;
        */
    }


    /// <summary>
    /// Check if the door should open or close.
    /// </summary>
    public void DoorStateCheck()
    {
        // (There is def a more optimal way to do this)
        bool botNearby = false;
        foreach (Vector2Int T in doorTiles)
        {
            if (MapManager.inst.pathdata[T.x, T.y] == 2) // Is there a bot here?
            {
                botNearby = true;
                break;
            }
        }

        if (botNearby) // If there is a bot nearby
        {
            if (!door_open) // If we're currently closed, we need to open
            {
                ToggleDoor(true);
            }
        }
        else if (!botNearby) // If there isn't a bot nearby
        {
            if (door_open) // If we're currently open, we need to close
            {
                ToggleDoor(false);
            }
        }
    }

    public void ToggleDoor(bool open)
    {
        AudioClip clip = null;

        // Change state
        door_open = open;
        MapManager.inst.mapdata[location.x, location.y].door_open = door_open;

        if (open)
        {
            // Pick sound clip
            clip = tileInfo.door_open[Random.Range(0, tileInfo.door_open.Count - 1)];
        }
        else
        {
            // Pick sound clip
            clip = tileInfo.door_open[Random.Range(0, tileInfo.door_close.Count - 1)];
        }

        // Play sound
        AudioManager.inst.CreateTempClip(new Vector3(location.x, location.y), clip, 0.4f); // (Note: This will need to be altered later if/when the audio system is updated to be local)

        // Update the tile
        MapManager.inst.UpdateTile(this, location);

        // Update the FOV
        TurnManager.inst.AllEntityVisUpdate(true);
    }
    #endregion

    #region Traps
    public void LocateTrap()
    {
        #region Vision
        vis = 2; // There is no possible way this could backfire
        MapManager.inst.mapdata[location.x, location.y].vis = vis;

        // Update the tile
        MapManager.inst.UpdateTile(this, location);

        // Update the FOV
        TurnManager.inst.AllEntityVisUpdate(true);
        #endregion

        trap_knowByPlayer = true;
        MapManager.inst.mapdata[location.x, location.y].trap_knowByPlayer = trap_knowByPlayer;

        // TODO: COME BACK TO THIS
        //UIManager.inst.CreateItemPopup(this.gameObject, trap_data.trapname, Color.black, tileInfo.asciiColor, HF.GetDarkerColor(tileInfo.asciiColor, 20f));

        AudioManager.inst.CreateTempClip(new Vector3(location.x, location.y), AudioManager.inst.dict_ui["TRAP_SCAN"]); // UI - TRAP_SCAN
    }

    public void SetTrapAlignment(BotAlignment newA)
    {
        trap_alignment = newA;
        MapManager.inst.mapdata[location.x, location.y].trap_alignment = trap_alignment;
    }

    public IEnumerator TripTrap(Actor victim)
    {
        trap_tripped = true;
        MapManager.inst.mapdata[location.x, location.y].trap_tripped = trap_tripped;

        if (victim != null && victim == PlayerData.inst.GetComponent<Actor>())
        {
            // Freeze player
            PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;
        }

        // Play trap triggered sound
        AudioManager.inst.CreateTempClip(new Vector3(location.x, location.y), AudioManager.inst.dict_game["TRAPTRIGGER"]); // GAME - TRAPTRIGGER

        // Log a message
        UIManager.inst.CreateNewLogMessage("Triggered " + trap_data.trapname, UIManager.inst.warningOrange, UIManager.inst.corruptOrange_faded, false, true);

        Debug.Log("Mine tripped!");

        // This trap tile is now a floor tile
        type = TileType.Floor;
        MapManager.inst.mapdata[location.x, location.y].type = type;
        MapManager.inst.UpdateTile(this, location); // (Occasionally irrelivent but that's ok)

        yield return new WaitForEndOfFrame();

        #region EXPLOSION!
        switch (trap_type)
        {
            case TrapType.Alarm: // Triggers an alarm

                break;
            case TrapType.Blade: // Removes some parts (+ minor damage to those parts)

                break;
            case TrapType.Chute: // 1 way ticket to the Wastes

                break;
            case TrapType.DirtyBomb: // Big EMP boom

                break;
            case TrapType.EMP: // EMP boom

                break;
            case TrapType.Fire: // Fire boom
                break;

            case TrapType.HE: // Big boom

                break;
            case TrapType.Hellfire: // Big fire boom(?)

                break;
            case TrapType.ProtonBomb: // Big (laser/thermal?) bomb(?)

                break;
            case TrapType.Segregator:

                break;
            case TrapType.Shock: // Small EMP (?)

                break;
            case TrapType.Stasis: // Freeze for X strength

                break;
            case TrapType.NONE:
                break;
            default:
                break;
        }
        #endregion

        if (victim != null && victim == PlayerData.inst.GetComponent<Actor>())
        {
            // Un-Freeze player
            PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;
        }

        // TODO: Remove this trap from whatever zone its in
    }

    /// <summary>
    /// Disarms the trap, making it no longer a threat to anyone.
    /// </summary>
    public void DeActivateTrap()
    {
        trap_active = false;
        MapManager.inst.mapdata[location.x, location.y].trap_active = trap_active;
    }

    /// <summary>
    /// Arms a trap, making it a threat to whichever alignment it is set to.
    /// </summary>
    public void ActivateTrap(BotAlignment newA)
    {
        trap_active = true;
        trap_alignment = newA;

        MapManager.inst.mapdata[location.x, location.y].trap_active = trap_active;
        MapManager.inst.mapdata[location.x, location.y].trap_alignment = trap_alignment;
    }
    /// <summary>
    /// Remove this trap from its "on floor" state and turn it into an item that can be picked up.
    /// </summary>
    public void ItemizeTrap()
    {

    }

    #endregion

    #region Exits (ACCESS)
    public void PingExit()
    {
        bool found = false;
        // If a popup for this doesn't already exist we need to create one.
        foreach (GameObject P in UIManager.inst.exitPopups)
        {
            if (P.GetComponentInChildren<UIExitPopup>()._parent == this.location)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            UIManager.inst.CreateExitPopup(this, access_destinationName);
            if (!MapManager.inst.firstExitFound) // If this is the first exit the player has found (on this level), display a log message.
            {
                MapManager.inst.firstExitFound = true;
                UIManager.inst.CreateNewLogMessage("EXIT=FOUND", UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, false, true);
            }
        }
    }
    #endregion

    #region Secret Door
    public void SecretDoorReveal()
    {
        // TODO
    }
    #endregion

    #region Machines
    // Dear god this is gonna be a lot.

    /// <summary>
    /// Intializes this machine, called upon Map Intialization in `MapManager`.
    /// </summary>
    public void MachineInit()
    {
        machinedata.avaiableCommands = new List<TerminalCommand>();
        machinedata.trojans = new List<HackObject>();

        switch (machinedata.type)
        {
            case MachineType.Fabricator:
                machinedata.displayName = "Fabricator";

                machinedata.FabricatorInit();
                break;
            case MachineType.Garrison:
                machinedata.displayName = "Garrison";

                machinedata.GarrisonInit();
                break;
            case MachineType.Recycling:
                machinedata.displayName = "Recycling Unit";

                machinedata.RecyclingInit();
                break;
            case MachineType.RepairStation:
                machinedata.displayName = "Repair Station";

                machinedata.RepairBayInit();
                break;
            case MachineType.Scanalyzer:
                machinedata.displayName = "Scanalyzer";

                machinedata.ScanalyzerInit();
                break;
            case MachineType.Terminal:
                machinedata.displayName = "Terminal";
                machinedata.terminalZone = new TerminalZone();
                machinedata.terminalZone.assignedTerminal = location;
                machinedata.terminalZone.assignedArea = new List<Vector2Int>();

                machinedata.TerminalInit();
                break;
            case MachineType.CustomTerminal:
                // TODO

                // NOTE: Since these are custom, they will be placed specifically, and have some details pre-assigned.
                machinedata.CustomTerminalInit();
                break;
            case MachineType.DoorTerminal:
                // TODO

                machinedata.DoorTerminal_Init();
                break;
            case MachineType.Static:
                // TODO

                machinedata.Static_Init();
                break;
            case MachineType.None:
                machinedata.displayName = "NONE";
                Debug.LogWarning($"Machine @ ({location.x}, {location.y}) has no type!");
                break;
            default:
                machinedata.displayName = "NONE";
                Debug.LogWarning($"Machine @ ({location.x}, {location.y}) has no type!");
                break;
        }

        // Color setup
        machinedata.activeColor = HF.GetMachineColor(machinedata.type);
        machinedata.disabledColor = Color.gray;

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y] = this;
    }

    /// <summary>
    /// Triggered by an intel reveal. We need to set this entire machine (children included) to green, and create a Border Indicator.
    /// </summary>
    public void MachineReveal()
    {
        // Get parent position
        Vector2Int parentPos = machinedata.parentLocation;

        List<Vector2Int> pieces = machinedata.children.ToList(); // Gather up all children
        pieces.Add(parentPos); // Include the parent as well

        // Go through pieces and set them all to revealing, along with settings the flag
        foreach (var P in pieces)
        {
            if (MapManager.inst.mapdata[P.x, P.y].vis == 0) { MapManager.inst.mapdata[P.x, P.y].vis = 1; } // Set all unexplored to explored

            MapManager.inst.mapdata[P.x, P.y].revealedViaIntel = true; // Set intel flag so it appears green
        }

        // !! Vision Update !!
        TurnManager.inst.AllEntityVisUpdate(true);
    }

    /// <summary>
    /// TO BE USED BY THE PARENT OBJECT ONLY! Tells the children of a parent machine part the location of the parent part.
    /// </summary>
    /// <param name="pos">Location of the parent machine part.</param>
    public void AssignParentLocation(Vector2Int pos)
    {
        foreach (Vector2Int P in machinedata.children)
        {
            MapManager.inst.mapdata[P.x, P.y].machinedata.parentLocation = pos;
        }
    }
    #endregion
}

/// <summary>
/// Will eventually replace the `MachinePart` script and store information about a machine at this position inside a WorldTile struct.
/// </summary>
public struct MachineData
{
    [Header("Visual")]
    public Tile sprite_normal;
    public Tile sprite_ascii;

    [Header("Basic Info")]
    [Tooltip("The location of this machine tile in the world.")]
    public Vector2Int location;
    [Tooltip("The general (generic) name for this machine. Mostly used in logging (ex: Garrison). Set upon startup in MachineData.")]
    public string displayName;
    [Tooltip("What this machine is reffered to as in the Terminal (Hacking) window. [Set in `AssignMachineNames()` inside `MapManager` upon startup.]")]
    public string terminalName;
    [Tooltip("What this machine is reffered to as in logging printouts. [Set in `AssignMachineNames()` inside `MapManager` upon startup.]")]
    public string logName;
    public MachineType type;
    [Tooltip("Is this machine ACTIVE/USABLE?")]
    public bool state;
    [Tooltip("Seperated from the based `isDamaged` because not only can this be destroyed but the floor before it can be to.")]
    public bool machineIsDestroyed;

    [Header("Explosion")]
    [Tooltip("Does this machine explode? Usually only some static machines do this.")]
    public bool explodes;
    public int explosion_heattransfer;
    public ExplosionGeneric explosion_details;
    public ItemExplosion explosion_potential;

    [Header("Random Junk")]
    [Tooltip("Instead of being a normal machine, this is just a piece of junk on the map that some NPCs can pass through.")]
    public bool isRandomJunk;
    [Tooltip("If set as a piece of junk, certain factions of NPC can walk through this.")]
    public bool junk_walkable;

    [Header("Parent & Ownership")]
    [Tooltip("Location of the parent part of this machine. If this machine IS the parent then this and `location` will be the same.")]
    public Vector2Int parentLocation;
    [Tooltip("If this part is the parent, will contain all other children that make up this machine.")]
    public Vector2Int[] children;
    [Tooltip("Reference to the UI indicator showing where this machine is IF it is currently not on the screen.")]
    public GameObject indicator;

    [Header("Colors")]
    [Tooltip("The color of this machine and its parts when its active. [Set in the Init function]")]
    public Color activeColor;
    [Tooltip("The color of this machine and its parts when it is damaged/destroyed an thus inactive. [Set in the Init function]")]
    public Color disabledColor;

    [Header("** Interactable Machine Variables **")]
    [Header("Hacking Info")]
    [Tooltip("Mostly flavor. If true, enables tracing.")]
    public bool restrictedAccess;
    [Tooltip("0, 1, 2, 3. 0 = Open System")]
    public int secLvl;
    [Tooltip("The detection chance while hacking.")]
    public float detectionChance;
    [Tooltip("If detected in hacking, what is the trace progress.")]
    public float traceProgress;
    [Tooltip("While hacking, has the user been detected?")]
    public bool detected;
    [Tooltip("Is this machine no longer accessible (in lockdown).")]
    public bool locked;
    [Tooltip("How many times has the user interacted with this machine.")]
    public int timesAccessed;

    [Header("Commands")]
    public List<TerminalCommand> avaiableCommands;

    [Header("Trojans")]
    public List<HackObject> trojans;

    [Tooltip("[PARENT ONLY] The ideal location to drop items, spawn bots, and place exits.")]
    public Vector2Int dropSpot;

    [Tooltip("If this machine does a timed operation, this is the location of the timer which is displayed in the world, on top of the parent part.")]
    public Vector2Int timerObjectLocation;

    // TODO: !! All the other stuff needed for the individual interactable machines !!

    // Future note:
    // -How to handle Machine audio: https://www.gridsagegames.com/blog/2020/06/building-cogminds-ambient-soundscape/

    #region Generic Operations
    [Header("Generic Operations")]
    [Tooltip("How long it will take to work on the specified component.")]
    public int timeToComplete;
    [Tooltip("When (based on the current global time) this machine operation started.")]
    public int begunBuildTime;
    [Tooltip("Is this machine currently working on something? (Fabricators, Repair Bays, Scanalyzers, etc. use this)")]
    public bool atWork;
    [Tooltip("The part this machine is focusing on.")]
    public ItemObject desiredPart;
    [Tooltip("The bot this machine is focusing on.")]
    public BotObject desiredBot;

    /// <summary>
    /// Called every turn, used by most "operational" machines when they are busy working on something.
    /// </summary>
    public void OperationTick()
    {
        if(locked) { return; }

        // TODO
        if (atWork)
        {
            switch (type)
            {
                case MachineType.Fabricator:
                    MapManager.inst.machine_timers[location].GetComponent<UITimerMachine>().Tick();

                    if (TurnManager.inst.globalTime >= begunBuildTime + timeToComplete)
                    {
                        Fabricator_FinishBuild();
                    }
                    break;
                case MachineType.Garrison:
                    break;
                case MachineType.Recycling:
                    break;
                case MachineType.RepairStation:
                    break;
                case MachineType.Scanalyzer:
                    break;
                case MachineType.Terminal:
                    break;
                case MachineType.CustomTerminal:
                    break;
                case MachineType.DoorTerminal:
                    break;
                case MachineType.Static:
                    if (static_flag_detonate || static_flag_unstable) // There is definently a difference between the two but i'm not sure what it is right now.
                    {
                        if(static_timeToDetonation > 0)
                        {
                            static_timeToDetonation--;
                        }
                        else
                        {
                            Static_Detonate();
                        }
                    }
                    break;
                case MachineType.None:
                    break;
                default:
                    break;
            }
        }

        if(type == MachineType.Recycling)
        {
            Recycling_OverflowCheck();
        }
    }
    #endregion

    #region Terminal
    [Header("Terminal")]
    public TerminalZone terminalZone;
    [Tooltip("The Operator class Actor assigned to monitor this machine.")]
    public Actor terminalOverseer;
    public List<TerminalCustomCode> terminalCustomCodes;
    public List<ItemObject> storedObjects;
    public bool databaseLockout;
    public void TerminalInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Terminal;

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        int amount = 0;

        // -- We want to generate:
        // - 1-4 Lore entries
        // - 0-2 Access+Alert entries
        // - 0-1 Analysis entries (scaled to level)
        // - 0-2 Enumerate entries
        // - 0-1 Index entries
        // - 0-1 Layout(Zone) entries
        // - 0-1 Recall entries (make sure to check if there is one first)
        // - 0-1 Schematic (item) entries (scaled to level)
        // - 0-1 Schematic (bot) entries (scaled to level)
        // - 0-1 Traps entries

        // Lore
        for (int i = 0; i < Random.Range(1, 4); i++)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);
            //
            KnowledgeObject data = MapManager.inst.knowledgeDatabase.Data[Random.Range(0, MapManager.inst.knowledgeDatabase.Data.Length)];
            string displayText = "\"" + data.name + "\"";

            HackObject hack = MapManager.inst.hackDatabase.dict["Query"];

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Query, "Record", hack, data);

            avaiableCommands.Add(newCommand);

            amount++;
        }

        // Access [73-75] Alert [76-77]
        if (Random.Range(0f, 1f) >= 0.4f)
        {
            // Access here
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string[] options = { "Access(Branch)", "Access(Emergency)", "Access(Main)" };
            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Length - 1)]];
            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Access, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        if (Random.Range(0f, 1f) >= 0.5f)
        {
            // Alert here
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string[] options = { "Alert(Check)", "Alert(Purge)" };
            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Length - 1)]];
            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Alert, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Analysis [78-87]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Analysis([Bot Name]) - Tier {tier}"];

            BotObject bot = HF.FindBotOfTier(tier);

            string displayText = bot.botName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Analysis, "Analysis", hack, null, null, bot);

            avaiableCommands.Add(newCommand);
        }

        // Enumerate [91-103]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Enumerate(Assaults)");
            options.Add("Enumerate(Coupling)");
            options.Add("Enumerate(Exterminations)");
            options.Add("Enumerate(Garrison)");
            options.Add("Enumerate(Guards)");
            options.Add("Enumerate(Intercept)");
            options.Add("Enumerate(Investigations)");
            options.Add("Enumerate(Maintenance)");
            options.Add("Enumerate(Patrols)");
            options.Add("Enumerate(Reinforcements)");
            options.Add("Enumerate(Squads)");
            options.Add("Enumerate(Surveillance)");
            options.Add("Enumerate(Transport)");

            int index = Random.Range(0, options.Count - 1);
            HackObject hack = MapManager.inst.hackDatabase.dict[options[index]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

            avaiableCommands.Add(newCommand);

            // Then do another one
            options.RemoveAt(index);
            index = Random.Range(0, options.Count - 1);
            hack = MapManager.inst.hackDatabase.dict[options[index]];

            displayText = hack.trueName;

            newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Enumerate, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Index [104-110]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Index(Fabricators)");
            options.Add("Index(Garrisons)");
            options.Add("Index(Machines)");
            options.Add("Index(Recycling Units)");
            options.Add("Index(Repair Stations)");
            options.Add("Index(Scanalyzers)");
            options.Add("Index(Terminals)");

            int rand = Random.Range(0, options.Count - 1);
            HackObject hack = MapManager.inst.hackDatabase.dict[options[rand]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Index, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Layout (Zone)
        if (Random.Range(0f, 1f) >= 0.6f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            HackObject hack = MapManager.inst.hackDatabase.dict["Layout(Zone)"];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Layout, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // Recall
        if (GameManager.inst.activeAssaults.Count > 0 || GameManager.inst.activeExterminations.Count > 0 || GameManager.inst.activeInvestigations.Count > 0 || GameManager.inst.activeReinforcements.Count > 0)
        {
            if ((Random.Range(0f, 1f) >= 0.4f))
            {
                string letter = alphabet[0].ToString().ToLower();
                alphabet.Remove(alphabet[0]);

                HackObject hack = null;

                if (GameManager.inst.activeAssaults.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Assaults)"];
                }
                else if (GameManager.inst.activeExterminations.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Extermination)"];
                }
                else if (GameManager.inst.activeInvestigations.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Investigation)"];
                }
                else if (GameManager.inst.activeReinforcements.Count > 0)
                {
                    hack = MapManager.inst.hackDatabase.dict["Recall(Reinforcements)"];
                }

                string displayText = hack.trueName;

                TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Recall, "", hack, null);

                avaiableCommands.Add(newCommand);
            }
        }

        // Schematic (Item) [129-145]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 2) // temp fix
            {
                tier = 3;
            }

            bool star = false;
            if (Random.Range(0f, 1f) > 0.65f) // Chance to get a "starred" item
            {
                star = true;
            }
            string p = "";
            if (star)
            {
                p = "P";
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Schematic([Part Name]) - Rating {tier}{p}"];

            ItemObject item = HF.FindItemOfTier(tier);

            string displayText = item.itemName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, null, item);

            avaiableCommands.Add(newCommand);
        }

        // Schematic (Bot) [120-128]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            // Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }

            HackObject hack = MapManager.inst.hackDatabase.dict[$"Schematic([Bot Name]) - Tier {tier}"];

            BotObject bot = HF.FindBotOfTier(tier);

            string displayText = bot.botName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "Schematic", hack, null, null, bot);

            avaiableCommands.Add(newCommand);
        }
        // Traps [146-148]
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            string letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            List<string> options = new List<string>();
            options.Add("Traps(Disarm)");
            options.Add("Traps(Locate)");
            options.Add("Traps(Reprogram)");

            HackObject hack = MapManager.inst.hackDatabase.dict[options[Random.Range(0, options.Count - 1)]];

            string displayText = hack.trueName;

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Schematic, "", hack, null);

            avaiableCommands.Add(newCommand);
        }

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void UseCustomCode(TerminalCustomCode code)
    {
        // TODO: idk how this is gonna get parsed
        // Create log messages
        string message = "";
        UIManager.inst.CreateNewLogMessage(message, UIManager.inst.highlightGreen, UIManager.inst.dullGreen, false, true);
    }
    #endregion

    #region Custom Terminal
    [Header("Custom Terminal Variables")]
    public CustomTerminalType customType;
    public List<int> cterminal_prototypes;
    [Header("-- Door Control")]
    [Tooltip("Coordinates to the wall(s) that will dissapear if this *door* is opened.")]
    public List<Vector2Int> wallRevealCoordinates;
    public AudioClip customDoorRevealSound;
    public int cacheStoredMatter;
    public void CustomTerminalInit()
    {
        /*
         // Whatever this is?
         if(obj.GetComponentInChildren<TerminalCustom>().customType == CustomTerminalType.DoorLock)
            {
                foreach (Vector2Int loc in obj.GetComponentInChildren<TerminalCustom>().wallRevealCoordinates)
                {
                    if (MapManager.inst._allTilesRealized.ContainsKey(loc))
                    {
                        obj.GetComponentInChildren<TerminalCustom>().linkedDoors.Add(MapManager.inst._allTilesRealized[loc].bottom);
                    }
                }
            }
         */

        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.CustomTerminal;

        switch (customType)
        {
            case CustomTerminalType.Shop:
                break;
            case CustomTerminalType.WarlordCamp:
                break;
            case CustomTerminalType.LoreEntry:
                break;
            case CustomTerminalType.DoorLock:
                break;
            case CustomTerminalType.PrototypeData:
                break;
            case CustomTerminalType.HideoutCache:
                displayName = "Hideout Cache";
                CTerminal_SetupAsCache();
                break;
            case CustomTerminalType.Misc:
                break;
            default:
                break;
        }

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void CTerminal_OpenScriptedDoor()
    {
        // Essentially we want to:
        // - Play the door open "sliding" sound
        //_doorSource.Play();
        // - Replace the walls with floor tiles (that have rubble texture).
        //foreach (TileBlock W in linkedDoors)
        //{
            // TODO
        //}
    }

    #region Hideout Cache
    public int cache_storedMatter;
    public void CTerminal_SetupAsCache()
    {
        string specialName = "Hideout Cache (Local)";

        // Setup component inventory
        if (InventoryControl.inst.hideout_inventory == null)
        {
            InventoryControl.inst.hideout_inventory = new InventoryObject(25, specialName + "'s component Inventory");
        }

        #region Add Commands
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We need to populate this machine with the following commands:
        // -Retrieve (Matter)
        // -Submit (Matter)

        // [Retrieve (Matter)]
        string letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        HackObject hack = MapManager.inst.hackDatabase.Hack[35];

        TerminalCommand newCommand = new TerminalCommand(letter, "Retrieve(Matter)", TerminalCommandType.Retrieve, "", hack);

        avaiableCommands.Add(newCommand);

        // [Submit (Matter)]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[186];

        newCommand = new TerminalCommand(letter, "Store(Matter)", TerminalCommandType.Submit, "", hack);

        avaiableCommands.Add(newCommand);
        #endregion

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    #endregion

    #endregion

    #region Fabricator
    [Tooltip("This machine is currently inoperable and is randomly send high corruption arcs of electromagnetic energy at any nearby bots.")]
    public bool fabricator_flag_overload;

    // TODO FUTURE WORK: AUTHCHIPS
    // https://www.gridsagegames.com/blog/2021/11/design-overhaul-4-fabrication-2-0/

    public void FabricatorInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Fabricator;

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We need to populate this machine with the following commands:
        // -Network Status
        // -Load Schematic
        // -A pre-loaded build command for a bot/item appropriate to the level + 1 (this is random, it can be empty sometimes)

        // [Network Status]
        string letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        HackObject hack = MapManager.inst.hackDatabase.Hack[27];

        TerminalCommand newCommand = new TerminalCommand(letter, "Network Status", TerminalCommandType.Network, "", hack);

        avaiableCommands.Add(newCommand);

        // [Load Schematic]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[185];

        newCommand = new TerminalCommand(letter, "Load Schematic", TerminalCommandType.LoadIndirect, "", hack);

        avaiableCommands.Add(newCommand);

        // Preload
        if (Random.Range(0f, 1f) > 0.5f) // 50/50
        {
            letter = alphabet[0].ToString().ToLower();
            alphabet.Remove(alphabet[0]);

            string displayText = "Build ";

            // Pick what to show
            // -Current level goes from -10 to -1. But we want to scale from tier 1 to 10, so we just add 11
            int tier = MapManager.inst.currentLevel + 11;
            if (tier <= 0)
            {
                tier = 1;
            }

            if (tier < 10)
            {
                tier++; // +1 for better rewards
            }

            BotObject bot = null;
            ItemObject item = null;

            if (Random.Range(0f, 1f) > 0.7f) // 30% to be a Bot
            {
                bot = HF.FindBotOfTier(tier);

                hack = MapManager.inst.hackDatabase.Hack[16 + tier]; // bot commands actually start at 17 but the lowest tier can be is 1 so 16 + 1 = 17.

                displayText += bot.botName;

                newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, bot);

                
                if(secLvl == 1)
                {
                    timeToComplete = bot.fabricationInfo.fabTime.x;
                }
                else if(secLvl == 2)
                {
                    timeToComplete = bot.fabricationInfo.fabTime.y;
                }
                else if( secLvl == 3)
                {
                    timeToComplete = bot.fabricationInfo.fabTime.z;
                }
                
            }
            else // 70% chance to be an item
            {
                item = HF.FindItemOfTier(tier, false);

                displayText += item.itemName;

                hack = HF.HackBuildParser(tier, item.star);

                newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, null, item);

                
                if (secLvl == 1)
                {
                    timeToComplete = item.fabricationInfo.fabTime.x;
                }
                else if (secLvl == 2)
                {
                    timeToComplete = item.fabricationInfo.fabTime.y;
                }
                else if (secLvl == 3)
                {
                    timeToComplete = item.fabricationInfo.fabTime.z;
                }
                
            }

            avaiableCommands.Add(newCommand);
        }
        else // We need to show the "No Schematic Loaded" false command
        {
            hack = MapManager.inst.hackDatabase.Hack[26];

            newCommand = new TerminalCommand(letter, "No Schematic Loaded", TerminalCommandType.NONE, "", hack);

            avaiableCommands.Add(newCommand);
        }

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    #region Operation
    public void Fabricator_AddBuildCommand(ItemObject item = null, BotObject bot = null)
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        
        string letter = HF.GetNextLetter(avaiableCommands[avaiableCommands.Count - 1].assignedChar);
        string displayText = "Build ";

        // Remove the old preloaded build option if its there
        foreach (var command in avaiableCommands.ToList())
        {
            if (command.subType == TerminalCommandType.Build)
            {
                avaiableCommands.Remove(command);
            }
        }

        if (item != null)
        {
            displayText += item.itemName;

            HackObject hack = HF.HackBuildParser(item.rating, item.star);

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, null, item);

            avaiableCommands.Add(newCommand);
        }
        else if(bot != null)
        {
            displayText += bot.botName;

            HackObject hack = MapManager.inst.hackDatabase.Hack[16 + bot.rating];

            TerminalCommand newCommand = new TerminalCommand(letter, displayText, TerminalCommandType.Build, "", hack, null, null, bot);

            avaiableCommands.Add(newCommand);
        }

        // Remove the [No Schematic Loaded] option if its there
        foreach (var command in avaiableCommands.ToList())
        {
            if(command.subType == TerminalCommandType.NONE)
            {
                avaiableCommands.Remove(command);
            }
        }

        // And refresh the options
        UIManager.inst.Terminal_RefreshHackingOptions();

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Fabricator_Load(int time, ItemObject item = null, BotObject bot = null)
    {
        if (item != null)
        {
            desiredPart = item;
        }
        else if (bot != null)
        {
            desiredBot = bot;
        }

        timeToComplete = time;

        Fabricator_AddBuildCommand(item, bot);

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Fabricator_Build()
    {
        
        // Set values
        begunBuildTime = TurnManager.inst.globalTime;
        atWork = true;

        // Create physical timer
        GameObject timerObject = GameObject.Instantiate(UIManager.inst.prefab_machineTimer, new Vector2(location.x, location.y), Quaternion.identity);
        // Assign Details
        timerObject.GetComponent<UITimerMachine>().Init(timeToComplete);

        // Save the object
        MapManager.inst.machine_timers.Add(location, timerObject);

        // Remove old build command
        foreach (var command in avaiableCommands.ToList())
        {
            if (command.subType == TerminalCommandType.Build)
            {
                avaiableCommands.Remove(command);
            }
        }

        // Add placeholder (empty) command
        HackObject hack = MapManager.inst.hackDatabase.Hack[26];

        TerminalCommand newCommand = new TerminalCommand(HF.GetNextLetter(avaiableCommands[avaiableCommands.Count - 1].assignedChar), "No Schematic Loaded", TerminalCommandType.NONE, "", hack);

        avaiableCommands.Add(newCommand);

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Fabricator_FinishBuild()
    {
        atWork = false;

        AudioManager.inst.CreateTempClip(new Vector3(location.x, location.y), AudioManager.inst.dict_game["FABRICATION"], 1f); // GAME - FABRICATION

        if (desiredPart != null)
        {
            Vector2Int dropLocation = HF.LocateFreeSpace(dropSpot, false, true);

            // Spawn in this part on the floor
            InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo(desiredPart.itemName, dropLocation, 1, true));
        }
        else if(desiredBot != null)
        {
            Vector2Int dropLocation = HF.LocateFreeSpace(dropSpot, true, false);

            // Spawn in a new ALLIED bot at this location
            Actor newBot = MapManager.inst.PlaceBot(dropLocation, desiredBot);
            newBot.directPlayerAlly = true;
            newBot.wasFabricated = true;

            // Modify relations to be friendly to the player and neutral to some other functions
            List<BotRelation> relationList = new List<BotRelation>();

            relationList.Add(BotRelation.Hostile); // Complex
            relationList.Add(BotRelation.Neutral); // Derelict
            relationList.Add(BotRelation.Hostile); // Assembled
            relationList.Add(BotRelation.Neutral); // Warlord
            relationList.Add(BotRelation.Neutral); // Zion
            relationList.Add(BotRelation.Neutral); // Exiles
            relationList.Add(BotRelation.Hostile); // Architect
            relationList.Add(BotRelation.Neutral); // Subcaves
            relationList.Add(BotRelation.Hostile); // Subcaves Hostile
            relationList.Add(BotRelation.Friendly); // Player
            relationList.Add(BotRelation.Neutral); // None

            HF.ModifyBotAllegance(newBot, relationList, BotAlignment.Player);
        }

        PlayerData.inst.GetComponent<Actor>().UpdateFieldOfView();

        GameObject.Destroy(MapManager.inst.machine_timers[location]);
        MapManager.inst.machine_timers.Remove(location);

        desiredPart = null;
        desiredBot = null;
        timeToComplete = 0;
        begunBuildTime = 0;

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }
    #endregion

    #region Hacks
    public void Force()
    {
        locked = true;

        if(desiredBot != null || desiredPart != null)
        {
            Fabricator_Build();
        }

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
        // (UPDATE VIS) Since we need to change this machine's color
        MapManager.inst.UpdateTilemap();
        MapManager.inst.TilemapVisUpdate();
    }

    #endregion
    #endregion

    #region Garrison
    [Header("Garrison")]
    [Tooltip("This garrison is permanently closed.")]
    public bool garrison_sealed;
    [Tooltip("The player can now ENTER the garrison, there is a specific EXIT placed in the world.")]
    public bool garrison_doorIsRevealed;
    //
    [Tooltip("[FALSEBYDEFAULT] This Garrison Access is communicating with additional reinforcements preparing for dispatch. Using a Signal Interpreter provides the precise number of turns remaining until the next dispatch.")]
    public bool garrison_flag_transmitting;
    [Tooltip("[FALSEBYDEFAULT] ???")]
    public bool garrison_flag_redeploying;
    //
    [Tooltip("List of item IDs referring to couplers that this machine will spawn when requested to via the command, and how many of each type there are.")]
    public List<(int, int)> garrison_couplerIDs;
    public void GarrisonInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Garrison;
        garrison_couplerIDs = new List<(int, int)>();

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        List<char> alphabet = alpha.ToList(); // Fill alphabet list

        // We need to load this machine with the following commands:
        // - Couplers
        // - Seal
        // - Unlock

        // [Couplers]
        string letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        HackObject hack = MapManager.inst.hackDatabase.Hack[28];

        TerminalCommand newCommand = new TerminalCommand(letter, "Couplers", TerminalCommandType.Couplers, "", hack);

        avaiableCommands.Add(newCommand);

        // While we're here, fill the garrison with a list of 3-5 random couplers
        for (int i = 0; i < Random.Range(3,5); i++)
        {
            // TODO: When all coupler items are added, update this range
            garrison_couplerIDs.Add((Random.Range(0, 5), Random.Range(1, 3)));
        }

        // [Seal]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[29];

        newCommand = new TerminalCommand(letter, "Seal", TerminalCommandType.Seal, "", hack);

        avaiableCommands.Add(newCommand);

        // [Unlock]
        letter = alphabet[0].ToString().ToLower();
        alphabet.Remove(alphabet[0]);

        hack = MapManager.inst.hackDatabase.Hack[30];

        newCommand = new TerminalCommand(letter, "Unlock", TerminalCommandType.Unlock, "", hack);

        avaiableCommands.Add(newCommand);

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Garrison_Open()
    {
        garrison_doorIsRevealed = true;

        // TODO
        // Not sure if anything else happens here?

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Garrison_Seal()
    {
        garrison_sealed = true;

        locked = true;

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
        // (UPDATE VIS) Since we need to change this machine's color
        MapManager.inst.UpdateTilemap();
        MapManager.inst.TilemapVisUpdate();
    }

    public void Garrison_CouplerStatus()
    {

    }

    #region Hacks
    public void ForceEject()
    {

    }

    public void ForceJam()
    {

    }

    public void TrojanBroadcast()
    {

    }

    public void TrojanDecay()
    {

    }

    public void TrojanIntercept()
    {

    }

    public void TrojanRedirect()
    {

    }

    public void TrojanReprogram()
    {

    }

    public void TrojanRestock()
    {

    }

    public void TrojanWatcher()
    {

    }
    #endregion
    #endregion

    #region Recycling Units
    [Header("Recycling Unit")]
    public int recycling_storedMatter;
    public InventoryObject recycling_storedComponents; // !! This may be bad to do since its a struct and considering the way we update this. !!

    public void RecyclingInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Recycling;

        // Setup component inventory
        recycling_storedComponents = new InventoryObject(10, displayName + "'s component Inventory");

        // We need to load this machine with the following commands:
        // TODO

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Recycling_Check()
    {

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    // If the stored matter goes above 500 it is reset. If components stored goes above 10 that inventory is emptied.
    private void Recycling_OverflowCheck()
    {
        if (recycling_storedMatter > 500)
        {
            recycling_storedMatter = 0;

            // (UPDATE MAPDATA)
            MapManager.inst.mapdata[location.x, location.y].machinedata = this;
        }
        if (recycling_storedComponents.Container.Items.Length > 10)
        {
            // Reset the inventory but keep the most recently added item
            Item final = recycling_storedComponents.Container.Items[0].item;

            recycling_storedComponents.Container.Clear();
            recycling_storedComponents.AddItem(final);

            // (UPDATE MAPDATA)
            MapManager.inst.mapdata[location.x, location.y].machinedata = this;
        }
    }
    #endregion

    #region Repair Station
    [Header("Repair Station")]
    public Item repair_desiredPart;

    // https://www.gridsagegames.com/blog/2014/01/recycling-units-repair-stations/

    public void RepairBayInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.RepairStation;

        // We need to load this machine with the following commands:
        // TODO

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Repair_Scan(Item item, int time)
    {
        repair_desiredPart = item;
        timeToComplete = time;

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    // TODO: Repair stations cannot repair faulty prototypes or deteriorating parts
    public void Repair_Repair(Item item)
    {
        repair_desiredPart = item;


        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Repair_Check()
    {


        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }
    #endregion

    #region Scanalyzer
    public void ScanalyzerInit()
    {
        detectionChance = GlobalSettings.inst.defaultHackingDetectionChance;
        type = MachineType.Scanalyzer;

        // We need to load this machine with the following commands:
        // TODO

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    // NOTE: Higher level scanalyzers are required to scan prototypes and more advanced parts, and scanalyzers will reject broken or faulty parts.
    public void Scanalyzer_Check()
    {
        // TODO

        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }
    #endregion

    #region Static
    [Header("Static")]
    public bool static_flag_detonate;
    public bool static_flag_unstable;
    public int static_timeToDetonation;

    public void Static_Init()
    {


        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }

    public void Static_Detonate()
    {
        // TODO
    }
    #endregion

    #region Door Terminal
    public void DoorTerminal_Init()
    {


        // (UPDATE MAPDATA)
        MapManager.inst.mapdata[location.x, location.y].machinedata = this;
    }
    #endregion
}

public enum CustomTerminalType
{
    Shop,
    WarlordCamp,
    LoreEntry,
    DoorLock,
    PrototypeData,
    HideoutCache,
    Misc
}

[System.Serializable]
public class TerminalCommand
{
    public string assignedChar;
    public string command;
    public TerminalCommandType subType; //
    [Tooltip("Record, Analysis, etc (The stuff in darker text)")]
    public string secondaryText;
    [Tooltip("Has this been used yet? True = no, False = yes")]
    public bool available = true;
    [Tooltip("Can this command be used multiple times? (Re-open terminal to see it again) Normally false, a few commands can be done again.")]
    public bool repeatable = false;

    [Header("Related Hack")]
    public HackObject hack;

    [Header("Reward")]
    public KnowledgeObject knowledge;
    public GlobalActions specialAction;
    public BotObject bot;       // Bot
    public ItemObject item;     // Item (Prototype)

    /// <summary>
    /// Create a new terminal command.
    /// </summary>
    /// <param name="letter">The letter that will represent this command (to the side), and the keybind that can automatically attempt it.</param>
    /// <param name="displayText">The display text of this command (what text you see in the commands list).</param>
    /// <param name="type">The command type (see list).</param>
    /// <param name="subText">Dark green text that will appear before the command's text (ex. Schematic, Record, etc.)</param>
    /// <param name="hackN">The assigned HackObject for this command. MUST BE ASSIGNED!!!</param>
    /// <param name="knowN">A knowledge reward object for this command. Optional.</param>
    /// <param name="action">A global action that happens if this hack succeeds. Optional.</param>
    /// <param name="botNew">An assigned bot object to this command. Optional.</param>
    /// <param name="itemNew">An assigned item object to this command. Optional.</param>
    public TerminalCommand(string letter, string displayText, TerminalCommandType type, string subText = "", HackObject hackN = null, KnowledgeObject knowN = null, GlobalActions action = null, BotObject botNew = null, ItemObject itemNew = null)
    {
        assignedChar = letter;
        command = displayText;
        subType = type;
        secondaryText = subText;
        hack = hackN;
        knowledge = knowN;
        specialAction = action;
        bot = botNew;
        item = itemNew;
    }
}

[System.Serializable]
public enum TerminalCommandType
{
    // - Terminal
    Access,
    Alert,
    Enumerate,
    Index,
    Inventory,
    Recall,
    Traps,
    Manifests,
    Open, // ---- Terminal (Door)
    Prototypes,
    Query,
    Layout,
    Analysis,
    Schematic,
    Download,
    // - Garrison
    Couplers,
    Seal,
    Unlock,
    // - Fabricator
    LoadIndirect,
    Load,
    Build,
    Network,
    // - Repair Station
    Refit,
    Repair,
    Scan,
    // - Recycling
    Retrieve,
    Recycle,
    // - Scanalyzer
    Insert,
    Scanalyze,
    // - Hacking https://gridsagegames.com/wiki/Unauthorized_Hacking
    Trojan,
    Force,
    // - Hideout Storage
    Submit,
    NONE
}

[System.Serializable]
public class TerminalZone
{
    [Header("Assigned Terminal")]
    public Vector2Int assignedTerminal;

    [Header("Content")]
    [Tooltip("A collection of points in a map that are assigned to this zone.")]
    public List<Vector2Int> assignedArea = new List<Vector2Int>();
    public List<WorldTile> trapList = new List<WorldTile>();
    public List<WorldTile> emergencyAccessDoors = new List<WorldTile>();

    public void RevealArea()
    {
        Debug.Log("Revealing area of terminal.");
        foreach (Vector2Int T in assignedArea)
        {
            WorldTile tile = MapManager.inst.mapdata[T.x, T.y];
            // Set all the tiles as explored and if not in direct LOS make them green
            // - Don't do cave walls
            if (!tile.tileInfo.isCaveWall)
            {
                // play an animation (once terminal is closed)
                //tile.RevealViaZone(); // TODO: Rework this (old code in TileObject.s)
            }

            // TODO

            // Set all the tiles as explored and if not in direct LOS make them green

            // - If there are any exits reveal them
        }
    }

    public void RevealLocalEAccess()
    {

    }

    public void RevealTraps()
    {

    }
}

[System.Serializable]
public class TerminalCustomCode
{
    // These are custom codes given to the player by NPCs or data files that they can use in specific instances while hacking.
    // If doing a manual hack on something, if the player knows one or more codes, a custom window should appear.

    [Tooltip("In the form of \\???? (Ex. \\6RCT)")]
    public string code;
    [Tooltip("Where/who this code came from (usually a name). (Ex. EX-BIN)")]
    public string source;
    [Tooltip("Where this code can be used at. (Ex. EX-Vault Control)")]
    public string target;

    public TerminalCustomCode(string nSource, string nTarget)
    {
        code = GenerateRandomString();
        source = nSource;
        target = nTarget;
    }

    /// <summary>
    /// For custom pre-existing codes (Exiles, warlord, etc...)
    /// </summary>
    /// <param name="id">The unique ID to target.</param>
    public TerminalCustomCode(int id)
    {
        switch (id)
        {
            case 0: // Exiles Vault Control
                code = GenerateRandomString();
                source = "EX-BIN";
                target = "EX-Vault Control";
                break;
            case 1:

                break;

            default:

                break;
        }
    }

    private const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string GenerateRandomString()
    {
        string randomString = "";

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, characters.Length);
            randomString += characters[randomIndex];
        }

        return randomString;
    }
}