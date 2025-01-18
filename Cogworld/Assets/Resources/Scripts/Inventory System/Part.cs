using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

public class Part : MonoBehaviour
{
    [Header("Key Values")]
    public bool inInventory; // True = In Inventory, False = On Ground
    public bool knownByPlayer; // Prototype

    public TileBlock _tile; // Assigned on Creation
    public Vector2Int location; // Assigned on Creation
    public Item _item; // Assigned on Creation
    [Tooltip("Is this item native to 0b10? If false, scavengers will move to pick up and recycle this item.")]
    public bool native = false;

    public SpriteRenderer _sprite;

    [SerializeField] private Animator protoAnimation;
    public Color protoColor = Color.white;
    public Color realColor;
    public Color halfColor;

    public string displayText;

    [Header("Visibility")]
    public bool isExplored;
    public bool isVisible;
    public bool showFloatingName = false;

    [Header("Matter Related")] // Needed if this item is matter, mostly for visuals
    public bool isMatterItem = false;

    [Header("Rigged Item Flash")] // If this item is Rigged to Explode, it should be playing an animation when visible.
    [SerializeField] private Animator _riggedAnimator;
    [SerializeField] private GameObject _riggedSprite;
    private bool isRigged = false;

    [Tooltip("Pickup cooldown for a few edge cases (like console-based item spawning)")]
    private float pickupcooldown = 0.2f;

    public void Init()
    {
        // Set basic values
        this._sprite.sprite = _item.itemData.floorDisplay;
        this.realColor = _item.itemData.itemColor;
        inInventory = false;

        if (_item.itemData.knowByPlayer) // We know what this item is
        {
            knownByPlayer = true;
            displayText = HF.GetFullItemName(_item);
            _sprite.color = realColor;
            halfColor = new Color((realColor.r / 2), (realColor.g / 2), (realColor.b / 2));
        }
        else
        {
            knownByPlayer = false;
            displayText = HF.ItemPrototypeName(_item);
            _sprite.color = protoColor;
            halfColor = Color.gray;
        }

        if (this._item != null && this._item.Id == 17) // Is this item just *Matter*?
        {
            isMatterItem = true;

            SetMatterColors();
        }

        // Rigged for Explosion
        if(this._item.itemData.quality == ItemQuality.Rigged)
        {
            isRigged = true;
            //_riggedAnimator.enabled = true;
            StartCoroutine(RiggedFlash());
        }
        
        StartCoroutine(PickupCooldown()); // Forbid picking up for a very small amount of time
    }

    private IEnumerator PickupCooldown()
    {
        float elapsedTime = 0f;
        float duration = pickupcooldown;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pickupcooldown = 0f;
    }

    public void SetMatterColors()
    {
        // - Now we want to set what shade of purple we will use. This depends on the amount of matter in the stack.
        // - Pink-ish purple = (>100)   | 194 0 255
        // - Purple          = (100-25) | 134 0 178
        // - Dark Purple     = (<25)    | 109 0 145
        int matter = this._item.amount;
        if (matter > 100)
        {
            realColor = new Color(194f / 255f, 0f, 255f / 255f);

            halfColor = new Color(154f / 255f, 0f, 205f / 255f);
        }
        else if (matter < 100 && matter > 25)
        {
            realColor = new Color(134f / 255f, 0f, 178f / 255f);

            halfColor = new Color(118f / 255f, 0f, 159f / 255f);
        }
        else
        {
            realColor = new Color(109f / 255f, 0f, 145f / 255f);

            // (We need to be careful with this one so that it doesn't go completely black when unseen)
            halfColor = new Color(95f / 255f, 0f, 125f / 255f);
        }
    }

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

        bool known = _item.itemData.knowByPlayer;
        Color full, half;
        if (known)
        {
            full = realColor;
            half = halfColor;
        }
        else
        {
            full = protoColor;
            half = Color.gray;

            if (isVisible)
            {
                protoAnimation.enabled = true;
                protoAnimation.Play("PrototypeFlash");
            }
            else
            {
                protoAnimation.enabled = false;
            }
        }

