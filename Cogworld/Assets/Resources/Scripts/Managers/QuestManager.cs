// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

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
    [Tooltip("A list of all quests that should be currently active.")]
    public List<QueuedQuest> questQueue = new List<QueuedQuest>();
    public List<GameObject> questPoints = new List<GameObject>();

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

        foreach (var newQuest in questQueue)
        {
            QuestObject Q = questDatabase.Quests[newQuest.questID];

            if (idToQuestMap.ContainsKey(Q.uniqueID))
            {
                Debug.LogWarning($"WARNING: Duplicate ID found when creating quest map: {Q.uniqueID}");
            }

            
            if(!QuestAlreadyActive(newQuest.questID)) // Make sure the physical quest point exists in the world.
            {
                CreateQuest(newQuest);
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

    private void Update()
    {
        if(questMap.Count > 0)
        {
            ShowQuestsButton();
        }
        else
        {
            HideQuestsButton();
        }
    }

    private void DEBUG_QuestTesting()
    {
        // Add the hideout test quest
        questQueue.Add(new QueuedQuest(2, new Vector2(PlayerData.inst.transform.position.x,
            PlayerData.inst.transform.position.y + 10), new Vector2(PlayerData.inst.transform.position.x, PlayerData.inst.transform.position.y + 10), null));
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
                break;
            }
        }

        // Check for: Item (in inventory) prereqs
        if (quest.info.prereq_items.Count > 0)
        {
            foreach (ItemObject item in quest.info.prereq_items)
            {
                InventoryObject inventory = PlayerData.inst.GetComponent<PartInventory>()._inventory;

                if (!inventory.HasGenericItem(item))
                {
                    meetsRequirements = false;
                    break;
                }
            }
        }

        // Check for: Matter amount prereqs
        if(quest.info.prereq_matter > 0)
        {
            if(PlayerData.inst.currentMatter + PlayerData.inst.currentInternalMatter < quest.info.prereq_matter)
            {
                meetsRequirements = false;
            }
        }

        return meetsRequirements;
    }

    // Checks to see if a questpoint with the matching id is already active
    private bool QuestAlreadyActive(int id)
    {
        foreach (GameObject obj in questPoints)
        {
            if(obj.GetComponent<QuestPoint>().questInfo != null && obj.GetComponent<QuestPoint>().questInfo.info.Id == id)
            {
                return true;
            }
        }

        return false;
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
    public void CreateQuest(QueuedQuest quest)
    {
        // Create the new quest based on requirements
        Quest newQuest = new Quest(questDatabase.Quests[quest.questID]);

        Transform parent = quest.botParent; // Assign parent
        if (parent == null)
            parent = this.transform;

        // Depending on circumstance, we may need to make two QuestPoints (one for the start location, one for the finish location
        if(quest.startPoint == quest.finishPoint)
        {
            GameObject obj = Instantiate(prefab_questPoint, quest.startPoint, Quaternion.identity, parent);
            obj.GetComponent<QuestPoint>().Init(newQuest, true, true);
            obj.name = newQuest.info.uniqueID;

            questPoints.Add(obj);
        }
        else
        {
            // Start point
            GameObject start = Instantiate(prefab_questPoint, quest.startPoint, Quaternion.identity, parent);
            start.GetComponent<QuestPoint>().Init(newQuest, true, false);
            start.name = newQuest.info.uniqueID;

            // Finish point
            GameObject finish = Instantiate(prefab_questPoint, quest.finishPoint, Quaternion.identity, parent);
            finish.GetComponent<QuestPoint>().Init(newQuest, false, true);
            finish.name = newQuest.info.uniqueID;

            questPoints.Add(start);
            questPoints.Add(finish);
        }

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

        // Remove quest point(s) from the quest queue
        foreach (GameObject qp in questPoints.ToList())
        {
            if(qp.name == id || qp.name.Contains(id))
            {
                Destroy(qp);
            }
        }
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
    [SerializeField] private Animator ui_animator;
    [SerializeField] private GameObject ui_mainReference;
    [SerializeField] private GameObject ui_buttonMain; // Should only show this if there are actually quests available
    [SerializeField] private TextMeshProUGUI ui_buttonMainText;
    [SerializeField] private GameObject ui_areaLeft;
    [SerializeField] private GameObject ui_areaRight;
    [Header("UI - Rightside")]
    [SerializeField] private TextMeshProUGUI text_questHeader;
    [SerializeField] private Image image_questType;
    [SerializeField] private List<Sprite> questTypeImages = new List<Sprite>();
    [SerializeField] private TextMeshProUGUI text_questType;
    [SerializeField] private TextMeshProUGUI text_questDifficulty;

    [Header("UI Prefabs")]
    public GameObject ui_prefab_smallQuest;
    private List<GameObject> ui_prefab_smallQuests = new List<GameObject>();

    public void ButtonHoverEnter()
    {
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.UI_Clips[48], 0.8f); // Play hover sound
    }

    public void ButtonHoverExit()
    {
        // UNUSED but here just in case
    }

    private void ShowQuestsButton()
    {
        ui_buttonMain.SetActive(true);
    }
    
    private void HideQuestsButton()
    {
        ui_buttonMain.SetActive(false);
    }

    public void OpenQuestMenu()
    {
        ui_mainReference.SetActive(true);
        ui_animator.Play("QUEST_WindowOpen");

        // Clear any pre-existing quests
        UI_ClearSmallQuests();

        // Fill the left side with all our quests (they animate by themselves)
        foreach (var Q in questMap)
        {
            UI_CreateSmallQuest(Q.Value);
        }

        // Auto select the first quest, and fill the right side up with its data
        if (questMap.Count > 0)
        {
            SelectQuest(ui_prefab_smallQuests[0].GetComponent<UISmallQuest>());
        }
    }

    public void SelectQuest(UISmallQuest sq)
    {
        Quest quest = sq.quest;
        QuestObject info = quest.info;

        // Header
        text_questHeader.text = info.name;
        // Type
        text_questType.text = "Type:\n" + info.type;
        // Type image
        Sprite typeImage = null;

        image_questType.sprite = typeImage;
        // Difficulty
        // - Second part has a unique color based on difficulty
        text_questDifficulty.text = $"Rank:\n<color=#{ColorUtility.ToHtmlStringRGB(ui_prefab_smallQuests[0].GetComponent<UISmallQuest>().color_main)}>{info.rank}</color>";

        // TODO: vv MORE vv
    }

    public void UnselectQuest(UISmallQuest sq)
    {
        // Do we need to actually do anything here?
        // All the stuff will be overriden by SelectQuest anyways.
    }

    public void CloseQuestMenu()
    {
        // Clear out the quests
        UI_ClearSmallQuests();

        // Disable the UI (no fancy animation, just turn it off)
        ui_mainReference.SetActive(false);

        // Play the close sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[20]);
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

[System.Serializable]
[Tooltip("Holds info for a quest that needs to be created")]
public class QueuedQuest
{
    public int questID;
    public Vector2 startPoint;
    public Vector2 finishPoint;
    public Transform botParent;

    public QueuedQuest(int questID, Vector2 startPoint, Vector2 finishPoint, Transform botParent)
    {
        this.questID = questID;
        this.startPoint = startPoint;
        this.finishPoint = finishPoint;
        this.botParent = botParent;
    }
}