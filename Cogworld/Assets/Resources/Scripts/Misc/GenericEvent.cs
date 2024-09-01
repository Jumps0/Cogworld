using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GenericEvent
{
    public event System.Action onTurnTick;

    /// <summary>
    /// Happens whenever a turn has just ended.
    /// </summary>
    public void TurnTick()
    {
        if (onTurnTick != null)
            onTurnTick();
    }
}
