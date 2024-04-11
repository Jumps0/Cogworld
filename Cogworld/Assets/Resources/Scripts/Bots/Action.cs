// Origionally made by: Chizaruu @ https://github.com/Chizaruu/Unity-RL-Tutorial/blob/part-4-field-of-view/Assets/Scripts/Entity/Entity.cs
// Expanded & Modified by: Cody Jackson @ codyj@nevada.unr.edu

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Color = UnityEngine.Color;

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
                MeleeAction(actor, target.gameObject);
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

    public static void MeleeAction(Actor source, GameObject target)
    {
        // TODO: Factor in attack time, and followup attacks.

        float targetCoreExposure;
        Item weapon = FindMeleeWeapon(source);

        // Are we attacking a bot or a structure?
        if (target.GetComponent<Actor>())
        {
            if (target.GetComponent<Actor>().botInfo) // Bot
            {
                targetCoreExposure = target.GetComponent<Actor>().botInfo.coreExposure;
                //ItemObject weapon = target.botInfo.armament
            }
            else // Player
            {
                targetCoreExposure = PlayerData.inst.currentCoreExposure;
            }

            if (weapon == null) // -- RAMMING --
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

                if (target.GetComponent<Actor>().botInfo) // Bot
                {
                    // TODO: Bot attacking!
                }
                else // Player
                {
                    int momentum = PlayerData.inst.GetComponent<Actor>().momentum;
                    float attackHigh = (((10 + PlayerData.inst.currentWeight) / 5) + 1) * (PlayerData.inst.moveSpeed1 / 100) * momentum;

                    float damage = Random.Range(0, attackHigh);

                    if (damage > 100)
                    {
                        damage = 100;
                    }

                    // Increase salvage potential
                    target.GetComponent<Actor>().botInfo.salvagePotential += new Vector2Int(3, 3);

                    // Deal damage to the bot
                    // -Consider resistances
                    (bool hasRes, float resAmount) = HasResistanceTo(target.GetComponent<Actor>(), ItemDamageType.Impact);
                    if (hasRes)
                    {
                        target.GetComponent<Actor>().currentHealth -= (int)(damage - (damage * resAmount));
                    }
                    else
                    {
                        target.GetComponent<Actor>().currentHealth -= (int)damage;
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

                    UIManager.inst.CreateNewLogMessage("Slammed into " + target.GetComponent<Actor>().botInfo.name + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false);
                }
            }
            else
            {
                #region Explainer
                /*
                > Hit Chance
                Melee attacks use the same hit chance calculations as ranged combat, with a few exceptions:
                  -Base hit% is 70
                  -No range modifier
                  -No heat modifiers
                  -Utility modifiers use those applicable to melee combat
                > Momentum
                All melee weapons benefit from greater momentum in the direction of the attack. 
                Moving multiple consecutive spaces in a row prior to an attack adds additional momentum, 
                up to a maximum of 3. Current momentum is displayed as a number at the end of the HUD's movement data readout.

                The damage bonus on a successful melee hit is +1~40% for non-piercing weapons, or +2~80% for piercing weapons, 
                calculated as: ([momentum] * [speed%] / 1200) * 40). For piercing attacks the multiplier is 80 instead of 40. 
                Speed% is current speed as a percentage of average speed, 100. Thus the more momentum and speed with which you attack, 
                the greater the damage. Your status page shows the damage bonus taking current momentum into account. 
                Note that while some utilities help increase momentum, the maximum bonus damage for a given weapon type cannot be exceeded.

                Stopping, performing a non-movement action, or otherwise moving in a direction that does not match the current
                momentum resets your momentum to 0, except when moving diagonally in the same general direction (e.g. turning from southeast to south).
                The latter case instead reduces total momentum by 1. Also note that technically any melee attack can take advantage
                of previously accumulated momentum, regardless of direction. For example, approaching the southern side of a target
                from the east and attacking northward while passing below it will apply whatever momentum value is displayed in the HUD.

                >Sneak Attacks 
                Melee attacking an enemy that has not yet noticed you gives a base hit chance of 120%, 
                and a +100% damage bonus which stacks with any momentum bonus. Sneak attacks work on any non-fleeing neutral targets as well,
                since they don't expect you'll suddenly attack them!

                >Multi-Wielding
                Although only one primary melee weapon can be active at a time, other attached but inactive melee
                weapons have a chance to carry out "follow-up attacks" alongside the primary weapon. 
                In Tactical HUD mode, that chance is shown next to each applicable melee weapon. 
                Follow-up attacks are more likely when the primary weapon is slower than a given backup melee weapon. 
                The chance may also be affected by supporting utilities.

                Multiple follow-up attacks by different melee weapons are possible in the same action.
                Each weapon is checked separately, and once confirmed any follow-up attack has a +10% to hit the target.
                Each weapon incurs no additional time cost to attack aside from modifying the total attack time by half of their time delay
                (whether positive or negative). Momentum bonuses that apply to the primary weapon all apply to follow-up attacks as well.
                The benefits of all actuators also apply to every follow-up attack. If the target is destroyed by an earlier attack,
                any remaining follow-up attacks will switch to another target in range if there is one.

                Datajacks cannot be involved in follow-up attacks, and these attacks are only applicable against robot targets.
                 */
                #endregion

                float toHit = Random.Range(0f, 1f);

                float hitChance = 0.7f; // Default hit chance is 70%
                int momentum = source.momentum; // Get momentum
                // Calculate and assign the various melee bonuses
                List<float> meleeBonuses = Action.GetMeleeBonuses(source.GetComponent<Actor>(), weapon);
                float bonus_followups = meleeBonuses[0];
                float bonus_maxDamage = meleeBonuses[1];
                float bonus_accuracy = meleeBonuses[2];
                int bonus_minDamage = (int)meleeBonuses[3];
                float bonus_attackTime = meleeBonuses[4];

                int attackTime = weapon.itemData.meleeAttack.delay;
                attackTime = Mathf.RoundToInt(attackTime + (attackTime *  bonus_attackTime));

                if(source.gameObject != PlayerData.inst.gameObject && !target.GetComponent<BotAI>().canSeePlayer) // Not the player and (currently) can't see the player
                {
                    hitChance = 1.2f; // Base hit chance is increased!
                }

                #region Hit Chance Calculation
                toHit += bonus_accuracy; // Modify by accuracy

                bool noCrit = false;
                List<ArmorType> types = new List<ArmorType>();
                // Use the to-hit function
                (hitChance, noCrit, types) = Action.CalculateMeleeHitChance(source, target.GetComponent<Actor>(), weapon, hitChance);
                #endregion

                if (toHit <= hitChance) // Hit!
                {
                    #region Damage Calculation
                    // - Calculate the Damage -
                    int damageAmount = 0;

                    Vector2Int lowHigh = weapon.itemData.meleeAttack.damage; // First get the flat damage rolls from the weapon
                    // Then modify the minimum and maximum values if needed
                    if(bonus_maxDamage > 0)
                    {
                        lowHigh.y = Mathf.RoundToInt(lowHigh.y + (lowHigh.y * bonus_maxDamage));
                    }
                    if (bonus_minDamage > 0)
                    {
                        lowHigh.x = Mathf.RoundToInt(lowHigh.x + (lowHigh.x * bonus_minDamage));
                    }

                    // Then get the new flat damage (semi-random)
                    damageAmount = Random.Range(lowHigh.x, lowHigh.y);

                    // Consider any flat damage bonuses
                    damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

                    // Player has analysis of target
                    if (source.GetComponent<PlayerData>())
                    {
                        if (target.GetComponent<Actor>().botInfo.playerHasAnalysisData)
                        {
                            damageAmount = Mathf.RoundToInt(damageAmount + (float)(damageAmount * 0.10f)); // +10% damage bonus
                        }
                    }

                    // link_complan damage bonus
                    if (target.GetComponent<Actor>().botInfo && target.GetComponent<Actor>().hacked_mods.Contains(ModHacks.link_complan))
                    {
                        damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * 0.25f)); // +25%
                    }

                    // Then modify based on momentum
                    if (momentum > 0)
                    {
                        float momentumBonus = 0;

                        // Determine the mult
                        int mult = 40;
                        if(weapon.itemData.meleeAttack.damageType == ItemDamageType.Piercing)
                        {
                            mult = 80;
                        }
                        // Get the speed
                        float speed;
                        if (source.GetComponent<PlayerData>())
                        {
                            speed = PlayerData.inst.moveSpeed1;
                        }
                        else
                        {
                            speed = target.GetComponent<Actor>().botInfo._movement.moveSpeedPercent;
                        }
                        // Calculate using: ([momentum] * [speed%] / 1200) * 40)
                        momentumBonus = (momentum * speed / 1200 * mult);

                        // Apply the momentum bonus
                        damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * momentumBonus));
                    }

                    // Now for sneak attacks
                    if (source.gameObject != PlayerData.inst.gameObject && !target.GetComponent<BotAI>().canSeePlayer) // Not the player and (currently) can't see the player
                    {
                        damageAmount *= 2; // +100% damage (so x2)
                    }

                    #endregion

                    #region Damage Dealing
                    // - Deal the damage -
                    bool crit = false;
                    if (!noCrit && weapon.itemData.meleeAttack.critType != CritType.Nothing && Random.Range(0f,1f) <= (weapon.itemData.meleeAttack.critical + Action.GatherCritBonuses(source))) // Critical hit?
                    {
                        crit = true;
                    }
                    if (target.GetComponent<PlayerData>()) // Player being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);

                        // Do a calc message
                        string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHit * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                        message = $"Recieved damage: {damageAmount}";
                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                    }
                    else // Bot being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);


                        // Show a popup that says how much damage occured
                        if (!target.GetComponent<PlayerData>())
                        {
                            UI_CombatPopup(target.GetComponent<Actor>(), damageAmount);
                        }

                        // Do a calc message
                        string message = $"{weapon.itemData.name} ({toHit * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                    }
                    #endregion

                    // -- Now for this visuals and audio --
                    // - We need to spawn a visual on top of the target. Where the line is facing from the player to the target.
                    // Calculate direction vector from object A to object B
                    Vector3 direction = target.transform.position - source.transform.position;
                    // Calculate angle in radians
                    float angleRad = Mathf.Atan2(direction.y, direction.x);
                    // Convert angle from radians to degrees
                    float angleDeg = angleRad * Mathf.Rad2Deg;
                    GameManager.inst.CreateMeleeAttackIndicator(HF.V3_to_V2I(target.transform.position), angleDeg, weapon);
                    // - Sound is taken care of in ^ -
                }
                else // Miss.
                {
                    // -- Now for this visuals and audio --
                    // - We need to spawn a visual on top of the target. Where the line is facing from the player to the target.
                    // Calculate direction vector from object A to object B
                    Vector3 direction = target.transform.position - source.transform.position;
                    // Calculate angle in radians
                    float angleRad = Mathf.Atan2(direction.y, direction.x);
                    // Convert angle from radians to degrees
                    float angleDeg = angleRad * Mathf.Rad2Deg;
                    GameManager.inst.CreateMeleeAttackIndicator(HF.V3_to_V2I(target.transform.position), angleDeg, weapon, false);
                    // - Sound is taken care of in ^ -
                }

                #region Followups
                List<Item> tertiaryWeapons = Action.GetMultiwieldingWeapons(source); // Look for any tertiary weapons

                if (tertiaryWeapons.Count > 0) // Has tertiary weapons
                {
                    foreach (Item SW in tertiaryWeapons)
                    {
                        hitChance = hitChance + (hitChance * 0.1f); // +10% bonus
                        hitChance = hitChance + (hitChance * bonus_followups); // Follow-ups bonuses

                        attackTime = Mathf.RoundToInt(attackTime + (attackTime * (weapon.itemData.meleeAttack.delay / 2))); // Halved attack time

                        #region Hit or Miss
                        toHit = Random.Range(0f, 1f);
                        if (toHit <= hitChance) // Hit!
                        {
                            // First of all, is the original target still alive?
                            if (target != null)
                            { // Yes continue.
                                continue;
                            }
                            else // Uh oh, we need a new target!
                            {
                                Actor neighbor = Action.FindNewNeighboringEnemy(source);

                                if(neighbor != null)
                                {
                                    target = neighbor.gameObject;
                                }
                                else
                                {
                                    // No valid neighbors. Bail out.
                                    break;
                                }
                            }

                            #region Damage Calculation
                            // - Calculate the Damage -
                            int damageAmount = 0;

                            Vector2Int lowHigh = weapon.itemData.meleeAttack.damage; // First get the flat damage rolls from the weapon
                                                                                     // Then modify the minimum and maximum values if needed
                            if (bonus_maxDamage > 0)
                            {
                                lowHigh.y = Mathf.RoundToInt(lowHigh.y + (lowHigh.y * bonus_maxDamage));
                            }
                            if (bonus_minDamage > 0)
                            {
                                lowHigh.x = Mathf.RoundToInt(lowHigh.x + (lowHigh.x * bonus_minDamage));
                            }

                            // Then get the new flat damage (semi-random)
                            damageAmount = Random.Range(lowHigh.x, lowHigh.y);

                            // Consider any flat damage bonuses
                            damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

                            // Player has analysis of target
                            if (source.GetComponent<PlayerData>())
                            {
                                if (target.GetComponent<Actor>().botInfo.playerHasAnalysisData)
                                {
                                    damageAmount = Mathf.RoundToInt(damageAmount + (float)(damageAmount * 0.10f)); // +10% damage bonus
                                }
                            }

                            // link_complan damage bonus
                            if (target.GetComponent<Actor>().botInfo && target.GetComponent<Actor>().hacked_mods.Contains(ModHacks.link_complan))
                            {
                                damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * 0.25f)); // +25%
                            }

                            // Then modify based on momentum
                            if (momentum > 0)
                            {
                                float momentumBonus = 0;

                                // Determine the mult
                                int mult = 40;
                                if (weapon.itemData.meleeAttack.damageType == ItemDamageType.Piercing)
                                {
                                    mult = 80;
                                }
                                // Get the speed
                                float speed;
                                if (source.GetComponent<PlayerData>())
                                {
                                    speed = PlayerData.inst.moveSpeed1;
                                }
                                else
                                {
                                    speed = target.GetComponent<Actor>().botInfo._movement.moveSpeedPercent;
                                }
                                // Calculate using: ([momentum] * [speed%] / 1200) * 40)
                                momentumBonus = (momentum * speed / 1200 * mult);

                                // Apply the momentum bonus
                                damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * momentumBonus));
                            }

                            // Now for sneak attacks
                            if (source.gameObject != PlayerData.inst.gameObject && !target.GetComponent<BotAI>().canSeePlayer) // Not the player and (currently) can't see the player
                            {
                                damageAmount *= 2; // +100% damage (so x2)
                            }

                            #endregion

                            #region Damage Dealing
                            // - Deal the damage -
                            bool crit = false;
                            if (!noCrit && weapon.itemData.meleeAttack.critType != CritType.Nothing && Random.Range(0f, 1f) <= (weapon.itemData.meleeAttack.critical + Action.GatherCritBonuses(source))) // Critical hit?
                            {
                                crit = true;
                            }
                            if (target.GetComponent<PlayerData>()) // Player being attacked
                            {
                                DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);

                                // Do a calc message
                                string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHit * 100}%) Hit";

                                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                                message = $"Recieved damage: {damageAmount}";
                                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                            }
                            else // Bot being attacked
                            {
                                DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);


                                // Show a popup that says how much damage occured
                                if (!target.GetComponent<PlayerData>())
                                {
                                    UI_CombatPopup(target.GetComponent<Actor>(), damageAmount);
                                }

                                // Do a calc message
                                string message = $"{weapon.itemData.name} ({toHit * 100}%) Hit";

                                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                            }
                            #endregion

                            // - No additional visuals/audio
                        }
                        else // Miss.
                        {
                            // - No additional visuals/audio
                        }

                        #endregion
                    }
                }

                #endregion
            }
        }
        else // Attacking a structure
        {
            // First off, are we attacking an empty floor tile?
            if(target.GetComponent<TileBlock>() && target.GetComponent<TileBlock>().tileInfo.type == TileType.Floor)
            {
                // Cancel early
                return;
            }

            // It's a structure, we cannot (and should not) miss.

            int momentum = source.momentum; // Get momentum
                                            // Calculate and assign the various melee bonuses
            List<float> meleeBonuses = Action.GetMeleeBonuses(source.GetComponent<Actor>(), weapon);
            float bonus_maxDamage = meleeBonuses[1];
            int bonus_minDamage = (int)meleeBonuses[3];
            float bonus_attackTime = meleeBonuses[4];

            int attackTime = weapon.itemData.meleeAttack.delay;
            attackTime = Mathf.RoundToInt(attackTime + (attackTime * bonus_attackTime));

            int armor = 0; // The armor value of the target

            #region Damage Calculation
            // - Calculate the Damage -
            int damageAmount = 0;

            Vector2Int lowHigh = weapon.itemData.meleeAttack.damage; // First get the flat damage rolls from the weapon
                                                                     // Then modify the minimum and maximum values if needed
            if (bonus_maxDamage > 0)
            {
                lowHigh.y = Mathf.RoundToInt(lowHigh.y + (lowHigh.y * bonus_maxDamage));
            }
            if (bonus_minDamage > 0)
            {
                lowHigh.x = Mathf.RoundToInt(lowHigh.x + (lowHigh.x * bonus_minDamage));
            }

            // Then get the new flat damage (semi-random)
            damageAmount = Random.Range(lowHigh.x, lowHigh.y);

            // Consider any flat damage bonuses
            damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

            // Player has analysis of target
            if (source.GetComponent<PlayerData>())
            {
                if (target.GetComponent<Actor>().botInfo.playerHasAnalysisData)
                {
                    damageAmount = Mathf.RoundToInt(damageAmount + (float)(damageAmount * 0.10f)); // +10% damage bonus
                }
            }

            // Then modify based on momentum
            if (momentum > 0)
            {
                float momentumBonus = 0;

                // Determine the mult
                int mult = 40;
                if (weapon.itemData.meleeAttack.damageType == ItemDamageType.Piercing)
                {
                    mult = 80;
                }
                // Get the speed
                float speed;
                if (source.GetComponent<PlayerData>())
                {
                    speed = PlayerData.inst.moveSpeed1;
                }
                else
                {
                    speed = target.GetComponent<Actor>().botInfo._movement.moveSpeedPercent;
                }
                // Calculate using: ([momentum] * [speed%] / 1200) * 40)
                momentumBonus = (momentum * speed / 1200 * mult);

                // Apply the momentum bonus
                damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * momentumBonus));
            }

            // Now for sneak attacks
            if (source.gameObject != PlayerData.inst.gameObject && !target.GetComponent<BotAI>().canSeePlayer) // Not the player and (currently) can't see the player
            {
                damageAmount *= 2; // +100% damage (so x2)
            }

            #endregion

            // What are we attacking?
            if (target.GetComponent<MachinePart>()) // Some type of machine
            {
                armor = target.GetComponent<MachinePart>().armor.y;
            }
            else if (target.GetComponent<TileBlock>()) // A wall or door
            {
                armor = target.GetComponent<TileBlock>().tileInfo.armor;
            }

            #region Beat the Armor?
            if (damageAmount > armor) // Success! Destroy the structure
            {
                // -- Now for this visuals and audio --
                // - We need to spawn a visual on top of the target. Where the line is facing from the player to the target.
                // Calculate direction vector from object A to object B
                Vector3 direction = target.transform.position - source.transform.position;
                // Calculate angle in radians
                float angleRad = Mathf.Atan2(direction.y, direction.x);
                // Convert angle from radians to degrees
                float angleDeg = angleRad * Mathf.Rad2Deg;
                GameManager.inst.CreateMeleeAttackIndicator(HF.V3_to_V2I(target.transform.position), angleDeg, weapon);
                // - Sound is taken care of in ^ -

                // - Destroy the object
                if (target.GetComponent<MachinePart>()) // Some type of machine
                {
                    target.GetComponent<MachinePart>().DestroyMe();

                    // Do a calc message
                    string message = $"{target.name} Destroyed ({damageAmount} > {armor})";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                }
                else if (target.GetComponent<TileBlock>()) // A wall or door
                {
                    target.GetComponent<TileBlock>().DestroyMe();
                }

            }
            else // Failure. Don't destroy the structure
            {
                // -- Now for this visuals and audio --
                // - We need to spawn a visual on top of the target. Where the line is facing from the player to the target.
                // Calculate direction vector from object A to object B
                Vector3 direction = target.transform.position - source.transform.position;
                // Calculate angle in radians
                float angleRad = Mathf.Atan2(direction.y, direction.x);
                // Convert angle from radians to degrees
                float angleDeg = angleRad * Mathf.Rad2Deg;
                GameManager.inst.CreateMeleeAttackIndicator(HF.V3_to_V2I(target.transform.position), angleDeg, weapon, false);
                // - Sound is taken care of in ^ -

                // Don't make a message about it
            }
            #endregion

            // No follow-ups for structures
        }

        // Finally, subtract the attack cost
        Action.DoWeaponAttackCost(source, weapon);

        // Overloaded consequences
        if (weapon.isOverloaded)
        {
            Action.OverloadedConsequences(source, weapon);
        }

        source.momentum = 0;
        TurnManager.inst.EndTurn(source); // Alter this later

    }

    /// <summary>
    /// Performs a Ranged Weapon Attack vs a specific target, with a specific weapon. (This also handles AOE attacks).
    /// </summary>
    /// <param name="source">The attacker.</param>
    /// <param name="target">The defender (being attacked).</param>
    /// <param name="weapon">The weapon being used to attack with.</param>
    public static void RangedAttackAction(Actor source, GameObject target, Item weapon)
    {
        if(weapon == null || target == null)
        {

            TurnManager.inst.EndTurn(source); // Alter this later

            return;
        }

        // Is this redundant?
        if(weapon.itemData.shot.shotRange < Vector2.Distance(Action.V3_to_V2I(source.gameObject.transform.position), Action.V3_to_V2I(source.gameObject.transform.position)))
        {
            Action.SkipAction(source);
            return;
        }

        ItemShot shotData = weapon.itemData.shot;

        if (weapon.itemData.explosionDetails.radius > 0) // AOE Attacks
        {
            List<GameObject> targets = new List<GameObject>();

            int falloff = weapon.itemData.explosionDetails.fallOff;
            int radius = weapon.itemData.explosionDetails.radius;
            Vector2Int center = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.y));
            List<Vector2Int> effectedTiles = new List<Vector2Int>();

            // == We are going to start in the center and then work outwards, dealing with any obstructions as we go. ==
            // The general idea that we will follow is:
            // - Bots will NEVER block explosion "waves", it goes straight through them (same with floor tiles, duh).
            // - Machines will initally block explosions unless they are destroyed by said explosion.
            // - Walls will block explosions unless they are destroyed by said explosion.
            // - Doors act similar to walls (unless they are open).


            // > Uniquely we will do the center tile alone incase the user shoots into a wall too strong for them to kill.
            bool centerAttack = Action.IndividualAOEAttack(source, HF.GetTargetAtPosition(center), weapon, 0);
            effectedTiles.Add(center);

            // Create a dictionary to keep track of blocking tiles
            Dictionary<Vector2Int, bool> blockingTiles = new Dictionary<Vector2Int, bool>();

            // Create a list of points to check
            List<Vector2Int> points = new List<Vector2Int>();
            Vector2Int bottomLeft = new Vector2Int(center.x - radius, center.y - radius);
            for (int x = bottomLeft.x; x < bottomLeft.x + (radius * 2); x++)
            {
                for (int y = bottomLeft.y; y < bottomLeft.y + (radius * 2); y++)
                {
                    Vector2Int point = new Vector2Int(x, y);
                    if(Vector2.Distance(point, center) <= radius && point != center)
                    {
                        points.Add(point);
                    }
                }
            }

            points.Sort((v1, v2) => (v1 - center).sqrMagnitude.CompareTo((v2 - center).sqrMagnitude)); // Sort list based on distance from center

            // We will loop through in a spiral pattern outward from the center.
            foreach (Vector2Int P in points)
            {
                Vector2Int tilePosition = P;
                int distFromCenter = Mathf.RoundToInt(Vector2.Distance(new Vector2(P.x, P.y), center));

                // Next check if this position is blocked by another tile via raycast
                RaycastHit2D[] hits = Physics2D.RaycastAll(center, tilePosition);
                bool clear = true;

                foreach (var H in hits) // Go through the hits and see if there are any obstructions.
                {
                    Vector2Int hPos = new Vector2Int(Mathf.RoundToInt(H.transform.position.x), Mathf.RoundToInt(H.transform.position.y));

                    if (blockingTiles.ContainsKey(hPos) && blockingTiles[hPos] == false)
                    {
                        clear = false;
                    }
                }

                if (clear) // Our path is clear
                {
                    // Perform AOE attack on the current tile
                    bool blocksExplosion = Action.IndividualAOEAttack(source, HF.GetTargetAtPosition(tilePosition), weapon, (distFromCenter * falloff));
                    blockingTiles.Add(tilePosition, blocksExplosion);

                    effectedTiles.Add(tilePosition);
                }
            }

            // > Visuals & Audio <
            #region Visuals & Audio
            // - Create the visuals on each effected tile. These vary greatly per weapon.
            // TODO: Explosion visuals
            CFXManager.inst.CreateExplosionFX(center, weapon.itemData, effectedTiles, center);
            // - Play the explosion sound where the attack lands.
            AudioManager.inst.CreateTempClip(new Vector3(center.x, center.y, 0f), HF.RandomClip(weapon.itemData.explosion.explosionSounds));
            #endregion

            TurnManager.inst.AllEntityVisUpdate();
        }
        else // Normal Ranged attacks
        {
            // We are doing a ranged attack vs a target
            float toHitChance = 0f;
            bool noCrit = false;
            List<ArmorType> types = new List<ArmorType>();
            ItemProjectile projData = weapon.itemData.projectile;
            int projAmount = weapon.itemData.projectileAmount;

            if (target.GetComponent<Actor>()) // We are actually attacking a bot
            {
                (toHitChance, noCrit, types) = Action.CalculateRangedHitChance(source, target.GetComponent<Actor>(), weapon);

                #region Hit or Miss
                float rand = Random.Range(0f, 1f);
                if (rand < toHitChance) // Success, a hit!
                {
                    // For both cases we want to:
                    // - Create an in-world projectile that goes to the target
                    UIManager.inst.CreateGenericProjectile(source.transform, target.transform, weapon.itemData.projectile, Random.Range(15f, 20f), true);
                    // - Play a shooting sound, from the source
                    source.GetComponent<AudioSource>().PlayOneShot(shotData.shotSound[Random.Range(0, shotData.shotSound.Count - 1)]);

                    #region Damage Calculation
                    // Deal Damage to the target
                    int damageAmount = (int)Random.Range(projData.damage.x, projData.damage.y);

                    // Consider if the weapon is overloaded
                    /*
                     *  Overloading
                        -------------
                        Some energy weapons are capable of overloading, which doubles damage and energy cost while generating triple the heat. 
                        Overloaded projectile heat transfer is also one level higher than usual where applicable. 
                        However, firing an overloaded weapon has a chance to cause negative side effects, as reflected in that weapon's "stability" stat.
                     */
                    if (weapon.isOverloaded)
                    {
                        damageAmount *= 2;
                    }

                    // Consider any flat damage bonuses
                    damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

                    // Player has analysis of target
                    if (source.GetComponent<PlayerData>())
                    {
                        if (target.GetComponent<Actor>().botInfo.playerHasAnalysisData)
                        {
                            damageAmount = Mathf.RoundToInt(damageAmount + (float)(damageAmount * 0.10f)); // +10% damage bonus
                        }
                    }

                    // link_complan damage bonus
                    if (target.GetComponent<Actor>().botInfo && target.GetComponent<Actor>().hacked_mods.Contains(ModHacks.link_complan))
                    {
                        damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * 0.25f)); // +25%
                    }
                    #endregion

                    bool crit = false;
                    if (!noCrit && weapon.itemData.projectile.critType != CritType.Nothing && Random.Range(0f, 1f) <= (projData.critChance + Action.GatherCritBonuses(source))) // Critical hit?
                    {
                        crit = true;
                    }
                    if (target.GetComponent<PlayerData>()) // Player being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);

                        // Do a calc message
                        string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHitChance * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                        message = $"Recieved damage: {damageAmount}";
                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                    }
                    else // Bot being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData, crit);


                        // Show a popup that says how much damage occured
                        if (!target.GetComponent<PlayerData>())
                        {
                            UI_CombatPopup(target.GetComponent<Actor>(), damageAmount);
                        }

                        // Do a calc message
                        string message = $"{weapon.itemData.name} ({toHitChance * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                    }
                }
                else
                {  // ---------------------------- // Failure, a miss.

                    // Create a projectile that will miss
                    UIManager.inst.CreateGenericProjectile(source.transform, target.transform, weapon.itemData.projectile, Random.Range(20f, 15f), false);

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
                #endregion
            }
            else // We are attacking a structure
            {
                // It's a structure, we cannot (and should not) miss.

                #region Damage Calculation
                int damageAmount = (int)Random.Range(projData.damage.x, projData.damage.y); // The damage we will do (must beat armor value to be effective).

                // Consider any flat damage bonuses
                damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

                // Consider overloading
                if (weapon.isOverloaded)
                {
                    damageAmount *= 2;
                }
                #endregion

                // Get the armor value
                int armor = HF.TryGetStructureArmor(target);

                #region Beat the Armor?
                if (damageAmount > armor) // Success! Destroy the structure
                {
                    // - Create an in-world projectile that goes to the target
                    UIManager.inst.CreateGenericProjectile(source.transform, target.transform, weapon.itemData.projectile, Random.Range(15f, 20f), true);
                    // - Play a shooting sound, from the source
                    source.GetComponent<AudioSource>().PlayOneShot(shotData.shotSound[Random.Range(0, shotData.shotSound.Count - 1)]);

                    // - Destroy the object
                    if (target.GetComponent<MachinePart>()) // Some type of machine
                    {
                        target.GetComponent<MachinePart>().DestroyMe();

                        // Do a calc message
                        if (target.GetComponent<MachinePart>().parentPart == target.GetComponent<MachinePart>()) // Only if this is the parent part
                        {
                            string message = $"{target.name} Destroyed ({damageAmount} > {armor})";

                            UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                        }
                    }
                    else if (target.GetComponent<TileBlock>()) // A wall or door
                    {
                        target.GetComponent<TileBlock>().DestroyMe();
                    }

                }
                else // Failure. Don't destroy the structure
                {
                    // Create a projectile that will miss
                    UIManager.inst.CreateGenericProjectile(source.transform, target.transform, weapon.itemData.projectile, Random.Range(20f, 15f), false);

                    // Play a sound
                    source.GetComponent<AudioSource>().PlayOneShot(shotData.shotSound[Random.Range(0, shotData.shotSound.Count - 1)]);

                    // Don't make a message about it
                }
                #endregion
            }
        }


        // After we're done, we need to subtract the cost to fire the specified weapon from the attacker.
        Action.DoWeaponAttackCost(source, weapon);

        // Overloaded consequences
        if (weapon.isOverloaded)
        {
            Action.OverloadedConsequences(source, weapon);
        }

        source.momentum = 0;
        TurnManager.inst.EndTurn(source); // Alter this later
    }

    public static void MovementAction(Actor actor, Vector2 direction)
    {
        actor.noMovementFor = 0;
        actor.Move(direction); // Actually move the actor
        actor.UpdateFieldOfView(); // Update their FOV

        // -- Misc stuff --
        if (actor == PlayerData.inst.GetComponent<Actor>() && UIManager.inst.volleyMode)
        {
            UIManager.inst.Evasion_VolleyNonAnimation(); // Re-draw volley visuals if its active
        }
        // --           --

        // End the actor's turn
        TurnManager.inst.EndTurn(actor);

    }

    public static void SkipAction(Actor actor)
    {
        actor.momentum = 0; // Stopped moving? Lose all momentum.
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
        List<GameObject> neighbors = HF.FindNeighbors((int)target.transform.position.x, (int)target.transform.position.y);

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

        // Neutral bots will flee temporarily (maybe change this later?)
        if (HF.DetermineRelation(target, source) == BotRelation.Neutral)
        {
            target.GetComponent<BotAI>().FleeFromSource(source);
        }

        target.confirmCollision = false; // Unset the flag
    }

    /// <summary>
    /// Part of a larger AOE attack. This will indivudally target something and handle what happens independently.
    /// </summary>
    /// <param name="source">The attacker.</param>
    /// <param name="target">The thing being attacked.</param>
    /// <param name="weapon">The weapon being used.</param>
    /// <returns>Returns if the attack is able to pass through the target object.</returns>
    public static bool IndividualAOEAttack(Actor source, GameObject target, Item weapon, int falloff)
    {
        bool permiable = true;

        // There are a couple things we could be attacking here:
        // - Walls
        // - Bots
        // - Machines
        // - Doors
        // - The floor

        // > Get launcher loader/accuracy bonuses <
        float bonus_launcherAccuracy = 0f;
        float bonus_launcherLoading = 0f;
        #region Loader/Accuarcy bonuses

        int activeWeapons = Action.CountActiveWeapons(source); // Some bonuses only apply with a single weapon active.
        bool stacks = true;

        if (source != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in source.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0 && stacks)
                {
                    if (item._item.itemData.itemEffect.Count > 0 && item._item.itemData.itemEffect[0].launcherBonus.hasEffect)
                    {
                        bonus_launcherAccuracy += item._item.itemData.itemEffect[0].launcherBonus.launcherAccuracy;
                        if(activeWeapons == 1)
                            bonus_launcherLoading += item._item.itemData.itemEffect[0].launcherBonus.launcherLoading;

                        if (item._item.itemData.itemEffect[0].launcherBonus.stacks)
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
            foreach (InventorySlot item in source.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0 && stacks)
                {
                    if (item.item.itemData.itemEffect.Count > 0 && item.item.itemData.itemEffect[0].launcherBonus.hasEffect)
                    {
                        bonus_launcherAccuracy += item.item.itemData.itemEffect[0].launcherBonus.launcherAccuracy;
                        if (activeWeapons == 1)
                            bonus_launcherLoading += item.item.itemData.itemEffect[0].launcherBonus.launcherLoading;

                        if (item.item.itemData.itemEffect[0].launcherBonus.stacks)
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
        #endregion

        // Get the armor value if this is a structure
        int armor = HF.TryGetStructureArmor(target);
        ItemExplosion attackData = weapon.itemData.explosion;

        if (target.GetComponent<Actor>()) // We are attacking a bot
        {
            // We need to do standard hit/miss for this.

            permiable = true; // Bots are always permiable

            float toHitChance = 0f;
            bool noCrit = false;
            List<ArmorType> types = new List<ArmorType>();
            int projAmount = weapon.itemData.projectileAmount;

            (toHitChance, noCrit, types) = Action.CalculateRangedHitChance(source, target.GetComponent<Actor>(), weapon);

            #region Hit or Miss
            float rand = Random.Range(0f, 1f);
            if (rand < toHitChance) // Success, a hit!
            {

                #region Damage Calculation
                // Deal Damage to the target
                int damageAmount = (int)Random.Range(weapon.itemData.explosionDetails.damage.x, weapon.itemData.explosionDetails.damage.y);

                // Consider any flat damage bonuses
                damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

                damageAmount += falloff; // Apply damage falloff if any (remember it's negative).
                if(damageAmount < 1)
                    damageAmount = 1; // Minimum 1 damage

                // Player has analysis of target
                if (source.GetComponent<PlayerData>())
                {
                    if (target.GetComponent<Actor>().botInfo.playerHasAnalysisData)
                    {
                        damageAmount = Mathf.RoundToInt(damageAmount + (float)(damageAmount * 0.10f)); // +10% damage bonus
                    }
                }

                // link_complan damage bonus
                if (target.GetComponent<Actor>().botInfo && target.GetComponent<Actor>().hacked_mods.Contains(ModHacks.link_complan))
                {
                    damageAmount = Mathf.RoundToInt(damageAmount + (damageAmount * 0.25f)); // +25%
                }
                #endregion

                if (target.GetComponent<PlayerData>()) // Player being attacked
                {
                    DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData);

                    // Do a calc message
                    string message = $"{source.botInfo.name}: {weapon.itemData.name} ({toHitChance * 100}%) Hit";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                    message = $"Recieved damage: {damageAmount}";
                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                }
                else // Bot being attacked
                {
                    if (rand < target.GetComponent<Actor>().botInfo.coreExposure) // Hits the core
                    {
                        target.GetComponent<Actor>().currentHealth -= damageAmount;
                    }
                    else // Hits a part
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, types, weapon.itemData);
                    }


                    // Show a popup that says how much damage occured
                    if (!target.GetComponent<PlayerData>())
                    {
                        UI_CombatPopup(target.GetComponent<Actor>(), damageAmount);
                    }

                    // Do a calc message
                    string message = $"{weapon.itemData.name} ({toHitChance * 100}%) Hit";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                }
            }
            else
            {  // ---------------------------- // Failure, a miss.

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
            #endregion
        }
        else if(target.GetComponent<MachinePart>()) // We are attacking a machine
        {
            // For machines we can't miss, we just need to check if the damage we do beats the machine's armor.

            permiable = false; // Machines block explosions unless they are destroyed

            #region Damage Calculation
            // Calculate Damage
            int damageAmount = (int)Random.Range(weapon.itemData.explosionDetails.damage.x, weapon.itemData.explosionDetails.damage.y);

            // Consider any flat damage bonuses
            damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

            damageAmount += falloff; // Apply damage falloff if any (remember it's negative).
            if (damageAmount < 1)
                damageAmount = 1; // Minimum 1 damage
            #endregion

            #region Beat the Armor?
            if (damageAmount > armor) // Success! Destroy the structure
            {
                target.GetComponent<MachinePart>().DestroyMe();

                // Do a calc message
                if(target.GetComponent<MachinePart>().parentPart == target.GetComponent<MachinePart>()) // Only if this is the parent part
                {
                    string message = $"{target.name} Destroyed ({damageAmount} > {armor})";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                }

                permiable = true; // Destroyed so pass through
            }
            else // Failure, do nothing.
            {

            }
            #endregion

        }
        else if (target.GetComponent<TileBlock>()) // Some kind of tile
        {
            #region Damage Calculation
            // Calculate Damage
            int damageAmount = (int)Random.Range(weapon.itemData.explosionDetails.damage.x, weapon.itemData.explosionDetails.damage.y);

            // Consider any flat damage bonuses
            damageAmount = Action.AddFlatDamageBonuses(damageAmount, source, weapon);

            damageAmount += falloff; // Apply damage falloff if any (remember it's negative).
            if (damageAmount < 1)
                damageAmount = 1; // Minimum 1 damage
            #endregion

            // For structures we can't miss, we just need to check if the damage we do beats the structure's armor.
            if (target.GetComponent<TileBlock>().tileInfo.type == TileType.Door) // A door
            {
                // Is the door open?
                permiable = target.GetComponent<DoorLogic>().state; // Also if this is destroyed it becomes permiable either way

                #region Beat the Armor?
                if (damageAmount > armor) // Success! Destroy the structure
                {
                    target.GetComponent<TileBlock>().DestroyMe();

                    permiable = true; // Destroyed so pass through
                }
                else // Failure, do nothing.
                {

                }
                #endregion
            }
            else if (target.GetComponent<TileBlock>().tileInfo.type == TileType.Wall) // A wall
            {
                permiable = false; // Wall's will block the explosion (unless they are destroyed)

                #region Beat the Armor?
                if (damageAmount > armor) // Success! Destroy the structure
                {
                    target.GetComponent<TileBlock>().DestroyMe();

                    permiable = true; // Destroyed so pass through
                }
                else // Failure, do nothing.
                {

                }
                #endregion
            }
            else if (target.GetComponent<TileBlock>().tileInfo.type == TileType.Floor) // A floor
            {
                permiable = true; // Floor tiles won't block the explosion

                #region Beat the Armor?
                if (damageAmount > armor) // Success! Destroy the structure
                {
                    target.GetComponent<TileBlock>().DestroyMe();
                }
                else // Failure, do nothing.
                {

                }
                #endregion
            }
        }

        return permiable;
    }

    #region HelperFunctions

    public static (float, bool, List<ArmorType>) CalculateRangedHitChance(Actor source, Actor target, Item weapon)
    {
        // The default to-hit chance is 60%, we need to calculate the actual chance, which is based on...

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

        float toHitChance = 1f;
        // First factor in the target's evasion/avoidance rate
        float avoidance = 0f;
        List<int> unused = new List<int>();
        (avoidance, unused) = Action.CalculateAvoidance(target);
        toHitChance -= (avoidance / 100);

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

        /* // Calculated in Evasion
        // - If target is flying or hovering (and not overweight or in stasis) - //
        if (HasFlight(target) && !IsOverweight(target) && !target.inStatis)
        {
            toHitChance += -0.10f;
        }
        else if (HasHover(target) && !IsOverweight(target) && !target.inStatis)
        {
            toHitChance += -0.05f;
        }
        */

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

        #region AI Only Afflictions (ModHacks)
        // (AI Only) Afflicted with "scatter_targeting" condition
        if (source.botInfo)
        {
            if (source.hacked_mods.Contains(ModHacks.scatter_targeting))
            {
                toHitChance /= 2; // Halved
            }
        }

        // (AI Only) Afflicted with "mark_system" condition
        if(target.botInfo && !source.botInfo && PlayerData.inst.allies.Contains(source)) // Target is AI, Attacker is player, & Attacker is ally of player.
        {
            if (target.hacked_mods.Contains(ModHacks.mark_system))
            {
                toHitChance = toHitChance + (toHitChance * 0.10f);
            }
        }

        // Afflicted with "link_complan" condition (Increases your accuracy and damage against this bot by 25% while also impairing its accuracy against you by 25%)
        if (source.botInfo)
        {
            if (source.hacked_mods.Contains(ModHacks.link_complan))
            {
                toHitChance = toHitChance - (toHitChance * 0.25f); // -25%
            }
        }
        else
        {
            if (target.botInfo && target.hacked_mods.Contains(ModHacks.link_complan))
            {
                toHitChance = toHitChance + (toHitChance * 0.25f); // +25%
            }
        }

        // (AI Only) Afflicted with "broadcast_data" condition (or neighbor has it). (Gives yourself and all allies 25% better accuracy against this and all 0b10 combat bots within a range of 3.)
        if (target.botInfo && target.allegances.GetRelation(BotAlignment.Complex) == BotRelation.Friendly)
        {
            // Does this bot, or any nearby bots have this affliction?
            // (This kinda sucks performance wise cause we will be doing this a lot and 99% percent of the time the bot won't have this affliction! ;_;)

            bool afflicted = false;
            if (target.hacked_mods.Contains(ModHacks.broadcast_data))
            {
                afflicted = true;
            }

            if (!afflicted)
            {
                List<Actor> neighbors = HF.FindBotsWithinRange(target, 3);

                foreach (Actor a in neighbors)
                {
                    if (a.botInfo && a.allegances.GetRelation(BotAlignment.Complex) == BotRelation.Friendly && a.hacked_mods.Contains(ModHacks.broadcast_data))
                    {
                        afflicted = true;
                    }
                }
            }

            if (afflicted)
            {
                toHitChance = toHitChance + (toHitChance * 0.25f); // +25%
            }
        }

        // (AI Only) Afflicted with "disrupt_area" condition (or neighbor has it). (Reduces the accuracy of this bot by 25%, and that of all 0b10 combat bots within a range of 3.)
        if (source.botInfo && source.allegances.GetRelation(BotAlignment.Complex) == BotRelation.Friendly)
        {
            // Does this bot, or any nearby bots have this affliction?
            // (This kinda sucks performance wise cause we will be doing this a lot and 99% percent of the time the bot won't have this affliction! ;_;)

            bool afflicted = false;
            if (source.hacked_mods.Contains(ModHacks.broadcast_data))
            {
                afflicted = true;
            }

            if (!afflicted)
            {
                List<Actor> neighbors = HF.FindBotsWithinRange(source, 3);

                foreach (Actor a in neighbors)
                {
                    if (a.botInfo && a.allegances.GetRelation(BotAlignment.Complex) == BotRelation.Friendly && a.hacked_mods.Contains(ModHacks.broadcast_data))
                    {
                        afflicted = true;
                    }
                }
            }

            if (afflicted)
            {
                toHitChance = toHitChance - (toHitChance * 0.25f); // -25%
            }
        }

        #endregion

        return (toHitChance, noCrit, types);
    }

    public static (float, bool, List<ArmorType>) CalculateMeleeHitChance(Actor source, Actor target, Item weapon, float startingHitChance)
    {
        // Melee to hit chance is slightly different but mostly the same. So a lot of this is copied from CalculateRangedHitChance

        /*
         *  Hit Chance
         *  ------------
         *  Many factors affect the chance to hit a target.  
         *  
         *  
         *  Modifiers:
         *  +10% if attacker didn't move for the last 2 actions
         *  +10%/+30% for large/huge targets
         *  +10% if defender immobile
         *  +5% w/robot analysis data
         *  -1~15% if defender moved last action, where faster = harder to hit
         *  -5~15% if defender running on legs (not overweight)
         *      (5% evasion for each level of momentum)
         *  -10%/-30% for small/tiny targets
         *  -10%/-5% if target is flying/hovering (and not overweight or in stasis)
         *  -5% against Cogmind by robots for which have analysis data
         *  -defender utility bonuses
         */

        ItemShot shotData = weapon.itemData.shot;
        ItemProjectile projData = weapon.itemData.projectile;
        int projAmount = weapon.itemData.projectileAmount;

        float toHitChance = startingHitChance;
        // First factor in the target's evasion/avoidance rate
        float avoidance = 0f;
        List<int> unused = new List<int>();
        (avoidance, unused) = Action.CalculateAvoidance(target);
        toHitChance -= (avoidance / 100);

        // - No Movement past 2 turns - //
        if (source.noMovementFor >= 2)
        {
            toHitChance += 0.10f;
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
            toHitChance += -0.05f;
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

        return (toHitChance, noCrit, types);
    }

    /// <summary>
    /// Searches an actor's ACTIVE weapons for a melee weapon. Returns that weapon if one is found.
    /// </summary>
    /// <param name="actor">The actor to focus on.</param>
    /// <returns>The melee weapon in item form.</returns>
    public static Item FindMeleeWeapon(Actor actor)
    {
        Item weapon = null;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.meleeAttack.isMelee)
                    {
                        weapon = item._item.itemData.data;
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
                    if (item.item.itemData.meleeAttack.isMelee && item.item.state)
                    {
                        weapon = item.item.itemData.data;
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

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.shot.shotRange > 3)
                    {
                        weapon = item._item.itemData.data;
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
                    if (item.item.itemData.shot.shotRange > 3 && item.item.state)
                    {
                        weapon = item.item;
                        return weapon;
                    }
                }
            }
        }

        return weapon;
    }

    public static Item FindActiveWeapon(Actor actor)
    {
        Item activeItem = FindRangedWeapon(actor);

        if(activeItem == null)
        {
            // Try melee weapons
            activeItem = FindMeleeWeapon(actor);
            if (activeItem == null)
            {
                return null; // No active weapon
            }
            else
            {
                return activeItem;
            }
        }
        else
        {
            return activeItem;
        }
    }

    /// <summary>
    /// Counts the number of currently ACTIVE weapons this bot has.
    /// </summary>
    /// <param name="actor">The bot in question.</param>
    /// <returns>The ammount of currently ACTIVE weapons this bot has.</returns>
    public static int CountActiveWeapons(Actor actor)
    {
        int activeWeapons = 0;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.data.state)
                    {
                        activeWeapons++;
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
                    if (item.item.state)
                    {
                        activeWeapons++;
                    }
                }
            }
        }

        return activeWeapons;
    }

    /// <summary>
    /// Checks to see if the specified item is a melee weapon or not.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>If this weapon is (True) a melee weapon or not (False).</returns>
    public static bool IsMeleeWeapon(Item item)
    {
        return item.itemData.meleeAttack.isMelee;
    }

    /// <summary>
    /// Locates equipped but not enabled melee weapons tertiary to an enabled one for multi-wielding. Returns a list of any found weapons.
    /// </summary>
    /// <param name="actor">The bot to look inside of.</param>
    /// <returns>A list of equipped but non-enabled weapons.</returns>
    public static List<Item> GetMultiwieldingWeapons(Actor actor)
    {
        List<Item> weapons = new List<Item>();
        Item mainWeapon = Action.FindMeleeWeapon(actor);

        if (mainWeapon != null) // Has a melee weapon already in use
        {
            if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
            {
                foreach (BotArmament item in actor.botInfo.armament)
                {
                    if (item._item.itemData.data.Id >= 0)
                    {
                        if (item._item.itemData.meleeAttack.isMelee && item._item.itemData.data != mainWeapon && !item._item.itemData.isSpecialAttack) // Is a melee weapon, isn't the main weapon, and isn't a datajack
                        {
                            weapons.Add(item._item.itemData.data);
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
                        if (item.item.itemData.meleeAttack.isMelee && item.item != mainWeapon && !item.item.itemData.isSpecialAttack) // Is a melee weapon, isn't the main weapon, and isn't a datajack
                        {
                            weapons.Add(item.item);
                        }
                    }
                }
            }
        }

        return weapons;
    }

    /// <summary>
    /// Does this actor have a launcher weapon, and are they using it right now? Returns said weapon if they have it. Mostly for player targeting purposes.
    /// </summary>
    /// <param name="actor">The actor in question.</param>
    /// <returns>Returns the launcher weapon if there is one, if not then returns null.</returns>
    public static Item HasLauncher(Actor actor)
    {
        Item weapon = null;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Launcher && item._item.itemData.data.state)
                    {
                        weapon = item._item.itemData.data;
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
                    if (item.item.itemData.type == ItemType.Launcher && item.item.state)
                    {
                        weapon = item.item;
                        return weapon;
                    }
                }
            }
        }

        return weapon;
    }

    /// <summary>
    /// Finds the max range of the current weapon a bot has equipped.
    /// </summary>
    /// <param name="actor">The bot to focus on.</param>
    /// <returns>The range (in int form) of the weapon.</returns>
    public static int GetWeaponRange(Actor actor)
    {
        Item rangedWeapon = FindRangedWeapon(actor);

        if(rangedWeapon == null)
        {
            // They don't have a ranged weapon equipped, try looking for a melee weapon.
            Item meleeWeapon = FindMeleeWeapon(actor);

            if(meleeWeapon == null) // This bot doesn't have any active weapons
            {
                return 0;
            }
            else
            {
                return 2; // Should be 1 but we want to visualize +1 range.
            }
        }
        else
        {
            return rangedWeapon.itemData.shot.shotRange;
        }
    }

    public static bool WeaponHasPenetration(ItemObject item)
    {
        return item.projectile.penetrationCapability > 0;
    }

    #region Has Propulsion Type
    public static bool HasTreads(Actor actor)
    {
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Treads)
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
                    if (item.item.itemData.type == ItemType.Treads && item.item.state)
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Legs)
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
                    if (item.item.itemData.type == ItemType.Legs && item.item.state)
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Wheels)
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
                    if (item.item.itemData.type == ItemType.Wheels && item.item.state)
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Hover)
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
                    if (item.item.itemData.type == ItemType.Hover && item.item.state)
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Flight)
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
                    if (item.item.itemData.type == ItemType.Flight && item.item.state)
                    {
                        return true;
                    }
                }
                
            }
        }

        return false;
    }
    #endregion

    /// <summary>
    /// To hit bonus from utilities, first return is ranged bonus, second is melee
    /// </summary>
    /// <param name="actor">The bot to focus on.</param>
    /// <returns>1. Ranged Bonus 2. Melee Bonus</returns>
    public static (float, float) HasToHitBonus(Actor actor)
    {
        bool stacks = true;
        float bonus_melee = 0f;
        float bonus_ranged = 0f;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.itemEffect.Count > 0 && item._item.itemData.itemEffect[0].hasAccuracyBuff && stacks)
                    {
                        bonus_melee += item._item.itemData.itemEffect[0].accBuff_melee;
                        bonus_ranged += item._item.itemData.itemEffect[0].accBuff_nonMelee;

                        if (item._item.itemData.itemEffect[0].accBuff_stacks)
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

    public static List<float> GetMeleeBonuses(Actor actor, Item weapon)
    {
        List<float> bonuses = new List<float>();

        // == There are a bunch of melee bonuses we need to gather, these are: ==
        // - per-weapon chance of follow-up melee attacks (Actuator Array's)
        // - melee weapon maximum damage by +##%, and decreases melee attack accuracy by -##% (Force Booster's)
        // - melee attack accuracy by +##%, and minimum damage by # (cannot exceed weapon's maximum damage). (Melee Analysis Suite)
        // - reduces melee attack time by 50%. <stacks, capped at 50 % > (-actuationors)

        float bonus_followups = 0f;
        float bonus_maxDamage = 0f;
        float bonus_accuracy = 0f;
        int bonus_minDamage = 0;
        float bonus_attackTime = 0f;

        int stackTrack = 0;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.itemEffect.Count > 0 && item._item.itemData.itemEffect[0].meleeBonus.hasEffect)
                    {
                        bonus_followups += item._item.itemData.itemEffect[0].meleeBonus.melee_followUpChance;
                        bonus_maxDamage += item._item.itemData.itemEffect[0].meleeBonus.melee_maxDamageBoost;
                        bonus_accuracy += item._item.itemData.itemEffect[0].meleeBonus.melee_accuracyIncrease;
                        bonus_accuracy += item._item.itemData.itemEffect[0].meleeBonus.melee_accuracyDecrease;
                        bonus_minDamage += item._item.itemData.itemEffect[0].meleeBonus.melee_minDamageBoost;


                        if (item._item.itemData.itemEffect[0].meleeBonus.actuator_stacks) // This effect should stack
                        {
                            if(stackTrack == 0) // No decrease
                            {
                                bonus_attackTime += item._item.itemData.itemEffect[0].meleeBonus.melee_attackTimeDecrease;
                                stackTrack++;
                            }
                            else // Decrease by specified amount
                            {
                                // (Maybe do this different? i.e.: use previous item's stack decrease. idk)
                                bonus_attackTime += (item._item.itemData.itemEffect[0].meleeBonus.melee_attackTimeDecrease * item._item.itemData.itemEffect[0].meleeBonus.actuator_cap);
                                stackTrack++;
                            }
                        }
                        else // This effect shouldn't stack
                        {
                            
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
                    if (item.item.itemData.itemEffect.Count > 0 && item.item.itemData.itemEffect[0].meleeBonus.hasEffect)
                    {
                        bonus_followups += item.item.itemData.itemEffect[0].meleeBonus.melee_followUpChance;
                        bonus_maxDamage += item.item.itemData.itemEffect[0].meleeBonus.melee_maxDamageBoost;
                        bonus_accuracy += item.item.itemData.itemEffect[0].meleeBonus.melee_accuracyIncrease;
                        bonus_accuracy += item.item.itemData.itemEffect[0].meleeBonus.melee_accuracyDecrease;
                        bonus_minDamage += item.item.itemData.itemEffect[0].meleeBonus.melee_minDamageBoost;


                        if (item.item.itemData.itemEffect[0].meleeBonus.actuator_stacks) // This effect should stack
                        {
                            if (stackTrack == 0) // No decrease
                            {
                                bonus_attackTime += item.item.itemData.itemEffect[0].meleeBonus.melee_attackTimeDecrease;
                                stackTrack++;
                            }
                            else // Decrease by specified amount
                            {
                                // (Maybe do this different? i.e.: use previous item's stack decrease. idk)
                                bonus_attackTime += (item.item.itemData.itemEffect[0].meleeBonus.melee_attackTimeDecrease * item.item.itemData.itemEffect[0].meleeBonus.actuator_cap);
                                stackTrack++;
                            }
                        }
                        else // This effect shouldn't stack
                        {

                        }
                    }
                }

            }
        }

        // Finally pack them all into a list
        bonuses.Add(bonus_followups);
        bonuses.Add(bonus_maxDamage);
        bonuses.Add(bonus_accuracy);
        bonuses.Add(bonus_minDamage);
        bonuses.Add(bonus_attackTime);

        return bonuses;
    }

    // Defence bonus from utilities, first return is list of defense bonuses, second is prevents crits, third is list of protected slot types
    public static (List<float>, bool, List<ArmorType>) DefenceBonus(Actor actor)
    {
        List<float> bonuses = new List<float>();
        List<ArmorType> types = new List<ArmorType>();
        bool stacks = true;
        bool noCrits = false;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.itemEffect.Count > 0 && item._item.itemData.itemEffect[0].armorProtectionEffect.hasEffect && stacks)
                    {
                        bonuses.Add(item._item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_absorbtion);

                        if (item._item.itemData.itemEffect[0].armorProtectionEffect.stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }

                        if (item._item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_preventCritStrikesVSSlot)
                        {
                            noCrits = true;
                        }

                        types.Add(item._item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_slotType);
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
                    if (item.item.itemData.itemEffect.Count > 0 && item.item.itemData.itemEffect[0].armorProtectionEffect.hasEffect && stacks)
                    {
                        bonuses.Add(item.item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_absorbtion);

                        if (item.item.itemData.itemEffect[0].armorProtectionEffect.stacks)
                        {
                            stacks = true;
                        }
                        else
                        {
                            stacks = false;
                        }

                        if (item.item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_preventCritStrikesVSSlot)
                        {
                            noCrits = true;
                        }

                        types.Add(item.item.itemData.itemEffect[0].armorProtectionEffect.armorEffect_slotType);
                    }
                }

            }
        }

        return (bonuses, noCrits, types);
    }
    public static List<ItemObject> HasArmor(Actor actor)
    {
        List<ItemObject> foundArmor = new List<ItemObject>();

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if (item._item.itemData.type == ItemType.Armor)
                    {
                        foundArmor.Add(item._item.itemData);
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
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
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    totalMass += item._item.itemData.mass;
                }
            }

            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    totalMass += item._item.itemData.mass;
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

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0) // There's something there
                {
                    if (item._item.itemData.propulsion.Count > 0) // And its got propulsion data
                    {
                        propulsionParts.Add(item._item.itemData.propulsion[0].timeToMove);
                        energyCost += item._item.itemData.propulsion[0].propEnergy;
                        heatCost += item._item.itemData.propulsion[0].propHeat;
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

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0) // There's something there
                {
                    if (item._item.itemData.propulsion.Count > 0) // And its got propulsion data
                    {
                        support += item._item.itemData.propulsion[0].support;
                    }

                    if (item._item.itemData.itemEffect[0].hasMassSupport && stacks)
                    {
                        support += item._item.itemData.itemEffect[0].massSupport;
                        if (item._item.itemData.itemEffect[0].massSupport_stacks)
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

    public static Item DamageBot(Actor target, int damage, List<ArmorType> protection, ItemObject weapon, bool crit = false, int forcedOverflow = 0)
    {
        ItemDamageType damageType = HF.GetDamageType(weapon);

        #region Explanation
        /*
         * Coverage/Exposure
        -------------------
        Each part has a "coverage" rating which determines its likeliness to be hit by an incoming attack. 
        Values are relative, so attacks are weighted towards hitting parts with higher coverage. 
        Robot cores also have their own "exposure" rating which determines their likeliness to be hit; 
        this value is considered along with part coverage when determining whether an attack will strike the core. 
        The exact chance of a core/part to be hit is shown in parenthesis after its exposure/coverage on the relevant info screen. 
        You can also press 'c' to have the main HUD's parts list display a visualization of relative coverage, i.e. longer bars represent a greater chance for a given part to be hit.

        Some examples of how coverage determines hit locations will help understand how that stat actually works.

        Example 1: Cogmind's core has an exposure of 100. Say you equip only one part, 
        a weapon which also has a coverage of 100. Their total value is 200 (100+100), 
        so if you are hit by a projectile, each one has a 50% (100/200) chance to be hit.

        Example 2: You have the following parts attached:
            Ion Engine (60)
            Light Treads (120)
            Light Treads (120)
            Medium Laser (60)
            Assault Rifle (100)
        With your core (100), the total is 560, so the chance to hit each location is:
            Ion Engine: 60/560=10.7%
            Light Treads: 120/560=21.4% (each)
            Medium Laser: 60/560=10.7%
            Assault Rifle: 100/560=17.9%
            Core: 100/560=17.9%

        Enemy robots work the same way, so the more parts you blow off, 
        the more likely you are to hit and destroy their core. 
        Armor plating has a very high coverage, so it's more likely to be hit, 
        while tiny utilities such as embedded processors have very low coverage, 
        so you can expect them to last much longer (unless you have little or nothing else covering you). 
        As you progress, your core will become more and more protected by attached parts, 
        because you'll have many more of them, but by that time there are other dangers such as system corruption.
         */
        #endregion

        #region Exposure
        int coreExposure = 0;
        float coreHitChance = 0f;
        if (target.botInfo)
        {
            //coreExposure = target.botInfo.coreExposure; // This is a percent value in the game files and I don't know how to calculate the true value. Fun!
            coreExposure = 100;
            coreHitChance = target.botInfo.coreExposure;
        }
        else
        {
            coreExposure = PlayerData.inst.currentCoreExposure;
        }

        List<ItemObject> items = new List<ItemObject>();
        List<ItemObject> protectiveItems = new List<ItemObject>();

        // Collect up all the items
        if (target.botInfo) // Bot
        {
            foreach (BotArmament item in target.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    items.Add(item._item.itemData);
                }
            }

            foreach(BotArmament item in target.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    items.Add(item._item.itemData);
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invPower.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item.itemData);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item.itemData);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item.itemData);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item.itemData);
                }
            }
        }

        // Calculate max exposure
        int totalExposure = 0;

        foreach (var item in items)
        {
            totalExposure += item.coverage;

            // Gather up all protective items (we use this later)
            if (item.itemEffect[0].armorProtectionEffect.hasEffect)
            {
                protectiveItems.Add(item);
            }
        }
        totalExposure += coreExposure;

        // Calculate individual chances to hit each object
        List<KeyValuePair<ItemObject, float>> pairs = new List<KeyValuePair<ItemObject, float>>();

        foreach (var item in items)
        {
            pairs.Add(new KeyValuePair<ItemObject, float>(item, item.coverage / totalExposure));
        }

        // Calculate to hit chance for player
        if (!target.botInfo)
        {
            coreHitChance = coreExposure / totalExposure;
        }
        #endregion

        #region Damage Modification
        // Modify damage value if target has resistances
        if (target.botInfo)
        {
            foreach (var R in target.botInfo.resistances)
            {
                if(damageType == R.damageType)
                {
                    damage = Mathf.RoundToInt(damage + (float)(damage * R.resistanceAmount));
                }
            }
        }

        // Modify damage value if target has certain types of protective armor
        bool stacks = true;
        foreach (var P in protectiveItems)
        {
            if (P.itemEffect[0].armorProtectionEffect.type_hasTypeResistance) // Has a resistance to a type
            {
                if (P.itemEffect[0].armorProtectionEffect.type_allTypes || P.itemEffect[0].armorProtectionEffect.type_damageType == damageType) // Has damage resist or generalist resist
                {
                    if (stacks)
                    {
                        damage = Mathf.RoundToInt(damage + (float)(damage * P.itemEffect[0].armorProtectionEffect.type_percentage)); // Modify damage
                    }

                    stacks = P.itemEffect[0].armorProtectionEffect.stacks;
                }
            }
        }
        int damageA = damage; // Saved for later (Heat transfer)

        // Shields
        #region Shields
        foreach (var P in protectiveItems)
        {
            if (P.itemEffect[0].armorProtectionEffect.projectionExchange)
            {
                float blocks = P.itemEffect[0].armorProtectionEffect.pe_blockPercent;
                Vector2 exchange = P.itemEffect[0].armorProtectionEffect.pe_exchange;

                // Calculate modification
                int removed = Mathf.RoundToInt(damage * blocks);

                // Subtract energy
                int cost = Mathf.RoundToInt(removed * exchange.y);

                int currentEnergy = 0;
                if (target.botInfo) // Bot
                {
                    currentEnergy = target.currentEnergy;
                }
                else // Player
                {
                    currentEnergy = PlayerData.inst.currentEnergy;
                }

                // Does the target have the power to cover this?
                if(currentEnergy >= cost)
                {
                    // Yes, alter the damage
                    damage = damage - removed;
                    // And remove the power
                    if (target.botInfo) // Bot
                    {
                        target.currentEnergy -= cost;
                    }
                    else // Player
                    {
                       PlayerData.inst.currentEnergy -= cost;
                    }
                }
            }
        }

        // Now consider allies with shields
        List<Actor> allies = HF.GatherVisibleAllies(target);

        // - This is a whole nightmare of nested for loops but we gotta check everything!
        foreach (var bot in allies)
        {
            List<Item> allyItems = Action.CollectAllBotItems(bot);

            foreach (var item in allyItems)
            {
                if (item.Id >= 0 && item.itemData.itemEffect[0].armorProtectionEffect.hasEffect && item.itemData.itemEffect[0].armorProtectionEffect.projectionExchange)
                {
                    if (item.itemData.itemEffect[0].armorProtectionEffect.pe_includeVisibileAllies)
                    {
                        float blocks = item.itemData.itemEffect[0].armorProtectionEffect.pe_blockPercent;
                        Vector2 exchange = item.itemData.itemEffect[0].armorProtectionEffect.pe_exchange;

                        // Calculate modification
                        int removed = Mathf.RoundToInt(damage * blocks);

                        // Subtract energy
                        int cost = Mathf.RoundToInt(removed * exchange.y);

                        int currentEnergy = 0;
                        if (bot.botInfo) // Bot
                        {
                            currentEnergy = bot.currentEnergy;
                        }
                        else // Player
                        {
                            currentEnergy = PlayerData.inst.currentEnergy;
                        }

                        // Does the target have the power to cover this?
                        if (currentEnergy >= cost)
                        {
                            // Yes, alter the damage
                            damage = damage - removed;
                            // And remove the power
                            if (target.botInfo) // Bot
                            {
                                target.currentEnergy -= cost;
                            }
                            else // Player
                            {
                                PlayerData.inst.currentEnergy -= cost;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        int damageB = damage;


        #endregion

        #region Hitting Things
        // Roll to what we will hit
        float rand = Random.Range(0f, 1f);

        List<float> cumulativeProbabilities = new List<float>();
        float cumulativeProbability = 0f;

        foreach (var pair in pairs)
        {
            cumulativeProbability += pair.Value;
            cumulativeProbabilities.Add(cumulativeProbability);
        }

        int overflow = 0;
        bool hitPart = false;
        bool armorDestroyed = false;
        ItemSlot slotHit = ItemSlot.Other;
        Item hitItem = null;

        // Check which range the random number falls into
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (rand < cumulativeProbabilities[i])
            {
                // Item i is hit

                // Now before we deal damage to this item. We need to check if the target has slot specific protection, and deal damage to that aswell
                ItemSlot slot = pairs[i].Key.slot;
                int splitDamage = 0;
                Item splitItem = null;
                foreach (var A in protectiveItems)
                {
                    if (A.itemEffect[0].armorProtectionEffect.armorEffect_slotType.ToString().ToLower() == slot.ToString().ToLower()) // A bit of a janky way to convert
                    {
                        float percent = A.itemEffect[0].armorProtectionEffect.armorEffect_absorbtion;

                        splitDamage = Mathf.RoundToInt(damage * percent);
                        damage -= splitDamage;
                        splitItem = A.data;
                        break;
                    }
                }

                // Damage the specific protection if it exists
                if(splitItem != null)
                {
                    // Deal damage
                    hitItem = splitItem;
                    Action.DamageItem(target, splitItem, splitDamage);
                }


                // Deal damage
                hitItem = pairs[i].Key.data;
                (overflow, armorDestroyed) = Action.DamageItem(target, pairs[i].Key.data, damage);
                slotHit = pairs[i].Key.slot;
                hitPart = true;
            }
        }

        // If none of the items are hit, the core is hit
        if (!hitPart)
        {
            // We also need to check if the player has a core shielding item
            int splitDamage = 0;
            Item splitItem = null;
            foreach (var A in protectiveItems)
            {
                if (A.itemEffect[0].armorProtectionEffect.armorEffect_slotType == ArmorType.None)
                {
                    float percent = A.itemEffect[0].armorProtectionEffect.armorEffect_absorbtion;

                    splitDamage = Mathf.RoundToInt(damage * percent);
                    damage -= splitDamage;
                    splitItem = A.data;
                    break;
                }
            }

            // Damage the specific protection if it exists
            if (splitItem != null)
            {
                // Deal damage
                (overflow, armorDestroyed) = Action.DamageItem(target, splitItem, splitDamage);
            }

            // Deal damage to the core
            if (target.botInfo)
            {
                target.currentHealth -= damage;
            }
            else
            {
                PlayerData.inst.currentHealth -= damage;
            }
        }
        #endregion

        #region Damage Overflow
        /* Damage Overflow
        -----------------
        When a part is destroyed by damage that exceeds its remaining integrity, 
        the surplus damage is then applied directly to the core or another part as chosen by standard coverage/exposure rules. 
        No additional defenses are applied against that damage. 

        Exceptions: 
            -There is no damage overflow if the destroyed part itself is armor, 
            and overflow damage always targets armor first if there is any. 
            -Critical strikes that outright destroy a part can still cause overflow if their original damage amount exceeded the part's integrity anyway. 
            Damage overflow is caused by all weapons except those of the "gun" type, and can overflow through multiple destroyed parts if there is sufficient damage.
        */

        if(forcedOverflow > 0)
        {
            overflow += forcedOverflow;
        }

        if (hitPart && overflow > 0 && !armorDestroyed)
        {
            Action.DamageBot(target, damage, protection, weapon); // Recursion! (This doesn't strictly target armor first but whatever).
        }
        #endregion

        #region Crits
        // https://www.gridsagegames.com/blog/2021/05/design-overhaul-3-damage-types-and-criticals/

        // First off, is what we are targeting immune to crits?
        foreach (var item in protectiveItems)
        {
            if (item.itemEffect[0].armorProtectionEffect.armorEffect_preventCritStrikesVSSlot) // Armor protection
            {
                if (item.itemEffect[0].armorProtectionEffect.armorEffect_slotType.ToString().ToLower() == slotHit.ToString().ToLower())
                {
                    crit = false;
                    UIManager.inst.CreateNewLogMessage("    " + item.itemName + " prevented critical effect.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    break;
                }
            }

            if (item.itemEffect[0].armorProtectionEffect.critImmunity)
            {
                crit = false;
                UIManager.inst.CreateNewLogMessage("    " + item.itemName + " prevented critical effect.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                break;
            }
        }

        bool burnCrit = false;
        bool corruptCrit = false;
        if(crit == true && weapon.explosionDetails.radius <= 0) // Is a crit & not an explosion
        {
            CritType type = CritType.Nothing;
            if (weapon.meleeAttack.isMelee) // Melee
            {
                type = weapon.meleeAttack.critType;
            }
            else // Ranged
            {
                type = weapon.projectile.critType;
            }

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

            switch (type)
            {
                case CritType.Nothing:
                    break;
                case CritType.Burn:
                    burnCrit = true;
                    if (target.botInfo)
                    {
                        UIManager.inst.CreateNewCalcMessage(target.botInfo.name + " suffers critical burn.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        UIManager.inst.CreateNewCalcMessage("Critical heat increase detected.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Meltdown:
                    if (target.botInfo)
                    {
                        target.Die(target.name + " suffers critical meltdown.");
                    }
                    else
                    {
                        PlayerData.inst.currentHealth = 0;
                    }
                    break;
                case CritType.Destroy:
                    if (!hitPart)
                    {
                        // Destroy the core
                        if (target.botInfo)
                        {
                            target.Die(target.name + "'s core has been completely destroyed.");
                        }
                        else
                        {
                            PlayerData.inst.currentHealth = 0;
                        }
                    }
                    else
                    {
                        // Destroy the part if it hasn't been destroyed already
                        if(hitItem.Id >= 0 && hitItem.integrityCurrent > 0)
                        {
                            Action.DestoyItem(target, hitItem);
                        }
                    }
                    break;
                case CritType.Blast:
                    // Roll again!
                    Item part = Action.DamageBot(target, damage, protection, weapon, false);
                    if(part != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            HF.RemovePartFromBotInventory(target, part);
                            InventoryControl.inst.DropItemOnFloor(part, target, null);
                            
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(part, target, HF.FindPlayerInventoryFromItem(part));
                        }
                        UIManager.inst.CreateNewCalcMessage(part.Name + " was blasted off.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }

                    break;
                case CritType.Corrupt:
                    corruptCrit = true;
                    if (target.botInfo)
                    {
                        UIManager.inst.CreateNewCalcMessage(target.botInfo.name + " suffers critical corruption.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        UIManager.inst.CreateNewCalcMessage("Critical corruption increase detected.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Smash:
                    // Roll again!
                    Item part2 = Action.DamageBot(target, damage, protection, weapon, false, damage);
                    if (part2 != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            HF.RemovePartFromBotInventory(target, part2);
                            InventoryControl.inst.DropItemOnFloor(part2, target, null);
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(part2, target, HF.FindPlayerInventoryFromItem(part2));
                        }

                        UIManager.inst.CreateNewCalcMessage(part2.Name + " was smashed off.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Sever:
                    /*  One crit of note in terms of design is the “Sever” effect, 
                     *  which was originally a side effect of slashing damage (using the formula mentioned earlier, damage/3%), 
                     *  but rather than having yet another separate effect tied to this weapon type, 
                     *  the crit expansion was a good opportunity to both make it more obvious and decouple the effect from damage, 
                     *  which is otherwise sometimes limiting when it comes to weapon design, as the ability to sever is then always 
                     *  tied to the amount of damage dealt. Now it can be a separate static value that’s easier to control for, 
                     *  naturally at the cost of somewhat nerfing slashing weapons where it was possible to increase raw damage 
                     *  (and therefore severing chance) via momentum or other buffs.
                     */

                    if (!hitPart) // Hit core
                    {
                        // Roll again!
                        Item part3 = Action.DamageBot(target, damage, protection, weapon, false);
                        if (part3 != null)
                        {
                            // If the 2nd part survives, forcefully drop it.
                            if (target.botInfo)
                            {
                                HF.RemovePartFromBotInventory(target, part3);
                                InventoryControl.inst.DropItemOnFloor(part3, target, null);
                            }
                            else
                            {
                                InventoryControl.inst.DropItemOnFloor(part3, target, HF.FindPlayerInventoryFromItem(part3));
                            }

                            UIManager.inst.CreateNewCalcMessage(part3.Name + " was severed.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                        }
                    }
                    else // Hit part
                    {
                        if (hitItem != null)
                        {
                            // If the 2nd part survives, forcefully drop it.
                            if (target.botInfo)
                            {
                                HF.RemovePartFromBotInventory(target, hitItem);
                                InventoryControl.inst.DropItemOnFloor(hitItem, target, null);
                            }
                            else
                            {
                                InventoryControl.inst.DropItemOnFloor(hitItem, target, HF.FindPlayerInventoryFromItem(hitItem));
                            }
                        }
                    }

                    break;
                case CritType.Puncture:
                    // Deal damage to the core
                    if (target.botInfo)
                    {
                        target.currentHealth -= (damage / 2);

                        UIManager.inst.CreateNewCalcMessage(target.botInfo.name + " suffers critical puncture.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        PlayerData.inst.currentHealth -= (damage / 2);

                        UIManager.inst.CreateNewCalcMessage("Core has suffered critical puncture.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Detonate: // Vortex Rail, Vortex Rifle & Vortex Shotgun has this
                    // (Assumption): Destroy random [utility] part
                    // "Entropic reaction triggered in %1."
                    // "%1 blocked entropic reaction in %2." (shielding power, player only)
                    Item shield = HF.DoesBotHaveSheild(target);
                    Item itemTarget = Action.FindRandomItemOfSlot(target, ItemSlot.Utilities);
                    if (!target.botInfo && shield != null)
                    {
                        UIManager.inst.CreateNewCalcMessage(shield.Name + " blocked entropic reaction in " + itemTarget.Name + ".", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        Action.DestoyItem(target, itemTarget);
                        UIManager.inst.CreateNewCalcMessage("Entropic reaction triggered in " + itemTarget.Name + ".", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }

                    break;
                case CritType.Sunder: // BFG-9k Vortex Edition, Vortex Driver, Vortex Lancer & Vortex Cannon has this
                    // "[name] %1 ripped off."
                    // (Assumption): Damage & Remove random propulsion component
                    Item item4 = Action.FindRandomItemOfSlot(target, ItemSlot.Propulsion);
                    item4 = Action.DamageBot(target, damage, protection, weapon, false);
                    if (item4 != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            HF.RemovePartFromBotInventory(target, item4);
                            InventoryControl.inst.DropItemOnFloor(item4, target, null);
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(item4, target, HF.FindPlayerInventoryFromItem(item4));
                        }

                        UIManager.inst.CreateNewCalcMessage(item4.Name + " was ripped off.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Intensify: // Zio. Phaser-S/M/H have this
                    // (Assumption): Double Damage
                    if (hitItem.Id >= 0 && hitItem.integrityCurrent > 0)
                    {
                        Action.DamageBot(target, damage, protection, weapon, false);
                    }
                    string botName = "";
                    if (target.botInfo)
                    {
                        botName = target.botInfo.name;
                    }
                    else
                    {
                        botName = "Cogmind";
                    }

                    UIManager.inst.CreateNewCalcMessage("Damage itensified against " + botName + " [" + damage + "].", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);

                    break;
                case CritType.Phase: // L-Cannon, Drained L-Cannon, Zio. Alpha-Cannon & Zio. Alpha-Cannon MK.2 has this
                    // "Damage phase-mirrored to [name] %1."
                    // (Assumption): Mirror damage to neighbor
                    Actor neighbor = Action.FindNewNeighboringEnemy(target);
                    DamageBot(neighbor, damage, protection, weapon, false);
                    string botName2 = "";
                    if (target.botInfo)
                    {
                        botName2 = target.botInfo.name;
                    }
                    else
                    {
                        botName2 = "Cogmind";
                    }

                    UIManager.inst.CreateNewCalcMessage("Damage phase-mirrored to " + botName2 + " [" + damage + "].", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    break;
                case CritType.Impale: // CR-A16's Behemoth Slayer & A bunch of other piercing weapons have this
                    // (Assumption): Insta-kill
                    // Destroy the core
                    if (target.botInfo)
                    {
                        UIManager.inst.CreateNewCalcMessage("Critical on " + target.botInfo.name + "'s core.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                        target.Die(target.name + "'s core has impaled by " + weapon.itemName + ", destroying it completely.");
                    }
                    else
                    {
                        PlayerData.inst.currentHealth = 0;
                    }
                    break;
            }
        }
        #endregion

        #region Salvage Modifier
        /* How much of a robot remains to salvage when it is destroyed depends on the value of its cumulative "salvage modifier" 
         * which reflects everything that happened to it before that point. This internal value is initially set to zero, 
         * and each projectile that impacts the robot will contribute its own weapon-based salvage modifier to the total. 
         * Some weapons lower the value (most notably ballistic cannons), others have no meaningful effect on it (most guns), 
         * while certain types may even raise it, ultimately increasing the likelihood of retrieving useful salvage.
         */

        if (target.botInfo)
        {
            int salvageMod = HF.GetSalvageMod(weapon.data);

            // Salvage Mod Items
            foreach (var item in items)
            {
                if (item.itemEffect[0].salvageBonus.hasEffect)
                {
                    if (item.itemEffect[0].salvageBonus.gunTypeOnly && weapon.type == ItemType.Gun)
                    {
                        if (weapon.projectileAmount == 1)
                        {
                            salvageMod += item.itemEffect[0].salvageBonus.bonus;
                        }
                    }
                    else if (!item.itemEffect[0].salvageBonus.gunTypeOnly)
                    {
                        salvageMod += item.itemEffect[0].salvageBonus.bonus;
                    }
                }
            }

            target.salvageModifier += salvageMod;
        }

        #endregion

        #region EM Damage/Corruption
        /*  Electromagnetic
            -----------------
            Electromagnetic (EM) weapons have less of an impact on integrity, but are capable of corrupting a target's computer systems. 
            Anywhere from 50 to 150% of damage done is also applied as system corruption. (Cogmind is less susceptible to EM-caused corruption, 
            but still has a damage% chance to suffer 1 point of system corruption per hit.) EM-based explosions only deal half damage to 
            inactive items lying on the ground, but can also corrupt them.
         */
        if (damageType == ItemDamageType.EMP)
        {
            if (target.botInfo) // Bot
            {
                float amount = Random.Range(0.5f, 1.5f);
                if (corruptCrit)
                {
                    amount = 1.5f; // Maximized due to crit
                }

                int corruption = Mathf.RoundToInt(damage * amount);
                target.corruption += (corruption / 100);
            }
            else // Player
            {
                float amount = Random.Range(0.5f, 1.5f);
                if (corruptCrit)
                {
                    amount = 1.5f; // Maximized due to crit
                }

                int corruption = Mathf.RoundToInt(damage * amount);
                float chance = corruption / 100;

                if(Random.Range(0f, 1f) < chance)
                {
                    PlayerData.inst.currentCorruption += 1;
                }
            }
        }
        #endregion

        #region Heat Transfer
        // Heat transfer
        Action.DealHeatTransfer(target.GetComponent<Actor>(), weapon.data, new Vector2Int(damageA, damageB), burnCrit);
        #endregion

        // If the item has survived, return it
        if(hitItem.Id >= 0 && hitItem.integrityCurrent > 0)
        {
            return hitItem;
        }
        else
        {
            return null;
        }
    }

    public static (int, bool) DamageItem(Actor target, Item item, int damage, bool crit = false)
    {
        int overflow = 0;
        bool destroyed = false;

        item.integrityCurrent -= damage;

        if (item.integrityCurrent <= 0) // Item destroyed
        {
            // If an item is destroyed we want to:
            // 1. Set the item's id to -1
            // 2. Remove the item from wherever its being stored (list)

            overflow = -1 * item.integrityCurrent;
            destroyed = true;

            Action.DestoyItem(target, item, crit);
        }
        else
        {
            if (!target.botInfo) // Player
            {
                // Update the UI and do a little animation
                // Play a sound?
            }
        }

        return (overflow, destroyed);
    }

    public static void DestoyItem(Actor owner, Item item, bool crit = false)
    {
        if (owner.botInfo) // Bot
        {
            foreach (var I in owner.botInfo.components.ToList())
            {
                if (I._item.Id >= 0 && I._item == item)
                {
                    owner.botInfo.components.Remove(I); // Remove the item
                    break;
                }
            }

            foreach (var I in owner.botInfo.armament.ToList())
            {
                if (I._item.Id >= 0 && I._item == item)
                {
                    owner.botInfo.components.Remove(I); // Remove the item
                    break;
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot I in owner.GetComponent<PartInventory>()._invPower.Container.Items.ToList())
            {
                if (I.item.itemData.data.Id >= 0)
                {
                    if (I.item == item)
                    {
                        owner.GetComponent<PartInventory>()._invPower.RemoveItem(I.item);
                        break;
                    }
                }
            }

            foreach (InventorySlot I in owner.GetComponent<PartInventory>()._invPropulsion.Container.Items.ToList())
            {
                if (I.item.itemData.data.Id >= 0)
                {
                    if (I.item == item)
                    {
                        owner.GetComponent<PartInventory>()._invPropulsion.RemoveItem(I.item);
                        break;
                    }
                }
            }

            foreach (InventorySlot I in owner.GetComponent<PartInventory>()._invUtility.Container.Items.ToList())
            {
                if (I.item.itemData.data.Id >= 0)
                {
                    if (I.item == item)
                    {
                        owner.GetComponent<PartInventory>()._invUtility.RemoveItem(I.item);
                        break;
                    }
                }
            }

            foreach (InventorySlot I in owner.GetComponent<PartInventory>()._invWeapon.Container.Items.ToList())
            {
                if (I.item.itemData.data.Id >= 0)
                {
                    if (I.item == item)
                    {
                        owner.GetComponent<PartInventory>()._invWeapon.RemoveItem(I.item);
                        break;
                    }
                }
            }

            // TODO:
            // Play a little destruction animation in the UI
            // Play a UI sound

            if(crit)
                UIManager.inst.CreateNewLogMessage(item.Name + " has been completely destroyed.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);

            // "Cogmind automatically recycles 5 matter from each attached part that is destroyed."
            PlayerData.inst.currentMatter += 5;
            if (PlayerData.inst.currentMatter > PlayerData.inst.maxMatter)
            {
                PlayerData.inst.currentMatter = PlayerData.inst.maxMatter;
            }

        }

        item.Id = -1;
    }

    public static float GatherCritBonuses(Actor actor)
    {
        float bonus = 0f;
        List<Item> items = Action.CollectAllBotItems(actor);

        bool first = true;
        bool stacks = true;
        bool half_stacks = false;
        foreach (var item in items)
        {
            if (item.itemData.itemEffect[0].toHitBuffs.hasEffect)
            {
                if (item.itemData.itemEffect[0].toHitBuffs.bonusCritChance)
                {
                    float amount = item.itemData.itemEffect[0].toHitBuffs.amount;

                    if (!first && stacks)
                    {
                        if (half_stacks)
                        {
                            amount /= 2;
                        }

                        bonus += amount;
                    }

                    if (first)
                    {
                        first = false;
                    }

                    stacks = item.itemData.itemEffect[0].toHitBuffs.stacks;
                    half_stacks = item.itemData.itemEffect[0].toHitBuffs.halfStacks;
                }
            }
        }

        return bonus;
    }

    public static int AddFlatDamageBonuses(int initialDamage, Actor attacker, Item weapon)
    {
        int damage = initialDamage;

        if (attacker.botInfo) // Bot
        {
            foreach (var item in attacker.botInfo.components)
            {
                if (item._item.Id >= 0 && item._item.itemData.itemEffect[0].flatDamageBonus.hasEffect) // Does it have the effect?
                {
                    if (item._item.itemData.itemEffect[0].flatDamageBonus.types.Contains(HF.GetDamageType(weapon.itemData))) // Same type?
                    {
                        damage = Mathf.RoundToInt(damage + (damage * item._item.itemData.itemEffect[0].flatDamageBonus.damageBonus));
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in attacker.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0 && item.item.itemData.itemEffect[0].flatDamageBonus.hasEffect) // Does it have the effect?
                {
                    if (item.item.itemData.itemEffect[0].flatDamageBonus.types.Contains(HF.GetDamageType(weapon.itemData))) // Same type?
                    {
                        damage = Mathf.RoundToInt(damage + (damage * item.item.itemData.itemEffect[0].flatDamageBonus.damageBonus));
                    }
                }
            }
        }

        return damage;
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

    public static BotMoveType DetermineBotMoveType(Actor actor)
    {
        BotMoveType result = BotMoveType.Running;

        if(HasHover(actor))
        {
            result = BotMoveType.Hovering;
        }
        else if (HasFlight(actor))
        {
            result = BotMoveType.Flying;
        }
        else if (HasWheels(actor))
        {
            result = BotMoveType.Rolling;
        }
        else if (HasLegs(actor))
        {
            result = BotMoveType.Walking;
        }
        else if (HasTreads(actor))
        {
            result = BotMoveType.Treading;
        }

        return result;
    }

    /// <summary>
    /// Calculates the Evasion/Avoidance level of a specific bot.
    /// </summary>
    /// <param name="actor">The bot to focus on.</param>
    /// <returns>Returns 1. The avoidance value as a float that is a WHOLE NUMBER (ex. 20) not a percent. 2. A list of all 5 avoidance values in int form.</returns>
    public static (float, List<int>) CalculateAvoidance(Actor actor)
    {
        List<int> individualValues = new List<int>();

        // The default evasion rate is 40%
        float avoidance = 40f;
        /*
         * The 5 factors contributing to evasion are:
         *  -Flight/Hover bonus (unless held in stasis and not overweight)
         *  -Heat level
         *  -Movement speed (and whether recently moved)
         *  -Evasion modifiers from utilities (e.g. Maneuvering Thrusters)
         *  -Cloaking modifiers from utilities (e.g. Cloaking Devices)
         */

        int evasionBonus1 = 0;
        int evasionBonus2 = 0; // negative
        int evasionBonus3 = 0;
        int evasionBonus4 = 0;
        int evasionBonus5 = 0;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            // -- Flight/Hover bonus -- //

            if (actor.inStatis || (actor.weightCurrent > actor.weightMax)) // checks for statis & overweight
            {
                evasionBonus1 = 0;
            }
            else 
            {
                foreach (BotArmament item in actor.botInfo.components)
                {
                    if (item._item.itemData.data.Id >= 0)
                    {
                        // I still have no idea how this is actually calculated, this is just a guess
                        if (item._item.itemData.type == ItemType.Flight)
                        {
                            evasionBonus1 += (int)(1.5f * item._item.itemData.slotsRequired);
                        }
                        else if (item._item.itemData.type == ItemType.Hover)
                        {
                            evasionBonus1 += (int)(1 * item._item.itemData.slotsRequired);
                        }
                    }
                }
            }

            // -- Heat Level -- //
            // This appears to add a -1 bonus for every 30 or so heat.
            int heatCalc = 0;
            if (actor.currentHeat > 29)
            {
                heatCalc = (int)(actor.currentHeat / 30);
            }
            evasionBonus2 = -heatCalc;

            // -- Movement Speed -- //
            int speed = actor.speed;
            /*
            if (speed <= 25) // FASTx3
            {
                evasionBonus3 = (int)(speed / 2);
            }
            else if (speed > 25 && speed <= 50) // FASTx2
            {
                evasionBonus3 = (int)(speed / 3);
            }
            else if (speed > 50 && speed <= 75) // FAST
            {
                evasionBonus3 = (int)(speed / 15);
            }*/
            if (speed <= 75) // Use a special formula (bonus goes from 0 to 20)
            {
                evasionBonus3 = (int)(1 + ((75f - speed) / 3.75f));
            }
            else // Not fast enough so no bonus
            {
                evasionBonus3 = 0;
            }

            // -- Evasion modifiers -- //
            // These effects can be found in 1. Legs 2. Variants of the reaction control system device
            bool rcsStack = true;
            BotMoveType myMoveType = DetermineBotMoveType(actor);
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    if(actor.momentum > 0)
                    {
                        if (item._item.itemData.type == ItemType.Legs)
                        {
                            foreach (ItemEffect effect in item._item.itemData.itemEffect)
                            {
                                if (effect.hasLegEffect)
                                {
                                    float potentialLegBonus = 0;
                                    potentialLegBonus += effect.evasionNaccuracyChange;
                                    if(actor.momentum > 3)
                                    {
                                        evasionBonus4 += (int)(1 * 3);
                                    }
                                    else if(actor.momentum > 0 && actor.momentum <= 3)
                                    {
                                        evasionBonus4 += (int)(1 * actor.momentum);
                                    }
                                }
                            }
                        }
                    }

                    if(item._item.itemData.type == ItemType.Device && actor.weightCurrent <= actor.weightMax && rcsStack)
                    {
                        foreach (ItemEffect effect in item._item.itemData.itemEffect)
                        {
                            if (effect.rcsEffect.hasEffect 
                                && (myMoveType == BotMoveType.Walking || myMoveType == BotMoveType.Flying || myMoveType == BotMoveType.Hovering))
                            {
                                if (myMoveType == BotMoveType.Walking)
                                {
                                    evasionBonus4 += (int)(effect.rcsEffect.percentage * 100);
                                }
                                else // Doubled
                                {
                                    evasionBonus4 += (int)(effect.rcsEffect.percentage * 2 * 100);
                                }

                                effect.rcsEffect.stacks = rcsStack;
                            }
                        }
                    }
                }
            }

            // -- Phasing / Cloaking modifiers -- //
            bool phasingStack = true;
            foreach (BotArmament item in actor.botInfo.components)
            {
                if(item._item.itemData.data.Id > 0 && item._item.itemData.type == ItemType.Device)
                {
                    foreach (ItemEffect effect in item._item.itemData.itemEffect)
                    {
                        if (effect.phasingEffect.hasEffect && phasingStack)
                        {
                            evasionBonus5 = (int)(effect.phasingEffect.percentage * 100);

                            phasingStack = effect.phasingEffect.stacks;
                        }
                    }
                }
            }


            // -- ADD THEM ALL UP -- //

            // -- 1
            avoidance += evasionBonus1;
            // -- 2
            avoidance += evasionBonus2; // This is negative, remember
            // -- 3
            avoidance += evasionBonus3;
            // -- 4
            avoidance += evasionBonus4;
            // -- 5
            avoidance += evasionBonus5;

        }
        else // Player
        {
            // We are also going to update the player's individual evasion1-5 variables as we go, because UIManager needs them.

            // -- Flight/Hover bonus -- //

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Flight && item.item.state)
                    {
                        evasionBonus1 += (int)(1.5f * item.item.itemData.slotsRequired);
                    }
                    else if (item.item.itemData.type == ItemType.Hover && item.item.state)
                    {
                        evasionBonus1 += (int)(1f * item.item.itemData.slotsRequired);
                    }
                }

            }

            evasionBonus1 = 0;

            // -- Heat Level -- //
            // This appears to add a -1 bonus for every 30 or so heat.
            int heatCalc = 0;
            if (PlayerData.inst.currentHeat > 29)
            {
                heatCalc = (int)(PlayerData.inst.currentHeat / 30);
            }
            evasionBonus2 = -heatCalc;

            // -- Movement Speed -- //
            int speed = PlayerData.inst.moveSpeed1;
            /*
            if (speed <= 25) // FASTx3
            {
                evasionBonus3 = (int)(speed / 2);
            }
            else if (speed > 25 && speed <= 50) // FASTx2
            {
                evasionBonus3 = (int)(speed / 3);
            }
            else if (speed > 50 && speed <= 75) // FAST
            {
                evasionBonus3 = (int)(speed / 15);
            }*/
            if(speed <= 75) // Use a special formula (bonus goes from 0 to 20)
            {
                evasionBonus3 = (int)(1 + ((75f - speed) / 3.75f));
            }
            else // Not fast enough so no bonus
            {
                evasionBonus3 = 0;
            }

            // -- Evasion modifiers -- //
            // These effects can be found in 1. Legs 2. Variants of the reaction control system device
            bool rcsStack = true;
            BotMoveType myMoveType = DetermineBotMoveType(actor);
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (actor.momentum > 0)
                    {
                        if (item.item.itemData.type == ItemType.Legs)
                        {
                            foreach (ItemEffect effect in item.item.itemData.itemEffect)
                            {
                                if (effect.hasLegEffect)
                                {
                                    float potentialLegBonus = 0;
                                    potentialLegBonus += effect.evasionNaccuracyChange;
                                    if (actor.momentum > 3)
                                    {
                                        evasionBonus4 += (int)(1 * 3);
                                    }
                                    else if (actor.momentum > 0 && actor.momentum <= 3)
                                    {
                                        evasionBonus4 += (int)(1 * actor.momentum);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id >= 0)
                {
                    if (item.item.itemData.type == ItemType.Device && actor.weightCurrent <= actor.weightMax && rcsStack)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffect)
                        {
                            if (effect.rcsEffect.hasEffect
                                && (myMoveType == BotMoveType.Walking || myMoveType == BotMoveType.Flying || myMoveType == BotMoveType.Hovering))
                            {
                                if (myMoveType == BotMoveType.Walking)
                                {
                                    evasionBonus4 += (int)(effect.rcsEffect.percentage * 100);
                                }
                                else // Doubled
                                {
                                    evasionBonus4 += (int)(effect.rcsEffect.percentage * 2 * 100);
                                }

                                effect.rcsEffect.stacks = rcsStack;
                            }
                        }
                    }
                }
            }

            // -- Phasing / Cloaking modifiers -- //
            bool phasingStack = true;
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if(item.item.Id > 0)
                {
                    if (item.item.itemData.type == ItemType.Device)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffect)
                        {
                            if (effect.phasingEffect.hasEffect && phasingStack)
                            {
                                evasionBonus5 = (int)(effect.phasingEffect.percentage * 100);

                                phasingStack = effect.phasingEffect.stacks;
                            }
                        }
                    }
                }
            }

            // -- ADD THEM ALL UP -- //

            // -- 1
            if (actor.inStatis || (actor.weightCurrent > actor.weightMax)) // checks for statis & overweight
            {
                // Unlike for bot calculations, we will return the bonus here, but just not add it in (For UI display purposes)
            }
            else
            {
                avoidance += evasionBonus1;
            }
            // -- 2
            avoidance += evasionBonus2; // This is negative, remember
            // -- 3
            avoidance += evasionBonus3;
            // -- 4
            avoidance += evasionBonus4;
            // -- 5
            avoidance += evasionBonus5;
        }

        individualValues.Add(evasionBonus1);
        individualValues.Add(evasionBonus2);
        individualValues.Add(evasionBonus3);
        individualValues.Add(evasionBonus4);
        individualValues.Add(evasionBonus5);

        return (avoidance, individualValues);
    }

    /// <summary>
    /// Calculates the two cloak bonuses the bot may have from cloaking items.
    /// </summary>
    /// <param name="actor">The bot in question.</param>
    /// <returns>The range reducion (as int) and spot chance reduction (as -float).</returns>
    public static (int, float) CalculateCloakEffectBonus(Actor actor)
    {
        int effects = 0; // Since some effects half stack, we need to track this.

        int rangeReduction = 0;
        float spotReduction = 0f;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Not the player
        {
            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0 && item._item.itemData.data.state)
                {
                    foreach (ItemEffect effect in item._item.itemData.data.itemData.itemEffect)
                    {
                        if (effect.cloakingEffect.hasEffect)
                        {
                            if(effects > 0 && effect.cloakingEffect.halfStacks)
                            {
                                rangeReduction += (effect.cloakingEffect.rangedReduction / 2);
                                spotReduction += (effect.cloakingEffect.noticeReduction / 2);
                            }
                            else
                            {
                                rangeReduction += effect.cloakingEffect.rangedReduction;
                                spotReduction += effect.cloakingEffect.noticeReduction;
                            }

                            effects++;
                        }
                    }
                }
            }
        }
        else // The player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.Id > 0 && item.item.state)
                {
                    if (item.item.itemData.type == ItemType.Device)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffect)
                        {
                            if (effect.cloakingEffect.hasEffect)
                            {
                                if (effects > 0 && effect.cloakingEffect.halfStacks)
                                {
                                    rangeReduction += (effect.cloakingEffect.rangedReduction / 2);
                                    spotReduction += (effect.cloakingEffect.noticeReduction / 2);
                                }
                                else
                                {
                                    rangeReduction += effect.cloakingEffect.rangedReduction;
                                    spotReduction += effect.cloakingEffect.noticeReduction;
                                }

                                effects++;
                            }
                        }
                    }
                }
            }
        }

        return (rangeReduction, spotReduction);
    }

    /// <summary>
    /// Will attempt the specified bot, and takes into account things like cloaking effects + evasion.
    /// </summary>
    /// <param name="spotter">The actor trying to spot.</param>
    /// <param name="target">The actor being looked for.</param>
    /// <returns>If the target gets spotted.</returns>
    public static bool TrySpotActor(Actor spotter, Actor target)
    {
        int spotRangeMod = 0;
        float spotChanceNoTurnMod = 0f;

        float defaultSpotChance = 0.5f;
        List<int> filler = new List<int>();
        (defaultSpotChance, filler) = Action.CalculateAvoidance(target); // We are just going to steal this.
        defaultSpotChance = 1f - ((float)defaultSpotChance / 100); // Convert to float and invert it (ex: 80% chance to avoid -> 20% chance to spot)

        (spotRangeMod, spotChanceNoTurnMod) = Action.CalculateCloakEffectBonus(target);
        float random = Random.Range(0f, 1f); // The spot RNG roll, used later.

        bool playerInFOV = HF.ActorInBotFOV(spotter, target);

        if (playerInFOV) // 1. Is the player within this bot's FOV?
        {
            // Yes? Move to the next check.
            if(spotRangeMod > 0) // The player has a spot bonus (and probably a non-turn bonus too)
            {
                // 2. Is the player too far away to be spotted based on the range bonus reduction?
                if(Vector2.Distance(HF.V3_to_V2I(spotter.transform.position), HF.V3_to_V2I(target.transform.position)) > (target.fieldOfViewRange - spotRangeMod))
                {
                    // Yes? Failure.
                    return false;
                }
                else
                {
                    // No? Continue.

                    // 3. If it's not the bot's turn, apply the other bonus.
                    if (!spotter.GetComponent<BotAI>().isTurn)
                    {
                        defaultSpotChance += spotChanceNoTurnMod;

                        // Now do the final roll.
                        defaultSpotChance = Mathf.Clamp(defaultSpotChance, GlobalSettings.inst.minSpotChance, 1f); // There should always be a small chance of being spotted.
                        if (random < defaultSpotChance)
                        {
                            // Spotted!
                            return true;
                        }
                        else
                        {
                            // Not spotted.
                            return false;
                        }
                    }
                    else
                    {
                        // No bonus? Do the final roll.
                        defaultSpotChance = Mathf.Clamp(defaultSpotChance, GlobalSettings.inst.minSpotChance, 1f); // There should always be a small chance of being spotted.
                        if (random < defaultSpotChance)
                        {
                            // Spotted!
                            return true;
                        }
                        else
                        {
                            // Not spotted.
                            return false;
                        }
                    }
                }
            }
            else
            {
                // Final roll
                defaultSpotChance = Mathf.Clamp(defaultSpotChance, GlobalSettings.inst.minSpotChance, 1f); // There should always be a small chance of being spotted.
                if(random < defaultSpotChance)
                {
                    // Spotted!
                    return true;
                }
                else
                {
                    // Not spotted.
                    return false;
                }
            }
        }
        else
        {
            // No? Failure.
            return false;
        }

    }

    /// <summary>
    /// Determines if the specified weapon has the range to attack the given target from the specified positon.
    /// </summary>
    /// <param name="attacker">The position of the attacker.</param>
    /// <param name="target">The position of the target.</param>
    /// <param name="weapon">The weapon (Item) being used.</param>
    /// <returns>If the target is within range. [True/False]</returns>
    public static bool IsTargetWithinRange(Vector2Int attacker, Vector2Int target, Item weapon)
    {
        float weaponRange = 0f;
        if (weapon.itemData.meleeAttack.isMelee)
        {
            weaponRange = 2; // Melee weapons have a range of "2";
        }
        else
        {
            weaponRange = weapon.itemData.shot.shotRange;
        }

        return Vector2.Distance(attacker, target) <= weaponRange;
    }

    /// <summary>
    /// Searches for a new (directly) neighboring enemy of the source actor. Used for follow-up melee attacks.
    /// </summary>
    /// <param name="source">The actor attacking (which we check for neighbors around).</param>
    /// <returns>A new target actor if possible.</returns>
    public static Actor FindNewNeighboringEnemy(Actor source)
    {
        foreach (Entity E in GameManager.inst.entities)
        {
            if(Vector2.Distance(HF.V3_to_V2I(source.transform.position), HF.V3_to_V2I(E.transform.position)) < 1.55f) // Is a neighbor (could probably be done better).
            {
                if(HF.DetermineRelation(source, E.GetComponent<Actor>()) == BotRelation.Hostile) // Is an enemy.
                { // NOTE: This avoids comical situations where the player follows up into attacking a neutral bot which is probably a good thing.
                    return E.GetComponent<Actor>(); // We've found one.
                }
            }
        }

        return null;
    }

    public static void DoWeaponAttackCost(Actor source, Item weapon)
    {
        if (weapon.itemData.meleeAttack.isMelee)
        {
            if (source.GetComponent<PlayerData>())
            {
                PlayerData.inst.currentHeat += weapon.itemData.meleeAttack.heat;
                PlayerData.inst.currentMatter += weapon.itemData.meleeAttack.matter;
                PlayerData.inst.currentEnergy += weapon.itemData.meleeAttack.energy;
            }
            else
            {
                source.currentHeat += weapon.itemData.meleeAttack.heat;
                source.currentMatter += weapon.itemData.meleeAttack.matter;
                source.currentEnergy += weapon.itemData.meleeAttack.energy;
                // energy?
            }
        }
        else
        {
            if (source.GetComponent<PlayerData>())
            {
                PlayerData.inst.currentHeat += weapon.itemData.shot.shotHeat;
                PlayerData.inst.currentMatter += weapon.itemData.shot.shotMatter;
                PlayerData.inst.currentEnergy += weapon.itemData.shot.shotEnergy;

                if (weapon.isOverloaded) // Overloading
                {
                    // Double the energy cost
                    PlayerData.inst.currentEnergy += weapon.itemData.shot.shotEnergy;

                    // Triple the heat production
                    PlayerData.inst.currentHeat += weapon.itemData.shot.shotHeat;
                    PlayerData.inst.currentHeat += weapon.itemData.shot.shotHeat;
                }
            }
            else
            {
                source.currentHeat += weapon.itemData.shot.shotHeat;
                source.currentMatter += weapon.itemData.shot.shotMatter;
                source.currentEnergy += weapon.itemData.shot.shotEnergy;

                if (weapon.isOverloaded) // Overloading
                {
                    // Double the energy cost
                    source.currentEnergy += weapon.itemData.shot.shotEnergy;

                    // Triple the heat production
                    source.currentHeat += weapon.itemData.shot.shotHeat;
                    source.currentHeat += weapon.itemData.shot.shotHeat;
                }
            }
        }
    }

    public static void DealHeatTransfer(Actor victim, Item weapon, Vector2Int damage, bool burnCrit = false)
    {
        /*  Thermal weapons attempt to transfer ([damageForHeatTransfer] / [originalDamage])% of the maximum heat transfer rating, 
         *  and also check for possible robot meltdown (the effect of which might be delayed until the robot's next turn).
         */

        if (victim == null) // Bots only
        {
            return;
        }

        float percent = damage.x / damage.y;

        // Heat transfer goes from 0 --> 4 [None, Low, Medium, High, Massive]
        int level = 0;
        if (weapon.itemData.itemEffect[0].heatTransfer != 0)
        {
            level = weapon.itemData.itemEffect[0].heatTransfer;
        }
        else
        {
            return;
        }

        if(weapon.isOverloaded && level < 4) // Overloaded bonus
        {
            level++;
        }

        int heat = 0;
        if (weapon.itemData.meleeAttack.isMelee)
        {
            heat = weapon.itemData.meleeAttack.heat;
        }
        else
        {
            heat = weapon.itemData.shot.shotHeat;
        }

        // Burn Crit effect
        if (burnCrit)
        {
            if(level < 4)
            {
                level = 4;
            }
        }

        if(level == 1)
        {
            heat = Mathf.RoundToInt(heat * 0.25f); // 25%
        }
        else if(level == 2)
        {
            heat = Mathf.RoundToInt(heat * 0.5f); // 50%
        }
        else if(level == 3)
        {
            heat = Mathf.RoundToInt(heat * 0.75f); // 75%
        }
        else if(level == 4)
        {
            heat = Mathf.RoundToInt(heat * 0.80f); // 80%
        }

        // Modify by initial percentage
        heat = Mathf.RoundToInt((float)(percent * heat));

        if (burnCrit)
        {
            heat *= 2;
        }

        // Deal the heat
        if (victim.botInfo) // Bot
        {
            victim.currentHeat += heat;
        }
        else // Player
        {
            PlayerData.inst.currentHeat += heat;
        }
    }

    public static void OverloadedConsequences(Actor actor, Item weapon)
    {
        #region Explanation
        /*  Each such weapon also has a stability rating that determines how likely it is to malfunction if fired while overloaded. 
         *  The chance is generally not high, but if it does happen the weapon could drain your energy reserves, 
         *  cause a massive surge of heat, or short circuit other parts.
         */

        #endregion

        // TODO
        if (!weapon.itemData.meleeAttack.isMelee && weapon.itemData.shot.hasStability)
        {
            float stability = weapon.itemData.shot.shotStability;

            if(Random.Range(0f, 1f) > stability)
            { // Something happens!
                string message = "";

                float random = Random.Range(0f, 1f);

                if(random >= 0.8f) // Short Circuit
                {
                    Item targetItem = null;


                    message = weapon.itemData.itemName + " has caused a short circuit due to being overloaded, damaging " + targetItem.itemData.itemName;
                }
                else if (random < 0.8f && random >= 0.4f) // Heat Surge
                {


                    message = weapon.itemData.itemName + " has caused a massive surge in heat due to being overloaded.";
                }
                else // Energy Drain
                {


                    message = weapon.itemData.itemName + " has caused a massive drain in energy due to being overloaded.";
                }


                UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
            }
        }
        else
        {
            return;
        }
    }

    public static bool HasResourcesToAttack(Actor attacker, Item weapon)
    {
        int matter = 0;
        int energy = 0;

        int cost_matter = 0;
        int cost_energy = 0;

        if (attacker.botInfo) // Bot
        {
            matter = attacker.currentMatter;
            energy = attacker.currentEnergy;

            if (Action.IsMeleeWeapon(weapon))
            {
                cost_matter = weapon.itemData.meleeAttack.matter;
                cost_energy = weapon.itemData.meleeAttack.energy;
            }
            else
            {
                cost_matter = weapon.itemData.shot.shotMatter;
                cost_energy = weapon.itemData.shot.shotEnergy;
            }
        }
        else // Player
        {
            matter = PlayerData.inst.currentMatter;
            energy = PlayerData.inst.currentEnergy;

            if (Action.IsMeleeWeapon(weapon))
            {
                cost_matter = weapon.itemData.meleeAttack.matter;
                cost_energy = weapon.itemData.meleeAttack.energy;
            }
            else
            {
                cost_matter = weapon.itemData.shot.shotMatter;
                cost_energy = weapon.itemData.shot.shotEnergy;
            }
        }

        if (matter >= cost_matter && energy >= cost_energy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static List<Item> CollectAllBotItems(Actor actor)
    {
        List<Item> items = new List<Item>();

        // Collect up all the items
        if (actor.botInfo) // Bot
        {
            foreach (BotArmament item in actor.botInfo.armament)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    items.Add(item._item);
                }
            }

            foreach (BotArmament item in actor.botInfo.components)
            {
                if (item._item.itemData.data.Id >= 0)
                {
                    items.Add(item._item);
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPower.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invPropulsion.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invUtility.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>()._invWeapon.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    items.Add(item.item);
                }
            }
        }

        return items;
    }

    public static Item FindRandomItemOfSlot(Actor actor, ItemSlot slot)
    {
        List<Item> items = Action.CollectAllBotItems(actor);

        List<Item> found = new List<Item>();

        foreach (var item in items)
        {
            if(item.Id >= 0 && item.itemData.slot == slot)
            {
                found.Add(item);
            }
        }

        return found[Random.Range(0, found.Count - 1)];
    }

    #endregion
}
