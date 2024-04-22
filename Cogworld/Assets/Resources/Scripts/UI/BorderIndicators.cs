using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BorderIndicators : MonoBehaviour
{
    [Header("Values")]
    public float screenEdgeBuffer = 50f; // Buffer distance from the screen edge

    [Header("Prefabs")]
    public GameObject prefab_indicator;

    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // We are gonna do this instead of update so we aren't calling 9 for loops every frame.
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetMouseButton(0)) // This last one might get a bit rough but we need to update it every time the camera moves (maybe have a separate call in camera movement?)
        {
            UpdateAllIndicators();
        }
    }

    public void UpdateAllIndicators()
    {
        List<GameObject> machines = HF.GetAllInteractableMachines();

        foreach (GameObject machine in machines)
        {
            MachinePart m = machine.GetComponentInChildren<MachinePart>();
            if (m.isExplored)
            {
                UpdateIndicator(m);
            }
        }
    }

    private void UpdateIndicator(MachinePart machine)
    {
        // Check if machine is off-screen
        if (!IsMachineVisible(machine))
        {
            // Position indicator at edge of screen
            Vector3 screenPosition = GetScreenPosition(machine.transform.position);
            PlaceIndicatorAtScreenEdge(screenPosition);
        }
        else
        {
            // If machine is on screen, position indicator above machine
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(machine.transform.position);
            PlaceIndicatorAboveMachine(screenPosition);
        }
    }

    private bool IsMachineVisible(MachinePart machine)
    {
        // Check if machine is within camera's view frustum
        Vector3 screenPosition = mainCamera.WorldToViewportPoint(machine.transform.position);
        return (screenPosition.x >= 0 && screenPosition.x <= 1 && screenPosition.y >= 0 && screenPosition.y <= 1);
    }

    private Vector3 GetScreenPosition(Vector3 worldPosition)
    {
        // Convert world position to screen position
        return mainCamera.WorldToScreenPoint(worldPosition);
    }

    private void PlaceIndicatorAtScreenEdge(Vector3 screenPosition)
    {
        // Clamp screen position to edge of screen
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float buffer = screenEdgeBuffer;

        // Clamp X position
        screenPosition.x = Mathf.Clamp(screenPosition.x, buffer, screenWidth - buffer);

        // Clamp Y position
        screenPosition.y = Mathf.Clamp(screenPosition.y, buffer, screenHeight - buffer);

        // Set indicator position
        // Instantiate or move indicator prefab to the calculated screen position
    }

    private void PlaceIndicatorAboveMachine(Vector3 screenPosition)
    {
        // Set indicator position above the machine
        // Instantiate or move indicator prefab to the calculated screen position
    }
}
