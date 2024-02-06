using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This is a MANUAL implementation of the / EVASION / expanded window opening.
/// This is done due to the wonky interactions with the animator and quickly opening/closing the window.
/// </summary>
public class UIEvasionAnimation : MonoBehaviour
{
    [Header("References")]
    public GameObject mainWindow_ref;
    //
    public TextMeshProUGUI title_text;
    public Image title_image;
    //
    public TextMeshProUGUI theoretical_text;
    public Image theoretical_image;
    //
    public TextMeshProUGUI running_text;
    public Image running_image;
    //
    public TextMeshProUGUI heat_text;
    public Image heat_image;
    //
    public TextMeshProUGUI speed_text;
    public Image speed_image;
    //
    public TextMeshProUGUI evasion_text;
    public Image evasion_image;
    //
    public TextMeshProUGUI phasing_text;
    public Image phasing_image;
    //
    public TextMeshProUGUI modified_text;
    public Image modified_image;
    //
    public TextMeshProUGUI realValue_text;
    public Image realValue_image;
    //
    //
    [Header("Slider Bar")]
    public GameObject bar;

    [Header("Colors")]
    public Color text_Gray;
    public Color highGreen;
    public Color lowGreen; // Used only by the bar
    public Color blackOut;

    public void OpenMenu()
    {
        StopAllCoroutines(); // Stop all any running

        SetStartConditions(); // Reset colors
        mainWindow_ref.SetActive(true); // Enable window
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[9]);

        StartCoroutine(Open()); // Begin
    }


    /// <summary>
    /// Mainly just set everything to black.
    /// </summary>
    private void SetStartConditions()
    {
        SetLeft(bar.GetComponent<RectTransform>(), 326.5f);

        title_text.color = Color.black;
        title_image.color = Color.black;

        theoretical_text.color = Color.black;
        theoretical_image.color = Color.black;

        running_text.text = "Running (?) - 0";
        running_text.color = Color.black;
        running_image.color = Color.black;

        heat_text.text = "Heat - 0";
        heat_text.color = Color.black;
        heat_image.color = Color.black;

        speed_text.text = "Speed + 0";
        speed_text.color = Color.black;
        speed_image.color = Color.black;

        evasion_text.text = "Evasion + 0";
        evasion_text.color = Color.black;
        evasion_image.color = Color.black;

        phasing_text.text = "Phasing + 0";
        phasing_text.color = Color.black;
        phasing_image.color = Color.black;

        modified_text.color = Color.black;
        modified_image.color = Color.black;

        realValue_text.color = Color.black;
        realValue_image.color = Color.black;
    }

    private IEnumerator Open()
    {
        StartCoroutine(Reveal_Title());        // 0 -> 0.1s
        StartCoroutine(Reveal_Theoretical());  // 0 -> 0.1s
        StartCoroutine(StretchLine());         // 0 -> 0.2s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Running());     // 0.05 -> 0.1s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Heat());        // 0.1 -> 0.15s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Speed());       // 0.15 -> 0.2s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Evasion());     // 0.2 -> 0.25s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Phasing());     // 0.25 -> 0.3s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_Modified());   // 0.3 -> 0.35s

        yield return new WaitForSeconds(0.05f);

        StartCoroutine(Reveal_True());       // 0.35 -> 0.4s

        yield return new WaitForSeconds(0.1f);

        UIManager.inst.Evasion_UpdateUI();
    }

    private IEnumerator Reveal_Title()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            title_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            title_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        title_text.color = highGreen;
        title_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            title_image.color = Color.Lerp(highGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        title_image.color = Color.black;
    }

    private IEnumerator StretchLine()
    {
        // Go from start conditions (326.5) to [15]
        // and go from HighGreen to LowGreen
        float start = 326.5f;
        float end = 15f;
        SetLeft(bar.GetComponent<RectTransform>(), start);

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            SetLeft(bar.GetComponent<RectTransform>(), Mathf.Lerp(start, end, elapsedTime / duration));
            bar.GetComponent<Image>().color = Color.Lerp(highGreen, lowGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        SetLeft(bar.GetComponent<RectTransform>(), end);
        bar.GetComponent<Image>().color = lowGreen;

        yield return null;
    }

    /*
     * The way we reveal these goes as follows:
     * -Start as black for both
     * -Text goes from black to HighGreen (Halfway)
     * -Image goes from black to HighGreen (Halfway)
     * -Text goes from HighGreen to Gray
     * -Image goes from black to black but 0% transparency
     */

    private IEnumerator Reveal_Theoretical()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            theoretical_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            theoretical_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        theoretical_text.color = highGreen;
        theoretical_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            theoretical_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            theoretical_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        theoretical_text.color = text_Gray;
        theoretical_image.color = blackOut;
    }

    private IEnumerator Reveal_Running()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            running_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            running_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        running_text.color = highGreen;
        running_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            running_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            running_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        running_text.color = text_Gray;
        running_image.color = blackOut;
    }

    private IEnumerator Reveal_Heat()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            heat_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            heat_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        heat_text.color = highGreen;
        heat_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            heat_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            heat_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        heat_text.color = text_Gray;
        heat_image.color = blackOut;
    }

    private IEnumerator Reveal_Speed()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            speed_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            speed_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        speed_text.color = highGreen;
        speed_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            speed_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            speed_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        speed_text.color = text_Gray;
        speed_image.color = blackOut;
    }

    private IEnumerator Reveal_Evasion()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            evasion_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            evasion_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        evasion_text.color = highGreen;
        evasion_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            evasion_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            evasion_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        evasion_text.color = text_Gray;
        evasion_image.color = blackOut;
    }

    private IEnumerator Reveal_Phasing()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            phasing_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            phasing_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        phasing_text.color = highGreen;
        phasing_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            phasing_text.color = Color.Lerp(highGreen, text_Gray, elapsedTime / duration);
            phasing_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        phasing_text.color = text_Gray;
        phasing_image.color = blackOut;
    }

    private IEnumerator Reveal_Modified()
    {
        #region | Black -> HighGreen
        float elapsedTime = 0f;
        float duration = 0.12f;

        while (elapsedTime < duration)
        {
            modified_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            modified_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        modified_text.color = highGreen;
        modified_image.color = highGreen;
        #endregion

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            modified_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        modified_image.color = blackOut;
    }

    private IEnumerator Reveal_True()
    {
        // This one is slightly different

        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            realValue_text.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);
            realValue_image.color = Color.Lerp(Color.black, highGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        realValue_text.color = highGreen;
        realValue_image.color = highGreen;

        // Then do the second half
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            realValue_text.color = Color.Lerp(highGreen, Color.black, elapsedTime / duration);
            realValue_image.color = Color.Lerp(highGreen, blackOut, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        realValue_text.color = Color.black;
        realValue_image.color = blackOut;
    }

    #region RectTransform Control
    private void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    private void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    #endregion
}
