using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Abstract_MapGen), true)]

public class RandomDungeonGeneratorEditor : Editor
{
    Abstract_MapGen generator;

    private void Awake()
    {
        generator = (Abstract_MapGen)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Create Dungeon"))
        {
            generator.GenerateDungeon();
        }
    }
}
