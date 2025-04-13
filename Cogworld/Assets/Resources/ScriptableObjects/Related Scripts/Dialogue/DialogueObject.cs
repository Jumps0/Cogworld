using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class DialogueObject : ScriptableObject
{
    [Header("Overview")]
    public int Id;
    [Tooltip("Name of the bot performing this dialogue.")]
    public string speakerName;
    public DialogueType type;


    [Tooltip("The backend name for this conversation.")]
    public string conversationName;

    [TextArea(3,5)]
    [Tooltip("List of all dialogue in the sequence.")]
    public List<string> dialogue;

}

[System.Serializable]
public enum DialogueType
{
    [Tooltip("Communicated via a dialogue box. Interrupts gameplay.")]
    Major,
    [Tooltip("Communicated via bottom screen message. Does not interrupt gameplay.")]
    Minor
}