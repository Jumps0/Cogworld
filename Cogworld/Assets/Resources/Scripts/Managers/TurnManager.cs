using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls player and NPC turns, along with Global Time.
/// </summary>
public class TurnManager : MonoBehaviour
{

    public static TurnManager inst;
    public void Awake()
    {
        inst = this;
        turnEvents = new GenericEvent(); // Setup the event listener
    }

    // -        -
    //
    [Header("Time")]
    [Tooltip("Global *TURN* time")]
    public int globalTime = 0; // Global *TURN* time
    [Tooltip("CLOCK time")]
    public int clockTime = 0;  // CLOCK time
    [Tooltip("RUN time")]
    public int runTime = 0;    // RUN time
    [SerializeField] private float timer = 0.0f;
    [SerializeField] private bool doTimer = true;
    private int turnCounter = 0;
    //
    [Tooltip("External timer for when specific events happen.")]
    public int absoluteTurnTime = 100; // When global things happen

    [Header("Variables")]
    public bool isPlayerTurn = true;

    [Header("Entities")]
    public int actorNum = 0;

    [Header("Events")]
    public GenericEvent turnEvents;

    public List<Actor> actors = new List<Actor>();

    #region ~Explanation~
    // -- Explanations -- //
    /*
     * 
     *     ~ Action Costs ~
     * 
     * The length of an action is extremely variable in Cogmind. Most actions require a static amount of time, 
     * but the two most common actions, moving and attacking, are calculated based on multiple factors, 
     * and therefore change throughout the game depending on your status.
     * 
     * To keep things simple, the majority of actions take either one turn, half a turn, or 1.5 turns:
     * 
     * [Cost]  [Action]
     *  100	   Pick up Item 
     *  100	   Attach Item
     *  150	   Attach from Ground
     *  50	   Detach Item
     *  50	   Drop Item
     *  150	   Swap Item (Inventory <-> Equipment)
     *  100	   Misc. Actions (Ram / Rewire / Escape Stasis…)
     *  
     *  And that’s it--Cogmind doesn’t actually have a wide variety of unique action types, 
     *  and for simplicity sake a lot of miscellaneous actions require precisely one turn. 
     *  But with one turn the equivalent of 100 time units, there’s a lot of leeway for fine-grained 
     *  requirements when it comes to the most common actions: moving and attacking.
     *  
     *   ~ Moving Costs ~
     *   
     * Moving even a single space involves a potentially huge range of time, quite different from the average roguelike. 
     * How long it takes to move
     * 
     * is highly dependent on the form of propulsion:
     * 
     * [Cost]	[Propulsion]
     *  40	    Flight
     *  60	    Hover
     *  80	    Wheels
     *  120	    Legs
     *  160	    Treads
     *  
     *  Those are simply base costs, though, which might vary somewhat with unique items, 
     *  and which in the case of flight and hover can be further modified by using multiple items at once. 
     *  For example using three flight units will be faster than using two.
     *  
     *  Because movement speed is an important factor in turn-to-turn play, 
     *  it is displayed on the HUD at all times, though in one of two forms. 
     *  For beginners it’s shown as a percent of base speed, so 100% when one move = one turn, 
     *  140% when 1.4 moves = one turn, etc. This is to keep it more intuitive at first, 
     *  rather than having new players misunderstand that in terms of time units, 
     *  technically lower “speed” numbers are faster. Players who activate the more advanced “Tactical HUD” 
     *  mode in the options see instead the actual current movement cost itself (thus the meaning is reversed, higher is slower).
     *  
     *   ~ Attacking ~
     *  
     *  Attacking has smaller time cost scaling than movement, but is interesting in that its costs are greater than those of other actions, 
     *  especially movement. The base cost to fire a single weapon is 200 (two turns!), 
     *  meaning defenders can more easily escape after coming under attack (if they want to). 
     *  This effect is even more apparent once the time cost of an entire “volley” is taken into account. 
     *  Weapons are often fired in groups (called volleys), and the total cost of firing the entire group is applied at once (front-loaded, like all action costs in Cogmind):
     *  [Cost] [# Weapons]
     *  200	    1
     *  300	    2
     *  325	    3
     *  350	    4
     *  375	    5
     *  400	    6+
     *  
     *  
     *       ~ Turn Queue Explained ~
     * Where does the idea of a “turn” even fit in here?
     * 
     * A specific example will really help:
     * Say you have three events in your queue: Player [0], Enemy [0], and Turn [100]. The initiative is set in that order.
     * Player goes first, spends 120 time on their action, and the new queue order is Enemy [0], Turn [100], Player [120].
     * The first event in the queue is always the next one to take place, so Enemy goes next, and decides to perform an action that requires 50 time. 
     * The new queue order is Enemy [50], Turn [100], Player [120].
     * So now the enemy gets to act again because they are still at the front of the queue. This time they do something that requires 100 time. 
     * The new queue is now Turn [100], Player [120], Enemy [150].
     * At the front of the queue is… the Turn counter itself! So it handles any absolute turn updates, i.e. things that should happen “once per turn.” 
     * Then because each turn is set to be 100 time, the new queue is Player [120], Enemy [150], Turn [200].
     * So the player acts next, and so on… As you can see, the turn itself is an event/actor, just like the others. 
     * You can even add other types of events into the queue if you like, for example as of Beta 8 Cogmind has autonomous weapons that take their own actions independent of the turn counter or even their owner.
     * Action costs are repeatedly added to each actor and everyone’s time goes increasingly positive as they take actions.  
     * Technically if you have a lot of persistent actors over a very long period, you’ll want to consider eventually resetting the values 
     * by subtracting from every event the [time] value of the first event in the queue.
     * 
     * Inserting a new actor is as easy as adding them to the front of the queue and matching their current time value with that of the 
     * actor already at the front (or insert them elsewhere as appropriate if it’s desirable to delay their first action).
     * 
     *  << Source >>: https://www.gridsagegames.com/blog/2019/04/turn-time-systems/
     */
    #endregion

