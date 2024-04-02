using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

/// <summary>
/// Custom FX Manager. Manages things like the visuals of an explosion. (Not named GFXManager because it starts with G and that will pop-up above GameObject in the search).
/// https://www.gridsagegames.com/blog/2014/04/making-particles/
/// </summary>
public class CFXManager : MonoBehaviour
{
    public static CFXManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Prefabs")]
    public GameObject prefab_tile;


    #region Explosion FX
    public void CreateExplosionFX(Vector2Int pos, ItemObject weapon, List<Vector2Int> tiles)
    {
        Dictionary<Vector2Int, GameObject> allTiles = new Dictionary<Vector2Int, GameObject>();

        ExplosionGFX gfx = weapon.explosion.explosionGFX;
        int radius = weapon.explosion.radius;

        /*
         *  >> We have many different types of explosion <<
         *   >> Launchers
         *   -
         *   -
         *   -
         *   -
         *   -
         *   >> Mines
         *   -
         *   -
         *   -
         * 
         */

        // Fill the radius with the specified explosion tiles
        foreach (var T in tiles)
        {
            CreateTile(T, Color.white, allTiles);
        }

        switch (weapon.explosion.explosionGFX)
        {
            case ExplosionGFX.Generic:
                break;
            case ExplosionGFX.Light:
                break;
            case ExplosionGFX.Neutron:
                break;
            case ExplosionGFX.Singularity:
                break;
            case ExplosionGFX.EMP:
                break;
            case ExplosionGFX.EMPCone:
                break;
        }

        StartCoroutine(AnimateExplosionFX(weapon, allTiles));
    }

    private IEnumerator AnimateExplosionFX(ItemObject weapon, Dictionary<Vector2Int, GameObject> tiles)
    {
        switch (weapon.explosion.explosionGFX)
        {
            case ExplosionGFX.Generic:
                break;
            case ExplosionGFX.Light:
                break;
            case ExplosionGFX.Neutron:
                break;
            case ExplosionGFX.Singularity:
                break;
            case ExplosionGFX.EMP:
                break;
            case ExplosionGFX.EMPCone:
                break;
        }

        yield return null;

        // Destroy all animated tiles

        yield return new WaitForSeconds(5f);
        ClearAllTiles(tiles);
    }

    private void CreateTile(Vector2Int pos, Color color, Dictionary<Vector2Int, GameObject> tiles)
    {
        var spawnedTile = Instantiate(prefab_tile, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        spawnedTile.name = $"ExplosionFX: {pos.x},{pos.y}"; // Give grid based name
        spawnedTile.transform.parent = this.transform;
        spawnedTile.GetComponent<SpriteRenderer>().color = color; // Default green color
        tiles.Add(pos, spawnedTile);
    }

    private void DestroyTile(Vector2Int pos, Dictionary<Vector2Int, GameObject> tiles)
    {
        if (tiles.ContainsKey(pos))
        {
            Destroy(tiles[pos]);

            if (tiles.ContainsKey(pos))
            { // ^ Safety check
                tiles.Remove(pos);
            }
        }
    }

    private void SetTileColor(Vector2Int loc, Color _color, Dictionary<Vector2Int, GameObject> tiles)
    {
        if (tiles.ContainsKey(loc))
        {
            tiles[loc].GetComponent<SpriteRenderer>().color = _color;
        }
    }

    private void ClearAllTiles(Dictionary<Vector2Int, GameObject> tiles)
    {
        foreach (var T in tiles.ToList())
        {
            Destroy(T.Value);
        }

        tiles.Clear();
    }


    #endregion
}


#region Explosion Enums
[System.Serializable]
public enum ExplosionGFX
{
    Generic, // Generic Explosion (orange/yellow/red)
    Light, // Grenade Launcher (small white)
    Neutron, // Special Neutron (purple scattered)
    Singularity, // Special Point Singularity (purple tight + wings)
    EMP, // EMP (Blue with random characters)
    EMPCone //
}

#endregion
