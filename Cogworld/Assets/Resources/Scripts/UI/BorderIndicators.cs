using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// from tutorial: https://www.youtube.com/watch?v=-zOMX7CcxAo

public class BorderIndicators : MonoBehaviour
{
    [Header("Border")]
    public Transform borderParent;
    public RectTransform borderRect;

    [Header("Values")]
    private Vector2 buffer = new Vector2(0.705f, 0.8f);
    public float size;

    [Header("Prefabs")]
    public GameObject prefab_indicator;
    public GameObject prefab_indicator_alt;

    private Camera _camera;
    private float _spriteWidth;
    private float _spriteHeight;

    public Dictionary<Vector2Int, GameObject> _targetIndicators = new Dictionary<Vector2Int, GameObject>();

    public void CreateIndicators()
    {
        TurnManager.inst.turnEvents.onTurnTick += TurnTick; // Begin listening to this event

        // Destroy any pre-existing indicators !!!
        foreach (var kvp in _targetIndicators.ToList())
        {
            Destroy(kvp.Value);
            // NOTE: The script `UIBorderIndicator` has an OnDestroy which will remove it from this list.
        }

        _camera = Camera.main;

        var bounds = prefab_indicator.GetComponent<SpriteRenderer>().bounds;
        _spriteHeight = bounds.size.y / 2f;
        _spriteWidth = bounds.size.x / 2f;

        List<Vector2Int> machines = HF.GetMachinesByType(MachineType.None);

        foreach (Vector2Int M in machines)
        {
            if (MapManager.inst.mapdata[M.x, M.y].machinedata.type != MachineType.Static && MapManager.inst.mapdata[M.x, M.y].machinedata.indicator == null)
            {
                var indicator = CreateMachineIndicator(M);
                _targetIndicators.Add(M, indicator);
            }
        }
    }


    private void TurnTick()
    {
        if (PlayerData.inst && MapManager.inst.loaded)
        {
            // Update the indicators
            foreach (KeyValuePair<Vector2Int, GameObject> pair in _targetIndicators)
            {
                UpdateIndicator(pair.Key, pair.Value);
            }
        }
    }

