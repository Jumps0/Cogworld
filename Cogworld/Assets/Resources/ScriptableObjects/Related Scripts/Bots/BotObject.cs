using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum BotClass
{
    // Source: https://noemica.github.io/cog-minder/bots.html
    Alpha7,   // Alpha 7
    Artisan,  // Derelict
    Assembled,
    Behemoth,
    Bolteater,// Derelict
    Borebot,  // Derelict /w Warlord
    Bouncer,  // Derelict
    Brawler,
    Builder,
    Butcher,  // Derelict
    Carrier,  // aka ARC
    Cobbler,  // Derelict
    Cogmind,  // That's me!
    Commander,// Derelict
    Compactor,// as seen in Wastes
    Cutter,
    Demolisher,
    Decomposer,// Derelict
    Demented,  // Derelict
    Dragon,    // Derelict
    Drone,
    DRS_Ranger,// Derelict
    Deulist,
    Elite,
    Executioner, // Prototype
    Explorer, // Derelict
    Federalist, // Derelict
    Fireman,  // Derelict
    Fortress, // as seen in Section 7
    Furnace,  // Derelict
    Golem,    // "Derelict"
    Guerrilla,// Derelict
    Guru,     // Derelict
    Grunt,
    Hauler,
    Heavy,
    Hunter,
    Hydra,    // Derelict
    Infiltrator, // Derelict
    Knight,   // Derelict
    LRC,      // Your family
    Marauder, // Derelict
    Martyr,   // Derelict
    Master_Theif, // Derelict
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
    Scientist,// Derelict
    Scrapoid, // Derelict
    Sentry,   // Cetus Guard & other permanent unique guards
    Special,  // Architect, Data miner, Godmode, etc. (Major NPCs)
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
    Unique,   // Mostly for (Minor) NPCs
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
    public Sprite displaySprite;
    public Sprite asciiRep;
    public Color idealColor = Color.white;

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

    [Header("Name")]
    public string botName;
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
    [Tooltip("Coverage")]
    public List<Item> _altChoices; // This is actually the item's coverage
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
    // There are currently only 9 total traits in the game

    [Header("Sensor Jamming")]
    [Tooltip("Sensor Jamming: Prevents Sensor Arrays within range from pinpointing signals, but gives away its position in the process.")]
    public bool trait_sensorJamming = false;

    [Header("Energy Emission")]
    [Tooltip("Energy Emission (##): Each turn transfers [##-range] energy to each robot in view and within a range of ##.")]
    public bool trait_energyEmmission = false;
    public int eeRange = 0;
    public int eeAmount = 0;

    [Header("Core Regeneration")]
    [Tooltip("Core Regeneration (#): Regenerates # core integrity every turn.")]
    public bool trait_coreRegeneration = false;
    public int crAmount = 0;

    [Header("Part Regeneration")]
    [Tooltip("Part Regeneration (#): All attached parts regenerate # integrity every turn. Also regenerates one missing part every ## turns.")]
    public bool trait_partRegeneration = false;
    public int prAmount = 0;
    public bool prIncludeMissing = false;
    public int prMissingDelay = 10;

    [Header("Corruption Emission")]
    [Tooltip("Corruption Emission (##): 0.##% chance each turn to cause anywhere from 0-##% corruption in each robot in view and within a range of ##. (Cogmind is less susceptible to the effect and only suffers 0-1% corruption.)")]
    public bool trait_corruptionEmission = false;
    public float ceChance = 0f;
    public Vector2 ceAmount = new Vector2(0f, 0.5f);
    public int ceRange;

    [Header("Energy Drain")]
    [Tooltip("Energy Drain (##): Each turn drains [##-(range*2)] energy from each robot in view and within a range of ##. Also drains an equivalent amount of heat.")]
    public bool trait_energyDrain = false;
    public int edAmount = 0;
    public int edRange = 0;
    public bool edDrainHeat = true;

    [Header("Heat Emission")]
    [Tooltip("Heat Emission (###): Each turn transfers [###-(range*##)] heat to each robot in view and within a range of ##.")]
    public bool trait_heatEmission = false;
    public int heAmount = 0;
    public int heRange = 0;

    [Header("Scan Cloak")]
    [Tooltip("Scan Cloak (#): Hides this robot from sensors without a Signal Interpreter of at least strength #.")]
    public bool trait_scanCloak = false;
    public int scStrength = 1;

    [Header("Self Destruct")]
    [Tooltip("Self-destructing: Leaves no parts on destruction, unless self-destruct mechanism fails due to system corruption.")]
    public bool trait_selfDestruct = false;
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
