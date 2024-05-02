using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Used for when an item is being moved around in the inventory.
/// (Player holding down click on it)
/// </summary>
public class InvMovingDisplayItem : MonoBehaviour
{
    public Color backColor;
    public Color redColor;
    public Image backImage;
    public TextMeshProUGUI _text;

    public void Setup(string name, bool useRed = false)
    {
        _text.text = name;
        _text.color = Color.black;
        backImage.color = backColor;
        this.GetComponentInParent<Canvas>().sortingOrder = 40;

        if (useRed)
        {
            backImage.color = redColor;
        }
    }

    public void SetUnRaycast()
    {
        _text.raycastTarget = false;
        backImage.raycastTarget = false;
    }
}
