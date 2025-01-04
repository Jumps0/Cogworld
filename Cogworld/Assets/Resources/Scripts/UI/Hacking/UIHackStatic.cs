using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Color = UnityEngine.Color;


public class UIHackStatic : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI text;

    [Header("Values")]
    private float cyclerRate = 0.1f;
    public Color colorA;
    public Color colorB;
    private float runtime = 9999f;
    [SerializeField] private string startString;

    void Start()
    {
        startString = text.text;
    }

    private Coroutine coroutine;
    public void DoStatic()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(Static());
    }

    public void StopStatic()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        text.text = startString;
    }

    private IEnumerator Static()
    {
        float elapsedTime = 0f;
        float duration = runtime;
        while (elapsedTime < duration)
        {
            StaticCycle();

            yield return new WaitForSeconds(cyclerRate);
        }

        // When done, just set it all to black
        text.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{startString}</mark>";
    }

    private void StaticCycle()
    {
        string newString = "";

        for (int i = 0; i < startString.Length; i++)
        {
            Color color = RandomColorRange();
            newString += $"<mark=#{ColorUtility.ToHtmlStringRGB(color)}>{startString[i]}</mark>";
        }

        text.text = newString;
        Debug.Log(text.text);
        Debug.Log(newString);
    }

    private Color RandomColorRange()
    {
        Color newColor = Color.black;

        float green = Random.Range(colorA.g, colorB.g);

        newColor.g = green;

        return newColor;
    }
}