        if (isVisible)
        {
            _sprite.color = full;

            if(isRigged)
                _riggedSprite.SetActive(true);
        }
        else if (isExplored && isVisible)
        {
            _sprite.color = full;

            if (isRigged)
                _riggedSprite.SetActive(true);
        }
        else if (isExplored && !isVisible)
        {
            _sprite.color = half;

            if (isRigged)
                _riggedSprite.SetActive(false);
        }
        else if (!isExplored)
        {
            _sprite.color = Color.black;

            if (isRigged)
                _riggedSprite.SetActive(false);
        }
    }

    private IEnumerator RiggedFlash()
    {
        // Our goal here is flash the _riggedSprite on and off once per second
        if (isRigged)
        {
            if (Time.time % 1 == 0)
            {
                _riggedSprite.SetActive(!_riggedSprite.gameObject.activeInHierarchy);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public string ParseType(ItemType type)
    {
        switch (type)
        {
            case ItemType.Default:
                return "DEFAULT";
            case ItemType.Engine:
                return "ENGINE";
            case ItemType.PowerCore:
                return "ENGINE";
            case ItemType.Reactor:
                return "ENGINE";
            case ItemType.Treads:
                return "PROPULSION";
            case ItemType.Legs:
                return "PROPULSION";
            case ItemType.Wheels:
                return "PROPULSION";
            case ItemType.Hover:
                return "PROPULSION";
            case ItemType.Flight:
                return "PROPULSION";
            case ItemType.Storage:
                return "DEVICE";
            case ItemType.Processor:
                return "PROCESSOR";
            case ItemType.Hackware:
                return "PROCESSOR";
            case ItemType.Device:
                return "DEVICE";
            case ItemType.Armor:
                return "ARMOR";
            case ItemType.Gun:
                return "WEAPON";
            case ItemType.EnergyCannon:
                return "WEAPON";
            case ItemType.EnergyGun:
                return "WEAPON";
            case ItemType.Impact:
                return "WEAPON";
            case ItemType.Cannon:
                return "WEAPON";
            case ItemType.Launcher:
                return "WEAPON";
            case ItemType.Special:
                return "WEAPON";
            case ItemType.Melee:
                return "WEAPON";
            default:
                return "OBJECT";

        }
    }

    private void OnMouseEnter()
    {
        if (isExplored && MouseData.tempItemBeingDragged == null) // Must be able to see the item & shouldn't display when moving an item
        {
            showFloatingName = true;
            _highlight.SetActive(true);

            bool found = false;
            // If a popup for this doesn't already exist we need to create one.
            // Start by looking for an existing one
            foreach (GameObject P in UIManager.inst.itemPopups)
            {
                if (P.GetComponentInChildren<UIItemPopup>()._parent == this.gameObject)
                {
                    found = true;
                    P.GetComponentInChildren<UIItemPopup>().mouseOver = true;
                    break;
                }
            }

            if (!found)
            {
                Color a = Color.black, b = Color.black, c = Color.black;
                a = Color.black;
                string _message = "";
                if (_item.itemData.instantUnique)
                {
                    _message = _item.amount.ToString() + " " + HF.GetFullItemName(_item);
                    b = _item.itemData.itemColor;
                    c = new Color(_item.itemData.itemColor.r, _item.itemData.itemColor.g, _item.itemData.itemColor.b, 0.7f);
                }
                else
                {
                    // Is this item a prototype? If so we don't show the true name & rating
                    if (_item.itemData.knowByPlayer)
                    {
                        _message = HF.GetFullItemName(_item) + " [" + _item.itemData.rating.ToString() + "]"; // Name [Rating]
                    }
                    else
                    {
                        _message = HF.ItemPrototypeName(_item);
                    }

                    // Set color related to current item health
                    float HP = (float)_item.integrityCurrent / (float)_item.itemData.integrityMax;
                    if (HP >= 0.75) // Healthy
                    {
                        b = UIManager.inst.highGreen; // Special item = special color
                        c = new Color(UIManager.inst.highGreen.r, UIManager.inst.highGreen.g, UIManager.inst.highGreen.b, 0.7f);
                    }
                    else if (HP < 0.75 && HP >= 0.5) // Minor Damage
                    {
                        b = UIManager.inst.cautiousYellow; // Special item = special color
                        c = new Color(UIManager.inst.cautiousYellow.r, UIManager.inst.cautiousYellow.g, UIManager.inst.cautiousYellow.b, 0.7f);
                    }
                    else if (HP < 0.5 && HP >= 0.25) // Medium Damage
                    {
                        b = UIManager.inst.slowOrange; // Special item = special color
                        c = new Color(UIManager.inst.slowOrange.r, UIManager.inst.slowOrange.g, UIManager.inst.slowOrange.b, 0.7f);
                    }
                    else // Heavy Damage
                    {
                        b = UIManager.inst.dangerRed; // Special item = special color
                        c = new Color(UIManager.inst.dangerRed.r, UIManager.inst.dangerRed.g, UIManager.inst.dangerRed.b, 0.7f);
                    }
                }
                UIManager.inst.CreateItemPopup(this.gameObject, _message, a, b, c);
            }
        }
    }

    void OnMouseExit()
    {
        showFloatingName = false;
        _highlight.SetActive(false);

        if (isExplored)
        {
            foreach (GameObject P in UIManager.inst.itemPopups.ToList())
            {
                if (P.GetComponentInChildren<UIItemPopup>()._parent == this.gameObject)
                {
                    UIManager.inst.itemPopups.Remove(P);

                    P.GetComponentInChildren<UIItemPopup>().MessageOut();
                    break;
                }
            }
        }
    }

    public void TryEquipItem()
    {
        if (PlayerData.inst.GetComponent<PlayerGridMovement>().GetCurrentPlayerTile() == _tile) // Is the player ontop of this item?
        {
            if (pickupcooldown > 0) return;

            // If the player clicks on this item, we want to first try and put it in one of their / PARTS / slots.
            bool slotAvailable = false;

            // First we want to see if there is space to add this item
            // - Check if the current amount of items the player holds in this sub-inventory is < the max,
            int size = _item.itemData.slotsRequired;

            switch (_item.itemData.slot)
            {
                case ItemSlot.Power:
                    if (PlayerData.inst.GetComponent<PartInventory>().inv_power.EmptySlotCount >= size)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Propulsion:
                    if (PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.EmptySlotCount >= size)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Utilities:
                    if (PlayerData.inst.GetComponent<PartInventory>().inv_utility.EmptySlotCount >= size)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Weapons:
                    if (PlayerData.inst.GetComponent<PartInventory>().inv_weapon.EmptySlotCount >= size)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Other: // This one goes into inventory instead
                    if (PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount >= size)
                    {
                        slotAvailable = false;
                    }
                    break;
                case ItemSlot.Inventory: // This one goes into inventory instead
                    if (PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount >= size)
                    {
                        slotAvailable = false;
                    }
                    break;
                default:
                    Debug.LogError("ERROR: Item slot type not set!");
                    break;
            }

            // By default we want to try and add it to the inventory first
            if (PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount >= size)
            {
                // There is space, we can add it to the inventory
                InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._inventory);
                InventoryControl.inst.UpdateInterfaceInventories();
                PlayerData.inst.currentInvCount = PlayerData.inst.GetComponent<PartInventory>()._inventory.ItemCount;
                UIManager.inst.CreateNewLogMessage("Aquired " + HF.GetFullItemName(_item) + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);

                // Play a sound
                PlayEquipSound();
                // Play an animation
                showFloatingName = false;
                _highlight.SetActive(false);
                // Remove this item from the ground
                TryDisableConnectedPopup();
                _tile._partOnTop = null;
                InventoryControl.inst.worldItems.Remove(new Vector2Int(_tile.locX, _tile.locY));
                Destroy(this.gameObject);
            }
            else if(slotAvailable) // If no space in inventory, then we try to add it to the slots
            {
                // Firstly, does the player have the required matter needed to equip this item?
                // "Attaching a part requires 20 energy and 10 matter. Detaching a part expends 10 energy."
                if(PlayerData.inst.currentMatter < 10)
                {
                    UIManager.inst.ShowCenterMessageTop($"Insufficient matter stored to equip item ({10 - PlayerData.inst.currentMatter})", UIManager.inst.dangerRed, Color.black);
                    return;
                }
                else if (PlayerData.inst.currentEnergy < 20)
                {
                    UIManager.inst.ShowCenterMessageTop($"Insufficient energy stored to equip item ({20 - PlayerData.inst.currentEnergy})", UIManager.inst.dangerRed, Color.black);
                    return;
                }

                switch (_item.itemData.slot)
                {
                    case ItemSlot.Power:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>().inv_power);

                        break;
                    case ItemSlot.Propulsion:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>().inv_propulsion);
                        PlayerData.inst.maxWeight += _item.itemData.propulsion[0].support;

                        break;
                    case ItemSlot.Utilities:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>().inv_utility);

                        break;
                    case ItemSlot.Weapons:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>().inv_weapon);

                        break;
                    case ItemSlot.Other: // This one goes into inventory instead
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._inventory);

                        break;
                    case ItemSlot.Inventory: // This one goes into inventory instead
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._inventory);

                        break;
                    default:
                        Debug.LogError("ERROR: Item slot type not set!");
                        break;
                }

                // Item Discovery and Logging
                HF.DiscoverPrototype(_item, true);

                // Play a sound
                PlayEquipSound();
                // Play an animation

                showFloatingName = false;
                _highlight.SetActive(false);
                // Remove this item from the ground
                TryDisableConnectedPopup();
                _tile._partOnTop = null;
                InventoryControl.inst.worldItems.Remove(new Vector2Int(_tile.locX, _tile.locY));
                Destroy(this.gameObject);
            }
            else // No slot available
            {
                InventoryControl.inst.UpdateInterfaceInventories();
                // Full here too, give up
                // Display a message(
                UIManager.inst.ShowCenterMessageTop("No free slot", UIManager.inst.dangerRed, Color.black);
                return;
            }
        }

        showFloatingName = false;
        
        // Update UI
        UIManager.inst.UpdatePSUI();
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
    }

    public void TryDisableConnectedPopup()
    {
        foreach (GameObject P in UIManager.inst.itemPopups)
        {
            if (P.GetComponentInChildren<UIItemPopup>() != null && P.GetComponentInChildren<UIItemPopup>()._parent == this.gameObject)
            {
                P.GetComponentInChildren<UIItemPopup>().mouseOver = false;
                P.GetComponentInChildren<UIItemPopup>().MessageOut();
                break;
            }
        }
    }

    private void Update()
    {
        CheckShowFloatingText();
        HighlightCheck();
    }

    private void CheckShowFloatingText()
    {

    }

    [Header("Highlighting")]
    public GameObject _highlight;
    public GameObject _highlightPerm;
    public Animator flashWhite;
    public Color highlight_white;

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

    #region Rigged Detonation

    public void DetonateRiggedItem()
    {
        // Create an explosion TODO


        // Destroy this item
        showFloatingName = false;
        _highlight.SetActive(false);
        // Remove this item from the ground
        TryDisableConnectedPopup();
        _tile._partOnTop = null;
        InventoryControl.inst.worldItems.Remove(new Vector2Int(_tile.locX, _tile.locY));
        Destroy(this.gameObject);

    }

    #endregion


    #region Audio

    public void PlayEquipSound()
    {
        // First check if this item is light or not
        string name = HF.GetFullItemName(_item);
        if (name.Contains("Lgt.") 
            || name.Contains("Lgt") 
            || name.Contains("LGT") 
            || name.Contains("LGT.") 
            || name.Contains("Light"))
        {
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_equipitem[$"PART_LGT_0{Random.Range(1, 3)}"]); // PART_LIGHT_1/2/3
        }
        else
        {
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_equipitem[$"PART_0{Random.Range(1, 3)}"]); // PART_1/2/3/4
        }
    }

    #endregion
}
