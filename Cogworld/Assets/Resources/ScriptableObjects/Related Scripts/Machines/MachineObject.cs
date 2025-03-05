using DungeonResources;
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
    public MachineType type = MachineType.Static;
    public AudioClip operationSound;

    [Header("Variants")]
    public MachineSymmetry symmetry;
    [Tooltip("All machines, regardless of symmetry, must have a variant that faces 'south', despite that not meaning much.")]
    public MachineBounds varSOUTH;
    [Tooltip("Partially symmetric machines and below must have an 'east' variant.")]
    public MachineBounds varEAST;
    public MachineBounds varNORTH;
    public MachineBounds varWEST;

    [Header("Explosion")]
    public bool explodes;
    public int explosion_heattransfer;
    public ExplosionGeneric explosion_details;
    public ItemExplosion explosion_potential;
    [Tooltip("If this machine explodes, what sound should play?")]
    public List<AudioClip> explosionSound = null;

    public Vector2Int Size(Direction direction = Direction.SO)
    {
        if(direction == Direction.SO)
        {
            return new Vector2Int(varSOUTH.sboundsTR.x - varSOUTH.sboundsBL.x, varSOUTH.sboundsTR.y - varSOUTH.sboundsBL.y);
        }
        else if (direction == Direction.EA)
        {
            return new Vector2Int(varEAST.sboundsTR.x - varEAST.sboundsBL.x, varEAST.sboundsTR.y - varEAST.sboundsBL.y);
        }
        else if (direction == Direction.NO)
        {
            return new Vector2Int(varNORTH.sboundsTR.x - varNORTH.sboundsBL.x, varNORTH.sboundsTR.y - varNORTH.sboundsBL.y);
        }
        else if (direction == Direction.WE)
        {
            return new Vector2Int(varWEST.sboundsTR.x - varWEST.sboundsBL.x, varWEST.sboundsTR.y - varWEST.sboundsBL.y);
        }

        return Vector2Int.zero;
    }

    public MachineBounds GetBounds(Direction direction)
    {
        if (direction == Direction.SO)
        {
            return varSOUTH;
        }
        else if (direction == Direction.EA)
        {
            return varEAST;
        }
        else if (direction == Direction.NO)
        {
            return varNORTH;
        }
        else if (direction == Direction.WE)
        {
            return varWEST;
        }

        return null;
    }
}

[System.Serializable]
[Tooltip("Provides information for where on the machine tilemap this variant can be found. Remember there is a unique tilemap for STATIC machines and one for INTERACTABLE machines.")]
public class MachineBounds
{
    [Header("Parent")]
    [Tooltip("The location of where the 'parent part' is, which contains the logic, and a list of its children.")]
    public Vector2Int parent;

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