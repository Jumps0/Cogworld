using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;
using static UnityEditor.Progress;
using System.Linq;

public class InvDisplayItem : MonoBehaviour
{
    [Header("UI References")]
    // -- UI References --
    //
    public TextMeshProUGUI assignedOrderText;
    public Image assignedOrderBacker;
    public Image partDisplay;
    public Image partDisplayFlasher;
    public Image healthDisplay;
    public Image healthDisplayBacker;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI specialDescText;
    [Tooltip("Used primarily for dragging around this item on the interface. UserInterface adds properties to this on startup.")]
    public Button _button;
    public Image _highlight;
    //
    public GameObject modeMain;
    public Image modeImage;
    public TextMeshProUGUI modeText;
    //
    public GameObject healthMode;
    public TextMeshProUGUI healthModeNumber; // The right side "##"
    public TextMeshProUGUI healthModeTextRep; // The bars
    public Image secondaryBacker; // Not actually for secondary objects
    //
    // --               --
    [Header("Colors")]
    // -- Colors --
    //
    public Color activeGreen;
    public Color inActiveGreen;
    public Color wideBlue;
    public Color hurtYellow;
    public Color badOrange;
    public Color dangerRed;
    public Color emptyGray;
    public Color letterWhite;
    //
    public Color highlightColor;
    public Color defaultTextColor;
    [Header("  Gradient Colors")]
    public Color gDarkBlueHigh;
    public Color gDarkBlueLow;
    public Color gBlueHigh;
    public Color gBlueLow;
    //
    // --        --

    [Header("Assignments")]
    public Item item;
    public string _assignedChar;
    [Tooltip("The display name of this item (excluding any <color>'s).")]
    public string nameUnmodified;
    public int _assignedNumber = -1;
    private bool canSiege = false;
    public UserInterface my_interface;

    [Header("Secondary")] // For Multi-Slot items
    public bool isSecondaryItem = false;
    public GameObject secondaryParent; // Reference to the InvDisplayItem that is in charge.
    public List<GameObject> secondaryChildren; // List to any secondary children

    private void Update()
    {
        if (canSiege && !isSecondaryItem) // Siege check
        {
            CheckSiegeStatus();
        }

        if(item != null)
        {
            ForceDisabledCheck();
        }
    }

    #region Setup
    /// <summary>
    /// This slot is empty, and therefore should not display anything besides "Unused"
    /// </summary>
    public void SetEmpty()
    {
        assignedOrderText.color = emptyGray; // Set assigned Letter to Gray
        itemNameText.color = emptyGray; // Set Name to Gray
        item = null;

        // Turn everything else off
        partDisplay.gameObject.SetActive(false);
        healthDisplay.enabled = false;
        modeMain.SetActive(false);
        specialDescText.gameObject.SetActive(false);
        healthMode.gameObject.SetActive(false);

        if(_assignedChar == "") // Will usually never be true
        {
            _assignedChar = "";
            assignedOrderText.text = "#";
            assignedOrderString = "";
        }
        
        itemNameText.text = "Unused";
        nameUnmodified = "Unused";
        NameUpdate();

        bonusAOS = "";
    }

    /// <summary>
    /// This slot has an item, and thus should display information about that item.
    /// </summary>
    public void SetAsFilled()
    {
        assignedOrderText.color = letterWhite;
        Color textColor = activeGreen;
        if (item.isBroken)
            textColor = UIManager.inst.highSecRed;
        if (isSecondaryItem)
            textColor = wideBlue;
        itemNameText.color = textColor;

        partDisplay.gameObject.SetActive(true);
        healthDisplay.enabled = true;
        modeMain.SetActive(true);
        //_button.gameObject.SetActive(true);
        specialDescText.gameObject.SetActive(true);
        healthMode.gameObject.SetActive(true);

        NameUpdate();
    }

    public void NameUpdate()
    {
        if(item != null && item.Id >= 0)
        {
            this.gameObject.name = $"IDI: {nameUnmodified}";
        }
        else
        {
            this.gameObject.name = "IDI: Unused";
        }
    }

    public void ForceIgnoreRaycasts()
    {
        assignedOrderText.raycastTarget = false;
        partDisplay.raycastTarget = false;
        healthDisplay.raycastTarget = false;
        itemNameText.raycastTarget = false;
        specialDescText.raycastTarget = false;
        //_button.gameObject.SetActive(false);
        modeImage.raycastTarget = false;
        modeText.raycastTarget = false;
        healthModeNumber.raycastTarget = false;
        healthModeTextRep.raycastTarget = false;
    }

    private string assignedOrderString = "";
    private string bonusAOS = "";
    public void SetLetter(string assignment, int number = -1)
    {
        if(number >= 0) // Inventory items use numbers instead of letters for assignments
        {
            _assignedNumber = number;
            assignedOrderText.text = _assignedNumber.ToString();
            assignedOrderString = _assignedNumber.ToString();

            assignedOrderText.color = letterWhite;
        }
        else // Parts menu items use letters
        {
            _assignedChar = assignment;
            assignedOrderText.text = _assignedChar.ToString();
            assignedOrderString = _assignedChar.ToString();

            if(item != null)
            {
                if (item.state)
                {
                    if (!isSecondaryItem)
                    {
                        assignedOrderText.color = activeGreen;
                    }
                    else
                    {
                        assignedOrderText.color = wideBlue;
                    }
                }
                else
                {
                    assignedOrderText.color = emptyGray;
                }

                // Also add an ":" before the indicator if removing it will destroy the item
                if (item.itemData.destroyOnRemove)
                {
                    bonusAOS = ":";
                    assignedOrderText.text = bonusAOS + _assignedChar.ToString();
                    assignedOrderString = assignedOrderText.text;
                }

                // We're gonna shove in the siege check here too since its convienient
                if (item.itemData.type == ItemType.Treads)
                {
                    if (item.itemData.propulsion[0].canSiege > 0)
                    {
                        canSiege = true;
                    }
                }
            }
        }
    }

