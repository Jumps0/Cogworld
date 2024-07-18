// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A start or end point for a quest (given or assigned).
/// </summary>
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    public Quest quest;
    private QuestPointInfo info;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;

    private string questID;
    private QuestState currentQuestState;

    [Header("Visuals")]
    [SerializeField] private Sprite sprite_available;
    [SerializeField] private SpriteRenderer sr;
    public bool isExplored = false;
    public bool isVisible = false;

    public void Init(Quest quest, bool isStart, bool isFinish, QuestPointInfo info)
    {
        this.quest = quest;
        questID = quest.info.uniqueID;

        startPoint = isStart;
        finishPoint = isFinish;

        this.info = info;

        Visibility();
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
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.ROBOTHACK_Clips[0], 0.6f); // Play a sound
        }
        else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
        {
            GameManager.inst.questEvents.FinishQuest(questID);
            AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[1], 0.6f); // Play a sound
        }
    }

    private void QuestStateChange(Quest quest)
    {
        // Only update the quest state if this point has the corresponding quest
        if(quest.info.uniqueID.Equals(questID))
        {
            currentQuestState = quest.state;
        }
    }

    public void Visibility()
    {
        StartCoroutine(FlashAnim());
    }

    public bool CanInteract()
    {
        return ((currentQuestState.Equals(QuestState.CAN_START) && startPoint) || (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint));
    }

    private IEnumerator FlashAnim()
    {
        while (true)
        {
            if (CanInteract())
            {
                sr.enabled = true;

                yield return new WaitForSeconds(1);

                sr.enabled = false;

                yield return new WaitForSeconds(1);
            }
            else
            {
                sr.enabled = false;
                yield return null;
            }
        }
    }

    public void CheckVisibility()
    {

        if (isVisible)
        {
            sr.color = Color.white;
        }
        else if (isExplored && isVisible)
        {
            sr.color = Color.white;
        }
        else if (isExplored && !isVisible)
        {
            sr.color = Color.gray;
        }
        else if (!isExplored)
        {
            sr.color = Color.black;
        }
    }
}
