// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the simple listen option seen in the settings menu when there are more than two possible settings.
/// </summary>
public class MMOptionSimple : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_back;
    [SerializeField] private TextMeshProUGUI text_main;

    [Header("Value")]
    public ScriptableSettingShort setting;
    private MMButtonSettings parent;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    public void Setup(string display, ScriptableSettingShort setting, MMButtonSettings parent)
    {
        this.parent = parent;
        this.setting = setting;
        text_main.text = display;

        // Play a little reveal animation
        RevealAnimation();
    }

    private void RevealAnimation()
    {
        List<string> strings = HF.RandomHighlightStringAnimation(text_main.text, color_bright);
        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.25f / (text_main.text.Length);

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(text_main, s, delay += perDelay));
        }
    }

    public void RemoveMe()
    {
        StartCoroutine(CloseAnimation());
    }

    private IEnumerator CloseAnimation()
    {
        float elapsedTime = 0f;
        float duration = 0.25f;

        Color start = Color.black, end = color_hover;

        string black = ColorUtility.ToHtmlStringRGB(Color.black);

        text_main.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(start)}aa><color=#{black}>{"["}</color></mark>";
        
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            string html = ColorUtility.ToHtmlStringRGB(lerp);

            text_main.text = $"<mark=#{html}aa><color=#{black}>{"["}</color></mark>";

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        text_main.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(end)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{"["}</color></mark>";

        yield return null;

        Destroy(this.gameObject);
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

    #region Click
    public void Click()
    {
        // Tell the option to change
        parent.ClickFromDetailBox(this);

        // Close the extra detail menu
        RemoveMe();

    }
    #endregion
}
