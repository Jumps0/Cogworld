using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The small display name that appears when first seeing an item/bot. Unused?
/// </summary>
public class HoverIdentity : MonoBehaviour
{
    [Header("References")]
    // - References -
    //
    public TextMeshProUGUI _text;
    //
    // -            -

    public bool belongsToBot; // True = Bot, False = Item
    //public BotObject _bot;
    public ItemObject _item;

    public void DoDisplay()
    {

    }

    public IEnumerator DisplayTempTime()
    {
        yield return null;
    }

    public void DisplayPerma()
    {

    }

    public void DisplayOff()
    {

    }
}
