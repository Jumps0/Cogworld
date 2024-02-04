using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum BotClass
{
    Hauler,
    Drone,
    Brawler,
    Behemoth,
    Cutter,
    Carrier,
    Compactor,
    Demolisher,
    Saboteur,
    Grunt,
    Hunter,
    Heavy,
    Protector,
    Worker,
    Duelist,
    Mechanic,
    Minesweeper,
    Operator,
    Programmer,
    Recycler,
    Swarmer,
    Tunneler,
    Builder,
    Watcher,
    Specialist,
    Sentry,
    Scavenger,
    NPC,
    Misc,
    Samaritan, // (Derelict)
    Derelict,
    Assembled,
    Unique, // Used for NPCs
    Turret
}

[System.Serializable]
public enum BotSize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge
}

[System.Serializable]
public abstract class BotObject : ScriptableObject
{
    public int Id;
    public Sprite asciiRep;
    [Header("Overview")]
    public BotClass _class;
    public BotSize _size;
    public BotProfile _profile;
    public int rating;
    public int tier;
    public int threat;
    public int value;
    public int energyGeneration;
    public int heatDissipation;
    public int visualRange;
    public int memory;
    [Tooltip("Percent value so: 0.##")]
    public float spotPercent;
    public BotMovement _movement;
    public int coreIntegrity;
    public int currentIntegrity;
    [Tooltip("Percent value so: 0.##")]
    public float coreExposure;
    public Vector2Int salvagePotential;

    public SchematicInfo schematicDetails;

    [Header("Armament")]
    public List<BotArmament> armament;

    [Header("Components")]
    public List<BotArmament> components;

    [Header("Resistances")]
    public List<BotResistances> resistances;
    public BotResistancesExtra resistancesExtra;

    [Header("Traits")]
    public List<BotTraits> traits;

    [Header("Fabrication")]
    public ItemFabInfo fabricationInfo;

    [Header("Description")]
    [TextArea(3,5)]
    public string description;

    [Header("Location")]
    public BotLocations locations;

    public bool playerHasAnalysisData = false;
}

[System.Serializable]
public class BotProfile
{
    public string forward = "";
    public Vector2Int size;
}

[System.Serializable]
public class BotArmament
{
    public ItemObject _item;
    [Tooltip("Float so 0.##")]
    public float dropChance;
    [Tooltip("Alternative Choices for this item")]
    public List<ItemObject> _altChoices;
    [Tooltip("Float so 0.##")] public List<float> altChoicesDropChance;
}

[System.Serializable]
public class BotMovement
{
    [Header("Normal Movement")]
    public BotMoveType moveType;
    public int moveTileRange;
    [Tooltip("Percent value so: 0.##")]
    public float moveSpeedPercent;

    [Header("Overloaded Movement")]
    public bool overloadedMovement = false;
    public BotMoveType moveType_ov;
    public int moveTileRange_ov;
    [Tooltip("Percent value so: 0.##")]
    public float moveSpeedPercent_ov;
}

[System.Serializable]
public enum BotMoveType
{
    Walking,
    Treading,
    Flying,
    Hovering,
    Rolling,
    Running,
    SIEGE
}

[System.Serializable]
public class BotResistances
{
    public ItemDamageType damageType;
    [Tooltip("Percent value so: 0.##")]
    public float resistanceAmount;
    public bool immune = false;
}

[System.Serializable]
public class BotResistancesExtra
{
    [Header("Checked = IMMUNE")]
    public bool coring = false;
    public bool disruption = false;
    public bool meltdown = false;
    public bool hacking = false;
    public bool jamming = false;
}

[System.Serializable]
public class BotTraits
{
    // TODO
    [Header("Sensor Jamming\n")]
    [Tooltip("Sensor Jamming: Prevents Sensor Arrays within range from pinpointing signals, but gives away its position in the process.")]
    public bool trait_sensorJamming = false;

    [Header("Energy Emission\n")]
    [Tooltip("Energy Emission (##): Each turn transfers [##-range] energy to each robot in view and within a range of ##.")]
    public bool trait_energyEmmission = false;
    public int eeRange = 0;
    public int eeAmount = 0;
}

[System.Serializable]
public class BotLocations
{
    public bool all0b10Maps = false;
    public Vector2Int _0b10Range;
    public bool fromHost = false;
    public bool fromEvent = false;
    public BotRelation relation;
    [Tooltip("What faction this bot is a part of.")]
    public BotAlignment alignment;
}

[System.Serializable]
public enum BotRelation
{
    Hostile,
    Neutral,
    Friendly,
    Default
}

[System.Serializable]
public enum BotAlignment
{
    Complex, // 0b10
    Derelict,
    Assembled,
    Warlord,
    Zion,
    Exiles,
    Architect,
    Subcaves,
    SubcavesHostile,
    Player,
    None
}

[System.Serializable]
public class Allegance
{
    public List<(BotAlignment, BotRelation)> alleganceTree = new List<(BotAlignment, BotRelation)> ();

    public Allegance(List<(BotAlignment, BotRelation)> aT)
    {
        alleganceTree = aT;
    }
}
