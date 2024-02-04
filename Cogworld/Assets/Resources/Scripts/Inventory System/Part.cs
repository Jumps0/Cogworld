using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Part : MonoBehaviour
{
    [Header("Key Values")]
    public bool inInventory; // True = In Inventory, False = On Ground
    public bool knownByPlayer; // Prototype

    public TileBlock _tile; // Assigned on Creation
    public Vector2Int location; // Assigned on Creation
    public ItemObject _item; // Assigned on Creation

    public SpriteRenderer _sprite;

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

    public void Init()
    {
        // Set basic values
        this._sprite.sprite = _item.floorDisplay;
        this.realColor = _item.itemColor;
        inInventory = false;

        if (InventoryControl.inst.knownItems.Contains(_item)) // We know what this item is
        {
            knownByPlayer = true;
            displayText = _item.itemName;
            _sprite.color = realColor;
        }
        else
        {
            knownByPlayer = false;
            displayText = "Unknown " + ParseType(_item.type);
            _sprite.color = protoColor;
        }

        halfColor = new Color((realColor.r / 2), (realColor.g / 2), (realColor.b / 2));

        if (this._item && this._item.data.Id == 17) // Is this item just *Matter*?
        {
            isMatterItem = true;

            SetMatterColors();
        }

        // Rigged for Explosion
        if(this._item.quality == ItemQuality.Rigged)
        {
            isRigged = true;
            //_riggedAnimator.enabled = true;
            StartCoroutine(RiggedFlash());
        }
        
    }

    private void SetMatterColors()
    {
        // - Now we want to set what shade of purple we will use. This depends on the amount of matter in the stack.
        // - Pink-ish purple = (>100)
        // - Purple          = (100-25)
        // - Dark Purple     = (<25)
        int matter = this._item.amount;
        if (matter > 100)
        {
            realColor = Color.white; // Full color, pinkish-purple

            halfColor = Color.gray;
        }
        else if (matter < 100 && matter > 25)
        {
            realColor = new Color(0.5f, 0.5f, 0.5f); // Purple

            halfColor = new Color(0.35f, 0.35f, 0.35f);
        }
        else
        {
            realColor = new Color(0.29f, 0.29f, 0.29f); // Dark Purple

            // (We need to be careful with this one so that it doesn't go completely black when unseen)
            halfColor = new Color(0.235f, 0.235f, 0.235f);
        }
    }

    public void UpdateVisibility()
    {
        // =============================================
        //    NOTE
        // 
        // There is an issue right now with this
        // because the IMAGEs being used for items
        // are mostly all BLUE and the SPRITE
        // being used is also set to BLUE (because
        // of the items level) so this ends up making
        // the item appear darker.
        // 
        // This will be fixed later if/when the IMAGE
        // for items are all set to white like they
        // should be.
        //
        // ==============================================
        
        if (isVisible)
        {
            _sprite.color = realColor;
            if(isRigged)
                _riggedSprite.SetActive(true);
        }
        else if (isExplored && isVisible)
        {
            _sprite.color = realColor;
            if (isRigged)
                _riggedSprite.SetActive(true);
        }
        else if (isExplored && !isVisible)
        {
            _sprite.color = halfColor;
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
                if (_item.instantUnique)
                {
                    _message = _item.amount.ToString() + " " + _item.itemName;
                    b = _item.itemColor;
                    c = new Color(_item.itemColor.r, _item.itemColor.g, _item.itemColor.b, 0.7f);
                }
                else
                {
                    _message = _item.itemName + " [" + _item.rating.ToString() + "]"; // Name [Rating]
                    // Set color related to current item health
                    float HP = (float)_item.integrityCurrent / (float)_item.integrityMax;
                    if (HP >= 0.75) // Healthy
                    {
                        b = UIManager.inst.activeGreen; // Special item = special color
                        c = new Color(UIManager.inst.activeGreen.r, UIManager.inst.activeGreen.g, UIManager.inst.activeGreen.b, 0.7f);
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
            foreach (GameObject P in UIManager.inst.itemPopups)
            {
                if (P.GetComponentInChildren<UIItemPopup>()._parent == this.gameObject)
                {
                    P.GetComponentInChildren<UIItemPopup>().mouseOver = false;
                    break;
                }
            }
        }
    }

    public void TryEquipItem()
    {
        
        if (PlayerData.inst.GetComponent<PlayerGridMovement>().GetCurrentPlayerTile() == _tile) // Is the player ontop of this item?
        {
            // If the player clicks on this item, we want to first try and put it in one of their / PARTS / slots.
            bool slotAvailable = false;
            switch (_item.slot)
            {
                // First we want to see if there is space to add this item
                // - Check if the current amount of items the player holds in this sub-inventory is < the max,

                case ItemSlot.Power:
                    if (PlayerData.inst.GetComponent<PartInventory>()._invPower.EmptySlotCount > 0)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Propulsion:
                    if (PlayerData.inst.GetComponent<PartInventory>()._invPropulsion.EmptySlotCount > 0)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Utilities:
                    if (PlayerData.inst.GetComponent<PartInventory>()._invUtility.EmptySlotCount > 0)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Weapons:
                    if (PlayerData.inst.GetComponent<PartInventory>()._invWeapon.EmptySlotCount > 0)
                    {
                        slotAvailable = true;
                    }
                    break;
                case ItemSlot.Other: // This one goes into inventory instead
                    if (PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount > 0)
                    {
                        slotAvailable = true;
                    }
                    break;
                default:
                    Debug.LogError("ERROR: Item slot type not set!");
                    break;
            }

            if (slotAvailable) // There is space, we can add it!
            {
                Debug.Log(">> Adding " + this._item.itemName + " - " + this._item + " - " + this._item.type + " - to inventory.");
                switch (_item.slot)
                {
                    case ItemSlot.Power:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._invPower);

                        break;
                    case ItemSlot.Propulsion:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._invPropulsion);
                        PlayerData.inst.maxWeight += _item.propulsion[0].support;

                        break;
                    case ItemSlot.Utilities:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._invUtility);

                        break;
                    case ItemSlot.Weapons:
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._invWeapon);

                        break;
                    case ItemSlot.Other: // This one goes into inventory instead
                        InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._inventory);

                        break;
                    default:
                        Debug.LogError("ERROR: Item slot type not set!");
                        break;
                }

                InventoryControl.inst.UpdateInterfaceInventories();
                UIManager.inst.CreateNewLogMessage("Aquired " + this._item.itemName + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                //PlayerData.inst.currentWeight += _item.mass;

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
            else // No space available in slot, try adding to inventory instead
            {

                if (PlayerData.inst.GetComponent<PartInventory>()._inventory.EmptySlotCount > 0)
                {
                    Debug.Log($"Adding item {this._item} to inventory.");
                    
                    // There is space, we can add it to the inventory
                    InventoryControl.inst.AddItemToPlayer(this, PlayerData.inst.GetComponent<PartInventory>()._inventory);
                    InventoryControl.inst.UpdateInterfaceInventories();
                    PlayerData.inst.currentInvCount += 1;
                    UIManager.inst.CreateNewLogMessage("Aquired " + this._item.itemName + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
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
                else
                {
                    Debug.Log($"Failed to add item {this._item} to inventory.");
                    InventoryControl.inst.UpdateInterfaceInventories();
                    // Full here too, give up
                    // Display a message(?)
                    return;
                }
            }
        }

        showFloatingName = false;
        
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
        UpdateVisibility();
        CheckShowFloatingText();
        HighlightCheck();

        if (isMatterItem) // Matter Check
            CheckMatter();
    }


    /// <summary>
    /// =================================================
    ///  TODO:
    /// 
    ///      IN THE FUTURE, MOVE THIS MATTER CHECK
    ///      INTO THE PLAYER'S LOGIC SO THAT INSTEAD
    ///      OF EVERY MATTER DROP ON THE MAP CHECKING
    ///      THIS EVERY FRAME, THE PLAYER JUST DOES
    ///      IT INSTEAD (ONLY ONE CALL PER FRAME).
    ///      
    ///      WILL MOST LIKELY INVOLVE A RAYCAST AND
    ///      SENDING A SIGNAL TO THIS ITEM/PART.
    ///         
    /// =================================================
    /// </summary>

    bool mCheck = true;
    public void CheckMatter()
    {
        // Player is current ON-TOP of this item && has space for more matter.
        if (PlayerData.inst.gameObject != null & PlayerData.inst.gameObject.transform.position == this.transform.position && mCheck && (PlayerData.inst.maxMatter != PlayerData.inst.currentMatter))
        {
            int diff = (PlayerData.inst.maxMatter - PlayerData.inst.currentMatter);
            if (diff >= this._item.amount) // Player can pick-up all of this matter
            {
                PlayerData.inst.currentMatter += this._item.amount; // Add it to inventory
                Destroy(this.gameObject); // Destroy this item
            }
            else if(diff < this._item.amount && diff != 0) // Player can only pick-up some of this matter
            {
                PlayerData.inst.currentMatter += diff; // Add it to inventory
                this._item.amount -= diff;
                SetMatterColors(); // May need to change to color of the ground item.
            }

            UIManager.inst.UpdatePSUI();
            UIManager.inst.CreateNewLogMessage(($"Aquired {this._item.amount} Matter."), UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
            // Include something for internal matter later

            mCheck = false;
        }
        else if (PlayerData.inst.gameObject != null & PlayerData.inst.gameObject.transform.position != this.transform.position)
        {
            mCheck = true; // Don't want to keep checking when player is on top, only once.
        }
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
        if (_item.itemName.Contains("Lgt.") || _item.itemName.Contains("Lgt") || _item.itemName.Contains("LGT") || _item.itemName.Contains("LGT.") || _item.itemName.Contains("Light"))
        {
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.equipItem_Clips[Random.Range(4,6)]);
        }
        else
        {
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.equipItem_Clips[Random.Range(0,3)]);
        }
    }

    #endregion
}
