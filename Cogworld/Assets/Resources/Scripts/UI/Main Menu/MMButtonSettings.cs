// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

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
    public bool isGrayedOut = false;
    public ScriptableSettingShort currentSetting;
    public string title = "";
    public string explainer = "";
    public List<(string, ScriptableSettingShort)> options = new List<(string, ScriptableSettingShort)>();

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_gray;

    public void Setup(int id)
    {
        character = MainMenuManager.inst.alphabet[id];

        (currentSetting, title, explainer, options) = HF.ParseSettingsOption(id);

        // Text layout is:
        // ? - [Option]         Current Setting
        //        ^ scramble text animation from black
        //                         ^  black to light green OR gray depending on the setting

        // If its a bool and it's false it should be gray
        if(currentSetting.value_bool != null)
        {
            isGrayedOut = !(bool)currentSetting.value_bool;
        }

        text_keybind.text = $"{this.character} - [";
        text_main.text = title;

        MainMenuManager.inst.SettingsRevealExplainerText(explainer);

        text_setting.text = ValueToString(currentSetting);

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

    #region Misc

    /// <summary>
    /// Given a ScriptableSettingShort, parses whatever its value is into a string that can be displayed.
    /// </summary>
    /// <param name="s">A ScriptableSettingShort with a single non-null value.</param>
    /// <returns>A string representing what that value should display.</returns>
    private string ValueToString(ScriptableSettingShort s)
    {
        string ret = "";

        if(s.value_bool != null)
        {
            if(s.value_bool == true)
            {
                ret = "On";
            }
            else
            {
                ret = "Off";
            }
        }
        else if(s.value_int != null)
        {
            // No change
            ret = s.value_int.ToString();
        }
        else if (s.value_float != null)
        {
            // A bit of parsing
            float f = (float)(s.value_float * 100f);
            int fi = (int)f;
            ret = fi.ToString();

        }
        else if (s.value_string != null)
        {
            // No change
            ret = s.value_string;
        }
        else if (s.enum_fov != null)
        {
            switch (s.enum_fov)
            {
                case FOVHandling.Delay:
                    ret = "Delay";
                    break;
                case FOVHandling.Instant:
                    ret = "Instant";
                    break;
                case FOVHandling.FadeIn:
                    ret = "Fade In";
                    break;
                case null:
                    break;
                default:
                    break;
            }
        }
        else if (s.enum_difficulty != null)
        {
            switch (s.enum_difficulty)
            {
                case Difficulty.Explorer:
                    ret = "Explorer";
                    break;
                case Difficulty.Adventurer:
                    ret = "Adventurer";
                    break;
                case Difficulty.Rogue:
                    ret = "Rogue";
                    break;
                case null:
                    break;
                default:
                    break;
            }
        }
        else if (s.enum_fullscreen != null)
        {
            switch (s.enum_fullscreen)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    ret = "Borderless Fullscreen";
                    break;
                case FullScreenMode.FullScreenWindow:
                    ret = "True Fullscreen";
                    break;
                case FullScreenMode.MaximizedWindow:
                    ret = "Windowed";
                    break;
                case FullScreenMode.Windowed:
                    ret = "Windowed";
                    break;
                case null:
                    break;
            }
        }
        else if (s.enum_modal != null)
        {
            switch (s.enum_modal)
            {
                case ModalUILayout.NonModal:
                    ret = "Non-modal";
                    break;
                case ModalUILayout.SemiModal:
                    ret = "Semi-modal";
                    break;
                case ModalUILayout.Modal:
                    ret = "Modal";
                    break;
                case null:
                    break;
            }
        }

        return ret;
    }

    #endregion
}
