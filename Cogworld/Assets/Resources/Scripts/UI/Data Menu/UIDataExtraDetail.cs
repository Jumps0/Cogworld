using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDataExtraDetail : MonoBehaviour
{
    [Header("Extra Details")]
    public TextMeshProUGUI extraText;
    public GameObject extraParent;
    public string extraAssignedString;
    private Coroutine extraAnim;
    [SerializeField] private Image extraBorders;
    [Tooltip("Whatever object this window is displaying the details of.")]
    [HideInInspector] public GameObject myGameObject = null;

    [Header("Colors")]
    public Color colorBlue;
    public Color colorGray;

    bool flip = false;

    public void Update()
    {
        // If the extra detail menu is active, move it with the mouse
        if (extraParent.activeInHierarchy)
        {
            RectTransform uiElement = extraParent.GetComponent<RectTransform>();
            Vector3 mousePosition = Mouse.current.position.ReadValue();

            uiElement.position = mousePosition + new Vector3 (10, 0);

            // Calculate the distance from the top of the screen
            float distanceFromTop = mousePosition.y;

            // Check if the mouse is close to the bottom of the screen
            if (distanceFromTop < 5f + uiElement.rect.height)
            {
                // Change anchor to bottom left if it hasn't already been changed
                if (!flip)
                {
                    uiElement.anchorMin = new Vector2(uiElement.anchorMin.x, 0f);
                    uiElement.anchorMax = new Vector2(uiElement.anchorMax.x, 0f);
                    uiElement.pivot = new Vector2(uiElement.pivot.x, 0f);
                    flip = true;
                }
            }
            else
            {
                // Reset anchor to top left if it has been changed
                if (flip)
                {
                    uiElement.anchorMin = new Vector2(uiElement.anchorMin.x, 1f);
                    uiElement.anchorMax = new Vector2(uiElement.anchorMax.x, 1f);
                    uiElement.pivot = new Vector2(uiElement.pivot.x, 1f);
                    flip = false;
                }
            }
        }
    }




    public void ShowExtraDetail(string detail)
    {
        extraText.text = detail;

        extraParent.gameObject.SetActive(true);
        if (extraAnim == null)
        {
            extraAnim = StartCoroutine(OpenExtra());
        }
        else
        {
            StopCoroutine(extraAnim);
        }
    }

    private IEnumerator OpenExtra()
    {
        // Play the opening sound
        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.UI_Clips[66], 0.9f); // UI - OPEN_OK

        // There is a bit more to this animation (a green scan bar that goes from top -> bottom, but its visibile for like 5 frames so honestly its not worth it)

        // Do the stretch animation
        if (stretch != null)
        {
            StopCoroutine(stretch);
            extraParent.GetComponent<RectTransform>().localScale = new Vector3(extraParent.GetComponent<RectTransform>().localScale.x, 1f, extraParent.GetComponent<RectTransform>().localScale.z);
        }
        stretch = StartCoroutine(Stretch());

        // Do the two color animations
        extraText.color = Color.black;
        extraBorders.color = Color.black;

        // Black -> Gray
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Black -> Gray
        {
            extraBorders.color = Color.Lerp(Color.black, colorGray, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        extraBorders.color = colorGray;

        // Black -> Blue
        elapsedTime = 0f;
        duration = 0.4f;
        while (elapsedTime < duration) // Black -> Blue
        {
            extraText.color = Color.Lerp(Color.black, colorBlue, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        extraText.color = colorBlue;

        extraAnim = null;
    }

    private Coroutine stretch;
    private IEnumerator Stretch()
    {
        RectTransform rectTransform = extraParent.GetComponent<RectTransform>();

        // Set the initial scale with y scaling set to 0
        Vector3 startScale = new Vector3(rectTransform.localScale.x, 0f, rectTransform.localScale.z);

        // Set the target scale with y scaling set to 1
        Vector3 targetScale = new Vector3(rectTransform.localScale.x, 1f, rectTransform.localScale.z);

        // Initialize the timer
        float timer = 0f;
        float duration = 0.2f;

        // Loop until the animation duration is reached
        while (timer < duration)
        {
            // Increment the timer
            timer += Time.deltaTime;

            // Calculate the interpolation factor
            float t = Mathf.Clamp01(timer / duration);

            // Interpolate the scale between start and target scales
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Wait for the next frame
            yield return null;
        }

        // Ensure the final scale is set to the target scale
        rectTransform.localScale = targetScale;
    }

    public void HideExtraDetail()
    {
        extraAnim = null;
        extraParent.gameObject.SetActive(false);
    }
}
