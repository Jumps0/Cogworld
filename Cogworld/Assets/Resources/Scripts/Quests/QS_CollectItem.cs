// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to collect an item.
/// </summary>
public class QS_CollectItem : QuestStep
{
    private Item itemToCollect;

    private void OnEnable()
    {
        GameManager.inst.questEvents.onItemCollected += ItemCollected;
    }

    private void OnDisable()
    {
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

    private void UpdateState() // [EXPL]: THIS FUNCTION SAVES THE CURRENT *PROGRESS* THE PLAYER HAS MADE ON THIS QUEST. NEEDS TO BE CALLED ANY TIME THE "STATE" (aka Progress) CHANGES.
    {
        // No progress save needed(?) since its true/false if the player has this.
        //string state = itemToCollect.ToString();
        //ChangeState(state);
    }

    protected override void SetQuestStepState(string state) // [EXPL]: USED TO TAKE PREVIOUSLY SAVED QUEST PROGRESS AND BRING IT IN TO A NEW INSTANCE OF A QUEST STEP. PARSE STRING TO <???>.
    {
        /* // Not needed since its just true false (?)
        // Convert *itemToCollect* (string) back to an actual item
        ItemObject parsed = null;
        foreach(var I in InventoryControl.inst._itemDatabase.Items)
        {
            if (I.name == state || I.name.Contains(state))
            {
                parsed = I;
                break;
            }
        }
        // Do something?
        UpdateState();
        */
    }
}
