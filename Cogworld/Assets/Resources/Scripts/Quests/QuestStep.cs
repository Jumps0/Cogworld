// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    private bool isFinished = false;
    private string questID;
    private int stepIndex;

    public void InitQuestStep(string questID, int stepIndex, string questStepState)
    {
        this.questID = questID;
        this.stepIndex = stepIndex;
        if(questStepState != null && questStepState != "")
        {
            SetQuestStepState(questStepState);
        }
    }

    protected void FinishQuestStep()
    {
        if(!isFinished)
        {
            isFinished = true;

            GameManager.inst.questEvents.AdvanceQuest(questID);

            Destroy(this.gameObject);
        }
    }

    protected void ChangeState(string newState)
    {
        GameManager.inst.questEvents.QuestStepStateChange(questID, stepIndex, new QuestStepState(newState));
    }

    protected abstract void SetQuestStepState(string state);
}
