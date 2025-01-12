using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bot Database", menuName = "SO Systems/Bots/Database")]
public class BotDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public BotObject[] Bots; // Contains all bots that exists within the game.
    public Dictionary<string, BotObject> dict;

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Bots.Length; i++)
        {
            if (Bots[i].Id != i)
                Bots[i].Id = i;
        }
    }

    [ContextMenu("Set Default Bot Knowledge")]
    public void SetDefaultBotKnowledge()
    {
        for (int i = 0; i < Bots.Length; i++)
        {
            // Unsure about this?
            /*
            if (Bots[i].rating > 1) // By default, anything higher than rating 1 is unknown.
            {
                Bots[i].playerHasAnalysisData = false;
            }
            else
            {
                Bots[i].playerHasAnalysisData = true;
            }
            */
            Bots[i].playerHasAnalysisData = false;
        }
    }

    public void OnAfterDeserialize()
    {
        UpdateIDs();
        SetDefaultBotKnowledge();
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, BotObject>();

        foreach (var v in Bots)
        {
            dict.Add(v.botName, v);
        }
    }

    public void OnBeforeSerialize()
    {

    }
}
