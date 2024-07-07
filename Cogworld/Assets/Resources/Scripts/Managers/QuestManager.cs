// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all things quest.
/// -Quest Creation
/// -Quest Assignment
/// -Quest Tracking
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Data")]
    public QuestDatabaseObject questDatabase;
    private Dictionary<string, Quest> questMap;
    public List<Quest> allQuests;

    private Dictionary<string, Quest> CreateQuestMap()
    {
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();

        foreach (var Q in allQuests)
        {
            if (idToQuestMap.ContainsKey(Q.uniqueID))
            {
                Debug.LogWarning($"WARNING: Duplicate ID found when creating quest map: {Q.uniqueID}");
            }
            idToQuestMap.Add(Q.uniqueID, Q);
        }

        return idToQuestMap;
    }

    public void Init()
    {
        Redraw();

        // Broadcast the initial state of all quests on startup
        foreach (Quest quest in questMap.Values)
        {
            GameManager.inst.questEvents.QuestStateChange(quest);
        }
    }

    public void Redraw()
    {
        questMap = CreateQuestMap();
    }

    private Quest GetQuestById(string id)
    {
        Quest quest = questMap[id];
        if(quest == null)
        {
            Debug.LogError($"ERROR: ID not found in the Quest Map: {id}");
        }
        return quest;
    }

    #region Event Related
    private void OnEnable()
    {
        GameManager.inst.questEvents.onStartQuest += StartQuest;
        GameManager.inst.questEvents.onAdvanceQuest += AdvanceQuest;
        GameManager.inst.questEvents.onFinishQuest += FinishQuest;
    }

    private void OnDisable()
    {
        GameManager.inst.questEvents.onStartQuest -= StartQuest;
        GameManager.inst.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameManager.inst.questEvents.onFinishQuest -= FinishQuest;
    }
    #endregion

    #region General
    public void CreateQuest(int id)
    {
        // Create the new quest based on requirements
        Quest newQuest = new Quest(questDatabase.Quests[id]);
        allQuests.Add(newQuest);
        // Redraw the quest map
        Redraw();
    }

    public void StartQuest(string id)
    {
        // TODO - start the quest
        Debug.Log($"Start Quest: {id}");
    }

    public void AdvanceQuest(string id)
    {
        // TODO - advance the quest
        Debug.Log($"Advance Quest: {id}");
    }

    public void FinishQuest(string id)
    {
        // TODO - finish the quest
        Debug.Log($"Finish Quest: {id}");
    }
    #endregion
}

public enum QuestState
{
    REQUIREMENTS_NOT_MET,
    CAN_START,
    IN_PROGRESS,
    CAN_FINISH,
    FINISHED
}
