using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDataHeader : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI mainText;
    private string mainString;
    //
    public TextMeshProUGUI bonusText;
    public GameObject bonusParent;

    [Header("Colors")]
    public Color normalColor;
    public Color highlightColor;
    public Color brightColor;
    public Color darkGreen;

    public void Setup(string text, string bonusString = "")
    {
        StopAllCoroutines();

        mainString = text;
        mainText.text = mainString;

        this.gameObject.name = "[Header]: " + text;

        if(bonusString != "")
        {
            bonusParent.SetActive(true);
            bonusText.text = bonusString;
        }
    }

    public void Open()
    {
        StartCoroutine(AnimateOpen());
    }

    private IEnumerator AnimateOpen()
    {
        string primaryStart = mainText.text;
        int len = primaryStart.Length;

        float delay = 0f;
        float perDelay = 0.5f / len;
        mainText.text = "";

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

            StartCoroutine(HF.DelayedSetText(mainText, s, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        mainText.text = primaryStart;
    }

    public void Close()
    {
        StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateClose()
    {
        string oldText = mainText.text;
        string oldB = bonusText.text;

        float elapsedTime = 0f;
        float duration = 0.45f;
        while (elapsedTime < duration) // Dark green -> Black
        {
            Color color = Color.Lerp(darkGreen, Color.black, elapsedTime / duration);

            mainText.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{oldText}</mark>";

            if (bonusParent.gameObject.activeInHierarchy)
            {
                bonusText.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{oldB}</mark>";
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
