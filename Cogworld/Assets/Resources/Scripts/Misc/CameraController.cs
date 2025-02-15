using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    /// <summary>
    /// If true, the camera is locked to the player.
    /// </summary>
    public bool lockCamToPlayer = true;

    [Tooltip("Reference to the Camera Core GameObject.")]
    public GameObject cameraReference;
    [SerializeField] private Transform owner;
    [SerializeField] private Camera cam;

    public float _offsetX = 17;
    public float _offsetY = 5;

    public override void OnNetworkSpawn()
    {
        cameraReference.SetActive(owner.gameObject.GetComponent<Actor>().IsOwner);

        if (owner.gameObject.GetComponent<Actor>().IsOwner)
        {
#if (UNITY_EDITOR) // For zoom level of 20
            _offsetX = 11;
            _offsetY = 5;

            cam.orthographicSize = 20; // So testing in editor isn't nearly impossible to see
#endif
        }
    }

    void Update()
    {
        if (!MapManager.inst.debugDisabled)
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
            cameraReference.transform.SetParent(owner, false);
            this.transform.position = owner.position;
            cam.transform.localPosition = new Vector3(0 + _offsetX, 0 + _offsetY, -10);
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
        cam.transform.localPosition = new Vector3(50, 50);
    }
}
