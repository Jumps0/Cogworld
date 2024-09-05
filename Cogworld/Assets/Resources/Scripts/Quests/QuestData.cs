// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for saving quest data in a JSON format
/// </summary>
[System.Serializable]
public class QuestData
{
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;
    public List<bool> completedSteps;

    public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates, List<bool> completedSteps)
    {
        this.state = state;
        this.questStepIndex = questStepIndex;
        this.questStepStates = questStepStates;
        this.completedSteps = completedSteps;
    }
}
