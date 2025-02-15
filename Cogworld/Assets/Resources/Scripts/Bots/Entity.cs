// Origionally made by: Chizaruu @ https://github.com/Chizaruu/Unity-RL-Tutorial/blob/part-4-field-of-view/Assets/Scripts/Entity/Entity.cs
// Expanded & Modified by: Cody Jackson @ codyj@nevada.unr.edu

using NUnit;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// A generic class to represent players, enemies, items, etc. Contains the NetworkBehavior
/// </summary>
public class Entity : NetworkBehaviour
{
    [Header("Network Values")]
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>(readPerm:NetworkVariableReadPermission.Everyone, writePerm:NetworkVariableWritePermission.Owner);

    public bool blocksMovement;
    public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }

    [Header("Bot Values")]
    public string uniqueName;
    // - Health
    public int currentHealth;
    public int maxHealth;
    // - Heat
    public int currentHeat = 0;
    public int heatDissipation;
    [Tooltip("In some instances (like a volley), heat creation is spread a cross multiple turns, that is stored here, and updated at the end of every bot's turn.")]
    public List<float> residualHeat = new List<float>();
    // - Energy
    public int energyGeneration;
    public int currentEnergy = 100; // ?
    public int maxEnergy = 250;
    [Tooltip("Goes from 1f to 0f.")]
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

    #region Network Setup
    public override void OnNetworkSpawn()
    {
        if (IsOwner && this.GetComponent<PlayerGridMovement>())
        {
            PlayerGridMovement pgm = this.GetComponent<PlayerGridMovement>();
            pgm.ent = this;

            pgm.inputActions = Resources.Load<InputActionsSO>("Inputs/InputActionsSO").InputActions;

            pgm.inputActions.Player.Move.performed += pgm.OnMovePerformed;
            pgm.inputActions.Player.Move.canceled += pgm.OnMoveCanceled;
            pgm.inputActions.Player.LeftClick.performed += pgm.OnLeftClick;
            pgm.inputActions.Player.RightClick.performed += pgm.OnRightClick;
            pgm.inputActions.Player.Quit.performed += pgm.OnQuit;
            pgm.inputActions.Player.Autocomplete.performed += pgm.OnAutocomplete;
            pgm.inputActions.Player.Volley.performed += pgm.OnVolley;

            // Random position for testing purposes
            Move(new Vector2Int(Random.Range(-1, 1), Random.Range(-1, 1)));
        }
    }

    private void OnDisable()
    {
        if (IsOwner && this.GetComponent<PlayerGridMovement>())
        {
            PlayerGridMovement pgm = this.GetComponent<PlayerGridMovement>();

            pgm.inputActions.Player.Move.performed -= pgm.OnMovePerformed;
            pgm.inputActions.Player.Move.canceled -= pgm.OnMoveCanceled;
            pgm.inputActions.Player.LeftClick.performed -= pgm.OnLeftClick;
            pgm.inputActions.Player.RightClick.performed -= pgm.OnRightClick;
            pgm.inputActions.Player.Quit.performed -= pgm.OnQuit;
            pgm.inputActions.Player.Autocomplete.performed -= pgm.OnAutocomplete;
            pgm.inputActions.Player.Volley.performed -= pgm.OnVolley;
        }
    }
    #endregion

    public void Move(Vector2 direction)
    {
        // Move character
        transform.position += (Vector3)direction;
        Position.Value = transform.position;

        /* // !TEMP-REMOVE
        // Update momentum
        if (lastDirection == direction)
        {
            momentum++;
        }
        else
        {
            momentum = 0;

            if (this.GetComponent<PlayerData>())
            {
                InventoryControl.inst.PartsSortingCheck(); // Check to see if the inventory needs to be auto-sorted.
                //GameManager.inst.UpdateNearbyVis(); // Update nearby vision on objects
            }
        }
        lastDirection = direction;

        // Update any nearby doors
        Vector2Int newLocation = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        GameManager.inst.LocalDoorUpdate(newLocation);

        // Matter Check (if player)
        if (this.GetComponent<PlayerData>())
        {
            SpecialPickupCheck(newLocation);
        }

        this.GetComponent<Actor>().UpdateFieldOfView(); // Update their FOV
        */
    }

    public void AddToGameManager()
    {
        GameManager.inst.Entities.Add(this);
    }

    /// <summary>
    /// Checks if the player has moved ontop of a special unique item, such-as: Matter, Data Logs, Scrap, etc.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    public void SpecialPickupCheck(Vector2Int pos)
    {
        // 1. Is there an item at this position?
        Part P = HF.TryFindPartAtLocation(pos);
        if(P != null)
        {
            // 2. Is this a unique item?
            Item item = P._item;
            if (item != null && item.itemData.instantUnique)
            {
                // 3. What type of unique item is this?
                if (item.uniqueDetail.isScrap) // Scrap
                {

                }
                else if (item.uniqueDetail.isDataLog) // Data Log
                {

                }
                else if (item.uniqueDetail.isDataCore) // Data Core
                {

                }
                else // Matter
                {
                    // 4. Does the player have spare space for matter?
                    if(PlayerData.inst.currentMatter < PlayerData.inst.maxMatter) // Add it to the normal storage
                    {
                        int diff = (PlayerData.inst.maxMatter - PlayerData.inst.currentMatter);
                        if (diff >= item.amount) // Player can pick-up all of this matter
                        {
                            PlayerData.inst.currentMatter += item.amount; // Add it to inventory
                            Destroy(P.gameObject); // Destroy the part
                        }
                        else if (diff < item.amount && diff != 0) // Player can only pick-up some of this matter
                        {
                            PlayerData.inst.currentMatter += diff; // Add it to inventory
                            item.amount -= diff;
                            P.SetMatterColors(); // May need to change to color of the ground item.
                        }

                        UIManager.inst.UpdatePSUI();
                        UIManager.inst.CreateNewLogMessage(($"Aquired {item.amount} Matter."), UIManager.inst.activeGreen, UIManager.inst.dullGreen, false, true);
                    }
                    else if (PlayerData.inst.maxInternalMatter > 0 && PlayerData.inst.currentInternalMatter < PlayerData.inst.maxInternalMatter) // Add it to internal storage
                    {
                        // A bit clunky but we do it this way.
                        
                        // Collect up all items
                        List<Item> items = Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>());

                        // Collect up all *matter* storage items.
                        List<Item> storage = new List<Item>();
                        foreach (var I in items)
                        {
                            foreach (var E in I.itemData.itemEffects)
                            {
                                if(E.internalStorageEffect.hasEffect && E.internalStorageEffect.internalStorageType == 0)
                                {
                                    storage.Add(I);
                                }
                            }
                        }

                        int toAdd = item.amount; // In total, we need to add this much matter to our internal storage.
                        foreach (var S in storage) // Go through each item, and start filling them up until we have none left.
                        {
                            int storageSize = 0;
                            foreach (var E in S.itemData.itemEffects)
                            {
                                if (E.internalStorageEffect.hasEffect)
                                {
                                    storageSize = E.internalStorageEffect.capacity; // Get the storage size
                                }
                            }

                            if (S.storageAmount < storageSize) // Is there space in this storage item?
                            {
                                int space = storageSize - S.storageAmount; // This individual item can hold this much.

                                if (space >= toAdd) // We can fit all of it
                                {
                                    S.storageAmount += toAdd;
                                    PlayerData.inst.currentInternalMatter += toAdd;
                                }
                                else // Can't fit it all
                                {
                                    S.storageAmount = storageSize; // Fill up this item
                                    PlayerData.inst.currentInternalMatter += space; // Add bits to player
                                    toAdd -= space; // Subtract from what we have left
                                }
                            }

                            if(toAdd <= 0) // Any left?
                            {
                                // Stop!
                                break;
                            }
                        }

                        // Did we add everything? (yea i know we are checking twice)
                        if(toAdd <= 0) // Yes! (Delete the matter)
                        {
                            Destroy(P.gameObject); // Destroy the part
                        }
                        else // No. Reduce the Matter
                        {
                            item.amount = toAdd;
                            P.SetMatterColors(); // May need to change to color of the ground item.
                        }

                        return; // bail out, we are done
                    }
                }
            }
        }

    }
}
