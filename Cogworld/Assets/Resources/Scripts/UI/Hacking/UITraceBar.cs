using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

/// <summary>
/// Used for the Trace progress bar while hacking
/// </summary>
public class UITraceBar : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI tracePercentText;
    public Image wideBar;
    public Image trueBar;
    public Image facadeBar;
    //
    public Image highlightLeft;
    public Image highlightRight;

    [Header("Color Pallet")]
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;
    //
    public Color trueColor;
    public Color facadeColor;

    public float traceAmount = 0f;

    //[SerializeField] private float textSpeed = 0.01f;

    public void Setup(GameObject terminal)
    {
        // The bar starts out at 0 percent.
        traceAmount = 0f;
        trueBar.fillAmount = 0;
        facadeBar.fillAmount = 0;

        RevealAnim();
    }

    private void SetPercentText(float percent)
    {
        StartCoroutine(PercentTextUpdate(percent));
    }

    private IEnumerator PercentTextUpdate(float percent)
    {
        float delay = 0.1f;

        if (traceAmount >= 0.8f) // V. High
        {
            //_detValueText.text = "High (" + detectionChance + "%)";
            tracePercentText.color = veryHighDetColor;
        }
        else if (traceAmount < 0.8f && traceAmount >= 0.6f) // High
        {
            tracePercentText.color = highDetColor;
        }
        else if (traceAmount < 0.6f && traceAmount >= 0.3f) // Medium
        {
            //_detValueText.text = "Medium (" + detectionChance + "%)";
            tracePercentText.color = mediumDetColor;
        }
        else // Low
        {
            //_detValueText.text = "Low (" + detectionChance + "%)";
            tracePercentText.color = lowDetColor;
        }
        tracePercentText.text = (int)(percent * 100) + "%";

        Color usedColor = tracePercentText.color;

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);

        yield return new WaitForSeconds(delay);

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);

        yield return new WaitForSeconds(delay);

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);

        yield return new WaitForSeconds(delay);

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.8f);

        yield return new WaitForSeconds(delay);

        tracePercentText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
    }

    public void RevealAnim()
    {
        SetPercentText(0f);
        StartCoroutine(AnimateReveal());
    }

    IEnumerator AnimateReveal()
    {
        float delay = 0.1f;

        // The "-------" and ##% both fade in from black

        // The [   ] flash in with highlights

        highlightLeft.gameObject.SetActive(true);
        highlightRight.gameObject.SetActive(true);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 1f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 1f);

        yield return new WaitForSeconds(delay);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.2f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.5f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.4f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.75f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.6f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.25f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.8f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.5f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        wideBar.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 1f);
        //
        highlightLeft.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0f);
        highlightRight.color = new Color(facadeColor.r, facadeColor.g, facadeColor.b, 0f);
        highlightLeft.gameObject.SetActive(false);
        highlightRight.gameObject.SetActive(false);

    }

    public void ExpandByPercent(float percentNew)
    {
        // Play sound
        AudioManager.inst.PlayMiscSpecific2(AudioManager.inst.UI_Clips[43]);

        // Update value
        traceAmount += percentNew;

        StartCoroutine(PercentTextUpdate(traceAmount));

        if(traceAmount >= 1f) // Filled to max
        {
            StartCoroutine(ExpandToMax());
        }
        else // Not filled to max
        {
            //
            // The facade expands first, in bright green.
            // It then fades out to the true color, where the true bar expands to cover it.
            //
            facadeBar.fillAmount = traceAmount;
            facadeBar.color = facadeColor;

            StartCoroutine(ExpandAnim(traceAmount));
        }
    }

    private IEnumerator ExpandAnim(float percentNew)
    {
        float delay = 0.1f;

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(facadeColor, trueColor, 0.2f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(facadeColor, trueColor, 0.4f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(facadeColor, trueColor, 0.6f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(facadeColor, trueColor, 0.8f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = trueColor;
        trueBar.fillAmount = percentNew;
        facadeBar.color = facadeColor;

    }

    private IEnumerator ExpandToMax()
    {
        trueBar.fillAmount = 1f;
        trueBar.color = trueColor;
        facadeBar.fillAmount = 1f;

        UIManager.inst.Terminal_DoConsequences(UIManager.inst.highSecRed, HF.GetMachineType(UIManager.inst.terminal_targetTerm).ToUpper() + " LOCKED");

        float delay = 0.1f;

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(trueColor, facadeColor, 0.2f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(trueColor, facadeColor, 0.4f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(trueColor, facadeColor, 0.6f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = Color.Lerp(trueColor, facadeColor, 0.8f);

        yield return new WaitForSeconds(delay);

        facadeBar.color = facadeColor;
        trueBar.color = facadeColor;
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
