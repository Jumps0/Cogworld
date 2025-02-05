// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the "BIG" options at the top of the settings menu (basically headers).
/// </summary>
public class MMOptionBig : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_back;
    [SerializeField] private TextMeshProUGUI text_main;

    [Header("Values")]
    public string display = "";
    public bool chosen = false;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    public void Setup(string display, bool startAsChosen = false)
    {
        text_main.text = display;
        this.display = display;

        this.gameObject.name = $"{display.ToUpper()}";

        chosen = startAsChosen;

        // Play a little reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        // We will animate the backer AND the text (random reveal)

        List<string> strings = HF.RandomHighlightStringAnimation(text_main.text, Color.black);
        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.25f / (text_main.text.Length);

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(text_main, s, delay += perDelay));
        }

        // Dark Green -> Final Color
        Color start = color_hover, end = color_main;
        if (chosen)
            end = color_bright;

        float elapsedTime = 0f;
        float duration = 0.45f;

        image_back.color = start;

        while (elapsedTime < duration)
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            image_back.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image_back.color = end;
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
    }

    private IEnumerator HoverAnimation(bool fadeIn)
    {
        if (chosen) { yield break; } // No hover for chosen

        float elapsedTime = 0f;
        float duration = 0.45f;

        Color start = Color.black, end = Color.black;

        if (fadeIn)
        {
            start = color_main;
            end = color_bright;
        }
        else
        {
            start = color_bright;
            end = color_main;
        }

        image_back.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            image_back.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_back.color = end;
    }
    #endregion

    #region Interaction
    public void Click()
    {
        // Tell MainMenuMgr to close all the options and that this one was clicked
        MainMenuManager.inst.SettingsBigOptionClicked(this);
    }

    public void SetAsChosen()
    {
        chosen = true;

        // Set color backer to the active color
        image_back.color = color_bright;
    }

    public void SetAsUnchosen()
    {
        chosen = false;

        // Set color backer to the inactive color
        image_back.color = color_main;
    }
    #endregion
}
