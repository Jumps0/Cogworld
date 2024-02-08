using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Belongs to derelicts & npcs.
/// </summary>
public class AI_Friendly : MonoBehaviour
{
    [Header("Dialogue Related")]
    public bool hasDialogue = false;
    public bool hasBufferDialogue = false; // Will freeze the screen if true. If false, just appears at the bottom + log.
    public bool talking = false; // Is this bot currently chatting with the player?
    public bool finishedTalking = false; // Has this bot finished chatting with the player?
    public List<DialogueC> dialogue = new List<DialogueC>();
    public bool moveToNextDialogue = false;
    public AudioClip uniqueDialogueSound = null;
    public string uniqueName;

    [Header("Info")]
    public BotObject botInfo;
    public FriendlyBotType _type;
    //public FriendlyBotState _state;

    public Actor squadLeader;

    [Header("Relevant Actions")]
    public bool isTurn = false;
    public int memory = 0;

    private void Start()
    {
        this.botInfo = GetComponent<Actor>().botInfo;
    }

    public void TakeTurn()
    {
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            return;
        }

        StartCoroutine(TakeTurnDebug());
    }

    IEnumerator TakeTurnDebug()
    {
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            yield break;
        }

        this.GetComponent<Actor>().UpdateFieldOfView();

        yield return null;

        Action.SkipAction(this.GetComponent<Actor>()); // fallback condition
    }


    bool LOSOnPlayer()
    {
        bool LOS = true;

        // - If line of sight being blocked - // (THIS ALSO GETS USED LATER)
        Vector2 targetDirection = PlayerData.inst.transform.position - this.transform.position;
        float distance = Vector2.Distance(Action.V3_to_V2I(this.transform.position), Action.V3_to_V2I(PlayerData.inst.transform.position));

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(this.transform.position.x, this.transform.position.y), targetDirection.normalized, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            DoorLogic door = hit.collider.GetComponent<DoorLogic>();
            MachinePart machine = hit.collider.GetComponent<MachinePart>();

            // If we encounter:
            // - A wall
            // - A closed door
            // - A machine

            // Then there is no LOS

            if(tile != null && tile.tileInfo.type == TileType.Wall)
            {
                return false;
            }

            if(door != null && tile.specialNoBlockVis == true)
            {
                LOS = true;
            }
            else if (door != null && tile.specialNoBlockVis == false)
            {
                return false;
            }

            if(machine != null)
            {
                return false;
            }
        }

        return LOS;
    }

    public IEnumerator PerformScriptedDialogue()
    {
        talking = true;

        string botName = this.botInfo.name;
        if (this.GetComponent<AI_Friendly>() && this.GetComponent<AI_Friendly>().uniqueName != "")
            botName = this.GetComponent<AI_Friendly>().uniqueName;
        UIManager.inst.Dialogue_OpenBox(botName, this.GetComponent<Actor>());

        // Wait for the box to open (and do its little animation)
        while (!UIManager.inst.dialogue_readyToDisplay)
        {
            yield return null;
        }

        foreach (DialogueC text in dialogue)
        {
            bool moreToSay = true;
            if(text.id == dialogue.Count)
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

}

[System.Serializable]
public enum FriendlyBotType
{
    Scripted,
    Ally,
    Drone,
    NPC,
    Misc
}
/*
[System.Serializable]
public enum FriendlyBotState
{
    Working,  // Gaurding, Patrolling, etc | This bot's normal activity
    Hunting,  // Engaging or Looking for the player
    Returning,// Going back to normal routine
    Idle,
    Misc
}
*/