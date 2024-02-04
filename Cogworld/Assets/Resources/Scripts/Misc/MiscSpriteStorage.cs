using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscSpriteStorage : MonoBehaviour
{
    public static MiscSpriteStorage inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Debris Sprites")]
    public List<Sprite> debrisSprites = new List<Sprite>();
    [Tooltip("0-15, 0 is the sprite WITHOUT debris")]
    public List<Sprite> ASCII_debrisSprites = new List<Sprite>();

    [Header("Access Sprites")]
    public List<Sprite> accessSprites = new List<Sprite>();


    [Header("Misc Machine Part Sprites")]
    public List<Sprite> machinePartSprites = new List<Sprite>();
}
