// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to kill some amount of bots.
/// </summary>
public class QS_KillBots : QuestStep
{
    [Header("Details")]
    public int a_progress = 0;
    public int a_max = 1;
    [Tooltip("Kill any bot of this faction")]
    public bool kill_faction = false;
    public BotAlignment kill_factionType;
    [Tooltip("Kill any bot of this class")]
    public bool kill_class;
    public BotClass kill_classType = BotClass.None;
    [Tooltip("Kill any HOSTILE bot.")]
    public bool killAny;

    private int startingStat = 0;

    private void Start()
    {
        int startCount = 0;

        // Start tracking the amount we need to keep an eye on based on the player's stats
        if (kill_faction)
        {
            foreach (var bot in PlayerData.inst.robotsKilledAlignment)
            {
                if (bot == kill_factionType)
                {
                    startCount++;
                }
            }
            string factionDesc = "";
            switch (kill_factionType)
            {
                case BotAlignment.Complex:
                    factionDesc = "bots belonging to 0b10";
                    break;
                case BotAlignment.Derelict:
                    factionDesc = "derelict bots";
                    break;
                case BotAlignment.Assembled:
                    factionDesc = "of the Assembled";
                    break;
                case BotAlignment.Warlord:
                    factionDesc = "of Warlord's allies";
                    break;
                case BotAlignment.Zion:
                    factionDesc = "Zionites";
                    break;
                case BotAlignment.Exiles:
                    factionDesc = "of the Exiles";
                    break;
                case BotAlignment.Architect:
                    factionDesc = "belonging to [REDACTED]";
                    break;
                case BotAlignment.Subcaves:
                    factionDesc = "bots dwelling in the Subcaves";
                    break;
                case BotAlignment.SubcavesHostile:
                    factionDesc = "hostile dwellers in the Subcaves";
                    break;
                case BotAlignment.Player:
                    factionDesc = "of your own friends";
                    break;
                case BotAlignment.None:
                    factionDesc = "of any type";
                    break;
                default:
                    break;
            }
            stepDescription = $"Kill {a_max} {factionDesc}.";
        }
        else if(kill_class)
        {
            foreach (var bot in PlayerData.inst.robotsKilledData)
            {
                if (bot._class == kill_classType)
                {
                    startCount++;
                }
            }
            stepDescription = $"Kill {a_max}-type bots.";
        }
        else if(killAny)
        {
            startCount = PlayerData.inst.robotsKilled;
            stepDescription = $"Kill {a_max} bots of any type.";
        }
        startingStat = startCount;
    }

    private void OnEnable()
    {
        GameManager.inst.questEvents.onBotsKilled += BotsKilled;
    }

    private void OnDisable()
    {
        if (GameManager.inst)
            GameManager.inst.questEvents.onBotsKilled -= BotsKilled;
    }

    private void BotsKilled() // [EXPL]: THIS "EVENT" STEP WILL KEEP CHECKING TO SEE IF THIS QUEST SHOULD BE COMPLETED
    {
        if (kill_faction)
        {
            foreach (var bot in PlayerData.inst.robotsKilledAlignment)
            {
                if (bot == kill_factionType)
                {
                    a_progress++;
                }
            }
        }
        else if (kill_class)
        {
            foreach (var bot in PlayerData.inst.robotsKilledData)
            {
                if (bot._class == kill_classType)
                {
                    a_progress++;
                }
            }
        }
        else if (killAny)
        {
            a_progress = PlayerData.inst.robotsKilled;
        }

        UpdateState(a_progress);

        if (a_progress - startingStat >= a_max)
        {
            FinishQuestStep();
        }
    }

    private void UpdateState(int progress) // [EXPL]: THIS FUNCTION SAVES THE CURRENT *PROGRESS* THE PLAYER HAS MADE ON THIS QUEST. NEEDS TO BE CALLED ANY TIME THE "STATE" (aka Progress) CHANGES.
    {
        string state = progress.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string state) // [EXPL]: USED TO TAKE PREVIOUSLY SAVED QUEST PROGRESS AND BRING IT IN TO A NEW INSTANCE OF A QUEST STEP. PARSE STRING TO <???>.
    {
        a_progress = System.Int32.Parse(state);
        UpdateState(a_progress);
    }
}
