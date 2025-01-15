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
    [Header("-- Destroy Generic")]
    [Tooltip("If true, will attempt to find a machine in world of the specified type to destroy.")]
    public bool destroy_isGeneric;
    public MachineType destroy_machtype;


    private void OnEnable()
    {
        GameManager.inst.questEvents.onItemCollected += CheckForDestruction;
    }

    private void OnDisable()
    {
        if (GameManager.inst)
            GameManager.inst.questEvents.onItemCollected -= CheckForDestruction;
    }
    
    private void Start()
    {
        if (destroy_isGeneric)
        {
            // Just find a machine in world to destroy
            destroy_machine = HF.GetRandomMachineOfType(destroy_machtype).GetComponent<MachinePart>();
        }

        if (destroy_specificMachine)
        {
            string name = HF.GetMachineTypeAsString(destroy_machine.GetComponent<InteractableMachine>());
            stepDescription = $"Locate and destroy {name}.";
        }
        else if (destroy_specificObject)
        {
            stepDescription = $"Locate and destroy {destroy_object.name}.";
        }
    }

    private void CheckForDestruction() // [EXPL]: THIS "EVENT" STEP WILL KEEP CHECKING TO SEE IF THIS QUEST SHOULD BE COMPLETED
    {
        bool complete = false;

        if (destroy_specificMachine)
        {
            if(destroy_machine == null || destroy_machine.destroyed)
            {
                complete = true;
            }
        }
        else if (destroy_specificObject)
        {
            if(destroy_object == null)
            {
                complete = true;
            }
        }

        if (complete)
        {
            UpdateState(1);
            FinishQuestStep();
        }
    }

    
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
