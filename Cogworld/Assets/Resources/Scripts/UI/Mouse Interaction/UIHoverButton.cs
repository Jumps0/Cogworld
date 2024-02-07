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

            case "INV_ITEM":
                // Turn on hover animation
                if(this.GetComponent<InvDisplayItem>()._assignedItem != null)
                    this.GetComponent<InvDisplayItem>().DoHighlight();
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

            case "INV_ITEM":
                // Turn off hover animation
                this.GetComponent<InvDisplayItem>().StopHighlight();
                break;

            default:
                break;
        }
    }



    
}
