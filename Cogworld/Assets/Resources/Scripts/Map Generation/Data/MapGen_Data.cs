using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapGenParams_",menuName = "PCG/MapGen_data")]
public class MapGen_Data : ScriptableObject
{
    /// <summary>
    /// How large the map is (and determines where the borders are drawn).
    /// </summary>
    public Vector2Int mapSize = new Vector2Int(100, 100);
    /// <summary>
    /// High/Low values of how many large halls (random) should be created. Setting 'doLargeCorridors' to false multiplies this value by 5.
    /// </summary>
    public Vector2Int largeHall_Amount = new Vector2Int(3, 6);
    public Vector2Int tunnelerLifeTime = new Vector2Int(20, 50);
    /// <summary>
    /// If true, do normal large hall generation. If false, make them MUCH shorter.
    /// </summary>
    public bool doLargerCorridors = false;
    /// <summary>
    /// Allow wide open spaces.
    /// </summary>
    public bool doOpenSpaces = true;
    /// <summary>
    /// Try and create more 1 wide corridors.
    /// </summary>
    public bool prioritizeCorridors = false;
    public Color mapColorTheme;
    //public bool startRandomlyEachIteration = true;
}
