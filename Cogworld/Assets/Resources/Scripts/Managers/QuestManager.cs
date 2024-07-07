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

    public void CreateQuest(int id)
    {
        // Create the new quest based on requirements
        Quest newQuest = new Quest(questDatabase.Quests[id]);
        allQuests.Add(newQuest);
        // Redraw the quest map
        Redraw();
    }

    public void AssignQuest()
    {

    }

    public void CompleteQuest()
    {

    }

    public void AbortQuest()
    {

    }

    public void FailQuest()
    {

    }

    public void QuestReward()
    {

    }

}

public enum QuestState
{
    REQUIREMENTS_NOT_MET,
    CAN_START,
    IN_PROGRESS,
    CAN_FINISH,
    FINISHED
}
