using JetBrains.Annotations;
using System;
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
    Inventory,
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
    [Tooltip("If this item is a unique item (like Scrap, Data Logs, etc.), the details of what it does go in here.")]
    public ItemUnique uniqueDetail;
    [Tooltip("If this item is storing some amount of something (energy, matter), how much is it currently storing?")]
    public int storageAmount = 0;

    [Header("Special States/Effects")]
    [Tooltip("Is this item currently overloaded?")]
    public bool isOverloaded = false;
    [Tooltip("Is this item currently losing HP because of an external reason (deteriorating)?")] // see: https://www.gridsagegames.com/blog/2013/12/burnout-momentum-em-disruption/
    public bool isDeteriorating = false;
    public bool isRigged = false;
    [Tooltip("Is this item corrupted?")]
    public bool corrupted = false;
    [Tooltip("If > 0, this item is disabled for the specified turns.")]
    public int disabledTimer = 0;
    [Tooltip("Unstable weapons implode after the indicated remaining number of shots. -1 = not unstable.")]
    public int unstable = -1; // -1 = not unstable | https://www.gridsagegames.com/forums/index.php?topic=1577.0
    [Tooltip("Some weapons have a limited number of uses and will destroy themselves after use. -1 = not disposable.")]
    public int disposable = -1;
    [Tooltip("Is this item currently in siege mode?")]
    public bool siege = false; // https://www.gridsagegames.com/blog/2019/09/siege-tread-mechanics/

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

        unstable = item.unstable;
        disposable = item.disposable;
        uniqueDetail = item.uniqueDetail;
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
    public int amount = 1;
    public Item data = new Item();
    [Tooltip("If false, the player doesn't know what it is, so it should be classified as a prototype until they equip/learn about it.")]
    public bool knowByPlayer = true;

    public Sprite floorDisplay;
    public Sprite inventoryDisplay;
    public Sprite asciiRep;
    public Sprite bigDisplay;

    public Color itemColor = Color.white;

    // Overview
    [Header("Overview")]
    public ItemType type;
    public ItemSlot slot;
    [Tooltip("How many slots does this item take up?")]
    public int slotsRequired = 1;

    public int mass;
    [Tooltip("Goes from 1 to 10")]
    public int rating;
    [Tooltip("If this item has a star * next to its rating, generally means it is better.")]
    public bool star = false;
    [Tooltip("Sometimes items have a little green text indicator to the right of the rating number.")]
    public ItemRatingType ratingType;
    public int integrityMax;
    [Tooltip("If false, the UI will only display the current health, and include a * if at max health.")]
    public bool repairable = true;
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
    public bool power_HasStability = false; // Seems like only Cld. power sources have this so it'l be rare that it will show up
    [Tooltip("Rarely used. 0.##%")]
    public float power_stability;

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
    public ExplosionGeneric explosionDetails;

    [Header("Deployable Item")]
    public ItemDeployable deployableItem;

    [Header("Special Attacks")]
    [Tooltip("A non-damaging special case such as Datajacks, Stasis Beams, Tearclaws, etc.")]
    public bool isSpecialAttack = false; // TODO: Consider these in attacks
    public ItemSpecialAttack specialAttack;

    // Effect
    [Header("Effect")]
    public List<ItemEffect> itemEffects;
    [Tooltip("Certain power sources, propulsion units, and energy weapons can be overloaded. Performing better but with dangerous downsides.")]
    public bool canOverload = false; // TODO: Complete functionality for relevant items
    public bool consumable = false; // TODO: Functionality for this too
    public int disposable = -1; // TODO: Functionality for this
    public int unstable = -1; // TODO: Functionality for this
    [Tooltip("Certain items (like processors), will destroy themselves when they are removed. Items like this give a warning to the player if they attempt to remove it.")]
    public bool destroyOnRemove = false; // TODO: Functionality for this

    [Header("Primary Details")]
    public ItemQuality quality;

    public string itemName;
    [TextArea(3, 5)]
    public string description;
    [Tooltip("Raw numbers of what this item effects. This is usually filled in automatically, but not for Utility items.")]
    public string mechanicalDescription;

    [Header("Fabrication Details")]
    public ItemFabInfo fabricationInfo;

    [Header("Special Unique")]
    [Tooltip("For unique items like matter, scrap, derelict info caches, etc.")]
    public bool instantUnique = false; // For unique items like matter, scrap, derelict info caches, etc.
    public ItemUnique uniqueDetail;

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}

