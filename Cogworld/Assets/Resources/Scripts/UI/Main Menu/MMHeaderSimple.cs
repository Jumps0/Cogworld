// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the simple headers seen in the settings menu.
/// </summary>
public class MMHeaderSimple : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_back;
    [SerializeField] private TextMeshProUGUI text_main;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    public void Setup(string display)
    {
        text_main.text = display;

        // Play a little reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        float delay = 0.1f;

        Color usedColor = color_bright;

        float headerValue = 0.25f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        headerValue = 0.75f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        headerValue = 1f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        headerValue = 0.75f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        headerValue = 0.25f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return new WaitForSeconds(delay);

        headerValue = 0f;

        image_back.color = new Color(usedColor.r, usedColor.g, usedColor.b, headerValue);

        yield return null;

        image_back.color = Color.black;
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

        // Close the extra detail menu

        // Play a sound

    }
    #endregion
}
