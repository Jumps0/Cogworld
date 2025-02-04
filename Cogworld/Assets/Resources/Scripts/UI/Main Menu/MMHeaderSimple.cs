// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// Script containing logic for the simple headers seen in the settings menu.
/// </summary>
public class MMHeaderSimple : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_line;
    [SerializeField] private TextMeshProUGUI text_main;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;

    public void Setup(string display)
    {
        text_main.text = display;

        this.gameObject.name = $"- {display} -";

        // Play a little reveal animation
        StartCoroutine(RevealAnimation());
    }

    private IEnumerator RevealAnimation()
    {
        // This is relatively simple
        // 1. Display text goes from Black -> Dark Green
        // 2. Line goes from Bright Green -> Dark Green

        float elapsedTime = 0f;
        float duration = 0.45f;

        Color startL = color_bright, startT = Color.black, end = color_hover;

        image_line.color = startL;
        text_main.color = startT;

        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerpL = Color.Lerp(startL, end, elapsedTime / duration);
            Color lerpT = Color.Lerp(startT, end, elapsedTime / duration);

            image_line.color = lerpL;
            text_main.color = lerpT;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_line.color = end;
        text_main.color = end;
    }
}
