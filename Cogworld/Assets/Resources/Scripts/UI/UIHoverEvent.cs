using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIHoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Events")]
    public UnityEvent onHoverStart;
    public UnityEvent onHoverEnd;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverStart.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverEnd.Invoke();
    }



    
}
