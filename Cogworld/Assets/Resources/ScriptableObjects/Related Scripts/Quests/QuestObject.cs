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
    public List<bool> completedSteps = new List<bool>();

    [Header("Progress")]
    public int a_max;
    public int a_progress;

    public Quest(QuestObject info, string uid = "")
    {
        this.info = info;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;

        info.reward_claimed = false;
        info.uniqueID = uid;
        if(uid == "")
        {
            info.uniqueID = $"{info.name} + {Random.Range(0, 99)}";
        }

        SetupQuestPrefabs(); // Setup the step prefabs
        SetQuestProgressMax(); // Set "max" value

        this.questStepStates = new QuestStepState[info.steps.Length];
        for(int i = 0; i < info.steps.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
            completedSteps.Add(false);
        }
    }

    public Quest(QuestObject info, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates, List<bool> completedSteps)
    {
        this.info = info;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;
        this.completedSteps = completedSteps;

        if (this.questStepStates.Length != this.info.steps.Length)
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

    public void SetQuestProgressMax()
    {
        // We need to parse the quest steps into a number.
        // -Sometimes a quest just boils down to doing one thing, so its 1/1
        // -Sometimes a quest requires doing a number of one thing, so we add all those up
        // -Sometimes a quest actually gives a number, so we use that.

        int max = 1;

        // First, go through each step and categorize them
        List<GameObject> steps_progressive = new List<GameObject>(); // Steps where you need to do some amount of something
        List<GameObject> steps_static = new List<GameObject>(); // Steps that are just 0/1 where you ONLY DO ONE THING

        // Go through each step and work out the max and current from there
        #region Sort Steps
        foreach (var step in info.steps)
        {
            if (step.GetComponent<QS_MeetActor>()) // static
            {
                steps_static.Add(step);
            }
            else if (step.GetComponent<QS_GoToLocation>()) // static
            {
                steps_static.Add(step);
            }
            else if (step.GetComponent<QS_DestroyThing>()) // usually static  (but may be progressive)
            {
                if (step.GetComponent<QS_DestroyThing>().a_max > 1)
                {
                    steps_progressive.Add(step);
                }
                else
                {
                    steps_static.Add(step);
                }
            }
            else if (step.GetComponent<QS_CollectItem>()) // usually static  (but may be progressive)
            {
                if (step.GetComponent<QS_CollectItem>().a_max > 1)
                {
                    steps_progressive.Add(step);
                }
                else
                {
                    steps_static.Add(step);
                }
            }
            else if (step.GetComponent<QS_KillBots>()) // progressive
            {
                steps_progressive.Add(step);
            }
        }
        #endregion

        // Then based on that, define an actual number we can use for our bar
        #region Determine Max
        if (steps_static.Count > 0 && steps_progressive.Count == 0) // Do we only have static steps? Add them all up and use that
        {
            max = steps_static.Count;
        }
        else if (steps_static.Count == 0 && steps_progressive.Count > 0) // Do we only have progressive steps? Add them all up and use those
        {
            foreach (var step in info.steps)
            {
                if (step.GetComponent<QS_DestroyThing>()) // usually static (but may be progressive)
                {
                    if (step.GetComponent<QS_DestroyThing>().a_max > 1)
                    {
                        max += step.GetComponent<QS_DestroyThing>().a_max;
                    }
                }
                else if (step.GetComponent<QS_CollectItem>()) // usually static  (but may be progressive)
                {
                    if (step.GetComponent<QS_CollectItem>().a_max > 1)
                    {
                        max += step.GetComponent<QS_CollectItem>().a_max;
                    }
                }
                else if (step.GetComponent<QS_KillBots>()) // progressive
                {
                    max += step.GetComponent<QS_KillBots>().a_max;
                }
            }
        }
        else if (steps_static.Count > 0 && steps_progressive.Count > 0) // Do we have a mix of both? Use only the progressive steps (and add them all up to use)
        {
            foreach (var step in info.steps)
            {
                if (step.GetComponent<QS_DestroyThing>()) // usually static (but may be progressive)
                {
                    if (step.GetComponent<QS_DestroyThing>().a_max > 1)
                    {
                        max += step.GetComponent<QS_DestroyThing>().a_max;
                    }
                }
                else if (step.GetComponent<QS_CollectItem>()) // usually static  (but may be progressive)
                {
                    if (step.GetComponent<QS_CollectItem>().a_max > 1)
                    {
                        max += step.GetComponent<QS_CollectItem>().a_max;
                    }
                }
                else if (step.GetComponent<QS_KillBots>()) // progressive
                {
                    max += step.GetComponent<QS_KillBots>().a_max;
                }
            }
        }
        #endregion

        // Debug.Log($"Progress update: max: {max} | current: {current} \n Steps: {info.steps.Length} | Prog: {steps_progressive.Count} | Static: {steps_static.Count}");

        // Set max
        a_max = max;
    }

    public void UpdateQuestProgress()
    {
        int progress = 0;

        // (Used later)
        List<KeyValuePair<bool, int>> current_progress = new List<KeyValuePair<bool, int>>(); // True = Progressive | False = Static

        // We can use the *questStepStates* list (which gets updated dynamically by our scripts whenever progress is made) to determine our overall progress.
        for (int i = 0; i < questStepStates.Length; i++)
        {
            string state = questStepStates[i].state;
            if (state == "") // Failsafe
                state = "0";
            int current = System.Int32.Parse(state); // Get the value (and convert it to int)

            // However, should we consider this value as true progress? We don't actually know what kind of quest this belongs to.
            // But thankfully since the order doesn't change, and the list length is identical, we can go through our list of
            // quest step "objects", and do the same thing we do about to determine the max.

            #region Sort Steps
            GameObject step = info.steps[i];
            if (step.GetComponent<QS_MeetActor>()) // static
            {
                current_progress.Add(new KeyValuePair<bool, int>(false, current));
            }
            else if (step.GetComponent<QS_GoToLocation>()) // static
            {
                current_progress.Add(new KeyValuePair<bool, int>(false, current));
            }
            else if (step.GetComponent<QS_DestroyThing>()) // usually static  (but may be progressive)
            {
                if (step.GetComponent<QS_DestroyThing>().a_max > 1)
                {
                    current_progress.Add(new KeyValuePair<bool, int>(true, current));
                }
                else
                {
                    current_progress.Add(new KeyValuePair<bool, int>(false, current));
                }
            }
            else if (step.GetComponent<QS_CollectItem>()) // usually static  (but may be progressive)
            {
                if (step.GetComponent<QS_CollectItem>().a_max > 1)
                {
                    current_progress.Add(new KeyValuePair<bool, int>(true, current));
                }
                else
                {
                    current_progress.Add(new KeyValuePair<bool, int>(false, current));
                }
            }
            else if (step.GetComponent<QS_KillBots>()) // progressive
            {
                current_progress.Add(new KeyValuePair<bool, int>(true, current));
            }
            #endregion

        }

        // Now based on our list with added detail, we can determine the current progress using the rules we established when calculating the max value.
        int count_static = 0;
        int count_prog = 0;
        foreach (var kvp in current_progress) // First determine how much of each we have
        {
            bool isProgressive = kvp.Key;

            if(isProgressive)
            {
                count_prog++;
            }
            else
            {
                count_static++;
            }
        }

        #region Determine Current Progress
        // Then act on that
        if (count_static > 0 && count_prog == 0) // Do we only have static steps? Add them all up and use that
        {
            foreach (var kvp in current_progress)
            {
                if(kvp.Key == false)
                {
                    progress += kvp.Value;
                }
            }
        }
        else if (count_static == 0 && count_prog > 0) // Do we only have progressive steps? Add them all up and use those
        {
            foreach (var kvp in current_progress)
            {
                if (kvp.Key == true)
                {
                    progress += kvp.Value;
                }
            }
        }
        else if (count_static > 0 && count_prog > 0) // Do we have a mix of both? Use only the progressive steps (and add them all up to use)
        {
            foreach (var kvp in current_progress)
            {
                if (kvp.Key == true)
                {
                    progress += kvp.Value;
                }
            }
        }
        #endregion

        // Set progress
        a_progress = progress;
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
                kb.a_max = action.amount_max;

                kb.kill_faction = action.kill_faction;
                kb.kill_factionType = action.kill_factionType;
                kb.kill_class = action.kill_class;
                kb.kill_classType = action.kill_classType;
                kb.killAny = action.killAny;
                break;
            case QuestType.Collect:
                // Add the componenet
                QS_CollectItem ci = obj.GetComponent<QS_CollectItem>();
                // Set the values
                ci.stepDescription = action.stepDescription;
                ci.a_max = action.amount_max;

                ci.collect_specific = action.collect_specific;
                ci.collect_specificItem = action.collect_specificItem;
                ci.collect_byType = action.collect_byType;
                ci.collect_type = action.collect_type;
                ci.collect_byRank = action.collect_byRank;
                ci.collect_rank = action.collect_rank;
                ci.collect_bySlot = action.collect_bySlot;
                ci.collect_slot = action.collect_slot;
                break;
            case QuestType.Find:
                // Add the componenet
                QS_GoToLocation gt = obj.GetComponent<QS_GoToLocation>();
                // Set the values
                gt.stepDescription = action.stepDescription;
                gt.a_max = action.amount_max;

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
                ma.a_max = action.amount_max;

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
                dt.a_max = action.amount_max;

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

        UpdateQuestProgress();
    }

    public QuestData GetQuestData()
    {
        return new QuestData(state, currentQuestStepIndex, questStepStates, completedSteps);
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
    public QuestPointInfo startLocation;
    [Tooltip("If this is set to null, then the quest will start AND end at the same location.")]
    public QuestPointInfo finishLocation;

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
    public List<ItemObject> reward_items;
    public int reward_matter;
    public bool reward_claimed = false;

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
    public int amount_max = 1;
    
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

[System.Serializable]
[Tooltip("Contains information about where questpoints should be placed.")]
public class QuestPointInfo
{
    [Header("Start & Finish")]
    [Tooltip("If true, this point acts as both the START & FINISH point.")]
    public bool isStartAndFinish = false;

    [Header("Bot")]
    public BotObject assignedBot;

    [Header("Reference Point")]
    public Transform refpoint;
    public Vector2 refpoint_offset = new Vector2(0, 0);

    [Tooltip("Location will be placed in reference to where the player is CURRENTLY located. Be careful with this.")]
    public bool inReferenceToPlayer = false;
}