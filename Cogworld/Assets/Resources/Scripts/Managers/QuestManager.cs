using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all things quest.
/// -Quest Creation
/// -Quest Assignment
/// -Quest Tracking
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager inst;
    public void Awake()
    {
        inst = this;
    }

    public List<PlayerQuest> allQuests = new List<PlayerQuest>();

    public void CreateQuest()
    {

    }

    public void AssignQuest()
    {

    }

    public void CompleteQuest()
    {

    }

    public void AbortQuest()
    {

    }

    public void FailQuest()
    {

    }

    public void QuestReward()
    {

    }

}

public abstract class PlayerQuest
{
    public List<QuestTask> tasks = new List<QuestTask>();
}

public abstract class QuestTask
{
    public QuestState _state;
    // Todo
}

public enum QuestState
{
    Available, // This quest exists, and the player can choose to take it
    Active,    // Player is actively doing this quest & it has been assigned to them
    Complete,  // The player has completed this quest
    Cancelled, // The player cancelled this quest. Log it for now
    Failed,    // The player failed this quest
    Default
}
