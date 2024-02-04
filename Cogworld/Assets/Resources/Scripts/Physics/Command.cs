using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Command
{
    public EntValues entity;

    public Command(EntValues ent)
    {
        entity = ent;
    }

    public virtual void Init()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual bool IsDone()
    {
        return false;
    }

    public virtual void Stop()
    {

    }
}
