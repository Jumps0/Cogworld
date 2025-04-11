using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICenterMessage : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI text;

    private Color textColor;
    private Color textHighlight;
    public string _message;

    public void Setup(string message, Color colorText, Color colorHighlight)
    {
        _message = message;
        textColor = colorText;
        textHighlight = colorHighlight;

        text.text = "";
    }

    public void Appear()
    {
        // Get the string list
        List<string> strings = HF.SteppedStringHighlightAnimation(_message, textHighlight, Color.black, textColor);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.15f / _message.Length;

        foreach (string s in strings)
        {
            StartCoroutine(HF.DelayedSetText(text, s, delay += perDelay));
        }

        // Play (typing) sound
        StartCoroutine(SoundTypingDelay(delay));
    }

    private IEnumerator SoundTypingDelay(float timeToPlay)
    {
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["PRINT_3"]); // UI - PRINT_3

        yield return new WaitForSeconds(timeToPlay);

        // When finished, stop playing the text sound
        AudioManager.inst.StopMiscSpecific();
    }
}
