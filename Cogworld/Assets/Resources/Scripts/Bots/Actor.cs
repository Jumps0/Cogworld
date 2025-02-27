using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Actor : Entity
{
    public bool isAlive = false;

    [Tooltip("Field of view range.")]
    public int fieldOfViewRange = 15; // COGMIND default is 15.

    [Tooltip("All tiles that are currently visible.")]
    [SerializeField] private List<Vector3Int> fieldOfView;

    [Header("Details")]
    public BotObject botInfo;
    public Allegance allegances;
    public BotAlignment myFaction;
    public BotClassRefined _class = BotClassRefined.None;
    public bool wasFabricated = false;

    AdamMilVisibility algorithm;

    public int Initiative = 0;

    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public List<Vector3Int> FieldofView { get => fieldOfView; }

    private Color baseColor;
    [SerializeField] private SpriteRenderer _sprite;
    [HideInInspector] public BoxCollider2D _collider;

    [Header("Special Bot States")]
    [Tooltip("DORMANT: Robots temporarily resting in this mode are only a problem if they wake up, which may be triggered by alarm traps, " +
        "being alerted by their nearby allies, or by hostiles hanging around in their field of vision.")]
    public bool state_DORMANT = false; // Grayed-out orange
    [Tooltip("This bot once had weapons but has since lost them and cannot fight. Has a different sprite.")]
    public bool state_DISARMED = false; // Grayed-out green
    [Tooltip("This bot is cloaked and is moving in stealth, but through one reason or another the player knows their position. Has a different sprite")]
    public bool state_CLOAKED = false; // Grayed-out blue
    [Tooltip("UNPOWERED: Unpowered robots will never become active. (Though yeah they’re very much real and you can harvest them for parts if you’d like.)")]
    public bool state_UNPOWERED = false; // Grayed-out red
    [Tooltip("DISABLED: Some robots have been temporarily disabled and put in garrison storage. Prime candidates for rewiring!")]
    public bool state_DISABLED = false; // Grayed-out normal

    [Header("Following Flags")]
    [Tooltip("If true, will be under the player's direct control, appearing in 'allies' tab.")]
    public bool directPlayerAlly = false;
    [Tooltip("If true, the bot will always try to be close to and fight with the player. Useful for non-allied but friendly bot followers.")]
    public bool followThePlayer = false;

    [Header("Conditions")]
    public List<ModHacks> hacked_mods = new List<ModHacks>();

    #region Setup
    public void Setup(BotObject info)
    {
        botInfo = info;
        if (botInfo)
        {
            baseColor = botInfo.idealColor;

            if (_sprite != null && botInfo.displaySprite != null)
            {
                // Auto-assign sprite
                _sprite.sprite = botInfo.displaySprite;
            }

            _collider = this.GetComponent<BoxCollider2D>();

            // Set up the other values while we're here
            heatDissipation = botInfo.heatDissipation;
            energyGeneration = botInfo.energyGeneration;
            fieldOfViewRange = botInfo.visualRange;
            maxHealth = botInfo.coreIntegrity;
        }
        else
        {
            baseColor = _sprite.color;
        }
        _sprite.color = baseColor;

        if (GameManager.inst)
        {
            TurnManager.inst.turnEvents.onTurnTick += TurnTick; // Begin listening to the turn tick event

            AddToGameManager();
            if (GetComponent<PlayerData>())
            {
                TurnManager.inst.InsertActor(this, 0);
            }
            else
            {
                TurnManager.inst.AddActor(this);
                fow_sprite.color = Color.black; // temp fix

                currentHealth = maxHealth;

                StartCoroutine(SetBotName());

                // Create new inventories
                armament = new InventoryObject(botInfo.armament.Count, botInfo.botName + "'s Armament");
                components = new InventoryObject(botInfo.components.Count, botInfo.botName + "'s Components");

                // Fill up armament & component inventories
                foreach (var item in botInfo.armament)
                {
                    armament.AddItem(item.item, 1);
                }
                foreach (var item in botInfo.components)
                {
                    components.AddItem(item.item, 1);
                }

                inventory = new InventoryObject(HF.CalculateMaxInventorySize(components), botInfo.botName + "'s Inventory");

                myFaction = botInfo.locations.alignment;
                uniqueName = botInfo.botName; // TODO: Fancy system for names (later)
            }

            algorithm = new AdamMilVisibility(this); // Set visual algo
            allegances = GlobalSettings.inst.GenerateDefaultAllengances(botInfo); // Set allegances
        }
        else
        {
            Debug.LogWarning("GameManager does not exist! Actor will not be initialized!");
        }
    }

    public void LateSetup()
    {
        if (GameManager.inst)
        {
            if (GetComponent<PlayerData>())
            {
                MapManager.inst.TilemapVisUpdate(); // One time vision update across the whole map (to set all the unseen tiles to black).
                UpdateFieldOfView(); // Normal FOV update
            }
            else // Here because PlayerData.inst hasn't been created yet (in Awake)
            {
                this.GetComponent<BotAI>().relationToPlayer = HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>()); // Set relation to player
            }
        }
    }

    private IEnumerator SetBotName()
    {
        while (!MapManager.inst.loaded)
        {
            yield return null;
        }

        string name = this.botInfo.botName;
        if (this.botInfo.botName.Contains("("))
        {
            // The name is most likely "Name (#)", we need to remove that second part
            string[] split = name.Split('(');
            name = split[0];
        }

        if (name.Contains("Assembled"))
        {
            name = "AS-" + UnityEngine.Random.Range(21111, 69999);
        }

        this.gameObject.name = name;
    }
    #endregion

    private void Update()
    {
        if (GameManager.inst)
        {
            HealthCheck();
            if (state_DISABLED && canDisabledCheck)
            {
                DisabledCheck();
            }
        }
    }

    #region Disabled Logic
    /// <summary>
    /// Disabled this bot, can be temporary or permanent.
    /// </summary>
    public void DisableThis(int _time = 0)
    {
        state_DISABLED = true;
        disabledTurn = TurnManager.inst.globalTime;

        // Only allow disabled countdown checking if a time has been set.
        disabledTime = _time;
        canDisabledCheck = _time >= 0;
    }

    private void DisabledCheck()
    {
        if (TurnManager.inst.globalTime >= disabledTurn + disabledTime)
        {
            state_DISARMED = false;
            canDisabledCheck = false;
            disabledTime = 0;
            disabledTurn = 0;
        }
    }

    private int disabledTime = 0;
    private int disabledTurn = 0;
    private bool canDisabledCheck = true;
    #endregion

    /*
    public void DoAction(int timeCost)
    {
        if (turnTime < timeCost)
        {
            Debug.LogWarning("Not enough time to perform action");
            return;
        }
        turnTime -= timeCost;
        Debug.Log(gameObject.name + " spends " + timeCost + " time units");
        if (turnTime <= 0)
        {
            GetComponent<TurnManager>().EndTurn(this);
        }
    }
    */

    #region Turns
    public void StartTurn()
    {
        if (this.GetComponent<BotAI>() != null)
        {
            this.GetComponent<BotAI>().isTurn = true;
            this.GetComponent<BotAI>().TakeTurn();
        }
    }

    public void EndTurn()
    {
        Action.DoEndOfTurn_EMH(this); // Do end of turn [ENERGY | MATTER | HEAT] calculations

        noMovementFor += 1; // should this be here?
        if (this.GetComponent<BotAI>() != null)
            this.GetComponent<BotAI>().isTurn = false;

        if (this.GetComponent<PlayerData>())
        { // Siege tracking
            int siegeTime = PlayerData.inst.timeTilSiege;
            if (siegeTime < 100)
            {
                if ((siegeTime <= 5 && siegeTime >= 1) || (siegeTime < 0 && siegeTime >= -5))
                {
                    PlayerData.inst.timeTilSiege--;
                }
                else if (siegeTime < -5)
                {
                    PlayerData.inst.timeTilSiege = 100;
                }
            }
        }
    }

    private void TurnTick()
    {

    }

    #endregion

    #region Navigation

    //finds an empty tile for this actor to move to
    public void FindNewTile()
    {
        List<WorldTile> neighbors = HF.FindNeighbors(((int)transform.localPosition.x), ((int)transform.localPosition.y));
        Vector2Int newgoal = Vector2Int.zero;
        foreach (WorldTile neighbor in neighbors)
        {
            if (HF.IsUnoccupiedTile(neighbor))
            {
                newgoal = neighbor.location;
            }
        }

        if (newgoal != Vector2Int.zero)
        {
            NewGoal(newgoal);
        }
        else
        {
            // Do nothing
            EndTurn();
        }
    }

    //set a new goal for this actor to seek
    public void NewGoal(Vector2Int goal)
    {
        /*
        if (this.GetComponent<PotentialField>() && this.GetComponent<PotentialField>().movementActive)
        {
            this.GetComponent<PotentialField>().setGoal(new Vector3(goal.x, goal.y, 0.0f));
        }
        */
    }

    //makes the actor stop moving
    public void DisableMovement()
    {
        /*
        if (this.GetComponent<PotentialField>())
        {
            this.GetComponent<PotentialField>().disableMovement();
        }
        */
    }

    //makes the actor start moving
    public void EnableMovement()
    {
        /*
        if (this.GetComponent<PotentialField>())
        {
            this.GetComponent<PotentialField>().enableMovement();
        }
        */
    }

    public void FollowLeader()
    {
        // !! Assumes that this bot is a hostile one !!

        NewGoal(HF.V3_to_V2I(this.GetComponent<BotAI>().squadLeader.transform.position));
            
        /*
        if (HF.IsUnoccupiedTile(MapManager.inst.mapdata[realPos.x, realPos.y]))
        {
            Debug.Log("Move successful");
            Action.MovementAction(this, moveToLocation);
        }
        else
        {
            //Debug.Log("Move failed {" + moveToLocation + "}");
            Action.SkipAction(this.GetComponent<Actor>());
        }
        */
    }

    public void MoveToPatrolPoint()
    {
        // !! Assumes that this bot is 1) Hostile 2) a Group Leader

        // If close to destination
        if (Vector2.Distance(this.transform.position, this.GetComponent<GroupLeader>().route[this.GetComponent<GroupLeader>().pointInRoute]) <= 3)
        {
            if (this.GetComponent<GroupLeader>().pointInRoute >= this.GetComponent<GroupLeader>().route.Count - 1)
            {
                this.GetComponent<GroupLeader>().pointInRoute = 0; // Loop
            }
            else
            {
                this.GetComponent<GroupLeader>().pointInRoute += 1; // Set next point
            }
        }

        Vector2Int goal = this.GetComponent<GroupLeader>().route[this.GetComponent<GroupLeader>().pointInRoute];
        NewGoal(goal);

        /*

        if (HF.IsUnoccupiedTile(MapManager.inst.mapdata[realPos.x, realPos.y]))
        {
            Action.MovementAction(this.GetComponent<Actor>(), moveToLocation);
        }
        else
        {
            Action.SkipAction(this.GetComponent<Actor>());
        }
        */
    }

    public Vector2 FindBestFleeLocation(GameObject target, GameObject entity, List<WorldTile> validMoveLocations)
    {
        Vector2 bestLocation = Vector2.zero;
        float furthestDistance = 0f;

        Vector2 entityPos = (Vector2)entity.transform.position;
        Vector2 targetPos = (Vector2)target.transform.position;

        // Calculate the current distance between the entity and the target
        float currentDistance = Vector2.Distance(entityPos, targetPos);

        // Loop through each valid move location
        foreach (WorldTile moveLocation in validMoveLocations)
        {
            // Calculate the distance between the move location and the target
            float distanceFromTarget = Vector2.Distance(targetPos, moveLocation.location);

            // Calculate the distance between the entity and the move location
            float distanceFromEntity = Vector2.Distance(entityPos, moveLocation.location);

            // Calculate the new distance between the entity and the target if the entity moves to this location
            float newDistance = Mathf.Max(currentDistance - distanceFromEntity, 0f) + distanceFromTarget;

            // If the new distance is greater than the furthest distance so far
            if (newDistance > furthestDistance)
            {
                // Set this location as the new best location and update the furthest distance
                bestLocation = moveLocation.location;
                furthestDistance = newDistance;
            }
        }

        // Return the best move location to flee to
        return bestLocation;
    }

    #endregion

    #region Combat
    [Tooltip("List of bots who have attacked this bot. Used for kill logging.")]
    public List<Actor> previousAttackers = new List<Actor>();

    public void HealthCheck()
    {
        if (this.GetComponent<PlayerData>())
        {
            if (PlayerData.inst.currentHealth <= 0 || corruption >= 1f)
            {
                // Die!
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            if (currentHealth <= 0) // CORE death
            {
                Die(new DeathType(previousAttackers[previousAttackers.Count - 1], DeathType.DTType.Core, CritType.Nothing));
            }
            else if (corruption >= 1f) // CORRUPTION death
            {
                Die(new DeathType(null, DeathType.DTType.Corruption, CritType.Nothing));
            }

            // Shoving this in here too
            state_DISARMED = this.armament.Container.Items.Length <= 0;
        }
    }

    public void HealthColorCheck(Color originColor)
    {
        // We are going to override this system for a few conditions (in cases where the bot is hostile)

        // >> Unpowered
        if (HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>()) == BotRelation.Hostile && state_UNPOWERED && originColor != Color.black)
        {
            // Faded out red
            _sprite.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.7f);
        } // >> Disarmed
        else if (HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>()) == BotRelation.Hostile && state_DISARMED && originColor != Color.black)
        {
            // Faded out green
            _sprite.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.7f);
        } // >> Cloaked
        else if (state_CLOAKED && originColor != Color.black)
        {
            // Faded out blue
            _sprite.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.7f);
        }  // Dormant
        else if (HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>()) == BotRelation.Hostile && state_DORMANT && originColor != Color.black)
        {
            // Faded out orange
            Color darkOrange = new Color(0.67f, 0.34f, 0.023f, 0.7f);
            _sprite.color = darkOrange;
        } // Disabled
        else if (HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>()) == BotRelation.Hostile && state_DISABLED && originColor != Color.black)
        {
            // Faded out normal color
            _sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f);
        }
        else
        {
            if (originColor != Color.black)
            {
                // Update the bot's color based on health
                // Calculate the amount of health as a percentage
                float healthPercent = currentHealth / maxHealth;

                // Interpolate between the start color and red based on the health percentage
                Color newColor = Color.Lerp(originColor, Color.red, Mathf.InverseLerp(1.0f, 0f, healthPercent));

                // Set the sprite's color to the interpolated color
                _sprite.color = newColor;
            }
            else
            {
                _sprite.color = originColor;
            }
        }
    }

    /// <summary>
    /// Kill this actor.
    /// </summary>
    /// <param name="deathInfo">Contains information regarding how this bot died.</param>
    public void Die(DeathType deathInfo)
    {
        Actor killer = deathInfo.killer;
        string overrideMessage = deathInfo.deathMessage;

        #region Kill Logging
        if (killer != null && killer.GetComponent<PlayerData>())
        {
            PlayerData.inst.robotsKilled += 1;
            PlayerData.inst.robotsKilledData.Add(this.botInfo);
            PlayerData.inst.robotsKilledAlignment.Add(this.myFaction);
        }
        #endregion

        #region Death Handling
        // Create the default message
        string botName = this.botInfo.botName;
        if (this.GetComponent<BotAI>().uniqueName != "")
            botName = this.GetComponent<BotAI>().uniqueName;

        switch (deathInfo.type)
        {
            case DeathType.DTType.Core: // Normal death

                // == Drop any remaining parts ==
                /* The chance for the robot's parts to survive, checked individually for each part, 
                 * is ([percent_remaining_integrity / 2] + [salvage_modifier]), thus more damaged parts are more likely to be 
                 * destroyed completely along with the robot. But even if that check succeeds, there are still two more possible 
                 * factors that may prevent a given part from dropping. If the robot has any residual heat, parts can be melted, 
                 * the chance of which is ([heat - max_integrity] / 4), so again less likely to affect large parts 
                 * (though still possible, especially at very high heat levels). If the robot was corrupted, parts can be "fried," 
                 * the chance of which is [system_corruption - max_integrity]; by the numbers, this will generally only affect 
                 * small electronic components like processors, and sometimes devices. Each salvageable part left by a corrupted 
                 * robot also has a corruption% chance to itself be corrupted, specifically by a random amount from 1 to (10*[corruption]/100), 
                 * and when attached will increase Cogmind's system corruption by the same value as well as possibly cause another side effect.

                   Note that robots built by a fabricator do not leave salvageable parts. 
                   Also, anything in a robot's inventory (not attached) is always dropped to the ground, regardless of salvage modifiers or other factors.
                 */
                if (!wasFabricated)
                {
                    List<Item> items = new List<Item>();

                    foreach (var item in this.armament.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }
                    foreach (var item in this.components.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }

                    foreach (var item in items)
                    {
                        if (item.Id >= 0)
                        {
                            // HP Check
                            float percentHP = item.integrityCurrent / item.itemData.integrityMax;
                            float chance = ((percentHP / 2) + salvageModifier);

                            if (Random.Range(0f, 1f) < chance) // Continue to next check
                            {
                                // Heat check
                                chance = ((currentHeat - item.itemData.integrityMax) / 4);

                                if (Random.Range(0f, 1f) < chance) // Success! Drop the item.
                                {
                                    InventoryControl.inst.DropItemOnFloor(item, this, null, Vector2Int.zero);
                                }
                            }
                        }
                    }
                }
                break;
            case DeathType.DTType.Corruption:

                // == Drop any remaining parts (/w corruption consideration) == 
                if (!wasFabricated)
                {
                    List<Item> items = new List<Item>();

                    foreach (var item in this.armament.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }
                    foreach (var item in this.components.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }

                    foreach (var item in items)
                    {
                        if (item.Id >= 0)
                        {
                            // HP Check
                            float percentHP = item.integrityCurrent / item.itemData.integrityMax;
                            float chance = ((percentHP / 2) + salvageModifier);

                            if (Random.Range(0f, 1f) < chance) // Continue to next check
                            {
                                // Heat check
                                chance = ((currentHeat - item.itemData.integrityMax) / 4);

                                if (Random.Range(0f, 1f) < chance) // Continue to next check
                                {
                                    // Corruption check
                                    /*
                                     * One of the big changes to EM damage in Beta 11 was of course the potential to corrupt the victim’s salvage, 
                                     * causing some amount of system corruption when attached. 
                                     * This may cause players to shy away from corrupting targets with desirable salvage, 
                                     * or avoid attaching quite as much of that salvage as they would before. 
                                     * Still, sometimes you just have to put on that extra corrupted part to survive, or maybe just don’t care :)
                                     */
                                    chance = ((100 * corruption) - item.itemData.integrityMax) / 100;

                                    if (Random.Range(0f, 1f) < chance) // Continue to next check
                                    {
                                        // Success! But will the item be corrupted?
                                        if (Random.Range(0f, 1f) < corruption)
                                        { // This should really be "1 to (10*[corruption]/100)" but I have no idea how that math works out
                                          // Corrupted!
                                            int calc = (10 * (int)(100 * corruption) / 100); // ex: This turns 60 -> 6
                                            item.corrupted = Random.Range(1, calc);
                                            InventoryControl.inst.DropItemOnFloor(item, this, null, Vector2Int.zero);
                                        }

                                        // Drop the item
                                        InventoryControl.inst.DropItemOnFloor(item, this, null, Vector2Int.zero);
                                    }
                                }
                            }
                        }
                    }
                }

                // Change death message
                overrideMessage = $"{this.botInfo.botName} system terminally corrupted.";

                // TODO: There is probably some other corruption death stuff here but im not sure what it is
                break;
            case DeathType.DTType.Critical:

                // !! Copied from Core death, need to verify if anything different happens here. !!
                if (!wasFabricated)
                {
                    List<Item> items = new List<Item>();

                    foreach (var item in this.armament.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }
                    foreach (var item in this.components.Container.Items.ToList())
                    {
                        items.Add(item.item);
                    }

                    foreach (var item in items)
                    {
                        if (item.Id >= 0)
                        {
                            // HP Check
                            float percentHP = item.integrityCurrent / item.itemData.integrityMax;
                            float chance = ((percentHP / 2) + salvageModifier);

                            if (Random.Range(0f, 1f) < chance) // Continue to next check
                            {
                                // Heat check
                                chance = ((currentHeat - item.itemData.integrityMax) / 4);

                                if (Random.Range(0f, 1f) < chance) // Success! Drop the item.
                                {
                                    InventoryControl.inst.DropItemOnFloor(item, this, null, Vector2Int.zero);
                                }
                            }
                        }
                    }
                }

                Color critColorM = UIManager.inst.activeGreen;
                Color critColorB = UIManager.inst.dullGreen;

                if (deathInfo.useUniqueColors)
                {
                    critColorM = UIManager.inst.energyBlue;
                    critColorB = UIManager.inst.deepInfoBlue;
                }

                // Play a unique critical death message
                UIManager.inst.CreateNewLogMessage($"{botName} suffers critical hit ({deathInfo.crit.ToString()}).", critColorM, critColorB, false, false);
                break;
            case DeathType.DTType.None:
                Debug.LogError($"ERROR: {this.gameObject.name} has death type of none!");
                break;
        }
        #endregion

        #region Overall Death Message
        string deathMessage = $"{this.botInfo.botName} destroyed.";
        // Make a log message
        if (overrideMessage != "")
        {
            deathMessage = overrideMessage;
        }

        // Set message color based on relation to player
        Color mcolorMain = UIManager.inst.activeGreen;
        Color mcolorBack = UIManager.inst.dullGreen;

        if(this.allegances.GetRelation(this.myFaction) == BotRelation.Hostile)
        {
            mcolorMain = UIManager.inst.highSecRed;
            mcolorBack = UIManager.inst.dangerRed;
        }

        UIManager.inst.CreateNewLogMessage(deathMessage, mcolorMain, mcolorBack, false, false);
        #endregion

        #region Death Sound
        // Play a death sound
        if (botInfo._size == BotSize.Tiny || botInfo._size == BotSize.Small)
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_robotdestruction[$"STATIC_SML_0{Random.Range(1, 3)}"]); // STATIC_SML_1/2/3
        }
        else if (botInfo._size == BotSize.Medium)
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_robotdestruction[$"STATIC_MED_0{Random.Range(1, 3)}"]); // STATIC_MED_1/2/3
        }
        else
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_robotdestruction[$"STATIC_LRG_0{Random.Range(1, 3)}"]); // STATIC_LRG_1/2/3/4/5/6/7/8
        }
        #endregion

        // Drop all items in inventory
        foreach (var I in inventory.Container.Items)
        {
            InventoryControl.inst.DropItemOnFloor(I.item, this, inventory, Vector2Int.zero);
        }
        armament.Clear();
        Destroy(armament);
        components.Clear();
        Destroy(components);
        inventory.Clear();
        Destroy(inventory);

        //  Drop some matter based on salvage mod
        #region Matter Dropping
        /* When a robot is destroyed, it leaves an amount of matter equivalent to its salvage potential 
         * (usually a random range you can see on its info page), modified directly by the salvage modifier. 
         * This means a large enough negative salvage modifier, e.g. from explosives or repeated cannon hits, 
         * has the potential to reduce the amount of salvageable matter to zero. A positive salvage modifier 
         * can never increase the resulting matter by more than the upper limit of a robot's salvage potential.
         */
        int randomPull = Random.Range(botInfo.salvagePotential.x, botInfo.salvagePotential.y);
        // -Modify this value by the salvage modifier
        randomPull += salvageModifier;
        if (randomPull > 0)
        {
            if (randomPull > botInfo.salvagePotential.y)
            {
                randomPull = botInfo.salvagePotential.y;
            }

            // Drop some matter
            InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo("Matter", HF.LocateFreeSpace(HF.V3_to_V2I(this.transform.position)), randomPull, false));
        }
        #endregion

        // Set tile underneath as dirty
        Vector2Int pos = HF.V3_to_V2I(this.transform.position);
        MapManager.inst.mapdata[pos.x, pos.y].isDirty = Random.Range(0, MapManager.inst.debrisTiles.Count - 1);
        MapManager.inst.UpdateTile(MapManager.inst.mapdata[pos.x, pos.y], pos);

        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        _sprite.color = new Color(1, 1, 1, 0);

        yield return null;

        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        if (TurnManager.inst)
            TurnManager.inst.turnEvents.onTurnTick -= TurnTick; // Stop listening to the turn tick event

        fieldOfView.Clear();

        // Remove from lists
        if (GameManager.inst)
        {
            GameManager.inst.entities.Remove(this);
        }

        if (TurnManager.inst)
        {
            TurnManager.inst.actors.Remove(this);
            TurnManager.inst.actorNum -= 1;
        }

        if (armament)
        { // Failsafe. We don't want infinite inventories cluttering up our file system.
            Destroy(armament);
        }
        if (components)
        { // Failsafe. We don't want infinite inventories cluttering up our file system.
            Destroy(components);
        }
        if (inventory)
        { // Failsafe. We don't want infinite inventories cluttering up our file system.
            Destroy(inventory);
        }
    }

    #endregion

    #region Visibility

    public void UpdateFieldOfView()
    {
        Vector3 location = this.transform.position;
        Vector3Int gridPosition = new Vector3Int((int)location.x, (int)location.y, (int)location.z);

        fieldOfView.Clear();
        algorithm.Compute(gridPosition, fieldOfViewRange, fieldOfView);

        if (GetComponent<PlayerData>())
        {
            FogOfWar.inst.UpdateFogMap(fieldOfView);
            FogOfWar.inst.SetEntityVisibility();
            HF.AttemptLocateTrap(fieldOfView);
        }
        else
        {

            // TODO: Expand this to include other bots hostile to this bot as well
            // If this is a hostile bot and is seeing the player for the first time, do the alert indicator
            if (this.GetComponent<BotAI>().relationToPlayer == BotRelation.Hostile && this.GetComponent<BotAI>().state != BotAIState.Hunting)
            {
                // Try to spot the player
                if (Action.TrySpotActor(this, PlayerData.inst.GetComponent<Actor>()))
                {
                    // Saw the player!

                    this.GetComponent<BotAI>().memory = botInfo.memory; // Set memory to max
                    this.GetComponent<BotAI>().state = BotAIState.Hunting; // Set to hunting mode
                    this.GetComponent<BotAI>().canSeePlayer = true;

                    if (!firstTimeSeen)
                    {
                        // Flash the indicator
                        FlashAlertIndicator();
                        // Do a name readout
                        DoBotPopup();
                        firstTimeSeen = true;
                    }

                    //PlayerData.inst.GetComponent<PotentialField>().enabled = true; // Enable the player's PF
                    if (this.GetComponent<BotAI>().squadLeader)
                        this.GetComponent<BotAI>().squadLeader.GetComponent<GroupLeader>().playerSpotted = true;
                }
                else
                {
                    // Didn't see the player
                }
            }


            // If this is a friendly bot and is seeing the player for the first AND, has; dialogue, hasn't talked yet, isn't talking, THEN perform that dialogue.
            if ((this.GetComponent<BotAI>().relationToPlayer == BotRelation.Neutral || this.GetComponent<BotAI>().relationToPlayer == BotRelation.Friendly)
                && GetComponent<BotAI>().hasDialogue
                && !GetComponent<BotAI>().talking
                && !GetComponent<BotAI>().finishedTalking &&
                fieldOfView.Contains(new Vector3Int((int)PlayerData.inst.transform.position.x, (int)PlayerData.inst.transform.position.y, 0)))
            {
                if (!firstTimeSeen)
                {
                    StartCoroutine(GetComponent<BotAI>().PerformScriptedDialogue());
                    firstTimeSeen = true;
                }
            }

            if (this.GetComponent<BotAI>().canSeePlayer) // If we have previously seen the player, we need to check if we can see them still
            {
                // Draw a Bresenham line from this actor to the player.
                List<Vector2Int> path = HF.BresenhamPath(HF.V3_to_V2I(this.transform.position), HF.V3_to_V2I(PlayerData.inst.transform.position));

                if (HF.ReturnObstacleInLOS(path) == Vector2Int.zero)
                {
                    // Yes we can still see them directly.
                }
                else
                {
                    this.GetComponent<BotAI>().canSeePlayer = false; // Can no longer directly see them.
                    if (this.GetComponent<BotAI>().memory > 0) // Decrement memory
                        this.GetComponent<BotAI>().memory--;
                }
            }
        }
    }
    bool firstTimeSeen = false;

    public void ClearFieldOfView()
    {
        fieldOfView.Clear();
    }

    [Header("Visibility")]
    public bool isExplored;
    public bool isVisible;
    public SpriteRenderer fow_sprite;

    /// <summary>
    /// Updates the visual state of a bot based on their flags. Ignores player.
    /// </summary>
    public void UpdateVis(byte update)
    {
        if (this.GetComponent<PlayerData>())
        {
            return;
        }

        if (update == 0) // UNSEEN/UNKNOWN
        {
            isExplored = false;
            isVisible = false;
        }
        else if (update == 1) // UNSEEN/EXPLORED
        {
            isExplored = true;
            isVisible = false;
        }
        else if (update == 2) // SEEN/EXPLORED
        {
            isExplored = true;
            isVisible = true;
        }

        if (isVisible)
        {
            //fow_sprite.color = baseColor;
            HealthColorCheck(baseColor);
            if (showSensorIndicator)
            {
                sensorSprite.SetActive(false);
            }
        }
        else if (isExplored && isVisible)
        {
            //fow_sprite.color = baseColor;
            HealthColorCheck(baseColor);
            if (showSensorIndicator)
            {
                sensorSprite.SetActive(false);
            }
        }
        else if (isExplored && !isVisible)
        {
            //fow_sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f);
            HealthColorCheck(new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f));
            if (showSensorIndicator)
            {
                sensorSprite.SetActive(true);
            }
        }
        else if (!isExplored)
        {
            //fow_sprite.color = Color.black;
            HealthColorCheck(Color.black);
            if (showSensorIndicator)
            {
                sensorSprite.SetActive(true);
            }
        }
    }

    #endregion

    #region Same-Sprite Indicators

    public GameObject sensorSprite;
    public bool showSensorIndicator = false;
    // Sensor Unknown Indicator
    public void ShowSensorIndicator()
    {
        sensorSprite.SetActive(true);
        showSensorIndicator = true;
    }

    public void HideSensorIndicator()
    {
        sensorSprite.SetActive(false);
        showSensorIndicator = false;
    }


    // Alert Indicator
    public GameObject flashAlertSprite;
    bool flashAlertActive = false;
    public void FlashAlertIndicator()
    {
        if (!flashAlertActive)
        {
            StartCoroutine(FlashIndictor());
        }
    }

    IEnumerator FlashIndictor()
    {
        // Flash 3 Times

        flashAlertActive = true;

        flashAlertSprite.SetActive(true); // 1

        yield return new WaitForSeconds(0.25f);

        flashAlertSprite.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        flashAlertSprite.SetActive(true); // 2

        yield return new WaitForSeconds(0.25f);

        flashAlertSprite.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        flashAlertSprite.SetActive(true); // 3

        yield return new WaitForSeconds(0.25f);

        flashAlertSprite.SetActive(false);

        flashAlertActive = false;
    }

    #endregion

    #region Misc

    private bool confirmColl = false;
    private float collisionCooldown = 3f;
    public bool confirmCollision = false;

    public void ConfirmCollision(Actor target)
    {
        if (confirmColl)
        {
            // Already confirmed, time to move!
            confirmCollision = true;
        }
        else // First time confirm
        {
            // Player a warning message
            UIManager.inst.ShowCenterMessageTop("Collision imminent! Confirm direction.", UIManager.inst.dangerRed, Color.black);
            // Play a warning sound
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["COLLISION_WARNING"], 0.7f); // UI - COLLISION_WARNING
            // Flash an indicator on the target
            target.FlashAlertIndicator();
            // Start the timer
            StartCoroutine(ConfirmCooldown());
        }
    }

    private IEnumerator ConfirmCooldown()
    {
        confirmColl = true;

        yield return new WaitForSeconds(collisionCooldown);

        confirmColl = false;
    }

    private void DoBotPopup()
    {

        Color a = Color.black, b = Color.black, c = Color.black;
        a = Color.black;
        string _message = "";
        _message = this.gameObject.name; // Name only

        float HP = (float)botInfo.currentIntegrity / (float)botInfo.coreIntegrity; // Set color related to current item health
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

        UIManager.inst.CreateItemPopup(this.gameObject, _message, a, b, c);

    }

    public void TransferStates(Actor target)
    {
        target.state_CLOAKED = this.state_CLOAKED;
        target.state_DISABLED = this.state_DISABLED;
        target.state_DISARMED = this.state_DISARMED;
        target.state_DORMANT = this.state_DORMANT;
        target.state_UNPOWERED = this.state_UNPOWERED;
    }

    #endregion

    #region IFF Burst
    /*  Contains functions related to the "IFF Burst" corruption effect
     *  which has a nice little animation. In practice only the player
     *  will be doing this but it is here for possible future use.
     */

    public void IFFBurst()
    {
        // EXAMPLE: https://youtu.be/89n0L64Dyvk?si=yi1ysBgj1JrQftIH&t=3193, or https://youtu.be/xbYol_eMeAY?si=jrgtM2jpRMKQD823&t=1992
        // IFF Burst - An IFF burst comes from Cogmind, alerting hostiles within a 15 tile radius.
        //             Bots will head to investigate the location of the IFF burst but do not gain active tracking on Cogmind.
        //             Also has the log message IFF burst signal emitted.

        #region Message
        Color printoutMain = UIManager.inst.corruptOrange;
        Color printoutBack = UIManager.inst.corruptOrange_faded;
        if (this.GetComponent<PlayerData>()) // -- PLAYER --
        {
            UIManager.inst.CreateNewLogMessage($"System corrupted: IFF burst signal emitted.", printoutMain, printoutBack, false, true);
        }
        else // -- BOT --
        {
            UIManager.inst.CreateNewLogMessage($"{uniqueName} emits IFF burst signal.", printoutMain, printoutBack, false, true);
        }
        #endregion

        // Audio
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_traps["SEGREGATOR"]); // TRAPS - SEGREGATOR (May be wrong clip)

        #region Animation
        IFFBurstAnimation();
        #endregion

        #region Hostile Bot Interaction
        List<Actor> nearbyBots = HF.FindBotsWithinRange(this, 15); // Get all nearby (15) bots
        foreach (var A in nearbyBots)
        {
            if(HF.DetermineRelation(this, A) == BotRelation.Hostile) // Only hostiles
            {
                A.GetComponent<BotAI>().ProvideEnemyLocation(this.transform.position); // Provide new location
            }
        }
        #endregion

    }

    private void IFFBurstAnimation()
    {
        // This neat little animation will require spawning in short lived animated tiles centered on this bot and raditing outward like electricity.

        // -We can make this a bit easier by raycasting out in a bunch of random (but semi-uniform) directions.
        // -8 to 10 rays
        int rayCount = Random.Range(8, 10);
        Vector2 center = this.transform.position;
        
        // The tiles we will spawn in the animating tiles on
        List<Vector2Int> activeTiles = new List<Vector2Int>();

        // Direction(s) - Like a flower's petals but with a bit of randomness
        Vector2[] directions = new Vector2[rayCount];
        float angleStep = 360f / rayCount; // Divide the circle into equal segments
        float randomSpread = 10f;
        for (int i = 0; i < rayCount; i++)
        {
            // Base angle of the ray (spread out evenly)
            float baseAngle = i * angleStep;

            // Add some randomness to the angle within the randomSpread range
            float randomAngle = baseAngle + Random.Range(-randomSpread, randomSpread);

            // Convert the angle to radians
            float angleInRadians = randomAngle * Mathf.Deg2Rad;

            // Calculate direction using trigonometry
            directions[i] = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        }

        // Collect up the tiles by doing a raycast
        for (int i = 0; i < rayCount; i++)
        {
            // Random ray distance between 6 and 11
            float distance = Random.Range(6, 11);

            // Raycast
            RaycastHit2D[] hits = Physics2D.RaycastAll(center, directions[i], distance);

            // Refine this into positions
            foreach (var H in hits)
            {
                Vector2Int pos = new Vector2Int((int)H.transform.position.x, (int)H.transform.position.y);

                if (!activeTiles.Contains(pos)) // Only add unique entries
                {
                    activeTiles.Add(pos);

                    // ONLY FOR DEBUG+TESTING PURPOSES
                    /*
                    var spawnedTile = Instantiate(UIManager.inst.prefab_basicTile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
                    spawnedTile.name = $"IFF Tile: {pos.x},{pos.y}"; // Give grid based name
                    spawnedTile.transform.parent = this.transform;
                    spawnedTile.GetComponent<SpriteRenderer>().sortingOrder = 31;
                    */
                }
            }
        }

        // Sort the positions by distance (closest -> furtherst)
        activeTiles.Sort((v1, v2) => (v1 - center).sqrMagnitude.CompareTo((v2 - center).sqrMagnitude));

        // -Create tiles in those locations
        float stagger = 0f; // Stagger out the animations based on this.
        float staggerIncrement = 0.002f;
        foreach (var pos in activeTiles)
        {
            var spawnedTile = Instantiate(UIManager.inst.prefab_basicTile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
            spawnedTile.name = $"IFF Tile: {pos.x},{pos.y}"; // Give grid based name
            spawnedTile.transform.parent = this.transform;
            spawnedTile.GetComponent<SpriteRenderer>().sortingOrder = 31;

            // -And have them perform their animations based on distance to the player
            // NOTE: We don't really need to keep track of these or force delete them because they handle that themselves in this instance.
            spawnedTile.GetComponent<SimpleTileAnimator>().InitIFF(stagger += staggerIncrement);
        }
    }
    #endregion
}

[System.Serializable]
[Tooltip("Contains information regarding a bot's death.")]
public class DeathType
{
    [Tooltip("Who killed this bot?")]
    public Actor killer;
    public DTType type = DTType.Core;
    public CritType crit;
    [Tooltip("Set to override the default death message.")]
    public string deathMessage = "";
    [Tooltip("If the bright blue colors should be used for the log message instead. Usually false.")]
    public bool useUniqueColors = false;

    public DeathType(Actor killer, DTType type, CritType crit, string deathMessage = "", bool useUniqueColors = false)
    {
        this.killer = killer;
        this.type = type;
        this.crit = crit;
        this.deathMessage = deathMessage;
        this.useUniqueColors = useUniqueColors;
    }

    public enum DTType
    {
        [Tooltip("Core health reached 0. The normal death type.")]
        Core,
        [Tooltip("Corruption reached 100.")]
        Corruption,
        [Tooltip("Killed by a critical attack.")]
        Critical,
        None
    }
}
