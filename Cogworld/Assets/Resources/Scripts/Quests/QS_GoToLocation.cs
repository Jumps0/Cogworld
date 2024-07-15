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

    [Header("Find")]
    [Tooltip("Reach a specific level.")]
    public bool find_specificLevel;
    public LevelName find_specific;
    [Tooltip("Reach a specific location on the map. Uses *find_specific (above) to determine where to spawn.")]
    public bool find_specificLocation;
    public Vector2 find_location;
    public Vector2 find_locationSize = new Vector2(1, 1);
    public Transform find_transform;

    private void Start()
    {
        // Position the collider where it is needed
        if(find_transform != null) // If a reference object is given, that becomes the center point, and the *center* variable becomes an offset
        {
            col.offset = new Vector2(find_transform.position.x, find_transform.position.y) + find_location;
        }
        else
        {
            col.offset = find_location;
        }
        // And set its size
        col.size = find_locationSize;
    }

    private void Update()
    {
        if (find_specificLevel)
        {
            if(MapManager.inst.levelName == find_specific)
            {
                FinishQuestStep();
            }
        }
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
