using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISimpleCycle : MonoBehaviour
{
    public string replace_text;

    [Header("References")]
    public Image image_backer;
    public TextMeshProUGUI text_main;

    [Header("Colors")]
    public Color darkGreen; // Normal "CYCLE" color
    public Color normalGreen; // For brackets
    public Color headerWhite; // Highlighted "CYCLE" color

    public void HoverBegin()
    {
        if (buttonAnim != null)
        {
            StopCoroutine(buttonAnim);
        }
        text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(darkGreen)}>{replace_text}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";
        buttonAnim = StartCoroutine(ButtonHoverAnim(true));

        // Play the hover UI sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_ui["HOVER"]); // UI - HOVER
    }

    public void HoverEnd()
    {
        if (buttonAnim != null)
        {
            StopCoroutine(buttonAnim);
        }
        text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(headerWhite)}>{replace_text}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";
        buttonAnim = StartCoroutine(ButtonHoverAnim(false));
    }

    private Coroutine buttonAnim;
    private IEnumerator ButtonHoverAnim(bool fadeIn)
    {
        // For this animation, the brackets stay the same color (normalGreen)
        // While the "CYCLE" text lerps between darkGreen and headerWhite

        float elapsedTime = 0f;
        float duration = 0.25f;
        Color lerp = normalGreen;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Dark Green -> Header White
            {
                lerp = Color.Lerp(darkGreen, headerWhite, elapsedTime / duration);

                text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>{replace_text}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (elapsedTime < duration) // Header White -> Dark Green
            {
                lerp = Color.Lerp(headerWhite, darkGreen, elapsedTime / duration);

                text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>{replace_text}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
