using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    #region Explosion FX
    public void CreateExplosionFX(Vector2Int pos, ItemObject weapon, List<Vector2Int> tiles)
    {
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

        StartCoroutine(AnimateExplosionFX());
    }

    private IEnumerator AnimateExplosionFX()
    {
        

        yield return null;

        // Destroy all animated tiles

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