[System.Serializable]
public class ItemEffect
{
    [Header("Heat Transfer Level")]
    [Tooltip("Heat Transfer level: 0 --> 4 [None, Low (25), Medium (37), High (50), Massive (80)]")]
    public int heatTransfer = 0;

    [Header("Chain Reaction Explosive")]
    public bool chainExplode = false;

    [Header("Leg Effect")]
    public bool hasLegEffect = false;
    public float extraKickChance; // % value
    public bool appliesToLargeTargets = false;
    public bool conferToRunningState;
    [Tooltip("Increase invasion by 0.##% but also decreases accuracy by 0.##% (per level)")]
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
    public ItemProtectionEffect armorProtectionEffect;

    [Header("Detection")]
    public ItemDetectionEffect detectionEffect;

    [Header("Signal Interpretation")]
    public bool hasSignalInterp = false;
    public int signalInterp_strength;

    [Header("Mass Support")]
    public bool hasMassSupport = false;
    public int massSupport;
    public bool massSupport_stacks = false;

    [Header("Matter Tractor-Beam")]
    public bool collectsMatterBeam = false;
    public int matterCollectionRange;

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
    public ItemToHitEffects toHitBuffs;

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

    [Header("Heat Dissipation")]
    public ItemHeatDissipation heatDissipation;

    [Header("Bonus Slots")]
    public ItemBonusSlots bonusSlots;

    [Header("Flat Damage Bonuses")]
    public ItemFlatDamageBonus flatDamageBonus;

    [Header("Salvage Bonuses")]
    public ItemSalvageBonuses salvageBonus;

    [Header("Alien Bonuses")]
    public ItemAlienBonuses alienBonus;
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
    [Tooltip("This is a direct modifier to the weapon's accuracy calculation when firing. Some weapons are inherently easier or more difficult to accurately target with.")]
    public float shotTargeting;
    [Tooltip("Each weapon incurs no additional time cost to attack aside from modifying the total attack time by half of their time delay (whether positive or negative).")]
    public int shotDelay;
    [Tooltip("0.##%")]
    public float shotStability;
    [Tooltip("Total angle within which projectiles are randomly distributed around the target, spreading them along an arc of a circle centered on the shot origin. Improved targeting has no effect on the spread, which is always centered around the line of fire.")]
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
    [Tooltip("Inactive non-airborne propulsion modify the movement time cost by this amouunt while airborne. However, inactive propulsion has no adverse effective on the speed of non-airborne propulsion, including core movement.")]
    public int drag; // Affects flying/hover movement
    public float propEnergy;
    public float propHeat;
    public int support;
    public int penalty;
    [Tooltip("Percentage value. | Seeing as overloading forces a part to output beyond what it was designed to regularly handle, over the long term the additional strain will cause it to gradually lose integrity dependent on its “burnout rate.” So overloading is more than just a tradeoff of energy/heat for speed, as it could eventually render your propulsion useless")]
    public int burnout;
    public bool hasBurnout = false;

    [Tooltip("There are three possible values for a tread’s Siege stat (0, 1, 2):\r\n\r\nN/A: Not capable of entering siege mode (applies to all single-slot treads)\r\nStandard: Capable of siege mode, as described above (most multislot treads)\r\nHigh: As Standard, but with a +30% accuracy bonus and 50% damage reduction (a select few treads)")]
    public int canSiege = 0;
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
    // https://www.gridsagegames.com/blog/2021/05/design-overhaul-3-damage-types-and-criticals/
    public CritType critType;
    [Tooltip("0 = None | 1 = Low | 2 = Medium | 3 = High | 4 = Massive")]
    public int heatTrasfer;
    [Tooltip("0.##% > Can be: Wide(10%) | Intermediate (30%) | Narrow (50%) | Fine (100%)")]
    public float spectrum;
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
    [Tooltip("(0.##%) The chance for each penetration event. This list should be equal to the amount of things this projectile can pen.")]
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
    public CritType critType;
    [Tooltip("0.##")]
    public float disruption;
    public int salvage;

    [Tooltip("When animated, how long does this attack's visual last?")]
    public float visualAttackTime = 0.5f;
    public List<AudioClip> missSound;

}

[System.Serializable]
public class ItemExplosion
{
    [Header("Audio")]
    public List<AudioClip> explosionSounds;
    [Header("Visuals")]
    public ExplosionGFX explosionGFX;

}

