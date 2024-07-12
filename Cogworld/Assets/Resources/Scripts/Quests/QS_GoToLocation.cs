// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to go to a specific location.
/// </summary>
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

        //stepDescription = // TODO: Set description 
    }

    private void OnEnable()
    {
        GameManager.inst.questEvents.onLocationReached += LocationReached;
    }

    private void OnDisable()
    {
        GameManager.inst.questEvents.onLocationReached -= LocationReached;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We just need to check to see if the Player has reached the specified destination
        if (collision.gameObject.GetComponent<PlayerData>())
        {
            LocationReached(); // Trigger the location reached function
            col.enabled = false; // And disable the collider since it is no longer needed
        }
    }

    private void LocationReached()
    {
        FinishQuestStep();
    }

    private void UpdateState()
    {
        //string state = itemToCollect.ToString();
        //ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        // No state is needed for this quest step
    }
}
