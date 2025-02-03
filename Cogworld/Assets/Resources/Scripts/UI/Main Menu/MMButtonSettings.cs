// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;
using UnityEngine.InputSystem;
using System.Linq;

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
    public bool canBeGray = false;
    public bool inputfield = false;
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
        if(currentSetting.canBeGrayedOut)
        {
            canBeGray = currentSetting.canBeGrayedOut;
        }

        inputfield = currentSetting.inputfield;

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

        // Check if the current option needs to be gray instead
        if (canBeGray)
        {
            foreach(var O in options) // <- Shouldn't be too bad performance wise
            {
                if(CompareSSO(currentSetting, O.Item2) && O.Item2.canBeGrayedOut) // Is the current setting the one that appears gray?
                {
                    end = color_gray;
                    break;
                }
            }
        }

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
        // Inputfield?
        if (inputfield)
        {
            // Open the input field box
            // TODO
        }
        else
        {
            if (options.Count > 2)
            {
                // Open the Detail Window Box
                // TODO
            }
            else
            {
                // Swap the option
                // TODO
            }
        }

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

    public void OnLeftClick(InputValue value)
    {
        // Close any open boxes (Input Field or Detail Window)
        // TODO
    }

    #endregion

    #region Detail Window
    [Header("Detail Window")]
    [SerializeField] private GameObject detail_main;
    [SerializeField] private Image detail_borders;
    [SerializeField] private Image detail_headerBack;
    [SerializeField] private TextMeshProUGUI detail_header;
    [SerializeField] private Transform detail_area;
    [SerializeField] private GameObject detail_prefab;
    [SerializeField] private List<GameObject> detail_objects = new List<GameObject>();
    private Coroutine detail_co = null;

    public void DetailOpen()
    {
        detail_main.SetActive(true);

        // Populate the menu with the options we need
        foreach(var O in options)
        {
            string text = O.Item1;
            ScriptableSettingShort setting = O.Item2;

            GameObject newOption = Instantiate(detail_prefab, Vector2.zero, Quaternion.identity, detail_area);


        }

        // Opener animation
        if(detail_co != null)
        {
            StopCoroutine(detail_co);
        }
        detail_co = StartCoroutine(DetailOpenAnimation());
    }

    private IEnumerator DetailOpenAnimation()
    {
        // We need to:
        // 1. Animate the header (and its backer)
        // 2. Animate the borders
        // 3. Do the random revealing highlights for every option's text element

        // TODO
        yield return null;
    }

    public void DetailClose()
    {
        if (detail_co != null)
        {
            StopCoroutine(detail_co);
        }
        detail_co = StartCoroutine(DetailCloseAnimation());
    }

    private IEnumerator DetailCloseAnimation()
    {
        // TODO

        yield return null;





        // Destroy all the objects
        foreach (GameObject obj in detail_objects.ToList())
        {
            Destroy(obj);
        }
        detail_objects.Clear();

        // Disable the box (no animation)
        detail_main.SetActive(false);
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
        bool potentialOverride = true;
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
            potentialOverride = false;
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
            potentialOverride = false;
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
            potentialOverride = false;
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
            potentialOverride = false;
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
            potentialOverride = false;
        }

        // Might need to override the display string if the option specifies not only a value but ALSO a string.
        if (potentialOverride && s.value_string != null)
        {
            ret = s.value_string;
        }

        return ret;
    }

    /// <summary>
    /// Given two ScriptableSettingShort(s) from the same setting, returns True/False if these are the same setting (based on their internal value).
    /// </summary>
    /// <param name="A">A ScriptableSettingShort</param>
    /// <param name="B">A ScriptableSettingShort</param>
    /// <returns>True/False if these are the "same" setting.</returns>
    private bool CompareSSO(ScriptableSettingShort A,  ScriptableSettingShort B)
    {
        bool ret = false;

        if (A.value_bool != null && B.value_bool != null)
        {
            ret = A.value_bool == B.value_bool;
        }
        else if (A.value_int != null && B.value_int != null)
        {
            ret = A.value_int == B.value_int;
        }
        else if (A.value_float != null && B.value_float != null)
        {
            ret = A.value_float == B.value_float;
        }
        else if (A.value_string != null && B.value_string != null)
        {
            ret = A.value_string == B.value_string;
        }
        else if (A.enum_fov != null && B.enum_fov != null)
        {
            ret = A.enum_fov == B.enum_fov;
        }
        else if (A.enum_difficulty != null && B.enum_difficulty != null)
        {
            ret = A.enum_difficulty == B.enum_difficulty;
        }
        else if (A.enum_fullscreen != null && B.enum_fullscreen != null)
        {
            ret = A.enum_fullscreen == B.enum_fullscreen;
        }
        else if (A.enum_modal != null && B.enum_modal != null)
        {
            ret = A.enum_modal == B.enum_modal;
        }

        return ret;
    }

    #endregion
}
