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
    [SerializeField] private Image image_back1;
    [SerializeField] private Image image_back2;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TextMeshProUGUI text_number;
    [SerializeField] private Image fill_line;

    [Header("Values")]
    public int myNumber;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    public void Setup(string display, int number)
    {
        myNumber = number;

        text_number.text = $"[<color=#00CC00>{myNumber}</color>]";

        text_main.text = display;

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
        hover_co = StartCoroutine(HoverAnimation(true));

        // Play the hover UI sound
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HOVER"]); // UI - HOVER
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

    #region Selection
    public bool selected = false;

    public void Click()
    {
        MainMenuManager.inst.UnSelectButtons(this.gameObject);

        selected = true;
        Select(selected);
    }

    public void Select(bool select)
    {
        selected = select;

        // Animation
        StartCoroutine(SelectionAnimation(selected));

        Fill(selected);
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

    #region Fill Line
    private float fill_speed = 0.25f;
    private Coroutine fill_co;
    private void Fill(bool extend)
    {
        if(fill_co != null)
        {
            StopCoroutine(fill_co);
        }
        fill_co = StartCoroutine(FillAnimation(extend));
    }

    private IEnumerator FillAnimation(bool extend)
    {
        float start = 0f, end = 0f;

        if (extend)
        {
            start = 0f;
            end = 1f;

            if(fill_line.fillAmount == 1f) // Stop early
            {
                yield break;
            }
        }
        else
        {
            start = 1f;
            end = 0f;

            if (fill_line.fillAmount == 0f) // Stop early
            {
                yield break;
            }
        }

        float elapsedTime = 0f;
        float duration = fill_speed;

        fill_line.fillAmount = start;

        while (elapsedTime < duration)
        {
            fill_line.fillAmount = Mathf.Lerp(start, end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fill_line.fillAmount = end;
    }

    #endregion

}
