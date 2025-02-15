using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This is assigned to an empty GameObject parent'd to the player. This GameObject will follow the players mouse, snapping to the nearest square.
/// Used for things like the launcher targeting indicator.
/// </summary>
public class MouseTracker : MonoBehaviour
{
    void Update()
    {
        if (PlayerData.inst)
        {
            /* // !TEMP-REMOVE
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePosition = new Vector3(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));
            this.transform.position = mousePosition;
            */
        }
    }
}
