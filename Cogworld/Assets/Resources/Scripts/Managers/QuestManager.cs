// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;
using static Unity.VisualScripting.Member;
using static UnityEditor.Progress;

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
    public List<int> questQueue = new List<int>();
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

        foreach (var queuedQuest in questQueue)
        {
            QuestObject Q = questDatabase.Quests[queuedQuest];

            if (idToQuestMap.ContainsKey(Q.uniqueID))
            {
                Debug.LogWarning($"WARNING: Duplicate ID found when creating quest map: {Q.uniqueID}");
            }

            if(!QuestAlreadyActive(queuedQuest)) // Make sure the physical quest point exists in the world.
            {
                CreateQuest(queuedQuest);
            }
            Quest newQuest = LoadQuest(Q);
            idToQuestMap.Add(Q.uniqueID, newQuest);

            // Update the visible in inspector lists
            qm_names.Add(Q.uniqueID);
            qm_quests.Add(newQuest);
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
        questQueue.Add(2);
        // Add the 20 kills quest
        questQueue.Add(3);
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

    public Quest GetQuestById(string id)
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
            if(obj.GetComponent<QuestPoint>().quest != null && obj.GetComponent<QuestPoint>().quest.info.Id == id)
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
    public Quest CreateQuest(int questID)
    {
        // Create the new quest based on requirements
        Quest newQuest = new Quest(questDatabase.Quests[questID]);

        // Gather info from this quests QuestPointInfo
        QuestPointInfo start = newQuest.info.startLocation;
        QuestPointInfo finish = newQuest.info.finishLocation;

        // Check to see if this quest should have the same start and finish point, then act on that
        if (start.isStartAndFinish) // Same start & end location (1 Object Needed)
        {
            // Determine where this quest should be placed (and parented to)
            Transform parent = null;
            Vector2 spawnPos = Vector2.zero;

            #region Position & Parent determination
            if (start.inReferenceToPlayer)
            {
                parent = this.transform; // Transform set to this manager
                spawnPos = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
            }
            else if(start.assignedBot != null)
            {
                // We need to try and find this bot in the world
                foreach (var bot in GameManager.inst.entities)
                {
                    if(bot.GetComponent<Actor>().botInfo == start.assignedBot) // ---------------- not finding the bot? is it even in this list yet?
                    {
                        parent = bot.transform; // Transform set to the bot
                        spawnPos = bot.transform.position; // Spawnpoint based on this bot's pos
                        break;
                    }
                }
                // Fallback incase the bot wasn't found
                if(parent == null)
                {
                    parent = this.transform; // Transform set to this manager
                    spawnPos = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
                    Debug.LogWarning($"No bot reference ({start.assignedBot}) found in world to assign {newQuest} to.");
                }
            }
            else if(start.refpoint != null)
            {
                parent = this.transform; // Transform set to this manager
                spawnPos = start.refpoint.position; // Position based on this reference point
            }
            spawnPos += start.refpoint_offset; // Add offset
            #endregion

            GameObject obj = Instantiate(prefab_questPoint, spawnPos, Quaternion.identity, parent);
            obj.GetComponent<QuestPoint>().Init(newQuest, true, true, start);
            obj.name = $"START+FINISH: {newQuest.info.uniqueID}";

            questPoints.Add(obj);
        }
        else // Different start & end locations (2 Objects Needed)
        {
            // Determine where this quest should be placed (and parented to)
            Transform parent_s = null, parent_f = null;
            Vector2 spawnPos_s = Vector2.zero, spawnPos_f = Vector2.zero;

            #region Position & Parent determination (Start)
            if (start.inReferenceToPlayer)
            {
                parent_s = this.transform; // Transform set to this manager
                spawnPos_s = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
            }
            else if (start.assignedBot != null)
            {
                // We need to try and find this bot in the world
                foreach (var bot in GameManager.inst.entities)
                {
                    if (bot.GetComponent<Actor>().botInfo == start.assignedBot)
                    {
                        parent_s = bot.transform; // Transform set to the bot
                        spawnPos_s = bot.transform.position; // Spawnpoint based on this bot's pos
                        break;
                    }
                }

                // Fallback incase the bot wasn't found
                if (parent_s == null)
                {
                    parent_s = this.transform; // Transform set to this manager
                    spawnPos_s = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
                    Debug.LogWarning($"No bot reference ({start.assignedBot}) found in world to assign {newQuest} to.");
                }
            }
            else if (start.refpoint != null)
            {
                parent_s = this.transform; // Transform set to this manager
                spawnPos_s = start.refpoint.position; // Position based on this reference point
            }
            spawnPos_s += start.refpoint_offset; // Add offset
            #endregion

            #region Position & Parent determination (Finish)
            if (start.inReferenceToPlayer)
            {
                parent_f = this.transform; // Transform set to this manager
                spawnPos_f = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
            }
            else if (start.assignedBot != null)
            {
                // We need to try and find this bot in the world
                foreach (var bot in GameManager.inst.entities)
                {
                    if (bot.GetComponent<Actor>().botInfo == start.assignedBot)
                    {
                        parent_f = bot.transform; // Transform set to the bot
                        spawnPos_f = bot.transform.position; // Spawnpoint based on this bot's pos
                        break;
                    }
                }

                // Fallback incase the bot wasn't found
                if (parent_f == null)
                {
                    parent_f = this.transform; // Transform set to this manager
                    spawnPos_f = PlayerData.inst.transform.position; // Spawnpoint based on player's pos
                    Debug.LogWarning($"No bot reference ({start.assignedBot}) found in world to assign {newQuest} to.");
                }
            }
            else if (start.refpoint != null)
            {
                parent_f = this.transform; // Transform set to this manager
                spawnPos_f = start.refpoint.position; // Position based on this reference point
            }
            spawnPos_f += start.refpoint_offset; // Add offset
            #endregion

            // Start point
            GameObject obj_start = Instantiate(prefab_questPoint, spawnPos_s, Quaternion.identity, parent_s);
            obj_start.GetComponent<QuestPoint>().Init(newQuest, true, false, start);
            obj_start.name = $"START: {newQuest.info.uniqueID}";

            // Finish point
            GameObject obj_finish = Instantiate(prefab_questPoint, spawnPos_f, Quaternion.identity, parent_f);
            obj_finish.GetComponent<QuestPoint>().Init(newQuest, false, true, finish);
            obj_finish.name = $"FINISH: {newQuest.info.uniqueID}";

            questPoints.Add(obj_start);
            questPoints.Add(obj_finish);
        }

        // Redraw the quest map
        Redraw();

        return newQuest;
    }

    public void StartQuest(string id)
    {
        Quest quest = GetQuestById(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.uniqueID, QuestState.IN_PROGRESS);

        // Leave a log message
        string logMessage = $"Started: {quest.info.displayName}";
        UIManager.inst.CreateNewLogMessage(logMessage, QuestManager.inst.c_yellow2, QuestManager.inst.c_yellow1, true);
    }

    public void AdvanceQuest(string id)
    {
        Quest quest = GetQuestById(id);
        quest.MoveToNextStep();

        if (quest.CurrentStepExists()) // If there are more steps, instantiate the next one
        {
            quest.InstantiateCurrentQuestStep(this.transform);
        }
        else // If there are no more steps, then we've finished all of them for this quest
        {
            // Change the state
            ChangeQuestState(quest.info.uniqueID, QuestState.CAN_FINISH);
        }
    }

    public void FinishQuest(string id)
    {
        Quest quest = GetQuestById(id);

        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.ENDINGS_Clips[5], 0.8f); // Play a sound
        
        // Make a message in the log
        string logMessage = $"Quest Completed: {quest.info.displayName}";
        UIManager.inst.CreateNewLogMessage(logMessage, QuestManager.inst.c_yellow2, QuestManager.inst.c_yellow1, true, true);

        if(QuestHasRewards(quest))
            UIManager.inst.CreateNewLogMessage($"Quest reward available to claim.", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false); // And tell the player about their reward

        // Do reward logic
        ChangeQuestState(quest.info.uniqueID, QuestState.FINISHED); // Set quest as finished

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
        quest.info.reward_claimed = true; // Set claimed flag

        ui_ClaimRewardsButton.SetActive(false); // Disable the Claim button
        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[14], 0.5f); // Play a claim sound [UI - CASH_REGISTER]

        List<ItemObject> rewards_item = quest.info.reward_items;
        int rewards_matter = quest.info.reward_matter;

        // How do we handle this?
        // - The player has a storage object in their hideout, we will simply put the items / matter in there.
        // - The player can interact with it when they are there.
        // - We do this instead of instantly rewarding it to the player (during their run) because they will probably lose it when they die, and they wouldn't like that.

        if(rewards_item.Count > 0) // -- Items --
        {
            // Add items to the Hideout Inventory Object
            foreach (var item in rewards_item)
            {
                InventoryControl.inst.hideout_inventory.AddItem(new Item(item));
            }
        }

        if(rewards_matter > 0) // -- Matter --
        {
            // Add a stack of matter (the item) with the specific amount to the Hideout Inventory Object
            InventoryControl.inst.hideout_inventory.AddItem(new Item(InventoryControl.inst._itemDatabase.Items[17]), rewards_matter);
        }
    }

    /// <summary>
    /// Determines if this quest actually has any rewards.
    /// </summary>
    /// <param name="quest">The quest to check.</param>
    /// <returns>True/False if a reward for this quest exists.</returns>
    private bool QuestHasRewards(Quest quest)
    {
        List<ItemObject> rewards_item = quest.info.reward_items;
        int rewards_matter = quest.info.reward_matter;

        if (rewards_item.Count > 0) // -- Items --
        {
            return true;
        }

        if (rewards_matter > 0) // -- Matter --
        {
            return true;
        }

        return false;
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
                quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates, questData.completedSteps);
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
    [SerializeField] private TextMeshProUGUI text_questTypeNP; // The static "Type:" text
    [SerializeField] private TextMeshProUGUI text_questDifficulty;
    [SerializeField] private TextMeshProUGUI text_questDifficultyNP; // The static "Difficulty:" text
    //
    [SerializeField] private Transform ui_QuestPrereqsArea;
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
    //
    [SerializeField] private GameObject ui_PrereqsNone;
    [SerializeField] private GameObject ui_RewardsNone;
    //
    [SerializeField] private GameObject ui_ClaimRewardsButton;

    [Header("UI Prefabs")]
    public GameObject ui_prefab_smallQuest;
    private List<GameObject> ui_smallQuests = new List<GameObject>();
    [SerializeField] private GameObject ui_prefab_questStep;
    [SerializeField] private GameObject ui_prefab_questPrereq;
    private List<GameObject> ui_questPrereqs = new List<GameObject>();
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

    private List<Color> UIGetColors(Quest quest, bool nogray = false)
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
        if (quest.state == QuestState.FINISHED && !nogray)
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
    #endregion

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
        for (int i = 0; i < questMap.Count; i++)
        {
            bool selected = false;
            if(i == 0)
                selected = true;

            UI_CreateSmallQuest(questMap.ToList()[i].Value, selected);
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
            if(Q.GetComponent<UISmallQuest>() != sq)
            {
                Q.GetComponent<UISmallQuest>().Unselect();
            }
        }
        // Clear out the quest prereqs, steps & rewards
        UI_ClearQuestPreReqs();
        UI_ClearQuestSteps();
        UI_ClearQuestRewards();

        // Get values
        Quest quest = sq.quest;
        QuestObject info = quest.info;
        List<Color> questColors = UIGetColors(quest); // Get the colors

        // Set this for later use
        currentActivateQuest = sq;

        // Header
        text_questHeader.text = info.name;
        // Type
        text_questType.text = info.type.ToString();
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
        text_questDifficulty.text = info.rank.ToString();
        text_questDifficulty.color = sq.color_main;

        // Prerequisites
        foreach (var PR in quest.info.prerequisites)
        {
            UI_CreateQuestPreReq(PR);
        }
        ui_PrereqsNone.SetActive(quest.info.prerequisites.Length <= 0); // Enable or Disable the (None) text

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
        for (int i = 0; i < quest.info.steps.Length; i++)
        {
            // These are gameObjects, and we need to give their info to our newly created quest step (UI) objects
            UI_CreateQuestStep(i, quest, quest.info.steps[i]);
        }

        // Rewards
        // - Same as above, fill up with rewards
        foreach (var rw in quest.info.reward_items)
        {
            UI_CreateQuestRewards(quest, rw);
        }
        // - And one for matter too if it exists
        if (quest.info.reward_matter > 0)
        {
            UI_CreateQuestRewards(quest, null, quest.info.reward_matter);
        }
        ui_RewardsNone.SetActive((quest.info.reward_items == null || quest.info.reward_items.Count <= 0) && quest.info.reward_matter <= 0); // Enable or Disable the (None) text

        // > Rewards Claim <
        ui_ClaimRewardsButton.SetActive(false);
        if (QuestHasRewards(quest)) // - Does this quest even have a reward?
        {
            if (quest.state == QuestState.FINISHED) // - Is the quest done?
            {
                if (!info.reward_claimed) // - Is the reward still unclaimed?
                {
                    // Show the claim rewards button
                    ui_ClaimRewardsButton.SetActive(true);
                }
            }
        }

        UI_RightSideAnimate();
    }

    private UISmallQuest currentActivateQuest;
    public void TryClaimQuestReward()
    {
        if(currentActivateQuest != null)
        {
            Quest quest = currentActivateQuest.quest;
            QuestObject info = quest.info;

            QuestReward(quest);

            // Now that we have claimed the reward, we need to set the UIQuestReward to be grayed out
            foreach (var R in ui_questRewards)
            {
                R.GetComponent<UIQuestReward>().ClaimedAnimation(UIGetColors(quest));
            }
        }
    }

    private List<Coroutine> typeout_coroutines = new List<Coroutine>();
    private void UI_RightSideAnimate()
    {
        // Stop all previous type-out coroutines
        foreach (var c in typeout_coroutines)
        {
            StopCoroutine(c);
        }
        typeout_coroutines.Clear();

        // Using the highlight animation helper, we will use that for most of the display text
        List<string> header = HF.RandomHighlightStringAnimation(text_questHeader.text, text_questHeader.color);
        List<string> type = HF.RandomHighlightStringAnimation(text_questType.text, text_questType.color);
        List<string> typeNP = HF.RandomHighlightStringAnimation("Type:", text_questTypeNP.color);
        List<string> rank = HF.RandomHighlightStringAnimation(text_questDifficulty.text, text_questDifficulty.color);
        List<string> rankNP = HF.RandomHighlightStringAnimation("Difficulty:", text_questDifficultyNP.color);
        List<string> giverName = HF.RandomHighlightStringAnimation(text_questGiverName.text, text_questGiverName.color);
        List<string> giverText = HF.RandomHighlightStringAnimation(text_questGiverDescription.text, text_questGiverDescription.color);
        List<string> descrption = HF.RandomHighlightStringAnimation(text_questDescription.text, text_questDescription.color);

        // Combine lists
        List<KeyValuePair<TextMeshProUGUI, List<string>>> lists = new List<KeyValuePair<TextMeshProUGUI, List<string>>>();
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questHeader, header));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questType, type));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questTypeNP, typeNP));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questDifficulty, rank));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questDifficultyNP, rankNP));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questGiverName, giverName));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questGiverDescription, giverText));
        lists.Add(new KeyValuePair<TextMeshProUGUI, List<string>>(text_questDescription, descrption));

        // Go through each list and begin delayed set text
        foreach (var LIST in lists)
        {
            // Animate the strings via our delay trick
            float delay = 0f;
            float perDelay = 0.5f / LIST.Key.text.Length;

            foreach (string s in LIST.Value)
            {
                typeout_coroutines.Add(StartCoroutine(HF.DelayedSetText(LIST.Key, s, delay += perDelay)));
            }
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

    private void UI_CreateQuestPreReq(Quest quest)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_questPrereq, ui_QuestPrereqsArea.transform);

        // Assign details to object
        obj.GetComponent<UIQuestPreReq>().Init(quest, UIGetColors(quest));

        // Add to list
        ui_questPrereqs.Add(obj);
    }

    private void UI_ClearQuestPreReqs()
    {
        foreach (var GO in ui_questPrereqs.ToList())
        {
            Destroy(GO);
        }

        ui_questPrereqs.Clear();
    }

    private void UI_CreateSmallQuest(Quest quest, bool startAsSelected = false)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_smallQuest, ui_areaLeft.transform);

        // Assign details to object
        obj.GetComponent<UISmallQuest>().Init(quest, UIGetColors(quest), startAsSelected);

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

    private void UI_CreateQuestStep(int sis, Quest quest, GameObject step)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_questStep, ui_QuestStepsArea.transform);

        // Assign details to object
        obj.GetComponent<UIQuestStep>().Init(sis, quest, step, UIGetColors(quest));

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

    private void UI_CreateQuestRewards(Quest quest, ItemObject itemReward = null, int matterReward = 0)
    {
        // Instantiate Object
        GameObject obj = Instantiate(ui_prefab_questReward, ui_QuestRewardsArea.transform);

        // Assign details to object
        // We only set it as grey if its been claimed
        List<Color> colors = new List<Color>();
        if (quest.info.reward_claimed)
        {
            colors = UIGetColors(quest);
        }
        else
        {
            colors = UIGetColors(quest, true);
        }
        obj.GetComponent<UIQuestReward>().Init(quest, colors, itemReward, matterReward);

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