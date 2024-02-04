using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


/// <summary>
/// Manages everything related to the "CODES" menu while hacking.
/// </summary>
public class UIHackCodes : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI codesHeader_text;
    public Image codesHeader_image;
    //
    public TextMeshProUGUI codeHeader_text;
    public Image codeHeader_image;
    public TextMeshProUGUI sourceHeader_text;
    public Image sourceHeader_image;
    public TextMeshProUGUI targetHeader_text;
    public Image targetHeader_image;
    [Header("    Box Refs")]
    public Image greenBorders;

    [Header("Color Pallet")]
    public Color fadedGray;
    public Color headerWhite;
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    [Header("Codes")]
    public List<TerminalCustomCode> codes = new List<TerminalCustomCode>();


    public void Setup()
    {
        codes = PlayerData.inst.customCodes; // Sync codes

        DoEntryAnimation();
    }

    public void DoEntryAnimation()
    {
        StartCoroutine(EntryAnim());
    }

    IEnumerator EntryAnim()
    {
        // No audio upon opening

        // ** Animation **
        // The backing bars, header, code, source, and target all flash in like the main menu does.
        // Any codes appear in with random highlights.

        float delay = 0.1f;

        Color usedColor = lowDetColor;

        codesHeader_image.color = usedColor;

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        codeHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        //targetHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);

        yield return new WaitForSeconds(delay);

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.8f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        //targetHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        codeHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        //targetHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);

        yield return new WaitForSeconds(delay);

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        codeHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        //targetHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);

        yield return new WaitForSeconds(delay);

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        codeHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        //targetHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        codesHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        //codeHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        codeHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        //sourceHeader_text.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        sourceHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        //targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        targetHeader_image.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);


        yield return null;
    }

    public void UpdateText()
    {

    }

    private IEnumerator UpdateDetText()
    {
        /*
        float delay = 0.1f;

        Color usedColor = _detValueText.color;

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        */
        yield return null;
    }

    public void ShutDown()
    {
        // No animation here, just closes instantly
    }
}
