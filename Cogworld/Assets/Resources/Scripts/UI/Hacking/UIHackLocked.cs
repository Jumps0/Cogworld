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

    public void Setup(Color setColor, string setText)
    {
        // This just appears so nice and easy
        backgroundImage.color = setColor;
        displayText.text = setText;

        // Play sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[46]); // HACK_TRACED
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
