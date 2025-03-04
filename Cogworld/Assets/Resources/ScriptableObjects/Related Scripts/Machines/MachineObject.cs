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

    [Header("Information")]
    public MachineType type;
    public AudioClip operationSound;

    [Header("Variants")]
    public MachineSymmetry symmetry;
    [Tooltip("All machines, regardless of symmetry, must have a variant that faces 'south', despite that not meaning much.")]
    public MachineBounds varSOUTH;
    [Tooltip("Partially symmetric machines and below must have an 'east' variant.")]
    public MachineBounds varEAST;
    public MachineBounds varNORTH;
    public MachineBounds varWEST;

}

[System.Serializable]
[Tooltip("Provides information for where on the machine tilemap this variant can be found. Remember there is a unique tilemap for STATIC machines and one for INTERACTABLE machines.")]
public class MachineBounds
{
    [Header("Standard Visual")]
    [Tooltip("The bottom left corner of this machine.")]
    public Vector2Int sboundsBL;
    [Tooltip("The top right corner of this machine.")]
    public Vector2Int sboundsTR;

    [Header("ASCII Visual")]
    [Tooltip("The bottom left corner of this machine (ASCII).")]
    public Vector2Int aboundsBL;
    [Tooltip("The top right corner of this machine (ASCII).")]
    public Vector2Int aboundsTR;
}

[System.Serializable]
public enum MachineSymmetry
{
    [Tooltip("This machine is fully symmetrical along both axes, having only one unique variant.")]
    FullSymmetry,
    [Tooltip("This machine is symmetrical along one axis, having two unique variants.")]
    PartialSymmetry,
    [Tooltip("This machine is not symmetrical and has four unique variants.")]
    Unique
}