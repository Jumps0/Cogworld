using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTile : MonoBehaviour
{
    [Tooltip("Mean Time To Happen (Turns)")]
    public int mtth;
    [Tooltip("Has this event been triggered to begin its countdown?")]
    public bool triggered = false;
    private bool countDown = false;
    [Tooltip("The turn when this event was initially triggered (countdown began).")]
    private int startTurn;

    [Header("Event Effects")]
    [Header("   -Add Dialogue Interaction")]
    public Actor dialogueTarget;
    public DialogueObject dialogue;
    [Header("   -Spawn Individual Bot")]
    public GameObject individualBot;
    [Header("   -Spawn Squad of Bots")]
    public GameObject squadLead;
    [Header("   -Give Trait")]
    [Tooltip("FARCOM, CRM, Imprinted, RIF, etc.")]
    public SpecialTrait trait;
    [Header("   -Reveal Secret Door")]
    public List<GameObject> secretWalls = new List<GameObject>();

    public void TriggerEvent()
    {
        triggered = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (triggered)
        {
            CountDown();
            if(!countDown)
            {
                startTurn = TurnManager.inst.globalTime;
                countDown = true;
            }
        }
    }

    private void CountDown()
    {
        triggered = true;

        if(TurnManager.inst.globalTime >= (startTurn + mtth))
        {
            DoEvent();
            triggered = false;
        }
    }

    private void DoEvent()
    {
        if(dialogueTarget != null)
        {
            // Set flags
            dialogueTarget.hasDialogue = true;
            dialogueTarget.finishedTalking = false;

            // Add dialogue to actor
            dialogueTarget.dialogue = dialogue;
        }

        if(trait.FARCOM || trait.imprinted || trait.CRM || trait.RIF)
        {
            PlayerData.inst.specialTrait = trait;
        }

        if(secretWalls.Count > 0)
        {
            foreach (GameObject S in secretWalls)
            {
                Vector2Int loc = HF.V3_to_V2I(S.transform.position);

                MapManager.inst.mapdata[loc.x, loc.y].SecretDoorReveal();
            }

            // Play a tile animation for each

            // Play a sound
            AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.dict_door[$"HEAVY_OPEN_{Random.Range(1,2)}"]); // "HEAVY_OPEN_1/2"
        }

        // TODO: More possible events!
        // -Spawning different levels/waves/types of bots
        // -???
    }

}
