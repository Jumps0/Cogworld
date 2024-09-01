using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All things related to base building, management, and storage.
/// </summary>


public class BaseManager : MonoBehaviour
{
    public static BaseManager inst;
    public void Awake()
    {
        inst = this;

        data = new HideoutData(); // Set Default values for hideout

        CanDeserializeHideoutJson(); // Try to load hideout data from .json file

        if (!CanDeserializeHideoutJson()) // If that fails
        {
            CreateNewHideoutJson(); // Make a new one
        }
    }

    // -- Data Related --
    private iDataService DataService = new JsonDataService();

    public HideoutData data;

    public void TryLoadIntoBase()
    {
        
        if(data.mapSeed == 0)
        {
            // No saved base exists, make a new one
            CreateNewBase();
        }
        else
        {
            // A saved base does exist, load it.
            InstantiateCurrentBase();
        }
    }

    public void CreateNewBase()
    {
        StartCoroutine(MapManager.inst.InitNewHideout());
    }
    
    public void InstantiateCurrentBase()
    {

    }

    public void ExitBase()
    {

        MapManager.inst.ChangeMap(-1, true, false); // Change maps to the starting map & reset stats

        // Attempt to save data
        if (CanSerializeHideoutJson())
        {
            SerializeHideoutJson();
        }
    }

    #region File I/O (.json)

    // Methods
    // Writes and saves hideout data to the JSON file; Returns if successful or not
    public bool CanSerializeHideoutJson()
    {
        if (DataService.SaveData("/hideout-data.json", data))
        {
            Debug.Log("Hideout Data Saved");

            return true;
        }
        else
        {
            Debug.LogError("Could not save hideout data.");

            return false;
        }
    }

    // Writes and saves hideout data to the JSON file
    public void SerializeHideoutJson()
    {
        // -- Save data from current world info --
        // - Location
        data.layer = MapManager.inst.currentLevel;
        data.layerName = MapManager.inst.currentLevelName;
        data.mapSeed = MapManager.inst.mapSeed;
        data.branchValue = MapManager.inst.currentBranch;
        // - Hideout Info
        data.storedMatter = HF.TryFindCachedMatter();
        // <<< EXPAND THIS AS NEEDED >>>

        if (DataService.SaveData("/hideout-data.json", data))
        {
            Debug.Log("Hideout Data Saved");
        }
        else
        {
            Debug.LogError("Could not save hideout data.");
        }
    }

    /*
     Reads and saves hideout data from the JSON file.
    If the file does not exist, the hideout will not be initialized.
        In this case, the hideout will copy the hideout data provided in the MainMenuMgr,
        where the data is initialized in the Unity Editor Inspection Window.
    Returns if succesful or not
     */
    public bool CanDeserializeHideoutJson()
    {
        try
        {
            data = DataService.LoadData<HideoutData>("/hideout-data.json");

            // -- And using that data, assign values --
            // - Location
            MapManager.inst.currentLevel = data.layer;
            MapManager.inst.currentLevelName = data.layerName;
            MapManager.inst.mapSeed = data.mapSeed;
            MapManager.inst.currentBranch = data.branchValue;
            // TODO: ??? Force map to (re)load with this info ???

            // - Hideout Info
            HF.TrySetCachedMatter(data.storedMatter);
            // <<< EXPAND THIS AS NEEDED >>>

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file." + " - Thrown Exception: " + e);

            return false;
        }
    }

    /*
    Reads and saves hideout data from the JSON file.
    If the file does not exist, the hideout will not be initialized.
    In this case, the hideout will copy the hideout data provided in the MainMenuMgr,
    where the data is initialized in the Unity Editor Inspection Window.
 */
    public void DeserializeHideoutJson()
    {
        try
        {
            data = DataService.LoadData<HideoutData>("/hideout-data.json");
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file." + " - Thrown Exception: " + e);
        }
    }

    public void CreateNewHideoutJson()
    {
        data = new HideoutData();
        CanSerializeHideoutJson();
    }

    #endregion

}

public static class BaseBuildTools
{
    // Macro functions for base building
    
    public static void BuildTile(Vector2Int location)
    {

    }

    /// <summary>
    /// Builds a "Large" machine, something that is bigger than 1x1.
    /// </summary>
    /// <param name="center">Approximate center of the machine.</param>
    /// <param name="size">X-By-Y size of the machine.</param>
    public static void BuildLargeMachine(Vector2Int center, Vector2Int size)
    {

    }
    
}
