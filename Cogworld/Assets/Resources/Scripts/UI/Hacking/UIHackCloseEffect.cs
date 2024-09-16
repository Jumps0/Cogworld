using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHackCloseEffect : MonoBehaviour
{
    [SerializeField] private RectTransform hackingArea; // The RectTransform of the hacking menu
    [SerializeField] private RectTransform movingBar;   // The bar that will move from top to bottom
    [SerializeField] private Image blackCover;          // The black cover sprite with a fill amount

    [Range(0.5f, 5f)]
    [SerializeField] private float animationDuration = 1.5f;

    void Start()
    {
        ResetEffect(); // Ensure initial states are set (bar at the top, cover at 0% fill)
    }

    public void ResetEffect()
    {
        // Place the bar at the top of the hackingArea
        movingBar.anchoredPosition = new Vector2(movingBar.anchoredPosition.x, hackingArea.rect.height);

        blackCover.fillAmount = 0f; // Set the black cover fill to 0%

        movingBar.gameObject.SetActive(false); // Disable the bar
        blackCover.enabled = false; // Disable the cover
        UIManager.inst.terminal_static.SetActive(false); // Disable the static
    }

    public void Close()
    {
        StartCoroutine(CloseEffectCoroutine());
    }

    IEnumerator CloseEffectCoroutine()
    {
        float elapsedTime = 0f;

        movingBar.gameObject.SetActive(true); // Enable the bar
        blackCover.enabled = true; // Enable the cover

        // Initial position of the bar (top of hackingArea)
        Vector2 startPos = new Vector2(movingBar.anchoredPosition.x, hackingArea.rect.height);
        Vector2 endPos = new Vector2(movingBar.anchoredPosition.x, 0f); // Final position (bottom)

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            // Move the bar from top to bottom
            movingBar.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // Gradually increase the fill amount of the black cover
            blackCover.fillAmount = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        // Ensure final state is set (bar at bottom, cover fully filled)
        movingBar.anchoredPosition = endPos;
        blackCover.fillAmount = 1f;

        movingBar.gameObject.SetActive(false); // Disable the bar

        // Enable the static in UIManager
        UIManager.inst.terminal_static.SetActive(true);
    }
}