[System.Serializable]
[Tooltip("Things like turrets, timed charges, and mines. Stored in the Inventory only.")]
public class ItemDeployable
{
    [Header("Details")]
    public bool deployable = false;
    public DeployableType type;

    /*
    [Header("Turrets")]

    [Header("Charges")]

    */
    [Header("Traps")]
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

[System.Serializable]
public class ExplosionGeneric
{
    [Header("Explosion Type")]
    [Tooltip("Usually for launchers and whatnot")]
    public bool isGeneral = false;
    [Tooltip("Explosion as an effect. Not that common.")]
    public bool isEffect = false;
    [Tooltip("For deployable things like mines and charges.")]
    public bool isDeployable = false;

    [Header("Details")]
    public ItemDamageType damageType;
    [Tooltip("Low-High")]
    public Vector2Int damage;
    public int radius = 0;
    public int fallOff = 0;
    public Vector2Int chunks;
    public int salvage;
    
    [Tooltip("0.##")]
    public float disruption = 0f;

    public bool hasSpectrum = false;
    [Header("0.##% > Can be: Wide(10%) | Intermediate (30%) | Narrow (50%) | Fine (100%)")]
    public float spectrum;

    [Header("Directional")]
    public bool directional = false;
    public float d_arc; // Usually 90 degrees
    public int d_distance;

    [Header("Machine Related")]
    [Tooltip("Ability of this potentially explosive machine to avoid being destabilized by a sutained attack, even those that do note pentrate its armor.")]
    public float stability;
    [Tooltip("")]
    public Vector2Int delay;
    
}

[System.Serializable]
public enum DeployableType
{
    Turret,
    Charge,
    Trap
}

[System.Serializable]
public class ItemSpecialAttack
{
    public bool specialMelee = false; // Removes the "Hit" section

    public bool datajack = false;
    public bool stasisBeam = false;
    public bool tearClaw = false;

}

#region Misc Effects
[System.Serializable]
[Tooltip("Various protection effects for armor pieces and similar items.")]
public class ItemProtectionEffect
{
    public bool hasEffect = false;

    [Header("General Protection")]
    [Tooltip("The slot type being protected. | None == Core")]
    public ArmorType armorEffect_slotType;
    [Tooltip("Absorbs 0.##% of damage that would otherwise affect <Slot Type>.")]
    public float armorEffect_absorbtion = 0f;
    [Tooltip("Negates extra effects of critical strikes against <Slot Type>.")]
    public bool armorEffect_preventCritStrikesVSSlot = false;
    [Tooltip("Prevents chain reactions due to electromagnetic damage.")]
    public bool armorEffect_preventChainReactions = false;
    [Tooltip("Can(not) protect against overflow damage.")]
    public bool armorEffect_preventOverflowDamage = false;

    [Header("Protection Exchange (Shields)")]
    public bool projectionExchange = false;
    [Tooltip("Blocks global damage. If false, only blocks damage to this part.")]
    public bool pe_global = false;
    [Tooltip("Blocks 0.##%")]
    public float pe_blockPercent = 0f;
    [Tooltip("Blocks ##% of damage to this part in exchange for energy loss at a #:# ratio (no effect if insufficient energy).")]
    public Vector2 pe_exchange = new Vector2(1, 1);
    public bool pe_includeVisibileAllies = false;
    public int pe_alliesDistance = 10;
    public bool pe_requireEnergy = true;

    [Header("High Coverage")]
    [Tooltip("Protects other parts via high coverage.")]
    public bool highCoverage = false;

    [Header("Type Damage Resistance")]
    public bool type_hasTypeResistance = false;
    public ItemDamageType type_damageType;
    [Tooltip("0.##")]
    public float type_percentage;
    public bool type_allTypes = false;
    public bool type_includeAllies = false;
    public int type_alliesRange = 10;

    [Header("Scrap Shield")]
    public bool scrapShield = false;

    [Header("Self Repair")]
    [Tooltip("Regenerates integrity at a rate of # per # turns..")]
    public bool selfRepair = false;
    public int selfRepair_amount = 0;
    public int selfRepairTurns = 0;

    [Header("Crit Immunity")] // Basically just Graphene Brace in here
    public bool critImmunity = false;

    public bool parallel = false;
    public bool resume = false;
    public bool stacks = false;
}

[System.Serializable]
public class ItemBonusSlots
{
    public bool hasEffect = false;
    public ItemSlot slotType;
    public int slots;
}

[System.Serializable]
public class ItemHeatDissipation
{
    public bool hasEffect = false;
    [Header("Per Turn")]
    public int dissipationPerTurn = 0;

