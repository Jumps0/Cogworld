using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
/// <summary>
/// A generic AI script for every type of bot.
/// </summary>
public class BotAI : MonoBehaviour
{
    [Header("Info")]
    private BotObject botInfo;
    private Allegance allegances;

    [Header("Squad")]
    [Tooltip("List of all squad members")]
    public List<Actor> squad = new List<Actor>();
    [Tooltip("The squad leader")]
    public Actor squadLeader;

    [Header("Relevant Actions")]
    public BotAIState state = BotAIState.Working;
    public bool isTurn = false;
    public int memory = 0;

    [Header("Pathing")]
    public List<Vector2Int> pointsOfInterest = new List<Vector2Int>(); // Bot will visit these

    private void Start()
    {
        this.botInfo = GetComponent<Actor>().botInfo;
        this.allegances = GetComponent<Actor>().allegances;
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
        // - Don't do anything if this actor isn't "awake" (or able to reasonably take their turn)
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            return;
        }

        this.GetComponent<Actor>().UpdateFieldOfView();

        StartCoroutine(DecideTurnAction());
    }

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

        Action.SkipAction(this.GetComponent<Actor>()); // Fallback condition
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
                List<GameObject> neighbors = this.GetComponent<Actor>().FindNeighbors((int)this.transform.position.x, (int)this.transform.position.y);

                List<GameObject> validMoveLocations = new List<GameObject>();

                foreach (var T in neighbors)
                {
                    if (this.GetComponent<Actor>().IsUnoccupiedTile(T.GetComponent<TileBlock>()))
                    {
                        validMoveLocations.Add(T);
                    }
                }

                GameObject fleeLocation = this.GetComponent<Actor>().GetBestFleeLocation(fleeSource.gameObject, this.gameObject, validMoveLocations);

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

        string botName = this.botInfo.name;
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
    Idle,
    Misc
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
