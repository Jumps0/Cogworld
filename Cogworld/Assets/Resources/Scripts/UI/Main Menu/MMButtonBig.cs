// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the BIG buttons seen in the NEW, CONTINUE, LOAD, and JOIN game windows.
/// </summary>
public class MMButtonBig : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_back1;
    [SerializeField] private Image image_back2;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TextMeshProUGUI text_number;

    [Header("Values")]
    public string character;
    [Tooltip("What happens when this button is actually clicked? Tells MainMenuMgr what to do.")]
    private string specification;
    [Tooltip("Instead is the multiplayer host button.")]
    private bool asMulti = false;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_main_b;
    [SerializeField] private Color color_hover_b;
    [SerializeField] private Color color_bright_b;

    public void Setup(string display, string character, string specification, bool asMulti = false)
    {
        this.character = character;
        this.specification = specification;
        this.asMulti = asMulti;

        text_number.text = $"[{character}]";

        text_main.text = display;

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

        if (asMulti)
        {
            start = color_hover_b;
            end = color_main_b;
        }

        float elapsedTime = 0f;
        float duration = 0.45f;

        image_back1.color = start;
        image_back2.color = start;

        while (elapsedTime < duration)
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            image_back1.color = lerp;
            image_back2.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image_back1.color = end;
        image_back2.color = end;
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
        float elapsedTime = 0f;
        float duration = 0.25f;

        Color start = color_main, end = color_main;
        if (asMulti)
        {
            start = color_main_b;
            end = color_main_b;
        }

        if (fadeIn)
        {
            end = color_bright;
            if (asMulti)
            {
                end = color_bright_b;
            }
        }
        else
        {
            start = color_bright;
            if (asMulti)
            {
                start = color_bright_b;
            }
        }

        image_back1.color = start;
        image_back2.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            image_back1.color = lerp;
            image_back2.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_back1.color = end;
        image_back2.color = end;
    }
    #endregion

    #region Click
    public void Click()
    {
        if (asMulti)
        {
            MainMenuManager.inst.StartGameMultiplayer(specification);
        }
        else
        {
            MainMenuManager.inst.StartGame(specification);
        }
    }
    #endregion

}
