// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Quest
{
    [Tooltip("A unique name that this quest and this quest alone has (each quest in the world should have a unique one.")]
    public string uniqueID;

    public QuestObject info;

    public QuestState state;
    private int currentQuestStepIndex;

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
            Object.Instantiate(questStepPrefab, parent);
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
    public QuestObject[] prerequisites;

    [Header("Steps")]
    public GameObject[] steps;

    [Header("Rewards")]
    public List<Item> reward_items;
    public int reward_matter;
}