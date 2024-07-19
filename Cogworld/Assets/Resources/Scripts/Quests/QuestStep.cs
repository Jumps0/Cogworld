// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    public bool isFinished = false;
    public string questID;
    [SerializeField] private int stepIndex;
    public string stepDescription;

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
            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[24], 0.8f); // Play a sound
            // Make a message in the log
            string logMessage = $"Subtask completed: {stepDescription}";
            UIManager.inst.CreateNewLogMessage(logMessage, QuestManager.inst.c_yellow2, QuestManager.inst.c_yellow1, true);

            Destroy(this.gameObject);
        }
    }

    protected void ChangeState(string newState) // This will (eventually) lead into *StoreQuestStepState* inside QuestObject.cs to save the quest step states in an Array.
    {
        GameManager.inst.questEvents.QuestStepStateChange(questID, stepIndex, new QuestStepState(newState));
    }

    protected abstract void SetQuestStepState(string state);
}
