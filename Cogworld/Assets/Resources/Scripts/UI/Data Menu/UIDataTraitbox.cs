using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDataTraitbox : MonoBehaviour
{
    [Header("References")]
    public Image image_titleBacker;
    public Image image_borders;
    public TextMeshProUGUI text_main;

    [Header("Colors")]
    public Color darkGreen;
    public Color green;
    public Color transparent;
    public Color highlightColor;
    public Color brightColor;

    public void Setup(string text)
    {
        text_main.text = text;
    }

    public void Open()
    {
        this.gameObject.SetActive(true);

        StartCoroutine(OpenAnim());
    }

    private IEnumerator OpenAnim()
    {
        StartCoroutine(OpenTitle());
        StartCoroutine(OpenBox());

        string primaryStart = text_main.text;
        int len = primaryStart.Length;

        float delay = 0f;
        float perDelay = 0.75f / len;
        text_main.text = "";

        List<string> segments = HF.StringToList(primaryStart);

        foreach (string segment in segments)
        {
            string s = segment;
            string last = HF.GetLastCharOfString(s);
            if (last == " ")
            {
                last = "_"; // Janky workaround because mark doesn't highlight spaces
            }

            if (s.Length > 0)
            {
                s = segment.Remove(segment.Length - 1, 1); // Remove the last character
                if (last == "_")
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{last}</color></mark>"; // Add it back with the highlight
                }
                else
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(brightColor)}>{last}</color></mark>"; // Add it back with the highlight
                }
            }

            StartCoroutine(HF.DelayedSetText(text_main, s, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        text_main.text = primaryStart;
    }

    private IEnumerator OpenTitle()
    {
        // Green -> Black
        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration)
        {
            Color color = Color.Lerp(green, Color.black, elapsedTime / duration);

            // Set the image color
            image_titleBacker.color = color;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        yield return null;
    }

    private IEnumerator OpenBox()
    {
        // Black -> Green -> Dark Green -> Green

        float elapsedTime = 0f;
        float duration = 0.1f;
        while (elapsedTime < duration)
        {
            Color color = Color.Lerp(Color.black, green, elapsedTime / duration);

            // Set the image color
            image_borders.color = color;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration)
        {
            Color color = Color.Lerp(green, darkGreen, elapsedTime / duration);

            // Set the image color
            image_borders.color = color;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration)
        {
            Color color = Color.Lerp(darkGreen, green, elapsedTime / duration);

            // Set the image color
            image_borders.color = color;

            elapsedTime += Time.deltaTime;

            yield return null;
        }



        yield return null;
    }

    public void Close()
    {
        // Nothing fancy!
        this.gameObject.SetActive(false);
    }
}
