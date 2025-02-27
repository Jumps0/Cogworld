// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// Script containing logic for the game save prefabs seen on the main menu.
/// </summary>
public class MMSavegame : MonoBehaviour
{
    // !!!!!!!!!!!!!!!!!!!!!!! TODO !!!!!!!!!!!!!!!!!!!

    [Header("References")]
    [SerializeField] private Image preview_image;
    [SerializeField] private Image borders;
    [SerializeField] private TextMeshProUGUI text_name;
    [SerializeField] private TextMeshProUGUI text_location;
    [SerializeField] private TextMeshProUGUI text_status;
    [SerializeField] private TextMeshProUGUI text_slots;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_gray;
    private string hex_blue = "<color=#00C7FF>";
    private string hex_purple = "<color=#CE00FF>";
    private string hex_white = "<color=#FFFFFF>";
    private string hex_cap = "</color>";

    [Header("TEMP!!! Save Data")] // TODO: Replace this later with the single data object!!!
    private string data_name;
    private string data_location;
    //
    private Vector2Int data_core;
    private Vector2Int data_energy;
    private Vector2Int data_matter;
    private Vector2Int data_corruption;
    //
    private Vector2Int data_powerslots;
    private Vector2Int data_propslots;
    private Vector2Int data_utilslots;
    private Vector2Int data_wepslots;
    //
    private List<ItemObject> data_items;
    private int data_maxInv;
    private List<string> data_conditions;
    //
    private int data_kills;
    //
    private Sprite data_image;

    public void Setup()
    {
        // TODO: Replace this with actual save data that will get loaded (and fed through this setup function)
        (data_name, data_location, data_core, data_energy, data_matter, data_corruption, data_powerslots, data_propslots, data_utilslots, data_wepslots, (data_items, data_maxInv), data_conditions, data_kills, data_image) = HF.DummyPlayerSaveData();

        // Update the display text
        text_name.text = data_name;
        text_location.text = $"LOC: {data_location}";
        text_status.text = $"STATUS: {data_core.x}/{data_energy.x}/{data_matter.x}/{data_corruption.x}";
        text_slots.text = $"SLOTS:{data_powerslots.y}/{data_propslots.y}/{data_utilslots.y}/{data_wepslots.y} INV:{data_items.Count}/{data_maxInv}";

        // Set the image
        preview_image.sprite = data_image;

        // Play the reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        // !! Start with grayed out borders !!
        float elapsedTime = 0f;
        float duration = 0.45f;

        Color start = Color.black, end = color_gray;
        // Text starts out as Black -> Bright
        Color endT = color_bright;

        borders.color = start;
        text_name.color = start;
        text_location.color = start;
        text_status.color = start;
        text_slots.color = start;
        preview_image.color = new Color(1f, 1f, 1f, 0f); // Preview image will do transparency instead

        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);
            float lerpF = Mathf.Lerp(0f, 1f, elapsedTime / duration);

            borders.color = lerp;
            text_name.color = lerp;
            text_location.color = lerp;
            text_status.color = lerp;
            text_slots.color = lerp;
            preview_image.color = new Color(1f, 1f, 1f, lerpF);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        borders.color = end;
        text_name.color = endT;
        text_location.color = endT;
        text_status.color = endT;
        text_slots.color = endT;
        preview_image.color = new Color(1f, 1f, 1f, 1f);

        // Not the most pleased by this
        text_status.text = $"STATUS: {data_core.x}/{hex_blue}{data_energy.x}{hex_cap}/{hex_purple}{data_matter.x}{hex_cap}/{hex_white}{data_corruption.x}{hex_cap}";
    }

    #region Hover
    private Coroutine hover_co;
    public void HoverBegin()
    {
        if (hover_co != null)
        {
            StopCoroutine(hover_co);
        }
        hover_co = StartCoroutine(HoverAnimation(true));

        // Play the hover UI sound
        MainMenuManager.inst.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["HOVER"], 0.7f); // UI - HOVER
    }

    public void HoverEnd()
    {
        if (hover_co != null)
        {
            StopCoroutine(hover_co);
        }
        hover_co = StartCoroutine(HoverAnimation(false));
        MainMenuManager.inst.SettingsHideExplainer();
    }

    private IEnumerator HoverAnimation(bool fadeIn)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;

        Color start = Color.black, end = Color.black;

        if (fadeIn)
        {
            if (isSelected)
            {
                // Bright -> Hover
                start = color_bright;
                end = color_hover;
            }
            else
            {
                // Gray -> Main
                start = color_gray;
                end = color_main;
            }
        }
        else
        {
            if (isSelected)
            {
                // Hover -> Main
                start = color_gray;
                end = color_main;
            }
            else
            {
                // Main -> Gray
                start = color_main;
                end = color_gray;
            }
        }

        borders.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            borders.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        borders.color = end;
    }
    #endregion

    #region Selection
    public bool isSelected = false;

    public void Click()
    {
        // Select this option
        Select();
    }

    public void Select()
    {
        isSelected = true;

        // Change the borders
        borders.color = color_bright;

        // Indicate to MainMenuMgr that this option is selected
        // (It will update all the other text stuff to the right of the \SAVED GAMES\ window
        // Also tell MainMenuMgr to deselect any other selected options
        MainMenuManager.inst.LSelectSave(this, (data_name, data_location, data_core, data_energy, data_matter, data_corruption, data_powerslots, data_propslots, data_utilslots, data_wepslots, (data_items, data_maxInv), data_conditions, data_kills, data_image));

    }

    public void DeSelect()
    {
        isSelected = false;

        // Change the borders
        borders.color = color_gray;
    }
    #endregion


}
