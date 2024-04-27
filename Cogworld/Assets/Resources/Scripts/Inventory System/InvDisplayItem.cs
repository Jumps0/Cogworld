using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;

public class InvDisplayItem : MonoBehaviour
{
    [Header("UI References")]
    // -- UI References --
    //
    public TextMeshProUGUI assignedOrderText;
    public Image partDisplay;
    public Image healthDisplay;
    public Image healthDisplayBacker;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI specialDescText;
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
        _button.gameObject.SetActive(false);
        specialDescText.gameObject.SetActive(false);
        healthMode.gameObject.SetActive(false);

        itemNameText.text = "Unused";
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
        _button.gameObject.SetActive(true);
        specialDescText.gameObject.SetActive(true);
        healthMode.gameObject.SetActive(true);
    }

    public void ForceIgnoreRaycasts()
    {
        assignedOrderText.raycastTarget = false;
        partDisplay.raycastTarget = false;
        healthDisplay.raycastTarget = false;
        itemNameText.raycastTarget = false;
        specialDescText.raycastTarget = false;
        _button.gameObject.SetActive(false);
        modeImage.raycastTarget = false;
        modeText.raycastTarget = false;
        healthModeNumber.raycastTarget = false;
        healthModeTextRep.raycastTarget = false;
    }

    public void SetLetter(char assignment)
    {
        _assignedChar = assignment;
        assignedOrderText.text = _assignedChar.ToString();
        if (item.state)
        {
            assignedOrderText.color = activeGreen;
        }
        else
        {
            assignedOrderText.color = emptyGray;
        }

        // If this item is in the player's inventory we don't show letters, we show numbers
        if(this.transform.parent.gameObject == UIManager.inst.inventoryArea)
        {
            int position = char.ToUpperInvariant(assignment) - 'A' + 1; // Convert from letter to number
            //_assignedNumber = position;
            //assignedOrderText.text = position.ToString(); // NOTE: DOING THIS BREAKS STUFF. LOOK INTO HOW THE INTERFACE/UIMANAGER SETS THIS AND FIX IT TO HAVE 2 SYSTEMS.
        }
        else
        {
            // Also add an ":" before the indicator if removing it will destroy the item
            if (item.itemData.destroyOnRemove)
            {
                assignedOrderText.text = ":" + assignedOrderText.text;
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
        }
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
        if(item != null)
        {
            // If the player clicks on this item, it should enable/disable the item itself.
            if (item.state) // DISABLE the item
            {
                UIDisable();
            }
            else // ENABLE the item
            {
                UIEnable();
            }
        }
    }

    private void UIDisable()
    {
        // Set the item's state to disabled
        item.state = false;

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        TextTypeOutAnimation(true);

        // Play a sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[62]); // PART_OFF

        // Update the UI

    }

    private void UIEnable()
    {
        // Set the item's state to enabled
        item.state = true;

        // Do a little animation
        StartCoroutine(SecondaryDataFlash()); // Flash the secondary
        TextTypeOutAnimation(false);

        // Play a sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[64]); // PART_ON

        // Update the UI

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

            // Set the assigned letter to a color while we're a it
            assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(emptyGray)}>{assignedOrderText.text}</color>";
        }
        else
        {
            // We want to ENABLE the text, so going from GRAY -> GREEN
            start = emptyGray;
            end = activeGreen;
            highlight = inActiveGreen;

            // Set the assigned letter to a color while we're a it
            assignedOrderText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(activeGreen)}>{assignedOrderText.text}</color>";
        }

        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.75f / text.Length;

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
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[44]); // HOVER
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
}
