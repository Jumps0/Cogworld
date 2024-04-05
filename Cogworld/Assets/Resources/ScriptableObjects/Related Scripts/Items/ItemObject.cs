using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemType
{
    Default,
    // Power
    Engine,
    PowerCore,
    Reactor,
    // Propulsion
    Treads,
    Legs,
    Wheels,
    Hover,
    Flight,
    // Utilities
    Storage,
    Processor,
    Hackware,
    Device,
    Armor,
    Alien,
    // Weapons
    Gun, // (Ballistic)
    EnergyGun,
    Cannon, // (Ballistic
    EnergyCannon,
    Launcher,
    Impact,
    Special,
    Melee,
    // Misc
    Data,
    Nonpart,
    Trap,
}

public enum ItemSlot
{
    Power,
    Propulsion,
    Utilities,
    Weapons,
    Other // Mostly traps
}

[System.Serializable]
public class Item
{
    public string Name;
    public int Id = -1;
    public bool stackable = false;
    public int amount = 1;
    public ItemObject itemData;
    // This section WILL need to be expanded later (probably BotObject too)
    [Tooltip("Active or In-active")]
    public bool state = true; // Active/In-Active
    [Tooltip("Current integrity of this item.")]
    public int integrityCurrent;

    public Item()
    {
        Name = "";
        Id = -1;
    }

    public Item(ItemObject item)
    {
        Name = item.itemName;
        Id = item.data.Id;
        itemData = item;
        integrityCurrent = item.integrityMax;
        SetupText();
    }

    public void SetupText()
    {
        // Here we set the mechanical descriptor text.
        // Most of these follow a standard format,
        // except Utilities which is done manually.
        if(itemData.mechanicalDescription.Length == 0)
        {
            switch (itemData.slot)
            {
                case ItemSlot.Power: // [Rating] M[Mass] E[Supply]
                    itemData.mechanicalDescription = itemData.rating.ToString();
                    if (itemData.star)
                        itemData.mechanicalDescription += "*";
                    itemData.mechanicalDescription += " M";
                    itemData.mechanicalDescription += itemData.mass;
                    itemData.mechanicalDescription += " E";
                    itemData.mechanicalDescription += itemData.supply;
                    break;
                case ItemSlot.Propulsion: // [Rating] T[Time to Move] S[Support]
                    itemData.mechanicalDescription = itemData.rating.ToString();
                    if (itemData.star)
                        itemData.mechanicalDescription += "*";
                    itemData.mechanicalDescription += " T";
                    itemData.mechanicalDescription += itemData.propulsion[0].timeToMove;
                    itemData.mechanicalDescription += " S";
                    itemData.mechanicalDescription += itemData.propulsion[0].support;
                    break;
                case ItemSlot.Utilities:
                    // This is too complex and has too many variables to do here.
                    break;
                case ItemSlot.Weapons: // [Rating] M[Mass] [Damage Range] [Damage Type (IN COLOR)]
                    itemData.mechanicalDescription = itemData.rating.ToString();
                    if (itemData.star)
                        itemData.mechanicalDescription += "*";
                    itemData.mechanicalDescription += " M";
                    itemData.mechanicalDescription += itemData.mass;
                    if (itemData.meleeAttack.isMelee)
                    {
                        itemData.mechanicalDescription += " ";
                        itemData.mechanicalDescription += itemData.meleeAttack.damage.x + "-" + itemData.meleeAttack.damage.y + " ";
                        itemData.mechanicalDescription += HF.ShortenDamageType(itemData.meleeAttack.damageType);
                    }
                    else
                    {
                        itemData.mechanicalDescription += " ";
                        itemData.mechanicalDescription += itemData.projectile.damage.x + "-" + itemData.projectile.damage.y + " ";
                        itemData.mechanicalDescription += HF.ShortenDamageType(itemData.projectile.damageType);
                    }
                    
                    break;
                case ItemSlot.Other:
                    break;
            }
        }
    }
}

public abstract class ItemObject : ScriptableObject
{
    [Header("Standard Info")]
    [Tooltip("For unique items like matter.")]
    public bool instantUnique = false; // For unique items like matter
    public int amount = 1;
    public Item data = new Item();
    [Tooltip("If false, the player doesn't know what it is, so it should be classified as a prototype until they equip/learn about it.")]
    public bool knowByPlayer = true;

    public Sprite floorDisplay;
    public Sprite inventoryDisplay;
    public Sprite asciiRep;

