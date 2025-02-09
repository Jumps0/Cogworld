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
    public Image image_backer;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TextMeshProUGUI text_keybind;
    [SerializeField] private TextMeshProUGUI text_rightbracket;
    public TextMeshProUGUI text_setting;

    [Header("Values")]
    public char character;
    public int myID;
    public bool canBeGray = false;
    public bool inputfield = false;
    public ScriptableSettingShort currentSetting;
    public string title = "";
    public string explainer = "";
    [Tooltip("Is this option related to `SETTINGS` or `PREFERENCES`?")]
    public bool isSetting = true;
    public List<(string, ScriptableSettingShort)> options = new List<(string, ScriptableSettingShort)>();

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_gray;

    public void Setup()
    {


        // Play the reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        yield return null;
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

        MainMenuManager.inst.SetRevealExplainerText(explainer); // Update below explainer
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
            end = color_hover;
        }
        else
        {
            start = color_hover;
        }

        image_backer.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            image_backer.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_backer.color = end;
    }
    #endregion

    #region Selection
    public bool isSelected = false;

    public void Click()
    {
        // Select this option

        // Change the borders

        // Indicate to MainMenuMgr that this option is selected
        // (It will update all the other text stuff to the right of the \SAVED GAMES\ window

        // Also tell MainMenuMgr to deselect any other selected options
    }

    public void Select()
    {



    }

    public void DeSelect()
    {

    }
    #endregion

}
