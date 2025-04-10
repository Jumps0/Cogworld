using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UIBorderIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sprite;
    public Vector2Int machine_parent;
    [SerializeField] private Animator animator;
    [SerializeField] private Material material;

    [Header("Values")]
    public float normalBrightness = 1.0f;
    public float flashBrightnessLow = 0.6f;
    public float flashBrightnessHigh = 1.6f;
    public float flashDuration = 2.0f;

    private bool isFlashing = false;
    private float startTime;

    public void SetValues(Sprite sprite_ascii, Color sprite_color, Vector2Int parentLocation)
    {
        sprite.sprite = sprite_ascii;
        sprite.color = sprite_color;
        machine_parent = parentLocation;
    }

    bool setup = false;
    private void Setup()
    {
        // Get the material attached to the sprite renderer or image
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            // Set the initial brightness
            material.SetFloat("_Brightness", normalBrightness);
        }
    }

    void Update()
    {
        if(this.gameObject.activeInHierarchy && !setup)
        {
            Setup();
        }
    }

    private Coroutine animate;

    public void SetFlash(bool flash)
    {
        isFlashing = flash;

        if (GlobalSettings.inst.animateBorderIndicators)
        {
            if (isFlashing)
            {
                if (animate == null)
                {
                    material.SetFloat("_Brightness", flashBrightnessLow);
                    animate = StartCoroutine(AnimateFlash());
                }
            }
            else
            {
                if (animate != null)
                {
                    StopCoroutine(animate);
                }

                // Reset to normal brightness when exiting flash state
                material.SetFloat("_Brightness", normalBrightness);
            }
        }
        else
        {
            material.SetFloat("_Brightness", normalBrightness);
        }
    }

    private IEnumerator AnimateFlash()
    {
        // We want to go from: Dark -> Bright -> Dark
        // Over a period of 2 seconds
        material.SetFloat("_Brightness", flashBrightnessLow);

        float elapsedTime = 0f;
        float duration = flashDuration / 2;
        while (elapsedTime < duration)
        {
            float lerp = Mathf.Lerp(flashBrightnessLow, flashBrightnessHigh, elapsedTime / duration);
            material.SetFloat("_Brightness", lerp);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = flashDuration / 2;
        while (elapsedTime < duration)
        {
            float lerp = Mathf.Lerp(flashBrightnessHigh, flashBrightnessLow, elapsedTime / duration);
            material.SetFloat("_Brightness", lerp);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_Brightness", flashBrightnessLow);

        // Loop again
        if (isFlashing)
        {
            animate = StartCoroutine(AnimateFlash());
        }
    }

    private void OnDestroy()
    {
        if (UIManager.inst && UIManager.inst.GetComponent<BorderIndicators>().locations.ContainsValue(this.gameObject))
        {
            UIManager.inst.GetComponent<BorderIndicators>().locations.Remove(machine_parent); // This should work?
        }
    }

}
