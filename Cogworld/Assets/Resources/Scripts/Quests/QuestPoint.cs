// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A start or end point for a quest (given or assigned).
/// </summary>
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    public Quest questInfo;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;

    private string questID;
    private QuestState currentQuestState;

    [Header("Visuals")]
    [SerializeField] private Sprite sprite_available;
    [SerializeField] private SpriteRenderer sr;

    public void Init(Quest quest, bool isStart, bool isFinish)
    {
        questInfo = quest;
        questID = quest.info.uniqueID;

        startPoint = isStart;
        finishPoint = isFinish;

        if(isStart && !finishPoint)
        {
            Flash(true);
        }
    }

    private void OnEnable()
    {
        GameManager.inst.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameManager.inst.questEvents.onQuestStateChange -= QuestStateChange;
    }

    public void Interact()
    {
        // Attempt to start or finish this quest
        if(currentQuestState.Equals(QuestState.CAN_START) && startPoint)
        {
            GameManager.inst.questEvents.StartQuest(questID);
        }
        else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
        {
            GameManager.inst.questEvents.FinishQuest(questID);
        }
    }

    private void QuestStateChange(Quest quest)
    {
        // Only update the quest state if this point has the corresponding quest
        if(quest.info.Id.Equals(questID))
        {
            currentQuestState = quest.state;
        }
    }

    public void Flash(bool state)
    {
        if (state) // Flash
        {
            if(flashAnim != null)
            {
                StopCoroutine(flashAnim);
            }
            flashAnim = StartCoroutine(FlashAnim());
        }
        else // Don't flash
        {
            StopCoroutine(flashAnim);
            flashAnim = null;
        }
    }

    private Coroutine flashAnim;
    private IEnumerator FlashAnim()
    {
        while (true)
        {
            sr.enabled = true;

            yield return new WaitForSeconds(1);

            sr.enabled = false;

            yield return new WaitForSeconds(1);
        }
    }
}
