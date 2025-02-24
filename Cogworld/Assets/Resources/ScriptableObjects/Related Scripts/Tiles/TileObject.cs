using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public enum TileType
{
    Floor,
    Wall,
    Door,
    Machine,
    Exit,
    Phasewall,
    Trap,
    Default
}

[System.Serializable]
public abstract class TileObject : ScriptableObject
{
    [Header("Primary")]
    public int Id;

    public string tileName;

    [Header("Display")]
    public Tile displaySprite;
    [Tooltip("Used for things like the open sprite of doors.")]
    public Tile altSprite;
    [Tooltip("What should be shown when this sprite is destroyed.")]
    public Tile destroyedSprite;
    [Tooltip("How this sprite looks while in ASCII mode.")]
    [Header("Display (ASCII)")]
    public Tile asciiRep;
    [Tooltip("Occasionally used alternate sprite when in ASCII mode, usually just used for doors.")]
    public Tile asciiAltSprite;
    [Tooltip("Sprite for when this tile is destroyed while in ASCII mode.")]
    public Tile asciiDestroyed;
    public Color asciiColor;

    [Header("Details")]
    public TileType type;

    public bool isDirty = false;

    /// <summary>
    /// Used for border tiles, normally false.
    /// </summary>
    public bool impassable = false;
    [Tooltip("Is this a cave wall or some kind of industrialized wall?")]
    public bool isCaveWall = false;

    [Tooltip("X = Current, Y = Max")]
    public Vector2Int health = new Vector2Int(10, 10);
    [Tooltip("The weapon attacking this tile must do more than the armor value to start damaging the tile.")]
    public int armor = 5;

    [Header("Destruction Sounds")]
    public List<AudioClip> destructionClips = new List<AudioClip>();

    [Header("Resistances")]
    public List<BotResistances> resistances;

    [Header("Door")]
    public List<AudioClip> door_open;
    public List<AudioClip> door_close;

    [Header("Trap")]
    public TileTrapData trapData;
}

[System.Serializable]
public class TileTrapData
{
    [Tooltip("Full name of this trap")]
    public string trapname;

    public TrapType trapType;
    // Sever
    public bool canSever = false;
    public Vector2Int sever_amount;
    [Tooltip("0.##% | Damaging each by ##-##% of remaining integrity.")]
    public Vector2 sever_damage;
    // Shock
    public bool canShock = false;
    [Tooltip("Percent range of corruption dealt. 0.##%")]
    public Vector2 shock_amount;
    // Burn
    public bool canBurn = false;
    public Vector2Int burn_amount;
    public Vector2Int burn_damage;
    public Vector2Int burn_heat;
    public bool burn_affectsNeighbors = false;
    // Pierce
    public bool canPierce = false;
    public int pierce_amount;
    public Vector2Int pierce_damage;
    [Tooltip("0.##%")]
    public float pierce_impaleChance;
    // Stasis
    public bool canStasis = false;
    [Tooltip("Default is 100")]
    public int stasis_strength;
}

public enum TrapType
{
    Alarm,
    Blade,
    Chute,
    DirtyBomb,
    EMP,
    Fire,
    HE, // Heavy Explosive
    Hellfire,
    ProtonBomb,
    Segregator,
    Shock,
    Stasis,
    NONE
}