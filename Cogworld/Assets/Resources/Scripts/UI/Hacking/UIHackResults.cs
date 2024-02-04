using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Used for the printed out results during hacking.
/// </summary>
public class UIHackResults : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI primaryText;
    public TextMeshProUGUI backerText; // The black highlight

    // Info
    private string setText;
    private Color setColor;

    [Header("Color Pallet")]
    public Color headerWhite;
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    [SerializeField] private float textSpeed = 0.007f;

    bool doDialogue = false;
    public void Setup(string text, Color newColor, bool hasDialogue = false)
    {
        setText = text;
        setColor = newColor;
        primaryText.color = setColor;
        doDialogue = hasDialogue;

        AppearAnim();
    }

    public void AppearAnim()
    {
        StartCoroutine(AnimateReveal());
    }

    IEnumerator AnimateReveal()
    {
        AudioManager.inst.PlayTyping();

        string _message = setText;
        int len = _message.Length;
        primaryText.text = "";
        backerText.text = "";
        for (int i = 0; i < len; i++)
        {
            if (instantFinish)
            {
                yield break;
            }

            // Set the primary text
            primaryText.text += _message[i];
            // Set the backer text
            backerText.text = "<mark=#000000>" + primaryText.text + "</mark><br><br>"; // Mark highlights it as pure black | br br for spacing

            yield return new WaitForSeconds(textSpeed * Time.deltaTime);

            if (instantFinish)
            {
                yield break;
            }
        }

        this.gameObject.name = _message;

        primaryText.text += "<br><br>";
        /*
        if (doDialogue) // !!! - Now rendundant, the log readout does the audio now. - !!!
        {
            // When finished, stop playing the text sound
            AudioManager.inst.StopMiscSpecific2();
        }
        */

        // Force the text to update to include the page breaks (this is stupid, why does TMPro work like this?).
        primaryText.ForceMeshUpdate();
        backerText.ForceMeshUpdate();

        AudioManager.inst.StopTyping();
    }

    private bool instantFinish = false;
    public void InstantFinish()
    {
        StopCoroutine(AnimateReveal());
        instantFinish = true;
        primaryText.text = setText;
        this.gameObject.name = setText;
        backerText.text = "<mark=#000000>" + primaryText.text + "</mark><br><br>";
        primaryText.text += "<br><br>";

        primaryText.ForceMeshUpdate();
        backerText.ForceMeshUpdate();
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
