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
        if(questMap != null && questMap.Count > 0)
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
    //
    [SerializeField] private Image image_questGiver;
    [SerializeField] private TextMeshProUGUI text_questGiverName;
    [SerializeField] private TextMeshProUGUI text_questGiverDescription;
    //
    [SerializeField] private TextMeshProUGUI text_questDescription;
    //
    [SerializeField] private Transform ui_QuestStepsArea;
    //
    [SerializeField] private Transform ui_QuestRewardsArea;

    [Header("UI Prefabs")]
    public GameObject ui_prefab_smallQuest;
    private List<GameObject> ui_smallQuests = new List<GameObject>();
    [SerializeField] private GameObject ui_prefab_questStep;
    private List<GameObject> ui_questSteps = new List<GameObject>();
    [SerializeField] private GameObject ui_prefab_questReward;
    private List<GameObject> ui_questRewards = new List<GameObject>();

    [Header("Colors")]
    #region Colors
    public Color color_main = Color.white;
    public Color color_bright = Color.white;
    public Color color_dark = Color.white;
    //
    public Color c_orange1;
    public Color c_orange2;
    public Color c_orange3;
    //
    public Color c_blue1;
    public Color c_blue2;
    public Color c_blue3;
    //
    public Color c_yellow1;
    public Color c_yellow2;
    public Color c_yellow3;
    //
    public Color c_red1;
    public Color c_red2;
    public Color c_red3;
    //
    public Color c_purple1;
    public Color c_purple2;
    public Color c_purple3;
    //
    public Color c_green1;
    public Color c_green2;
    public Color c_green3;
    //
    public Color c_gray1;
    public Color c_gray2;
    public Color c_gray3;
    #endregion

    private List<Color> UIGetColors(Quest quest)
    {
        List<Color> colors = new List<Color>();
        QuestObject info = quest.info;

        switch (info.rank) // Set the primary colors based on difficulty
        {
            case QuestRank.Default:
                Debug.LogWarning($"{quest} ({info} - {info.Id}) did not get a set rank! Visuals will not be properly set.");
                break;
            case QuestRank.Easy: // Green
                color_main = c_green1;
                color_bright = c_green2;
                color_dark = c_green3;
                break;
            case QuestRank.Medium: // Blue
                color_main = c_blue1;
                color_bright = c_blue2;
                color_dark = c_blue3;
                break;
            case QuestRank.Hard: // Orange
                color_main = c_orange1;
                color_bright = c_orange2;
                color_dark = c_orange3;
                break;
            case QuestRank.Difficult: // Red
                color_main = c_red1;
                color_bright = c_red2;
                color_dark = c_red3;
                break;
            case QuestRank.Expert: // Purple
                color_main = c_purple1;
                color_bright = c_purple2;
                color_dark = c_purple3;
                break;
            case QuestRank.Legendary: // Yellow
                color_main = c_yellow1;
                color_bright = c_yellow2;
                color_dark = c_yellow3;
                break;
        }

        // If the quest is finished set everything to gray instead
        if (quest.state == QuestState.FINISHED)
        {
            color_main = c_gray1;
            color_bright = c_gray2;
            color_dark = c_gray3;
        }

        colors.Add(color_main);
        colors.Add(color_bright);
        colors.Add(color_dark);

        return colors;
    }

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
        // Play the OPEN sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[64]);

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
            SelectQuest(ui_smallQuests[0].GetComponent<UISmallQuest>());
        }
    }

    public void SelectQuest(UISmallQuest sq)
    {
        // Unselect everything
        foreach (var Q in ui_smallQuests)
        {
            Q.GetComponent<UISmallQuest>().Unselect();
        }
        // Clear out the quest steps & rewards
        UI_ClearQuestSteps();
        UI_ClearQuestRewards();

        // Mark quest as selected
        sq.Select();

        Quest quest = sq.quest;
        QuestObject info = quest.info;

        // Header
        text_questHeader.text = info.name;
        // Type
        text_questType.text = "Type:\n" + info.type;
        // Type image
        Sprite typeImage = null;
        switch (info.type)
        {
            case QuestType.Kill: // Bot
                typeImage = questTypeImages[0];
                break;
            case QuestType.Collect: // Green Data
                typeImage = questTypeImages[1];
                break;
            case QuestType.Find: // Door
                typeImage = questTypeImages[2];
                break;
            case QuestType.Meet: // NPC
                typeImage = questTypeImages[3];
                break;
            case QuestType.Destroy: // Machine
                typeImage = questTypeImages[4];
                break;
        }
        image_questType.sprite = typeImage;
        // Difficulty
        // - Second part has a unique color based on difficulty
        text_questDifficulty.text = $"Rank:\n<color=#{ColorUtility.ToHtmlStringRGB(ui_smallQuests[0].GetComponent<UISmallQuest>().color_main)}>{info.rank}</color>";

        // Prerequisites <?????> TODO


        // Quest Giver Image
        image_questGiver.sprite = quest.info.questGiverSprite;
        // Quest Giver Name
        text_questGiverName.text = quest.info.questGiverName;
        // Quest Giver Description
        text_questGiverDescription.text = quest.info.questGiverDescription;
        
        // Description
        text_questDescription.text = quest.info.description;

        // Additional Details
        // -Fill up with the quest steps the player needs to do
        foreach (var qs in quest.info.steps)
        {
            // These are gameObjects, and we need to give their info to our newly created quest step (UI) objects
            UI_CreateQuestStep(quest, qs);
        }

        // Rewards
        // - Same as above, fill up with rewards
        foreach (var rw in quest.info.reward_items)
        {
            UI_CreateQuestRewards(quest, rw);
        }
        // - And one for matter too if it exists
        if(quest.info.reward_matter > 0)
        {
            UI_CreateQuestRewards(quest, null, quest.info.reward_matter);
        }
    }

    public void CloseQuestMenu()
    {
        // Play the close sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[20]);

        // Clear out the quests
        UI_ClearSmallQuests();

        // Disable the UI (no fancy animation, just turn it off)
        ui_mainReference.SetActive(false);
    }

    private void UI_CreateSmallQuest(Quest quest)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_smallQuest, ui_areaLeft.transform);

        // Assign details to object
        obj.GetComponent<UISmallQuest>().Init(quest, UIGetColors(quest));

        // Add to list
        ui_smallQuests.Add(obj);
    }

    private void UI_ClearSmallQuests()
    {
        foreach (var GO in ui_smallQuests.ToList())
        {
            Destroy(GO);
        }

        ui_smallQuests.Clear();
    }

    private void UI_CreateQuestStep(Quest quest, GameObject step)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_questStep, ui_QuestStepsArea.transform);

        // Assign details to object
        obj.GetComponent<UIQuestStep>().Init(quest, step, UIGetColors(quest));

        // Add to list
        ui_questSteps.Add(obj);
    }

    private void UI_ClearQuestSteps()
    {
        foreach (var GO in ui_questSteps.ToList())
        {
            Destroy(GO);
        }

        ui_questSteps.Clear();
    }

    private void UI_CreateQuestRewards(Quest quest, Item itemReward = null, int matterReward = 0)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_questReward, ui_QuestRewardsArea.transform);

        // Assign details to object
        obj.GetComponent<UIQuestReward>().Init(quest, UIGetColors(quest), itemReward, matterReward);

        // Add to list
        ui_questRewards.Add(obj);
    }

    private void UI_ClearQuestRewards()
    {
        foreach (var GO in ui_questRewards.ToList())
        {
            Destroy(GO);
        }

        ui_questRewards.Clear();
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