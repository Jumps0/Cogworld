using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Knowledge Database", menuName = "SO Systems/Knowledge/Database")]
public class KnowledgeDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public KnowledgeObject[] Data; // Contains all knowledge data that exists within the game.

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Data.Length; i++)
        {
            if (Data[i].Id != i)
                Data[i].Id = i;
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
