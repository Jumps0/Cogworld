using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[Tooltip("Holds information for prefab maps/rooms/encounters that get parsed during CTR map generation.")]
public class PMapInfo : MonoBehaviour
{
    [Header("Information")]
    public string prefabname;
    public PMapType type;
    [Tooltip("The (dictionary) ID of the main walls that appear on this map.")]
    public int tile_wallID = -1;
    [Tooltip("The (dictionary) ID of the main doors that appear on this map.")]
    public int tile_doorID = -1;
    [Tooltip("The (dictionary) ID of the main floors that appear on this map.")]
    public int tile_floorID = -1;

    [Header("List Components")]
    [Tooltip("List containing all floor item gameObjects.")]
    public List<GameObject> objs_item;
    [Tooltip("List containing all pre-placed bots.")]
    public List<GameObject> objs_bot;
    [Tooltip("All trigger tiles.")]
    public List<GameObject> objs_trigger;
    [Tooltip("All event tiles.")]
    public List<GameObject> objs_event;
    [Tooltip("All entrance points.")]
    public List<GameObject> objs_entrance;
    [Tooltip("All exits points.")]
    public List<GameObject> objs_exit;
    [Tooltip("The PARENT parts for all machines.")]
    public List<GameObject> objs_machine;

    [Header("Bounds")]
    [Tooltip("Bottom Left corner marking the bounds of this prefab.")]
    public Vector2Int bounds_BL;
    [Tooltip("Top right corner marking the bounds of this prefab.")]
    public Vector2Int bounds_TR;
}


[System.Serializable]
public enum PMapType
{
    [Tooltip("An entire map, with little to no modification required.")]
    Full,
    [Tooltip("A moderately sized section of a map, which will be stitched together with other sections, or other types of natural map generation.")]
    Section,
    [Tooltip("A somewhat open space containing some kind of complex interaction. Not that large.")]
    Encounter,
    [Tooltip("An individual room.")]
    Room,
    [Tooltip("A very small collection of tiles and information.")]
    Miniature

}