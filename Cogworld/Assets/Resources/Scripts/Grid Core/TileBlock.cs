/*
 * Originally Created by: TaroDev
 * Expanded by: Cody Jackson
 * Youtube Link: https://www.youtube.com/watch?v=kkAjpQAM-jE
 * 
 * 
 */

using System.Collections;
using UnityEngine;

/// <summary>
/// A script used for the physical *real world* tiles used to build the world. What this tile is gets determined by its "tileInfo" (a TileObject variable).
/// </summary>
public class TileBlock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
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
    public int locX;
    public int locY;

    [Header("Visibility")]
    public bool isExplored;
    public bool isVisible;
    [Tooltip("If true, this tile won't block LOS like normal. Used for open doors.")]
    public bool specialNoBlockVis = false;

    [Header("Colors")]
    public Color intel_green;
    [SerializeField] private Color unstableCollapse_red;
    [SerializeField] private Color trojan_Blue;
    [SerializeField] private Color highlight_white;
    [SerializeField] private Color caution_yellow;

    [Header("Phase Wall")]
    [Tooltip("Is this tile a phase wall?")]
    public bool phaseWall = false;
    [Tooltip("Which team can *use* this phase wall?")]
    public BotAlignment phaseWallTeam = BotAlignment.Complex;
    public Sprite phaseWallSprite;
    [Tooltip("Does the player know this tile is actually a phase wall?")]
    public bool phaseWall_revealed = false;

    public void Init(bool isOffset)
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void Start()
    {
        StartCheck();
    }

    public void StartCheck()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.sprite = tileInfo.displaySprite; // Set the sprite
        if (tileInfo.type == TileType.Wall || tileInfo.type == TileType.Machine) // Walls/Machines take up space
        {
            occupied = true;
            this.gameObject.layer = LayerMask.NameToLayer("BlockVision");

            /*
            if (tileInfo.Id == 1)
            {
                this.AddComponent<PotentialField>();
                this.AddComponent<EntValues>();
                this.GetComponent<PotentialField>().repulsive = true;
                this.GetComponent<PotentialField>().Rconstant = 125;
                this.GetComponent<PotentialField>().ent = this.GetComponent<EntValues>();
            }
            */

        }
        else if (tileInfo.type == TileType.Floor)
        {
            occupied = false; // This is a risky assumption!
            this.gameObject.layer = LayerMask.NameToLayer("Floor");
        }
        //this.tileInfo.currentVis = TileVisibility.Visible;
        //isDirty = tileInfo.isDirty; // Set dirt flag
        SetHighlightPerma(this.tileInfo.impassable); // Set perma highlight
    }

    public TileVisibility vis;

    public void SetVisibility(TileVisibility newVis)
    {
        vis = newVis;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Right Click to open /DATA/ Menu
        {
            if (UIManager.inst.dataMenu.data_parent.activeInHierarchy) // For tile objects, we only want to show info if the menu is already open so the player doesn't do it on accident in combat.
            {
                UIManager.inst.Data_OpenMenu(null, this.gameObject);
            }
        }
    }

    private void Update()
    {
        if (_renderer == null)
        {
            return;
        }

        vis = tileInfo.currentVis;
        CheckVisibility(); // This too this is also pretty bad. Finding a good solution for this may be difficult.
        // See https://www.youtube.com/watch?v=XNcEZHqtC0g for the many ways to finding problems when optimizing in Unity

        /*
        if (this.GetComponent<PotentialField>() && isExplored)
        {
            this.GetComponent<PotentialField>().active = true;
        }
        */
    }

    #region Vision/Display

    public void CheckVisibility()
    {
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
            this.GetComponent<SpriteRenderer>().color = Color.white;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = Color.white;
            }
            recentlyRevealedViaIntel = false;
        }
        else if (isExplored && isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = Color.white;
            }
            recentlyRevealedViaIntel = false;
        }
        else if (isExplored && !isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = Color.gray;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = Color.gray;
            }
            if (this.GetComponent<Animator>().enabled) // Stop animating!
            {
                this.GetComponent<Animator>().enabled = false;
            }
        }
        else if (!isExplored)
        {
            this.GetComponent<SpriteRenderer>().color = Color.black;
            if (isDirty)
            {
                _debrisSprite.GetComponent<SpriteRenderer>().color = Color.black;
            }
            if (this.GetComponent<Animator>().enabled) // Stop animating!
            {
                this.GetComponent<Animator>().enabled = false;
            }
        }
    }

    public IEnumerator RevealAnim()
    {
        /*
         *  NOTE: The animator being enabled breaks the lighting (Fog of War).
         *  So we only want it to be on when needed.
         */
        
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play("TileRevealGreen");

        yield return new WaitForSeconds(0.1f);

        if(this != null) // contingency
        {
            this.GetComponent<Animator>().enabled = false;
        }
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
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0], 0.5f); // UI - ACCESS
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
        _debrisSprite.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.debrisSprites[Random.Range(0, MiscSpriteStorage.inst.debrisSprites.Count)];
    }

    public void CleanSpriteDebris()
    {
        this.isDirty = false;
        this._debrisSprite.SetActive(false);
        _debrisSprite.GetComponent<SpriteRenderer>().sprite = null;
    }

    public void SetHighlightPerma(bool state)
    {
        Color halfColor = new Color((MapManager.inst.currentTheme.r / 2) / 255f, (MapManager.inst.currentTheme.g / 2) / 255f, (MapManager.inst.currentTheme.b / 2) / 255f);
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
        // Change the sprite
        this.GetComponent<SpriteRenderer>().sprite = tileInfo.destroyedSprite;

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
        // Change the sprite
        this.GetComponent<SpriteRenderer>().sprite = tileInfo.displaySprite;

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

    public Actor GetBotOnTop()
    {
        foreach (var E in GameManager.inst.entities) // A bit more performance heavy but raycasting wasn't working
        {
            if (HF.V3_to_V2I(E.transform.position) == new Vector2Int(locX, locY))
            {
                return E.GetComponent<Actor>();
            }
        }

        return null; // Failure
    }

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
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.DOOR_Clips[Random.Range(8, 9)]); // HEAVY_OPEN_1/2

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
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.DOOR_Clips[Random.Range(10, 12)]); // PHASE_CLOSE_1/2/3

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
}