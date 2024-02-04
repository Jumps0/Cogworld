using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// If true, the camera is locked to the player.
    /// </summary>
    public bool lockCamToPlayer = true;

    [Tooltip("Reference to the Camera Core GameObject.")]
    public GameObject cameraReference;

    [SerializeField] private float _offsetX = 11;
    [SerializeField] private float _offsetY = 5;

    void Update()
    {
        if(!MapManager.inst.debugDisabled && MapManager.inst.playerRef != null)
        {
            CheckCamSettings();
        }
        else
        {
            SetCamFree();
        }
            
    }

    private void CheckCamSettings()
    {
        if (lockCamToPlayer)
        {
            cameraReference.transform.SetParent(MapManager.inst.playerRef.transform, false);
            this.transform.position = MapManager.inst.playerRef.transform.position;
            Camera.main.transform.localPosition = new Vector3(0 + _offsetX, 0 + _offsetY, -10);
        }
        else
        {
            SetCamFree();
        }
    }

    public void SetCamFree()
    {
        this.transform.parent = null;
        cameraReference.transform.SetParent(null);
        Camera.main.transform.localPosition = new Vector3(50, 50);
    }
}