    [Header("Lower Base")]
    public int lowerBaseTemp = 0;
    public bool preventPowerShutdown = false;
    [Tooltip("has a 0.##% chance to prevent other types of overheating side effects.")]
    public float preventOverheatSideEffects = 0f;

    [Header("Integrity Loss")]
    public int integrityLossPerTurn = 0;
    public int minTempToActivate = 0;

    [Header("Heat to Power")]
    [Tooltip("Generates # energy per # surplus heat every turn.")]
    public int energyGeneration = 0;
    public int heatToEnergyAmount = 0;

    [Tooltip("Dissipates heat each turn, losing # integrity per ## heat dissipated, " +
        "applied after all standard heat dissipation and any injectors, " +
        "and only when heat levels rise above ###. If multiple similar parts attached, " +
        "heat is distributed among them equally where possible.")]
    public bool abalativeArmorEffect = false;

    public bool parallel = false;
    public bool stacks = true;
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

    [Tooltip("True = Launchers | False = Guns, Cannons, & Launchers")]
    public bool launchersOnly = false;
    [Tooltip("0.##%")]
    public float fireTimeReduction;
    [Tooltip("Incompatible with Qunatum Capacitor & Autonomous Weapons")]
    public bool compatability = false;

    public bool stacks = true;
    public bool capped = false;
    [Tooltip("0.##%")]
    public float cap;
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
    [Tooltip("Breaks down over time")]
    public bool lifetimeDecay = false;

    // add more later
}

[System.Serializable]
[Tooltip("Effects chances to hit, crit chance, bypassing armor, etc.")]
public class ItemToHitEffects
{
    public bool hasEffect = false;
    [Tooltip("0.##")]
    public float amount;
    
    [Header("Type of Effect")]
    public bool bonusCritChance = false; // target analyzer
    [Tooltip("Doesn't apply to AOE attacks.")]
    public bool bypassArmor = false; // armor integ analyzer
    [Tooltip("Increases target core exposure.")]
    public bool coreExposureEffect = false; // Core analyzer
    [Tooltip("Applies to only gun, cannon, and melee attacks.")]
    public bool coreExposureGCM_only = true;

    [Tooltip("Flat bonuses (non melee only)")]
    public bool flatBonus = false;

    public bool stacks = false;
    [Tooltip("This effect stacks, but the bonus is halved.")]
    public bool halfStacks = false;
}

[System.Serializable]
[Tooltip("% chance to identify a random unidentified part in inventory each turn.")]
public class ItemPartIdentification
{
    public bool hasEffect = false;
    [Tooltip("0.##")]
    public float amount;
    public bool canIdentifyAlien = false;
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
    public float actuator_cap = 0.5f;

    public bool halfStacks = false;
    public bool stacks = false;

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
    [Tooltip("Instead of launchers, Energy gun or cannon (Quantum Capacitor)")]
    public bool forEnergyOrCannon = false;

    public bool stacks = false;
}

[System.Serializable]
[Tooltip("Increases <Weapon Type> damage by ##%.")]
public class ItemFlatDamageBonus
{
    public bool hasEffect = false;
    public List<ItemDamageType> types = new List<ItemDamageType>();
    [Tooltip("0.##%")]
    public float damageBonus = 0f;

    public bool stacks = false;
    public bool halfStacks = false;
}

[System.Serializable]
[Tooltip("Increases salvage recovered from targets, +# modifier. Compatible only with gun-type weapons that fire a single projectile.")]
public class ItemSalvageBonuses
{
    public bool hasEffect = false;
    public int bonus = 0;
    public bool gunTypeOnly = true;

    public bool stacks = false;
}

[System.Serializable]
public class ItemAlienBonuses
{
    public bool hasEffect = false;
    [Tooltip("0.##%")]
    public float amount = 0f;

    [Tooltip("While active, ##% of damage to this part is instead passed along to the core.")]
    public bool singleDamageToCore = false;
    [Tooltip("##% of damage to parts is instead transferred directly to the core.")]
    public bool allDamageToCore = false;

    // TODO: Expand this with more effects when needed

    public bool stacks = false;
    public bool half_stacks = false;
}

[System.Serializable]
public class ItemDetectionEffect
{
    public bool hasEffect = false;

    public int range = 0;

