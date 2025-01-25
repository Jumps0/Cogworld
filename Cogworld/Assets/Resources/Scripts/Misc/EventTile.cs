using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTile : MonoBehaviour
{
    // NOTE:
    // Due to a quirk in map generation, DO NOT put a floor tile underneath an event tile in the prefab.
    // It will cause an error.
    // Assume that a floor tile will always be placed under event tile.
    // Because of this DO NOT put event tiles in walls or objects.

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
    public List<string> dialogue;
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
            dialogueTarget.GetComponent<BotAI>().hasDialogue = true;
            dialogueTarget.GetComponent<BotAI>().finishedTalking = false;

            // Add lines of dialogue to actor
            int i = 1;
            foreach (string line in dialogue)
            {
                dialogueTarget.GetComponent<BotAI>().dialogue.Add(new DialogueC(i, line));
                i++;
            }
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

                if (MapManager.inst._allTilesRealized.ContainsKey(loc))
                {
                    MapManager.inst._allTilesRealized[loc].bottom.tileInfo = MapManager.inst.tileDatabase.Tiles[4]; // Replace type with floor tile
                    MapManager.inst._allTilesRealized[loc].bottom.Init(); // Force the tile to update
                    MapManager.inst._allTilesRealized[loc].bottom.SecretDoorReveal(); // Make it play its reveal animation
                }
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
