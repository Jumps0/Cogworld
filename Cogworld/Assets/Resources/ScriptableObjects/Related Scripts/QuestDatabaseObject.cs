using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Database", menuName = "SO Systems/Quests/Database")]
public class QuestDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public QuestObject[] Quests; // Contains all quests.

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Quests.Length; i++)
        {
            if (Quests[i].Id != i)
                Quests[i].Id = i;
        }
    }

    public void OnAfterDeserialize()
    {
        UpdateIDs();
    }

    public void OnBeforeSerialize()
    {

    }
}
