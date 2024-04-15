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
    public int currentEnergy = 100; // ?
    public float corruption = 0.0f;
    //
    public bool siegeMode = false;
    public bool inStatis = false;
    //
    public int speed = 0;
    //
    public int weightCurrent;
    public int weightMax;
    //
    public int salvageModifier = 0; // TODO: (See in manual under Salvage: https://www.gridsagegames.com/cogmind/manual.txt)
    //
    [Tooltip("This bot hasn't moved for the past X times it could have.")]
    public int noMovementFor = 1;
    [Tooltip("If a bot continues to move in the same direction multiple times, their 'momentum' increases.")]
    public int momentum = 0;
    private Vector2 lastDirection = Vector2.zero;

    [Header("Inventories")]
    public InventoryObject armament;
    public InventoryObject components;
    public InventoryObject inventory;

    public void Move(Vector2 direction)
    {
        // Move character
        transform.position += (Vector3)direction;

        // Update momentum
        if(lastDirection == direction)
        {
            momentum++;
        }
        else
        {
            momentum = 0;
        }
        lastDirection = direction;
    }

    public void AddToGameManager()
    {
        GameManager.inst.Entities.Add(this);
    }
}