    public Color itemColor = Color.white;

    // Overview
    [Header("Overview")]
    public ItemType type;
    public ItemSlot slot;
    [Tooltip("How many slots does this item take up?")]
    public int slotsRequired = 1;

    public int mass;
    public int rating;
    [Tooltip("If this item has a star * next to its rating, generally means it is better.")]
    public bool star = false;
    public int integrityMax;
    [Tooltip("Percentage value.")]
    public int coverage;

    [Header("Schematic")]
    public SchematicInfo schematicDetails;

    // Active Upkeep
    [Header("Active Upkeep")]
    public bool hasUpkeep = true; // Weapons don't have upkeep
    public float energyUpkeep = 0;
    public float matterUpkeep = 0;
    public float heatUpkeep = 0;

    // Power
    [Header("Power")]
    public int supply;
    public int storage;
    public bool power_HasStability = false;
    public int power_stability;

    [Header("Propulsion")]
    public List<ItemPropulsion> propulsion = new List<ItemPropulsion>();

    [Header("Shot")]
    public ItemShot shot;

    [Header("Projectile")]
    public int projectileAmount = 1;
    public ItemProjectile projectile;

    [Header("Melee - Attack/Hit")]
    public ItemMeleeAttack meleeAttack;

    [Header("Explosion")]
    public ItemExplosion explosion;

    // Effect
    [Header("Effect")]
    public List<ItemEffect> itemEffect;

    [Header("Primary Details")]
    public ItemQuality quality;

    public string itemName;
    [TextArea(3, 5)]
    public string description;
    [Tooltip("Raw numbers of what this item effects. This is usually filled in automatically, but not for Utility items.")]
    public string mechanicalDescription;

    [Header("Fabrication Details")]
    public ItemFabInfo fabricationInfo;

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}

[System.Serializable]
public class ItemEffect
{
    [Header("Chain Reaction Explosive")]
    public bool chainExplode = false;
    [Tooltip("How much damage this can do, low/high.")]
    public Vector2 damage;
    public ItemDamageType chainDamageType;
    public int radius;
    [Tooltip("??? A range of X --> Y")]
    public Vector2 chunks;
    public int fallOff;
    public int salvage;
    [Header("Heat Transfer")]
    [Tooltip("Heat Transfer level: 0 --> 3 [None, Low, Medium, High]")]
    public int transferLevel;
    [Tooltip("0.##")]
    public float disruption = 0f;
    [Tooltip("0 = None, 1 = Short, 2 = Wide, 3 = Long")]
    public int spectrum = 0;

    [Header("Heat Dissipation")]
    public bool doesHeatDissip = false;
    public int heatDissipation_perTurn;
    public bool heatDissipation_stacks = false;

    [Header("Leg Effect")]
    public bool hasLegEffect = false;
    public float extraKickChance; // % value
    public bool appliesToLargeTargets = false;
    public bool conferToRunningState;
    [Tooltip("Increase invasion by X% but also decreases accuracy by X% (per level)")]
    public float evasionNaccuracyChange;
    public int maxMomentumAmount = 3; // This value seems to always be 3

    [Header("Inventory Size")]
    public bool inventorySizeEffect = false;
    public int sizeIncrease;
    public bool effectStacks;

    [Header("Crush/Ram")]
    public bool ramCrushEffect = false;
    [Tooltip("Percent so: 0.##")]
    public float ramCrushChance = 0f;
    [Tooltip("Can crush targets of this size")]
    public bool ramCrush_canDoLarge = false;
    public bool ramCrush_canDoGigantic = false;
    public int ramCrush_integLimit = 0;
    public bool ramCrush_canStack = false;
    [Tooltip("Percent so: 0.##")]
    public float ramCrush_stackCap = 0f;

    [Header("Stability")]
    public bool hasStabEffect = false;
    public int stab_recoilPerWeapon = 0;
    public bool stab_KnockbackImmune = false;

    [Header("Armor")]
    public bool armorEffect = false;
    [Tooltip("Other == General Purpose")]
    public ArmorType armorEffect_slotType;
    [Tooltip("Percent so: 0.##")]
    public float armorEffect_absorbtion = 0f;
    public bool armorEffect_preventCritStrikes = false;
    [Tooltip("Prevents chain reactions due to electromagnetic damage")]
    public bool armorEffect_preventChainReactions = false;
    public bool armorEffect_stacks = false;
    public List<ItemDamageResistance> armorEffect_resistances;

