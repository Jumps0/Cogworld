using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpritePrefabUpdateTool : MonoBehaviour
{
    [MenuItem("Tools/Update MachinePart.cs ScriptableObjects")]
    public static void UpdateMachineParts()
    {
        // Get the currently selected GameObject in the editor
        GameObject selectedObject = Selection.activeGameObject;

        // Check if a GameObject is selected
        if (selectedObject == null)
        {
            Debug.LogError("No prefab selected. Please select a prefab.");
            return;
        }

        // Load the TileDatabaseObject from the Resources folder
        TileDatabaseObject tileDatabase = Resources.Load<TileDatabaseObject>("ScriptableObjects/Tile Database");

        if (tileDatabase == null)
        {
            Debug.LogError("TileDatabaseObject not found in Resources folder at path 'ScriptableObjects/Tile Database'.");
            return;
        }

        // Iterate over all child objects, including the parent itself
        foreach (Transform child in selectedObject.GetComponentsInChildren<Transform>())
        {
            // Check if the child has both a SpriteRenderer and MachinePart
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            MachinePart machinePart = child.GetComponent<MachinePart>();

            if (spriteRenderer != null && machinePart != null)
            {
                // Try to find the matching ScriptableObject based on the sprite itself
                Sprite oldSprite = spriteRenderer.sprite;
                TileObject matchingSO = null;

                foreach (var tileSO in tileDatabase.Tiles)
                {
                    if (tileSO.displaySprite != null && tileSO.displaySprite == oldSprite)
                    {
                        matchingSO = tileSO;
                        break;
                    }
                }

                // If a match is found, assign it to the MachinePart
                if (matchingSO != null)
                {
                    machinePart.info = matchingSO;
                    EditorUtility.SetDirty(machinePart); // Mark as dirty so the change is saved
                    Debug.Log($"Assigned {matchingSO.name} to {child.name}");
                }
                else
                {
                    Debug.LogWarning($"No matching ScriptableObject found for sprite: {oldSprite}");
                }
            }
        }

        // Save the changes to the prefab
        PrefabUtility.RecordPrefabInstancePropertyModifications(selectedObject);
        AssetDatabase.SaveAssets();
        Debug.Log("Prefab update completed.");
    }
}
