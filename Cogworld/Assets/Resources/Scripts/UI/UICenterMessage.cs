using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UICenterMessage : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI _text;
    public Image backBar;

    public Color textColor;
    public Color backgroundColor;
    public string _message;

    [SerializeField] private float textSpeed = 0.5f;

    public void Setup(string message, Color setColorText, Color setColorBar)
    {
        _message = message;
        textColor = setColorText;
        backgroundColor = setColorBar;
    }

    public void Appear()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        // Play (typing) sound
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[77]); // PRINT_3

        _text.color = textColor;
        backBar.color = backgroundColor;

        int len = _message.Length;
        _text.text = "";
        for (int i = 0; i < len; i++)
        {
            _text.text += _message[i];

            yield return new WaitForSeconds(textSpeed * Time.deltaTime);
        }

        // When finished, stop playing the text sound
        AudioManager.inst.StopMiscSpecific();
    }
}
