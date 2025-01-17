// Origionally made by: Chizaruu @ https://github.com/Chizaruu/Unity-RL-Tutorial/blob/part-4-field-of-view/Assets/Scripts/Entity/Entity.cs
// Expanded & Modified by: Cody Jackson @ cody@krselectric.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;
using Transform = UnityEngine.Transform;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;
using static UnityEditor.Progress;

public static class Action
{
    public static void EscapeAction()
    {
        Debug.Log("Quit");
        //Application.Quit();
    }

    #region Attack Resolution Explainer
    // from the manual: https://www.gridsagegames.com/cogmind/manual.txt
    /* Attack Resolution
        -------------------
        With so many mechanics playing into an attack, especially later in the game when there are numerous active parts involved at once, 
        min-maxers looking to optimize their build may want a clearer understanding of the order in which all elements of an attack are carried out. 
        This manual already covered hit chances above, but there are many more steps to what happens once an attack hits a robot. 
        Below is an ordered list detailing that entire process, which applies to both ranged and melee combat. 
        Note that you most likely DO NOT need to know this stuff, but it may help answer a few specific questions min-maxers have about prioritization.

        1. Check if the attack is a non-damaging special case such as Datajacks, Stasis Beams, Tearclaws, etc., and handle that before quitting early.

        2. Calculate base damage, a random value selected from the weapon's damage range and multiplied by applicable modifiers for overloading, 
        momentum, and melee sneak attacks, in that order. Potential damage range modified by Melee Analysis Suites, Kinecellerators, and Force Boosters here.

        3. Apply robot analysis damage modifier, if applicable (+10%).

        4. Apply link_complan hack damage modifier, if applicable (+25%).

        5. Apply Particle Charger damage modifier, if applicable.

        6. Reduce damage by resistances.

        7. Apply salvage modifiers from the weapon and any Salvage Targeting Computers.

        8. Determine whether the attack caused a critical hit.

        9. Split damage into a random number of chunks if an explosion, usually 1~3. The process from here is repeated for each chunk.

        10. Store the current damage value as [originalDamage] for later.

        11. Apply the first and only first defense applicable from the following list: phase wall, 75% personal shield (VFP etc), 
        Force Field, Shield Generator, stasis bubble, active Stasis Trap, Remote Shield, 50% remote shield (Energy Mantle etc.), Hardlight Generator.

        12. Store the current damage value as [damageForHeatTransfer] for later.

        13. Choose target part (or core) based on coverage, where an Armor Integrity Analyzer first applies its chance to bypass all armor, 
        if applicable, then available Core Analyzers increase core exposure, before finally testing individual target chances normally.

        14. Cancel critical strike intent if that target has applicable part shielding, or if not applicable to target.

        15. If targeting core, apply damage and, if robot survives, check for core disruption if applicable.

        16. If targeting a power source with an EM weapon, check for a chain reaction due to spectrum.

        17. If not a core hit or chain reaction, prepare to apply damage to target part. If the part is Phase Armor or have an active Phase Redirector, 
        first reduce the damage by their effect(s) and store the amount of reduction as [transferredCoreDamage] for later, 
        then if part is Powered Armor reduce damage in exchange for energy (if available), or if part is treads currently in siege mode reduce damage appropriately. 
        If part not destroyed then also check for EM disruption if applicable.

        18. If part destroyed by damage, record any excess damage as overflow. If outright destroyed by a critical hit but damage exceeded 
        part's integrity anyway, any excess damage is still recorded as overflow.

        19. If part was not armor and the attack was via cannon, launcher, or melee, transfer remaining damage to another random target 
        (forcing to a random armor if any exists). Continue transferring through additional parts if destroyed (and didn't hit armor).

        20. If part not destroyed, check whether heat transfer melts it instead.

        21. Apply [transferredCoreDamage] directly to the core if applicable, with no further modifiers.

        22. Apply damage type side effects:

        * Thermal weapons attempt to transfer ([damageForHeatTransfer] / [originalDamage])% of the maximum heat transfer rating, 
          and also check for possible robot meltdown (the effect of which might be delayed until the robot's next turn).

        * Kinetic damage may cause knockback.

        * The amount of EM corruption is based on [originalDamage].

        * Impact damage may cause knockback, and applies corruption for each part destroyed.
     */

    #endregion

    #region General Actions

