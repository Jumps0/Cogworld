// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A quest step that requires the player to meet a specific bot.
/// </summary>
public class QS_MeetActor : QuestStep
{
    [Header("Meet")]
    public int a_progress = 0;
    public int a_max = 1;
    [Tooltip("Meet a specific bot that has the same BotObject on it.")]
    public bool meet_specificBot;
    public BotObject meet_specific;
    public string meet_specificName;
    private Actor target_actor;
    [Tooltip("Meet any member of the specified faction.")]
    public bool meet_faction;
    public BotAlignment meet_factionBR;

    private void OnEnable()
    {
        GameManager.inst.questEvents.onActorMet += ActorMet;
    }

    private void OnDisable()
    {
        if (GameManager.inst)
            GameManager.inst.questEvents.onActorMet -= ActorMet;
    }

    private void Start()
    {
        if (meet_faction)
        {
            stepDescription = $"Meet any member of the {meet_factionBR.ToString()} faction.";
            // Is anyone we can meet on this level?
            foreach (Entity E in GameManager.inst.Entities)
            {
                if (E.GetComponent<Actor>().myFaction == meet_factionBR)
                {
                    validFactionMembers.Add(E.GetComponent<Actor>());
                    targetsPresent = true;
                }
            }
        }
        else if (meet_specificBot)
        {
            // First try and find the actor in world
            foreach (Entity E in GameManager.inst.Entities)
            {
                if (E.GetComponent<Actor>().uniqueName == meet_specificName)
                {
                    target_actor = E.GetComponent<Actor>();
                    targetsPresent = true;
                    break;
                }
            }

            stepDescription = $"Meet {target_actor.uniqueName}";
        }
    }

    [Tooltip("Is someone we can meet actually on this map?")]
    private bool targetsPresent = false;
    private List<Actor> validFactionMembers = new List<Actor>();

    private void ActorMet() // [EXPL]: THIS "EVENT" STEP WILL KEEP CHECKING TO SEE IF THIS QUEST SHOULD BE COMPLETED
    {
        // Can we even meet anyone here that we are looking for?
        if (targetsPresent)
        {
            Vector3Int position = Vector3Int.zero;

            if (meet_faction)
            {
                // This has a little bit more overhead than i'd like, this could get bad if there are a lot of valid targets
                foreach (var A in validFactionMembers)
                {
                    // Check to see if the bot we are looking for is within the player's FOV
                    position = new Vector3Int((int)target_actor.transform.position.x, (int)target_actor.transform.position.y, (int)target_actor.transform.position.z);
                    if (PlayerData.inst.GetComponent<Actor>().FieldofView.Contains(position))
                    {
                        // We can see the bot, and have now met them. Mission complete.
                        UpdateState(1);
                        FinishQuestStep();
                    }
                }
            }
            else if (meet_specificBot)
            {
                // Check to see if the bot we are looking for is within the player's FOV
                position = new Vector3Int((int)target_actor.transform.position.x, (int)target_actor.transform.position.y, (int)target_actor.transform.position.z);
                if (PlayerData.inst.GetComponent<Actor>().FieldofView.Contains(position))
                {
                    // We can see the bot, and have now met them. Mission complete.
                    UpdateState(1);
                    FinishQuestStep();
                }
            }
        }
    }
    
    private void UpdateState(int progress) // [EXPL]: THIS FUNCTION SAVES THE CURRENT *PROGRESS* THE PLAYER HAS MADE ON THIS QUEST. NEEDS TO BE CALLED ANY TIME THE "STATE" (aka Progress) CHANGES.
    {
        // No progress save needed since its true/false if the player has this. We will set it to 0 or 1.
        string state = progress.ToString();
        ChangeState(state);
    }
    

    protected override void SetQuestStepState(string state) // [EXPL]: USED TO TAKE PREVIOUSLY SAVED QUEST PROGRESS AND BRING IT IN TO A NEW INSTANCE OF A QUEST STEP. PARSE STRING TO <???>.
    {
        a_progress = System.Int32.Parse(state);
        UpdateState(a_progress);
    }
}