    [Header("Accurracy Buffs")]
    public bool hasAccuracyBuff = false;
    public float accBuff_nonMelee = 0f;
    public float accBuff_melee = 0f;
    public bool accBuff_stacks = false;

    [Header("Detection")]
    public bool hasDetectionEffect = false;
    public int detection_range = 0;
    public bool detect_seismic = false;
    public bool detect_machines = false;
    public bool detect_bots = false;
    public bool detect_pure = false;
    public bool detect_structural = false;
    public bool detect_stacks = false;

    [Header("Signal Interpretation")]
    public bool hasSignalInterp = false;
    public int signalInterp_strength;

    [Header("Network Info")]
    public bool haulerTracking = false;

    [Header("Mass Support")]
    public bool hasMassSupport = false;
    public int massSupport;
    public bool massSupport_stacks = false;

    [Header("Matter Tractor-Beam")]
    public bool collectsMatterBeam = false;
    public int matterCollectionRange;

    [Header("Terrain Scanning")]
    public bool terrainScan = false;
    public int terrainScanRange;
    public bool terrainScan_boost = false;
    public int terrainScan_densBoost;
    public bool terrainScanBoost_stacks = false;

    [Header("Internal Storage")]
    public bool internalStorage = false;
    [Tooltip("0 = Matter, 1 = Power")]
    public int internalStorageType;
    [Tooltip("Internal Storage Capacity")]
    public int internalStorageCapacity;
    public bool internalStorageType_stacks = false;

    [Header("Hacking Related")]
    public HackBonus hackBonuses;

    [Header("Emitting Effect")]
    public ItemEmitEffect emitEffect;

    [Header("Firetime Effect")]
    public ItemFiretimeEffect fireTimeEffect;

    [Header("Transmission Jamming Effect")]
    public ItemTransmissionJamming transmissionJammingEffect;

    [Header("Effective Corruption Prevention Effect")]
    public ItemEffectiveCorruptionPrevention effectiveCorruptionEffect;

    [Header("Part Restore Effect")]
    public ItemPartRestore partRestoreEffect;

    [Header("EXILEs Specific")]
    public ItemExilesSpecific exilesEffects;

    [Header("Various to-hit Buffs")]
    public ItemBetterHitEffects toHitBuffs;

    [Header("Part Identification")]
    public ItemPartIdentification partIdent;

    [Header("Anti-Corruption")]
    public ItemAntiCorruption antiCorruption;

    [Header("Reaction Control System")]
    public ItemRCS rcsEffect;

    [Header("Phasing")]
    public ItemPhasing phasingEffect;

    [Header("Cloaking")]
    public ItemCloaking cloakingEffect;

    [Header("Melee Bonuses")]
    public ItemMeleeBonus meleeBonus;

    [Header("Launcher Bonuses")]
    public ItemLauncherBonuses launcherBonus;
}

public enum ItemQuality
{
    Makeshift,
    Improved,
    Standard,
    Advanced,
    Compact,
    Enhanced,
    Experimental,
    High_Powered,
    Heavy,
    Light,
    Hyper,
    Long_Range,
    Micro,
    Medium,
    Precise,
    Rigged,
    Extra_Large,
    Guided,
    Combined,
    Cooled,
    Armorered,
    ASB,
    OB1O,
    Zion,
    Reinforced
}

[System.Serializable]
public class ItemShot
{
    // Shot
    public int shotRange;
    public int shotEnergy;
    public int shotMatter;
    public int shotHeat;
    public int shotRecoil;
    public float shotTargeting;
    [Tooltip("Each weapon incurs no additional time cost to attack aside from modifying the total attack time by half of their time delay (whether positive or negative).")]
    public int shotDelay;
    public int shotStability;
    public int shotArc;
    public bool hasStability = false;
    public bool hasArc = false;
    public List<AudioClip> shotSound;
}

[System.Serializable]
public class ItemPropulsion
{
    // Propulsion
    public int timeToMove;
    public int modExtra;
    public int drag; // Affects flying/hover movement
    public float propEnergy;
    public float propHeat;
    public int support;
    public int penalty;
    [Tooltip("Percentage value.")]
    public int burnout;
    public bool hasBurnout = false;
}

// Weapon classes