    public void UpdateDisplay(bool startEmpty = false)
    {
        if(item != null)
        {
            bool known = item.itemData.knowByPlayer;
            bool inInventory = false;
            if (this.transform.parent.gameObject == UIManager.inst.inventoryArea)
            {
                inInventory = true;
            }

            //if(_assignedItem != null)
            //    _part = InventoryControl.inst._itemDatabase.Items[_assignedItem.Id].data;

            // - Name - // TODO: This will need to change with things like trap storage which update based on internal values
            nameUnmodified = item.itemData.itemName;
            if (item.isBroken)
            {
                nameUnmodified = "Broken " + nameUnmodified;
            }
            else if (!known)
            {
                nameUnmodified = HF.ItemPrototypeName(item);
            }
            itemNameText.text = nameUnmodified;
            if (startEmpty)
            {
                // For the one time initial animation we have the start text set to black
                itemNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>" + item.itemData.itemName + "</color>";
            }

            // - Letter Assignment - //
            if (inInventory)
            {
                partDisplay.gameObject.SetActive(true);

                // - Icon - //
                partDisplay.sprite = item.itemData.inventoryDisplay;

                // - Icon Color - //
                if (known)
                {
                    partDisplay.color = item.itemData.itemColor;
                }
                else
                {
                    partDisplay.color = Color.white; // Prototype
                }
            }
            else
            {
                partDisplay.gameObject.SetActive(false);
            }

            // - Integrity Color Square - //
            float currentIntegrity = (float)item.integrityCurrent / (float)item.itemData.integrityMax;
            if (currentIntegrity >= 0.75f) // Green (>=75%)
            {
                healthDisplay.color = activeGreen;
            }
            else if (currentIntegrity < 0.75f && currentIntegrity <= 0.50f) // Yellow (75-50%)
            {
                healthDisplay.color = hurtYellow;
            }
            else if (currentIntegrity < 0.5f && currentIntegrity <= 0.25f) // Orange (50-25%)
            {
                healthDisplay.color = badOrange;
            }
            else // Red (25-0%)
            {
                healthDisplay.color = dangerRed;
            }

            // - < Right Side Text > - //
            SetRightSideDisplay();

            // - Mode - //
            // Figure out if mode needs to be on or not
            if(item.itemData.slot == ItemSlot.Weapons && item.itemData.projectile.damage.x > 0 && item.itemData.projectile.guided.isGuided)
            {
                UISetBoxDisplay("GUIDED " + item.itemData.projectile.guided.waypoints, activeGreen);
            }
            else
            {
                modeMain.gameObject.SetActive(false);
            }

            // - Special Extra Text - //
            // Figure out if it needs to be on or not
            specialDescText.gameObject.SetActive(false);

            // Melee check
            MeleeCheck();

            // Duplicates setup
            if(!item.isDuplicate && item.duplicates.Count > 0)
            {
                // Find & Collect
                foreach (var I in item.duplicates)
                {
                    // We need to locate these duplicate items in their created slots, and link the related variables.
                    foreach (KeyValuePair<GameObject, InventorySlot> display in my_interface.slotsOnInterface)
                    {
                        //Debug.Log($"[INFO: {display.Key.name}]\n Duplicate?: {display.Value.item.isDuplicate} / {I.Name} / {display.Value.item.Name} / The same?: {I == display.Value.item} ");
                        if(display.Value.item.isDuplicate && I == display.Value.item.duplicate_uuid)
                        {
                            secondaryChildren.Add(display.Key);
                        }
                    }
                }

                // Then assign values & set display via the function
                foreach (GameObject C in secondaryChildren)
                {
                    C.GetComponent<InvDisplayItem>().SetAsSecondaryItem(this.gameObject);
                }
            }
        }
    }
    #endregion

