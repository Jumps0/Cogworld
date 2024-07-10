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
    private int currentQuestStepIndex;
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
    public QuestActions actions;

    [Header("Requirements")]
    public Quest[] prerequisites;
    public List<ItemObject> prereq_items;
    public int prereq_matter;

    [Header("Steps")]
    public GameObject[] steps;

    [Header("Rewards")]
    public List<Item> reward_items;
    public int reward_matter;
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
    public Vector2 find_locationSize;
    public Transform find_transform;

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