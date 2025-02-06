// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;

/// <summary>
/// Script containing logic for the input fields on the main menu
/// </summary>
public class MMInputField : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField field;
    [SerializeField] private AudioSource source;
    public MMButtonSettings setting;

    [Header("Values")]
    private string startString = "";
    private AudioClip clip;
    private int myID = -1;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1f);

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_text;

    private void Start()
    {
        MainMenuManager.inst.settings_ifield.onValueChanged.AddListener(OnInputFieldValueChanged);
        clip = AudioManager.inst.dict_ui["KEYIN"]; // UI - KEYIN
    }

    private void Update()
    {
        if (this.gameObject.activeInHierarchy && field != null)
        {
            SetFocus(true);
            field.caretPosition = field.text.Length;

            if (field.isFocused)
            {
                ForceMeshUpdates();
            }

            SetFocus(true);
            field.caretPosition = field.text.Length;
        }
    }

    public void Open(MMButtonSettings caller, string start)
    {
        field = MainMenuManager.inst.settings_ifield;

        setting = caller;
        myID = caller.myID;

        startString = start;
        field.text = startString;
    }

    public void Enter()
    {
        string text = field.text;

        // Save whatever is in the field (as long as it isn't empty)
        if (text != "")
        {
            setting.currentSetting.value_string = text;

            // Conditional, if this is the <SEED> setting (currently ID 27), setting it to 0 should set it to be blank instead.
            if(text == "0")
            {
                setting.currentSetting.value_string = "";
            }
        }
        else // Invalid? Don't change anything
        {
            text = startString;
        }

        // Update the setting
        HF.UpdateSetting(myID, setting.currentSetting);
        if (MainMenuManager.inst != null)
        {
            MainMenuManager.inst.ApplySettingsSimple();
        }
        else if (GlobalSettings.inst != null)
        {
            GlobalSettings.inst.ApplySettings();
        }

        // (MainMenuMgr will close the window directly after this)
    }

    private void OnInputFieldValueChanged(string value) // Place a sound every time a character is altered
    {
        // Sounds
        source.pitch = Random.Range(pitchRange.x, pitchRange.y); // Randomize pitch so it sounds distinct each time
        source.PlayOneShot(clip, 1f);

        // Prevent from going over a certain amount of characters
        if(field.text.Length > 18)
        {
            field.text = field.text.Substring(0, 18);
        }

        // Update the secret filler text
        if(field.text.Length > 8)
        {
            MainMenuManager.inst.settings_if_headerfill.text = field.text;
        }
    }

    #region Misc Helpers
    public void SetFocus(bool active)
    {

        if (active)
        {
            field.Select();
            field.ActivateInputField();
        }
        else
        {
            field.ReleaseSelection();
            field.DeactivateInputField();
        }

    }

    public void ForceMeshUpdates()
    {
        field.textComponent.ForceMeshUpdate();
    }
    #endregion
}
