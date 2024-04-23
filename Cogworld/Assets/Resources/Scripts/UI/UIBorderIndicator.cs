using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBorderIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sprite;
    public Image image;
    public MachinePart machine_parent;
    private Transform main;
    private float size;

    private void Start()
    {
        main = UIManager.inst.GetComponent<BorderIndicators>().borderParent;
        size = UIManager.inst.GetComponent<BorderIndicators>().size;

        if(image != null)
        {
            this.GetComponent<Canvas>().sortingOrder = 75;
        }
    }

    private void Update()
    {
        
    }

    public void LocationUpdate()
    {
        size = UIManager.inst.GetComponent<BorderIndicators>().size;

        /*
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, main.position.x-size, size+main.position.x),
            Mathf.Clamp(transform.position.y, main.position.y - size, size + main.position.y),
            transform.position.z
        );
        */
        /*
        Vector3 offset = Vector3.ClampMagnitude(parent.transform.position - PlayerData.inst.transform.position, Camera.main.orthographicSize);
        offset = offset / Camera.main.orthographicSize * (UIManager.inst.GetComponent<BorderIndicators>().borderRect.rect.width / 2f);
        this.transform.position = new Vector3(offset.x * -1, offset.y * -1);
        */

        bool visible = false;
        if (sprite != null)
        {
            visible = IsVisible(sprite);
        }

        // Check if the GameObject has an Image component
        if (image != null)
        {
            visible = IsVisible(image.rectTransform);
        }

        if (!visible) // Not visible
        {
            Vector2 direction = PlayerData.inst.transform.position - machine_parent.transform.position;

            int layer_mask = LayerMask.GetMask("Edge");
            RaycastHit2D ray = Physics2D.Raycast(this.transform.position, direction, 2000, layer_mask);

            if (ray.collider != null)
            {
                Debug.Log("Hit! " + ray.collider.gameObject.name + " - " + ray.point);
                this.transform.position = ray.point;
            }
            
        }
        else
        {
            this.transform.position = machine_parent.transform.position;
        }
    }
    

    private void OnDestroy()
    {
        if (UIManager.inst && UIManager.inst.GetComponent<BorderIndicators>().locations.ContainsValue(this.gameObject))
        {
            UIManager.inst.GetComponent<BorderIndicators>().locations.Remove(HF.V3_to_V2I(this.gameObject.transform.position)); // This should work?
        }
    }

    #region Helpers
    public bool IsVisibleToCamera(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("GameObject is null.");
            return false;
        }

        // Check if the GameObject has a SpriteRenderer component
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return IsVisible(spriteRenderer);
        }

        // Check if the GameObject has an Image component
        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            return IsVisible(image.rectTransform);
        }

        Debug.LogWarning("GameObject does not have a SpriteRenderer or Image component.");
        return false;
    }

    private bool IsVisible(Renderer renderer)
    {
        // Check if the renderer is visible from any camera
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), renderer.bounds);
    }

    private bool IsVisible(RectTransform rectTransform)
    {
        // Get the camera's viewport bounds
        Rect viewportBounds = new Rect(0f, 0f, 1f, 1f); // Full viewport
        //viewportBounds.yMin += 0.15f; // Adjusted for UI covering top 15%

        // Convert the RectTransform's position to screen space
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(rectTransform.position);

        // Check if the RectTransform is within the viewport bounds
        return viewportBounds.Contains(screenPoint);
    }
    #endregion
}
