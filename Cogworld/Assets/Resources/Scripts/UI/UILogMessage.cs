using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UILogMessage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI _text;
    public Image _hover;
    public string _message;
    public Color setColor;
    public Color highLightColor;

    bool hasAudioPlayout = false;

    public void Setup(string message, Color color, Color highlight, bool hasAudio)
    {
        this.GetComponent<RectTransform>().sizeDelta = (new Vector2(600, 300));

        // Should this happen in the other log too? (Only if it's active)
        if (UIManager.inst.currentActiveMenu_LAIC == "L" || UIManager.inst.currentActiveMenu_LAIC == "l")
        {
            if(UIManager.inst.calcMessages.Count > 0 && UIManager.inst.calcMessages[UIManager.inst.calcMessages.Count - 1]) // Failsafes to not cause a stackoverflow
            {
                if(UIManager.inst.calcMessages[UIManager.inst.calcMessages.Count - 1].GetComponent<UILogMessage>()._message != _message)
                {
                    UIManager.inst.CreateNewSecondaryLogMessage(message, color, highlight, false, false);
                }
            }
        }

        _message = message;
        setColor = color;
        highLightColor = highlight;
        _text.color = setColor;
        _hover.color = highLightColor;
        hasAudioPlayout = hasAudio;
        TypeOutAnimation();
    }

    public void TypeOutAnimation()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        if (hasAudioPlayout)
        {
            // Play (typing) sound
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["PRINT_2"]); // UI - PRINT_2
        }

        int len = _message.Length;

        float delay = 0f;
        float perDelay = GlobalSettings.inst.globalTextSpeed / len;

        _text.text = "";

        List<string> segments = HF.StringToList(_message);

        foreach (string segment in segments)
        {
            StartCoroutine(DelayedSetText(_text, segment, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        if (hasAudioPlayout)
        {
            // When finished, stop playing the text sound
            AudioManager.inst.StopMiscSpecific();
        }
        
    }

    private IEnumerator DelayedSetText(TextMeshProUGUI UI, string text, float delay)
    {
        yield return new WaitForSeconds(delay);

        UI.text = text;
    }

    public void HoverOn()
    {
        _hover.enabled = true;
    }

    public void HoverOff()
    {
        _hover.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverOn();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverOff();
    }
}
