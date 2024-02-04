using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntValues : MonoBehaviour
{
    public Vector3 position = Vector3.zero;
    public Vector3 velocity = Vector3.zero;

    public float speed;
    public float desiredSpeed;
    public float heading; //degrees
    public float desiredHeading;

    public float acceleration;
    public float turnRate;
    public float maxSpeed;
    public float minSpeed;

    // Start is called before the first frame update
    void Start()
    {
        position = transform.localPosition;
        desiredHeading = transform.localEulerAngles.z;
        heading = desiredHeading;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (this.GetComponent<PlayerData>())
        {
            setLocation(this.transform.position, 0);
        }
        */
    }

    // takes in a position vector and heading and sets it to the object, used in the randomization of positions and headings
    public void setLocation(Vector3 newPosition, float newHeading)
    {
        position = newPosition;
        heading = newHeading;
        desiredHeading = newHeading;
        transform.localPosition = position;

        Vector3 eulerRotation = Vector3.zero;
        eulerRotation.z = heading;
        transform.localEulerAngles = eulerRotation;
    }
}
