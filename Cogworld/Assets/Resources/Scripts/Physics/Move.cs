using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Command
{
    public Vector3 movePosition;

    public Move(EntValues ent, Vector3 pos) : base(ent)
    {
        movePosition = pos;
    }

    public override void Init()
    {
        entity.desiredSpeed = entity.maxSpeed;
        Debug.Log("New destination: " + movePosition);
    }

    public override void Tick()
    {
        // credit Michael Dorado, based on this source as well: https://answers.unity.com/questions/230204/using-mathf-to-find-the-inverse-tan-in-degrees.html
        Vector3 distance = movePosition - entity.position;
        float newHeading = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
        newHeading -= 90;
        newHeading *= -1;
        newHeading = Utils.Degrees360(newHeading);
        entity.desiredHeading = newHeading;
    }

    public Vector3 diff = Vector3.positiveInfinity;
    public float doneDistanceSq = 1;

    public override bool IsDone()
    {
        diff = movePosition - entity.position;
        return (diff.sqrMagnitude < doneDistanceSq);
    }

    public override void Stop()
    {
        entity.desiredSpeed = 0;
        entity.desiredHeading = entity.heading;
    }
}
