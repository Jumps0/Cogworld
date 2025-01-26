using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages everything related to a floor trap.
/// NOTE FOR PREFABS:
/// The naming scheme for this prefab is: Trap-'TrapType'-'BotRelation'
/// Where: 
/// Trap is generic name we search for 
/// 'TrapType' is the (correctly named) enum we determine. Can be set to 'Any' to determine based on floor.
/// 'BotRelation' is the alignment of the trap. If friendly the player will be told where it is when its spotted.
/// </summary>
public class FloorTrap : MonoBehaviour
{
    [Header("Information")]
    public Vector2Int location;
    public string fullName;
    private Color setColor;
    [Tooltip("What faction this trap belongs to, will detonate vs all other hostile factions via player's tree.")]
    public BotAlignment alignment;
    public TerminalZone zone;

    public bool tripped = false;
    public bool active = true;
    public bool knowByPlayer = false;

    [Header("Assignments")]
    public TileBlock assignedTile;
    public SpriteRenderer _sprite;
    public ItemObject trapData;
    public TrapType type;

    [Header("Colors")]
    public Color C_alarm;
    public Color C_blade;
    public Color C_chute;
    public Color C_dirtyBomb;
    public Color C_EMP;
    public Color C_Fire;
    public Color C_HE;
    public Color C_Hellfire;
    public Color C_ProtonBomb;
    public Color C_Segregator;
    public Color C_Shock;
    public Color C_Stasis;
    public Color C_NONE;

    #region Visibility

    private void Update()
    {
        if (knowByPlayer)
        {
            UpdateVisibility();
        }
        else
        {
            _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 0f);
        }
    }

    private void UpdateVisibility()
    {
        if (assignedTile.isVisible)
        {
            if(active)
            {
                _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 1f);
            }
            else
            {
                _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 0.6f);
            }
        }
        else if (assignedTile.isExplored && assignedTile.isVisible)
        {
            if (active)
            {
                _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 1f);
            }
            else
            {
                _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 0.6f);
            }
        }
        else if (assignedTile.isExplored && !assignedTile.isVisible)
        {
            _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 0.6f);
        }
        else if (!assignedTile.isExplored)
        {
            _sprite.color = new Color(setColor.r, setColor.g, setColor.b, 0f);
        }
    }

    #endregion

    public void Setup(ItemObject data, Vector2Int loc, TileBlock tile, BotAlignment alignmentN)
    {
        trapData = data;
        location = loc;
        type = data.deployableItem.trapType;
        assignedTile = tile;
        alignment = alignmentN;

        // -- Each has a different color and name

        switch (type)
        {
            case TrapType.Alarm:
                fullName = "Alarm";
                setColor = C_alarm;
                break;
            case TrapType.Blade:
                fullName = "Blade";
                setColor = C_blade;
                break;
            case TrapType.Chute:
                fullName = "Chute";
                setColor = C_chute;
                break;
            case TrapType.DirtyBomb:
                fullName = "Dirty Bomb";
                setColor = C_dirtyBomb;
                break;
            case TrapType.EMP:
                fullName = "EMP";
                setColor = C_EMP;
                break;
            case TrapType.Fire:
                fullName = "Fire";
                setColor = C_Fire;
                break;
            case TrapType.HE:
                fullName = "Heavy Explosive";
                setColor = C_HE;
                break;
            case TrapType.Hellfire:
                fullName = "Hellfire";
                setColor = C_Hellfire;
                break;
            case TrapType.ProtonBomb:
                fullName = "Proton Bomb";
                setColor = C_ProtonBomb;
                break;
            case TrapType.Segregator:
                fullName = "Segregator";
                setColor = C_Segregator;
                break;
            case TrapType.Shock:
                fullName = "Shock";
                setColor = C_Shock;
                break;
            case TrapType.Stasis:
                fullName = "Stasis";
                setColor = C_Stasis;
                break;
            case TrapType.NONE:
                fullName = "Unknown";
                setColor = C_NONE;
                break;
        }
        fullName += " Trap";
    }

    public void LocateTrap()
    {
        assignedTile.isExplored = true; // There is no possible way this could backfire
        knowByPlayer = true;

        UIManager.inst.CreateItemPopup(this.gameObject, fullName, Color.black, setColor, HF.GetDarkerColor(setColor, 20f));

        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_ui["TRAP_SCAN"]); // TRAP_SCAN
    }

    public void SetAlignment(BotAlignment newA)
    {
        alignment = newA;
    }

    #region Trap Detonation

    public void TripTrap(GameObject fool = null)
    {
        tripped = true;

        if(fool != null && fool == PlayerData.inst.gameObject)
        {
            // Freeze player
            PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = false;
        }

        // Play trap triggered sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_game["TRAPTRIGGER"]); // GAME - TRAPTRIGGER

        // Log a message
        UIManager.inst.CreateNewLogMessage("Triggered " + fullName, UIManager.inst.warningOrange, UIManager.inst.corruptOrange_faded, false, true);

        Debug.Log("Mine tripped!");

        StartCoroutine(TripTrap_Consequences(fool));
    }

    private IEnumerator TripTrap_Consequences(GameObject fool = null)
    {
        yield return new WaitForEndOfFrame();

        switch (type)
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

        if (fool != null && fool == PlayerData.inst.gameObject)
        {
            // Un-Freeze player
            PlayerData.inst.GetComponent<PlayerGridMovement>().playerMovementAllowed = true;
        }

        RemoveFromWorld();
    }

    #endregion

    /// <summary>
    /// Disarms the trap, making it no longer a threat to anyone.
    /// </summary>
    public void DeActivateTrap()
    {
        active = false;
    }

    /// <summary>
    /// Arms a trap, making it a threat to whichever alignment it is set to.
    /// </summary>
    public void ActivateTrap(BotAlignment newA)
    {
        active = true;
        alignment = newA;

    }
    /// <summary>
    /// Remove this trap from its "on floor" state and turn it into an item that can be picked up.
    /// </summary>
    public void ItemizeTrap()
    {

    }

    bool finished = false;
    IEnumerator RemoveFromWorld()
    {
        zone.trapList.Remove(this);

        while (!finished)
        {
            yield return null;
        }

        Destroy(this.gameObject);
    }
}

public enum TrapType
{
    Alarm,
    Blade,
    Chute,
    DirtyBomb,
    EMP,
    Fire,
    HE, // Heavy Explosive
    Hellfire,
    ProtonBomb,
    Segregator,
    Shock,
    Stasis,
    NONE
}