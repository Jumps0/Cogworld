// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public QuestObject info;

    public QuestState state;
    public int currentQuestStepIndex;
    private QuestStepState[] questStepStates;

    [Header("Tracking Value")]
    [Tooltip("Used to track quest progress in a generic manner.")]
    public float value;

    public Quest(QuestObject info, string uid = "")
    {
        this.info = info;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;

        info.uniqueID = uid;
        if(uid == "")
        {
            info.uniqueID = $"{info.name} + {Random.Range(0, 99)}";
        }

        SetupQuestPrefabs(); // Setup the step prefabs

        this.questStepStates = new QuestStepState[info.steps.Length];
        for(int i = 0; i < info.steps.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestObject info, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.info = info;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;

        if(this.questStepStates.Length != this.info.steps.Length)
        {
            Debug.LogWarning($"WARNING: Quest step prefabs & quest step states are of different lengths. This indicates something changed with" +
                $"the QuestObject and the saved data is now out of sync. Reset your data - as this may cause issues. QuestID: {this.info.uniqueID}");
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
            questStep.InitQuestStep(info.uniqueID, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
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

    private void SetupQuestPrefabs()
    {
        for (int i = 0; i < info.steps.Length; i++)
        {
            AssignStepDetails(info.steps[i], info.stepsInfo[i]);
        }
    }

    /// <summary>
    /// Assigns proper details to a STEP gameObject based on information from an action.
    /// </summary>
    /// <param name="obj">The STEP object that needs details.</param>
    /// <param name="action">The action details which are used.</param>
    private void AssignStepDetails(GameObject obj, QuestActions action)
    {
        // We need to go through every "QS_" script since they can vary
        switch (info.type)
        {
            case QuestType.Default:
                break;
            case QuestType.Kill:
                // Add the componenet
                QS_KillBots kb = obj.GetComponent<QS_KillBots>();
                // Set the values
                kb.stepDescription = action.stepDescription;

                kb.kill_faction = action.kill_faction;
                kb.kill_factionType = action.kill_factionType;
                kb.kill_class = action.kill_class;
                kb.kill_classType = action.kill_classType;
                kb.killAny = action.killAny;
                kb.kill_amount = action.amount;
                break;
            case QuestType.Collect:
                // Add the componenet
                QS_CollectItem ci = obj.GetComponent<QS_CollectItem>();
                // Set the values
                ci.stepDescription = action.stepDescription;

                ci.collect_specific = action.collect_specific;
                ci.collect_specificItem = action.collect_specificItem;
                ci.collect_byType = action.collect_byType;
                ci.collect_type = action.collect_type;
                ci.collect_byRank = action.collect_byRank;
                ci.collect_rank = action.collect_rank;
                ci.collect_bySlot = action.collect_bySlot;
                ci.collect_slot = action.collect_slot;
                ci.amount = action.amount;
                break;
            case QuestType.Find:
                // Add the componenet
                QS_GoToLocation gt = obj.GetComponent<QS_GoToLocation>();
                // Set the values
                gt.stepDescription = action.stepDescription;

                gt.find_specificLevel = action.find_specificLevel;
                gt.find_specific = action.find_specific;
                gt.find_specificLocation = action.find_specificLocation;
                gt.find_location = action.find_location;
                gt.find_locationSize = action.find_locationSize;
                gt.find_transform = action.find_transform;
                gt.find_InReferenceToPlayer = action.find_InReferenceToPlayer;
                break;
            case QuestType.Meet:
                // Add the componenet
                QS_MeetActor ma = obj.GetComponent<QS_MeetActor>();
                // Set the values
                ma.stepDescription = action.stepDescription;

                ma.meet_specificBot = action.meet_specificBot;
                ma.meet_specific = action.meet_specific;
                ma.meet_faction = action.meet_faction;
                ma.meet_factionBR = action.meet_factionBR;
                break;
            case QuestType.Destroy:
                // Add the componenet
                QS_DestroyThing dt = obj.GetComponent<QS_DestroyThing>();
                // Set the values
                dt.stepDescription = action.stepDescription;

                dt.destroy_specificMachine = action.destroy_specificMachine;
                dt.destroy_machine = action.destroy_machine;
                dt.destroy_specificObject = action.destroy_specificObject;
                dt.destroy_object = action.destroy_object;
                break;
        }

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
    public string shortDescription;
    [TextArea(3,5)] public string description;
    public Sprite sprite;

    [Tooltip("A unique name that this quest and this quest alone has (each quest in the world should have a unique one.")]
    public string uniqueID;

    [Header("Quest Details")]
    public QuestType type;
    public QuestRank rank;

    [Header("Requirements")]
    public Quest[] prerequisites;
    public List<ItemObject> prereq_items;
    public int prereq_matter;

    [Header("Steps")]
    [Tooltip("Prefabs of requried steps for this quest, each one is dynamically used when needed.")]
    public GameObject[] steps;
    [Tooltip("A ledger of detailed info for each step, will be assigned to the prefabs when needed.")]
    public List<QuestActions> stepsInfo;

    [Header("Rewards")]
    public List<Item> reward_items;
    public int reward_matter;

    [Header("Flair")]
    public Sprite questGiverSprite;
    public string questGiverName;
    public string questGiverDescription;
}

[System.Serializable]
[Tooltip("What kind of thing you will be doing during this quest.")]
public enum QuestType
{
    Default,
    Kill,    // Some number of a kind of bot
    Collect, // An item/items
    Find,    // A location
    Meet,    // An NPC
    Destroy  // A structure
}

[System.Serializable]
[Tooltip("The *rank* (or difficulty) of this quest.")]
public enum QuestRank
{
    Default,
    Easy,
    Medium,
    Hard,
    Difficult,
    Expert,
    Legendary
}

[System.Serializable]
[Tooltip("What you actually need to do during this quest.")]
public class QuestActions
{
    [Header("Step Description")]
    public string stepDescription;

    [Header("Amount")]
    public int amount = 1;
    
    [Header("Kill")]
    [Tooltip("Kill any bot of this faction")]
    public bool kill_faction = false;
    public BotAlignment kill_factionType;
    [Tooltip("Kill any bot of this class")]
    public bool kill_class;
    public BotClass kill_classType = BotClass.None;
    [Tooltip("Kill any HOSTILE bot.")]
    public bool killAny;

    [Header("Collect")]
    [Tooltip("Collect this specific item.")]
    public bool collect_specific;
    public ItemObject collect_specificItem;
    [Tooltip("Collect an item of this specified type.")]
    public bool collect_byType;
    public ItemType collect_type;
    [Tooltip("Collect an item within this range of ranking.")]
    public bool collect_byRank;
    public Vector2Int collect_rank;
    [Tooltip("Collect this specific item.")]
    public bool collect_bySlot;
    public ItemSlot collect_slot;

    [Header("Find")]
    [Tooltip("Reach a specific level.")]
    public bool find_specificLevel;
    public LevelName find_specific;
    [Tooltip("Reach a specific location on the map. Uses *find_specific (above) to determine where to spawn.")]
    public bool find_specificLocation;
    public Vector2 find_location;
    public Vector2 find_locationSize = new Vector2(1, 1);
    public Transform find_transform;
    [Tooltip("If true, the *find_transform* (seen above) automatically gets set to the player's transform.")]
    public bool find_InReferenceToPlayer;

    [Header("Meet")]
    [Tooltip("Meet a specific bot that has the same BotObject on it.")]
    public bool meet_specificBot;
    public BotObject meet_specific;
    [Tooltip("Meet any member of the specified faction.")]
    public bool meet_faction;
    public BotAlignment meet_factionBR;

    [Header("Destroy")]
    [Tooltip("Destroy a specific machine in the world (use parent object please).")]
    public bool destroy_specificMachine;
    public MachinePart destroy_machine;
    [Tooltip("Destroy a specific object (gameObject) somewhere in the world.")]
    public bool destroy_specificObject;
    public GameObject destroy_object;
}