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
/// Script containing logic for the SETTINGS buttons in the settings menu on the main menu screen.
/// </summary>
public class MMButtonSettings : MonoBehaviour
{
    [Header("References")]
    public Image image_backer;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TextMeshProUGUI text_keybind;
    [SerializeField] private TextMeshProUGUI text_rightbracket;
    [SerializeField] private TextMeshProUGUI text_setting;

    [Header("Values")]
    public char character;
    private int myID;
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
        myID = id;
        character = MainMenuManager.inst.alphabet[myID];

        (currentSetting, title, explainer, options) = HF.ParseSettingsOption(myID);

        this.gameObject.name = $"{character} - {title}";

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
    public void Click()
    {
        // Inputfield?
        if (inputfield)
        {
            // Open the input field box
            IFOpen();
        }
        else
        {
            if (options.Count > 2)
            {
                // Open the Detail Window Box
                MainMenuManager.inst.DetailOpen(options, title, this);
            }
            else
            {
                // Swap the option
                foreach (var O in options)
                {
                    if(!CompareSSO(currentSetting, O.Item2))
                    {
                        AssignNewOption(O.Item2);
                        return;
                    }
                }
            }
        }
    }

    private Coroutine ano_co;
    private void AssignNewOption(ScriptableSettingShort option)
    {
        // Re-assign the current option
        currentSetting = option;

        // Replace the text
        text_setting.text = ValueToString(currentSetting);

        // Play a sound
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["OPTION"]); // UI - OPTION

        // Update the setting
        HF.UpdateSetting(myID, currentSetting);
        if(MainMenuManager.inst != null)
        {
            MainMenuManager.inst.ApplySettingsSimple();
        }
        else if (GlobalSettings.inst != null)
        {
            GlobalSettings.inst.ApplySettings();
        }

        // Do the animation
        if (ano_co != null)
        {
            StopCoroutine(ano_co);
        }
        ano_co = StartCoroutine(ANO_Animation());
    }

    private IEnumerator ANO_Animation()
    {
        // Relatively Simple [Black -> Bright Green]
        float elapsedTime = 0f;
        float duration = 0.45f;

        Color start = Color.black, end = color_bright;

        // Exception for grayed out
        if (currentSetting.canBeGrayedOut)
        {
            end = color_gray;
        }

        text_setting.color = start;
        while (elapsedTime < duration)
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            text_setting.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        text_setting.color = end;
    }

    public void ClickFromDetailBox(MMOptionSimple option)
    {
        // Change the setting
        AssignNewOption(option.setting);

        // Tell the box to close
        MainMenuManager.inst.DetailClose();
    }

    public void OnLeftClick(InputValue value)
    {
        // Close any open boxes (Input Field or Detail Window)
        // Maybe change this later? Race condition /w the detail box selections?
        // TODO
        if (MainMenuManager.inst.detail_main.activeInHierarchy)
        {
            //DetailClose();
        }
        else if (inputfield_main.activeInHierarchy)
        {
            //IFClose();
        }
    }

    #endregion

    #region InputField
    [Header("Input Field")]
    [SerializeField] private GameObject inputfield_main;
    [SerializeField] private TMP_InputField ifield;
    private Coroutine inputfield_co = null;

    public void OnEnter(InputValue value)
    {
        if (!inputfield_main.activeInHierarchy) { return; }

        // Save whatever is in the field (as long as it isn't empty)
        if(ifield.text != "")
        {

        }

        // Close the inputfield
        IFClose();
    }

    public void IFOpen()
    {
        inputfield_main.SetActive(true);

        if (inputfield_co != null)
        {
            StopCoroutine(inputfield_co);
        }
        inputfield_co = StartCoroutine(IFOpenAnimation());
    }

    private IEnumerator IFOpenAnimation()
    {
        yield return null;
    }

    public void IFClose()
    {
        inputfield_main.SetActive(false);

        if (inputfield_co != null)
        {
            StopCoroutine(inputfield_co);
        }
        inputfield_co = StartCoroutine(IFCloseAnimation());
    }

    private IEnumerator IFCloseAnimation()
    {
        yield return null;
    }
    #endregion

    #region Misc

    /// <summary>
    /// Given a ScriptableSettingShort, finds its matching display string in the options list.
    /// </summary>
    /// <param name="s">A ScriptableSettingShort with a single non-null value.</param>
    /// <returns>A string representing what that value should display.</returns>
    private string ValueToString(ScriptableSettingShort s)
    {
        string ret = "";

        foreach (var O in options)
        {
            if(CompareSSO(s, O.Item2))
            {
                ret = O.Item1;
                break;
            }
        }

        // If string is longer than 12 characters, shorten it by replacing the further characters with "..."
        if(ret.Length > 12)
        {
            ret = ret.Substring(0, 12) + "...";
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
