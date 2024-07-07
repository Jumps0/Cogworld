// This script was orginally based on a tutorial by "trevermock": https://www.youtube.com/watch?v=UyTJLDGcT64
// Modified & Expanded by: Cody Jackson

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class QuestEvents
{
    #region General Events
    public event System.Action<string> onStartQuest;
    public void StartQuest(string id)
    {
        if(onStartQuest != null)
        {
            onStartQuest(id);
        }
    }

    public event System.Action<string> onAdvanceQuest;
    public void AdvanceQuest(string id)
    {
        if (onAdvanceQuest != null)
        {
            onAdvanceQuest(id);
        }
    }

    public event System.Action<string> onFinishQuest;
    public void FinishQuest(string id)
    {
        if (onFinishQuest != null)
        {
            onFinishQuest(id);
        }
    }

    public event System.Action<Quest> onQuestStateChange;
    public void QuestStateChange(Quest quest)
    {
        if (onQuestStateChange != null)
        {
            onQuestStateChange(quest);
        }
    }

    public event System.Action<string, int, QuestStepState> onQuestStepStateChange;
    public void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        if (onQuestStepStateChange != null)
        {
            onQuestStepStateChange(id, stepIndex, questStepState);
        }
    }
    #endregion

    #region Individual Quest Events
    public event System.Action onItemCollected;
    public event System.Action onLocationReached;

    public void ItemCollected()
    {
        if(onItemCollected != null)
            onItemCollected();
    }

    public void LocationReached()
    {
        if (onLocationReached != null)
            onLocationReached();
    }
    #endregion
}
