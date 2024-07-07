// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class Quest
{
    [Tooltip("A unique name that this quest and this quest alone has (each quest in the world should have a unique one.")]
    public string uniqueID;

    public QuestObject info;

    public QuestState state;
    private int currentQuestStepIndex;
    private QuestStepState[] questStepStates;

    public Quest(QuestObject info, string uid = "")
    {
        this.info = info;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;

        uniqueID = uid;
        if(uid == "")
        {
            uniqueID = $"{info.name} + {Random.Range(0, 99)}";
        }

        this.questStepStates = new QuestStepState[info.steps.Length];
        for(int i = 0; i < info.steps.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestObject questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.info = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;

        if(this.questStepStates.Length != this.info.steps.Length)
        {
            Debug.LogWarning($"WARNING: Quest step prefabs & quest step states are of different lengths. This indicates something changed with" +
                $"the QuestObject and the saved data is now out of sync. Reset your data - as this may cause issues. QuestID: {this.uniqueID}");
        }
    }

    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool CurrentStepExists()
    {
        return (currentQuestStepIndex < info.steps.Length);
    }

    public void InstantiateCurrentQuestStep(Transform parent)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
            QuestStep questStep = Object.Instantiate(questStepPrefab, parent).GetComponent<QuestStep>();
            questStep.InitQuestStep(uniqueID, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if(CurrentStepExists())
        {
            questStepPrefab = info.steps[currentQuestStepIndex];
        }
        else
        {
            Debug.LogWarning("WARNING: Tried to get quest step prefab, but stepIndex was out of range indicating that there's no current step:\n" +
                $"QuestId={info.Id} | stepIndex={currentQuestStepIndex}");
        }
        return questStepPrefab;
    }

    public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
    {
        if(stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex].state = questStepState.state;
        }
        else
        {
            Debug.LogWarning("WARNING: Tried to access quest step data, but stepIndex was out of range:\n" +
                $"QuestId={info.Id} | stepIndex={currentQuestStepIndex}");
        }
    }

    public QuestData GetQuestData()
    {
        return new QuestData(state, currentQuestStepIndex, questStepStates);
    }
}

[System.Serializable]
public abstract class QuestObject : ScriptableObject
{
   
    [Header("Overview")]
    [Tooltip("A generic ID for this type of quest.")]
    public int Id;
    public string displayName;
    [TextArea(3,5)] public string description;

    [Header("Requirements")]
    public Quest[] prerequisites;

    [Header("Steps")]
    public GameObject[] steps;

    [Header("Rewards")]
    public List<Item> reward_items;
    public int reward_matter;
}