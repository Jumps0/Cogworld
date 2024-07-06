using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class QuestObject : ScriptableObject
{
   
    [Header("Overview")]
    public int Id;
    public string displayName;
    [TextArea(3,5)] public string description;

    [Header("Requirements")]
    public QuestObject[] prerequisites;

    [Header("Steps")]
    public GameObject[] steps;

    [Header("Rewards")]
    public List<Item> reward_items;
    public int reward_matter;

}