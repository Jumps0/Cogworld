using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*
     * CAUTION:
     * The PointerEvents appear to not only include the parent area for detection but also any children.
     * This can be bad if the child is larger than the parent, use UIMouseBounds instead if this is the case.
     */

    [Header("Events")]
    public UnityEvent onHoverStart;
    public UnityEvent onHoverEnd;

    [Header("Variables")]
    public bool disabled = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!disabled)
            onHoverStart.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!disabled)
            onHoverEnd.Invoke();
    }

    public void DebugTestMessage(string message)
    {
        Debug.Log(this.gameObject.name + " / " + message);
    }
}