[System.Serializable]
public class ItemProjectile // (Disregarded in AOE attacks except for projectile visuals)
{
    // Projectile
    [Tooltip("Low-High")]
    public Vector2 damage;
    public ItemDamageType damageType;
    [Tooltip("In percentage so: 0.##")]
    public float critChance;
    [Header("Crit Types: 1 = Burn | 2 = Blast | 3 = Destroy \n 4 = Sever | 5 = Blast | 6 = Corrupt")]
    [Tooltip("1 = Burn | 2 = Blast | 3 = Destroy | 4 = Sever | 5 = Blast | 6 = Corrupt")]
    public int critType;
    [Header("x# #/#")]
    [Tooltip("Multiplier - Amount / Amount")]
    public Vector3 penChance;
    public int heatTrasfer;
    [Tooltip("1 = Low | 2 = Medium | 3 = High | 4 = Massive")]
    public int heatTransferDegree;
    public int spectrum;
    public bool hasSpectrum = false;
    public float disruption;
    public int salvage;
    [Header("Projectile Visuals")]
    public Color projectileColor = Color.red;
    public Sprite projectileSprite;
    public bool projectileRotates = true;
    [Tooltip("0 = None | 1 = Rocket Minor | 2 = Rocket Major")]
    public ProjectileTrailStyle projectileTrail = ProjectileTrailStyle.None;
    //public ??? projectileStyle = ???; TODO
    [Header("Penetration")]
    [Tooltip("Each weapon specifies a maximum number of obstructions its projectiles can penetrate, " +
        "along with a separate chance to pass through each consecutive obstruction. " +
        "For example the basic Coil Gun can penetrate up to two objects, the first at a 60% chance, " +
        "and the second at a 30% chance. A third object after that would still take damage, " +
        "but the projectile would stop there. If any chance fails along the way, the projectile does " +
        "not continue beyond the point of impact, even if it destroys the object. You don’t have to destroy " +
        "something to penetrate it--the mechanic is completely independent of damage, meaning you can now " +
        "even fire through walls to hit targets on the other side without taking down the wall itself.")]
    public int penetrationCapability = 0; // How many things this can penetrate through
    [Tooltip("The chance for each penetration event. This list should be equal to the amount of things this projectile can pen.")]
    public List<float> penetrationChances = new List<float>();

}

[System.Serializable]
public class ItemMeleeAttack
{
    public bool isMelee = false;
    [Header("Attack")]
    public int energy;
    public int matter;
    public int heat;
    [Tooltip("0.##")]
    public float targeting;
    public int delay;
    [Header("Hit")]
    public Vector2Int damage;
    public ItemDamageType damageType = ItemDamageType.Impact;
    [Tooltip("0.##")]
    public float critical;
    [Tooltip("1 = Burn | 2 = Blast | 3 = Destroy | 4 = Sever")]
    public int critType;
    [Tooltip("0.##")]
    public float disruption;
    public int salvage;

    [Tooltip("When animated, how long does this attack's visual last?")]
    public float visualAttackTime = 0.5f;
    public List<AudioClip> missSound;

    public bool canDatajack = false; // Can do what datajacks do.

}

[System.Serializable]
public class ItemExplosion
{
    // Explosion
    public int radius = 0;
    public int damageLow;
    public int damageHigh;
    public int falloff;
    public ItemDamageType damageType = ItemDamageType.Explosive;
    public int spectrum;
    public bool hasSpectrum = false;
    public float disruption;
    public int salvage;

    [Header("Audio")]
    public List<AudioClip> explosionSounds;
    [Header("Visuals")]
    public ExplosionGFX explosionGFX;

}

#region Misc Effects
[System.Serializable]
public class ItemDamageResistance
{
    public ItemDamageType damageType;
    [Header("0.##")]
    public float percentage;
    public bool stacks = false;
}

[System.Serializable]
public class ItemEmitEffect
{
    public bool hasEffect = false;

    [Tooltip("False = Assembled, True = 0b10")]
    public bool target = false;
    [Tooltip("This effect happens every X turns.")]
    public int turnTime = 5;
    public int range;
}

[System.Serializable]
public class ItemFiretimeEffect
{
    public bool hasEffect = false;

    [Tooltip("0.##%")]
    public float fireTimeReduction;
    public bool capped = false;
    [Tooltip("0.##%")]
    public float cap;
    public bool stacks = true;
    [Tooltip("Incompatible with Qunatum Capacitor & Autonomous Weapons")]
    public bool compatability = false;
}

[System.Serializable]
[Tooltip("Blocks Local Transmissions from visibile hostiles within a range of ##, " +
    "making it impossible for them to share information about your current position. Also prevents calls for reinforcements, and supresses alarm traps.")]
