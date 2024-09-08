using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Belongs to bots like: Haulers, Serfs, Engineers, Drillers, Scavengers, etc.
/// LEGACY CODE. SEE *BotAI.cs*
/// </summary>
public class AI_Passive : MonoBehaviour
{
    /// <summary>
    /// Does this bot belong to the 0b10 complex? (Most likely yes)
    /// </summary>
    public bool _is0b10 = true;

    public PassiveBotType _type;
    public PassiveBotState _state;

    [Header("Pathing")]
    public List<Vector2Int> pointsOfInterest = new List<Vector2Int>(); // Bot will visit these
    public int poi_id = 0;
    public Astar pathing;

    public bool isTurn = false;

    bool destinationReached = false;
    int timeOnPath = 0;

    private void Start()
    {
        _state = PassiveBotState.Working;
    }

    #region Pathing
    public void NavigateToPOI(Vector2Int poi)
    {
        float distanceToPoi = Vector2.Distance(this.transform.position, poi);

        if (distanceToPoi > 3) // Navigate to it
        {
            destinationReached = false;
            int index = pathing.path.Count - timeOnPath;
            if(index < 0)
            { // Stop early!!!
                destinationReached = true;
                _state = PassiveBotState.Idle;
                return;
            }

            Vector2 destination = new Vector2(pathing.path[index].X, pathing.path[index].Y);
            // Normalize move direction
            Vector2Int moveDir = new Vector2Int(0, 0);
            if (destination.x > this.transform.position.x)
            {
                moveDir.x++;
            }
            else if (destination.x < this.transform.position.x)
            {
                moveDir.x--;
            }

            if (destination.y > this.transform.position.y)
            {
                moveDir.y++;
            }
            else if (destination.y < this.transform.position.y)
            {
                moveDir.y--;
            }
            // Only move to valid tiles!
            if (MapManager.inst._allTilesRealized.ContainsKey(new Vector2Int((int)destination.x, (int)destination.y)) 
                && this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[new Vector2Int((int)destination.x, (int)destination.y)].bottom))
            {
                Action.MovementAction(this.GetComponent<Actor>(), moveDir); // Move
            }
            else
            {
                Action.SkipAction(this.GetComponent<Actor>()); // Wait...
            }

            _state = PassiveBotState.Working;
        }
        else // We are done
        {
            destinationReached = true;
            _state = PassiveBotState.Idle;
        }
    }

    public void SetNewAStar()
    {
        pathing = new Astar(GridManager.inst.grid); // Set up the A*
    }

    public void CreateAStarPath(Vector2Int point)
    {
        pathing.CreatePath(GridManager.inst.grid, (int)this.transform.position.x, (int)this.transform.position.y, point.x, point.y);
    }

    public void CreatePOIList()
    {
        List<Vector2Int> potentialPOIs = new List<Vector2Int>();

        foreach (var M in MapManager.inst._allTilesRealized)
        {
            if (M.Value.top && M.Value.top.GetComponent<MachinePart>())
            {
                potentialPOIs.Add(M.Key);
            }
        }

        if(potentialPOIs.Count >= 8)
        {
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
            pointsOfInterest.Add(potentialPOIs[Random.Range(0, potentialPOIs.Count - 1)]);
        }
        else
        {
            Debug.LogError($"{this} could not find enough points of interest!");
        }
    }
    #endregion
    public void TakeTurn()
    {
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            return;
        }

        StartCoroutine(TakeTurnDebug());
    }

    IEnumerator TakeTurnDebug()
    {
        if (this.GetComponent<Actor>().state_DORMANT || this.GetComponent<Actor>().state_UNPOWERED || this.GetComponent<Actor>().state_DISABLED)
        {
            Action.SkipAction(this.GetComponent<Actor>());
            yield break;
        }

        yield return new WaitForSeconds(5f);

        this.GetComponent<Actor>().UpdateFieldOfView();

        if (pointsOfInterest.Count == 0) // Create list of POIs if needed
        {
            CreatePOIList();
        }

        switch (_state)
        {
            case PassiveBotState.Working: // Continue working
                timeOnPath += 1;

                if (pathing == null) // If A* is null, make a new one
                {
                    SetNewAStar();
                }

                
                if (pathing.path.Count == 0) // If there is no current path, generate a new one
                {
                    CreateAStarPath(pointsOfInterest[poi_id]);
                }

                NavigateToPOI(pointsOfInterest[poi_id]);
                break;
            case PassiveBotState.Idle: // Find some work to do
                if (pathing == null) // If A* is null, make a new one
                {
                    SetNewAStar();
                }

                if (destinationReached) // Go to new POI if needed
                {
                    if (poi_id == pointsOfInterest.Count - 1)
                    {
                        poi_id = 0;
                    }
                    else
                    {
                        poi_id++;
                    }
                    destinationReached = false;
                    CreateAStarPath(pointsOfInterest[poi_id]);
                }

                NavigateToPOI(pointsOfInterest[poi_id]);
                break;
            case PassiveBotState.Fleeing: // Flee!
                Flee();
                break;
            case PassiveBotState.Misc: // ?

                break;
            default:
                break;
        }

        Action.SkipAction(this.GetComponent<Actor>()); // fallback condition
        //TakeTurn();
    }

    /// <summary>
    /// Attempt to flee from the specified source, get out of LOS and/or Field of View Range
    /// </summary>
    /// <param name="source">The actor to flee from.</param>
    public void FleeFromSource(Actor source)
    {
        fleeSource = source;
        //this.GetComponent<PotentialField>().setFlee(source);

        _state = PassiveBotState.Fleeing;

        Flee();
    }

    public void Flee()
    {
        if(fleeSource == null) // Stop?
        {
            _state = PassiveBotState.Idle;
            return;
        }
        else
        {
            int viewRange = this.GetComponent<Actor>().fieldOfViewRange;

            if (Vector3.Distance(this.transform.position, fleeSource.transform.position) <= viewRange) // Flee!
            {
                // We need to move away from the source
                List<GameObject> neighbors = HF.FindNeighbors((int)this.transform.position.x, (int)this.transform.position.y);

                List<GameObject> validMoveLocations = new List<GameObject>();

                foreach (var T in neighbors)
                {
                    if (this.GetComponent<Actor>().IsUnoccupiedTile(T.GetComponent<TileBlock>()))
                    {
                        validMoveLocations.Add(T);
                    }
                }

                GameObject fleeLocation = this.GetComponent<Actor>().FindBestFleeLocation(fleeSource.gameObject, this.gameObject, validMoveLocations);

                if(fleeLocation != null)
                {
                    // Normalize move direction
                    Vector2Int moveDir = new Vector2Int(0, 0);
                    if (fleeLocation.transform.position.x > this.transform.position.x)
                    {
                        moveDir.x++;
                    }
                    else if (fleeLocation.transform.position.x < this.transform.position.x)
                    {
                        moveDir.x--;
                    }

                    if (fleeLocation.transform.position.y > this.transform.position.y)
                    {
                        moveDir.y++;
                    }
                    else if (fleeLocation.transform.position.y < this.transform.position.y)
                    {
                        moveDir.y--;
                    }

                    // Now move there
                    Action.MovementAction(this.GetComponent<Actor>(), moveDir);
                }
                else
                {
                    // We can't move!
                    Action.SkipAction(this.GetComponent<Actor>());
                }
                

            }
            else // Safe. (for now)
            {
                _state = PassiveBotState.Idle;
                //this.GetComponent<PotentialField>().fleeSource = null;
                //this.GetComponent<PotentialField>().mustFlee = false;
                Action.SkipAction(this.GetComponent<Actor>());
                fleeSource = null;
            }
        }
    }

    private Actor fleeSource = null;
}

[System.Serializable]
public enum PassiveBotType
{
    Hauler,
    Serf,
    Engineer,
    Driller,
    Scavenger,
    Misc
}

[System.Serializable]
public enum PassiveBotState
{
    Working,
    Idle,
    Fleeing,
    Misc
}