    /// <summary>
    /// Attempts to move an actor in a specified direction, if there is a bot in the way, it will attempt to ram them instead.
    /// </summary>
    /// <param name="actor">The actor that needs to move.</param>
    /// <param name="direction">The direction the actor is moving in.</param>
    /// <returns>Returns true if the bot can (and will) move there. False if it can't (and won't).</returns>
    public static bool BumpAction(Actor actor, Vector2 direction)
    {
        Actor target = GameManager.inst.GetBlockingActorAtLocation(actor.transform.position + (Vector3)direction);

        if (target)
        {
            // Check for quest interaction
            QuestPoint quest = HF.ActorHasQuestPoint(target);
            if (quest != null && quest.CanInteract())
            {
                // Interact with the quest and bail out early
                quest.Interact();
                return false;
            }

            actor.ConfirmCollision(target);
            if (actor.confirmCollision)
            {
                // Play collision sound
                AudioManager.inst.PlayGlobalCombatSound(AudioManager.inst.dict_game["CAVEHIT_02"], 0.7f); // GAME - CAVEHIT_02
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
                        Action.ModifyPlayerCore(-(int)(damage / 2));
                    }
                    else
                    {
                        Action.ModifyPlayerCore(-(int)damage);
                    }

                    UIManager.inst.CreateNewLogMessage("Slammed into " + target.GetComponent<Actor>().botInfo.botName + ".", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false);
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
                        UIManager.inst.CreateNewLogMessage($"Sneak attack on {source.uniqueName}", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
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
                        DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);

                        // Do a calc message
                        string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHit * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                        message = $"Recieved damage: {damageAmount}";
                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                    }
                    else // Bot being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);

                        // Show a popup that says how much damage occured
                        if (!target.GetComponent<PlayerData>())
                        {
                            UI_CombatPopup(target.GetComponent<Actor>(), damageAmount.ToString());
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
                                UIManager.inst.CreateNewLogMessage($"Sneak attack on {source.uniqueName}", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
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
                                DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);

                                // Do a calc message
                                string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHit * 100}%) Hit";

                                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                                message = $"Recieved damage: {damageAmount}";
                                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                            }
                            else // Bot being attacked
                            {
                                DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);


                                // Show a popup that says how much damage occured
                                if (!target.GetComponent<PlayerData>())
                                {
                                    UI_CombatPopup(target.GetComponent<Actor>(), damageAmount.ToString());
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
                UIManager.inst.CreateNewLogMessage($"Sneak attack on {source.uniqueName}", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
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

                // Message
                string message = $" Damage insufficient to overcome {target.name} armor";

                UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.inactiveGray, UIManager.inst.dullGreen, false, true);
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
                        DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);

                        // Do a calc message
                        string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHitChance * 100}%) Hit";

                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                        message = $"Recieved damage: {damageAmount}";
                        UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                    }
                    else // Bot being attacked
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount, weapon, source, crit);


                        // Show a popup that says how much damage occured
                        if (!target.GetComponent<PlayerData>())
                        {
                            UI_CombatPopup(target.GetComponent<Actor>(), damageAmount.ToString());
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
                        string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHitChance * 100}%) Miss";

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

                    // Message
                    string message = $" Damage insufficient to overcome {target.name} armor";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.inactiveGray, UIManager.inst.dullGreen, false, true);
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

        // Unstable consequences
        if(weapon.unstable > -1)
        {
            weapon.unstable--;
            if (weapon.unstable == 0)
            {
                Action.UnstableWeaponConsequences(source, weapon);
            }
        }

        source.momentum = 0;
        TurnManager.inst.EndTurn(source); // Alter this later
    }

    public static void MovementAction(Actor actor, Vector2 direction)
    {
        actor.noMovementFor = 0;
        actor.Move(direction); // Actually move the actor
        actor.UpdateFieldOfView(); // Update their FOV

        // Incurr costs for moving
        float tomove_energy = 0, tomove_heat = 0;
        foreach (var I in Action.CollectAllBotItems(actor))
        {
            if (Action.IsItemActionable(I) && I.itemData.slot == ItemSlot.Propulsion)
            {
                tomove_energy = I.itemData.propulsion[0].propEnergy;
                tomove_heat = I.itemData.propulsion[0].propHeat;
            }
        }

        if (actor.GetComponent<PlayerData>())
        {
            PlayerData.inst.currentHeat += (int)tomove_heat;
            ModifyPlayerEnergy((int)tomove_energy);
        }
        else
        {
            actor.currentHeat += (int)tomove_heat;
            actor.currentEnergy += (int)tomove_energy;
        }

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

        // Update any nearby doors
        GameManager.inst.LocalDoorUpdate(new Vector2Int((int)actor.transform.position.x, (int)actor.transform.position.y));
        actor.UpdateFieldOfView();
    }

    /// <summary>
    /// "Shunt" (push) the specified target on tile away from the source.
    /// </summary>
    /// <param name="source">The source to be pushed away from.</param>
    /// <param name="target">The actor being pushed.</param>
    public static void ShuntAction(Actor source, Actor target)
    {
        // Consider if the target is immune to knockback (siege mode and whatnot)
        List<Item> items = Action.CollectAllBotItems(target);
        foreach (var item in items)
        {
            if(item.Id > -1 && item.itemData.type == ItemType.Treads)
            {
                foreach(var E in item.itemData.itemEffects)
                {
                    if(E.hasStabEffect && E.stab_KnockbackImmune)
                    {
                        if (target.botInfo) // Bot
                        {
                            UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to being knocked back.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                            return;
                        }
                        else // Player
                        {
                            UIManager.inst.CreateNewLogMessage(item.itemData.itemName + " prevented being knocked back.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                            return;
                        }
                    }
                }
            }
        }

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

        GameObject moveLocation = target.GetComponent<Actor>().FindBestFleeLocation(source.gameObject, target.gameObject, validMoveLocations);

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
    /// Batter up.
    /// </summary>
    /// <param name="target">The baseball.</param>
    /// <param name="direction">Where we are sending it.</param>
    public static void KnockbackAction(Actor target, Item weapon, Vector2Int direction, float og_knockback, int og_damage, int dealtDamage = 0)
    {
        // First off, (due to recursion) do we need to deal any extra damage?
        if(dealtDamage > 0)
        {
            Item ph = Action.DamageBot(target, dealtDamage, weapon);
            // Did we destroy that item (and was it originally an Impact attack?)
            if(ph == null && weapon.itemData.meleeAttack.isMelee && weapon.itemData.meleeAttack.damageType == ItemDamageType.Impact)
            {
                // Apply a bit of corruption!
                if (target.botInfo) // Bot
                {
                    target.corruption += (5 / 100);
                }
                else // Player
                {
                    Action.ModifyPlayerCorruption(1);
                }
            }
        }

        Vector2Int pos = HF.V3_to_V2I(target.transform.position) + direction;
        TileBlock firstAttemptTile = MapManager.inst._allTilesRealized[pos].bottom.GetComponent<TileBlock>();

        // Is the tile where we want to send the target unoccupied?
        if (target.GetComponent<Actor>().IsUnoccupiedTile(firstAttemptTile))
        {
            // Yes!
            MovementAction(target, direction); // Force them to move
            // Do a message
            if (target.botInfo)
            {
                UIManager.inst.CreateNewLogMessage(target.botInfo + " was knocked back.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
            }
            else
            {
                UIManager.inst.CreateNewLogMessage("Knocked back.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
            }
        }
        else
        {
            // No. Is there a bot there?
            Actor blocker = GameManager.inst.GetBlockingActorAtLocation(new Vector3(pos.x, pos.y));
            if(blocker != null)
            {
                // Yes! Try and knock them back instead.
                /*
                 * A robot hit by another displaced 
                 * robot has a chance to itself be displaced and sustain damage, where the chance equals the original knockback chance further 
                 * modified by +/-10% per size class difference between the blocking robot and the knocked back robot, and the resulting damage 
                 * equals [originalDamage] (see Attack Resolution), further divided by the blocker size class if that class is greater than 1 (where Medium = 2, and so on).
                 */
                float followUpChance = og_knockback;
                int followUpDamage = og_damage;

                BotSize size_a = BotSize.Medium;
                if (target.botInfo)
                {
                    size_a = target.botInfo._size;
                }
                BotSize size_b = BotSize.Medium;
                if (blocker.botInfo)
                {
                    size_b = target.botInfo._size;
                }
                float sizeMod = Action.CalculateSizeModifier(size_b, size_a);
                followUpChance = Mathf.RoundToInt(followUpChance + (float)(followUpChance * sizeMod));

                int divide = 0;

                if(size_b == BotSize.Medium)
                {
                    divide = 2;
                }
                else if(size_b == BotSize.Large)
                {
                    divide = 3;
                }
                else if(size_b == BotSize.Huge)
                {
                    divide = 4;
                }

                if(divide > 0)
                {
                    followUpDamage /= divide;
                }

                if (Random.Range(0f, 1f) < followUpChance)
                {
                    Action.KnockbackAction(blocker, weapon, direction, followUpChance, og_damage, followUpDamage);
                }
            }
            else
            {
                // No. Stop here, but is there a machine at the location?
                GameObject machine = HF.GetMachineParentAtPosition(pos);

                if(machine != null)
                {
                    // Is the machine static and does it explode?
                    if (machine.GetComponent<StaticMachine>())
                    {
                        if (machine.GetComponent<StaticMachine>().explosive)
                        {
                            // Do a message
                            if (target.botInfo)
                            {
                                UIManager.inst.CreateNewLogMessage(target.botInfo + " was knocked back into " + machine.GetComponent<StaticMachine>()._name + ", causing it to explode.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                            }
                            else
                            {
                                UIManager.inst.CreateNewLogMessage("Knocked back into " + machine.GetComponent<StaticMachine>()._name + ", causing it to explode.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                            }
                            machine.GetComponent<StaticMachine>().Detonate(); // Kaboom.
                        }
                    }

                    // If not, just destroy that part of the machine.
                    if (MapManager.inst._allTilesRealized[pos].top.GetComponent<MachinePart>())
                    {
                        MapManager.inst._allTilesRealized[pos].top.GetComponent<MachinePart>().DestroyMe();
                        // Do a message
                        if (target.botInfo)
                        {
                            UIManager.inst.CreateNewLogMessage(target.botInfo + " was knocked back into " + MapManager.inst._allTilesRealized[pos].top.GetComponent<MachinePart>().displayName + ", heavily damaging it.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                        }
                        else
                        {
                            UIManager.inst.CreateNewLogMessage("Knocked back into " + MapManager.inst._allTilesRealized[pos].top.GetComponent<MachinePart>().displayName + ", heavily damaging it.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                        }
                    }
                }
            }
        }
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
        int chunks = Random.Range(weapon.itemData.explosionDetails.chunks.x, weapon.itemData.explosionDetails.chunks.y);
        if(chunks == 0)
        {
            chunks = 1;
        }

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
            foreach (var item in source.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item) && stacks)
                {
                    foreach(var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0 && E.launcherBonus.hasEffect)
                        {
                            bonus_launcherAccuracy += E.launcherBonus.launcherAccuracy;
                            if (activeWeapons == 1)
                                bonus_launcherLoading += E.launcherBonus.launcherLoading;

                            if (E.launcherBonus.stacks)
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
        }
        else // Player
        {
            foreach (InventorySlot item in source.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item) && stacks)
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0 && E.launcherBonus.hasEffect)
                        {
                            bonus_launcherAccuracy += E.launcherBonus.launcherAccuracy;
                            if (activeWeapons == 1)
                                bonus_launcherLoading += E.launcherBonus.launcherLoading;

                            if (E.launcherBonus.stacks)
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
                    // Before we deal damage we need to split it into chunks
                    for(int i = 0; i < chunks; i++)
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount / chunks, weapon, source);
                    }

                    // Do a calc message
                    string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHitChance * 100}%) Hit";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);

                    message = $"Recieved damage: {damageAmount}";
                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                }
                else // Bot being attacked
                {
                    // Before we deal damage we need to split it into chunks
                    for (int i = 0; i < chunks; i++)
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount / chunks, weapon, source);
                    }

                    // Show a popup that says how much damage occured
                    if (!target.GetComponent<PlayerData>())
                    {
                        UI_CombatPopup(target.GetComponent<Actor>(), damageAmount.ToString());
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
                    string message = $"{source.botInfo.botName}: {weapon.itemData.name} ({toHitChance * 100}%) Miss";

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

    /// <summary>
    /// Performs an explosion (like an AOE attack) but with no source (since this is probably a mine or something).
    /// </summary>
    /// <param name="details">The item details of the explosion.</param>
    /// <param name="pos">The location in the world of where this explosion is happening.</param>
    public static void UnboundExplosion(Item details, Vector2Int pos)
    {
        ExplosionGeneric eDetails = details.itemData.explosionDetails;

        // < Most of this is copied from the AOE method >
        List<GameObject> targets = new List<GameObject>();

        int falloff = eDetails.fallOff;
        int radius = eDetails.radius;
        Vector2Int center = pos;
        List<Vector2Int> effectedTiles = new List<Vector2Int>();

        // == We are going to start in the center and then work outwards, dealing with any obstructions as we go. ==
        // The general idea that we will follow is:
        // - Bots will NEVER block explosion "waves", it goes straight through them (same with floor tiles, duh).
        // - Machines will initally block explosions unless they are destroyed by said explosion.
        // - Walls will block explosions unless they are destroyed by said explosion.
        // - Doors act similar to walls (unless they are open).


        // > Uniquely we will do the center tile alone incase the user shoots into a wall too strong for them to kill.
        bool centerAttack = Action.UnboundExplosiveAttack(HF.GetTargetAtPosition(center), details, 0);
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
                if (Vector2.Distance(point, center) <= radius && point != center)
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
                bool blocksExplosion = Action.UnboundExplosiveAttack(HF.GetTargetAtPosition(tilePosition), details, (distFromCenter * falloff));
                blockingTiles.Add(tilePosition, blocksExplosion);

                effectedTiles.Add(tilePosition);
            }
        }

        // > Visuals & Audio <
        #region Visuals & Audio
        // - Create the visuals on each effected tile. These vary greatly per weapon.
        // TODO: Explosion visuals
        CFXManager.inst.CreateExplosionFX(center, details.itemData, effectedTiles, center);
        // - Play the explosion sound where the attack lands.
        AudioManager.inst.CreateTempClip(new Vector3(center.x, center.y, 0f), HF.RandomClip(details.itemData.explosion.explosionSounds));
        #endregion

        TurnManager.inst.AllEntityVisUpdate();
    }

    #endregion

    #region HelperFunctions

    public static bool UnboundExplosiveAttack(GameObject target, Item weapon, int falloff)
    {
        // <Mostly copied from IndividualAOEAttack>

        bool permiable = true;
        int chunks = Random.Range(weapon.itemData.explosionDetails.chunks.x, weapon.itemData.explosionDetails.chunks.y);

        // There are a couple things we could be attacking here:
        // - Walls
        // - Bots
        // - Machines
        // - Doors
        // - The floor

        // Get the armor value if this is a structure
        int armor = HF.TryGetStructureArmor(target);
        ItemExplosion attackData = weapon.itemData.explosion;

        if (target.GetComponent<Actor>()) // We are attacking a bot
        {
            permiable = true; // Bots are always permiable

            float toHitChance = 0f;
            List<ArmorType> types = new List<ArmorType>();
            int projAmount = weapon.itemData.projectileAmount;

            // We are going to do a compacted method of hit chance here without a bunch of the usual stuff
            #region To-Hit Calculation
            // First factor in the target's evasion/avoidance rate
            float avoidance = 0f;
            List<int> unused = new List<int>();
            (avoidance, unused) = Action.CalculateAvoidance(target.GetComponent<Actor>());
            toHitChance -= (avoidance / 100);

            // - Siege Mode - //
            if (target.GetComponent<PlayerData>())
            {
                if (PlayerData.inst.timeTilSiege == 0)
                {
                    toHitChance += Random.Range(0.2f, 0.3f);
                }
            }
            else
            {
                if (target.GetComponent<Actor>().siegeMode)
                {
                    toHitChance += Random.Range(0.2f, 0.3f);
                }
            }

            // - No Movement past 2 turns - //
            if (target.GetComponent<Actor>().noMovementFor >= 2)
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
                if (target.GetComponent<Actor>().currentHeat > 0)
                {
                    toHitChance += 0.03f;
                }
            }

            // - Large/Huge target (doesn't apply to player) - //
            if (target.GetComponent<Actor>().botInfo)
            {
                if (target.GetComponent<Actor>().botInfo._size == BotSize.Large)
                {
                    toHitChance += 0.10f;
                }
                else if (target.GetComponent<Actor>().botInfo._size == BotSize.Huge)
                {
                    toHitChance += 0.30f;
                }
            }

            // - Last move + speed - //
            if (target.GetComponent<Actor>().noMovementFor > 0)
            {
                if (target.GetComponent<PlayerData>())
                {
                    toHitChance += Mathf.RoundToInt(Mathf.Clamp(Mathf.Log(100 * (100 / PlayerData.inst.moveSpeed1), 2) * -2f, -15f, -1f));
                }
                else
                {
                    toHitChance += Mathf.RoundToInt(Mathf.Clamp(Mathf.Log(100 * (100 / target.GetComponent<Actor>().botInfo._movement.moveSpeedPercent), 2) * -2f, -15f, -1f));
                }
            }

            // - Target has legs - //
            if (HasLegs(target.GetComponent<Actor>()))
            {
                toHitChance += -0.05f;
            }

            // - If attacker moved last action - //
            if (target.GetComponent<Actor>().noMovementFor > 0)
            {
                toHitChance += -0.10f;
            }

            // - If attacker is using legs - //
            if (HasLegs(target.GetComponent<Actor>()))
            {
                toHitChance += -0.05f;
            }

            // - Target is Small/Tiny - //
            if (!target.GetComponent<PlayerData>()) // Applies to bots only
            {
                if (target.GetComponent<Actor>().botInfo._size == BotSize.Tiny)
                {
                    toHitChance += -0.30f;
                }
                else if (target.GetComponent<Actor>().botInfo._size == BotSize.Small)
                {
                    toHitChance += -0.10f;
                }
            }
            #endregion

            #region Hit or Miss
            float rand = Random.Range(0f, 1f);
            if (rand < toHitChance) // Success, a hit!
            {

                #region Damage Calculation
                // Deal Damage to the target
                int damageAmount = (int)Random.Range(weapon.itemData.explosionDetails.damage.x, weapon.itemData.explosionDetails.damage.y);

                damageAmount += falloff; // Apply damage falloff if any (remember it's negative).
                if (damageAmount < 1)
                    damageAmount = 1; // Minimum 1 damage

                #endregion

                if (target.GetComponent<PlayerData>()) // Player being attacked
                {
                    // Before we deal damage we need to split it into chunks
                    for (int i = 0; i < chunks; i++)
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount / chunks, weapon);
                    }

                    // Do a calc message
                    string message = $"Recieved damage: {damageAmount}, due to being hit by explosion.";
                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange, UIManager.inst.warmYellow, false, true);
                }
                else // Bot being attacked
                {
                    // Before we deal damage we need to split it into chunks
                    for (int i = 0; i < chunks; i++)
                    {
                        DamageBot(target.GetComponent<Actor>(), damageAmount / chunks, weapon);
                    }

                    // Show a popup that says how much damage occured
                    if (!target.GetComponent<PlayerData>())
                    {
                        UI_CombatPopup(target.GetComponent<Actor>(), damageAmount.ToString());
                    }

                    // Do a calc message
                    string message = target.GetComponent<Actor>() + " was hit by explosion [" + damageAmount + "].";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                }
            }
            else
            {  // ---------------------------- // Failure, a miss.

                if (target.GetComponent<PlayerData>()) // Player being targeted
                {
                    // Do a calc message
                    string message = "Evaded the explosion.";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.corruptOrange_faded, UIManager.inst.corruptOrange, false, true);
                }
                else // AI being targeted
                {
                    // Do a calc message
                    string message = target.GetComponent<Actor>().botInfo.botName + " evaded the explosion.";

                    UIManager.inst.CreateNewCalcMessage(message, UIManager.inst.dullGreen, UIManager.inst.normalGreen, false, true);
                }
            }
            #endregion
        }
        else if (target.GetComponent<MachinePart>()) // We are attacking a machine
        {
            // For machines we can't miss, we just need to check if the damage we do beats the machine's armor.

            permiable = false; // Machines block explosions unless they are destroyed

            #region Damage Calculation
            // Calculate Damage
            int damageAmount = (int)Random.Range(weapon.itemData.explosionDetails.damage.x, weapon.itemData.explosionDetails.damage.y);

            damageAmount += falloff; // Apply damage falloff if any (remember it's negative).
            if (damageAmount < 1)
                damageAmount = 1; // Minimum 1 damage
            #endregion

            #region Beat the Armor?
            if (damageAmount > armor) // Success! Destroy the structure
            {
                target.GetComponent<MachinePart>().DestroyMe();

                // Do a calc message
                if (target.GetComponent<MachinePart>().parentPart == target.GetComponent<MachinePart>()) // Only if this is the parent part
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
         *  -Attacker accuracy reduced by [system corruption]/X% (X = 4 for Cogmind, 8 for others)
         *  -defender utility bonuses
         */

        ItemShot shotData = weapon.itemData.shot;
        ItemProjectile projData = weapon.itemData.projectile;
        int projAmount = weapon.itemData.projectileAmount;

        List<Item> items = Action.CollectAllBotItems(source);

        float toHitChance = 1f;
        // First factor in the target's evasion/avoidance rate
        float avoidance = 0f;
        List<int> unused = new List<int>();
        (avoidance, unused) = Action.CalculateAvoidance(target);
        toHitChance -= (avoidance / 100);

        // Consider any targeting bonuses (usually 0)
        toHitChance += shotData.shotTargeting;

        // - Recoil - //
        /* Anyway, weapon with recoil reduces the accuracy of all other weapons (regardless of their order).
           So if you carry a Lgt. Assault Rifle (with 1 recoil) as well as 2 Sml. Lasers, those lasers will receive -1% accuracy due to the 1 recoil.
           Recoil stacks, so if you carry 3 Lgt. Assault Riles, each one of them will receive -2% accuracy (due to the combined 2 recoil from the other 2 rifles).
         */
        float recoilBonus = 0f;
        if((source.botInfo && source.siegeMode) || (!source.botInfo && PlayerData.inst.timeTilSiege == 0)) // Negated by being in siege mode.
        {

        }
        else
        {
            foreach (var item in items)
            {
                if (Action.IsItemActionable(item) && item.itemData.slot == ItemSlot.Weapons && item != weapon)
                {
                    int recoil = item.itemData.shot.shotRecoil;
                    if (recoil > 0)
                    {
                        recoilBonus += (recoil / 100);
                    }
                }
            }
        }
        // - Then add up stability effects (from treads)
        int recoilMod = 0;
        int weaponCount = 0;
        foreach (var item in items)
        {
            if (Action.IsItemActionable(item) && item.itemData.type == ItemType.Treads)
            {
                foreach(var E in item.itemData.itemEffects)
                {
                    if (E.hasStabEffect)
                    {
                        recoilMod += E.stab_recoilPerWeapon;
                    }
                }
            }
            else if (Action.IsItemActionable(item) && item.itemData.slot == ItemSlot.Weapons)
            {
                weaponCount++;
            }
        }
        // - Then use that mod to modify the recoil bonus based on weapons
        recoilBonus += (recoilMod * weaponCount) / 100;
        

        toHitChance -= recoilBonus;

        // - Range - //
        int distance = (int)Vector2Int.Distance(Action.V3_to_V2I(source.gameObject.transform.position), Action.V3_to_V2I(target.gameObject.transform.position));
        if (distance < 6)
        {
            toHitChance += (0.03f * distance);
        }

        // - Siege Mode - //
        int siegeType = HF.DetermineSiegeType(source);
        if(siegeType == 1)
        {
            toHitChance += 0.2f;
        }
        else if(siegeType == 2)
        {
            toHitChance += 0.3f;
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

        // - Corruption Debuff -
        if (source.GetComponent<PlayerData>())
        {
            toHitChance -= (PlayerData.inst.currentCorruption / 100) / 0.04f;
        }
        else
        {
            toHitChance -= source.corruption / 0.08f;
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
         *  -Attacker accuracy reduced by [system corruption]/X% (X = 4 for Cogmind, 8 for others)
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

        // - Corruption Debuff -
        if (source.GetComponent<PlayerData>())
        {
            toHitChance -= (PlayerData.inst.currentCorruption / 100) / 0.04f;
        }
        else
        {
            toHitChance -= source.corruption / 0.08f;
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
            foreach (var item in actor.armament.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.meleeAttack.isMelee)
                    {
                        weapon = item.item.itemData.data;
                        return weapon;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.meleeAttack.isMelee)
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
            foreach (var item in actor.armament.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.shot.shotRange > 3)
                    {
                        weapon = item.item.itemData.data;
                        return weapon;
                    }
                }


            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.armament.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.data.state)
                    {
                        activeWeapons++;
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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
                foreach (var item in actor.armament.Container.Items)
                {
                    if (Action.IsItemActionable(item.item))
                    {
                        if (item.item.itemData.meleeAttack.isMelee && item.item.itemData.data != mainWeapon && !item.item.itemData.isSpecialAttack) // Is a melee weapon, isn't the main weapon, and isn't a datajack
                        {
                            weapons.Add(item.item.itemData.data);
                        }
                    }


                }
            }
            else // Player
            {
                foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items)
                {
                    if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.armament.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Launcher && item.item.itemData.data.state)
                    {
                        weapon = item.item.itemData.data;
                        return weapon;
                    }
                }


            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Treads)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Legs)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Wheels)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if(Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Hover)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Flight)
                    {
                        return true;
                    }
                }

                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if (Action.IsItemActionable(item.item))
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
        float bonus_melee = 0f;
        float bonus_ranged = 0f;

        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0)
                        {
                            if (E.toHitBuffs.hasEffect && E.toHitBuffs.flatBonus)
                            {
                                if (E.toHitBuffs.stacks)
                                {
                                    bonus_ranged += E.toHitBuffs.amount;
                                }
                                else if (E.toHitBuffs.halfStacks)
                                {
                                    bonus_ranged += E.toHitBuffs.amount;
                                }
                                else
                                {
                                    bonus_ranged = E.toHitBuffs.amount;
                                }
                            }
                            else if (E.meleeBonus.hasEffect)
                            {
                                bonus_melee += E.meleeBonus.melee_accuracyIncrease;
                            }
                        }
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0)
                        {
                            if (E.toHitBuffs.hasEffect && E.toHitBuffs.flatBonus)
                            {
                                if (E.toHitBuffs.stacks)
                                {
                                    bonus_ranged += E.toHitBuffs.amount;
                                }
                                else if (E.toHitBuffs.halfStacks)
                                {
                                    bonus_ranged += E.toHitBuffs.amount;
                                }
                                else
                                {
                                    bonus_ranged = E.toHitBuffs.amount;
                                }
                            }
                            else if (E.meleeBonus.hasEffect)
                            {
                                bonus_melee += E.meleeBonus.melee_accuracyIncrease;
                            }
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
            foreach (var item in actor.components.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0 && E.meleeBonus.hasEffect)
                        {
                            bonus_followups += E.meleeBonus.melee_followUpChance;
                            bonus_maxDamage += E.meleeBonus.melee_maxDamageBoost;
                            bonus_accuracy += E.meleeBonus.melee_accuracyIncrease;
                            bonus_accuracy += E.meleeBonus.melee_accuracyDecrease;
                            bonus_minDamage += E.meleeBonus.melee_minDamageBoost;


                            if (E.meleeBonus.stacks) // This effect should stack
                            {
                                if (stackTrack == 0) // No decrease
                                {
                                    bonus_attackTime += E.meleeBonus.melee_attackTimeDecrease;
                                    stackTrack++;
                                }
                                else // Decrease by specified amount
                                {
                                    // (Maybe do this different? i.e.: use previous item's stack decrease. idk)
                                    bonus_attackTime += (E.meleeBonus.melee_attackTimeDecrease * E.meleeBonus.actuator_cap);
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
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.itemEffects.Count > 0 && E.meleeBonus.hasEffect)
                        {
                            bonus_followups += E.meleeBonus.melee_followUpChance;
                            bonus_maxDamage += E.meleeBonus.melee_maxDamageBoost;
                            bonus_accuracy += E.meleeBonus.melee_accuracyIncrease;
                            bonus_accuracy += E.meleeBonus.melee_accuracyDecrease;
                            bonus_minDamage += E.meleeBonus.melee_minDamageBoost;


                            if (E.meleeBonus.stacks) // This effect should stack
                            {
                                if (stackTrack == 0) // No decrease
                                {
                                    bonus_attackTime += E.meleeBonus.melee_attackTimeDecrease;
                                    stackTrack++;
                                }
                                else // Decrease by specified amount
                                {
                                    // (Maybe do this different? i.e.: use previous item's stack decrease. idk)
                                    bonus_attackTime += (E.meleeBonus.melee_attackTimeDecrease * E.meleeBonus.actuator_cap);
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach(var E in item.item.itemData.itemEffects)
                    {
                        if (E.armorProtectionEffect.hasEffect && stacks)
                        {
                            bonuses.Add(E.armorProtectionEffect.armorEffect_absorbtion);

                            if (E.armorProtectionEffect.stacks)
                            {
                                stacks = true;
                            }
                            else
                            {
                                stacks = false;
                            }

                            if (E.armorProtectionEffect.armorEffect_preventCritStrikesVSSlot)
                            {
                                noCrits = true;
                            }

                            types.Add(E.armorProtectionEffect.armorEffect_slotType);
                        }
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (E.armorProtectionEffect.hasEffect && stacks)
                        {
                            bonuses.Add(E.armorProtectionEffect.armorEffect_absorbtion);

                            if (E.armorProtectionEffect.stacks)
                            {
                                stacks = true;
                            }
                            else
                            {
                                stacks = false;
                            }

                            if (E.armorProtectionEffect.armorEffect_preventCritStrikesVSSlot)
                            {
                                noCrits = true;
                            }

                            types.Add(E.armorProtectionEffect.armorEffect_slotType);
                        }
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Armor)
                    {
                        foundArmor.Add(item.item.itemData);
                    }
                }
                
                
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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

        foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items)
        {
            if (Action.IsItemActionable(item.item))
            {
                if (item.item.itemData.type == ItemType.Hackware)
                {
                    hackware.Add(item.item.itemData);
                }
            }

        }

        return (hasHackware, hackware);
    }


    public static Vector2Int V3_to_V2I(Vector3 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static int GetTotalMass(Actor actor)
    {
        int totalMass = 0;

        // Items in inventory don't count!
        if (actor != PlayerData.inst.GetComponent<Actor>()) // Bot
        {
            foreach (var item in actor.armament.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }

            foreach (var item in actor.components.Container.Items)
            {
                if (item.item.itemData.data.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_power.Container.Items.ToList())
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items.ToList())
            {
                if (item.item.Id >= 0)
                {
                    totalMass += item.item.itemData.mass;
                }
            }
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_weapon.Container.Items.ToList())
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item)) // There's something there
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
        else // Player
        {
            if(PlayerData.inst.moveType == BotMoveType.Running) // Running has a base speed of 50
            {
                propulsionParts.Add(50);
                energyCost += 1;
                heatCost += 0;
            }

            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item)) // There's something there
                {
                    foreach(var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.propulsion.Count > 0) // And its got propulsion data
                        {
                            support += item.item.itemData.propulsion[0].support;
                        }

                        if (E.hasMassSupport && stacks)
                        {
                            support += E.massSupport;
                            if (E.massSupport_stacks)
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
        }
        else // Player
        {
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (item.item.itemData.propulsion.Count > 0) // And its got propulsion data
                        {
                            support += item.item.itemData.propulsion[0].support;
                        }

                        if (E.hasMassSupport && stacks)
                        {
                            support += E.massSupport;
                            if (E.massSupport_stacks)
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
        }

        return support;
    }

    public static void UI_CombatPopup(Actor actor, string text)
    {
        Color a = Color.black, b = Color.black, c = Color.black;
        a = Color.black;
        string _message = "";

        _message = text;

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

    public static Item DamageBot(Actor target, int damage, Item weapon, Actor source = null, bool crit = false, int forcedOverflow = 0)
    {
        ItemDamageType damageType = HF.GetDamageType(weapon.itemData);

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
        List<Item> items = new List<Item>();
        List<Item> protectiveItems = new List<Item>();

        // Collect up all the items
        if (target.botInfo) // Bot
        {
            foreach (var item in target.armament.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }

            foreach (var item in target.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in target.GetComponent<PartInventory>().inv_power.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in target.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    items.Add(item.item);
                }
            }
        }

        // Calculate chance to bypass armor
        List<SimplifiedItemEffect> eList = new List<SimplifiedItemEffect>();
        foreach (var item in items)
        {
            foreach (var E in item.itemData.itemEffects)
            {
                if (E.toHitBuffs.hasEffect && E.toHitBuffs.bypassArmor)
                {
                    eList.Add(new SimplifiedItemEffect(0, E.toHitBuffs.amount, E.toHitBuffs.stacks, E.toHitBuffs.halfStacks));
                }
            }
        }
        bool bypassArmor = false;
        float bypassChance;
        int filler;
        (filler, bypassChance) = HF.ParseEffectStackingValues(eList);

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
        // - Apply any core exposure boost effects
        float coreExposureBoost = 0f;
        foreach (var item in items)
        {
            foreach (var E in item.itemData.itemEffects)
            {
                if (E.toHitBuffs.hasEffect && E.toHitBuffs.coreExposureEffect)
                {
                    if (E.toHitBuffs.coreExposureGCM_only)
                    {
                        if(weapon.itemData.type == ItemType.Gun || weapon.itemData.type == ItemType.Cannon || weapon.itemData.type == ItemType.Melee)
                        {
                            if (E.toHitBuffs.stacks)
                            {
                                if (E.toHitBuffs.halfStacks) // Half effect
                                {
                                    coreExposureBoost += E.toHitBuffs.amount / 2;
                                }
                                else // Full effect
                                {
                                    coreExposureBoost += E.toHitBuffs.amount;
                                }
                            }
                            else // Replace effect
                            {
                                coreExposureBoost = E.toHitBuffs.amount;
                            }
                        }
                    }
                    else
                    {
                        if (E.toHitBuffs.stacks)
                        {
                            if (E.toHitBuffs.halfStacks) // Half effect
                            {
                                coreExposureBoost += E.toHitBuffs.amount / 2;
                            }
                            else // Full effect
                            {
                                coreExposureBoost += E.toHitBuffs.amount;
                            }
                        }
                        else // Replace effect
                        {
                            coreExposureBoost = E.toHitBuffs.amount;
                        }
                    }
                }
            }
        }
        coreExposure = Mathf.RoundToInt(coreExposure + (float)(coreExposure * coreExposureBoost));

        // - Should we bypass the armor?
        if(Random.Range(0f, 1f) < bypassChance)
        {
            bypassArmor = true;
        }

        // Calculate max exposure
        int totalExposure = 0;

        foreach (var item in items)
        {
            if (bypassArmor && item.itemData.type == ItemType.Armor) // Do we bypass any armor?
            {

            }
            else
            {
                totalExposure += item.itemData.coverage;

                // All armor and heavy treads have double coverage in *Siege Mode*
                if((source.botInfo && source.siegeMode) || (!source.botInfo && PlayerData.inst.timeTilSiege == 0))
                {
                    if(item.itemData.type == ItemType.Armor || item.itemData.propulsion[0].canSiege != 0)
                    {
                        totalExposure += item.itemData.coverage;
                    }
                }

                // Gather up all protective items (we use this later)
                foreach (var E in item.itemData.itemEffects)
                {
                    if (E.armorProtectionEffect.hasEffect)
                    {
                        protectiveItems.Add(item);
                    }
                }
            }
        }
        totalExposure += coreExposure;

        // Calculate individual chances to hit each object
        List<KeyValuePair<ItemObject, float>> pairs = new List<KeyValuePair<ItemObject, float>>();

        foreach (var item in items)
        {
            pairs.Add(new KeyValuePair<ItemObject, float>(item.itemData, item.itemData.coverage / totalExposure));
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
            foreach (var E in P.itemData.itemEffects)
            {
                if (E.armorProtectionEffect.type_hasTypeResistance) // Has a resistance to a type
                {
                    if (E.armorProtectionEffect.type_allTypes || E.armorProtectionEffect.type_damageType == damageType) // Has damage resist or generalist resist
                    {
                        if (stacks)
                        {
                            damage = Mathf.RoundToInt(damage + (float)(damage * E.armorProtectionEffect.type_percentage)); // Modify damage
                        }

                        stacks = E.armorProtectionEffect.stacks;
                    }
                }
            }
        }
        int damageA = damage; // Saved for later (Heat transfer)
        int damageB = 0;
        bool firstSheild = false;

        // Shields
        #region Shields
        List<Item> pe_individual = new List<Item>(); // Individual damage -> energy protection items (used later)

        // Shield items
        foreach (var P in protectiveItems)
        {
            foreach(var E in P.itemData.itemEffects)
            {
                if (E.armorProtectionEffect.projectionExchange)
                {
                    if (E.armorProtectionEffect.pe_global)
                    {
                        float blocks = E.armorProtectionEffect.pe_blockPercent;
                        Vector2 exchange = E.armorProtectionEffect.pe_exchange;

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
                        if ((E.armorProtectionEffect.pe_requireEnergy && currentEnergy >= cost) || !E.armorProtectionEffect.pe_requireEnergy)
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

                        firstSheild = true;
                    }
                    else
                    {
                        pe_individual.Add(P);
                    }
                }

                if (firstSheild && damageB <= 0)
                {
                    damageB = damage;
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
                foreach(var E in item.itemData.itemEffects)
                {
                    if (item.Id >= 0 && E.armorProtectionEffect.hasEffect && E.armorProtectionEffect.projectionExchange)
                    {
                        if (E.armorProtectionEffect.pe_includeVisibileAllies)
                        {
                            if(Vector2.Distance(bot.transform.position, target.transform.position) <= E.armorProtectionEffect.pe_alliesDistance)
                            {
                                float blocks = E.armorProtectionEffect.pe_blockPercent;
                                Vector2 exchange = E.armorProtectionEffect.pe_exchange;

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
                                if ((E.armorProtectionEffect.pe_requireEnergy && currentEnergy >= cost) || !E.armorProtectionEffect.pe_requireEnergy)
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

                                firstSheild = true;
                            }
                        }
                        else if (E.armorProtectionEffect.type_includeAllies)
                        {
                            if (E.armorProtectionEffect.type_damageType == HF.GetDamageType(weapon.itemData) || E.armorProtectionEffect.type_allTypes)
                            {
                                if(Vector2.Distance(bot.transform.position, target.transform.position) <= E.armorProtectionEffect.type_alliesRange)
                                {
                                    damage = Mathf.RoundToInt(damage + (float)(damage * E.armorProtectionEffect.type_percentage));
                                }
                            }
                        }
                    }

                    if (firstSheild && damageB <= 0)
                    {
                        damageB = damage;
                    }
                }
            }
        }

        // Phase walls (-10% damage)
        if(source != null)
        {
            // - Is the source firing through a phase wall?
            // (This is a bit overkill cause this will rarely happen but here we go)
            Vector2 targetDirection = target.transform.position - source.transform.position;
            float distance = Vector2.Distance(Action.V3_to_V2I(source.transform.position), Action.V3_to_V2I(target.transform.position));
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(source.transform.position.x, source.transform.position.y), targetDirection.normalized, distance);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.GetComponent<TileBlock>() && hits[i].collider.GetComponent<TileBlock>().phaseWall)
                {
                    damage = Mathf.RoundToInt(damage - (float)(damage * 0.1f));

                    firstSheild = true;
                    if (firstSheild && damageB <= 0)
                    {
                        damageB = damage;
                    }
                    break;
                }
            }
        }

        // Stasis Bubble / Stasis Trap (+25% damage)
        // - Is the target currently in a stasis bubble or stasis trap?
        if (target.botInfo)
        {
            if (target.inStatis)
            {
                damage = Mathf.RoundToInt(damage + (float)(damage * 0.25f));

                firstSheild = true;
                if (firstSheild && damageB <= 0)
                {
                    damageB = damage;
                }
            }
        }
        else
        {
            if (PlayerData.inst.lockedInStasis)
            {
                damage = Mathf.RoundToInt(damage + (float)(damage * 0.25f));

                firstSheild = true;
                if (firstSheild && damageB <= 0)
                {
                    damageB = damage;
                }
            }
        }
        #endregion

        #endregion

        #region EM Disruption
        bool chainReaction = false;
        if (target.botInfo) // Only effects AI
        {
            // Pick a random item (probably wrong but whatever)
            Item targetItem = items[Random.Range(0, items.Count - 1)];

            if (target.botInfo.resistancesExtra.disruption == false) // Not immune to it
            {
                // Disable roll
                if (Random.Range(0f, 1f) < 0.1f) // Not too sure what the chances of this should be so its gonna be at 10%
                {
                    if (Random.Range(0f, 1f) < 0.7f) // Hit the part (70% chance)
                    {
                        targetItem.disabledTimer += 10;
                    }
                    else // Hit the core
                    {
                        target.DisableThis(10);
                    }
                }

                // Chain reaction roll
                if (targetItem.itemData.type == ItemType.Engine) // Is an engine
                {
                    float spectrum = 0;
                    if (weapon.itemData.projectile.hasSpectrum)
                    {
                        spectrum = weapon.itemData.projectile.spectrum;
                    }
                    else if (weapon.itemData.explosionDetails.hasSpectrum)
                    {
                        spectrum = weapon.itemData.explosionDetails.spectrum;
                    }

                    if (spectrum > 0) // Has spectrum
                    {
                        // Roll for chain reaction explosion
                        if (Random.Range(0f, 1f) < spectrum)
                        {
                            chainReaction = true;

                            // Rope, Lamp Oil, Bombs? You want it? It's yours my friend.
                            ChainReactionExplode(target, targetItem);
                            targetItem = null; // This thing ain't coming back
                        }
                    }
                }


            }
        }
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
        int transferredCoreDamage = 0;

        // No part attacking when a chain reaction happened
        if (!chainReaction)
        {
            // Check which range the random number falls into
            for (int i = 0; i < cumulativeProbabilities.Count; i++)
            {
                if (rand < cumulativeProbabilities[i])
                {
                    #region Alien Protection
                    // Hold it! Does the target have any of the wacky alien protection effects?
                    float alienProtection = Action.CalculateAlienDamageTransfer(items);
                    if(alienProtection > 0f) // Yes
                    {
                        transferredCoreDamage = Mathf.RoundToInt(damage * alienProtection); // Save this reduced damage for later (goes against core)
                        damage -= transferredCoreDamage; // And reduce the damage
                    }
                    #endregion

                    // Item i is hit
                    Item part = pairs[i].Key.data;

                    // Now before we deal damage to this item. We need to check a few things.

                    // 1. If the part we just hit has the protection effect of damage -> power.
                    foreach (var item in pe_individual)
                    {
                        if(item == part)
                        {
                            foreach(var E in item.itemData.itemEffects)
                            {
                                if (E.armorProtectionEffect.projectionExchange)
                                {
                                    float blocks = E.armorProtectionEffect.pe_blockPercent;
                                    Vector2 exchange = E.armorProtectionEffect.pe_exchange;

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
                                    if ((E.armorProtectionEffect.pe_requireEnergy && currentEnergy >= cost) || !E.armorProtectionEffect.pe_requireEnergy)
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

                                        // Then damage the item
                                        hitItem = part;
                                        Action.DamageItem(target, hitItem, damage);
                                    }
                                }
                            }
                        }
                    }

                    if(hitItem != null) // Stop if we hit something in step 1.
                    {
                        break;
                    }

                    // 2. If part is treads currently in siege mode reduce damage appropriately (-25% to -50% damage)
                    // https://www.gridsagegames.com/blog/2019/09/siege-tread-mechanics/
                    if (part.itemData.type == ItemType.Treads && part.itemData.propulsion[0].canSiege != 0 && part.siege)
                    {
                        float reduction = 0f;
                        if (part.itemData.propulsion[0].canSiege == 1)
                        {
                            reduction = 0.25f;
                        }
                        else if (part.itemData.propulsion[0].canSiege == 2)
                        {
                            reduction = 0.50f;
                        }

                        damage = Mathf.RoundToInt(damage - (float)(damage * reduction));
                    }

                    // 3. If the target has slot specific protection, and deal damage to that aswell
                    ItemSlot slot = part.itemData.slot;
                    int splitDamage = 0;
                    Item splitItem = null;
                    foreach (var A in protectiveItems)
                    {
                        foreach (var E in A.itemData.itemEffects)
                        {
                            if (E.armorProtectionEffect.armorEffect_slotType.ToString().ToLower() == slot.ToString().ToLower()) // A bit of a janky way to convert
                            {
                                float percent = E.armorProtectionEffect.armorEffect_absorbtion;

                                splitDamage = Mathf.RoundToInt(damage * percent);
                                damage -= splitDamage;
                                splitItem = A.itemData.data;
                                break;
                            }
                        }
                    }

                    // Damage the specific protection if it exists
                    if (splitItem != null)
                    {
                        // Deal damage
                        hitItem = splitItem;
                        Action.DamageItem(target, splitItem, splitDamage);
                    }


                    // Deal damage
                    hitItem = part;
                    (overflow, armorDestroyed) = Action.DamageItem(target, part, damage);
                    slotHit = part.itemData.slot;
                    hitPart = true;
                }
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
                foreach(var E in A.itemData.itemEffects)
                {
                    if (E.armorProtectionEffect.armorEffect_slotType == ArmorType.None)
                    {
                        float percent = E.armorProtectionEffect.armorEffect_absorbtion;

                        splitDamage = Mathf.RoundToInt(damage * percent);
                        damage -= splitDamage;
                        splitItem = A.itemData.data;
                        break;
                    }
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
                Action.ModifyPlayerCore(-damage);
            }
        }

        // And if it exists, apply transfer damage to core
        if(transferredCoreDamage > 0)
        {
            if (target.botInfo)
            {
                target.currentHealth -= transferredCoreDamage;
            }
            else
            {
                Action.ModifyPlayerCore(-transferredCoreDamage);
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
            Action.DamageBot(target, damage, weapon, source); // Recursion! (This doesn't strictly target armor first but whatever).
        }
        #endregion

        #region Crits
        // https://www.gridsagegames.com/blog/2021/05/design-overhaul-3-damage-types-and-criticals/
        CritType critType = CritType.Nothing;
        if (weapon.itemData.meleeAttack.isMelee) // Melee
        {
            critType = weapon.itemData.meleeAttack.critType;
        }
        else // Ranged
        {
            critType = weapon.itemData.projectile.critType;
        }

        // First off, is what we are targeting immune to crits?
        #region Crit Immunity
        // Check innate stats
        if (crit && target.botInfo)
        {
            // Some bots have special resistances that are actually immunities to some types of crits.
            BotResistancesExtra imm = target.botInfo.resistancesExtra;

            if (imm.criticals)
            {
                crit = false;
                UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to critical effects.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
            }

            switch (critType)
            {
                case CritType.Nothing:
                    crit = false;
                    break;
                case CritType.Burn:
                    break;
                case CritType.Meltdown:
                    if (imm.meltdown)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to a total meltdown.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Destroy:
                    if (imm.coring)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to coring.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Blast:
                    if (imm.coring)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to coring.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    else if(imm.dismemberment)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to dismemberment.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Corrupt:
                    break;
                case CritType.Smash:
                    if (imm.coring)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to coring.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Sever:
                    if (imm.dismemberment)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to dismemberment.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Puncture:
                    if (imm.coring)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to coring.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Detonate:
                    break;
                case CritType.Sunder:
                    if (imm.dismemberment)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to dismemberment.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Intensify:
                    break;
                case CritType.Phase:
                    if (imm.coring)
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage(target.botInfo.botName + " is immune to coring.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                    }
                    break;
                case CritType.Impale:
                    break;
            }
        }
        else if(crit && !target.botInfo) // Conditional player crit immunity
        {
            // Siege mode makes them immune to part destruction from critical hits
            if(PlayerData.inst.timeTilSiege == 0) // In siege mode
            {
                if(critType == CritType.Destroy || critType == CritType.Blast || critType == CritType.Smash || critType == CritType.Detonate || critType == CritType.Sunder)
                {
                    crit = false;
                    UIManager.inst.CreateNewLogMessage("    " + "Critical part destruction prevented due to siege mode.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                }
            }
        }

        if (crit)
        {
            // Check items
            foreach (var item in protectiveItems)
            {
                foreach(var E in item.itemData.itemEffects)
                {
                    if (E.armorProtectionEffect.armorEffect_preventCritStrikesVSSlot) // Armor protection
                    {
                        if (E.armorProtectionEffect.armorEffect_slotType.ToString().ToLower() == slotHit.ToString().ToLower())
                        {
                            crit = false;
                            UIManager.inst.CreateNewLogMessage("    " + item.itemData.itemName + " prevented critical effect.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                            break;
                        }
                    }

                    if (E.armorProtectionEffect.critImmunity) // Special anti-crit item
                    {
                        crit = false;
                        UIManager.inst.CreateNewLogMessage("    " + item.itemData.itemName + " prevented critical effect.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
                        break;
                    }
                }
            }
        }
        #endregion

        bool burnCrit = false;
        bool corruptCrit = false;
        if(crit == true && weapon.itemData.explosionDetails.radius <= 0) // Is a crit & not an explosion
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

            switch (critType)
            {
                case CritType.Nothing:
                    break;
                case CritType.Burn:
                    burnCrit = true;
                    if (target.botInfo)
                    {
                        UIManager.inst.CreateNewCalcMessage(target.botInfo.botName + " suffers critical burn.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        UIManager.inst.CreateNewCalcMessage("Critical heat increase detected.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Meltdown:
                    if (target.botInfo)
                    {
                        target.Die(target.botInfo.botName + " suffers critical meltdown.");
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
                            target.Die(target.botInfo.botName + "'s core has been completely destroyed.");
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
                    Item part = Action.DamageBot(target, damage, weapon, source, false);
                    if(part != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            Action.RemovePartFromBotInventory(target, part);
                            InventoryControl.inst.DropItemOnFloor(part, target, null, Vector2Int.zero);
                            
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(part, target, HF.FindPlayerInventoryFromItem(part), Vector2Int.zero);
                        }
                        UIManager.inst.CreateNewCalcMessage(part.Name + " was blasted off.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }

                    break;
                case CritType.Corrupt:
                    corruptCrit = true;
                    if (target.botInfo)
                    {
                        UIManager.inst.CreateNewCalcMessage(target.botInfo.botName + " suffers critical corruption.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        UIManager.inst.CreateNewCalcMessage("Critical corruption increase detected.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Smash:
                    // Roll again!
                    Item part2 = Action.DamageBot(target, damage, weapon, source, false, damage);
                    if (part2 != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            Action.RemovePartFromBotInventory(target, part2);
                            InventoryControl.inst.DropItemOnFloor(part2, target, null, Vector2Int.zero);
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(part2, target, HF.FindPlayerInventoryFromItem(part2), Vector2Int.zero);
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
                        // Forcefully drop a random item
                        Item dropItem = items[Random.Range(0, items.Count - 1)];
                        if (target.botInfo)
                        {
                            Action.RemovePartFromBotInventory(target, dropItem);
                            InventoryControl.inst.DropItemOnFloor(dropItem, target, null, Vector2Int.zero);
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(dropItem, target, HF.FindPlayerInventoryFromItem(dropItem), Vector2Int.zero);
                        }

                        UIManager.inst.CreateNewCalcMessage(dropItem.Name + " was severed.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                        
                    }
                    else // Hit part
                    {
                        if (hitItem != null)
                        {
                            // If the 2nd part survives, forcefully drop it.
                            if (target.botInfo)
                            {
                                Action.RemovePartFromBotInventory(target, hitItem);
                                InventoryControl.inst.DropItemOnFloor(hitItem, target, null, Vector2Int.zero);
                            }
                            else
                            {
                                InventoryControl.inst.DropItemOnFloor(hitItem, target, HF.FindPlayerInventoryFromItem(hitItem), Vector2Int.zero);
                            }
                        }
                    }

                    break;
                case CritType.Puncture:
                    // Deal damage to the core
                    if (target.botInfo)
                    {
                        target.currentHealth -= (damage / 2);

                        UIManager.inst.CreateNewCalcMessage(target.botInfo.botName + " suffers critical puncture.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    else
                    {
                        Action.ModifyPlayerCore(-(int)(damage / 2));

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
                    item4 = Action.DamageBot(target, damage, weapon, source, false);
                    if (item4 != null)
                    {
                        // If the 2nd part survives, forcefully drop it.
                        if (target.botInfo)
                        {
                            Action.RemovePartFromBotInventory(target, item4);
                            InventoryControl.inst.DropItemOnFloor(item4, target, null, Vector2Int.zero);
                        }
                        else
                        {
                            InventoryControl.inst.DropItemOnFloor(item4, target, HF.FindPlayerInventoryFromItem(item4), Vector2Int.zero);
                        }

                        UIManager.inst.CreateNewCalcMessage(item4.Name + " was ripped off.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                    }
                    break;
                case CritType.Intensify: // Zio. Phaser-S/M/H have this
                    // (Assumption): Double Damage
                    if (hitItem.Id >= 0 && hitItem.integrityCurrent > 0)
                    {
                        Action.DamageBot(target, damage, weapon, source, false);
                    }
                    string botName = "";
                    if (target.botInfo)
                    {
                        botName = target.botInfo.botName;
                    }
                    else
                    {
                        botName = "Cogmind";
                    }

                    UIManager.inst.CreateNewCalcMessage("Damage itensified against " + botName + " [" + damage + "].", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);

                    break;
                case CritType.Phase: // L-Cannon, Drained L-Cannon, Zio. Alpha-Cannon & Zio. Alpha-Cannon MK.2 has this
                    // "Damage phase-mirrored to [name] %1." | [name] enveloped in phasic energy. see: https://youtu.be/0I_-Tuuv4Ww?si=MfiJZ4VfZbAkKsKX&t=9715
                    // (Assumption): Mirror damage to neighbor
                    Actor neighbor = Action.FindNewNeighboringEnemy(target);
                    DamageBot(neighbor, damage, weapon, source, false);
                    string botName2 = "";
                    if (target.botInfo)
                    {
                        botName2 = target.botInfo.botName;
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
                        UIManager.inst.CreateNewCalcMessage("Critical on " + target.botInfo.botName + "'s core.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                        target.Die(target.botInfo.botName + "'s core has impaled by " + weapon + ", destroying it completely.");
                    }
                    else
                    {
                        PlayerData.inst.currentHealth = 0;
                    }
                    break;
            }

            if (!target.botInfo) // AI Only
            {
                if (critType == CritType.Burn) // Since Burn is so common, only show it if the bot is already extremely hot already.
                {
                    if(target.currentHeat > 250)
                    {
                        UIManager.inst.CreateShortCombatPopup(target.gameObject, critType.ToString(), Color.black, Color.black, target); // Create a special (short) combat popup
                    }
                }
                else
                {
                    UIManager.inst.CreateShortCombatPopup(target.gameObject, critType.ToString(), Color.black, Color.black, target); // Create a special (short) combat popup
                }
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
            int salvageMod = HF.GetSalvageMod(weapon);
            int bonus = 0;

            // Salvage Mod Items
            foreach (var item in items)
            {
                foreach(var E in item.itemData.itemEffects)
                {
                    if (E.salvageBonus.hasEffect)
                    {
                        if (E.salvageBonus.gunTypeOnly && weapon.itemData.type == ItemType.Gun)
                        {
                            if (weapon.itemData.projectileAmount == 1)
                            {
                                if (E.salvageBonus.stacks)
                                {
                                    bonus += E.salvageBonus.bonus;
                                }
                                else
                                {
                                    bonus = E.salvageBonus.bonus;
                                }
                            }
                        }
                        else if (!E.salvageBonus.gunTypeOnly)
                        {
                            if (E.salvageBonus.stacks)
                            {
                                bonus += E.salvageBonus.bonus;
                            }
                            else
                            {
                                bonus = E.salvageBonus.bonus;
                            }
                        }
                    }
                }
            }

            salvageMod += bonus;
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

                int corruption = Mathf.RoundToInt(damageA * amount);
                target.corruption += (corruption / 100);
            }
            else // Player
            {
                float amount = Random.Range(0.5f, 1.5f);
                if (corruptCrit)
                {
                    amount = 1.5f; // Maximized due to crit
                }

                int corruption = Mathf.RoundToInt(damageA * amount);
                float chance = corruption / 100;

                if(Random.Range(0f, 1f) < chance)
                {
                    Action.ModifyPlayerCorruption(1);
                }
            }
        }
        #endregion

        #region Heat Transfer
        // Heat transfer
        Action.DealHeatTransfer(target.GetComponent<Actor>(), weapon, new Vector2Int(damageA, damageB), burnCrit);
        #endregion

        #region Knockback
        // First check if this weapon is capable of knocking back the target (Kinetic cannons OR Impact melee weapons).
        if(weapon.itemData.type == ItemType.Cannon || (weapon.itemData.meleeAttack.isMelee && weapon.itemData.meleeAttack.damageType == ItemDamageType.Impact))
        {
            // Then check if bot is knockback immune.
            bool knockbackImmune = false;
            // Consider if the target is immune to knockback (siege mode and whatnot)
            foreach (var item in items)
            {
                if (item.Id > -1 && item.itemData.type == ItemType.Treads)
                {
                    foreach (var E in item.itemData.itemEffects)
                    {
                        if (E.hasStabEffect && E.stab_KnockbackImmune)
                        {
                            if (target.botInfo) // Bot
                            {
                                knockbackImmune = true;
                            }
                            else // Player
                            {
                                knockbackImmune = true;
                            }
                        }
                    }
                }
            }
            if (!knockbackImmune)
            {
                int knockback = 0;
                /* >> Knockback! <<
                 * "Kinetic cannons have a damage-equivalent chance to cause knockback, with a (10 - range) * 5 modifier and a +/-10% per size class (T/S/M/L/H) difference 
                 * between the target and medium size (targets knocked into another robot may also damage and displace it, see Impact below)."
                 * &
                 * "Impact melee weapons have a damage-equivalent chance to cause knockback, with a +/-10% per size class (T/S/M/L/H) difference 
                 * between attacker and target. Targets knocked into another robot may also damage and displace it. A robot hit by another displaced 
                 * robot has a chance to itself be displaced and sustain damage, where the chance equals the original knockback chance further 
                 * modified by +/-10% per size class difference between the blocking robot and the knocked back robot, and the resulting damage 
                 * equals [originalDamage] (see Attack Resolution), further divided by the blocker size class if that class is greater than 1 (where Medium = 2, and so on)."
                 */
                if (weapon.itemData.type == ItemType.Cannon)
                {
                    knockback = damage + ((10 - weapon.itemData.shot.shotRange) * 5);
                    // and consider size
                    BotSize size = BotSize.Medium;
                    if (target.botInfo)
                    {
                        size = target.botInfo._size;
                    } // Player size doesn't change and is medium

                    switch (size)
                    {
                        case BotSize.Tiny:
                            knockback = Mathf.RoundToInt(knockback + (float)(knockback * 0.20f));
                            break;
                        case BotSize.Small:
                            knockback = Mathf.RoundToInt(knockback + (float)(knockback * 0.10f));
                            break;
                        case BotSize.Medium:
                            break;
                        case BotSize.Large:
                            knockback = Mathf.RoundToInt(knockback - (float)(knockback * 0.10f));
                            break;
                        case BotSize.Huge:
                            knockback = Mathf.RoundToInt(knockback - (float)(knockback * 0.20f));
                            break;
                    }

                    // No idea how to calculate this roll so we doing this
                    if(Random.Range(0f, 1f) < knockback / 100)
                    {
                        Vector2Int direction = HF.V3_to_V2I(target.transform.position) - HF.V3_to_V2I(source.transform.position);

                        Action.KnockbackAction(target, weapon, direction, knockback, damageA);
                    }
                }
                else if (weapon.itemData.meleeAttack.isMelee && weapon.itemData.meleeAttack.damageType == ItemDamageType.Impact)
                {
                    BotSize size_a = BotSize.Medium;
                    if (source)
                    {
                        if (source.botInfo)
                        {
                            size_a = target.botInfo._size;
                        } // Player size doesn't change and is medium
                    }
                    BotSize size_t = BotSize.Medium;
                    if (target.botInfo)
                    {
                        size_t = target.botInfo._size;
                    } // Player size doesn't change and is medium

                    float sizeMod = Action.CalculateSizeModifier(size_a, size_t);
                    knockback = damage;
                    knockback = Mathf.RoundToInt(knockback + (float)(knockback * sizeMod));

                    // No idea how to calculate this roll so we doing this
                    if (Random.Range(0f, 1f) < knockback / 100)
                    {
                        Vector2Int direction = HF.V3_to_V2I(target.transform.position) - HF.V3_to_V2I(source.transform.position);

                        Action.KnockbackAction(target, weapon, direction, knockback, damageA);
                    }
                }
            }
        }

        #endregion

        // If the item has survived, return it (used for some stuff)
        if (hitItem.Id >= 0 && hitItem.integrityCurrent > 0)
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
        // There is a chance the player doesn't know what the item is that got destroyed
        string item_name = item.itemData.itemName;
        if (!item.itemData.knowByPlayer)
        {
            item_name = HF.ItemPrototypeName(item);
        }

        if (owner.botInfo) // Bot
        {
            Action.RemovePartFromBotInventory(owner, item);

            // Display a (Short) Combat popup targeting on the bot to show what was lost
            UIManager.inst.CreateShortCombatPopup(owner.gameObject, $"-{item_name}", UIManager.inst.highSecRed, UIManager.inst.dangerRed);
        }
        else // Player
        {
            Action.FindRemoveItemFromPlayer(item);

            // Have the InvDisplayItem we just destroyed do its destruction animation (this also plays the sound, destroys duplicates, & updates the UI.)
            foreach (var I in InventoryControl.inst.interfaces)
            {
                foreach (KeyValuePair<GameObject, InventorySlot> kvp in I.GetComponent<UserInterface>().slotsOnInterface.ToList())
                {
                    if(kvp.Value.item == item)
                    {
                        kvp.Key.GetComponent<InvDisplayItem>().DestroyAnimation();
                        break;
                    }
                }
            }
            // Display a (Short) Combat popup targeting on the bot to show what was lost
            UIManager.inst.CreateShortCombatPopup(PlayerData.inst.gameObject, $"-{item_name}", UIManager.inst.highSecRed, UIManager.inst.dangerRed);

            if (crit)
            {
                UIManager.inst.CreateNewLogMessage(item_name + " has been completely destroyed.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
            }
            else
            {
                UIManager.inst.CreateNewLogMessage(item_name + " destroyed.", UIManager.inst.highSecRed, UIManager.inst.dangerRed, false, true);
            }

            // "Cogmind automatically recycles 5 matter from each attached part that is destroyed."
            PlayerData.inst.currentMatter += 5;
            if (PlayerData.inst.currentMatter > PlayerData.inst.maxMatter)
            {
                PlayerData.inst.currentMatter = PlayerData.inst.maxMatter;
            }

            UIManager.inst.UpdatePSUI();
        }

        item.Id = -1;
    }

    public static void FindRemoveItemFromPlayer(Item item)
    {
        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_power.Container.Items.ToList())
        {
            if (I.item.Id >= 0)
            {
                if (I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, PlayerData.inst.GetComponent<PartInventory>().inv_power);
                    return;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items.ToList())
        {
            if (I.item.Id >= 0)
            {
                if (I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, PlayerData.inst.GetComponent<PartInventory>().inv_propulsion);
                    return;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items.ToList())
        {
            if (I.item.Id >= 0)
            {
                if (I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, PlayerData.inst.GetComponent<PartInventory>().inv_utility);
                    return;
                }
            }
        }

        foreach (InventorySlot I in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items.ToList())
        {
            if (I.item.Id >= 0)
            {
                if (I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, PlayerData.inst.GetComponent<PartInventory>().inv_weapon);
                    return;
                }
            }
        }
    }

    public static void RemovePartFromBotInventory(Actor bot, Item item)
    {
        if (bot.botInfo) // Bot
        {
            foreach (var I in bot.components.Container.Items.ToList())
            {
                if (I.item.Id >= 0 && I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, bot.components);
                    return;
                }
            }

            foreach (var I in bot.armament.Container.Items.ToList())
            {
                if (I.item.Id >= 0 && I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, bot.armament);
                    return;
                }
            }

            foreach (var I in bot.inventory.Container.Items.ToList())
            {
                if (I.item == item)
                {
                    InventoryControl.inst.RemoveItemFromAnInventory(I.item, bot.inventory);
                    return;
                }
            }
        }
    }

    public static float GatherCritBonuses(Actor actor)
    {
        float bonus = 0f;
        List<Item> items = Action.CollectAllBotItems(actor);

        int A;
        List<SimplifiedItemEffect> eList = new List<SimplifiedItemEffect>();
        foreach (var item in items)
        {
            foreach(var E in item.itemData.itemEffects)
            {
                if (E.toHitBuffs.hasEffect)
                {
                    if (E.toHitBuffs.bonusCritChance)
                    {
                        float amount = E.toHitBuffs.amount;
                        eList.Add(new SimplifiedItemEffect(0, amount, E.toHitBuffs.stacks, E.toHitBuffs.halfStacks));
                    }
                }
            }
        }

        (A, bonus) = HF.ParseEffectStackingValues(eList);

        return bonus;
    }

    public static int AddFlatDamageBonuses(int initialDamage, Actor attacker, Item weapon)
    {
        int damage = initialDamage;
        float bonus = 0f;

        if (attacker.botInfo) // Bot
        {
            foreach (var item in attacker.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach(var E in item.item.itemData.itemEffects)
                    {
                        if (E.flatDamageBonus.hasEffect)
                        {
                            if (E.flatDamageBonus.types.Contains(HF.GetDamageType(weapon.itemData))) // Same type?
                            {
                                if (E.flatDamageBonus.stacks)
                                {
                                    bonus += E.flatDamageBonus.damageBonus;
                                }
                                else
                                {
                                    if (E.flatDamageBonus.halfStacks)
                                    {
                                        bonus += E.flatDamageBonus.damageBonus / 2;
                                    }
                                    else
                                    {
                                        bonus = E.flatDamageBonus.damageBonus;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in attacker.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (var E in item.item.itemData.itemEffects)
                    {
                        if (E.flatDamageBonus.hasEffect)
                        {
                            if (E.flatDamageBonus.types.Contains(HF.GetDamageType(weapon.itemData))) // Same type?
                            {
                                if (E.flatDamageBonus.stacks)
                                {
                                    bonus += E.flatDamageBonus.damageBonus;
                                }
                                else
                                {
                                    if (E.flatDamageBonus.halfStacks)
                                    {
                                        bonus += E.flatDamageBonus.damageBonus / 2;
                                    }
                                    else
                                    {
                                        bonus = E.flatDamageBonus.damageBonus;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        damage = Mathf.RoundToInt(damage + (damage * bonus));

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
                foreach (var item in actor.components.Container.Items)
                {
                    if (Action.IsItemActionable(item.item))
                    {
                        // I still have no idea how this is actually calculated, this is just a guess
                        if (item.item.itemData.type == ItemType.Flight)
                        {
                            evasionBonus1 += (int)(1.5f * item.item.itemData.slotsRequired);
                        }
                        else if (item.item.itemData.type == ItemType.Hover)
                        {
                            evasionBonus1 += (int)(1 * item.item.itemData.slotsRequired);
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if(actor.momentum > 0)
                    {
                        if (item.item.itemData.type == ItemType.Legs)
                        {
                            foreach (ItemEffect effect in item.item.itemData.itemEffects)
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

                    if(item.item.itemData.type == ItemType.Device && actor.weightCurrent <= actor.weightMax && rcsStack)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffects)
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
            foreach (var item in actor.components.Container.Items)
            {
                if(item.item.itemData.data.Id > 0 && item.item.itemData.type == ItemType.Device)
                {
                    foreach (ItemEffect effect in item.item.itemData.itemEffects)
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

            // -- Flight/Hover bonus (1) -- //

            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
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

            // -- Heat Level (2) -- //
            // This appears to add a -1 bonus for every 30 or so heat.
            int heatCalc = 0;
            if (PlayerData.inst.currentHeat > 29)
            {
                heatCalc = (int)(PlayerData.inst.currentHeat / 30);
            }
            evasionBonus2 = -heatCalc;

            // -- Movement Speed (3) -- //
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

            // -- Evasion modifiers (4) -- //
            // These effects can be found in 1. Legs 2. Variants of the reaction control system device
            bool rcsStack = true;
            BotMoveType myMoveType = DetermineBotMoveType(actor);
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (actor.momentum > 0)
                    {
                        if (item.item.itemData.type == ItemType.Legs)
                        {
                            foreach (ItemEffect effect in item.item.itemData.itemEffects)
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

            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Device && actor.weightCurrent <= actor.weightMax && rcsStack)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffects)
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

            // -- Phasing / Cloaking modifiers (5) -- //
            bool phasingStack = true;
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if(item.item.Id > 0)
                {
                    if (item.item.itemData.type == ItemType.Device)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffects)
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
            foreach (var item in actor.components.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    foreach (ItemEffect effect in item.item.itemData.data.itemData.itemEffects)
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
            foreach (InventorySlot item in actor.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (Action.IsItemActionable(item.item))
                {
                    if (item.item.itemData.type == ItemType.Device)
                    {
                        foreach (ItemEffect effect in item.item.itemData.itemEffects)
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
        int h_cost = 0;
        int m_cost = 0;
        int e_cost = 0;

        if (weapon.itemData.meleeAttack.isMelee)
        {
            if (source.GetComponent<PlayerData>())
            {
                h_cost = weapon.itemData.meleeAttack.heat;
                m_cost = weapon.itemData.meleeAttack.matter;
                e_cost = weapon.itemData.meleeAttack.energy;

                Action.ModifyPlayerEnergy(e_cost);
                Action.ModifyPlayerMatter(m_cost);

                PlayerData.inst.currentHeat += h_cost;
                PlayerData.inst.currentMatter += m_cost;
                PlayerData.inst.currentEnergy += e_cost;
            }
            else
            {
                source.currentHeat += weapon.itemData.meleeAttack.heat;
                source.currentEnergy += weapon.itemData.meleeAttack.energy;
                // Bots dont have matter
            }
        }
        else
        {
            if (source.GetComponent<PlayerData>())
            {
                h_cost += weapon.itemData.shot.shotHeat;
                m_cost += weapon.itemData.shot.shotMatter;
                e_cost += weapon.itemData.shot.shotEnergy;

                if (weapon.isOverloaded) // Overloading
                {
                    // Double the energy cost
                    e_cost += weapon.itemData.shot.shotEnergy;

                    // Triple the heat production
                    h_cost += weapon.itemData.shot.shotHeat;
                    h_cost += weapon.itemData.shot.shotHeat;
                }

                UIManager.inst.Matter_AnimateDecrease(m_cost);
                UIManager.inst.Energy_AnimateDecrease(e_cost);

                PlayerData.inst.currentHeat += h_cost;
                PlayerData.inst.currentMatter += m_cost;
                PlayerData.inst.currentEnergy += e_cost;
            }
            else
            {
                source.currentHeat += weapon.itemData.shot.shotHeat;
                source.currentEnergy += weapon.itemData.shot.shotEnergy;
                // Bots dont have matter

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
        foreach(var E in weapon.itemData.itemEffects)
        {
            if (E.heatTransfer != 0)
            {
                level = E.heatTransfer;
            }
        }

        // The item "Mak. Microdissipator Network" (and possibly a few other items) reduce heat transfer by 1.
        foreach (var I in Action.CollectAllBotItems(victim)) // I don't like having to do this. 99% of the time this is a waste of CPU 
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if(E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.AblativeBroad)
                {
                    level--;
                }
            }
        }

        if (level <= 0) // No heat transfer? Exit
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
            heat = Mathf.RoundToInt(heat * 0.37f); // 37%
        }
        else if(level == 3)
        {
            heat = Mathf.RoundToInt(heat * 0.50f); // 50%
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

    public static void UnstableWeaponConsequences(Actor owner, Item weapon)
    {

    }

    public static bool HasResourcesToAttack(Actor attacker, Item weapon)
    {
        int matter = 0;
        int energy = 0;

        int cost_matter = 0;
        int cost_energy = 0;

        if (attacker.botInfo) // Bot
        {
            matter = 999; // Bots dont have matter
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

    /// <summary>
    /// Goes through the inventory of a bot and returns a list of all items, active or not [Doesn't include the parent's inventory unless told to].
    /// </summary>
    /// <param name="actor">The actor to search through.</param>
    /// <param name="includeInventory">If set as true, and the target actor is the player, will also include the player's inventory.</param>
    /// <returns></returns>
    public static List<Item> CollectAllBotItems(Actor actor, bool includeInventory = false)
    {
        List<Item> items = new List<Item>();

        // Collect up all the items
        if (actor.botInfo) // Bot
        {
            foreach (var item in actor.armament.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }

            foreach (var item in actor.components.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }
        }
        else // Player
        {
            foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_power.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_propulsion.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }

            foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>().inv_weapon.Container.Items)
            {
                if (item.item.Id >= 0 && !item.item.isDuplicate)
                {
                    items.Add(item.item);
                }
            }

            if (includeInventory) // Include the player's inventory
            {
                foreach (InventorySlot item in PlayerData.inst.GetComponent<PartInventory>()._inventory.Container.Items)
                {
                    if (item.item.Id >= 0 && !item.item.isDuplicate)
                    {
                        items.Add(item.item);
                    }
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
            if(item.Id >= 0 && item.itemData.slot == slot && !item.isDuplicate)
            {
                found.Add(item);
            }
        }

        return found[Random.Range(0, found.Count - 1)];
    }

    public static void ChainReactionExplode(Actor source, Item item)
    {
        // Create message(s)
        string name = "";
        if (source.botInfo)
        {
            name = source.botInfo.botName;
        }
        else
        {
            name = "Cogmind";
        }
        UIManager.inst.CreateNewLogMessage(name + "'s engine has exploded due to a chain reaction.", UIManager.inst.cautiousYellow, UIManager.inst.slowOrange, false, false);
        UIManager.inst.CreateNewCalcMessage(item.itemData.itemName + " explodes due to a chain reaction.", UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);

        // Create an explosion
        Action.UnboundExplosion(item, HF.V3_to_V2I(source.transform.position));

        // Destroy the item
        Action.DestoyItem(source, item);
    }

    public static float CalculateAlienDamageTransfer(List<Item> items)
    {
        float amount = 0f;

        List<SimplifiedItemEffect> eList = new List<SimplifiedItemEffect>();
        foreach (Item item in items)
        {
            if(item.Id > -1 && !item.isDuplicate)
            {
                foreach (var E in item.itemData.itemEffects)
                {
                    if (E.alienBonus.hasEffect && (E.alienBonus.allDamageToCore || E.alienBonus.singleDamageToCore)) // This shouldn't matter?
                    {
                        eList.Add(new SimplifiedItemEffect(0, E.alienBonus.amount, E.alienBonus.stacks, E.alienBonus.half_stacks));
                    }
                }
            }
        }

        int filler;
        (filler, amount) = HF.ParseEffectStackingValues(eList);

        return amount;
    }

    public static float CalculateSizeModifier(BotSize attackerSize, BotSize targetSize)
    {
        // Define the size modifier values for each size class
        Dictionary<BotSize, float> sizeModifiers = new Dictionary<BotSize, float>()
        {
            { BotSize.Tiny, 0.5f },
            { BotSize.Small, 0.75f },
            { BotSize.Medium, 1.0f },
            { BotSize.Large, 1.25f },
            { BotSize.Huge, 1.5f }
        };

        // Calculate the size modifiers for the attacker and target
        float attackerModifier = sizeModifiers[attackerSize];
        float targetModifier = sizeModifiers[targetSize];

        // Calculate the difference between the modifiers
        float difference = attackerModifier - targetModifier;

        // Calculate the final size multiplier with a +/-10% difference
        float finalMultiplier = 1.0f + (difference * 0.1f);

        return finalMultiplier;
    }

    public static int CalculateDrag(Actor actor)
    {
        /* Inactive non-airborne propulsion modify the movement time cost by this amouunt while airborne. 
         * However, inactive propulsion has no adverse effective on the speed of non-airborne propulsion, including core movement.
         */

        int drag = 0;

        List<Item> items = Action.CollectAllBotItems(actor);

        foreach (var I in items)
        {
            if (I.itemData.propulsion[0].timeToMove > 0)
            {
                // Is this part inactive?
                if (!I.state)
                {
                    // Is this part non-airborne?
                    if(I.itemData.type != ItemType.Hover && I.itemData.type != ItemType.Flight)
                    {
                        // Add its drag amount
                        drag += I.itemData.propulsion[0].drag;
                    }
                }
            }
        }

        return drag;
    }

    /// <summary>
    /// Temporarily disable a specific item for a specific time. Comes with indications on the UI and whatnot.
    /// </summary>
    /// <param name="source">The actor who holds this item.</param>
    /// <param name="item">The item to disable.</param>
    /// <param name="duration">The time this item should be disabled.</param>
    public static void TemporarilyDisableItem(Actor source, Item item, int duration)
    {
        if (item == null || item.Id <= 0 || item.disabledTimer > 0) { return; } // Failsafe

        // If the player owns it we need to update the UI, if not its a bit simpler
        if (source.GetComponent<PlayerData>())
        {
            // Log a message
            string message = "";
            switch (item.itemData.slot)
            {
                case ItemSlot.Power:
                    message = $"Power failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                case ItemSlot.Propulsion:
                    message = $"Propulsion failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                case ItemSlot.Utilities:
                    message = $"Device failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                case ItemSlot.Weapons:
                    message = $"Weapon failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                case ItemSlot.Inventory:
                    message = $"Item failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                case ItemSlot.Other:
                    message = $"Item failure, {item.itemData.itemName} shutdown.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, false);
                    break;
                default:
                    break;
            }

            // Update the item's timer internally
            item.disabledTimer = duration;

            // We need to find the specific InvDisplayItem which holds the item and tell it to animate
            InvDisplayItem element = null;
            foreach (var I in InventoryControl.inst.interfaces)
            {
                if (I.GetComponentInChildren<DynamicInterface>())
                {
                    foreach (var S in I.GetComponentInChildren<DynamicInterface>().slotsOnInterface)
                    {
                        InvDisplayItem reference = null;

                        if (S.Key.GetComponent<InvDisplayItem>().item != null)
                        {
                            reference = S.Key.GetComponent<InvDisplayItem>();
                            if (reference.item == item)
                            {
                                element = reference;
                            }
                        }
                    }
                }
            }
            // Tell it to animate
            element.UIForceDisabled(duration);
        }
        else // Bot owner
        {
            // Update the item's timer internally
            item.disabledTimer = duration;
        }
    }

    public static bool IsItemActionable(Item item)
    {
        return item.Id >= 0 && item.state && !item.isDuplicate && item.disabledTimer <= 0 && !item.isBroken;
    }
    #endregion

    #region END OF TURN (ENERGY / MATTER / HEAT)
    /// <summary>
    /// Calculates the End of Turn amounts for Energy, Matter, and Heat for a specific bot.
    /// </summary>
    /// <param name="actor">The actor being focused on.</param>
    public static void DoEndOfTurn_EMH(Actor actor)
    {
        // Gather up all items so we can use them in each function
        List<Item> allItems = Action.CollectAllBotItems(actor);

        // Matter first
        Action.DoMatterUpkeep(actor, allItems);
        // Then heat since it could create some surplus energy
        int surplusEnergy = Action.CalculateBotHeat(actor, allItems);
        // Finally do energy
        Action.DoEnergyUpkeep(actor, allItems, surplusEnergy);

        // Ensure to clamp matter & energy
        if (actor.GetComponent<PlayerData>())
        {
            int e = PlayerData.inst.currentEnergy;
            int m = PlayerData.inst.currentMatter;

            int diff_e = PlayerData.inst.currentEnergy - PlayerData.inst.maxEnergy;
            int diff_m = PlayerData.inst.currentMatter - PlayerData.inst.maxMatter;

            if(diff_e > 0 || diff_m > 0) // Too much
            {
                // But what if the player has internal storage?
                if(PlayerData.inst.GetComponent<PartInventory>().inv_utility.Container.Items.Length > 0)
                {
                    foreach (var I in allItems)
                    {
                        if(Action.IsItemActionable(I) && I.itemData.slot == ItemSlot.Utilities && (diff_e > 0 || diff_m > 0))
                        {
                            foreach (var E in I.itemData.itemEffects)
                            {
                                if (E.internalStorageEffect.hasEffect && I.storageAmount < E.internalStorageEffect.capacity)
                                {
                                    if(diff_e > 0 && E.internalStorageEffect.internalStorageType == 1) // Energy
                                    {
                                        // Try to add surplus
                                        if(diff_e + I.storageAmount > E.internalStorageEffect.capacity)
                                        { // There will be extra, add all we can
                                            diff_e -= (E.internalStorageEffect.capacity - I.storageAmount);
                                            I.storageAmount = E.internalStorageEffect.capacity;
                                        }
                                        else
                                        { // No extra, add it all!
                                            I.storageAmount += diff_e;
                                            diff_e = 0;
                                            break;
                                        }
                                    }
                                    else if (diff_m > 0 && E.internalStorageEffect.internalStorageType == 0) // Matter
                                    {
                                        // Try to add surplus
                                        if (diff_m + I.storageAmount > E.internalStorageEffect.capacity)
                                        { // There will be extra, add all we can
                                            diff_m -= (E.internalStorageEffect.capacity - I.storageAmount);
                                            I.storageAmount = E.internalStorageEffect.capacity;
                                        }
                                        else
                                        { // No extra, add it all!
                                            I.storageAmount += diff_m;
                                            diff_m = 0;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // To cap it off, clamp to the normal values
            PlayerData.inst.currentEnergy = Mathf.Clamp(PlayerData.inst.currentEnergy, 0, PlayerData.inst.maxEnergy);
            PlayerData.inst.currentMatter = Mathf.Clamp(PlayerData.inst.currentMatter, 0, PlayerData.inst.maxMatter);
        }
        else
        {
            int e = actor.currentEnergy;
            int diff_e = actor.currentEnergy - actor.maxEnergy;
            // Bots don't have matter

            if(diff_e > 0)
            {
                // Although highly unlikely, we need to check to see if the bot has internal energy storage
                if (actor.components.Container.Items.Length > 0)
                {
                    foreach (var I in actor.components.Container.Items)
                    {
                        if (Action.IsItemActionable(I.item) && I.item.itemData.slot == ItemSlot.Utilities && diff_e > 0)
                        {
                            foreach (var E in I.item.itemData.itemEffects)
                            {
                                if (E.internalStorageEffect.hasEffect && I.item.storageAmount < E.internalStorageEffect.capacity)
                                {
                                    if (diff_e > 0 && E.internalStorageEffect.internalStorageType == 1) // Energy
                                    {
                                        // Try to add surplus
                                        if (diff_e + I.item.storageAmount > E.internalStorageEffect.capacity)
                                        { // There will be extra, add all we can
                                            diff_e -= (E.internalStorageEffect.capacity - I.item.storageAmount);
                                            I.item.storageAmount = E.internalStorageEffect.capacity;
                                        }
                                        else
                                        { // No extra, add it all!
                                            I.item.storageAmount += diff_e;
                                            diff_e = 0;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // To cap it off, clamp to the normal values
            actor.currentEnergy = Mathf.Clamp(actor.currentEnergy, 0, actor.maxEnergy);
        }

        // Update the UI
        UIManager.inst.UpdatePSUI();
    }

    /// <summary>
    /// Calculates the end of turn energy usage from items and innate production for a single actor.
    /// </summary>
    /// <param name="actor">The actor being focused on.</param>
    /// <param name="surplus">Any surplus energy that was just created by some heat related item.</param>
    public static void DoEnergyUpkeep(Actor actor, List<Item> allItems, int surplus)
    {
        float energy_consumed = 0; // From items that have something listed on their "upkeep" section (will be negative)
        float energy_created = 0; // From engines

        foreach (var I in allItems)
        {
            if (Action.IsItemActionable(I))
            {
                energy_consumed += I.itemData.energyUpkeep;
                energy_created += I.itemData.supply;
            }
        }

        // Then get natural power generation
        float energy_innate = 0f;
        if (actor.GetComponent<PlayerData>())
        {
            energy_innate = PlayerData.inst.innateEnergyProduction;
        }
        else // Bots
        {
            energy_innate = actor.energyGeneration;
        }

        // Add up the totals, including any surplus power generated from heat
        float total = energy_consumed + energy_created + energy_innate + surplus;

        // Then modify the current energy based on what we got
        if (actor.GetComponent<PlayerData>())
        {
            PlayerData.inst.currentEnergy += (int)total;
        }
        else
        {
            actor.currentEnergy += (int)total;
        }
    }

    /// <summary>
    /// Calculates the end of turn matter usage from unique (non weapon) sources.
    /// </summary>
    /// <param name="actor">The actor being focused on.</param>
    public static void DoMatterUpkeep(Actor actor, List<Item> allItems)
    {
        // Not that many things passively consume matter
        // -Some Utility Devices
        // -One or Two power cores

        float total = 0;
        foreach (var I in allItems)
        {
            if(Action.IsItemActionable(I))
            {
                if(I.itemData.slot == ItemSlot.Power || I.itemData.slot == ItemSlot.Utilities)
                {
                    if(I.itemData.matterUpkeep != 0)
                    {
                        total += I.itemData.matterUpkeep;
                    }
                }
            }
        }

        // Change the value
        if (actor.GetComponent<PlayerData>())
        {
            ModifyPlayerMatter((int)total);
        }
        else
        {
            // Bots dont have matter
        }
    }

    #region Heat
    /// <summary>
    /// Here we calculate the net heat a single bot produces at the end of its turn. 
    /// Does not include weapon firing costs, but will take into account existing heat (which could come from a weapon which was just fired).
    /// </summary>
    /// <param name="actor">The bot we are trying to calculate the heat of.</param>
    /// <returns>Returns surplus energy created from some specific heat items if that occurs.</returns>
    public static int CalculateBotHeat(Actor actor, List<Item> allItems)
    {
        // Get the heat the bot already has
        int startingHeat = 0; // The heat the bot is starting with this turn
        if (actor.GetComponent<PlayerData>())
        {
            startingHeat = actor.GetComponent<PlayerData>().currentHeat;
        }
        else
        {
            startingHeat = actor.currentHeat;
        }

        int surplusEnergy = 0; // Surplus energy generated from heat (needs specific items but can be returned at then)

        #region Explainer
        /* (Retrieved from the Cogmind BETA 14 manual: https://www.gridsagegames.com/cogmind/manual.txt)
        -----------------------------------------------------------------
            Heat
        -----------------------------------------------------------------

        Movement, firing weapons, and running power sources and some utilities generates heat, as does being hit by thermal weapons. 
        Some of this heat is naturally dissipated by the core's own heat sinks, but not enough to deal with heat generated by numerous/large energy weapons. 
        Heat sinks and cooling systems can be used to avoid overheating, which can have a wide range of negative effects. 
        Heat is only a significant issue for robots that rely heavily on energy weapons. 
        However, note that when firing a volley the heat produced is averaged over the volley's turn duration rather than being immediately applied all at once.

         <> Side Effects <>
        --------------
        Once heat reaches the "Hot" level (200+), active utilities and weapons may be temporarily disabled. 
        At "Warning" levels (300+) power sources are likely to shut down. 
        Many more serious (and permanent) effects are possible, especially at higher heat levels.

        Disabled power sources automatically restart when possible, but other parts must be manually reactivated.

        Heat effects are not calculated until after the dissipation phase, so heat can temporarily spike
        very high with no side effects as long as there are sufficient utilities to dissipate it.

         <> Cooling Resolution <>
        --------------------
        Eventually you will discover a range of different mechanics that factor into heat management, 
        especially with regard to cooling parts, thus knowing the specific order of operations between them may be of help for min-maxers seeking to optimize a build. 
        Below is a list of heat-related processes that play out once per turn. 
        Note that you most likely DO NOT need to know this stuff, but it may help answer a few specific questions min-maxers have about prioritization.

        1. Add heat from all sources, including that generated by active attached parts, gradual heat from an ongoing weapon volley, and any ambient heat. 
           Spread combined volley heat across its duration, in other words a 3-turn volley which according to its weapon properties generates 100 heat adds 33 heat per turn until complete.

        2. If used the unique "ITN" artifact, automatically drop heat to 250 if above that value.

        3. Subtract innate heat dissipation.

        4. Subtract heat dissipation from direct cooling utilities such as Heat Sinks and Cooling Systems.

        5. If heat exceeds 200, apply dissipation effects of disposable cooling systems such as Coolant Injectors, 
           up to that threshold. If more disposable systems than necessary, apply them in random order.

        6. If heat exceeds 200, apply self-damaging ablative cooling systems such as Mak. Ablative Armor. 
           If using multiple ablative systems at once, dissipation is split into equal-sized chunks for each, 
           where if one system fails, the entire chunk it is responsible for is still successfully dissipated.

        7. If heat exceeds 200, apply broad-effect ablative cooling systems such as Mak. Microdissipator Network.
         */
        #endregion

        // * HUD shows full heat breakdown including stationary upkeep, when mobile, and injector/ablative cooling *
        // As we go, we want to calculate the following things for the end
        // 1) The total heat the player ends up with
        // 2) Heat reduction when not moving
        // 3) Heat reduction when moving
        // 4) Heat reduction from items that lose HP as their function
        float reduction1 = 0, reduction2 = 0, reduction3 = 0;
        float moveCost = 0f;

        float totalHeat = startingHeat;

        // - Firstly, gather up all items that produce cooling, and all items that naturally eminate heat.
        List<Item> items_cold = new List<Item>();
        List<Item> items_hot = new List<Item>();

        foreach (var I in allItems)
        {
            if (Action.IsItemActionable(I)) // Active items only
            {
                foreach (var E in I.itemData.itemEffects)
                {
                    if (E.heatDissipation.hasEffect) // Cooling
                    {
                        items_cold.Add(I);
                    }
                    else if (I.itemData.heatUpkeep > 0 && I.itemData.slot != ItemSlot.Weapons) // Heat making (it seems that anything that makes heat is put under the "heat upkeep" section and doesn't have a separate effect)
                    { // We don't include weapons because their upkeep is part of their usage cost not a per turn thing
                        items_hot.Add(I);
                    }
                }

                // While we're here, calculate the movement cost
                if(I.itemData.slot == ItemSlot.Propulsion)
                {
                    moveCost += I.itemData.heatUpkeep;
                }
            }

            // IMPORTANT NOTE: For efficiencies sake, we are going to handle the disabled item countdown here so we don't have to add another item search loop in
            if(I.disabledTimer > 0)
            {
                I.disabledTimer--;
            }
        }

        // 0. Before we get going, we need to clamp the heat to 0 if its negative
        if(totalHeat < 0)
        {
            totalHeat = 0;
        }

        // 1. Add heat from all sources, including that generated by active attached parts, gradual heat from an ongoing weapon volley, and any ambient heat.
        //    Spread combined volley heat across its duration, in other words
        //    a 3 - turn volley which according to its weapon properties generates 100 heat adds 33 heat per turn until complete.

        // a) Active attached parts
        float heat_parts = 0;
        foreach (var I in items_hot)
        {
            heat_parts += I.itemData.heatUpkeep;
        }
        // b) Gradual (volley) heat // TODO NOTE: Volley logic isn't actually in yet, when it is, make sure to divide heat evenly.
        float heat_gradual = 0;
        if(actor.residualHeat.Count > 0)
        {
            heat_gradual = actor.residualHeat[0]; // Add heat from first in list
            actor.residualHeat.RemoveAt(0); // Then remove that from the list
        }
        // c) Ambient heat
        float heat_ambient = MapManager.inst.ambientHeat;

        // Then begin to add it all up
        totalHeat += heat_parts + heat_gradual + heat_ambient;

        // 1.5 Consider energy generation from Heat
        bool once = true;
        foreach (var I in items_cold)
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if(E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.EnergyGeneration)
                {
                    ItemHeatDissipation d = E.heatDissipation;
                    
                    if(once || d.parallel)
                    {
                        once = false;

                        int gen = d.energyGeneration;
                        int ratio = d.heatToEnergyAmount;

                        surplusEnergy += (int) (totalHeat / ratio) * gen;
                    }
                }
            }
        }

        // 2. If used the unique "ITN" artifact, automatically drop heat to 250 if above that value.
        int artifact_heat_dissipation = 0; // Also get this here while we're at it
        if (actor.GetComponent<PlayerData>() && PlayerData.inst.artifacts_used.Count > 0)
        {
            foreach (var A in PlayerData.inst.artifacts_used)
            {
                foreach (var E in A.itemData.itemEffects)
                {
                    if (E.alienBonus.id_effect) // Collect heat dissipation effect
                    {
                        artifact_heat_dissipation += E.alienBonus.id_heatDissipationValue;
                    }
                    else if (E.alienBonus.itn_effect) // ITN heat conversion
                    { // Converts all heat above 250 to energy every turn at a 3:1 ratio. <consumed> Example: At 300 heat precisely 50 heat would be used to generate 150 energy ((300-250)*3=150).
                        if (totalHeat > E.alienBonus.itn_value)
                        {
                            float diff = totalHeat - E.alienBonus.itn_value; // Calculate difference
                            totalHeat = E.alienBonus.itn_value; // Set to value

                            // Generate some energy
                            surplusEnergy += (int)(diff * 3);
                        }
                    }
                }
            }
        }

        // 3. Subtract innate heat dissipation.
        int innateDissipation = 0;
        if (actor.GetComponent<PlayerData>())
        {
            innateDissipation = actor.GetComponent<PlayerData>().naturalHeatDissipation;
        }
        else
        {
            innateDissipation = actor.heatDissipation;
        }
        totalHeat -= innateDissipation;

        // 4. Subtract heat dissipation from direct cooling utilities such as Heat Sinks and Cooling Systems.
        bool stackable = true;
        int directDissipation = 0;
        foreach (var I in items_cold.ToList())
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if(E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.Direct) // Only want direct cooling
                {
                    ItemHeatDissipation d = E.heatDissipation;

                    if (d.stacks || stackable)
                    {
                        stackable = false;
                        directDissipation += d.dissipationPerTurn;
                    }
                }
            }
        }
        totalHeat -= directDissipation;

        // Calculate first heat reduction mark here
        reduction1 = heat_ambient + artifact_heat_dissipation + innateDissipation + directDissipation;
        // Then calculate the 2nd based on movement
        reduction2 += moveCost;

        float dissipationByDamage = 0f;

        // 5. If heat exceeds X, apply dissipation effects of disposable cooling systems such as Coolant Injectors, 
        //    up to that threshold. If more disposable systems than necessary, apply them in random order.
        stackable = true;
        foreach (var I in items_cold.ToList())
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if (E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.Disposable && totalHeat > E.heatDissipation.minTempToActivate)
                {
                    ItemHeatDissipation d = E.heatDissipation;

                    if (d.stacks || stackable)
                    {
                        stackable = false;
                        totalHeat -= d.dissipationPerTurn;
                        dissipationByDamage -= d.dissipationPerTurn;
                        // Damage the part
                        int damage = d.integrityLossPerTurn;
                        DamageItem(actor, I, damage);
                    }
                }
            }
        }


        // 6. If heat exceeds X, apply self-damaging ablative cooling systems such as Mak. Ablative Armor. 
        //    If using multiple ablative systems at once, dissipation is split into equal - sized chunks for each,
        //    where if one system fails, the entire chunk it is responsible for is still successfully dissipated.
        stackable = true;
        // - First off, collect up all VALID ablative items (that also meet the temp threshold)
        List<KeyValuePair<Item, ItemHeatDissipation>> abla_wide = new List<KeyValuePair<Item, ItemHeatDissipation>>();
        foreach (var I in items_cold.ToList())
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if (E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.AblativeIndividual && totalHeat > E.heatDissipation.minTempToActivate)
                {
                    ItemHeatDissipation d = E.heatDissipation;

                    abla_wide.Add(new KeyValuePair<Item, ItemHeatDissipation>(I, d));
                }
            }
        }
        // - Then consider the multi item scenario (99% of the time this won't happen but we gotta be thorough)
        if(abla_wide.Count > 1) // Multi scenario, spread equally
        {
            foreach (var I in abla_wide)
            {
                if (I.Value.stacks || stackable)
                {
                    stackable = false;
                    totalHeat -= I.Value.dissipationPerTurn;
                    dissipationByDamage -= I.Value.dissipationPerTurn;
                    // Damage the part
                    int damage = I.Value.integrityLossPerTurn / abla_wide.Count; // Spread it out equally
                    DamageItem(actor, I.Key, damage);
                }
            }
        }
        else if(abla_wide.Count > 0) // Only 1 simple stuff
        {
            // Should this try and dissipate all at once? Or only in 1 amount of what it can do per turn? Lets go with the 2nd option.
            totalHeat -= abla_wide[0].Value.dissipationPerTurn; // Dissipate heat
            dissipationByDamage -= abla_wide[0].Value.dissipationPerTurn;
            DamageItem(actor, abla_wide[0].Key, abla_wide[0].Value.integrityLossPerTurn); // Damage the item
        }

        // 7. If heat exceeds X, apply broad-effect ablative cooling systems such as Mak. Microdissipator Network.
        stackable = true;
        foreach (var I in items_cold.ToList())
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if (E.heatDissipation.hasEffect && E.heatDissipation.type == HeatDissipationType.AblativeBroad && totalHeat > E.heatDissipation.minTempToActivate)
                {
                    ItemHeatDissipation d = E.heatDissipation;

                    if (d.stacks || stackable)
                    {
                        stackable = false;
                        totalHeat -= d.dissipationPerTurn;
                        dissipationByDamage -= d.dissipationPerTurn;
                        int damage = d.ablativeDamage;
                        int chunks = d.ablativeChunks;

                        // Say goodbye to your parts blockhead
                        for (int i = 0; i < chunks; i++)
                        {
                            DamageItem(actor, allItems[Random.Range(0, allItems.Count - 1)], damage);
                        }
                    }
                }
            }
        }

        // Calculate the last reduction here
        reduction3 = dissipationByDamage;

        // 8. Clamp the heat again (if its negative), and then apply items that LOWER the base heat
        if (totalHeat < 0)
        {
            totalHeat = 0;
        }
        foreach (var I in items_cold)
        {
            foreach (var E in I.itemData.itemEffects)
            {
                if (E.heatDissipation.hasEffect)
                {
                    totalHeat += E.heatDissipation.lowerBaseTemp;
                }
            }
        }

        // 9. Consider overheating consequences
        Action.ConsiderOverheatingConsequences(actor);

        // 10. Update the bots values
        if (actor.GetComponent<PlayerData>())
        {
            PlayerData.inst.currentHeat = (int)totalHeat;
            PlayerData.inst.heatRate1 = -reduction1;
            PlayerData.inst.heatRate2 = -reduction2;
            PlayerData.inst.heatRate3 = -reduction3;
        }
        else
        {
            actor.currentHeat = (int)totalHeat;
        }

        return surplusEnergy; // Return any surplus energy created
    }

    /// <summary>
    /// Called immedietly after heat calculations. Getting too hot can have consequences. This function determines if something happens for a single bot.
    /// </summary>
    /// <param name="actor">The bot to examine overheating for.</param>
    public static void ConsiderOverheatingConsequences(Actor actor)
    {
        #region Explanation
        /*
        --------------
         <> Side Effects <>
        --------------
        Once heat reaches the "Hot" level (200+), active utilities and weapons may be temporarily disabled. 
        At "Warning" levels (300+) power sources are likely to shut down. 
        Many more serious (and permanent) effects are possible, especially at higher heat levels.

        Disabled power sources automatically restart when possible, but other parts must be manually reactivated.

        Heat effects are not calculated until after the dissipation phase, so heat can temporarily spike
        very high with no side effects as long as there are sufficient utilities to dissipate it.
         */
        #endregion

        // Get the current heat level
        int heat = 0;

        if (actor.GetComponent<PlayerData>())
        {
            heat = PlayerData.inst.currentHeat;
        }
        else
        {
            heat = actor.currentHeat;
        }

        // Consequences
        if(heat >= 200 && heat < 300) // "Hot"
        {
            // We are going to gather up all ACTIVE utilities & weapons.
            List<Item> items = Action.CollectAllBotItems(actor);
            List<Item> sorted = new List<Item>();

            foreach (var I in items)
            {
                if (Action.IsItemActionable(I) && (I.itemData.slot == ItemSlot.Utilities || I.itemData.slot == ItemSlot.Weapons))
                {
                    sorted.Add(I);
                }
            }

            // Since the way this functions in the game are not written down in the manual, i'm just going to wing it here.
            foreach (var I in sorted) // We're just going to go through every item, and roll randomly (low) if to disable it or not
            {
                int rand = Random.Range(0, 100);
                if(rand <= 10)
                {
                    Action.TemporarilyDisableItem(actor, I, Random.Range(5,9));
                }
            }
        }
        else if (heat >= 300) // "Warning"
        {
            // We are going to gather up all ACTIVE utilities & weapons. Along with power sources (separately).
            List<Item> items = Action.CollectAllBotItems(actor);
            List<Item> power = new List<Item>();
            List<Item> util_wep = new List<Item>();

            foreach (var I in items)
            {
                if (Action.IsItemActionable(I)) // Active and not disabled already
                {
                    if (I.itemData.slot == ItemSlot.Utilities || I.itemData.slot == ItemSlot.Weapons)
                    {
                        util_wep.Add(I);
                    }
                    else if (I.itemData.slot == ItemSlot.Power)
                    {
                        power.Add(I);
                    }
                }
            }

            // First go for the power sources
            foreach (var P in power)
            {
                int rand = Random.Range(0, 100);
                if (rand <= Action.COC_MathHelper(heat)) // Random % based on a curve. For power sources its higher
                {
                    Action.TemporarilyDisableItem(actor, P, Random.Range(5, 9));
                }
            }

            // Then go through utility & weapon items but lower the percentage, with the consequence of doing damage too.
            foreach (var I in util_wep)
            {
                int rand = Random.Range(0, 100);
                if (rand <= Action.COC_MathHelper(heat) + 50) // Random % based on a curve. Lower than power sources
                {
                    Action.DamageItem(actor, I, Action.COC_MathHelper2(heat)); // Since its less likely to happen, we will deal damage too (this is gonna hurt)
                    Action.TemporarilyDisableItem(actor, I, Random.Range(5, 9));
                }
            }

            // Bonus consequences for higher heat level
            if(heat >= 400)
            {
                // Corruption!
                if(Random.Range(0, 100) <= 15) // 15% for corruption
                {
                    ModifyPlayerCorruption(1);
                    // Play a message
                    string message = "Core corrupted due to overheating.";
                    UIManager.inst.CreateNewLogMessage(message, UIManager.inst.corruptOrange, UIManager.inst.corruptOrange_faded, false, true);
                }

                // Damage a (random) power source
                if(Random.Range(0, 100) <= 25) // 25% chance
                {
                    Action.DamageItem(actor, power[Random.Range(0, power.Count - 1)], Action.COC_MathHelper2(heat));
                }

                // Possible to add more in the future when needed
            }
        }

        // Do the warning logic here, since this is variable through the player's settings
        if(heat >= GlobalSettings.inst.heatWarningLevel)
        {
            // - Play a sound (with a cooldown so we don't spam it)
            PlayerData.inst.OverheatWarning();
            // - Flash the heat backer bar
            // (This is handled in UIManager)
        }
    }

    private static float COC_MathHelper(int heat)
    {
        // Cap the percentage between 0 and 100
        if (heat < 300)
            return 0;
        if (heat >= 500)
            return 100;

        // Use a piecewise linear approximation
        if (heat >= 300 && heat < 350)
            return 50 + (20.0f / 50.0f) * (heat - 300);  // Linear interpolation between 50% and 70%
        else if (heat >= 350 && heat < 400)
            return 70 + (15.0f / 50.0f) * (heat - 350);  // Linear interpolation between 70% and 85%
        else
            return 85 + (15.0f / 100.0f) * (heat - 400); // Linear interpolation between 85% and 100%
    }

    private static int COC_MathHelper2(int heat)
    {
        if (heat < 300)
            return Random.Range(1, 4); // Default to the range 1 to 3 if less than 300

        if (heat >= 500)
            return Random.Range(10, 21); // Cap the range at 10 to 20 if 500 or more

        if (heat >= 300 && heat < 350)
            return Random.Range(1, 4 + (heat - 300) * 2 / 50); // Linear interpolation from 1-3 to 3-7

        if (heat >= 350 && heat < 400)
            return Random.Range(3, 5 + (heat - 350) * 6 / 50); // Linear interpolation from 3-7 to 5-11

        if (heat >= 400 && heat < 450)
            return Random.Range(5, 7 + (heat - 400) * 8 / 50); // Linear interpolation from 5-11 to 7-15

        if (heat >= 450 && heat < 500)
            return Random.Range(7, 10 + (heat - 450) * 10 / 50); // Linear interpolation from 7-15 to 10-20

        // This shouldn't be reached, but just in case
        return Random.Range(10, 21);
    }

    #endregion
    #endregion

    #region Basic Player Stat Changes
    // Storing them all here to simplify things, especially since we need to animate the bar.

    public static void ModifyPlayerCore(int amount)
    {
        if(amount == 0)
        {

        }
        else if(amount > 0)
        {
            UIManager.inst.Core_AnimateIncrease(amount);
        }
        else if (amount < 0)
        {
            UIManager.inst.Core_AnimateDecrease(amount);
        }

        PlayerData.inst.currentHealth += amount;
    }

    public static void ModifyPlayerMatter(int amount)
    {
        if (amount == 0)
        {

        }
        else if (amount > 0)
        {
            //UIManager.inst.Matter_AnimateIncrease(amount);
        }
        else if (amount < 0)
        {
            UIManager.inst.Matter_AnimateDecrease(amount);
        }

        PlayerData.inst.currentMatter += amount;

        // Clamp it for safety
        if (PlayerData.inst.currentMatter < 0)
            PlayerData.inst.currentMatter = 0;
    }

    public static void ModifyPlayerEnergy(int amount)
    {
        if (amount == 0)
        {

        }
        else if (amount > 0)
        {
            //UIManager.inst.Energy_AnimateIncrease(amount);
        }
        else if (amount < 0)
        {
            UIManager.inst.Energy_AnimateDecrease(amount);
        }

        PlayerData.inst.currentEnergy += amount;

        // Clamp it for safety
        if (PlayerData.inst.currentEnergy < 0)
            PlayerData.inst.currentEnergy = 0;
    }

    /// <summary>
    /// Atempt to modify the player's corruption value by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the player's corruption by.</param>
    public static void ModifyPlayerCorruption(int amount)
    {
        if(amount == 0)
        {

        }
        else if (amount > 0)
        {
            // Player may be immune to corruption
            if (!PlayerData.inst.rif_immuneToCorruption)
            {
                UIManager.inst.Corruption_AnimateIncrease(amount);
            }
        }
        else if (amount < 0)
        {
            // No animation for corruption decrease
        }

        PlayerData.inst.currentCorruption += amount;
    }

    #endregion
}
