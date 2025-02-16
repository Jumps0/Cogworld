using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles bulk logic for all tiles.
/// </summary>
public class TileManager : MonoBehaviour
{
    public static TileManager inst;
    public void Awake()
    {
        inst = this;
    }


}
