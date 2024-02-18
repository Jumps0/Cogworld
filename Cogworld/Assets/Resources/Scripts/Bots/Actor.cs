using JetBrains.Annotations;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class Actor : Entity
{
    public bool isAlive = false;

    [Tooltip("Field of view range.")]
    public int fieldOfViewRange = 15; // COGMIND default is 15.

    [Tooltip("All tiles that are currently visible.")]
    [SerializeField] private List<Vector3Int> fieldOfView;

    [Header("Details")]
    public UnitAI AI;
    public BotObject botInfo;
    public Allegance allegances;
    public BotClassRefined _class = BotClassRefined.None;

    AdamMilVisibility algorithm;

    public int Initiative = 0;

    public bool IsAlive { get => isAlive; set => isAlive = value;  }
    public List<Vector3Int> FieldofView { get => fieldOfView; }

    private Color baseColor;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Special Bot States")]
    [Tooltip("DORMANT: Robots temporarily resting in this mode are only a problem if they wake up, which may be triggered by alarm traps, " +
        "being alerted by their nearby allies, or by hostiles hanging around in their field of vision.")]
    public bool state_DORMANT = false; // Grayed-out orange
    [Tooltip("This bot once had weapons but has since lost them and cannot fight. Has a different sprite.")]
    public bool state_DISARMED = false; // Grayed-out green
    [Tooltip("This bot is cloaked is moving in steal, but through one reason or another the player knows their position. Has a different sprite")]
    public bool state_CLOAKED = false; // Grayed-out blue
    [Tooltip("UNPOWERED: Unpowered robots will never become active. (Though yeah they’re very much real and you can harvest them for parts if you’d like.)")]
    public bool state_UNPOWERED = false; // Grayed-out red
    [Tooltip("DISABLED: Some robots have been temporarily disabled and put in garrison storage. Prime candidates for rewiring!")]
    public bool state_DISABLED = false; // Grayed-out normal

    [Header("Following Flags")]
    [Tooltip("If true, will be under the player's direct control, appearing in 'allies' tab.")]
    public bool directPlayerAlly = false;
    [Tooltip("If true, the bot will always try to be close to and fight with the player.")]
    public bool followThePlayer = false;

    private void OnValidate()
    {
        if (GetComponent<UnitAI>())
        {
            AI = GetComponent<UnitAI>();
        }
    }


    private void Start()
    {
        baseColor = _sprite.color;

        if (GameManager.inst)
        {
            AddToGameManager();
            if (GetComponent<PlayerData>())
            {
                TurnManager.inst.InsertActor(this, 0);
            }
            else
            {
                TurnManager.inst.AddActor(this);
                fow_sprite.color = Color.black; // temp fix
                /*
                if (this.GetComponent<PotentialField>() && (this.GetComponent<AI_Hostile>() || this.GetComponent<AI_Passive>()))
                {
                    this.GetComponent<PotentialField>().movementActive = true;
                }
                */
                fieldOfViewRange = botInfo.visualRange;
                maxHealth = botInfo.coreIntegrity;
                currentHealth = maxHealth;
                heatDissipation = botInfo.heatDissipation;
                energyGeneration = botInfo.energyGeneration;

                StartCoroutine(SetBotName());
            }

            algorithm = new AdamMilVisibility();
            allegances = GlobalSettings.inst.GenerateDefaultAllengances(botInfo);

            if (GetComponent<PlayerData>())
            {
                UpdateFieldOfView();
            }
        }
        else
        {
            Debug.LogWarning("GameManager does not exist! Actor will not be initialized!");
        }
    }

    private IEnumerator SetBotName()
    {
        while (!MapManager.inst.loaded)
        {
            yield return null;
        }

        string name = this.botInfo.name;
        if (this.botInfo.name.Contains("("))
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
        if(TurnManager.inst.globalTime >= disabledTurn + disabledTime)
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

    public void StartTurn()
    {
        if(this.GetComponent<BotAI>() != null)
        {
            this.GetComponent<BotAI>().isTurn = true;
            this.GetComponent<BotAI>().TakeTurn();
        }
    }

    public void EndTurn()
    {
        noMovementFor += 1; // should this be here?
        if (this.GetComponent<BotAI>() != null)
            this.GetComponent<BotAI>().isTurn = false;
        //TurnManager.inst.EndTurn(this);
        //MapManager.inst.NearTileVisUpdate(this.fieldOfViewRange, HF.V3_to_V2I(this.transform.position)); // experimental

    }

    #region Navigation

    //finds an empty tile for this actor to move to
    public void FindNewTile()
    {
        List<GameObject> neighbors = HF.FindNeighbors(((int)transform.localPosition.x), ((int)transform.localPosition.y));
        GameObject tile = null;
        foreach (GameObject neighbor in neighbors)
        {
            if (IsUnoccupiedTile(neighbor.GetComponent<TileBlock>()))
            {
                tile = neighbor;
            }
        }

        if (tile != null)
        {
            NewGoal(new Vector2Int(tile.GetComponent<TileBlock>().locX, tile.GetComponent<TileBlock>().locY));
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

        NewGoal(Action.V3_to_V2I(this.GetComponent<BotAI>().squadLeader.transform.position));
        // Then move based on inputs from PF
        Vector2Int moveToLocation = Action.NormalizeMovement(this.gameObject.transform, this.GetComponent<OrientedPhysics>().desiredPostion);
        Vector2Int realPos = Action.V3_to_V2I(this.GetComponent<OrientedPhysics>().desiredPostion);

        if (MapManager.inst._allTilesRealized.ContainsKey(realPos) && this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[realPos]))
        {
            Debug.Log("Move successful");
            Action.MovementAction(this, moveToLocation);
        }
        else
        {
            //Debug.Log("Move failed {" + moveToLocation + "}");
            Action.SkipAction(this.GetComponent<Actor>());
        }
    }

    public void MoveToPatrolPoint()
    {
        // !! Assumes that this bot is 1) Hostile 2) a Group Leader

        // If close to destination
        if (Vector2.Distance(this.transform.position, this.GetComponent<GroupLeader>().route[this.GetComponent<GroupLeader>().pointInRoute]) <= 3)
        {
            if(this.GetComponent<GroupLeader>().pointInRoute >= this.GetComponent<GroupLeader>().route.Count - 1)
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

        // Then move based on inputs from PF
        Vector2Int moveToLocation = Action.NormalizeMovement(this.gameObject.transform, this.GetComponent<OrientedPhysics>().desiredPostion);
        Vector2Int realPos = Action.V3_to_V2I(this.GetComponent<OrientedPhysics>().desiredPostion); 

        if (MapManager.inst._allTilesRealized.ContainsKey(realPos) && this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[realPos]))
        {
            Action.MovementAction(this.GetComponent<Actor>(), moveToLocation);
        }
        else
        {
            Action.SkipAction(this.GetComponent<Actor>());
        }
    }

    /// <summary>
    /// Checks to see if a specified tile is unoccupied.
    /// </summary>
    /// <param name="tile">The specified tile to check.</param>
    /// <returns></returns>
    public bool IsUnoccupiedTile(TileBlock tile)
    {
        if (tile.tileInfo.type == TileType.Wall)
        {
            return false;
        }

        if (MapManager.inst._layeredObjsRealized.ContainsKey(new Vector2Int(tile.locX, tile.locY)))
        {
            if (MapManager.inst._layeredObjsRealized[new Vector2Int(tile.locX, tile.locY)].GetComponent<DoorLogic>()) // This is a door
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if (GameManager.inst.GetBlockingActorAtLocation(tile.transform.position))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public GameObject GetBestFleeLocation(GameObject target, GameObject entity, List<GameObject> validMoveLocations)
    {
        GameObject bestLocation = null;
        float furthestDistance = 0f;

        // Calculate the current distance between the entity and the target
        float currentDistance = Vector3.Distance(entity.transform.position, target.transform.position);

        // Loop through each valid move location
        foreach (GameObject moveLocation in validMoveLocations)
        {
            // Calculate the distance between the move location and the target
            float distanceFromTarget = Vector3.Distance(target.transform.position, moveLocation.transform.position);

            // Calculate the distance between the entity and the move location
            float distanceFromEntity = Vector3.Distance(entity.transform.position, moveLocation.transform.position);

            // Calculate the new distance between the entity and the target if the entity moves to this location
            float newDistance = Mathf.Max(currentDistance - distanceFromEntity, 0f) + distanceFromTarget;

            // If the new distance is greater than the furthest distance so far
            if (newDistance > furthestDistance)
            {
                // Set this location as the new best location and update the furthest distance
                bestLocation = moveLocation;
                furthestDistance = newDistance;
            }
        }

        // Return the best move location to flee to
        return bestLocation;
    }

    #endregion

    #region Combat

    public void HealthCheck()
    {
        if (this.GetComponent<PlayerData>())
        {
            if(PlayerData.inst.currentHealth <= 0)
            {
                // Die!
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            if (currentHealth <= 0)
            {
                Die();
                PlayerData.inst.robotsKilled += 1; // TODO: CHANGE THIS LATER TO TELL IF THE PLAYER ACTUALLY GOT THE KILL (not killsteal)
            }

            // Shoving this in here too
            state_DISARMED = this.botInfo.armament.Count <= 0;
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

    public void Die()
    {
        // Make a log message
        string botName = this.botInfo.name;
        if (this.GetComponent<BotAI>().uniqueName != "")
            botName = this.GetComponent<BotAI>().uniqueName;
        UIManager.inst.CreateNewLogMessage(botName + " destroyed.", UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, false);

        // Play a death sound
        if (botInfo._size == BotSize.Tiny || botInfo._size == BotSize.Small)
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.RobotDestruction_Clips[Random.Range(36,38)]);
        }
        else if (botInfo._size == BotSize.Medium)
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.RobotDestruction_Clips[Random.Range(33, 35)]);
        }
        else
        {
            this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.RobotDestruction_Clips[Random.Range(25, 32)]);
        }

        // Drop any remaining parts
        foreach (BotArmament item in this.botInfo.armament.ToList())
        {
            float random = Random.Range(0f, 1f);

            if (item._item.data.integrityCurrent > 0 && random <= item.dropChance)
            {
                InventoryControl.inst.DropItemOnFloor(item._item.data);
            }
        }
        foreach (BotArmament item in this.botInfo.components.ToList())
        {
            float random = Random.Range(0f, 1f);

            if (item._item.data.integrityCurrent > 0 && random <= item.dropChance)
            {
                InventoryControl.inst.DropItemOnFloor(item._item.data);
            }
        }

        // Set tile underneath as dirty
        MapManager.inst._allTilesRealized[Action.V3_to_V2I(this.transform.position)].SetToDirty();
        
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
        fieldOfView.Clear();

        // Remove from lists
        if(GameManager.inst)
            GameManager.inst.entities.Remove(this);
        if(TurnManager.inst)
            TurnManager.inst.actors.Remove(this);
            TurnManager.inst.actorNum -= 1;
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
            FogOfWar.inst.SetEntitiesVisibilities();
            HF.AttemptLocateTrap(fieldOfView);
        }
        else
        {
            BotRelation relation = HF.DetermineRelation(this, PlayerData.inst.GetComponent<Actor>());

            // TODO: Expand this to include other bots hostile to this bot as well
            // If this is a hostile bot and is seeing the player for the first time, do the alert indicator
            if (relation == BotRelation.Hostile && this.GetComponent<BotAI>().state != BotAIState.Hunting)
            {
                // Try to spot the player
                if (Action.TrySpotActor(this, PlayerData.inst.GetComponent<Actor>()))
                {
                    // Saw the player!

                    this.GetComponent<BotAI>().memory = botInfo.memory; // Set memory to max
                    this.GetComponent<BotAI>().state = BotAIState.Hunting; // Set to hunting mode

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
            if ((relation == BotRelation.Neutral || relation == BotRelation.Friendly) 
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

    public void CheckVisibility()
    {
        if (this.GetComponent<PlayerData>())
        {
            return;
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
            AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[18], 0.7f);
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
}
