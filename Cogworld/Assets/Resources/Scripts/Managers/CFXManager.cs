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

    [Header("Colors")]
    public Color e_yellow;
    public Color e_orange;
    // For launcher trails
    public Color t_red;
    public Color t_gray;


    #region Explosion FX
    public void CreateExplosionFX(Vector2Int pos, ItemObject weapon, List<Vector2Int> tiles, Vector2Int center)
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

        StartCoroutine(AnimateExplosionFX(weapon, allTiles, center));
    }

    private IEnumerator AnimateExplosionFX(ItemObject weapon, Dictionary<Vector2Int, GameObject> tiles, Vector2Int center)
    {
        float delay = 0f;

        switch (weapon.explosion.explosionGFX)
        {
            case ExplosionGFX.Generic:
                break;
            case ExplosionGFX.Light:
                /*
                 *  Generally this is split into two parts:
                 *  1. The explosion appears to radiate outwards from the center with the main tile being yellow and the surrounding tiles
                 *  being orange though becoming darker (randomly) as they get further away. 
                 *  The appearance is uniform until halfway through, where it becomes a bit random.
                 *  2. All tiles fade out in a uniform manner.
                 */

                List<Vector2Int> distList = new List<Vector2Int>();

                // Put dictionary into list
                foreach (KeyValuePair<Vector2Int, GameObject> kvp in tiles)
                {
                    kvp.Value.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); // Set all tiles to transparent
                    distList.Add(kvp.Key);
                }
                distList.Sort((v1, v2) => (v1 - center).sqrMagnitude.CompareTo((v2 - center).sqrMagnitude)); // Sort list based on distance from center

                float animationSpeed = 0.1f;

                float fadeInDuration = animationSpeed / distList.Count;
                delay = 0f;

                distList = ShuffleList(distList);

                // Go through the tiles and assign them a semi-random shade of orange based on their distance to the center point
                foreach (Vector2Int tilePos in distList)
                {
                    GameObject tileObject = tiles[tilePos];
                    float distance = Vector2Int.Distance(tilePos, center);
                    float normalizedDistance = distance / weapon.explosion.radius;

                    Color tileColor = AdjustColor(e_orange, normalizedDistance); // Tiles are orange

                    if(tilePos == center) // Center is yellow
                    {
                        tileColor = e_yellow;
                    }

                    SetTileColor(tileObject, new Color(tileColor.r, tileColor.g, tileColor.b, 0f)); // Start with fully transparent

                    // Fade in this tile before moving to the next one
                    StartCoroutine(IndividualFade(tileObject, true, 0.2f, delay += fadeInDuration));
                    //yield return new WaitForSeconds(fadeInDuration);
                }

                //yield return null;
                yield return new WaitForSeconds(0.5f);

                // Now fade out
                float fadeOutDuration = animationSpeed / distList.Count;
                delay = 0f;

                foreach (Vector2Int tilePos in distList)
                {
                    GameObject tileObject = tiles[tilePos];

                    StartCoroutine(IndividualFade(tileObject, false, 0.2f, delay += fadeInDuration));
                    //yield return new WaitForSeconds(fadeOutDuration);
                }

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

        yield return new WaitForSeconds(delay);

        // Destroy all animated tiles
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

    private void SetTileColor(GameObject tileObject, Color color)
    {
        SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
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

    private Color AdjustColor(Color baseColor, float normalizedDistance)
    {
        // Adjust brightness based on distance
        float brightness = Mathf.Lerp(0.5f, 1f, 1f - normalizedDistance); // Adjust these values for the desired range of brightness

        // Add a bit of randomness to the brightness
        brightness += Random.Range(-0.1f, 0.1f);

        // Create and return the adjusted color
        return new Color(brightness, baseColor.g, baseColor.b);
    }

    private IEnumerator IndividualFade(GameObject tile, bool fadeIn, float animTime, float delay = 0f)
    {
        float A = 1f, B = 1f;
        if (fadeIn)
        {
            A = 0f;
        }
        else
        {
            B = 0f;
        }

        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;

        float duration = animTime;
        while (elapsedTime < duration)
        {
            if(tile != null)
            {
                Color setColor = tile.GetComponent<SpriteRenderer>().color;
                setColor.a = Mathf.Lerp(A, B, elapsedTime / duration);
                tile.GetComponent<SpriteRenderer>().color = setColor;

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            else
            { // In case we delete this tile while its animating
                yield break;
            }
        }
    }

    private List<Vector2Int> ShuffleList(List<Vector2Int> inputList)
    {    //take any list of points and return it with Fischer-Yates shuffle
        int i = 0;
        int t = inputList.Count;
        int r = 0;
        Vector2Int p = Vector2Int.zero;
        List<Vector2Int> tempList = new List<Vector2Int>();
        tempList.AddRange(inputList);

        while (i < t)
        {
            r = Random.Range(i, tempList.Count);
            p = tempList[i];
            tempList[i] = tempList[r];
            tempList[r] = p;
            i++;
        }

        return tempList;
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
