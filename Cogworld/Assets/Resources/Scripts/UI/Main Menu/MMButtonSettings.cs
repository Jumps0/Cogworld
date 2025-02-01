// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the SETTINGS buttons in the settings menu on the main menu screen.
/// </summary>
public class MMButtonSettings : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_backer;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TextMeshProUGUI text_keybind;
    [SerializeField] private TextMeshProUGUI text_rightbracket;
    [SerializeField] private TextMeshProUGUI text_setting;

    [Header("Values")]
    public char character;
    public bool isGrayedOut = false; // TODO: How to determine this?
    public string explainer = "";

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_gray;

    public void Setup(string main, string setting, char character)
    {
        this.character = character;

        // Text layout is:
        // ? - [Option]         Current Setting
        //        ^ scramble text animation from black
        //                         ^  black to light green OR gray depending on the setting

        text_keybind.text = $"{this.character} - [";
        text_main.text = main;
        text_setting.text = setting;

        // Play the reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        // Heres how this animation works:
        // ? - [ and ] do nothing (they start as they are)
        text_keybind.color = color_main;
        text_rightbracket.color = color_main;

        // The actual settings option starts black, and goes through the scramble animation
        text_main.color = color_bright;
        List<string> strings = HF.RandomHighlightStringAnimation(text_main.text, color_main);
        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.25f / (text_main.text.Length);

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(text_main, s, delay += perDelay));
        }

        // and lastly, the actual option just goes from Black -> its set color
        Color start = Color.black, end = color_bright;

        if (isGrayedOut)
            end = color_gray;

        float elapsedTime = 0f;
        float duration = 0.45f;

        text_setting.color = start;

        while (elapsedTime < duration)
        {
            text_setting.color = Color.Lerp(start, end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        text_setting.color = end;
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

        MainMenuManager.inst.SettingsRevealExplainerText(explainer); // Update below explainer
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
    // -- THIS PROBABLY NEEDS TO BE REDONE
    public bool selected = false;

    public void Click()
    {
        MainMenuManager.inst.UnSelectButtons(this.gameObject);
        //MainMenuManager.inst.ButtonAction(myNumber);

        selected = true;
        Select(selected);
    }

    public void Select(bool select)
    {
        selected = select;

        // Animation
        StartCoroutine(SelectionAnimation(selected));
    }

    private IEnumerator SelectionAnimation(bool select)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;
        Color start = Color.white, end = Color.white;

        // Pretty simple here. Just change the color of the main text
        if (select) // This button needs to go from Normal Color -> Bright Color
        {
            start = color_main;
            end = color_bright;

            if(text_main.color == end) // Break out early if no change is needed
            {
                yield break;
            }
        }
        else // This button need to go from Bright Color -> Normal Color
        {
            start = color_bright;
            end = color_main;

            if (text_main.color == end) // Break out early if no change is needed
            {
                yield break;
            }
        }

        text_main.color = start;

        while (elapsedTime < duration)
        {
            text_main.color = Color.Lerp(start, end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        text_main.color = end;
    }

    #endregion

    #region Detail Window
    [Header("Detail Window")]
    [SerializeField] private GameObject detail_main;
    [SerializeField] private Image detail_borders;
    [SerializeField] private GameObject detail_area;

    public void DetailOpen()
    {

    }

    public void DetailClose()
    {

    }
    #endregion

}