    public void LoadActors()
    {
        // Add all actors to the turn queue
        Actor[] allActors = FindObjectsOfType<Actor>();
        foreach (Actor actor in allActors)
        {
            actors.Add(actor);
        }
        // Sort actors by their initiative
        actors.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));
        // Start the first turn
        StartTurn();
    }

    private void StartTurn()
    {
        foreach (Actor actor in actors.ToList())
        {
            actor.StartTurn();
        }
    }

    public void EndTurn(Actor actor)
    {
        // -- TODO: Iron this out
        actor.EndTurn();
        actors.Remove(actor);
        Debug.Log(actors.Count + " left to act.");
        if (actors.Count == 0)
        {
            turnCounter++;
            AdvanceTime();
            Debug.Log($"Advancing time: T: {turnCounter} | G: {globalTime}");

            // add new actors to the turn queue (e.g. reinforcements)
            LoadActors();
            // ...
            // sort actors by their initiative
            actors.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));
            // start the next turn
            StartTurn();
        }
    }
    

    private void Update()
    {
        if(MapManager.inst && MapManager.inst.loaded)
        {
            if (doTimer)
                timer += Time.deltaTime;
            HandleClockTime();
            HandleRunTime();
        }
    }

    /// <summary>
    /// Advanced time by 1 turn
    /// </summary>
    public void AdvanceTime()
    {
        // Do stuff
        globalTime += 1;
        UIManager.inst.UpdateTimer(globalTime);
        GameManager.inst.MachineTimerUpdate();

        // Send out an event (used by various things)
        turnEvents.TurnTick();
    }

    /*
    public void EndTurn()
    {
        Debug.Log($"{actors[actorNum].name} ends its turn.");
        if (actors[actorNum].GetComponent<PlayerData>())
        {
            isPlayerTurn = false;
        }

        if (actorNum == actors.Count - 1)
        {
            actorNum = 0;
        }
        else
        {
            actorNum++;
        }
    }
    */

    public void AddActor(Actor actor)
    {
        actors.Add(actor);
    }

    public void InsertActor(Actor actor, int index)
    {
        actors.Insert(index, actor);
    }

    public void RemoveActor(Actor actor)
    {
        actors.Remove(actor);
    }

    #region Clock & Run

    public void HandleClockTime()
    {

    }

    public void HandleRunTime()
    {
        int minutes = Mathf.FloorToInt(timer / 60.0f);
        int seconds = Mathf.FloorToInt(timer - minutes * 60); // Unused
        int hours = Mathf.FloorToInt(timer / 60.0f / 60.0f);

        // Update the UI
        UIManager.inst.run_text.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    #endregion

    /// <summary>
    /// Go through all actors and refresh their visibility state.
    /// </summary>
    public void AllEntityVisUpdate(bool late = false)
    {
        if (late)
        {
            StartCoroutine(LateAllEntityVisUpdate());
        }
        else
        {
            foreach (Actor A in actors)
            {
                A.UpdateFieldOfView();
                A.CheckVisibility();
            }
        }
    }

    private IEnumerator LateAllEntityVisUpdate()
    {
        yield return null;
        
        foreach (Actor A in actors)
        {
            A.UpdateFieldOfView();
            A.CheckVisibility();
        }
    }

    public void SetAllUnknown()
    {
        foreach (KeyValuePair<Vector2Int,TileBlock> T in MapManager.inst._allTilesRealized)
        {
            T.Value.isExplored = false;
            T.Value.isVisible = false;

            // While we're here, we will assign tiles to their respective regions.

            // Find the region that contains this object
            int regionX = Mathf.FloorToInt((float)T.Key.x / MapManager.inst.regionSize);
            int regionY = Mathf.FloorToInt((float)T.Key.y / MapManager.inst.regionSize);

            Vector2Int pos = new Vector2Int(regionX, regionY);

            // Add the object to the corresponding region
            MapManager.inst.regions[pos].objects.Add(T.Value.gameObject);
        }

        foreach (KeyValuePair<Vector2Int, GameObject> T in MapManager.inst._layeredObjsRealized)
        {
            if (T.Value.GetComponent<AccessObject>()) // Access
            {
                T.Value.GetComponent<AccessObject>().isExplored = false;
                T.Value.GetComponent<AccessObject>().isVisible = false;
            }
            else if (T.Value.GetComponent<TileBlock>()) // Door
            {
                T.Value.GetComponent<TileBlock>().isExplored = false;
                T.Value.GetComponent<TileBlock>().isVisible = false;
            }
            else if (T.Value.GetComponent<MachinePart>()) // Machine
            {
                T.Value.GetComponent<MachinePart>().isExplored = false;
                T.Value.GetComponent<MachinePart>().isVisible = false;
            }
        }
    }
}
