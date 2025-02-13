//
// Created by: inanoglu | github: https://github.com/inanoglu/Unity-Hierarchy-Object-Color/blob/main/HierarchyObjectColor.cs
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

/// <summary> Sets a background color for game objects in the Hierarchy tab</summary>
[UnityEditor.InitializeOnLoad]
#endif
public class HierarchyObjectColor
{

    private static Vector2 offset = new Vector2(20, 1);
#if UNITY_EDITOR
    static HierarchyObjectColor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {

        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            Color backgroundColor = Color.white;
            Color textColor = Color.white;
            Texture2D texture = null;

            // Write your object name in the hierarchy.
            //if (obj.name == "Main Camera")
            //{
            //    backgroundColor = new Color(0.2f, 0.6f, 0.1f);
            //    textColor = new Color(0.9f, 0.9f, 0.9f);
            //}


            //Or you can use switch case
            switch (obj.name)
            {
                case "Main Camera":
                    backgroundColor = Color.gray;
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;

                case "~ Player Core ~":
                    backgroundColor = Color.blue;
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;

                case "~ Player Core ~(Clone)":
                    backgroundColor = Color.blue;
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;

                case "== Canvas ==":
                    backgroundColor = new Color(0.5f, 0.25f, 0.3f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;

                case "== ARCHITECTURE ==":
                    backgroundColor = new Color(0.7f, 0.4f, 0.15f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;

                // (Important) Managers
                case "MapManager":
                    backgroundColor = new Color(0.6f, 0.15f, 0.45f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "GameManager":
                    backgroundColor = new Color(0.2f, 0.55f, 0.45f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "UI Manager":
                    backgroundColor = new Color(0.2f, 0.35f, 0.45f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "AudioManager":
                    backgroundColor = new Color(0.5f, 0.48f, 0.05f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "InventorySystem":
                    backgroundColor = new Color(0.2f, 0.2f, 0.7f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "TurnManager":
                    backgroundColor = new Color(0.4f, 0.7f, 0.2f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "QuestManager":
                    backgroundColor = new Color(0.9f, 0.6f, 0.1f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "GlobalSettings":
                    backgroundColor = new Color(0.2f, 0.6f, 0.1f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "NetworkManager":
                    backgroundColor = new Color(0.8f, 0.1f, 0.1f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                // Testing
                case "MainControl": // PathfindingTest
                    backgroundColor = new Color(0.5f, 0.48f, 0.05f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
            }


            if (backgroundColor != Color.white)
            {
                Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);

                EditorGUI.DrawRect(bgRect, backgroundColor);
                EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = textColor },
                    fontStyle = FontStyle.Bold
                }
                );

                if (texture != null)
                    EditorGUI.DrawPreviewTexture(new Rect(selectionRect.position, new Vector2(selectionRect.height, selectionRect.height)), texture);
            }
        }
    }
#endif
}