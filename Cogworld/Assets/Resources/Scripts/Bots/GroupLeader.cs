using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The bot leader of a group/patrol will have this script on it, attached upon initialization.
/// </summary>
public class GroupLeader : MonoBehaviour
{
    public Actor _leader;
    public List<Actor> members = new List<Actor>();

    [Header("Patrol Info")]
    public List<Vector2Int> route = new List<Vector2Int>(); // Can be changed to relevant patrol point component later if needed
    public int pointInRoute = 0;
    public bool playerSpotted = false;

    public BotAIState _state;

    private void Awake()
    {
        _leader = GetComponent<Actor>();   // Make Self leader
        members.Add(GetComponent<Actor>());// Add self to list

        _state = BotAIState.Idle;

        CreatePatrolRoutes();
    }
    /*
    public void TakeTurn()
    {
        switch (_state)
        {
            case BotAIState.Working:

                break;
            case BotAIState.Hunting:

                break;
            case BotAIState.Returning:

                break;
            case BotAIState.Idle:

                break;
            case BotAIState.Misc:

                break;
            default:
                break;
        }
    }
    */

    #region Patrol Related

    public void Patrol()
    {

    }

    public void PersueTarget(Actor target)
    {

    }

    public void DivertToLocation(Transform location)
    {

    }

    public void ReturnToPatrol()
    {



        //
        Patrol();
    }

    public void CreatePatrolRoutes()
    {
        route = new List<Vector2Int>(); // Clear list
        List<Tunnel> halls = DungeonManagerCTR.instance.GetComponent<DungeonGeneratorCTR>().tunnels;
        int routeCount = 0;
        pointInRoute = 0;

        while(routeCount < 8)
        {
            Vector2Int possibleSpot = halls[Random.Range(0, halls.Count - 1)].GetCenter;

            if (!route.Contains(possibleSpot))
            {
                route.Add(possibleSpot);
                routeCount++;
            }
        }

    }

    public void AddBotToPatrol(Actor bot)
    {
        members.Add(bot);
        bot.GetComponent<BotAI>().squadLeader = this.GetComponent<Actor>();
    }

    public void RemoveMemberFromPatrol(Actor member)
    {
        member.GetComponent<BotAI>().squadLeader = null;
        members.Add(member);
    }

    #endregion
}
