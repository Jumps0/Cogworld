using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDataComparisonDetail : MonoBehaviour
{
    [Header("References")]
    public Image image_backer;
    public TextMeshProUGUI text_main;

    [Header("Colors")]
    public Color transparent;
    //
    public Color lowGreen;
    public Color lowRed;
    // Text
    public Color textGreen;
    public Color textRed;
    // Alt
    public Color gray;

    /// <summary>
    /// Assign values to this prefab.
    /// </summary>
    /// <param name="isGreen">Should the backing & text color be green? If false it will be red.</param>
    /// <param name="display">The text to display.</param>
    /// <param name="altDisplay">If true instead of a backing /w color it will display the string with gray ( ) and white text inside.</param>
    public void Setup(bool isGreen, string display, bool altDisplay = false)
    {
        // First off, is this just empty? (basically just a spacer)
        if(display == "EMPTY")
        {
            image_backer.gameObject.SetActive(false);
            text_main.gameObject.SetActive(false);
            this.gameObject.name += "SPACER";
        }
        else
        {
            if (altDisplay) // (string)
            {
                // Disable the backer
                image_backer.gameObject.SetActive(false);

                // Set the text
                string s = $"<color=#{ColorUtility.ToHtmlStringRGB(gray)}>{"("}</color><color=#{ColorUtility.ToHtmlStringRGB(Color.white)}>{display}</color><color=#{ColorUtility.ToHtmlStringRGB(gray)}>{")"}</color>";
                text_main.text = s;
                this.gameObject.name += s;
            }
            else
            {
                text_main.text = display;
                if (isGreen)
                {
                    image_backer.color = lowGreen;
                    text_main.color = textGreen;
                }
                else
                {
                    image_backer.color = lowRed;
                    text_main.color = textRed;
                }

                this.gameObject.name += display;
            }

            Appear();
        }
    }

    public void Appear()
    {
        StartCoroutine(AppearAnimation());
    }

    private IEnumerator AppearAnimation()
    {
        // Just a real basic fade-in here
        Color t = text_main.color;
        Color i = image_backer.color;

        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            text_main.color = Color.Lerp(transparent, t, elapsedTime / duration);
            image_backer.color = Color.Lerp(transparent, i, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

}
