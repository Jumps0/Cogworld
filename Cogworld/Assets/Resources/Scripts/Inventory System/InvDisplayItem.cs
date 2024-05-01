using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;
using static UnityEditor.Progress;

public class InvDisplayItem : MonoBehaviour
{
    [Header("UI References")]
    // -- UI References --
    //
    public TextMeshProUGUI assignedOrderText;
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
    public Image secondaryBacker;
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
    //
    // --        --

    [Header("Assignments")]
    public Item item;
    public char _assignedChar;
    public int _assignedNumber = -1;
    private bool canSiege = false;
    public UserInterface my_interface;

    private void Update()
    {
        if (canSiege)
        {
            CheckSiegeStatus();
        }
    }

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
        healthDisplay.gameObject.SetActive(false);
        modeMain.SetActive(false);
        //_button.gameObject.SetActive(false);
        specialDescText.gameObject.SetActive(false);
        healthMode.gameObject.SetActive(false);

        itemNameText.text = "Unused";
        this.gameObject.name = "IDI: Unused";
    }

    /// <summary>
    /// This slot has an item, and thus should display information about that item.
    /// </summary>
    public void SetAsFilled()
    {
        assignedOrderText.color = letterWhite;
        itemNameText.color = activeGreen;

        partDisplay.gameObject.SetActive(true);
        healthDisplay.gameObject.SetActive(true);
        modeMain.SetActive(true);
        //_button.gameObject.SetActive(true);
        specialDescText.gameObject.SetActive(true);
        healthMode.gameObject.SetActive(true);

        this.gameObject.name = $"IDI: {item.itemData.itemName}";
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
    public void SetLetter(char assignment, int number = -1)
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

            if (item.state)
            {
                assignedOrderText.color = activeGreen;
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
            if(item.itemData.type == ItemType.Treads)
            {
                if (item.itemData.propulsion[0].canSiege > 0)
                {
                    canSiege = true;
                }
            }
        }
    }

    public void UpdateDisplay()
    {
        if(item != null)
        {
            bool inInventory = false;
            if (this.transform.parent.gameObject == UIManager.inst.inventoryArea)
            {
                inInventory = true;
            }

            //if(_assignedItem != null)
            //    _part = InventoryControl.inst._itemDatabase.Items[_assignedItem.Id].data;

            // - Name - //
            itemNameText.text = item.itemData.itemName;

            // - Letter Assignment - //
            // ?

            if (inInventory)
            {
                partDisplay.gameObject.SetActive(true);

                // - Icon - //
                partDisplay.sprite = item.itemData.inventoryDisplay;

                // - Icon Color - //
                partDisplay.color = item.itemData.itemColor;
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
            #region Right Side Text
            if (UIManager.inst.inventoryDisplayMode == "I") // Integrity (both are used)
            {
                healthModeTextRep.gameObject.SetActive(true);
                healthModeNumber.gameObject.SetActive(true);

                // There can only be a max of 12 bars
                string displayText = "";
                float referenceValue = currentIntegrity * 12;
                float dummyValue = 12;
                while (dummyValue > (referenceValue / 12))
                {
                    displayText += "|";
                    dummyValue -= 1;
                }
                healthModeTextRep.text = displayText; // Set text (bars)
                healthModeTextRep.color = healthDisplay.color; // Set color
                healthModeNumber.text = item.integrityCurrent.ToString(); // Set text (numbers)
                healthModeNumber.color = healthDisplay.color; // Set color
            }
            else if (UIManager.inst.inventoryDisplayMode == "M") // Mass (this is bars AND %)
            {
                healthModeTextRep.gameObject.SetActive(true);
                healthModeNumber.gameObject.SetActive(true);
            }
            else // Type (This is data about the part, no bars)
            {
                healthModeTextRep.gameObject.SetActive(false);
                healthModeNumber.gameObject.SetActive(true);

                healthModeNumber.text = HF.HighlightDamageType(item.itemData.mechanicalDescription); // Set mechanical text & color damage type if it exists
            }
            #endregion

            // - Mode - //
            // Figure out if mode needs to be on or not
            modeMain.gameObject.SetActive(false);

            // - Special Extra Text - //
            // Figure out if it needs to be on or not
            specialDescText.gameObject.SetActive(false);

            // Melee check
            MeleeCheck();
        }
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
        string text = item.itemData.itemName; // (this also resets old mark & color tags)

        // We want to ENABLE the text, so going from GRAY -> GREEN
        start = Color.black;
        end = activeGreen;
        highlight = inActiveGreen;

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

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Right Click to open /DATA/ Menu
        {
            UIManager.inst.Data_OpenMenu(item, null, PlayerData.inst.GetComponent<Actor>());
        }
    }

    public void Click()
    {
        if (my_interface.GetComponent<StaticInterface>()) // No toggling items in the inventory!!!
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

        if (partDisplay.gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashItemDisplayAnim(partDisplayFlasher));
        }
        else
        {
            StartCoroutine(FlashItemDisplayAnim(healthDisplayBacker));
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
                        if (item.Key.GetComponent<InvDisplayItem>().item != null && item.Key.GetComponent<InvDisplayItem>() != this && item.Key.GetComponent<InvDisplayItem>().item.state)
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

                        if (reference.item != null && reference != this && reference.item.itemData.meleeAttack.isMelee && reference.modeMain.activeInHierarchy)
                        {
                            reference.UIDisable();
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
        UISetBoxDisplay("OVERLOADED", UIManager.inst.siegeYellow);

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
        end = UIManager.inst.siegeYellow;
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
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.ITEMS_Clips[66]); // ITEMS/SIEGE_TREADS_ACTIVE
        }
        else if (startState == 2 && endState == 3) // SIEGE -> (end)
        {
            type = 2;
            MapManager.inst.FreezePlayer(true);
            PlayerData.inst.timeTilSiege = -5;
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
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.ITEMS_Clips[67]); // ITEMS/SIEGE_TREADS_END
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
                end = UIManager.inst.siegeYellow;
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
                UISetBoxDisplay("SIEGE", UIManager.inst.siegeYellow); // Alter the box
                // No actual animation here

                break;
            case 2: // SIEGE -> (end)
                UISetBoxDisplay("SIEGE -5", emptyGray); // Alter the box

                break;
            case 3: // (end) -> Enabled
                modeMain.SetActive(false); // Disable the box

                // Change the text color from yellow to green using our animation

                // YELLOW -> GREEN
                start = UIManager.inst.siegeYellow;
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

        if(siegeState == 1 || siegeState == 3) // Transition states
        {
            UISetBoxDisplay("SIEGE " + PlayerData.inst.timeTilSiege, emptyGray); // Set the time

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
    #endregion

    #endregion
}
