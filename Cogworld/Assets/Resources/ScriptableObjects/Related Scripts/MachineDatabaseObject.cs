using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Machine Database", menuName = "SO Systems/Machines/Database")]
public class MachineDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public MachineObject[] Machines; // Contains all machines.
    public Dictionary<string, MachineObject> dict;

    [ContextMenu("Update ID's")]
    public void UpdateIDs()
    {
        for (int i = 0; i < Machines.Length; i++)
        {
            if (Machines[i].Id != i)
                Machines[i].Id = i;
        }
    }

    public void SetupDict()
    {
        dict = new Dictionary<string, MachineObject>();

        foreach (var machine in Machines)
        {
            dict.Add(machine.name, machine); // Doesn't use trueName because that is used for a special purpose.
        }
    }

    public void OnAfterDeserialize()
    {
        UpdateIDs();
    }

    public void OnBeforeSerialize()
    {

    }
}
