using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Database", menuName = "SO Systems/Tile/Database")]
public class TileDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public TileObject[] Tiles; // Contains all Tiles that exists within the game.
    public Dictionary<string, TileObject> dict;

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Tiles.Length; i++)
        {
            if (Tiles[i].Id != i)
                Tiles[i].Id = i;
        }
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, TileObject>();

        foreach (var v in Tiles)
        {
            dict.Add(v.tileName, v);
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
