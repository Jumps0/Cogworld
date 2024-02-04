// Origionally made by: Chizaruu @ https://github.com/Chizaruu/Unity-RL-Tutorial/blob/part-4-field-of-view/Assets/Scripts/Entity/Entity.cs
// Expanded & Modified by: Cody Jackson @ codyj@nevada.unr.edu

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour
{
    public bool blocksMovement;
    public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }

    [Header("Bot Values")]
    public int currentHealth;
    public int maxHealth;
    //
    public int currentHeat = 0;
    public int heatDissipation;
    public int energyGeneration;
    public int currentMatter = 0; // Probably won't matter?
    //
    public bool siegeMode = false;
    public bool inStatis = false;
    //
    [Tooltip("This bot hasn't moved for the past X times it could have.")]
    public int noMovementFor = 0;

    public void Move(Vector2 direction)
    {
        transform.position += (Vector3)direction;
    }

    public void AddToGameManager()
    {
        GameManager.inst.Entities.Add(this);
    }
}
