// Origionally made by: Chizaruu @ https://github.com/Chizaruu/Unity-RL-Tutorial/blob/part-4-field-of-view/Assets/Scripts/Entity/Entity.cs
// Expanded & Modified by: Cody Jackson @ codyj@nevada.unr.edu

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public static class Action
{
    public static void EscapeAction()
    {
        Debug.Log("Quit");
        //Application.Quit();
    }

    public static bool BumpAction(Actor actor, Vector2 direction)
    {
        Actor target = GameManager.inst.GetBlockingActorAtLocation(actor.transform.position + (Vector3)direction);

        if (target)
        {
            actor.ConfirmCollision(target);
            if (actor.confirmCollision)
            {
                // Play collision sound
                AudioManager.inst.PlayGlobalCombatSound(AudioManager.inst.GAME_Clips[7], 0.7f);
                MeleeAction(actor, target);
                ShuntAction(actor, target);
            }
            return false;
        }
        else
        {
            MovementAction(actor, direction);
            return true;
        }
    }

    public static void MeleeAction(Actor source, Actor target)
    {
        float targetCoreExposure;
        ItemObject weapon = FindMeleeWeapon(source);

        if (target.botInfo) // Bot
        {
            targetCoreExposure = target.botInfo.coreExposure;
            //ItemObject weapon = target.botInfo.armament
        }
        else // Player
        {
            targetCoreExposure = PlayerData.inst.currentCoreExposure;
        }

        if (weapon == null)
        {
            // This is gonna be a ramming attack

            /*
             * As a last resort, Cogmind can ram other robots to damage and/or push them out of the way. 
             * Damage is a random amount from 0 to (((10 + [mass]) / 5) + 1) * ([speed%] / 100) * [momentum], 
             * where speed% is current speed as a percentage of average speed (100) and effective momentum is a combination of both Cogmind and the target's momentum. 
             * However, the damage per attack is capped at 100 before the roll. Smashing into a robot headed straight for you can deal some serious damage, 
             * though there are serious negative consequences to go with this unarmed attack, and half as much damage is inflicted on Cogmind as well. 
             * Ramming increases the salvage potential of the target (by 3 for each hit), and also enables the collection of a small random amount of matter per collision. 
             * Ramming with active treads or legs always avoids self-damage and destabilization. 
             * Treads have a per-tread chance to instantly crush targets of medium size and below which have no more than 50 remaining core integrity. 
             * Crushed robots have their salvage modified by -20. Legs have a 20% chance per leg to kick the target out of the way. (Not applicable against huge targets.)
             * The time cost to ram is the greater (slower) of 100 and your current move speed.
             */

            if (target.botInfo) // Bot
            {

            }
            else // Player
            {
                float momentum = 1;
                float attackHigh = (((10 + PlayerData.inst.currentWeight) / 5) + 1) * (PlayerData.inst.moveSpeed1 / 100) * momentum;

                float damage = Random.Range(0, attackHigh);

                if(damage > 100)
                {
                    damage = 100;
                }

                // Increase salvage potential
                target.botInfo.salvagePotential += new Vector2Int(3, 3);

                // Deal damage to the bot
                // -Consider resistances
                (bool hasRes, float resAmount) = HasResistanceTo(target, ItemDamageType.Impact);
                if (hasRes)
                {
                    target.currentHealth -= (int)(damage - (damage * resAmount));
                }
                else
                {
                    target.currentHealth -= (int)damage;
                }
                

                // Deal half damage to player (if no legs/treads)
                if (HasTreads(source) || HasLegs(source))
                {
                    PlayerData.inst.currentHealth -= (int)(damage / 2);
                }
                else
                {
                    PlayerData.inst.currentHealth -= (int)(damage);
                }

                UIManager.inst.CreateNewLogMessage("Slammed into " + target.botInfo.name + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false);
            }
        }
        else
        {

        }



        TurnManager.inst.EndTurn(source);
 
        
    }

    /// <summary>
    /// Performs a Ranged Weapon Attack vs a specific target, with a specific weapon/
    /// </summary>
    /// <param name="source">The attacker.</param>
    /// <param name="target">The defender (being attacked).</param>
    /// <param name="weapon">The weapon being used to attack with.</param>
    public static void RangedAttackAction(Actor source, Actor target, Item weapon)
    {
        if(weapon == null || target == null)
        {

            TurnManager.inst.EndTurn(source);

            return;
        }

        if(weapon.itemData.shot.shotRange < Vector2.Distance(Action.V3_to_V2I(source.gameObject.transform.position), Action.V3_to_V2I(source.gameObject.transform.position)))
        {
            Action.SkipAction(source);
            return;
        }

        // We are doing a ranged attack vs a target
        // The default to-hit chance is 60%, we need to calculate the actual chance, which is based on...
        #region Hit Chance Calculation
        /*
         *  Hit Chance
         *  ------------
         *  Many factors affect the chance to hit a target. 
         *  The chance shown in the scan readout takes into account all factors except those which affect individual weapons in the volley. 
         *  Per-weapon chances are instead shown in the parts list next to their respective weapons once firing mode is activated and the cursor is highlighting a target.
         *  
         *  Base hit chance before any modification is 60%.
         *  
         *  Volley Modifiers:
         *  +3%/cell if range < 6
         *  +20%/30% if attacker in standard/high siege mode (non-melee only)
         *  +attacker utility bonuses
         *  +10% if attacker didn't move for the last 2 actions
         *  +3% of defender heat (if heat positive)
         *  +10%/+30% for large/huge targets
         *  +10% if defender immobile
         *  +5% w/robot analysis data
         *  -1~15% if defender moved last action, where faster = harder to hit
         *  -5~15% if defender running on legs (not overweight)
         *      (5% evasion for each level of momentum)
         *  -10% if attacker moved last action (ignored in melee combat)
         *  -5~15% if attacker running on legs (ranged attacks only)
         *      (5% for each level of momentum)
         *  -3% of attacker heat (if heat positive)
         *  -10%/-30% for small/tiny targets
         *  -10%/-5% if target is flying/hovering (and not overweight or in stasis)
         *  -20% for each robot obstructing line of fire
         *  -5% against Cogmind by robots for which have analysis data
         *  -defender utility bonuses
         */

        ItemShot shotData = weapon.itemData.shot;
        ItemProjectile projData = weapon.itemData.projectile;
        int projAmount = weapon.itemData.projectileAmount;

        float toHitChance = 0.60f; // Default to-hit chance of 60%.

        // - Range - //
        int distance = (int)Vector2Int.Distance(Action.V3_to_V2I(source.gameObject.transform.position), Action.V3_to_V2I(target.gameObject.transform.position));
        if (distance < 6)
        {
            toHitChance += (0.03f * distance);
        }

        // - Siege Mode - //
        if (source.GetComponent<PlayerData>())
        {
            if (PlayerData.inst.timeTilSiege == 0)
            {
                toHitChance += Random.Range(0.2f, 0.3f);
            }
        }
        else
        {
            if (source.siegeMode)
            {
                toHitChance += Random.Range(0.2f, 0.3f);
            }
        }

        // - Utility Bonuses - //
        (float bonus_melee, float bonus_ranged) = HasToHitBonus(source);
        toHitChance += bonus_ranged;

        // - No Movement past 2 turns - //
        if (source.noMovementFor >= 2)
        {
            toHitChance += 0.10f;
        }

        // - Target Heat value - //
        if (target.GetComponent<PlayerData>())
        {
            if (PlayerData.inst.currentHeat > 0)
            {
                toHitChance += 0.03f;
            }
        }
        else
        {
            if (target.currentHeat > 0)
            {
                toHitChance += 0.03f;
            }
        }

        // - Large/Huge target (doesn't apply to player) - //
        if (source.GetComponent<PlayerData>())
        {
            if (target.botInfo._size == BotSize.Large)
            {
                toHitChance += 0.10f;
            }
            else if (target.botInfo._size == BotSize.Huge)
            {
                toHitChance += 0.30f;
            }
        }

        // - Last move + speed - //
        if (target.noMovementFor > 0)
        {
            if (source.GetComponent<PlayerData>())
            {
                toHitChance += Mathf.RoundToInt(Mathf.Clamp(Mathf.Log(100 * (100 / PlayerData.inst.moveSpeed1), 2) * -2f, -15f, -1f));
            }
            else
            {
                toHitChance += Mathf.RoundToInt(Mathf.Clamp(Mathf.Log(100 * (100 / target.botInfo._movement.moveSpeedPercent), 2) * -2f, -15f, -1f));
            }
        }

        // - Target has legs - //
        if (HasLegs(target))
        {
            // Add momentum calculation in later
            toHitChance += -0.05f;
        }

        // - If attacker moved last action - //
        if (source.noMovementFor > 0)
        {
            toHitChance += -0.10f;
        }

        // - If attacker is using legs - //
        if (HasLegs(source))
        {
            // Add momentum calculation in later
            toHitChance += -0.05f;
        }

        // - Attacker heat - //
        if (source.GetComponent<PlayerData>())
        {
            if (PlayerData.inst.currentHeat > 0)
            {
                toHitChance += -0.03f;
            }
        }
        else
        {
            if (source.currentHeat > 0)
            {
                toHitChance += -0.03f;
            }
        }

        // - Target is Small/Tiny - //
        if (!target.GetComponent<PlayerData>()) // Applies to bots only
        {
            if (target.botInfo._size == BotSize.Tiny)
            {
                toHitChance += -0.30f;
            }
            else if (target.botInfo._size == BotSize.Small)
            {
                toHitChance += -0.10f;
            }
        }

        // - If target is flying or hovering (and not overweight or in stasis) - //
        if (HasFlight(target) && !IsOverweight(target) && !target.inStatis)
        {
            toHitChance += -0.10f;
        }
        else if (HasHover(target) && !IsOverweight(target) && !target.inStatis)
        {
            toHitChance += -0.05f;
        }

        // - If line of sight being blocked - // (THIS ALSO GETS USED LATER)
        Vector3 targetDirection = target.transform.position - source.transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector3(source.transform.position.x, source.transform.position.y, 0), targetDirection.normalized);
        List<Actor> botsHit = new List<Actor>();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            Actor bot = hit.collider.GetComponent<Actor>();
            if (bot != null && bot != source && bot != target)
            {
                botsHit.Add(bot);
            }
        }

        toHitChance += (botsHit.Count * -0.20f);

        // - Player has analysis of target -- //
        if (source.GetComponent<PlayerData>())
        {
            if (target.botInfo.playerHasAnalysisData)
            {
                toHitChance += 0.05f;
            }
        }


        // - Defender utility bonuses - // (also important later)
        (List<float> bonuses, bool noCrit, List<ArmorType> types) = DefenceBonus(target);
        toHitChance -= bonuses.Sum();

        #endregion

        float rand = Random.Range(0f, 1f);
        if (rand < toHitChance) // Success, a hit!
        {
            // For both cases we want to:
            // - Create an in-world projectile that goes to the target
            Color projColor = Random.ColorHSV();
            Color boxColor = new Color(projColor.r, projColor.g, projColor.b, 0.7f);
            UIManager.inst.CreateGenericProjectile(source.transform, target.transform, projColor, boxColor, Random.Range(15f, 20f), true);
            // - Play a shooting sound, from the source
            source.GetComponent<AudioSource>().PlayOneShot(shotData.shotSound[Random.Range(0, shotData.shotSound.Count - 1)]);

            // Deal Damage to the target
            int damageAmount = (int)Random.Range(projData.damage.x, projData.damage.y);

            if (!noCrit && rand <= projData.critChance) // Critical hit?
            {
                // A crit!
            }
            if (target.GetComponent<PlayerData>()) // Player being attacked
            {
                if (rand < PlayerData.inst.currentCoreExposure) // Hits the core
                {
                    PlayerData.inst.currentHealth -= damageAmount;
                }
                else // Hits a part
                {
                    DamageRandomPart(target, damageAmount, types);
                }

                // Do a calc message
                string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHitChance * 100}%) Hit";

                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                message = $"Recieved damage: {damageAmount}";
                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
            }
            else // Bot being attacked
            {
                if (rand < target.botInfo.coreExposure) // Hits the core
                {
                    target.currentHealth -= damageAmount;
                }
                /*
                else // Hits a part
                {
                    DamageRandomPart(target, damageAmount, types);
                }
                */

                // Show a popup that says how much damage occured
                if (!target.GetComponent<PlayerData>())
                {
                    UI_CombatPopup(target, damageAmount);
                }

                // Do a calc message
                string message = $"{weapon.itemData.name} ({toHitChance * 100}%) Hit";

                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
            }
        }
        else
        {  // ---------------------------- // Failure, a miss.

            // Create a projectile that will miss
            Color projColor = Random.ColorHSV();
            Color boxColor = new Color(projColor.r, projColor.g, projColor.b, 0.7f);
            UIManager.inst.CreateGenericProjectile(source.transform, target.transform, projColor, boxColor, Random.Range(20f, 15f), false);

            // Play a sound
            source.GetComponent<AudioSource>().PlayOneShot(shotData.shotSound[Random.Range(0, shotData.shotSound.Count - 1)]);


            if (target.GetComponent<PlayerData>()) // Player being targeted
            {
                // Do a calc message
                string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHitChance * 100}%) Miss";

                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange_faded, UIManager.inst.corruptOrange, false, true);
            }
            else // AI being targeted
            {
                // Do a calc message
                string message = $"{weapon.itemData.name} ({toHitChance * 100}%) Miss";

                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.dullGreen, UIManager.inst.normalGreen, false, true);
            }
        }

        // After we're done, we need to subtract the cost to fire the specified weapon from the attacker.
        if (source.GetComponent<PlayerData>())
        {
            PlayerData.inst.currentHeat += shotData.shotHeat;
            PlayerData.inst.currentMatter += shotData.shotMatter;
            PlayerData.inst.currentEnergy += shotData.shotEnergy;
        }
        else
        {
            source.currentHeat += shotData.shotHeat;
            source.currentMatter += shotData.shotMatter;
            // energy?
        }


        TurnManager.inst.EndTurn(source);


    }

    public static void MovementAction(Actor actor, Vector2 direction)
    {
        //Debug.Log($"{actor.name} moves {direction}!");
        actor.noMovementFor = 0;
        actor.Move(direction);
        actor.UpdateFieldOfView();

        TurnManager.inst.EndTurn(actor);

    }

    public static void SkipAction(Actor actor)
    {
        TurnManager.inst.EndTurn(actor);
    }

    /// <summary>
    /// "Shunt" (push) the specified target on tile away from the source.
    /// </summary>
    /// <param name="source">The source to be pushed away from.</param>
    /// <param name="target">The actor being pushed.</param>
    public static void ShuntAction(Actor source, Actor target)
    {
        // We need to move away from the source
        List<GameObject> neighbors = target.FindNeighbors((int)target.transform.position.x, (int)target.transform.position.y);

        List<GameObject> validMoveLocations = new List<GameObject>();

        foreach (var T in neighbors)
        {
            if (target.GetComponent<Actor>().IsUnoccupiedTile(T.GetComponent<TileBlock>()))
            {
                validMoveLocations.Add(T);
            }
        }

        GameObject moveLocation = target.GetComponent<Actor>().GetBestFleeLocation(source.gameObject, target.gameObject, validMoveLocations);

        if(moveLocation != null)
        {
            // Normalize move direction
            Vector2Int moveDir = new Vector2Int(0, 0);
            if (moveLocation.transform.position.x > target.transform.position.x)
            {
                moveDir.x++;
            }
            else if (moveLocation.transform.position.x < target.transform.position.x)
            {
                moveDir.x--;
            }

            if (moveLocation.transform.position.y > target.transform.position.y)
            {
                moveDir.y++;
            }
            else if (moveLocation.transform.position.y < target.transform.position.y)
            {
                moveDir.y--;
            }

            // Move here
            MovementAction(target, moveDir);
        }
        else
        {
            // Don't move
        }

        // Passive bots will flee temporarily
        if (target.GetComponent<AI_Passive>())
        {
            target.GetComponent<AI_Passive>().FleeFromSource(source);
        }

        target.confirmCollision = false; // Unset the flag
    }

    #region HelperFunctions

    public static ItemObject FindMeleeWeapon(Actor actor)
    {
        ItemObject weapon = null;

        // For this case, we consider any weapon with a range less than or equal to 2 a melee weapon.

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.shot.shotRange <= 2)
                    {
                        weapon = item._item;
                        return weapon;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.shot.shotRange <= 2 && item.item.itemData.state)
                    {
                        weapon = item.item.itemData;
                        return weapon;
                    }
                }
            }
        }

        return weapon;
    }

    public static Item FindRangedWeapon(Actor actor)
    {
        Item weapon = null;

        // For this case, we consider any weapon with a range greater than 3 to be a ranged weapon.

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.shot.shotRange > 3)
                    {
                        weapon = item._item.data;
                        return weapon;
                    }
                }


            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.shot.shotRange > 3 && item.item.itemData.state)
                    {
                        weapon = item.item;
                        return weapon;
                    }
                }
            }
        }

        return weapon;
    }

    public static bool HasTreads(Actor actor)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Treads)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Treads && item.item.itemData.state)
                    {
                        return true;
                    }
                }
                
            }
        }

        return false;
    }

    public static bool HasLegs(Actor actor)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Legs)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Legs && item.item.itemData.state)
                    {
                        return true;
                    }
                }
                
            }
        }

        return false;
    }

    public static bool HasWheels(Actor actor)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Wheels)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if(item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Wheels && item.item.itemData.state)
                    {
                        return true;
                    }
                }

                
            }
        }
        return false;
    }

    public static bool HasHover(Actor actor)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Hover)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Hover && item.item.itemData.state)
                    {
                        return true;
                    }
                }
                
            }
        }

        return false;
    }

    public static bool HasFlight(Actor actor)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Flight)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Flight && item.item.itemData.state)
                    {
                        return true;
                    }
                }
                
            }
        }

        return false;
    }

    // To hit bonus from utilities, first return is ranged bonus, second is melee
    public static (float, float) HasToHitBonus(Actor actor)
    {
        bool stacks = true;
        float bonus_melee = 0f;
        float bonus_ranged = 0f;

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.itemEffect.Count > 0 && item._item.itemEffect[0].hasAccuracyBuff && stacks)
                    {
                        bonus_melee += item._item.itemEffect[0].accBuff_melee;
                        bonus_ranged += item._item.itemEffect[0].accBuff_nonMelee;

                        if (item._item.itemEffect[0].accBuff_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.itemEffect.Count > 0 && item.item.itemData.itemEffect[0].hasAccuracyBuff && stacks)
                    {
                        bonus_melee += item.item.itemData.itemEffect[0].accBuff_melee;
                        bonus_ranged += item.item.itemData.itemEffect[0].accBuff_nonMelee;

                        if (item.item.itemData.itemEffect[0].accBuff_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }
                    }
                }

            }
        }

        return (bonus_melee, bonus_ranged);
    }

    // Defence bonus from utilities, first return is list of defense bonuses, second is prevents crits, third is list of protected slot types
    public static (List<float>, bool, List<ArmorType>) DefenceBonus(Actor actor)
    {
        List<float> bonuses = new List<float>();
        List<ArmorType> types = new List<ArmorType>();
        bool stacks = true;
        bool noCrits = false;

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.itemEffect.Count > 0 && item._item.itemEffect[0].armorEffect && stacks)
                    {
                        bonuses.Add(item._item.itemEffect[0].armorEffect_absorbtion);

                        if (item._item.itemEffect[0].armorEffect_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }

                        if (item._item.itemEffect[0].armorEffect_preventCritStrikes)
                        {
                            noCrits = true;
                        }

                        types.Add(item._item.itemEffect[0].armorEffect_slotType);
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.itemEffect.Count > 0 && item.item.itemData.itemEffect[0].armorEffect && stacks)
                    {
                        bonuses.Add(item.item.itemData.itemEffect[0].armorEffect_absorbtion);

                        if (item.item.itemData.itemEffect[0].armorEffect_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }

                        if (item.item.itemData.itemEffect[0].armorEffect_preventCritStrikes)
                        {
                            noCrits = true;
                        }

                        types.Add(item.item.itemData.itemEffect[0].armorEffect_slotType);
                    }
                }

            }
        }

        return (bonuses, noCrits, types);
    }
    public static List<ItemObject> HasArmor(Actor actor)
    {
        List<ItemObject> foundArmor = new List<ItemObject>();

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if (item._item.type == ItemType.Armor)
                    {
                        foundArmor.Add(item._item);
                    }
                }
                
                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Armor)
                    {
                        foundArmor.Add(item.item.itemData);
                    }
                }
                
            }
        }

        return foundArmor;
    }

    public static (bool, float) HasResistanceTo(Actor actor, ItemDamageType resistance)
    {
        if (actor.botInfo) // Bot
        {
            foreach (BotResistances res in actor.botInfo.resistances)
            {
                if (res.damageType == resistance)
                {
                    return (true, res.resistanceAmount);
                }
            }
        }
        else // Player
        {
            // Player shouldn't be resistant to anything... right?
            return (false, 0f);
        }

        return (false, 0f);
    }

    public static (bool, List<ItemObject>) FindPlayerHackware()
    {
        bool hasHackware = false;

        List<ItemObject> hackware = new List<ItemObject>();

        foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>()._invUtility.Container.Items)
        {
            if (item.item.Id >= 0)
            {
                if (item.item.itemData.type == ItemType.Hackware)
                {
                    hackware.Add(item.item.itemData);
                }
            }

        }

        return (hasHackware, hackware);
    }


    public static Vector2Int V3_to_V2I(Vector3 input)
    {
        return new Vector2Int((int)input.x, (int)input.y);
    }

    public static int GetTotalMass(Actor actor)
    {
        int totalMass = 0;

        // Items in inventory don't count!
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    totalMass += item._item.mass;
                }
            }

            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.data.Id >= 0)
                {
                    totalMass += item._item.mass;
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPower.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
        }

        return totalMass;
    }

    public static bool IsOverweight(Actor actor)
    {
        return GetTotalMass(actor) > GetTotalSupport(actor);
    }

    /// <summary>
    /// Calculates the speed an actor can move at, along with the energy cost & head cost to move 1 tile.
    /// </summary>
    /// <param name="actor">The actor in question.</param>
    /// <returns>The speed an actor can move at (Time/Move), the energy it will cost to move one tile, and the heat produced traveling one tile.</returns>
    public static (int, float, float) GetSpeed(Actor actor)
    {
        int speed = 0;
        float energyCost = 0;
        float heatCost = 0;

        List<int> propulsionParts = new List<int>();

        /* We want to do the following:
         * -Get the total movement of whatever propulsion parts they have (averaged out???)
         * -Calculate their total mass (inventory items don't count!)
         * 
         */

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.data.Id >= 0) // There's something there
                {
                    if (item._item.propulsion.Count > 0) // And its got propulsion data
                    {
                        propulsionParts.Add(item._item.propulsion[0].timeToMove);
                        energyCost += item._item.propulsion[0].propEnergy;
                        heatCost += item._item.propulsion[0].propHeat;
                    }
                }
            }
        }
        else // Player
        {
            if(PlayerData.inst.moveType == BotMoveType.Running) // Running has a base speed of 50
            {
                propulsionParts.Add(50);
                energyCost += 1;
                heatCost += 0;
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.propulsion.Count > 0) // And its got propulsion data
                    {
                        propulsionParts.Add(item.item.itemData.propulsion[0].timeToMove);
                        energyCost += item.item.itemData.propulsion[0].propEnergy;
                        heatCost += item.item.itemData.propulsion[0].propHeat;
                    }
                }
            }
        }

        // Average out the time to move
        if (propulsionParts.Count > 0)
        {
            speed = (int)propulsionParts.Average();
            float ratio = GetTotalMass(actor) / speed; // Get ratio of mass / speed
            speed = (int)(speed - (speed * ratio)); // Give penatly from mass
        }
        else
        {
            speed = 50;
        }

        return (speed, energyCost, heatCost);
    }

    public static int GetTotalSupport(Actor actor)
    {
        int support = PlayerData.inst.baseWeightSupport; // Just use 3 as default
        bool stacks = true;

        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.data.Id >= 0) // There's something there
                {
                    if (item._item.propulsion.Count > 0) // And its got propulsion data
                    {
                        support += item._item.propulsion[0].support;
                    }

                    if (item._item.itemEffect[0].hasMassSupport && stacks)
                    {
                        support += item._item.itemEffect[0].massSupport;
                        if (item._item.itemEffect[0].massSupport_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.propulsion.Count > 0) // And its got propulsion data
                    {
                        support += item.item.itemData.propulsion[0].support;
                    }

                    if (item.item.itemData.itemEffect[0].hasMassSupport && stacks)
                    {
                        support += item.item.itemData.itemEffect[0].massSupport;
                        if (item.item.itemData.itemEffect[0].massSupport_stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }
                    }
                }
            }
        }

        return support;
    }

    public static void UI_CombatPopup(Actor actor, int damage)
    {
        Color a = Color.black, b = Color.black, c = Color.black;
        a = Color.black;
        string _message = "";

        _message = damage.ToString();
        
        // Set color related to current item health
        float HP = actor.currentHealth / actor.maxHealth;
        if (HP >= 0.75) // Healthy
        {
            b = UIManager.inst.activeGreen; // Special item = special color
            c = new Color(UIManager.inst.activeGreen.r, UIManager.inst.activeGreen.g, UIManager.inst.activeGreen.b, 0.7f);
        }
        else if (HP < 0.75 && HP >= 0.5) // Minor Damage
        {
            b = UIManager.inst.cautiousYellow; // Special item = special color
            c = new Color(UIManager.inst.cautiousYellow.r, UIManager.inst.cautiousYellow.g, UIManager.inst.cautiousYellow.b, 0.7f);
        }
        else if (HP < 0.5 && HP >= 0.25) // Medium Damage
        {
            b = UIManager.inst.slowOrange; // Special item = special color
            c = new Color(UIManager.inst.slowOrange.r, UIManager.inst.slowOrange.g, UIManager.inst.slowOrange.b, 0.7f);
        }
        else // Heavy Damage
        {
            b = UIManager.inst.dangerRed; // Special item = special color
            c = new Color(UIManager.inst.dangerRed.r, UIManager.inst.dangerRed.g, UIManager.inst.dangerRed.b, 0.7f);
        }
        
        UIManager.inst.CreateCombatPopup(actor.gameObject, _message, a, b, c);
    }

    public static void DamageRandomPart(Actor target, int damage, List<ArmorType> protection)
    {
        if (target.botInfo) // Bot
        {
            foreach (BotArmament item in target.botInfo.armament)
            {
                if (item._item.data.Id >= 0)
                {
                    if(protection.Count > 0)
                    {
                        if (item._item.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item._item.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item._item.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item._item.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (protection.Contains(ArmorType.General))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                    }
                    else
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }

            foreach (BotArmament item in target.botInfo.components)
            {
                if (item._item.data.Id >= 0)
                {
                    // If we got here, we need to damage some kind of armor
                    if (item._item.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item._item.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item._item.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item._item.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (protection.Contains(ArmorType.General))
                    {
                        item._item.integrityCurrent -= damage;
                        if (item._item.integrityCurrent <= 0)
                        {
                            item._item.data.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invPower.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (protection.Count > 0)
                    {
                        if (item.item.itemData.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (protection.Contains(ArmorType.General))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                    }
                    else
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }
            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (protection.Count > 0)
                    {
                        if (item.item.itemData.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (protection.Contains(ArmorType.General))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                    }
                    else
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }
            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (protection.Count > 0)
                    {
                        if (item.item.itemData.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (item.item.itemData.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                        else if (protection.Contains(ArmorType.General))
                        {
                            // Don't damage this part, damage the armor itself
                        }
                    }
                    else
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }
            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    // If we got here, we need to damage some kind of armor
                    if (item.item.itemData.slot == ItemSlot.Power && protection.Contains(ArmorType.Power))
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item.item.itemData.slot == ItemSlot.Propulsion && protection.Contains(ArmorType.Propulsion))
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item.item.itemData.slot == ItemSlot.Utilities && protection.Contains(ArmorType.Utility))
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (item.item.itemData.slot == ItemSlot.Weapons && protection.Contains(ArmorType.Weapon))
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                    else if (protection.Contains(ArmorType.General))
                    {
                        item.item.itemData.integrityCurrent -= damage;
                        if (item.item.itemData.integrityCurrent <= 0)
                        {
                            item.item.Id = -1; // Destroy it
                            // And play a sound ?
                        }
                        return;
                    }
                }
            }

            int random = Random.Range(0, target.GetComponent<PartInventory>()._invUtility.Container.Items.Length - 1);
            // As a failsafe, try to damage a random item in the utilities slot
            target.GetComponent<PartInventory>()._invUtility.Container.Items[random].item.itemData.integrityCurrent -= damage;
            if (target.GetComponent<PartInventory>()._invUtility.Container.Items[random].item.itemData.integrityCurrent <= 0)
            {
                target.GetComponent<PartInventory>()._invUtility.Container.Items[random].item.Id = -1; // Destroy it
                                    // And play a sound ?
            }
            return;
            
        }
    }

    public static Vector2Int NormalizeMovement(Transform obj, Vector3 destination)
    {
        // Normalize move direction
        Vector2Int moveDir = new Vector2Int(0, 0);
        if (destination.x > obj.position.x)
        {
            moveDir.x++;
        }
        else if (destination.x < obj.position.x)
        {
            moveDir.x--;
        }

        if (destination.y > obj.position.y)
        {
            moveDir.y++;
        }
        else if (destination.y < obj.position.y)
        {
            moveDir.y--;
        }

        return moveDir;
    }

    #endregion
}
