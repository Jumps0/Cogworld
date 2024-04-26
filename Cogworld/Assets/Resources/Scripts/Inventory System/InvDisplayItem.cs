using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;

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
    //
    public GameObject modeMain;
    public Image modeImage;
    public TextMeshProUGUI modeText;
    //
    public GameObject healthMode;
    public TextMeshProUGUI healthModeNumber; // The right side "##"
    public TextMeshProUGUI healthModeTextRep; // The bars
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
    // --        --

    public Item item;
    public char _assignedChar;

    [Header("Animation")]
    public Image _highlight;
    public Animator _highlightAnim;

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
    private Coroutine unusedHighlight;
    public void HoverBegin()
    {
        if(item != null)
        {
            DoHighlight();
        }
        else
        {
            if(unusedHighlight != null)
            {
                StopCoroutine(unusedHighlight);
            }
            healthDisplayBacker.color = emptyGray;
            unusedHighlight = StartCoroutine(UnusedHighlightAnim(true));
        }
    }

    public void HoverEnd()
    {
        if (item != null)
        {
            StopHighlight();
        }
        else
        {
            if (unusedHighlight != null)
            {
                StopCoroutine(unusedHighlight);
            }
            healthDisplayBacker.color = letterWhite;
            unusedHighlight = StartCoroutine(UnusedHighlightAnim(false));
        }
    }


    public void DoHighlight()
    {
        _highlight.gameObject.SetActive(true);
        _highlightAnim.Play("item_highlight");

    }

    public void StopHighlight()
    {
        _highlight.gameObject.SetActive(false);
    }

    private IEnumerator UnusedHighlightAnim(bool fadeIn)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;
        Color lerp = Color.white;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Emtpy -> White
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

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Right Click to open /DATA/ Menu
        {
            UIManager.inst.Data_OpenMenu(item, null, PlayerData.inst.GetComponent<Actor>());
        }
    }
}
