using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class KnowledgeObject : ScriptableObject
{
   
    [Header("Overview")]
    public int Id;
    public KnowledgeDataType type;

    [TextArea(3, 5)]
    public string lore = "";    // Lore

    public bool knowByPlayer = false;
}

[System.Serializable]
public enum KnowledgeDataType
{
    Prototype,
    Bot,
    Lore,
    Misc
}