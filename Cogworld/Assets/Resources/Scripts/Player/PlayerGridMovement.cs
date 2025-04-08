using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerGridMovement : MonoBehaviour
{
    /// <summary>
    /// Is the player allowed to move?
    /// </summary>
    public bool playerMovementAllowed = true;
    public bool inDialogueSequence = false;
    public bool isMoving;
    private Vector3 originPos, targetPos;
    private float timeToMove = 0.2f;

    [Tooltip("Use non-smooth \"instant\" tile movement.")]
    public bool doInstantMovement = true;

    #region Input Registering
    public InterfacingMode interfacingMode = InterfacingMode.COMBAT;
    public PlayerInputActions inputActions;
    private Vector2 moveInput;

    private void OnEnable()
    {
        inputActions = Resources.Load<InputActionsSO>("Inputs/InputActionsSO").InputActions;

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        //inputActions.Player.LeftClick.performed += OnLeftClick;
        //inputActions.Player.RightClick.performed += OnRightClick;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        //inputActions.Player.LeftClick.performed -= OnLeftClick;
        //inputActions.Player.RightClick.performed -= OnRightClick;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        HandleMovement(moveInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void UpdateInterfacingMode(InterfacingMode mode)
    {
        interfacingMode = mode;
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.scroll.ReadValue().y != 0f)
        {
            TrySkipTurn();
        }

        if (this.gameObject.GetComponent<PartInventory>())
        {
            InventoryInputDetection();
        }
    }

    #region Turn Skipping
    private bool skipTurnCooldown = false;
    private float skipTurnCooldownAmount = 0.1f;
    private Coroutine skipTurnCooldownRoutine = null;

    private void TrySkipTurn()
    {
        if (skipTurnCooldown)
        {
            // Cooldown done, go for it.
            Action.SkipAction(this.GetComponent<Actor>());
            skipTurnCooldown = false;
        }
        else // First time confirm
        {
            // Start the cooldown
            if(skipTurnCooldownRoutine != null)
            {
                StopCoroutine(skipTurnCooldownRoutine);
            }
            skipTurnCooldownRoutine = StartCoroutine(SkipTurnCooldown());
        }
    }

    private IEnumerator SkipTurnCooldown()
    {
        yield return new WaitForSeconds(skipTurnCooldownAmount);

        skipTurnCooldown = true;
    }
    #endregion

    #region Movement
    private void HandleMovement(Vector2 direction)
    {
        if (isMoving || !playerMovementAllowed || inDialogueSequence || !GetComponent<Actor>().isAlive || interfacingMode != InterfacingMode.COMBAT) return;

        int x = Mathf.RoundToInt(direction.x);
        int y = Mathf.RoundToInt(direction.y);

        if (x != 0 || y != 0)
        {
            AttemptMovement(x, y);
        }
    }

    public void OnToggleMovementMode(InputValue value)
    {
        if(interfacingMode == InterfacingMode.COMBAT)
        {
            GameManager.inst.allowMouseMovement = !GameManager.inst.allowMouseMovement;
            GridManager.inst.ClearGridOfHighlightColor(UIManager.inst.dullGreen);
        }
    }

    // Future movement QoL: https://www.gridsagegames.com/blog/2019/11/movement-qol/
    [SerializeField] private bool moveKeyHeld;
    public void AttemptMovement(int X, int Y)
    {
        // Before we try anything, we need to make sure the player can *afford* to move since there is usually always a movement cost.
        if (!HF.HasResourcesToMove(this.GetComponent<Actor>()))
        {
            return;
        }

        // Attempt to move in the *direction* described by X & Y
        // So we need to add that to our current position to obtain where we want to end up

        isMoving = true;

        Vector2Int currentPos = HF.V3_to_V2I(PlayerData.inst.transform.position);
        Vector2Int moveTarget = new Vector2Int(currentPos.x + X, currentPos.y + Y); // This is where we want to move to

        // -- Machine interaction detection --
        Vector2Int machineInteraction = Vector2Int.zero;

        if (MapManager.inst.mapdata[moveTarget.x, moveTarget.y].type == TileType.Machine) // Tile is type of machine?
        {
            WorldTile machineTile = MapManager.inst.mapdata[moveTarget.x, moveTarget.y];

            // Now check if this tile is viable to interact with
            bool MACHINE_BROKEN = machineTile.machinedata.machineIsDestroyed;
            bool MACHINE_LOCKED = machineTile.machinedata.locked;
            bool MACHINE_INTERACTABLE = (machineTile.machinedata.type != MachineType.Static && machineTile.machinedata.type != MachineType.None);

            if(!MACHINE_BROKEN && !MACHINE_LOCKED && MACHINE_INTERACTABLE)
            {
                machineInteraction = moveTarget;
            }
        }

        List<WorldTile> neighbors = HF.FindNeighbors(currentPos.x, currentPos.y);

        WorldTile desiredDestinationTile = MapManager.inst.mapdata[0,0]; // Set as default value because later code requires one!

        foreach (WorldTile t in neighbors) // Find which of the neighbors we want to try and move to.
        {
            Vector2Int pos = t.location;
            if(moveTarget == pos)
            {
                desiredDestinationTile = t;
            }
        }

        // ------------------------------------------------

        if (desiredDestinationTile.location == Vector2Int.zero)
        {
            Debug.LogError("ERROR: Desired destination tile for movement is null! <PlayerGridMovement.cs>");
            return;
        }

        if (desiredDestinationTile.type != TileType.Exit)
        {
            if (HF.LocationUnoccupied(desiredDestinationTile.location)) // Space is Clear (Move!)
            {
                moveKeyHeld = Action.BumpAction(GetComponent<Actor>(), new Vector2(X, Y));

                if (desiredDestinationTile.type == TileType.Trap)
                {
                    // There is a trap here!
                    if(HF.RelationToTrap(this.GetComponent<Actor>(), desiredDestinationTile) != BotRelation.Friendly && desiredDestinationTile.trap_active && !desiredDestinationTile.trap_tripped)
                    {
                        HF.AttemptTriggerTrap(desiredDestinationTile, this.GetComponent<Actor>());
                    }
                }
            }
            else
            {
                UIManager.inst.ShowCenterMessageTop("Invalid move", Color.black, UIManager.inst.alertRed);
            }
        }
        else // "About to Leave Area" (Walking into exit)
        {
            ConfirmLeaveArea(desiredDestinationTile);
        }

        // -- Machine Interaction (Parsing) --
        if (machineInteraction != Vector2Int.zero) // A Machine to interact with
        {
            WorldTile machineTile = MapManager.inst.mapdata[moveTarget.x, moveTarget.y];
            MachineType type = machineTile.machinedata.type;

            if (type == MachineType.CustomTerminal)
            {
                UIManager.inst.CTerminal_Open(machineInteraction);
            }
            else if(type == MachineType.Garrison)
            {
                if (!machineInteraction.GetComponent<Garrison>().g_sealed)
                {
                    // Create log messages
                    UIManager.inst.CreateNewLogMessage("Connecting with Garrison Access...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                    UIManager.inst.Terminal_OpenGeneric(machineInteraction);
                }
            }
            else
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage($"Connecting with {HF.GetMachineTypeAsString(type)}...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
        }
        // -----------------------------------------

        isMoving = false;
    }

    #endregion

    #region Leave Area 

    [SerializeField] private bool confirmLeaveLevel = false;
    [SerializeField] private float leaveLevelCooldown = 5f;

    private void ConfirmLeaveArea(WorldTile target)
    {
        if (confirmLeaveLevel)
        {
            // Already confirmed, time to leave!
            MapManager.inst.ChangeMap(target.access_destination, target.access_branch);
        }
        else // First time confirm
        {
            // Player a warning message
            UIManager.inst.ShowCenterMessageTop("About to leave area! Confirm direction", UIManager.inst.dangerRed, Color.black);
            // Start the timer
            StartCoroutine(ConfirmCooldown());
        }
    }

    private IEnumerator ConfirmCooldown()
    {
        confirmLeaveLevel = true;

        yield return new WaitForSeconds(leaveLevelCooldown);

        confirmLeaveLevel = false;
    }

    #endregion

    #region Input Handling

    public void OnEnter(InputValue value)
    {
        if (!GetComponent<Actor>().isAlive || interfacingMode != InterfacingMode.COMBAT) return;

        // Player must be on top of a tile with an item on it to be able to interact with it
        Vector2Int playerPos = HF.V3_to_V2I(this.transform.position);
        if (!InventoryControl.inst.worldItems.ContainsKey(playerPos))
        {
            return;
        }

        // There is an item here? Allow the user to pick it up

        // Need to check for:
        // - Player clicking on the item (clicking on themselves really)
        // See OnLeftClick() below

        // - Player hitting enter
        InventoryControl.inst.worldItems[playerPos].GetComponent<Part>().TryEquipItem(); // Try equipping it
        //InventoryControl.inst.DebugPrintInventory();
    }

    // -- Handle [LEFT] Clicks
    public void OnLeftClick(InputValue value)
    {
        if (!GetComponent<Actor>().isAlive) return;

        // -- Combat --
        if(this.GetComponent<PlayerData>().doTargeting)
            this.GetComponent<PlayerData>().HandleMouseAttack();

        #region /DATA/ menu
        if (UIManager.inst.dataMenu.data_parent.gameObject.activeInHierarchy)
        {
            if(UIManager.inst.dataMenu.data_focusObject == null && UIManager.inst.dataMenu.data_onTraits == false && UIManager.inst.dataMenu.data_onAnalysis == false)
            {
                UIManager.inst.Data_CloseMenu();
            }
            else if (UIManager.inst.dataMenu.data_focusObject != null) // Open the special detail menu instead
            {
                if (!UIManager.inst.dataMenu.data_extraDetail.activeInHierarchy) // Menu isn't already open
                {
                    UIManager.inst.dataMenu.data_extraDetail.GetComponent<UIDataExtraDetail>().ShowExtraDetail(UIManager.inst.dataMenu.data_focusObject.extraDetailString);
                }
            }
            else if (UIManager.inst.dataMenu.data_onTraits && UIManager.inst.dataMenu.selection_obj != null) // Open the traits menu
            {
                if (!UIManager.inst.dataMenu.data_traitBox.activeInHierarchy) // Menu isn't already open
                {
                    UIManager.inst.dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Open();
                }
            }
            else if (UIManager.inst.dataMenu.data_onAnalysis && UIManager.inst.dataMenu.selection_obj != null) // Open the analysis menu
            {
                if (!UIManager.inst.dataMenu.data_traitBox.activeInHierarchy) // Menu isn't already open
                {
                    UIManager.inst.dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Open();
                }
            }
        }

        #endregion

        // Get mouse position
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Check if mouse overlaps with the player
        if (PlayerData.inst.GetComponent<BoxCollider2D>().OverlapPoint(mousePos))
        {
            // If the user left clicks on themselves, they are trying to pick up an item beneath them.
            Vector2Int playerPos = HF.V3_to_V2I(this.transform.position);
            if (!InventoryControl.inst.worldItems.ContainsKey(playerPos))
            {
                return;
            }

            InventoryControl.inst.worldItems[playerPos].GetComponent<Part>().TryEquipItem(); // Try equipping it

            return; // Finished
        }
        else // Okay maybe they clicked on something else, lets find out.
        {
            GameObject target = HF.GetTargetAtPosition(new Vector2Int((int)mousePos.x, (int)mousePos.y));

            // What did we get?
            if(target == null) {} // Nothing
            else if (target.GetComponent<Actor>()) // A bot
            {
                // If left clicked, the bot should have some special interaction if it has a quest point
                Actor a = target.GetComponent<Actor>();
                QuestPoint quest = HF.ActorHasQuestPoint(a);

                if (quest != null && quest.CanInteract()) // Has a quest that can be interacted with
                {
                    if (Vector2.Distance(a.transform.position, PlayerData.inst.transform.position) < 1.2f) // Adjacency check (Expensive so we do it last)
                    {
                        quest.Interact();
                    }

                    // Finished
                    return;
                }
            }
            else if (target.GetComponent<TileBlock>()) // Some kind of structure
            {
                // ??

                // Finished
                return;
            }
        }

        // If there is nothing else happening, consider mouse movement
        // NOTE: This feels kinda bad to put here (at the end) considering how common this may be. Try and re-organize this later.
        if (!playerMovementAllowed || !GameManager.inst.allowMouseMovement || GridManager.inst.astar == null) return;

        if (GridManager.inst.astar.path.Count > 0 &&
            GridManager.inst.astar.searchStatus == AStarSearchStatus.Success)
        {
            Vector2Int playerPos = HF.V3_to_V2I(this.transform.position);
            WorldTile tile = MapManager.inst.mapdata[playerPos.x, playerPos.y];
            int lastItem = GridManager.inst.astar.path.Count - 2;
            int targetX = GridManager.inst.astar.path[lastItem].X - tile.location.x;
            int targetY = GridManager.inst.astar.path[lastItem].Y - tile.location.y;

            AttemptMovement(targetX, targetY);
            GridManager.inst.ClearGridOfHighlightColor(UIManager.inst.dullGreen);
        }
    }

    // -- Handle [RIGHT] Clicks
    public void OnRightClick(InputValue value)
    {
        if (!GetComponent<Actor>().isAlive) return;

        // -- Combat Targeting --
        if (this.GetComponent<PlayerData>().canDoTargeting)
        {
            // Toggle
            this.GetComponent<PlayerData>().doTargeting = !this.GetComponent<PlayerData>().doTargeting;
        }
        // --                  --

        // -- Terminal Extra Detail Tooltip --
        if (UIManager.inst.terminal_targetresultsAreaRef.activeInHierarchy) // We must be in the menu
        {
            UIManager.inst.Terminal_TryExtraDetail(); // Transfer over logic to UIManager to make things easier
        }

        // Get mouse position
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Check if mouse overlaps with the player
        if (PlayerData.inst.GetComponent<BoxCollider2D>().OverlapPoint(mousePos))
        {
            // If the user right clicks on themselves, we need to display their stats in the left side window.
            // https://www.gridsagegames.com/blog/2024/01/full-ui-upscaling-part-2-holy-mockups/

            // TODO: SHOW PLAYER INFO
            Debug.Log("THIS FUNCTIONALITY HAS NOT BEEN IMPLEMENTED YET.");
        }
        else // Okay maybe they clicked on something else, lets find out.
        {
            Vector2Int mPos = new Vector2Int((int)(mousePos.x + 0.5f), (int)(mousePos.y + 0.5f)); // Adjustment due to tiles being offset slightly from natural grid

            GameObject target = HF.GetTargetAtPosition(mPos);

            // If they click on a Bot, Item, or Machine, the /DATA/ window should open
            if (target == null || UIManager.inst.dataMenu.isAnimating) { return; } // Nothing, bail out (or the menu is in the process of opening or closing)

            // Forcefully exit out of targeting mode since we are about to open this window
            this.GetComponent<PlayerData>().doTargeting = false;

            if (target.GetComponent<Actor>()) // A bot
            {
                // If right clicked, the /DATA/ menu should open and display info about the bot.
                Actor a = target.GetComponent<Actor>();
                UIManager.inst.Data_OpenMenu(null, a.gameObject, a); // Open the /DATA/ menu
            }
            else if (target.GetComponent<Part>()) // An item
            {
                // If right clicked, the /DATA/ menu should open and display info about the item.
                Part p = target.GetComponent<Part>();
                UIManager.inst.Data_OpenMenu(p._item);
            }
            else if (target.GetComponent<MachinePart>()) // A machine
            {
                // If right clicked, the /DATA/ menu should open and display info about the machine.
                MachinePart m = target.GetComponent<MachinePart>();
                UIManager.inst.Data_OpenMenu(null, m.gameObject);
            }
        }
    }

    /// <summary>
    /// aka the ESCAPE key
    /// </summary>
    /// <param name="value"></param>
    public void OnQuit(InputValue value)
    {
        // - Check to close Terminal window -
        if (UIManager.inst.terminal_targetTerm != null) // Window is open
        {
            if (UIManager.inst.terminal_activeIField == null) // And the player isn't in the input window
            {
                UIManager.inst.Terminal_CloseAny(); // The close the window
            }
            else // If it does exist we need to tell it escape was pressed incase it should be closed out of.
            {
                UIManager.inst.terminal_activeIField.GetComponent<UIHackInputfield>().Input_Escape();
            }
        }

        // - Check to close the /DATA/ window -
        if (UIManager.inst.dataMenu.data_parent.gameObject.activeInHierarchy)
        {
            UIManager.inst.Data_CloseMenu();
        }
    }

    /// <summary>
    /// aka the V key. Used for EVASION - Volley mode switching. See `Evasion_VolleyModeFlip()` inside UIManager for more details.
    /// </summary>
    /// <param name="value"></param>
    public void OnVolley(InputValue value)
    {
        // Here we are just checking if the player presses the "V" key or not. This activates a special visual, and also changes the color of the "V".
        if ((UIManager.inst.volleyMain.activeInHierarchy || UIManager.inst.volleyTiles.Count > 0) && !UIManager.inst.volleyAnimating)
        {
            UIManager.inst.Evasion_VolleyModeFlip();
        }
    }

    /// <summary>
    /// aka the TAB key. Used for Autocompleting text while hacking
    /// </summary>
    /// <param name="value"></param>
    public void OnAutocomplete(InputValue value)
    {
        if (UIManager.inst.terminal_targetTerm != null) // Window is open
        {
            if (UIManager.inst.terminal_activeIField != null) // And the player is in the input window
            {
                UIManager.inst.terminal_activeIField.GetComponent<UIHackInputfield>().Input_Tab();
            }
        }
    }

    /// <summary>
    /// For items in the /PARTS/ menu. If the corresponding letter is pressed on the keyboard, that item should be toggled. WE IGNORE INVENTORY ITEMS.
    /// </summary>
    private void InventoryInputDetection()
    {
        // Check for player input
        if (Keyboard.current.anyKey.wasPressedThisFrame
            && !UIManager.inst.terminal_targetresultsAreaRef.gameObject.activeInHierarchy
            && !InventoryControl.inst.awaitingSort
            && !GlobalSettings.inst.db_main.activeInHierarchy)
        {
            // Go through all the interfaces
            foreach (var I in InventoryControl.inst.interfaces)
            {
                string detect = "";
                InvDisplayItem reference = null;

                // Get the letter
                if (I.GetComponent<DynamicInterface>()) // Includes all items found in /PARTS/ menus (USES LETTER)
                {
                    foreach (var item in I.GetComponent<DynamicInterface>().slotsOnInterface)
                    {
                        reference = item.Key.GetComponent<InvDisplayItem>();
                        if (reference.item != null && reference.item.Id >= 0)
                        {
                            detect = reference._assignedChar;

                            // Validate the character and check key press
                            if (!string.IsNullOrEmpty(detect))
                            {
                                // Convert assigned character to KeyControl
                                var keyControl = Keyboard.current[detect.ToLower()] as KeyControl;

                                if (keyControl != null && keyControl.wasPressedThisFrame)
                                {
                                    // Toggle!
                                    reference?.Click();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}

[System.Serializable]
[Tooltip("Which interaction mode is the player in?")]
public enum InterfacingMode
{
    DEFAULT,
    [Tooltip("Moving around, shooting, etc.")]
    COMBAT,
    [Tooltip("Interacting with something, like a terminal. Normal keyboard typing.")]
    TYPING
}
