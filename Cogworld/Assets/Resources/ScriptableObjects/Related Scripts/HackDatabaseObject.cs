using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hack Database", menuName = "SO Systems/Hack/Database")]
public class HackDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public HackObject[] Hack; // Contains all Hack data that exists within the game.
    public Dictionary<string, HackObject> dict;

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Hack.Length; i++)
        {
            if (Hack[i].Id != i)
                Hack[i].Id = i;
        }
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, HackObject>();

        foreach(var hack in Hack)
        {
            dict.Add(hack.name, hack); // Doesn't use trueName because that is used for a special purpose.
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
