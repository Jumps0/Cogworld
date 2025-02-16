// Originally by *Sunny Valley Studio* | https://www.sunnyvalleystudio.com/blog/unity-2d-sprite-preview-inspector-custom-editor

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileObject), true)]
public class SOSPrites : Editor
{
    // Reference to a general TileObject
    TileObject tile;

    private void OnEnable()
    {
        tile = target as TileObject;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (tile == null || tile.displaySprite == null)
            return;

        // Display the sprite's name above the preview
        GUILayout.Label($"Sprite: {tile.displaySprite.sprite.name}");

        // Convert the tileSprite (see SO script) to Texture
        Texture2D texture = AssetPreview.GetAssetPreview(tile.displaySprite.sprite);

        // Create an empty space for the sprite preview (you may tweak dimensions)
        GUILayout.Label("", GUILayout.Height(80), GUILayout.Width(80));

        // Draws the texture where we have defined our Label (empty space)
        // NOTE: All the extra variables are here because the only constructor(s) that allow a color change vvv require them.
        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture, ScaleMode.ScaleToFit, true, 0, tile.asciiColor, 0, 0);
    }
}