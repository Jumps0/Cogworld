using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedPhysics : MonoBehaviour
{
    public Vector3 desiredPostion;

    // Start is called before the first frame update
    void Start()
    {
        entity.position = transform.localPosition;
        desiredPostion = this.transform.position;
    }

    public EntValues entity;
    public Vector3 eulerRotation = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (Utils.AproximatelyEqual(entity.speed, entity.desiredSpeed))
        {
            entity.speed = entity.desiredSpeed;
        }
        else if (entity.speed < entity.desiredSpeed)
        {
            entity.speed = entity.speed + entity.acceleration * Time.deltaTime;
        }
        else if (entity.speed > entity.desiredSpeed)
        {
            entity.speed = entity.speed - entity.acceleration * Time.deltaTime;
        }
        entity.speed = Utils.Clamp(entity.speed, entity.minSpeed, entity.maxSpeed);

        if (Utils.AproximatelyEqual(entity.heading, entity.desiredHeading))
        {
            entity.heading = entity.desiredHeading;
        }
        else if (Utils.AngleDiffPosNeg(entity.desiredHeading, entity.heading) > 0)
        {
            entity.heading += entity.turnRate * Time.deltaTime;
        }
        else if (Utils.AngleDiffPosNeg(entity.desiredHeading, entity.heading) < 0)
        {
            entity.heading -= entity.turnRate * Time.deltaTime;
        }
        entity.heading = Utils.Degrees360(entity.heading);

        entity.velocity.x = Mathf.Sin(entity.heading * Mathf.Deg2Rad) * entity.speed;
        entity.velocity.z = 0;
        entity.velocity.y = Mathf.Cos(entity.heading * Mathf.Deg2Rad) * entity.speed;

        entity.position = entity.position + entity.velocity * Time.deltaTime;
        transform.localPosition = new Vector3((int)entity.position.x, (int)entity.position.y, 0); // Snap to nearest
        desiredPostion = transform.localPosition;
        /*
        if(Vector3.Distance(entity.position, this.transform.position) <= 1.5f)
        {
            desiredPostion = new Vector3((int)entity.position.x, (int)entity.position.y, 0); // Snap to nearest
            entity.position = desiredPostion;
        }
        else
        {
            entity.position = this.transform.position;
            
            if (entity.speed > 5)
            {
                entity.speed = 1;
            }
        }
        */

        //eulerRotation.z = entity.heading;
        eulerRotation.z = 0;
        //transform.localEulerAngles = eulerRotation;
    }
}
