// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class QuestEvents : MonoBehaviour
{
    public event System.Action onItemCollected;
    public event System.Action onLocationReached;

    public void ItemCollected()
    {
        if(onItemCollected != null)
            onItemCollected();
    }

    public void LocationReached()
    {
        if (onLocationReached != null)
            onLocationReached();
    }
}
