using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // For sprite auto-assigning
public class MachinePart : MonoBehaviour
{
    [Header("Core Info")]
    public TileObject info;
    [Header("----Overview----")]
    public string displayName;
    public MachineType type;
    [Tooltip("Basically health. X = current, Y = max.")]
    public Vector2Int armor;
    public bool state = true; // Active
    [Header("----Explosive Potential----")]
    public bool explodes = false;
    public int heatTransfer = 0;
    public ExplosionGeneric explosiveDetails;
    public ItemExplosion explosivePotential;
    [Header("----Resistances----")]
    public List<BotResistances> resistances;
    [Header("----Random----")]
    public bool initializeRandom = false; // If true, will be set to a random machine sprite, and to a random rotation
    public bool walkable = false; // If true, certain bots (zionities, subcaves, etc.) will be able to walk through this machine.

    [Header("Special Types")]
    public bool isSealedDoor = false; // Orange
    public bool isSealedStorage = false; // Blue
    public Color sealedDoorOrange = new Color32(255, 128, 0, 255);
    public Color sealedStorageBlue = new Color32(199, 234, 255, 255);

    public bool destroyed = false;

    [Header("Colors")]
    public Color activeColor;
    public Color disabledColor;

    [Header("Components")]
    public List<MachinePart> connectedParts;
    public MachinePart parentPart;

    [Header("Visibility")]
    public bool isExplored = false;
    public bool isVisible = false;
    public GameObject indicator = null;
    private SpriteRenderer sr;

    #region Pre-setup
    private void OnValidate()
    {
        if(info != null)
        {
            // Get sprite renderer
            if(sr == null)
            {
                sr = this.GetComponent<SpriteRenderer>();
            }

            if(sr != null && info.displaySprite != null)
            {
                // Auto-assign sprite
                sr.sprite = info.displaySprite;
            }

            // Set up the other values while we're here
            armor = info.health;
            resistances = info.resistances;
        }   
    }
    #endregion

    private void Start()
    {
        if (initializeRandom)
        {
            this.transform.Rotate(new Vector3(0f, 0f, 90f * Random.Range(0, 4))); // Random Rotation
            this.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.machinePartSprites[Random.Range(0, MiscSpriteStorage.inst.machinePartSprites.Count - 1)]; // Random sprite
        }

        if (isSealedDoor)
        {
            this.GetComponent<SpriteRenderer>().color = sealedDoorOrange;
        }
        if (isSealedStorage)
        {
            this.GetComponent<SpriteRenderer>().color = sealedStorageBlue;
        }

        // Set the name
        SetName();

        activeColor = this.GetComponent<SpriteRenderer>().color;
    }

    private void SetName()
    {
        // -- TODO: This will need to be expanded later if/when new machines are added --

        // First check if this is an interactable machine
        if(this.GetComponent<Terminal>() || (parentPart && parentPart.GetComponent<Terminal>()))
        {
            displayName = "Terminal";
            type = MachineType.Terminal;
        }
        else if (this.GetComponent<Fabricator>() || (parentPart && parentPart.GetComponent<Fabricator>()))
        {
            displayName = "Fabricator";
            type = MachineType.Fabricator;
        }
        else if (this.GetComponent<RecyclingUnit>() || (parentPart && parentPart.GetComponent<RecyclingUnit>()))
        {
            displayName = "Recycling Unit";
            type = MachineType.Recycling;
        }
        else if (this.GetComponent<Garrison>() || (parentPart && parentPart.GetComponent<Garrison>()))
        {
            displayName = "Garrison";
            type = MachineType.Garrison;
        }
        else if (this.GetComponent<RepairStation>() || (parentPart && parentPart.GetComponent<RepairStation>()))
        {
            displayName = "Repair Station";
            type = MachineType.RepairStation;
        }
        else if (this.GetComponent<Scanalyzer>() || (parentPart && parentPart.GetComponent<Scanalyzer>()))
        {
            displayName = "Scanalyzer";
            type = MachineType.Scanalyzer;
        }
        else if (this.GetComponent<TerminalCustom>() || (parentPart && parentPart.GetComponent<TerminalCustom>()))
        {
            if (parentPart)
            {
                displayName = parentPart.GetComponent<TerminalCustom>().systemName;
            }
            else
            {
                displayName = this.GetComponent<TerminalCustom>().systemName;
            }
            type = MachineType.CustomTerminal;
        }
        else if(this.GetComponent<StaticMachine>() || (parentPart && parentPart.GetComponent<StaticMachine>()))
        {
            if (parentPart)
            {
                displayName = parentPart.GetComponent<StaticMachine>()._name;
            }
            else
            {
                displayName = this.GetComponent<StaticMachine>()._name;
            }
            type = MachineType.Misc;
        }
    }

