using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIMouseBounds : MonoBehaviour
{
    public UnityEvent onMouseEnter;
    public UnityEvent onMouseLeave;

    private RectTransform rectTransform;
    private bool isMouseOver = false;
    public bool disabled = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!disabled)
        {
            // Check if mouse is over the UI element
            bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Mouse.current.position.ReadValue());

            // If mouse just entered the bounds
            if (!isMouseOver && mouseOver)
            {
                onMouseEnter.Invoke(); // Trigger onMouseEnter event
            }
            // If mouse just left the bounds
            else if (isMouseOver && !mouseOver)
            {
                onMouseLeave.Invoke(); // Trigger onMouseLeave event
            }

            isMouseOver = mouseOver; // Update mouseOver state
        }
    }
}
