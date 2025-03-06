using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// from tutorial: https://www.youtube.com/watch?v=-zOMX7CcxAo

public class BorderIndicators : MonoBehaviour
{
    [Header("Border")]
    public Transform borderParent;
    public RectTransform borderRect;
    public List<Transform> points = new List<Transform>();
    public Dictionary<Vector2Int, GameObject> locations = new Dictionary<Vector2Int, GameObject>();

    [Header("Values")]
    private Vector2 buffer = new Vector2(0.705f, 0.813f);
    public float size;

    [Header("Prefabs")]
    public GameObject prefab_indicator;
    public GameObject prefab_indicator_alt;

    private Camera _camera;
    private float _spriteWidth;
    private float _spriteHeight;

    private Dictionary<Vector2Int, GameObject> _targetIndicators = new Dictionary<Vector2Int, GameObject>();

    public void CreateIndicators()
    {
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


    private void LateUpdate()
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

    private void UpdateIndicator(Vector2Int target, GameObject indicator)
    {
        var screenPos = _camera.WorldToViewportPoint(new Vector3(target.x, target.y));
        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= buffer.x || screenPos.y <= 0 || screenPos.y >= buffer.y; // Defaults are 0 | 1 | 0 | 1. We modify due to UI covering some of the screen
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
            worldPosition.z = 0;

            // Minor adjustment
            Vector3 offset = new Vector3(0, 0);
            /*
            if(worldPosition.x < PlayerData.inst.transform.position.x) // Left side of screen
            {
                offset.x = 1;
            }
            else // Right side of screen
            {
                offset.x = -1;
            }
            */
            /*
            if(worldPosition.y < PlayerData.inst.transform.position.y) // Bottom part of screen
            {
                offset.y = 1;
            }
            else // Top part of screen
            {
                offset.y = -1;
            }
            */
            worldPosition += offset;

            // Set the position
            indicator.transform.position = worldPosition;

            // Make sure its flashing
            indicator.GetComponent<UIBorderIndicator>().SetFlash(true);

            //Vector3 direction = target.transform.position - indicator.transform.position;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //indicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            // Stop it from flashing
            indicator.GetComponent<UIBorderIndicator>().SetFlash(false);

            indicator.SetActive(false);
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

        // Assign Sprite
        go.GetComponent<UIBorderIndicator>().sprite.sprite = MapManager.inst.GetTileAt(pos).sprite;

        // Assign Parent
        go.GetComponent<UIBorderIndicator>().machine_parent = pos;

        return go;
    }
}
