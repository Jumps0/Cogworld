using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// If the specified entity (Player, Enemies, Allies, etc.) enters this area, an event will be triggered.
/// </summary>
public class TriggerArea : MonoBehaviour
{
    public bool destroyAfterTriggered = true;

    [Header("Target")]
    [Tooltip("Does the target have a specific tag?")]
    public bool t_tagBased = false;
    [Tooltip("Do we allow for anything to be the target?")]
    public bool t_any = false;
    [Tooltip("Does the target need to be in any relation to the player?")]
    public bool t_relation = false;
    public string targetTag = "";
    public BotRelation targetRelation = BotRelation.Default;

    [Header("Event to Trigger")]
    public EventTile eventTile;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(t_tagBased)
        {
            if(collision.tag == targetTag)
            {
                TriggerEvent();
            }
        }
        else if(t_any)
        {
            if (collision.gameObject.GetComponent<Actor>())
            {
                TriggerEvent();
            }
        }
        else if(t_relation)
        {
            if (collision.gameObject.GetComponent<Actor>() && collision.gameObject.GetComponent<Actor>().botInfo && collision.gameObject.GetComponent<Actor>().botInfo.locations.relation == targetRelation)
            {
                TriggerEvent();
            }
        }
    }

    private void TriggerEvent()
    {
        if (eventTile)
        {
            eventTile.TriggerEvent();
        }


        if(destroyAfterTriggered)
            Destroy(this.gameObject); // Trigger no longer needed
    }
}
