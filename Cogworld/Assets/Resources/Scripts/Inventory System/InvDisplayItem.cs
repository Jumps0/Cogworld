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
    public Image _highlight;
    public Animator _highlightAnim;
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

    public Item _assignedItem;
    //public Item _part;
    public char _assignedChar;

    public void SetEmpty()
    {
        assignedOrderText.color = emptyGray; // Set assigned Letter to Gray
        itemNameText.color = emptyGray; // Set Name to Gray

        // Turn everything else off
        partDisplay.gameObject.SetActive(false);
        healthDisplay.gameObject.SetActive(false);
        modeMain.SetActive(false);
        _button.gameObject.SetActive(false);
        specialDescText.gameObject.SetActive(false);
        healthMode.gameObject.SetActive(false);

        itemNameText.text = "Unused";
    }

    public void SetUnEmpty()
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

    public void SetUnRaycast()
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
        if (_assignedItem.state)
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
        //if(_assignedItem != null)
        //    _part = InventoryControl.inst._itemDatabase.Items[_assignedItem.Id].data;

        // - Name - //
        itemNameText.text = _assignedItem.itemData.itemName;

        // - Letter Assignment - //
        // ?

        // - Icon - //
        partDisplay.sprite = _assignedItem.itemData.inventoryDisplay;

        // - Icon Color - //
        partDisplay.color = _assignedItem.itemData.itemColor;

        // - Integrity Color Square - //
        float currentIntegrity = (float)_assignedItem.integrityCurrent / (float)_assignedItem.itemData.integrityMax;
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
        if(UIManager.inst.inventoryDisplayMode == "I") // Integrity (both are used)
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
            healthModeNumber.text = _assignedItem.integrityCurrent.ToString(); // Set text (numbers)
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

        }

        // - Mode - //
        // Figure out if mode needs to be on or not
        modeMain.gameObject.SetActive(false);

        // - Special Extra Text - //
        // Figure out if it needs to be on or not
        specialDescText.gameObject.SetActive(false);

    }

    #region Highlight Animation

    public void DoHighlight()
    {
        _highlight.gameObject.SetActive(true);
        _highlightAnim.Play("item_highlight");

    }

    public void StopHighlight()
    {
        _highlight.gameObject.SetActive(false);
    }


    #endregion

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Right Click to open /DATA/ Menu
        {
            if (!UIManager.inst.dataMenu.data_parent.gameObject.activeInHierarchy)
            {
                UIManager.inst.Data_OpenMenu(_assignedItem);
            }
        }
    }
}
