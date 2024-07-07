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
    [Header("Config")]
    [SerializeField] private bool loadQuestState = false;

    [Header("Data")]
    public QuestDatabaseObject questDatabase;
    private Dictionary<string, Quest> questMap;
    public List<Quest> allQuests = new List<Quest>();

    private Dictionary<string, Quest> CreateQuestMap()
    {
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();

        foreach (var Q in allQuests)
        {
            if (idToQuestMap.ContainsKey(Q.uniqueID))
            {
                Debug.LogWarning($"WARNING: Duplicate ID found when creating quest map: {Q.uniqueID}");
            }
            idToQuestMap.Add(Q.uniqueID, LoadQuest(Q));
        }

        return idToQuestMap;
    }

    public void Init()
    {
        GameManager.inst.questEvents.onStartQuest += StartQuest;
        GameManager.inst.questEvents.onAdvanceQuest += AdvanceQuest;
        GameManager.inst.questEvents.onFinishQuest += FinishQuest;

        GameManager.inst.questEvents.onQuestStepStateChange += QuestStepStateChange;

        Redraw();

        // Broadcast the initial state of all quests on startup
        foreach (Quest quest in questMap.Values)
        {
            // Initialize any loaded quest steps
            if(quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            // Broadcast the initial state of all quests on startup
            GameManager.inst.questEvents.QuestStateChange(quest);
        }
    }

    public void Redraw()
    {
        questMap = CreateQuestMap();

        // Loop through ALL quests
        foreach (Quest quest in questMap.Values)
        {
            // If we're now meeting the requirements, switch over to the CAN_START state
            if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.uniqueID, QuestState.CAN_START);
            }
        }
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

    private void ChangeQuestState(string id, QuestState state)
    {
        Quest quest = GetQuestById(id);
        quest.state = state;
        GameManager.inst.questEvents.QuestStateChange(quest);
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        // Start true and prove to be false
        bool meetsRequirements = true;

        // Check quest prerequisites for completion
        foreach (Quest prereq in quest.info.prerequisites)
        {
            if (GetQuestById(prereq.uniqueID).state != QuestState.FINISHED)
            {
                meetsRequirements = false;
            }
        }

        return meetsRequirements;
    }

    #region Event Related

    private void OnDisable()
    {
        GameManager.inst.questEvents.onStartQuest -= StartQuest;
        GameManager.inst.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameManager.inst.questEvents.onFinishQuest -= FinishQuest;

        GameManager.inst.questEvents.onQuestStepStateChange -= QuestStepStateChange;
    }
    #endregion

    #region General
    private void Update()
    {
        
    }

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
        Quest quest = GetQuestById(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.uniqueID, QuestState.IN_PROGRESS);
    }

    public void AdvanceQuest(string id)
    {
        Quest quest = GetQuestById(id);

        quest.MoveToNextStep(); // Move on to the next step

        if (quest.CurrentStepExists()) // If there are more steps, instantiate the next one
        {
            quest.InstantiateCurrentQuestStep(this.transform);
        }
        else // If there are no more steps, then we've finished all of them for this quest
        {
            ChangeQuestState(quest.uniqueID, QuestState.CAN_FINISH);
        }
    }

    public void FinishQuest(string id)
    {
        Quest quest = GetQuestById(id);
        QuestReward(quest);
        ChangeQuestState(quest.uniqueID, QuestState.FINISHED);
    }

    private void QuestReward(Quest quest)
    {

    }

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id, quest.state);
    }
    #endregion

    #region Data Save/Load
    private void OnApplicationQuit()
    {
        foreach (Quest quest in questMap.Values) 
        {
            SaveQuest(quest);
        }
    }

    private void SaveQuest(Quest quest)
    {
        try
        {
            QuestData questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);
            PlayerPrefs.SetString(quest.uniqueID, serializedData);

            Debug.Log($"Saved quest to PlayerPrefs: {serializedData}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save quest with id {quest.uniqueID}: {ex}");
        }
    }

    private Quest LoadQuest(Quest quest)
    {
        try
        {
            // Load quest from saved data
            if (PlayerPrefs.HasKey(quest.uniqueID) && loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(quest.uniqueID);
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(quest.info, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            // Otherwise, initialize a new quest
            else
            {
                quest = new Quest(quest.info);
            }
        }
        catch
        { 
        
        }
        return quest;
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
