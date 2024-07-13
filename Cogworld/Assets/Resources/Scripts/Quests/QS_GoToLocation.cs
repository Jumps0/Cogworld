// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to go to a specific location.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class QS_GoToLocation : QuestStep
{
    [SerializeField] private BoxCollider2D col;
    public Vector2 center;
    [SerializeField] private Vector2 size = new Vector2(1, 1);

    [Header("In reference to")]
    [SerializeField] private bool inReferenceToPlayer = false;
    public GameObject referenceObject = null;

    private void Start()
    {
        // Assign information based on the serialized object
        QuestObject baseObj = null;
        foreach (var Q in QuestManager.inst.questDatabase.Quests)
        {
            if (questID.Contains(Q.uniqueID) || questID == Q.uniqueID)
            {
                size = Q.actions.find_locationSize;
                center = Q.actions.find_location;
                baseObj = Q;
                break;
            }
        }

        // Position the collider where it is needed
        if (inReferenceToPlayer)
        {
            col.offset = new Vector2(PlayerData.inst.transform.position.x, PlayerData.inst.transform.position.y) + center;
        }
        else if(referenceObject != null) // If a reference object is given, that becomes the center point, and the *center* variable becomes an offset
        {
            col.offset = new Vector2(referenceObject.transform.position.x, referenceObject.transform.position.y) + center;
        }
        else
        {
            col.offset = center;
        }
        // And set its size
        col.size = size;

        stepDescription = baseObj.shortDescription;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We just need to check to see if the Player has reached the specified destination
        if (collision.gameObject.GetComponent<PlayerData>())
        {
            FinishQuestStep();
            col.enabled = false; // And disable the collider since it is no longer needed
        }
    }

    protected override void SetQuestStepState(string state) // [EXPL]: USED TO TAKE PREVIOUSLY SAVED QUEST PROGRESS AND BRING IT IN TO A NEW INSTANCE OF A QUEST STEP. PARSE STRING TO <???>.
    {
        // No state is needed for this quest step
    }
}
