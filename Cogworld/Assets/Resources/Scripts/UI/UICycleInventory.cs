using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    #region Interaction
    public void Click()
    {

    }

    public void HoverBegin()
    {

    }

    public void HoverEnd()
    {

    }

    #endregion

    #region Animation
    private Coroutine buttonAnim;
    private IEnumerator ButtonHoverAnim(bool show)
    {
        yield return null;
    }
    #endregion
}
