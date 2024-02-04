using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AI_Melee))]
public class Complex_Enemy : UnitAI
{
    [SerializeField] private AI_Melee AI_m;
    [SerializeField] private bool isFighting;

    private void OnValidate()
    {
        AI_m = GetComponent<AI_Melee>();
        // Pathfinding here
    }

    public void RunAI()
    {
        if (!AI_m.Target)
        {
            AI_m.Target = null;
        }
        else if (AI_m.Target && !AI_m.Target.IsAlive)
        {
            AI_m.Target = null;
        }

        if (AI_m.Target)
        {
            Vector3 tp = AI_m.Target.transform.position;
            Vector3Int targetPosition = new Vector3Int((int)tp.x, (int)tp.y, (int)tp.z);
            if (isFighting || GetComponent<Actor>().FieldofView.Contains(targetPosition))
            {
                if (!isFighting)
                {
                    isFighting = true;
                }

                float targetDistance = Vector3.Distance(transform.position, AI_m.Target.transform.position);

                if(targetDistance <= 1.5f)
                {
                    Action.MeleeAction(GetComponent<Actor>(), AI_m.Target);
                    return;
                }
                else // If not in range, move towards target
                {
                    //MoveAlongPath(targetPosition);
                    return;
                }
            }
        }

        Action.SkipAction(this.GetComponent<Actor>());
    }
}
