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

    private void OnEnable()
    {
        GameManager.inst.questEvents.onLocationReached += LocationReached;
    }

    private void OnDisable()
    {
        GameManager.inst.questEvents.onLocationReached -= LocationReached;
    }

    private void LocationReached()
    {
        FinishQuestStep();
    }

    private void UpdateState()
    {
        //string state = itemToCollect.ToString();
        //ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        // No state is needed for this quest step
    }
}
