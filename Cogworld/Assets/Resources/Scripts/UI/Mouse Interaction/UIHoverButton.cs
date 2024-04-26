using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script is largely unused, TODO: Migrate away to a different method.
/// </summary>
public class UIHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string identifier;

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (identifier)
        {
            case "l":
            case "i":
            case "a":
            case "c":
                UIManager.inst.SetActiveLAICMenu(identifier);
                break;
                break;

            default:
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switch (identifier)
        {
            case "l":
            case "i":
            case "a":
            case "c":
                //UIManager.inst.SetActiveLAICMenu(identifier);
                break;

            default:
                break;
        }
    }



    
}
