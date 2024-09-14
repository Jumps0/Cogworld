using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Actor))]
/// <summary>
/// A generic AI script for every type of bot.
/// </summary>
public class BotAI : MonoBehaviour
{

    [Header("Squad")]
    [Tooltip("List of all squad members")]
    public List<Actor> squad = new List<Actor>();
    [Tooltip("The squad leader")]
    public Actor squadLeader;

    [Header("Relevant Actions")]
    public BotAIState state = BotAIState.Working;
    public bool isTurn = false;

    [Header("Vision")]
    public int memory = 0;
    public bool canSeePlayer = false;
    public BotRelation relationToPlayer = BotRelation.Neutral;

    [Header("Pathing")]
    public List<Vector2Int> pointsOfInterest = new List<Vector2Int>(); // Bot will visit these

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region General Turn-taking

    /// <summary>
    /// Initate this bot's turn.
    /// </summary>
    public void TakeTurn()
    {
        // DEBUG
        if (isTurn)
        {
            isTurn = false;
            Action.SkipAction(this.GetComponent<Actor>());
        }

        // - Don't do anything if this actor isn't "awake" (or able to reasonably take their turn)
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            return;
        }

        this.GetComponent<Actor>().UpdateFieldOfView();

        StartCoroutine(DecideTurnAction());
    }

    // FUTURE NOTE TO SELF, MAKE SURE THE AI CHECKS IF THEY CAN AFFORD TO MOVE WITH `HF.HasResourcesToMove(this.GetComponent<Actor>)` BEFORE THEM DECIDING TO MOVE

    /// <summary>
    /// Actually decide what to do during this bots turn.
    /// </summary>
    private IEnumerator DecideTurnAction()
    {
        // - Don't do anything if this actor isn't "awake" (or able to reasonably take their turn)
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            yield break;
        }

        yield return null;

        // -- Now decide what to do based on their *Refined Class* --
        switch (this.GetComponent<Actor>()._class)
        {
            case BotClassRefined.Worker:
                /* -- Each type of worker does different things, these are:
                 *  Hauler:     Roam the map, pick up (non-player made) items created via fabricators.
                 *  Mechanic:   Roam the map, look for allied bots to repair.
                 *  Operator:   Roams around a home terminal, reports Cogmind and turns neutral on visual contact.
                 *  Recycler:   Roam the map, look for parts to recycle.
                 *  Builder:    Roam the map, look for walls and doors to repair.
                 *  Watcher:    Roam the map, report Cogmind to nearby allies, stay close unless attacked.
                 *  Worker:     Roam the map looking for debris & machines to clean up
                 *  Tunneler:   Roam the map, dig scripted sections
                 *  Researcher: Stand at station, fight Cogmind if they act up, roam around if alert is up.
                 */

                switch (state)
                {
                    case BotAIState.Working:
                        // In this state, the worker is going to be performing some kind of action.
                        if(this.GetComponent<Actor>().botInfo._class == BotClass.Hauler)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Mechanic)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Operator)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Recycler)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Builder)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Watcher)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Worker)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Tunneler)
                        {

                        }
                        else if (this.GetComponent<Actor>().botInfo._class == BotClass.Researcher)
                        {

                        }
                        break;
                    case BotAIState.Hunting:

                        break;
                    case BotAIState.Returning:

                        break;
                    case BotAIState.Fleeing:

                        break;
                    case BotAIState.Idle:

                        break;
                }

                break;
            case BotClassRefined.Fighter:



                break;
            case BotClassRefined.Support:



                break;
            case BotClassRefined.Static:
                // -- We are going to spend most of our time standing still, unless told otherwise. --
                switch (state)
                {
                    case BotAIState.Working:
                        if(HF.V3_to_V2I(this.transform.position) != locationOfInterest)
                        {
                            state = BotAIState.Returning;

                            // Move back to the position
                            if (pathing.path.Count < 0)
                            {
                                // Need to calculate an A* path, this may be computationally expensive!
                                SetNewAStar();
                                CreateAStarPath(locationOfInterest);
                            }

                            NavigateToPOI(locationOfInterest);
                        }
                        else
                        {
                            // Do... nothing?

                            if(pathing.path.Count > 0) // Just in case, clear this path (due to logic above)
                            {
                                pathing.path.Clear();
                            }
                        }

                        break;

                    case BotAIState.Returning:
                        if (HF.V3_to_V2I(this.transform.position) != locationOfInterest)
                        {
                            // Move back to the position
                            if (pathing.path.Count < 0)
                            {
                                // Need to calculate an A* path, this may be computationally expensive!
                                SetNewAStar();
                                CreateAStarPath(locationOfInterest);
                            }

                            NavigateToPOI(locationOfInterest);
                        }
                        else
                        {
                            state = BotAIState.Working;
                        }
                        break;
                    case BotAIState.Fleeing:
                        Flee(); // Flee!
                        break;
                    case BotAIState.Idle:
                        break;
                    default:
                        break;
                }

                break;
            case BotClassRefined.Ambient:
                // -- Our main goal here is to just wander around if nothing is happening --
                switch (state)
                {
                    case BotAIState.Working: // aka Wandering
                        if(timeOnTask > Random.Range(5, 10) || locationOfInterest == Vector2Int.zero) // Find a new spot to wander to
                        {
                            locationOfInterest = FindNewWanderPoint(Random.Range(3, 6));
                            timeOnTask = 0;
                        }
                        else // Loiter around location of interest
                        {
                            timeOnTask++;
                            Loiter(locationOfInterest);
                        }
                        break;
                    case BotAIState.Returning:
                        if(timeOnTask < 20) // Try and move back to our loiter spot
                        {
                            timeOnTask++;
                            Loiter(locationOfInterest);
                        }
                        else // Switch states
                        {
                            locationOfInterest = FindNewWanderPoint(Random.Range(3, 6));
                            timeOnTask = 0;
                            state = BotAIState.Working;
                        }
                        break;
                    case BotAIState.Fleeing:
                        Flee(); // Flee!
                        break;
                    case BotAIState.Idle:
                        // Do nothing
                        break;
                    default:
                        // Do nothing
                        break;
                }

                break;
            case BotClassRefined.None:



                break;
        }




        Action.SkipAction(this.GetComponent<Actor>()); // Fallback condition
    }

    [Tooltip("How long has this bot been doing this specific task?")]
    int timeOnTask = 0;
    [Tooltip("A location of importance, used in decisionmaking.")]
    Vector2Int locationOfInterest = Vector2Int.zero;

    #endregion


    #region Loitering
    /// <summary>
    /// Will make the bot "loiter" around a specific area. Randomly wandering around but staying near the center.
    /// </summary>
    /// <param name="center">The point to stay near.</param>
    private void Loiter(Vector2Int center, float maxDist = 4)
    {
        float maxDistance = maxDist;

        Vector2Int myPos = HF.V3_to_V2I(this.transform.position);
        // Get neighbors
        List<GameObject> neighbors = HF.FindNeighbors(myPos.x, myPos.y);

        float currentDistance = Vector2.Distance(myPos, center);

        if(currentDistance > maxDistance) // Too far, try to get closer
        {
            // Sort the neighbors based on distance
            neighbors.Sort((a, b) => Vector3.Distance(this.transform.position, a.transform.position).CompareTo(Vector3.Distance(this.transform.position, b.transform.position)));

            // We are going to try the 3 closest tiles
            for (int i = 0; i < 3; i++)
            {
                // Try moving here
                if (this.GetComponent<Actor>().IsUnoccupiedTile(neighbors[i].GetComponent<TileBlock>()))
                {
                    Vector2 direction = HF.V3_to_V2I(neighbors[i].transform.position) - myPos;
                    Action.MovementAction(this.GetComponent<Actor>(), direction);
                    return;
                }
            }

            // Can't move, so don't
            Action.SkipAction(this.GetComponent<Actor>());
            return;
        }
        else // If not, continue
        {
            float random = Random.Range(0f, 1f);

            if(random > 0.5f) // Don't move
            {
                Action.SkipAction(this.GetComponent<Actor>());
                return;
            }
            else // Move to a random space (if possible)
            {
                List<GameObject> validMoveLocations = new List<GameObject>();

                foreach (var T in neighbors)
                {
                    if (this.GetComponent<Actor>().IsUnoccupiedTile(T.GetComponent<TileBlock>()))
                    {
                        validMoveLocations.Add(T);
                    }
                }

                if(validMoveLocations.Count > 0)
                {
                    // May need to swap these
                    Vector2 direction = HF.V3_to_V2I(validMoveLocations[Random.Range(0, validMoveLocations.Count - 1)].transform.position) - myPos;
                    Action.MovementAction(this.GetComponent<Actor>(), direction);
                    return;
                }
                else // We're stuck, don't move
                {
                    Action.SkipAction(this.GetComponent<Actor>());
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Will attempt to find an unoccupied space nearby to wander around.
    /// </summary>
    /// <param name="distance">The max distance away the point can be. Don't set this too high!</param>
    /// <returns>The position to wander to.</returns>
    private Vector2Int FindNewWanderPoint(int distance)
    {
        Vector2Int myPos = HF.V3_to_V2I(this.transform.position);

        // Calculate the bottom left corner of the square area
        Vector2Int bottomLeftCorner = new Vector2Int(myPos.x - distance, myPos.y - distance);

        List<Vector2Int> validPositions = new List<Vector2Int>();

        // Iterate through the square area
        for (int x = bottomLeftCorner.x; x <= bottomLeftCorner.x + 2 * distance; x++)
        {
            for (int y = bottomLeftCorner.y; y <= bottomLeftCorner.y + 2 * distance; y++)
            {
                Vector2Int currentTile = new Vector2Int(x, y);

                if (MapManager.inst._allTilesRealized.ContainsKey(currentTile))
                {
                    // Check if the tile exists and is unoccupied.
                    if (this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[new Vector2Int(x, y)].bottom))
                    {
                        validPositions.Add(currentTile); // Add to valid positions
                    }
                }
            }
        }

        if(validPositions.Count > 0)
        {
            return validPositions[Random.Range(0, validPositions.Count - 1)];
        }
        else
        {
            return Vector2Int.zero; // Failsafe
        }
    }
    #endregion

    #region A* & Fun!
    [Header("A* & Pathing")]
    public Astar pathing;
    bool destinationReached = false;
    int timeOnPath = 0;

    /* ==== MAJOR TODO ====
     * -Instead of using A*, swap out to a better algorithm
     * -Consider some of the following:
     * https://cstheory.stackexchange.com/questions/11855/how-do-the-state-of-the-art-pathfinding-algorithms-for-changing-graphs-d-d-l
     * Personally i'm interested in: Decentralized Lifelong Multi-agent Pathfinding via Planning and Learning | aka DynamicSWSF-FP
     * (see: https://www.youtube.com/watch?v=LOJabCIDXiM)
     * ====================
     */

    public void SetNewAStar()
    {
        pathing = new Astar(GridManager.inst.grid); // Set up the A*
    }

    public void CreateAStarPath(Vector2Int point)
    {
        pathing.CreatePath(GridManager.inst.grid, (int)this.transform.position.x, (int)this.transform.position.y, point.x, point.y);
    }

    public void NavigateToPOI(Vector2Int poi)
    {
        float distanceToPoi = Vector2.Distance(this.transform.position, poi);

        if (distanceToPoi > 3) // Navigate to it
        {
            destinationReached = false;
            int index = pathing.path.Count - timeOnPath;
            if (index < 0)
            { // Stop early!!!
                destinationReached = true;
                state = BotAIState.Idle;
                return;
            }

            Vector2 destination = new Vector2(pathing.path[index].X, pathing.path[index].Y);
            // Normalize move direction
            Vector2Int moveDir = new Vector2Int(0, 0);
            if (destination.x > this.transform.position.x)
            {
                moveDir.x++;
            }
            else if (destination.x < this.transform.position.x)
            {
                moveDir.x--;
            }

            if (destination.y > this.transform.position.y)
            {
                moveDir.y++;
            }
            else if (destination.y < this.transform.position.y)
            {
                moveDir.y--;
            }
            // Only move to valid tiles!
            if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int((int)destination.x, (int)destination.y))
                && this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[new Vector2Int((int)destination.x, (int)destination.y)].bottom))
            {
                Action.MovementAction(this.GetComponent<Actor>(), moveDir); // Move
            }
            else
            {
                Action.SkipAction(this.GetComponent<Actor>()); // Wait...
            }

            state = BotAIState.Working;
        }
        else // We are done
        {
            destinationReached = true;
            state = BotAIState.Idle;
        }
    }

    #endregion

    #region Fleeing

    /// <summary>
    /// Attempt to flee from the specified source, get out of LOS and/or Field of View Range
    /// </summary>
    /// <param name="source">The actor to flee from.</param>
    public void FleeFromSource(Actor source)
    {
        fleeSource = source;
        //this.GetComponent<PotentialField>().setFlee(source);

        state = BotAIState.Fleeing;

        Flee();
    }

    public void Flee()
    {
        if (fleeSource == null) // Stop?
        {
            state = BotAIState.Idle;
            return;
        }
        else
        {
            int viewRange = this.GetComponent<Actor>().fieldOfViewRange;

            if (Vector3.Distance(this.transform.position, fleeSource.transform.position) <= viewRange) // Flee!
            {
                // We need to move away from the source
                List<GameObject> neighbors = HF.FindNeighbors((int)this.transform.position.x, (int)this.transform.position.y);

                List<GameObject> validMoveLocations = new List<GameObject>();

                foreach (var T in neighbors)
                {
                    if (this.GetComponent<Actor>().IsUnoccupiedTile(T.GetComponent<TileBlock>()))
                    {
                        validMoveLocations.Add(T);
                    }
                }

                GameObject fleeLocation = this.GetComponent<Actor>().FindBestFleeLocation(fleeSource.gameObject, this.gameObject, validMoveLocations);

                if (fleeLocation != null)
                {
                    // Normalize move direction
                    Vector2Int moveDir = new Vector2Int(0, 0);
                    if (fleeLocation.transform.position.x > this.transform.position.x)
                    {
                        moveDir.x++;
                    }
                    else if (fleeLocation.transform.position.x < this.transform.position.x)
                    {
                        moveDir.x--;
                    }

                    if (fleeLocation.transform.position.y > this.transform.position.y)
                    {
                        moveDir.y++;
                    }
                    else if (fleeLocation.transform.position.y < this.transform.position.y)
                    {
                        moveDir.y--;
                    }

                    // Now move there
                    Action.MovementAction(this.GetComponent<Actor>(), moveDir);
                }
                else
                {
                    // We can't move!
                    Action.SkipAction(this.GetComponent<Actor>());
                }


            }
            else // Safe. (for now)
            {
                state = BotAIState.Idle;
                //this.GetComponent<PotentialField>().fleeSource = null;
                //this.GetComponent<PotentialField>().mustFlee = false;
                Action.SkipAction(this.GetComponent<Actor>());
                fleeSource = null;
            }
        }
    }

    private Actor fleeSource = null;

    #endregion

    #region Find me a...

    /// <summary>
    /// Finds the nearest recycler to this bot.
    /// </summary>
    /// <returns>The parent part of the recycler.</returns>
    public RecyclingUnit FindNearestRecycler()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.transform.position;
        foreach (GameObject M in MapManager.inst.machines_recyclingUnits)
        {
            float dist = Vector3.Distance(M.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = M.transform;
                minDist = dist;
            }
        }
        return tMin.transform.GetChild(0).GetComponent<RecyclingUnit>(); ;
    }

    /// <summary>
    /// Find a random static machine.
    /// </summary>
    /// <returns>The parent part of a static machine.</returns>
    public StaticMachine FindRandomStaticMachine()
    {
        GameObject M = MapManager.inst.machines_static[Random.Range(0, MapManager.inst.machines_static.Count)];

        return M.transform.GetChild(0).GetComponent<StaticMachine>();
    }

    /// <summary>
    /// Finds a dirty floor tile somewhere on the map.
    /// </summary>
    /// <returns>A dirty floor tile in the form of a GameObject.</returns>
    public GameObject FindDirtyFloor()
    {
        foreach (var T in MapManager.inst._allTilesRealized)
        {
            if (T.Value.bottom.isDirty)
            {
                return T.Value.bottom.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds nearby dirty floor tiles within specified distance.
    /// </summary>
    /// <param name="distance">How far away to check, keep this low.</param>
    /// <returns>A list of any dirty floor tiles.</returns>
    public List<GameObject> FindDirtyFloorLocal(int distance = 8)
    {
        List<GameObject> local = new List<GameObject>();

        Vector2Int bottomLeft = HF.V3_to_V2I(this.transform.position) - new Vector2Int(distance, distance);

        for (int x = bottomLeft.x; x < bottomLeft.x + (distance * 2); x++)
        {
            for (int y = bottomLeft.y; y < bottomLeft.y + (distance * 2); y++)
            {
                if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int(x, y)))
                {
                    if (MapManager.inst._allTilesRealized[new Vector2Int(x, y)].bottom.isDirty)
                    {
                        local.Add(MapManager.inst._allTilesRealized[new Vector2Int(x, y)].bottom.gameObject);
                    }
                }
            }
        }

        return local;
    }

    /// <summary>
    /// Checks all floor items in the map and returns the first one that isn't native to the environment.
    /// </summary>
    /// <returns>A floor item to pick up.</returns>
    public GameObject CheckFloorItems()
    {
        foreach (Transform child in InventoryControl.inst.allFloorItems.transform)
        {
            if (child.GetComponent<Part>() && !child.GetComponent<Part>().native)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    #endregion

    #region Dialogue
    [Header("Dialogue Related")]
    public bool hasDialogue = false;
    public bool hasBufferDialogue = false; // Will freeze the screen if true. If false, just appears at the bottom + log.
    public bool talking = false; // Is this bot currently chatting with the player?
    public bool finishedTalking = false; // Has this bot finished chatting with the player?
    public List<DialogueC> dialogue = new List<DialogueC>();
    public bool moveToNextDialogue = false;
    public AudioClip uniqueDialogueSound = null;
    public string uniqueName;

    public IEnumerator PerformScriptedDialogue()
    {
        talking = true;

        string botName = this.GetComponent<Actor>().botInfo.name;
        if (uniqueName != "")
            botName = uniqueName;
        UIManager.inst.Dialogue_OpenBox(botName, this.GetComponent<Actor>());

        // Wait for the box to open (and do its little animation)
        while (!UIManager.inst.dialogue_readyToDisplay)
        {
            yield return null;
        }

        foreach (DialogueC text in dialogue)
        {
            bool moreToSay = true;
            if (text.id == dialogue.Count)
            {
                // Out of text
                moreToSay = false;
            }
            else
            {
                moreToSay = true;
            }

            //Debug.Log(this.name + " has started chatting with the player [" + text.id + "].");
            UIManager.inst.Dialogue_DisplayText(text, moreToSay);

            while (!moveToNextDialogue)
            {
                // Wait
                yield return null;
            }
            moveToNextDialogue = false;
        }

        UIManager.inst.Dialogue_Quit();

        talking = false;
        finishedTalking = true;

        // Finally, show all the text in the log
        bool playOnce = true;
        foreach (DialogueC text in dialogue)
        {

            string speech = botName + ": " + "\"" + text.speech + "\""; // NAME: "Text"
            UIManager.inst.CreateNewLogMessage(speech, UIManager.inst.deepInfoBlue, UIManager.inst.coolBlue, false, playOnce);
            playOnce = false; // We are only going to play the audio output sound for the log messages once so we dont blast the player with X layered sounds all at once.
        }
    }

    #endregion
}

// Note, this will probably need to be expanded later v
[System.Serializable]
public enum BotAIState
{
    Working,  // Gaurding, Patrolling, Cleaning, etc | This bot's normal activity
    Hunting,  // Engaging or Looking for the player
    Returning,// Going back to normal routine
    Fleeing,  // Running from something, probably the player
    Idle
}

[System.Serializable]
public class DialogueC
{
    [Tooltip("Starts at 1, sorry.")]
    public int id;
    [TextArea(3, 5)]
    public string speech;

    public DialogueC(int id, string speech)
    {
        this.id = id;
        this.speech = speech;
    }
}
