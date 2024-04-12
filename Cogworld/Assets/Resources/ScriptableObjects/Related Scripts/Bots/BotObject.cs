using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum BotClass
{
    // Source: https://noemica.github.io/cog-minder/bots.html
    Alpha7,   // Alpha 7
    Assembled,
    Behemoth,
    Borebot,  // Derelict /w Warlord
    Bouncer,  // Derelict
    Brawler,
    Builder,
    Butcher,  // Derelict
    Carrier,  // aka ARC
    Cogmind,  // That's me!
    Commander,// Derelict
    Compactor,// as seen in Wastes
    Cutter,
    Demolisher,
    Decomposer,// Derelict
    Demented,  // Derelict
    Dragon,    // Derelict
    Drone,
    Deulist,
    Executioner,
    Fireman,  // Derelict
    Fortress, // as seen in Section 7
    Furnace,  // Derelict
    Golem,    // "Derelict"
    Guerrilla,// Derelict
    Grunt,
    Hauler,
    Heavy,
    Hunter,
    Hydra,    // Derelict
    Infiltrator, // Derelict
    Knight,   // Derelict
    LRC,      // What was what now is
    Marauder, // Derelict
    Martyr,   // Derelict
    MasterTheif, // Derelict
    Mechanic,
    Minesweeper,
    Mutant,   // Derelict
    Operator,
    Packrat,  // Derelict
    Parasite, // Derelict
    Protector,
    Programmer,
    Q_Series, // _ should be -
    Recycler,
    Researcher,
    Saboteur,
    Samaritan,// Derelict
    Sapper,   // Derelict
    Savage,   // Derelict
    Sentry,   // Cetus Guard & other permanent unique guards
    Special,  // Architect, Data miner, Godmode, etc.
    Specialist, // Dudes that are mostly gun
    Striker,
    Surgeon,  // Derelict
    Swarmer,
    Theif,    // Derelict
    Thug,     // Derelict
    Tinkerer, // Derelict
    Troll,    // Derelict
    Tunneler,
    Turret,
    Unique,   // Mostly for NPCs
    Wasp,     // Derelict
    Watcher,
    Wizard,   // Derelict
    Worker,   // K-01 Serf aka Jannies
    Z_Courier,// Heroes of Zion (Derelict)
    Z_Drone,  // Heroes of Zion (Derelict)
    Z_EX,     // Heroes of Zion (Derelict)
    Z_Heavy,  // Heroes of Zion (Derelict)
    Z_Light,  // Heroes of Zion (Derelict)
    Z_Technicatian, // Heroes of Zion (Derelict)
    Zionite,  // (Derelict)
    None
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
    [Tooltip("Targeting is actually tested on a finer grid than is visible onscreen. " +
        "Each map space is divided into a 9x9 grid of squares, and as robot sizes vary (S/M/L), they may take up more or fewer of these squares. " +
        "This has several implications, e.g. you may be able to hit a larger target before it has completely rounded a corner, " +
        "while smaller targets may require a more direct line-of-sight. It also means that smaller targets are actually easier to shoot around than larger ones. " +
        "As long as the targeting line is green Cogmind has a clear LOF to the target, even if it looks like it passes through another robot or obstacle.")]
    public BotProfile _profile;
    public int rating;
    public bool star;
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
    public Item item;
    [Tooltip("Float so 0.##")]
    public float dropChance;
    [Tooltip("Alternative Choices for this item")]
    public List<Item> _altChoices; // TODO: Make this stuff functional
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
    [Tooltip("Immune to any core-affecting effects from critical strikes (including Destroy, Blast, Smash, Puncture, Phase), as well as the effect of core analyzers.")]
    public bool coring = false;
    [Tooltip("Immune to any part removal effects from critical strikes (including Blast, Sever, Sunder).")]
    public bool dismemberment = false;
    [Tooltip("Immune to the special feature of EM attacks where a random part or ever the core can be disabled for a short time.")]
    public bool disruption = false;
    public bool hacking = false; // TODO: Consider this
    public bool jamming = false; // TODO: Consider this
    [Tooltip("Immune to the meltdown effect.")]
    public bool meltdown = false;
    [Tooltip("Total immunity to all criticals.")]
    public bool criticals = false;
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
    Hostile, // Will attack on sight
    Neutral, // Will co-exist
    Friendly,// Will share information and protect
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

    public BotRelation GetRelation(BotAlignment alignment)
    {
        foreach (var relation in alleganceTree)
        {
            if(relation.Item1 == alignment)
            {
                return relation.Item2;
            }
        }

        return BotRelation.Neutral;
    }
}

[System.Serializable]
[Tooltip("A more refined class split up into generalized categories.")]
public enum BotClassRefined
{
    Worker,  // Has a job, and will spend its time doing that job
    Fighter, // Will patrol, fight, and guard
    Support, // Follows around fighters (and sometimes supports them)
    Static,  // Will not move unless told otherwise (sometimes scripted to perform actions, like NPCs)
    Ambient, // Wanders around and does nothing
    None
}
