// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    [Header("Prefabs")]
    [SerializeField] private GameObject prefab_questPoint;

    [Header("QuestMap")]
    [SerializeField] private List<string> qm_names = new List<string>();
    [SerializeField] private List<Quest> qm_quests = new List<Quest>();

    private Dictionary<string, Quest> CreateQuestMap()
    {
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        qm_names.Clear();
        qm_quests.Clear();

        foreach (QuestObject Q in questDatabase.Quests)
        {
            if (idToQuestMap.ContainsKey(Q.uniqueID))
            {
                Debug.LogWarning($"WARNING: Duplicate ID found when creating quest map: {Q.uniqueID}");
            }
            idToQuestMap.Add(Q.uniqueID, LoadQuest(Q));

            // Update the visible in inspector lists
            qm_names.Add(Q.uniqueID);
            qm_quests.Add(LoadQuest(Q));
        }

        return idToQuestMap;
    }

    public void Init()
    {
        GameManager.inst.questEvents.onStartQuest += StartQuest;
        GameManager.inst.questEvents.onAdvanceQuest += AdvanceQuest;
        GameManager.inst.questEvents.onFinishQuest += FinishQuest;

        GameManager.inst.questEvents.onQuestStepStateChange += QuestStepStateChange;

        DEBUG_QuestTesting();

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

    private void DEBUG_QuestTesting()
    {
        CreateQuest(2, PlayerData.inst.transform.position + new Vector3(0, 10, 0));
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
                ChangeQuestState(quest.info.uniqueID, QuestState.CAN_START);
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
            if (GetQuestById(prereq.info. uniqueID).state != QuestState.FINISHED)
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

    public void CreateQuest(int id, Vector3 location)
    {
        // Create the new quest based on requirements
        Quest newQuest = new Quest(questDatabase.Quests[id]);
        GameObject obj = Instantiate(prefab_questPoint, location, Quaternion.identity, this.transform);
        obj.GetComponent<QuestPoint>().Init(newQuest);

        // Redraw the quest map
        Redraw();
    }

    public void StartQuest(string id)
    {
        Quest quest = GetQuestById(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.uniqueID, QuestState.IN_PROGRESS);
    }

    public void AdvanceQuest(string id)
    {
        Quest quest = GetQuestById(id);


        if (quest.CurrentStepExists()) // If there are more steps, instantiate the next one
        {
            quest.InstantiateCurrentQuestStep(this.transform);
        }
        else // If there are no more steps, then we've finished all of them for this quest
        {
            ChangeQuestState(quest.info.uniqueID, QuestState.CAN_FINISH);
        }
    }

    public void FinishQuest(string id)
    {
        Quest quest = GetQuestById(id);
        QuestReward(quest);
        ChangeQuestState(quest.info.uniqueID, QuestState.FINISHED);
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
            PlayerPrefs.SetString(quest.info.uniqueID, serializedData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save quest with id {quest.info.uniqueID}: {ex}");
        }
    }

    private Quest LoadQuest(QuestObject questInfo)
    {
        Quest quest = null;
        try
        {
            // Load quest from saved data
            if (PlayerPrefs.HasKey(questInfo.uniqueID) && loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.uniqueID);
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            // Otherwise, initialize a new quest
            else
            {
                quest = new Quest(questInfo, questInfo.uniqueID);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR: Failed to load quest with id {quest.info.uniqueID}: with {e}");
        }
        return quest;
    }
    #endregion

    #region UI
    [Header("UI")]
    public GameObject ui_mainReference;
    public GameObject ui_buttonMain; // Should only show this if there are actually quests available
    public TextMeshProUGUI ui_buttonMainText;
    [SerializeField] private GameObject ui_areaLeft;
    [SerializeField] private GameObject ui_areaRight;

    [Header("UI Prefabs")]
    public GameObject ui_prefab_smallQuest;
    public GameObject ui_prefab_questDetail;
    private List<GameObject> ui_prefab_smallQuests = new List<GameObject>();

    public void ButtonHoverEnter()
    {
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.UI_Clips[48], 0.8f); // Play hover sound
    }

    public void ButtonHoverExit()
    {

    }

    private void ShowQuestsButton()
    {

    }
    
    private void HideQuestsButton()
    {

    }

    public void OpenQuestMenu()
    {

    }

    public void CloseQuestMenu()
    {

    }

    private void UI_CreateSmallQuest(Quest quest)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_smallQuest, ui_areaLeft.transform);

        // Assign details to object
        obj.GetComponent<UISmallQuest>().Init(quest);

        // Add to list
        ui_prefab_smallQuests.Add(obj);
    }

    private void UI_ClearSmallQuests()
    {
        foreach (var GO in ui_prefab_smallQuests.ToList())
        {
            Destroy(GO);
        }

        ui_prefab_smallQuests.Clear();
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