    public void SetRightSideDisplay(bool doAnimation = false)
    {
        int mode = UIManager.inst.cewq_mode;

        // Duplicates/Secondaries don't show anything at all
        if (isSecondaryItem || item == null)
        {
            healthModeTextRep.gameObject.SetActive(false);
            healthModeNumber.gameObject.SetActive(false);
            specialDescText.gameObject.SetActive(false);
            return;
        }

        // If its a prototype we just show ???
        if (!item.itemData.knowByPlayer)
        {
            healthModeTextRep.gameObject.SetActive(false);
            healthModeNumber.gameObject.SetActive(true);

            healthModeNumber.text = "<color=#008100>???</color>";
        }
        else // If its not a prototype we show the info
        {
            healthModeNumber.color = defaultTextColor;  // Reset colors to defaults
            healthModeTextRep.color = defaultTextColor; //

            switch (mode)
            {
                case 0: // COVERAGE
                    // We need to display the bar and the amount
                    healthModeTextRep.gameObject.SetActive(true);
                    healthModeNumber.gameObject.SetActive(true);

                    // Get the coverage. 
                    float coverage = HF.FindPercentCoverageFor(item);

                    // Bail out if coverage is 0
                    if (coverage <= 0)
                    {
                        healthModeTextRep.gameObject.SetActive(false);
                        healthModeNumber.gameObject.SetActive(false);
                        return;
                    }

                    // Set %
                    float value = coverage * 100;
                    // Set to 1% if its too small (but still not 0)
                    if (value < 1 && value > 0)
                        value = 1;
                    healthModeNumber.text = Mathf.RoundToInt(value) + "%";
                    // Then set the bar, we will set it to have a max of 12
                    string c_bars = HF.ValueToStringBar(coverage, 0.2f);

                    // If this item is currently in the inventory everything should be grayed out
                    if(!item.state || !my_interface.GetComponent<DynamicInterface>())
                    {
                        healthModeNumber.color = emptyGray;

                        healthModeTextRep.text = c_bars;
                        healthModeTextRep.color = emptyGray;
                    }
                    else
                    {
                        // Uniquely, the bar has a nice little gradient, so this becomes 10x more complex
                        healthModeTextRep.text = HF.StringCoverageGradient(c_bars, activeGreen, inActiveGreen, true);
                    }

                    break;
                case 1: // ENERGY
                    // This uses both bars & the number
                    healthModeTextRep.gameObject.SetActive(true);
                    healthModeNumber.gameObject.SetActive(true);

                    string e_bars = "";

                    // We only display for things that actually USE or EMIT power
                    if (item.itemData.hasUpkeep && item.itemData.energyUpkeep < 0) // -- USE --
                    {
                        float upkeep = Mathf.Abs(item.itemData.energyUpkeep);

                        // If we are using power, the color should be DARK BLUE (#0500FF) to (#2623B5)
                        // Although if this part is OFF then the color should be GRAY
                        if (item.state && my_interface.GetComponent<DynamicInterface>())
                        {
                            // Set value
                            healthModeNumber.text = upkeep.ToString();

                            e_bars = HF.ValueToStringBar(upkeep, 50);
                            // Apply gradient
                            Color left = gDarkBlueHigh, right = gDarkBlueLow;

                            healthModeTextRep.text = HF.StringCoverageGradient(e_bars, left, right, true);
                            healthModeNumber.color = right;
                        }
                        else
                        {
                            // Set value
                            healthModeNumber.text = upkeep.ToString();

                            e_bars = HF.ValueToStringBar(upkeep, 50);
                            // Thankfully no gradient, just gray
                            healthModeTextRep.text = "<color=#464646>" + e_bars + "</color>";
                            healthModeNumber.color = emptyGray;
                        }
                    }
                    else if (item.itemData.supply > 0) // -- EMIT --
                    {
                        float supply = Mathf.Abs(item.itemData.supply);

                        // If we are supplying power, the color should be BRIGHT BLUE (#00FFFF) to (#39A9A6)
                        // Although if this part is OFF then the color should be GRAY
                        if (item.state && my_interface.GetComponent<DynamicInterface>())
                        {
                            // Set value
                            healthModeNumber.text = supply.ToString();

                            e_bars = HF.ValueToStringBar(supply, 50);
                            // Apply gradient
                            Color left = gBlueHigh, right = gBlueLow;

                            healthModeTextRep.text = HF.StringCoverageGradient(e_bars, left, right, true);
                            healthModeNumber.color = right;
                        }
                        else
                        {
                            // Set value
                            healthModeNumber.text = supply.ToString();

                            e_bars = HF.ValueToStringBar(supply, 50);
                            // Thankfully no gradient, just gray
                            healthModeTextRep.text = "<color=#464646>" + e_bars + "</color>";
                            healthModeNumber.color = emptyGray;
                        }
                    }
                    else // NOTHING
                    {
                        healthModeTextRep.gameObject.SetActive(false);
                        healthModeNumber.gameObject.SetActive(false);
                        doAnimation = false;
                    }

                    break;
                case 2: // INTEGRITY
                    healthModeTextRep.gameObject.SetActive(true);
                    healthModeNumber.gameObject.SetActive(true);

                    int cur = item.integrityCurrent, max = item.itemData.integrityMax;

                    // Bail out if current HP is 0. Unlikely that this will happen but good to be safe
                    if(cur <= 0)
                    {
                        healthModeTextRep.gameObject.SetActive(false);
                        healthModeNumber.gameObject.SetActive(false);
                        return;
                    }

                    // There can only be a max of 12 bars
                    string displayText = HF.ValueToStringBar(cur, max);
                    healthModeTextRep.text = displayText; // Set text (bars)
                    healthModeTextRep.color = healthDisplay.color; // Set color
                    healthModeNumber.text = item.integrityCurrent.ToString(); // Set text (numbers)
                    healthModeNumber.color = healthDisplay.color; // Set color
                    break;
                case 3: // INFO
                        // This is data about the part, no bars
                    healthModeTextRep.gameObject.SetActive(false);
                    healthModeNumber.gameObject.SetActive(true);

                    healthModeNumber.text = HF.HighlightDamageType(item.itemData.mechanicalDescription); // Set mechanical text & color damage type if it exists
                    break;
            }
        }

        // We also may need to do an animation
        if (doAnimation)
        {
            if(rd_animation != null)
            {
                StopCoroutine(rd_animation);
            }
            rd_animation = StartCoroutine(RightDataAnimation());
        }
    }

    private Coroutine rd_animation;
    private IEnumerator RightDataAnimation()
    {
        // We just flash the backer
        Image image = healthMode.GetComponent<Image>();

        image.enabled = true;

        // Just quickly flash from Black -> Green -> Black
        float elapsedTime = 0f;
        float duration = 0.5f;
        Color start = new Color(0f, 0f, 0f, 0f);
        Color middle = highlightColor;
        /*
        while (elapsedTime < duration) // Black -> Green
        {
            image.color = Color.Lerp(start, middle, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        */
        while (elapsedTime < duration) // Green -> Black
        {
            image.color = Color.Lerp(middle, start, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.enabled = false;
    }

    /// <summary>
    /// Happens when this item is first added to its respective menu. Basically just UIEnable but BLACK -> ENABLED
    /// </summary>
    public void InitialReveal()
    {
        // Set the item's state to enabled
        item.state = true;
        item.isOverloaded = false;
        item.siege = false;

        modeMain.SetActive(false); // Turn off the box

        // Do the text animation (BLACK -> Green)
        Color start = Color.white, end = Color.white, highlight = Color.white;
        string text = nameUnmodified; // (this also resets old mark & color tags)

        // We want to ENABLE the text, so going from GRAY -> GREEN
        start = Color.black;
        end = activeGreen;
        highlight = inActiveGreen;

        if (item.isBroken)
            end = UIManager.inst.highSecRed;
        if (isSecondaryItem)
            end = wideBlue;

        // Set the assigned letter to a color while we're a it
        assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(activeGreen)}>{assignedOrderString}</color>";
        

        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
        }

        // Melee Check
        MeleeCheck();

        // Update the UI
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
    }

    #region Highlight Animation

