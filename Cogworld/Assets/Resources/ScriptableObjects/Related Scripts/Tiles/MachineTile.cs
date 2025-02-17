using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Machine Tile", menuName = "SO Systems/Tile/Machine")]
public class MachineTile : TileObject
{
    [Header("Sprites")]
    public List<Tile> sprites = new List<Tile>();
    public List<Tile> ASCII_sprites = new List<Tile>();

    public void Awake()
    {
        //type = TileType.Machine;
    }

}
