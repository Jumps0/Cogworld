/*
 * Originally Created by: TaroDev
 * Youtube Link: https://www.youtube.com/watch?v=kkAjpQAM-jE
 * 
 * 
 */

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileBlock : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] public GameObject _highlight;
    public GameObject _highlightPerm; // Used for map border mostly
    public GameObject _debrisSprite;
    [SerializeField] private GameObject _collapseSprite;
    [SerializeField] private Animator _collapseAnim;
    public bool isDirty = false;

    [Tooltip("What this tile would look in DOS mode.")]
    public GameObject dosLetter;

    [Tooltip("Alternate state for this tile.")]
    public GameObject subTile;

    [Tooltip("Should this tile's highlight be blinking?")]
    public bool blinkMode = false;

    [Tooltip("Can this tile be walked on?")]
    public bool walkable;

    [Tooltip("Is something currently occupying this space?")]
    public bool occupied = false;

    [Tooltip("The current item laying on top of this Tile, if none then = null.")]
    public Part _partOnTop = null;

    [Tooltip("The specific details of what this tile is, set upon spawning.")]
    public TileObject tileInfo;

    [Tooltip("If true, this tile won't block LOS like normal. Used for open doors.")]
    public bool specialNoBlockVis = false;

    public GameObject targetingHighlight;

    public int locX;
    public int locY;

    public bool isExplored;
    public bool isVisible;

    // -- Colors --
    public Color intel_green;
    public Color unstableCollapse_red;
    public Color trojan_Blue;
    public Color highlight_white;
    public Color caution_yellow;

    public Animator flashWhite;

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

    private void Update()
    {
        if (_renderer == null)
        {
            return;
        }

        vis = tileInfo.currentVis;
        HighlightCheck();
        CheckVisibility();

        /*
        if (this.GetComponent<PotentialField>() && isExplored)
        {
            this.GetComponent<PotentialField>().active = true;
        }
        */
    }

    #region Vision/Display

    /// <summary>
    /// Should the white highlight animation be played?
    /// </summary>
    private void HighlightCheck()
    {
        if (_highlightPerm.activeInHierarchy) // Don't highlight when it's permanently on.
        {
            return;
        }

        if (_highlight.activeInHierarchy && // If the highlight is on
            (_highlight.GetComponent<SpriteRenderer>().color.r == highlight_white.r) && // R
            (_highlight.GetComponent<SpriteRenderer>().color.g == highlight_white.g) && // G
            (_highlight.GetComponent<SpriteRenderer>().color.b == highlight_white.b)    // B
            ) // and the RGB (white) matches with the highlight color
        {
            flashWhite.enabled = true;
            flashWhite.Play("FlashWhite");
        }
        else
        {
            flashWhite.enabled = false;
        }
    }

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
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[0], 0.5f);
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
    public void DamageMe()
    {
        // Change the sprite
        this.GetComponent<SpriteRenderer>().sprite = tileInfo.altSprite;

        // Activate the "danger roof will collapse" red indicator (if needed)
        if (tileInfo.type == TileType.Wall)
        {
            _collapseSprite.SetActive(true);
            _collapseAnim.enabled = true;
            _collapseAnim.Play("");
        }

        // Change walkablility if needed
        if(tileInfo.type == TileType.Wall || tileInfo.type == TileType.Machine)
        {
            walkable = true;
        }

        // Play a sound
        AudioManager.inst.CreateTempClip(this.transform.position, tileInfo.destructionClips[Random.Range(0, tileInfo.destructionClips.Count - 1)]);

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

        // De-active the "danger roof will collapse" red indicator (if its active)
        if (_collapseSprite.activeInHierarchy)
        {
            _collapseSprite.SetActive(false);
            _collapseAnim.enabled = false;
            walkable = false;
        }

        // Remove from MapManager list
        MapManager.inst.damagedStructures.Remove(this.gameObject);

    }

    #endregion

    public Actor GetBotOnTop()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.zero), 10f);

        if (hit)
        {
            return hit.collider.gameObject.GetComponent<Actor>(); // Success
        }

        return null; // Failure
    }
}