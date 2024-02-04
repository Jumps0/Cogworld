using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Actor>().isAlive)
        {
            CheckForPlayerMovement();
            CheckForGroundItemInteraction();
            CheckMoveMethod();
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            TileBlock currentTile = GetCurrentPlayerTile();
            int currentX = currentTile.locX;
            int currentY = currentTile.locY;
            Debug.Log(MapManager.inst._allTilesRealized[new Vector2Int(currentX, currentY)] + " <> " + GridManager.inst.grid[currentX,currentY]);
        }
    }

    #region Movement

    private void CheckForPlayerMovement()
    {
        if (playerMovementAllowed && !inDialogueSequence)
        {

            // -- Mouse Movement --

            if (GameManager.inst.allowMouseMovement)
            {
                if(GridManager.inst.astar == null) {
                    return;
                }

                if(GridManager.inst.astar.path.Count > 0 && GridManager.inst.astar.searchStatus == AStarSearchStatus.Success) // Does a successful path exist?
                {
                    if (Input.GetMouseButtonDown(0)) // Move on left click
                    {
                        //Debug.Log(GridManager.inst.astar.path[0].X + "," + GridManager.inst.astar.path[0].Y);
                        TileBlock currentTile = GetCurrentPlayerTile();
                        int lastItem = GridManager.inst.astar.path.Count - 2; // List includes the starting spot
                        int targetX = GridManager.inst.astar.path[lastItem].X - currentTile.locX;
                        int targetY = GridManager.inst.astar.path[lastItem].Y - currentTile.locY;

                        AttemptMovement(targetX, targetY);
                        GridManager.inst.ClearGridOfHighlightColor(UIManager.inst.dullGreen);
                    }
                }
            }

            // -- Arrow Key Movement --


            // Diagonal Movement
            if (Input.GetKey(KeyCode.UpArrow) && !isMoving) // [UP-LEFT]
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving)
                {
                    AttemptMovement(-1, 1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && !isMoving) // [UP-LEFT]
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) && !isMoving)
                {
                    AttemptMovement(-1, 1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.UpArrow) && !isMoving) // [UP-RIGHT]
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving)
                {
                    AttemptMovement(1, 1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow) && !isMoving) // [UP-RIGHT]
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) && !isMoving)
                {
                    AttemptMovement(1, 1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) && !isMoving) // [DOWN-LEFT]
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving)
                {
                    AttemptMovement(-1, -1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && !isMoving) // [DOWN-LEFT]
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) && !isMoving)
                {
                    AttemptMovement(-1, -1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) && !isMoving) // [DOWN-RIGHT]
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving)
                {
                    AttemptMovement(1, -1);
                    return;
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow) && !isMoving) // [DOWN-RIGHT]
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) && !isMoving)
                {
                    AttemptMovement(1, -1);
                    return;
                }
            }
            // Regular Movement
            if (Input.GetKeyDown(KeyCode.UpArrow) && !isMoving) // [UP]
            {
                AttemptMovement(0, 1);
                //StartCoroutine(MovePlayer(Vector3.up));
                return;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && !isMoving) // [DOWN]
            {
                AttemptMovement(0, -1);
                //StartCoroutine(MovePlayer(Vector3.down));
                return;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving) // [LEFT]
            {
                AttemptMovement(-1, 0);
                //StartCoroutine(MovePlayer(Vector3.left));
                return;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving) // [RIGHT]
            {
                AttemptMovement(1, 0);
                //StartCoroutine(MovePlayer(Vector3.right));
                return;
            }

        }
    }

    /// <summary>
    /// Directly move the player.
    /// </summary>
    /// <param name="moveDirection">Direction for player to be moved.</param>
    private IEnumerator MovePlayer(Vector3 moveDirection)
    {

        isMoving = true;

        float elapsedTime = 0;

        originPos = transform.position;
        targetPos = originPos + moveDirection;

        if (!doInstantMovement)
        {
            while (elapsedTime < timeToMove)
            {
                transform.position = Vector3.Lerp(originPos, targetPos, (elapsedTime / timeToMove));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }

    [SerializeField] private bool moveKeyHeld;
    public void AttemptMovement(int X, int Y)
    {
        // Attempt to move in the *direction* described by X & Y
        // So we need to add that to our current position to obtain where we want to end up

        TurnManager.inst.DoorCheck(); // !!! MOVE THIS LATER !!!

        isMoving = true;

        TileBlock currentTile = GetCurrentPlayerTile();
        int currentX = currentTile.locX;
        int currentY = currentTile.locY;
        Vector2Int moveTarget = new Vector2Int(currentX + X, currentY + Y); // This is where we want to move to
        //Debug.Log("Want to move from: (" + currentX + "," + currentY + ") to " + moveTarget);

        // -- Machine interaction detection --
        GameObject machineInteraction = null;

        if (MapManager.inst._layeredObjsRealized.ContainsKey(moveTarget)) // Is there a layered object here?
        {
            if (MapManager.inst._layeredObjsRealized[moveTarget].GetComponent<MachinePart>()) // Is there a machine here?
            {
                machineInteraction = MapManager.inst._layeredObjsRealized[moveTarget];
            }
        }

        // -- Ordinary Movement to tiles / exits detection --

        List<GameObject> neighbors = FindNeighbors(currentX, currentY);

        GameObject desiredDestinationTile = null;

        foreach (GameObject t in neighbors) // Find which of the neighbors we want to try and move to.
        {

            // Exits take priority
            if (MapManager.inst._layeredObjsRealized.ContainsKey(HF.V3_to_V2I(t.transform.position)) && MapManager.inst._layeredObjsRealized[(HF.V3_to_V2I(t.transform.position))].GetComponent<AccessObject>())
            {
                desiredDestinationTile = MapManager.inst._layeredObjsRealized[(HF.V3_to_V2I(t.transform.position))]; // This is the one
                break;
            }
            else if (t.GetComponent<TileBlock>())
            {
                if ((t.GetComponent<TileBlock>().locX == moveTarget.x) && (t.GetComponent<TileBlock>().locY == moveTarget.y)) // This tile is the one we want to move to
                {
                    desiredDestinationTile = t; // This is the one
                    break; // Stop looking
                }
            }
        }

        // ------------------------------------------------

        if (desiredDestinationTile == null)
        {
            Debug.LogError("ERROR: Desired destination tile for movement is null! <PlayerGridMovement.cs>");
            return;
        }

        if (desiredDestinationTile.GetComponent<TileBlock>())
        {
            if (!desiredDestinationTile.GetComponent<TileBlock>().occupied) // Space is Clear (Move!)
            {
                moveKeyHeld = Action.BumpAction(GetComponent<Actor>(), new Vector2(X, Y));

                if (MapManager.inst._layeredObjsRealized.ContainsKey(moveTarget) && MapManager.inst._layeredObjsRealized[moveTarget].GetComponent<FloorTrap>())
                {
                    // There is a trap here!
                    FloorTrap trap = MapManager.inst._layeredObjsRealized[moveTarget].GetComponent<FloorTrap>();
                    if(trap.alignment != BotRelation.Friendly && trap.active && !trap.tripped)
                    {
                        HF.AttemptTriggerTrap(trap, PlayerData.inst.gameObject);
                    }
                }
            }
            else if (!desiredDestinationTile.GetComponent<TileBlock>().occupied && desiredDestinationTile.GetComponent<TileBlock>().GetBotOnTop() == null) // "Invalid Move" (Blocked)
            {
                UIManager.inst.ShowCenterMessageTop("Invalid move", Color.black, UIManager.inst.alertRed);
            }
        }
        else if (desiredDestinationTile.GetComponent<AccessObject>()) // "About to Leave Area" (Walking into exit)
        {
            ConfirmLeaveArea(desiredDestinationTile.GetComponent<AccessObject>());
        } 
        
        // -- Machine Interaction (Parsing) --

        // Theres a lot here [0 = Static, 1 = Terminal, 2 = Fabricator, 3 = Scanalyzer, 4 = Repair Station, 5 = Recycling Unit, 6 = Garrison, 7 = Custom Terminal]
        if(machineInteraction != null)
        {
            if (machineInteraction.GetComponent<Terminal>()) // Open Terminal
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage("Connecting with Terminal...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
            else if (machineInteraction.GetComponent<Fabricator>()) // Open Fabricator
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage("Connecting with Fabricator...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
            else if (machineInteraction.GetComponent<Scanalyzer>()) // Open Scanalyzer
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage("Connecting with Scanalyzer...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
            else if (machineInteraction.GetComponent<RepairStation>()) // Open Repair Station
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage("Connecting with Repair Station...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
            else if (machineInteraction.GetComponent<RecyclingUnit>()) // Open Recycling Unit
            {
                // Create log messages
                UIManager.inst.CreateNewLogMessage("Connecting with Recycling Unit...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                UIManager.inst.Terminal_OpenGeneric(machineInteraction);
            }
            else if (machineInteraction.GetComponent<Garrison>()) // Open Garrison
            {
                if (!machineInteraction.GetComponent<Garrison>().g_sealed)
                {
                    // Create log messages
                    UIManager.inst.CreateNewLogMessage("Connecting with Garrison Access...", UIManager.inst.highlightGreen, UIManager.inst.dullGreen, true);
                    UIManager.inst.Terminal_OpenGeneric(machineInteraction);
                }
            }
            else if (machineInteraction.GetComponent<TerminalCustom>()) // Open Custom Terminal
            {
                UIManager.inst.CTerminal_Open(machineInteraction);
            }
        }
        // -----------------------------------------

        // Kick (Bot in way, kick it)

        // Crush (Bot in way, run it over)


        // <Some Kind of Attack>

        // <Use Something>

        isMoving = false;

        if(playerMovementAllowed)
            this.GetComponent<Actor>().UpdateFieldOfView();
        TurnManager.inst.DoorCheck(); // !!! MOVE THIS LATER !!!
    }

    public TileBlock GetCurrentPlayerTile()
    {


        // - Not gonna lie, raycasting to find this sucks, it has a lot of issues and isn't that versitle.
        /*
        int layerMask = ~(LayerMask.GetMask("Player")); // Ignore player Layer

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.zero), 10f, layerMask);

        if (hit && hit.collider.tag == "Tile")
        {
            return hit.collider.gameObject.GetComponent<TileBlock>(); // Success
        }
        //Debug.LogError("Failed to get current player tile.");
        */

        // - So we do this instead
        Vector2Int pLoc = HF.V3_to_V2I(PlayerData.inst.transform.position); // Get player's location (to V2I)
        if (MapManager.inst._allTilesRealized.ContainsKey(pLoc) && MapManager.inst._allTilesRealized[pLoc].GetComponent<TileBlock>())
        {
            return MapManager.inst._allTilesRealized[pLoc].GetComponent<TileBlock>();
        }

        return null; // Failure
    }

    /// <summary>
    /// Finds *VALID* neighbors given a current position on a grid.
    /// </summary>
    /// <param name="X">Current X position on the grid.</param>
    /// <param name="Y">Current Y position on the grid.</param>
    /// <returns>Returns a list of *VALID* neighbors that exist.</returns>
    public List<GameObject> FindNeighbors(int X, int Y)
    {
        // --
        // Copied from "Astar.cs"
        // --

        // NOTE: I hate GridManager, the array sucks. We are going to use _allTilesRealized instead.

        List<GameObject> neighbors = new List<GameObject>();

        // We want to include diagonals into this.
        if (X < MapManager.inst._mapSizeX - 1) // [ RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y)].gameObject);
        }
        if (X > 0) // [ LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y)].gameObject);
        }
        if (Y < MapManager.inst._mapSizeY - 1) // [ UP ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X, Y + 1)].gameObject);
        }
        if (Y > 0) // [ DOWN ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X, Y - 1)].gameObject);
        }
        // -- 
        // Diagonals
        // --
        if (X < MapManager.inst._mapSizeX - 1 && Y < MapManager.inst._mapSizeY - 1) // [ UP-RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y + 1)].gameObject);
        }
        if (Y < MapManager.inst._mapSizeY - 1 && X > 0) // [ UP-LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y + 1)].gameObject);
        }
        if (Y > 0 && X > 0) // [ DOWN-LEFT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X - 1, Y - 1)].gameObject);
        }
        if (Y > 0 && X < MapManager.inst._mapSizeX - 1) // [ DOWN-RIGHT ]
        {
            neighbors.Add(MapManager.inst._allTilesRealized[new Vector2Int(X + 1, Y - 1)].gameObject);
        }

        return neighbors;

    }

    #endregion

    #region Leave Area 

    [SerializeField] private bool confirmLeaveLevel = false;
    [SerializeField] private float leaveLevelCooldown = 5f;

    private void ConfirmLeaveArea(AccessObject target)
    {
        if (confirmLeaveLevel)
        {
            // Already confirmed, time to leave!
            MapManager.inst.ChangeMap(target.targetDestination, target.isBranch);
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

    private void CheckForGroundItemInteraction()
    {
        // Player must be on top of a tile with an item on it to be able to interact with it
        TileBlock currentTile = GetCurrentPlayerTile();

        if(currentTile != null && currentTile._partOnTop == null)
        {
            return;
        }

        // There is an item here? Allow the user to pick it up

        // Need to check for:
        // - Player clicking on the item (clicking on themselves really)
        // See OnMouseDown()

        // - Player hitting enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            currentTile._partOnTop.TryEquipItem(); // Try equipping it
            //InventoryControl.inst.DebugPrintInventory();
        }
    }

    private void OnMouseDown() // When the player clicks on themselves
    {
        TileBlock itemCheck = GetCurrentPlayerTile(); // Get the current tile the player is on
        if (itemCheck._partOnTop != null) // If there is an item on it (underneath the player)
        {
            Debug.Log("Try equip...");
            itemCheck._partOnTop.TryEquipItem(); // Try equipping it
        }
    }

    private void CheckMoveMethod()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.inst.allowMouseMovement = !GameManager.inst.allowMouseMovement;
            GridManager.inst.ClearGridOfHighlightColor(UIManager.inst.dullGreen);
        }
    }
}
