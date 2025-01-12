using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "SO Systems/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] Items; // Contains all items that exists within the game.
    public Dictionary<string, ItemObject> dict;
    //public Dictionary<ItemObject, int> GetId = new Dictionary<ItemObject, int>();   // This is a memory vs performance choice.
    //public Dictionary<int, ItemObject> GetItem = new Dictionary<int, ItemObject>(); // 2 dictionaries = 2x memory | 2 for loops = 2x performance

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if(Items[i].data.Id != i)
                Items[i].data.Id = i;
        }
    }

    [ContextMenu("Set Default Part Knowledge")]
    public void SetDefaultPartKnowledge()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].rating > 2) // By default, anything higher than rating 2 is unknown.
            {
                Items[i].knowByPlayer = false;
            }
            else
            {
                Items[i].knowByPlayer = true;
            }
        }
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, ItemObject>();

        foreach (var v in Items)
        {
            dict.Add(v.itemName, v);
        }
    }

    public void OnAfterDeserialize()
    {
        UpdateIDs();
        SetDefaultPartKnowledge();
    }

    public void OnBeforeSerialize()
    {

    }
}
