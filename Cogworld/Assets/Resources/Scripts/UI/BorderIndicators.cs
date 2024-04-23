using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class BorderIndicators : MonoBehaviour
{
    [Header("Border")]
    public Transform borderParent;
    public List<Transform> points = new List<Transform>();
    public Dictionary<Vector2Int, GameObject> locations = new Dictionary<Vector2Int, GameObject>();

    [Header("Values")]
    private float screenEdgeBuffer = 35f; // Buffer distance from the screen edge

    [Header("Prefabs")]
    public GameObject prefab_indicator;

    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (PlayerData.inst)
        {
            // We are gonna do this instead of update so we aren't calling 9 for loops every frame.
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
                || Input.GetMouseButton(0)) // This last one might get a bit rough but we need to update it every time the camera moves (maybe have a separate call in camera movement?)
            {
                // Reposition the border
                borderParent.position = mainCamera.transform.position - new Vector3(mainCamera.GetComponentInParent<CameraController>()._offsetX, mainCamera.GetComponentInParent<CameraController>()._offsetY);

                // Update the indicators
                UpdateAllIndicators();
            }
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
            // Convert machine's world position to screen coordinates
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(machine.transform.position);

            // Adjust screen position for covered areas and screen buffer
            Vector3 adjustedScreenPosition = AdjustScreenPosition(screenPosition);

            // Convert adjusted screen position back to world coordinates
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(adjustedScreenPosition);

            // Place indicator at adjusted world position
            PlaceIndicatorAtScreenEdge(worldPosition, machine);
        }
        else
        {
            // If machine is on screen, position indicator above machine
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(machine.transform.position);
            PlaceIndicatorAboveMachine(screenPosition, machine);
        }
    }

    public float topCoveredPercentage = 0.25f; // Percentage of the top of the screen covered by UI
    public float rightCoveredPercentage = 0.25f; // Percentage of the right of the screen covered by UI


    private Vector3 AdjustScreenPosition(Vector3 screenPosition)
    {
        float topCoveredHeight = Screen.height * topCoveredPercentage;
        float rightCoveredWidth = Screen.width * rightCoveredPercentage;

        // Adjust screen position for covered areas and screen buffer
        float x = Mathf.Clamp(screenPosition.x, screenEdgeBuffer, Screen.width - screenEdgeBuffer - rightCoveredWidth);
        float y = Mathf.Clamp(screenPosition.y, screenEdgeBuffer, Screen.height - screenEdgeBuffer - topCoveredHeight);

        return new Vector3(x, y, screenPosition.z);
    }

    private bool IsMachineVisible(MachinePart machine)
    {
        // Check if machine is within camera's view frustum, adjusted for UI coverage
        Vector3 screenPosition = mainCamera.WorldToViewportPoint(machine.transform.position);

        float topCoveredHeight = Screen.height * topCoveredPercentage;
        float rightCoveredWidth = Screen.width * rightCoveredPercentage;

        return (screenPosition.x >= 0 && screenPosition.x <= 1 &&
                screenPosition.y >= topCoveredHeight / Screen.height && screenPosition.y <= 1 &&
                screenPosition.x >= 0 && screenPosition.x <= (1 - rightCoveredWidth / Screen.width));
    }

    private Vector3 GetScreenPosition(Vector3 worldPosition)
    {
        // Convert world position to screen position
        return mainCamera.WorldToScreenPoint(worldPosition);
    }

    private void PlaceIndicatorAtScreenEdge(Vector3 screenPosition, MachinePart machine)
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
        // If an indicator doesn't exist for this machine create one
        if (machine.parentPart.indicator == null)
        {
            CreateMachineIndicator(machine);
        }
        GameObject indicator = machine.parentPart.indicator;
        indicator.transform.position = screenPosition;
    }

    private void PlaceIndicatorAboveMachine(Vector3 screenPosition, MachinePart machine)
    {
        // Set indicator position above the machine
        // Instantiate or move indicator prefab to the calculated screen position
        // If an indicator doesn't exist for this machine create one
        if (machine.parentPart.indicator == null)
        {
            CreateMachineIndicator(machine);
        }
        GameObject indicator = machine.parentPart.indicator;
        indicator.transform.position = machine.parentPart.transform.position;
    }

    /*
    private void PlaceIndicatorAtScreenEdge(Vector3 screenPosition, MachinePart machine)
    {
        // If an indicator doesn't exist for this machine create one
        if (machine.parentPart.indicator == null)
        {
            CreateMachineIndicator(machine);
        }
        GameObject indicator = machine.parentPart.indicator;
        Vector3 pos = machine.parentPart.transform.position;
        // We want to place this indicator to the nearest free border point
        Transform closest = points[0];

        foreach (Transform P in points)
        {
            // Is this position already taken?
            if (!locations.ContainsKey(HF.V3_to_V2I(P.transform.position)))
            {
                float distance1 = Vector2.Distance(pos, closest.position);
                float distance2 = Vector2.Distance(pos, P.position);

                if(distance1 < distance2) // What we have is better, don't add it
                {

                }
                else // This point is better, replace it
                {
                    closest = P;
                }
            }
        }

        // Assign the indicator to the new position (as child)
        indicator.transform.SetParent(closest);
        // And mark that spot as taken in the dictionary
        if (!locations.ContainsKey(HF.V3_to_V2I(closest.transform.position)))
        {
            locations.Add(HF.V3_to_V2I(closest.transform.position), indicator);
        }
    }

    private void PlaceIndicatorAboveMachine(Vector3 screenPosition, MachinePart machine)
    {
        // If an indicator doesn't exist for this machine create one
        if (machine.parentPart.indicator == null)
        {
            CreateMachineIndicator(machine);
        }
        GameObject indicator = machine.parentPart.indicator;
        indicator.transform.position = machine.parentPart.transform.position; // Reset position

        // And remove it from the dictionary
        if (locations.ContainsKey(HF.V3_to_V2I(indicator.transform.position)))
        {
            locations.Remove(HF.V3_to_V2I(indicator.transform.position));
        }
    }
    */

    public void CreateMachineIndicator(MachinePart machine)
    {
        GameObject go = Instantiate(prefab_indicator, machine.parentPart.transform.parent.transform.position, Quaternion.identity);
        go.transform.SetParent(machine.parentPart.transform.parent.transform);
        // Assign it the to machine
        machine.parentPart.indicator = go;
        // Assign Sprite
        go.GetComponent<UIBorderIndicator>().sprite.sprite = machine.parentPart.GetComponent<SpriteRenderer>().sprite;
        go.GetComponent<UIBorderIndicator>().parent = machine.parentPart;
    }
}
