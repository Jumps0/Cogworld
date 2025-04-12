using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hack Dialogue", menuName = "SO Systems/Dialogue/Database")]
public class DialogueDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public DialogueObject[] Dialogue; // Contains all Dialogue interactions that exists within the game.
    public Dictionary<string, DialogueObject> dict;

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Dialogue.Length; i++)
        {
            if (Dialogue[i].Id != i)
                Dialogue[i].Id = i;
        }
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, DialogueObject>();

        foreach(var D in Dialogue)
        {
            dict.Add(D.conversationName, D);
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
