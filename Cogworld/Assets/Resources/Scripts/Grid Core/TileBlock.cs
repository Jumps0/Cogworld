// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using UnityEngine;

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
        while (UIManager.inst.terminal_targetTerm)
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
    public TerminalZone zone;

    [Header("States")]
    [Tooltip("Where -1 = Not dirty, and any other number indicates the ID of the debris sprite.")]
    public int isDirty;
    public bool isImpassible;
    [Tooltip("Is this tile currently damaged? Default is FALSE.")]
    public bool isDamaged; // Important for machines too!

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

    public void MachineInit()
    {
        MachineSetName();

        // Color setup
        #region Colors
        machinedata.activeColor = tileInfo.asciiColor;
        machinedata.disabledColor = Color.gray;

        // And save this info
        MapManager.inst.mapdata[location.x, location.y].machinedata.activeColor = machinedata.activeColor;
        MapManager.inst.mapdata[location.x, location.y].machinedata.disabledColor = machinedata.disabledColor;
        #endregion
    }

    private void MachineSetName()
    {
        switch (machinedata.type)
        {
            case MachineType.Fabricator:
                machinedata.displayName = "Fabricator";
                break;
            case MachineType.Garrison:
                machinedata.displayName = "Garrison";
                break;
            case MachineType.Recycling:
                machinedata.displayName = "Recycling Unit";
                break;
            case MachineType.RepairStation:
                machinedata.displayName = "Repair Station";
                break;
            case MachineType.Scanalyzer:
                machinedata.displayName = "Scanalyzer";
                break;
            case MachineType.Terminal:
                machinedata.displayName = "Terminal";
                break;
            case MachineType.CustomTerminal:
                // TODO
                // More complicated
                break;
            case MachineType.DoorTerminal:
                // TODO
                // More complicated
                break;
            case MachineType.Static:
                // TODO
                // More complicated
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

        // And save this name
        MapManager.inst.mapdata[location.x, location.y].machinedata.displayName = machinedata.displayName;
    }
    #endregion
}

/// <summary>
/// Will eventually replace the `MachinePart` script and store information about a machine at this position inside a WorldTile struct.
/// </summary>
public struct MachineData
{
    [Header("Basic Info")]
    public string displayName;
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
    [Tooltip("Is this part the PARENT or PRIMARY interactable for all the other parts that make up this machine")]
    public bool isParent;
    [Tooltip("If this part is the parent, will contain all other children that make up this machine.")]
    public Vector2Int[] children;
    [Tooltip("Reference to the UI indicator showing where this machine is IF it is currently not on the screen.")]
    public GameObject indicator;

    [Header("Colors")]
    public Color activeColor;
    public Color disabledColor;

    // TODO: !! All the other stuff needed for the individual interactable machines !!

    // Future note:
    // -How to handle Machine audio: https://www.gridsagegames.com/blog/2020/06/building-cogminds-ambient-soundscape/

    public WorldTile GetParent(Vector2Int loc)
    {
        if (MapManager.inst.mapdata[loc.x, loc.y].machinedata.isParent)
        {
            return MapManager.inst.mapdata[loc.x, loc.y];
        }
        else
        {
            foreach (Vector2Int MD in MapManager.inst.mapdata[loc.x, loc.y].machinedata.children)
            {
                if(MapManager.inst.mapdata[MD.x, MD.y].machinedata.isParent)
                {
                    return MapManager.inst.mapdata[MD.x, MD.y];
                }
            }
        }

        return default(WorldTile);
    }
}