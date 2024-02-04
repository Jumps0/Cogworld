using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    public List<Move> commands = new List<Move>();
    public int length = 0;
    public bool activeCommand = false;

    // Update is called once per frame
    void Update()
    {
        if (activeCommand)
        {
            if (commands[0].IsDone())
            {
                commands[0].Stop();
                commands.RemoveAt(0);
                activeCommand = false;
                --length;
            }
            else
            {
                commands[0].Tick();
            }
        }
        else
        {
            if (length > 0)
            {
                activeCommand = true;
                commands[0].Init();
                commands[0].Tick();
            }
        }
    }

    public void SetCommand(Move c)
    {
        if (activeCommand)
        {
            commands[0].Stop();
            activeCommand = false;
        }
        commands.Clear();
        commands.Add(c);
        length = 1;
    }

    public void AddCommand(Move c)
    {
        commands.Add( c);
        ++length;
    }
}
