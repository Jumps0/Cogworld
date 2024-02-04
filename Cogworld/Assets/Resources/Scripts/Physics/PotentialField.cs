using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PotentialField : MonoBehaviour
{
    public bool attractive;
    public bool repulsive;
    public bool movementActive; //if movement is active
    public float Aconstant;
    public float Rconstant; //distinguish between attractive and repulsive forces in case an entity has both
    public EntValues ent; //the entity that this attached to the parent
    public bool hasGoal = false;
    public bool mustFlee = false;

    public Vector3 goal; //the personal goal that this entity is assigned to follow
    public Actor fleeSource; //the personal source that this must flee from

    private float elapsed = 0; //the amount of time that has elapsed since the last time the force was calculated

    private void OnEnable()
    {
        //when this force is enabled, it will add it to the list of total forces
        //in the potentialFieldMgr
        PotentialFieldMgr.inst.forces.Add(this);
    }

    private void OnDisable()
    {
        //should this component be disabled for any reason, remove it from the list
        //of total forces
        PotentialFieldMgr.inst.forces.Remove(this);
    }

    //set a new goal for this entity to follow
    public void setGoal(Vector3 newGoal)
    {
        goal = newGoal;
        hasGoal = true;
    }

    public void setFlee(Actor source)
    {
        fleeSource = source;
        mustFlee = true;
    }

    //disables the active boolean and sets desired movement speed to 0
    public void disableMovement()
    {
        movementActive = false;
        ent.desiredSpeed = 0;
    }

    //enables the movement
    public void enableMovement()
    {
        movementActive = true;
    }

    private void Update()
    {
        //if movement is active
        if (movementActive)
        {
            //if the total elapsed time is greater than or equal to the buffer, recalculate the force
            if (elapsed >= PotentialFieldMgr.inst.buffer)
            {
                elapsed = 0;
                Vector3 force = PotentialFieldMgr.inst.totalForce(this, ent);
                if(hasGoal)
                {
                    Vector3 f = (goal - ent.position);
                    float mag = f.magnitude;
                    float value = 100 / (mag * mag);
                    force += value * f.normalized;
                }
                if (mustFlee)
                {
                    Vector3 f = (ent.position - fleeSource.transform.localPosition);
                    float value = 100 / (f.magnitude);
                    force += value * f.normalized;
                }
                // credit Michael Dorado, based on this source as well: https://answers.unity.com/questions/230204/using-mathf-to-find-the-inverse-tan-in-degrees.html
                float newHeading = Mathf.Atan2(force.y, force.x) * Mathf.Rad2Deg;
                newHeading -= 90;
                newHeading *= -1;
                newHeading = Utils.Degrees360(newHeading);
                ent.desiredHeading = newHeading;
                float range = ent.maxSpeed - ent.minSpeed;
                float angleDiff = Mathf.Abs(Utils.AngleDiffPosNeg(ent.desiredHeading, ent.heading));
                ent.desiredSpeed = ent.minSpeed + (range * ((Mathf.Cos(angleDiff) + 1.0f) / 2.0f));
            }
            //otherwise increment by the dt
            else
            {
                elapsed += Time.deltaTime;
            }
        }
        if(hasGoal && (goal - ent.position).sqrMagnitude <= 1)
        {
            hasGoal = false;
        }
    }
}
