using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BotAI))]
public class DebugBotAITesting : Editor
{
    /*
    public override void OnInspectorGUI()
    {
        BotAI botAI = (BotAI)target;

        if(GUILayout.Button("Force Take Turn"))
        {
            botAI.TakeTurn();
        }
    }
    */
}