    #region Visibility
    public void UpdateVis(byte update)
    {
        if (update == 0) // UNSEEN/UNKNOWN
        {
            isExplored = false;
            isVisible = false;
        }
        else if (update == 1) // UNSEEN/EXPLORED
        {
            isExplored = true;
            isVisible = false;
        }
        else if (update == 2) // SEEN/EXPLORED
        {
            isExplored = true;
            isVisible = true;
        }

        Color actiColor = activeColor;
        if(parentPart == null || parentPart == this)
        {
            actiColor = Color.white;
        }


        if (parentPart && parentPart.state)
        {
            if (isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = actiColor;
            }
            else if (isExplored && isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = actiColor;
            }
            else if (isExplored && !isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = new Color(actiColor.r, actiColor.g, actiColor.b, 0.7f);
            }
            else if (!isExplored)
            {
                this.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
        else
        {
            if (isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = disabledColor;
            }
            else if (isExplored && isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = disabledColor;
            }
            else if (isExplored && !isVisible)
            {
                this.GetComponent<SpriteRenderer>().color = new Color(disabledColor.r, disabledColor.g, disabledColor.b, 0.9f);
            }
            else if (!isExplored)
            {
                this.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

        
    }

    public void RevealMe()
    {
        // We need to:
        // 1. Reveal the indicator
        // 2. Set all parts of this machine to explored

        // 1.


        // 2.
        parentPart.isExplored = true;
        foreach (var M in connectedParts)
        {
            M.isExplored = true;
        }
    }
    #endregion

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Right Click to open /DATA/ Menu
        {
            UIManager.inst.Data_OpenMenu(null, this.gameObject);
        }
    }

    #region Destruction

    /// <summary>
    /// For when this part needs to actually be destroyed. Plays a sound effect, and leaves some trash behind.
    /// </summary>
    public void DestroyMe()
    {
        destroyed = true;

        // Disable any ambient audio (Terminals & Static Machines have this)
        if (parentPart.GetComponent<Terminal>())
        {
            parentPart.GetComponent<Terminal>().GetComponent<AudioSource>().enabled = false;
        }
        else if (parentPart.GetComponent<StaticMachine>())
        {
            parentPart.GetComponent<StaticMachine>().GetComponent<AudioSource>().enabled = false;
        }

        // Decide if this should leave debris or note (random)
        if (Random.Range(0f, 1f) > 0.5f) // Yes, debris
        {
            if (MapManager.inst.loaded)
            {
                MapManager.inst._allTilesRealized[new Vector2Int((int)this.transform.position.x, (int)this.transform.position.y)].bottom.SetToDirty();
            }
            // Play a sound
            AudioClip clip = AudioManager.inst.nonBotDestruction_Clips[Random.Range(18, 22)]; // METAL_DEBRIS_1/2/3/4/5
            AudioManager.inst.CreateTempClip(this.transform.position, clip, 0.8f);
        }
        else // No debris
        {
            // Play a sound
            AudioClip clip = AudioManager.inst.nonBotDestruction_Clips[Random.Range(13, 17)]; // METAL_1/2/3/4/5
            AudioManager.inst.CreateTempClip(this.transform.position, clip, 0.8f);
        }

        // Annouce that this machine has been disabled (don't wanna spam this)
        if(parentPart != this && parentPart.state && !annouced) // Not the parent part but still active
        {
            parentPart.AnnouceDisabled();
        }
        else if(parentPart == this && state && !annouced) // Is the parent part and still active
        {
            AnnouceDisabled();
        }

        state = false;
        Destroy(parentPart.indicator);

        // Remove from MapManager data
        Vector2Int pos = HF.V3_to_V2I(this.transform.position);
        MapManager.inst._allTilesRealized.Remove(pos);
        HF.RemoveMachineFromList(this);
        MapManager.inst._allTilesRealized[pos].bottom.occupied = false;
        MapManager.inst._allTilesRealized[pos].bottom.walkable = true; // should be fine?

        Destroy(this.transform);
    }

    bool annouced = false;
    private void AnnouceDisabled()
    {
        annouced = true;
        string message = "";

        if (this.GetComponent<Terminal>())
        {
            message = "Terminal";
        }
        else if (this.GetComponent<Fabricator>())
        {
            message = "Fabricator";
        }
        else if (this.GetComponent<RecyclingUnit>())
        {
            message = "Recycling Unit";
        }
        else if (this.GetComponent<Garrison>())
        {
            message = "Garrison";
        }
        else if (this.GetComponent<RepairStation>())
        {
            message = "Repair Station";
        }
        else if (this.GetComponent<Scanalyzer>())
        {
            message = "Scanalyzer";
        }
        else if (this.GetComponent<TerminalCustom>())
        {
            if (parentPart)
            {
                message = parentPart.GetComponent<TerminalCustom>().systemName;
            }
            else
            {
                message = this.GetComponent<TerminalCustom>().systemName;
            }
        }
        else if (this.GetComponent<StaticMachine>())
        {
            if (parentPart)
            {
                message = parentPart.GetComponent<StaticMachine>()._name;
            }
            else
            {
                message = this.GetComponent<StaticMachine>()._name;
            }
        }

        message += " disabled.";

        UIManager.inst.CreateNewLogMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
    }

    private void OnDestroy()
    {
        if (MapManager.inst)
        {
            if (MapManager.inst.loaded) // This odd workaround is necessary for machines loaded from prefabs. Since two will exist and the first gets deleted, without this check it will remove the 2nd from the dictionary.
            {
                Vector2Int pos = HF.V3_to_V2I(this.transform.position);

                TData T = MapManager.inst._allTilesRealized[pos];
                T.bottom.occupied = false;
                T.top = null;
                MapManager.inst._allTilesRealized[pos] = T;
            }
            destroyed = true;
            if(parentPart)
                parentPart.state = false;
        }
    }

    #endregion
}
