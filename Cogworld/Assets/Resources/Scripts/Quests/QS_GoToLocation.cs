// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A quest step that requires the player to go to a specific location.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class QS_GoToLocation : QuestStep
{
    public int a_progress = 0;
    public int a_max = 1;

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
    public bool find_InReferenceToPlayer;

    private void Start()
    {
        if(find_InReferenceToPlayer)
        {
            find_transform = PlayerData.inst.transform;
        }

        if(col == null)  // Add a collider if there isn't one already
        {
            this.AddComponent<BoxCollider2D>();
            col = this.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

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

        UpdateState(0);
    }

    private void Update()
    {
        if (find_specificLevel)
        {
            if(MapManager.inst.levelName == find_specific)
            {
                a_progress = a_max;
                UpdateState(1);
                FinishQuestStep();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We just need to check to see if the Player has reached the specified destination
        if (collision.gameObject.GetComponent<PlayerData>())
        {
            UpdateState(1);
            FinishQuestStep();
            col.enabled = false; // And disable the collider since it is no longer needed
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
