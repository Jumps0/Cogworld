using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;
using static UnityEditor.Progress;

/// <summary>
/// Used on the "Cycle" buttons in the inventory UI.
/// </summary>
public class UICycleInventory : MonoBehaviour
{
    [Header("References")]
    public Image image_backer;
    public TextMeshProUGUI text_main;

    [Header("Values")]
    public ItemSlot type;

    [Header("Colors")]
    public Color darkGreen; // Normal "CYCLE" color
    public Color normalGreen; // For brackets
    public Color headerWhite; // Highlighted "CYCLE" color

    public void Cycle()
    {
        List<InvDisplayItem> elements = new List<InvDisplayItem>();

        // Find all items this section has control over
        foreach (var I in InventoryControl.inst.interfaces)
        {
            if (I.GetComponentInChildren<DynamicInterface>())
            {
                foreach (var item in I.GetComponentInChildren<DynamicInterface>().slotsOnInterface)
                {
                    InvDisplayItem reference = null;

                    if (item.Key.GetComponent<InvDisplayItem>().item != null)
                    {
                        reference = item.Key.GetComponent<InvDisplayItem>();
                        if(reference != null && reference.item != null && reference.item.itemData.slot == type) // Does it match our slot type?
                        {
                            elements.Add(reference);
                        }
                    }
                }
            }
        }

        // Tell all the elements in our list to cycle.
        foreach (var E in elements)
        {
            // TODO: Check if we can cycle to SIEGE or OVERLOAD modes too

            if (E.item.state) // Cycle OFF
            {
                E.UIDisable();
            }
            else // Cycle ON
            {
                E.UIEnable();
            }
        }
    }

    #region Interaction
    public void Click()
    {
        Cycle();

        // Suprisingly we don't actually play a sound on click
    }

    public void HoverBegin()
    {
        if (buttonAnim != null)
        {
            StopCoroutine(buttonAnim);
        }
        text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(darkGreen)}>{"CYCLE"}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";
        buttonAnim = StartCoroutine(ButtonHoverAnim(true));

        // Play the hover UI sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[44]); // HOVER
    }

    public void HoverEnd()
    {
        if (buttonAnim != null)
        {
            StopCoroutine(buttonAnim);
        }
        text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(headerWhite)}>{"CYCLE"}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";
        buttonAnim = StartCoroutine(ButtonHoverAnim(false));
    }

    #endregion

    #region Animation
    private Coroutine buttonAnim;
    private IEnumerator ButtonHoverAnim(bool fadeIn)
    {
        // For this animation, the brackets stay the same color (normalGreen)
        // While the "CYCLE" text lerps between darkGreen and headerWhite

        float elapsedTime = 0f;
        float duration = 0.45f;
        Color lerp = normalGreen;

        if (fadeIn)
        {
            while (elapsedTime < duration) // Dark Green -> Header White
            {
                lerp = Color.Lerp(darkGreen, headerWhite, elapsedTime / duration);

                text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>{"CYCLE"}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (elapsedTime < duration) // Header White -> Dark Green
            {
                lerp = Color.Lerp(headerWhite, darkGreen, elapsedTime / duration);

                text_main.text = $"<color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"["}</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>{"CYCLE"}</color><color=#{ColorUtility.ToHtmlStringRGB(normalGreen)}>{"]"}</color>";

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
    #endregion
}
