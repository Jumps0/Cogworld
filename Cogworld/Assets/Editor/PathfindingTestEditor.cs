using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathfindingTestControl))]
public class PathfindingTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PathfindingTestControl pathfindingTest = (PathfindingTestControl)target;

        // Add a button to the inspector
        if (GUILayout.Button("\nPathfind\n"))
        {
            // Call the Pathfind function when the button is clicked
            pathfindingTest.Pathfind();
        }
    }
}
