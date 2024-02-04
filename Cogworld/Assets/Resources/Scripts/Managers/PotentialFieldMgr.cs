using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PotentialFieldMgr : MonoBehaviour
{
    public static PotentialFieldMgr inst;

    public List<PotentialField> forces = new List<PotentialField> ();
    public float buffer = 0; //the amount of time (in seconds) that must pass before a potential field can recalculate the force
                             //defaults to 0 meaning every frame
    public float distsqr = 100.0f; //the maximum magnitude squared for a force to be considered in the calculation
    void Awake()
    {
        inst = this;
    }

    //returns the total forces of the potential fields acting on the current entity (except its personal goal)
    public Vector3 totalForce(PotentialField curr, EntValues entity)
    {
        Vector3 total = Vector3.zero;
        foreach(PotentialField force in forces)
        {
            if (curr != force)
            {
                if (force.attractive)
                {
                    Vector3 f = (force.GetComponentInParent<EntValues>().position - entity.position);
                    float mag = f.magnitude;
                    float value = force.Aconstant / (mag * mag);
                    total += value * f.normalized;
                }
                if (force.repulsive)
                {
                    Vector3 f = (entity.position - force.GetComponentInParent<EntValues>().position);
                    if (f.sqrMagnitude <= distsqr)
                    {
                        float value = force.Rconstant / (f.magnitude);
                        total += value * f.normalized;
                    }
                }
            }
        }

        return total;
    }
}
