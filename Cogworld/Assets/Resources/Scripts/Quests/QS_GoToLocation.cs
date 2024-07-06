// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to go to a specific location.
/// </summary>
public class QS_GoToLocation : QuestStep
{
    [Tooltip("True = Specific 1x1 tile | False = General area around this point")]
    private bool isSpecificLocation;
    private Vector2 center;
    private int radius = 1;

}
