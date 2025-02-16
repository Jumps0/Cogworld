using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiscSpriteStorage : MonoBehaviour
{
    public static MiscSpriteStorage inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Debris Sprites")]
    public List<Tile> debrisSprites = new List<Tile>();
    [Tooltip("0-15, 0 is the sprite WITHOUT debris")]
    public List<Sprite> ASCII_debrisSprites = new List<Sprite>();

    [Header("ASCII Character Sprites")]
    public List<Sprite> ASCII_characters = new List<Sprite>();

    [Header("Projectile Sprites")] // TODO: Relocate this (with the update sprites too)
    public List<Sprite> projectileSprites = new List<Sprite>();
}
