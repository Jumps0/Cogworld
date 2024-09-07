// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Meet any member of the specified faction.")]
    public bool meet_faction;
    public BotAlignment meet_factionBR;

    // TOOD: LOGIC HERE

    /*
    private Item itemToCollect;

    private void OnEnable()
    {
        GameManager.inst.questEvents.onItemCollected += ItemCollected;
    }

    private void OnDisable()
    {
        if (GameManager.inst)
            GameManager.inst.questEvents.onItemCollected -= ItemCollected;
    }

    private void Start()
    {
        stepDescription = $"Find and collect: {itemToCollect.Name}";
    }

    private void ItemCollected() // [EXPL]: THIS "EVENT" STEP WILL KEEP CHECKING TO SEE IF THIS QUEST SHOULD BE COMPLETED
    {
        // Check if the player actually has the item (in their inventory)
        foreach (var slot in PlayerData.inst.GetComponent<PartInventory>()._inventory.Container.Items)
        {
            if(slot.item != null && slot.item.Id >= 0) // An item exists here
            {
                if(slot.item == itemToCollect)
                {
                    FinishQuestStep(); // They have it! Finish this step
                    break;
                }
            }
        }
    }
    */
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