public class ItemTransmissionJamming
{

    public bool hasEffect = false;

    public int range;
    public bool preventReinforcementCall = true;
    public bool suppressAlarmTraps = true;
}

[System.Serializable]
[Tooltip("Reduces effective system corruption by ##.")]
public class ItemEffectiveCorruptionPrevention
{

    public bool hasEffect = false;

    public int amount;
    public bool stacks = true;
}

[System.Serializable]
[Tooltip("##% chance each turn to restore a broken part, attached or in inventory, to functionality. Unable to repair alien technology, or prototypes above rating #.")]
public class ItemPartRestore
{

    public bool hasEffect = false;

    [Tooltip("0.##%")]
    public float percentChance;
    [Tooltip("Can this device repair alien tech?")]
    public bool canRepairAlienTech = false;
    public int protoRepairCap;
    [Tooltip("Can this run in parralel with another device that also has this effect? (Multiple restores at once)")]
    public bool canRunInParralel = true;
}

[System.Serializable]
[Tooltip("Effects specific to, or commonly found in weapons made by the EXILEs. Some really odd effects in here.")]
public class ItemExilesSpecific
{

    public bool hasEffect = false;
    [Tooltip("Auto Weapon")]
    public bool isAutoWeapon = false;
    [Tooltip("Chrono Wheel")]
    public bool chronoWheelEffect = false;
    [Tooltip("Deployable Turret")]
    public bool deployableTurret = false;
    [Tooltip("Breaks down over time")]
    public bool lifetimeDecay = false;

}

[System.Serializable]
[Tooltip("Effects chances to hit, crit chance, bypassing armor, etc.")]
public class ItemBetterHitEffects
{
    public bool hasEffect = false;
    [Tooltip("0.##")]
    public float amount;
    public bool stacks = false;
    [Tooltip("This effect stacks, but the bonus is halved.")]
    public bool halfStacks = false;
    [Header("Type of Effect")]
    public bool bonusCritChance = false;
    [Tooltip("Doesn't apply to AOE attacks.")]
    public bool bypassArmor = false;
    [Tooltip("Increases target core exposure. Applies to only gun, cannon, and melee attacks.")]
    public bool coreExposureEffect = false;
}

[System.Serializable]
[Tooltip("% chance to identify a random unidentified part in inventory each turn.")]
public class ItemPartIdentification
{
    public bool hasEffect = false;
    [Tooltip("0.##")]
    public float amount;
    public bool parallel = false;
}

[System.Serializable]
[Tooltip("% chance each turn to purge 1% of system corruption, losing 3 integrity each time the effect is applied.")]
public class ItemAntiCorruption
{
    public bool hasEffect = false;
    [Tooltip("0.##")]
    public float amount;
    public int integrityLossPer;
    public bool parallel = false;
}

[System.Serializable]
[Tooltip("Enables responsive movement to avoid direct attacks, #% to dodge while on legs, or #% * 2 while hovering or flying (no effect on tracked or wheeled movement). Same chance to evade triggered traps, and a +# to effective momentum for melee attacks and ramming. No effects while overweight.")]
public class ItemRCS
{
    public bool hasEffect = false;
    [Header("0.## | Effect doubled while hovering or flying")]
    public float percentage;
    public bool stacks = false;
    public int momentumBonus = 1;
}

[System.Serializable]
[Tooltip("Reduces enemy ranged targeting accuracy by ##%.")]
public class ItemPhasing
{
    public bool hasEffect = false;
    [Header("0.## | Effect doubled while hovering or flying")]
    public float percentage;
    public bool stacks = false;
}

[System.Serializable]
[Tooltip("Effective sight range of robots attempting to spot you reduced by #. Also -##% chance of being noticed by hostiles if passing through their field of view when not their turn.")]
public class ItemCloaking
{
    public bool hasEffect = false;
    [Tooltip("Effective sight range of robots attempting to spot you reduced by #.")]
    public int rangedReduction;
    [Tooltip("[In the form -0.##] -##% chance of being noticed by hostiles if passing through their field of view when not their turn.")]
    public float noticeReduction;
    [Tooltip("If multiple of this effect is applied, then then only half of that effect is added.")]
    public bool halfStacks = true;
}