    /// <summary>
    /// Update a specific indiciator for a machine to a new location.
    /// </summary>
    /// <param name="target">The origin location of the indicator (where it should ideally be).</param>
    /// <param name="indicator">The indicator itself.</param>
    private void UpdateIndicator(Vector2Int target, GameObject indicator)
    {
        UIBorderIndicator I = indicator.GetComponent<UIBorderIndicator>();

        var screenPos = _camera.WorldToViewportPoint(new Vector3(target.x, target.y));

        // Check if the indicator (aka the core of an interactable machine) is off screen
        bool OFF_LEFT = screenPos.x <= 0, OFF_RIGHT = screenPos.x >= buffer.x;  // Defaults are 0 | 1 | 0 | 1. We modify due to UI covering some of the screen
        bool OFF_BOTTOM = screenPos.y <= 0, OFF_TOP = screenPos.y >= buffer.y;
        bool isOffScreen = OFF_LEFT || OFF_RIGHT || OFF_BOTTOM || OFF_TOP;

        // If it is offscreen, we need to move it to the edge of the screen.
        if (isOffScreen)
        {
            indicator.SetActive(true);
            var spriteSizeInViewPort = _camera.WorldToViewportPoint(new Vector3(_spriteWidth, _spriteHeight, 0)) - _camera.WorldToViewportPoint(Vector3.zero);

            // UI Clamping
            if (screenPos.x >= buffer.x || screenPos.y >= buffer.y)
            {
                screenPos.x = Mathf.Clamp(screenPos.x, 0, buffer.x);
                screenPos.y = Mathf.Clamp(screenPos.y, 0, buffer.y);
            }

            screenPos.x = Mathf.Clamp(screenPos.x, spriteSizeInViewPort.x, 1 - spriteSizeInViewPort.x);
            screenPos.y = Mathf.Clamp(screenPos.y, spriteSizeInViewPort.y, 1 - spriteSizeInViewPort.y);

            var worldPosition = _camera.ViewportToWorldPoint(screenPos);
            /*
            // Round to nearest 0.5
            worldPosition.x = Mathf.Round(worldPosition.x * 2) / 2;
            worldPosition.y = Mathf.Round(worldPosition.y * 2) / 2;
            */
            // Round to nearest int
            worldPosition.x = Mathf.RoundToInt(worldPosition.x);
            worldPosition.y = Mathf.RoundToInt(worldPosition.y);
            worldPosition.z = 0;

            #region Overlapping Prevention
            // Before we relocate the indicator, we need to make sure it currently isn't overlapping with another indicator.
            // If it is, we need to shift it over slightly.

            // This is a double loop and I don't like it but the indicator list will (usually) not be that large so hopefully its not a big deal.
            foreach (KeyValuePair<Vector2Int, GameObject> kvp in _targetIndicators.ToList())
            {
                // Is the current indicator overlapping with this indicator?
                if(indicator != kvp.Value && (worldPosition == kvp.Value.transform.position))
                {
                    // It is, we need to "bump" the current indicator `1` in a certain direction.
                    // We determine this based on the flags we generated earlier.

                    float bumpAmount = 1;
                    Vector3 bumpedPosition = worldPosition;

                    // First check if its at a corner
                    bool CORNER_TL = OFF_LEFT && OFF_TOP, CORNER_TR = OFF_RIGHT && OFF_TOP, CORNER_BL = OFF_LEFT && OFF_BOTTOM, CORNER_BR = OFF_RIGHT && OFF_BOTTOM;
                    bool IN_CORNER = CORNER_TL || CORNER_TR || CORNER_BL || CORNER_BR;
                    if (IN_CORNER)
                    {
                        int cf = Random.Range(0, 2);
                        if (CORNER_TL)
                        {
                            // Move DOWN or RIGHT
                            if(cf > 0)
                            {
                                // DOWN
                                bumpedPosition.y -= bumpAmount;
                            }
                            else
                            {
                                // RIGHT
                                bumpedPosition.x += bumpAmount;
                            }
                        }
                        else if (CORNER_TR)
                        {
                            // Move DOWN or LEFT
                            if (cf > 0)
                            {
                                // DOWN
                                bumpedPosition.y -= bumpAmount;
                            }
                            else
                            {
                                // LEFT
                                bumpedPosition.x -= bumpAmount;
                            }
                        }
                        else if (CORNER_BL)
                        {
                            // Move UP or RIGHT
                            if (cf > 0)
                            {
                                // UP
                                bumpedPosition.y += bumpAmount;
                            }
                            else
                            {
                                // RIGHT
                                bumpedPosition.x += bumpAmount;
                            }
                        }
                        else if (CORNER_BR)
                        {
                            // Move UP or LEFT
                            if (cf > 0)
                            {
                                // UP
                                bumpedPosition.y += bumpAmount;
                            }
                            else
                            {
                                // LEFT
                                bumpedPosition.x -= bumpAmount;
                            }
                        }
                    }
                    else // If not then it's a bit simpler
                    {
                        // Coinflip choice of which direction we bump in, since we usually will have space on either side.
                        int cf = Random.Range(0, 2);
                        if(cf > 0) { bumpAmount *= -1; }

                        if (OFF_LEFT || OFF_RIGHT)
                        {
                            // We need to move UP or DOWN
                            bumpedPosition.y += bumpAmount;
                        }
                        else if (OFF_TOP || OFF_BOTTOM)
                        {
                            // We need to move LEFT or RIGHT
                            bumpedPosition.x += bumpAmount;
                        }
                    }

                    // But before we go and bump the indicator, we need to verify that it would not be pushed off the screen.
                    // If it does, we can accept the overlapping.
                    if (bumpedPosition.x <= 0 || bumpedPosition.x >= 59 || bumpedPosition.y <= 0 || bumpedPosition.y >= 63.55f)
                    {
                        // What a shame, no bumping.
                        Debug.Log($"No bump [{bumpedPosition}] LEFT:{bumpedPosition.x <= 0}, RIGHT:{bumpedPosition.x >= 59}, DOWN:{bumpedPosition.y <= 0}, UP:{bumpedPosition.y >= 63.55f}");
                    }
                    else
                    {
                        // All good! Go ahead and bump.
                        worldPosition = bumpedPosition;
                        Debug.Log($"Bumping! OG{worldPosition} vs {kvp.Value.transform.position}");
                    }
                }
            }

            #endregion

            // Set the position
            indicator.transform.position = worldPosition;

            // Make sure its flashing
            I.SetFlash(true);

            //Vector3 direction = target.transform.position - indicator.transform.position;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //indicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else // If it's not offscreen, it shouldn't be flashing, and it needs to be placed ontop of the parent machine.
        {
            // Stop it from flashing
            I.SetFlash(false);

            // And set it back to being ontop of the machine
            indicator.transform.position = new Vector3(I.machine_parent.x, I.machine_parent.y);

            //indicator.SetActive(false);
        }
    }

    public GameObject CreateMachineIndicator(Vector2Int pos)
    {
        GameObject go = Instantiate(prefab_indicator, new Vector3(pos.x, pos.y), Quaternion.identity);

        go.transform.SetParent(UIManager.inst.transform);

        // - What was this?
        //GameObject go = Instantiate(prefab_indicator_alt, machine.parentPart.transform.parent.transform.position, Quaternion.identity);
        //go.transform.SetParent(machine.parentPart.transform.parent.transform);
        //go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        // Assign it the to machine
        MapManager.inst.mapdata[pos.x, pos.y].machinedata.indicator = go;
        WorldTile tile = MapManager.inst.mapdata[pos.x, pos.y];

        UIBorderIndicator indicator = go.GetComponent<UIBorderIndicator>();
        // Assign the indicators values.
        indicator.SetValues(tile.machinedata.sprite_override.sprite, HF.GetMachineColor(tile.machinedata.type), pos);

        return go;
    }

    private void OnDisable()
    {
        if (GameManager.inst)
        {
            TurnManager.inst.turnEvents.onTurnTick -= TurnTick; // Stop listening to this event
        }
    }
}
