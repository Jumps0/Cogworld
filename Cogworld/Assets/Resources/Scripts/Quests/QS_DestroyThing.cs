// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to destroy a specific physical object.
/// </summary>
public class QS_DestroyThing : QuestStep
{
    [Header("Destroy")]
    public int a_progress = 0;
    public int a_max = 1;
    [Tooltip("Destroy a specific machine in the world (use parent object please).")]
    public bool destroy_specificMachine;
    public MachinePart destroy_machine;
    [Tooltip("Destroy a specific object (gameObject) somewhere in the world.")]
    public bool destroy_specificObject;
    public GameObject destroy_object;

    // TODO: LOGIC HERE

    
    private Item itemToCollect;

    private void OnEnable()
    {
        //GameManager.inst.questEvents.onItemCollected += ItemCollected;
    }

    private void OnDisable()
    {
        //if (GameManager.inst)
            //GameManager.inst.questEvents.onItemCollected -= ItemCollected;
    }
    /*
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
        // Usually just 0/1 but may be more
        string state = progress.ToString();
        ChangeState(state);
    }
    

    protected override void SetQuestStepState(string state) // [EXPL]: USED TO TAKE PREVIOUSLY SAVED QUEST PROGRESS AND BRING IT IN TO A NEW INSTANCE OF A QUEST STEP. PARSE STRING TO <???>.
    {
        a_progress = System.Int32.Parse(state);
        UpdateState(a_progress);
    }
}
