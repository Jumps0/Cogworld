using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Belongs to most combat 0b10 bots
/// LEGACY CODE. NO LONGER USED. GOTO *BotAI.cs*
/// </summary>
public class AI_Hostile : MonoBehaviour
{
    /*
    public HostileBotType _type;
    public HostileBotState _state;

    public Actor squadLeader;

    public bool isTurn = false;

    public int memory = 0;
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

        if (this.GetComponent<GroupLeader>() && this.GetComponent<GroupLeader>().route.Count == 0)
        {
            this.GetComponent<GroupLeader>().CreatePatrolRoutes();
        }

        yield return new WaitForSeconds(3f);

        this.GetComponent<Actor>().UpdateFieldOfView();

        switch (_state)
        {
            case HostileBotState.Working:
                if ((!this.GetComponent<GroupLeader>() && !squadLeader.GetComponent<GroupLeader>().playerSpotted) || 
                    (this.GetComponent<GroupLeader>() && !this.GetComponent<GroupLeader>().playerSpotted)) // If the player hasn't been spotted
                {
                    if (squadLeader == null) // If the squad leader
                    {
                        this.GetComponent<Actor>().MoveToPatrolPoint();

                    }
                    else // If not the squad leader
                    {
                        this.GetComponent<Actor>().FollowLeader();
                    }
                }
                else // If the player has been spotted
                {
                    _state = HostileBotState.Hunting;
                }

                break;
            case HostileBotState.Hunting:
                // Track / Attack the player

                if (LOSOnPlayer()) // Can see the player
                {
                    memory = 0;
                    // Attack the player!
                    Action.RangedAttackAction(this.GetComponent<Actor>(), PlayerData.inst.gameObject, Action.FindRangedWeapon(this.GetComponent<Actor>()));
                }
                else // Can't see the player
                {
                    if(memory < 100)
                    {
                        // Move towards the player
                        memory += 1;
                        this.GetComponent<Actor>().NewGoal(Action.V3_to_V2I(PlayerData.inst.transform.position));
                        Vector2Int moveToLocation = Action.NormalizeMovement(this.gameObject.transform, this.GetComponent<OrientedPhysics>().desiredPostion);
                        Vector2Int realPos = Action.V3_to_V2I(this.GetComponent<OrientedPhysics>().desiredPostion);
                        if (MapManager.inst._allTilesRealized.ContainsKey(realPos) && this.GetComponent<Actor>().IsUnoccupiedTile(MapManager.inst._allTilesRealized[realPos]))
                        {
                            Action.MovementAction(this.GetComponent<Actor>(), moveToLocation);
                        }
                        else
                        {
                            Action.SkipAction(this.GetComponent<Actor>());
                        }
                    }
                    else
                    {
                        //PlayerData.inst.GetComponent<PotentialField>().enabled = false; // Disable the player's PF
                        if (this.GetComponent<BotAI>().squadLeader)
                            this.GetComponent<BotAI>().squadLeader.GetComponent<GroupLeader>().playerSpotted = false;

                        memory = 0;
                        _state = HostileBotState.Working;
                        Action.SkipAction(this.GetComponent<Actor>());
                    }
                }

                break;
            case HostileBotState.Returning:

                break;
            case HostileBotState.Idle:
                _state = HostileBotState.Working;
                Action.SkipAction(this.GetComponent<Actor>());

                break;
            case HostileBotState.Misc:

                break;
            default:
                break;
        }

        Action.SkipAction(this.GetComponent<Actor>()); // fallback condition
    }


    bool LOSOnPlayer()
    {
        bool LOS = true;

        // - If line of sight being blocked - // (THIS ALSO GETS USED LATER)
        Vector2 targetDirection = PlayerData.inst.transform.position - this.transform.position;
        float distance = Vector2.Distance(Action.V3_to_V2I(this.transform.position), Action.V3_to_V2I(PlayerData.inst.transform.position));

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(this.transform.position.x, this.transform.position.y), targetDirection.normalized, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            TileBlock tile = hit.collider.GetComponent<TileBlock>();
            DoorLogic door = hit.collider.GetComponent<DoorLogic>();
            MachinePart machine = hit.collider.GetComponent<MachinePart>();

            // If we encounter:
            // - A wall
            // - A closed door
            // - A machine

            // Then there is no LOS

            if(tile != null && tile.tileInfo.type == TileType.Wall)
            {
                return false;
            }

            if(door != null && tile.specialNoBlockVis == true)
            {
                LOS = true;
            }
            else if (door != null && tile.specialNoBlockVis == false)
            {
                return false;
            }

            if(machine != null)
            {
                return false;
            }
        }

        return LOS;
    }
    */
}
/*
[System.Serializable]
public enum HostileBotType
{
    Guard,      // Generally sits in one spot, will return to it when not in combat
    Recon,      // Will explore large spaces looking for threats, when found will goto + alert nearest patrol
    Patrol,     // Patrols around a loop with its squadmates
    Programmer, // Has a designated terminal, will stick near it and disable it if necessary
    Misc
}

[System.Serializable]
public enum HostileBotState
{
    Working,  // Gaurding, Patrolling, etc | This bot's normal activity
    Hunting,  // Engaging or Looking for the player
    Returning,// Going back to normal routine
    Idle,
    Misc
}
*/