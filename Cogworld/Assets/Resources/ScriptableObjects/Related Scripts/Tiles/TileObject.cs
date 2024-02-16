using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TileType
{
    Floor,
    Wall,
    Door,
    Machine,
    Exit,
    Default
}

public abstract class TileObject : ScriptableObject
{
    [Header("Primary")]
    public int Id;

    public Sprite displaySprite;
    public string tileName;

    [Header("Details")]
    public TileType type;
    public TileDetails details;

    public Sprite altSprite;

    public Sprite asciiRep;
    public Color asciiColor;
    public bool isDirty = false;

    /// <summary>
    /// Used for border tiles, normally false.
    /// </summary>
    public bool impassable = false;

    [Tooltip("X = Current, Y = Max")]
    public Vector2Int health = new Vector2Int(10, 10);
    [Tooltip("The weapon attacking this tile must do more than the armor value to start damaging the tile.")]
    public int armor = 5;

    [Header("Field of View")]
    public TileVisibility currentVis;

    [Header("Destruction Sounds")]
    public List<AudioClip> destructionClips = new List<AudioClip>();

}

[System.Serializable]
public class TileDetails
{
    


}

[System.Serializable]
public enum TileVisibility
{
    Visible,
    Known,
    Unknown,
    NAN
}