// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the buttons on the Main Menu
/// </summary>
public class MMButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_hover;
    [SerializeField] private TextMeshProUGUI text_main;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    void Start()
    {
        // Play a little reveal animation
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
        image_hover.color = new Color(color_hover.r, color_hover.g, color_hover.b, 0.0f);
        hover_co = StartCoroutine(HoverAnimation(true));

        // Play the hover UI sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_ui["HOVER"]); // UI - HOVER
    }

    public void HoverEnd()
    {
        if (hover_co != null)
        {
            StopCoroutine(hover_co);
        }
        image_hover.color = new Color(color_hover.r, color_hover.g, color_hover.b, 0.7f);
        hover_co = StartCoroutine(HoverAnimation(false));
    }

    private IEnumerator HoverAnimation(bool fadeIn)
    {
        float elapsedTime = 0f;
        float duration = 0.45f;
        Color lerp = color_hover;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Empty -> Green
            {
                float transparency = Mathf.Lerp(0, 0.7f, elapsedTime / duration);

                lerp = new Color(color_hover.r, color_hover.g, color_hover.b, transparency);

                image_hover.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            image_hover.color = new Color(color_hover.r, color_hover.g, color_hover.b, 0.7f);
        }
        else
        {
            while (elapsedTime < duration) // Green -> Empty
            {
                float transparency = Mathf.Lerp(0.7f, 0, elapsedTime / duration);

                lerp = new Color(color_hover.r, color_hover.g, color_hover.b, transparency);

                image_hover.color = lerp;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            image_hover.color = new Color(color_hover.r, color_hover.g, color_hover.b, 0.0f);
        }
    }
    #endregion

    #region Selection
    public bool selected = false;

    public void Select(bool select)
    {
        // Animation
        StartCoroutine(SelectionAnimation(selected));

        if (select) // This button needs to go from unselected -> selected
        {
            
        }
        else // This button need to go from unselected -> selected
        {

        }
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
        }
        else // This button need to go from Bright Color -> Normal Color
        {
            start = color_bright;
            end = color_main;
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

}