    [Header("Bot Detection")]
    [Tooltip("Enables robot scanning up to a distance of ##, once per turn.")]
    public bool botDetection = false;
    // extra stuff (sensor suite)
    /* Enables robot scanning up to a distance of 20, once per turn, in addition to all effects of a maximum-strength Signal Interpreter. 
     * Also detects long-term residual evidence of prior robot activity within field of view. 
     * 0b10 combat robots scanned by this device will report the event, once per bot.
     */
    public bool bd_maxInterpreterEffect = false;
    public bool bd_previousActivity = false;
    public bool bd_botReporting = false;

    [Header("Terrain Scanning")]
    public bool terrainScanning = false;

    [Header("Hauler Tracking")]
    [Tooltip("Enables both real-time tracking of 0b10 Haulers across the entire floor, and gives access to their current manifest. " +
        "Toggle active state to temporarily list all inventories in view. Only applies in 0b10-controlled areas.")]
    public bool haulerTracking = false;
    public bool ht_viewInventories = false;

    [Header("Seismic")]
    public bool seismic = false;

    [Header("Machine")]
    [Tooltip("Analyzes visible machines to locate others on the 0b10 network. Also determines whether an explosive machine has been destabilized and how long until detonation.")]
    public bool machine = false;

    [Header("Structural")]
    [Tooltip("Scans all visible walls to analyze the structure behind them, out to a depth of 1. " +
        "Also identifies hidden doorways and highlights areas that will soon cave in due to instability even without further stimulation.")]
    public bool structural = false;
    public int structural_depth = 1;

    [Header("Warlord Comms")]
    [Tooltip("Enables long-distance communication with Warlord forces to determine their composition and hiding locations. " +
        "Toggle active state to temporarily list all squads in the area. If active while near a squad, will signal it to emerge and assist.")]
    public bool warlordComms = false;


    public bool stacks = false;
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
    public int hackDetectRateBonus = 0; // Reduces Chances of Detection
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
#endregion

[System.Serializable]
public class ItemUnique 
{
    [Header("Scrap")]
    public bool isScrap = false;
    public ItemObject scrapReward;

    [Header("Data Log")]
    public bool isDataLog = false;
    public string logMessage = "";
    public GlobalActions logAction;

    [Header("Data Core")]
    public bool isDataCore;
    public Terminal coreTerminal;
}

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

    [Tooltip("Does the player have this schematic?")]
    public bool hasSchematic = false;
}

[System.Serializable]
public enum ItemDamageType
{
    Kinetic,
    Thermal,
    Explosive,
    EMP,
    Phasic,
    Impact,
    Slashing,
    Piercing,
    Entropic
}

[System.Serializable]
public enum ArmorType
{
    Power,
    Propulsion,
    Utility,
    Weapon,
    General,
    None // Core
}

[System.Serializable]
public enum ProjectileTrailStyle
{
    MissileMinor,
    MissileMajor,
    None
}

[System.Serializable]
public enum ItemRatingType
{
    [Tooltip("No special text here")]
    Standard,
    [Tooltip("Has a special text box next to the rating")]
    Prototype,
    [Tooltip("Has a special text box next to the rating")]
    Alien // ayy lmao
}

[System.Serializable]
public enum CritType
{
    /*  Burn: Significantly increase heat transfer (TH guns)
        Meltdown: Instantly melt target bot regardless of what part was hit (TH cannons)
        Destroy: Destroy part/core outright (= the original critical effect) (KI guns)
        Blast: Also damage a second part and knock it off target, as described above (KI cannons)
        Corrupt: Maximize system corruption effect (EM guns/cannons)
        Smash: As Destroy, but gets to also apply an equal amount of damage as overflow damage (impact weapons)
        Sever: Sever target part, or if hit core also damages and severs a different part (slashing weapons)
        Puncture: Half of damage automatically transferred to core (piercing weapons)
        (there are four other crit types associated with the special damage types not covered in this article: Detonate, Sunder, Intensity, and Phase)
     */

    Nothing, // Default
    Burn,
    Meltdown,
    Destroy,
    Blast,
    Corrupt,
    Smash,
    Sever,
    Puncture,
    Detonate,
    Sunder,
    Intensify,
    Phase,
    Impale
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

[System.Serializable]
public class SimplifiedItemEffect
{
    public int intVal;
    public float floatVal;

    public bool stacks;
    public bool half_stacks;

    public SimplifiedItemEffect(int intVal, float floatVal, bool stacks, bool half_stacks)
    {
        this.intVal = intVal;
        this.floatVal = floatVal;
        this.stacks = stacks;
        this.half_stacks = half_stacks;
    }
}