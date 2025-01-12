using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Used for the "X LOCKED" while hacking
/// </summary>
public class UIHackLocked : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI displayText;
    public Image backgroundImage;

    public void Setup(Color setColor, string setText, bool doSound = true)
    {
        // This just appears so nice and easy
        backgroundImage.color = setColor;
        displayText.text = setText;

        if (doSound)
        {
            // Play sound
            AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HACK_TRACED"]); // UI - HACK_TRACED
        }
    }

    public void ShutDown()
    {
        StartCoroutine(ShutdownAnim());
    }

    private IEnumerator ShutdownAnim()
    {

        yield return null;

        Destroy(this.gameObject);

    }
}