    private IEnumerator ActiveHighlightAnim(bool fadeIn)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;
        Color lerp = highlightColor;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Empty -> Green
            {
                float transparency = Mathf.Lerp(0, 0.7f, elapsedTime / duration);

                lerp = new Color(highlightColor.r, highlightColor.g, highlightColor.b, transparency);

                _highlight.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _highlight.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.7f);
        }
        else
        {
            while (elapsedTime < duration) // Green -> Empty
            {
                float transparency = Mathf.Lerp(0.7f, 0, elapsedTime / duration);

                lerp = new Color(highlightColor.r, highlightColor.g, highlightColor.b, transparency);

                _highlight.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _highlight.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.0f);
        }
    }

    private IEnumerator UnusedHighlightAnim(bool fadeIn)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;
        Color lerp = Color.white;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Empty -> White
            {
                lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);

                assignedOrderText.color = lerp;
                itemNameText.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            healthDisplayBacker.color = letterWhite;
        }
        else
        {
            while (elapsedTime < duration) // White -> Empty
            {
                lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);

                assignedOrderText.color = lerp;
                itemNameText.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            healthDisplayBacker.color = emptyGray;
        }
    }

    /// <summary>
    /// When called, briefly flashes the image behind the part health indicator from black -> green -> black (2.2s total).
    /// </summary>
    public void RecentAttachmentAnimation()
    {
        StartCoroutine(RAA());
    }

    private IEnumerator RAA()
    {
        healthDisplayBacker.enabled = true;
        healthDisplayBacker.color = Color.black;

        // Black -> Green (quick)
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Black -> Main Green
        {
            healthDisplayBacker.color = Color.Lerp(Color.black, inActiveGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        healthDisplayBacker.color = inActiveGreen;

        // Green -> Black
        elapsedTime = 0f;
        duration = 2f;
        while (elapsedTime < duration) // Black -> Main Green
        {
            healthDisplayBacker.color = Color.Lerp(inActiveGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        healthDisplayBacker.color = Color.black;
        healthDisplayBacker.enabled = false;
    }

    #endregion

    #region Interaction

    public void RightClick()
    {
        if(UIManager.inst.cTerminal_machine == null && UIManager.inst.terminal_targetTerm)
            UIManager.inst.Data_OpenMenu(item, null, PlayerData.inst.GetComponent<Actor>());
    }

    public void Click()
    {
        if ((my_interface != null && my_interface.GetComponent<StaticInterface>()) // We shouldn't toggle items in the inventory.
            || discardAnimationCoroutine != null // We should forbid toggling while in the middle of animating.
            || isSecondaryItem // Only lead items should be able to be toggled.
            || (item != null && item.isBroken) // Forbid broken items from being toggled
            || (item != null && item.disabledTimer > 0) // Forbid force disabled items from being toggled.
            || (UIManager.inst.cTerminal_machine != null && UIManager.inst.cTerminal_machine.type == CustomTerminalType.HideoutCache)) // Don't toggle items while in the cache inventory mode
        {
            return;
        }

        if(item != null)
        {
            // If the player clicks on this item, it should enable/disable the item itself.
            // Also they need to actually be able to toggle this. Some items forbid it.

            if (item.itemData.canBeDisabled)
            {
                if (item.state) // DISABLE the item
                {
                    UIDisable();
                }
                else // ENABLE the item
                {
                    if(item.itemData.canOverload && !item.isOverloaded) // Unless we can overload this item?
                    {
                        UIOverload();
                    }
                    else // Nope. Enable it
                    {
                        UIEnable();
                    }
                }
            }
            else
            {
                if (!item.state)
                {
                    UIManager.inst.ShowCenterMessageTop($"{item.itemData.itemName} cannot be disabled", UIManager.inst.dangerRed, Color.black);
                }
            }
        }
    }

    /// <summary>
    /// Briefly animate the cover that is on top of the item display. (or the cover under the health)
    /// </summary>
    public void FlashItemDisplay()
    {
        // Very cheeky workaround check here
        partDisplay.gameObject.SetActive(my_interface.GetComponent<StaticInterface>()); // Part display should only be on in the inventory window

        if (!isSecondaryItem)
        {
            if (partDisplay.gameObject.activeInHierarchy && partDisplay.enabled)
            {
                StartCoroutine(FlashItemDisplayAnim(partDisplayFlasher));
            }
            else
            {
                StartCoroutine(FlashItemDisplayAnim(healthDisplayBacker));
            }
        }
    }

    private IEnumerator FlashItemDisplayAnim(Image I)
    {
        // 1. Enable the display
        I.enabled = true;

        // 2. Set the color to dark green, with a bit of transparency
        float startTP = 0.6f;
        float endTP = 0f;
        float TP;
        I.color = new Color(inActiveGreen.r, inActiveGreen.g, inActiveGreen.b, startTP);

        // 3. Over a set period of time, lerp the transparency to our end value
        float elapsedTime = 0f;
        float duration = 3.5f;

        while (elapsedTime < duration)
        {
            TP = Mathf.Lerp(startTP, endTP, elapsedTime / duration);

            I.color = new Color(inActiveGreen.r, inActiveGreen.g, inActiveGreen.b, TP);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4. Disable the display
        I.enabled = false;
    }

    private IEnumerator SecondaryDataFlash()
    {
        // Take the secondary data backer image, enable it and set it to dark green, and fade it out.

        secondaryBacker.enabled = true;

        float elapsedTime = 0f;
        float duration = 0.5f;
        Color lerp = inActiveGreen;

        while (elapsedTime < duration)
        {
            lerp = Color.Lerp(inActiveGreen, Color.black, elapsedTime / duration);

            secondaryBacker.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        secondaryBacker.enabled = false;
    }

    private void TextTypeOutAnimation(bool disable)
    {
        Color start = Color.white, end = Color.white, highlight = Color.white;
        string text = item.itemData.itemName; // (this also resets old mark & color tags)

        // Assign values based on what we want to do
        if (disable)
        {
            // We want to DISABLE the text, so going from GREEN -> GRAY
            start = activeGreen;
            end = emptyGray;
            highlight = inActiveGreen;

            // Set the assigned letter to a color while we're at it
            assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(emptyGray)}>{assignedOrderString}</color>";
        }
        else
        {
            // We want to ENABLE the text, so going from GRAY -> GREEN
            start = emptyGray;
            end = activeGreen;
            highlight = inActiveGreen;

            // Set the assigned letter to a color while we're a it
            assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(activeGreen)}>{assignedOrderString}</color>";
        }

        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
        }

    }

    private Coroutine unusedHighlight;
    private Coroutine activeHighlight;
    public void HoverBegin()
    {
        if (item != null) // Active
        {
            if (activeHighlight != null)
            {
                StopCoroutine(activeHighlight);
            }
            _highlight.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.0f);
            activeHighlight = StartCoroutine(ActiveHighlightAnim(true));
        }
        else // Unused
        {
            if (unusedHighlight != null)
            {
                StopCoroutine(unusedHighlight);
            }
            healthDisplayBacker.color = emptyGray;
            unusedHighlight = StartCoroutine(UnusedHighlightAnim(true));
        }

        // Play the hover UI sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[48]); // HOVER
    }

    public void HoverEnd()
    {
        if (item != null) // Active
        {
            if (activeHighlight != null)
            {
                StopCoroutine(activeHighlight);
            }
            _highlight.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.7f);
            activeHighlight = StartCoroutine(ActiveHighlightAnim(false));
        }
        else // Unused
        {
            if (unusedHighlight != null)
            {
                StopCoroutine(unusedHighlight);
            }
            healthDisplayBacker.color = letterWhite;
            unusedHighlight = StartCoroutine(UnusedHighlightAnim(false));
        }
    }

    /// <summary>
    /// If double clicked on (in the inventory), we want to try and directly equip this item into an empty slot of its type. [This is triggered by UIHoverEvent.cs]
    /// </summary>
    public void TryDirectEquip()
    {
        if (my_interface.GetComponent<StaticInterface>() && canTryDirectEquip && !isSecondaryItem) // Inventory only
        {
            // Do the rest of the logic inside UserInterface.cs
            my_interface.TryDirectEquip(this.gameObject, item);

            // Enable a cooldown so this can't be spammed
            if (directEquipCooldown != null)
            {
                StopCoroutine(directEquipCooldown);
            }
            directEquipCooldown = StartCoroutine(directEquipCooldownIE());
        }
    }

    private bool canTryDirectEquip = true;
    private Coroutine directEquipCooldown = null;
    private IEnumerator directEquipCooldownIE()
    {
        canTryDirectEquip = false;
        yield return new WaitForSeconds(3.5f);
        canTryDirectEquip = true;
    }

    #endregion

    #region Cycling

    /// <summary>
    /// Cycle to being DISABLED
    /// </summary>
    public void UIDisable()
    {
        // Set the item's state to disabled
        item.state = false;
        item.isOverloaded = false;
        item.siege = false;

        // Turn off the box
        modeMain.SetActive(false);

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        TextTypeOutAnimation(true);

        // Play a sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[71]); // PART_OFF

        // Update the UI
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();

    }

    /// <summary>
    /// Cycle to being ENABLED
    /// </summary>
    public void UIEnable()
    {
        // Set the item's state to enabled
        item.state = true;
        item.isOverloaded = false;
        item.siege = false;

        modeMain.SetActive(false); // Turn off the box

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        TextTypeOutAnimation(false);

        // Play a sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[73]); // PART_ON

        // Update the UI
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();

        // -- Melee Check --
        MeleeCheck();
    }

    /// <summary>
    /// Cycle from being Enabled to being Force Disabled. Also start the timer
    /// </summary>
    public void UIForceDisabled(int timer)
    {
        // Set this item's timer
        item.disabledTimer = timer;

        // Enable the box
        UISetBoxDisplay($"DISABLED {timer}", UIManager.inst.warningOrange);

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        OverheatDisabledTransitionAnimation();

        // Update the UI
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
    }

    private void ForceDisabledCheck()
    {
        if(item.disabledTimer > 0 && modeMain.activeInHierarchy) // Need to update the number
        {
            UISetBoxDisplay($"DISABLED {item.disabledTimer}", UIManager.inst.warningOrange);
        }
        else if(item.disabledTimer <= 0 && modeMain.activeInHierarchy) // Need to get out of being disabled
        {
            UIEnable();
        }
    }

    /// <summary>
    /// Melee weapons are unique in that, only one can be active at the same time. We need to do some checks here.
    /// </summary>
    public void MeleeCheck()
    {
        // Pre-check incase this is being spamming
        if(item.itemData.meleeAttack.isMelee && modeMain.activeInHierarchy && modeText.text.Contains("MELEE")) // Box is already on
        {
            return;
        }

        if (item.itemData.meleeAttack.isMelee)
        {
            // Enable the box
            UISetBoxDisplay("MELEE", activeGreen);

            // Disable ALL other active weapons
            foreach (var I in InventoryControl.inst.interfaces)
            {
                if (I.GetComponentInChildren<DynamicInterface>()) // Includes all items found in /PARTS/ menus
                {
                    foreach (var item in I.GetComponentInChildren<DynamicInterface>().slotsOnInterface)
                    {
                        InvDisplayItem reference = item.Key.GetComponent<InvDisplayItem>();

                        if (reference.item != null && reference.item.Id >= 0 && reference != this && reference.item.state)
                        {
                            item.Key.GetComponent<InvDisplayItem>().UIDisable();
                        }
                    }
                }
            }
        }
        else
        {
            // We also need to check if there are any melee weapons active (if this is a non-melee weapon), cause they need to be disabled too.

            // Disable the box
            modeMain.SetActive(false);

            // Disable ALL other active melee weapons
            foreach (var I in InventoryControl.inst.interfaces)
            {
                if (I.GetComponentInChildren<DynamicInterface>()) // Includes all items found in /PARTS/ menus
                {
                    foreach (var item in I.GetComponentInChildren<DynamicInterface>().slotsOnInterface)
                    {
                        InvDisplayItem reference = item.Key.GetComponent<InvDisplayItem>();

                        if (reference != null && reference.item != null && reference.item.Id >= 0 && reference != this)
                        {
                            if (reference.item.itemData.meleeAttack.isMelee && reference.modeMain.activeInHierarchy)
                            {
                                reference.UIDisable();
                            }
                            
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Cycle from being Enabled to being Overloaded
    /// </summary>
    public void UIOverload()
    {
        // Set the item's flag
        item.isOverloaded = true;

        // Enable the box
        UISetBoxDisplay("OVERLOADED", UIManager.inst.slowOrange);

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        OverloadTransitionAnimation();

        // Play a sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[73]); // PART_ON

        // Update the UI
        UIManager.inst.UpdateInventory();
        UIManager.inst.UpdateParts();
    }

    private void OverloadTransitionAnimation()
    {
        // Go from active Green -> Yellow
        Color start = Color.white, end = Color.white, highlight = Color.white;
        string text = item.itemData.itemName; // (this also resets old mark & color tags)

        // GREEN -> YELLOW
        start = activeGreen;
        end = UIManager.inst.slowOrange;
        highlight = inActiveGreen;

        // Set the assigned letter to a color while we're at it
        assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{assignedOrderString}</color>";

        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
        }
    }

    private void OverheatDisabledTransitionAnimation()
    {
        // Go from active Green -> Orange
        Color start = Color.white, end = Color.white, highlight = Color.white;
        string text = item.itemData.itemName; // (this also resets old mark & color tags)

        // GREEN -> ORANGE
        start = activeGreen;
        end = UIManager.inst.warningOrange;
        highlight = UIManager.inst.corruptOrange;

        // Set the assigned letter to a color while we're at it
        assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{assignedOrderString}</color>";

        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
        }
    }

    public void UISetBoxDisplay(string display, Color backgroundColor)
    {
        modeMain.SetActive(true);

        modeText.text = display;
        modeImage.color = backgroundColor;
    }

    #region SIEGE
    public int siegeStartTurn = -1;
    [Tooltip("Possible states: 0 = Disabled, 1 = (begin) SIEGE, 2 = SIEGE, 3 = (end) SIEGE, 4 = Enabled")]
    public int siegeState = 0;
    Coroutine siegeTransition = null;

    /// <summary>
    /// Transition from one state to another in siege mode. 0 = Disabled, 1 = (begin) SIEGE, 2 = SIEGE, 3 = (end) SIEGE, 4 = Enabled
    /// </summary>
    /// <param name="state"></param>
    public void SiegeTransitionTo(int startState, int endState)
    {
        // We have multiple possible transition states here:
        // -Disabled -> (begin)    | 0
        // -(begin) -> SIEGE       | 1
        // -SIEGE -> (end)         | 2
        // -(end) -> Enabled       | 3

        int type = 0;

        if(startState == 0 && endState == 1) // DISABLED -> (begin)
        {
            type = 0;
            MapManager.inst.FreezePlayer(true);
            PlayerData.inst.timeTilSiege = 5;
            item.siege = true;

            // Play a sound
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[73]); // PART_ON
        }
        else if (startState == 1 && endState == 2) // (begin) -> SIEGE
        {
            type = 1;
            MapManager.inst.FreezePlayer(true);
            PlayerData.inst.timeTilSiege = 0;
            item.siege = true;

            // Play a sound
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.ITEMS_Clips[72]); // ITEMS/SIEGE_TREADS_ACTIVE
        }
        else if (startState == 2 && endState == 3) // SIEGE -> (end)
        {
            type = 2;
            MapManager.inst.FreezePlayer(true);
            PlayerData.inst.timeTilSiege = -1;
            item.siege = true;

            // Play a sound
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[71]); // PART_OFF
        }
        else if (startState == 3 && endState == 4) // (end) -> Enabled
        {
            type = 3;
            MapManager.inst.FreezePlayer(false);
            PlayerData.inst.timeTilSiege = 100;
            item.siege = false;

            // Play a sound
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.ITEMS_Clips[73]); // ITEMS/SIEGE_TREADS_END
        }

        StartCoroutine(SecondaryDataFlash()); // Flash the secondary

        if (siegeTransition != null)
        {
            StopCoroutine(siegeTransition);
        }
        siegeTransition = StartCoroutine(SiegeTransitionAnimation(type));
    }

    private IEnumerator SiegeTransitionAnimation(int type)
    {
        Color start = Color.white, end = Color.white, highlight = Color.white;
        string text = item.itemData.itemName; // (this also resets old mark & color tags)
        List<string> strings = new List<string>();
        float delay = 0;
        float perDelay = 0;

        switch (type)
        {
            case 0: // Disabled -> (begin) 
                UISetBoxDisplay("SIEGE 5", emptyGray); // Enable the box
                // Change the text color from gray to yellow using our animation

                // GRAY -> YELLOW
                start = emptyGray;
                end = UIManager.inst.slowOrange;
                highlight = inActiveGreen;

                // Set the assigned letter to a color while we're at it
                assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{assignedOrderString}</color>";

                // Get the string list
                strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

                // Animate the strings via our delay trick
                delay = 0f;
                perDelay = 0.35f / text.Length;

                foreach (string s in strings)
                {
                    StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
                }
                break;
            case 1: // (begin) -> SIEGE
                UISetBoxDisplay("SIEGE", UIManager.inst.slowOrange); // Alter the box
                // No actual animation here

                break;
            case 2: // SIEGE -> (end)
                UISetBoxDisplay("SIEGE -5", emptyGray); // Alter the box

                break;
            case 3: // (end) -> Enabled
                modeMain.SetActive(false); // Disable the box

                // Change the text color from yellow to green using our animation

                // YELLOW -> GREEN
                start = UIManager.inst.slowOrange;
                end = activeGreen;
                highlight = inActiveGreen;

                // Set the assigned letter to a color while we're at it
                assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(end)}>{assignedOrderString}</color>";

                // Get the string list
                strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

                // Animate the strings via our delay trick
                delay = 0f;
                perDelay = 0.35f / text.Length;

                foreach (string s in strings)
                {
                    StartCoroutine(HF.DelayedSetText(itemNameText, s, delay += perDelay));
                }
                break;
        }

        yield return null;
    }

    private void CheckSiegeStatus()
    {
        // Here we need to check and update the player's siege status

        // One of these is timeTilSiege, so we need to track turn time.
        if (siegeState == 1 || siegeState == 3) // Transition states
        {
            int siegeTime = PlayerData.inst.timeTilSiege;
            if (siegeTime < 0)
                siegeTime = SiegeNumAdjust(siegeTime);
            UISetBoxDisplay("SIEGE " + siegeTime, emptyGray); // Set the time

            // Check for time
            if (siegeState == 1) // waiting in (begin) state
            {
                if (TurnManager.inst.globalTime >= siegeStartTurn + 5)
                {
                    siegeState = 2;
                    siegeStartTurn = -1;

                    SiegeTransitionTo(1, 2); // Transition from (begin) to SIEGE
                }
            }
            else if(siegeState == 3) // waiting in (end) state
            {
                if (TurnManager.inst.globalTime >= siegeStartTurn + 5)
                {
                    siegeState = 4;
                    siegeStartTurn = -1;

                    SiegeTransitionTo(3, 4); // Transition from (end) to ENABLED
                }
            }
        }
    }

    // Needed due to the way PlayerData.inst.timeTilSiege is tracked and modified.
    private int SiegeNumAdjust(int input)
    {
        if(input == -1)
        {
            return -5;
        }
        else if (input == -2)
        {
            return -4;
        }
        else if (input == -3)
        {
            return -3;
        }
        else if (input == -4)
        {
            return -2;
        }
        else if (input == -5)
        {
            return -1;
        }

        return -1;
    }

    #endregion

    #endregion

    #region Forced Item Discarding
    // For things like processors that are destroyed upon being removed

    float discard_delay = 5f;
    public bool discard_readyToDestroy = false;
    Coroutine discardCoroutine = null;
    public Coroutine discardAnimationCoroutine = null;

    public void StartDiscardTimeout()
    {
        discard_readyToDestroy = true;
        DiscardWaitingVisual();

        if (discardCoroutine != null)
        {
            StopCoroutine(discardCoroutine);
        }
        discardCoroutine = StartCoroutine(DiscardCountdown());
    }

    private IEnumerator DiscardCountdown()
    {
        yield return new WaitForSeconds(discard_delay);

        if (this.gameObject != null) // Incase this object is discard & destroyed
        {
            DiscardSetAsNormal();
            discardCoroutine = null;
        }
    }

    // Set the visuals back to normal.
    public void DiscardSetAsNormal()
    {
        if(this.gameObject != null)
        {
            discard_readyToDestroy = false;

            if (discardAnimationCoroutine != null)
            {
                StopCoroutine(discardAnimationCoroutine);
            }

            if (item.state)
            {
                itemNameText.color = activeGreen;
                assignedOrderText.color = activeGreen;
            }
            else
            {
                itemNameText.color = emptyGray;
                assignedOrderText.color = emptyGray;
            }

            // Disabled the backer
            assignedOrderBacker.enabled = false;
        }
    }

    // Change the visuals to indicate this may be discarded.
    private void DiscardWaitingVisual()
    {
        if (discardAnimationCoroutine != null)
        {
            StopCoroutine(discardAnimationCoroutine);
        }
        discardAnimationCoroutine = StartCoroutine(DiscardWaitingAnimation());
    }

    public void DiscardForceStop()
    {
        if (discardAnimationCoroutine != null)
        {
            StopCoroutine(discardAnimationCoroutine);
        }

        if (discardCoroutine != null)
        {
            StopCoroutine(discardCoroutine);
        }

        assignedOrderBacker.enabled = false;
        itemNameText.color = emptyGray;
        assignedOrderText.color = emptyGray;
    }

    private IEnumerator DiscardWaitingAnimation()
    {
        // This is a continuous looping animation that lasts for 5 seconds. It heavily uses the color gray and slowly flashing images.

        // Start by setting the assigned :# to black text.
        assignedOrderText.color = Color.black;

        // Activate the backer behind the assigned :#
        assignedOrderBacker.enabled = true;

        // Set the backer and the main text to their starting color (letter white).
        string modifiedName = itemNameText.text;
        itemNameText.text = nameUnmodified; // Set name to its state but without any <color>'s
        assignedOrderBacker.color = letterWhite;
        itemNameText.color = letterWhite;

        // Now we want to flash both the :# backer AND the main text from emtpy grey to letter white.
        #region Flashing
        float elapsedTime;
        float duration;
        Color lerp = Color.white;

        // 1.
        elapsedTime = 0f;
        duration = 0.5f;
        while (elapsedTime < duration) // WHITE -> GRAY
        {
            lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) // GRAY -> WHITE
        {
            lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 2.
        elapsedTime = 0f;
        while (elapsedTime < duration) // WHITE -> GRAY
        {
            lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) // GRAY -> WHITE
        {
            lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 3.
        elapsedTime = 0f;
        while (elapsedTime < duration) // WHITE -> GRAY
        {
            lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) // GRAY -> WHITE
        {
            lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4.
        elapsedTime = 0f;
        while (elapsedTime < duration) // WHITE -> GRAY
        {
            lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) // GRAY -> WHITE
        {
            lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 5.
        elapsedTime = 0f;
        while (elapsedTime < duration) // WHITE -> GRAY
        {
            lerp = Color.Lerp(letterWhite, emptyGray, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) // GRAY -> WHITE
        {
            lerp = Color.Lerp(emptyGray, letterWhite, elapsedTime / duration);
            assignedOrderBacker.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        #endregion

        // Now that we're done, set it back to its original state (aka Disabled or Enabled visuals).
        Debug.Log("State: " + item.state);
        if (item.state)
        {
            itemNameText.color = activeGreen;
            assignedOrderText.color = activeGreen;
        }
        else
        {
            itemNameText.color = emptyGray;
            assignedOrderText.color = emptyGray;
        }
        itemNameText.text = modifiedName; // Reset name with <color>'s.

        // Disabled the backer
        assignedOrderBacker.enabled = false;

        discardAnimationCoroutine = null;
    }

    /// <summary>
    /// To be played when this item has just been discarded. Transitions to Unusued visuals.
    /// </summary>
    public void DiscardedAnimation(string key, string iName)
    {
        StartCoroutine(DiscardedAnim(key, iName));
    }

    private IEnumerator DiscardedAnim(string key, string iName)
    {
        // 1. Set both the indicator & name text do red, and temporarly reset the name & key to what they were.
        itemNameText.color = UIManager.inst.highSecRed;
        assignedOrderText.color = UIManager.inst.highSecRed;
        assignedOrderText.text = key;
        itemNameText.text = iName;

        // 2. Fade text to black
        Color lerp = Color.white;
        float elapsedTime = 0f;
        float duration = 1.2f;

        while (elapsedTime < duration) // RED -> BLACK
        {
            lerp = Color.Lerp(UIManager.inst.highSecRed, Color.black, elapsedTime / duration);
            assignedOrderText.color = lerp;
            itemNameText.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 3. Set as empty
        SetEmpty();
    }

    #endregion

    #region Secondary Item Related
    public void SetAsSecondaryItem(GameObject parent) // Called by its parent (doesn't have assignments yet)
    {
        isSecondaryItem = true;
        secondaryParent = parent;

        // We only want to display the order text, and name
        modeMain.SetActive(false);
        specialDescText.gameObject.SetActive(false);
        healthMode.gameObject.SetActive(false);

        // However we will display these (as black) so the text lines up
        if (!my_interface.GetComponent<DynamicInterface>())
        {
            partDisplay.enabled = false;
        }
        healthDisplay.enabled = false;
    }

    public void SecondaryCompleteSetup() // Called after the above function when being updated in UserInterface. Now it actually has its item assignment
    {
        // Set name
        nameUnmodified = item.itemData.itemName;
        if (!item.itemData.knowByPlayer)
        {
            nameUnmodified = HF.ItemPrototypeName(item);
        }
        itemNameText.text = nameUnmodified;
        this.gameObject.name = $"IDI: <DUPE> {item.itemData.itemName}";

        // Set color
        itemNameText.color = wideBlue;
        assignedOrderText.color = wideBlue;
    }
    #endregion

    #region Destroyed Animation
    public void DestroyAnimation()
    {
        StartCoroutine(DestroyedAnimation());

        // And destroy any duplicates too
        if (!isSecondaryItem)
        {
            foreach (GameObject D in secondaryChildren)
            {
                D.GetComponent<InvDisplayItem>().DestroyAnimation();
            }
        }
    }

    private IEnumerator DestroyedAnimation()
    {
        // Play a destroyed item sound
        if (!isSecondaryItem) // dont play multiple at the same time!
            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[85]); // UI/PT_LOST

        // Disabled the health indicator (but keep the spacing)
        healthDisplay.enabled = false;

        // Disabled the secondary detail & box
        modeMain.SetActive(false);
        specialDescText.gameObject.SetActive(false);

        // Disabled hover interaction (because we are going to use it's image)
        this.GetComponent<UIHoverEvent>().disabled = true;
        Color start = new Color(UIManager.inst.highSecRed.r, UIManager.inst.highSecRed.g, UIManager.inst.highSecRed.b, 1f);
        _button.gameObject.GetComponent<Image>().color = start; // Start at full red

        // Set the text(s) to black
        itemNameText.color = Color.black;
        assignedOrderText.color = Color.black;

        // Then slowly fade out the red over a period of 1 second
        float elapsedTime = 0f;
        float duration = 1f;
        float tp = 1f;

        while (elapsedTime < duration)
        {
            tp = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            _button.gameObject.GetComponent<Image>().color = new Color(start.r, start.b, start.g, tp);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // And then force a UI update, which will destroy this gameObject.
        UIManager.inst.UpdatePSUI();
        UIManager.inst.UpdateParts();
        InventoryControl.inst.UpdateInterfaceInventories();
    }

    #endregion

    #region Sorting

    public void Sort_StaggeredMove(Vector3 end, List<Vector3> positions, float delay = 0f)
    {
        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[70]); // UI | PART_SORT

        StartCoroutine(StaggeredMove(end, positions, delay));

        sort_letter = StartCoroutine(Sort_Letter(delay));
    }

    private IEnumerator StaggeredMove(Vector3 end, List<Vector3> positions, float delay = 0f)
    {
        // Delay if needed
        if(delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector3 originPosition = this.transform.position;

        // 1. Slide to the left
        float distance = 21f;
        float elapsedTime = 0f;
        float duration = 0.35f;

        while (elapsedTime < duration)
        {
            float adjustment = Mathf.Lerp(originPosition.x, originPosition.x - distance, elapsedTime / duration);

            this.transform.position = new Vector3(adjustment, this.transform.position.y, this.transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 2. Move up/down to new position (filled or not)
        float moveTime = 0.5f;

        #region Old
        /*
        // First determine the points we need to snap to while moving (since this isn't a smooth animation).
        List<Vector3> path = positions; // In this state, it goes from top to bottom.
        
        // Now we need to filter out the unneccessary positions and only use the ones we need (start, finish, and those in between)
        foreach (Vector3 P in path.ToList())
        {
            if(P == end || P == originPosition) // Start or end, keep it
            {
                // Keep it
            }
            else if (flip == 1) // Moving up
            {
                if(P.y > end.y) // Above our end point, remove it
                {
                    path.Remove(P);
                }
                else if (P.y < originPosition.y) // Below our end point, remove it
                {
                    path.Remove(P);
                }
            }
            else if(flip == -1) // Moving down
            {
                if (P.y > originPosition.y) // Above our end point, remove it
                {
                    path.Remove(P);
                }
                else if (P.y < end.y) // Below our end point, remove it
                {
                    path.Remove(P);
                }
            }
        }

        if (flip == 1) // Reverse dirction if needed
        {
            path.Reverse();
        }
        */
        #endregion

        #region Path Refinement
        List<Vector2> path = new List<Vector2>();

        // Get indices of current and target positions
        int currentIndex = positions.IndexOf(originPosition);
        int targetIndex = positions.IndexOf(end);

        // Determine the direction and add positions to the path
        if (currentIndex < targetIndex)
        {
            for (int i = currentIndex + 1; i <= targetIndex; i++)
            {
                path.Add(positions[i]);
            }
        }
        else if (currentIndex > targetIndex)
        {
            for (int i = currentIndex - 1; i >= targetIndex; i--)
            {
                path.Add(positions[i]);
            }
        }
        #endregion

        // Now move along these points over the time period we set
        foreach (Vector3 P in path.ToList()) // WHO is modifying this list? There is an error here for some reason???
        {
            float y = P.y;

            this.transform.position = new Vector3(this.transform.position.x, y, this.transform.position.z);
            yield return new WaitForSeconds(moveTime / path.Count); // don't want to take all day to do this so cut the speed by the amount of chunks we move
        }

        // 3. Slide to the right
        elapsedTime = 0f;
        duration = 0.35f;

        Vector2 start = this.transform.position;
        while (elapsedTime < duration)
        {
            float adjustment = Mathf.Lerp(start.x, start.x + distance, elapsedTime / duration);

            this.transform.position = new Vector3(adjustment, this.transform.position.y, this.transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4. Briefly flash the text
        elapsedTime = 0f;
        duration = 0.45f;

        Color startColor = Color.white;
        Color end_text = itemNameText.color;
        Color end_health = healthDisplay.color;
        Color end_rightside = healthModeTextRep.color;

        while (elapsedTime < duration) // White -> OG Colors
        {
            itemNameText.color = Color.Lerp(startColor, end_text, elapsedTime / duration);
            assignedOrderText.color = Color.Lerp(startColor, end_text, elapsedTime / duration);

            healthModeTextRep.color = Color.Lerp(startColor, end_rightside, elapsedTime / duration);
            healthModeNumber.color = Color.Lerp(startColor, end_rightside, elapsedTime / duration);
            specialDescText.color = Color.Lerp(startColor, end_rightside, elapsedTime / duration);

            healthDisplay.color = Color.Lerp(startColor, end_health, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 5. Stop the random letter shuffle
        if (sort_letter != null)
            StopCoroutine(sort_letter);

        // Unset the flag
        InventoryControl.inst.awaitingSort = false; // Maybe move this somewhere else?
    }

    private Coroutine sort_letter;
    private IEnumerator Sort_Letter(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        while (true) // Stopped when this coroutine is stopped.
        {
            assignedOrderText.text = chars[Random.Range(0, chars.Length - 1)].ToString();
            yield return null;
        }
    }

    #endregion
}
