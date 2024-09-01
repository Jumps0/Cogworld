using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HideoutData
{
    [Header("Hideout Data")]
    // - Location Data
    public int layer;
    public string layerName;
    public int mapSeed;
    public int branchValue;
    // - Storage (Cache) Data
    public int storedMatter;
    // Inventory here too? (Its a serialized object)

    // todo


    public HideoutData()
    {
        // - Location Data
        layer = -11;
        layerName = "Lower Caves";
        mapSeed = 0;
        branchValue = 0;
        // - Storage (Cache) Data
        storedMatter = 0;
    }

    public HideoutData(int layer, string layerName, int newSeed, int branchValue, int storedMatter)
    {
        // - Location Data
        this.mapSeed = newSeed;
        this.layer = layer;
        this.layerName = layerName;
        this.branchValue = branchValue;
        // - Storage (Cache) Data
        this.storedMatter = storedMatter;
    }
}

[System.Serializable]
public class SaveData
{
    [Header("Save Data")]
    // - Location Data
    public int layer; // Depth
    public string layerName;
    public int mapSeed;
    public int branchValue;
    // - Storage (Cache) Data
    public int storedMatter;
    //
    public int mode; // Novice, Explorer, Rogue
    public bool difficulty; // Normal or Hard
    public int turn;
    public Vector2Int core1; // Core setup #/#/
    public Vector2Int core2; // Core setup /#/#
    public int killCount;


    public SaveData()
    {
        layer = -11;
        layerName = "Lower Caves";
        mapSeed = 0;
        branchValue = 0;
        storedMatter = 0;
        mode = 0;
        difficulty = false;
        turn = 0;
        core1 = new Vector2Int(1,2);
        core2 = new Vector2Int(2,2);
        killCount = 0;
    }

    public SaveData(int layer, string layerName, int newSeed, int branchValue, int storedMatter, int mode, bool difficulty, int turn, Vector2Int core1, Vector2Int core2, int killCount)
    {
        this.mapSeed = newSeed;
        this.layer = layer;
        this.layerName = layerName;
        this.branchValue = branchValue;
        //
        this.storedMatter = storedMatter;
        //
        this.mode = mode;
        this.difficulty = difficulty;
        this.turn = turn;
        this.core1 = core1;
        this.core2 = core2;
        this.killCount = killCount;
    }
}