[System.Serializable]
[Tooltip("Various to melee related bonuses")]
public class ItemMeleeBonus
{
    public bool hasEffect = false;
    // - per-weapon chance of follow-up melee attacks (Actuator Array's)
    // - melee weapon maximum damage by +##%, and decreases melee attack accuracy by -##% (Force Booster's)
    // - melee attack accuracy by +##%, and minimum damage by # (cannot exceed weapon's maximum damage). (Melee Analysis Suite)
    // - reduces melee attack time by 50%. <stacks, capped at 50 % > (-actuationors)

    [Header("Actuator Array Effects")]
    public float melee_followUpChance = 0f;
    [Header("Force Booster Effects")]
    public float melee_maxDamageBoost = 0;
    [Tooltip("Should be negative.")]
    public float melee_accuracyDecrease = 0f;
    [Header("Melee Analysis Suite Effects")]
    public float melee_accuracyIncrease = 0f;
    public int melee_minDamageBoost = 0;
    [Header("Microactuator Effects")] // Micro, Femto, Nano, etc.
    public float melee_attackTimeDecrease = 0f;
    public bool actuator_stacks = true;
    public float actuator_cap = 0.5f;

}

[System.Serializable]
[Tooltip("Various bonuses related to launchers.")]
public class ItemLauncherBonuses
{
    public bool hasEffect = false;

    [Tooltip("Increases launcher accuracy by ##% (0.##). Also prevents launcher misfires caused by system corruption.")]
    public float launcherAccuracy = 0f;
    [Tooltip("Reduces firing time for any launcher by ##%, if fired alone. Incompatible with Weapon Cyclers and autonomous or overloaded weapons.")]
    public float launcherLoading = 0f;
    public bool stacks = false;
}

#endregion


[System.Serializable]
public class ItemFabInfo
{
    public bool canBeFabricated = true;

    public int amountMadePer = 1;

    [Tooltip("How much time it takes to fabricate this item. Long/Normal/Quick")]
    public Vector3Int fabTime;

    public int matterCost;

    [Tooltip("Some items require other items to craft. Leave this empty if that's not required.")]
    public List<ItemObject> componenetsRequired = new List<ItemObject>();
}

[System.Serializable]
public class SchematicInfo
{
    public bool hackable = true;
    [Tooltip("Min Terminal Sec lvl / Depth")]
    public List<Vector2Int> location = new List<Vector2Int>();
}

[System.Serializable]
public class HackBonus
{
    // For Hacking Suite
    public bool hasHackBonus = false;
    [Tooltip("Increases chance of successful machine hack by ##%")]
    public float hackSuccessBonus;
    [Tooltip("Provides a +##% bonus to rewiring traps and disrupted robots")]
    public float rewireBonus;
    [Tooltip("Applies a -##% penalty to hostile programmers attempting to defend their allies against your hacks.")]
    public float programmerHackDefenseBonus;
    public bool stacks = true;

    // For System Shield
    public bool hasSystemShieldBonus = false;
    [Tooltip("Reduces Chances of Detection while hacking machines")]
    public float hackDetectRateBonus = 0f; // Reduces Chances of Detection
    [Tooltip("Reduces Rate of Detection Chance increases while hacking machines")]
    public float hackDetectChanceBonus = 0f;
    [Tooltip("Lowers the chance that hacking machines will be considered serious enough to trigger an increase in security")]
    public float securityLevelIncreaseChance = 0f;
    [Tooltip("Reduces central database lockout chance")]
    public float databaseLockoutBonus = 0f;
    [Tooltip("Blocks hacking feedback side effects ##% of the time")]
    public float hackFeedbackPreventBonus = 0f;
    [Tooltip("Repels ##% of hacking attempts against allies within a range of ##")]
    public float allyHackDefenseBonus = 0f;
    public int allyHackRange;
}

[System.Serializable]
public enum ItemDamageType
{
    Kinetic,
    Thermal,
    Explosive,
    EMP,
    Energy,
    Phasic,
    Impact,
    Slashing,
    Piercing
}

[System.Serializable]
public enum ArmorType
{
    Power,
    Propulsion,
    Utility,
    Weapon,
    General,
    None
}

[System.Serializable]
public enum ProjectileTrailStyle
{
    MissileMinor,
    MissileMajor,
    None
}

[System.Serializable]
[Tooltip("FARCOM, CRM, Imprinted, RIF, etc.")]
public class SpecialTrait // TODO: Expand + effects!
{
    public bool FARCOM = false;
    public bool CRM = false;
    public bool imprinted = false;
    public bool RIF = false;
}