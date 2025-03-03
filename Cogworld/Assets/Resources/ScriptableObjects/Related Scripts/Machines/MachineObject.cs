using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


[System.Serializable]
public abstract class MachineObject : ScriptableObject
{
   
    [Header("Overview")]
    [Tooltip("A generic ID for this type of quest.")]
    public int Id;
    public string displayName;
    [TextArea(3,5)] public string description;

    

}
