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

        // Go through the list and determine what we should do.
        bool allEnabled = true, allDisabled = true, sieging = false, inSiege = false, overloaded = false, canSiege = false, canOverload = false;
        foreach (var E in elements)
        {
            // Enabled / Disabled
            if (E.item.state)
            {
                allDisabled = false;
            }
            else
            {
                allEnabled = false;
            }

            // Overloaded
            if (E.item.isOverloaded)
            {
                overloaded = true;
            }
            if (E.item.itemData.canOverload)
            {
                canOverload = true;
            }

            // Siege
            if(E.item.itemData.propulsion.Count > 0 && E.item.itemData.propulsion[0].canSiege > 0)
            {
                canSiege = true;
                if (E.item.siege)
                {
                    inSiege = true;
                }
            }
        }

        if(PlayerData.inst.timeTilSiege > 0 && PlayerData.inst.timeTilSiege <= 5) // Player is transitioning to siege mode, they should be unable to bail out
        {
            sieging = true;
        }

        // Now that we have determine the state of our items, we decide what to do, and apply it to all of them.
        int choice = 0; // 0 = Disable all | 1 = Enable all | 2 = Enter Siege | 3 = Exit Siege | 4 = Enter Overload | 5 = Do nothing
        if(!allDisabled && !allEnabled) // We have a mix of enabled/disable. Here we disable everything that is currently enabled.
        {
            choice = 0;
        }
        else if (allDisabled) // If everything is disabled, we want to enable everything.
        {
            if (canSiege) // Unless we can enter siege mode?
            {
                choice = 2;
            }
            else // No siege, enable all.
            {
                choice = 1;
            }
        }
        else if (allEnabled) // If everything is enabled, we want to turn everything off.
        {
            if (canOverload) // Unless we can overload some items?
            {
                choice = 4;
            }
            else // No overload, disable all.
            {
                choice = 0;
            }
        }
        if (overloaded) // One or more items are overloaded, we need to disable everything.
        {
            choice = 0;
        }
        if (sieging) // We are trying to enter siege, do nothing
        {
            choice = 5;
        }
        if (inSiege) // Are we actively in siege, we should be able to bail out
        {
            choice = 3;
        }

        foreach (var E in elements)
        {
            // NOTE: Don't forget to check the current states of items! And if items can do the thing we want them to do.
            switch (choice)
            {
                case 0: // DISABLE all
                    if (E.item.state)
                    {
                        E.modeMain.SetActive(false); // Just incase its overloaded
                        E.item.isOverloaded = false; //

                        E.UIDisable();
                    }
                    break;
                case 1: // ENABLE all
                    if (!E.item.state)
                    {
                        E.UIEnable();
                    }
                    break;
                case 2: // Enter SIEGE
                    E.siegeStartTurn = TurnManager.inst.globalTime; // Set the start time
                    E.siegeState = 1; // Set the flag

                    E.SiegeTransitionTo(0, 1); // Begin transition
                    break;
                case 3: // Exit SIEGE
                    E.siegeState = 3; // Set the flag
                    E.siegeStartTurn = TurnManager.inst.globalTime; // Set the start time

                    E.SiegeTransitionTo(2, 3); // Begin transition
                    break;
                case 4: // Overload
                    E.UIOverload();
                    break;
                case 5: // Do nothing

                    break;
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
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[48]); // HOVER
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
        float duration = 0.25f;
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
