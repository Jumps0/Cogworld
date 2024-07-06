// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    private bool isFinished = false;

    protected void FinishQuestStep()
    {
        if(!isFinished)
        {
            isFinished = true;

            // TODO - Advance the quest forward now that this step is done

            Destroy(this.gameObject);
        }
    }
}
