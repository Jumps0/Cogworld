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
    public int kill_amount;
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
        // Assign information based on the serialized object
        foreach (var Q in QuestManager.inst.questDatabase.Quests)
        {
            if (questID.Contains(Q.uniqueID) || questID == Q.uniqueID)
            {
                kill_amount = Q.actions.amount;
                kill_faction = Q.actions.kill_faction;
                kill_factionType = Q.actions.kill_factionType;
                kill_class = Q.actions.kill_class;
                kill_classType = Q.actions.kill_classType;
                killAny = Q.actions.killAny;
                break;
            }
        }

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
            stepDescription = $"Kill {kill_amount} {factionDesc}.";
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
            stepDescription = $"Kill {kill_amount}-type bots.";
        }
        else if(killAny)
        {
            startCount = PlayerData.inst.robotsKilled;
            stepDescription = $"Kill {kill_amount} bots of any type.";
        }
        startingStat = startCount;
    }

    private void OnEnable()
    {
        GameManager.inst.questEvents.onBotsKilled += BotsKilled;
    }

    private void OnDisable()
    {
        GameManager.inst.questEvents.onBotsKilled -= BotsKilled;
    }

    private void BotsKilled()
    {
        int amount = 0;
        if (kill_faction)
        {
            foreach (var bot in PlayerData.inst.robotsKilledAlignment)
            {
                if (bot == kill_factionType)
                {
                    amount++;
                }
            }
        }
        else if (kill_class)
        {
            foreach (var bot in PlayerData.inst.robotsKilledData)
            {
                if (bot._class == kill_classType)
                {
                    amount++;
                }
            }
        }
        else if (killAny)
        {
            amount = PlayerData.inst.robotsKilled;
        }
        
        if(amount - startingStat >= kill_amount)
        {
            FinishQuestStep();
        }
    }

    private void UpdateState()
    {
        //string state = itemToCollect.ToString();
        //ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        // ???
    }
}
