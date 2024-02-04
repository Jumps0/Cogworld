using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hack Database", menuName = "SO Systems/Hack/Database")]
public class HackDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public HackObject[] Hack; // Contains all Hack data that exists within the game.

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Hack.Length; i++)
        {
            if (Hack[i].Id != i)
                Hack[i].Id = i;
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
